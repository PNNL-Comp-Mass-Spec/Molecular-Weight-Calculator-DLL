using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

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

        public Peptide() : base()
        {
            ElementAndMassRoutines = new ElementAndMassTools();
            InitializeClass();
        }

        public Peptide(ElementAndMassTools objElementAndMassTools) : base()
        {
            ElementAndMassRoutines = objElementAndMassTools;
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

        public enum ctgCTerminusGroupConstants
        {
            ctgHydroxyl = 0,
            ctgAmide = 1,
            ctgNone = 2
        }

        public enum ntgNTerminusGroupConstants
        {
            ntgHydrogen = 0,
            ntgHydrogenPlusProton = 1,
            ntgAcetyl = 2,
            ntgPyroGlu = 3,
            ntgCarbamyl = 4,
            ntgPTC = 5,
            ntgNone = 6
        }

        private const itIonTypeConstants ION_TYPE_MAX = itIonTypeConstants.itZIon;

        public enum itIonTypeConstants
        {
            itAIon = 0,
            itBIon = 1,
            itYIon = 2,
            itCIon = 3,
            itZIon = 4
        }

        private class udtModificationSymbolType
        {
            public string Symbol; // Symbol used for modification in formula; may be 1 or more characters; for example: + ++ * ** etc.
            public double ModificationMass; // Normally positive, but could be negative
            public bool IndicatesPhosphorylation; // When true, then this symbol means a residue is phosphorylated
            public string Comment;
        }

        private class udtResidueType
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
                    ModificationIDs = new int[(MAX_MODIFICATIONS + 1)];
                }
            }
        }

        private class udtTerminusType
        {
            public string Formula;
            public double Mass;
            public udtResidueType PrecedingResidue = new udtResidueType(); // If the peptide sequence is part of a protein, the user can record the final residue of the previous peptide sequence here
            public udtResidueType FollowingResidue = new udtResidueType(); // If the peptide sequence is part of a protein, the user can record the first residue of the next peptide sequence here

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                PrecedingResidue.Initialize();
                FollowingResidue.Initialize();
            }
        }

        public class udtFragmentationSpectrumIntensitiesType
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
        public class udtIonTypeOptionsType
        {
            public bool ShowIon;
            public bool NeutralLossWater;
            public bool NeutralLossAmmonia;
            public bool NeutralLossPhosphate;
        }

        public class udtFragmentationSpectrumOptionsType
        {
            public udtFragmentationSpectrumIntensitiesType IntensityOptions = new udtFragmentationSpectrumIntensitiesType();
            public udtIonTypeOptionsType[] IonTypeOptions;
            public bool DoubleChargeIonsShow;
            public float DoubleChargeIonsThreshold;
            public bool TripleChargeIonsShow;
            public float TripleChargeIonsThreshold;

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                IntensityOptions.Initialize();
                IonTypeOptions = new udtIonTypeOptionsType[5];
            }
        }

        public class udtFragmentationSpectrumDataType
        {
            public double Mass;
            public double Intensity;
            public string Symbol; // The symbol, with the residue number (e.g. y1, y2, b3-H2O, Shoulder-y1, etc.)
            public string SymbolGeneric; // The symbol, without the residue number (e.g. a, b, y, b++, Shoulder-y, etc.)
            public int SourceResidueNumber; // The residue number that resulted in this mass
            public string SourceResidueSymbol3Letter; // The residue symbol that resulted in this mass
            public short Charge;
            public itIonTypeConstants IonType;
            public bool IsShoulderIon; // B and Y ions can have Shoulder ions at +-1

            public override string ToString()
            {
                return Symbol + ", " + Mass.ToString("0.00");
            }
        }

        // Note: A peptide goes from N to C, eg. HGlyLeuTyrOH has N-Terminus = H and C-Terminus = OH
        // Residue 1 would be Gly, Residue 2 would be Leu, Residue 3 would be Tyr
        private udtResidueType[] Residues; // 1-based array
        private int ResidueCount;
        private int ResidueCountDimmed;

        // ModificationSymbols() holds a list of the potential modification symbols and the mass of each modification
        // Modification symbols can be 1 or more letters long
        private udtModificationSymbolType[] ModificationSymbols; // 1-based array
        private int ModificationSymbolCount;
        private int ModificationSymbolCountDimmed;

        // ReSharper disable once UnassignedField.Local - initialized in InitializeClass() when it calls InitializeArrays()
        private readonly udtTerminusType mNTerminus = new udtTerminusType(); // Formula on the N-Terminus

        // ReSharper disable once UnassignedField.Local - initialized in InitializeClass() when it calls InitializeArrays()
        private readonly udtTerminusType mCTerminus = new udtTerminusType(); // Formula on the C-Terminus
        private double mTotalMass;

        private string mWaterLossSymbol; // -H2O
        private string mAmmoniaLossSymbol; // -NH3
        private string mPhosphoLossSymbol; // -H3PO4

        private udtFragmentationSpectrumOptionsType mFragSpectrumOptions = new udtFragmentationSpectrumOptionsType();

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

        private void AppendDataToFragSpectrum(ref int lngIonCount, ref udtFragmentationSpectrumDataType[] FragSpectrumWork, float sngMass, float sngIntensity, string strIonSymbol, string strIonSymbolGeneric, int lngSourceResidue, string strSourceResidueSymbol3Letter, short intCharge, itIonTypeConstants eIonType, bool blnIsShoulderIon)
        {
            try
            {
                if (lngIonCount > Information.UBound(FragSpectrumWork))
                {
                    // This shouldn't happen
                    Console.WriteLine("In AppendDataToFragSpectrum, lngIonCount is greater than UBound(FragSpectrumWork); this is unexpected");
                    Array.Resize(ref FragSpectrumWork, Information.UBound(FragSpectrumWork) + 10 + 1);
                }

                var fragIon = FragSpectrumWork[lngIonCount];
                fragIon.Mass = sngMass;
                fragIon.Intensity = sngIntensity;
                fragIon.Symbol = strIonSymbol;
                fragIon.SymbolGeneric = strIonSymbolGeneric;
                fragIon.SourceResidueNumber = lngSourceResidue;
                fragIon.SourceResidueSymbol3Letter = strSourceResidueSymbol3Letter;
                fragIon.Charge = intCharge;
                fragIon.IonType = eIonType;
                fragIon.IsShoulderIon = blnIsShoulderIon;

                lngIonCount += 1;
            }
            catch
            {
                Console.WriteLine(Information.Err().Description);
            }
        }

        public int AssureNonZero(int lngNumber)
        {
            // Returns a non-zero number, either -1 if lngNumber = 0 or lngNumber if it's nonzero
            if (lngNumber == 0)
            {
                return -1;
            }
            else
            {
                return lngNumber;
            }
        }

        private int CheckForModifications(string strPartialSequence, int intResidueNumber, bool blnAddMissingModificationSymbols = false)
        {
            // Looks at strPartialSequence to see if it contains 1 or more modifications
            // If any modification symbols are found, the modification is recorded in .ModificationIDs()
            // If all or part of the modification symbol is not found in ModificationSymbols(), then a new entry
            // is added to ModificationSymbols()
            // Returns the total length of all modifications found

            var intSequenceStrLength = strPartialSequence.Length;

            // Find the entire group of potential modification symbols
            var strModSymbolGroup = string.Empty;
            var intCompareIndex = 0;
            while (intCompareIndex < intSequenceStrLength)
            {
                var strTestChar = strPartialSequence.Substring(intCompareIndex, 1);
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
                    if (blnAddMissingModificationSymbols)
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
                    var residue = Residues[intResidueNumber];
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
            short ComputeMaxIonsPerResidueRet = default;
            // Estimate the total ions per residue that will be created
            // This number will nearly always be much higher than the number of ions that will actually
            // be stored for a given sequence, since not all will be doubly charged, and not all will show
            // all of the potential neutral losses

            itIonTypeConstants eIonIndex;

            short intIonCount = 0;

            for (eIonIndex = 0; eIonIndex <= ION_TYPE_MAX; eIonIndex++)
            {
                if (mFragSpectrumOptions.IonTypeOptions[(int)eIonIndex].ShowIon)
                {
                    intIonCount = (short)(intIonCount + 1);
                    if (Math.Abs(mFragSpectrumOptions.IntensityOptions.BYIonShoulder) > 0d)
                    {
                        if (eIonIndex == itIonTypeConstants.itBIon || eIonIndex == itIonTypeConstants.itYIon ||
                            eIonIndex == itIonTypeConstants.itCIon || eIonIndex == itIonTypeConstants.itZIon)
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

            ComputeMaxIonsPerResidueRet = intIonCount;
            return ComputeMaxIonsPerResidueRet;
        }

        private udtResidueType FillResidueStructureUsingSymbol(string strSymbol, bool blnUse3LetterCode = true)
        {
            // Returns a variable of type udtResidueType containing strSymbol as the residue symbol
            // If strSymbol is a valid amino acid type, then also updates udtResidue with the default information

            int lngAbbrevID;
            var udtResidue = new udtResidueType();

            // Initialize the UDTs
            udtResidue.Initialize();
            var strSymbol3Letter = string.Empty;

            if (strSymbol.Length > 0)
            {
                if (blnUse3LetterCode)
                {
                    strSymbol3Letter = strSymbol;
                }
                else
                {
                    strSymbol3Letter = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbol, true);
                    if (strSymbol3Letter.Length == 0)
                    {
                        strSymbol3Letter = strSymbol;
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
        /// <param name="udtFragSpectrum"></param>
        /// <returns>The number of ions in udtFragSpectrum()</returns>
        /// <remarks></remarks>
        public int GetFragmentationMasses(ref udtFragmentationSpectrumDataType[] udtFragSpectrum)
        {
            // Old: Func GetFragmentationMasses(lngMaxIonCount As Long, ByRef sngIonMassesZeroBased() As Single, ByRef sngIonIntensitiesZeroBased() As Single, ByRef strIonSymbolsZeroBased() As String) As Long

            var lstFragSpectraData = GetFragmentationMasses();

            if (lstFragSpectraData.Count == 0)
            {
                udtFragSpectrum = new udtFragmentationSpectrumDataType[1];
                return 0;
            }

            udtFragSpectrum = new udtFragmentationSpectrumDataType[lstFragSpectraData.Count + 1];

            for (int intIndex = 0; intIndex < lstFragSpectraData.Count; intIndex++)
                udtFragSpectrum[intIndex] = lstFragSpectraData[intIndex];

            return lstFragSpectraData.Count;
        }

        public List<udtFragmentationSpectrumDataType> GetFragmentationMasses()
        {
            const int MAX_CHARGE = 3;

            int lngResidueIndex;
            itIonTypeConstants eIonType;
            int lngIndex;
            var sngIonIntensities = new float[5];

            var blnPhosphorylated = default(bool);

            if (ResidueCount == 0)
            {
                // No residues
                return new List<udtFragmentationSpectrumDataType>();
            }

            var blnShowCharge = new bool[4];
            var sngChargeThreshold = new float[4];

            // Copy some of the values from mFragSpectrumOptions to local variables to make things easier to read
            for (eIonType = 0; eIonType <= ION_TYPE_MAX; eIonType++)
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
            var FragSpectrumWork = new udtFragmentationSpectrumDataType[lngPredictedIonCount + 1];

            // Need to update the residue masses in case the modifications have changed
            UpdateResidueMasses();

            var lngIonCount = 0;
            for (lngResidueIndex = 1; lngResidueIndex <= ResidueCount; lngResidueIndex++)
            {
                var residue = Residues[lngResidueIndex];

                for (eIonType = 0; eIonType <= ION_TYPE_MAX; eIonType++)
                {
                    if (mFragSpectrumOptions.IonTypeOptions[(int)eIonType].ShowIon)
                    {
                        if ((lngResidueIndex == 1 || lngResidueIndex == ResidueCount) && (eIonType == itIonTypeConstants.itAIon || eIonType == itIonTypeConstants.itBIon || eIonType == itIonTypeConstants.itCIon))
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
                            var strResidues = GetInternalResidues(lngResidueIndex, eIonType, ref blnPhosphorylated);

                            short intChargeIndex;
                            for (intChargeIndex = 1; intChargeIndex <= MAX_CHARGE; intChargeIndex++)
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
                                        if (eIonType == itIonTypeConstants.itYIon || eIonType == itIonTypeConstants.itZIon)
                                        {
                                            strIonSymbol = strIonSymbolGeneric + Strings.Trim(Conversion.Str(ResidueCount - lngResidueIndex + 1));
                                        }
                                        else
                                        {
                                            strIonSymbol = strIonSymbolGeneric + Strings.Trim(Conversion.Str(lngResidueIndex));
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
                                        if (Math.Abs(sngIonShoulderIntensity) > 0f && (eIonType == itIonTypeConstants.itBIon || eIonType == itIonTypeConstants.itYIon || eIonType == itIonTypeConstants.itCIon || eIonType == itIonTypeConstants.itZIon))
                                        {
                                            short intShoulderIndex;
                                            for (intShoulderIndex = -1; intShoulderIndex <= 1; intShoulderIndex += 2)
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

            for (lngIndex = 0; lngIndex < lngIonCount; lngIndex++)
                PointerArray[lngIndex] = lngIndex;

            ShellSortFragSpectrum(ref FragSpectrumWork, ref PointerArray, 0, lngIonCount - 1);

            // Copy the data from FragSpectrumWork() to lstFragSpectraData
            var lstFragSpectraData = new List<udtFragmentationSpectrumDataType>(lngIonCount);

            for (lngIndex = 0; lngIndex <= lngIonCount; lngIndex++)
                lstFragSpectraData.Add(FragSpectrumWork[PointerArray[lngIndex]]);

            return lstFragSpectraData;
        }

        public int GetFragmentationSpectrumRequiredDataPoints()
        {
            // Determines the total number of data points that will be required for a theoretical fragmentation spectrum

            return ResidueCount * ComputeMaxIonsPerResidue();
        }

        public udtFragmentationSpectrumOptionsType GetFragmentationSpectrumOptions()
        {
            try
            {
                return mFragSpectrumOptions;
            }
            catch (Exception ex)
            {
                ElementAndMassRoutines.GeneralErrorHandler("Peptide.GetFragmentationSpectrumOptions", ex);
            }

            var udtDefaultOptions = new udtFragmentationSpectrumOptionsType();
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

        private string GetInternalResidues(int lngCurrentResidueIndex, itIonTypeConstants eIonType)
        {
            bool blnPhosphorylated = false;
            return GetInternalResidues(lngCurrentResidueIndex, eIonType, ref blnPhosphorylated);
        }

        private string GetInternalResidues(int lngCurrentResidueIndex, itIonTypeConstants eIonType, ref bool blnPhosphorylated)
        {
            // Determines the residues preceding or following the given residue (up to and including the current residue)
            // If eIonType is a, b, or c ions, then returns residues from the N terminus
            // If eIonType is y or ions, then returns residues from the C terminus
            // Also, set blnPhosphorylated to true if any of the residues is Ser, Thr, or Tyr and is phosphorylated
            //
            // Note that the residue symbols are separated by a space to avoid accidental matching by the InStr() function

            int lngResidueIndex;

            var strInternalResidues = string.Empty;
            blnPhosphorylated = false;
            if (eIonType == itIonTypeConstants.itYIon || eIonType == itIonTypeConstants.itZIon)
            {
                for (lngResidueIndex = lngCurrentResidueIndex; lngResidueIndex <= ResidueCount; lngResidueIndex++)
                {
                    strInternalResidues = strInternalResidues + Residues[lngResidueIndex].Symbol + " ";
                    if (Residues[lngResidueIndex].Phosphorylated)
                        blnPhosphorylated = true;
                }
            }
            else
            {
                for (lngResidueIndex = 1; lngResidueIndex <= lngCurrentResidueIndex; lngResidueIndex++)
                {
                    strInternalResidues = strInternalResidues + Residues[lngResidueIndex].Symbol + " ";
                    if (Residues[lngResidueIndex].Phosphorylated)
                        blnPhosphorylated = true;
                }
            }

            return strInternalResidues;
        }

        public int GetModificationSymbol(int lngModificationID, ref string strModSymbol, ref double dblModificationMass, ref bool blnIndicatesPhosphorylation, ref string strComment)
        {
            // Returns information on the modification with lngModificationID
            // Returns 0 if success, 1 if failure

            if (lngModificationID >= 1 && lngModificationID <= ModificationSymbolCount)
            {
                var mod = ModificationSymbols[lngModificationID];
                strModSymbol = mod.Symbol;
                dblModificationMass = mod.ModificationMass;
                blnIndicatesPhosphorylation = mod.IndicatesPhosphorylation;
                strComment = mod.Comment;

                return 0;
            }
            else
            {
                strModSymbol = string.Empty;
                dblModificationMass = 0d;
                blnIndicatesPhosphorylation = false;
                strComment = string.Empty;
                return 1;
            }
        }

        public int GetModificationSymbolCount()
        {
            // Returns the number of modifications defined

            return ModificationSymbolCount;
        }

        public int GetModificationSymbolID(string strModSymbol)
        {
            // Returns the ID for a given modification
            // Returns 0 if not found, the ID if found

            int intIndex;
            var lngModificationIDMatch = default(int);

            for (intIndex = 1; intIndex <= ModificationSymbolCount; intIndex++)
            {
                if ((ModificationSymbols[intIndex].Symbol ?? "") == (strModSymbol ?? ""))
                {
                    lngModificationIDMatch = intIndex;
                    break;
                }
            }

            return lngModificationIDMatch;
        }

        public int GetResidue(int lngResidueNumber, ref string strSymbol, ref double dblMass, ref bool blnIsModified, ref short intModificationCount)
        {
            // Returns 0 if success, 1 if failure
            if (lngResidueNumber >= 1 && lngResidueNumber <= ResidueCount)
            {
                var residue = Residues[lngResidueNumber];
                strSymbol = residue.Symbol;
                dblMass = residue.Mass;
                blnIsModified = residue.ModificationIDCount > 0;
                intModificationCount = residue.ModificationIDCount;

                return 0;
            }
            else
            {
                return 1;
            }
        }

        public int GetResidueCount()
        {
            return ResidueCount;
        }

        public int GetResidueCountSpecificResidue(string strResidueSymbol, bool blnUse3LetterCode)
        {
            // Returns the number of occurrences of the given residue in the loaded sequence

            string strSearchResidue3Letter;
            int lngResidueIndex;

            if (blnUse3LetterCode)
            {
                strSearchResidue3Letter = strResidueSymbol;
            }
            else
            {
                strSearchResidue3Letter = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strResidueSymbol, true);
            }

            var lngResidueCount = 0;
            for (lngResidueIndex = 0; lngResidueIndex < ResidueCount; lngResidueIndex++)
            {
                if ((Residues[lngResidueIndex].Symbol ?? "") == (strSearchResidue3Letter ?? ""))
                {
                    lngResidueCount += 1;
                }
            }

            return lngResidueCount;
        }

        public int GetResidueModificationIDs(int lngResidueNumber, ref int[] lngModificationIDsOneBased)
        {
            // Returns the number of Modifications
            // ReDims lngModificationIDsOneBased() to hold the values

            if (lngResidueNumber >= 1 && lngResidueNumber <= ResidueCount)
            {
                var residue = Residues[lngResidueNumber];

                // Need to use this in case the calling program is sending an array with fixed dimensions
                try
                {
                    lngModificationIDsOneBased = new int[(residue.ModificationIDCount + 1)];
                }
                catch
                {
                    // Ignore errors
                }

                for (var intIndex = 1; intIndex <= residue.ModificationIDCount; intIndex++)
                    lngModificationIDsOneBased[intIndex] = residue.ModificationIDs[intIndex];

                return residue.ModificationIDCount;
            }
            else
            {
                return 0;
            }
        }

        public string GetResidueSymbolOnly(int lngResidueNumber, bool blnUse3LetterCode)
        {
            // Returns the symbol at the given residue number, or string.empty if an invalid residue number

            string strSymbol;

            if (lngResidueNumber >= 1 && lngResidueNumber <= ResidueCount)
            {
                strSymbol = Residues[lngResidueNumber].Symbol;

                if (!blnUse3LetterCode)
                    strSymbol = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbol, false);
            }
            else
            {
                strSymbol = string.Empty;
            }

            return strSymbol;
        }

        public string GetSequence()
        {
            return GetSequence(blnUse3LetterCode: true, blnAddSpaceEvery10Residues: false, blnSeparateResiduesWithDash: false, blnIncludeNAndCTerminii: false, blnIncludeModificationSymbols: true);
        }

        public string GetSequence1LetterCode()
        {
            return GetSequence(blnUse3LetterCode: false, blnAddSpaceEvery10Residues: false, blnSeparateResiduesWithDash: false, blnIncludeNAndCTerminii: false, blnIncludeModificationSymbols: true);
        }

        public string GetSequence(bool blnUse3LetterCode)
        {
            return GetSequence(blnUse3LetterCode, blnAddSpaceEvery10Residues: false, blnSeparateResiduesWithDash: false, blnIncludeNAndCTerminii: false, blnIncludeModificationSymbols: true);
        }

        public string GetSequence(bool blnUse3LetterCode,
            bool blnAddSpaceEvery10Residues)
        {
            return GetSequence(blnUse3LetterCode, blnAddSpaceEvery10Residues, blnSeparateResiduesWithDash: false, blnIncludeNAndCTerminii: false, blnIncludeModificationSymbols: true);
        }

        public string GetSequence(bool blnUse3LetterCode,
            bool blnAddSpaceEvery10Residues,
            bool blnSeparateResiduesWithDash)
        {
            return GetSequence(blnUse3LetterCode, blnAddSpaceEvery10Residues, blnSeparateResiduesWithDash, blnIncludeNAndCTerminii: false, blnIncludeModificationSymbols: true);
        }

        public string GetSequence(bool blnUse3LetterCode,
            bool blnAddSpaceEvery10Residues,
            bool blnSeparateResiduesWithDash,
            bool blnIncludeNAndCTerminii)
        {
            return GetSequence(blnUse3LetterCode, blnAddSpaceEvery10Residues, blnSeparateResiduesWithDash, blnIncludeNAndCTerminii, blnIncludeModificationSymbols: true);
        }

        public string GetSequence(bool blnUse3LetterCode,
            bool blnAddSpaceEvery10Residues,
            bool blnSeparateResiduesWithDash,
            bool blnIncludeNAndCTerminii,
            bool blnIncludeModificationSymbols)
        {
            // Construct a text sequence using Residues() and the N and C Terminus info

            string strDashAdd;
            string strModSymbol = string.Empty;
            string strModSymbolComment = string.Empty;
            var blnIndicatesPhosphorylation = default(bool);
            var dblModMass = default(double);
            int lngIndex;

            if (blnSeparateResiduesWithDash)
                strDashAdd = "-";
            else
                strDashAdd = string.Empty;

            var strSequence = string.Empty;
            for (lngIndex = 1; lngIndex <= ResidueCount; lngIndex++)
            {
                var residue = Residues[lngIndex];
                var strSymbol3Letter = residue.Symbol;
                if (blnUse3LetterCode)
                {
                    strSequence += strSymbol3Letter;
                }
                else
                {
                    var strSymbol1Letter = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbol3Letter, false);
                    if ((strSymbol1Letter ?? "") == (string.Empty ?? ""))
                        strSymbol1Letter = UNKNOWN_SYMBOL_ONE_LETTER;
                    strSequence += strSymbol1Letter;
                }

                if (blnIncludeModificationSymbols)
                {
                    short intModIndex;
                    for (intModIndex = 1; intModIndex <= residue.ModificationIDCount; intModIndex++)
                    {
                        var lngError = GetModificationSymbol(residue.ModificationIDs[intModIndex], ref strModSymbol, ref dblModMass, ref blnIndicatesPhosphorylation, ref strModSymbolComment);
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
                    if (blnAddSpaceEvery10Residues)
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

            if (blnIncludeNAndCTerminii)
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

        public string GetTrypticName(string strProteinResidues, string strPeptideResidues)
        {
            return GetTrypticName(strProteinResidues, strPeptideResidues, out _, out _, false,
                                  TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true, 1);
        }

        public string GetTrypticName(string strProteinResidues, string strPeptideResidues,
            int lngProteinSearchStartLoc)
        {
            return GetTrypticName(strProteinResidues, strPeptideResidues, out _, out _, false,
                                  TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true, lngProteinSearchStartLoc);
        }

        public string GetTrypticName(string strProteinResidues, string strPeptideResidues,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd)
        {
            return GetTrypticName(strProteinResidues, strPeptideResidues, out lngReturnResidueStart, out lngReturnResidueEnd, false,
                                  TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true, 1);
        }

        public string GetTrypticName(string strProteinResidues, string strPeptideResidues,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd,
            bool blnICR2LSCompatible)
        {
            return GetTrypticName(strProteinResidues, strPeptideResidues, out lngReturnResidueStart, out lngReturnResidueEnd, blnICR2LSCompatible,
                                  TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true, 1);
        }

        public string GetTrypticName(string strProteinResidues, string strPeptideResidues,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd,
            bool blnICR2LSCompatible,
            string strRuleResidues,
            string strExceptionResidues,
            string strTerminiiSymbol)
        {
            return GetTrypticName(strProteinResidues, strPeptideResidues, out lngReturnResidueStart, out lngReturnResidueEnd, blnICR2LSCompatible,
                                  strRuleResidues, strExceptionResidues, strTerminiiSymbol, true, 1);
        }

        /// <summary>
        /// Examines strPeptideResidues to see where they exist in strProteinResidues
        /// Constructs a name string based on their position and based on whether the fragment is truly tryptic
        /// In addition, returns the position of the first and last residue in lngReturnResidueStart and lngReturnResidueEnd
        /// </summary>
        /// <param name="strProteinResidues"></param>
        /// <param name="strPeptideResidues"></param>
        /// <param name="lngReturnResidueStart">Output: start peptides of the peptide residues in the protein</param>
        /// <param name="lngReturnResidueEnd">Output: end peptides of the peptide residues in the protein</param>
        /// <param name="blnICR2LSCompatible"></param>
        /// <param name="strRuleResidues"></param>
        /// <param name="strExceptionResidues"></param>
        /// <param name="strTerminiiSymbol"></param>
        /// <param name="blnIgnoreCase"></param>
        /// <param name="lngProteinSearchStartLoc"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetTrypticName(string strProteinResidues, string strPeptideResidues,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd,
            bool blnICR2LSCompatible,
            string strRuleResidues,
            string strExceptionResidues,
            string strTerminiiSymbol,
            bool blnIgnoreCase,
            int lngProteinSearchStartLoc)
        {
            // The tryptic name in the following format
            // t1  indicates tryptic peptide 1
            // t2 represents tryptic peptide 2, etc.
            // t1.2  indicates tryptic peptide 1, plus one more tryptic peptide, i.e. t1 and t2
            // t5.2  indicates tryptic peptide 5, plus one more tryptic peptide, i.e. t5 and t6
            // t5.3  indicates tryptic peptide 5, plus two more tryptic peptides, i.e. t5, t6, and t7
            // 40.52  means that the residues are not tryptic, and simply range from residue 40 to 52
            // If the peptide residues are not present in strProteinResidues, then returns ""
            // Since a peptide can occur multiple times in a protein, one can set lngProteinSearchStartLoc to a value larger than 1 to ignore previous hits

            // If blnICR2LSCompatible is True, then the values returned when a peptide is not tryptic are modified to
            // range from the starting residue, to the ending residue +1
            // lngReturnResidueEnd is always equal to the position of the final residue, regardless of blnICR2LSCompatible

            // For example, if strProteinResidues = "IGKANR"
            // Then when strPeptideResidues = "IGK", the TrypticName is t1
            // Then when strPeptideResidues = "ANR", the TrypticName is t2
            // Then when strPeptideResidues = "IGKANR", the TrypticName is t1.2
            // Then when strPeptideResidues = "IG", the TrypticName is 1.2
            // Then when strPeptideResidues = "KANR", the TrypticName is 3.6
            // Then when strPeptideResidues = "NR", the TrypticName is 5.6

            // However, if blnICR2LSCompatible = True, then the last three are changed to:
            // Then when strPeptideResidues = "IG", the TrypticName is 1.3
            // Then when strPeptideResidues = "KANR", the TrypticName is 3.7
            // Then when strPeptideResidues = "NR", the TrypticName is 5.7

            int intStartLoc;

            if (blnIgnoreCase)
            {
                strProteinResidues = Strings.UCase(strProteinResidues);
                strPeptideResidues = Strings.UCase(strPeptideResidues);
            }

            if (lngProteinSearchStartLoc <= 1)
            {
                intStartLoc = Strings.InStr(strProteinResidues, strPeptideResidues);
            }
            else
            {
                intStartLoc = Strings.InStr(Strings.Mid(strProteinResidues, lngProteinSearchStartLoc), strPeptideResidues);
                if (intStartLoc > 0)
                {
                    intStartLoc = intStartLoc + lngProteinSearchStartLoc - 1;
                }
            }

            var lngPeptideResiduesLength = Strings.Len(strPeptideResidues);

            if (intStartLoc > 0 && Strings.Len(strProteinResidues) > 0 && lngPeptideResiduesLength > 0)
            {
                var intEndLoc = intStartLoc + lngPeptideResiduesLength - 1;

                // Determine if the residue is tryptic
                // Use CheckSequenceAgainstCleavageRule() for this
                string strPrefix;
                if (intStartLoc > 1)
                {
                    strPrefix = Strings.Mid(strProteinResidues, intStartLoc - 1, 1);
                }
                else
                {
                    strPrefix = strTerminiiSymbol;
                }

                string strSuffix;
                if (intEndLoc == Strings.Len(strProteinResidues))
                {
                    strSuffix = strTerminiiSymbol;
                }
                else
                {
                    strSuffix = Strings.Mid(strProteinResidues, intEndLoc + 1, 1);
                }

                var blnMatchesCleavageRule = CheckSequenceAgainstCleavageRule(strPrefix + "." + strPeptideResidues + "." + strSuffix,
                    strRuleResidues,
                    strExceptionResidues,
                    false,
                    ".",
                    strTerminiiSymbol,
                    blnIgnoreCase);

                string strTrypticName;
                if (blnMatchesCleavageRule)
                {
                    // Construct strTrypticName

                    // Determine which tryptic residue strPeptideResidues is
                    short intTrypticResidueNumber;
                    int lngRuleResidueLoc;
                    if (intStartLoc == 1)
                    {
                        intTrypticResidueNumber = 1;
                    }
                    else
                    {
                        var strProteinResiduesBeforeStartLoc = Strings.Left(strProteinResidues, intStartLoc - 1);
                        var strResidueFollowingSearchResidues = Strings.Left(strPeptideResidues, 1);
                        intTrypticResidueNumber = 0;
                        lngRuleResidueLoc = 0;
                        do
                        {
                            lngRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResiduesBeforeStartLoc, strResidueFollowingSearchResidues, lngRuleResidueLoc + 1, strRuleResidues, strExceptionResidues, strTerminiiSymbol);
                            if (lngRuleResidueLoc > 0)
                            {
                                intTrypticResidueNumber = (short)(intTrypticResidueNumber + 1);
                            }
                        }
                        while (lngRuleResidueLoc > 0 && lngRuleResidueLoc + 1 < intStartLoc);
                        intTrypticResidueNumber = (short)(intTrypticResidueNumber + 1);
                    }

                    // Determine number of K or R residues in strPeptideResidues
                    // Ignore K or R residues followed by Proline
                    short intRuleResidueMatchCount = 0;
                    lngRuleResidueLoc = 0;
                    do
                    {
                        lngRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strPeptideResidues, strSuffix, lngRuleResidueLoc + 1, strRuleResidues, strExceptionResidues, strTerminiiSymbol);
                        if (lngRuleResidueLoc > 0)
                        {
                            intRuleResidueMatchCount = (short)(intRuleResidueMatchCount + 1);
                        }
                    }
                    while (lngRuleResidueLoc > 0 && lngRuleResidueLoc < lngPeptideResiduesLength);

                    strTrypticName = "t" + Strings.Trim(Conversion.Str(intTrypticResidueNumber));
                    if (intRuleResidueMatchCount > 1)
                    {
                        strTrypticName = strTrypticName + "." + Strings.Trim(Conversion.Str(intRuleResidueMatchCount));
                    }
                }
                else if (blnICR2LSCompatible)
                {
                    strTrypticName = Strings.Trim(Conversion.Str(intStartLoc)) + "." + Strings.Trim(Conversion.Str(intEndLoc + 1));
                }
                else
                {
                    strTrypticName = Strings.Trim(Conversion.Str(intStartLoc)) + "." + Strings.Trim(Conversion.Str(intEndLoc));
                }

                lngReturnResidueStart = intStartLoc;
                lngReturnResidueEnd = intEndLoc;
                return strTrypticName;
            }
            else
            {
                // Residues not found
                lngReturnResidueStart = 0;
                lngReturnResidueEnd = 0;
                return string.Empty;
            }
        }

        public string GetTrypticNameMultipleMatches(string strProteinResidues,
            string strPeptideResidues)
        {
            return GetTrypticNameMultipleMatches(strProteinResidues, strPeptideResidues,
                                                 out _, out _, out _, false,
                                                 TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true, 1, ", ");
        }

        public string GetTrypticNameMultipleMatches(string strProteinResidues,
            string strPeptideResidues,
            int lngProteinSearchStartLoc)
        {
            return GetTrypticNameMultipleMatches(strProteinResidues, strPeptideResidues,
                                                 out _, out _, out _, false,
                                                 TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true,
                                                 lngProteinSearchStartLoc, ", ");
        }

        public string GetTrypticNameMultipleMatches(string strProteinResidues,
            string strPeptideResidues,
            int lngProteinSearchStartLoc,
            string strListDelimiter)
        {
            return GetTrypticNameMultipleMatches(strProteinResidues, strPeptideResidues,
                                                 out _, out _, out _, false,
                                                 TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true,
                                                 lngProteinSearchStartLoc, strListDelimiter);
        }

        public string GetTrypticNameMultipleMatches(string strProteinResidues,
            string strPeptideResidues,
            out int lngReturnMatchCount,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd)
        {
            return GetTrypticNameMultipleMatches(strProteinResidues, strPeptideResidues,
                                                 out lngReturnMatchCount, out lngReturnResidueStart, out lngReturnResidueEnd, false,
                                                 TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true, 1, ", ");
        }

        public string GetTrypticNameMultipleMatches(string strProteinResidues,
            string strPeptideResidues,
            out int lngReturnMatchCount,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd,
            bool blnICR2LSCompatible)
        {
            return GetTrypticNameMultipleMatches(strProteinResidues, strPeptideResidues,
                                                 out lngReturnMatchCount, out lngReturnResidueStart, out lngReturnResidueEnd, blnICR2LSCompatible,
                                                 TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true, 1, ", ");
        }

        public string GetTrypticNameMultipleMatches(string strProteinResidues,
            string strPeptideResidues,
            out int lngReturnMatchCount,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd,
            bool blnICR2LSCompatible,
            string strRuleResidues,
            string strExceptionResidues,
            string strTerminiiSymbol)
        {
            return GetTrypticNameMultipleMatches(strProteinResidues, strPeptideResidues,
                                                 out lngReturnMatchCount, out lngReturnResidueStart, out lngReturnResidueEnd, blnICR2LSCompatible,
                                                 strRuleResidues, strExceptionResidues, strTerminiiSymbol, true, 1, ", ");
        }

        /// <summary>
        /// Examines strPeptideResidues to see where they exist in strProteinResidues
        /// Looks for all possible matches, returning them as a comma separated list
        /// </summary>
        /// <param name="strProteinResidues"></param>
        /// <param name="strPeptideResidues"></param>
        /// <param name="lngReturnMatchCount"></param>
        /// <param name="lngReturnResidueStart"></param>
        /// <param name="lngReturnResidueEnd"></param>
        /// <param name="blnICR2LSCompatible"></param>
        /// <param name="strRuleResidues"></param>
        /// <param name="strExceptionResidues"></param>
        /// <param name="strTerminiiSymbol"></param>
        /// <param name="blnIgnoreCase"></param>
        /// <param name="lngProteinSearchStartLoc"></param>
        /// <param name="strListDelimiter"></param>
        /// <returns>The number of matches</returns>
        /// <remarks></remarks>
        public string GetTrypticNameMultipleMatches(string strProteinResidues,
            string strPeptideResidues,
            out int lngReturnMatchCount,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd,
            bool blnICR2LSCompatible,
            string strRuleResidues,
            string strExceptionResidues,
            string strTerminiiSymbol,
            bool blnIgnoreCase,
            int lngProteinSearchStartLoc,
            string strListDelimiter)
        {
            // Returns the number of matches in lngReturnMatchCount
            // lngReturnResidueStart contains the residue number of the start of the first match
            // lngReturnResidueEnd contains the residue number of the end of the last match

            // See GetTrypticName for additional information

            var lngCurrentSearchLoc = lngProteinSearchStartLoc;
            lngReturnMatchCount = 0;
            lngReturnResidueStart = 0;
            lngReturnResidueEnd = 0;
            var strNameList = string.Empty;

            do
            {
                var strCurrentName = GetTrypticName(strProteinResidues, strPeptideResidues, out var lngCurrentResidueStart, out var lngCurrentResidueEnd, blnICR2LSCompatible, strRuleResidues, strExceptionResidues, strTerminiiSymbol, blnIgnoreCase, lngCurrentSearchLoc);

                if (Strings.Len(strCurrentName) > 0)
                {
                    if (strNameList.Length > 0)
                    {
                        strNameList += strListDelimiter;
                    }

                    strNameList += strCurrentName;
                    lngCurrentSearchLoc = lngCurrentResidueEnd + 1;
                    lngReturnMatchCount += 1;

                    if (lngReturnMatchCount == 1)
                    {
                        lngReturnResidueStart = lngCurrentResidueStart;
                    }

                    lngReturnResidueEnd = lngCurrentResidueEnd;

                    if (lngCurrentSearchLoc > Strings.Len(strProteinResidues))
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

        private int GetTrypticNameFindNextCleavageLoc(string strSearchResidues, string strResidueFollowingSearchResidues,
            int lngStartChar,
            string strSearchChars = TRYPTIC_RULE_RESIDUES,
            string strExceptionSuffixResidues = TRYPTIC_EXCEPTION_RESIDUES,
            string strTerminiiSymbol = TERMINII_SYMBOL)
        {
            // Finds the location of the next strSearchChar in strSearchResidues (K or R by default)
            // Assumes strSearchResidues are already upper case
            // Examines the residue following the matched residue
            // If it matches one of the characters in strExceptionSuffixResidues, then the match is not counted
            // Note that strResidueFollowingSearchResidues is necessary in case the potential cleavage residue is the final residue in strSearchResidues
            // We need to know the next residue to determine if it matches an exception residue

            // ReSharper disable CommentTypo

            // For example, if strSearchResidues =      "IGASGEHIFIIGVDKPNR"
            // and the protein it is part of is: TNSANFRIGASGEHIFIIGVDKPNRQPDS
            // and strSearchChars = "KR while strExceptionSuffixResidues  = "P"
            // Then the K in IGASGEHIFIIGVDKPNR is ignored because the following residue is P,
            // while the R in IGASGEHIFIIGVDKPNR is OK because strResidueFollowingSearchResidues is Q

            // ReSharper restore CommentTypo

            // It is the calling function's responsibility to assign the correct residue to strResidueFollowingSearchResidues
            // If no match is found, but strResidueFollowingSearchResidues is "-", then the cleavage location returned is Len(strSearchResidues) + 1

            short intCharLocInSearchChars;

            var intExceptionSuffixResidueCount = (short)Strings.Len(strExceptionSuffixResidues);

            var lngMinCharLoc = -1;
            for (intCharLocInSearchChars = 0; intCharLocInSearchChars < strSearchChars.Length; intCharLocInSearchChars++)
            {
                var lngCharLoc = Strings.InStr(Strings.Mid(strSearchResidues, lngStartChar), Strings.Mid(strSearchChars, intCharLocInSearchChars, 1));

                if (lngCharLoc > 0)
                {
                    lngCharLoc = lngCharLoc + lngStartChar - 1;

                    if (intExceptionSuffixResidueCount > 0)
                    {
                        // Make sure strSuffixResidue does not match strExceptionSuffixResidues
                        int lngExceptionCharLocInSearchResidues;
                        string strResidueFollowingCleavageResidue;
                        if (lngCharLoc < Strings.Len(strSearchResidues))
                        {
                            lngExceptionCharLocInSearchResidues = lngCharLoc + 1;
                            strResidueFollowingCleavageResidue = Strings.Mid(strSearchResidues, lngExceptionCharLocInSearchResidues, 1);
                        }
                        else
                        {
                            // Matched the last residue in strSearchResidues
                            lngExceptionCharLocInSearchResidues = Strings.Len(strSearchResidues) + 1;
                            strResidueFollowingCleavageResidue = strResidueFollowingSearchResidues;
                        }

                        short intCharLocInExceptionChars;
                        for (intCharLocInExceptionChars = 1; intCharLocInExceptionChars <= intExceptionSuffixResidueCount; intCharLocInExceptionChars++)
                        {
                            if ((strResidueFollowingCleavageResidue ?? "") == (Strings.Mid(strExceptionSuffixResidues, intCharLocInExceptionChars, 1) ?? ""))
                            {
                                // Exception char is the following character; can't count this as the cleavage point

                                if (lngExceptionCharLocInSearchResidues < Strings.Len(strSearchResidues))
                                {
                                    // Recursively call this function to find the next cleavage position, using an updated lngStartChar position
                                    var lngCharLocViaRecursiveSearch = GetTrypticNameFindNextCleavageLoc(strSearchResidues, strResidueFollowingSearchResidues, lngExceptionCharLocInSearchResidues, strSearchChars, strExceptionSuffixResidues, strTerminiiSymbol);

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

            if (lngMinCharLoc < 0 && (strResidueFollowingSearchResidues ?? "") == (strTerminiiSymbol ?? ""))
            {
                lngMinCharLoc = strSearchResidues.Length + 1;
            }

            if (lngMinCharLoc < 0)
            {
                return 0;
            }
            else
            {
                return lngMinCharLoc;
            }
        }

        public string GetTrypticPeptideNext(string strProteinResidues,
            int lngSearchStartLoc)
        {
            return GetTrypticPeptideNext(strProteinResidues, lngSearchStartLoc, out _, out _, TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL);
        }

        public string GetTrypticPeptideNext(string strProteinResidues,
            int lngSearchStartLoc,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd,
            string strRuleResidues,
            string strExceptionResidues,
            string strTerminiiSymbol)
        {
            // Returns the next tryptic peptide in strProteinResidues, starting the search as lngSearchStartLoc
            // Useful when obtaining all of the tryptic peptides for a protein, since this function will operate
            // much faster than repeatedly calling GetTrypticPeptideByFragmentNumber()

            // Returns the position of the start and end residues using lngReturnResidueStart and lngReturnResidueEnd

            lngReturnResidueStart = 1;
            lngReturnResidueEnd = 1;

            if (lngSearchStartLoc < 1)
                lngSearchStartLoc = 1;

            var lngProteinResiduesLength = Strings.Len(strProteinResidues);
            if (lngSearchStartLoc > lngProteinResiduesLength)
            {
                return string.Empty;
            }

            var lngRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResidues, strTerminiiSymbol, lngSearchStartLoc, strRuleResidues, strExceptionResidues, strTerminiiSymbol);
            if (lngRuleResidueLoc > 0)
            {
                lngReturnResidueStart = lngSearchStartLoc;
                if (lngRuleResidueLoc > lngProteinResiduesLength)
                {
                    lngReturnResidueEnd = lngProteinResiduesLength;
                }
                else
                {
                    lngReturnResidueEnd = lngRuleResidueLoc;
                }

                return Strings.Mid(strProteinResidues, lngReturnResidueStart, lngReturnResidueEnd - lngReturnResidueStart + 1);
            }
            else
            {
                lngReturnResidueStart = 1;
                lngReturnResidueEnd = lngProteinResiduesLength;
                return strProteinResidues;
            }
        }

        public string GetTrypticPeptideByFragmentNumber(string strProteinResidues,
            short intDesiredPeptideNumber)
        {
            return GetTrypticPeptideByFragmentNumber(strProteinResidues, intDesiredPeptideNumber, out _, out _,
                                                     TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true);
        }

        public string GetTrypticPeptideByFragmentNumber(string strProteinResidues,
            short intDesiredPeptideNumber,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd)
        {
            return GetTrypticPeptideByFragmentNumber(strProteinResidues, intDesiredPeptideNumber,
                                                     out lngReturnResidueStart, out lngReturnResidueEnd,
                                                     TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, TERMINII_SYMBOL, true);
        }

        public string GetTrypticPeptideByFragmentNumber(string strProteinResidues,
            short intDesiredPeptideNumber,
            out int lngReturnResidueStart,
            out int lngReturnResidueEnd,
            string strRuleResidues,
            string strExceptionResidues,
            string strTerminiiSymbol,
            bool blnIgnoreCase)
        {
            // Returns the desired tryptic peptide from strProteinResidues

            // ReSharper disable CommentTypo

            // For example, if strProteinResidues = "IGKANRMTFGL" then
            // when intDesiredPeptideNumber = 1, returns "IGK"
            // when intDesiredPeptideNumber = 2, returns "ANR"
            // when intDesiredPeptideNumber = 3, returns "MTFGL"

            // ReSharper enable CommentTypo

            // Optionally, returns the position of the start and end residues
            // using lngReturnResidueStart and lngReturnResidueEnd

            int lngRuleResidueLoc;
            var lngPrevStartLoc = default(int);

            string strMatchingFragment;

            lngReturnResidueStart = 1;
            lngReturnResidueEnd = 1;

            if (intDesiredPeptideNumber < 1)
            {
                return string.Empty;
            }

            if (blnIgnoreCase)
            {
                strProteinResidues = Strings.UCase(strProteinResidues);
            }

            var lngProteinResiduesLength = Strings.Len(strProteinResidues);

            var lngStartLoc = 1;
            short intCurrentTrypticPeptideNumber = 0;
            do
            {
                lngRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResidues, strTerminiiSymbol, lngStartLoc, strRuleResidues, strExceptionResidues, strTerminiiSymbol);
                if (lngRuleResidueLoc > 0)
                {
                    intCurrentTrypticPeptideNumber = (short)(intCurrentTrypticPeptideNumber + 1);
                    lngPrevStartLoc = lngStartLoc;
                    lngStartLoc = lngRuleResidueLoc + 1;

                    if (lngPrevStartLoc > lngProteinResiduesLength)
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
            while (intCurrentTrypticPeptideNumber < intDesiredPeptideNumber);

            if (intCurrentTrypticPeptideNumber > 0 && lngPrevStartLoc > 0)
            {
                if (lngPrevStartLoc > Strings.Len(strProteinResidues))
                {
                    // User requested a peptide number that is too high
                    lngReturnResidueStart = 0;
                    lngReturnResidueEnd = 0;
                    strMatchingFragment = string.Empty;
                }
                else
                {
                    // Match found, find the extent of this peptide
                    lngReturnResidueStart = lngPrevStartLoc;
                    if (lngRuleResidueLoc > lngProteinResiduesLength)
                    {
                        lngReturnResidueEnd = lngProteinResiduesLength;
                    }
                    else
                    {
                        lngReturnResidueEnd = lngRuleResidueLoc;
                    }

                    strMatchingFragment = Strings.Mid(strProteinResidues, lngPrevStartLoc, lngRuleResidueLoc - lngPrevStartLoc + 1);
                }
            }
            else
            {
                lngReturnResidueStart = 1;
                lngReturnResidueEnd = lngProteinResiduesLength;
                strMatchingFragment = strProteinResidues;
            }

            return strMatchingFragment;
        }

        public bool CheckSequenceAgainstCleavageRule(string strSequence,
            string strRuleResidues,
            string strExceptionSuffixResidues,
            bool blnAllowPartialCleavage)
        {
            return CheckSequenceAgainstCleavageRule(strSequence, strRuleResidues, strExceptionSuffixResidues,
                                                    blnAllowPartialCleavage, ".", TERMINII_SYMBOL, true, out _);
        }

        public bool CheckSequenceAgainstCleavageRule(string strSequence,
            string strRuleResidues,
            string strExceptionSuffixResidues,
            bool blnAllowPartialCleavage,
            out short intRuleMatchCount)
        {
            return CheckSequenceAgainstCleavageRule(strSequence, strRuleResidues, strExceptionSuffixResidues,
                                                    blnAllowPartialCleavage, ".", TERMINII_SYMBOL, true, out intRuleMatchCount);
        }

        public bool CheckSequenceAgainstCleavageRule(string strSequence,
            string strRuleResidues,
            string strExceptionSuffixResidues,
            bool blnAllowPartialCleavage,
            string strSeparationChar,
            string strTerminiiSymbol,
            bool blnIgnoreCase)
        {
            return CheckSequenceAgainstCleavageRule(strSequence, strRuleResidues, strExceptionSuffixResidues,
                                                    blnAllowPartialCleavage, strSeparationChar, strTerminiiSymbol, blnIgnoreCase, out _);
        }

        public bool CheckSequenceAgainstCleavageRule(string strSequence,
            string strRuleResidues,
            string strExceptionSuffixResidues,
            bool blnAllowPartialCleavage,
            string strSeparationChar,
            string strTerminiiSymbol,
            bool blnIgnoreCase,
            out short intRuleMatchCount)
        {
            // Checks strSequence to see if it matches the cleavage rule
            // Returns True if valid, False if invalid
            // Returns True if doesn't contain any periods, and thus, can't be examined
            // The ByRef variable intRuleMatchCount can be used to retrieve the number of ends that matched the rule (0, 1, or 2); terminii are counted as rule matches

            // The residues in strRuleResidues specify the cleavage rule
            // The peptide must end in one of the residues, or in -
            // The preceding residue must be one of the residues or be -
            // EXCEPTION: if blnAllowPartialCleavage = True then the rules need only apply to one end
            // Finally, the suffix residue cannot match any of the residues in strExceptionSuffixResidues

            // For example, if strRuleResidues = "KR" and strExceptionSuffixResidues = "P"
            // Then if strSequence = "R.AEQDDLANYGPGNGVLPSAGSSISMEK.L" then blnMatchesCleavageRule = True
            // However, if strSequence = "R.IGASGEHIFIIGVDK.P" then blnMatchesCleavageRule = False since strSuffix = "P"
            // Finally, if strSequence = "R.IGASGEHIFIIGVDKPNR.Q" then blnMatchesCleavageRule = True since K is ignored, but the final R.Q is valid

            string strSequenceStart, strSequenceEnd;
            bool blnMatchesCleavageRule = default;

            // Need to reset this to zero since passed ByRef
            intRuleMatchCount = 0;
            var strPrefix = string.Empty;
            var strSuffix = string.Empty;

            // First, make sure the sequence is in the form A.BCDEFG.H or A.BCDEFG or BCDEFG.H
            // If it isn't, then we can't check it (we'll return true)

            if (string.IsNullOrEmpty(strRuleResidues))
            {
                // No rule residues
                return true;
            }

            if (strSeparationChar == null)
                strSeparationChar = ".";

            if (!strSequence.Contains(strSeparationChar))
            {
                // No periods, can't check
                Console.WriteLine("Warning: strSequence does not contain " + strSeparationChar + "; unable to determine cleavage state");
                return true;
            }

            if (blnIgnoreCase)
            {
                strSequence = Strings.UCase(strSequence);
            }

            // Find the prefix residue and starting residue
            if ((Strings.Mid(strSequence, 2, 1) ?? "") == (strSeparationChar ?? ""))
            {
                strPrefix = Strings.Left(strSequence, 1);
                strSequenceStart = Strings.Mid(strSequence, 3, 1);
            }
            else
            {
                strSequenceStart = Strings.Left(strSequence, 1);
            }

            // Find the suffix residue and the ending residue
            if ((Strings.Mid(strSequence, Strings.Len(strSequence) - 1, 1) ?? "") == (strSeparationChar ?? ""))
            {
                strSuffix = Strings.Right(strSequence, 1);
                strSequenceEnd = Strings.Mid(strSequence, Strings.Len(strSequence) - 2, 1);
            }
            else
            {
                strSequenceEnd = Strings.Right(strSequence, 1);
            }

            if ((strRuleResidues ?? "") == (strTerminiiSymbol ?? ""))
            {
                // Peptide database rules
                // See if prefix and suffix are "" or are strTerminiiSymbol
                if ((strPrefix ?? "") == (strTerminiiSymbol ?? "") && (strSuffix ?? "") == (strTerminiiSymbol ?? "") ||
                    (strPrefix ?? "") == (string.Empty ?? "") && (strSuffix ?? "") == (string.Empty ?? ""))
                {
                    intRuleMatchCount = 2;
                    blnMatchesCleavageRule = true;
                }
                else
                {
                    blnMatchesCleavageRule = false;
                }
            }
            else
            {
                if (blnIgnoreCase)
                {
                    strRuleResidues = Strings.UCase(strRuleResidues);
                }

                // Test each character in strRuleResidues against both strPrefix and strSequenceEnd
                // Make sure strSuffix does not match strExceptionSuffixResidues
                short intEndToCheck;
                for (intEndToCheck = 0; intEndToCheck <= 1; intEndToCheck++)
                {
                    var blnSkipThisEnd = false;
                    string strTestResidue;
                    if (intEndToCheck == 0)
                    {
                        strTestResidue = strPrefix;
                        if ((strPrefix ?? "") == (strTerminiiSymbol ?? ""))
                        {
                            intRuleMatchCount = (short)(intRuleMatchCount + 1);
                            blnSkipThisEnd = true;
                        }
                        // See if strSequenceStart matches one of the exception residues
                        // If it does, make sure strPrefix does not match one of the rule residues
                        else if (CheckSequenceAgainstCleavageRuleMatchTestResidue(strSequenceStart, strExceptionSuffixResidues))
                        {
                            // Match found
                            // Make sure strPrefix does not match one of the rule residues
                            if (CheckSequenceAgainstCleavageRuleMatchTestResidue(strPrefix, strRuleResidues))
                            {
                                // Match found; thus does not match cleavage rule
                                blnSkipThisEnd = true;
                            }
                        }
                    }
                    else
                    {
                        strTestResidue = strSequenceEnd;
                        if ((strSuffix ?? "") == (strTerminiiSymbol ?? ""))
                        {
                            intRuleMatchCount = (short)(intRuleMatchCount + 1);
                            blnSkipThisEnd = true;
                        }
                        // Make sure strSuffix does not match strExceptionSuffixResidues
                        else if (CheckSequenceAgainstCleavageRuleMatchTestResidue(strSuffix, strExceptionSuffixResidues))
                        {
                            // Match found; thus does not match cleavage rule
                            blnSkipThisEnd = true;
                        }
                    }

                    if (!blnSkipThisEnd)
                    {
                        if (CheckSequenceAgainstCleavageRuleMatchTestResidue(strTestResidue, strRuleResidues))
                        {
                            intRuleMatchCount = (short)(intRuleMatchCount + 1);
                        }
                    }
                }

                if (intRuleMatchCount == 2)
                {
                    blnMatchesCleavageRule = true;
                }
                else if (intRuleMatchCount >= 1 && blnAllowPartialCleavage)
                {
                    blnMatchesCleavageRule = true;
                }
            }

            return blnMatchesCleavageRule;
        }

        private bool CheckSequenceAgainstCleavageRuleMatchTestResidue(string strTestResidue, string strRuleResidues)
        {
            // Checks to see if strTestResidue matches one of the residues in strRuleResidues
            // Used to test by Rule Residues and Exception Residues

            for (int intCharIndex = 0; intCharIndex < strRuleResidues.Length; intCharIndex++)
            {
                var strCompareResidue = strRuleResidues.Substring(intCharIndex, 1).Trim();
                if (strCompareResidue.Length > 0)
                {
                    if ((strTestResidue ?? "") == (strCompareResidue ?? ""))
                    {
                        // Match found
                        return true;
                    }
                }
            }

            return false;
        }

        public double ComputeImmoniumMass(double dblResidueMass)
        {
            return dblResidueMass - dblImmoniumMassDifference;
        }

        private void InitializeArrays()
        {
            mNTerminus.Initialize();
            mCTerminus.Initialize();
            mFragSpectrumOptions.Initialize();
        }

        public string LookupIonTypeString(itIonTypeConstants eIonType)
        {
            switch (eIonType)
            {
                case itIonTypeConstants.itAIon:
                    return "a";
                case itIonTypeConstants.itBIon:
                    return "b";
                case itIonTypeConstants.itYIon:
                    return "y";
                case itIonTypeConstants.itCIon:
                    return "c";
                case itIonTypeConstants.itZIon:
                    return "z";
                default:
                    return string.Empty;
            }
        }

        public int RemoveAllResidues()
        {
            int RemoveAllResiduesRet = default;
            // Removes all the residues
            // Returns 0 on success, 1 on failure

            ReserveMemoryForResidues(50, false);
            ResidueCount = 0;
            mTotalMass = 0d;

            RemoveAllResiduesRet = 0;
            return RemoveAllResiduesRet;
        }

        public int RemoveAllModificationSymbols()
        {
            int RemoveAllModificationSymbolsRet = default;
            // Removes all possible Modification Symbols
            // Returns 0 on success, 1 on failure
            // Removing all modifications will invalidate any modifications present in a sequence

            ReserveMemoryForModifications(10, false);
            ModificationSymbolCount = 0;

            RemoveAllModificationSymbolsRet = 0;
            return RemoveAllModificationSymbolsRet;
        }

        private void RemoveLeadingH(ref string strWorkingSequence)
        {
            // Returns True if a leading H is removed

            if (strWorkingSequence.Length >= 4 && strWorkingSequence.ToUpper().StartsWith("H"))
            {
                // If next character is not a character, then remove the H and the non-letter character
                if (!char.IsLetter(strWorkingSequence[1]))
                {
                    // Remove the leading H
                    strWorkingSequence = strWorkingSequence.Substring(2);
                }
                // Otherwise, see if next three characters are letters
                else if (char.IsLetter(strWorkingSequence[1]) && char.IsLetter(strWorkingSequence[2]) && char.IsLetter(strWorkingSequence[3]))
                {
                    // Formula starts with 4 characters and the first is H, see if the first 3 characters are a valid amino acid code
                    var lngAbbrevID = ElementAndMassRoutines.GetAbbreviationIDInternal(strWorkingSequence.Substring(0, 3), true);

                    if (lngAbbrevID <= 0)
                    {
                        // Doesn't start with a valid amino acid 3 letter abbreviation, so remove the initial H
                        strWorkingSequence = strWorkingSequence.Substring(1);
                    }
                }
            }

            return;
        }

        private bool RemoveTrailingOH(ref string strWorkingSequence)
        {
            bool RemoveTrailingOHRet = default;
            // Returns True if a trailing OH is removed

            var blnOHRemoved = false;
            var lngStringLength = Strings.Len(strWorkingSequence);
            if (strWorkingSequence.Length >= 5 && strWorkingSequence.ToUpper().EndsWith("OH"))
            {
                // If previous character is not a character, then remove the OH
                if (!char.IsLetter(Conversions.ToChar(Strings.Mid(strWorkingSequence, lngStringLength - 2, 1))))
                {
                    strWorkingSequence = Strings.Left(strWorkingSequence, lngStringLength - 3);
                    blnOHRemoved = true;
                }
                // Otherwise, see if previous three characters are letters
                else if (char.IsLetter(Conversions.ToChar(Strings.Mid(strWorkingSequence, lngStringLength - 2, 1))))
                {
                    // Formula ends with 3 characters and the last two are OH, see if the last 3 characters are a valid amino acid code
                    var lngAbbrevID = ElementAndMassRoutines.GetAbbreviationIDInternal(Strings.Mid(strWorkingSequence, lngStringLength - 2, 3), true);

                    if (lngAbbrevID <= 0)
                    {
                        // Doesn't end with a valid amino acid 3 letter abbreviation, so remove the trailing OH
                        strWorkingSequence = Strings.Left(strWorkingSequence, lngStringLength - 2);
                        blnOHRemoved = true;
                    }
                }
            }

            RemoveTrailingOHRet = blnOHRemoved;
            return RemoveTrailingOHRet;
        }

        public int RemoveModification(ref string strModSymbol)
        {
            int RemoveModificationRet = default;
            // Returns 0 if found and removed; 1 if error

            int lngIndex;
            var blnRemoved = default(bool);

            for (lngIndex = 1; lngIndex <= ModificationSymbolCount; lngIndex++)
            {
                if ((ModificationSymbols[lngIndex].Symbol ?? "") == (strModSymbol ?? ""))
                {
                    RemoveModificationByID(lngIndex);
                    blnRemoved = true;
                }
            }

            if (blnRemoved)
            {
                RemoveModificationRet = 0;
            }
            else
            {
                RemoveModificationRet = 1;
            }

            return RemoveModificationRet;
        }

        public int RemoveModificationByID(int lngModificationID)
        {
            // Returns 0 if found and removed; 1 if error

            bool blnRemoved;

            if (lngModificationID >= 1 && lngModificationID <= ModificationSymbolCount)
            {
                for (var lngIndex = lngModificationID; lngIndex < ModificationSymbolCount; lngIndex++)
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
            else
            {
                return 1;
            }
        }

        public int RemoveResidue(int lngResidueNumber)
        {
            // Returns 0 if found and removed; 1 if error

            if (lngResidueNumber >= 1 && lngResidueNumber <= ResidueCount)
            {
                int lngIndex;
                for (lngIndex = lngResidueNumber; lngIndex < ResidueCount; lngIndex++)
                    Residues[lngIndex] = Residues[lngIndex + 1];

                ResidueCount -= 1;
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private void ReserveMemoryForResidues(int lngNewResidueCount, bool blnPreserveContents)
        {
            // Only reserves the memory if necessary
            // Thus, do not use this sub to clear Residues()

            if (lngNewResidueCount > ResidueCountDimmed)
            {
                ResidueCountDimmed = lngNewResidueCount + RESIDUE_DIM_CHUNK;
                int intIndex;
                if (blnPreserveContents && Residues != null)
                {
                    var intOldIndexEnd = Residues.Length - 1;
                    Array.Resize(ref Residues, ResidueCountDimmed + 1);
                    for (intIndex = intOldIndexEnd + 1; intIndex <= ResidueCountDimmed; intIndex++)
                        Residues[intIndex].Initialize(true);
                }
                else
                {
                    Residues = new udtResidueType[ResidueCountDimmed + 1];
                    for (intIndex = 0; intIndex <= ResidueCountDimmed; intIndex++)
                        Residues[intIndex].Initialize(true);
                }
            }
        }

        private void ReserveMemoryForModifications(int lngNewModificationCount, bool blnPreserveContents)
        {
            if (lngNewModificationCount > ModificationSymbolCountDimmed)
            {
                ModificationSymbolCountDimmed = lngNewModificationCount + 10;
                if (blnPreserveContents)
                {
                    Array.Resize(ref ModificationSymbols, ModificationSymbolCountDimmed + 1);
                }
                else
                {
                    ModificationSymbols = new udtModificationSymbolType[ModificationSymbolCountDimmed + 1];
                }
            }
        }

        public int SetCTerminus(string strFormula)
        {
            return SetCTerminus(strFormula, "", true);
        }

        public int SetCTerminus(string strFormula, string strFollowingResidue)
        {
            return SetCTerminus(strFormula, strFollowingResidue, true);
        }

        public int SetCTerminus(string strFormula, string strFollowingResidue, bool blnUse3LetterCode)
        {
            int SetCTerminusRet = default;

            // Returns 0 if success; 1 if error

            // Typical N terminus mods
            // Free Acid = OH
            // Amide = NH2

            mCTerminus.Formula = strFormula;
            mCTerminus.Mass = ElementAndMassRoutines.ComputeFormulaWeight(ref mCTerminus.Formula);
            if (mCTerminus.Mass < 0d)
            {
                mCTerminus.Mass = 0d;
                SetCTerminusRet = 1;
            }
            else
            {
                SetCTerminusRet = 0;
            }

            mCTerminus.PrecedingResidue = FillResidueStructureUsingSymbol(string.Empty);
            mCTerminus.FollowingResidue = FillResidueStructureUsingSymbol(strFollowingResidue, blnUse3LetterCode);

            UpdateResidueMasses();
            return SetCTerminusRet;
        }

        public int SetCTerminusGroup(ctgCTerminusGroupConstants eCTerminusGroup)
        {
            return SetCTerminusGroup(eCTerminusGroup, "", true);
        }

        public int SetCTerminusGroup(ctgCTerminusGroupConstants eCTerminusGroup, string strFollowingResidue)
        {
            return SetCTerminusGroup(eCTerminusGroup, strFollowingResidue, true);
        }

        public int SetCTerminusGroup(ctgCTerminusGroupConstants eCTerminusGroup,
            string strFollowingResidue,
            bool blnUse3LetterCode)
        {
            // Returns 0 if success; 1 if error
            int lngError;
            switch (eCTerminusGroup)
            {
                case ctgCTerminusGroupConstants.ctgHydroxyl:
                    lngError = SetCTerminus("OH", strFollowingResidue, blnUse3LetterCode);
                    break;
                case ctgCTerminusGroupConstants.ctgAmide:
                    lngError = SetCTerminus("NH2", strFollowingResidue, blnUse3LetterCode);
                    break;
                case ctgCTerminusGroupConstants.ctgNone:
                    lngError = SetCTerminus(string.Empty, strFollowingResidue, blnUse3LetterCode);
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
                intensityOptions.IonType[(int)itIonTypeConstants.itAIon] = DEFAULT_A_ION_INTENSITY;
                intensityOptions.IonType[(int)itIonTypeConstants.itBIon] = DEFAULT_BYCZ_ION_INTENSITY;
                intensityOptions.IonType[(int)itIonTypeConstants.itYIon] = DEFAULT_BYCZ_ION_INTENSITY;
                intensityOptions.IonType[(int)itIonTypeConstants.itCIon] = DEFAULT_BYCZ_ION_INTENSITY;
                intensityOptions.IonType[(int)itIonTypeConstants.itZIon] = DEFAULT_BYCZ_ION_INTENSITY;
                intensityOptions.BYIonShoulder = DEFAULT_B_Y_ION_SHOULDER_INTENSITY;
                intensityOptions.NeutralLoss = DEFAULT_NEUTRAL_LOSS_ION_INTENSITY;

                // A ions can have ammonia and phosphate loss, but not water loss
                var aIonOption = mFragSpectrumOptions.IonTypeOptions[(int)itIonTypeConstants.itAIon];
                aIonOption.ShowIon = true;
                aIonOption.NeutralLossAmmonia = true;
                aIonOption.NeutralLossPhosphate = true;
                aIonOption.NeutralLossWater = false;

                itIonTypeConstants eIonIndex;
                for (eIonIndex = itIonTypeConstants.itBIon; eIonIndex <= itIonTypeConstants.itZIon; eIonIndex++)
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

        public void SetFragmentationSpectrumOptions(udtFragmentationSpectrumOptionsType udtNewFragSpectrumOptions)
        {
            mFragSpectrumOptions = udtNewFragSpectrumOptions;
        }

        public void SetModificationSymbol(string strModSymbol, double dblModificationMass)
        {
            SetModificationSymbol(strModSymbol, dblModificationMass, blnIndicatesPhosphorylation: false, strComment: string.Empty);
        }

        public void SetModificationSymbol(string strModSymbol, double dblModificationMass, string strComment)
        {
            SetModificationSymbol(strModSymbol, dblModificationMass, blnIndicatesPhosphorylation: false, strComment: strComment);
        }

        public int SetModificationSymbol(string strModSymbol, double dblModificationMass, bool blnIndicatesPhosphorylation, string strComment)
        {
            int SetModificationSymbolRet = default;
            // Adds a new modification or updates an existing one (based on strModSymbol)
            // Returns 0 if successful, otherwise, returns -1

            var lngErrorID = 0;
            if (Strings.Len(strModSymbol) < 1)
            {
                lngErrorID = -1;
            }
            else
            {
                // Make sure strModSymbol contains no letters, numbers, spaces, dashes, or periods
                int lngIndex;
                for (lngIndex = 0; lngIndex < strModSymbol.Length; lngIndex++)
                {
                    var strTestChar = Strings.Mid(strModSymbol, lngIndex, 1);
                    if (!ElementAndMassRoutines.IsModSymbolInternal(strTestChar))
                    {
                        lngErrorID = -1;
                    }
                }

                if (lngErrorID == 0)
                {
                    // See if the modification is alrady present
                    var lngIndexToUse = GetModificationSymbolID(strModSymbol);

                    if (lngIndexToUse == 0)
                    {
                        // Need to add the modification
                        ModificationSymbolCount += 1;
                        lngIndexToUse = ModificationSymbolCount;
                        ReserveMemoryForModifications(ModificationSymbolCount, true);
                    }

                    var mod = ModificationSymbols[lngIndexToUse];
                    mod.Symbol = strModSymbol;
                    mod.ModificationMass = dblModificationMass;
                    mod.IndicatesPhosphorylation = blnIndicatesPhosphorylation;
                    mod.Comment = strComment;
                }
            }

            SetModificationSymbolRet = lngErrorID;
            return SetModificationSymbolRet;
        }

        public int SetNTerminus(string strFormula)
        {
            return SetNTerminus(strFormula, "", true);
        }

        public int SetNTerminus(string strFormula, string strPrecedingResidue)
        {
            return SetNTerminus(strFormula, strPrecedingResidue, true);
        }

        public int SetNTerminus(string strFormula, string strPrecedingResidue, bool blnUse3LetterCode)
        {
            int SetNTerminusRet = default;
            // Returns 0 if success; 1 if error

            // Typical N terminus mods
            // Hydrogen = H
            // Acetyl = C2OH3
            // PyroGlu = C5O2NH6
            // Carbamyl = CONH2
            // PTC = C7H6NS

            mNTerminus.Formula = strFormula;
            mNTerminus.Mass = ElementAndMassRoutines.ComputeFormulaWeight(ref mNTerminus.Formula);
            if (mNTerminus.Mass < 0d)
            {
                mNTerminus.Mass = 0d;
                SetNTerminusRet = 1;
            }
            else
            {
                SetNTerminusRet = 0;
            }

            mNTerminus.PrecedingResidue = FillResidueStructureUsingSymbol(strPrecedingResidue, blnUse3LetterCode);
            mNTerminus.FollowingResidue = FillResidueStructureUsingSymbol(string.Empty);

            UpdateResidueMasses();
            return SetNTerminusRet;
        }

        public int SetNTerminusGroup(ntgNTerminusGroupConstants eNTerminusGroup)
        {
            return SetNTerminusGroup(eNTerminusGroup, "", true);
        }

        public int SetNTerminusGroup(ntgNTerminusGroupConstants eNTerminusGroup, string strPrecedingResidue)
        {
            return SetNTerminusGroup(eNTerminusGroup, strPrecedingResidue, true);
        }

        public int SetNTerminusGroup(ntgNTerminusGroupConstants eNTerminusGroup,
            string strPrecedingResidue,
            bool blnUse3LetterCode)
        {
            // Returns 0 if success; 1 if error
            int lngError;

            switch (eNTerminusGroup)
            {
                case ntgNTerminusGroupConstants.ntgHydrogen:
                    lngError = SetNTerminus("H", strPrecedingResidue, blnUse3LetterCode);
                    break;
                case ntgNTerminusGroupConstants.ntgHydrogenPlusProton:
                    lngError = SetNTerminus("HH", strPrecedingResidue, blnUse3LetterCode);
                    break;
                case ntgNTerminusGroupConstants.ntgAcetyl:
                    lngError = SetNTerminus("C2OH3", strPrecedingResidue, blnUse3LetterCode);
                    break;
                case ntgNTerminusGroupConstants.ntgPyroGlu:
                    lngError = SetNTerminus("C5O2NH6", strPrecedingResidue, blnUse3LetterCode);
                    break;
                case ntgNTerminusGroupConstants.ntgCarbamyl:
                    lngError = SetNTerminus("CONH2", strPrecedingResidue, blnUse3LetterCode);
                    break;
                case ntgNTerminusGroupConstants.ntgPTC:
                    lngError = SetNTerminus("C7H6NS", strPrecedingResidue, blnUse3LetterCode);
                    break;
                case ntgNTerminusGroupConstants.ntgNone:
                    lngError = SetNTerminus(string.Empty, strPrecedingResidue, blnUse3LetterCode);
                    break;
                default:
                    lngError = 1;
                    break;
            }

            return lngError;
        }

        public int SetResidue(int lngResidueNumber,
            string strSymbol)
        {
            return SetResidue(lngResidueNumber, strSymbol, true, false);
        }

        public int SetResidue(int lngResidueNumber,
            string strSymbol, bool
                blnIs3LetterCode)
        {
            return SetResidue(lngResidueNumber, strSymbol, blnIs3LetterCode, false);
        }

        public int SetResidue(int lngResidueNumber,
            string strSymbol,
            bool blnIs3LetterCode,
            bool blnPhosphorylated)
        {
            int SetResidueRet = default;

            // Sets or adds a residue (must add residues in order)
            // Returns the index of the modified residue, or the new index if added
            // Returns -1 if a problem

            int lngIndexToUse;
            string str3LetterSymbol;

            if (string.IsNullOrEmpty(strSymbol))
            {
                return -1;
            }

            if (lngResidueNumber > ResidueCount)
            {
                ResidueCount += 1;
                ReserveMemoryForResidues(ResidueCount, true);
                lngIndexToUse = ResidueCount;
            }
            else
            {
                lngIndexToUse = lngResidueNumber;
            }

            var residue = Residues[lngIndexToUse];
            if (blnIs3LetterCode)
            {
                str3LetterSymbol = strSymbol;
            }
            else
            {
                str3LetterSymbol = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbol, true);
            }

            if (Strings.Len(str3LetterSymbol) == 0)
            {
                residue.Symbol = UNKNOWN_SYMBOL;
            }
            else
            {
                residue.Symbol = str3LetterSymbol;
            }

            residue.Phosphorylated = blnPhosphorylated;
            if (blnPhosphorylated)
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

            SetResidueRet = lngIndexToUse;
            return SetResidueRet;
        }

        public int SetResidueModifications(int lngResidueNumber, short intModificationCount, int[] lngModificationIDsOneBased)
        {
            int SetResidueModificationsRet = default;
            // Sets the modifications for a specific residue
            // Modification Symbols are defined using successive calls to SetModificationSymbol()

            // Returns 0 if modifications set; returns 1 if an error

            if (lngResidueNumber >= 1 && lngResidueNumber <= ResidueCount && intModificationCount >= 0)
            {
                var residue = Residues[lngResidueNumber];
                if (intModificationCount > MAX_MODIFICATIONS)
                {
                    intModificationCount = MAX_MODIFICATIONS;
                }

                residue.ModificationIDCount = 0;
                residue.Phosphorylated = false;
                short intIndex;
                for (intIndex = 1; intIndex <= intModificationCount; intIndex++)
                {
                    var lngNewModID = lngModificationIDsOneBased[intIndex];
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

                SetResidueModificationsRet = 0;
            }
            else
            {
                SetResidueModificationsRet = 1;
            }

            return SetResidueModificationsRet;
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="strSequence">Peptide sequence using 3-letter amino acid symbols</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If strSequence is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence(string strSequence)
        {
            return SetSequence(strSequence,
                   ntgNTerminusGroupConstants.ntgHydrogen,
                   ctgCTerminusGroupConstants.ctgHydroxyl,
                   blnIs3LetterCode: true, bln1LetterCheckForPrefixAndSuffixResidues: true, bln3LetterCheckForPrefixHandSuffixOH: true, blnAddMissingModificationSymbols: false);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="strSequence">Peptide sequence using 1-letter amino acid symbols</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If strSequence is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence1LetterSymbol(string strSequence)
        {
            return SetSequence(strSequence,
                ntgNTerminusGroupConstants.ntgHydrogen,
                ctgCTerminusGroupConstants.ctgHydroxyl,
                blnIs3LetterCode: false, bln1LetterCheckForPrefixAndSuffixResidues: true, bln3LetterCheckForPrefixHandSuffixOH: true, blnAddMissingModificationSymbols: false);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="strSequence">Peptide sequence</param>
        /// <param name="blnIs3LetterCode">Set to True for 3-letter amino acid symbols, False for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="bln1LetterCheckForPrefixAndSuffixResidues"></param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If strSequence is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence(string strSequence,
            bool blnIs3LetterCode,
            bool bln1LetterCheckForPrefixAndSuffixResidues)
        {
            return SetSequence(strSequence, ntgNTerminusGroupConstants.ntgHydrogen, ctgCTerminusGroupConstants.ctgHydroxyl,
                blnIs3LetterCode, bln1LetterCheckForPrefixAndSuffixResidues, bln3LetterCheckForPrefixHandSuffixOH: true, blnAddMissingModificationSymbols: false);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="strSequence">Peptide sequence using 3-letter amino acid symbols</param>
        /// <param name="eNTerminus">N-terminus group</param>
        /// <param name="eCTerminus">C-terminus group</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If strSequence is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence(string strSequence,
            ntgNTerminusGroupConstants eNTerminus,
            ctgCTerminusGroupConstants eCTerminus)
        {
            return SetSequence(strSequence, eNTerminus, eCTerminus,
                blnIs3LetterCode: true, bln1LetterCheckForPrefixAndSuffixResidues: true, bln3LetterCheckForPrefixHandSuffixOH: true, blnAddMissingModificationSymbols: false);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="strSequence">Peptide sequence</param>
        /// <param name="eNTerminus">N-terminus group</param>
        /// <param name="eCTerminus">C-terminus group</param>
        /// <param name="blnIs3LetterCode">Set to True for 3-letter amino acid symbols, False for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If strSequence is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence(string strSequence,
            ntgNTerminusGroupConstants eNTerminus,
            ctgCTerminusGroupConstants eCTerminus,
            bool blnIs3LetterCode)
        {
            return SetSequence(strSequence, eNTerminus, eCTerminus,
                blnIs3LetterCode, bln1LetterCheckForPrefixAndSuffixResidues: true, bln3LetterCheckForPrefixHandSuffixOH: true, blnAddMissingModificationSymbols: false);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="strSequence">Peptide sequence</param>
        /// <param name="eNTerminus">N-terminus group</param>
        /// <param name="eCTerminus">C-terminus group</param>
        /// <param name="blnIs3LetterCode">Set to True for 3-letter amino acid symbols, False for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="bln1LetterCheckForPrefixAndSuffixResidues">Set to True to check for and remove prefix and suffix residues when blnIs3LetterCode = False</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If strSequence is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence(string strSequence,
            ntgNTerminusGroupConstants eNTerminus,
            ctgCTerminusGroupConstants eCTerminus,
            bool blnIs3LetterCode,
            bool bln1LetterCheckForPrefixAndSuffixResidues)
        {
            return SetSequence(strSequence, eNTerminus, eCTerminus,
                blnIs3LetterCode, bln1LetterCheckForPrefixAndSuffixResidues, bln3LetterCheckForPrefixHandSuffixOH: true, blnAddMissingModificationSymbols: false);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="strSequence">Peptide sequence</param>
        /// <param name="eNTerminus">N-terminus group</param>
        /// <param name="eCTerminus">C-terminus group</param>
        /// <param name="blnIs3LetterCode">Set to True for 3-letter amino acid symbols, False for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="bln1LetterCheckForPrefixAndSuffixResidues">Set to True to check for and remove prefix and suffix residues when blnIs3LetterCode = False</param>
        /// <param name="bln3LetterCheckForPrefixHandSuffixOH">Set to True to check for and remove prefix H and OH when blnIs3LetterCode = True</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If strSequence is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence(string strSequence,
            ntgNTerminusGroupConstants eNTerminus,
            ctgCTerminusGroupConstants eCTerminus,
            bool blnIs3LetterCode,
            bool bln1LetterCheckForPrefixAndSuffixResidues,
            bool bln3LetterCheckForPrefixHandSuffixOH)
        {
            return SetSequence(strSequence, eNTerminus, eCTerminus,
                blnIs3LetterCode,
                bln1LetterCheckForPrefixAndSuffixResidues,
                bln3LetterCheckForPrefixHandSuffixOH, blnAddMissingModificationSymbols: false);
        }

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="strSequence">Peptide sequence</param>
        /// <param name="eNTerminus">N-terminus group</param>
        /// <param name="eCTerminus">C-terminus group</param>
        /// <param name="blnIs3LetterCode">Set to True for 3-letter amino acid symbols, False for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="bln1LetterCheckForPrefixAndSuffixResidues">Set to True to check for and remove prefix and suffix residues when blnIs3LetterCode = False</param>
        /// <param name="bln3LetterCheckForPrefixHandSuffixOH">Set to True to check for and remove prefix H and OH when blnIs3LetterCode = True</param>
        /// <param name="blnAddMissingModificationSymbols">Set to True to automatically add missing modification symbols (though the mod masses will be 0)</param>
        /// <returns>0 if success or 1 if an error</returns>
        /// <remarks>If strSequence is blank or contains no valid residues, then will still return 0</remarks>
        public int SetSequence(string strSequence,
            ntgNTerminusGroupConstants eNTerminus,
            ctgCTerminusGroupConstants eCTerminus,
            bool blnIs3LetterCode,
            bool bln1LetterCheckForPrefixAndSuffixResidues,
            bool bln3LetterCheckForPrefixHandSuffixOH,
            bool blnAddMissingModificationSymbols)
        {
            try
            {
                strSequence = Strings.Trim(strSequence);

                var lngSequenceStrLength = Strings.Len(strSequence);
                if (lngSequenceStrLength == 0)
                {
                    return AssureNonZero(0);
                }

                // Clear any old residue information
                ResidueCount = 0;
                ReserveMemoryForResidues(ResidueCount, false);

                string str3LetterSymbol;
                int lngModSymbolLength;
                int lngIndex;
                if (!blnIs3LetterCode)
                {
                    // Sequence is 1 letter codes

                    if (bln1LetterCheckForPrefixAndSuffixResidues)
                    {
                        // First look if sequence is in the form A.BCDEFG.Z or -.BCDEFG.Z or A.BCDEFG.-
                        // If so, then need to strip out the preceding A and Z residues since they aren't really part of the sequence
                        if (lngSequenceStrLength > 1 && strSequence.Contains("."))
                        {
                            if (Strings.Mid(strSequence, 2, 1) == ".")
                            {
                                strSequence = Strings.Mid(strSequence, 3);
                                lngSequenceStrLength = Strings.Len(strSequence);
                            }

                            if (Strings.Mid(strSequence, lngSequenceStrLength - 1, 1) == ".")
                            {
                                strSequence = Strings.Left(strSequence, lngSequenceStrLength - 2);
                                lngSequenceStrLength = Strings.Len(strSequence);
                            }

                            // Also check for starting with a . or ending with a .
                            if (Strings.Left(strSequence, 1) == ".")
                            {
                                strSequence = Strings.Mid(strSequence, 2);
                            }

                            if (Strings.Right(strSequence, 1) == ".")
                            {
                                strSequence = Strings.Left(strSequence, Strings.Len(strSequence) - 1);
                            }

                            lngSequenceStrLength = Strings.Len(strSequence);
                        }
                    }

                    for (lngIndex = 0; lngIndex < lngSequenceStrLength; lngIndex++)
                    {
                        var str1LetterSymbol = Strings.Mid(strSequence, lngIndex, 1);
                        if (char.IsLetter(Conversions.ToChar(str1LetterSymbol)))
                        {
                            // Character found
                            // Look up 3 letter symbol
                            // If none is found, this will return an empty string
                            str3LetterSymbol = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(str1LetterSymbol, true);

                            if (Strings.Len(str3LetterSymbol) == 0)
                                str3LetterSymbol = UNKNOWN_SYMBOL;

                            SetSequenceAddResidue(str3LetterSymbol);

                            // Look at following character(s), and record any modification symbols present
                            lngModSymbolLength = CheckForModifications(Strings.Mid(strSequence, lngIndex + 1), ResidueCount, blnAddMissingModificationSymbols);

                            lngIndex += lngModSymbolLength;
                        }
                        // If . or - or space, then ignore it
                        // If a number, ignore it
                        // If anything else, then should have been skipped, or should be skipped
                        else if (str1LetterSymbol == "." || str1LetterSymbol == "-" || str1LetterSymbol == " ")
                        {
                            // All is fine; we can skip this
                        }
                        else
                        {
                            // Ignore it
                        }
                    }
                }
                else
                {
                    // Sequence is 3 letter codes
                    lngIndex = 1;

                    if (bln3LetterCheckForPrefixHandSuffixOH)
                    {
                        // Look for a leading H or trailing OH, provided those don't match any of the amino acids
                        RemoveLeadingH(ref strSequence);
                        RemoveTrailingOH(ref strSequence);

                        // Recompute sequence length
                        lngSequenceStrLength = Strings.Len(strSequence);
                    }

                    while (lngIndex <= lngSequenceStrLength - 2)
                    {
                        var strFirstChar = Strings.Mid(strSequence, lngIndex, 1);
                        if (char.IsLetter(Conversions.ToChar(strFirstChar)))
                        {
                            if (char.IsLetter(Conversions.ToChar(Strings.Mid(strSequence, lngIndex + 1, 1))) && char.IsLetter(Conversions.ToChar(Strings.Mid(strSequence, lngIndex + 2, 1))))
                            {
                                str3LetterSymbol = Strings.UCase(strFirstChar) + Strings.LCase(Strings.Mid(strSequence, lngIndex + 1, 2));

                                if (ElementAndMassRoutines.GetAbbreviationIDInternal(str3LetterSymbol, true) == 0)
                                {
                                    // 3 letter symbol not found
                                    // Add anyway, but mark as Xxx
                                    str3LetterSymbol = UNKNOWN_SYMBOL;
                                }

                                SetSequenceAddResidue(str3LetterSymbol);

                                // Look at following character(s), and record any modification symbols present
                                lngModSymbolLength = CheckForModifications(Strings.Mid(strSequence, lngIndex + 3), ResidueCount, blnAddMissingModificationSymbols);

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
                            else
                            {
                                // Ignore it
                            }

                            lngIndex += 1;
                        }
                    }
                }

                // By calling SetNTerminus and SetCTerminus, the UpdateResidueMasses() Sub will also be called
                mDelayUpdateResidueMass = true;
                SetNTerminusGroup(eNTerminus);
                SetCTerminusGroup(eCTerminus);

                mDelayUpdateResidueMass = false;
                UpdateResidueMasses();

                return 0;
            }
            catch
            {
                return AssureNonZero(Information.Err().Number);
            }
        }

        private void SetSequenceAddResidue(string str3LetterSymbol)
        {
            if (string.IsNullOrWhiteSpace(str3LetterSymbol))
            {
                str3LetterSymbol = UNKNOWN_SYMBOL;
            }

            ResidueCount += 1;
            ReserveMemoryForResidues(ResidueCount, true);

            var residue = Residues[ResidueCount];
            residue.Symbol = str3LetterSymbol;
            residue.Phosphorylated = false;
            residue.ModificationIDCount = 0;
        }

        public void SetSymbolAmmoniaLoss(string strNewSymbol)
        {
            if (!string.IsNullOrWhiteSpace(strNewSymbol))
            {
                mAmmoniaLossSymbol = strNewSymbol;
            }
        }

        public void SetSymbolPhosphoLoss(string strNewSymbol)
        {
            if (!string.IsNullOrWhiteSpace(strNewSymbol))
            {
                mPhosphoLossSymbol = strNewSymbol;
            }
        }

        public void SetSymbolWaterLoss(string strNewSymbol)
        {
            if (!string.IsNullOrWhiteSpace(strNewSymbol))
            {
                mWaterLossSymbol = strNewSymbol;
            }
        }

        private void ShellSortFragSpectrum(ref udtFragmentationSpectrumDataType[] FragSpectrumWork, ref int[] PointerArray, int lngLowIndex, int lngHighIndex)
        {
            // Sort the list using a shell sort

            // Sort PointerArray[lngLowIndex..lngHighIndex] by comparing FragSpectrumWork(PointerArray(x)).Mass

            // Compute largest increment
            var lngCount = lngHighIndex - lngLowIndex + 1;
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
                int lngIndex;
                for (lngIndex = lngLowIndex + lngIncrement; lngIndex <= lngHighIndex; lngIndex++)
                {
                    var lngPointerSwap = PointerArray[lngIndex];
                    int lngIndexCompare;
                    for (lngIndexCompare = lngIndex - lngIncrement; lngIndexCompare >= lngLowIndex; lngIndexCompare += -lngIncrement)
                    {
                        // Use <= to sort ascending; Use > to sort descending
                        if (FragSpectrumWork[PointerArray[lngIndexCompare]].Mass <= FragSpectrumWork[lngPointerSwap].Mass)
                            break;
                        PointerArray[lngIndexCompare + lngIncrement] = PointerArray[lngIndexCompare];
                    }

                    PointerArray[lngIndexCompare + lngIncrement] = lngPointerSwap;
                }

                lngIncrement /= 3;
            }
        }

        private void UpdateResidueMasses()
        {
            int lngIndex;
            var lngValidResidueCount = default(int);
            var blnProtonatedNTerminus = default(bool);

            if (mDelayUpdateResidueMass)
                return;

            // The N-terminus ions are the basis for the running total
            var dblRunningTotal = mNTerminus.Mass;
            if (Strings.UCase(mNTerminus.Formula) == "HH")
            {
                // ntgHydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                // we need to subtract it out here
                // However, we need to subtract out dblHydrogenMass, and not dblChargeCarrierMass since the current
                // formula's mass was computed using two hydrogens, and not one hydrogen and one charge carrier
                blnProtonatedNTerminus = true;
                dblRunningTotal -= dblHydrogenMass;
            }

            for (lngIndex = 1; lngIndex <= ResidueCount; lngIndex++)
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
                    short intModIndex;
                    for (intModIndex = 1; intModIndex <= residue.ModificationIDCount; intModIndex++)
                    {
                        if (residue.ModificationIDs[intModIndex] <= ModificationSymbolCount)
                        {
                            residue.MassWithMods = residue.MassWithMods + ModificationSymbols[residue.ModificationIDs[intModIndex]].ModificationMass;
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
                            residue.MassWithMods = residue.MassWithMods + dblPhosphorylationMass;
                        }
                    }

                    dblRunningTotal += residue.MassWithMods;

                    residue.IonMass[(int)itIonTypeConstants.itAIon] = dblRunningTotal - dblImmoniumMassDifference - dblChargeCarrierMass;
                    residue.IonMass[(int)itIonTypeConstants.itBIon] = dblRunningTotal;

                    // Add NH3 (ammonia) to the B ion mass to get the C ion mass
                    residue.IonMass[(int)itIonTypeConstants.itCIon] = residue.IonMass[(int)itIonTypeConstants.itBIon] + dblNH3Mass;
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

            for (lngIndex = ResidueCount; lngIndex >= 1; lngIndex -= 1)
            {
                var residue = Residues[lngIndex];
                if (residue.IonMass[(int)itIonTypeConstants.itAIon] > 0d)
                {
                    dblRunningTotal += residue.MassWithMods;
                    residue.IonMass[(int)itIonTypeConstants.itYIon] = dblRunningTotal + dblChargeCarrierMass;
                    if (lngIndex == 1)
                    {
                        // Add the N-terminus mass to highest y ion
                        residue.IonMass[(int)itIonTypeConstants.itYIon] = residue.IonMass[(int)itIonTypeConstants.itYIon] + mNTerminus.Mass - dblChargeCarrierMass;
                        if (blnProtonatedNTerminus)
                        {
                            // ntgHydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                            // we need to subtract it out here
                            // However, we need to subtract out dblHydrogenMass, and not dblChargeCarrierMass since the current
                            // formula's mass was computed using two hydrogens, and not one hydrogen and one charge carrier
                            residue.IonMass[(int)itIonTypeConstants.itYIon] = residue.IonMass[(int)itIonTypeConstants.itYIon] - dblHydrogenMass;
                        }
                    }

                    // Subtract NH2 (amide) from the Y ion mass to get the Z ion mass
                    residue.IonMass[(int)itIonTypeConstants.itZIon] = residue.IonMass[(int)itIonTypeConstants.itYIon] - (dblNH3Mass - dblHydrogenMass);
                }
            }
        }

        public void UpdateStandardMasses()
        {
            try
            {
                var eElementModeSaved = ElementAndMassRoutines.GetElementModeInternal();

                ElementAndMassRoutines.SetElementModeInternal(ElementAndMassTools.emElementModeConstants.emIsotopicMass);

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