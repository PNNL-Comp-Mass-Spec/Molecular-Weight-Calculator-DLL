using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal class ElementsAndAbbrevs
    {
        public ElementsAndAbbrevs(ElementAndMassTools massCalc)
        {
            mMassCalc = massCalc;

            mElementAlph = new List<KeyValuePair<string, int>>(ELEMENT_COUNT);
            mElementStats = new ElementInfo[ELEMENT_COUNT + 1];
            mElementStats[0] = new ElementInfo(); // 'Invalid' element at index 0

            mAbbrevStats = new List<AbbrevStatsData>(60);

            mMasterSymbolsList = new List<SymbolLookupInfo>();
        }

        private readonly ElementAndMassTools mMassCalc;
        private FormulaParser Parser => mMassCalc.Parser;
        private FormulaOptions Options => mMassCalc.ComputationOptions;

        public const int ELEMENT_COUNT = 103;
        internal const int MAX_ISOTOPES = 11;

        public const int MAX_ABBREV_COUNT = 500;
        internal const int MAX_ABBREV_LENGTH = 6;

        /// <summary>
        /// Stores the elements in alphabetical order, with Key==Symbol, and Value==Index in <see cref="mElementStats"/>
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
        /// 0 To .Count - 1
        /// </summary>
        private readonly List<SymbolLookupInfo> mMasterSymbolsList;

        /// <summary>
        /// Includes both abbreviations and amino acids
        /// </summary>
        private readonly List<AbbrevStatsData> mAbbrevStats;

        /// <summary>
        /// Charge carrier mass
        /// 1.00727649 for monoisotopic mass or 1.00739 for average mass
        /// </summary>
        private double mChargeCarrierMass;

        private ElementMassMode mCurrentElementMode = ElementMassMode.Average;

        /// <summary>
        /// List of element symbols, sorted alphabetically
        /// </summary>
        internal IReadOnlyList<KeyValuePair<string, int>> ElementAlph => mElementAlph;

        /// <summary>
        /// List of elements. Index 0 is invalid, so that index should be the same as the atomic number of the element
        /// </summary>
        internal IReadOnlyList<ElementInfo> ElementStats => mElementStats;

        /// <summary>
        /// List of abbreviations, 0-based
        /// </summary>
        internal IReadOnlyList<AbbrevStatsData> AbbrevStats => mAbbrevStats;

        public double ChargeCarrierMass
        {
            get => mChargeCarrierMass;
            set => mChargeCarrierMass = value;
        }

        public ElementMassMode ElementModeInternal
        {
            get => mCurrentElementMode;
            set => SetElementMode(value);
        }

        /// <summary>
        /// Add an abbreviation
        /// </summary>
        /// <param name="abbrevIndex"></param>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetter"></param>
        /// <param name="comment"></param>
        /// <param name="invalidSymbolOrFormula"></param>
        /// <returns><paramref name="formula"/> with format standardized by ParseFormulaPublic</returns>
        private string AddAbbreviationWork(
            short abbrevIndex, string symbol,
            string formula, float charge,
            bool isAminoAcid,
            string oneLetter = "",
            string comment = "",
            bool invalidSymbolOrFormula = false)
        {
            AbbrevStatsData stats;
            if (abbrevIndex < 0 || abbrevIndex >= mAbbrevStats.Count)
            {
                stats = new AbbrevStatsData(symbol, formula, charge, isAminoAcid, oneLetter, comment, invalidSymbolOrFormula);
                mAbbrevStats.Add(stats);
            }
            else
            {
                stats = mAbbrevStats[abbrevIndex];
                stats.InvalidSymbolOrFormula = invalidSymbolOrFormula;
                stats.Symbol = symbol;
                stats.Formula = formula;
                stats.Charge = charge;
                stats.OneLetterSymbol = oneLetter.ToUpper();
                stats.IsAminoAcid = isAminoAcid;
                stats.Comment = comment;
            }

            if (!Parser.ComputeAbbrevWeight(stats))
            {
                // Error occurred computing mass for abbreviation
                stats.Mass = 0d;
                stats.InvalidSymbolOrFormula = true;
            }

            return formula;
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
            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                mMasterSymbolsList.Add(new SymbolLookupInfo(mElementStats[index].Symbol, index, SymbolMatchMode.Element));
            }

            // Note: mAbbrevStats is 0-based
            if (Options.AbbrevRecognitionMode != AbbrevRecognitionMode.NoAbbreviations)
            {
                bool includeAmino;
                if (Options.AbbrevRecognitionMode == AbbrevRecognitionMode.NormalPlusAminoAcids)
                {
                    includeAmino = true;
                }
                else
                {
                    includeAmino = false;
                }

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
            mMasterSymbolsList.Sort(); // Will use the IComparable implementation for longest-to-shortest, then alphabetical.
        }

        /// <summary>
        /// Get the number of abbreviations in memory
        /// </summary>
        /// <returns></returns>
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
            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if ((mAbbrevStats[index].Symbol?.ToLower() ?? "") == (symbol?.ToLower() ?? ""))
                {
                    if (!aminoAcidsOnly || mAbbrevStats[index].IsAminoAcid)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        internal int GetAbbreviation(
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
        /// <returns> 0 if success, 1 if failure</returns>
        internal int GetAbbreviation(
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

                return 0;
            }

            symbol = string.Empty;
            formula = string.Empty;
            charge = 0f;
            isAminoAcid = false;
            oneLetterSymbol = string.Empty;
            comment = string.Empty;
            invalidSymbolOrFormula = true;

            return 1;
        }

        public double GetAbbreviationMass(int abbreviationId)
        {
            // Returns the mass if success, 0 if failure
            // Could return -1 if failure, but might mess up some calculations

            // This function does not recompute the abbreviation mass each time it is called
            // Rather, it uses the .Mass member of AbbrevStats
            // This requires that .Mass be updated if the abbreviation is changed, if an element is changed, or if the element mode is changed

            if (abbreviationId >= 0 && abbreviationId < mAbbrevStats.Count)
            {
                return mAbbrevStats[abbreviationId].Mass;
            }

            return 0d;
        }

        internal string GetAminoAcidSymbolConversion(string symbolToFind, bool oneLetterTo3Letter)
        {
            // If oneLetterTo3Letter = true, then converting 1 letter codes to 3 letter codes
            // Returns the symbol, if found
            // Otherwise, returns ""

            var returnSymbol = "";
            // Use AbbrevStats[] array to lookup code
            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if (mAbbrevStats[index].IsAminoAcid)
                {
                    string compareSymbol;
                    if (oneLetterTo3Letter)
                    {
                        compareSymbol = mAbbrevStats[index].OneLetterSymbol;
                    }
                    else
                    {
                        compareSymbol = mAbbrevStats[index].Symbol;
                    }

                    if ((compareSymbol?.ToLower() ?? "") == (symbolToFind?.ToLower() ?? ""))
                    {
                        if (oneLetterTo3Letter)
                        {
                            returnSymbol = mAbbrevStats[index].Symbol;
                        }
                        else
                        {
                            returnSymbol = mAbbrevStats[index].OneLetterSymbol;
                        }

                        break;
                    }
                }
            }

            return returnSymbol;
        }

        public double GetChargeCarrierMass()
        {
            return mChargeCarrierMass;
        }

        internal int GetElementCount()
        {
            return ELEMENT_COUNT;
        }

        /// <summary>
        /// Returns the settings for the element with <paramref name="elementId"/> in the ByRef variables
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
        /// <param name="charge"></param>
        /// <param name="isotopeCount"></param>
        /// <returns>0 if success, 1 if failure</returns>
        internal int GetElement(
            short elementId,
            out string symbol,
            out double mass,
            out double uncertainty,
            out float charge,
            out short isotopeCount)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                var stats = mElementStats[elementId];
                symbol = stats.Symbol;
                mass = stats.Mass;
                uncertainty = stats.Uncertainty;
                charge = stats.Charge;
                isotopeCount = (short)stats.Isotopes.Count;

                return 0;
            }

            symbol = string.Empty;
            mass = 0d;
            uncertainty = 0d;
            charge = 0f;
            isotopeCount = 0;
            return 1;
        }

        /// <summary>
        /// Get the element ID for the given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        internal short GetElementId(string symbol)
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
        /// Returns the isotope masses and abundances for the element with <paramref name="elementId"/>
        /// </summary>
        /// <param name="elementId">Element ID, or atomic number</param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses">output, 0-based array</param>
        /// <param name="isotopeAbundances">output, 0-based array</param>
        /// <returns>0 if a valid ID, 1 if invalid</returns>
        internal int GetElementIsotopes(short elementId, out short isotopeCount, out double[] isotopeMasses, out float[] isotopeAbundances)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                var stats = mElementStats[elementId];
                isotopeCount = (short)stats.Isotopes.Count;
                isotopeMasses = new double[isotopeCount];
                isotopeAbundances = new float[isotopeCount];
                for (var isotopeIndex = 0; isotopeIndex < stats.Isotopes.Count; isotopeIndex++)
                {
                    isotopeMasses[isotopeIndex] = stats.Isotopes[isotopeIndex].Mass;
                    isotopeAbundances[isotopeIndex] = stats.Isotopes[isotopeIndex].Abundance;
                }

                return 0;
            }

            isotopeCount = 0;
            isotopeMasses = new double[1];
            isotopeAbundances = new float[1];

            return 1;
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
        /// <param name="elementId"></param>
        /// <returns></returns>
        /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        internal string GetElementSymbol(short elementId)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                return mElementStats[elementId].Symbol;
            }

            return "";
        }

        public List<ElementInfo> GetElements()
        {
            return mElementStats.ToList();
        }

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <param name="elementStat">Value to obtain: mass, charge, or uncertainty</param>
        /// <returns></returns>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        internal double GetElementStat(short elementId, ElementStatsType elementStat)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                return elementStat switch
                {
                    ElementStatsType.Mass => mElementStats[elementId].Mass,
                    ElementStatsType.Charge => mElementStats[elementId].Charge,
                    ElementStatsType.Uncertainty => mElementStats[elementId].Uncertainty,
                    _ => 0d
                };
            }

            return 0d;
        }

        public bool IsValidElementSymbol(string elementSymbol, bool caseSensitive = true)
        {
            if (caseSensitive)
            {
                var query = from item in mElementStats where (item.Symbol ?? "") == (elementSymbol ?? "") select item;
                return query.Any();
            }
            else
            {
                var query = from item in mElementStats where (item.Symbol?.ToLower() ?? "") == (elementSymbol?.ToLower() ?? "") select item;
                return query.Any();
            }
        }

        public void MemoryLoadAllElements(ElementMassMode elementMode)
        {
            // Make sure this value accurately reflects what's loaded...
            mCurrentElementMode = elementMode;

            // TODO: Consider calling this as part of the constructor
            MemoryLoadElements(elementMode);

            // Reconstruct master symbols list
            // This is needed here to properly load the abbreviations
            ConstructMasterSymbolsList();

            MemoryLoadAbbreviations();

            // Reconstruct master symbols list
            // Needed here to load abbreviations into the list
            ConstructMasterSymbolsList();
        }

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
            mAbbrevStats.Add(new AbbrevStatsData("D", "^2.014H", 1f, false, "", "Deuterium"));
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

            foreach (var stats in mAbbrevStats)
            {
                if (!Parser.ComputeAbbrevWeight(stats))
                {
                    // Error occurred computing mass for abbreviation
                    stats.Mass = 0d;
                    stats.InvalidSymbolOrFormula = true;
                }
            }

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
        /// Load elements and isotopes
        /// </summary>
        /// <param name="elementMode">Element mode: 1 for average weights, 2 for monoisotopic weights, 3 for integer weights</param>
        /// <param name="specificElement"></param>
        /// <param name="specificStatToReset"></param>
        /// <remarks>
        /// <paramref name="specificElement"/> and <paramref name="specificStatToReset"/> are zero when updating all of the elements and isotopes.
        /// nonzero <paramref name="specificElement"/> and <paramref name="specificStatToReset"/> values will set just that specific value to the default
        /// </remarks>
        public void MemoryLoadElements(
            ElementMassMode elementMode = ElementMassMode.Average,
            short specificElement = 0,
            ElementStatsType specificStatToReset = ElementStatsType.Mass)
        {
            const double defaultChargeCarrierMassAvg = 1.00739d;
            const double defaultChargeCarrierMassMonoiso = 1.00727649d;

            // Data Load Statements
            // Uncertainties from CRC Handbook of Chemistry and Physics
            // For Radioactive elements, the most stable isotope is NOT used;
            // instead, an average Mol. Weight is used, just like with other elements.
            // Data obtained from the Perma-Chart Science Series periodic table, 1993.
            // Uncertainties from CRC Handbook of Chemistry and Physics, except for
            // Radioactive elements, where uncertainty was estimated to be .n5 where
            // specificElementProperty represents the number digits after the decimal point but before the last
            // number of the molecular weight.
            // For example, for No, MW = 259.1009 (±0.0005)

            // Define the charge carrier mass
            if (elementMode == ElementMassMode.Average)
            {
                SetChargeCarrierMass(defaultChargeCarrierMassAvg);
            }
            else
            {
                SetChargeCarrierMass(defaultChargeCarrierMassMonoiso);
            }

            // elementNames stores the element names
            // elemVals[elementIndex,1] stores the element's weight
            // elemVals[elementIndex,2] stores the element's uncertainty
            // elemVals[elementIndex,3] stores the element's charge
            // Note: We could make this array of type ElementInfo, but the size of this method would increase dramatically
            var elementMemoryData = ElementsLoader.MemoryLoadElements();

            // Set uncertainty to 0 for all elements if using exact isotopic or integer isotopic weights
            // Reduce branching - use Func<> to get the correct value based on the settings
            Func<ElementMem, double> getMass;
            Func<ElementMem, double> getUncertainty;
            switch (elementMode)
            {
                case ElementMassMode.Integer:
                    getMass = elementMem => elementMem.MassInteger;
                    getUncertainty = elementMem => 0;
                    break;
                case ElementMassMode.Isotopic:
                    getMass = elementMem => elementMem.MassIsotopic;
                    getUncertainty = elementMem => 0;
                    break;
                case ElementMassMode.Average:
                default:
                    getMass = elementMem => elementMem.MassAverage;
                    getUncertainty = elementMem => elementMem.UncertaintyAverageMass;
                    break;
            }

            // Set uncertainty to 0 for all elements if using exact isotopic or integer isotopic weights
            if (specificElement == 0)
            {
                // Updating all the elements
                mElementAlph.Clear();
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    var elementMem = elementMemoryData[elementIndex];
                    mElementStats[elementIndex] = new ElementInfo(elementMem.Symbol, elementMem.Charge, getMass(elementMem), getUncertainty(elementMem));

                    mElementAlph.Add(new KeyValuePair<string, int>(elementMem.Symbol, elementIndex));
                }

                // Alphabetize mElementAlph by Key/symbol
                mElementAlph.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));

                // Also load the isotopes, since if any were loaded we just cleared them.
                ElementsLoader.MemoryLoadIsotopes(mElementStats);
            }
            else if (specificElement >= 1 && specificElement <= ELEMENT_COUNT)
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
                    default:
                        // Ignore it
                        break;
                }
            }
        }

        /// <summary>
        /// Recomputes the Mass for all of the loaded abbreviations
        /// </summary>
        internal void RecomputeAbbreviationMasses()
        {
            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                Parser.ComputeAbbrevWeight(mAbbrevStats[index], true);
            }
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
        /// <returns>0 if found and removed; 1 if error</returns>
        internal int RemoveAbbreviation(string abbreviationSymbol)
        {
            var removed = default(bool);

            abbreviationSymbol = abbreviationSymbol?.ToLower();

            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if ((mAbbrevStats[index].Symbol?.ToLower() ?? "") == (abbreviationSymbol ?? ""))
                {
                    RemoveAbbreviationById(index);
                    removed = true;
                }
            }

            return removed ? 0 : 1;
        }

        /// <summary>
        /// Remove the abbreviation at index <paramref name="abbreviationId"/>
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        internal int RemoveAbbreviationById(int abbreviationId)
        {
            bool removed;

            if (abbreviationId >= 0 && abbreviationId < mAbbrevStats.Count)
            {
                mAbbrevStats.RemoveAt(abbreviationId);

                ConstructMasterSymbolsList();
                removed = true;
            }
            else
            {
                removed = false;
            }

            return removed ? 0 : 1;
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on <paramref name="symbol"/>)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetterSymbol"></param>
        /// <param name="comment"></param>
        /// <param name="validateFormula">If true, make sure the formula is valid</param>
        /// <returns>0 if successful, otherwise an error ID</returns>
        /// <remarks>
        /// It is useful to set <paramref name="validateFormula"/> = false when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
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
                if ((mAbbrevStats[index].Symbol?.ToUpper() ?? "") == (symbol?.ToUpper() ?? ""))
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
                SetAbbreviationById((short)abbrevId, symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
            }

            return errorId;
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on <paramref name="abbrevId"/>)
        /// </summary>
        /// <param name="abbrevId">If abbrevId is less than 1, adds as a new abbreviation</param>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetterSymbol"></param>
        /// <param name="comment"></param>
        /// <param name="validateFormula"></param>
        /// <returns>0 if successful, otherwise an error ID</returns>
        internal int SetAbbreviationById(
            short abbrevId, string symbol,
            string formula, float charge,
            bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true)
        {
            var invalidSymbolOrFormula = false;

            var errorId = 0;

            if (symbol.Length < 1)
            {
                // Symbol length is 0
                errorId = 192;
            }
            else if (symbol.Length > MAX_ABBREV_LENGTH)
            {
                // Abbreviation symbol too long
                errorId = 190;
            }
            else if (symbol.All(char.IsLetter))
            {
                if (formula.Length > 0)
                {
                    // Convert symbol to proper case mode
                    symbol = symbol.Substring(0, 1).ToUpper() + symbol.Substring(1).ToLower();

                    // If abbrevId is < 1 or larger than AbbrevAllCount, then define it
                    if (abbrevId < 0 || abbrevId >= mAbbrevStats.Count)
                    {
                        if (mAbbrevStats.Count < MAX_ABBREV_COUNT)
                        {
                            abbrevId = (short)mAbbrevStats.Count;
                        }
                        else
                        {
                            // Too many abbreviations
                            errorId = 196;
                            abbrevId = -1;
                        }
                    }

                    if (abbrevId >= 1)
                    {
                        // Make sure the abbreviation doesn't match one of the standard elements
                        var symbolMatchType = CheckElemAndAbbrev(symbol, out var symbolReference);

                        if (symbolMatchType == SymbolMatchMode.Element)
                        {
                            if ((mElementStats[symbolReference].Symbol ?? "") == symbol)
                            {
                                invalidSymbolOrFormula = true;
                            }
                        }

                        if (!invalidSymbolOrFormula && validateFormula)
                        {
                            // Make sure the abbreviation's formula is valid
                            // This will also auto-capitalize the formula if auto-capitalize is turned on
                            var computationStats = new ComputationStats();
                            Parser.ParseFormulaPublic(ref formula, computationStats);

                            errorId = mMassCalc.GetErrorId();
                            if (errorId != 0)
                            {
                                // An error occurred while parsing
                                // Already present in ErrorParams.ErrorID
                                // We'll still add the formula, but mark it as invalid
                                invalidSymbolOrFormula = true;
                            }
                        }

                        AddAbbreviationWork(abbrevId, symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, invalidSymbolOrFormula);

                        ConstructMasterSymbolsList();
                    }
                }
                else
                {
                    // Invalid formula (actually, blank formula)
                    errorId = 160;
                }
            }
            else
            {
                // Symbol does not just contain letters
                errorId = 194;
            }

            return errorId;
        }

        internal void SetChargeCarrierMass(double mass)
        {
            mChargeCarrierMass = mass;
        }

        /// <summary>
        /// Update the values for a single element (based on <paramref name="symbol"/>)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
        /// <param name="charge"></param>
        /// <param name="recomputeAbbreviationMasses">Set to False if updating several elements</param>
        /// <returns></returns>
        internal int SetElement(string symbol, double mass,
            double uncertainty, float charge,
            bool recomputeAbbreviationMasses = true)
        {
            var found = default(bool);

            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                if ((symbol?.ToLower() ?? "") == (mElementStats[index].Symbol?.ToLower() ?? ""))
                {
                    var stats = mElementStats[index];
                    stats.Mass = mass;
                    stats.Uncertainty = uncertainty;
                    stats.Charge = charge;

                    found = true;
                    break;
                }
            }

            if (found)
            {
                if (recomputeAbbreviationMasses)
                    RecomputeAbbreviationMasses();
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Set the isotopes for the element
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses">0-based array</param>
        /// <param name="isotopeAbundances">0-based array</param>
        /// <returns>0 if successful, 1 if symbol not found</returns>
        internal int SetElementIsotopes(string symbol, short isotopeCount, double[] isotopeMasses, float[] isotopeAbundances)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return 1;
            }

            var found = default(bool);

            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                if (string.Equals(symbol.ToLower(), mElementStats[index].Symbol.ToLower(), StringComparison.OrdinalIgnoreCase))
                {
                    var stats = mElementStats[index];
                    stats.Isotopes.Clear();
                    for (var isotopeIndex = 0; isotopeIndex < isotopeMasses.Length; isotopeIndex++)
                    {
                        if (isotopeIndex > MAX_ISOTOPES)
                            break;
                        stats.Isotopes.Add(new IsotopeInfo(isotopeMasses[isotopeIndex], isotopeAbundances[isotopeIndex]));
                    }

                    stats.Isotopes.Capacity = stats.Isotopes.Count;

                    found = true;
                    break;
                }
            }

            return found ? 0 : 1;
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
                if (newElementMode < ElementMassMode.Average || newElementMode > ElementMassMode.Integer)
                {
                    return;
                }

                if (newElementMode != mCurrentElementMode || forceMemoryLoadElementValues || mElementAlph.Count == 0)
                {
                    mCurrentElementMode = newElementMode;

                    MemoryLoadElements(mCurrentElementMode);
                    ConstructMasterSymbolsList();
                    RecomputeAbbreviationMasses();
                }
            }
            catch (Exception ex)
            {
                mMassCalc.GeneralErrorHandler("ElementAndMassTools.SetElementModeInternal", ex);
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
            var invalidAbbreviationCount = default(short);

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
