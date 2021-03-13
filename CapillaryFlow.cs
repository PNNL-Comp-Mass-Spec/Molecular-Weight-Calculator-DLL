using System;
using System.Runtime.InteropServices;

namespace MwtWinDll
{
    public class CapillaryFlow
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: CapillaryFlow

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
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

        public CapillaryFlow() : base()
        {
            InitializeClass();
        }

        #region "Enum Statements"

        public enum ctCapillaryTypeConstants
        {
            ctOpenTubularCapillary = 0,
            ctPackedCapillary
        }

        public enum uprUnitsPressureConstants
        {
            uprPsi = 0,
            uprPascals,
            uprKiloPascals,
            uprAtmospheres,
            uprBar,
            uprTorr,
            uprDynesPerSquareCm
        }

        public enum ulnUnitsLengthConstants
        {
            ulnM = 0,
            ulnCM,
            ulnMM,
            ulnMicrons,
            ulnInches
        }

        public enum uviUnitsViscosityConstants
        {
            uviPoise = 0,
            uviCentiPoise
        }

        public enum ufrUnitsFlowRateConstants
        {
            ufrMLPerMin = 0,
            ufrULPerMin,
            ufrNLPerMin
        }

        public enum ulvUnitsLinearVelocityConstants
        {
            ulvCmPerHr = 0,
            ulvMmPerHr,
            ulvCmPerMin,
            ulvMmPerMin,
            ulvCmPerSec,
            ulvMmPerSec
        }

        public enum utmUnitsTimeConstants
        {
            utmHours = 0,
            utmMinutes,
            utmSeconds
        }

        public enum uvoUnitsVolumeConstants
        {
            uvoML = 0,
            uvoUL,
            uvoNL,
            uvoPL
        }

        public enum ucoUnitsConcentrationConstants
        {
            ucoMolar = 0,
            ucoMilliMolar,
            ucoMicroMolar,
            ucoNanoMolar,
            ucoPicoMolar,
            ucoFemtoMolar,
            ucoAttoMolar,
            ucoMgPerML,
            ucoUgPerML,
            ucoNgPerML,
            ucoUgPerUL,
            ucoNgPerUL
        }

        public enum utpUnitsTemperatureConstants
        {
            utpCelsius = 0,
            utpKelvin,
            utpFahrenheit
        }

        public enum umfMassFlowRateConstants
        {
            umfPmolPerMin = 0,
            umfFmolPerMin,
            umfAmolPerMin,
            umfPmolPerSec,
            umfFmolPerSec,
            umfAmolPerSec,
            umfMolesPerMin
        }

        public enum umaMolarAmountConstants
        {
            umaMoles = 0,
            umaMilliMoles,
            umaMicroMoles,
            umaNanoMoles,
            umaPicoMoles,
            umaFemtoMoles,
            umaAttoMoles
        }

        public enum udcDiffusionCoefficientConstants
        {
            udcCmSquaredPerHr = 0,
            udcCmSquaredPerMin,
            udcCmSquaredPerSec
        }

        public enum acmAutoComputeModeConstants
        {
            acmBackPressure = 0,
            acmColumnID,
            acmColumnLength,
            acmDeadTime,
            acmLinearVelocity,
            acmVolFlowRate,
            acmVolFlowRateUsingDeadTime
        }

        #endregion

        #region "Data classes"

        private class udtCapillaryFlowParametersType
        {
            public ctCapillaryTypeConstants CapillaryType;

            /// <summary>
        /// Units: dynes/cm^2
        /// </summary>
            public double BackPressure;

            /// <summary>
        /// Units: cm
        /// </summary>
            public double ColumnLength;

            /// <summary>
        /// Units: cm
        /// </summary>
            public double ColumnID;

            /// <summary>
        /// Units: poise
        /// </summary>
            public double SolventViscosity;

            /// <summary>
        /// Units: cm
        /// </summary>
            public double ParticleDiameter;

            /// <summary>
        /// Units: mL/min
        /// </summary>
            public double VolumetricFlowRate;

            /// <summary>
        /// Units: cm/min
        /// </summary>
            public double LinearVelocity;

            /// <summary>
        /// Units: min
        /// </summary>
            public double ColumnDeadTime;
            public double InterparticlePorosity;
        }

        private class udtMassRateParametersType
        {
            /// <summary>
        /// Units: Molar
        /// </summary>
            public double SampleConcentration;

            /// <summary>
        /// Units: g/mole
        /// </summary>
            public double SampleMass;

            /// <summary>
        /// Units: mL/min
        /// </summary>
            public double VolumetricFlowRate;

            /// <summary>
        /// Units: min
        /// </summary>
            public double InjectionTime;

            /// <summary>
        /// Units: Moles/min
        /// </summary>
            public double MassFlowRate;

            /// <summary>
        /// Units: moles
        /// </summary>
            public double MolesInjected;
        }

        private class udtExtraColumnBroadeningParametersType
        {

            /// <summary>
        /// Units: cm/min
        /// </summary>
            public double LinearVelocity;

            /// <summary>
        /// Units: cm^2/sec
        /// </summary>
            public double DiffusionCoefficient;

            /// <summary>
        /// Units: cm
        /// </summary>
            public double OpenTubeLength;

            /// <summary>
        /// Units: cm
        /// </summary>
            public double OpenTubeID;

            /// <summary>
        /// Units: sec
        /// </summary>
            public double InitialPeakWidth;

            /// <summary>
        /// Units: sec^2
        /// </summary>
            public double TemporalVariance;

            /// <summary>
        /// Units: sec^2
        /// </summary>
            public double AdditionalTemporalVariance;

            /// <summary>
        /// Units: sec
        /// </summary>
            public double ResultantPeakWidth;
        }

        #endregion

        // Conversion Factors
        private const float CM_PER_INCH = 2.54f;
        private const double PI = 3.14159265359d;

        private udtCapillaryFlowParametersType mCapillaryFlowParameters = new udtCapillaryFlowParametersType();
        private udtMassRateParametersType mMassRateParameters = new udtMassRateParametersType();
        private udtExtraColumnBroadeningParametersType mExtraColumnBroadeningParameters = new udtExtraColumnBroadeningParametersType();

        /// <summary>
    /// When true, automatically compute results whenever any value changes
    /// </summary>
        private bool mAutoCompute;

        /// <summary>
    /// The value to compute when mAutoCompute is true
    /// </summary>
        private acmAutoComputeModeConstants mAutoComputeMode;

        private void CheckAutoCompute()
        {
            if (mAutoCompute)
            {
                switch (mAutoComputeMode)
                {
                    case acmAutoComputeModeConstants.acmBackPressure:
                        {
                            ComputeBackPressure();
                            break;
                        }

                    case acmAutoComputeModeConstants.acmColumnID:
                        {
                            ComputeColumnID();
                            break;
                        }

                    case acmAutoComputeModeConstants.acmColumnLength:
                        {
                            ComputeColumnLength();
                            break;
                        }

                    case acmAutoComputeModeConstants.acmDeadTime:
                        {
                            ComputeDeadTime();
                            break;
                        }

                    case acmAutoComputeModeConstants.acmLinearVelocity:
                        {
                            ComputeLinearVelocity();
                            break;
                        }

                    case acmAutoComputeModeConstants.acmVolFlowRateUsingDeadTime:
                        {
                            double argdblNewBackPressure = 0d;
                            this.ComputeVolFlowRateUsingDeadTime(out argdblNewBackPressure);
                            break;
                        }

                    default:
                        {
                            // Includes acmVolFlowRate
                            ComputeVolFlowRate();
                            break;
                        }
                }
            }
        }

        /// <summary>
    /// Computes the back pressure, stores in .BackPressure, and returns it
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        public double ComputeBackPressure(uprUnitsPressureConstants eUnits = uprUnitsPressureConstants.uprPsi)
        {
            double dblBackPressure, dblRadius;
            {
                var withBlock = mCapillaryFlowParameters;
                dblRadius = withBlock.ColumnID / 2.0d;
                if (Math.Abs(dblRadius) > float.Epsilon)
                {
                    if (withBlock.CapillaryType == ctCapillaryTypeConstants.ctOpenTubularCapillary)
                    {
                        // Open tubular capillary
                        dblBackPressure = withBlock.VolumetricFlowRate * 8d * withBlock.SolventViscosity * withBlock.ColumnLength / (Math.Pow(dblRadius, 4d) * PI * 60d); // Pressure in dynes/cm^2
                    }
                    // Packed capillary
                    else if (Math.Abs(withBlock.ParticleDiameter) > float.Epsilon & Math.Abs(withBlock.InterparticlePorosity) > float.Epsilon)
                    {
                        // Flow rate in mL/sec
                        dblBackPressure = withBlock.VolumetricFlowRate * 180d * withBlock.SolventViscosity * withBlock.ColumnLength * Math.Pow(1d - withBlock.InterparticlePorosity, 2d) / (Math.Pow(withBlock.ParticleDiameter, 2d) * Math.Pow(withBlock.InterparticlePorosity, 2d) * PI * Math.Pow(dblRadius, 2d) * 60d) / withBlock.InterparticlePorosity;
                    }
                    else
                    {
                        dblBackPressure = 0d;
                    }
                }
                else
                {
                    dblBackPressure = 0d;
                }

                withBlock.BackPressure = dblBackPressure;
            }

            // Compute Dead Time (and Linear Velocity)
            // Must send false for RecalculateVolFlowRate since we're finding the back pressure, not volumetric flow rate
            ComputeDeadTime(utmUnitsTimeConstants.utmMinutes, false);

            // Return Back Pressure
            return ConvertPressure(dblBackPressure, uprUnitsPressureConstants.uprDynesPerSquareCm, eUnits);
        }

        /// <summary>
    /// Computes the column length, stores in .ColumnLength, and returns it
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        public double ComputeColumnLength(ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnCM)
        {
            double dblColumnLength, dblRadius;
            {
                var withBlock = mCapillaryFlowParameters;
                dblRadius = withBlock.ColumnID / 2.0d;
                if (Math.Abs(withBlock.SolventViscosity) > float.Epsilon & Math.Abs(withBlock.VolumetricFlowRate) > float.Epsilon)
                {
                    if (withBlock.CapillaryType == ctCapillaryTypeConstants.ctOpenTubularCapillary)
                    {
                        // Open tubular capillary
                        dblColumnLength = withBlock.BackPressure * Math.Pow(dblRadius, 4d) * PI * 60d / (8d * withBlock.SolventViscosity * withBlock.VolumetricFlowRate); // Column length in cm
                    }
                    // Packed capillary
                    else if (Math.Abs(withBlock.InterparticlePorosity - 1d) > float.Epsilon)
                    {
                        // Flow rate in mL/sec
                        dblColumnLength = withBlock.BackPressure * Math.Pow(withBlock.ParticleDiameter, 2d) * Math.Pow(withBlock.InterparticlePorosity, 2d) * PI * Math.Pow(dblRadius, 2d) * 60d * withBlock.InterparticlePorosity / (180d * withBlock.SolventViscosity * withBlock.VolumetricFlowRate * Math.Pow(1d - withBlock.InterparticlePorosity, 2d));
                    }
                    else
                    {
                        dblColumnLength = 0d;
                    }
                }
                else
                {
                    dblColumnLength = 0d;
                }

                withBlock.ColumnLength = dblColumnLength;
            }

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(utmUnitsTimeConstants.utmMinutes, true);

            // Return Column Length
            return ConvertLength(dblColumnLength, ulnUnitsLengthConstants.ulnCM, eUnits);
        }

        public double ComputeColumnVolume(uvoUnitsVolumeConstants eUnits = 0)
        {
            // Computes the column volume and returns it (does not store it)

            double dblColumnVolume, dblRadius;
            {
                var withBlock = mCapillaryFlowParameters;
                dblRadius = withBlock.ColumnID / 2.0d;
                dblColumnVolume = withBlock.ColumnLength * PI * Math.Pow(dblRadius, 2d); // In mL
                if (withBlock.CapillaryType == ctCapillaryTypeConstants.ctPackedCapillary)
                {
                    dblColumnVolume *= withBlock.InterparticlePorosity;
                }
            }

            return ConvertVolume(dblColumnVolume, uvoUnitsVolumeConstants.uvoML, eUnits);
        }

        /// <summary>
    /// Computes the column length, stores in .ColumnLength, and returns it
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        public double ComputeColumnID(ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnMicrons)
        {
            double dblRadius;
            {
                var withBlock = mCapillaryFlowParameters;
                if (Math.Abs(withBlock.BackPressure) > float.Epsilon)
                {
                    if (withBlock.CapillaryType == ctCapillaryTypeConstants.ctOpenTubularCapillary)
                    {
                        // Open tubular capillary
                        dblRadius = Math.Pow(withBlock.VolumetricFlowRate * 8d * withBlock.SolventViscosity * withBlock.ColumnLength / (withBlock.BackPressure * PI * 60d), 0.25d);
                    }
                    // Packed capillary
                    else if (Math.Abs(withBlock.ParticleDiameter) > float.Epsilon & Math.Abs(withBlock.InterparticlePorosity - 1d) > float.Epsilon)
                    {
                        // Flow rate in mL/sec
                        dblRadius = Math.Pow(withBlock.VolumetricFlowRate * 180d * withBlock.SolventViscosity * withBlock.ColumnLength * Math.Pow(1d - withBlock.InterparticlePorosity, 2d) / (withBlock.BackPressure * Math.Pow(withBlock.ParticleDiameter, 2d) * Math.Pow(withBlock.InterparticlePorosity, 2d) * PI * 60d) / withBlock.InterparticlePorosity, 0.5d);
                    }
                    else
                    {
                        dblRadius = 0d;
                    }
                }
                else
                {
                    dblRadius = 0d;
                }

                withBlock.ColumnID = dblRadius * 2.0d;
            }

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(utmUnitsTimeConstants.utmMinutes, true);

            // Return Column ID
            return ConvertLength(dblRadius * 2.0d, ulnUnitsLengthConstants.ulnCM, eUnits);
        }

        /// <summary>
    /// Computes the column dead time, stores in .ColumnDeadTime, and returns it
    /// </summary>
    /// <param name="eUnits"></param>
    /// <param name="blnRecalculateVolFlowRate"></param>
    /// <returns></returns>
        public double ComputeDeadTime(utmUnitsTimeConstants eUnits = utmUnitsTimeConstants.utmMinutes, bool blnRecalculateVolFlowRate = true)
        {
            double dblDeadTime;

            // Dead time is dependent on Linear Velocity, so compute
            ComputeLinearVelocity(ulvUnitsLinearVelocityConstants.ulvCmPerSec, blnRecalculateVolFlowRate);
            {
                var withBlock = mCapillaryFlowParameters;
                if (Math.Abs(withBlock.LinearVelocity) > float.Epsilon)
                {
                    dblDeadTime = withBlock.ColumnLength / withBlock.LinearVelocity; // Dead time in minutes
                }
                else
                {
                    dblDeadTime = 0d;
                }

                withBlock.ColumnDeadTime = dblDeadTime;
            }

            // Return Dead Time
            return ConvertTime(dblDeadTime, utmUnitsTimeConstants.utmMinutes, eUnits);
        }

        public double ComputeExtraColumnBroadeningResultantPeakWidth(utmUnitsTimeConstants eUnits = utmUnitsTimeConstants.utmSeconds)
        {
            ComputeExtraColumnBroadeningValues();
            return GetExtraColumnBroadeningResultantPeakWidth(eUnits);
        }

        private void ComputeExtraColumnBroadeningValues()
        {
            double dblInitialPeakVariance;
            double dblSumOfVariances;
            {
                var withBlock = mExtraColumnBroadeningParameters;
                if (Math.Abs(withBlock.LinearVelocity) > float.Epsilon & Math.Abs(withBlock.DiffusionCoefficient) > float.Epsilon)
                {
                    withBlock.TemporalVariance = Math.Pow(withBlock.OpenTubeID, 2d) * withBlock.OpenTubeLength / (96d * withBlock.DiffusionCoefficient * withBlock.LinearVelocity / 60d); // in sec^2
                }
                else
                {
                    withBlock.TemporalVariance = 0d;
                }

                dblInitialPeakVariance = Math.Pow(withBlock.InitialPeakWidth / 4d, 2d);
                dblSumOfVariances = dblInitialPeakVariance + withBlock.TemporalVariance + withBlock.AdditionalTemporalVariance;
                if (dblSumOfVariances >= 0d)
                {
                    // ResultantPeakWidth at the base = 4 sigma  and  sigma = Sqr(Total_Variance)
                    withBlock.ResultantPeakWidth = 4d * Math.Sqrt(dblSumOfVariances);
                }
                else
                {
                    withBlock.ResultantPeakWidth = 0d;
                }
            }
        }

        /// <summary>
    /// Computes the Linear velocity, stores in .LinearVelocity, and returns it
    /// </summary>
    /// <param name="eUnits"></param>
    /// <param name="blnRecalculateVolFlowRate"></param>
    /// <returns></returns>
        public double ComputeLinearVelocity(ulvUnitsLinearVelocityConstants eUnits = ulvUnitsLinearVelocityConstants.ulvCmPerSec, bool blnRecalculateVolFlowRate = true)
        {
            double dblLinearVelocity, dblRadius;
            if (blnRecalculateVolFlowRate)
            {
                ComputeVolFlowRate(ufrUnitsFlowRateConstants.ufrMLPerMin);
            }

            {
                var withBlock = mCapillaryFlowParameters;
                dblRadius = withBlock.ColumnID / 2.0d;
                if (Math.Abs(dblRadius) > float.Epsilon)
                {
                    dblLinearVelocity = withBlock.VolumetricFlowRate / (PI * Math.Pow(dblRadius, 2d)); // Units in cm/min

                    // Divide Linear Velocity by epsilon if a packed capillary
                    if (withBlock.CapillaryType == ctCapillaryTypeConstants.ctPackedCapillary & Math.Abs(withBlock.InterparticlePorosity) > float.Epsilon)
                    {
                        dblLinearVelocity /= withBlock.InterparticlePorosity;
                    }
                }
                else
                {
                    dblLinearVelocity = 0d;
                }

                withBlock.LinearVelocity = dblLinearVelocity;
            }

            // Return Linear Velocity
            return ConvertLinearVelocity(dblLinearVelocity, ulvUnitsLinearVelocityConstants.ulvCmPerMin, eUnits);
        }

        /// <summary>
    /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        public double ComputeMassFlowRate(umfMassFlowRateConstants eUnits = umfMassFlowRateConstants.umfFmolPerSec)
        {
            ComputeMassRateValues();
            return GetMassFlowRate(eUnits);
        }

        /// <summary>
    /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        public double ComputeMassRateMolesInjected(umaMolarAmountConstants eUnits = umaMolarAmountConstants.umaFemtoMoles)
        {
            ComputeMassRateValues();
            return GetMassRateMolesInjected(eUnits);
        }

        private void ComputeMassRateValues()
        {
            {
                var withBlock = mMassRateParameters;
                withBlock.MassFlowRate = withBlock.SampleConcentration * withBlock.VolumetricFlowRate / 1000d; // Compute mass flow rate in moles/min
                withBlock.MolesInjected = withBlock.MassFlowRate * withBlock.InjectionTime; // Compute moles injected in moles
            }
        }

        /// <summary>
    /// Computes the optimum linear velocity, based on
    /// mCapillaryFlowParameters.ParticleDiameter
    /// and mExtraColumnBroadeningParameters.DiffusionCoefficient
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        public double ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(ulvUnitsLinearVelocityConstants eUnits = ulvUnitsLinearVelocityConstants.ulvCmPerSec)
        {
            var dblOptimumLinearVelocity = default(double);
            {
                var withBlock = mCapillaryFlowParameters;
                if (Math.Abs(withBlock.ParticleDiameter) > float.Epsilon)
                {
                    dblOptimumLinearVelocity = 3d * mExtraColumnBroadeningParameters.DiffusionCoefficient / withBlock.ParticleDiameter;
                    dblOptimumLinearVelocity = ConvertLinearVelocity(dblOptimumLinearVelocity, ulvUnitsLinearVelocityConstants.ulvCmPerSec, eUnits);
                }
            }

            return dblOptimumLinearVelocity;
        }

        public double ComputeMeCNViscosity(double dblPercentAcetonitrile, double dblTemperature, utpUnitsTemperatureConstants eTemperatureUnits = utpUnitsTemperatureConstants.utpCelsius, uviUnitsViscosityConstants eViscosityUnits = uviUnitsViscosityConstants.uviPoise)
        {
            double dblPhi; // Fraction Acetonitrile
            double dblKelvin;
            double dblViscosityInCentiPoise;
            try
            {
                dblPhi = dblPercentAcetonitrile / 100.0d;
                if (dblPhi < 0d)
                    dblPhi = 0d;
                if (dblPhi > 100d)
                    dblPhi = 100d;
                dblKelvin = ConvertTemperature(dblTemperature, eTemperatureUnits, utpUnitsTemperatureConstants.utpKelvin);
                if (dblKelvin > 0d)
                {
                    dblViscosityInCentiPoise = Math.Exp(dblPhi * (-3.476d + 726d / dblKelvin) + (1d - dblPhi) * (-5.414d + 1566d / dblKelvin) + dblPhi * (1d - dblPhi) * (-1.762d + 929d / dblKelvin));
                }
                else
                {
                    dblViscosityInCentiPoise = 0d;
                }

                return ConvertViscosity(dblViscosityInCentiPoise, uviUnitsViscosityConstants.uviCentiPoise, eViscosityUnits);
            }
            catch (Exception ex)
            {
                return 0d;
            }
        }

        /// <summary>
    /// Computes the Volumetric flow rate, stores in .VolumetricFlowRate, and returns it
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        public double ComputeVolFlowRate(ufrUnitsFlowRateConstants eUnits = ufrUnitsFlowRateConstants.ufrNLPerMin)
        {
            double dblVolFlowRate, dblRadius;
            {
                var withBlock = mCapillaryFlowParameters;
                dblRadius = withBlock.ColumnID / 2.0d;
                if (Math.Abs(withBlock.SolventViscosity) > float.Epsilon & Math.Abs(withBlock.ColumnLength) > float.Epsilon)
                {
                    if (withBlock.CapillaryType == ctCapillaryTypeConstants.ctOpenTubularCapillary)
                    {
                        // Open tubular capillary
                        dblVolFlowRate = withBlock.BackPressure * Math.Pow(dblRadius, 4d) * PI / (8d * withBlock.SolventViscosity * withBlock.ColumnLength); // Flow rate in mL/sec
                    }
                    // Packed capillary
                    else if (Math.Abs(withBlock.InterparticlePorosity - 1d) > float.Epsilon)
                    {
                        // Flow rate in mL/sec
                        dblVolFlowRate = withBlock.BackPressure * Math.Pow(withBlock.ParticleDiameter, 2d) * Math.Pow(withBlock.InterparticlePorosity, 2d) * PI * Math.Pow(dblRadius, 2d) * withBlock.InterparticlePorosity / (180d * withBlock.SolventViscosity * withBlock.ColumnLength * Math.Pow(1d - withBlock.InterparticlePorosity, 2d));
                    }
                    else
                    {
                        dblVolFlowRate = 0d;
                    }

                    // Convert dblVolFlowRate to mL/min
                    dblVolFlowRate *= 60d;
                }
                else
                {
                    dblVolFlowRate = 0d;
                }

                withBlock.VolumetricFlowRate = dblVolFlowRate;
            }

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(utmUnitsTimeConstants.utmMinutes, false);
            return ConvertVolFlowRate(dblVolFlowRate, ufrUnitsFlowRateConstants.ufrMLPerMin, eUnits);
        }

        /// <summary>
    /// Computes the Volumetric flow rate using the dead time, stores in .VolumetricFlowRate, and returns it
    /// This requires modifying the pressure value to give the computed volumetric flow rate
    /// </summary>
    /// <param name="dblNewBackPressure">Output: new back pressure</param>
    /// <param name="eUnits"></param>
    /// <param name="ePressureUnits"></param>
    /// <returns></returns>
        public double ComputeVolFlowRateUsingDeadTime([Optional, DefaultParameterValue(0d)] out double dblNewBackPressure, ufrUnitsFlowRateConstants eUnits = ufrUnitsFlowRateConstants.ufrNLPerMin, uprUnitsPressureConstants ePressureUnits = uprUnitsPressureConstants.uprPsi)
        {
            dblNewBackPressure = 0;
            double dblVolFlowRate, dblRadius;
            {
                var withBlock = mCapillaryFlowParameters;
                dblRadius = withBlock.ColumnID / 2.0d;

                // First find vol flow rate that gives observed dead time
                if (Math.Abs(withBlock.ColumnDeadTime) > float.Epsilon)
                {
                    dblVolFlowRate = withBlock.ColumnLength * (PI * Math.Pow(dblRadius, 2d)) / withBlock.ColumnDeadTime; // Vol flow rate in mL/sec
                    if (withBlock.CapillaryType == ctCapillaryTypeConstants.ctPackedCapillary)
                    {
                        // Packed Capillary
                        dblVolFlowRate *= withBlock.InterparticlePorosity;
                    }

                    // Store the new value
                    withBlock.VolumetricFlowRate = dblVolFlowRate;

                    // Now find pressure that gives computed dblVolFlowRate
                    // The ComputeBackPressure sub will store the new pressure
                    dblNewBackPressure = ComputeBackPressure(ePressureUnits);
                }
                else
                {
                    dblVolFlowRate = 0d;
                    withBlock.VolumetricFlowRate = 0d;
                }
            }

            // Compute Linear Velocity (but not the dead time)
            ComputeLinearVelocity(ulvUnitsLinearVelocityConstants.ulvCmPerSec, false);
            return ConvertVolFlowRate(dblVolFlowRate, ufrUnitsFlowRateConstants.ufrMLPerMin, eUnits);
        }

        /// <summary>
    /// Convert concentration
    /// </summary>
    /// <param name="dblConcentrationIn"></param>
    /// <param name="eCurrentUnits"></param>
    /// <param name="eNewUnits"></param>
    /// <returns></returns>
    /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        public double ConvertConcentration(double dblConcentrationIn, ucoUnitsConcentrationConstants eCurrentUnits, ucoUnitsConcentrationConstants eNewUnits)
        {
            double dblValue, dblFactor;
            double dblSampleMass;
            if (eCurrentUnits == eNewUnits)
            {
                return dblConcentrationIn;
            }

            dblSampleMass = mMassRateParameters.SampleMass;
            dblFactor = FactorConcentration(eCurrentUnits, dblSampleMass);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblConcentrationIn * dblFactor;
            }

            dblFactor = FactorConcentration(eNewUnits, dblSampleMass);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertDiffusionCoefficient(double dblDiffusionCoefficientIn, udcDiffusionCoefficientConstants eCurrentUnits, udcDiffusionCoefficientConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblDiffusionCoefficientIn;
            }

            dblFactor = FactorDiffusionCoeff(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblDiffusionCoefficientIn * dblFactor;
            }

            dblFactor = FactorDiffusionCoeff(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertLength(double dblLengthIn, ulnUnitsLengthConstants eCurrentUnits, ulnUnitsLengthConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblLengthIn;
            }

            dblFactor = FactorLength(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblLengthIn * dblFactor;
            }

            dblFactor = FactorLength(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertLinearVelocity(double dblLinearVelocityIn, ulvUnitsLinearVelocityConstants eCurrentUnits, ulvUnitsLinearVelocityConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblLinearVelocityIn;
            }

            dblFactor = FactorLinearVelocity(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblLinearVelocityIn * dblFactor;
            }

            dblFactor = FactorLinearVelocity(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertMassFlowRate(double dblMassFlowRateIn, umfMassFlowRateConstants eCurrentUnits, umfMassFlowRateConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblMassFlowRateIn;
            }

            dblFactor = FactorMassFlowRate(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblMassFlowRateIn * dblFactor;
            }

            dblFactor = FactorMassFlowRate(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertMoles(double dblMolesIn, umaMolarAmountConstants eCurrentUnits, umaMolarAmountConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblMolesIn;
            }

            dblFactor = FactorMoles(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblMolesIn * dblFactor;
            }

            dblFactor = FactorMoles(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertPressure(double dblPressureIn, uprUnitsPressureConstants eCurrentUnits, uprUnitsPressureConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblPressureIn;
            }

            dblFactor = FactorPressure(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblPressureIn * dblFactor;
            }

            dblFactor = FactorPressure(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertTemperature(double dblTemperatureIn, utpUnitsTemperatureConstants eCurrentUnits, utpUnitsTemperatureConstants eNewUnits)
        {
            var dblValue = default(double);
            if (eCurrentUnits == eNewUnits)
            {
                return dblTemperatureIn;
            }

            // First convert to Kelvin
            switch (eCurrentUnits)
            {
                case utpUnitsTemperatureConstants.utpCelsius:
                    {
                        // K = C + 273
                        dblValue = dblTemperatureIn + 273d;
                        break;
                    }

                case utpUnitsTemperatureConstants.utpFahrenheit:
                    {
                        // Convert to Kelvin: C = 5/9*(F-32) and K = C + 273
                        dblValue = 5.0d / 9.0d * (dblTemperatureIn - 32d) + 273d;
                        break;
                    }

                default:
                    {
                        break;
                    }
                    // Includes utpKelvin
                    // Assume already Kelvin
            }

            // We cannot get colder than absolute 0
            if (dblValue < 0d)
                dblValue = 0d;

            // Now convert to the target units
            switch (eNewUnits)
            {
                case utpUnitsTemperatureConstants.utpCelsius:
                    {
                        // C = K - 273
                        dblValue -= 273d;
                        break;
                    }

                case utpUnitsTemperatureConstants.utpFahrenheit:
                    {
                        // Convert to Fahrenheit: C = K - 273 and F = (9/5)C + 32
                        dblValue = 9.0d / 5.0d * (dblValue - 273d) + 32d;
                        break;
                    }

                default:
                    {
                        break;
                    }
                    // Includes utpKelvin
                    // Already in Kelvin
            }

            return dblValue;
        }

        public double ConvertTime(double dblTimeIn, utmUnitsTimeConstants eCurrentUnits, utmUnitsTimeConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblTimeIn;
            }

            dblFactor = FactorTime(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblTimeIn * dblFactor;
            }

            dblFactor = FactorTime(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertViscosity(double dblViscosityIn, uviUnitsViscosityConstants eCurrentUnits, uviUnitsViscosityConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblViscosityIn;
            }

            dblFactor = FactorViscosity(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblViscosityIn * dblFactor;
            }

            dblFactor = FactorViscosity(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertVolFlowRate(double dblVolFlowRateIn, ufrUnitsFlowRateConstants eCurrentUnits, ufrUnitsFlowRateConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblVolFlowRateIn;
            }

            dblFactor = FactorVolFlowRate(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblVolFlowRateIn * dblFactor;
            }

            dblFactor = FactorVolFlowRate(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        public double ConvertVolume(double dblVolume, uvoUnitsVolumeConstants eCurrentUnits, uvoUnitsVolumeConstants eNewUnits)
        {
            double dblValue, dblFactor;
            if (eCurrentUnits == eNewUnits)
            {
                return dblVolume;
            }

            dblFactor = FactorVolume(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                dblValue = dblVolume * dblFactor;
            }

            dblFactor = FactorVolume(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon | Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }
            else
            {
                return dblValue / dblFactor;
            }
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to M
    /// dblSampleMass is required for mass-based units
    /// </summary>
    /// <param name="eUnits"></param>
    /// <param name="dblSampleMass"></param>
    /// <returns></returns>
    /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        private double FactorConcentration(ucoUnitsConcentrationConstants eUnits, double dblSampleMass = 0d)
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
                    case ucoUnitsConcentrationConstants.ucoMolar:
                        {
                            dblFactor = 1.0d;
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoMilliMolar:
                        {
                            dblFactor = 1d / 1000.0d;
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoMicroMolar:
                        {
                            dblFactor = 1d / 1000000.0d;
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoNanoMolar:
                        {
                            dblFactor = 1d / 1000000000.0d;
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoPicoMolar:
                        {
                            dblFactor = 1d / 1000000000000.0d;
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoFemtoMolar:
                        {
                            dblFactor = 1d / 1.0E+15d;
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoAttoMolar:
                        {
                            dblFactor = 1d / 1.0E+18d;
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoMgPerML:
                        {
                            dblFactor = 1d / dblSampleMass; // 1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoUgPerML:
                        {
                            dblFactor = 1d / (dblSampleMass * 1000.0d); // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoNgPerML:
                        {
                            dblFactor = 1d / (dblSampleMass * 1000000.0d); // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoUgPerUL:
                        {
                            dblFactor = 1d / dblSampleMass; // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                            break;
                        }

                    case ucoUnitsConcentrationConstants.ucoNgPerUL:
                        {
                            dblFactor = 1d / (dblSampleMass * 1000.0d); // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000000 uL/L)]
                            break;
                        }

                    default:
                        {
                            dblFactor = -1;
                            break;
                        }
                }
            }

            return dblFactor;
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to Cm
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorLength(ulnUnitsLengthConstants eUnits)
        {
            switch (eUnits)
            {
                case ulnUnitsLengthConstants.ulnM:
                    {
                        return 100.0d;
                    }

                case ulnUnitsLengthConstants.ulnCM:
                    {
                        return 1.0d;
                    }

                case ulnUnitsLengthConstants.ulnMM:
                    {
                        return 1d / 10.0d;
                    }

                case ulnUnitsLengthConstants.ulnMicrons:
                    {
                        return 1d / 10000.0d;
                    }

                case ulnUnitsLengthConstants.ulnInches:
                    {
                        return CM_PER_INCH;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to Cm/Min
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorLinearVelocity(ulvUnitsLinearVelocityConstants eUnits)
        {
            switch (eUnits)
            {
                case ulvUnitsLinearVelocityConstants.ulvCmPerHr:
                    {
                        return 1d / 60.0d;
                    }

                case ulvUnitsLinearVelocityConstants.ulvMmPerHr:
                    {
                        return 1d / 60.0d / 10.0d;
                    }

                case ulvUnitsLinearVelocityConstants.ulvCmPerMin:
                    {
                        return 1d;
                    }

                case ulvUnitsLinearVelocityConstants.ulvMmPerMin:
                    {
                        return 1d / 10.0d;
                    }

                case ulvUnitsLinearVelocityConstants.ulvCmPerSec:
                    {
                        return 60.0d;
                    }

                case ulvUnitsLinearVelocityConstants.ulvMmPerSec:
                    {
                        return 60.0d / 10.0d;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to moles/min
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorMassFlowRate(umfMassFlowRateConstants eUnits)
        {
            switch (eUnits)
            {
                case umfMassFlowRateConstants.umfPmolPerMin:
                    {
                        return 1d / 1000000000000.0d;
                    }

                case umfMassFlowRateConstants.umfFmolPerMin:
                    {
                        return 1d / 1.0E+15d;
                    }

                case umfMassFlowRateConstants.umfAmolPerMin:
                    {
                        return 1d / 1.0E+18d;
                    }

                case umfMassFlowRateConstants.umfPmolPerSec:
                    {
                        return 1d / (1000000000000.0d / 60.0d);
                    }

                case umfMassFlowRateConstants.umfFmolPerSec:
                    {
                        return 1d / (1.0E+15d / 60.0d);
                    }

                case umfMassFlowRateConstants.umfAmolPerSec:
                    {
                        return 1d / (1.0E+18d / 60.0d);
                    }

                case umfMassFlowRateConstants.umfMolesPerMin:
                    {
                        return 1.0d;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to moles
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorMoles(umaMolarAmountConstants eUnits)
        {
            switch (eUnits)
            {
                case umaMolarAmountConstants.umaMoles:
                    {
                        return 1.0d;
                    }

                case umaMolarAmountConstants.umaMilliMoles:
                    {
                        return 1d / 1000.0d;
                    }

                case umaMolarAmountConstants.umaMicroMoles:
                    {
                        return 1d / 1000000.0d;
                    }

                case umaMolarAmountConstants.umaNanoMoles:
                    {
                        return 1d / 1000000000.0d;
                    }

                case umaMolarAmountConstants.umaPicoMoles:
                    {
                        return 1d / 1000000000000.0d;
                    }

                case umaMolarAmountConstants.umaFemtoMoles:
                    {
                        return 1d / 1.0E+15d;
                    }

                case umaMolarAmountConstants.umaAttoMoles:
                    {
                        return 1d / 1.0E+18d;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to dynes per cm^2
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorPressure(uprUnitsPressureConstants eUnits)
        {
            switch (eUnits)
            {
                case uprUnitsPressureConstants.uprPsi:
                    {
                        return 68947.57d;
                    }

                case uprUnitsPressureConstants.uprPascals:
                    {
                        return 10.0d;
                    }

                case uprUnitsPressureConstants.uprKiloPascals:
                    {
                        return 10000.0d;
                    }

                case uprUnitsPressureConstants.uprAtmospheres:
                    {
                        return 1013250.0d;
                    }

                case uprUnitsPressureConstants.uprBar:
                    {
                        return 1000000.0d;
                    }

                case uprUnitsPressureConstants.uprTorr:
                    {
                        return 1333.22d;
                    }

                case uprUnitsPressureConstants.uprDynesPerSquareCm:
                    {
                        return 1d;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to minutes
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorTime(utmUnitsTimeConstants eUnits)
        {
            switch (eUnits)
            {
                case utmUnitsTimeConstants.utmHours:
                    {
                        return 60.0d;
                    }

                case utmUnitsTimeConstants.utmMinutes:
                    {
                        return 1.0d;
                    }

                case utmUnitsTimeConstants.utmSeconds:
                    {
                        return 1d / 60.0d;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to cm^2/sec
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorDiffusionCoeff(udcDiffusionCoefficientConstants eUnits)
        {
            switch (eUnits)
            {
                case udcDiffusionCoefficientConstants.udcCmSquaredPerHr:
                    {
                        return 1d / 3600.0d;
                    }

                case udcDiffusionCoefficientConstants.udcCmSquaredPerMin:
                    {
                        return 1d / 60.0d;
                    }

                case udcDiffusionCoefficientConstants.udcCmSquaredPerSec:
                    {
                        return 1.0d;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to poise
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorViscosity(uviUnitsViscosityConstants eUnits)
        {
            switch (eUnits)
            {
                case uviUnitsViscosityConstants.uviPoise:
                    {
                        return 1.0d;
                    }

                case uviUnitsViscosityConstants.uviCentiPoise:
                    {
                        return 1d / 100.0d;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to mL/min
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorVolFlowRate(ufrUnitsFlowRateConstants eUnits)
        {
            double FactorVolFlowRateRet = default;
            switch (eUnits)
            {
                case ufrUnitsFlowRateConstants.ufrMLPerMin:
                    {
                        FactorVolFlowRateRet = 1.0d;
                        break;
                    }

                case ufrUnitsFlowRateConstants.ufrULPerMin:
                    {
                        FactorVolFlowRateRet = 1d / 1000.0d;
                        break;
                    }

                case ufrUnitsFlowRateConstants.ufrNLPerMin:
                    {
                        FactorVolFlowRateRet = 1d / 1000000.0d;
                        break;
                    }

                default:
                    {
                        FactorVolFlowRateRet = -1;
                        break;
                    }
            }

            return FactorVolFlowRateRet;
        }

        /// <summary>
    /// Multiplication factor for converting from eUnits to mL
    /// </summary>
    /// <param name="eUnits"></param>
    /// <returns></returns>
        private double FactorVolume(uvoUnitsVolumeConstants eUnits)
        {
            switch (eUnits)
            {
                case uvoUnitsVolumeConstants.uvoML:
                    {
                        return 1.0d;
                    }

                case uvoUnitsVolumeConstants.uvoUL:
                    {
                        return 1d / 1000.0d;
                    }

                case uvoUnitsVolumeConstants.uvoNL:
                    {
                        return 1d / 1000000.0d;
                    }

                case uvoUnitsVolumeConstants.uvoPL:
                    {
                        return 1d / 1000000000.0d;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        // Get Methods
        // Gets the most recently computed value
        // If mAutoCompute = False, then must manually call a Compute Sub to recompute the value

        public bool GetAutoComputeEnabled()
        {
            return mAutoCompute;
        }

        public acmAutoComputeModeConstants GetAutoComputeMode()
        {
            return mAutoComputeMode;
        }

        public double GetBackPressure(uprUnitsPressureConstants eUnits = uprUnitsPressureConstants.uprPsi)
        {
            return ConvertPressure(mCapillaryFlowParameters.BackPressure, uprUnitsPressureConstants.uprDynesPerSquareCm, eUnits);
        }

        public ctCapillaryTypeConstants GetCapillaryType()
        {
            return mCapillaryFlowParameters.CapillaryType;
        }

        public double GetColumnID(ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnMicrons)
        {
            return ConvertLength(mCapillaryFlowParameters.ColumnID, ulnUnitsLengthConstants.ulnCM, eUnits);
        }

        public double GetColumnLength(ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnCM)
        {
            return ConvertLength(mCapillaryFlowParameters.ColumnLength, ulnUnitsLengthConstants.ulnCM, eUnits);
        }

        public double GetColumnVolume(uvoUnitsVolumeConstants eUnits = uvoUnitsVolumeConstants.uvoUL)
        {
            // Column volume isn't stored; simply re-compute it
            return ComputeColumnVolume(eUnits);
        }

        public double GetDeadTime(utmUnitsTimeConstants eUnits = utmUnitsTimeConstants.utmMinutes)
        {
            return ConvertTime(mCapillaryFlowParameters.ColumnDeadTime, utmUnitsTimeConstants.utmMinutes, eUnits);
        }

        public double GetExtraColumnBroadeningAdditionalVarianceInSquareSeconds()
        {
            return mExtraColumnBroadeningParameters.AdditionalTemporalVariance;
        }

        public double GetExtraColumnBroadeningDiffusionCoefficient(udcDiffusionCoefficientConstants eUnits = udcDiffusionCoefficientConstants.udcCmSquaredPerSec)
        {
            return ConvertDiffusionCoefficient(mExtraColumnBroadeningParameters.DiffusionCoefficient, udcDiffusionCoefficientConstants.udcCmSquaredPerSec, eUnits);
        }

        public double GetExtraColumnBroadeningInitialPeakWidthAtBase(utmUnitsTimeConstants eUnits = utmUnitsTimeConstants.utmSeconds)
        {
            return ConvertTime(mExtraColumnBroadeningParameters.InitialPeakWidth, utmUnitsTimeConstants.utmSeconds, eUnits);
        }

        public double GetExtraColumnBroadeningLinearVelocity(ulvUnitsLinearVelocityConstants eUnits = ulvUnitsLinearVelocityConstants.ulvMmPerMin)
        {
            return ConvertLinearVelocity(mExtraColumnBroadeningParameters.LinearVelocity, ulvUnitsLinearVelocityConstants.ulvCmPerMin, eUnits);
        }

        public double GetExtraColumnBroadeningOpenTubeID(ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnMicrons)
        {
            return ConvertLength(mExtraColumnBroadeningParameters.OpenTubeID, ulnUnitsLengthConstants.ulnCM, eUnits);
        }

        public double GetExtraColumnBroadeningOpenTubeLength(ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnCM)
        {
            return ConvertLength(mExtraColumnBroadeningParameters.OpenTubeLength, ulnUnitsLengthConstants.ulnCM, eUnits);
        }

        public double GetExtraColumnBroadeningResultantPeakWidth(utmUnitsTimeConstants eUnits = utmUnitsTimeConstants.utmSeconds)
        {
            return ConvertTime(mExtraColumnBroadeningParameters.ResultantPeakWidth, utmUnitsTimeConstants.utmSeconds, eUnits);
        }

        public double GetExtraColumnBroadeningTemporalVarianceInSquareSeconds()
        {
            return mExtraColumnBroadeningParameters.TemporalVariance;
        }

        public double GetInterparticlePorosity()
        {
            return mCapillaryFlowParameters.InterparticlePorosity;
        }

        public double GetLinearVelocity(ulvUnitsLinearVelocityConstants eUnits = ulvUnitsLinearVelocityConstants.ulvCmPerSec)
        {
            return ConvertLinearVelocity(mCapillaryFlowParameters.LinearVelocity, ulvUnitsLinearVelocityConstants.ulvCmPerMin, eUnits);
        }

        public double GetMassRateConcentration(ucoUnitsConcentrationConstants eUnits = ucoUnitsConcentrationConstants.ucoMicroMolar)
        {
            return ConvertConcentration(mMassRateParameters.SampleConcentration, ucoUnitsConcentrationConstants.ucoMolar, eUnits);
        }

        public double GetMassRateInjectionTime(utmUnitsTimeConstants eUnits = utmUnitsTimeConstants.utmMinutes)
        {
            return ConvertTime(mMassRateParameters.InjectionTime, utmUnitsTimeConstants.utmMinutes, eUnits);
        }

        public double GetMassFlowRate(umfMassFlowRateConstants eUnits = umfMassFlowRateConstants.umfFmolPerSec)
        {
            return ConvertMassFlowRate(mMassRateParameters.MassFlowRate, umfMassFlowRateConstants.umfMolesPerMin, eUnits);
        }

        public double GetMassRateMolesInjected(umaMolarAmountConstants eUnits = umaMolarAmountConstants.umaFemtoMoles)
        {
            return ConvertMoles(mMassRateParameters.MolesInjected, umaMolarAmountConstants.umaMoles, eUnits);
        }

        public double GetMassRateSampleMass()
        {
            return mMassRateParameters.SampleMass;
        }

        public double GetMassRateVolFlowRate(ufrUnitsFlowRateConstants eUnits = ufrUnitsFlowRateConstants.ufrNLPerMin)
        {
            return ConvertVolFlowRate(mMassRateParameters.VolumetricFlowRate, ufrUnitsFlowRateConstants.ufrMLPerMin, eUnits);
        }

        public double GetParticleDiameter(ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnMicrons)
        {
            return ConvertLength(mCapillaryFlowParameters.ParticleDiameter, ulnUnitsLengthConstants.ulnCM, eUnits);
        }

        public double GetSolventViscosity(uviUnitsViscosityConstants eUnits = uviUnitsViscosityConstants.uviPoise)
        {
            return ConvertViscosity(mCapillaryFlowParameters.SolventViscosity, uviUnitsViscosityConstants.uviPoise, eUnits);
        }

        public double GetVolFlowRate(ufrUnitsFlowRateConstants eUnits = ufrUnitsFlowRateConstants.ufrNLPerMin)
        {
            return ConvertVolFlowRate(mCapillaryFlowParameters.VolumetricFlowRate, ufrUnitsFlowRateConstants.ufrMLPerMin, eUnits);
        }


        // Set Methods
        // If mAutoCompute = False, then must manually call a Compute Sub to recompute other values

        public void SetAutoComputeEnabled(bool blnAutoCompute)
        {
            mAutoCompute = blnAutoCompute;
        }

        public void SetAutoComputeMode(acmAutoComputeModeConstants eAutoComputeMode)
        {
            if (eAutoComputeMode >= acmAutoComputeModeConstants.acmBackPressure & eAutoComputeMode <= acmAutoComputeModeConstants.acmVolFlowRateUsingDeadTime)
            {
                mAutoComputeMode = eAutoComputeMode;
            }
        }

        public void SetBackPressure(double dblBackPressure, uprUnitsPressureConstants eUnits = uprUnitsPressureConstants.uprPsi)
        {
            mCapillaryFlowParameters.BackPressure = ConvertPressure(dblBackPressure, eUnits, uprUnitsPressureConstants.uprDynesPerSquareCm);
            CheckAutoCompute();
        }

        public void SetCapillaryType(ctCapillaryTypeConstants eCapillaryType)
        {
            if (eCapillaryType >= ctCapillaryTypeConstants.ctOpenTubularCapillary & eCapillaryType <= ctCapillaryTypeConstants.ctPackedCapillary)
            {
                mCapillaryFlowParameters.CapillaryType = eCapillaryType;
            }

            CheckAutoCompute();
        }

        public void SetColumnID(double dblColumnID, ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnMicrons)
        {
            mCapillaryFlowParameters.ColumnID = ConvertLength(dblColumnID, eUnits, ulnUnitsLengthConstants.ulnCM);
            CheckAutoCompute();
        }

        public void SetColumnLength(double dblColumnLength, ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnCM)
        {
            mCapillaryFlowParameters.ColumnLength = ConvertLength(dblColumnLength, eUnits, ulnUnitsLengthConstants.ulnCM);
            CheckAutoCompute();
        }

        public void SetDeadTime(double dblDeadTime, utmUnitsTimeConstants eUnits = utmUnitsTimeConstants.utmMinutes)
        {
            mCapillaryFlowParameters.ColumnDeadTime = ConvertTime(dblDeadTime, eUnits, utmUnitsTimeConstants.utmMinutes);
            CheckAutoCompute();
        }

        public void SetExtraColumnBroadeningAdditionalVariance(double dblAdditionalVarianceInSquareSeconds)
        {
            mExtraColumnBroadeningParameters.AdditionalTemporalVariance = dblAdditionalVarianceInSquareSeconds;
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningDiffusionCoefficient(double dblDiffusionCoefficient, udcDiffusionCoefficientConstants eUnits = udcDiffusionCoefficientConstants.udcCmSquaredPerSec)
        {
            mExtraColumnBroadeningParameters.DiffusionCoefficient = ConvertDiffusionCoefficient(dblDiffusionCoefficient, eUnits, udcDiffusionCoefficientConstants.udcCmSquaredPerSec);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningInitialPeakWidthAtBase(double dblWidth, utmUnitsTimeConstants eUnits = utmUnitsTimeConstants.utmSeconds)
        {
            mExtraColumnBroadeningParameters.InitialPeakWidth = ConvertTime(dblWidth, eUnits, utmUnitsTimeConstants.utmSeconds);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningLinearVelocity(double dblLinearVelocity, ulvUnitsLinearVelocityConstants eUnits = ulvUnitsLinearVelocityConstants.ulvMmPerMin)
        {
            mExtraColumnBroadeningParameters.LinearVelocity = ConvertLinearVelocity(dblLinearVelocity, eUnits, ulvUnitsLinearVelocityConstants.ulvCmPerMin);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningOpenTubeID(double dblOpenTubeID, ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnMicrons)
        {
            mExtraColumnBroadeningParameters.OpenTubeID = ConvertLength(dblOpenTubeID, eUnits, ulnUnitsLengthConstants.ulnCM);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningOpenTubeLength(double dblLength, ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnCM)
        {
            mExtraColumnBroadeningParameters.OpenTubeLength = ConvertLength(dblLength, eUnits, ulnUnitsLengthConstants.ulnCM);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetInterparticlePorosity(double dblPorosity)
        {
            if (dblPorosity >= 0d & dblPorosity <= 1d)
            {
                mCapillaryFlowParameters.InterparticlePorosity = dblPorosity;
            }

            CheckAutoCompute();
        }

        public void SetMassRateConcentration(double dblConcentration, ucoUnitsConcentrationConstants eUnits = ucoUnitsConcentrationConstants.ucoMicroMolar)
        {
            mMassRateParameters.SampleConcentration = ConvertConcentration(dblConcentration, eUnits, ucoUnitsConcentrationConstants.ucoMolar);
            ComputeMassRateValues();
        }

        public void SetMassRateInjectionTime(double dblInjectionTime, utmUnitsTimeConstants eUnits = utmUnitsTimeConstants.utmMinutes)
        {
            mMassRateParameters.InjectionTime = ConvertTime(dblInjectionTime, eUnits, utmUnitsTimeConstants.utmMinutes);
            ComputeMassRateValues();
        }

        public void SetMassRateSampleMass(double dblMassInGramsPerMole)
        {
            if (dblMassInGramsPerMole >= 0d)
            {
                mMassRateParameters.SampleMass = dblMassInGramsPerMole;
            }
            else
            {
                mMassRateParameters.SampleMass = 0d;
            }

            ComputeMassRateValues();
        }

        public void SetMassRateVolFlowRate(double dblVolFlowRate, ufrUnitsFlowRateConstants eUnits = ufrUnitsFlowRateConstants.ufrNLPerMin)
        {
            mMassRateParameters.VolumetricFlowRate = ConvertVolFlowRate(dblVolFlowRate, eUnits, ufrUnitsFlowRateConstants.ufrMLPerMin);
            ComputeMassRateValues();
        }

        public void SetParticleDiameter(double dblParticleDiameter, ulnUnitsLengthConstants eUnits = ulnUnitsLengthConstants.ulnMicrons)
        {
            mCapillaryFlowParameters.ParticleDiameter = ConvertLength(dblParticleDiameter, eUnits, ulnUnitsLengthConstants.ulnCM);
            CheckAutoCompute();
        }

        public void SetSolventViscosity(double dblSolventViscosity, uviUnitsViscosityConstants eUnits = uviUnitsViscosityConstants.uviPoise)
        {
            mCapillaryFlowParameters.SolventViscosity = ConvertViscosity(dblSolventViscosity, eUnits, uviUnitsViscosityConstants.uviPoise);
            CheckAutoCompute();
        }

        public void SetVolFlowRate(double dblVolFlowRate, ufrUnitsFlowRateConstants eUnits = ufrUnitsFlowRateConstants.ufrNLPerMin)
        {
            mCapillaryFlowParameters.VolumetricFlowRate = ConvertVolFlowRate(dblVolFlowRate, eUnits, ufrUnitsFlowRateConstants.ufrMLPerMin);
            CheckAutoCompute();
        }

        private void InitializeClass()
        {
            SetAutoComputeEnabled(false);
            SetAutoComputeMode(acmAutoComputeModeConstants.acmVolFlowRate);
            SetCapillaryType(ctCapillaryTypeConstants.ctPackedCapillary);
            SetBackPressure(3000d, uprUnitsPressureConstants.uprPsi);
            SetColumnLength(50d, ulnUnitsLengthConstants.ulnCM);
            SetColumnID(75d, ulnUnitsLengthConstants.ulnMicrons);
            SetSolventViscosity(0.0089d, uviUnitsViscosityConstants.uviPoise);
            SetParticleDiameter(5d, ulnUnitsLengthConstants.ulnMicrons);
            SetInterparticlePorosity(0.4d);
            SetMassRateConcentration(1d, ucoUnitsConcentrationConstants.ucoMicroMolar);
            SetMassRateVolFlowRate(600d, ufrUnitsFlowRateConstants.ufrNLPerMin);
            SetMassRateInjectionTime(5d, utmUnitsTimeConstants.utmMinutes);

            // Recompute
            ComputeVolFlowRate();
            ComputeMassRateValues();
            ComputeExtraColumnBroadeningValues();
            SetAutoComputeEnabled(true);
        }
    }
}