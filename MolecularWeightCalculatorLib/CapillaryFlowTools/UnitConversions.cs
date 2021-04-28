using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.CapillaryFlowTools
{
    [ComVisible(false)]
    public static class UnitConversions
    {
        // Ignore Spelling: dynes, ng, ug

        /// <summary>
        /// Conversion factor: number of centimeters in an inch
        /// </summary>
        public const float CM_PER_INCH = 2.54f;

        /// <summary>
        /// Convert concentration
        /// </summary>
        /// <param name="concentrationIn"></param>
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        /// <param name="sampleMassGramsPerMole">sample mass in g/mole</param>
        public static double ConvertConcentration(double concentrationIn, UnitOfConcentration currentUnits, UnitOfConcentration newUnits, double sampleMassGramsPerMole)
        {
            if (currentUnits == newUnits)
            {
                return concentrationIn;
            }

            var factor = FactorConcentration(currentUnits, sampleMassGramsPerMole);
            if (factor < 0)
            {
                return -1;
            }

            var value = concentrationIn * factor;

            factor = FactorConcentration(newUnits, sampleMassGramsPerMole);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertDiffusionCoefficient(double diffusionCoefficientIn, UnitOfDiffusionCoefficient currentUnits, UnitOfDiffusionCoefficient newUnits)
        {
            if (currentUnits == newUnits)
            {
                return diffusionCoefficientIn;
            }

            var factor = FactorDiffusionCoeff(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = diffusionCoefficientIn * factor;

            factor = FactorDiffusionCoeff(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertLength(double lengthIn, UnitOfLength currentUnits, UnitOfLength newUnits)
        {
            if (currentUnits == newUnits)
            {
                return lengthIn;
            }

            var factor = FactorLength(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = lengthIn * factor;

            factor = FactorLength(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertLinearVelocity(double linearVelocityIn, UnitOfLinearVelocity currentUnits, UnitOfLinearVelocity newUnits)
        {
            if (currentUnits == newUnits)
            {
                return linearVelocityIn;
            }

            var factor = FactorLinearVelocity(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = linearVelocityIn * factor;

            factor = FactorLinearVelocity(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertMassFlowRate(double massFlowRateIn, UnitOfMassFlowRate currentUnits, UnitOfMassFlowRate newUnits)
        {
            if (currentUnits == newUnits)
            {
                return massFlowRateIn;
            }

            var factor = FactorMassFlowRate(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = massFlowRateIn * factor;

            factor = FactorMassFlowRate(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertMoles(double molesIn, UnitOfMolarAmount currentUnits, UnitOfMolarAmount newUnits)
        {
            if (currentUnits == newUnits)
            {
                return molesIn;
            }

            var factor = FactorMoles(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = molesIn * factor;

            factor = FactorMoles(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertPressure(double pressureIn, UnitOfPressure currentUnits, UnitOfPressure newUnits)
        {
            if (currentUnits == newUnits)
            {
                return pressureIn;
            }

            var factor = FactorPressure(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = pressureIn * factor;

            factor = FactorPressure(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertTemperature(double temperatureIn, UnitOfTemperature currentUnits, UnitOfTemperature newUnits)
        {
            var value = 0.0;
            if (currentUnits == newUnits)
            {
                return temperatureIn;
            }

            // First convert to Kelvin
            switch (currentUnits)
            {
                case UnitOfTemperature.Celsius:
                    // K = C + 273.15
                    value = temperatureIn + 273.15;
                    break;

                case UnitOfTemperature.Fahrenheit:
                    // Convert to Kelvin: C = 5/9*(F-32) and K = C + 273.15
                    value = 5.0 / 9.0 * (temperatureIn - 32) + 273.15;
                    break;

                // ReSharper disable once RedundantEmptySwitchSection
                default:
                    // Includes UnitOfTemperature.Kelvin
                    // Assume already Kelvin
                    break;
            }

            // We cannot get colder than absolute 0
            if (value < 0)
                value = 0;

            // Now convert to the target units
            switch (newUnits)
            {
                case UnitOfTemperature.Celsius:
                    // C = K - 273.15
                    value -= 273.15;
                    break;

                case UnitOfTemperature.Fahrenheit:
                    // Convert to Fahrenheit: C = K - 273.15 and F = (9/5)C + 32
                    value = 9.0 / 5.0 * (value - 273.15) + 32;
                    break;

                // ReSharper disable once RedundantEmptySwitchSection
                default:
                    // Includes UnitOfTemperature.Kelvin
                    // Already in Kelvin
                    break;
            }

            return value;
        }

        public static double ConvertTime(double timeIn, UnitOfTime currentUnits, UnitOfTime newUnits)
        {
            if (currentUnits == newUnits)
            {
                return timeIn;
            }

            var factor = FactorTime(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = timeIn * factor;

            factor = FactorTime(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertViscosity(double viscosityIn, UnitOfViscosity currentUnits, UnitOfViscosity newUnits)
        {
            if (currentUnits == newUnits)
            {
                return viscosityIn;
            }

            var factor = FactorViscosity(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = viscosityIn * factor;

            factor = FactorViscosity(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertVolumetricFlowRate(double volumetricFlowRateIn, UnitOfFlowRate currentUnits, UnitOfFlowRate newUnits)
        {
            if (currentUnits == newUnits)
            {
                return volumetricFlowRateIn;
            }

            var factor = FactorVolumetricFlowRate(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = volumetricFlowRateIn * factor;

            factor = FactorVolumetricFlowRate(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public static double ConvertVolume(double volume, UnitOfVolume currentUnits, UnitOfVolume newUnits)
        {
            if (currentUnits == newUnits)
            {
                return volume;
            }

            var factor = FactorVolume(currentUnits);
            if (factor < 0)
            {
                return -1;
            }

            var value = volume * factor;

            factor = FactorVolume(newUnits);
            if (factor < 0 || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to M
        /// <paramref name="sampleMass"/> is required for mass-based units
        /// </summary>
        /// <param name="units"></param>
        /// <param name="sampleMass"></param>
        private static double FactorConcentration(UnitOfConcentration units, double sampleMass = 0)
        {
            if (Math.Abs(sampleMass) < float.Epsilon)
            {
                return -1;
            }

            return units switch
            {
                UnitOfConcentration.Molar => 1.0,
                UnitOfConcentration.MilliMolar => 1 / 1000.0,
                UnitOfConcentration.MicroMolar => 1 / 1000000.0,
                UnitOfConcentration.NanoMolar => 1 / 1000000000.0,
                UnitOfConcentration.PicoMolar => 1 / 1000000000000.0,
                UnitOfConcentration.FemtoMolar => 1 / 1.0E+15,
                UnitOfConcentration.AttoMolar => 1 / 1.0E+18,
                UnitOfConcentration.MgPerML => 1 / sampleMass, // 1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                UnitOfConcentration.UgPerML => 1 / (sampleMass * 1000.0), // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                UnitOfConcentration.NgPerML => 1 / (sampleMass * 1000000.0), // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                UnitOfConcentration.UgPerUL => 1 / sampleMass, // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                UnitOfConcentration.NgPerUL => 1 / (sampleMass * 1000.0), // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000000 uL/L)]
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to Cm
        /// </summary>
        /// <param name="units"></param>
        private static double FactorLength(UnitOfLength units)
        {
            return units switch
            {
                UnitOfLength.M => 100.0,
                UnitOfLength.CM => 1.0,
                UnitOfLength.MM => 1 / 10.0,
                UnitOfLength.Microns => 1 / 10000.0,
                UnitOfLength.Inches => CM_PER_INCH,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to Cm/Min
        /// </summary>
        /// <param name="units"></param>
        private static double FactorLinearVelocity(UnitOfLinearVelocity units)
        {
            return units switch
            {
                UnitOfLinearVelocity.CmPerHr => 1 / 60.0,
                UnitOfLinearVelocity.MmPerHr => 1 / 60.0 / 10.0,
                UnitOfLinearVelocity.CmPerMin => 1,
                UnitOfLinearVelocity.MmPerMin => 1 / 10.0,
                UnitOfLinearVelocity.CmPerSec => 60.0,
                UnitOfLinearVelocity.MmPerSec => 60.0 / 10.0,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to moles/min
        /// </summary>
        /// <param name="units"></param>
        private static double FactorMassFlowRate(UnitOfMassFlowRate units)
        {
            return units switch
            {
                UnitOfMassFlowRate.PmolPerMin => 1 / 1000000000000.0,
                UnitOfMassFlowRate.FmolPerMin => 1 / 1.0E+15,
                UnitOfMassFlowRate.AmolPerMin => 1 / 1.0E+18,
                UnitOfMassFlowRate.PmolPerSec => 1 / (1000000000000.0 / 60.0),
                UnitOfMassFlowRate.FmolPerSec => 1 / (1.0E+15 / 60.0),
                UnitOfMassFlowRate.AmolPerSec => 1 / (1.0E+18 / 60.0),
                UnitOfMassFlowRate.MolesPerMin => 1.0,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to moles
        /// </summary>
        /// <param name="units"></param>
        private static double FactorMoles(UnitOfMolarAmount units)
        {
            return units switch
            {
                UnitOfMolarAmount.Moles => 1.0,
                UnitOfMolarAmount.MilliMoles => 1 / 1000.0,
                UnitOfMolarAmount.MicroMoles => 1 / 1000000.0,
                UnitOfMolarAmount.NanoMoles => 1 / 1000000000.0,
                UnitOfMolarAmount.PicoMoles => 1 / 1000000000000.0,
                UnitOfMolarAmount.FemtoMoles => 1 / 1.0E+15,
                UnitOfMolarAmount.AttoMoles => 1 / 1.0E+18,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to dynes per cm^2
        /// </summary>
        /// <param name="units"></param>
        private static double FactorPressure(UnitOfPressure units)
        {
            return units switch
            {
                UnitOfPressure.Psi => 68947.57,
                UnitOfPressure.Pascals => 10.0,
                UnitOfPressure.KiloPascals => 10000.0,
                UnitOfPressure.Atmospheres => 1013250.0,
                UnitOfPressure.Bar => 1000000.0,
                UnitOfPressure.Torr => 1333.22,
                UnitOfPressure.DynesPerSquareCm => 1,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to minutes
        /// </summary>
        /// <param name="units"></param>
        private static double FactorTime(UnitOfTime units)
        {
            return units switch
            {
                UnitOfTime.Hours => 60.0,
                UnitOfTime.Minutes => 1.0,
                UnitOfTime.Seconds => 1 / 60.0,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to cm^2/sec
        /// </summary>
        /// <param name="units"></param>
        private static double FactorDiffusionCoeff(UnitOfDiffusionCoefficient units)
        {
            return units switch
            {
                UnitOfDiffusionCoefficient.CmSquaredPerHr => 1 / 3600.0,
                UnitOfDiffusionCoefficient.CmSquaredPerMin => 1 / 60.0,
                UnitOfDiffusionCoefficient.CmSquaredPerSec => 1.0,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to poise
        /// </summary>
        /// <param name="units"></param>
        private static double FactorViscosity(UnitOfViscosity units)
        {
            return units switch
            {
                UnitOfViscosity.Poise => 1.0,
                UnitOfViscosity.CentiPoise => 1 / 100.0,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to mL/min
        /// </summary>
        /// <param name="units"></param>
        private static double FactorVolumetricFlowRate(UnitOfFlowRate units)
        {
            return units switch
            {
                UnitOfFlowRate.MLPerMin => 1.0,
                UnitOfFlowRate.ULPerMin => 1 / 1000.0,
                UnitOfFlowRate.NLPerMin => 1 / 1000000.0,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to mL
        /// </summary>
        /// <param name="units"></param>
        private static double FactorVolume(UnitOfVolume units)
        {
            return units switch
            {
                UnitOfVolume.ML => 1.0,
                UnitOfVolume.UL => 1 / 1000.0,
                UnitOfVolume.NL => 1 / 1000000.0,
                UnitOfVolume.PL => 1 / 1000000000.0,
                _ => -1
            };
        }
    }
}
