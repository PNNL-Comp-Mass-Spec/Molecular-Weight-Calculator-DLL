using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.CompilerServices;

namespace MwtWinDll
{
    public class MoleMassDilution
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: MoleMassDilution

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2002
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

        public MoleMassDilution() : base()
        {
            InitializeClass();
        }

        #region "Enums"

        public enum acdAutoComputeDilutionModeConstants
        {
            acdFindRequiredDilutionVolumes = 0,
            acdFindRequiredTotalVolume,
            acdFindFinalConcentration,
            acdFindInitialConcentration
        }

        public enum acqAutoComputeQuantityModeConstants
        {
            acqFindAmount = 0,
            acqFindVolume,
            acqFindConcentration
        }

        private const uamUnitsAmountConstants AMOUNT_UNITS_VOLUME_INDEX_START = uamUnitsAmountConstants.uamLiters;
        private const uamUnitsAmountConstants AMOUNT_UNITS_LIST_INDEX_MAX = uamUnitsAmountConstants.uamPints;

        public enum uamUnitsAmountConstants
        {
            uamMoles = 0,
            uamMillimoles,
            uamMicroMoles,
            uamNanoMoles,
            uamPicoMoles,
            uamFemtoMoles,
            uamAttoMoles,
            uamKilograms,
            uamGrams,
            uamMilligrams,
            uamMicrograms,
            uamPounds,
            uamOunces,
            uamLiters,
            uamDeciLiters,
            uamMilliLiters,
            uamMicroLiters,
            uamNanoLiters,
            uamPicoLiters,
            uamGallons,
            uamQuarts,
            uamPints
        }

        public enum uevUnitsExtendedVolumeConstants
        {
            uevL = 0,
            uevDL,
            uevML,
            uevUL,
            uevNL,
            uevPL,
            uevGallons,
            uevQuarts,
            uevPints
        }

        public enum ummcUnitsMoleMassConcentrationConstants
        {
            ummcMolar = 0,
            ummcMilliMolar,
            ummcMicroMolar,
            ummcNanoMolar,
            ummcPicoMolar,
            ummcFemtoMolar,
            ummcAttoMolar,
            ummcMgPerDL,
            ummcMgPerML,
            ummcUgPerML,
            ummcNgPerML,
            ummcUgPerUL,
            ummcNgPerUL
        }


        #endregion

        #region "Data classes"
        private class udtMoleMassQuantityType
        {
            public double Amount; // In Moles
            public double Volume; // In L
            public double Concentration; // In Molar
            public double SampleMass; // In g
            public double SampleDensity; // In g/mL
        }

        private class udtMoleMassDilutionValuesType
        {
            public double InitialConcentration; // In Molar
            public double StockSolutionVolume; // In L
            public double FinalConcentration; // In Molar
            public double DilutingSolventVolume; // In L
            public double TotalFinalVolume; // In L
        }

        #endregion

        private const float POUNDS_PER_KG = 2.20462262f;
        private const float GALLONS_PER_L = 0.264172052f;

        private udtMoleMassQuantityType mQuantity = new udtMoleMassQuantityType();
        private udtMoleMassDilutionValuesType mDilutionValues = new udtMoleMassDilutionValuesType();

        private bool mAutoComputeDilution; // When true, automatically compute dilution results whenever any value changes
        private acdAutoComputeDilutionModeConstants mAutoComputeDilutionMode; // The value to compute when mAutoComputeDilution is true

        private bool mAutoComputeQuantity; // When true, automatically compute quantities whenever any value changes
        private acqAutoComputeQuantityModeConstants mAutoComputeQuantityMode; // The value to compute when mAutoComputeQuantity is true

        /// <summary>
        /// Checks if AutoCompute Dilution is enabled
        /// If yes, calls the appropriate function
        /// </summary>
        private void CheckAutoComputeDilution()
        {
            if (mAutoComputeDilution)
            {
                switch (mAutoComputeDilutionMode)
                {
                    case acdAutoComputeDilutionModeConstants.acdFindRequiredTotalVolume:
                        ComputeDilutionTotalVolume(out _);
                        break;
                    case acdAutoComputeDilutionModeConstants.acdFindFinalConcentration:
                        ComputeDilutionFinalConcentration();
                        break;
                    case acdAutoComputeDilutionModeConstants.acdFindInitialConcentration:
                        ComputeDilutionInitialConcentration();
                        break;
                    default:
                        // Includes acdFindRequiredDilutionVolumes
                        ComputeDilutionRequiredStockAndDilutingSolventVolumes(out _);
                        break;
                }
            }
        }

        /// <summary>
        /// Checks if AutoCompute Quantity is enabled
        /// If yes, calls the appropriate function
        /// </summary>
        private void CheckAutoComputeQuantity()
        {
            if (mAutoComputeQuantity)
            {
                switch (mAutoComputeQuantityMode)
                {
                    case acqAutoComputeQuantityModeConstants.acqFindVolume:
                        ComputeQuantityVolume();
                        break;
                    case acqAutoComputeQuantityModeConstants.acqFindConcentration:
                        ComputeQuantityConcentration();
                        break;
                    default:
                        // Includes acqFindAmount
                        ComputeQuantityAmount();
                        break;
                }
            }
        }

        /// <summary>
        /// Computes the Final Concentration, storing in .FinalConcentration, and returning it
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeDilutionFinalConcentration(ummcUnitsMoleMassConcentrationConstants eUnits = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        {
            if (Math.Abs(mDilutionValues.TotalFinalVolume) > float.Epsilon)
            {
                mDilutionValues.FinalConcentration = mDilutionValues.InitialConcentration * mDilutionValues.StockSolutionVolume / mDilutionValues.TotalFinalVolume;
            }
            else
            {
                mDilutionValues.TotalFinalVolume = 0d;
            }

            mDilutionValues.DilutingSolventVolume = mDilutionValues.TotalFinalVolume - mDilutionValues.StockSolutionVolume;
            if (mDilutionValues.DilutingSolventVolume < 0d)
                mDilutionValues.DilutingSolventVolume = -1;

            return ConvertConcentration(mDilutionValues.FinalConcentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits);
        }

        /// <summary>
        /// Computes the Initial Concentration, storing in .InitialConcentration, and returning it
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeDilutionInitialConcentration(ummcUnitsMoleMassConcentrationConstants eUnits = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        {
            if (Math.Abs(mDilutionValues.StockSolutionVolume) > float.Epsilon)
            {
                mDilutionValues.InitialConcentration = mDilutionValues.FinalConcentration * mDilutionValues.TotalFinalVolume / mDilutionValues.StockSolutionVolume;
            }
            else
            {
                mDilutionValues.InitialConcentration = 0d;
            }

            mDilutionValues.DilutingSolventVolume = mDilutionValues.TotalFinalVolume - mDilutionValues.StockSolutionVolume;
            if (mDilutionValues.DilutingSolventVolume < 0d)
                mDilutionValues.DilutingSolventVolume = -1;

            return ConvertConcentration(mDilutionValues.InitialConcentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits);
        }

        /// <summary>
        /// Computes the required dilution volumes using initial concentration, final concentration
        /// and total final volume, storing in .StockSolutionVolume and .DilutingSolventVolume,
        /// and returning .StockSolutionVolume
        /// </summary>
        /// <param name="dblNewDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="eStockSolutionUnits"></param>
        /// <param name="eDilutingSolventUnits"></param>
        /// <returns></returns>
        public double ComputeDilutionRequiredStockAndDilutingSolventVolumes(
            [Optional, DefaultParameterValue(0d)] out double dblNewDilutingSolventVolume,
            uevUnitsExtendedVolumeConstants eStockSolutionUnits = uevUnitsExtendedVolumeConstants.uevML,
            uevUnitsExtendedVolumeConstants eDilutingSolventUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            if (Math.Abs(mDilutionValues.InitialConcentration) > float.Epsilon)
            {
                mDilutionValues.StockSolutionVolume = mDilutionValues.FinalConcentration * mDilutionValues.TotalFinalVolume / mDilutionValues.InitialConcentration;
            }
            else
            {
                mDilutionValues.StockSolutionVolume = 0d;
            }

            mDilutionValues.DilutingSolventVolume = mDilutionValues.TotalFinalVolume - mDilutionValues.StockSolutionVolume;

            if (mDilutionValues.DilutingSolventVolume < 0d)
            {
                mDilutionValues.DilutingSolventVolume = -1;
                mDilutionValues.StockSolutionVolume = -1;
            }

            dblNewDilutingSolventVolume = ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, uevUnitsExtendedVolumeConstants.uevL, eDilutingSolventUnits);

            return ConvertVolumeExtended(mDilutionValues.StockSolutionVolume, uevUnitsExtendedVolumeConstants.uevL, eStockSolutionUnits);
        }

        /// <summary>
        /// Compute the total volume following the dilution, storing in .TotalFinalVolume, and returning it
        /// </summary>
        /// <param name="dblNewDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="eTotalVolumeUnits"></param>
        /// <param name="eDilutingSolventUnits"></param>
        /// <returns></returns>
        public double ComputeDilutionTotalVolume(
            [Optional, DefaultParameterValue(0d)] out double dblNewDilutingSolventVolume,
            uevUnitsExtendedVolumeConstants eTotalVolumeUnits = uevUnitsExtendedVolumeConstants.uevML,
            uevUnitsExtendedVolumeConstants eDilutingSolventUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            if (mDilutionValues.InitialConcentration > 0d && mDilutionValues.FinalConcentration > 0d)
            {
                mDilutionValues.TotalFinalVolume = mDilutionValues.InitialConcentration * mDilutionValues.StockSolutionVolume / mDilutionValues.FinalConcentration;
                if (mDilutionValues.TotalFinalVolume < 0d)
                {
                    mDilutionValues.TotalFinalVolume = 0d;
                }
            }
            else
            {
                mDilutionValues.TotalFinalVolume = 0d;
            }

            mDilutionValues.DilutingSolventVolume = mDilutionValues.TotalFinalVolume - mDilutionValues.StockSolutionVolume;
            if (mDilutionValues.DilutingSolventVolume < 0d)
                mDilutionValues.DilutingSolventVolume = -1;

            dblNewDilutingSolventVolume = ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, uevUnitsExtendedVolumeConstants.uevL, eDilutingSolventUnits);

            return ConvertVolumeExtended(mDilutionValues.TotalFinalVolume, uevUnitsExtendedVolumeConstants.uevL, eTotalVolumeUnits);
        }

        /// <summary>
        /// Computes mQuantity.Amount using mQuantity.Volume and mQuantity.Concentration, storing the result in mQuantity.Amount
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns>mQuantity.Amount, with the specified units</returns>
        public double ComputeQuantityAmount(uamUnitsAmountConstants eUnits = uamUnitsAmountConstants.uamMoles)
        {
            mQuantity.Amount = mQuantity.Concentration * mQuantity.Volume;

            return ConvertAmount(mQuantity.Amount, uamUnitsAmountConstants.uamMoles, eUnits);
        }

        /// <summary>
        /// Computes mQuantity.Concentration using mQuantity.Amount and mQuantity.Volume, storing the result in mQuantity.Concentration
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns>mQuantity.Concentration, with the specified units</returns>
        public double ComputeQuantityConcentration(ummcUnitsMoleMassConcentrationConstants eUnits = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        {
            if (Math.Abs(mQuantity.Volume) > float.Epsilon)
            {
                mQuantity.Concentration = mQuantity.Amount / mQuantity.Volume;
            }
            else
            {
                mQuantity.Concentration = 0d;
            }

            return ConvertConcentration(mQuantity.Concentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits);
        }

        /// <summary>
        /// Computes mQuantity.Volume using mQuantity.Amount and mQuantity.Concentration, storing the result in mQuantity.Volume
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns>mQuantity.Volume, with the specified units</returns>
        public double ComputeQuantityVolume(uevUnitsExtendedVolumeConstants eUnits = uevUnitsExtendedVolumeConstants.uevL)
        {
            if (Math.Abs(mQuantity.Concentration) > float.Epsilon)
            {
                mQuantity.Volume = mQuantity.Amount / mQuantity.Concentration;
            }
            else
            {
                mQuantity.Volume = 0d;
            }

            return ConvertVolumeExtended(mQuantity.Volume, uevUnitsExtendedVolumeConstants.uevL, eUnits);
        }

        /// <summary>
        /// This function uses .SampleMass and .SampleDensity if the units are mass and/or volume-based
        /// </summary>
        /// <param name="dblAmountIn"></param>
        /// <param name="eCurrentUnits"></param>
        /// <param name="eNewUnits"></param>
        /// <returns></returns>
        public double ConvertAmount(double dblAmountIn, uamUnitsAmountConstants eCurrentUnits, uamUnitsAmountConstants eNewUnits)
        {
            double dblValue, dblFactor;
            double dblSampleMass, dblSampleDensity;
            uevUnitsExtendedVolumeConstants eCurrentVolumeUnits;
            uevUnitsExtendedVolumeConstants eNewVolumeUnits;

            if (eCurrentUnits == eNewUnits)
            {
                // No conversion, simply return dblAmountIn
                return dblAmountIn;
            }

            if (eCurrentUnits >= AMOUNT_UNITS_VOLUME_INDEX_START && eCurrentUnits <= AMOUNT_UNITS_LIST_INDEX_MAX && eNewUnits >= AMOUNT_UNITS_VOLUME_INDEX_START && eNewUnits <= AMOUNT_UNITS_LIST_INDEX_MAX)
            {
                // Converting from one volume unit to another volume unit
                // No need to explicitly specify mass or density

                eCurrentVolumeUnits = (uevUnitsExtendedVolumeConstants)Conversions.ToInteger((int)eCurrentUnits - (int)AMOUNT_UNITS_VOLUME_INDEX_START);
                eNewVolumeUnits = (uevUnitsExtendedVolumeConstants)Conversions.ToInteger((int)eNewUnits - (int)AMOUNT_UNITS_VOLUME_INDEX_START);

                return ConvertVolumeExtended(dblAmountIn, eCurrentVolumeUnits, eNewVolumeUnits);
            }
            else
            {
                dblSampleMass = mQuantity.SampleMass;
                dblSampleDensity = mQuantity.SampleDensity;

                dblFactor = FactorAmount(eCurrentUnits, dblSampleMass, dblSampleDensity);
                if (dblFactor < 0d)
                {
                    return -1;
                }
                else
                {
                    dblValue = dblAmountIn * dblFactor;
                }

                dblFactor = FactorAmount(eNewUnits, dblSampleMass, dblSampleDensity);
                if (dblFactor <= 0d)
                {
                    return -1;
                }
                else
                {
                    return dblValue / dblFactor;
                }
            }
        }

        /// <summary>
        /// Convert concentration
        /// </summary>
        /// <param name="dblConcentrationIn"></param>
        /// <param name="eCurrentUnits"></param>
        /// <param name="eNewUnits"></param>
        /// <returns></returns>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        public double ConvertConcentration(double dblConcentrationIn, ummcUnitsMoleMassConcentrationConstants eCurrentUnits, ummcUnitsMoleMassConcentrationConstants eNewUnits)
        {
            double dblValue, dblFactor;
            double dblSampleMass;

            if (eCurrentUnits == eNewUnits)
            {
                return dblConcentrationIn;
            }

            dblSampleMass = mQuantity.SampleMass;

            dblFactor = FactorConcentration(eCurrentUnits, dblSampleMass);
            if (dblFactor < 0d)
            {
                return -1;
            }
            else
            {
                dblValue = dblConcentrationIn * dblFactor;
            }

            dblFactor = FactorConcentration(eNewUnits, dblSampleMass);
            if (dblFactor <= 0d)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertVolumeExtended(double dblVolume, uevUnitsExtendedVolumeConstants eCurrentUnits, uevUnitsExtendedVolumeConstants eNewUnits)
        {
            double dblValue, dblFactor;

            if (eCurrentUnits == eNewUnits)
            {
                return dblVolume;
            }

            dblFactor = FactorVolumeExtended(eCurrentUnits);
            if (dblFactor < 0d)
            {
                return -1;
            }
            else
            {
                dblValue = dblVolume * dblFactor;
            }

            dblFactor = FactorVolumeExtended(eNewUnits);
            if (dblFactor <= 0d)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to Moles
        /// </summary>
        /// <param name="eUnits"></param>
        /// <param name="dblSampleMass">required for mass-based units</param>
        /// <param name="dblSampleDensity">required for volume-based units</param>
        /// <returns></returns>
        private double FactorAmount(
            uamUnitsAmountConstants eUnits,
            double dblSampleMass = -1,
            double dblSampleDensity = 0d)
        {
            double dblFactor;

            if (Math.Abs(dblSampleMass) < float.Epsilon)
            {
                dblFactor = -1;
            }
            else
            {
                // Determine the Amount multiplication dblFactor
                switch (eUnits)
                {
                    case uamUnitsAmountConstants.uamMoles:
                        dblFactor = 1d;
                        break;
                    case uamUnitsAmountConstants.uamMillimoles:
                        dblFactor = 1d / 1000.0d;
                        break;
                    case uamUnitsAmountConstants.uamMicroMoles:
                        dblFactor = 1d / 1000000.0d;
                        break;
                    case uamUnitsAmountConstants.uamNanoMoles:
                        dblFactor = 1d / 1000000000.0d;
                        break;
                    case uamUnitsAmountConstants.uamPicoMoles:
                        dblFactor = 1d / 1000000000000.0d;
                        break;
                    case uamUnitsAmountConstants.uamFemtoMoles:
                        dblFactor = 1d / 1.0E+15d;
                        break;
                    case uamUnitsAmountConstants.uamAttoMoles:
                        dblFactor = 1d / 1.0E+18d;
                        break;
                    case uamUnitsAmountConstants.uamKilograms:
                        dblFactor = 1000.0d / dblSampleMass;
                        break;
                    case uamUnitsAmountConstants.uamGrams:
                        dblFactor = 1d / dblSampleMass;
                        break;
                    case uamUnitsAmountConstants.uamMilligrams:
                        dblFactor = 1d / (dblSampleMass * 1000.0d);
                        break;
                    case uamUnitsAmountConstants.uamMicrograms:
                        dblFactor = 1d / (dblSampleMass * 1000000.0d);
                        break;
                    case uamUnitsAmountConstants.uamPounds:
                        dblFactor = 1000.0d / (dblSampleMass * POUNDS_PER_KG);
                        break;
                    case uamUnitsAmountConstants.uamOunces:
                        dblFactor = 1000.0d / (dblSampleMass * POUNDS_PER_KG * 16d);
                        break;
                    case uamUnitsAmountConstants.uamLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevL);
                        break;
                    case uamUnitsAmountConstants.uamDeciLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevDL);
                        break;
                    case uamUnitsAmountConstants.uamMilliLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevML);
                        break;
                    case uamUnitsAmountConstants.uamMicroLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevUL);
                        break;
                    case uamUnitsAmountConstants.uamNanoLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevNL);
                        break;
                    case uamUnitsAmountConstants.uamPicoLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevPL);
                        break;
                    case uamUnitsAmountConstants.uamGallons:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevGallons);
                        break;
                    case uamUnitsAmountConstants.uamQuarts:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevQuarts);
                        break;
                    case uamUnitsAmountConstants.uamPints:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevPints);
                        break;
                    default:
                        dblFactor = -1;
                        break;
                }
            }

            return dblFactor;
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to M
        /// </summary>
        /// <param name="eUnits"></param>
        /// <param name="dblSampleMass">required for mass-based units</param>
        /// <returns></returns>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        private double FactorConcentration(ummcUnitsMoleMassConcentrationConstants eUnits, double dblSampleMass = 0d)
        {
            double dblFactor;

            if (Math.Abs(dblSampleMass) < float.Epsilon)
            {
                dblFactor = -1;
            }
            else
            {
                switch (eUnits)
                {
                    case ummcUnitsMoleMassConcentrationConstants.ummcMolar:
                        dblFactor = 1.0d;
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcMilliMolar:
                        dblFactor = 1d / 1000.0d;
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcMicroMolar:
                        dblFactor = 1d / 1000000.0d;
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcNanoMolar:
                        dblFactor = 1d / 1000000000.0d;
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcPicoMolar:
                        dblFactor = 1d / 1000000000000.0d;
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcFemtoMolar:
                        dblFactor = 1d / 1.0E+15d;
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcAttoMolar:
                        dblFactor = 1d / 1.0E+18d;
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcMgPerDL:
                        dblFactor = 1d / dblSampleMass / 100.0d; // 1/[(1 g / 1000 mg) * (1 / MW) * (10 dL/L)]
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcMgPerML:
                        dblFactor = 1d / dblSampleMass; // 1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcUgPerML:
                        dblFactor = 1d / (dblSampleMass * 1000.0d); // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcNgPerML:
                        dblFactor = 1d / (dblSampleMass * 1000000.0d); // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcUgPerUL:
                        dblFactor = 1d / dblSampleMass; // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                        break;
                    case ummcUnitsMoleMassConcentrationConstants.ummcNgPerUL:
                        dblFactor = 1d / (dblSampleMass * 1000.0d); // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000000 uL/L)]
                        break;
                    default:
                        dblFactor = -1;
                        break;
                }
            }

            return dblFactor;
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to mL
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        /// <remarks>An extended version of the FactorVolume function in CapillaryFlow</remarks>
        private double FactorVolumeExtended(uevUnitsExtendedVolumeConstants eUnits)
        {
            double dblFactor;

            // Note: 4 quarts per gallon, 2 pints per quart
            switch (eUnits)
            {
                case uevUnitsExtendedVolumeConstants.uevL:
                    dblFactor = 1d * 1000.0d;
                    break;
                case uevUnitsExtendedVolumeConstants.uevDL:
                    dblFactor = 1d * 100.0d;
                    break;
                case uevUnitsExtendedVolumeConstants.uevML:
                    dblFactor = 1.0d;
                    break;
                case uevUnitsExtendedVolumeConstants.uevUL:
                    dblFactor = 1d / 1000.0d;
                    break;
                case uevUnitsExtendedVolumeConstants.uevNL:
                    dblFactor = 1d / 1000000.0d;
                    break;
                case uevUnitsExtendedVolumeConstants.uevPL:
                    dblFactor = 1d / 1000000000.0d;
                    break;
                case uevUnitsExtendedVolumeConstants.uevGallons:
                    dblFactor = 1000.0d / GALLONS_PER_L;
                    break;
                case uevUnitsExtendedVolumeConstants.uevQuarts:
                    dblFactor = 1000.0d / GALLONS_PER_L / 4.0d;
                    break;
                case uevUnitsExtendedVolumeConstants.uevPints:
                    dblFactor = 1000.0d / GALLONS_PER_L / 8.0d;
                    break;
                default:
                    dblFactor = -1;
                    break;
            }

            return dblFactor;
        }


        // Get Methods
        // These retrieve the most recently computed value
        // If mAutoComputeDilution = False, must manually call a Compute Sub to recompute the value
        // Similarly, if mAutoComputeQuantity = False, must manually call a Compute Sub to recompute the value

        public bool GetAutoComputeDilutionEnabled()
        {
            return mAutoComputeDilution;
        }

        public acdAutoComputeDilutionModeConstants GetAutoComputeDilutionMode()
        {
            return mAutoComputeDilutionMode;
        }

        public bool GetAutoComputeQuantityEnabled()
        {
            return mAutoComputeQuantity;
        }

        public acqAutoComputeQuantityModeConstants GetAutoComputeQuantityMode()
        {
            return mAutoComputeQuantityMode;
        }

        public double GetDilutionFinalConcentration(ummcUnitsMoleMassConcentrationConstants eUnits = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        {
            return ConvertConcentration(mDilutionValues.FinalConcentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits);
        }

        public double GetDilutionInitialConcentration(ummcUnitsMoleMassConcentrationConstants eUnits = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        {
            return ConvertConcentration(mDilutionValues.InitialConcentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits);
        }

        public double GetDilutionTotalFinalVolume(uevUnitsExtendedVolumeConstants eUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            return ConvertVolumeExtended(mDilutionValues.TotalFinalVolume, uevUnitsExtendedVolumeConstants.uevL, eUnits);
        }

        public double GetDilutionVolumeDilutingSolvent(uevUnitsExtendedVolumeConstants eUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            return ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, uevUnitsExtendedVolumeConstants.uevL, eUnits);
        }

        public double GetDilutionVolumeStockSolution(uevUnitsExtendedVolumeConstants eUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            return ConvertVolumeExtended(mDilutionValues.StockSolutionVolume, uevUnitsExtendedVolumeConstants.uevL, eUnits);
        }

        public double GetQuantityAmount(uamUnitsAmountConstants eUnits = uamUnitsAmountConstants.uamMoles)
        {
            return ConvertAmount(mQuantity.Amount, uamUnitsAmountConstants.uamMoles, eUnits);
        }

        public double GetQuantityConcentration(ummcUnitsMoleMassConcentrationConstants eUnits = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        {
            return ConvertConcentration(mQuantity.Concentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits);
        }

        public double GetQuantityVolume(uevUnitsExtendedVolumeConstants eUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            return ConvertVolumeExtended(mQuantity.Volume, uevUnitsExtendedVolumeConstants.uevL, eUnits);
        }

        public double GetSampleDensity()
        {
            return mQuantity.SampleDensity;
        }

        public double GetSampleMass()
        {
            return mQuantity.SampleMass;
        }


        // Set Methods
        // If mAutoComputeDilution = False, must manually call a Compute Sub to recompute the value
        // Similarly, if mAutoComputeQuantity = False, must manually call a Compute Sub to recompute the value

        public void SetAutoComputeDilutionEnabled(bool blnAutoCompute)
        {
            mAutoComputeDilution = blnAutoCompute;
        }

        public void SetAutoComputeDilutionMode(acdAutoComputeDilutionModeConstants eAutoComputeMode)
        {
            if (eAutoComputeMode >= acdAutoComputeDilutionModeConstants.acdFindRequiredDilutionVolumes && eAutoComputeMode <= acdAutoComputeDilutionModeConstants.acdFindFinalConcentration)
            {
                mAutoComputeDilutionMode = eAutoComputeMode;
            }
        }

        public void SetAutoComputeQuantityEnabled(bool blnAutoCompute)
        {
            mAutoComputeQuantity = blnAutoCompute;
        }

        public void SetAutoComputeQuantityMode(acqAutoComputeQuantityModeConstants eAutoComputeMode)
        {
            if (eAutoComputeMode >= acqAutoComputeQuantityModeConstants.acqFindAmount && eAutoComputeMode <= acqAutoComputeQuantityModeConstants.acqFindConcentration)
            {
                mAutoComputeQuantityMode = eAutoComputeMode;
            }
        }

        public void SetDilutionFinalConcentration(double dblConcentration, ummcUnitsMoleMassConcentrationConstants eUnits = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        {
            mDilutionValues.FinalConcentration = ConvertConcentration(dblConcentration, eUnits, ummcUnitsMoleMassConcentrationConstants.ummcMolar);
            CheckAutoComputeDilution();
        }

        public void SetDilutionInitialConcentration(double dblConcentration, ummcUnitsMoleMassConcentrationConstants eUnits = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        {
            mDilutionValues.InitialConcentration = ConvertConcentration(dblConcentration, eUnits, ummcUnitsMoleMassConcentrationConstants.ummcMolar);
            CheckAutoComputeDilution();
        }

        public void SetDilutionTotalFinalVolume(double dblVolume, uevUnitsExtendedVolumeConstants eUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            mDilutionValues.TotalFinalVolume = ConvertVolumeExtended(dblVolume, eUnits, uevUnitsExtendedVolumeConstants.uevL);
            CheckAutoComputeDilution();
        }

        public void SetDilutionVolumeDilutingSolvent(double dblVolume, uevUnitsExtendedVolumeConstants eUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            mDilutionValues.DilutingSolventVolume = ConvertVolumeExtended(dblVolume, eUnits, uevUnitsExtendedVolumeConstants.uevL);
            CheckAutoComputeDilution();
        }

        public void SetDilutionVolumeStockSolution(double dblVolume, uevUnitsExtendedVolumeConstants eUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            mDilutionValues.StockSolutionVolume = ConvertVolumeExtended(dblVolume, eUnits, uevUnitsExtendedVolumeConstants.uevL);
            CheckAutoComputeDilution();
        }

        public void SetQuantityAmount(double dblAmount, uamUnitsAmountConstants eUnits = uamUnitsAmountConstants.uamMoles)
        {
            mQuantity.Amount = ConvertAmount(dblAmount, eUnits, uamUnitsAmountConstants.uamMoles);
            CheckAutoComputeQuantity();
        }

        public void SetQuantityConcentration(double dblConcentration, ummcUnitsMoleMassConcentrationConstants eUnits = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        {
            mQuantity.Concentration = ConvertConcentration(dblConcentration, eUnits, ummcUnitsMoleMassConcentrationConstants.ummcMolar);
            CheckAutoComputeQuantity();
        }

        public void SetQuantityVolume(double dblVolume, uevUnitsExtendedVolumeConstants eUnits = uevUnitsExtendedVolumeConstants.uevML)
        {
            mQuantity.Volume = ConvertVolumeExtended(dblVolume, eUnits, uevUnitsExtendedVolumeConstants.uevL);
        }

        public void SetSampleDensity(double dblDensityInGramsPerML)
        {
            if (dblDensityInGramsPerML >= 0d)
            {
                mQuantity.SampleDensity = dblDensityInGramsPerML;
            }
            else
            {
                mQuantity.SampleDensity = 0d;
            }

            CheckAutoComputeQuantity();
        }

        public void SetSampleMass(double dblMassInGramsPerMole)
        {
            if (dblMassInGramsPerMole >= 0d)
            {
                mQuantity.SampleMass = dblMassInGramsPerMole;
            }
            else
            {
                mQuantity.SampleMass = 0d;
            }

            CheckAutoComputeQuantity();
        }

        private void InitializeClass()
        {
            SetAutoComputeDilutionEnabled(false);
            SetAutoComputeQuantityEnabled(false);

            SetAutoComputeDilutionMode(acdAutoComputeDilutionModeConstants.acdFindRequiredDilutionVolumes);

            SetAutoComputeQuantityMode(acqAutoComputeQuantityModeConstants.acqFindConcentration);

            SetQuantityAmount(1d, uamUnitsAmountConstants.uamMoles);
            SetQuantityVolume(100d, uevUnitsExtendedVolumeConstants.uevML);
            SetQuantityConcentration(1d, ummcUnitsMoleMassConcentrationConstants.ummcMolar);

            SetDilutionInitialConcentration(10d, ummcUnitsMoleMassConcentrationConstants.ummcMolar);
            SetDilutionVolumeStockSolution(3d, uevUnitsExtendedVolumeConstants.uevML);
            SetDilutionFinalConcentration(2d, ummcUnitsMoleMassConcentrationConstants.ummcMolar);
            SetDilutionVolumeDilutingSolvent(12d, uevUnitsExtendedVolumeConstants.uevML);
            SetDilutionTotalFinalVolume(15d, uevUnitsExtendedVolumeConstants.uevML);

            // Recompute
            ComputeQuantityAmount();
            double argdblNewDilutingSolventVolume = 0d;
            this.ComputeDilutionRequiredStockAndDilutingSolventVolumes(out argdblNewDilutingSolventVolume);

            SetAutoComputeDilutionEnabled(true);
            SetAutoComputeQuantityEnabled(true);
        }

        public short AmountsUnitListCount
        {
            get
            {
                return Conversions.ToShort((int)AMOUNT_UNITS_LIST_INDEX_MAX + 1);
            }
        }

        public short AmountsUnitListVolumeIndexStart
        {
            get
            {
                return Conversions.ToShort(AMOUNT_UNITS_VOLUME_INDEX_START);
            }
        }

        public short AmountsUnitListVolumeIndexEnd
        {
            get
            {
                return (short)Conversions.ToInteger(AMOUNT_UNITS_LIST_INDEX_MAX);
            }
        }
    }
}