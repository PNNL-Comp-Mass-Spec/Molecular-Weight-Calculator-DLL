﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;
using MolecularWeightCalculator.EventLogging;
using MolecularWeightCalculator.Formula;

namespace MolecularWeightCalculator.Sequence
{
    /// <summary>
    /// Molecular Weight Calculator routines with ActiveX Class interfaces: Peptide
    /// </summary>
    [Guid("1E33887D-563A-4A5F-909B-A3DF18E03EDC"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class Peptide : EventReporter, IPeptide
    {
        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004
        // Converted to C# by Bryson Gibbons in 2021
        // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
        // Website: https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL
        // -------------------------------------------------------------------------------
        //
        // Licensed under the Apache License, Version 2.0; you may not use this file except
        // in compliance with the License.  You may obtain a copy of the License at
        // http://www.apache.org/licenses/LICENSE-2.0
        //
        // Notice: This computer software was prepared by Battelle Memorial Institute,
        // hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
        // Department of Energy (DOE).  All rights in the computer software are reserved
        // by DOE on behalf of the United States Government and the Contractor as
        // provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY
        // WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS
        // SOFTWARE.  This notice including this sentence must appear on any copies of
        // this computer software.

        // Ignore Spelling: Acetyl, acrylamide, Bryson, Carbamidomethylation, carbamyl, Carboxymethylation, Cleavable, Da
        // Ignore Spelling: frag, immonium, methylation, Phospho, phosphorylated, phosphorylates, phosphorylation
        // Ignore Spelling: Proline, pyro, terminii, tryptic, Xxx
        // Ignore Spelling: Arg, Asn, Gln, Glu, Gly, Leu, Lys, Phe, Ser, Thr, Tyr

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elementAndMassTools"></param>
        public Peptide(ElementAndMassTools elementAndMassTools = null)
        {
            mElementAndMassRoutines = elementAndMassTools ?? new ElementAndMassTools();

            try
            {
                mResidues = new List<Residue>(50);
                mModificationSymbols = new List<ModificationSymbol>(10);

                SetDefaultOptions();
            }
            catch (Exception ex)
            {
                OnErrorEvent(Logging.GeneralErrorHandler("Peptide.Constructor", ex));
            }
        }

        /// <summary>
        /// Default intensity for b/y shoulder ions
        /// </summary>
        public const float DEFAULT_B_Y_ION_SHOULDER_INTENSITY = 50f;

        /// <summary>
        /// Default intensity for b/y/c/z ions
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public const float DEFAULT_BYCZ_ION_INTENSITY = 100f;

        /// <summary>
        /// Default intensity for a ions
        /// </summary>
        public const float DEFAULT_A_ION_INTENSITY = 20f;

        /// <summary>
        /// Default intensity for neutral loss ions
        /// </summary>
        public const float DEFAULT_NEUTRAL_LOSS_ION_INTENSITY = 20f;

        /// <summary>
        /// Default m/z threshold for reporting ions as 2+ species
        /// </summary>
        public const float DEFAULT_DOUBLE_CHARGE_MZ_THRESHOLD = 800f;

        /// <summary>
        /// Default m/z threshold for reporting ions as 3+ species
        /// </summary>
        public const float DEFAULT_TRIPLE_CHARGE_MZ_THRESHOLD = 900f;

        /// <summary>
        ///  Maximum number of modifications for a single residue
        /// </summary>
        internal const short MAX_MODIFICATIONS = 6;

        /// <summary>
        /// Unknown amino acid symbol, 3 letter notation
        /// </summary>
        private const string UNKNOWN_SYMBOL = "Xxx";

        /// <summary>
        /// Unknown amino acid symbol, 1 letter notation
        /// </summary>
        private const string UNKNOWN_SYMBOL_ONE_LETTER = "X";

        internal const string TERMINII_SYMBOL = "-";
        internal const string TRYPTIC_RULE_RESIDUES = "KR";
        internal const string TRYPTIC_EXCEPTION_RESIDUES = "P";

        private const string SHOULDER_ION_PREFIX = "Shoulder-";

        private readonly ElementAndMassTools mElementAndMassRoutines;

        private const IonType ION_TYPE_MAX = IonType.ZIon;

        /// <summary>
        /// Peptide residues
        /// </summary>
        /// <remarks>
        /// A peptide goes from N to C, e.g. HGlyLeuTyrOH has N-Terminus = H and C-Terminus = OH
        /// mResidues[0] = "Gly"
        /// mResidues[1] = "Leu"
        /// mResidues[2] = "Tyr"
        /// </remarks>
        private readonly List<Residue> mResidues;

        /// <summary>
        /// Tracks potential modification symbols and the mass of each modification
        /// </summary>
        /// <remarks>
        /// Modification symbols are typically a single character, but we allow for multiple characters for the symbol
        /// </remarks>
        private readonly List<ModificationSymbol> mModificationSymbols;

        /// <summary>
        /// Formula on the N-Terminus
        /// </summary>
        private readonly Terminus mNTerminus = new();

        /// <summary>
        /// Formula on the C-Terminus
        /// </summary>
        private readonly Terminus mCTerminus = new();

        /// <summary>
        /// Peptide mass, including modifications
        /// </summary>
        private double mTotalMass;

        /// <summary>
        /// Ammonia loss: -NH3
        /// </summary>
        private string mAmmoniaLossSymbol;

        /// <summary>
        /// Phospho loss: -H3PO4
        /// </summary>
        private string mPhosphoLossSymbol;

        /// <summary>
        /// Water loss: -H2O
        /// </summary>
        private string mWaterLossSymbol;

        private FragmentationSpectrumOptions mFragSpectrumOptions = new();

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Mass of ammonia
        /// </summary>
        private double mMassNH3;

        /// <summary>
        /// Mass of the phospho group, including water
        /// </summary>
        private double mMassH3PO4;

        /// <summary>
        /// Mass of water
        /// </summary>
        private double mMassHOH;

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Mass of the phospho group, excluding water
        /// </summary>
        /// <remarks>
        /// H3PO4 minus HOH = 79.9663326
        /// </remarks>
        private double mMassPhosphorylation;

        /// <summary>
        /// Mass of hydrogen
        /// </summary>
        private double mMassHydrogen;

        /// <summary>
        /// Charge carrier mass
        /// </summary>
        /// <remarks>
        /// Typically H minus one electron
        /// </remarks>
        private double mChargeCarrierMass;

        /// <summary>
        /// Immonium mass difference
        /// </summary>
        /// <remarks>
        /// CO minus H = 26.9871
        /// </remarks>
        private double mImmoniumMassDifference;

        private bool mDelayUpdateResidueMass;

        /// <summary>
        /// Examines partialSequence to see if it contains 1 or more modifications
        /// If any modification symbols are found, the modification is recorded in .ModificationIDs[]
        /// If all or part of the modification symbol is not found in ModificationSymbols[], a new entry is added to ModificationSymbols[]
        /// </summary>
        /// <param name="partialSequence"></param>
        /// <param name="residueIndex"></param>
        /// <param name="addMissingModificationSymbols"></param>
        /// <returns>Total length of all modifications found</returns>
        private int CheckForModifications(string partialSequence, int residueIndex, bool addMissingModificationSymbols = false)
        {
            var sequenceStrLength = partialSequence.Length;

            // Find the entire group of potential modification symbols
            var modSymbolGroup = string.Empty;

            for (var compareIndex = 0; compareIndex < sequenceStrLength; compareIndex++)
            {
                var testChar = partialSequence.Substring(compareIndex, 1);
                if (mElementAndMassRoutines.IsModSymbol(testChar))
                {
                    modSymbolGroup += testChar;
                }
                else
                {
                    break;
                }
            }

            var modSymbolLengthTotal = modSymbolGroup.Length;

            while (modSymbolGroup.Length > 0)
            {
                // Step through modSymbolGroup to see if all of it or parts of it match any of the defined
                // modification symbols

                var modificationId = 0;
                var matchFound = false;
                int subPartLength;
                for (subPartLength = modSymbolGroup.Length; subPartLength >= 1; --subPartLength)
                {
                    // See if the modification is already defined
                    modificationId = GetModificationSymbolId(modSymbolGroup.Substring(0, subPartLength));
                    if (modificationId >= 0)
                    {
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    if (addMissingModificationSymbols)
                    {
                        // Add modSymbolGroup as a new modification, using a mass of 0 since we don't know the modification mass
                        SetModificationSymbol(modSymbolGroup, 0d);
                        matchFound = true;
                    }

                    modSymbolGroup = string.Empty;
                }

                if (matchFound)
                {
                    // Record the modification for this residue
                    var residue = mResidues[residueIndex];
                    if (residue.ModificationIDs.Count < MAX_MODIFICATIONS)
                    {
                        residue.ModificationIDs.Add(modificationId);
                        if (mModificationSymbols[modificationId].IndicatesPhosphorylation)
                        {
                            residue.Phosphorylated = true;
                        }
                    }

                    if (subPartLength < modSymbolGroup.Length)
                    {
                        // Remove the matched portion from modSymbolGroup and test again
                        modSymbolGroup = modSymbolGroup.Substring(subPartLength);
                    }
                    else
                    {
                        modSymbolGroup = string.Empty;
                    }
                }
            }

            return modSymbolLengthTotal;
        }

        private short ComputeMaxIonsPerResidue()
        {
            // Estimate the total ions per residue that will be created
            // This number will nearly always be much higher than the number of ions that will actually
            // be stored for a given sequence, since not all will be doubly charged, and not all will show
            // all of the potential neutral losses

            short ionCount = 0;

            for (IonType ionIndex = 0; ionIndex <= ION_TYPE_MAX; ionIndex++)
            {
                if (mFragSpectrumOptions.IonTypeOptions[(int)ionIndex].ShowIon)
                {
                    ionCount++;
                    if (Math.Abs(mFragSpectrumOptions.IntensityOptions.BYIonShoulder) > 0d)
                    {
                        if (ionIndex is IonType.BIon or IonType.YIon or IonType.CIon or IonType.ZIon)
                        {
                            ionCount += 2;
                        }
                    }

                    if (mFragSpectrumOptions.IonTypeOptions[(int)ionIndex].NeutralLossAmmonia)
                        ionCount++;
                    if (mFragSpectrumOptions.IonTypeOptions[(int)ionIndex].NeutralLossPhosphate)
                        ionCount++;
                    if (mFragSpectrumOptions.IonTypeOptions[(int)ionIndex].NeutralLossWater)
                        ionCount++;
                }
            }

            // Double Charge ions could be created for all ions, so simply double ionCount
            if (mFragSpectrumOptions.DoubleChargeIonsShow)
            {
                ionCount = (short)(ionCount * 2);
            }

            if (mFragSpectrumOptions.TripleChargeIonsShow)
            {
                ionCount = (short)(ionCount * 2);
            }

            return ionCount;
        }

        /// <summary>
        /// Obtain an instance of Residue containing symbol as the residue symbol
        /// If symbol is a valid amino acid type, also updates residue with the default information
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>Residue instance</returns>
        private Residue FillResidueStructureUsingSymbol(string symbol, bool use3LetterCode = true)
        {
            int abbrevId;
            var symbol3Letter = string.Empty;

            if (symbol.Length > 0)
            {
                if (use3LetterCode)
                {
                    symbol3Letter = symbol;
                }
                else
                {
                    symbol3Letter = mElementAndMassRoutines.Elements.GetAminoAcidSymbolConversion(symbol, true);
                    if (symbol3Letter.Length == 0)
                    {
                        symbol3Letter = symbol;
                    }
                }

                abbrevId = mElementAndMassRoutines.Elements.GetAbbreviationId(symbol3Letter, true);
            }
            else
            {
                abbrevId = -1;
            }

            var residue = new Residue(symbol3Letter) {
                Phosphorylated = false
            };

            if (abbrevId >= 0)
            {
                residue.Mass = mElementAndMassRoutines.Elements.GetAbbreviationMass(abbrevId);
            }
            else
            {
                residue.Mass = 0d;
            }

            residue.MassWithMods = residue.Mass;

            return residue;
        }

        /// <summary>
        /// Get fragmentation masses
        /// </summary>
        /// <param name="fragSpectrum"></param>
        /// <returns>The number of ions in fragSpectrum[]</returns>
        public int GetFragmentationMasses(out FragmentationSpectrumData[] fragSpectrum)
        {
            var fragSpectraData = GetFragmentationMasses();

            if (fragSpectraData.Count == 0)
            {
                fragSpectrum = new FragmentationSpectrumData[1];
                fragSpectrum[0] = new FragmentationSpectrumData();
                return 0;
            }

            fragSpectrum = fragSpectraData.ToArray();

            return fragSpectraData.Count;
        }

        /// <summary>
        /// Get the theoretical MS/MS fragmentation ions for the current peptide sequence
        /// </summary>
        /// <remarks>
        /// Use <see cref="SetFragmentationSpectrumOptions"/> to configure the ion types to return
        /// </remarks>
        /// <returns>List of peptide fragments</returns>
        public List<FragmentationSpectrumData> GetFragmentationMasses()
        {
            const int maxCharge = 3;

            var ionIntensities = new float[5];

            if (mResidues.Count == 0)
            {
                // No residues
                return new List<FragmentationSpectrumData>();
            }

            var showCharge = new bool[4];
            var chargeThreshold = new double[4];

            // Copy some of the values from mFragSpectrumOptions to local variables to make things easier to read
            for (IonType ionType = 0; ionType <= ION_TYPE_MAX; ionType++)
            {
                ionIntensities[(int)ionType] = (float)mFragSpectrumOptions.IntensityOptions.IonType[(int)ionType];
            }

            var ionShoulderIntensity = (float)mFragSpectrumOptions.IntensityOptions.BYIonShoulder;
            var neutralLossIntensity = (float)mFragSpectrumOptions.IntensityOptions.NeutralLoss;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (maxCharge >= 2)
            {
                showCharge[2] = mFragSpectrumOptions.DoubleChargeIonsShow;
                chargeThreshold[2] = mFragSpectrumOptions.DoubleChargeIonsThreshold;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (maxCharge >= 3)
            {
                showCharge[3] = mFragSpectrumOptions.TripleChargeIonsShow;
                chargeThreshold[3] = mFragSpectrumOptions.TripleChargeIonsThreshold;
            }

            // Populate ionMassesZeroBased[] and ionIntensitiesZeroBased[]
            // Put ion descriptions in ionSymbolsZeroBased
            var predictedIonCount = GetFragmentationSpectrumRequiredDataPoints();

            if (predictedIonCount == 0)
                predictedIonCount = mResidues.Count;
            var fragSpectrumWork = new List<FragmentationSpectrumData>(predictedIonCount + 1);

            // Need to update the residue masses in case the modifications have changed
            UpdateResidueMasses();

            for (var residueIndex = 0; residueIndex < mResidues.Count; residueIndex++)
            {
                var residue = mResidues[residueIndex];

                for (IonType ionType = 0; ionType <= ION_TYPE_MAX; ionType++)
                {
                    if (mFragSpectrumOptions.IonTypeOptions[(int)ionType].ShowIon)
                    {
                        if ((residueIndex == 0 || residueIndex == mResidues.Count - 1) && (ionType is IonType.AIon or IonType.BIon or IonType.CIon))
                        {
                            // Don't include a, b, or c ions in the output masses for this residue
                        }
                        else
                        {
                            // Ion is used
                            var baseMass = (float)residue.IonMass[(int)ionType];
                            var intensity = ionIntensities[(int)ionType];

                            // Get the list of residues preceding or following this residue
                            // Note that the residue symbols are separated by a space to avoid accidental matching when using IndexOf()
                            var residues = GetInternalResidues(residueIndex, ionType, out var phosphorylated);

                            for (short chargeIndex = 1; chargeIndex <= maxCharge; chargeIndex++)
                            {
                                if (chargeIndex == 1 || chargeIndex > 1 && showCharge[chargeIndex])
                                {
                                    float convolutedMass;
                                    if (chargeIndex == 1)
                                    {
                                        convolutedMass = baseMass;
                                    }
                                    else
                                    {
                                        // Compute mass at higher charge
                                        convolutedMass = (float)mElementAndMassRoutines.ConvoluteMass(baseMass, 1, chargeIndex, mChargeCarrierMass);
                                    }

                                    if (chargeIndex > 1 && baseMass < chargeThreshold[chargeIndex])
                                    {
                                        // BaseMass is below threshold, do not add to Predicted Spectrum
                                    }
                                    else
                                    {
                                        // Add ion to Predicted Spectrum

                                        // Y and Z Ions are numbered in decreasing order: y5, y4, y3, y2, y1
                                        // A, B, and C ions are numbered in increasing order: a1, a2, etc.  or b1, b2, etc.
                                        var ionSymbolGeneric = LookupIonTypeString(ionType);
                                        string ionSymbol;
                                        if (ionType is IonType.YIon or IonType.ZIon)
                                        {
                                            ionSymbol = ionSymbolGeneric + (mResidues.Count - residueIndex);
                                        }
                                        else
                                        {
                                            ionSymbol = ionSymbolGeneric + (residueIndex + 1);
                                        }

                                        if (chargeIndex > 1)
                                        {
                                            ionSymbol += new string('+', chargeIndex);
                                            ionSymbolGeneric += new string('+', chargeIndex);
                                        }

                                        fragSpectrumWork.Add(new FragmentationSpectrumData(
                                            convolutedMass, intensity, ionSymbol, ionSymbolGeneric,
                                            residueIndex, residue.Symbol,
                                            chargeIndex, ionType));

                                        // Add shoulder ions to PredictedSpectrum() if a B, Y, C, or Z ion and the shoulder intensity is > 0
                                        // Need to use Abs() here since user can define negative theoretical intensities (which allows for plotting a spectrum inverted)
                                        float observedMass;
                                        if (Math.Abs(ionShoulderIntensity) > 0f && ionType is IonType.BIon or IonType.YIon or IonType.CIon or IonType.ZIon)
                                        {
                                            for (var shoulderIndex = -1; shoulderIndex <= 1; shoulderIndex += 2)
                                            {
                                                observedMass = (float)(convolutedMass + shoulderIndex * (1d / chargeIndex));
                                                fragSpectrumWork.Add(new FragmentationSpectrumData(
                                                    observedMass, ionShoulderIntensity,
                                                    SHOULDER_ION_PREFIX + ionSymbol,
                                                    SHOULDER_ION_PREFIX + ionSymbolGeneric,
                                                    residueIndex, residue.Symbol,
                                                    chargeIndex, ionType, true));
                                            }
                                        }

                                        // Apply neutral loss modifications
                                        if (mFragSpectrumOptions.IonTypeOptions[(int)ionType].NeutralLossWater)
                                        {
                                            // Loss of water only affects Ser, Thr, Asp, or Glu (S, T, E, or D)
                                            // See if the residues up to this point contain any of these residues
                                            if (residues.Contains("Ser") || residues.Contains("Thr") || residues.Contains("Glu") || residues.Contains("Asp"))
                                            {
                                                observedMass = (float)(convolutedMass - mMassHOH / chargeIndex);
                                                fragSpectrumWork.Add(new FragmentationSpectrumData(
                                                    observedMass, neutralLossIntensity,
                                                    ionSymbol + mWaterLossSymbol,
                                                    ionSymbolGeneric + mWaterLossSymbol,
                                                    residueIndex, residue.Symbol,
                                                    chargeIndex, ionType));
                                            }
                                        }

                                        if (mFragSpectrumOptions.IonTypeOptions[(int)ionType].NeutralLossAmmonia)
                                        {
                                            // Loss of Ammonia only affects Arg, Lys, Gln, or Asn (R, K, Q, or N)
                                            // See if the residues up to this point contain any of these residues
                                            if (residues.Contains("Arg") || residues.Contains("Lys") || residues.Contains("Gln") || residues.Contains("Asn"))
                                            {
                                                observedMass = (float)(convolutedMass - mMassNH3 / chargeIndex);
                                                fragSpectrumWork.Add(new FragmentationSpectrumData(
                                                    observedMass, neutralLossIntensity,
                                                    ionSymbol + mAmmoniaLossSymbol,
                                                    ionSymbolGeneric + mAmmoniaLossSymbol,
                                                    residueIndex, residue.Symbol,
                                                    chargeIndex, ionType));
                                            }
                                        }

                                        if (mFragSpectrumOptions.IonTypeOptions[(int)ionType].NeutralLossPhosphate)
                                        {
                                            // Loss of phosphate only affects phosphorylated residues
                                            // Technically, only Ser, Thr, or Tyr (S, T, or Y) can be phosphorylated, but if the user marks other residues as phosphorylated, we'll allow that
                                            // See if the residues up to this point contain phosphorylated residues
                                            if (phosphorylated)
                                            {
                                                observedMass = (float)(convolutedMass - mMassH3PO4 / chargeIndex);
                                                fragSpectrumWork.Add(new FragmentationSpectrumData(
                                                    observedMass, neutralLossIntensity,
                                                    ionSymbol + mPhosphoLossSymbol,
                                                    ionSymbolGeneric + mPhosphoLossSymbol,
                                                    residueIndex, residue.Symbol,
                                                    chargeIndex, ionType));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Sort frag spectra by mass (using a pointer array to synchronize the arrays)
            // Uses the default comparator defined for FragmentationSpectrumData, which sorts by mass
            fragSpectrumWork.Sort();

            return fragSpectrumWork;
        }

        /// <summary>
        /// Get the maximum number of data points that will be required for a theoretical fragmentation spectrum
        /// </summary>
        public int GetFragmentationSpectrumRequiredDataPoints()
        {
            return mResidues.Count * ComputeMaxIonsPerResidue();
        }

        /// <summary>
        /// Get the MS/MS fragmentation options
        /// </summary>
        public FragmentationSpectrumOptions GetFragmentationSpectrumOptions()
        {
            return mFragSpectrumOptions ?? new FragmentationSpectrumOptions();
        }

        /// <summary>
        /// Get the mass of the entire peptide, including modifications
        /// </summary>
        /// <returns>Mass, in Da</returns>
        public double GetPeptideMass()
        {
            // Update the residue masses in order to update mTotalMass
            UpdateResidueMasses();

            return mTotalMass;
        }

        /// <summary>
        /// Determines the residues preceding or following the given residue (up to and including the current residue)
        /// If ionType is a, b, or c ions, returns residues from the N terminus
        /// If ionType is y or ions, returns residues from the C terminus
        /// Sets phosphorylated to true if any of the residues is Ser, Thr, or Tyr and is phosphorylated
        /// </summary>
        /// <remarks>
        /// Residue symbols are separated by a space to avoid accidental matching when using IndexOf()
        /// </remarks>
        /// <param name="currentResidueIndex"></param>
        /// <param name="ionType"></param>
        /// <param name="phosphorylated">Output: true if a phosphorylated S, T, or Y</param>
        /// <returns>Space separated list of residue symbols (three letter notation)</returns>
        private string GetInternalResidues(int currentResidueIndex, IonType ionType, out bool phosphorylated)
        {
            var internalResidues = string.Empty;
            phosphorylated = false;

            if (ionType is IonType.YIon or IonType.ZIon)
            {
                for (var residueIndex = currentResidueIndex; residueIndex < mResidues.Count; residueIndex++)
                {
                    internalResidues += mResidues[residueIndex].Symbol + " ";
                    if (mResidues[residueIndex].Phosphorylated)
                        phosphorylated = true;
                }
            }
            else
            {
                for (var residueIndex = 0; residueIndex <= currentResidueIndex; residueIndex++)
                {
                    internalResidues += mResidues[residueIndex].Symbol + " ";
                    if (mResidues[residueIndex].Phosphorylated)
                        phosphorylated = true;
                }
            }

            return internalResidues;
        }

        /// <summary>
        /// Get information on the modification with modificationId
        /// </summary>
        /// <param name="modificationId"></param>
        /// <param name="modSymbol"></param>
        /// <param name="modificationMass"></param>
        /// <param name="indicatesPhosphorylation"></param>
        /// <param name="comment"></param>
        /// <returns>True if success, false if modificationId is invalid</returns>
        public bool GetModificationSymbol(
            int modificationId,
            out string modSymbol,
            out double modificationMass,
            out bool indicatesPhosphorylation,
            out string comment)
        {
            if (modificationId >= 0 && modificationId < mModificationSymbols.Count)
            {
                var mod = mModificationSymbols[modificationId];
                modSymbol = mod.Symbol;
                modificationMass = mod.ModificationMass;
                indicatesPhosphorylation = mod.IndicatesPhosphorylation;
                comment = mod.Comment;

                return true;
            }

            modSymbol = string.Empty;
            modificationMass = 0d;
            indicatesPhosphorylation = false;
            comment = string.Empty;
            return false;
        }

        /// <summary>
        /// The number of modifications defined
        /// </summary>
        public int GetModificationSymbolCount()
        {
            return mModificationSymbols.Count;
        }

        /// <summary>
        /// Get the modification symbol ID for the given modification
        /// </summary>
        /// <param name="modSymbol"></param>
        /// <returns>Id if found, -1 if not found</returns>
        public int GetModificationSymbolId(string modSymbol)
        {
            if (string.IsNullOrWhiteSpace(modSymbol))
            {
                return -1;
            }

            var modificationIdMatch = -1;

            for (var index = 0; index < mModificationSymbols.Count; index++)
            {
                if (mModificationSymbols[index].Symbol == modSymbol)
                {
                    modificationIdMatch = index;
                    break;
                }
            }

            return modificationIdMatch;
        }

        /// <summary>
        /// Get information about the residue at the given index
        /// </summary>
        /// <param name="residueIndex"></param>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="isModified"></param>
        /// <param name="modificationCount"></param>
        /// <returns>True if success, false if residueIndex is invalid</returns>
        public bool GetResidue(int residueIndex, out string symbol, out double mass, out bool isModified, out short modificationCount)
        {
            if (residueIndex >= 0 && residueIndex < mResidues.Count)
            {
                var residue = mResidues[residueIndex];
                symbol = residue.Symbol;
                mass = residue.Mass;
                isModified = residue.ModificationIDs.Count > 0;
                modificationCount = (short)residue.ModificationIDs.Count;

                return true;
            }

            symbol = string.Empty;
            mass = 0;
            isModified = false;
            modificationCount = 0;
            return false;
        }

        /// <summary>
        /// Get the number of residues in this peptide
        /// </summary>
        public int GetResidueCount()
        {
            return mResidues.Count;
        }

        /// <summary>
        /// Determine the number of occurrences of the given residue in the loaded sequence
        /// </summary>
        /// <param name="residueSymbol"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>Count of matching residues</returns>
        public int GetResidueCountSpecificResidue(string residueSymbol, bool use3LetterCode)
        {
            if (string.IsNullOrWhiteSpace(residueSymbol))
            {
                return 0;
            }

            string searchResidue3Letter;

            if (use3LetterCode)
            {
                searchResidue3Letter = residueSymbol;
            }
            else
            {
                searchResidue3Letter = mElementAndMassRoutines.Elements.GetAminoAcidSymbolConversion(residueSymbol, true);
            }

            var residueCount = 0;
            foreach (var residue in mResidues)
            {
                if (residue.Symbol == searchResidue3Letter)
                {
                    residueCount++;
                }
            }

            return residueCount;
        }

        /// <summary>
        /// Get the array of modification IDs
        /// </summary>
        /// <param name="residueIndex"></param>
        /// <param name="modificationIDs"></param>
        /// <returns>The number of modifications</returns>
        public int GetResidueModificationIDs(int residueIndex, out int[] modificationIDs)
        {
            if (residueIndex >= 0 && residueIndex < mResidues.Count)
            {
                var residue = mResidues[residueIndex];

                modificationIDs = new int[residue.ModificationIDs.Count];

                for (var index = 0; index < residue.ModificationIDs.Count; index++)
                {
                    modificationIDs[index] = residue.ModificationIDs[index];
                }

                return residue.ModificationIDs.Count;
            }

            modificationIDs = Array.Empty<int>();

            return 0;
        }

        /// <summary>
        /// Obtain the symbol at the given residue number, or string.Empty if an invalid residue number
        /// </summary>
        /// <param name="residueIndex"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>Symbol if residueIndex is valid, otherwise an empty string</returns>
        public string GetResidueSymbolOnly(int residueIndex, bool use3LetterCode)
        {
            var symbol = string.Empty;

            if (residueIndex >= 0 && residueIndex < mResidues.Count)
            {
                symbol = mResidues[residueIndex].Symbol;

                if (!use3LetterCode)
                    symbol = mElementAndMassRoutines.Elements.GetAminoAcidSymbolConversion(symbol, false);
            }

            return symbol;
        }

        /// <summary>
        /// Get the sequence of the current peptide, using 1-letter amino acid symbols
        /// </summary>
        public string GetSequence1LetterCode()
        {
            return GetSequence(false);
        }

        /// <summary>
        /// Get the sequence of the current peptide, formatting as specified
        /// </summary>
        /// <param name="use3LetterCode"></param>
        /// <param name="addSpaceEvery10Residues"></param>
        /// <param name="separateResiduesWithDash"></param>
        /// <param name="includeNAndCTerminii"></param>
        /// <param name="includeModificationSymbols"></param>
        public string GetSequence(
            bool use3LetterCode = true,
            bool addSpaceEvery10Residues = false,
            bool separateResiduesWithDash = false,
            bool includeNAndCTerminii = false,
            bool includeModificationSymbols = true)
        {
            // Construct a text sequence using Residues[] and the N and C Terminus info

            string dashAdd;

            if (separateResiduesWithDash)
                dashAdd = "-";
            else
                dashAdd = string.Empty;

            var sequence = string.Empty;
            for (var index = 0; index < mResidues.Count; index++)
            {
                var residue = mResidues[index];
                var symbol3Letter = residue.Symbol;
                if (use3LetterCode)
                {
                    sequence += symbol3Letter;
                }
                else
                {
                    var symbol1Letter = mElementAndMassRoutines.Elements.GetAminoAcidSymbolConversion(symbol3Letter, false);
                    if (symbol1Letter?.Length == 0)
                        symbol1Letter = UNKNOWN_SYMBOL_ONE_LETTER;
                    sequence += symbol1Letter;
                }

                if (includeModificationSymbols)
                {
                    foreach (var modificationId in residue.ModificationIDs)
                    {
                        var success = GetModificationSymbol(modificationId, out var modSymbol, out _, out _, out _);
                        if (success)
                        {
                            sequence += modSymbol;
                        }
                        else
                        {
                            Console.WriteLine("GetModificationSymbol returned false for modificationId {0} in GetSequence", modificationId);
                        }
                    }
                }

                if (index != mResidues.Count - 1)
                {
                    if (addSpaceEvery10Residues)
                    {
                        if (index % 10 == 9) // 0-based index: mod-10 == 9 will do every 10th residue, starting with the 10th
                        {
                            sequence += " ";
                        }
                        else
                        {
                            sequence += dashAdd;
                        }
                    }
                    else
                    {
                        sequence += dashAdd;
                    }
                }
            }

            if (includeNAndCTerminii)
            {
                sequence = mNTerminus.Formula + dashAdd + sequence + dashAdd + mCTerminus.Formula;
            }

            return sequence;
        }

        /// <summary>
        /// Get the ammonia loss formula
        /// </summary>
        /// <remarks>
        /// Defaults to -NH3
        /// </remarks>
        public string GetSymbolAmmoniaLoss()
        {
            return mAmmoniaLossSymbol;
        }

        /// <summary>
        /// Get the phosphorylation loss formula
        /// </summary>
        /// <remarks>
        /// Defaults to -H3PO4
        /// </remarks>
        public string GetSymbolPhosphoLoss()
        {
            return mPhosphoLossSymbol;
        }

        /// <summary>
        /// Get the water loss formula
        /// </summary>
        /// <remarks>
        /// Defaults to -H2O
        /// </remarks>
        public string GetSymbolWaterLoss()
        {
            return mWaterLossSymbol;
        }

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Constructs a name string based on their position and based on whether the fragment is truly tryptic
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="proteinSearchStartLoc"></param>
        public string GetTrypticName(string proteinResidues, string peptideResidues, int proteinSearchStartLoc = 1)
        {
            return GetTrypticName(proteinResidues, peptideResidues, out _, out _, proteinSearchStartLoc: proteinSearchStartLoc);
        }

        // ReSharper disable CommentTypo

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Constructs a name string based on their position and based on whether the fragment is truly tryptic
        /// In addition, returns the position of the first and last residue in <paramref name="returnResidueStart"/> and <paramref name="returnResidueEnd"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// The tryptic name is in the following format:
        ///   t1  indicates tryptic peptide 1
        ///   t2 represents tryptic peptide 2, etc.
        ///   t1.2  indicates tryptic peptide 1, plus one more tryptic peptide, i.e. t1 and t2
        ///   t5.2  indicates tryptic peptide 5, plus one more tryptic peptide, i.e. t5 and t6
        ///   t5.3  indicates tryptic peptide 5, plus two more tryptic peptides, i.e. t5, t6, and t7
        ///   40.52  means that the residues are not tryptic, and simply range from residue 40 to 52
        /// </para>
        /// <para>
        /// If the peptide residues are not present in proteinResidues, returns an empty string
        /// </para>
        /// <para>
        /// Since a peptide can occur multiple times in a protein, you can set proteinSearchStartLoc to a value larger than 1 to ignore previous hits
        /// </para>
        /// <para>
        /// If ICR2LSCompatible is True, the values returned when a peptide is not tryptic are modified to
        /// range from the starting residue to the ending residue +1
        /// </para>
        /// <para>
        /// returnResidueEnd is always equal to the position of the final residue, regardless of ICR2LSCompatible
        /// </para>
        /// <para>
        /// For example, if proteinResidues = "IGKANR"
        ///   when peptideResidues = "IGK",  the TrypticName is t1
        ///   when peptideResidues = "ANR",  the TrypticName is t2
        ///   when peptideResidues = "IGKANR", the TrypticName is t1.2
        ///   when peptideResidues = "IG",   the TrypticName is 1.2
        ///   when peptideResidues = "KANR", the TrypticName is 3.6
        ///   when peptideResidues = "NR",   the TrypticName is 5.6
        /// </para>
        /// <para>
        /// However, if ICR2LSCompatible = True, the last three tryptic names shown above are changed:
        ///   when peptideResidues = "IG",   the TrypticName is 1.3
        ///   when peptideResidues = "KANR", the TrypticName is 3.7
        ///   when peptideResidues = "NR",   the TrypticName is 5.7
        /// </para>
        /// </remarks>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="returnResidueStart">Output: start peptides of the peptide residues in the protein</param>
        /// <param name="returnResidueEnd">Output: end peptides of the peptide residues in the protein</param>
        /// <param name="ICR2LSCompatible"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionResidues"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="proteinSearchStartLoc"></param>
        // ReSharper restore CommentTypo
        public string GetTrypticName(
            string proteinResidues,
            string peptideResidues,
            out int returnResidueStart,
            out int returnResidueEnd,
            // ReSharper disable once InconsistentNaming
            bool ICR2LSCompatible = false,
            string ruleResidues = TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true,
            int proteinSearchStartLoc = 0)
        {
            int startLoc;

            if (ignoreCase)
            {
                proteinResidues = proteinResidues.ToUpper();
                peptideResidues = peptideResidues.ToUpper();
            }

            if (proteinSearchStartLoc <= 0)
            {
                startLoc = proteinResidues.IndexOf(peptideResidues, StringComparison.Ordinal);
            }
            else
            {
                startLoc = proteinResidues.Substring(proteinSearchStartLoc).IndexOf(peptideResidues, StringComparison.Ordinal);
                if (startLoc >= 0)
                {
                    startLoc += proteinSearchStartLoc;
                }
            }

            var peptideResiduesLength = peptideResidues.Length;

            if (startLoc >= 0 && proteinResidues.Length > 0 && peptideResiduesLength > 0)
            {
                var endLoc = startLoc + peptideResiduesLength - 1;

                // Determine if the residue is tryptic
                // Use CheckSequenceAgainstCleavageRule() for this
                string prefix;
                if (startLoc > 0)
                {
                    prefix = proteinResidues.Substring(startLoc - 1, 1);
                }
                else
                {
                    prefix = terminiiSymbol;
                }

                string suffix;
                if (endLoc == proteinResidues.Length - 1)
                {
                    suffix = terminiiSymbol;
                }
                else
                {
                    suffix = proteinResidues.Substring(endLoc + 1, 1);
                }

                var matchesCleavageRule = CheckSequenceAgainstCleavageRule(prefix + "." + peptideResidues + "." + suffix,
                    ruleResidues,
                    exceptionResidues,
                    false,
                    ".",
                    terminiiSymbol,
                    ignoreCase);

                string trypticName;
                if (matchesCleavageRule)
                {
                    // Construct trypticName

                    // Determine which tryptic residue peptideResidues is
                    short trypticResidueNumber;
                    int ruleResidueLoc;
                    if (startLoc == 0)
                    {
                        trypticResidueNumber = 1;
                    }
                    else
                    {
                        var proteinResiduesBeforeStartLoc = proteinResidues.Substring(0, startLoc);
                        var residueFollowingSearchResidues = peptideResidues.Substring(0, 1);
                        trypticResidueNumber = 0;
                        ruleResidueLoc = -1;
                        do
                        {
                            ruleResidueLoc = GetTrypticNameFindNextCleavageLoc(proteinResiduesBeforeStartLoc, residueFollowingSearchResidues, ruleResidueLoc + 1, ruleResidues, exceptionResidues, terminiiSymbol);
                            if (ruleResidueLoc >= 0)
                            {
                                trypticResidueNumber++;
                            }
                        }
                        while (ruleResidueLoc >= 0 && ruleResidueLoc + 1 < startLoc);
                        trypticResidueNumber++;
                    }

                    // Determine number of K or R residues in peptideResidues
                    // Ignore K or R residues followed by Proline
                    short ruleResidueMatchCount = 0;
                    ruleResidueLoc = -1;
                    do
                    {
                        ruleResidueLoc = GetTrypticNameFindNextCleavageLoc(peptideResidues, suffix, ruleResidueLoc + 1, ruleResidues, exceptionResidues, terminiiSymbol);
                        if (ruleResidueLoc >= 0)
                        {
                            ruleResidueMatchCount++;
                        }
                    }
                    while (ruleResidueLoc >= 0 && ruleResidueLoc < peptideResiduesLength - 1);

                    trypticName = "t" + trypticResidueNumber;
                    if (ruleResidueMatchCount > 1)
                    {
                        trypticName += "." + ruleResidueMatchCount;
                    }
                }
                else if (ICR2LSCompatible)
                {
                    trypticName = (startLoc + 1) + "." + (endLoc + 2);
                }
                else
                {
                    trypticName = (startLoc + 1) + "." + (endLoc + 1);
                }

                returnResidueStart = startLoc;
                returnResidueEnd = endLoc;
                return trypticName;
            }

            // Residues not found
            returnResidueStart = 0;
            returnResidueEnd = 0;
            return string.Empty;
        }

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Looks for all possible matches, returning them as a comma separated list
        /// </summary>
        /// <remarks>See GetTrypticName for additional information</remarks>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="proteinSearchStartLoc"></param>
        /// <param name="listDelimiter"></param>
        public string GetTrypticNameMultipleMatches(
            string proteinResidues,
            string peptideResidues,
            int proteinSearchStartLoc = 0,
            string listDelimiter = ", ")
        {
            return GetTrypticNameMultipleMatches(proteinResidues, peptideResidues,
                                                 out _, out _, out _,
                                                 proteinSearchStartLoc: proteinSearchStartLoc, listDelimiter: listDelimiter);
        }

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Looks for all possible matches, returning them as a comma separated list
        /// </summary>
        /// <remarks>See GetTrypticName for additional information</remarks>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="returnMatchCount">Output: the number of matches</param>
        /// <param name="returnResidueStart">Output: the residue number of the start of the first match</param>
        /// <param name="returnResidueEnd">Output: the residue number of the end of the last match</param>
        /// <param name="ICR2LSCompatible"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionResidues"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="proteinSearchStartLoc"></param>
        /// <param name="listDelimiter"></param>
        /// <returns>The number of matches</returns>
        public string GetTrypticNameMultipleMatches(
            string proteinResidues,
            string peptideResidues,
            out int returnMatchCount,
            out int returnResidueStart,
            out int returnResidueEnd,
            // ReSharper disable once InconsistentNaming
            bool ICR2LSCompatible = false,
            string ruleResidues = TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true,
            int proteinSearchStartLoc = 0,
            string listDelimiter = ", ")
        {
            var currentSearchLoc = proteinSearchStartLoc;
            returnMatchCount = 0;
            returnResidueStart = 0;
            returnResidueEnd = 0;
            var nameList = string.Empty;

            while (true)
            {
                var currentName = GetTrypticName(proteinResidues, peptideResidues, out var currentResidueStart, out var currentResidueEnd, ICR2LSCompatible, ruleResidues, exceptionResidues, terminiiSymbol, ignoreCase, currentSearchLoc);

                if (currentName.Length > 0)
                {
                    if (nameList.Length > 0)
                    {
                        nameList += listDelimiter;
                    }

                    nameList += currentName;
                    currentSearchLoc = currentResidueEnd + 1;
                    returnMatchCount++;

                    if (returnMatchCount == 1)
                    {
                        returnResidueStart = currentResidueStart;
                    }

                    returnResidueEnd = currentResidueEnd;

                    if (currentSearchLoc > proteinResidues.Length - 1)
                        break;
                }
                else
                {
                    break;
                }
            }

            return nameList;
        }

        /// <summary>
        /// Finds the index of the next searchChar in searchResidues (K or R by default)
        /// Assumes searchResidues are already upper case
        /// Examines the residue following the matched residue
        /// If it matches one of the characters in exceptionSuffixResidues, then the match is not counted
        /// </summary>
        /// <remarks>
        /// ResidueFollowingSearchResidues is necessary in case the potential cleavage residue is the final residue in searchResidues
        /// We need to know the next residue to determine if it matches an exception residue
        /// </remarks>
        /// <param name="searchResidues"></param>
        /// <param name="residueFollowingSearchResidues"></param>
        /// <param name="startIndex"></param>
        /// <param name="searchChars"></param>
        /// <param name="exceptionSuffixResidues"></param>
        /// <param name="terminiiSymbol"></param>
        /// <returns>The index of the next searchChar, or -1 if not found</returns>
        private int GetTrypticNameFindNextCleavageLoc(
            string searchResidues,
            string residueFollowingSearchResidues,
            int startIndex,
            string searchChars = TRYPTIC_RULE_RESIDUES,
            string exceptionSuffixResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL)
        {
            // ReSharper disable CommentTypo

            // If searchResidues =      "IGASGEHIFIIGVDKPNR"
            //     and the protein it is part of is: TNSANFRIGASGEHIFIIGVDKPNRQPDS
            //     and searchChars = "KR while exceptionSuffixResidues  = "P"
            // the K in IGASGEHIFIIGVDKPNR is ignored because the following residue is P,
            // while the R in IGASGEHIFIIGVDKPNR is OK because residueFollowingSearchResidues is Q

            // ReSharper restore CommentTypo

            // It is the calling method's responsibility to assign the correct residue to residueFollowingSearchResidues
            // If no match is found, but residueFollowingSearchResidues is "-", the cleavage location returned is Len(searchResidues) + 1

            var exceptionSuffixResidueCount = (short)exceptionSuffixResidues.Length;

            if (startIndex >= searchResidues.Length)
            {
                return searchResidues.Length;
            }

            var minCharIndex = -2;
            for (var charLocInSearchChars = 0; charLocInSearchChars < searchChars.Length; charLocInSearchChars++)
            {
                var charIndex = searchResidues.Substring(startIndex).IndexOf(searchChars.Substring(charLocInSearchChars, 1), StringComparison.Ordinal);

                if (charIndex >= 0)
                {
                    charIndex += startIndex;

                    if (exceptionSuffixResidueCount > 0)
                    {
                        // Make sure suffixResidue does not match exceptionSuffixResidues
                        int exceptionCharLocInSearchResidues;
                        string residueFollowingCleavageResidue;
                        if (charIndex < searchResidues.Length - 1)
                        {
                            exceptionCharLocInSearchResidues = charIndex + 1;
                            residueFollowingCleavageResidue = searchResidues.Substring(exceptionCharLocInSearchResidues, 1);
                        }
                        else
                        {
                            // Matched the last residue in searchResidues
                            exceptionCharLocInSearchResidues = searchResidues.Length;
                            residueFollowingCleavageResidue = residueFollowingSearchResidues;
                        }

                        for (var charLocInExceptionChars = 0; charLocInExceptionChars < exceptionSuffixResidueCount; charLocInExceptionChars++)
                        {
                            if ((residueFollowingCleavageResidue ?? string.Empty) == exceptionSuffixResidues.Substring(charLocInExceptionChars, 1))
                            {
                                // Exception char is the following character; can't count this as the cleavage point

                                if (exceptionCharLocInSearchResidues < searchResidues.Length - 1)
                                {
                                    // Recursively call this method to find the next cleavage position, using an updated startIndex position
                                    var charLocViaRecursiveSearch = GetTrypticNameFindNextCleavageLoc(searchResidues, residueFollowingSearchResidues, exceptionCharLocInSearchResidues, searchChars, exceptionSuffixResidues, terminiiSymbol);

                                    if (charLocViaRecursiveSearch > 0)
                                    {
                                        // Found a residue further along that is a valid cleavage point
                                        charIndex = charLocViaRecursiveSearch;
                                    }
                                    else
                                    {
                                        charIndex = 0;
                                    }
                                }
                                else
                                {
                                    charIndex = 0;
                                }

                                break;
                            }
                        }
                    }
                }

                if (charIndex > 0)
                {
                    if (minCharIndex < -1)
                    {
                        minCharIndex = charIndex;
                    }
                    else if (charIndex < minCharIndex)
                    {
                        minCharIndex = charIndex;
                    }
                }
            }

            if (minCharIndex < 0 && (residueFollowingSearchResidues ?? string.Empty) == (terminiiSymbol ?? string.Empty))
            {
                minCharIndex = searchResidues.Length;
            }

            if (minCharIndex < -1)
            {
                return 0;
            }

            return minCharIndex;
        }

        /// <summary>
        /// Get the next tryptic peptide in proteinResidues, starting the search at startIndex
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="startIndex"></param>
        public string GetTrypticPeptideNext(string proteinResidues, int startIndex)
        {
            return GetTrypticPeptideNext(proteinResidues, startIndex, out _, out _);
        }

        /// <summary>
        /// Get the next tryptic peptide in proteinResidues, starting the search at startIndex
        /// </summary>
        /// <remarks>
        /// Useful when obtaining all of the tryptic peptides for a protein, since this method will operate
        /// much faster than repeatedly calling GetTrypticPeptideByFragmentNumber()
        /// </remarks>
        /// <param name="proteinResidues"></param>
        /// <param name="startIndex"></param>
        /// <param name="returnResidueStart">Output: the residue number in the protein where the peptide starts (1 if no match)</param>
        /// <param name="returnResidueEnd">Output: the residue number in the protein where the peptide ends (1 if no match)</param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionResidues"></param>
        /// <param name="terminiiSymbol"></param>
        /// <returns>The next tryptic peptide, or an empty string if startIndex is beyond the end of proteinResidues</returns>
        public string GetTrypticPeptideNext(
            string proteinResidues,
            int startIndex,
            out int returnResidueStart,
            out int returnResidueEnd,
            string ruleResidues = TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL)
        {
            returnResidueStart = 1;
            returnResidueEnd = 1;

            if (startIndex < 0)
                startIndex = 0;

            var proteinResiduesLength = proteinResidues.Length;
            if (startIndex >= proteinResiduesLength)
            {
                return string.Empty;
            }

            var ruleResidueLoc = GetTrypticNameFindNextCleavageLoc(proteinResidues, terminiiSymbol, startIndex, ruleResidues, exceptionResidues, terminiiSymbol);
            if (ruleResidueLoc >= 0)
            {
                returnResidueStart = startIndex;
                if (ruleResidueLoc >= proteinResiduesLength)
                {
                    returnResidueEnd = proteinResiduesLength;
                }
                else
                {
                    returnResidueEnd = ruleResidueLoc;
                }

                return proteinResidues.Substring(returnResidueStart, returnResidueEnd - returnResidueStart + 1);
            }

            returnResidueStart = 1;
            returnResidueEnd = proteinResiduesLength;
            return proteinResidues;
        }

        /// <summary>
        /// Obtain the desired tryptic peptide from proteinResidues
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="desiredPeptideNumber"></param>
        public string GetTrypticPeptideByFragmentNumber(string proteinResidues, int desiredPeptideNumber)
        {
            return GetTrypticPeptideByFragmentNumber(proteinResidues, desiredPeptideNumber, out _, out _);
        }

        // ReSharper disable CommentTypo

        /// <summary>
        /// <para>
        /// Obtain the desired tryptic peptide from proteinResidues
        /// </para>
        /// <para>
        /// For example, given proteinResidues = "IGKANRMTFGL"
        /// if desiredPeptideNumber = 1, returns "IGK"
        /// if desiredPeptideNumber = 2, returns "ANR"
        /// if desiredPeptideNumber = 3, returns "MTFGL"
        /// </para>
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="desiredPeptideNumber"></param>
        /// <param name="returnResidueStart">Output: the residue number in the protein where the peptide starts (0 if no match)</param>
        /// <param name="returnResidueEnd">Output: the residue number in the protein where the peptide ends (0 if no match)</param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionResidues"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <returns>The matching fragment, or an empty string</returns>
        // ReSharper enable CommentTypo
        public string GetTrypticPeptideByFragmentNumber(
            string proteinResidues,
            int desiredPeptideNumber,
            out int returnResidueStart,
            out int returnResidueEnd,
            string ruleResidues = TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true)
        {
            var ruleResidueLoc = -1;
            var prevStartLoc = 0;

            string matchingFragment;

            returnResidueStart = 0;
            returnResidueEnd = 0;

            if (desiredPeptideNumber < 1)
            {
                return string.Empty;
            }

            if (ignoreCase)
            {
                proteinResidues = proteinResidues.ToUpper();
            }

            var proteinResiduesLength = proteinResidues.Length;

            var startLoc = 0;
            short currentTrypticPeptideNumber = 0;
            while (currentTrypticPeptideNumber < desiredPeptideNumber)
            {
                ruleResidueLoc = GetTrypticNameFindNextCleavageLoc(proteinResidues, terminiiSymbol, startLoc, ruleResidues, exceptionResidues, terminiiSymbol);
                if (ruleResidueLoc >= 0)
                {
                    currentTrypticPeptideNumber++;
                    prevStartLoc = startLoc;
                    startLoc = ruleResidueLoc + 1;

                    if (prevStartLoc >= proteinResiduesLength)
                    {
                        // User requested a peptide number that doesn't exist
                        return string.Empty;
                    }
                }
                else
                {
                    // This code should never be reached
                    break;
                }
            }

            if (currentTrypticPeptideNumber > 0 && prevStartLoc >= 0)
            {
                if (prevStartLoc >= proteinResidues.Length)
                {
                    // User requested a peptide number that is too high
                    returnResidueStart = 0;
                    returnResidueEnd = 0;
                    matchingFragment = string.Empty;
                }
                else
                {
                    // Match found, find the extent of this peptide
                    returnResidueStart = prevStartLoc;
                    if (ruleResidueLoc >= proteinResiduesLength)
                    {
                        returnResidueEnd = proteinResiduesLength - 1;
                    }
                    else
                    {
                        returnResidueEnd = ruleResidueLoc;
                    }

                    // TODO: Determine and fix the exact cases causing a string length overflow
                    matchingFragment = proteinResidues.Substring(prevStartLoc, Math.Min(ruleResidueLoc - prevStartLoc + 1, proteinResidues.Length - prevStartLoc));
                }
            }
            else
            {
                returnResidueStart = 0;
                returnResidueEnd = proteinResiduesLength - 1;
                matchingFragment = proteinResidues;
            }

            return matchingFragment;
        }

        /// <summary>
        /// Examines sequence to see if it matches the cleavage rule
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionSuffixResidues"></param>
        /// <param name="allowPartialCleavage"></param>
        /// <param name="separationChar"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        public bool CheckSequenceAgainstCleavageRule(
            string sequence,
            string ruleResidues,
            string exceptionSuffixResidues,
            bool allowPartialCleavage,
            string separationChar = ".",
            string terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true)
        {
            return CheckSequenceAgainstCleavageRule(sequence, ruleResidues, exceptionSuffixResidues,
                                                    allowPartialCleavage, out _, separationChar, terminiiSymbol, ignoreCase);
        }

        /// <summary>
        /// Examines sequence to see if it matches the cleavage rule
        /// </summary>
        /// <remarks>
        /// <para>
        /// Returns true if sequence doesn't contain any periods, and thus, can't be examined
        /// </para>
        /// <para>
        /// The peptide must end in one of the residues, or in -
        /// The preceding residue must be one of the residues or be -
        /// EXCEPTION: if allowPartialCleavage = True, the rules need only apply to one end
        /// Finally, the suffix residue cannot match any of the residues in exceptionSuffixResidues
        /// </para>
        /// <para>
        /// For example, given ruleResidues = "KR" and exceptionSuffixResidues = "P"
        /// If sequence = "R.AEQDDLANYGPGNGVLPSAGSSISMEK.L", matchesCleavageRule = True
        /// However, if sequence = "R.IGASGEHIFIIGVDK.P", matchesCleavageRule = False since suffix = "P"
        /// Finally, if sequence = "R.IGASGEHIFIIGVDKPNR.Q", matchesCleavageRule = True since K is ignored, but the final R.Q is valid
        /// </para>
        /// </remarks>
        /// <param name="sequence"></param>
        /// <param name="ruleResidues">Residues that define cleavage points</param>
        /// <param name="exceptionSuffixResidues"></param>
        /// <param name="allowPartialCleavage"></param>
        /// <param name="ruleMatchCount">Output: the number of ends that matched the rule (0, 1, or 2); terminii are counted as rule matches</param>
        /// <param name="separationChar"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <returns>True if it matches the cleavage rule, false if it does not match, false if invalid</returns>
        public bool CheckSequenceAgainstCleavageRule(
            string sequence,
            string ruleResidues,
            string exceptionSuffixResidues,
            bool allowPartialCleavage,
            out short ruleMatchCount,
            string separationChar = ".",
            string terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true)
        {
            string sequenceStart, sequenceEnd;
            var matchesCleavageRule = false;

            ruleMatchCount = 0;
            var prefix = string.Empty;
            var suffix = string.Empty;

            // First, make sure the sequence is in the form A.BCDEFG.H or A.BCDEFG or BCDEFG.H
            // If it isn't, then we can't check it (we'll return true)

            if (string.IsNullOrEmpty(ruleResidues))
            {
                // No rule residues
                return true;
            }

            separationChar ??= ".";

            if (!sequence.Contains(separationChar))
            {
                // No periods, can't check
                Console.WriteLine("Warning: sequence does not contain " + separationChar + "; unable to determine cleavage state");
                return true;
            }

            if (ignoreCase)
            {
                sequence = sequence.ToUpper();
            }

            // Find the prefix residue and starting residue
            if (sequence.Substring(1, 1) == separationChar)
            {
                prefix = sequence.Substring(0, 1);
                sequenceStart = sequence.Substring(2, 1);
            }
            else
            {
                sequenceStart = sequence.Substring(0, 1);
            }

            // Find the suffix residue and the ending residue
            if (sequence.Substring(sequence.Length - 2, 1) == separationChar)
            {
                suffix = sequence.Substring(sequence.Length - 1);
                sequenceEnd = sequence.Substring(sequence.Length - 3, 1);
            }
            else
            {
                sequenceEnd = sequence.Substring(sequence.Length - 1);
            }

            if (ruleResidues == (terminiiSymbol ?? string.Empty))
            {
                // Peptide database rules
                // See if prefix and suffix are "" or are terminiiSymbol
                if (prefix == (terminiiSymbol ?? string.Empty) && suffix == (terminiiSymbol ?? string.Empty) ||
                    prefix.Length == 0 && suffix.Length == 0)
                {
                    ruleMatchCount = 2;
                    matchesCleavageRule = true;
                }
            }
            else
            {
                if (ignoreCase)
                {
                    ruleResidues = ruleResidues.ToUpper();
                }

                // Test each character in ruleResidues against both prefix and sequenceEnd
                // Make sure suffix does not match exceptionSuffixResidues
                for (var endToCheck = 0; endToCheck <= 1; endToCheck++)
                {
                    var skipThisEnd = false;
                    string testResidue;
                    if (endToCheck == 0)
                    {
                        testResidue = prefix;
                        if (prefix == (terminiiSymbol ?? string.Empty))
                        {
                            ruleMatchCount++;
                            skipThisEnd = true;
                        }
                        // See if sequenceStart matches one of the exception residues
                        // If it does, make sure prefix does not match one of the rule residues
                        else if (CheckSequenceAgainstCleavageRuleMatchTestResidue(sequenceStart, exceptionSuffixResidues))
                        {
                            // Match found
                            // Make sure prefix does not match one of the rule residues
                            if (CheckSequenceAgainstCleavageRuleMatchTestResidue(prefix, ruleResidues))
                            {
                                // Match found; thus does not match cleavage rule
                                skipThisEnd = true;
                            }
                        }
                    }
                    else
                    {
                        testResidue = sequenceEnd;
                        if (suffix == (terminiiSymbol ?? string.Empty))
                        {
                            ruleMatchCount++;
                            skipThisEnd = true;
                        }
                        // Make sure suffix does not match exceptionSuffixResidues
                        else if (CheckSequenceAgainstCleavageRuleMatchTestResidue(suffix, exceptionSuffixResidues))
                        {
                            // Match found; thus does not match cleavage rule
                            skipThisEnd = true;
                        }
                    }

                    if (!skipThisEnd)
                    {
                        if (CheckSequenceAgainstCleavageRuleMatchTestResidue(testResidue, ruleResidues))
                        {
                            ruleMatchCount++;
                        }
                    }
                }

                if (ruleMatchCount == 2 || (ruleMatchCount >= 1 && allowPartialCleavage))
                {
                    matchesCleavageRule = true;
                }
            }

            return matchesCleavageRule;
        }

        /// <summary>
        /// Checks to see if testResidue matches one of the residues in ruleResidues
        /// </summary>
        /// <param name="testResidue"></param>
        /// <param name="ruleResidues"></param>
        /// <returns>True if ruleResidues contains testResidue</returns>
        private bool CheckSequenceAgainstCleavageRuleMatchTestResidue(string testResidue, string ruleResidues)
        {
            return ruleResidues.IndexOf(testResidue, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Given a residue mass, return the value after immonium loss
        /// </summary>
        /// <param name="residueMass"></param>
        public double ComputeImmoniumMass(double residueMass)
        {
            return residueMass - mImmoniumMassDifference;
        }

        /// <summary>
        /// Convert from IonType enum to string
        /// </summary>
        /// <param name="ionType"></param>
        public string LookupIonTypeString(IonType ionType)
        {
            return ionType switch
            {
                IonType.AIon => "a",
                IonType.BIon => "b",
                IonType.YIon => "y",
                IonType.CIon => "c",
                IonType.ZIon => "z",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Remove all residues from this peptide
        /// </summary>
        public void RemoveAllResidues()
        {
            mResidues.Clear();
            mResidues.Capacity = 50;
            mTotalMass = 0d;
        }

        /// <summary>
        /// Remove all possible modification symbols
        /// </summary>
        /// <remarks>
        /// Removing all modifications will invalidate any modifications present in a sequence
        /// </remarks>
        public void RemoveAllModificationSymbols()
        {
            mModificationSymbols.Clear();
            mModificationSymbols.Capacity = 10;
        }

        /// <summary>
        /// Remove the leading H, if present
        /// </summary>
        /// <param name="workingSequence"></param>
        private void RemoveLeadingH(ref string workingSequence)
        {
            if (workingSequence.Length >= 4 && workingSequence.StartsWith("H", StringComparison.OrdinalIgnoreCase))
            {
                // If next character is not a character, then remove the H and the non-letter character
                if (!char.IsLetter(workingSequence[1]))
                {
                    // Remove the leading H
                    workingSequence = workingSequence.Substring(2);
                }
                // Otherwise, see if next three characters are letters
                else if (char.IsLetter(workingSequence[1]) && char.IsLetter(workingSequence[2]) && char.IsLetter(workingSequence[3]))
                {
                    // Formula starts with 4 characters and the first is H, see if the first 3 characters are a valid amino acid code
                    var abbrevId = mElementAndMassRoutines.Elements.GetAbbreviationId(workingSequence.Substring(0, 3), true);

                    if (abbrevId < 0)
                    {
                        // Doesn't start with a valid amino acid 3 letter abbreviation, so remove the initial H
                        workingSequence = workingSequence.Substring(1);
                    }
                }
            }
        }

        /// <summary>
        /// Remove the trailing OH, if present
        /// </summary>
        /// <param name="workingSequence"></param>
        /// <returns>True if OH was removed</returns>
        // ReSharper disable once InconsistentNaming
        private bool RemoveTrailingOH(ref string workingSequence)
        {
            // ReSharper disable once InconsistentNaming
            var removedOH = false;
            var stringLength = workingSequence.Length;
            if (workingSequence.Length >= 5 && workingSequence.EndsWith("OH", StringComparison.OrdinalIgnoreCase))
            {
                // If previous character is not a character, then remove the OH
                if (!char.IsLetter(workingSequence[stringLength - 3]))
                {
                    workingSequence = workingSequence.Substring(0, stringLength - 3);
                    removedOH = true;
                }
                // Otherwise, see if previous three characters are letters
                else if (char.IsLetter(workingSequence[stringLength - 3]))
                {
                    // Formula ends with 3 characters and the last two are OH, see if the last 3 characters are a valid amino acid code
                    var abbrevId = mElementAndMassRoutines.Elements.GetAbbreviationId(workingSequence.Substring(stringLength - 3, 3), true);

                    if (abbrevId < 0)
                    {
                        // Doesn't end with a valid amino acid 3 letter abbreviation, so remove the trailing OH
                        workingSequence = workingSequence.Substring(0, stringLength - 2);
                        removedOH = true;
                    }
                }
            }

            return removedOH;
        }

        /// <summary>
        /// Remove the modification with the given symbol
        /// </summary>
        /// <param name="modSymbol"></param>
        /// <returns>
        /// 0 if found and removed; 1 if error
        /// </returns>
        public int RemoveModification(string modSymbol)
        {
            if (string.IsNullOrWhiteSpace(modSymbol))
            {
                return 1;
            }

            var removed = false;

            for (var index = 0; index < mModificationSymbols.Count; index++)
            {
                if (mModificationSymbols[index].Symbol == modSymbol)
                {
                    RemoveModificationById(index);
                    removed = true;
                }
            }

            if (removed)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Remove the modification with the given modification Id
        /// </summary>
        /// <param name="modificationId"></param>
        /// <returns>
        /// 0 if found and removed; 1 if error
        /// </returns>
        public int RemoveModificationById(int modificationId)
        {
            bool removed;

            if (modificationId >= 0 && modificationId < mModificationSymbols.Count)
            {
                mModificationSymbols.RemoveAt(modificationId);
                removed = true;
            }
            else
            {
                removed = false;
            }

            if (removed)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Remove the residue at the given index
        /// </summary>
        /// <param name="residueIndex"></param>
        /// <returns> 0 if found and removed; 1 if error</returns>
        public int RemoveResidue(int residueIndex)
        {
            if (residueIndex >= 0 && residueIndex < mResidues.Count)
            {
                mResidues.RemoveAt(residueIndex);

                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Set the C terminus group using an empirical formula
        /// </summary>
        /// <remarks>
        /// Typical C terminus groups
        /// Free Acid = OH
        /// Amide = NH2
        /// </remarks>
        /// <param name="formula"></param>
        /// <param name="followingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>True if success, false if an error</returns>
        public bool SetCTerminus(string formula, string followingResidue = "", bool use3LetterCode = true)
        {
            bool success;

            mCTerminus.Mass = mElementAndMassRoutines.Parser.ComputeFormulaWeight(ref formula);
            mCTerminus.Formula = formula;
            if (mCTerminus.Mass < 0d)
            {
                mCTerminus.Mass = 0d;
                success = false;
            }
            else
            {
                success = true;
            }

            mCTerminus.PrecedingResidue = FillResidueStructureUsingSymbol(string.Empty);
            mCTerminus.FollowingResidue = FillResidueStructureUsingSymbol(followingResidue, use3LetterCode);

            UpdateResidueMasses();
            return success;
        }

        /// <summary>
        /// Set the C terminus group using an enum
        /// </summary>
        /// <param name="cTerminusGroup"></param>
        /// <param name="followingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>True if success, false if an error</returns>
        public bool SetCTerminusGroup(CTerminusGroupType cTerminusGroup, string followingResidue = "", bool use3LetterCode = true)
        {
            return cTerminusGroup switch
            {
                CTerminusGroupType.Hydroxyl => SetCTerminus("OH", followingResidue, use3LetterCode),
                CTerminusGroupType.Amide => SetCTerminus("NH2", followingResidue, use3LetterCode),
                CTerminusGroupType.None => SetCTerminus(string.Empty, followingResidue, use3LetterCode),
                _ => false
            };
        }

        /// <summary>
        /// Reset modification symbols to defaults
        /// </summary>
        public void SetDefaultModificationSymbols()
        {
            try
            {
                RemoveAllModificationSymbols();

                // Add the symbol for phosphorylation
                SetModificationSymbol("*", mMassPhosphorylation, true, "Phosphorylation [HPO3]");

                // Define the other default modifications
                // Valid Mod Symbols are ! # $ % & ' * + ? ^ _ ` ~

                SetModificationSymbol("+", 14.01565d, false, "Methylation [CH2]");
                SetModificationSymbol("@", 15.99492d, false, "Oxidation [O]");
                SetModificationSymbol("!", 57.02146d, false, "Carbamidomethylation [C2H3NO]");
                SetModificationSymbol("&", 58.00548d, false, "Carboxymethylation [CH2CO2]");
                // ReSharper disable once StringLiteralTypo
                SetModificationSymbol("#", 71.03711d, false, "Acrylamide [CHCH2CONH2]");
                SetModificationSymbol("$", 227.127d, false, "Cleavable ICAT [(^12C10)H17N3O3]");
                SetModificationSymbol("%", 236.127d, false, "Cleavable ICAT [(^13C9)(^12C)H17N3O3]");
                SetModificationSymbol("~", 442.225d, false, "ICAT D0 [C20H34N4O5S]");
                SetModificationSymbol("`", 450.274d, false, "ICAT D8 [C20H26D8N4O5S]");
            }
            catch (Exception ex)
            {
                OnErrorEvent(Logging.GeneralErrorHandler("Peptide.SetDefaultModificationSymbols", ex));
            }
        }

        /// <summary>
        /// Reset options to defaults
        /// </summary>
        /// <remarks>
        /// Also calls SetDefaultModificationSymbols
        /// </remarks>
        public void SetDefaultOptions()
        {
            try
            {
                var intensityOptions = mFragSpectrumOptions.IntensityOptions;
                intensityOptions.IonType[(int)IonType.AIon] = DEFAULT_A_ION_INTENSITY;
                intensityOptions.IonType[(int)IonType.BIon] = DEFAULT_BYCZ_ION_INTENSITY;
                intensityOptions.IonType[(int)IonType.YIon] = DEFAULT_BYCZ_ION_INTENSITY;
                intensityOptions.IonType[(int)IonType.CIon] = DEFAULT_BYCZ_ION_INTENSITY;
                intensityOptions.IonType[(int)IonType.ZIon] = DEFAULT_BYCZ_ION_INTENSITY;
                intensityOptions.BYIonShoulder = DEFAULT_B_Y_ION_SHOULDER_INTENSITY;
                intensityOptions.NeutralLoss = DEFAULT_NEUTRAL_LOSS_ION_INTENSITY;

                // A ions can have ammonia and phosphate loss, but not water loss
                // Thus, NeutralLossWater is false
                var aIonOption = mFragSpectrumOptions.IonTypeOptions[(int)IonType.AIon];
                aIonOption.ShowIon = true;
                aIonOption.NeutralLossAmmonia = true;
                aIonOption.NeutralLossPhosphate = true;
                aIonOption.NeutralLossWater = false;

                for (var ionIndex = IonType.BIon; ionIndex <= IonType.ZIon; ionIndex++)
                {
                    var ionOption = mFragSpectrumOptions.IonTypeOptions[(int)ionIndex];
                    ionOption.ShowIon = true;
                    ionOption.NeutralLossAmmonia = true;
                    ionOption.NeutralLossPhosphate = true;
                    ionOption.NeutralLossWater = true;
                }

                mFragSpectrumOptions.DoubleChargeIonsShow = true;
                mFragSpectrumOptions.DoubleChargeIonsThreshold = DEFAULT_DOUBLE_CHARGE_MZ_THRESHOLD;

                mFragSpectrumOptions.TripleChargeIonsShow = false;
                mFragSpectrumOptions.TripleChargeIonsThreshold = DEFAULT_TRIPLE_CHARGE_MZ_THRESHOLD;

                SetSymbolWaterLoss("-H2O");
                SetSymbolAmmoniaLoss("-NH3");
                SetSymbolPhosphoLoss("-H3PO4");

                UpdateStandardMasses();

                SetDefaultModificationSymbols();
            }
            catch (Exception ex)
            {
                OnErrorEvent(Logging.GeneralErrorHandler("Peptide.SetDefaultOptions", ex));
            }
        }

        /// <summary>
        /// Set MS/MS fragmentation options
        /// </summary>
        /// <param name="newFragSpectrumOptions"></param>
        public void SetFragmentationSpectrumOptions(FragmentationSpectrumOptions newFragSpectrumOptions)
        {
            mFragSpectrumOptions = newFragSpectrumOptions;
        }

        /// <summary>
        /// Adds a new modification or updates an existing one (based on modSymbol)
        /// </summary>
        /// <param name="modSymbol"></param>
        /// <param name="modificationMass"></param>
        /// <param name="indicatesPhosphorylation"></param>
        /// <param name="comment"></param>
        /// <returns>True if success, false if an error</returns>
        public bool SetModificationSymbol(string modSymbol, double modificationMass, bool indicatesPhosphorylation = false, string comment = "")
        {
            if (string.IsNullOrWhiteSpace(modSymbol))
            {
                return false;
            }

            // Make sure modSymbol contains no letters, numbers, spaces, dashes, or periods
            for (var index = 0; index < modSymbol.Length; index++)
            {
                var testChar = modSymbol.Substring(index, 1);
                if (!mElementAndMassRoutines.IsModSymbol(testChar))
                {
                    return false;
                }
            }

            // See if the modification is already present
            var indexToUse = GetModificationSymbolId(modSymbol);

            if (indexToUse < 0)
            {
                // Need to add the modification
                var mod = new ModificationSymbol(modSymbol, modificationMass, indicatesPhosphorylation, comment);
                mModificationSymbols.Add(mod);
            }
            else
            {
                // Not updating the symbol, since we looked it up by symbol...
                var mod = mModificationSymbols[indexToUse];
                mod.ModificationMass = modificationMass;
                mod.IndicatesPhosphorylation = indicatesPhosphorylation;
                mod.Comment = comment;
            }

            return true;
        }

        /// <summary>
        /// Set the N terminus group using an empirical formula
        /// </summary>
        /// <remarks>
        /// Typical N terminus groups
        /// Hydrogen = H
        /// Acetyl = C2OH3
        /// PyroGlu = C5O2NH6
        /// Carbamyl = CONH2
        /// PTC = C7H6NS
        /// </remarks>
        /// <param name="formula"></param>
        /// <param name="precedingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>True if success, false if an error</returns>
        public bool SetNTerminus(string formula, string precedingResidue = "", bool use3LetterCode = true)
        {
            bool success;

            mNTerminus.Mass = mElementAndMassRoutines.Parser.ComputeFormulaWeight(ref formula);
            mNTerminus.Formula = formula;
            if (mNTerminus.Mass < 0d)
            {
                mNTerminus.Mass = 0d;
                success = false;
            }
            else
            {
                success = true;
            }

            mNTerminus.PrecedingResidue = FillResidueStructureUsingSymbol(precedingResidue, use3LetterCode);
            mNTerminus.FollowingResidue = FillResidueStructureUsingSymbol(string.Empty);

            UpdateResidueMasses();
            return success;
        }

        /// <summary>
        /// Set the N terminus group using an enum
        /// </summary>
        /// <param name="nTerminusGroup"></param>
        /// <param name="precedingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>True if success, false if an error</returns>
        public bool SetNTerminusGroup(NTerminusGroupType nTerminusGroup, string precedingResidue = "", bool use3LetterCode = true)
        {
            return nTerminusGroup switch
            {
                NTerminusGroupType.Hydrogen => SetNTerminus("H", precedingResidue, use3LetterCode),
                NTerminusGroupType.HydrogenPlusProton => SetNTerminus("HH", precedingResidue, use3LetterCode),
                NTerminusGroupType.Acetyl => SetNTerminus("C2OH3", precedingResidue, use3LetterCode),
                NTerminusGroupType.PyroGlu => SetNTerminus("C5O2NH6", precedingResidue, use3LetterCode),
                NTerminusGroupType.Carbamyl => SetNTerminus("CONH2", precedingResidue, use3LetterCode),
                NTerminusGroupType.PTC => SetNTerminus("C7H6NS", precedingResidue, use3LetterCode),
                NTerminusGroupType.None => SetNTerminus(string.Empty, precedingResidue, use3LetterCode),
                _ => false
            };
        }

        /// <summary>
        /// Set the residue at the specified index
        /// </summary>
        /// <param name="residueIndex">0-based index of residue</param>
        /// <param name="symbol"></param>
        /// <param name="is3LetterCode"></param>
        /// <param name="phosphorylated"></param>
        public int SetResidue(int residueIndex, string symbol, bool is3LetterCode = true, bool phosphorylated = false)
        {
            // Sets or adds a residue (must add residues in order)
            // Returns the index of the modified residue, or the new index if added
            // Returns -1 if a problem

            int indexToUse;
            string threeLetterSymbol;

            if (string.IsNullOrEmpty(symbol))
            {
                return -1;
            }

            if (is3LetterCode)
            {
                threeLetterSymbol = symbol;
            }
            else
            {
                threeLetterSymbol = mElementAndMassRoutines.Elements.GetAminoAcidSymbolConversion(symbol, true);
            }

            if (threeLetterSymbol.Length == 0)
            {
                threeLetterSymbol = UNKNOWN_SYMBOL;
            }

            var residue = new Residue(threeLetterSymbol);
            if (residueIndex > mResidues.Count)
            {
                indexToUse = mResidues.Count;
                mResidues.Add(new Residue());
            }
            else
            {
                indexToUse = residueIndex;
                mResidues[residueIndex] = residue;
            }

            residue.Phosphorylated = phosphorylated;
            if (phosphorylated)
            {
                // Only Ser, Thr, or Tyr should be phosphorylated
                // However, if the user sets other residues as phosphorylated, we'll allow that
                if (residue.Symbol is not ("Ser" or "Thr" or "Tyr"))
                {
                    Console.WriteLine("Residue '" + residue.Symbol + "' is marked as being phosphorylated; this is unexpected");
                }
            }

            UpdateResidueMasses();

            return indexToUse;
        }

        /// <summary>
        /// Sets modifications on a residue
        /// </summary>
        /// <remarks>
        /// Modification Symbols are defined using successive calls to SetModificationSymbol()
        /// </remarks>
        /// <param name="residueIndex">0-based index of residue</param>
        /// <param name="modificationCount"></param>
        /// <param name="modificationIDs">0-based array</param>
        /// <returns>True if success, false if an error</returns>
        public bool SetResidueModifications(int residueIndex, int modificationCount, int[] modificationIDs)
        {
            if (residueIndex < 0 || residueIndex >= mResidues.Count || modificationCount < 0)
                return false;

            var residue = mResidues[residueIndex];
            if (modificationCount > MAX_MODIFICATIONS)
            {
                modificationCount = MAX_MODIFICATIONS;
            }

            residue.ModificationIDs.Clear();
            residue.Phosphorylated = false;
            for (var index = 0; index < modificationCount; index++)
            {
                var newModId = modificationIDs[index];
                if (newModId >= 0 && newModId < mModificationSymbols.Count)
                {
                    residue.ModificationIDs.Add(newModId);

                    // Check for phosphorylation
                    if (mModificationSymbols[newModId].IndicatesPhosphorylation)
                    {
                        residue.Phosphorylated = true;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <remarks>If <paramref name="sequence"/> is blank or contains no valid residues, then will still return 0</remarks>
        /// <param name="sequence">Peptide sequence using 1-letter amino acid symbols</param>
        /// <returns>True if success, false if an error</returns>
        public bool SetSequence1LetterSymbol(string sequence)
        {
            return SetSequence(sequence, is3LetterCode: false);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <remarks>If <paramref name="sequence"/> is blank or contains no valid residues, then will still return 0</remarks>
        /// <param name="sequence">Peptide sequence</param>
        /// <param name="is3LetterCode">Set to true for 3-letter amino acid symbols, false for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues">Set to true to check for and remove prefix and suffix residues when <paramref name="is3LetterCode"/> = false</param>
        /// <returns>True if success, false if an error</returns>
        public bool SetSequence(
            string sequence,
            bool is3LetterCode,
            bool oneLetterCheckForPrefixAndSuffixResidues)
        {
            return SetSequence(sequence, NTerminusGroupType.Hydrogen, CTerminusGroupType.Hydroxyl,
                is3LetterCode, oneLetterCheckForPrefixAndSuffixResidues);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <remarks>If <paramref name="sequence" /> is blank or contains no valid residues, then will still return 0</remarks>
        /// <param name="sequence">Peptide sequence, using 3-letter amino acid symbols (unless <paramref name="is3LetterCode"/> = false)</param>
        /// <param name="nTerminus">N-terminus group</param>
        /// <param name="cTerminus">C-terminus group</param>
        /// <param name="is3LetterCode">Set to true for 3-letter amino acid symbols, false for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues">Set to true to check for and remove prefix and suffix residues when <paramref name="is3LetterCode"/> = false</param>
        /// <param name="threeLetterCheckForPrefixHandSuffixOH">Set to true to check for and remove prefix H and OH when <paramref name="is3LetterCode"/> = true</param>
        /// <param name="addMissingModificationSymbols">Set to true to automatically add missing modification symbols (though the mod masses will be 0)</param>
        /// <returns>True if success, false if an error</returns>
        public bool SetSequence(
            string sequence,
            NTerminusGroupType nTerminus = NTerminusGroupType.Hydrogen,
            CTerminusGroupType cTerminus = CTerminusGroupType.Hydroxyl,
            bool is3LetterCode = true,
            bool oneLetterCheckForPrefixAndSuffixResidues = true,
            // ReSharper disable once InconsistentNaming
            bool threeLetterCheckForPrefixHandSuffixOH = true,
            bool addMissingModificationSymbols = false)
        {
            try
            {
                sequence = sequence.Trim();

                var sequenceStrLength = sequence.Length;
                if (sequenceStrLength == 0)
                {
                    return false;
                }

                // Clear any old residue information
                mResidues.Clear();
                mResidues.Capacity = sequence.Length;

                string threeLetterSymbol;
                int modSymbolLength;
                if (!is3LetterCode)
                {
                    // Sequence is 1 letter codes

                    if (oneLetterCheckForPrefixAndSuffixResidues)
                    {
                        // First look if sequence is in the form A.BCDEFG.Z or -.BCDEFG.Z or A.BCDEFG.-
                        // If so, then need to strip out the preceding A and Z residues since they aren't really part of the sequence
                        if (sequenceStrLength > 1 && sequence.Contains("."))
                        {
                            if (sequence.Substring(1, 1) == ".")
                            {
                                sequence = sequence.Substring(2);
                                sequenceStrLength = sequence.Length;
                            }

                            if (sequence.Substring(sequenceStrLength - 2, 1) == ".")
                            {
                                sequence = sequence.Substring(0, sequenceStrLength - 2);
                            }

                            // Also check for starting with a . or ending with a .
                            if (sequence.Substring(0, 1) == ".")
                            {
                                sequence = sequence.Substring(1);
                            }

                            if (sequence.Substring(sequence.Length - 1) == ".")
                            {
                                sequence = sequence.Substring(0, sequence.Length - 1);
                            }

                            sequenceStrLength = sequence.Length;
                        }
                    }

                    for (var index = 0; index < sequenceStrLength; index++)
                    {
                        var oneLetterSymbol = sequence.Substring(index, 1);
                        if (char.IsLetter(oneLetterSymbol[0]))
                        {
                            // Character found
                            // Look up 3 letter symbol
                            // If none is found, this will return an empty string
                            threeLetterSymbol = mElementAndMassRoutines.Elements.GetAminoAcidSymbolConversion(oneLetterSymbol, true);

                            if (threeLetterSymbol.Length == 0)
                                threeLetterSymbol = UNKNOWN_SYMBOL;

                            SetSequenceAddResidue(threeLetterSymbol);

                            // Look at following character(s), and record any modification symbols present
                            modSymbolLength = CheckForModifications(sequence.Substring(index + 1), mResidues.Count - 1, addMissingModificationSymbols);

                            index += modSymbolLength;
                        }
                        // If . or - or space, then ignore it
                        // If a number, ignore it
                        // If anything else, then should have been skipped, or should be skipped
                        else if (oneLetterSymbol is "." or "-" or " ")
                        {
                            // All is fine; we can skip this
                        }
                    }
                }
                else
                {
                    // Sequence is 3 letter codes
                    var index = 0;

                    if (threeLetterCheckForPrefixHandSuffixOH)
                    {
                        // Look for a leading H or trailing OH, provided those don't match any of the amino acids
                        RemoveLeadingH(ref sequence);
                        RemoveTrailingOH(ref sequence);

                        // Recompute sequence length
                        sequenceStrLength = sequence.Length;
                    }

                    while (index < sequenceStrLength - 2)
                    {
                        var firstChar = sequence.Substring(index, 1);
                        if (char.IsLetter(firstChar[0]))
                        {
                            if (char.IsLetter(sequence[index + 1]) && char.IsLetter(sequence[index + 2]))
                            {
                                threeLetterSymbol = firstChar.ToUpper() + sequence.Substring(index + 1, 2).ToLower();

                                if (mElementAndMassRoutines.Elements.GetAbbreviationId(threeLetterSymbol, true) == -1)
                                {
                                    // 3 letter symbol not found
                                    // Add anyway, but mark as Xxx
                                    threeLetterSymbol = UNKNOWN_SYMBOL;
                                }

                                SetSequenceAddResidue(threeLetterSymbol);

                                // Look at following character(s), and record any modification symbols present
                                modSymbolLength = CheckForModifications(sequence.Substring(index + 3), mResidues.Count - 1, addMissingModificationSymbols);

                                index += 3;
                                index += modSymbolLength;
                            }
                            else
                            {
                                // First letter is a character, but next two are not; ignore it
                                index++;
                            }
                        }
                        else
                        {
                            // If . or - or space, then ignore it
                            // If a number, ignore it
                            // If anything else, then should have been skipped or should be skipped
                            if (firstChar is "." or "-" or " ")
                            {
                                // All is fine; we can skip this
                            }

                            index++;
                        }
                    }
                }

                // By calling SetNTerminus and SetCTerminus, the UpdateResidueMasses() Sub will also be called
                mDelayUpdateResidueMass = true;
                SetNTerminusGroup(nTerminus);
                SetCTerminusGroup(cTerminus);

                mDelayUpdateResidueMass = false;
                UpdateResidueMasses();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Peptide.SetSequence: {0}", ex.Message);
                return false;
            }
        }

        private void SetSequenceAddResidue(string threeLetterSymbol)
        {
            if (string.IsNullOrWhiteSpace(threeLetterSymbol))
            {
                threeLetterSymbol = UNKNOWN_SYMBOL;
            }

            mResidues.Add(new Residue(threeLetterSymbol));
        }

        /// <summary>
        /// Set the symbol for ammonia loss
        /// </summary>
        /// <param name="newSymbol"></param>
        public void SetSymbolAmmoniaLoss(string newSymbol)
        {
            if (!string.IsNullOrWhiteSpace(newSymbol))
            {
                mAmmoniaLossSymbol = newSymbol;
            }
        }

        /// <summary>
        /// Set the symbol for phospho loss
        /// </summary>
        /// <param name="newSymbol"></param>
        public void SetSymbolPhosphoLoss(string newSymbol)
        {
            if (!string.IsNullOrWhiteSpace(newSymbol))
            {
                mPhosphoLossSymbol = newSymbol;
            }
        }

        /// <summary>
        /// Set the symbol for water loss
        /// </summary>
        /// <param name="newSymbol"></param>
        public void SetSymbolWaterLoss(string newSymbol)
        {
            if (!string.IsNullOrWhiteSpace(newSymbol))
            {
                mWaterLossSymbol = newSymbol;
            }
        }

        private void UpdateResidueMasses()
        {
            var validResidueCount = 0;
            var protonatedNTerminus = false;

            if (mDelayUpdateResidueMass)
                return;

            // The N-terminus ions are the basis for the running total
            var runningTotal = mNTerminus.Mass;
            if (string.Equals(mNTerminus.Formula, "HH", StringComparison.OrdinalIgnoreCase))
            {
                // HydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                // we need to subtract it out here
                // However, we need to subtract out massHydrogen, and not chargeCarrierMass since the current
                // formula's mass was computed using two hydrogens, and not one hydrogen and one charge carrier
                protonatedNTerminus = true;
                runningTotal -= mMassHydrogen;
            }

            foreach (var residue in mResidues)
            {
                var abbrevId = mElementAndMassRoutines.Elements.GetAbbreviationId(residue.Symbol, true);

                if (abbrevId >= 0)
                {
                    validResidueCount++;
                    residue.Mass = mElementAndMassRoutines.Elements.GetAbbreviationMass(abbrevId);

                    var phosphorylationMassAdded = false;

                    // Compute the mass, including the modifications
                    residue.MassWithMods = residue.Mass;
                    foreach (var modificationId in residue.ModificationIDs)
                    {
                        if (modificationId < mModificationSymbols.Count)
                        {
                            residue.MassWithMods += mModificationSymbols[modificationId].ModificationMass;
                            if (mModificationSymbols[modificationId].IndicatesPhosphorylation)
                            {
                                phosphorylationMassAdded = true;
                            }
                        }
                        else
                        {
                            // Invalid ModificationID
                            Console.WriteLine("Invalid ModificationID: " + modificationId);
                        }
                    }

                    if (residue.Phosphorylated)
                    {
                        // Only add a mass if none of the .ModificationIDs has .IndicatesPhosphorylation = True
                        if (!phosphorylationMassAdded)
                        {
                            residue.MassWithMods += mMassPhosphorylation;
                        }
                    }

                    runningTotal += residue.MassWithMods;

                    residue.IonMass[(int)IonType.AIon] = runningTotal - mImmoniumMassDifference - mChargeCarrierMass;
                    residue.IonMass[(int)IonType.BIon] = runningTotal;

                    // Add NH3 (ammonia) to the B ion mass to get the C ion mass
                    residue.IonMass[(int)IonType.CIon] = residue.IonMass[(int)IonType.BIon] + mMassNH3;
                }
                else
                {
                    residue.Mass = 0d;
                    residue.MassWithMods = 0d;
                    Array.Clear(residue.IonMass, 0, residue.IonMass.Length);
                }
            }

            runningTotal += mCTerminus.Mass;
            if (protonatedNTerminus)
            {
                runningTotal += mChargeCarrierMass;
            }

            if (validResidueCount > 0)
            {
                mTotalMass = runningTotal;
            }
            else
            {
                mTotalMass = 0d;
            }

            // Now compute the y-ion and z-ion masses
            runningTotal = mCTerminus.Mass + mChargeCarrierMass;

            for (var index = mResidues.Count - 1; index >= 0; --index)
            {
                var residue = mResidues[index];
                if (residue.IonMass[(int)IonType.AIon] > 0d)
                {
                    runningTotal += residue.MassWithMods;
                    residue.IonMass[(int)IonType.YIon] = runningTotal + mChargeCarrierMass;
                    if (index == 0)
                    {
                        // Add the N-terminus mass to highest y ion
                        residue.IonMass[(int)IonType.YIon] = residue.IonMass[(int)IonType.YIon] + mNTerminus.Mass - mChargeCarrierMass;
                        if (protonatedNTerminus)
                        {
                            // HydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                            // we need to subtract it out here
                            // However, we need to subtract out massHydrogen, and not chargeCarrierMass since the current
                            // formula's mass was computed using two hydrogens, and not one hydrogen and one charge carrier
                            residue.IonMass[(int)IonType.YIon] -= mMassHydrogen;
                        }
                    }

                    // Subtract NH2 (amide) from the Y ion mass to get the Z ion mass
                    residue.IonMass[(int)IonType.ZIon] = residue.IonMass[(int)IonType.YIon] - (mMassNH3 - mMassHydrogen);
                }
            }
        }

        /// <summary>
        /// Update standard monoisotopic masses for water, hydrogen, ammonia, the phospho group, and the immonium group
        /// </summary>
        public void UpdateStandardMasses()
        {
            try
            {
                var elementModeSaved = mElementAndMassRoutines.Elements.GetElementMode();

                mElementAndMassRoutines.Elements.SetElementMode(ElementMassMode.Isotopic);

                mChargeCarrierMass = mElementAndMassRoutines.Elements.GetChargeCarrierMass();

                // Update standard mass values
                mMassHOH = mElementAndMassRoutines.Parser.ComputeFormulaWeight("HOH");
                mMassNH3 = mElementAndMassRoutines.Parser.ComputeFormulaWeight("NH3");
                mMassH3PO4 = mElementAndMassRoutines.Parser.ComputeFormulaWeight("H3PO4");
                mMassHydrogen = mElementAndMassRoutines.Parser.ComputeFormulaWeight("H");

                // Phosphorylation is the loss of OH and the addition of H2PO4, for a net change of HPO3
                mMassPhosphorylation = mElementAndMassRoutines.Parser.ComputeFormulaWeight("HPO3");

                // The immonium mass is equal to the mass of CO minus the mass of H, thus typically 26.9871
                mImmoniumMassDifference = mElementAndMassRoutines.Parser.ComputeFormulaWeight("CO") - mMassHydrogen;

                mElementAndMassRoutines.Elements.SetElementMode(elementModeSaved);
            }
            catch (Exception ex)
            {
                OnErrorEvent(Logging.GeneralErrorHandler("Peptide.UpdateStandardMasses", ex));
            }
        }
    }
}
