using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace MolecularWeightCalculator
{
    public class Peptide
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: Peptide

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004
        // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
        // Website: https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL and https://omics.pnl.gov/
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

        public Peptide(ElementAndMassTools elementAndMassTools = null)
        {
            ElementAndMassRoutines = elementAndMassTools;
            if (ElementAndMassRoutines == null)
            {
                ElementAndMassRoutines = new ElementAndMassTools();
            }

            InitializeClass();
        }

        public const float DEFAULT_B_Y_ION_SHOULDER_INTENSITY = 50f;
        public const float DEFAULT_BYCZ_ION_INTENSITY = 100f;
        public const float DEFAULT_A_ION_INTENSITY = 20f;
        public const float DEFAULT_NEUTRAL_LOSS_ION_INTENSITY = 20f;

        public const float DEFAULT_DOUBLE_CHARGE_MZ_THRESHOLD = 800f;
        public const float DEFAULT_TRIPLE_CHARGE_MZ_THRESHOLD = 900f;

        private const short RESIDUE_DIM_CHUNK = 50;
        private const short MAX_MODIFICATIONS = 6; // Maximum number of modifications for a single residue
        private const string UNKNOWN_SYMBOL = "Xxx";
        private const string UNKNOWN_SYMBOL_ONE_LETTER = "X";

        private const string TERMINII_SYMBOL = "-";
        private const string TRYPTIC_RULE_RESIDUES = "KR";
        private const string TRYPTIC_EXCEPTION_RESIDUES = "P";

        private const string SHOULDER_ION_PREFIX = "Shoulder-";

        private readonly ElementAndMassTools ElementAndMassRoutines;

        public enum CTerminusGroupType
        {
            Hydroxyl = 0,
            Amide = 1,
            None = 2
        }

        public enum NTerminusGroupType
        {
            Hydrogen = 0,
            HydrogenPlusProton = 1,
            Acetyl = 2,
            PyroGlu = 3,
            Carbamyl = 4,
            PTC = 5,
            None = 6
        }

        private const IonType ION_TYPE_MAX = IonType.ZIon;

        public enum IonType
        {
            AIon = 0,
            BIon = 1,
            YIon = 2,
            CIon = 3,
            ZIon = 4
        }

        private class ModificationSymbol
        {
            public string Symbol; // Symbol used for modification in formula; may be 1 or more characters; for example: + ++ * ** etc.
            public double ModificationMass; // Normally positive, but could be negative
            public bool IndicatesPhosphorylation; // When true, then this symbol means a residue is phosphorylated
            public string Comment;
        }

        private class Residue
        {
            public string Symbol; // 3 letter symbol
            public double Mass; // The mass of the residue alone (excluding any modification)
            public double MassWithMods; // The mass of the residue, including phosphorylation or any modification
            public double[] IonMass; // 0-based array; the masses that the a, b, and y ions ending/starting with this residue will produce in the mass spectrum (includes H+)
            public bool Phosphorylated; // Technically, only Ser, Thr, or Tyr residues can be phosphorylated (H3PO4), but if the user phosphorylates other residues, we'll allow that
            public short ModificationIDCount;
            public int[] ModificationIDs; // 1-based array

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                Initialize(false);
            }

            public void Initialize(bool blnForceInit)
            {
                if (blnForceInit || IonMass == null)
                {
                    IonMass = new double[5];
                    ModificationIDs = new int[MAX_MODIFICATIONS + 1];
                }
            }
        }

        private class Terminus
        {
            public string Formula;
            public double Mass;
            public Residue PrecedingResidue = new Residue(); // If the peptide sequence is part of a protein, the user can record the final residue of the previous peptide sequence here
            public Residue FollowingResidue = new Residue(); // If the peptide sequence is part of a protein, the user can record the first residue of the next peptide sequence here

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                PrecedingResidue.Initialize();
                FollowingResidue.Initialize();
            }
        }

        public class FragmentationSpectrumIntensities
        {
            public double[] IonType; // 0-based array
            public double BYIonShoulder; // If > 0 then shoulder ions will be created by B and Y ions
            public double NeutralLoss;

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                IonType = new double[5];
            }
        }

        // Note: A ions can have ammonia and phosphate loss, but not water loss, so this is set to false by default
        // The graphical version of MwtWin does not allow this to be overridden, but a programmer could do so via a call to this Dll
        public class IonTypeOptions
        {
            public bool ShowIon;
            public bool NeutralLossWater;
            public bool NeutralLossAmmonia;
            public bool NeutralLossPhosphate;
        }

        public class FragmentationSpectrumOptions
        {
            public FragmentationSpectrumIntensities IntensityOptions = new FragmentationSpectrumIntensities();
            public IonTypeOptions[] IonTypeOptions;
            public bool DoubleChargeIonsShow;
            public float DoubleChargeIonsThreshold;
            public bool TripleChargeIonsShow;
            public float TripleChargeIonsThreshold;

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                IntensityOptions.Initialize();
                IonTypeOptions = new IonTypeOptions[5];
                for (var i = 0; i < IonTypeOptions.Length; i++)
                {
                    IonTypeOptions[i] = new IonTypeOptions();
                }
            }
        }

        public class FragmentationSpectrumData
        {
            public double Mass;
            public double Intensity;
            public string Symbol; // The symbol, with the residue number (e.g. y1, y2, b3-H2O, Shoulder-y1, etc.)
            public string SymbolGeneric; // The symbol, without the residue number (e.g. a, b, y, b++, Shoulder-y, etc.)
            public int SourceResidueNumber; // The residue number that resulted in this mass
            public string SourceResidueSymbol3Letter; // The residue symbol that resulted in this mass
            public short Charge;
            public IonType IonType;
            public bool IsShoulderIon; // B and Y ions can have Shoulder ions at +-1

            public override string ToString()
            {
                return Symbol + ", " + Mass.ToString("0.00");
            }
        }

        // Note: A peptide goes from N to C, eg. HGlyLeuTyrOH has N-Terminus = H and C-Terminus = OH
        // Residue 1 would be Gly, Residue 2 would be Leu, Residue 3 would be Tyr
        private Residue[] Residues; // 1-based array
        private int ResidueCount;
        private int ResidueCountDimmed;

        // ModificationSymbols() holds a list of the potential modification symbols and the mass of each modification
        // Modification symbols can be 1 or more letters long
        private ModificationSymbol[] ModificationSymbols; // 1-based array
        private int ModificationSymbolCount;
        private int ModificationSymbolCountDimmed;

        // ReSharper disable once UnassignedField.Local - initialized in InitializeClass() when it calls InitializeArrays()
        private readonly Terminus mNTerminus = new Terminus(); // Formula on the N-Terminus

        // ReSharper disable once UnassignedField.Local - initialized in InitializeClass() when it calls InitializeArrays()
        private readonly Terminus mCTerminus = new Terminus(); // Formula on the C-Terminus
        private double mTotalMass;

        private string mWaterLossSymbol; // -H2O
        private string mAmmoniaLossSymbol; // -NH3
        private string mPhosphoLossSymbol; // -H3PO4

        private FragmentationSpectrumOptions mFragSpectrumOptions = new FragmentationSpectrumOptions();

        private double dblHOHMass;
        private double dblNH3Mass;
        private double dblH3PO4Mass;
        private double dblPhosphorylationMass; // H3PO4 minus HOH = 79.9663326
        private double dblHydrogenMass; // Mass of hydrogen
        private double dblChargeCarrierMass; // H minus one electron

        private double dblImmoniumMassDifference; // CO minus H = 26.9871

        private double dblHistidineFW; // 110
        private double dblPhenylalanineFW; // 120
        private double dblTyrosineFW; // 136

        private bool mDelayUpdateResidueMass;
        //

        private void AppendDataToFragSpectrum(ref int ionCount, ref FragmentationSpectrumData[] fragSpectrumWork, float mass, float intensity, string ionSymbol, string ionSymbolGeneric, int sourceResidue, string sourceResidueSymbol3Letter, short charge, IonType ionType, bool isShoulderIon)
        {
            try
            {
                if (ionCount >= fragSpectrumWork.Length)
                {
                    // This shouldn't happen
                    Console.WriteLine("In AppendDataToFragSpectrum, lngIonCount is greater than FragSpectrumWork.Length - 1; this is unexpected");
                    Array.Resize(ref fragSpectrumWork, fragSpectrumWork.Length + 10);
                }

                var fragIon = fragSpectrumWork[ionCount];
                fragIon.Mass = mass;
                fragIon.Intensity = intensity;
                fragIon.Symbol = ionSymbol;
                fragIon.SymbolGeneric = ionSymbolGeneric;
                fragIon.SourceResidueNumber = sourceResidue;
                fragIon.SourceResidueSymbol3Letter = sourceResidueSymbol3Letter;
                fragIon.Charge = charge;
                fragIon.IonType = ionType;
                fragIon.IsShoulderIon = isShoulderIon;

                ionCount += 1;
            }
            catch
            {
                Console.WriteLine(Information.Err().Description);
            }
        }

        public int AssureNonZero(int number)
        {
            // Returns a non-zero number, either -1 if lngNumber = 0 or lngNumber if it's nonzero
            if (number == 0)
            {
                return -1;
            }

            return number;
        }

        private int CheckForModifications(string partialSequence, int residueNumber, bool addMissingModificationSymbols = false)
        {
            // Looks at strPartialSequence to see if it contains 1 or more modifications
            // If any modification symbols are found, the modification is recorded in .ModificationIDs()
            // If all or part of the modification symbol is not found in ModificationSymbols(), then a new entry
            // is added to ModificationSymbols()
            // Returns the total length of all modifications found

            var intSequenceStrLength = partialSequence.Length;

            // Find the entire group of potential modification symbols
            var strModSymbolGroup = string.Empty;
            var intCompareIndex = 0;
            while (intCompareIndex < intSequenceStrLength)
            {
                var strTestChar = partialSequence.Substring(intCompareIndex, 1);
                if (ElementAndMassRoutines.IsModSymbolInternal(strTestChar))
                {
                    strModSymbolGroup += strTestChar;
                }
                else
                {
                    break;
                }

                intCompareIndex += 1;
            }

            var intModSymbolLengthTotal = strModSymbolGroup.Length;
            while (strModSymbolGroup.Length > 0)
            {
                // Step through strModSymbolGroup to see if all of it or parts of it match any of the defined
                // modification symbols

                int intModificationID = default;
                var blnMatchFound = false;
                int intSubPartLength;
                for (intSubPartLength = strModSymbolGroup.Length; intSubPartLength >= 1; intSubPartLength -= 1)
                {
                    // See if the modification is already defined
                    intModificationID = GetModificationSymbolID(strModSymbolGroup.Substring(0, intSubPartLength));
                    if (intModificationID > 0)
                    {
                        blnMatchFound = true;
                        break;
                    }
                }

                if (!blnMatchFound)
                {
                    if (addMissingModificationSymbols)
                    {
                        // Add strModSymbolGroup as a new modification, using a mass of 0 since we don't know the modification mass
                        SetModificationSymbol(strModSymbolGroup, 0d);
                        blnMatchFound = true;
                    }
                    else
                    {
                        // Ignore the modification
                        strModSymbolGroup = "0";
                    }

                    strModSymbolGroup = string.Empty;
                }

                if (blnMatchFound)
                {
                    // Record the modification for this residue
                    var residue = Residues[residueNumber];
                    if (residue.ModificationIDCount < MAX_MODIFICATIONS)
                    {
                        residue.ModificationIDCount = (short)(residue.ModificationIDCount + 1);
                        residue.ModificationIDs[residue.ModificationIDCount] = intModificationID;
                        if (ModificationSymbols[intModificationID].IndicatesPhosphorylation)
                        {
                            residue.Phosphorylated = true;
                        }
                    }

                    if (intSubPartLength < strModSymbolGroup.Length)
                    {
                        // Remove the matched portion from strModSymbolGroup and test again
                        strModSymbolGroup = strModSymbolGroup.Substring(intSubPartLength);
                    }
                    else
                    {
                        strModSymbolGroup = string.Empty;
                    }
                }
            }

            return intModSymbolLengthTotal;
        }

        private short ComputeMaxIonsPerResidue()
        {
            // Estimate the total ions per residue that will be created
            // This number will nearly always be much higher than the number of ions that will actually
            // be stored for a given sequence, since not all will be doubly charged, and not all will show
            // all of the potential neutral losses

            short intIonCount = 0;

            for (IonType eIonIndex = 0; eIonIndex <= ION_TYPE_MAX; eIonIndex++)
            {
                if (mFragSpectrumOptions.IonTypeOptions[(int)eIonIndex].ShowIon)
                {
                    intIonCount = (short)(intIonCount + 1);
                    if (Math.Abs(mFragSpectrumOptions.IntensityOptions.BYIonShoulder) > 0d)
                    {
                        if (eIonIndex == IonType.BIon || eIonIndex == IonType.YIon ||
                            eIonIndex == IonType.CIon || eIonIndex == IonType.ZIon)
                        {
                            intIonCount = (short)(intIonCount + 2);
                        }
                    }

                    if (mFragSpectrumOptions.IonTypeOptions[(int)eIonIndex].NeutralLossAmmonia)
                        intIonCount = (short)(intIonCount + 1);
                    if (mFragSpectrumOptions.IonTypeOptions[(int)eIonIndex].NeutralLossPhosphate)
                        intIonCount = (short)(intIonCount + 1);
                    if (mFragSpectrumOptions.IonTypeOptions[(int)eIonIndex].NeutralLossWater)
                        intIonCount = (short)(intIonCount + 1);
                }
            }

            // Double Charge ions could be created for all ions, so simply double intIonCount
            if (mFragSpectrumOptions.DoubleChargeIonsShow)
            {
                intIonCount = (short)(intIonCount * 2);
            }

            if (mFragSpectrumOptions.TripleChargeIonsShow)
            {
                intIonCount = (short)(intIonCount * 2);
            }

            return intIonCount;
        }

        private Residue FillResidueStructureUsingSymbol(string symbol, bool use3LetterCode = true)
        {
            // Returns a variable of type udtResidueType containing strSymbol as the residue symbol
            // If strSymbol is a valid amino acid type, then also updates udtResidue with the default information

            int lngAbbrevID;
            var udtResidue = new Residue();

            // Initialize the UDTs
            udtResidue.Initialize();
            var strSymbol3Letter = string.Empty;

            if (symbol.Length > 0)
            {
                if (use3LetterCode)
                {
                    strSymbol3Letter = symbol;
                }
                else
                {
                    strSymbol3Letter = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(symbol, true);
                    if (strSymbol3Letter.Length == 0)
                    {
                        strSymbol3Letter = symbol;
                    }
                }

                lngAbbrevID = ElementAndMassRoutines.GetAbbreviationIDInternal(strSymbol3Letter, true);
            }
            else
            {
                lngAbbrevID = 0;
            }

            udtResidue.Symbol = strSymbol3Letter;
            udtResidue.ModificationIDCount = 0;
            udtResidue.Phosphorylated = false;
            if (lngAbbrevID > 0)
            {
                udtResidue.Mass = ElementAndMassRoutines.GetAbbreviationMass(lngAbbrevID);
            }
            else
            {
                udtResidue.Mass = 0d;
            }

            udtResidue.MassWithMods = udtResidue.Mass;

            return udtResidue;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fragSpectrum"></param>
        /// <returns>The number of ions in udtFragSpectrum()</returns>
        /// <remarks></remarks>
        public int GetFragmentationMasses(out FragmentationSpectrumData[] fragSpectrum)
        {
            // Old: Func GetFragmentationMasses(lngMaxIonCount As Long, ByRef sngIonMassesZeroBased() As Single, ByRef sngIonIntensitiesZeroBased() As Single, ByRef strIonSymbolsZeroBased() As String) As Long

            var lstFragSpectraData = GetFragmentationMasses();

            if (lstFragSpectraData.Count == 0)
            {
                fragSpectrum = new FragmentationSpectrumData[1];
                fragSpectrum[0] = new FragmentationSpectrumData();
                return 0;
            }

            fragSpectrum = new FragmentationSpectrumData[lstFragSpectraData.Count + 1];

            for (var intIndex = 0; intIndex < lstFragSpectraData.Count; intIndex++)
                fragSpectrum[intIndex] = lstFragSpectraData[intIndex];

            return lstFragSpectraData.Count;
        }

        public List<FragmentationSpectrumData> GetFragmentationMasses()
        {
            const int MAX_CHARGE = 3;

            var sngIonIntensities = new float[5];

            if (ResidueCount == 0)
            {
                // No residues
                return new List<FragmentationSpectrumData>();
            }

            var blnShowCharge = new bool[4];
            var sngChargeThreshold = new float[4];

            // Copy some of the values from mFragSpectrumOptions to local variables to make things easier to read
            for (IonType eIonType = 0; eIonType <= ION_TYPE_MAX; eIonType++)
                sngIonIntensities[(int)eIonType] = (float)mFragSpectrumOptions.IntensityOptions.IonType[(int)eIonType];

            var sngIonShoulderIntensity = (float)mFragSpectrumOptions.IntensityOptions.BYIonShoulder;
            var sngNeutralLossIntensity = (float)mFragSpectrumOptions.IntensityOptions.NeutralLoss;

            if (MAX_CHARGE >= 2)
            {
                blnShowCharge[2] = mFragSpectrumOptions.DoubleChargeIonsShow;
                sngChargeThreshold[2] = mFragSpectrumOptions.DoubleChargeIonsThreshold;
            }

            if (MAX_CHARGE >= 3)
            {
                blnShowCharge[3] = mFragSpectrumOptions.TripleChargeIonsShow;
                sngChargeThreshold[3] = mFragSpectrumOptions.TripleChargeIonsThreshold;
            }

            // Populate sngIonMassesZeroBased() and sngIonIntensitiesZeroBased()
            // Put ion descriptions in strIonSymbolsZeroBased
            var lngPredictedIonCount = GetFragmentationSpectrumRequiredDataPoints();

            if (lngPredictedIonCount == 0)
                lngPredictedIonCount = ResidueCount;
            var FragSpectrumWork = new FragmentationSpectrumData[lngPredictedIonCount + 1];
            for (var i = 0; i < FragSpectrumWork.Length; i++)
            {
                FragSpectrumWork[i] = new FragmentationSpectrumData();
            }

            // Need to update the residue masses in case the modifications have changed
            UpdateResidueMasses();

            var lngIonCount = 0;
            for (var lngResidueIndex = 1; lngResidueIndex <= ResidueCount; lngResidueIndex++)
            {
                var residue = Residues[lngResidueIndex];

                for (IonType eIonType = 0; eIonType <= ION_TYPE_MAX; eIonType++)
                {
                    if (mFragSpectrumOptions.IonTypeOptions[(int)eIonType].ShowIon)
                    {
                        if ((lngResidueIndex == 1 || lngResidueIndex == ResidueCount) && (eIonType == IonType.AIon || eIonType == IonType.BIon || eIonType == IonType.CIon))
                        {
                            // Don't include a, b, or c ions in the output masses for this residue
                        }
                        else
                        {
                            // Ion is used
                            var sngBaseMass = (float)residue.IonMass[(int)eIonType];
                            var sngIntensity = sngIonIntensities[(int)eIonType];

                            // Get the list of residues preceding or following this residue
                            // Note that the residue symbols are separated by a space to avoid accidental matching by the InStr() functions below
                            var strResidues = GetInternalResidues(lngResidueIndex, eIonType, out var blnPhosphorylated);

                            for (short intChargeIndex = 1; intChargeIndex <= MAX_CHARGE; intChargeIndex++)
                            {
                                if (intChargeIndex == 1 || intChargeIndex > 1 && blnShowCharge[intChargeIndex])
                                {
                                    float sngConvolutedMass;
                                    if (intChargeIndex == 1)
                                    {
                                        sngConvolutedMass = sngBaseMass;
                                    }
                                    else
                                    {
                                        // Compute mass at higher charge
                                        sngConvolutedMass = (float)ElementAndMassRoutines.ConvoluteMassInternal(sngBaseMass, 1, intChargeIndex, dblChargeCarrierMass);
                                    }

                                    if (intChargeIndex > 1 && sngBaseMass < sngChargeThreshold[intChargeIndex])
                                    {
                                        // BaseMass is below threshold, do not add to Predicted Spectrum
                                    }
                                    else
                                    {
                                        // Add ion to Predicted Spectrum

                                        // Y and Z Ions are numbered in decreasing order: y5, y4, y3, y2, y1
                                        // A, B, and C ions are numbered in increasing order: a1, a2, etc.  or b1, b2, etc.
                                        var strIonSymbolGeneric = LookupIonTypeString(eIonType);
                                        string strIonSymbol;
                                        if (eIonType == IonType.YIon || eIonType == IonType.ZIon)
                                        {
                                            strIonSymbol = strIonSymbolGeneric + (ResidueCount - lngResidueIndex + 1);
                                        }
                                        else
                                        {
                                            strIonSymbol = strIonSymbolGeneric + lngResidueIndex;
                                        }

                                        if (intChargeIndex > 1)
                                        {
                                            strIonSymbol += new string('+', intChargeIndex);
                                            strIonSymbolGeneric += new string('+', intChargeIndex);
                                        }

                                        AppendDataToFragSpectrum(ref lngIonCount, ref FragSpectrumWork, sngConvolutedMass, sngIntensity, strIonSymbol, strIonSymbolGeneric, lngResidueIndex, residue.Symbol, intChargeIndex, eIonType, false);

                                        // Add shoulder ions to PredictedSpectrum() if a B, Y, C, or Z ion and the shoulder intensity is > 0
                                        // Need to use Abs() here since user can define negative theoretical intensities (which allows for plotting a spectrum inverted)
                                        float sngObservedMass;
                                        if (Math.Abs(sngIonShoulderIntensity) > 0f && (eIonType == IonType.BIon || eIonType == IonType.YIon || eIonType == IonType.CIon || eIonType == IonType.ZIon))
                                        {
                                            for (var intShoulderIndex = -1; intShoulderIndex <= 1; intShoulderIndex += 2)
                                            {
                                                sngObservedMass = (float)(sngConvolutedMass + intShoulderIndex * (1d / intChargeIndex));
                                                AppendDataToFragSpectrum(ref lngIonCount, ref FragSpectrumWork, sngObservedMass, sngIonShoulderIntensity, SHOULDER_ION_PREFIX + strIonSymbol, SHOULDER_ION_PREFIX + strIonSymbolGeneric, lngResidueIndex, residue.Symbol, intChargeIndex, eIonType, true);
                                            }
                                        }

                                        // Apply neutral loss modifications
                                        if (mFragSpectrumOptions.IonTypeOptions[(int)eIonType].NeutralLossWater)
                                        {
                                            // Loss of water only affects Ser, Thr, Asp, or Glu (S, T, E, or D)
                                            // See if the residues up to this point contain any of these residues
                                            if (strResidues.Contains("Ser") || strResidues.Contains("Thr") || strResidues.Contains("Glue") || strResidues.Contains("Asp"))
                                            {
                                                sngObservedMass = (float)(sngConvolutedMass - dblHOHMass / intChargeIndex);
                                                AppendDataToFragSpectrum(ref lngIonCount, ref FragSpectrumWork, sngObservedMass, sngNeutralLossIntensity, strIonSymbol + mWaterLossSymbol, strIonSymbolGeneric + mWaterLossSymbol, lngResidueIndex, residue.Symbol, intChargeIndex, eIonType, false);
                                            }
                                        }

                                        if (mFragSpectrumOptions.IonTypeOptions[(int)eIonType].NeutralLossAmmonia)
                                        {
                                            // Loss of Ammonia only affects Arg, Lys, Gln, or Asn (R, K, Q, or N)
                                            // See if the residues up to this point contain any of these residues
                                            if (strResidues.Contains("Arg") || strResidues.Contains("Lys") || strResidues.Contains("Gln") || strResidues.Contains("Asn"))
                                            {
                                                sngObservedMass = (float)(sngConvolutedMass - dblNH3Mass / intChargeIndex);
                                                AppendDataToFragSpectrum(ref lngIonCount, ref FragSpectrumWork, sngObservedMass, sngNeutralLossIntensity, strIonSymbol + mAmmoniaLossSymbol, strIonSymbolGeneric + mAmmoniaLossSymbol, lngResidueIndex, residue.Symbol, intChargeIndex, eIonType, false);
                                            }
                                        }

                                        if (mFragSpectrumOptions.IonTypeOptions[(int)eIonType].NeutralLossPhosphate)
                                        {
                                            // Loss of phosphate only affects phosphorylated residues
                                            // Technically, only Ser, Thr, or Tyr (S, T, or Y) can be phosphorylated, but if the user marks other residues as phosphorylated, we'll allow that
                                            // See if the residues up to this point contain phosphorylated residues
                                            if (blnPhosphorylated)
                                            {
                                                sngObservedMass = (float)(sngConvolutedMass - dblH3PO4Mass / intChargeIndex);
                                                AppendDataToFragSpectrum(ref lngIonCount, ref FragSpectrumWork, sngObservedMass, sngNeutralLossIntensity, strIonSymbol + mPhosphoLossSymbol, strIonSymbolGeneric + mPhosphoLossSymbol, lngResidueIndex, residue.Symbol, intChargeIndex, eIonType, false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Sort arrays by mass (using a pointer array to synchronize the arrays)
            var PointerArray = new int[lngIonCount + 1];

            for (var lngIndex = 0; lngIndex < lngIonCount; lngIndex++)
                PointerArray[lngIndex] = lngIndex;

            ShellSortFragSpectrum(ref FragSpectrumWork, ref PointerArray, 0, lngIonCount - 1);

            // Copy the data from FragSpectrumWork() to lstFragSpectraData
            var lstFragSpectraData = new List<FragmentationSpectrumData>(lngIonCount);

            for (var lngIndex = 0; lngIndex <= lngIonCount; lngIndex++)
                lstFragSpectraData.Add(FragSpectrumWork[PointerArray[lngIndex]]);

            return lstFragSpectraData;
        }

        public int GetFragmentationSpectrumRequiredDataPoints()
        {
            // Determines the total number of data points that will be required for a theoretical fragmentation spectrum

            return ResidueCount * ComputeMaxIonsPerResidue();
        }

        public FragmentationSpectrumOptions GetFragmentationSpectrumOptions()
        {
            try
            {
                return mFragSpectrumOptions;
            }
            catch (Exception ex)
            {
                ElementAndMassRoutines.GeneralErrorHandler("Peptide.GetFragmentationSpectrumOptions", ex);
            }

            var udtDefaultOptions = new FragmentationSpectrumOptions();
            udtDefaultOptions.Initialize();

            return udtDefaultOptions;
        }

        public double GetPeptideMass()
        {
            // Returns the mass of the entire peptide

            // Update the residue masses in order to update mTotalMass
            UpdateResidueMasses();

            return mTotalMass;
        }

        private string GetInternalResidues(int currentResidueIndex, IonType ionType)
        {
            var blnPhosphorylated = false;
            return GetInternalResidues(currentResidueIndex, ionType, out blnPhosphorylated);
        }

        private string GetInternalResidues(int currentResidueIndex, IonType ionType, out bool phosphorylated)
        {
            // Determines the residues preceding or following the given residue (up to and including the current residue)
            // If eIonType is a, b, or c ions, then returns residues from the N terminus
            // If eIonType is y or ions, then returns residues from the C terminus
            // Also, set blnPhosphorylated to true if any of the residues is Ser, Thr, or Tyr and is phosphorylated
            //
            // Note that the residue symbols are separated by a space to avoid accidental matching by the InStr() function

            var strInternalResidues = string.Empty;
            phosphorylated = false;
            if (ionType == IonType.YIon || ionType == IonType.ZIon)
            {
                for (var lngResidueIndex = currentResidueIndex; lngResidueIndex <= ResidueCount; lngResidueIndex++)
                {
                    strInternalResidues = strInternalResidues + Residues[lngResidueIndex].Symbol + " ";
                    if (Residues[lngResidueIndex].Phosphorylated)
                        phosphorylated = true;
                }
            }
            else
            {
                for (var lngResidueIndex = 1; lngResidueIndex <= currentResidueIndex; lngResidueIndex++)
                {
                    strInternalResidues = strInternalResidues + Residues[lngResidueIndex].Symbol + " ";
                    if (Residues[lngResidueIndex].Phosphorylated)
                        phosphorylated = true;
                }
            }

            return strInternalResidues;
        }

        public int GetModificationSymbol(int modificationId, out string modSymbol, out double modificationMass, out bool indicatesPhosphorylation, out string comment)
        {
            // Returns information on the modification with lngModificationID
            // Returns 0 if success, 1 if failure

            if (modificationId >= 1 && modificationId <= ModificationSymbolCount)
            {
                var mod = ModificationSymbols[modificationId];
                modSymbol = mod.Symbol;
                modificationMass = mod.ModificationMass;
                indicatesPhosphorylation = mod.IndicatesPhosphorylation;
                comment = mod.Comment;

                return 0;
            }

            modSymbol = string.Empty;
            modificationMass = 0d;
            indicatesPhosphorylation = false;
            comment = string.Empty;
            return 1;
        }

        public int GetModificationSymbolCount()
        {
            // Returns the number of modifications defined

            return ModificationSymbolCount;
        }

        public int GetModificationSymbolID(string modSymbol)
        {
            // Returns the ID for a given modification
            // Returns 0 if not found, the ID if found

            var lngModificationIDMatch = default(int);

            for (var intIndex = 1; intIndex <= ModificationSymbolCount; intIndex++)
            {
                if ((ModificationSymbols[intIndex].Symbol ?? "") == (modSymbol ?? ""))
                {
                    lngModificationIDMatch = intIndex;
                    break;
                }
            }

            return lngModificationIDMatch;
        }

        public int GetResidue(int residueNumber, ref string symbol, ref double mass, ref bool isModified, ref short modificationCount)
        {
            // Returns 0 if success, 1 if failure
            if (residueNumber >= 1 && residueNumber <= ResidueCount)
            {
                var residue = Residues[residueNumber];
                symbol = residue.Symbol;
                mass = residue.Mass;
                isModified = residue.ModificationIDCount > 0;
                modificationCount = residue.ModificationIDCount;

                return 0;
            }

            return 1;
        }

        public int GetResidueCount()
        {
            return ResidueCount;
        }

        public int GetResidueCountSpecificResidue(string residueSymbol, bool use3LetterCode)
        {
            // Returns the number of occurrences of the given residue in the loaded sequence

            string strSearchResidue3Letter;

            if (use3LetterCode)
            {
                strSearchResidue3Letter = residueSymbol;
            }
            else
            {
                strSearchResidue3Letter = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(residueSymbol, true);
            }

            var lngResidueCount = 0;
            for (var lngResidueIndex = 0; lngResidueIndex < ResidueCount; lngResidueIndex++)
            {
                if ((Residues[lngResidueIndex].Symbol ?? "") == (strSearchResidue3Letter ?? ""))
                {
                    lngResidueCount += 1;
                }
            }

            return lngResidueCount;
        }

        public int GetResidueModificationIDs(int residueNumber, ref int[] modificationIDsOneBased)
        {
            // Returns the number of Modifications
            // ReDims lngModificationIDsOneBased() to hold the values

            if (residueNumber >= 1 && residueNumber <= ResidueCount)
            {
                var residue = Residues[residueNumber];

                // Need to use this in case the calling program is sending an array with fixed dimensions
                try
                {
                    modificationIDsOneBased = new int[residue.ModificationIDCount + 1];
                }
                catch
                {
                    // Ignore errors
                }

                for (var intIndex = 1; intIndex <= residue.ModificationIDCount; intIndex++)
                    modificationIDsOneBased[intIndex] = residue.ModificationIDs[intIndex];

                return residue.ModificationIDCount;
            }

            return 0;
        }

        public string GetResidueSymbolOnly(int residueNumber, bool use3LetterCode)
        {
            // Returns the symbol at the given residue number, or string.empty if an invalid residue number

            string strSymbol;

            if (residueNumber >= 1 && residueNumber <= ResidueCount)
            {
                strSymbol = Residues[residueNumber].Symbol;

                if (!use3LetterCode)
                    strSymbol = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbol, false);
            }
            else
            {
                strSymbol = string.Empty;
            }

            return strSymbol;
        }

        public string GetSequence1LetterCode()
        {
            return GetSequence(false);
        }

        public string GetSequence(bool use3LetterCode = true,
            bool addSpaceEvery10Residues = false,
            bool separateResiduesWithDash = false,
            bool includeNAndCTerminii = false,
            bool includeModificationSymbols = true)
        {
            // Construct a text sequence using Residues() and the N and C Terminus info

            string strDashAdd;

            if (separateResiduesWithDash)
                strDashAdd = "-";
            else
                strDashAdd = string.Empty;

            var strSequence = string.Empty;
            for (var lngIndex = 1; lngIndex <= ResidueCount; lngIndex++)
            {
                var residue = Residues[lngIndex];
                var strSymbol3Letter = residue.Symbol;
                if (use3LetterCode)
                {
                    strSequence += strSymbol3Letter;
                }
                else
                {
                    var strSymbol1Letter = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbol3Letter, false);
                    if ((strSymbol1Letter ?? "") == string.Empty)
                        strSymbol1Letter = UNKNOWN_SYMBOL_ONE_LETTER;
                    strSequence += strSymbol1Letter;
                }

                if (includeModificationSymbols)
                {
                    for (var intModIndex = 1; intModIndex <= residue.ModificationIDCount; intModIndex++)
                    {
                        var lngError = GetModificationSymbol(residue.ModificationIDs[intModIndex], out var strModSymbol, out _, out _, out _);
                        if (lngError == 0)
                        {
                            strSequence += strModSymbol;
                        }
                        else
                        {
                            Console.WriteLine("GetModificationSymbol returned error code " + lngError + " in GetSequence");
                        }
                    }
                }

                if (lngIndex != ResidueCount)
                {
                    if (addSpaceEvery10Residues)
                    {
                        if (lngIndex % 10 == 0)
                        {
                            strSequence += " ";
                        }
                        else
                        {
                            strSequence += strDashAdd;
                        }
                    }
                    else
                    {
                        strSequence += strDashAdd;
                    }
                }
            }

            if (includeNAndCTerminii)
            {
                strSequence = mNTerminus.Formula + strDashAdd + strSequence + strDashAdd + mCTerminus.Formula;
            }

            return strSequence;
        }

        public string GetSymbolWaterLoss()
        {
            return mWaterLossSymbol;
        }

        public string GetSymbolPhosphoLoss()
        {
            return mPhosphoLossSymbol;
        }

        public string GetSymbolAmmoniaLoss()
        {
            return mAmmoniaLossSymbol;
        }

        public string GetTrypticName(string proteinResidues, string peptideResidues,
            int proteinSearchStartLoc = 1)
        {
            return GetTrypticName(proteinResidues, peptideResidues, out _, out _, proteinSearchStartLoc: proteinSearchStartLoc);
        }

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Constructs a name string based on their position and based on whether the fragment is truly tryptic
        /// In addition, returns the position of the first and last residue in <paramref name="returnResidueStart"/> and <paramref name="returnResidueEnd"/>
        /// </summary>
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
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetTrypticName(string proteinResidues, string peptideResidues,
            out int returnResidueStart,
            out int returnResidueEnd,
            bool ICR2LSCompatible = false,
            string ruleResidues = TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true,
            int proteinSearchStartLoc = 1)
        {
            // The tryptic name in the following format
            // t1  indicates tryptic peptide 1
            // t2 represents tryptic peptide 2, etc.
            // t1.2  indicates tryptic peptide 1, plus one more tryptic peptide, i.e. t1 and t2
            // t5.2  indicates tryptic peptide 5, plus one more tryptic peptide, i.e. t5 and t6
            // t5.3  indicates tryptic peptide 5, plus two more tryptic peptides, i.e. t5, t6, and t7
            // 40.52  means that the residues are not tryptic, and simply range from residue 40 to 52
            // If the peptide residues are not present in proteinResidues, then returns ""
            // Since a peptide can occur multiple times in a protein, one can set proteinSearchStartLoc to a value larger than 1 to ignore previous hits

            // If ICR2LSCompatible is True, then the values returned when a peptide is not tryptic are modified to
            // range from the starting residue, to the ending residue +1
            // returnResidueEnd is always equal to the position of the final residue, regardless of ICR2LSCompatible

            // For example, if proteinResidues = "IGKANR"
            // Then when peptideResidues = "IGK", the TrypticName is t1
            // Then when peptideResidues = "ANR", the TrypticName is t2
            // Then when peptideResidues = "IGKANR", the TrypticName is t1.2
            // Then when peptideResidues = "IG", the TrypticName is 1.2
            // Then when peptideResidues = "KANR", the TrypticName is 3.6
            // Then when peptideResidues = "NR", the TrypticName is 5.6

            // However, if ICR2LSCompatible = True, then the last three are changed to:
            // Then when peptideResidues = "IG", the TrypticName is 1.3
            // Then when peptideResidues = "KANR", the TrypticName is 3.7
            // Then when peptideResidues = "NR", the TrypticName is 5.7

            int intStartLoc;

            if (ignoreCase)
            {
                proteinResidues = proteinResidues.ToUpper();
                peptideResidues = peptideResidues.ToUpper();
            }

            if (proteinSearchStartLoc <= 0)
            {
                intStartLoc = proteinResidues.IndexOf(peptideResidues, StringComparison.Ordinal);
            }
            else
            {
                intStartLoc = proteinResidues.Substring(proteinSearchStartLoc).IndexOf(peptideResidues, StringComparison.Ordinal);
                if (intStartLoc >= 0)
                {
                    intStartLoc = intStartLoc + proteinSearchStartLoc - 1;
                }
            }

            var lngPeptideResiduesLength = peptideResidues.Length;

            if (intStartLoc >= 0 && proteinResidues.Length > 0 && lngPeptideResiduesLength > 0)
            {
                var intEndLoc = intStartLoc + lngPeptideResiduesLength - 1;

                // Determine if the residue is tryptic
                // Use CheckSequenceAgainstCleavageRule() for this
                string strPrefix;
                if (intStartLoc > 0)
                {
                    strPrefix = proteinResidues.Substring(intStartLoc - 1, 1);
                }
                else
                {
                    strPrefix = terminiiSymbol;
                }

                string strSuffix;
                if (intEndLoc == proteinResidues.Length - 1)
                {
                    strSuffix = terminiiSymbol;
                }
                else
                {
                    strSuffix = proteinResidues.Substring(intEndLoc + 1, 1);
                }

                var blnMatchesCleavageRule = CheckSequenceAgainstCleavageRule(strPrefix + "." + peptideResidues + "." + strSuffix,
                    ruleResidues,
                    exceptionResidues,
                    false,
                    ".",
                    terminiiSymbol,
                    ignoreCase);

                string strTrypticName;
                if (blnMatchesCleavageRule)
                {
                    // Construct strTrypticName

                    // Determine which tryptic residue peptideResidues is
                    short intTrypticResidueNumber;
                    int lngRuleResidueLoc;
                    if (intStartLoc == 0)
                    {
                        intTrypticResidueNumber = 0;
                    }
                    else
                    {
                        var strProteinResiduesBeforeStartLoc = proteinResidues.Substring(0, intStartLoc - 1);
                        var strResidueFollowingSearchResidues = peptideResidues.Substring(0, 1);
                        intTrypticResidueNumber = 0;
                        lngRuleResidueLoc = -1;
                        do
                        {
                            lngRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResiduesBeforeStartLoc, strResidueFollowingSearchResidues, lngRuleResidueLoc + 1, ruleResidues, exceptionResidues, terminiiSymbol);
                            if (lngRuleResidueLoc >= 0)
                            {
                                intTrypticResidueNumber = (short)(intTrypticResidueNumber + 1);
                            }
                        }
                        while (lngRuleResidueLoc >= 0 && lngRuleResidueLoc + 1 < intStartLoc);
                        intTrypticResidueNumber = (short)(intTrypticResidueNumber + 1);
                    }

                    // Determine number of K or R residues in peptideResidues
                    // Ignore K or R residues followed by Proline
                    short intRuleResidueMatchCount = 0;
                    lngRuleResidueLoc = -1;
                    do
                    {
                        lngRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(peptideResidues, strSuffix, lngRuleResidueLoc + 1, ruleResidues, exceptionResidues, terminiiSymbol);
                        if (lngRuleResidueLoc >= 0)
                        {
                            intRuleResidueMatchCount = (short)(intRuleResidueMatchCount + 1);
                        }
                    }
                    while (lngRuleResidueLoc >= 0 && lngRuleResidueLoc < lngPeptideResiduesLength);

                    strTrypticName = "t" + intTrypticResidueNumber;
                    if (intRuleResidueMatchCount > 1)
                    {
                        strTrypticName = strTrypticName + "." + intRuleResidueMatchCount;
                    }
                }
                else if (ICR2LSCompatible)
                {
                    strTrypticName = intStartLoc + "." + (intEndLoc + 1);
                }
                else
                {
                    strTrypticName = intStartLoc + "." + intEndLoc;
                }

                returnResidueStart = intStartLoc;
                returnResidueEnd = intEndLoc;
                return strTrypticName;
            }

            // Residues not found
            returnResidueStart = 0;
            returnResidueEnd = 0;
            return string.Empty;
        }

        public string GetTrypticNameMultipleMatches(string proteinResidues,
            string peptideResidues,
            int proteinSearchStartLoc = 1,
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
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="returnMatchCount"></param>
        /// <param name="returnResidueStart"></param>
        /// <param name="returnResidueEnd"></param>
        /// <param name="ICR2LSCompatible"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionResidues"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="proteinSearchStartLoc"></param>
        /// <param name="listDelimiter"></param>
        /// <returns>The number of matches</returns>
        /// <remarks></remarks>
        public string GetTrypticNameMultipleMatches(string proteinResidues,
            string peptideResidues,
            out int returnMatchCount,
            out int returnResidueStart,
            out int returnResidueEnd,
            bool ICR2LSCompatible = false,
            string ruleResidues = TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true,
            int proteinSearchStartLoc = 1,
            string listDelimiter = ", ")
        {
            // Returns the number of matches in returnMatchCount
            // returnResidueStart contains the residue number of the start of the first match
            // returnResidueEnd contains the residue number of the end of the last match

            // See GetTrypticName for additional information

            var lngCurrentSearchLoc = proteinSearchStartLoc;
            returnMatchCount = 0;
            returnResidueStart = 0;
            returnResidueEnd = 0;
            var strNameList = string.Empty;

            do
            {
                var strCurrentName = GetTrypticName(proteinResidues, peptideResidues, out var lngCurrentResidueStart, out var lngCurrentResidueEnd, ICR2LSCompatible, ruleResidues, exceptionResidues, terminiiSymbol, ignoreCase, lngCurrentSearchLoc);

                if (strCurrentName.Length > 0)
                {
                    if (strNameList.Length > 0)
                    {
                        strNameList += listDelimiter;
                    }

                    strNameList += strCurrentName;
                    lngCurrentSearchLoc = lngCurrentResidueEnd + 1;
                    returnMatchCount += 1;

                    if (returnMatchCount == 1)
                    {
                        returnResidueStart = lngCurrentResidueStart;
                    }

                    returnResidueEnd = lngCurrentResidueEnd;

                    if (lngCurrentSearchLoc > proteinResidues.Length)
                        break;
                }
                else
                {
                    break;
                }
            }
            while (true);
            return strNameList;
        }

        private int GetTrypticNameFindNextCleavageLoc(string searchResidues, string residueFollowingSearchResidues,
            int startChar,
            string searchChars = TRYPTIC_RULE_RESIDUES,
            string exceptionSuffixResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL)
        {
            // Finds the location of the next strSearchChar in searchResidues (K or R by default)
            // Assumes searchResidues are already upper case
            // Examines the residue following the matched residue
            // If it matches one of the characters in exceptionSuffixResidues, then the match is not counted
            // Note that residueFollowingSearchResidues is necessary in case the potential cleavage residue is the final residue in searchResidues
            // We need to know the next residue to determine if it matches an exception residue

            // ReSharper disable CommentTypo

            // For example, if searchResidues =      "IGASGEHIFIIGVDKPNR"
            // and the protein it is part of is: TNSANFRIGASGEHIFIIGVDKPNRQPDS
            // and searchChars = "KR while exceptionSuffixResidues  = "P"
            // Then the K in IGASGEHIFIIGVDKPNR is ignored because the following residue is P,
            // while the R in IGASGEHIFIIGVDKPNR is OK because residueFollowingSearchResidues is Q

            // ReSharper restore CommentTypo

            // It is the calling function's responsibility to assign the correct residue to residueFollowingSearchResidues
            // If no match is found, but residueFollowingSearchResidues is "-", then the cleavage location returned is Len(searchResidues) + 1

            var intExceptionSuffixResidueCount = (short)exceptionSuffixResidues.Length;

            var lngMinCharLoc = -1;
            for (var intCharLocInSearchChars = 0; intCharLocInSearchChars < searchChars.Length; intCharLocInSearchChars++)
            {
                var lngCharLoc = searchResidues.Substring(startChar).IndexOf(searchChars.Substring(intCharLocInSearchChars, 1), StringComparison.Ordinal);

                if (lngCharLoc >= 0)
                {
                    lngCharLoc = lngCharLoc + startChar - 1;

                    if (intExceptionSuffixResidueCount > 0)
                    {
                        // Make sure strSuffixResidue does not match exceptionSuffixResidues
                        int lngExceptionCharLocInSearchResidues;
                        string strResidueFollowingCleavageResidue;
                        if (lngCharLoc < searchResidues.Length - 1)
                        {
                            lngExceptionCharLocInSearchResidues = lngCharLoc + 1;
                            strResidueFollowingCleavageResidue = searchResidues.Substring(lngExceptionCharLocInSearchResidues, 1);
                        }
                        else
                        {
                            // Matched the last residue in searchResidues
                            lngExceptionCharLocInSearchResidues = searchResidues.Length + 1;
                            strResidueFollowingCleavageResidue = residueFollowingSearchResidues;
                        }

                        for (var intCharLocInExceptionChars = 0; intCharLocInExceptionChars < intExceptionSuffixResidueCount; intCharLocInExceptionChars++)
                        {
                            if ((strResidueFollowingCleavageResidue ?? "") == exceptionSuffixResidues.Substring(intCharLocInExceptionChars, 1))
                            {
                                // Exception char is the following character; can't count this as the cleavage point

                                if (lngExceptionCharLocInSearchResidues < searchResidues.Length - 1)
                                {
                                    // Recursively call this function to find the next cleavage position, using an updated startChar position
                                    var lngCharLocViaRecursiveSearch = GetTrypticNameFindNextCleavageLoc(searchResidues, residueFollowingSearchResidues, lngExceptionCharLocInSearchResidues, searchChars, exceptionSuffixResidues, terminiiSymbol);

                                    if (lngCharLocViaRecursiveSearch > 0)
                                    {
                                        // Found a residue further along that is a valid cleavage point
                                        lngCharLoc = lngCharLocViaRecursiveSearch;
                                    }
                                    else
                                    {
                                        lngCharLoc = 0;
                                    }
                                }
                                else
                                {
                                    lngCharLoc = 0;
                                }

                                break;
                            }
                        }
                    }
                }

                if (lngCharLoc > 0)
                {
                    if (lngMinCharLoc < 0)
                    {
                        lngMinCharLoc = lngCharLoc;
                    }
                    else if (lngCharLoc < lngMinCharLoc)
                    {
                        lngMinCharLoc = lngCharLoc;
                    }
                }
            }

            if (lngMinCharLoc < 0 && (residueFollowingSearchResidues ?? "") == (terminiiSymbol ?? ""))
            {
                lngMinCharLoc = searchResidues.Length + 1;
            }

            if (lngMinCharLoc < 0)
            {
                return 0;
            }

            return lngMinCharLoc;
        }

        public string GetTrypticPeptideNext(string proteinResidues,
            int searchStartLoc)
        {
            return GetTrypticPeptideNext(proteinResidues, searchStartLoc, out _, out _);
        }

        public string GetTrypticPeptideNext(string proteinResidues,
            int searchStartLoc,
            out int returnResidueStart,
            out int returnResidueEnd,
            string ruleResidues = TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL)
        {
            // Returns the next tryptic peptide in proteinResidues, starting the search as searchStartLoc
            // Useful when obtaining all of the tryptic peptides for a protein, since this function will operate
            // much faster than repeatedly calling GetTrypticPeptideByFragmentNumber()

            // Returns the position of the start and end residues using returnResidueStart and returnResidueEnd

            returnResidueStart = 1;
            returnResidueEnd = 1;

            if (searchStartLoc < 0)
                searchStartLoc = 0;

            var lngProteinResiduesLength = proteinResidues.Length;
            if (searchStartLoc >= lngProteinResiduesLength)
            {
                return string.Empty;
            }

            var lngRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(proteinResidues, terminiiSymbol, searchStartLoc, ruleResidues, exceptionResidues, terminiiSymbol);
            if (lngRuleResidueLoc >= 0)
            {
                returnResidueStart = searchStartLoc;
                if (lngRuleResidueLoc >= lngProteinResiduesLength)
                {
                    returnResidueEnd = lngProteinResiduesLength;
                }
                else
                {
                    returnResidueEnd = lngRuleResidueLoc;
                }

                return proteinResidues.Substring(returnResidueStart, returnResidueEnd - returnResidueStart + 1);
            }

            returnResidueStart = 1;
            returnResidueEnd = lngProteinResiduesLength;
            return proteinResidues;
        }

        public string GetTrypticPeptideByFragmentNumber(string proteinResidues,
            short desiredPeptideNumber)
        {
            return GetTrypticPeptideByFragmentNumber(proteinResidues, desiredPeptideNumber, out _, out _);
        }

        public string GetTrypticPeptideByFragmentNumber(string proteinResidues,
            short desiredPeptideNumber,
            out int returnResidueStart,
            out int returnResidueEnd,
            string ruleResidues = TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true)
        {
            // Returns the desired tryptic peptide from proteinResidues

            // ReSharper disable CommentTypo

            // For example, if proteinResidues = "IGKANRMTFGL" then
            // when intDesiredPeptideNumber = 1, returns "IGK"
            // when intDesiredPeptideNumber = 2, returns "ANR"
            // when intDesiredPeptideNumber = 3, returns "MTFGL"

            // ReSharper enable CommentTypo

            // Optionally, returns the position of the start and end residues
            // using returnResidueStart and returnResidueEnd

            int lngRuleResidueLoc;
            var lngPrevStartLoc = default(int);

            string strMatchingFragment;

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

            var lngProteinResiduesLength = proteinResidues.Length;

            var lngStartLoc = 0;
            short intCurrentTrypticPeptideNumber = 0;
            do
            {
                lngRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(proteinResidues, terminiiSymbol, lngStartLoc, ruleResidues, exceptionResidues, terminiiSymbol);
                if (lngRuleResidueLoc >= 0)
                {
                    intCurrentTrypticPeptideNumber = (short)(intCurrentTrypticPeptideNumber + 1);
                    lngPrevStartLoc = lngStartLoc;
                    lngStartLoc = lngRuleResidueLoc + 1;

                    if (lngPrevStartLoc >= lngProteinResiduesLength)
                    {
                        // User requested a peptide number that doesn't exist
                        return string.Empty;
                    }
                }
                else
                {
                    // I don't think I'll ever reach this code
                    break;
                }
            }
            while (intCurrentTrypticPeptideNumber < desiredPeptideNumber);

            if (intCurrentTrypticPeptideNumber > 0 && lngPrevStartLoc >= 0)
            {
                if (lngPrevStartLoc >= proteinResidues.Length)
                {
                    // User requested a peptide number that is too high
                    returnResidueStart = 0;
                    returnResidueEnd = 0;
                    strMatchingFragment = string.Empty;
                }
                else
                {
                    // Match found, find the extent of this peptide
                    returnResidueStart = lngPrevStartLoc;
                    if (lngRuleResidueLoc >= lngProteinResiduesLength)
                    {
                        returnResidueEnd = lngProteinResiduesLength;
                    }
                    else
                    {
                        returnResidueEnd = lngRuleResidueLoc;
                    }

                    strMatchingFragment = proteinResidues.Substring(lngPrevStartLoc, lngRuleResidueLoc - lngPrevStartLoc + 1);
                }
            }
            else
            {
                returnResidueStart = 0;
                returnResidueEnd = lngProteinResiduesLength - 1;
                strMatchingFragment = proteinResidues;
            }

            return strMatchingFragment;
        }

        public bool CheckSequenceAgainstCleavageRule(string sequence,
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

        public bool CheckSequenceAgainstCleavageRule(string sequence,
            string ruleResidues,
            string exceptionSuffixResidues,
            bool allowPartialCleavage,
            out short ruleMatchCount,
            string separationChar = ".",
            string terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true)
        {
            // Checks sequence to see if it matches the cleavage rule
            // Returns True if valid, False if invalid
            // Returns True if doesn't contain any periods, and thus, can't be examined
            // The ByRef variable intRuleMatchCount can be used to retrieve the number of ends that matched the rule (0, 1, or 2); terminii are counted as rule matches

            // The residues in ruleResidues specify the cleavage rule
            // The peptide must end in one of the residues, or in -
            // The preceding residue must be one of the residues or be -
            // EXCEPTION: if allowPartialCleavage = True then the rules need only apply to one end
            // Finally, the suffix residue cannot match any of the residues in exceptionSuffixResidues

            // For example, if ruleResidues = "KR" and exceptionSuffixResidues = "P"
            // Then if sequence = "R.AEQDDLANYGPGNGVLPSAGSSISMEK.L" then blnMatchesCleavageRule = True
            // However, if sequence = "R.IGASGEHIFIIGVDK.P" then blnMatchesCleavageRule = False since strSuffix = "P"
            // Finally, if sequence = "R.IGASGEHIFIIGVDKPNR.Q" then blnMatchesCleavageRule = True since K is ignored, but the final R.Q is valid

            string strSequenceStart, strSequenceEnd;
            bool blnMatchesCleavageRule = default;

            // Need to reset this to zero since passed ByRef
            ruleMatchCount = 0;
            var strPrefix = string.Empty;
            var strSuffix = string.Empty;

            // First, make sure the sequence is in the form A.BCDEFG.H or A.BCDEFG or BCDEFG.H
            // If it isn't, then we can't check it (we'll return true)

            if (string.IsNullOrEmpty(ruleResidues))
            {
                // No rule residues
                return true;
            }

            if (separationChar == null)
                separationChar = ".";

            if (!sequence.Contains(separationChar))
            {
                // No periods, can't check
                Console.WriteLine("Warning: strSequence does not contain " + separationChar + "; unable to determine cleavage state");
                return true;
            }

            if (ignoreCase)
            {
                sequence = sequence.ToUpper();
            }

            // Find the prefix residue and starting residue
            if (sequence.Substring(1, 1) == separationChar)
            {
                strPrefix = sequence.Substring(0, 1);
                strSequenceStart = sequence.Substring(2, 1);
            }
            else
            {
                strSequenceStart = sequence.Substring(0, 1);
            }

            // Find the suffix residue and the ending residue
            if (sequence.Substring(sequence.Length - 2, 1) == separationChar)
            {
                strSuffix = sequence.Substring(sequence.Length - 1);
                strSequenceEnd = sequence.Substring(sequence.Length - 3, 1);
            }
            else
            {
                strSequenceEnd = sequence.Substring(sequence.Length - 1);
            }

            if (ruleResidues == (terminiiSymbol ?? ""))
            {
                // Peptide database rules
                // See if prefix and suffix are "" or are terminiiSymbol
                if (strPrefix == (terminiiSymbol ?? "") && strSuffix == (terminiiSymbol ?? "") ||
                    strPrefix == string.Empty && strSuffix == string.Empty)
                {
                    ruleMatchCount = 2;
                    blnMatchesCleavageRule = true;
                }
                else
                {
                    blnMatchesCleavageRule = false;
                }
            }
            else
            {
                if (ignoreCase)
                {
                    ruleResidues = ruleResidues.ToUpper();
                }

                // Test each character in ruleResidues against both strPrefix and strSequenceEnd
                // Make sure strSuffix does not match exceptionSuffixResidues
                for (var intEndToCheck = 0; intEndToCheck <= 1; intEndToCheck++)
                {
                    var blnSkipThisEnd = false;
                    string strTestResidue;
                    if (intEndToCheck == 0)
                    {
                        strTestResidue = strPrefix;
                        if (strPrefix == (terminiiSymbol ?? ""))
                        {
                            ruleMatchCount = (short)(ruleMatchCount + 1);
                            blnSkipThisEnd = true;
                        }
                        // See if strSequenceStart matches one of the exception residues
                        // If it does, make sure strPrefix does not match one of the rule residues
                        else if (CheckSequenceAgainstCleavageRuleMatchTestResidue(strSequenceStart, exceptionSuffixResidues))
                        {
                            // Match found
                            // Make sure strPrefix does not match one of the rule residues
                            if (CheckSequenceAgainstCleavageRuleMatchTestResidue(strPrefix, ruleResidues))
                            {
                                // Match found; thus does not match cleavage rule
                                blnSkipThisEnd = true;
                            }
                        }
                    }
                    else
                    {
                        strTestResidue = strSequenceEnd;
                        if (strSuffix == (terminiiSymbol ?? ""))
                        {
                            ruleMatchCount = (short)(ruleMatchCount + 1);
                            blnSkipThisEnd = true;
                        }
                        // Make sure strSuffix does not match exceptionSuffixResidues
                        else if (CheckSequenceAgainstCleavageRuleMatchTestResidue(strSuffix, exceptionSuffixResidues))
                        {
                            // Match found; thus does not match cleavage rule
                            blnSkipThisEnd = true;
                        }
                    }

                    if (!blnSkipThisEnd)
                    {
                        if (CheckSequenceAgainstCleavageRuleMatchTestResidue(strTestResidue, ruleResidues))
                        {
                            ruleMatchCount = (short)(ruleMatchCount + 1);
                        }
                    }
                }

                if (ruleMatchCount == 2)
                {
                    blnMatchesCleavageRule = true;
                }
                else if (ruleMatchCount >= 1 && allowPartialCleavage)
                {
                    blnMatchesCleavageRule = true;
                }
            }

            return blnMatchesCleavageRule;
        }

        private bool CheckSequenceAgainstCleavageRuleMatchTestResidue(string testResidue, string ruleResidues)
        {
            // Checks to see if strTestResidue matches one of the residues in ruleResidues
            // Used to test by Rule Residues and Exception Residues

            for (var intCharIndex = 0; intCharIndex < ruleResidues.Length; intCharIndex++)
            {
                var strCompareResidue = ruleResidues.Substring(intCharIndex, 1).Trim();
                if (strCompareResidue.Length > 0)
                {
                    if ((testResidue ?? "") == strCompareResidue)
                    {
                        // Match found
                        return true;
                    }
                }
            }

            return false;
        }

        public double ComputeImmoniumMass(double residueMass)
        {
            return residueMass - dblImmoniumMassDifference;
        }

        private void InitializeArrays()
        {
            mNTerminus.Initialize();
            mCTerminus.Initialize();
            mFragSpectrumOptions.Initialize();
        }

        public string LookupIonTypeString(IonType ionType)
        {
            switch (ionType)
            {
                case IonType.AIon:
                    return "a";
                case IonType.BIon:
                    return "b";
                case IonType.YIon:
                    return "y";
                case IonType.CIon:
                    return "c";
                case IonType.ZIon:
                    return "z";
                default:
                    return string.Empty;
            }
        }

        public int RemoveAllResidues()
        {
            // Removes all the residues
            // Returns 0 on success, 1 on failure

            ReserveMemoryForResidues(50, false);
            ResidueCount = 0;
            mTotalMass = 0d;

            return 0;
        }

        public int RemoveAllModificationSymbols()
        {
            // Removes all possible Modification Symbols
            // Returns 0 on success, 1 on failure
            // Removing all modifications will invalidate any modifications present in a sequence

            ReserveMemoryForModifications(10, false);
            ModificationSymbolCount = 0;

            return 0;
        }

        private void RemoveLeadingH(ref string workingSequence)
        {
            // Returns True if a leading H is removed

            if (workingSequence.Length >= 4 && workingSequence.ToUpper().StartsWith("H"))
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
                    var lngAbbrevID = ElementAndMassRoutines.GetAbbreviationIDInternal(workingSequence.Substring(0, 3), true);

                    if (lngAbbrevID <= 0)
                    {
                        // Doesn't start with a valid amino acid 3 letter abbreviation, so remove the initial H
                        workingSequence = workingSequence.Substring(1);
                    }
                }
            }
        }

        private bool RemoveTrailingOH(ref string workingSequence)
        {
            // Returns True if a trailing OH is removed

            var blnOHRemoved = false;
            var lngStringLength = workingSequence.Length;
            if (workingSequence.Length >= 5 && workingSequence.ToUpper().EndsWith("OH"))
            {
                // If previous character is not a character, then remove the OH
                if (!char.IsLetter(workingSequence[lngStringLength - 3]))
                {
                    workingSequence = workingSequence.Substring(0, lngStringLength - 3);
                    blnOHRemoved = true;
                }
                // Otherwise, see if previous three characters are letters
                else if (char.IsLetter(workingSequence[lngStringLength - 3]))
                {
                    // Formula ends with 3 characters and the last two are OH, see if the last 3 characters are a valid amino acid code
                    var lngAbbrevID = ElementAndMassRoutines.GetAbbreviationIDInternal(workingSequence.Substring(lngStringLength - 3, 3), true);

                    if (lngAbbrevID <= 0)
                    {
                        // Doesn't end with a valid amino acid 3 letter abbreviation, so remove the trailing OH
                        workingSequence = workingSequence.Substring(0, lngStringLength - 2);
                        blnOHRemoved = true;
                    }
                }
            }

            return blnOHRemoved;
        }

        public int RemoveModification(ref string modSymbol)
        {
            // Returns 0 if found and removed; 1 if error

            var blnRemoved = false;

            for (var lngIndex = 1; lngIndex <= ModificationSymbolCount; lngIndex++)
            {
                if ((ModificationSymbols[lngIndex].Symbol ?? "") == (modSymbol ?? ""))
                {
                    RemoveModificationByID(lngIndex);
                    blnRemoved = true;
                }
            }

            if (blnRemoved)
            {
                return 0;
            }

            return 1;
        }

        public int RemoveModificationByID(int modificationID)
        {
            // Returns 0 if found and removed; 1 if error

            bool blnRemoved;

            if (modificationID >= 1 && modificationID <= ModificationSymbolCount)
            {
                for (var lngIndex = modificationID; lngIndex < ModificationSymbolCount; lngIndex++)
                    ModificationSymbols[lngIndex] = ModificationSymbols[lngIndex + 1];

                ModificationSymbolCount -= 1;
                blnRemoved = true;
            }
            else
            {
                blnRemoved = false;
            }

            if (blnRemoved)
            {
                return 0;
            }

            return 1;
        }

        public int RemoveResidue(int residueNumber)
        {
            // Returns 0 if found and removed; 1 if error

            if (residueNumber >= 1 && residueNumber <= ResidueCount)
            {
                for (var lngIndex = residueNumber; lngIndex < ResidueCount; lngIndex++)
                    Residues[lngIndex] = Residues[lngIndex + 1];

                ResidueCount -= 1;
                return 0;
            }

            return 1;
        }

        private void ReserveMemoryForResidues(int newResidueCount, bool preserveContents)
        {
            // Only reserves the memory if necessary
            // Thus, do not use this sub to clear Residues[]

            if (newResidueCount > ResidueCountDimmed)
            {
                ResidueCountDimmed = newResidueCount + RESIDUE_DIM_CHUNK;
                if (preserveContents && Residues != null)
                {
                    var intOldIndexEnd = Residues.Length - 1;
                    Array.Resize(ref Residues, ResidueCountDimmed + 1);
                    for (var intIndex = intOldIndexEnd + 1; intIndex <= ResidueCountDimmed; intIndex++)
                        Residues[intIndex].Initialize(true);
                }
                else
                {
                    Residues = new Residue[ResidueCountDimmed + 1];
                    for (var intIndex = 0; intIndex <= ResidueCountDimmed; intIndex++)
                    {
                        Residues[intIndex] = new Residue();
                        Residues[intIndex].Initialize(true);
                    }
                }
            }
        }

        private void ReserveMemoryForModifications(int newModificationCount, bool preserveContents)
        {
            if (newModificationCount > ModificationSymbolCountDimmed)
            {
                ModificationSymbolCountDimmed = newModificationCount + 10;
                if (preserveContents)
                {
                    Array.Resize(ref ModificationSymbols, ModificationSymbolCountDimmed + 1);
                }
                else
                {
                    ModificationSymbols = new ModificationSymbol[ModificationSymbolCountDimmed + 1];
                }

                for (var i = 0; i < ModificationSymbols.Length; i++)
                {
                    ModificationSymbols[i] = new ModificationSymbol();
                }
            }
        }

        public int SetCTerminus(string formula, string followingResidue = "", bool use3LetterCode = true)
        {
            // Returns 0 if success; 1 if error
            var success = 0;

            // Typical N terminus mods
            // Free Acid = OH
            // Amide = NH2

            mCTerminus.Formula = formula;
            mCTerminus.Mass = ElementAndMassRoutines.ComputeFormulaWeight(ref mCTerminus.Formula);
            if (mCTerminus.Mass < 0d)
            {
                mCTerminus.Mass = 0d;
                success = 1;
            }
            else
            {
                success = 0;
            }

            mCTerminus.PrecedingResidue = FillResidueStructureUsingSymbol(string.Empty);
            mCTerminus.FollowingResidue = FillResidueStructureUsingSymbol(followingResidue, use3LetterCode);

            UpdateResidueMasses();
            return success;
        }

        public int SetCTerminusGroup(CTerminusGroupType cTerminusGroup,
            string followingResidue = "",
            bool use3LetterCode = true)
        {
            // Returns 0 if success; 1 if error
            int lngError;
            switch (cTerminusGroup)
            {
                case CTerminusGroupType.Hydroxyl:
                    lngError = SetCTerminus("OH", followingResidue, use3LetterCode);
                    break;
                case CTerminusGroupType.Amide:
                    lngError = SetCTerminus("NH2", followingResidue, use3LetterCode);
                    break;
                case CTerminusGroupType.None:
                    lngError = SetCTerminus(string.Empty, followingResidue, use3LetterCode);
                    break;
                default:
                    lngError = 1;
                    break;
            }

            return lngError;
        }

        public void SetDefaultModificationSymbols()
        {
            try
            {
                RemoveAllModificationSymbols();

                // Add the symbol for phosphorylation
                SetModificationSymbol("*", dblPhosphorylationMass, true, "Phosphorylation [HPO3]");

                // Define the other default modifications
                // Valid Mod Symbols are ! # $ % & ' * + ? ^ _ ` ~

                SetModificationSymbol("+", 14.01565d, false, "Methylation [CH2]");
                SetModificationSymbol("@", 15.99492d, false, "Oxidation [O]");
                SetModificationSymbol("!", 57.02146d, false, "Carbamidomethylation [C2H3NO]");
                SetModificationSymbol("&", 58.00548d, false, "Carboxymethylation [CH2CO2]");
                SetModificationSymbol("#", 71.03711d, false, "Acrylamide [CHCH2CONH2]");
                SetModificationSymbol("$", 227.127d, false, "Cleavable ICAT [(^12C10)H17N3O3]");
                SetModificationSymbol("%", 236.127d, false, "Cleavable ICAT [(^13C9)(^12C)H17N3O3]");
                SetModificationSymbol("~", 442.225d, false, "ICAT D0 [C20H34N4O5S]");
                SetModificationSymbol("`", 450.274d, false, "ICAT D8 [C20H26D8N4O5S]");
            }
            catch (Exception ex)
            {
                ElementAndMassRoutines.GeneralErrorHandler("Peptide.SetDefaultModificationSymbols", ex);
            }
        }

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
                var aIonOption = mFragSpectrumOptions.IonTypeOptions[(int)IonType.AIon];
                aIonOption.ShowIon = true;
                aIonOption.NeutralLossAmmonia = true;
                aIonOption.NeutralLossPhosphate = true;
                aIonOption.NeutralLossWater = false;

                for (var eIonIndex = IonType.BIon; eIonIndex <= IonType.ZIon; eIonIndex++)
                {
                    var ionOption = mFragSpectrumOptions.IonTypeOptions[(int)eIonIndex];
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
                ElementAndMassRoutines.GeneralErrorHandler("Peptide.SetDefaultOptions", ex);
            }
        }

        public void SetFragmentationSpectrumOptions(FragmentationSpectrumOptions newFragSpectrumOptions)
        {
            mFragSpectrumOptions = newFragSpectrumOptions;
        }

        public int SetModificationSymbol(string modSymbol, double modificationMass, bool indicatesPhosphorylation = false, string comment = "")
        {
            // Adds a new modification or updates an existing one (based on modSymbol)
            // Returns 0 if successful, otherwise, returns -1

            var lngErrorID = 0;
            if (modSymbol.Length < 1)
            {
                lngErrorID = -1;
            }
            else
            {
                // Make sure modSymbol contains no letters, numbers, spaces, dashes, or periods
                for (var lngIndex = 0; lngIndex < modSymbol.Length; lngIndex++)
                {
                    var strTestChar = modSymbol.Substring(lngIndex, 1);
                    if (!ElementAndMassRoutines.IsModSymbolInternal(strTestChar))
                    {
                        lngErrorID = -1;
                    }
                }

                if (lngErrorID == 0)
                {
                    // See if the modification is alrady present
                    var lngIndexToUse = GetModificationSymbolID(modSymbol);

                    if (lngIndexToUse == 0)
                    {
                        // Need to add the modification
                        ModificationSymbolCount += 1;
                        lngIndexToUse = ModificationSymbolCount;
                        ReserveMemoryForModifications(ModificationSymbolCount, true);
                    }

                    var mod = ModificationSymbols[lngIndexToUse];
                    mod.Symbol = modSymbol;
                    mod.ModificationMass = modificationMass;
                    mod.IndicatesPhosphorylation = indicatesPhosphorylation;
                    mod.Comment = comment;
                }
            }

            return lngErrorID;
        }

        public int SetNTerminus(string formula, string precedingResidue = "", bool use3LetterCode = true)
        {
            // Returns 0 if success; 1 if error
            var success = 0;

            // Typical N terminus mods
            // Hydrogen = H
            // Acetyl = C2OH3
            // PyroGlu = C5O2NH6
            // Carbamyl = CONH2
            // PTC = C7H6NS

            mNTerminus.Formula = formula;
            mNTerminus.Mass = ElementAndMassRoutines.ComputeFormulaWeight(ref mNTerminus.Formula);
            if (mNTerminus.Mass < 0d)
            {
                mNTerminus.Mass = 0d;
                success = 1;
            }
            else
            {
                success = 0;
            }

            mNTerminus.PrecedingResidue = FillResidueStructureUsingSymbol(precedingResidue, use3LetterCode);
            mNTerminus.FollowingResidue = FillResidueStructureUsingSymbol(string.Empty);

            UpdateResidueMasses();
            return success;
        }

        public int SetNTerminusGroup(NTerminusGroupType nTerminusGroup,
            string precedingResidue = "",
            bool use3LetterCode = true)
        {
            // Returns 0 if success; 1 if error
            int lngError;

            switch (nTerminusGroup)
            {
                case NTerminusGroupType.Hydrogen:
                    lngError = SetNTerminus("H", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupType.HydrogenPlusProton:
                    lngError = SetNTerminus("HH", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupType.Acetyl:
                    lngError = SetNTerminus("C2OH3", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupType.PyroGlu:
                    lngError = SetNTerminus("C5O2NH6", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupType.Carbamyl:
                    lngError = SetNTerminus("CONH2", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupType.PTC:
                    lngError = SetNTerminus("C7H6NS", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupType.None:
                    lngError = SetNTerminus(string.Empty, precedingResidue, use3LetterCode);
                    break;
                default:
                    lngError = 1;
                    break;
            }

            return lngError;
        }

        public int SetResidue(int residueNumber,
            string symbol,
            bool is3LetterCode = true,
            bool phosphorylated = false)
        {
            // Sets or adds a residue (must add residues in order)
            // Returns the index of the modified residue, or the new index if added
            // Returns -1 if a problem

            int lngIndexToUse;
            string str3LetterSymbol;

            if (string.IsNullOrEmpty(symbol))
            {
                return -1;
            }

            if (residueNumber > ResidueCount)
            {
                ResidueCount += 1;
                ReserveMemoryForResidues(ResidueCount, true);
                lngIndexToUse = ResidueCount;
            }
            else
            {
                lngIndexToUse = residueNumber;
            }

            var residue = Residues[lngIndexToUse];
            if (is3LetterCode)
            {
                str3LetterSymbol = symbol;
            }
            else
            {
                str3LetterSymbol = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(symbol, true);
            }

            if (str3LetterSymbol.Length == 0)
            {
                residue.Symbol = UNKNOWN_SYMBOL;
            }
            else
            {
                residue.Symbol = str3LetterSymbol;
            }

            residue.Phosphorylated = phosphorylated;
            if (phosphorylated)
            {
                // Only Ser, Thr, or Tyr should be phosphorylated
                // However, if the user sets other residues as phosphorylated, we'll allow that
                if (!(residue.Symbol == "Ser" || residue.Symbol == "Thr" || residue.Symbol == "Tyr"))
                {
                    Console.WriteLine("Residue '" + residue.Symbol + "' is marked as being phosphorylated; this is unexpected");
                }
            }

            residue.ModificationIDCount = 0;

            UpdateResidueMasses();

            return lngIndexToUse;
        }

        public int SetResidueModifications(int residueNumber, short modificationCount, int[] modificationIDsOneBased)
        {
            // Sets the modifications for a specific residue
            // Modification Symbols are defined using successive calls to SetModificationSymbol()

            // Returns 0 if modifications set; returns 1 if an error

            if (residueNumber >= 1 && residueNumber <= ResidueCount && modificationCount >= 0)
            {
                var residue = Residues[residueNumber];
                if (modificationCount > MAX_MODIFICATIONS)
                {
                    modificationCount = MAX_MODIFICATIONS;
                }

                residue.ModificationIDCount = 0;
                residue.Phosphorylated = false;
                for (var intIndex = 1; intIndex <= modificationCount; intIndex++)
                {
                    var lngNewModID = modificationIDsOneBased[intIndex];
                    if (lngNewModID >= 1 && lngNewModID <= ModificationSymbolCount)
                    {
                        residue.ModificationIDs[residue.ModificationIDCount] = lngNewModID;

                        // Check for phosphorylation
                        if (ModificationSymbols[lngNewModID].IndicatesPhosphorylation)
                        {
                            residue.Phosphorylated = true;
                        }

                        residue.ModificationIDCount = (short)(residue.ModificationIDCount + 1);
                    }
                }

                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="sequence">Peptide sequence using 1-letter amino acid symbols</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If <paramref name="sequence"/> is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence1LetterSymbol(string sequence)
        {
            return SetSequence(sequence, is3LetterCode: false);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="sequence">Peptide sequence</param>
        /// <param name="is3LetterCode">Set to true for 3-letter amino acid symbols, false for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues">Set to true to check for and remove prefix and suffix residues when <paramref name="is3LetterCode"/> = false</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If <paramref name="sequence"/> is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence(string sequence,
            bool is3LetterCode,
            bool oneLetterCheckForPrefixAndSuffixResidues)
        {
            return SetSequence(sequence, NTerminusGroupType.Hydrogen, CTerminusGroupType.Hydroxyl,
                is3LetterCode, oneLetterCheckForPrefixAndSuffixResidues);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="sequence">Peptide sequence, using 3-letter amino acid symbols (unless <paramref name="is3LetterCode"/> = false)</param>
        /// <param name="nTerminus">N-terminus group</param>
        /// <param name="cTerminus">C-terminus group</param>
        /// <param name="is3LetterCode">Set to true for 3-letter amino acid symbols, false for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues">Set to true to check for and remove prefix and suffix residues when <paramref name="is3LetterCode"/> = false</param>
        /// <param name="threeLetterCheckForPrefixHandSuffixOH">Set to true to check for and remove prefix H and OH when <paramref name="is3LetterCode"/> = true</param>
        /// <param name="addMissingModificationSymbols">Set to true to automatically add missing modification symbols (though the mod masses will be 0)</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If <paramref name="sequence" /> is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence(string sequence,
            NTerminusGroupType nTerminus = NTerminusGroupType.Hydrogen,
            CTerminusGroupType cTerminus = CTerminusGroupType.Hydroxyl,
            bool is3LetterCode = true,
            bool oneLetterCheckForPrefixAndSuffixResidues = true,
            bool threeLetterCheckForPrefixHandSuffixOH = true,
            bool addMissingModificationSymbols = false)
        {
            try
            {
                sequence = sequence.Trim();

                var lngSequenceStrLength = sequence.Length;
                if (lngSequenceStrLength == 0)
                {
                    return AssureNonZero(0);
                }

                // Clear any old residue information
                ResidueCount = 0;
                ReserveMemoryForResidues(ResidueCount, false);

                string str3LetterSymbol;
                int lngModSymbolLength;
                if (!is3LetterCode)
                {
                    // Sequence is 1 letter codes

                    if (oneLetterCheckForPrefixAndSuffixResidues)
                    {
                        // First look if sequence is in the form A.BCDEFG.Z or -.BCDEFG.Z or A.BCDEFG.-
                        // If so, then need to strip out the preceding A and Z residues since they aren't really part of the sequence
                        if (lngSequenceStrLength > 1 && sequence.Contains("."))
                        {
                            if (sequence.Substring(1, 1) == ".")
                            {
                                sequence = sequence.Substring(2);
                                lngSequenceStrLength = sequence.Length;
                            }

                            if (sequence.Substring(lngSequenceStrLength - 2, 1) == ".")
                            {
                                sequence = sequence.Substring(0, lngSequenceStrLength - 2);
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

                            lngSequenceStrLength = sequence.Length;
                        }
                    }

                    for (var lngIndex = 0; lngIndex < lngSequenceStrLength; lngIndex++)
                    {
                        var str1LetterSymbol = sequence.Substring(lngIndex, 1);
                        if (char.IsLetter(str1LetterSymbol[0]))
                        {
                            // Character found
                            // Look up 3 letter symbol
                            // If none is found, this will return an empty string
                            str3LetterSymbol = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(str1LetterSymbol, true);

                            if (str3LetterSymbol.Length == 0)
                                str3LetterSymbol = UNKNOWN_SYMBOL;

                            SetSequenceAddResidue(str3LetterSymbol);

                            // Look at following character(s), and record any modification symbols present
                            lngModSymbolLength = CheckForModifications(sequence.Substring(lngIndex + 1), ResidueCount, addMissingModificationSymbols);

                            lngIndex += lngModSymbolLength;
                        }
                        // If . or - or space, then ignore it
                        // If a number, ignore it
                        // If anything else, then should have been skipped, or should be skipped
                        else if (str1LetterSymbol == "." || str1LetterSymbol == "-" || str1LetterSymbol == " ")
                        {
                            // All is fine; we can skip this
                        }
                    }
                }
                else
                {
                    // Sequence is 3 letter codes
                    var lngIndex = 0;

                    if (threeLetterCheckForPrefixHandSuffixOH)
                    {
                        // Look for a leading H or trailing OH, provided those don't match any of the amino acids
                        RemoveLeadingH(ref sequence);
                        RemoveTrailingOH(ref sequence);

                        // Recompute sequence length
                        lngSequenceStrLength = sequence.Length;
                    }

                    while (lngIndex < lngSequenceStrLength - 3)
                    {
                        var strFirstChar = sequence.Substring(lngIndex, 1);
                        if (char.IsLetter(strFirstChar[0]))
                        {
                            if (char.IsLetter(sequence[lngIndex + 1]) && char.IsLetter(sequence[lngIndex + 2]))
                            {
                                str3LetterSymbol = strFirstChar.ToUpper() + sequence.Substring(lngIndex + 1, 2).ToLower();

                                if (ElementAndMassRoutines.GetAbbreviationIDInternal(str3LetterSymbol, true) == 0)
                                {
                                    // 3 letter symbol not found
                                    // Add anyway, but mark as Xxx
                                    str3LetterSymbol = UNKNOWN_SYMBOL;
                                }

                                SetSequenceAddResidue(str3LetterSymbol);

                                // Look at following character(s), and record any modification symbols present
                                lngModSymbolLength = CheckForModifications(sequence.Substring(lngIndex + 3), ResidueCount, addMissingModificationSymbols);

                                lngIndex += 3;
                                lngIndex += lngModSymbolLength;
                            }
                            else
                            {
                                // First letter is a character, but next two are not; ignore it
                                lngIndex += 1;
                            }
                        }
                        else
                        {
                            // If . or - or space, then ignore it
                            // If a number, ignore it
                            // If anything else, then should have been skipped or should be skipped
                            if (strFirstChar == "." || strFirstChar == "-" || strFirstChar == " ")
                            {
                                // All is fine; we can skip this
                            }

                            lngIndex += 1;
                        }
                    }
                }

                // By calling SetNTerminus and SetCTerminus, the UpdateResidueMasses() Sub will also be called
                mDelayUpdateResidueMass = true;
                SetNTerminusGroup(nTerminus);
                SetCTerminusGroup(cTerminus);

                mDelayUpdateResidueMass = false;
                UpdateResidueMasses();

                return 0;
            }
            catch
            {
                return AssureNonZero(Information.Err().Number);
            }
        }

        private void SetSequenceAddResidue(string threeLetterSymbol)
        {
            if (string.IsNullOrWhiteSpace(threeLetterSymbol))
            {
                threeLetterSymbol = UNKNOWN_SYMBOL;
            }

            ResidueCount += 1;
            ReserveMemoryForResidues(ResidueCount, true);

            var residue = Residues[ResidueCount];
            residue.Symbol = threeLetterSymbol;
            residue.Phosphorylated = false;
            residue.ModificationIDCount = 0;
        }

        public void SetSymbolAmmoniaLoss(string newSymbol)
        {
            if (!string.IsNullOrWhiteSpace(newSymbol))
            {
                mAmmoniaLossSymbol = newSymbol;
            }
        }

        public void SetSymbolPhosphoLoss(string newSymbol)
        {
            if (!string.IsNullOrWhiteSpace(newSymbol))
            {
                mPhosphoLossSymbol = newSymbol;
            }
        }

        public void SetSymbolWaterLoss(string newSymbol)
        {
            if (!string.IsNullOrWhiteSpace(newSymbol))
            {
                mWaterLossSymbol = newSymbol;
            }
        }

        private void ShellSortFragSpectrum(ref FragmentationSpectrumData[] fragSpectrumWork, ref int[] pointerArray, int lowIndex, int highIndex)
        {
            // Sort the list using a shell sort

            // Sort PointerArray[lngLowIndex..lngHighIndex] by comparing FragSpectrumWork(PointerArray(x)).Mass

            // Compute largest increment
            var lngCount = highIndex - lowIndex + 1;
            var lngIncrement = 1;
            if (lngCount < 14)
            {
                lngIncrement = 1;
            }
            else
            {
                while (lngIncrement < lngCount)
                    lngIncrement = 3 * lngIncrement + 1;

                lngIncrement /= 3;
                lngIncrement /= 3;
            }

            while (lngIncrement > 0)
            {
                // Sort by insertion in increments of lngIncrement
                for (var lngIndex = lowIndex + lngIncrement; lngIndex <= highIndex; lngIndex++)
                {
                    var lngPointerSwap = pointerArray[lngIndex];
                    int lngIndexCompare;
                    for (lngIndexCompare = lngIndex - lngIncrement; lngIndexCompare >= lowIndex; lngIndexCompare += -lngIncrement)
                    {
                        // Use <= to sort ascending; Use > to sort descending
                        if (fragSpectrumWork[pointerArray[lngIndexCompare]].Mass <= fragSpectrumWork[lngPointerSwap].Mass)
                            break;
                        pointerArray[lngIndexCompare + lngIncrement] = pointerArray[lngIndexCompare];
                    }

                    pointerArray[lngIndexCompare + lngIncrement] = lngPointerSwap;
                }

                lngIncrement /= 3;
            }
        }

        private void UpdateResidueMasses()
        {
            var lngValidResidueCount = default(int);
            var blnProtonatedNTerminus = default(bool);

            if (mDelayUpdateResidueMass)
                return;

            // The N-terminus ions are the basis for the running total
            var dblRunningTotal = mNTerminus.Mass;
            if (mNTerminus.Formula.ToUpper() == "HH")
            {
                // ntgHydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                // we need to subtract it out here
                // However, we need to subtract out dblHydrogenMass, and not dblChargeCarrierMass since the current
                // formula's mass was computed using two hydrogens, and not one hydrogen and one charge carrier
                blnProtonatedNTerminus = true;
                dblRunningTotal -= dblHydrogenMass;
            }

            for (var lngIndex = 1; lngIndex <= ResidueCount; lngIndex++)
            {
                var residue = Residues[lngIndex];
                residue.Initialize();

                var lngAbbrevID = ElementAndMassRoutines.GetAbbreviationIDInternal(residue.Symbol, true);

                if (lngAbbrevID > 0)
                {
                    lngValidResidueCount += 1;
                    residue.Mass = ElementAndMassRoutines.GetAbbreviationMass(lngAbbrevID);

                    var blnPhosphorylationMassAdded = false;

                    // Compute the mass, including the modifications
                    residue.MassWithMods = residue.Mass;
                    for (var intModIndex = 1; intModIndex <= residue.ModificationIDCount; intModIndex++)
                    {
                        if (residue.ModificationIDs[intModIndex] <= ModificationSymbolCount)
                        {
                            residue.MassWithMods += ModificationSymbols[residue.ModificationIDs[intModIndex]].ModificationMass;
                            if (ModificationSymbols[residue.ModificationIDs[intModIndex]].IndicatesPhosphorylation)
                            {
                                blnPhosphorylationMassAdded = true;
                            }
                        }
                        else
                        {
                            // Invalid ModificationID
                            Console.WriteLine("Invalid ModificationID: " + residue.ModificationIDs[intModIndex]);
                        }
                    }

                    if (residue.Phosphorylated)
                    {
                        // Only add a mass if none of the .ModificationIDs has .IndicatesPhosphorylation = True
                        if (!blnPhosphorylationMassAdded)
                        {
                            residue.MassWithMods += dblPhosphorylationMass;
                        }
                    }

                    dblRunningTotal += residue.MassWithMods;

                    residue.IonMass[(int)IonType.AIon] = dblRunningTotal - dblImmoniumMassDifference - dblChargeCarrierMass;
                    residue.IonMass[(int)IonType.BIon] = dblRunningTotal;

                    // Add NH3 (ammonia) to the B ion mass to get the C ion mass
                    residue.IonMass[(int)IonType.CIon] = residue.IonMass[(int)IonType.BIon] + dblNH3Mass;
                }
                else
                {
                    residue.Mass = 0d;
                    residue.MassWithMods = 0d;
                    Array.Clear(residue.IonMass, 0, residue.IonMass.Length);
                }
            }

            dblRunningTotal += mCTerminus.Mass;
            if (blnProtonatedNTerminus)
            {
                dblRunningTotal += dblChargeCarrierMass;
            }

            if (lngValidResidueCount > 0)
            {
                mTotalMass = dblRunningTotal;
            }
            else
            {
                mTotalMass = 0d;
            }

            // Now compute the y-ion and z-ion masses
            dblRunningTotal = mCTerminus.Mass + dblChargeCarrierMass;

            for (var lngIndex = ResidueCount; lngIndex >= 1; lngIndex -= 1)
            {
                var residue = Residues[lngIndex];
                if (residue.IonMass[(int)IonType.AIon] > 0d)
                {
                    dblRunningTotal += residue.MassWithMods;
                    residue.IonMass[(int)IonType.YIon] = dblRunningTotal + dblChargeCarrierMass;
                    if (lngIndex == 1)
                    {
                        // Add the N-terminus mass to highest y ion
                        residue.IonMass[(int)IonType.YIon] = residue.IonMass[(int)IonType.YIon] + mNTerminus.Mass - dblChargeCarrierMass;
                        if (blnProtonatedNTerminus)
                        {
                            // ntgHydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                            // we need to subtract it out here
                            // However, we need to subtract out dblHydrogenMass, and not dblChargeCarrierMass since the current
                            // formula's mass was computed using two hydrogens, and not one hydrogen and one charge carrier
                            residue.IonMass[(int)IonType.YIon] = residue.IonMass[(int)IonType.YIon] - dblHydrogenMass;
                        }
                    }

                    // Subtract NH2 (amide) from the Y ion mass to get the Z ion mass
                    residue.IonMass[(int)IonType.ZIon] = residue.IonMass[(int)IonType.YIon] - (dblNH3Mass - dblHydrogenMass);
                }
            }
        }

        public void UpdateStandardMasses()
        {
            try
            {
                var eElementModeSaved = ElementAndMassRoutines.GetElementModeInternal();

                ElementAndMassRoutines.SetElementModeInternal(ElementAndMassTools.ElementMassMode.Isotopic);

                dblChargeCarrierMass = ElementAndMassRoutines.GetChargeCarrierMassInternal();

                // Update standard mass values
                dblHOHMass = ElementAndMassRoutines.ComputeFormulaWeight("HOH");
                dblNH3Mass = ElementAndMassRoutines.ComputeFormulaWeight("NH3");
                dblH3PO4Mass = ElementAndMassRoutines.ComputeFormulaWeight("H3PO4");
                dblHydrogenMass = ElementAndMassRoutines.ComputeFormulaWeight("H");

                // Phosphorylation is the loss of OH and the addition of H2PO4, for a net change of HPO3
                dblPhosphorylationMass = ElementAndMassRoutines.ComputeFormulaWeight("HPO3");

                // The immonium mass is equal to the mass of CO minus the mass of H, thus typically 26.9871
                dblImmoniumMassDifference = ElementAndMassRoutines.ComputeFormulaWeight("CO") - dblHydrogenMass;
                dblHistidineFW = ElementAndMassRoutines.ComputeFormulaWeight("His");
                dblPhenylalanineFW = ElementAndMassRoutines.ComputeFormulaWeight("Phe");
                dblTyrosineFW = ElementAndMassRoutines.ComputeFormulaWeight("Tyr");

                ElementAndMassRoutines.SetElementModeInternal(eElementModeSaved);
            }
            catch (Exception ex)
            {
                ElementAndMassRoutines.GeneralErrorHandler("Peptide.UpdateStandardMasses", ex);
            }
        }

        private void InitializeClass()
        {
            try
            {
                InitializeArrays();

                ResidueCountDimmed = 0;
                ResidueCount = 0;
                ReserveMemoryForResidues(50, false);

                ModificationSymbolCountDimmed = 0;
                ModificationSymbolCount = 0;
                ReserveMemoryForModifications(10, false);

                SetDefaultOptions();
            }
            catch (Exception ex)
            {
                ElementAndMassRoutines.GeneralErrorHandler("Peptide.Class_Initialize", ex);
            }
        }
    }
}