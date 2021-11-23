using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.MoleMassDilutionTools
{
    [ComVisible(false)]
    public static class UnitConversions
    {
        // Ignore Spelling: ug, ng

        private const Unit AMOUNT_UNITS_VOLUME_INDEX_START = Unit.Liters;
        private const Unit AMOUNT_UNITS_LIST_INDEX_MAX = Unit.Pints;

        private const float POUNDS_PER_KG = 2.20462262f;
        private const float GALLONS_PER_L = 0.264172052f;

        public static short AmountsUnitListCount => (short)AMOUNT_UNITS_LIST_INDEX_MAX + 1;

        public static short AmountsUnitListVolumeIndexStart => (short)AMOUNT_UNITS_VOLUME_INDEX_START;

        public static short AmountsUnitListVolumeIndexEnd => (short)AMOUNT_UNITS_LIST_INDEX_MAX;

        /// <summary>
        /// This method uses <paramref name="sampleMassGrams"/> and <paramref name="sampleDensityGramsPermL"/>
        /// if the units are mass and/or volume-based
        /// </summary>
        /// <param name="amountIn"></param>re
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        /// <param name="sampleMassGrams">sample mass in grams</param>
        /// <param name="sampleDensityGramsPermL">sample density in g/mL</param>
        public static double ConvertAmount(double amountIn, Unit currentUnits, Unit newUnits, double sampleMassGrams, double sampleDensityGramsPermL)
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

            var factor = FactorAmount(currentUnits, sampleMassGrams, sampleDensityGramsPermL);
            if (factor < 0)
            {
                return -1;
            }

            var value = amountIn * factor;

            factor = FactorAmount(newUnits, sampleMassGrams, sampleDensityGramsPermL);
            if (factor <= 0)
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
        /// <param name="sampleMassGrams">sample mass in grams</param>
        public static double ConvertConcentration(double concentrationIn, UnitOfMoleMassConcentration currentUnits, UnitOfMoleMassConcentration newUnits, double sampleMassGrams)
        {
            if (currentUnits == newUnits)
            {
                return concentrationIn;
            }

            var factor = FactorConcentration(currentUnits, sampleMassGrams);
            if (factor < 0)
            {
                return -1;
            }

            var value = concentrationIn * factor;

            factor = FactorConcentration(newUnits, sampleMassGrams);
            if (factor <= 0)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertVolumeExtended(double volume, UnitOfExtendedVolume currentUnits, UnitOfExtendedVolume newUnits)
        {
            if (currentUnits == newUnits)
            {
                return volume;
            }

            var factor = FactorVolumeExtended(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = volume * factor;

            factor = FactorVolumeExtended(newUnits);
            if (factor <= 0)
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
        private static double FactorAmount(
            Unit units,
            double sampleMass = -1,
            double sampleDensity = 0)
        {
            if (Math.Abs(sampleMass) < float.Epsilon)
            {
                return -1;
            }

            // Determine the Amount multiplication factor
            return units switch
            {
                Unit.Moles => 1,
                Unit.Millimoles => 1 / 1000.0,
                Unit.MicroMoles => 1 / 1000000.0,
                Unit.NanoMoles => 1 / 1000000000.0,
                Unit.PicoMoles => 1 / 1000000000000.0,
                Unit.FemtoMoles => 1 / 1.0E+15,
                Unit.AttoMoles => 1 / 1.0E+18,
                Unit.Kilograms => 1000.0 / sampleMass,
                Unit.Grams => 1 / sampleMass,
                Unit.Milligrams => 1 / (sampleMass * 1000.0),
                Unit.Micrograms => 1 / (sampleMass * 1000000.0),
                Unit.Pounds => 1000.0 / (sampleMass * POUNDS_PER_KG),
                Unit.Ounces => 1000.0 / (sampleMass * POUNDS_PER_KG * 16),
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
        private static double FactorConcentration(UnitOfMoleMassConcentration units, double sampleMass = 0)
        {
            if (Math.Abs(sampleMass) < float.Epsilon)
            {
                return -1;
            }

            return units switch
            {
                UnitOfMoleMassConcentration.Molar => 1.0,
                UnitOfMoleMassConcentration.MilliMolar => 1 / 1000.0,
                UnitOfMoleMassConcentration.MicroMolar => 1 / 1000000.0,
                UnitOfMoleMassConcentration.NanoMolar => 1 / 1000000000.0,
                UnitOfMoleMassConcentration.PicoMolar => 1 / 1000000000000.0,
                UnitOfMoleMassConcentration.FemtoMolar => 1 / 1.0E+15,
                UnitOfMoleMassConcentration.AttoMolar => 1 / 1.0E+18,
                UnitOfMoleMassConcentration.MgPerDL => 1 / sampleMass / 100.0, // 1/[(1 g / 1000 mg) * (1 / MW) * (10 dL/L)]
                UnitOfMoleMassConcentration.MgPerML => 1 / sampleMass, // 1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                UnitOfMoleMassConcentration.UgPerML => 1 / (sampleMass * 1000.0), // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                UnitOfMoleMassConcentration.NgPerML => 1 / (sampleMass * 1000000.0), // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                UnitOfMoleMassConcentration.UgPerUL => 1 / sampleMass, // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                UnitOfMoleMassConcentration.NgPerUL => 1 / (sampleMass * 1000.0), // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000000 uL/L)]
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to mL
        /// </summary>
        /// <remarks>An extended version of the FactorVolume method is in CapillaryFlow</remarks>
        /// <param name="units"></param>
        private static double FactorVolumeExtended(UnitOfExtendedVolume units)
        {
            // Note: 4 quarts per gallon, 2 pints per quart
            var factor = units switch
            {
                UnitOfExtendedVolume.L => 1 * 1000.0,
                UnitOfExtendedVolume.DL => 1 * 100.0,
                UnitOfExtendedVolume.ML => 1.0,
                UnitOfExtendedVolume.UL => 1 / 1000.0,
                UnitOfExtendedVolume.NL => 1 / 1000000.0,
                UnitOfExtendedVolume.PL => 1 / 1000000000.0,
                UnitOfExtendedVolume.Gallons => 1000.0 / GALLONS_PER_L,
                UnitOfExtendedVolume.Quarts => 1000.0 / GALLONS_PER_L / 4.0,
                UnitOfExtendedVolume.Pints => 1000.0 / GALLONS_PER_L / 8.0,
                _ => -1
            };

            return factor;
        }
    }
}
