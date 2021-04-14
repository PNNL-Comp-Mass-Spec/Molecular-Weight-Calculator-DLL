using System;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator
{
    [Guid("502420CE-99CB-4674-B41A-EBF7D9A78BBA"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class CapillaryFlow : ICapillaryFlow
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: CapillaryFlow

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
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

        public CapillaryFlow()
        {
            // Ignore Spelling: acetonitrile, dynes, ng, ug

            SetAutoComputeEnabled(false);

            // ReSharper disable RedundantArgumentDefaultValue

            SetAutoComputeMode(AutoComputeMode.VolFlowRate);
            SetCapillaryType(CapillaryType.PackedCapillary);
            SetBackPressure(3000d, UnitOfPressure.Psi);
            SetColumnLength(50d, UnitOfLength.CM);
            SetColumnId(75d, UnitOfLength.Microns);
            SetSolventViscosity(0.0089d, UnitOfViscosity.Poise);
            SetParticleDiameter(5d, UnitOfLength.Microns);
            SetInterparticlePorosity(0.4d);

            SetMassRateConcentration(1d, UnitOfConcentration.MicroMolar);
            SetMassRateVolFlowRate(600d, UnitOfFlowRate.NLPerMin);
            SetMassRateInjectionTime(5d, UnitOfTime.Minutes);

            // ReSharper restore RedundantArgumentDefaultValue

            // Recompute
            ComputeVolFlowRate();
            ComputeMassRateValues();
            ComputeExtraColumnBroadeningValues();

            SetAutoComputeEnabled(true);
        }

        #region "Private data classes"

        private class CapillaryFlowParameters
        {
            public CapillaryType CapillaryType { get; set; }

            /// <summary>
            /// Units: dynes/cm^2
            /// </summary>
            public double BackPressure { get; set; }

            /// <summary>
            /// Units: cm
            /// </summary>
            public double ColumnLength { get; set; }

            /// <summary>
            /// Units: cm
            /// </summary>
            public double ColumnId { get; set; }

            /// <summary>
            /// Units: poise
            /// </summary>
            public double SolventViscosity { get; set; }

            /// <summary>
            /// Units: cm
            /// </summary>
            public double ParticleDiameter { get; set; }

            /// <summary>
            /// Units: mL/min
            /// </summary>
            public double VolumetricFlowRate { get; set; }

            /// <summary>
            /// Units: cm/min
            /// </summary>
            public double LinearVelocity { get; set; }

            /// <summary>
            /// Units: min
            /// </summary>
            public double ColumnDeadTime { get; set; }

            public double InterparticlePorosity { get; set; }
        }

        private class MassRateParameters
        {
            /// <summary>
            /// Units: Molar
            /// </summary>
            public double SampleConcentration { get; set; }

            /// <summary>
            /// Units: g/mole
            /// </summary>
            public double SampleMass { get; set; }

            /// <summary>
            /// Units: mL/min
            /// </summary>
            public double VolumetricFlowRate { get; set; }

            /// <summary>
            /// Units: min
            /// </summary>
            public double InjectionTime { get; set; }

            /// <summary>
            /// Units: Moles/min
            /// </summary>
            public double MassFlowRate { get; set; }

            /// <summary>
            /// Units: moles
            /// </summary>
            public double MolesInjected { get; set; }
        }

        private class ExtraColumnBroadeningParameters
        {
            /// <summary>
            /// Units: cm/min
            /// </summary>
            public double LinearVelocity { get; set; }

            /// <summary>
            /// Units: cm^2/sec
            /// </summary>
            public double DiffusionCoefficient { get; set; }

            /// <summary>
            /// Units: cm
            /// </summary>
            public double OpenTubeLength { get; set; }

            /// <summary>
            /// Units: cm
            /// </summary>
            public double OpenTubeId { get; set; }

            /// <summary>
            /// Units: sec
            /// </summary>
            public double InitialPeakWidth { get; set; }

            /// <summary>
            /// Units: sec^2
            /// </summary>
            public double TemporalVariance { get; set; }

            /// <summary>
            /// Units: sec^2
            /// </summary>
            public double AdditionalTemporalVariance { get; set; }

            /// <summary>
            /// Units: sec
            /// </summary>
            public double ResultantPeakWidth { get; set; }
        }

        #endregion

        // Conversion Factors
        private const float CM_PER_INCH = 2.54f;
        private const double PI = 3.14159265359d;

        private readonly CapillaryFlowParameters mCapillaryFlowParameters = new();
        private readonly MassRateParameters mMassRateParameters = new();
        private readonly ExtraColumnBroadeningParameters mExtraColumnBroadeningParameters = new();

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
                    case AutoComputeMode.ColumnId:
                        ComputeColumnId();
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
                        // Includes VolFlowRate
                        ComputeVolFlowRate();
                        break;
                }
            }
        }

        /// <summary>
        /// Computes the back pressure, stores in .BackPressure, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeBackPressure(UnitOfPressure units = UnitOfPressure.Psi)
        {
            double backPressure;

            var radius = mCapillaryFlowParameters.ColumnId / 2.0d;

            if (Math.Abs(radius) > float.Epsilon)
            {
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    backPressure = mCapillaryFlowParameters.VolumetricFlowRate * 8d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength / (Math.Pow(radius, 4d) * PI * 60d); // Pressure in dynes/cm^2
                }
                // Packed capillary
                else if (Math.Abs(mCapillaryFlowParameters.ParticleDiameter) > float.Epsilon && Math.Abs(mCapillaryFlowParameters.InterparticlePorosity) > float.Epsilon)
                {
                    // Flow rate in mL/sec
                    backPressure = mCapillaryFlowParameters.VolumetricFlowRate * 180d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength * Math.Pow(1d - mCapillaryFlowParameters.InterparticlePorosity, 2d) / (Math.Pow(mCapillaryFlowParameters.ParticleDiameter, 2d) * Math.Pow(mCapillaryFlowParameters.InterparticlePorosity, 2d) * PI * Math.Pow(radius, 2d) * 60d) / mCapillaryFlowParameters.InterparticlePorosity;
                }
                else
                {
                    backPressure = 0d;
                }
            }
            else
            {
                backPressure = 0d;
            }

            mCapillaryFlowParameters.BackPressure = backPressure;

            // Compute Dead Time (and Linear Velocity)
            // Must send false for RecalculateVolFlowRate since we're finding the back pressure, not volumetric flow rate
            ComputeDeadTime(UnitOfTime.Minutes, false);

            // Return Back Pressure
            return ConvertPressure(backPressure, UnitOfPressure.DynesPerSquareCm, units);
        }

        /// <summary>
        /// Computes the column length, stores in .ColumnLength, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeColumnLength(UnitOfLength units = UnitOfLength.CM)
        {
            double columnLength;

            var radius = mCapillaryFlowParameters.ColumnId / 2.0d;

            if (Math.Abs(mCapillaryFlowParameters.SolventViscosity) > float.Epsilon && Math.Abs(mCapillaryFlowParameters.VolumetricFlowRate) > float.Epsilon)
            {
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    columnLength = mCapillaryFlowParameters.BackPressure * Math.Pow(radius, 4d) * PI * 60d / (8d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.VolumetricFlowRate); // Column length in cm
                }
                // Packed capillary
                else if (Math.Abs(mCapillaryFlowParameters.InterparticlePorosity - 1d) > float.Epsilon)
                {
                    // Flow rate in mL/sec
                    columnLength = mCapillaryFlowParameters.BackPressure * Math.Pow(mCapillaryFlowParameters.ParticleDiameter, 2d) * Math.Pow(mCapillaryFlowParameters.InterparticlePorosity, 2d) * PI * Math.Pow(radius, 2d) * 60d * mCapillaryFlowParameters.InterparticlePorosity / (180d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.VolumetricFlowRate * Math.Pow(1d - mCapillaryFlowParameters.InterparticlePorosity, 2d));
                }
                else
                {
                    columnLength = 0d;
                }
            }
            else
            {
                columnLength = 0d;
            }

            mCapillaryFlowParameters.ColumnLength = columnLength;

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(UnitOfTime.Minutes, true);

            // Return Column Length
            return ConvertLength(columnLength, UnitOfLength.CM, units);
        }

        public double ComputeColumnVolume(UnitOfVolume units = 0)
        {
            // Computes the column volume and returns it (does not store it)

            var radius = mCapillaryFlowParameters.ColumnId / 2.0d;

            var columnVolume = mCapillaryFlowParameters.ColumnLength * PI * Math.Pow(radius, 2d);

            if (mCapillaryFlowParameters.CapillaryType == CapillaryType.PackedCapillary)
            {
                columnVolume *= mCapillaryFlowParameters.InterparticlePorosity;
            }

            return ConvertVolume(columnVolume, UnitOfVolume.ML, units);
        }

        /// <summary>
        /// Computes the column length, stores in .ColumnLength, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeColumnId(UnitOfLength units = UnitOfLength.Microns)
        {
            double radius;

            if (Math.Abs(mCapillaryFlowParameters.BackPressure) > float.Epsilon)
            {
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    radius = Math.Pow(mCapillaryFlowParameters.VolumetricFlowRate * 8d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength / (mCapillaryFlowParameters.BackPressure * PI * 60d), 0.25d);
                }
                // Packed capillary
                else if (Math.Abs(mCapillaryFlowParameters.ParticleDiameter) > float.Epsilon && Math.Abs(mCapillaryFlowParameters.InterparticlePorosity - 1d) > float.Epsilon)
                {
                    // Flow rate in mL/sec
                    radius = Math.Pow(mCapillaryFlowParameters.VolumetricFlowRate * 180d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength * Math.Pow(1d - mCapillaryFlowParameters.InterparticlePorosity, 2d) / (mCapillaryFlowParameters.BackPressure * Math.Pow(mCapillaryFlowParameters.ParticleDiameter, 2d) * Math.Pow(mCapillaryFlowParameters.InterparticlePorosity, 2d) * PI * 60d) / mCapillaryFlowParameters.InterparticlePorosity, 0.5d);
                }
                else
                {
                    radius = 0d;
                }
            }
            else
            {
                radius = 0d;
            }

            mCapillaryFlowParameters.ColumnId = radius * 2.0d;

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(UnitOfTime.Minutes, true);

            // Return Column ID
            return ConvertLength(radius * 2.0d, UnitOfLength.CM, units);
        }

        /// <summary>
        /// Computes the column dead time, stores in .ColumnDeadTime, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <param name="recalculateVolFlowRate"></param>
        public double ComputeDeadTime(UnitOfTime units = UnitOfTime.Minutes, bool recalculateVolFlowRate = true)
        {
            double deadTime;

            // Dead time is dependent on Linear Velocity, so compute
            ComputeLinearVelocity(UnitOfLinearVelocity.CmPerSec, recalculateVolFlowRate);

            if (Math.Abs(mCapillaryFlowParameters.LinearVelocity) > float.Epsilon)
            {
                deadTime = mCapillaryFlowParameters.ColumnLength / mCapillaryFlowParameters.LinearVelocity; // Dead time in minutes
            }
            else
            {
                deadTime = 0d;
            }

            mCapillaryFlowParameters.ColumnDeadTime = deadTime;

            // Return Dead Time
            return ConvertTime(deadTime, UnitOfTime.Minutes, units);
        }

        public double ComputeExtraColumnBroadeningResultantPeakWidth(UnitOfTime units = UnitOfTime.Seconds)
        {
            ComputeExtraColumnBroadeningValues();

            return GetExtraColumnBroadeningResultantPeakWidth(units);
        }

        private void ComputeExtraColumnBroadeningValues()
        {
            if (Math.Abs(mExtraColumnBroadeningParameters.LinearVelocity) > float.Epsilon && Math.Abs(mExtraColumnBroadeningParameters.DiffusionCoefficient) > float.Epsilon)
            {
                mExtraColumnBroadeningParameters.TemporalVariance = Math.Pow(mExtraColumnBroadeningParameters.OpenTubeId, 2d) * mExtraColumnBroadeningParameters.OpenTubeLength / (96d * mExtraColumnBroadeningParameters.DiffusionCoefficient * mExtraColumnBroadeningParameters.LinearVelocity / 60d); // in sec^2
            }
            else
            {
                mExtraColumnBroadeningParameters.TemporalVariance = 0d;
            }

            var initialPeakVariance = Math.Pow(mExtraColumnBroadeningParameters.InitialPeakWidth / 4d, 2d);

            var sumOfVariances = initialPeakVariance + mExtraColumnBroadeningParameters.TemporalVariance + mExtraColumnBroadeningParameters.AdditionalTemporalVariance;

            if (sumOfVariances >= 0d)
            {
                // ResultantPeakWidth at the base = 4 sigma  and  sigma = SquareRoot(Total_Variance)
                mExtraColumnBroadeningParameters.ResultantPeakWidth = 4d * Math.Sqrt(sumOfVariances);
            }
            else
            {
                mExtraColumnBroadeningParameters.ResultantPeakWidth = 0d;
            }
        }

        /// <summary>
        /// Computes the Linear velocity, stores in .LinearVelocity, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <param name="recalculateVolFlowRate"></param>
        public double ComputeLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec, bool recalculateVolFlowRate = true)
        {
            double linearVelocity;

            if (recalculateVolFlowRate)
            {
                ComputeVolFlowRate(UnitOfFlowRate.MLPerMin);
            }

            var radius = mCapillaryFlowParameters.ColumnId / 2.0d;
            if (Math.Abs(radius) > float.Epsilon)
            {
                linearVelocity = mCapillaryFlowParameters.VolumetricFlowRate / (PI * Math.Pow(radius, 2d)); // Units in cm/min

                // Divide Linear Velocity by epsilon if a packed capillary
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.PackedCapillary && Math.Abs(mCapillaryFlowParameters.InterparticlePorosity) > float.Epsilon)
                {
                    linearVelocity /= mCapillaryFlowParameters.InterparticlePorosity;
                }
            }
            else
            {
                linearVelocity = 0d;
            }

            mCapillaryFlowParameters.LinearVelocity = linearVelocity;

            // Return Linear Velocity
            return ConvertLinearVelocity(linearVelocity, UnitOfLinearVelocity.CmPerMin, units);
        }

        /// <summary>
        /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
        /// </summary>
        /// <param name="units"></param>
        public double ComputeMassFlowRate(UnitOfMassFlowRate units = UnitOfMassFlowRate.FmolPerSec)
        {
            ComputeMassRateValues();
            return GetMassFlowRate(units);
        }

        /// <summary>
        /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
        /// </summary>
        /// <param name="units"></param>
        public double ComputeMassRateMolesInjected(UnitOfMolarAmount units = UnitOfMolarAmount.FemtoMoles)
        {
            ComputeMassRateValues();
            return GetMassRateMolesInjected(units);
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
        /// <param name="units"></param>
        public double ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec)
        {
            var optimumLinearVelocity = 0.0;

            if (Math.Abs(mCapillaryFlowParameters.ParticleDiameter) > float.Epsilon)
            {
                optimumLinearVelocity = 3d * mExtraColumnBroadeningParameters.DiffusionCoefficient / mCapillaryFlowParameters.ParticleDiameter;

                optimumLinearVelocity = ConvertLinearVelocity(optimumLinearVelocity, UnitOfLinearVelocity.CmPerSec, units);
            }

            return optimumLinearVelocity;
        }

        // ReSharper disable once InconsistentNaming
        public double ComputeMeCNViscosity(double percentAcetonitrile, double temperature, UnitOfTemperature temperatureUnits = UnitOfTemperature.Celsius, UnitOfViscosity viscosityUnits = UnitOfViscosity.Poise)
        {
            try
            {
                // compute fraction acetonitrile
                var phi = percentAcetonitrile / 100.0d;
                if (phi < 0d)
                    phi = 0d;

                if (phi > 100d)
                    phi = 100d;

                var kelvin = ConvertTemperature(temperature, temperatureUnits, UnitOfTemperature.Kelvin);

                double viscosityInCentiPoise;
                if (kelvin > 0d)
                {
                    viscosityInCentiPoise = Math.Exp(phi * (-3.476d + 726d / kelvin) + (1d - phi) * (-5.414d + 1566d / kelvin) + phi * (1d - phi) * (-1.762d + 929d / kelvin));
                }
                else
                {
                    viscosityInCentiPoise = 0d;
                }

                return ConvertViscosity(viscosityInCentiPoise, UnitOfViscosity.CentiPoise, viscosityUnits);
            }
            catch
            {
                return 0d;
            }
        }

        /// <summary>
        /// Computes the Volumetric flow rate, stores in .VolumetricFlowRate, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            double volFlowRate;

            var radius = mCapillaryFlowParameters.ColumnId / 2.0d;

            if (Math.Abs(mCapillaryFlowParameters.SolventViscosity) > float.Epsilon && Math.Abs(mCapillaryFlowParameters.ColumnLength) > float.Epsilon)
            {
                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    volFlowRate = mCapillaryFlowParameters.BackPressure * Math.Pow(radius, 4d) * PI / (8d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength); // Flow rate in mL/sec
                }
                // Packed capillary
                else if (Math.Abs(mCapillaryFlowParameters.InterparticlePorosity - 1d) > float.Epsilon)
                {
                    // Flow rate in mL/sec
                    volFlowRate = mCapillaryFlowParameters.BackPressure * Math.Pow(mCapillaryFlowParameters.ParticleDiameter, 2d) * Math.Pow(mCapillaryFlowParameters.InterparticlePorosity, 2d) * PI * Math.Pow(radius, 2d) * mCapillaryFlowParameters.InterparticlePorosity / (180d * mCapillaryFlowParameters.SolventViscosity * mCapillaryFlowParameters.ColumnLength * Math.Pow(1d - mCapillaryFlowParameters.InterparticlePorosity, 2d));
                }
                else
                {
                    volFlowRate = 0d;
                }

                // Convert volFlowRate to mL/min
                volFlowRate *= 60d;
            }
            else
            {
                volFlowRate = 0d;
            }

            mCapillaryFlowParameters.VolumetricFlowRate = volFlowRate;

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(UnitOfTime.Minutes, false);

            return ConvertVolFlowRate(volFlowRate, UnitOfFlowRate.MLPerMin, units);
        }

        /// <summary>
        /// Computes the Volumetric flow rate using the dead time, stores in .VolumetricFlowRate, and returns it
        /// This requires modifying the pressure value to give the computed volumetric flow rate
        /// </summary>
        /// <param name="newBackPressure">Output: new back pressure</param>
        /// <param name="units"></param>
        /// <param name="pressureUnits"></param>
        public double ComputeVolFlowRateUsingDeadTime(
            out double newBackPressure,
            UnitOfFlowRate units = UnitOfFlowRate.NLPerMin,
            UnitOfPressure pressureUnits = UnitOfPressure.Psi)
        {
            newBackPressure = 0;
            double volFlowRate;

            var radius = mCapillaryFlowParameters.ColumnId / 2.0d;

            // First find vol flow rate that gives observed dead time
            if (Math.Abs(mCapillaryFlowParameters.ColumnDeadTime) > float.Epsilon)
            {
                volFlowRate = mCapillaryFlowParameters.ColumnLength * (PI * Math.Pow(radius, 2d)) / mCapillaryFlowParameters.ColumnDeadTime; // Vol flow rate in mL/sec

                if (mCapillaryFlowParameters.CapillaryType == CapillaryType.PackedCapillary)
                {
                    // Packed Capillary
                    volFlowRate *= mCapillaryFlowParameters.InterparticlePorosity;
                }

                // Store the new value
                mCapillaryFlowParameters.VolumetricFlowRate = volFlowRate;

                // Now find pressure that gives computed volFlowRate
                // The ComputeBackPressure sub will store the new pressure
                newBackPressure = ComputeBackPressure(pressureUnits);
            }
            else
            {
                volFlowRate = 0d;
                mCapillaryFlowParameters.VolumetricFlowRate = 0d;
            }

            // Compute Linear Velocity (but not the dead time)
            ComputeLinearVelocity(UnitOfLinearVelocity.CmPerSec, false);

            return ConvertVolFlowRate(volFlowRate, UnitOfFlowRate.MLPerMin, units);
        }

        /// <summary>
        /// Convert concentration
        /// </summary>
        /// <param name="concentrationIn"></param>
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        public double ConvertConcentration(double concentrationIn, UnitOfConcentration currentUnits, UnitOfConcentration newUnits)
        {
            if (currentUnits == newUnits)
            {
                return concentrationIn;
            }

            var sampleMass = mMassRateParameters.SampleMass;

            var factor = FactorConcentration(currentUnits, sampleMass);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = concentrationIn * factor;

            factor = FactorConcentration(newUnits, sampleMass);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertDiffusionCoefficient(double diffusionCoefficientIn, UnitOfDiffusionCoefficient currentUnits, UnitOfDiffusionCoefficient newUnits)
        {
            if (currentUnits == newUnits)
            {
                return diffusionCoefficientIn;
            }

            var factor = FactorDiffusionCoeff(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = diffusionCoefficientIn * factor;

            factor = FactorDiffusionCoeff(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertLength(double lengthIn, UnitOfLength currentUnits, UnitOfLength newUnits)
        {
            if (currentUnits == newUnits)
            {
                return lengthIn;
            }

            var factor = FactorLength(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = lengthIn * factor;

            factor = FactorLength(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertLinearVelocity(double linearVelocityIn, UnitOfLinearVelocity currentUnits, UnitOfLinearVelocity newUnits)
        {
            if (currentUnits == newUnits)
            {
                return linearVelocityIn;
            }

            var factor = FactorLinearVelocity(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = linearVelocityIn * factor;

            factor = FactorLinearVelocity(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertMassFlowRate(double massFlowRateIn, UnitOfMassFlowRate currentUnits, UnitOfMassFlowRate newUnits)
        {
            if (currentUnits == newUnits)
            {
                return massFlowRateIn;
            }

            var factor = FactorMassFlowRate(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = massFlowRateIn * factor;

            factor = FactorMassFlowRate(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertMoles(double molesIn, UnitOfMolarAmount currentUnits, UnitOfMolarAmount newUnits)
        {
            if (currentUnits == newUnits)
            {
                return molesIn;
            }

            var factor = FactorMoles(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = molesIn * factor;

            factor = FactorMoles(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertPressure(double pressureIn, UnitOfPressure currentUnits, UnitOfPressure newUnits)
        {
            if (currentUnits == newUnits)
            {
                return pressureIn;
            }

            var factor = FactorPressure(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = pressureIn * factor;

            factor = FactorPressure(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertTemperature(double temperatureIn, UnitOfTemperature currentUnits, UnitOfTemperature newUnits)
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
                    // K = C + 273
                    value = temperatureIn + 273d;
                    break;

                case UnitOfTemperature.Fahrenheit:
                    // Convert to Kelvin: C = 5/9*(F-32) and K = C + 273
                    value = 5.0d / 9.0d * (temperatureIn - 32d) + 273d;
                    break;

                // ReSharper disable once RedundantEmptySwitchSection
                default:
                    // Includes UnitOfTemperature.Kelvin
                    // Assume already Kelvin
                    break;
            }

            // We cannot get colder than absolute 0
            if (value < 0d)
                value = 0d;

            // Now convert to the target units
            switch (newUnits)
            {
                case UnitOfTemperature.Celsius:
                    // C = K - 273
                    value -= 273d;
                    break;

                case UnitOfTemperature.Fahrenheit:
                    // Convert to Fahrenheit: C = K - 273 and F = (9/5)C + 32
                    value = 9.0d / 5.0d * (value - 273d) + 32d;
                    break;

                // ReSharper disable once RedundantEmptySwitchSection
                default:
                    // Includes UnitOfTemperature.Kelvin
                    // Already in Kelvin
                    break;
            }

            return value;
        }

        public double ConvertTime(double timeIn, UnitOfTime currentUnits, UnitOfTime newUnits)
        {
            if (currentUnits == newUnits)
            {
                return timeIn;
            }

            var factor = FactorTime(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = timeIn * factor;

            factor = FactorTime(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertViscosity(double viscosityIn, UnitOfViscosity currentUnits, UnitOfViscosity newUnits)
        {
            if (currentUnits == newUnits)
            {
                return viscosityIn;
            }

            var factor = FactorViscosity(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = viscosityIn * factor;

            factor = FactorViscosity(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertVolFlowRate(double volFlowRateIn, UnitOfFlowRate currentUnits, UnitOfFlowRate newUnits)
        {
            if (currentUnits == newUnits)
            {
                return volFlowRateIn;
            }

            var factor = FactorVolFlowRate(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = volFlowRateIn * factor;

            factor = FactorVolFlowRate(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
            {
                return -1;
            }

            return value / factor;
        }

        public double ConvertVolume(double volume, UnitOfVolume currentUnits, UnitOfVolume newUnits)
        {
            if (currentUnits == newUnits)
            {
                return volume;
            }

            var factor = FactorVolume(currentUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon)
            {
                return -1;
            }

            var value = volume * factor;

            factor = FactorVolume(newUnits);
            if (Math.Abs(factor + 1d) < float.Epsilon || Math.Abs(factor) < float.Epsilon)
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
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        private double FactorConcentration(UnitOfConcentration units, double sampleMass = 0d)
        {
            double factor;

            if (Math.Abs(sampleMass) < float.Epsilon)
            {
                factor = -1;
            }
            else
            {
                factor = units switch
                {
                    UnitOfConcentration.Molar => 1.0d,
                    UnitOfConcentration.MilliMolar => 1d / 1000.0d,
                    UnitOfConcentration.MicroMolar => 1d / 1000000.0d,
                    UnitOfConcentration.NanoMolar => 1d / 1000000000.0d,
                    UnitOfConcentration.PicoMolar => 1d / 1000000000000.0d,
                    UnitOfConcentration.FemtoMolar => 1d / 1.0E+15d,
                    UnitOfConcentration.AttoMolar => 1d / 1.0E+18d,
                    UnitOfConcentration.MgPerML => 1d / sampleMass, // 1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                    UnitOfConcentration.UgPerML => 1d / (sampleMass * 1000.0d), // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                    UnitOfConcentration.NgPerML => 1d / (sampleMass * 1000000.0d), // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                    UnitOfConcentration.UgPerUL => 1d / sampleMass, // 1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                    UnitOfConcentration.NgPerUL => 1d / (sampleMass * 1000.0d), // 1/[(1 g / 1000000000 ng) * (1 / MW) * (1000000 uL/L)]
                    _ => -1
                };
            }

            return factor;
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to Cm
        /// </summary>
        /// <param name="units"></param>
        private double FactorLength(UnitOfLength units)
        {
            return units switch
            {
                UnitOfLength.M => 100.0d,
                UnitOfLength.CM => 1.0d,
                UnitOfLength.MM => 1d / 10.0d,
                UnitOfLength.Microns => 1d / 10000.0d,
                UnitOfLength.Inches => CM_PER_INCH,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to Cm/Min
        /// </summary>
        /// <param name="units"></param>
        private double FactorLinearVelocity(UnitOfLinearVelocity units)
        {
            return units switch
            {
                UnitOfLinearVelocity.CmPerHr => 1d / 60.0d,
                UnitOfLinearVelocity.MmPerHr => 1d / 60.0d / 10.0d,
                UnitOfLinearVelocity.CmPerMin => 1d,
                UnitOfLinearVelocity.MmPerMin => 1d / 10.0d,
                UnitOfLinearVelocity.CmPerSec => 60.0d,
                UnitOfLinearVelocity.MmPerSec => 60.0d / 10.0d,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to moles/min
        /// </summary>
        /// <param name="units"></param>
        private double FactorMassFlowRate(UnitOfMassFlowRate units)
        {
            return units switch
            {
                UnitOfMassFlowRate.PmolPerMin => 1d / 1000000000000.0d,
                UnitOfMassFlowRate.FmolPerMin => 1d / 1.0E+15d,
                UnitOfMassFlowRate.AmolPerMin => 1d / 1.0E+18d,
                UnitOfMassFlowRate.PmolPerSec => 1d / (1000000000000.0d / 60.0d),
                UnitOfMassFlowRate.FmolPerSec => 1d / (1.0E+15d / 60.0d),
                UnitOfMassFlowRate.AmolPerSec => 1d / (1.0E+18d / 60.0d),
                UnitOfMassFlowRate.MolesPerMin => 1.0d,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to moles
        /// </summary>
        /// <param name="units"></param>
        private double FactorMoles(UnitOfMolarAmount units)
        {
            return units switch
            {
                UnitOfMolarAmount.Moles => 1.0d,
                UnitOfMolarAmount.MilliMoles => 1d / 1000.0d,
                UnitOfMolarAmount.MicroMoles => 1d / 1000000.0d,
                UnitOfMolarAmount.NanoMoles => 1d / 1000000000.0d,
                UnitOfMolarAmount.PicoMoles => 1d / 1000000000000.0d,
                UnitOfMolarAmount.FemtoMoles => 1d / 1.0E+15d,
                UnitOfMolarAmount.AttoMoles => 1d / 1.0E+18d,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to dynes per cm^2
        /// </summary>
        /// <param name="units"></param>
        private double FactorPressure(UnitOfPressure units)
        {
            return units switch
            {
                UnitOfPressure.Psi => 68947.57d,
                UnitOfPressure.Pascals => 10.0d,
                UnitOfPressure.KiloPascals => 10000.0d,
                UnitOfPressure.Atmospheres => 1013250.0d,
                UnitOfPressure.Bar => 1000000.0d,
                UnitOfPressure.Torr => 1333.22d,
                UnitOfPressure.DynesPerSquareCm => 1d,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to minutes
        /// </summary>
        /// <param name="units"></param>
        private double FactorTime(UnitOfTime units)
        {
            return units switch
            {
                UnitOfTime.Hours => 60.0d,
                UnitOfTime.Minutes => 1.0d,
                UnitOfTime.Seconds => 1d / 60.0d,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to cm^2/sec
        /// </summary>
        /// <param name="units"></param>
        private double FactorDiffusionCoeff(UnitOfDiffusionCoefficient units)
        {
            return units switch
            {
                UnitOfDiffusionCoefficient.CmSquaredPerHr => 1d / 3600.0d,
                UnitOfDiffusionCoefficient.CmSquaredPerMin => 1d / 60.0d,
                UnitOfDiffusionCoefficient.CmSquaredPerSec => 1.0d,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to poise
        /// </summary>
        /// <param name="units"></param>
        private double FactorViscosity(UnitOfViscosity units)
        {
            return units switch
            {
                UnitOfViscosity.Poise => 1.0d,
                UnitOfViscosity.CentiPoise => 1d / 100.0d,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to mL/min
        /// </summary>
        /// <param name="units"></param>
        private double FactorVolFlowRate(UnitOfFlowRate units)
        {
            return units switch
            {
                UnitOfFlowRate.MLPerMin => 1.0d,
                UnitOfFlowRate.ULPerMin => 1d / 1000.0d,
                UnitOfFlowRate.NLPerMin => 1d / 1000000.0d,
                _ => -1
            };
        }

        /// <summary>
        /// Multiplication factor for converting from <paramref name="units"/> to mL
        /// </summary>
        /// <param name="units"></param>
        private double FactorVolume(UnitOfVolume units)
        {
            return units switch
            {
                UnitOfVolume.ML => 1.0d,
                UnitOfVolume.UL => 1d / 1000.0d,
                UnitOfVolume.NL => 1d / 1000000.0d,
                UnitOfVolume.PL => 1d / 1000000000.0d,
                _ => -1
            };
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

        public double GetBackPressure(UnitOfPressure units = UnitOfPressure.Psi)
        {
            return ConvertPressure(mCapillaryFlowParameters.BackPressure, UnitOfPressure.DynesPerSquareCm, units);
        }

        public CapillaryType GetCapillaryType()
        {
            return mCapillaryFlowParameters.CapillaryType;
        }

        public double GetColumnId(UnitOfLength units = UnitOfLength.Microns)
        {
            return ConvertLength(mCapillaryFlowParameters.ColumnId, UnitOfLength.CM, units);
        }

        public double GetColumnLength(UnitOfLength units = UnitOfLength.CM)
        {
            return ConvertLength(mCapillaryFlowParameters.ColumnLength, UnitOfLength.CM, units);
        }

        public double GetColumnVolume(UnitOfVolume units = UnitOfVolume.UL)
        {
            // Column volume isn't stored; simply re-compute it
            return ComputeColumnVolume(units);
        }

        public double GetDeadTime(UnitOfTime units = UnitOfTime.Minutes)
        {
            return ConvertTime(mCapillaryFlowParameters.ColumnDeadTime, UnitOfTime.Minutes, units);
        }

        public double GetExtraColumnBroadeningAdditionalVarianceInSquareSeconds()
        {
            return mExtraColumnBroadeningParameters.AdditionalTemporalVariance;
        }

        public double GetExtraColumnBroadeningDiffusionCoefficient(UnitOfDiffusionCoefficient units = UnitOfDiffusionCoefficient.CmSquaredPerSec)
        {
            return ConvertDiffusionCoefficient(mExtraColumnBroadeningParameters.DiffusionCoefficient, UnitOfDiffusionCoefficient.CmSquaredPerSec, units);
        }

        public double GetExtraColumnBroadeningInitialPeakWidthAtBase(UnitOfTime units = UnitOfTime.Seconds)
        {
            return ConvertTime(mExtraColumnBroadeningParameters.InitialPeakWidth, UnitOfTime.Seconds, units);
        }

        public double GetExtraColumnBroadeningLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.MmPerMin)
        {
            return ConvertLinearVelocity(mExtraColumnBroadeningParameters.LinearVelocity, UnitOfLinearVelocity.CmPerMin, units);
        }

        public double GetExtraColumnBroadeningOpenTubeId(UnitOfLength units = UnitOfLength.Microns)
        {
            return ConvertLength(mExtraColumnBroadeningParameters.OpenTubeId, UnitOfLength.CM, units);
        }

        public double GetExtraColumnBroadeningOpenTubeLength(UnitOfLength units = UnitOfLength.CM)
        {
            return ConvertLength(mExtraColumnBroadeningParameters.OpenTubeLength, UnitOfLength.CM, units);
        }

        public double GetExtraColumnBroadeningResultantPeakWidth(UnitOfTime units = UnitOfTime.Seconds)
        {
            return ConvertTime(mExtraColumnBroadeningParameters.ResultantPeakWidth, UnitOfTime.Seconds, units);
        }

        public double GetExtraColumnBroadeningTemporalVarianceInSquareSeconds()
        {
            return mExtraColumnBroadeningParameters.TemporalVariance;
        }

        public double GetInterparticlePorosity()
        {
            return mCapillaryFlowParameters.InterparticlePorosity;
        }

        public double GetLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec)
        {
            return ConvertLinearVelocity(mCapillaryFlowParameters.LinearVelocity, UnitOfLinearVelocity.CmPerMin, units);
        }

        public double GetMassRateConcentration(UnitOfConcentration units = UnitOfConcentration.MicroMolar)
        {
            return ConvertConcentration(mMassRateParameters.SampleConcentration, UnitOfConcentration.Molar, units);
        }

        public double GetMassRateInjectionTime(UnitOfTime units = UnitOfTime.Minutes)
        {
            return ConvertTime(mMassRateParameters.InjectionTime, UnitOfTime.Minutes, units);
        }

        public double GetMassFlowRate(UnitOfMassFlowRate units = UnitOfMassFlowRate.FmolPerSec)
        {
            return ConvertMassFlowRate(mMassRateParameters.MassFlowRate, UnitOfMassFlowRate.MolesPerMin, units);
        }

        public double GetMassRateMolesInjected(UnitOfMolarAmount units = UnitOfMolarAmount.FemtoMoles)
        {
            return ConvertMoles(mMassRateParameters.MolesInjected, UnitOfMolarAmount.Moles, units);
        }

        public double GetMassRateSampleMass()
        {
            return mMassRateParameters.SampleMass;
        }

        public double GetMassRateVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            return ConvertVolFlowRate(mMassRateParameters.VolumetricFlowRate, UnitOfFlowRate.MLPerMin, units);
        }

        public double GetParticleDiameter(UnitOfLength units = UnitOfLength.Microns)
        {
            return ConvertLength(mCapillaryFlowParameters.ParticleDiameter, UnitOfLength.CM, units);
        }

        public double GetSolventViscosity(UnitOfViscosity units = UnitOfViscosity.Poise)
        {
            return ConvertViscosity(mCapillaryFlowParameters.SolventViscosity, UnitOfViscosity.Poise, units);
        }

        public double GetVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            return ConvertVolFlowRate(mCapillaryFlowParameters.VolumetricFlowRate, UnitOfFlowRate.MLPerMin, units);
        }

        // Set Methods
        // If mAutoCompute = False, then must manually call a Compute Sub to recompute other values

        public void SetAutoComputeEnabled(bool autoCompute)
        {
            mAutoCompute = autoCompute;
        }

        public void SetAutoComputeMode(AutoComputeMode autoComputeMode)
        {
            mAutoComputeMode = autoComputeMode;
        }

        public void SetBackPressure(double backPressure, UnitOfPressure units = UnitOfPressure.Psi)
        {
            mCapillaryFlowParameters.BackPressure = ConvertPressure(backPressure, units, UnitOfPressure.DynesPerSquareCm);
            CheckAutoCompute();
        }

        public void SetCapillaryType(CapillaryType capillaryType)
        {
            mCapillaryFlowParameters.CapillaryType = capillaryType;
            CheckAutoCompute();
        }

        public void SetColumnId(double columnId, UnitOfLength units = UnitOfLength.Microns)
        {
            mCapillaryFlowParameters.ColumnId = ConvertLength(columnId, units, UnitOfLength.CM);
            CheckAutoCompute();
        }

        public void SetColumnLength(double columnLength, UnitOfLength units = UnitOfLength.CM)
        {
            mCapillaryFlowParameters.ColumnLength = ConvertLength(columnLength, units, UnitOfLength.CM);
            CheckAutoCompute();
        }

        public void SetDeadTime(double deadTime, UnitOfTime units = UnitOfTime.Minutes)
        {
            mCapillaryFlowParameters.ColumnDeadTime = ConvertTime(deadTime, units, UnitOfTime.Minutes);
            CheckAutoCompute();
        }

        public void SetExtraColumnBroadeningAdditionalVariance(double additionalVarianceInSquareSeconds)
        {
            mExtraColumnBroadeningParameters.AdditionalTemporalVariance = additionalVarianceInSquareSeconds;
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningDiffusionCoefficient(double diffusionCoefficient, UnitOfDiffusionCoefficient units = UnitOfDiffusionCoefficient.CmSquaredPerSec)
        {
            mExtraColumnBroadeningParameters.DiffusionCoefficient = ConvertDiffusionCoefficient(diffusionCoefficient, units, UnitOfDiffusionCoefficient.CmSquaredPerSec);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningInitialPeakWidthAtBase(double width, UnitOfTime units = UnitOfTime.Seconds)
        {
            mExtraColumnBroadeningParameters.InitialPeakWidth = ConvertTime(width, units, UnitOfTime.Seconds);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningLinearVelocity(double linearVelocity, UnitOfLinearVelocity units = UnitOfLinearVelocity.MmPerMin)
        {
            mExtraColumnBroadeningParameters.LinearVelocity = ConvertLinearVelocity(linearVelocity, units, UnitOfLinearVelocity.CmPerMin);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningOpenTubeId(double openTubeId, UnitOfLength units = UnitOfLength.Microns)
        {
            mExtraColumnBroadeningParameters.OpenTubeId = ConvertLength(openTubeId, units, UnitOfLength.CM);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetExtraColumnBroadeningOpenTubeLength(double length, UnitOfLength units = UnitOfLength.CM)
        {
            mExtraColumnBroadeningParameters.OpenTubeLength = ConvertLength(length, units, UnitOfLength.CM);
            ComputeExtraColumnBroadeningValues();
        }

        public void SetInterparticlePorosity(double porosity)
        {
            if (porosity is >= 0d and <= 1d)
            {
                mCapillaryFlowParameters.InterparticlePorosity = porosity;
            }

            CheckAutoCompute();
        }

        public void SetMassRateConcentration(double concentration, UnitOfConcentration units = UnitOfConcentration.MicroMolar)
        {
            mMassRateParameters.SampleConcentration = ConvertConcentration(concentration, units, UnitOfConcentration.Molar);
            ComputeMassRateValues();
        }

        public void SetMassRateInjectionTime(double injectionTime, UnitOfTime units = UnitOfTime.Minutes)
        {
            mMassRateParameters.InjectionTime = ConvertTime(injectionTime, units, UnitOfTime.Minutes);
            ComputeMassRateValues();
        }

        public void SetMassRateSampleMass(double massInGramsPerMole)
        {
            if (massInGramsPerMole >= 0d)
            {
                mMassRateParameters.SampleMass = massInGramsPerMole;
            }
            else
            {
                mMassRateParameters.SampleMass = 0d;
            }

            ComputeMassRateValues();
        }

        public void SetMassRateVolFlowRate(double volFlowRate, UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            mMassRateParameters.VolumetricFlowRate = ConvertVolFlowRate(volFlowRate, units, UnitOfFlowRate.MLPerMin);
            ComputeMassRateValues();
        }

        public void SetParticleDiameter(double particleDiameter, UnitOfLength units = UnitOfLength.Microns)
        {
            mCapillaryFlowParameters.ParticleDiameter = ConvertLength(particleDiameter, units, UnitOfLength.CM);
            CheckAutoCompute();
        }

        public void SetSolventViscosity(double solventViscosity, UnitOfViscosity units = UnitOfViscosity.Poise)
        {
            mCapillaryFlowParameters.SolventViscosity = ConvertViscosity(solventViscosity, units, UnitOfViscosity.Poise);
            CheckAutoCompute();
        }

        public void SetVolFlowRate(double volFlowRate, UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            mCapillaryFlowParameters.VolumetricFlowRate = ConvertVolFlowRate(volFlowRate, units, UnitOfFlowRate.MLPerMin);
            CheckAutoCompute();
        }
    }
}