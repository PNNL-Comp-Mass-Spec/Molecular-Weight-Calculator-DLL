using System;

namespace MolecularWeightCalculator
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

        public CapillaryFlow()
        {
            InitializeClass();
        }

        #region "Enum Statements"

        public enum CapillaryType
        {
            OpenTubularCapillary = 0,
            PackedCapillary
        }

        public enum UnitOfPressure
        {
            Psi = 0,
            Pascals,
            KiloPascals,
            Atmospheres,
            Bar,
            Torr,
            DynesPerSquareCm
        }

        public enum UnitOfLength
        {
            M = 0,
            CM,
            MM,
            Microns,
            Inches
        }

        public enum UnitOfViscosity
        {
            Poise = 0,
            CentiPoise
        }

        public enum UnitOfFlowRate
        {
            MLPerMin = 0,
            ULPerMin,
            NLPerMin
        }

        public enum UnitOfLinearVelocity
        {
            CmPerHr = 0,
            MmPerHr,
            CmPerMin,
            MmPerMin,
            CmPerSec,
            MmPerSec
        }

        public enum UnitOfTime
        {
            Hours = 0,
            Minutes,
            Seconds
        }

        public enum UnitOfVolume
        {
            ML = 0,
            UL,
            NL,
            PL
        }

        public enum UnitOfConcentration
        {
            Molar = 0,
            MilliMolar,
            MicroMolar,
            NanoMolar,
            PicoMolar,
            FemtoMolar,
            AttoMolar,
            MgPerML,
            UgPerML,
            NgPerML,
            UgPerUL,
            NgPerUL
        }

        public enum UnitOfTemperature
        {
            Celsius = 0,
            Kelvin,
            Fahrenheit
        }

        public enum UnitOfMassFlowRate
        {
            PmolPerMin = 0,
            FmolPerMin,
            AmolPerMin,
            PmolPerSec,
            FmolPerSec,
            AmolPerSec,
            MolesPerMin
        }

        public enum UnitOfMolarAmount
        {
            Moles = 0,
            MilliMoles,
            MicroMoles,
            NanoMoles,
            PicoMoles,
            FemtoMoles,
            AttoMoles
        }

        public enum UnitOfDiffusionCoefficient
        {
            CmSquaredPerHr = 0,
            CmSquaredPerMin,
            CmSquaredPerSec
        }

        public enum AutoComputeMode
        {
            BackPressure = 0,
            ColumnID,
            ColumnLength,
            DeadTime,
            LinearVelocity,
            VolFlowRate,
            VolFlowRateUsingDeadTime
        }

        #endregion

        #region "Data classes"

        private class CapillaryFlowParameters
        {
            public CapillaryType CapillaryType;

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

        private class MassRateParameters
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

        private class ExtraColumnBroadeningParameters
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

        private readonly CapillaryFlowParameters mCapillaryFlowParameters = new CapillaryFlowParameters();
        private readonly MassRateParameters mMassRateParameters = new MassRateParameters();
        private readonly ExtraColumnBroadeningParameters mExtraColumnBroadeningParameters = new ExtraColumnBroadeningParameters();

        /// <summary>
        /// When true, automatically compute results whenever any value changes
        /// </summary>
        private bool mAutoCompute;

        /// <summary>
        /// The value to compute when mAutoCompute is true
        /// </summary>
        private AutoComputeMode mAutoComputeMode;

        private void CheckAutoCompute()
        {
            if (mAutoCompute)
            {
                switch (mAutoComputeMode)
                {
                    case AutoComputeMode.BackPressure:
                        ComputeBackPressure();
                        break;
                    case AutoComputeMode.ColumnID:
                        ComputeColumnID();
                        break;
                    case AutoComputeMode.ColumnLength:
                        ComputeColumnLength();
                        break;
                    case AutoComputeMode.DeadTime:
                        ComputeDeadTime();
                        break;
                    case AutoComputeMode.LinearVelocity:
                        ComputeLinearVelocity();
                        break;
                    case AutoComputeMode.VolFlowRateUsingDeadTime:
                        ComputeVolFlowRateUsingDeadTime(out _);
                        break;
                    default:
                        // Includes acmVolFlowRate
                        ComputeVolFlowRate();
                        break;
                }
            }
        }

        /// <summary>
        /// Computes the back pressure, stores in .BackPressure, and returns it
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeBackPressure(UnitOfPressure eUnits = UnitOfPressure.Psi)
        {
            double dblBackPressure;

            var dblRadius = mCapillaryFlowParameters.ColumnID / 2.0d;

            if (Math.Abs(dblRadius) > float.Epsilon)
            {
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    dblBackPressure = mCapillaryFlowParameters.VolumetricFlowRate * 8d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength / (Math.Pow(dblRadius, 4d) * PI * 60d); // Pressure in dynes/cm^2
                }
                // Packed capillary
                else if (Math.Abs(mCapillaryFlowParameters.ParticleDiameter) > float.Epsilon && Math.Abs(mCapillaryFlowParameters.InterparticlePorosity) > float.Epsilon)
                {
                    // Flow rate in mL/sec
                    dblBackPressure = mCapillaryFlowParameters.VolumetricFlowRate * 180d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength * Math.Pow(1d - mCapillaryFlowParameters.InterparticlePorosity, 2d) / (Math.Pow(mCapillaryFlowParameters.ParticleDiameter, 2d) * Math.Pow(mCapillaryFlowParameters.InterparticlePorosity, 2d) * PI * Math.Pow(dblRadius, 2d) * 60d) / mCapillaryFlowParameters.InterparticlePorosity;
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

            mCapillaryFlowParameters.BackPressure = dblBackPressure;

            // Compute Dead Time (and Linear Velocity)
            // Must send false for RecalculateVolFlowRate since we're finding the back pressure, not volumetric flow rate
            ComputeDeadTime(UnitOfTime.Minutes, false);

            // Return Back Pressure
            return ConvertPressure(dblBackPressure, UnitOfPressure.DynesPerSquareCm, eUnits);
        }

        /// <summary>
        /// Computes the column length, stores in .ColumnLength, and returns it
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeColumnLength(UnitOfLength eUnits = UnitOfLength.CM)
        {
            double dblColumnLength;

            var dblRadius = mCapillaryFlowParameters.ColumnID / 2.0d;

            if (Math.Abs(mCapillaryFlowParameters.SolventViscosity) > float.Epsilon && Math.Abs(mCapillaryFlowParameters.VolumetricFlowRate) > float.Epsilon)
            {
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    dblColumnLength = mCapillaryFlowParameters.BackPressure * Math.Pow(dblRadius, 4d) * PI * 60d / (8d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.VolumetricFlowRate); // Column length in cm
                }
                // Packed capillary
                else if (Math.Abs(mCapillaryFlowParameters.InterparticlePorosity - 1d) > float.Epsilon)
                {
                    // Flow rate in mL/sec
                    dblColumnLength = mCapillaryFlowParameters.BackPressure * Math.Pow(mCapillaryFlowParameters.ParticleDiameter, 2d) * Math.Pow(mCapillaryFlowParameters.InterparticlePorosity, 2d) * PI * Math.Pow(dblRadius, 2d) * 60d * mCapillaryFlowParameters.InterparticlePorosity / (180d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.VolumetricFlowRate * Math.Pow(1d - mCapillaryFlowParameters.InterparticlePorosity, 2d));
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

            mCapillaryFlowParameters.ColumnLength = dblColumnLength;

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(UnitOfTime.Minutes, true);

            // Return Column Length
            return ConvertLength(dblColumnLength, UnitOfLength.CM, eUnits);
        }

        public double ComputeColumnVolume(UnitOfVolume eUnits = 0)
        {
            // Computes the column volume and returns it (does not store it)

            var dblRadius = mCapillaryFlowParameters.ColumnID / 2.0d;

            var dblColumnVolume = mCapillaryFlowParameters.ColumnLength * PI * Math.Pow(dblRadius, 2d);

            if (mCapillaryFlowParameters.CapillaryType == CapillaryType.PackedCapillary)
            {
                dblColumnVolume *= mCapillaryFlowParameters.InterparticlePorosity;
            }

            return ConvertVolume(dblColumnVolume, UnitOfVolume.ML, eUnits);
        }

        /// <summary>
        /// Computes the column length, stores in .ColumnLength, and returns it
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeColumnID(UnitOfLength eUnits = UnitOfLength.Microns)
        {
            double dblRadius;

            if (Math.Abs(mCapillaryFlowParameters.BackPressure) > float.Epsilon)
            {
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    dblRadius = Math.Pow(mCapillaryFlowParameters.VolumetricFlowRate * 8d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength / (mCapillaryFlowParameters.BackPressure * PI * 60d), 0.25d);
                }
                // Packed capillary
                else if (Math.Abs(mCapillaryFlowParameters.ParticleDiameter) > float.Epsilon && Math.Abs(mCapillaryFlowParameters.InterparticlePorosity - 1d) > float.Epsilon)
                {
                    // Flow rate in mL/sec
                    dblRadius = Math.Pow(mCapillaryFlowParameters.VolumetricFlowRate * 180d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength * Math.Pow(1d - mCapillaryFlowParameters.InterparticlePorosity, 2d) / (mCapillaryFlowParameters.BackPressure * Math.Pow(mCapillaryFlowParameters.ParticleDiameter, 2d) * Math.Pow(mCapillaryFlowParameters.InterparticlePorosity, 2d) * PI * 60d) / mCapillaryFlowParameters.InterparticlePorosity, 0.5d);
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

            mCapillaryFlowParameters.ColumnID = dblRadius * 2.0d;

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(UnitOfTime.Minutes, true);

            // Return Column ID
            return ConvertLength(dblRadius * 2.0d, UnitOfLength.CM, eUnits);
        }

        /// <summary>
        /// Computes the column dead time, stores in .ColumnDeadTime, and returns it
        /// </summary>
        /// <param name="eUnits"></param>
        /// <param name="blnRecalculateVolFlowRate"></param>
        /// <returns></returns>
        public double ComputeDeadTime(UnitOfTime eUnits = UnitOfTime.Minutes, bool blnRecalculateVolFlowRate = true)
        {
            double dblDeadTime;

            // Dead time is dependent on Linear Velocity, so compute
            ComputeLinearVelocity(UnitOfLinearVelocity.CmPerSec, blnRecalculateVolFlowRate);

            if (Math.Abs(mCapillaryFlowParameters.LinearVelocity) > float.Epsilon)
            {
                dblDeadTime = mCapillaryFlowParameters.ColumnLength / mCapillaryFlowParameters.LinearVelocity; // Dead time in minutes
            }
            else
            {
                dblDeadTime = 0d;
            }

            mCapillaryFlowParameters.ColumnDeadTime = dblDeadTime;

            // Return Dead Time
            return ConvertTime(dblDeadTime, UnitOfTime.Minutes, eUnits);
        }

        public double ComputeExtraColumnBroadeningResultantPeakWidth(UnitOfTime eUnits = UnitOfTime.Seconds)
        {
            ComputeExtraColumnBroadeningValues();

            return GetExtraColumnBroadeningResultantPeakWidth(eUnits);
        }

        private void ComputeExtraColumnBroadeningValues()
        {
            if (Math.Abs(mExtraColumnBroadeningParameters.LinearVelocity) > float.Epsilon && Math.Abs(mExtraColumnBroadeningParameters.DiffusionCoefficient) > float.Epsilon)
            {
                mExtraColumnBroadeningParameters.TemporalVariance = Math.Pow(mExtraColumnBroadeningParameters.OpenTubeID, 2d) * mExtraColumnBroadeningParameters.OpenTubeLength / (96d * mExtraColumnBroadeningParameters.DiffusionCoefficient * mExtraColumnBroadeningParameters.LinearVelocity / 60d); // in sec^2
            }
            else
            {
                mExtraColumnBroadeningParameters.TemporalVariance = 0d;
            }

            var dblInitialPeakVariance = Math.Pow(mExtraColumnBroadeningParameters.InitialPeakWidth / 4d, 2d);

            var dblSumOfVariances = dblInitialPeakVariance + mExtraColumnBroadeningParameters.TemporalVariance + mExtraColumnBroadeningParameters.AdditionalTemporalVariance;

            if (dblSumOfVariances >= 0d)
            {
                // ResultantPeakWidth at the base = 4 sigma  and  sigma = Sqr(Total_Variance)
                mExtraColumnBroadeningParameters.ResultantPeakWidth = 4d * Math.Sqrt(dblSumOfVariances);
            }
            else
            {
                mExtraColumnBroadeningParameters.ResultantPeakWidth = 0d;
            }
        }

        /// <summary>
        /// Computes the Linear velocity, stores in .LinearVelocity, and returns it
        /// </summary>
        /// <param name="eUnits"></param>
        /// <param name="blnRecalculateVolFlowRate"></param>
        /// <returns></returns>
        public double ComputeLinearVelocity(UnitOfLinearVelocity eUnits = UnitOfLinearVelocity.CmPerSec, bool blnRecalculateVolFlowRate = true)
        {
            double dblLinearVelocity;

            if (blnRecalculateVolFlowRate)
            {
                ComputeVolFlowRate(UnitOfFlowRate.MLPerMin);
            }

            var dblRadius = mCapillaryFlowParameters.ColumnID / 2.0d;
            if (Math.Abs(dblRadius) > float.Epsilon)
            {
                dblLinearVelocity = mCapillaryFlowParameters.VolumetricFlowRate / (PI * Math.Pow(dblRadius, 2d)); // Units in cm/min

                // Divide Linear Velocity by epsilon if a packed capillary
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.PackedCapillary && Math.Abs(mCapillaryFlowParameters.InterparticlePorosity) > float.Epsilon)
                {
                    dblLinearVelocity /= mCapillaryFlowParameters.InterparticlePorosity;
                }
            }
            else
            {
                dblLinearVelocity = 0d;
            }

            mCapillaryFlowParameters.LinearVelocity = dblLinearVelocity;

            // Return Linear Velocity
            return ConvertLinearVelocity(dblLinearVelocity, UnitOfLinearVelocity.CmPerMin, eUnits);
        }

        /// <summary>
        /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeMassFlowRate(UnitOfMassFlowRate eUnits = UnitOfMassFlowRate.FmolPerSec)
        {
            ComputeMassRateValues();
            return GetMassFlowRate(eUnits);
        }

        /// <summary>
        /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeMassRateMolesInjected(UnitOfMolarAmount eUnits = UnitOfMolarAmount.FemtoMoles)
        {
            ComputeMassRateValues();
            return GetMassRateMolesInjected(eUnits);
        }

        private void ComputeMassRateValues()
        {
            mMassRateParameters.MassFlowRate = mMassRateParameters.SampleConcentration * mMassRateParameters.VolumetricFlowRate / 1000d; // Compute mass flow rate in moles/min
            mMassRateParameters.MolesInjected = mMassRateParameters.MassFlowRate * mMassRateParameters.InjectionTime; // Compute moles injected in moles
        }

        /// <summary>
        /// Computes the optimum linear velocity, based on
        /// mCapillaryFlowParameters.ParticleDiameter
        /// and mExtraColumnBroadeningParameters.DiffusionCoefficient
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(UnitOfLinearVelocity eUnits = UnitOfLinearVelocity.CmPerSec)
        {
            var dblOptimumLinearVelocity = default(double);

            if (Math.Abs(mCapillaryFlowParameters.ParticleDiameter) > float.Epsilon)
            {
                dblOptimumLinearVelocity = 3d * mExtraColumnBroadeningParameters.DiffusionCoefficient / mCapillaryFlowParameters.ParticleDiameter;

                dblOptimumLinearVelocity = ConvertLinearVelocity(dblOptimumLinearVelocity, UnitOfLinearVelocity.CmPerSec, eUnits);
            }

            return dblOptimumLinearVelocity;
        }

        public double ComputeMeCNViscosity(double dblPercentAcetonitrile, double dblTemperature, UnitOfTemperature eTemperatureUnits = UnitOfTemperature.Celsius, UnitOfViscosity eViscosityUnits = UnitOfViscosity.Poise)
        {
            try
            {
                var dblPhi = dblPercentAcetonitrile / 100.0d; // Fraction Acetonitrile
                if (dblPhi < 0d)
                    dblPhi = 0d;

                if (dblPhi > 100d)
                    dblPhi = 100d;

                var dblKelvin = ConvertTemperature(dblTemperature, eTemperatureUnits, UnitOfTemperature.Kelvin);

                double dblViscosityInCentiPoise;
                if (dblKelvin > 0d)
                {
                    dblViscosityInCentiPoise = Math.Exp(dblPhi * (-3.476d + 726d / dblKelvin) + (1d - dblPhi) * (-5.414d + 1566d / dblKelvin) + dblPhi * (1d - dblPhi) * (-1.762d + 929d / dblKelvin));
                }
                else
                {
                    dblViscosityInCentiPoise = 0d;
                }

                return ConvertViscosity(dblViscosityInCentiPoise, UnitOfViscosity.CentiPoise, eViscosityUnits);
            }
            catch
            {
                return 0d;
            }
        }

        /// <summary>
        /// Computes the Volumetric flow rate, stores in .VolumetricFlowRate, and returns it
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        public double ComputeVolFlowRate(UnitOfFlowRate eUnits = UnitOfFlowRate.NLPerMin)
        {
            double dblVolFlowRate;

            var dblRadius = mCapillaryFlowParameters.ColumnID / 2.0d;

            if (Math.Abs(mCapillaryFlowParameters.SolventViscosity) > float.Epsilon && Math.Abs(mCapillaryFlowParameters.ColumnLength) > float.Epsilon)
            {
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    dblVolFlowRate = mCapillaryFlowParameters.BackPressure * Math.Pow(dblRadius, 4d) * PI / (8d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength); // Flow rate in mL/sec
                }
                // Packed capillary
                else if (Math.Abs(mCapillaryFlowParameters.InterparticlePorosity - 1d) > float.Epsilon)
                {
                    // Flow rate in mL/sec
                    dblVolFlowRate = mCapillaryFlowParameters.BackPressure * Math.Pow(mCapillaryFlowParameters.ParticleDiameter, 2d) * Math.Pow(mCapillaryFlowParameters.InterparticlePorosity, 2d) * PI * Math.Pow(dblRadius, 2d) * mCapillaryFlowParameters.InterparticlePorosity / (180d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength * Math.Pow(1d - mCapillaryFlowParameters.InterparticlePorosity, 2d));
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

            mCapillaryFlowParameters.VolumetricFlowRate = dblVolFlowRate;

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(UnitOfTime.Minutes, false);

            return ConvertVolFlowRate(dblVolFlowRate, UnitOfFlowRate.MLPerMin, eUnits);
        }

        /// <summary>
        /// Computes the Volumetric flow rate using the dead time, stores in .VolumetricFlowRate, and returns it
        /// This requires modifying the pressure value to give the computed volumetric flow rate
        /// </summary>
        /// <param name="dblNewBackPressure">Output: new back pressure</param>
        /// <param name="eUnits"></param>
        /// <param name="ePressureUnits"></param>
        /// <returns></returns>
        public double ComputeVolFlowRateUsingDeadTime(
            out double dblNewBackPressure,
            UnitOfFlowRate eUnits = UnitOfFlowRate.NLPerMin,
            UnitOfPressure ePressureUnits = UnitOfPressure.Psi)
        {
            dblNewBackPressure = 0;
            double dblVolFlowRate;

            var dblRadius = mCapillaryFlowParameters.ColumnID / 2.0d;

            // First find vol flow rate that gives observed dead time
            if (Math.Abs(mCapillaryFlowParameters.ColumnDeadTime) > float.Epsilon)
            {
                dblVolFlowRate = mCapillaryFlowParameters.ColumnLength * (PI * Math.Pow(dblRadius, 2d)) / mCapillaryFlowParameters.ColumnDeadTime; // Vol flow rate in mL/sec

                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.PackedCapillary)
                {
                    // Packed Capillary
                    dblVolFlowRate *= mCapillaryFlowParameters.InterparticlePorosity;
                }

                // Store the new value
                mCapillaryFlowParameters.VolumetricFlowRate = dblVolFlowRate;

                // Now find pressure that gives computed dblVolFlowRate
                // The ComputeBackPressure sub will store the new pressure
                dblNewBackPressure = ComputeBackPressure(ePressureUnits);
            }
            else
            {
                dblVolFlowRate = 0d;
                mCapillaryFlowParameters.VolumetricFlowRate = 0d;
            }

            // Compute Linear Velocity (but not the dead time)
            ComputeLinearVelocity(UnitOfLinearVelocity.CmPerSec, false);

            return ConvertVolFlowRate(dblVolFlowRate, UnitOfFlowRate.MLPerMin, eUnits);
        }

        /// <summary>
        /// Convert concentration
        /// </summary>
        /// <param name="dblConcentrationIn"></param>
        /// <param name="eCurrentUnits"></param>
        /// <param name="eNewUnits"></param>
        /// <returns></returns>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        public double ConvertConcentration(double dblConcentrationIn, UnitOfConcentration eCurrentUnits, UnitOfConcentration eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblConcentrationIn;
            }

            var dblSampleMass = mMassRateParameters.SampleMass;

            var dblFactor = FactorConcentration(eCurrentUnits, dblSampleMass);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblConcentrationIn * dblFactor;

            dblFactor = FactorConcentration(eNewUnits, dblSampleMass);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertDiffusionCoefficient(double dblDiffusionCoefficientIn, UnitOfDiffusionCoefficient eCurrentUnits, UnitOfDiffusionCoefficient eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblDiffusionCoefficientIn;
            }

            var dblFactor = FactorDiffusionCoeff(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblDiffusionCoefficientIn * dblFactor;

            dblFactor = FactorDiffusionCoeff(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertLength(double dblLengthIn, UnitOfLength eCurrentUnits, UnitOfLength eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblLengthIn;
            }

            var dblFactor = FactorLength(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblLengthIn * dblFactor;

            dblFactor = FactorLength(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertLinearVelocity(double dblLinearVelocityIn, UnitOfLinearVelocity eCurrentUnits, UnitOfLinearVelocity eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblLinearVelocityIn;
            }

            var dblFactor = FactorLinearVelocity(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblLinearVelocityIn * dblFactor;

            dblFactor = FactorLinearVelocity(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertMassFlowRate(double dblMassFlowRateIn, UnitOfMassFlowRate eCurrentUnits, UnitOfMassFlowRate eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblMassFlowRateIn;
            }

            var dblFactor = FactorMassFlowRate(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblMassFlowRateIn * dblFactor;

            dblFactor = FactorMassFlowRate(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertMoles(double dblMolesIn, UnitOfMolarAmount eCurrentUnits, UnitOfMolarAmount eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblMolesIn;
            }

            var dblFactor = FactorMoles(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblMolesIn * dblFactor;

            dblFactor = FactorMoles(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertPressure(double dblPressureIn, UnitOfPressure eCurrentUnits, UnitOfPressure eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblPressureIn;
            }

            var dblFactor = FactorPressure(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblPressureIn * dblFactor;

            dblFactor = FactorPressure(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertTemperature(double dblTemperatureIn, UnitOfTemperature eCurrentUnits, UnitOfTemperature eNewUnits)
        {
            var dblValue = default(double);
            if (eCurrentUnits == eNewUnits)
            {
                return dblTemperatureIn;
            }

            // First convert to Kelvin
            switch (eCurrentUnits)
            {
                case UnitOfTemperature.Celsius:
                    // K = C + 273
                    dblValue = dblTemperatureIn + 273d;
                    break;
                case UnitOfTemperature.Fahrenheit:
                    // Convert to Kelvin: C = 5/9*(F-32) and K = C + 273
                    dblValue = 5.0d / 9.0d * (dblTemperatureIn - 32d) + 273d;
                    break;
                default:
                    // Includes utpKelvin
                    // Assume already Kelvin
                    break;
            }

            // We cannot get colder than absolute 0
            if (dblValue < 0d)
                dblValue = 0d;

            // Now convert to the target units
            switch (eNewUnits)
            {
                case UnitOfTemperature.Celsius:
                    // C = K - 273
                    dblValue -= 273d;
                    break;
                case UnitOfTemperature.Fahrenheit:
                    // Convert to Fahrenheit: C = K - 273 and F = (9/5)C + 32
                    dblValue = 9.0d / 5.0d * (dblValue - 273d) + 32d;
                    break;
                default:
                    // Includes utpKelvin
                    // Already in Kelvin
                    break;
            }

            return dblValue;
        }

        public double ConvertTime(double dblTimeIn, UnitOfTime eCurrentUnits, UnitOfTime eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblTimeIn;
            }

            var dblFactor = FactorTime(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblTimeIn * dblFactor;

            dblFactor = FactorTime(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertViscosity(double dblViscosityIn, UnitOfViscosity eCurrentUnits, UnitOfViscosity eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblViscosityIn;
            }

            var dblFactor = FactorViscosity(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblViscosityIn * dblFactor;

            dblFactor = FactorViscosity(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertVolFlowRate(double dblVolFlowRateIn, UnitOfFlowRate eCurrentUnits, UnitOfFlowRate eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblVolFlowRateIn;
            }

            var dblFactor = FactorVolFlowRate(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblVolFlowRateIn * dblFactor;

            dblFactor = FactorVolFlowRate(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        public double ConvertVolume(double dblVolume, UnitOfVolume eCurrentUnits, UnitOfVolume eNewUnits)
        {
            if (eCurrentUnits == eNewUnits)
            {
                return dblVolume;
            }

            var dblFactor = FactorVolume(eCurrentUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var dblValue = dblVolume * dblFactor;

            dblFactor = FactorVolume(eNewUnits);
            if (Math.Abs(dblFactor + 1d) < float.Epsilon || Math.Abs(dblFactor) < float.Epsilon)
            {
                return -1;
            }

            return dblValue / dblFactor;
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to M
        /// dblSampleMass is required for mass-based units
        /// </summary>
        /// <param name="eUnits"></param>
        /// <param name="dblSampleMass"></param>
        /// <returns></returns>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        private double FactorConcentration(UnitOfConcentration eUnits, double dblSampleMass = 0d)
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
                    case UnitOfConcentration.Molar:
                        dblFactor = 1.0d;
                        break;
                    case UnitOfConcentration.MilliMolar:
                        dblFactor = 1d / 1000.0d;
                        break;
                    case UnitOfConcentration.MicroMolar:
                        dblFactor = 1d / 1000000.0d;
                        break;
                    case UnitOfConcentration.NanoMolar:
                        dblFactor = 1d / 1000000000.0d;
                        break;
                    case UnitOfConcentration.PicoMolar:
                        dblFactor = 1d / 1000000000000.0d;
                        break;
                    case UnitOfConcentration.FemtoMolar:
                        dblFactor = 1d / 1.0E+15d;
                        break;
                    case UnitOfConcentration.AttoMolar:
                        dblFactor = 1d / 1.0E+18d;
                        break;
                    case UnitOfConcentration.MgPerML:
                        dblFactor = 1d / dblSampleMass; // 1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                        break;
                    case UnitOfConcentration.UgPerML:
                        dblFactor = 1d / (dblSampleMass * 1000.0d); // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                        break;
                    case UnitOfConcentration.NgPerML:
                        dblFactor = 1d / (dblSampleMass * 1000000.0d); // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                        break;
                    case UnitOfConcentration.UgPerUL:
                        dblFactor = 1d / dblSampleMass; // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                        break;
                    case UnitOfConcentration.NgPerUL:
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
        /// Multiplication factor for converting from eUnits to Cm
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorLength(UnitOfLength eUnits)
        {
            switch (eUnits)
            {
                case UnitOfLength.M:
                    return 100.0d;
                case UnitOfLength.CM:
                    return 1.0d;
                case UnitOfLength.MM:
                    return 1d / 10.0d;
                case UnitOfLength.Microns:
                    return 1d / 10000.0d;
                case UnitOfLength.Inches:
                    return CM_PER_INCH;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to Cm/Min
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorLinearVelocity(UnitOfLinearVelocity eUnits)
        {
            switch (eUnits)
            {
                case UnitOfLinearVelocity.CmPerHr:
                    return 1d / 60.0d;
                case UnitOfLinearVelocity.MmPerHr:
                    return 1d / 60.0d / 10.0d;
                case UnitOfLinearVelocity.CmPerMin:
                    return 1d;
                case UnitOfLinearVelocity.MmPerMin:
                    return 1d / 10.0d;
                case UnitOfLinearVelocity.CmPerSec:
                    return 60.0d;
                case UnitOfLinearVelocity.MmPerSec:
                    return 60.0d / 10.0d;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to moles/min
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorMassFlowRate(UnitOfMassFlowRate eUnits)
        {
            switch (eUnits)
            {
                case UnitOfMassFlowRate.PmolPerMin:
                    return 1d / 1000000000000.0d;
                case UnitOfMassFlowRate.FmolPerMin:
                    return 1d / 1.0E+15d;
                case UnitOfMassFlowRate.AmolPerMin:
                    return 1d / 1.0E+18d;
                case UnitOfMassFlowRate.PmolPerSec:
                    return 1d / (1000000000000.0d / 60.0d);
                case UnitOfMassFlowRate.FmolPerSec:
                    return 1d / (1.0E+15d / 60.0d);
                case UnitOfMassFlowRate.AmolPerSec:
                    return 1d / (1.0E+18d / 60.0d);
                case UnitOfMassFlowRate.MolesPerMin:
                    return 1.0d;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to moles
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorMoles(UnitOfMolarAmount eUnits)
        {
            switch (eUnits)
            {
                case UnitOfMolarAmount.Moles:
                    return 1.0d;
                case UnitOfMolarAmount.MilliMoles:
                    return 1d / 1000.0d;
                case UnitOfMolarAmount.MicroMoles:
                    return 1d / 1000000.0d;
                case UnitOfMolarAmount.NanoMoles:
                    return 1d / 1000000000.0d;
                case UnitOfMolarAmount.PicoMoles:
                    return 1d / 1000000000000.0d;
                case UnitOfMolarAmount.FemtoMoles:
                    return 1d / 1.0E+15d;
                case UnitOfMolarAmount.AttoMoles:
                    return 1d / 1.0E+18d;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to dynes per cm^2
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorPressure(UnitOfPressure eUnits)
        {
            switch (eUnits)
            {
                case UnitOfPressure.Psi:
                    return 68947.57d;
                case UnitOfPressure.Pascals:
                    return 10.0d;
                case UnitOfPressure.KiloPascals:
                    return 10000.0d;
                case UnitOfPressure.Atmospheres:
                    return 1013250.0d;
                case UnitOfPressure.Bar:
                    return 1000000.0d;
                case UnitOfPressure.Torr:
                    return 1333.22d;
                case UnitOfPressure.DynesPerSquareCm:
                    return 1d;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to minutes
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorTime(UnitOfTime eUnits)
        {
            switch (eUnits)
            {
                case UnitOfTime.Hours:
                    return 60.0d;
                case UnitOfTime.Minutes:
                    return 1.0d;
                case UnitOfTime.Seconds:
                    return 1d / 60.0d;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to cm^2/sec
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorDiffusionCoeff(UnitOfDiffusionCoefficient eUnits)
        {
            switch (eUnits)
            {
                case UnitOfDiffusionCoefficient.CmSquaredPerHr:
                    return 1d / 3600.0d;
                case UnitOfDiffusionCoefficient.CmSquaredPerMin:
                    return 1d / 60.0d;
                case UnitOfDiffusionCoefficient.CmSquaredPerSec:
                    return 1.0d;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to poise
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorViscosity(UnitOfViscosity eUnits)
        {
            switch (eUnits)
            {
                case UnitOfViscosity.Poise:
                    return 1.0d;
                case UnitOfViscosity.CentiPoise:
                    return 1d / 100.0d;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to mL/min
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorVolFlowRate(UnitOfFlowRate eUnits)
        {
            switch (eUnits)
            {
                case UnitOfFlowRate.MLPerMin:
                    return 1.0d;
                case UnitOfFlowRate.ULPerMin:
                    return 1d / 1000.0d;
                case UnitOfFlowRate.NLPerMin:
                    return 1d / 1000000.0d;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Multiplication factor for converting from eUnits to mL
        /// </summary>
        /// <param name="eUnits"></param>
        /// <returns></returns>
        private double FactorVolume(UnitOfVolume eUnits)
        {
            switch (eUnits)
            {
                case UnitOfVolume.ML:
                    return 1.0d;
                case UnitOfVolume.UL:
                    return 1d / 1000.0d;
                case UnitOfVolume.NL:
                    return 1d / 1000000.0d;
                case UnitOfVolume.PL:
                    return 1d / 1000000000.0d;
                default:
                    return -1;
            }
        }

        // Get Methods
        // Gets the most recently computed value
        // If mAutoCompute = False, then must manually call a Compute Sub to recompute the value

        public bool GetAutoComputeEnabled()
        {
            return mAutoCompute;
        }

        public AutoComputeMode GetAutoComputeMode()
        {
            return mAutoComputeMode;
        }

        public double GetBackPressure(UnitOfPressure eUnits = UnitOfPressure.Psi)
        {
            return ConvertPressure(mCapillaryFlowParameters.BackPressure, UnitOfPressure.DynesPerSquareCm, eUnits);
        }

        public CapillaryType GetCapillaryType()
        {
            return mCapillaryFlowParameters.CapillaryType;
        }

        public double GetColumnID(UnitOfLength eUnits = UnitOfLength.Microns)
        {
            return ConvertLength(mCapillaryFlowParameters.ColumnID, UnitOfLength.CM, eUnits);
        }

        public double GetColumnLength(UnitOfLength eUnits = UnitOfLength.CM)
        {
            return ConvertLength(mCapillaryFlowParameters.ColumnLength, UnitOfLength.CM, eUnits);
        }

        public double GetColumnVolume(UnitOfVolume eUnits = UnitOfVolume.UL)
        {
            // Column volume isn't stored; simply re-compute it
            return ComputeColumnVolume(eUnits);
        }

        public double GetDeadTime(UnitOfTime eUnits = UnitOfTime.Minutes)
        {
            return ConvertTime(mCapillaryFlowParameters.ColumnDeadTime, UnitOfTime.Minutes, eUnits);
        }

        public double GetExtraColumnBroadeningAdditionalVarianceInSquareSeconds()
        {
            return mExtraColumnBroadeningParameters.AdditionalTemporalVariance;
        }

        public double GetExtraColumnBroadeningDiffusionCoefficient(UnitOfDiffusionCoefficient eUnits = UnitOfDiffusionCoefficient.CmSquaredPerSec)
        {
            return ConvertDiffusionCoefficient(mExtraColumnBroadeningParameters.DiffusionCoefficient, UnitOfDiffusionCoefficient.CmSquaredPerSec, eUnits);
        }

        public double GetExtraColumnBroadeningInitialPeakWidthAtBase(UnitOfTime eUnits = UnitOfTime.Seconds)
        {
            return ConvertTime(mExtraColumnBroadeningParameters.InitialPeakWidth, UnitOfTime.Seconds, eUnits);
        }

        public double GetExtraColumnBroadeningLinearVelocity(UnitOfLinearVelocity eUnits = UnitOfLinearVelocity.MmPerMin)
        {
            return ConvertLinearVelocity(mExtraColumnBroadeningParameters.LinearVelocity, UnitOfLinearVelocity.CmPerMin, eUnits);
        }

        public double GetExtraColumnBroadeningOpenTubeID(UnitOfLength eUnits = UnitOfLength.Microns)
        {
            return ConvertLength(mExtraColumnBroadeningParameters.OpenTubeID, UnitOfLength.CM, eUnits);
        }

        public double GetExtraColumnBroadeningOpenTubeLength(UnitOfLength eUnits = UnitOfLength.CM)
        {
            return ConvertLength(mExtraColumnBroadeningParameters.OpenTubeLength, UnitOfLength.CM, eUnits);
        }

        public double GetExtraColumnBroadeningResultantPeakWidth(UnitOfTime eUnits = UnitOfTime.Seconds)
        {
            return ConvertTime(mExtraColumnBroadeningParameters.ResultantPeakWidth, UnitOfTime.Seconds, eUnits);
        }

        public double GetExtraColumnBroadeningTemporalVarianceInSquareSeconds()
        {
            return mExtraColumnBroadeningParameters.TemporalVariance;
        }

        public double GetInterparticlePorosity()
        {
            return mCapillaryFlowParameters.InterparticlePorosity;
        }

        public double GetLinearVelocity(UnitOfLinearVelocity eUnits = UnitOfLinearVelocity.CmPerSec)
        {
            return ConvertLinearVelocity(mCapillaryFlowParameters.LinearVelocity, UnitOfLinearVelocity.CmPerMin, eUnits);
        }

        public double GetMassRateConcentration(UnitOfConcentration eUnits = UnitOfConcentration.MicroMolar)
        {
            return ConvertConcentration(mMassRateParameters.SampleConcentration, UnitOfConcentration.Molar, eUnits);
        }

        public double GetMassRateInjectionTime(UnitOfTime eUnits = UnitOfTime.Minutes)
        {
            return ConvertTime(mMassRateParameters.InjectionTime, UnitOfTime.Minutes, eUnits);
        }

        public double GetMassFlowRate(UnitOfMassFlowRate eUnits = UnitOfMassFlowRate.FmolPerSec)
        {
            return ConvertMassFlowRate(mMassRateParameters.MassFlowRate, UnitOfMassFlowRate.MolesPerMin, eUnits);
        }

        public double GetMassRateMolesInjected(UnitOfMolarAmount eUnits = UnitOfMolarAmount.FemtoMoles)
        {
            return ConvertMoles(mMassRateParameters.MolesInjected, UnitOfMolarAmount.Moles, eUnits);
        }

        public double GetMassRateSampleMass()
        {
            return mMassRateParameters.SampleMass;
        }

        public double GetMassRateVolFlowRate(UnitOfFlowRate eUnits = UnitOfFlowRate.NLPerMin)
        {
            return ConvertVolFlowRate(mMassRateParameters.VolumetricFlowRate, UnitOfFlowRate.MLPerMin, eUnits);
        }

        public double GetParticleDiameter(UnitOfLength eUnits = UnitOfLength.Microns)
        {
            return ConvertLength(mCapillaryFlowParameters.ParticleDiameter, UnitOfLength.CM, eUnits);
        }

        public double GetSolventViscosity(UnitOfViscosity eUnits = UnitOfViscosity.Poise)
        {
            return ConvertViscosity(mCapillaryFlowParameters.SolventViscosity, UnitOfViscosity.Poise, eUnits);
        }

        public double GetVolFlowRate(UnitOfFlowRate eUnits = UnitOfFlowRate.NLPerMin)
        {
            return ConvertVolFlowRate(mCapillaryFlowParameters.VolumetricFlowRate, UnitOfFlowRate.MLPerMin, eUnits);
        }


        // Set Methods
        // If mAutoCompute = False, then must manually call a Compute Sub to recompute other values

        public void SetAutoComputeEnabled(bool blnAutoCompute)
        {
            mAutoCompute = blnAutoCompute;
        }

        public void SetAutoComputeMode(AutoComputeMode eAutoComputeMode)
        {
            if (eAutoComputeMode >= AutoComputeMode.BackPressure && eAutoComputeMode <= AutoComputeMode.VolFlowRateUsingDeadTime)
            {
                mAutoComputeMode = eAutoComputeMode;
            }
        }

        public void SetBackPressure(double dblBackPressure, UnitOfPressure eUnits = UnitOfPressure.Psi)
        {
            mCapillaryFlowParameters.BackPressure = ConvertPressure(dblBackPressure, eUnits, UnitOfPressure.DynesPerSquareCm);
            CheckAutoCompute();
        }

        public void SetCapillaryType(CapillaryType eCapillaryType)
        {
            if (eCapillaryType >= CapillaryType.OpenTubularCapillary && eCapillaryType <= CapillaryType.PackedCapillary)
            {
                mCapillaryFlowParameters.CapillaryType = eCapillaryType;
            }

            CheckAutoCompute();
        }

        public void SetColumnID(double dblColumnID, UnitOfLength eUnits = UnitOfLength.Microns)
        {
            mCapillaryFlowParameters.ColumnID = ConvertLength(dblColumnID, eUnits, UnitOfLength.CM);
            CheckAutoCompute();
        }

        public void SetColumnLength(double dblColumnLength, UnitOfLength eUnits = UnitOfLength.CM)
        {
            mCapillaryFlowParameters.ColumnLength = ConvertLength(dblColumnLength, eUnits, UnitOfLength.CM);
            CheckAutoCompute();
        }

        public void SetDeadTime(double dblDeadTime, UnitOfTime eUnits = UnitOfTime.Minutes)
        {
            mCapillaryFlowParameters.ColumnDeadTime = ConvertTime(dblDeadTime, eUnits, UnitOfTime.Minutes);
            CheckAutoCompute();
        }

        public void SetExtraColumnBroadeningAdditionalVariance(double dblAdditionalVarianceInSquareSeconds)
        {
            mExtraColumnBroadeningParameters.AdditionalTemporalVariance = dblAdditionalVarianceInSquareSeconds;
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningDiffusionCoefficient(double dblDiffusionCoefficient, UnitOfDiffusionCoefficient eUnits = UnitOfDiffusionCoefficient.CmSquaredPerSec)
        {
            mExtraColumnBroadeningParameters.DiffusionCoefficient = ConvertDiffusionCoefficient(dblDiffusionCoefficient, eUnits, UnitOfDiffusionCoefficient.CmSquaredPerSec);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningInitialPeakWidthAtBase(double dblWidth, UnitOfTime eUnits = UnitOfTime.Seconds)
        {
            mExtraColumnBroadeningParameters.InitialPeakWidth = ConvertTime(dblWidth, eUnits, UnitOfTime.Seconds);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningLinearVelocity(double dblLinearVelocity, UnitOfLinearVelocity eUnits = UnitOfLinearVelocity.MmPerMin)
        {
            mExtraColumnBroadeningParameters.LinearVelocity = ConvertLinearVelocity(dblLinearVelocity, eUnits, UnitOfLinearVelocity.CmPerMin);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningOpenTubeID(double dblOpenTubeID, UnitOfLength eUnits = UnitOfLength.Microns)
        {
            mExtraColumnBroadeningParameters.OpenTubeID = ConvertLength(dblOpenTubeID, eUnits, UnitOfLength.CM);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningOpenTubeLength(double dblLength, UnitOfLength eUnits = UnitOfLength.CM)
        {
            mExtraColumnBroadeningParameters.OpenTubeLength = ConvertLength(dblLength, eUnits, UnitOfLength.CM);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetInterparticlePorosity(double dblPorosity)
        {
            if (dblPorosity >= 0d && dblPorosity <= 1d)
            {
                mCapillaryFlowParameters.InterparticlePorosity = dblPorosity;
            }

            CheckAutoCompute();
        }

        public void SetMassRateConcentration(double dblConcentration, UnitOfConcentration eUnits = UnitOfConcentration.MicroMolar)
        {
            mMassRateParameters.SampleConcentration = ConvertConcentration(dblConcentration, eUnits, UnitOfConcentration.Molar);
            ComputeMassRateValues();
        }

        public void SetMassRateInjectionTime(double dblInjectionTime, UnitOfTime eUnits = UnitOfTime.Minutes)
        {
            mMassRateParameters.InjectionTime = ConvertTime(dblInjectionTime, eUnits, UnitOfTime.Minutes);
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

        public void SetMassRateVolFlowRate(double dblVolFlowRate, UnitOfFlowRate eUnits = UnitOfFlowRate.NLPerMin)
        {
            mMassRateParameters.VolumetricFlowRate = ConvertVolFlowRate(dblVolFlowRate, eUnits, UnitOfFlowRate.MLPerMin);
            ComputeMassRateValues();
        }

        public void SetParticleDiameter(double dblParticleDiameter, UnitOfLength eUnits = UnitOfLength.Microns)
        {
            mCapillaryFlowParameters.ParticleDiameter = ConvertLength(dblParticleDiameter, eUnits, UnitOfLength.CM);
            CheckAutoCompute();
        }

        public void SetSolventViscosity(double dblSolventViscosity, UnitOfViscosity eUnits = UnitOfViscosity.Poise)
        {
            mCapillaryFlowParameters.SolventViscosity = ConvertViscosity(dblSolventViscosity, eUnits, UnitOfViscosity.Poise);
            CheckAutoCompute();
        }

        public void SetVolFlowRate(double dblVolFlowRate, UnitOfFlowRate eUnits = UnitOfFlowRate.NLPerMin)
        {
            mCapillaryFlowParameters.VolumetricFlowRate = ConvertVolFlowRate(dblVolFlowRate, eUnits, UnitOfFlowRate.MLPerMin);
            CheckAutoCompute();
        }

        private void InitializeClass()
        {
            SetAutoComputeEnabled(false);

            SetAutoComputeMode(AutoComputeMode.VolFlowRate);
            SetCapillaryType(CapillaryType.PackedCapillary);
            SetBackPressure(3000d, UnitOfPressure.Psi);
            SetColumnLength(50d, UnitOfLength.CM);
            SetColumnID(75d, UnitOfLength.Microns);
            SetSolventViscosity(0.0089d, UnitOfViscosity.Poise);
            SetParticleDiameter(5d, UnitOfLength.Microns);
            SetInterparticlePorosity(0.4d);

            SetMassRateConcentration(1d, UnitOfConcentration.MicroMolar);
            SetMassRateVolFlowRate(600d, UnitOfFlowRate.NLPerMin);
            SetMassRateInjectionTime(5d, UnitOfTime.Minutes);

            // Recompute
            ComputeVolFlowRate();
            ComputeMassRateValues();
            ComputeExtraColumnBroadeningValues();

            SetAutoComputeEnabled(true);
        }
    }
}