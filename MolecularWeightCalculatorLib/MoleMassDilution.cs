using System;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator
{
    [Guid("FD3FADF4-C3E4-4C4C-AF7A-AF7018FC3DBB"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class MoleMassDilution : IMoleMassDilution
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: MoleMassDilution

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2002
        // Converted to C# by Bryson Gibbons in 2021
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

        // Ignore Spelling: Bryson

        public MoleMassDilution()
        {
            // Ignore Spelling: ug, ng

            SetAutoComputeDilutionEnabled(false);
            SetAutoComputeQuantityEnabled(false);

            SetAutoComputeDilutionMode(AutoComputeDilutionMode.FindRequiredDilutionVolumes);

            SetAutoComputeQuantityMode(AutoComputeQuantityMode.FindConcentration);

            // ReSharper disable RedundantArgumentDefaultValue

            SetQuantityAmount(1d, Unit.Moles);
            SetQuantityVolume(100d, UnitOfExtendedVolume.ML);
            SetQuantityConcentration(1d, UnitOfMoleMassConcentration.Molar);

            SetDilutionInitialConcentration(10d, UnitOfMoleMassConcentration.Molar);
            SetDilutionVolumeStockSolution(3d, UnitOfExtendedVolume.ML);
            SetDilutionFinalConcentration(2d, UnitOfMoleMassConcentration.Molar);
            SetDilutionVolumeDilutingSolvent(12d, UnitOfExtendedVolume.ML);
            SetDilutionTotalFinalVolume(15d, UnitOfExtendedVolume.ML);

            // ReSharper restore RedundantArgumentDefaultValue

            // Recompute
            ComputeQuantityAmount();
            ComputeDilutionRequiredStockAndDilutingSolventVolumes(out _);

            SetAutoComputeDilutionEnabled(true);
            SetAutoComputeQuantityEnabled(true);
        }

        private const Unit AMOUNT_UNITS_VOLUME_INDEX_START = Unit.Liters;
        private const Unit AMOUNT_UNITS_LIST_INDEX_MAX = Unit.Pints;

        #region "Data classes"
        private class MoleMassQuantity
        {
            public double Amount { get; set; } // In Moles
            public double Volume { get; set; } // In L
            public double Concentration { get; set; } // In Molar
            public double SampleMass { get; set; } // In g
            public double SampleDensity { get; set; } // In g/mL
        }

        private class MoleMassDilutionValues
        {
            public double InitialConcentration { get; set; } // In Molar
            public double StockSolutionVolume { get; set; } // In L
            public double FinalConcentration { get; set; } // In Molar
            public double DilutingSolventVolume { get; set; } // In L
            public double TotalFinalVolume { get; set; } // In L
        }

        #endregion

        private const float POUNDS_PER_KG = 2.20462262f;
        private const float GALLONS_PER_L = 0.264172052f;

        private readonly MoleMassQuantity mQuantity = new();
        private readonly MoleMassDilutionValues mDilutionValues = new();

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
                        // Includes FindRequiredDilutionVolumes
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
                        // Includes FindAmount
                        ComputeQuantityAmount();
                        break;
                }
            }
        }

        /// <summary>
        /// Computes the Final Concentration, storing in .FinalConcentration, and returning it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeDilutionFinalConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
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

            return ConvertConcentration(mDilutionValues.FinalConcentration, UnitOfMoleMassConcentration.Molar, units);
        }

        /// <summary>
        /// Computes the Initial Concentration, storing in .InitialConcentration, and returning it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeDilutionInitialConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
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

            return ConvertConcentration(mDilutionValues.InitialConcentration, UnitOfMoleMassConcentration.Molar, units);
        }

        /// <summary>
        /// Computes the required dilution volumes using initial concentration, final concentration
        /// and total final volume, storing in .StockSolutionVolume and .DilutingSolventVolume,
        /// and returning .StockSolutionVolume
        /// </summary>
        /// <param name="newDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="stockSolutionUnits"></param>
        /// <param name="dilutingSolventUnits"></param>
        public double ComputeDilutionRequiredStockAndDilutingSolventVolumes(
            out double newDilutingSolventVolume,
            UnitOfExtendedVolume stockSolutionUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume dilutingSolventUnits = UnitOfExtendedVolume.ML)
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

            newDilutingSolventVolume = ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, UnitOfExtendedVolume.L, dilutingSolventUnits);

            return ConvertVolumeExtended(mDilutionValues.StockSolutionVolume, UnitOfExtendedVolume.L, stockSolutionUnits);
        }

        /// <summary>
        /// Compute the total volume following the dilution, storing in .TotalFinalVolume, and returning it
        /// </summary>
        /// <param name="newDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="totalVolumeUnits"></param>
        /// <param name="dilutingSolventUnits"></param>
        public double ComputeDilutionTotalVolume(
            out double newDilutingSolventVolume,
            UnitOfExtendedVolume totalVolumeUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume dilutingSolventUnits = UnitOfExtendedVolume.ML)
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

            newDilutingSolventVolume = ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, UnitOfExtendedVolume.L, dilutingSolventUnits);

            return ConvertVolumeExtended(mDilutionValues.TotalFinalVolume, UnitOfExtendedVolume.L, totalVolumeUnits);
        }

        /// <summary>
        /// Computes mQuantity.Amount using mQuantity.Volume and mQuantity.Concentration, storing the result in mQuantity.Amount
        /// </summary>
        /// <param name="units"></param>
        /// <returns>mQuantity.Amount, with the specified units</returns>
        public double ComputeQuantityAmount(Unit units = Unit.Moles)
        {
            mQuantity.Amount = mQuantity.Concentration * mQuantity.Volume;

            return ConvertAmount(mQuantity.Amount, Unit.Moles, units);
        }

        /// <summary>
        /// Computes mQuantity.Concentration using mQuantity.Amount and mQuantity.Volume, storing the result in mQuantity.Concentration
        /// </summary>
        /// <param name="units"></param>
        /// <returns>mQuantity.Concentration, with the specified units</returns>
        public double ComputeQuantityConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            if (Math.Abs(mQuantity.Volume) > float.Epsilon)
            {
                mQuantity.Concentration = mQuantity.Amount / mQuantity.Volume;
            }
            else
            {
                mQuantity.Concentration = 0d;
            }

            return ConvertConcentration(mQuantity.Concentration, UnitOfMoleMassConcentration.Molar, units);
        }

        /// <summary>
        /// Computes mQuantity.Volume using mQuantity.Amount and mQuantity.Concentration, storing the result in mQuantity.Volume
        /// </summary>
        /// <param name="units"></param>
        /// <returns>mQuantity.Volume, with the specified units</returns>
        public double ComputeQuantityVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.L)
        {
            if (Math.Abs(mQuantity.Concentration) > float.Epsilon)
            {
                mQuantity.Volume = mQuantity.Amount / mQuantity.Concentration;
            }
            else
            {
                mQuantity.Volume = 0d;
            }

            return ConvertVolumeExtended(mQuantity.Volume, UnitOfExtendedVolume.L, units);
        }

        /// <summary>
        /// This function uses .SampleMass and .SampleDensity if the units are mass and/or volume-based
        /// </summary>
        /// <param name="amountIn"></param>
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        public double ConvertAmount(double amountIn, Unit currentUnits, Unit newUnits)
        {
            if (currentUnits == newUnits)
            {
                // No conversion, simply return amountIn
                return amountIn;
            }

            if (currentUnits is >= AMOUNT_UNITS_VOLUME_INDEX_START and <= AMOUNT_UNITS_LIST_INDEX_MAX &&
                newUnits is >= AMOUNT_UNITS_VOLUME_INDEX_START and <= AMOUNT_UNITS_LIST_INDEX_MAX)
            {
                // Converting from one volume unit to another volume unit
                // No need to explicitly specify mass or density

                var currentVolumeUnits = (UnitOfExtendedVolume)((int)currentUnits - (int)AMOUNT_UNITS_VOLUME_INDEX_START);
                var newVolumeUnits = (UnitOfExtendedVolume)((int)newUnits - (int)AMOUNT_UNITS_VOLUME_INDEX_START);

                return ConvertVolumeExtended(amountIn, currentVolumeUnits, newVolumeUnits);
            }

            var sampleMass = mQuantity.SampleMass;
            var sampleDensity = mQuantity.SampleDensity;

            var factor = FactorAmount(currentUnits, sampleMass, sampleDensity);
            if (factor < 0d)
            {
                return -1;
            }

            var value = amountIn * factor;

            factor = FactorAmount(newUnits, sampleMass, sampleDensity);
            if (factor <= 0d)
            {
                return -1;
            }

            return value / factor;
        }

        /// <summary>
        /// Convert concentration
        /// </summary>
        /// <param name="concentrationIn"></param>
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        public double ConvertConcentration(double concentrationIn, UnitOfMoleMassConcentration currentUnits, UnitOfMoleMassConcentration newUnits)
        {
            if (currentUnits == newUnits)
            {
                return concentrationIn;
            }

            var sampleMass = mQuantity.SampleMass;

            var factor = FactorConcentration(currentUnits, sampleMass);
            if (factor < 0d)
            {
                return -1;
            }

            var value = concentrationIn * factor;

            factor = FactorConcentration(newUnits, sampleMass);
            if (factor <= 0d)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertVolumeExtended(double volume, UnitOfExtendedVolume currentUnits, UnitOfExtendedVolume newUnits)
        {
            if (currentUnits == newUnits)
            {
                return volume;
            }

            var factor = FactorVolumeExtended(currentUnits);
            if (factor < 0d)
            {
                return -1;
            }

            var value = volume * factor;

            factor = FactorVolumeExtended(newUnits);
            if (factor <= 0d)
            {
                return -1;
            }

            return value / factor;
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to Moles
        /// </summary>
        /// <param name="units"></param>
        /// <param name="sampleMass">required for mass-based units</param>
        /// <param name="sampleDensity">required for volume-based units</param>
        private double FactorAmount(
            Unit units,
            double sampleMass = -1,
            double sampleDensity = 0d)
        {
            if (Math.Abs(sampleMass) < float.Epsilon)
            {
                return -1;
            }

            // Determine the Amount multiplication factor
            return units switch
            {
                Unit.Moles => 1d,
                Unit.Millimoles => 1d / 1000.0d,
                Unit.MicroMoles => 1d / 1000000.0d,
                Unit.NanoMoles => 1d / 1000000000.0d,
                Unit.PicoMoles => 1d / 1000000000000.0d,
                Unit.FemtoMoles => 1d / 1.0E+15d,
                Unit.AttoMoles => 1d / 1.0E+18d,
                Unit.Kilograms => 1000.0d / sampleMass,
                Unit.Grams => 1d / sampleMass,
                Unit.Milligrams => 1d / (sampleMass * 1000.0d),
                Unit.Micrograms => 1d / (sampleMass * 1000000.0d),
                Unit.Pounds => 1000.0d / (sampleMass * POUNDS_PER_KG),
                Unit.Ounces => 1000.0d / (sampleMass * POUNDS_PER_KG * 16d),
                Unit.Liters => sampleDensity / sampleMass * FactorVolumeExtended(UnitOfExtendedVolume.L),
                Unit.DeciLiters => sampleDensity / sampleMass * FactorVolumeExtended(UnitOfExtendedVolume.DL),
                Unit.MilliLiters => sampleDensity / sampleMass * FactorVolumeExtended(UnitOfExtendedVolume.ML),
                Unit.MicroLiters => sampleDensity / sampleMass * FactorVolumeExtended(UnitOfExtendedVolume.UL),
                Unit.NanoLiters => sampleDensity / sampleMass * FactorVolumeExtended(UnitOfExtendedVolume.NL),
                Unit.PicoLiters => sampleDensity / sampleMass * FactorVolumeExtended(UnitOfExtendedVolume.PL),
                Unit.Gallons => sampleDensity / sampleMass * FactorVolumeExtended(UnitOfExtendedVolume.Gallons),
                Unit.Quarts => sampleDensity / sampleMass * FactorVolumeExtended(UnitOfExtendedVolume.Quarts),
                Unit.Pints => sampleDensity / sampleMass * FactorVolumeExtended(UnitOfExtendedVolume.Pints),
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to M
        /// </summary>
        /// <param name="units"></param>
        /// <param name="sampleMass">required for mass-based units</param>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        private double FactorConcentration(UnitOfMoleMassConcentration units, double sampleMass = 0d)
        {
            if (Math.Abs(sampleMass) < float.Epsilon)
            {
               return -1;
            }

            return units switch
            {
                UnitOfMoleMassConcentration.Molar => 1.0d,
                UnitOfMoleMassConcentration.MilliMolar => 1d / 1000.0d,
                UnitOfMoleMassConcentration.MicroMolar => 1d / 1000000.0d,
                UnitOfMoleMassConcentration.NanoMolar => 1d / 1000000000.0d,
                UnitOfMoleMassConcentration.PicoMolar => 1d / 1000000000000.0d,
                UnitOfMoleMassConcentration.FemtoMolar => 1d / 1.0E+15d,
                UnitOfMoleMassConcentration.AttoMolar => 1d / 1.0E+18d,
                UnitOfMoleMassConcentration.MgPerDL => 1d / sampleMass / 100.0d, // 1/[(1 g / 1000 mg) * (1 / MW) * (10 dL/L)]
                UnitOfMoleMassConcentration.MgPerML => 1d / sampleMass, // 1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                UnitOfMoleMassConcentration.UgPerML => 1d / (sampleMass * 1000.0d), // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                UnitOfMoleMassConcentration.NgPerML => 1d / (sampleMass * 1000000.0d), // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                UnitOfMoleMassConcentration.UgPerUL => 1d / sampleMass, // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                UnitOfMoleMassConcentration.NgPerUL => 1d / (sampleMass * 1000.0d), // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000000 uL/L)]
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to mL
        /// </summary>
        /// <param name="units"></param>
        /// <remarks>An extended version of the FactorVolume function in CapillaryFlow</remarks>
        private double FactorVolumeExtended(UnitOfExtendedVolume units)
        {
            // Note: 4 quarts per gallon, 2 pints per quart
            var factor = units switch
            {
                UnitOfExtendedVolume.L => 1d * 1000.0d,
                UnitOfExtendedVolume.DL => 1d * 100.0d,
                UnitOfExtendedVolume.ML => 1.0d,
                UnitOfExtendedVolume.UL => 1d / 1000.0d,
                UnitOfExtendedVolume.NL => 1d / 1000000.0d,
                UnitOfExtendedVolume.PL => 1d / 1000000000.0d,
                UnitOfExtendedVolume.Gallons => 1000.0d / GALLONS_PER_L,
                UnitOfExtendedVolume.Quarts => 1000.0d / GALLONS_PER_L / 4.0d,
                UnitOfExtendedVolume.Pints => 1000.0d / GALLONS_PER_L / 8.0d,
                _ => -1
            };

            return factor;
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

        public double GetDilutionFinalConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return ConvertConcentration(mDilutionValues.FinalConcentration, UnitOfMoleMassConcentration.Molar, units);
        }

        public double GetDilutionInitialConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return ConvertConcentration(mDilutionValues.InitialConcentration, UnitOfMoleMassConcentration.Molar, units);
        }

        public double GetDilutionTotalFinalVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return ConvertVolumeExtended(mDilutionValues.TotalFinalVolume, UnitOfExtendedVolume.L, units);
        }

        public double GetDilutionVolumeDilutingSolvent(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, UnitOfExtendedVolume.L, units);
        }

        public double GetDilutionVolumeStockSolution(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return ConvertVolumeExtended(mDilutionValues.StockSolutionVolume, UnitOfExtendedVolume.L, units);
        }

        public double GetQuantityAmount(Unit units = Unit.Moles)
        {
            return ConvertAmount(mQuantity.Amount, Unit.Moles, units);
        }

        public double GetQuantityConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return ConvertConcentration(mQuantity.Concentration, UnitOfMoleMassConcentration.Molar, units);
        }

        public double GetQuantityVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return ConvertVolumeExtended(mQuantity.Volume, UnitOfExtendedVolume.L, units);
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

        public void SetAutoComputeDilutionEnabled(bool autoCompute)
        {
            mAutoComputeDilution = autoCompute;
        }

        public void SetAutoComputeDilutionMode(AutoComputeDilutionMode autoComputeMode)
        {
            if (autoComputeMode is >= AutoComputeDilutionMode.FindRequiredDilutionVolumes and <= AutoComputeDilutionMode.FindFinalConcentration)
            {
                mAutoComputeDilutionMode = autoComputeMode;
            }
        }

        public void SetAutoComputeQuantityEnabled(bool autoCompute)
        {
            mAutoComputeQuantity = autoCompute;
        }

        public void SetAutoComputeQuantityMode(AutoComputeQuantityMode autoComputeMode)
        {
            mAutoComputeQuantityMode = autoComputeMode;
        }

        public void SetDilutionFinalConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            mDilutionValues.FinalConcentration = ConvertConcentration(concentration, units, UnitOfMoleMassConcentration.Molar);
            CheckAutoComputeDilution();
        }

        public void SetDilutionInitialConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            mDilutionValues.InitialConcentration = ConvertConcentration(concentration, units, UnitOfMoleMassConcentration.Molar);
            CheckAutoComputeDilution();
        }

        public void SetDilutionTotalFinalVolume(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mDilutionValues.TotalFinalVolume = ConvertVolumeExtended(volume, units, UnitOfExtendedVolume.L);
            CheckAutoComputeDilution();
        }

        public void SetDilutionVolumeDilutingSolvent(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mDilutionValues.DilutingSolventVolume = ConvertVolumeExtended(volume, units, UnitOfExtendedVolume.L);
            CheckAutoComputeDilution();
        }

        public void SetDilutionVolumeStockSolution(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mDilutionValues.StockSolutionVolume = ConvertVolumeExtended(volume, units, UnitOfExtendedVolume.L);
            CheckAutoComputeDilution();
        }

        public void SetQuantityAmount(double amount, Unit units = Unit.Moles)
        {
            mQuantity.Amount = ConvertAmount(amount, units, Unit.Moles);
            CheckAutoComputeQuantity();
        }

        public void SetQuantityConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            mQuantity.Concentration = ConvertConcentration(concentration, units, UnitOfMoleMassConcentration.Molar);
            CheckAutoComputeQuantity();
        }

        public void SetQuantityVolume(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mQuantity.Volume = ConvertVolumeExtended(volume, units, UnitOfExtendedVolume.L);
        }

        // ReSharper disable once InconsistentNaming
        public void SetSampleDensity(double densityInGramsPerML)
        {
            if (densityInGramsPerML >= 0d)
            {
                mQuantity.SampleDensity = densityInGramsPerML;
            }
            else
            {
                mQuantity.SampleDensity = 0d;
            }

            CheckAutoComputeQuantity();
        }

        public void SetSampleMass(double massInGramsPerMole)
        {
            if (massInGramsPerMole >= 0d)
            {
                mQuantity.SampleMass = massInGramsPerMole;
            }
            else
            {
                mQuantity.SampleMass = 0d;
            }

            CheckAutoComputeQuantity();
        }

        public short AmountsUnitListCount => (short)AMOUNT_UNITS_LIST_INDEX_MAX + 1;

        public short AmountsUnitListVolumeIndexStart => (short)AMOUNT_UNITS_VOLUME_INDEX_START;

        public short AmountsUnitListVolumeIndexEnd => (short)AMOUNT_UNITS_LIST_INDEX_MAX;
    }
}