using System;

namespace MolecularWeightCalculator
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

        public enum AutoComputeDilutionMode
        {
            FindRequiredDilutionVolumes = 0,
            FindRequiredTotalVolume,
            FindFinalConcentration,
            FindInitialConcentration
        }

        public enum AutoComputeQuantityMode
        {
            FindAmount = 0,
            FindVolume,
            FindConcentration
        }

        private const Unit AMOUNT_UNITS_VOLUME_INDEX_START = Unit.Liters;
        private const Unit AMOUNT_UNITS_LIST_INDEX_MAX = Unit.Pints;

        public enum Unit
        {
            Moles = 0,
            Millimoles,
            MicroMoles,
            NanoMoles,
            PicoMoles,
            FemtoMoles,
            AttoMoles,
            Kilograms,
            Grams,
            Milligrams,
            Micrograms,
            Pounds,
            Ounces,
            Liters,
            DeciLiters,
            MilliLiters,
            MicroLiters,
            NanoLiters,
            PicoLiters,
            Gallons,
            Quarts,
            Pints
        }

        public enum UnitOfExtendedVolume
        {
            L = 0,
            DL,
            ML,
            UL,
            NL,
            PL,
            Gallons,
            Quarts,
            Pints
        }

        public enum UnitOfMoleMassConcentration
        {
            Molar = 0,
            MilliMolar,
            MicroMolar,
            NanoMolar,
            PicoMolar,
            FemtoMolar,
            AttoMolar,
            MgPerDL,
            MgPerML,
            UgPerML,
            NgPerML,
            UgPerUL,
            NgPerUL
        }


        #endregion

        #region "Data classes"
        private class MoleMassQuantity
        {
            public double Amount; // In Moles
            public double Volume; // In L
            public double Concentration; // In Molar
            public double SampleMass; // In g
            public double SampleDensity; // In g/mL
        }

        private class MoleMassDilutionValues
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

        private readonly MoleMassQuantity mQuantity = new MoleMassQuantity();
        private readonly MoleMassDilutionValues mDilutionValues = new MoleMassDilutionValues();

        private bool mAutoComputeDilution; // When true, automatically compute dilution results whenever any value changes
        private AutoComputeDilutionMode mAutoComputeDilutionMode; // The value to compute when mAutoComputeDilution is true

        private bool mAutoComputeQuantity; // When true, automatically compute quantities whenever any value changes
        private AutoComputeQuantityMode mAutoComputeQuantityMode; // The value to compute when mAutoComputeQuantity is true

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
                    case AutoComputeDilutionMode.FindRequiredTotalVolume:
                        ComputeDilutionTotalVolume(out _);
                        break;
                    case AutoComputeDilutionMode.FindFinalConcentration:
                        ComputeDilutionFinalConcentration();
                        break;
                    case AutoComputeDilutionMode.FindInitialConcentration:
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
                    case AutoComputeQuantityMode.FindVolume:
                        ComputeQuantityVolume();
                        break;
                    case AutoComputeQuantityMode.FindConcentration:
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
        public double ComputeDilutionFinalConcentration(UnitOfMoleMassConcentration eUnits = UnitOfMoleMassConcentration.Molar)
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

            return ConvertConcentration(mDilutionValues.FinalConcentration, UnitOfMoleMassConcentration.Molar, eUnits);
        }

        /// <summary>
        /// Computes the Initial Concentration, storing in .InitialConcentration, and returning it
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeDilutionInitialConcentration(UnitOfMoleMassConcentration eUnits = UnitOfMoleMassConcentration.Molar)
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

            return ConvertConcentration(mDilutionValues.InitialConcentration, UnitOfMoleMassConcentration.Molar, eUnits);
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
            out double dblNewDilutingSolventVolume,
            UnitOfExtendedVolume eStockSolutionUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume eDilutingSolventUnits = UnitOfExtendedVolume.ML)
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

            dblNewDilutingSolventVolume = ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, UnitOfExtendedVolume.L, eDilutingSolventUnits);

            return ConvertVolumeExtended(mDilutionValues.StockSolutionVolume, UnitOfExtendedVolume.L, eStockSolutionUnits);
        }

        /// <summary>
        /// Compute the total volume following the dilution, storing in .TotalFinalVolume, and returning it
        /// </summary>
        /// <param name="dblNewDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="eTotalVolumeUnits"></param>
        /// <param name="eDilutingSolventUnits"></param>
        /// <returns></returns>
        public double ComputeDilutionTotalVolume(
            out double dblNewDilutingSolventVolume,
            UnitOfExtendedVolume eTotalVolumeUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume eDilutingSolventUnits = UnitOfExtendedVolume.ML)
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

            dblNewDilutingSolventVolume = ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, UnitOfExtendedVolume.L, eDilutingSolventUnits);

            return ConvertVolumeExtended(mDilutionValues.TotalFinalVolume, UnitOfExtendedVolume.L, eTotalVolumeUnits);
        }

        /// <summary>
        /// Computes mQuantity.Amount using mQuantity.Volume and mQuantity.Concentration, storing the result in mQuantity.Amount
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns>mQuantity.Amount, with the specified units</returns>
        public double ComputeQuantityAmount(Unit eUnits = Unit.Moles)
        {
            mQuantity.Amount = mQuantity.Concentration * mQuantity.Volume;

            return ConvertAmount(mQuantity.Amount, Unit.Moles, eUnits);
        }

        /// <summary>
        /// Computes mQuantity.Concentration using mQuantity.Amount and mQuantity.Volume, storing the result in mQuantity.Concentration
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns>mQuantity.Concentration, with the specified units</returns>
        public double ComputeQuantityConcentration(UnitOfMoleMassConcentration eUnits = UnitOfMoleMassConcentration.Molar)
        {
            if (Math.Abs(mQuantity.Volume) > float.Epsilon)
            {
                mQuantity.Concentration = mQuantity.Amount / mQuantity.Volume;
            }
            else
            {
                mQuantity.Concentration = 0d;
            }

            return ConvertConcentration(mQuantity.Concentration, UnitOfMoleMassConcentration.Molar, eUnits);
        }

        /// <summary>
        /// Computes mQuantity.Volume using mQuantity.Amount and mQuantity.Concentration, storing the result in mQuantity.Volume
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns>mQuantity.Volume, with the specified units</returns>
        public double ComputeQuantityVolume(UnitOfExtendedVolume eUnits = UnitOfExtendedVolume.L)
        {
            if (Math.Abs(mQuantity.Concentration) > float.Epsilon)
            {
                mQuantity.Volume = mQuantity.Amount / mQuantity.Concentration;
            }
            else
            {
                mQuantity.Volume = 0d;
            }

            return ConvertVolumeExtended(mQuantity.Volume, UnitOfExtendedVolume.L, eUnits);
        }

        /// <summary>
        /// This function uses .SampleMass and .SampleDensity if the units are mass and/or volume-based
        /// </summary>
        /// <param name="dblAmountIn"></param>
        /// <param name="eCurrentUnits"></param>
        /// <param name="eNewUnits"></param>
        /// <returns></returns>
        public double ConvertAmount(double dblAmountIn, Unit eCurrentUnits, Unit eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                // No conversion, simply return dblAmountIn
                return dblAmountIn;
            }

            if (eCurrentUnits >= AMOUNT_UNITS_VOLUME_INDEX_START && eCurrentUnits <= AMOUNT_UNITS_LIST_INDEX_MAX && eNewUnits >= AMOUNT_UNITS_VOLUME_INDEX_START && eNewUnits <= AMOUNT_UNITS_LIST_INDEX_MAX)
            {
                // Converting from one volume unit to another volume unit
                // No need to explicitly specify mass or density

                var eCurrentVolumeUnits = (UnitOfExtendedVolume)((int)eCurrentUnits - (int)AMOUNT_UNITS_VOLUME_INDEX_START);
                var eNewVolumeUnits = (UnitOfExtendedVolume)((int)eNewUnits - (int)AMOUNT_UNITS_VOLUME_INDEX_START);

                return ConvertVolumeExtended(dblAmountIn, eCurrentVolumeUnits, eNewVolumeUnits);
            }

            var dblSampleMass = mQuantity.SampleMass;
            var dblSampleDensity = mQuantity.SampleDensity;

            var dblFactor = FactorAmount(eCurrentUnits, dblSampleMass, dblSampleDensity);
            if (dblFactor < 0d)
            {
                return -1;
            }

            var dblValue = dblAmountIn * dblFactor;

            dblFactor = FactorAmount(eNewUnits, dblSampleMass, dblSampleDensity);
            if (dblFactor <= 0d)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        /// <summary>
        /// Convert concentration
        /// </summary>
        /// <param name="dblConcentrationIn"></param>
        /// <param name="eCurrentUnits"></param>
        /// <param name="eNewUnits"></param>
        /// <returns></returns>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        public double ConvertConcentration(double dblConcentrationIn, UnitOfMoleMassConcentration eCurrentUnits, UnitOfMoleMassConcentration eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblConcentrationIn;
            }

            var dblSampleMass = mQuantity.SampleMass;

            var dblFactor = FactorConcentration(eCurrentUnits, dblSampleMass);
            if (dblFactor < 0d)
            {
                return -1;
            }

            var dblValue = dblConcentrationIn * dblFactor;

            dblFactor = FactorConcentration(eNewUnits, dblSampleMass);
            if (dblFactor <= 0d)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertVolumeExtended(double dblVolume, UnitOfExtendedVolume eCurrentUnits, UnitOfExtendedVolume eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblVolume;
            }

            var dblFactor = FactorVolumeExtended(eCurrentUnits);
            if (dblFactor < 0d)
            {
                return -1;
            }

            var dblValue = dblVolume * dblFactor;

            dblFactor = FactorVolumeExtended(eNewUnits);
            if (dblFactor <= 0d)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to Moles
        /// </summary>
        /// <param name="eUnits"></param>
        /// <param name="dblSampleMass">required for mass-based units</param>
        /// <param name="dblSampleDensity">required for volume-based units</param>
        /// <returns></returns>
        private double FactorAmount(
            Unit eUnits,
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
                    case Unit.Moles:
                        dblFactor = 1d;
                        break;
                    case Unit.Millimoles:
                        dblFactor = 1d / 1000.0d;
                        break;
                    case Unit.MicroMoles:
                        dblFactor = 1d / 1000000.0d;
                        break;
                    case Unit.NanoMoles:
                        dblFactor = 1d / 1000000000.0d;
                        break;
                    case Unit.PicoMoles:
                        dblFactor = 1d / 1000000000000.0d;
                        break;
                    case Unit.FemtoMoles:
                        dblFactor = 1d / 1.0E+15d;
                        break;
                    case Unit.AttoMoles:
                        dblFactor = 1d / 1.0E+18d;
                        break;
                    case Unit.Kilograms:
                        dblFactor = 1000.0d / dblSampleMass;
                        break;
                    case Unit.Grams:
                        dblFactor = 1d / dblSampleMass;
                        break;
                    case Unit.Milligrams:
                        dblFactor = 1d / (dblSampleMass * 1000.0d);
                        break;
                    case Unit.Micrograms:
                        dblFactor = 1d / (dblSampleMass * 1000000.0d);
                        break;
                    case Unit.Pounds:
                        dblFactor = 1000.0d / (dblSampleMass * POUNDS_PER_KG);
                        break;
                    case Unit.Ounces:
                        dblFactor = 1000.0d / (dblSampleMass * POUNDS_PER_KG * 16d);
                        break;
                    case Unit.Liters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(UnitOfExtendedVolume.L);
                        break;
                    case Unit.DeciLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(UnitOfExtendedVolume.DL);
                        break;
                    case Unit.MilliLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(UnitOfExtendedVolume.ML);
                        break;
                    case Unit.MicroLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(UnitOfExtendedVolume.UL);
                        break;
                    case Unit.NanoLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(UnitOfExtendedVolume.NL);
                        break;
                    case Unit.PicoLiters:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(UnitOfExtendedVolume.PL);
                        break;
                    case Unit.Gallons:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(UnitOfExtendedVolume.Gallons);
                        break;
                    case Unit.Quarts:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(UnitOfExtendedVolume.Quarts);
                        break;
                    case Unit.Pints:
                        dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(UnitOfExtendedVolume.Pints);
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
        private double FactorConcentration(UnitOfMoleMassConcentration eUnits, double dblSampleMass = 0d)
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
                    case UnitOfMoleMassConcentration.Molar:
                        dblFactor = 1.0d;
                        break;
                    case UnitOfMoleMassConcentration.MilliMolar:
                        dblFactor = 1d / 1000.0d;
                        break;
                    case UnitOfMoleMassConcentration.MicroMolar:
                        dblFactor = 1d / 1000000.0d;
                        break;
                    case UnitOfMoleMassConcentration.NanoMolar:
                        dblFactor = 1d / 1000000000.0d;
                        break;
                    case UnitOfMoleMassConcentration.PicoMolar:
                        dblFactor = 1d / 1000000000000.0d;
                        break;
                    case UnitOfMoleMassConcentration.FemtoMolar:
                        dblFactor = 1d / 1.0E+15d;
                        break;
                    case UnitOfMoleMassConcentration.AttoMolar:
                        dblFactor = 1d / 1.0E+18d;
                        break;
                    case UnitOfMoleMassConcentration.MgPerDL:
                        dblFactor = 1d / dblSampleMass / 100.0d; // 1/[(1 g / 1000 mg) * (1 / MW) * (10 dL/L)]
                        break;
                    case UnitOfMoleMassConcentration.MgPerML:
                        dblFactor = 1d / dblSampleMass; // 1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                        break;
                    case UnitOfMoleMassConcentration.UgPerML:
                        dblFactor = 1d / (dblSampleMass * 1000.0d); // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                        break;
                    case UnitOfMoleMassConcentration.NgPerML:
                        dblFactor = 1d / (dblSampleMass * 1000000.0d); // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                        break;
                    case UnitOfMoleMassConcentration.UgPerUL:
                        dblFactor = 1d / dblSampleMass; // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                        break;
                    case UnitOfMoleMassConcentration.NgPerUL:
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
        private double FactorVolumeExtended(UnitOfExtendedVolume eUnits)
        {
            double dblFactor;

            // Note: 4 quarts per gallon, 2 pints per quart
            switch (eUnits)
            {
                case UnitOfExtendedVolume.L:
                    dblFactor = 1d * 1000.0d;
                    break;
                case UnitOfExtendedVolume.DL:
                    dblFactor = 1d * 100.0d;
                    break;
                case UnitOfExtendedVolume.ML:
                    dblFactor = 1.0d;
                    break;
                case UnitOfExtendedVolume.UL:
                    dblFactor = 1d / 1000.0d;
                    break;
                case UnitOfExtendedVolume.NL:
                    dblFactor = 1d / 1000000.0d;
                    break;
                case UnitOfExtendedVolume.PL:
                    dblFactor = 1d / 1000000000.0d;
                    break;
                case UnitOfExtendedVolume.Gallons:
                    dblFactor = 1000.0d / GALLONS_PER_L;
                    break;
                case UnitOfExtendedVolume.Quarts:
                    dblFactor = 1000.0d / GALLONS_PER_L / 4.0d;
                    break;
                case UnitOfExtendedVolume.Pints:
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

        public AutoComputeDilutionMode GetAutoComputeDilutionMode()
        {
            return mAutoComputeDilutionMode;
        }

        public bool GetAutoComputeQuantityEnabled()
        {
            return mAutoComputeQuantity;
        }

        public AutoComputeQuantityMode GetAutoComputeQuantityMode()
        {
            return mAutoComputeQuantityMode;
        }

        public double GetDilutionFinalConcentration(UnitOfMoleMassConcentration eUnits = UnitOfMoleMassConcentration.Molar)
        {
            return ConvertConcentration(mDilutionValues.FinalConcentration, UnitOfMoleMassConcentration.Molar, eUnits);
        }

        public double GetDilutionInitialConcentration(UnitOfMoleMassConcentration eUnits = UnitOfMoleMassConcentration.Molar)
        {
            return ConvertConcentration(mDilutionValues.InitialConcentration, UnitOfMoleMassConcentration.Molar, eUnits);
        }

        public double GetDilutionTotalFinalVolume(UnitOfExtendedVolume eUnits = UnitOfExtendedVolume.ML)
        {
            return ConvertVolumeExtended(mDilutionValues.TotalFinalVolume, UnitOfExtendedVolume.L, eUnits);
        }

        public double GetDilutionVolumeDilutingSolvent(UnitOfExtendedVolume eUnits = UnitOfExtendedVolume.ML)
        {
            return ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, UnitOfExtendedVolume.L, eUnits);
        }

        public double GetDilutionVolumeStockSolution(UnitOfExtendedVolume eUnits = UnitOfExtendedVolume.ML)
        {
            return ConvertVolumeExtended(mDilutionValues.StockSolutionVolume, UnitOfExtendedVolume.L, eUnits);
        }

        public double GetQuantityAmount(Unit eUnits = Unit.Moles)
        {
            return ConvertAmount(mQuantity.Amount, Unit.Moles, eUnits);
        }

        public double GetQuantityConcentration(UnitOfMoleMassConcentration eUnits = UnitOfMoleMassConcentration.Molar)
        {
            return ConvertConcentration(mQuantity.Concentration, UnitOfMoleMassConcentration.Molar, eUnits);
        }

        public double GetQuantityVolume(UnitOfExtendedVolume eUnits = UnitOfExtendedVolume.ML)
        {
            return ConvertVolumeExtended(mQuantity.Volume, UnitOfExtendedVolume.L, eUnits);
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

        public void SetAutoComputeDilutionMode(AutoComputeDilutionMode eAutoComputeMode)
        {
            if (eAutoComputeMode >= AutoComputeDilutionMode.FindRequiredDilutionVolumes && eAutoComputeMode <= AutoComputeDilutionMode.FindFinalConcentration)
            {
                mAutoComputeDilutionMode = eAutoComputeMode;
            }
        }

        public void SetAutoComputeQuantityEnabled(bool blnAutoCompute)
        {
            mAutoComputeQuantity = blnAutoCompute;
        }

        public void SetAutoComputeQuantityMode(AutoComputeQuantityMode eAutoComputeMode)
        {
            if (eAutoComputeMode >= AutoComputeQuantityMode.FindAmount && eAutoComputeMode <= AutoComputeQuantityMode.FindConcentration)
            {
                mAutoComputeQuantityMode = eAutoComputeMode;
            }
        }

        public void SetDilutionFinalConcentration(double dblConcentration, UnitOfMoleMassConcentration eUnits = UnitOfMoleMassConcentration.Molar)
        {
            mDilutionValues.FinalConcentration = ConvertConcentration(dblConcentration, eUnits, UnitOfMoleMassConcentration.Molar);
            CheckAutoComputeDilution();
        }

        public void SetDilutionInitialConcentration(double dblConcentration, UnitOfMoleMassConcentration eUnits = UnitOfMoleMassConcentration.Molar)
        {
            mDilutionValues.InitialConcentration = ConvertConcentration(dblConcentration, eUnits, UnitOfMoleMassConcentration.Molar);
            CheckAutoComputeDilution();
        }

        public void SetDilutionTotalFinalVolume(double dblVolume, UnitOfExtendedVolume eUnits = UnitOfExtendedVolume.ML)
        {
            mDilutionValues.TotalFinalVolume = ConvertVolumeExtended(dblVolume, eUnits, UnitOfExtendedVolume.L);
            CheckAutoComputeDilution();
        }

        public void SetDilutionVolumeDilutingSolvent(double dblVolume, UnitOfExtendedVolume eUnits = UnitOfExtendedVolume.ML)
        {
            mDilutionValues.DilutingSolventVolume = ConvertVolumeExtended(dblVolume, eUnits, UnitOfExtendedVolume.L);
            CheckAutoComputeDilution();
        }

        public void SetDilutionVolumeStockSolution(double dblVolume, UnitOfExtendedVolume eUnits = UnitOfExtendedVolume.ML)
        {
            mDilutionValues.StockSolutionVolume = ConvertVolumeExtended(dblVolume, eUnits, UnitOfExtendedVolume.L);
            CheckAutoComputeDilution();
        }

        public void SetQuantityAmount(double dblAmount, Unit eUnits = Unit.Moles)
        {
            mQuantity.Amount = ConvertAmount(dblAmount, eUnits, Unit.Moles);
            CheckAutoComputeQuantity();
        }

        public void SetQuantityConcentration(double dblConcentration, UnitOfMoleMassConcentration eUnits = UnitOfMoleMassConcentration.Molar)
        {
            mQuantity.Concentration = ConvertConcentration(dblConcentration, eUnits, UnitOfMoleMassConcentration.Molar);
            CheckAutoComputeQuantity();
        }

        public void SetQuantityVolume(double dblVolume, UnitOfExtendedVolume eUnits = UnitOfExtendedVolume.ML)
        {
            mQuantity.Volume = ConvertVolumeExtended(dblVolume, eUnits, UnitOfExtendedVolume.L);
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

            SetAutoComputeDilutionMode(AutoComputeDilutionMode.FindRequiredDilutionVolumes);

            SetAutoComputeQuantityMode(AutoComputeQuantityMode.FindConcentration);

            SetQuantityAmount(1d, Unit.Moles);
            SetQuantityVolume(100d, UnitOfExtendedVolume.ML);
            SetQuantityConcentration(1d, UnitOfMoleMassConcentration.Molar);

            SetDilutionInitialConcentration(10d, UnitOfMoleMassConcentration.Molar);
            SetDilutionVolumeStockSolution(3d, UnitOfExtendedVolume.ML);
            SetDilutionFinalConcentration(2d, UnitOfMoleMassConcentration.Molar);
            SetDilutionVolumeDilutingSolvent(12d, UnitOfExtendedVolume.ML);
            SetDilutionTotalFinalVolume(15d, UnitOfExtendedVolume.ML);

            // Recompute
            ComputeQuantityAmount();
            ComputeDilutionRequiredStockAndDilutingSolventVolumes(out _);

            SetAutoComputeDilutionEnabled(true);
            SetAutoComputeQuantityEnabled(true);
        }

        public short AmountsUnitListCount => (short)AMOUNT_UNITS_LIST_INDEX_MAX + 1;

        public short AmountsUnitListVolumeIndexStart => (short)AMOUNT_UNITS_VOLUME_INDEX_START;

        public short AmountsUnitListVolumeIndexEnd => (short)AMOUNT_UNITS_LIST_INDEX_MAX;
    }
}