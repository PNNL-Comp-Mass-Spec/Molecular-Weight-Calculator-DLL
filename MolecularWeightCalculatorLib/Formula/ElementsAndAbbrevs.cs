﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.EventLogging;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal class ElementsAndAbbrevs : EventReporter
    {
        // Ignore Spelling: ElementAlph, Func, Isoelectric, unprotonated

        public ElementsAndAbbrevs(ElementAndMassTools massCalc)
        {
            mMassCalc = massCalc;

            mElementAlph = new List<KeyValuePair<string, int>>(ELEMENT_COUNT);
            mElementStats = new ElementInfo[ELEMENT_COUNT + 1];

            // The first element is hydrogen at mElementStats[1]
            // Initialize a placeholder class at index 0 (indicating an 'Invalid' element)
            mElementStats[0] = new ElementInfo();

            mAbbrevStats = new List<AbbrevStatsData>(60);

            mMasterSymbolsList = new List<SymbolLookupInfo>();
        }

        private readonly ElementAndMassTools mMassCalc;
        private FormulaParser Parser => mMassCalc.Parser;
        private FormulaOptions Options => mMassCalc.ComputationOptions;

        public const int ELEMENT_COUNT = 118;
        internal const int MAX_ISOTOPES = 11;

        public const int MAX_ABBREV_COUNT = 500;
        internal const int MAX_ABBREV_LENGTH = 6;

        /// <summary>
        /// Stores the elements in alphabetical order (after 'C' and 'H', which are first), with Key==Symbol, and Value==Index in <see cref="mElementStats"/>
        /// Used for constructing empirical formulas
        /// 0 to ELEMENT_COUNT - 1
        /// </summary>
        private readonly List<KeyValuePair<string, int>> mElementAlph;

        /// <summary>
        /// Element stats
        /// 1 to ELEMENT_COUNT, 0 is basically 'invalid element'
        /// Leaving '0' as an invalid element allows indexing this array with the atomic number of the element
        /// </summary>
        private readonly ElementInfo[] mElementStats;

        /// <summary>
        /// Stores the element symbols, abbreviations, and amino acids in order of longest symbol length to shortest length, non-alphabetized,
        /// for use in symbol matching when parsing a formula
        /// </summary>
        /// <remarks>0 To .Count - 1</remarks>
        private readonly List<SymbolLookupInfo> mMasterSymbolsList;

        /// <summary>
        /// Includes both abbreviations and amino acids
        /// </summary>
        /// <remarks>0 to .Count - 1</remarks>
        private readonly List<AbbrevStatsData> mAbbrevStats;

        private ElementMassMode mCurrentElementMode = ElementMassMode.Average;

        /// <summary>
        /// List of element symbols, sorted alphabetically (except 'C' and 'H' are first and second, respectively)
        /// </summary>
        internal IReadOnlyList<KeyValuePair<string, int>> ElementAlph => mElementAlph;

        /// <summary>
        /// List of elements, starting at ElementStats[1] for Hydrogen
        /// </summary>
        /// <remarks>ElementStats[0] exists but is not a valid element</remarks>
        internal IReadOnlyList<ElementInfo> ElementStats => mElementStats;

        /// <summary>
        /// List of abbreviations, 0-based
        /// </summary>
        internal IReadOnlyList<AbbrevStatsData> AbbrevStats => mAbbrevStats;

        /// <summary>
        /// Charge carrier mass
        /// </summary>
        /// <remarks>
        /// 1.00727649 for monoisotopic mass or 1.00739 for average mass
        /// </remarks>
        public double ChargeCarrierMass { get; set; }

        public ElementMassMode ElementMode
        {
            get => mCurrentElementMode;
            set => SetElementMode(value);
        }

        /// <summary>
        /// Examines the formula excerpt to determine if it is an element, abbreviation, amino acid, or unknown
        /// </summary>
        /// <param name="formulaExcerpt"></param>
        /// <param name="symbolReference">Output: index of the matched element or abbreviation in mMasterSymbolsList[]</param>
        /// <returns>
        /// Element if matched an element
        /// Abbreviation if matched an abbreviation or amino acid
        /// Unknown if no match
        /// </returns>
        internal SymbolMatchMode CheckElemAndAbbrev(string formulaExcerpt, out short symbolReference)
        {
            var symbolMatchType = default(SymbolMatchMode);
            symbolReference = -1;

            // mMasterSymbolsList[] stores the element symbols, abbreviations, & amino acids in order of longest length to
            // shortest length, then by alphabet sorting, for use in symbol matching when parsing a formula

            // Look for match, stepping directly through mMasterSymbolsList[]
            // List is sorted by reverse length, so can do all at once

            foreach (var lookupSymbol in mMasterSymbolsList)
            {
                if (lookupSymbol.Symbol?.Length > 0)
                {
                    if (formulaExcerpt.Substring(0, Math.Min(formulaExcerpt.Length, lookupSymbol.Symbol.Length)) == lookupSymbol.Symbol)
                    {
                        // Matched a symbol
                        symbolMatchType = lookupSymbol.MatchType;
                        if (symbolMatchType == SymbolMatchMode.Unknown)
                        {
                            symbolReference = -1;
                        }
                        else
                        {
                            symbolReference = (short)lookupSymbol.Index;
                        }

                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Zero-length entry found in mMasterSymbolsList[]; this is unexpected");
                }
            }

            return symbolMatchType;
        }

        public void ConstructMasterSymbolsList()
        {
            // Call after loading or changing abbreviations or elements
            // Call after loading or setting abbreviation mode

            mMasterSymbolsList.Clear();
            mMasterSymbolsList.Capacity = ELEMENT_COUNT + mAbbrevStats.Count;

            // Construct search list
            for (var atomicNumber = 1; atomicNumber <= ELEMENT_COUNT; atomicNumber++)
            {
                mMasterSymbolsList.Add(new SymbolLookupInfo(mElementStats[atomicNumber].Symbol, atomicNumber, SymbolMatchMode.Element));
            }

            // Note: mAbbrevStats is 0-based
            if (Options.AbbrevRecognitionMode != AbbrevRecognitionMode.NoAbbreviations)
            {
                var includeAmino = Options.AbbrevRecognitionMode == AbbrevRecognitionMode.NormalPlusAminoAcids;

                for (var index = 0; index < mAbbrevStats.Count; index++)
                {
                    var stats = mAbbrevStats[index];
                    // If includeAmino = False then do not include amino acids
                    if (includeAmino || !stats.IsAminoAcid)
                    {
                        // Do not include if the formula is invalid
                        if (!stats.InvalidSymbolOrFormula)
                        {
                            mMasterSymbolsList.Add(new SymbolLookupInfo(stats.Symbol, index, SymbolMatchMode.Abbreviation));
                        }
                    }
                }
            }

            mMasterSymbolsList.Capacity = mMasterSymbolsList.Count;

            // Sort the search list
            // Will use the IComparable implementation for longest-to-shortest, then alphabetical.
            mMasterSymbolsList.Sort();
        }

        /// <summary>
        /// Get the number of abbreviations in memory
        /// </summary>
        internal int GetAbbreviationCount()
        {
            return mAbbrevStats.Count;
        }

        /// <summary>
        /// Get the abbreviation ID for the given abbreviation symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="aminoAcidsOnly"></param>
        /// <returns>ID if found, otherwise -1</returns>
        internal int GetAbbreviationId(string symbol, bool aminoAcidsOnly = false)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return -1;

            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if (mAbbrevStats[index].Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))
                {
                    if (!aminoAcidsOnly || mAbbrevStats[index].IsAminoAcid)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        internal bool GetAbbreviation(
            int abbreviationId,
            out string symbol,
            out string formula,
            out float charge,
            out bool isAminoAcid)
        {
            return GetAbbreviation(abbreviationId, out symbol, out formula, out charge, out isAminoAcid, out _, out _, out _);
        }

        /// <summary>
        /// Get an abbreviation, by ID
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <param name="symbol">Output: symbol</param>
        /// <param name="formula">Output: empirical formula</param>
        /// <param name="charge">Output: charge</param>
        /// <param name="isAminoAcid">Output: true if an amino acid</param>
        /// <param name="oneLetterSymbol">Output: one letter symbol (only used by amino acids)</param>
        /// <param name="comment">Output: comment</param>
        /// <param name="invalidSymbolOrFormula">Output: true if an invalid symbol or formula</param>
        /// <returns>True if success, false if abbreviationId is invalid</returns>
        internal bool GetAbbreviation(
            int abbreviationId,
            out string symbol,
            out string formula,
            out float charge,
            out bool isAminoAcid,
            out string oneLetterSymbol,
            out string comment,
            out bool invalidSymbolOrFormula)
        {
            if (abbreviationId >= 0 && abbreviationId < mAbbrevStats.Count)
            {
                var stats = mAbbrevStats[abbreviationId];
                symbol = stats.Symbol;
                formula = stats.Formula;
                charge = stats.Charge;
                isAminoAcid = stats.IsAminoAcid;
                oneLetterSymbol = stats.OneLetterSymbol;
                comment = stats.Comment;
                invalidSymbolOrFormula = stats.InvalidSymbolOrFormula;

                return true;
            }

            symbol = string.Empty;
            formula = string.Empty;
            charge = 0f;
            isAminoAcid = false;
            oneLetterSymbol = string.Empty;
            comment = string.Empty;
            invalidSymbolOrFormula = true;

            return false;
        }

        /// <summary>
        /// Get the mass of the abbreviation
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <returns>Mass if success, 0 if abbreviationId is invalid</returns>
        public double GetAbbreviationMass(int abbreviationId)
        {
            // This method does not recompute the abbreviation mass each time it is called
            // Rather, it uses the .Mass member of AbbrevStats
            // This requires that .Mass be updated if the abbreviation is changed, if an element is changed, or if the element mode is changed

            if (abbreviationId >= 0 && abbreviationId < mAbbrevStats.Count)
            {
                return mAbbrevStats[abbreviationId].Mass;
            }

            return 0d;
        }

        /// <summary>
        /// Convert an amino acid symbol to 1-letter or 3-letter notation
        /// </summary>
        /// <param name="symbolToFind">Amino acid to find</param>
        /// <param name="oneLetterTo3Letter">If true, assume symbolToFind is a one-letter amino acid symbol and return the 3-letter symbol</param>
        /// <returns>1-letter or 3-letter amino acid symbol if found, otherwise an empty string</returns>
        internal string GetAminoAcidSymbolConversion(string symbolToFind, bool oneLetterTo3Letter)
        {
            if (string.IsNullOrWhiteSpace(symbolToFind))
                return string.Empty;

            // Use AbbrevStats[] array to lookup code
            foreach (var item in mAbbrevStats)
            {
                if (item.IsAminoAcid)
                {
                    if (oneLetterTo3Letter)
                    {
                        if (item.OneLetterSymbol.Equals(symbolToFind, StringComparison.OrdinalIgnoreCase))
                        {
                            return item.Symbol;
                        }
                    }
                    else
                    {
                        if (item.Symbol.Equals(symbolToFind, StringComparison.OrdinalIgnoreCase))
                        {
                            return item.OneLetterSymbol;
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the current charge carrier mass
        /// </summary>
        public double GetChargeCarrierMass()
        {
            return ChargeCarrierMass;
        }

        internal int GetElementCount()
        {
            return ELEMENT_COUNT;
        }

        /// <summary>
        /// Returns the settings for the element with <paramref name="atomicNumber"/> in the output variables
        /// </summary>
        /// <param name="atomicNumber">Element atomic number (1 for hydrogen, 2 for helium, etc.)</param>
        /// <param name="symbol">Element symbol</param>
        /// <param name="mass">Mass</param>
        /// <param name="uncertainty">Uncertainty of the mass</param>
        /// <param name="charge">Charge</param>
        /// <param name="isotopeCount">Number of isotopes</param>
        /// <returns>True if success, false if atomicNumber is invalid</returns>
        internal bool GetElement(
            int atomicNumber,
            out string symbol,
            out double mass,
            out double uncertainty,
            out float charge,
            out short isotopeCount)
        {
            if (atomicNumber is >= 1 and <= ELEMENT_COUNT)
            {
                var stats = mElementStats[atomicNumber];
                symbol = stats.Symbol;
                mass = stats.Mass;
                uncertainty = stats.Uncertainty;
                charge = stats.Charge;
                isotopeCount = (short)stats.Isotopes.Count;

                return true;
            }

            symbol = string.Empty;
            mass = 0d;
            uncertainty = 0d;
            charge = 0f;
            isotopeCount = 0;
            return false;
        }

        /// <summary>
        /// Get the element ID for the given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        internal short GetAtomicNumber(string symbol)
        {
            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                if (string.Equals(mElementStats[index].Symbol, symbol, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (short)index;
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns the isotope masses and relative abundances for the element with <paramref name="atomicNumber"/>
        /// </summary>
        /// <param name="atomicNumber">Element atomic number (1 for hydrogen, 2 for helium, etc.)</param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses">output, 0-based array</param>
        /// <param name="isotopeAbundances">output, 0-based array</param>
        /// <returns>True if success, false if atomicNumber is invalid</returns>
        internal bool GetElementIsotopes(int atomicNumber, out short isotopeCount, out double[] isotopeMasses, out float[] isotopeAbundances)
        {
            if (atomicNumber is >= 1 and <= ELEMENT_COUNT)
            {
                var stats = mElementStats[atomicNumber];
                isotopeCount = (short)stats.Isotopes.Count;
                isotopeMasses = new double[isotopeCount];
                isotopeAbundances = new float[isotopeCount];
                for (var isotopeIndex = 0; isotopeIndex < stats.Isotopes.Count; isotopeIndex++)
                {
                    isotopeMasses[isotopeIndex] = stats.Isotopes[isotopeIndex].Mass;
                    isotopeAbundances[isotopeIndex] = stats.Isotopes[isotopeIndex].Abundance;
                }

                return true;
            }

            isotopeCount = 0;
            isotopeMasses = new double[1];
            isotopeAbundances = new float[1];

            return false;
        }

        /// <summary>
        /// Get the current element mode
        /// </summary>
        /// <returns>
        /// ElementMassMode.Average  = 1
        /// ElementMassMode.Isotopic = 2
        /// ElementMassMode.Integer  = 3
        /// </returns>
        internal ElementMassMode GetElementMode()
        {
            return mCurrentElementMode;
        }

        /// <summary>
        /// Return the element symbol for the given element ID
        /// </summary>
        /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        /// <param name="atomicNumber"></param>
        internal string GetElementSymbol(int atomicNumber)
        {
            if (atomicNumber is >= 1 and <= ELEMENT_COUNT)
            {
                return mElementStats[atomicNumber].Symbol;
            }

            return string.Empty;
        }

        public List<ElementInfo> GetElements()
        {
            return mElementStats.ToList();
        }

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        /// <param name="atomicNumber">Element ID</param>
        /// <param name="elementStat">Value to obtain: mass, charge, or uncertainty</param>
        internal double GetElementStat(int atomicNumber, ElementStatsType elementStat)
        {
            if (atomicNumber is >= 1 and <= ELEMENT_COUNT)
            {
                return elementStat switch
                {
                    ElementStatsType.Mass => mElementStats[atomicNumber].Mass,
                    ElementStatsType.Charge => mElementStats[atomicNumber].Charge,
                    ElementStatsType.Uncertainty => mElementStats[atomicNumber].Uncertainty,
                    _ => 0d
                };
            }

            return 0d;
        }

        public bool IsValidElementSymbol(string elementSymbol, bool caseSensitive = true)
        {
            if (caseSensitive)
            {
                var query = from item in mElementStats where (item.Symbol ?? string.Empty) == (elementSymbol ?? string.Empty) select item;
                return query.Any();
            }
            else
            {
                var query = from item in mElementStats where (item.Symbol?.ToLower() ?? string.Empty) == (elementSymbol?.ToLower() ?? string.Empty) select item;
                return query.Any();
            }
        }

        public void MemoryLoadAllElements(ElementMassMode elementMode)
        {
            // Make sure this value accurately reflects what's loaded...
            mCurrentElementMode = elementMode;

            // TODO: Consider calling this as part of the constructor
            MemoryLoadElements(elementMode);

            MemoryLoadAbbreviations();
        }

        /// <summary>
        /// Loads the hard-coded abbreviations; also calls <see cref="ConstructMasterSymbolsList"/> when done
        /// </summary>
        public void MemoryLoadAbbreviations()
        {
            mAbbrevStats.Clear();
            // Symbol                            Formula            1 letter abbreviation
            mAbbrevStats.Add(new AbbrevStatsData("Ala", "C3H5NO", 0f, true, "A", "Alanine"));
            mAbbrevStats.Add(new AbbrevStatsData("Arg", "C6H12N4O", 0f, true, "R", "Arginine, (unprotonated NH2)"));
            mAbbrevStats.Add(new AbbrevStatsData("Asn", "C4H6N2O2", 0f, true, "N", "Asparagine"));
            mAbbrevStats.Add(new AbbrevStatsData("Asp", "C4H5NO3", 0f, true, "D", "Aspartic acid (undissociated COOH)"));
            mAbbrevStats.Add(new AbbrevStatsData("Cys", "C3H5NOS", 0f, true, "C", "Cysteine (no disulfide link)"));
            mAbbrevStats.Add(new AbbrevStatsData("Gla", "C6H7NO5", 0f, true, "U", "gamma-Carboxyglutamate"));
            mAbbrevStats.Add(new AbbrevStatsData("Gln", "C5H8N2O2", 0f, true, "Q", "Glutamine"));
            mAbbrevStats.Add(new AbbrevStatsData("Glu", "C5H7NO3", 0f, true, "E", "Glutamic acid (undissociated COOH)"));
            mAbbrevStats.Add(new AbbrevStatsData("Gly", "C2H3NO", 0f, true, "G", "Glycine"));
            mAbbrevStats.Add(new AbbrevStatsData("His", "C6H7N3O", 0f, true, "H", "Histidine (unprotonated NH)"));
            mAbbrevStats.Add(new AbbrevStatsData("Hse", "C4H7NO2", 0f, true, "", "Homoserine"));
            mAbbrevStats.Add(new AbbrevStatsData("Hyl", "C6H12N2O2", 0f, true, "", "Hydroxylysine"));
            mAbbrevStats.Add(new AbbrevStatsData("Hyp", "C5H7NO2", 0f, true, "", "Hydroxyproline"));
            mAbbrevStats.Add(new AbbrevStatsData("Ile", "C6H11NO", 0f, true, "I", "Isoleucine"));
            mAbbrevStats.Add(new AbbrevStatsData("Leu", "C6H11NO", 0f, true, "L", "Leucine"));
            mAbbrevStats.Add(new AbbrevStatsData("Lys", "C6H12N2O", 0f, true, "K", "Lysine (unprotonated NH2)"));
            mAbbrevStats.Add(new AbbrevStatsData("Met", "C5H9NOS", 0f, true, "M", "Methionine"));
            mAbbrevStats.Add(new AbbrevStatsData("Orn", "C5H10N2O", 0f, true, "O", "Ornithine"));
            mAbbrevStats.Add(new AbbrevStatsData("Phe", "C9H9NO", 0f, true, "F", "Phenylalanine"));
            mAbbrevStats.Add(new AbbrevStatsData("Pro", "C5H7NO", 0f, true, "P", "Proline"));
            mAbbrevStats.Add(new AbbrevStatsData("Pyr", "C5H5NO2", 0f, true, "", "Pyroglutamic acid"));
            mAbbrevStats.Add(new AbbrevStatsData("Sar", "C3H5NO", 0f, true, "", "Sarcosine"));
            mAbbrevStats.Add(new AbbrevStatsData("Ser", "C3H5NO2", 0f, true, "S", "Serine"));
            mAbbrevStats.Add(new AbbrevStatsData("Thr", "C4H7NO2", 0f, true, "T", "Threonine"));
            mAbbrevStats.Add(new AbbrevStatsData("Trp", "C11H10N2O", 0f, true, "W", "Tryptophan"));
            mAbbrevStats.Add(new AbbrevStatsData("Tyr", "C9H9NO2", 0f, true, "Y", "Tyrosine"));
            mAbbrevStats.Add(new AbbrevStatsData("Val", "C5H9NO", 0f, true, "V", "Valine"));
            mAbbrevStats.Add(new AbbrevStatsData("Xxx", "C6H12N2O", 0f, true, "X", "Unknown"));

            mAbbrevStats.Add(new AbbrevStatsData("Bpy", "C10H8N2", 0f, false, "", "Bipyridine"));
            mAbbrevStats.Add(new AbbrevStatsData("Bu", "C4H9", 1f, false, "", "Butyl"));
            mAbbrevStats.Add(new AbbrevStatsData("D", "^2.0141018H", 1f, false, "", "Deuterium"));
            mAbbrevStats.Add(new AbbrevStatsData("En", "C2H8N2", 0f, false, "", "Ethylenediamine"));
            mAbbrevStats.Add(new AbbrevStatsData("Et", "CH3CH2", 1f, false, "", "Ethyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Me", "CH3", 1f, false, "", "Methyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Ms", "CH3SOO", -1, false, "", "Mesyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Oac", "C2H3O2", -1, false, "", "Acetate"));
            mAbbrevStats.Add(new AbbrevStatsData("Otf", "OSO2CF3", -1, false, "", "Triflate"));
            mAbbrevStats.Add(new AbbrevStatsData("Ox", "C2O4", -2, false, "", "Oxalate"));
            mAbbrevStats.Add(new AbbrevStatsData("Ph", "C6H5", 1f, false, "", "Phenyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Phen", "C12H8N2", 0f, false, "", "Phenanthroline"));
            mAbbrevStats.Add(new AbbrevStatsData("Py", "C5H5N", 0f, false, "", "Pyridine"));
            mAbbrevStats.Add(new AbbrevStatsData("Tpp", "(C4H2N(C6H5C)C4H2N(C6H5C))2", 0f, false, "", "Tetraphenylporphyrin"));
            mAbbrevStats.Add(new AbbrevStatsData("Ts", "CH3C6H4SOO", -1, false, "", "Tosyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Urea", "H2NCONH2", 0f, false, "", "Urea"));

            // Compute the masses for all abbreviations. Also calls ConstructMasterSymbolsList()
            RecomputeAbbreviationMasses();

            // Note Asx or B is often used for Asp or Asn
            // Note Glx or Z is often used for Glu or Gln
            // Note X is often used for "unknown"
            //
            // Other amino acids without widely agreed upon 1 letter codes
            //
            // FlexGridAddItems .grdAminoAcids, "Aminosuberic Acid", "Asu"     ' A pair of Cys residues bonded by S-S
            // FlexGridAddItems .grdAminoAcids, "Cystine", "Cyn"
            // FlexGridAddItems .grdAminoAcids, "Homocysteine", "Hcy"
            // FlexGridAddItems .grdAminoAcids, "Homoserine", "Hse"            ' 101.04 (C4H7NO2)
            // FlexGridAddItems .grdAminoAcids, "Hydroxylysine", "Hyl"         ' 144.173 (C6H12N2O2)
            // FlexGridAddItems .grdAminoAcids, "Hydroxyproline", "Hyp"        ' 113.116 (C5H7NO2)
            // FlexGridAddItems .grdAminoAcids, "Norleucine", "Nle"            ' 113.06
            // FlexGridAddItems .grdAminoAcids, "Norvaline", "Nva"
            // FlexGridAddItems .grdAminoAcids, "Pencillamine", "Pen"
            // FlexGridAddItems .grdAminoAcids, "Phosphoserine", "Sep"
            // FlexGridAddItems .grdAminoAcids, "Phosphothreonine", "Thp"
            // FlexGridAddItems .grdAminoAcids, "Phosphotyrosine", "Typ"
            // FlexGridAddItems .grdAminoAcids, "Pyroglutamic Acid", "Pyr"     ' 111.03 (C5H5NO2) (also Glp in some tables)
            // FlexGridAddItems .grdAminoAcids, "Sarcosine", "Sar"             ' 71.08 (C3H5NO)
            // FlexGridAddItems .grdAminoAcids, "Statine", "Sta"
            // FlexGridAddItems .grdAminoAcids, "b-[2-Thienyl]Ala", "Thi"

            // Need to explore http://www.abrf.org/ABRF/ResearchCommittees/deltamass/deltamass.html

            // Isoelectric points
            // TYR   Y   C9H9NO2     163.06333  163.1760      0               9.8
            // HIS   H   C6H7N3O     137.05891  137.1411      1               6.8
            // LYS   K   C6H12N2O    128.09496  128.1741      1              10.1
            // ASP   D   C4H5NO3     115.02694  115.0886      1               4.5
            // GLU   E   C5H7NO3     129.04259  129.1155      1               4.5
            // CYS   C   C3H5NOS     103.00919  103.1388      0               8.6
            // ARG   R   C6H12N4O    156.10111  156.1875      1              12.0
        }

        /// <summary>
        /// Load elements and isotopes; also calls <see cref="ConstructMasterSymbolsList"/> and <see cref="RecomputeAbbreviationMasses"/> when done
        /// </summary>
        /// <remarks>
        /// <param name="elementMode">Element mode: 1 for average weights, 2 for monoisotopic weights, 3 for integer weights</param>
        /// <param name="specificElement"></param>
        /// <param name="specificStatToReset"></param>
        /// <paramref name="specificElement"/> and <paramref name="specificStatToReset"/> are zero when updating all of the elements and isotopes.
        /// nonzero <paramref name="specificElement"/> and <paramref name="specificStatToReset"/> values will set just that specific value to the default
        /// </remarks>
        public void MemoryLoadElements(
            ElementMassMode elementMode = ElementMassMode.Average,
            int specificElement = 0,
            ElementStatsType specificStatToReset = ElementStatsType.Mass)
        {
            const double defaultChargeCarrierMassAvg = 1.00739d;
            const double defaultChargeCarrierMassMonoiso = 1.00727649d;

            // Data obtained from https://www.nist.gov/pml/atomic-weights-and-isotopic-compositions-relative-atomic-masses
            // which obtained its data from https://www.ciaaw.org/atomic-weights.htm and https://www.degruyter.com/document/doi/10.1351/PAC-REP-10-06-02/html

            // For elements with a standard atomic weight range (e.g. [6.938,6.997] for Lithium), use the conventional atomic-weight,
            // as defined in Table 3 in "Atomic weights of the elements 2013 (IUPAC Technical Report)"
            // Published in Pure and Applied Chemistry, Volume 88, Issue 3
            // https://doi.org/10.1515/pac-2015-0305

            // For radioactive elements, the mass of the most stable isotope is stored for the isotopic mass

            // Naturally occurring radioactive elements have an average weight and associated uncertainty
            // For the other radioactive elements, the mass of the most stable isotope is used, rounded to one decimal place
            // When an average mass uncertainty is not available, a value of 0.0005 is used

            // For example, Nobelium has average mass 259.1 (±0.0005)

            // Define the charge carrier mass
            if (elementMode == ElementMassMode.Average)
            {
                SetChargeCarrierMass(defaultChargeCarrierMassAvg);
            }
            else
            {
                SetChargeCarrierMass(defaultChargeCarrierMassMonoiso);
            }

            var elementMemoryData = ElementsLoader.MemoryLoadElements();

            // Assign delegate functions to be used when instantiating an ElementInfo instance for each element
            // Set uncertainty to 0 for all elements if using exact isotopic or integer isotopic weights

            Func<ElementMem, double> getMass;
            Func<ElementMem, double> getUncertainty;
            switch (elementMode)
            {
                // ReSharper disable UnusedParameter.Local
                case ElementMassMode.Integer:
                    getMass = elementMem => elementMem.MassInteger;
                    getUncertainty = elementMem => 0;
                    break;

                case ElementMassMode.Isotopic:
                    getMass = elementMem => elementMem.MassIsotopic;
                    getUncertainty = elementMem => 0;
                    break;
                // ReSharper restore UnusedParameter.Local

                case ElementMassMode.Average:
                    getMass = elementMem => elementMem.MassAverage;
                    getUncertainty = elementMem => elementMem.UncertaintyAverageMass;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(elementMode), elementMode, null);
            }

            // Set uncertainty to 0 for all elements if using exact isotopic or integer isotopic weights
            if (specificElement == 0)
            {
                // Updating all the elements
                mElementAlph.Clear();
                for (var atomicNumber = 1; atomicNumber <= ELEMENT_COUNT; atomicNumber++)
                {
                    var elementMem = elementMemoryData[atomicNumber];
                    mElementStats[atomicNumber] = new ElementInfo(elementMem.Symbol, elementMem.Charge, getMass(elementMem), getUncertainty(elementMem));

                    mElementAlph.Add(new KeyValuePair<string, int>(elementMem.Symbol, atomicNumber));
                }

                // Alphabetize mElementAlph by Key/symbol
                // mElementAlph.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));

                // Alphabetize mElementAlph by Key/symbol, except for carbon and hydrogen (put them first)
                mElementAlph.Sort((x, y) =>
                {
                    // Test 'C' and 'H' on value, since number comparisons are faster than string comparisons

                    // Always sort C/carbon first
                    if (x.Value == 6)
                        return -1;
                    if (y.Value == 6)
                        return 1;

                    // Hydrogen is always second
                    if (x.Value == 1)
                        return -1;

                    if (y.Value == 1)
                        return 1;

                    // Everything else is alphabetical.
                    return string.CompareOrdinal(x.Key, y.Key);
                });

                // Also load the isotopes, since if any were loaded we just cleared them.
                ElementsLoader.MemoryLoadIsotopes(mElementStats);
            }
            else if (specificElement is >= 1 and <= ELEMENT_COUNT)
            {
                var stats = mElementStats[specificElement];
                switch (specificStatToReset)
                {
                    case ElementStatsType.Mass:
                        stats.Mass = getMass(elementMemoryData[specificElement]);
                        break;

                    case ElementStatsType.Uncertainty:
                        stats.Uncertainty = getUncertainty(elementMemoryData[specificElement]);
                        break;

                    case ElementStatsType.Charge:
                        stats.Charge = elementMemoryData[specificElement].Charge;
                        break;

                    // ReSharper disable once RedundantEmptySwitchSection
                    default:
                        // Ignore it
                        break;
                }
            }

            // Reconstruct master symbols list
            // This is needed here to properly load the abbreviations
            ConstructMasterSymbolsList();

            // Also update and existing abbreviations
            RecomputeAbbreviationMasses();
        }

        /// <summary>
        /// Recomputes the Mass for all of the loaded abbreviations; also calls <see cref="ConstructMasterSymbolsList"/>
        /// </summary>
        /// <returns>ErrorID: '0' for no error</returns>
        internal int RecomputeAbbreviationMasses()
        {
            if (mAbbrevStats.Count == 0)
            {
                return 0;
            }

            // Reset the values that can affect computation; also allows later resolving of a dependency on another abbreviation.
            foreach (var stats in mAbbrevStats)
            {
                stats.InvalidSymbolOrFormula = false;
                stats.ClearElements();
            }

            // Rebuild the master symbols list; this will temporarily add all abbreviations to the list (since we reset the invalid flag)
            ConstructMasterSymbolsList();

            var retryList = new List<AbbrevStatsData>(mAbbrevStats);
            var lastErrorId = 0;

            // Loop to allow abbreviations to depend on other abbreviations without requiring a specific calculation order.
            while (retryList.Count > 0)
            {
                var toRetry = new List<AbbrevStatsData>(mAbbrevStats.Count);
                foreach (var stats in retryList)
                {
                    var errorId = Parser.ComputeAbbrevWeight(stats);
                    if (errorId == 31)
                    {
                        // Error occurred: abbreviation depends on another abbreviation that has not yet been parsed
                        // Retry later
                        toRetry.Add(stats);
                    }
                    else if (errorId != 0)
                    {
                        // Error occurred parsing abbreviation or computing mass; usually bad formula or circular reference.
                        stats.Mass = 0d;
                        stats.InvalidSymbolOrFormula = true;
                        lastErrorId = errorId;
                    }
                }

                retryList = toRetry;
            }

            // Rebuild the master symbols list; this time it will exclude any abbreviations that were marked as invalid.
            ConstructMasterSymbolsList();

            return lastErrorId;
        }

        internal void RemoveAllAbbreviations()
        {
            mAbbrevStats.Clear();
            ConstructMasterSymbolsList();
        }

        /// <summary>
        /// Look for the abbreviation and remove it
        /// </summary>
        /// <param name="abbreviationSymbol"></param>
        /// <returns>True if found and removed, false abbreviationSymbol was not found</returns>
        internal bool RemoveAbbreviation(string abbreviationSymbol)
        {
            var removed = false;

            abbreviationSymbol = abbreviationSymbol?.ToLower();

            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if ((mAbbrevStats[index].Symbol?.ToLower() ?? string.Empty) == (abbreviationSymbol ?? string.Empty))
                {
                    RemoveAbbreviationById(index);
                    removed = true;
                }
            }

            return removed;
        }

        /// <summary>
        /// Remove the abbreviation at index <paramref name="abbreviationId"/>
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <returns>True if found and removed, false abbreviationId is invalid</returns>
        internal bool RemoveAbbreviationById(int abbreviationId)
        {
            if (abbreviationId < 0 || abbreviationId >= mAbbrevStats.Count)
            {
                return false;
            }

            mAbbrevStats.RemoveAt(abbreviationId);

            ConstructMasterSymbolsList();
            return true;
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on <paramref name="symbol"/>)
        /// </summary>
        /// <remarks>
        /// It is useful to set <paramref name="validateFormula"/> = false when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetterSymbol"></param>
        /// <param name="comment"></param>
        /// <param name="validateFormula">If true, make sure the formula is valid</param>
        /// <returns>0 if success, otherwise an error ID</returns>
        internal int SetAbbreviation(
            string symbol, string formula,
            float charge, bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true)
        {
            var abbrevId = 0;
            var errorId = 0;

            // See if the abbreviation is already present
            var alreadyPresent = false;
            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if ((mAbbrevStats[index].Symbol?.ToUpper() ?? string.Empty) == (symbol?.ToUpper() ?? string.Empty))
                {
                    alreadyPresent = true;
                    abbrevId = index;
                    break;
                }
            }

            // AbbrevStats is a 1-based array
            if (!alreadyPresent)
            {
                if (mAbbrevStats.Count < MAX_ABBREV_COUNT)
                {
                    abbrevId = mAbbrevStats.Count;
                }
                else
                {
                    // Too many abbreviations
                    errorId = 196;
                }
            }

            if (abbrevId >= 1)
            {
                errorId = SetAbbreviationById((short)abbrevId, symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
            }

            return errorId;
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on <paramref name="abbrevId"/>)
        /// </summary>
        /// <param name="abbrevId">0-based abbreviation id; if less than 0, adds as a new abbreviation</param>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetterSymbol"></param>
        /// <param name="comment"></param>
        /// <param name="validateFormula"></param>
        /// <returns>0 if success, otherwise an error ID</returns>
        internal int SetAbbreviationById(
            int abbrevId, string symbol,
            string formula, float charge,
            bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true)
        {
            var invalidSymbolOrFormula = false;

            if (symbol.Length < 1)
            {
                // Symbol length is 0
                return 192;
            }

            if (symbol.Length > MAX_ABBREV_LENGTH)
            {
                // Abbreviation symbol too long
                return 190;
            }

            if (!symbol.All(char.IsLetter))
            {
                // Symbol does not just contain letters
                return 194;
            }

            if (formula.Length == 0)
            {
                // Invalid formula (actually, blank formula)
                return 160;
            }

            // Convert symbol to proper case mode
            symbol = symbol.Substring(0, 1).ToUpper() + symbol.Substring(1).ToLower();

            // If abbrevId is < 0 or >= mAbbrevStats.Count, define it
            if (abbrevId < 0 || abbrevId >= mAbbrevStats.Count)
            {
                if (mAbbrevStats.Count < MAX_ABBREV_COUNT)
                {
                    abbrevId = (short)mAbbrevStats.Count;
                }
                else
                {
                    // Too many abbreviations
                    return 196;
                }
            }

            // Make sure the abbreviation doesn't match one of the standard elements
            var symbolMatchType = CheckElemAndAbbrev(symbol, out var symbolReference);

            if (symbolMatchType == SymbolMatchMode.Element)
            {
                if ((mElementStats[symbolReference].Symbol ?? string.Empty) == symbol)
                {
                    invalidSymbolOrFormula = true;
                }
            }

            if (!invalidSymbolOrFormula && validateFormula)
            {
                // Make sure the abbreviation's formula is valid
                // This will also auto-capitalize the formula if auto-capitalize is turned on
                var data = Parser.ParseFormula(formula);
                formula = data.Formula;

                if (data.ErrorData.ErrorId != 0)
                {
                    // An error occurred while parsing
                    // Already present in ErrorParams.ErrorID
                    // We'll still add the formula, but mark it as invalid
                    invalidSymbolOrFormula = true;
                }
            }

            var stats = new AbbrevStatsData(symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, invalidSymbolOrFormula);

            if (abbrevId < 0 || abbrevId >= mAbbrevStats.Count)
                mAbbrevStats.Add(stats);
            else
                mAbbrevStats[abbrevId] = stats;

            // Compute the mass, handling any now-resolved abbreviation dependencies
            // Also calls ConstructMasterSymbolsList();
            return RecomputeAbbreviationMasses();
        }

        /// <summary>
        /// Set the charge carrier mass
        /// </summary>
        /// <param name="mass"></param>
        internal void SetChargeCarrierMass(double mass)
        {
            ChargeCarrierMass = mass;
        }

        /// <summary>
        /// Update the values for a single element (based on <paramref name="symbol"/>)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
        /// <param name="charge"></param>
        /// <param name="recomputeAbbreviationMasses">Set to False if updating several elements</param>
        /// <returns>True if success, false if symbol is not a valid element symbol</returns>
        internal bool SetElement(string symbol, double mass, double uncertainty, float charge, bool recomputeAbbreviationMasses = true)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return false;
            }

            var found = false;

            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                if (!string.Equals(symbol, mElementStats[index].Symbol, StringComparison.OrdinalIgnoreCase))
                    continue;

                var stats = mElementStats[index];
                stats.Mass = mass;
                stats.Uncertainty = uncertainty;
                stats.Charge = charge;

                found = true;
                break;
            }

            if (found)
            {
                if (recomputeAbbreviationMasses)
                    RecomputeAbbreviationMasses();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the isotopes for the element
        /// </summary>
        /// <remarks>
        /// The sum of the relative abundances should be 1.00
        /// </remarks>
        /// <param name="symbol"></param>
        /// <param name="isotopeMasses">0-based array of isotope masses</param>
        /// <param name="isotopeAbundances">0-based array of relative isotopic abundances (values between 0 and 1)</param>
        /// <returns>True if success, false if symbol is not a valid element symbol</returns>
        internal bool SetElementIsotopes(string symbol, double[] isotopeMasses, float[] isotopeAbundances)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return false;
            }

            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                if (!string.Equals(symbol, mElementStats[index].Symbol, StringComparison.OrdinalIgnoreCase))
                    continue;

                var stats = mElementStats[index];
                stats.Isotopes.Clear();
                for (var isotopeIndex = 0; isotopeIndex < isotopeMasses.Length; isotopeIndex++)
                {
                    if (isotopeIndex > MAX_ISOTOPES)
                        break;

                    stats.Isotopes.Add(new IsotopeInfo(isotopeMasses[isotopeIndex], isotopeAbundances[isotopeIndex]));
                }

                stats.Isotopes.Capacity = stats.Isotopes.Count;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the element mode
        /// </summary>
        /// <param name="newElementMode"></param>
        /// <param name="forceMemoryLoadElementValues">Set to true if you want to force a reload of element weights, even if not changing element modes</param>
        internal void SetElementMode(ElementMassMode newElementMode, bool forceMemoryLoadElementValues = false)
        {
            try
            {
                if (newElementMode != mCurrentElementMode || forceMemoryLoadElementValues || mElementAlph.Count == 0)
                {
                    mCurrentElementMode = newElementMode;

                    MemoryLoadElements(mCurrentElementMode);
                }
            }
            catch (Exception ex)
            {
                OnErrorEvent(Logging.GeneralErrorHandler("ElementAndMassTools.SetElementMode", ex));
            }
        }

        internal void SortAbbreviations()
        {
            // Default comparison specified in AbbrevStatsData
            mAbbrevStats.Sort();

            // Need to re-construct the master symbols list
            ConstructMasterSymbolsList();
        }

        /// <summary>
        /// Checks the formula of all abbreviations to make sure it's valid
        /// Marks any abbreviations as Invalid if a problem is found or a circular reference exists
        /// </summary>
        /// <returns>Count of the number of invalid abbreviations found</returns>
        internal int ValidateAllAbbreviations()
        {
            var invalidAbbreviationCount = 0;

            for (short abbrevIndex = 0; abbrevIndex < mAbbrevStats.Count; abbrevIndex++)
            {
                var stats = mAbbrevStats[abbrevIndex];
                SetAbbreviationById(abbrevIndex, stats.Symbol, stats.Formula, stats.Charge, stats.IsAminoAcid, stats.OneLetterSymbol, stats.Comment, true);
                if (stats.InvalidSymbolOrFormula)
                {
                    invalidAbbreviationCount++;
                }
            }

            return invalidAbbreviationCount;
        }
    }
}
