using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.CapillaryFlowTools
{
    [ComVisible(false)]
    public class CapillaryFlow
    {
        public CapillaryFlow()
        {
            // Ignore Spelling: acetonitrile, dynes

            SetAutoComputeEnabled(false);

            // ReSharper disable RedundantArgumentDefaultValue

            SetAutoComputeMode(AutoComputeMode.VolFlowRate);
            SetCapillaryType(CapillaryType.PackedCapillary);
            SetBackPressure(3000, UnitOfPressure.Psi);
            SetColumnLength(50, UnitOfLength.CM);
            SetColumnInnerDiameter(75, UnitOfLength.Microns);
            SetSolventViscosity(0.0089, UnitOfViscosity.Poise);
            SetParticleDiameter(5, UnitOfLength.Microns);
            SetInterparticlePorosity(0.4);

            // ReSharper restore RedundantArgumentDefaultValue

            // Recompute
            ComputeVolumetricFlowRate();
        }

        private CapillaryType mCapillaryType;

        /// <summary>
        /// Units: dynes/cm^2
        /// </summary>
        private double mBackPressure;

        /// <summary>
        /// Units: cm
        /// </summary>
        private double mColumnLength;

        /// <summary>
        /// Units: cm
        /// </summary>
        private double mColumnInnerDiameter;

        /// <summary>
        /// Units: poise
        /// </summary>
        private double mSolventViscosity;

        /// <summary>
        /// Units: cm
        /// </summary>
        private double mParticleDiameter;

        /// <summary>
        /// Units: mL/min
        /// </summary>
        private double mVolumetricFlowRate;

        /// <summary>
        /// Units: cm/min
        /// </summary>
        private double mLinearVelocity;

        /// <summary>
        /// Units: min
        /// </summary>
        private double mColumnDeadTime;

        private double mInterparticlePorosity;

        /// <summary>
        /// When true, automatically compute results whenever any value changes
        /// </summary>
        private bool mAutoCompute;

        /// <summary>
        /// The value to compute when <see cref="mAutoCompute"/> is true
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
                        ComputeColumnInnerDiameter();
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
                        ComputeVolumetricFlowRateUsingDeadTime(out _);
                        break;
                    default:
                        // Includes VolFlowRate
                        ComputeVolumetricFlowRate();
                        break;
                }
            }
        }

        /// <summary>
        /// Computes the back pressure, stores in mBackPressure, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeBackPressure(UnitOfPressure units = UnitOfPressure.Psi)
        {
            double backPressure;

            var radius = mColumnInnerDiameter / 2.0;

            if (Math.Abs(radius) > float.Epsilon)
            {
                // Compute pressure, in dynes/cm^2
                if (mCapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    backPressure = mVolumetricFlowRate * 8 * mSolventViscosity * mColumnLength / (Math.Pow(radius, 4) * Math.PI * 60);
                }
                else if (Math.Abs(mParticleDiameter) > float.Epsilon && Math.Abs(mInterparticlePorosity) > float.Epsilon)
                {
                    // Packed capillary
                    backPressure = mVolumetricFlowRate * 180 * mSolventViscosity * mColumnLength * Math.Pow(1 - mInterparticlePorosity, 2) / (Math.Pow(mParticleDiameter, 2) * Math.Pow(mInterparticlePorosity, 2) * Math.PI * Math.Pow(radius, 2) * 60) / mInterparticlePorosity;
                }
                else
                {
                    backPressure = 0;
                }
            }
            else
            {
                backPressure = 0;
            }

            mBackPressure = backPressure;

            // Compute Dead Time (and Linear Velocity)
            // Must send false for RecalculateVolFlowRate since we're finding the back pressure, not volumetric flow rate
            ComputeDeadTime(UnitOfTime.Minutes, false);

            // Return Back Pressure
            return UnitConversions.ConvertPressure(backPressure, UnitOfPressure.DynesPerSquareCm, units);
        }

        /// <summary>
        /// Computes the column length, stores in mColumnLength, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeColumnLength(UnitOfLength units = UnitOfLength.CM)
        {
            double columnLength;

            var radius = mColumnInnerDiameter / 2.0;

            if (Math.Abs(mSolventViscosity) > float.Epsilon && Math.Abs(mVolumetricFlowRate) > float.Epsilon)
            {
                // Compute column length, in cm
                if (mCapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    columnLength = mBackPressure * Math.Pow(radius, 4) * Math.PI * 60 / (8 * mSolventViscosity * mVolumetricFlowRate);
                }
                else if (Math.Abs(mInterparticlePorosity - 1) > float.Epsilon)
                {
                    // Packed capillary
                    columnLength = mBackPressure * Math.Pow(mParticleDiameter, 2) * Math.Pow(mInterparticlePorosity, 2) * Math.PI * Math.Pow(radius, 2) * 60 * mInterparticlePorosity / (180 * mSolventViscosity * mVolumetricFlowRate * Math.Pow(1 - mInterparticlePorosity, 2));
                }
                else
                {
                    columnLength = 0;
                }
            }
            else
            {
                columnLength = 0;
            }

            mColumnLength = columnLength;

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(UnitOfTime.Minutes, true);

            // Return Column Length
            return UnitConversions.ConvertLength(columnLength, UnitOfLength.CM, units);
        }

        public double ComputeColumnVolume(UnitOfVolume units = 0)
        {
            // Computes the column volume and returns it (does not store it)

            var radius = mColumnInnerDiameter / 2.0;

            var columnVolume = mColumnLength * Math.PI * Math.Pow(radius, 2);

            if (mCapillaryType == CapillaryType.PackedCapillary)
            {
                columnVolume *= mInterparticlePorosity;
            }

            return UnitConversions.ConvertVolume(columnVolume, UnitOfVolume.ML, units);
        }

        /// <summary>
        /// Computes the column inner diameter, stores in mColumnInnerDiameter, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeColumnInnerDiameter(UnitOfLength units = UnitOfLength.Microns)
        {
            double radius;

            if (Math.Abs(mBackPressure) > float.Epsilon)
            {
                if (mCapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    radius = Math.Pow(mVolumetricFlowRate * 8 * mSolventViscosity * mColumnLength / (mBackPressure * Math.PI * 60), 0.25);
                }
                else if (Math.Abs(mParticleDiameter) > float.Epsilon && Math.Abs(mInterparticlePorosity - 1) > float.Epsilon)
                {
                    // Packed capillary
                    radius = Math.Pow(mVolumetricFlowRate * 180 * mSolventViscosity * mColumnLength * Math.Pow(1 - mInterparticlePorosity, 2) / (mBackPressure * Math.Pow(mParticleDiameter, 2) * Math.Pow(mInterparticlePorosity, 2) * Math.PI * 60) / mInterparticlePorosity, 0.5);
                }
                else
                {
                    radius = 0;
                }
            }
            else
            {
                radius = 0;
            }

            mColumnInnerDiameter = radius * 2.0;

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(UnitOfTime.Minutes, true);

            // Return Column ID
            return UnitConversions.ConvertLength(radius * 2.0, UnitOfLength.CM, units);
        }

        /// <summary>
        /// Computes the column dead time, stores in mColumnDeadTime, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <param name="recalculateVolumetricFlowRate"></param>
        public double ComputeDeadTime(UnitOfTime units = UnitOfTime.Minutes, bool recalculateVolumetricFlowRate = true)
        {
            double deadTime;

            // Dead time is dependent on Linear Velocity, so compute
            ComputeLinearVelocity(UnitOfLinearVelocity.CmPerSec, recalculateVolumetricFlowRate);

            if (Math.Abs(mLinearVelocity) > float.Epsilon)
            {
                // Dead time, in minutes
                deadTime = mColumnLength / mLinearVelocity;
            }
            else
            {
                deadTime = 0;
            }

            mColumnDeadTime = deadTime;

            // Return Dead Time
            return UnitConversions.ConvertTime(deadTime, UnitOfTime.Minutes, units);
        }

        /// <summary>
        /// Computes the Linear velocity, stores in mLinearVelocity, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <param name="recalculateVolumetricFlowRate"></param>
        public double ComputeLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec, bool recalculateVolumetricFlowRate = true)
        {
            double linearVelocity;

            if (recalculateVolumetricFlowRate)
            {
                ComputeVolumetricFlowRate(UnitOfFlowRate.MLPerMin);
            }

            var radius = mColumnInnerDiameter / 2.0;
            if (Math.Abs(radius) > float.Epsilon)
            {
                // Units in cm/min
                linearVelocity = mVolumetricFlowRate / (Math.PI * Math.Pow(radius, 2));

                // Divide Linear Velocity by epsilon if a packed capillary
                if (mCapillaryType == CapillaryType.PackedCapillary && Math.Abs(mInterparticlePorosity) > float.Epsilon)
                {
                    linearVelocity /= mInterparticlePorosity;
                }
            }
            else
            {
                linearVelocity = 0;
            }

            mLinearVelocity = linearVelocity;

            // Return Linear Velocity
            return UnitConversions.ConvertLinearVelocity(linearVelocity, UnitOfLinearVelocity.CmPerMin, units);
        }

        /// <summary>
        /// Compute the viscosity of an acetonitrile / methanol solution
        /// </summary>
        /// <param name="percentAcetonitrile">Percent acetonitrile (0 to 100)</param>
        /// <param name="temperature">Temperature</param>
        /// <param name="temperatureUnits">Temperature units</param>
        /// <param name="viscosityUnits">Viscosity units</param>
        /// <returns>Computed viscosity</returns>
        public static double ComputeMeCNViscosity(double percentAcetonitrile, double temperature, UnitOfTemperature temperatureUnits = UnitOfTemperature.Celsius, UnitOfViscosity viscosityUnits = UnitOfViscosity.Poise)
        {
            try
            {
                // Convert percent acetonitrile to a fraction
                var phi = percentAcetonitrile / 100.0;
                if (phi < 0)
                    phi = 0;

                if (phi > 100)
                    phi = 100;

                var kelvin = UnitConversions.ConvertTemperature(temperature, temperatureUnits, UnitOfTemperature.Kelvin);

                double viscosityInCentiPoise;
                if (kelvin > 0)
                {
                    viscosityInCentiPoise = Math.Exp(phi * (-3.476 + 726.0 / kelvin) + (1 - phi) * (-5.414 + 1566.0 / kelvin) + phi * (-1.762 + 929.0 / kelvin));
                }
                else
                {
                    viscosityInCentiPoise = 0;
                }

                return UnitConversions.ConvertViscosity(viscosityInCentiPoise, UnitOfViscosity.CentiPoise, viscosityUnits);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Computes the Volumetric flow rate, stores in mVolumetricFlowRate, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeVolumetricFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            double volFlowRate;

            var radius = mColumnInnerDiameter / 2.0;

            if (Math.Abs(mSolventViscosity) > float.Epsilon && Math.Abs(mColumnLength) > float.Epsilon)
            {
                // Compute flow rate, in mL/min
                if (mCapillaryType == CapillaryType.OpenTubularCapillary)
                {
                    // Open tubular capillary
                    volFlowRate = mBackPressure * Math.Pow(radius, 4) * Math.PI / (8 * mSolventViscosity * mColumnLength);
                }
                else if (Math.Abs(mInterparticlePorosity - 1) > float.Epsilon)
                {
                    // Packed capillary
                    volFlowRate = mBackPressure * Math.Pow(mParticleDiameter, 2) * Math.Pow(mInterparticlePorosity, 2) * Math.PI * Math.Pow(radius, 2) * mInterparticlePorosity / (180 * mSolventViscosity * mColumnLength * Math.Pow(1 - mInterparticlePorosity, 2));
                }
                else
                {
                    volFlowRate = 0;
                }

                // Convert volFlowRate to mL/min
                volFlowRate *= 60;
            }
            else
            {
                volFlowRate = 0;
            }

            mVolumetricFlowRate = volFlowRate;

            // Compute Dead Time (and Linear Velocity)
            ComputeDeadTime(UnitOfTime.Minutes, false);

            return UnitConversions.ConvertVolumetricFlowRate(volFlowRate, UnitOfFlowRate.MLPerMin, units);
        }

        /// <summary>
        /// Computes the Volumetric flow rate using the dead time, stores in mVolumetricFlowRate, and returns it
        /// This requires modifying the pressure value to give the computed volumetric flow rate
        /// </summary>
        /// <param name="newBackPressure">Output: new back pressure</param>
        /// <param name="units"></param>
        /// <param name="pressureUnits"></param>
        public double ComputeVolumetricFlowRateUsingDeadTime(
            out double newBackPressure,
            UnitOfFlowRate units = UnitOfFlowRate.NLPerMin,
            UnitOfPressure pressureUnits = UnitOfPressure.Psi)
        {
            newBackPressure = 0;
            double volFlowRate;

            var radius = mColumnInnerDiameter / 2.0;

            // First find vol flow rate that gives observed dead time
            if (Math.Abs(mColumnDeadTime) > float.Epsilon)
            {
                // Vol flow rate in mL/min
                volFlowRate = mColumnLength * (Math.PI * Math.Pow(radius, 2)) / mColumnDeadTime;

                if (mCapillaryType == CapillaryType.PackedCapillary)
                {
                    // Packed Capillary
                    volFlowRate *= mInterparticlePorosity;
                }

                // Store the new value
                mVolumetricFlowRate = volFlowRate;

                // Now find pressure that gives computed volFlowRate
                // The ComputeBackPressure sub will store the new pressure
                newBackPressure = ComputeBackPressure(pressureUnits);
            }
            else
            {
                volFlowRate = 0;
                mVolumetricFlowRate = 0;
            }

            // Compute Linear Velocity (but not the dead time)
            ComputeLinearVelocity(UnitOfLinearVelocity.CmPerSec, false);

            return UnitConversions.ConvertVolumetricFlowRate(volFlowRate, UnitOfFlowRate.MLPerMin, units);
        }

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
            return UnitConversions.ConvertPressure(mBackPressure, UnitOfPressure.DynesPerSquareCm, units);
        }

        public CapillaryType GetCapillaryType()
        {
            return mCapillaryType;
        }

        public double GetColumnInnerDiameter(UnitOfLength units = UnitOfLength.Microns)
        {
            return UnitConversions.ConvertLength(mColumnInnerDiameter, UnitOfLength.CM, units);
        }

        public double GetColumnLength(UnitOfLength units = UnitOfLength.CM)
        {
            return UnitConversions.ConvertLength(mColumnLength, UnitOfLength.CM, units);
        }

        public double GetColumnVolume(UnitOfVolume units = UnitOfVolume.UL)
        {
            // Column volume isn't stored; simply re-compute it
            return ComputeColumnVolume(units);
        }

        public double GetDeadTime(UnitOfTime units = UnitOfTime.Minutes)
        {
            return UnitConversions.ConvertTime(mColumnDeadTime, UnitOfTime.Minutes, units);
        }

        public double GetInterparticlePorosity()
        {
            return mInterparticlePorosity;
        }

        public double GetLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec)
        {
            return UnitConversions.ConvertLinearVelocity(mLinearVelocity, UnitOfLinearVelocity.CmPerMin, units);
        }

        public double GetParticleDiameter(UnitOfLength units = UnitOfLength.Microns)
        {
            return UnitConversions.ConvertLength(mParticleDiameter, UnitOfLength.CM, units);
        }

        public double GetSolventViscosity(UnitOfViscosity units = UnitOfViscosity.Poise)
        {
            return UnitConversions.ConvertViscosity(mSolventViscosity, UnitOfViscosity.Poise, units);
        }

        public double GetVolumetricFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            return UnitConversions.ConvertVolumetricFlowRate(mVolumetricFlowRate, UnitOfFlowRate.MLPerMin, units);
        }

        /// <summary>
        /// Updates the auto-compute mode
        /// </summary>
        /// <param name="autoCompute"></param>
        /// <remarks>
        /// When true, values will be auto-computed based on mAutoComputeMode
        /// When false, you must manually call a Compute method to re-compute the value
        /// </remarks>
        public void SetAutoComputeEnabled(bool autoCompute)
        {
            mAutoCompute = autoCompute;
        }

        /// <summary>
        /// The value to compute when mAutoCompute is true
        /// </summary>
        /// <param name="autoComputeMode"></param>
        public void SetAutoComputeMode(AutoComputeMode autoComputeMode)
        {
            mAutoComputeMode = autoComputeMode;
        }

        public void SetBackPressure(double backPressure, UnitOfPressure units = UnitOfPressure.Psi)
        {
            mBackPressure = UnitConversions.ConvertPressure(backPressure, units, UnitOfPressure.DynesPerSquareCm);
            CheckAutoCompute();
        }

        public void SetCapillaryType(CapillaryType capillaryType)
        {
            mCapillaryType = capillaryType;
            CheckAutoCompute();
        }

        public void SetColumnInnerDiameter(double columnInnerDiameter, UnitOfLength units = UnitOfLength.Microns)
        {
            mColumnInnerDiameter = UnitConversions.ConvertLength(columnInnerDiameter, units, UnitOfLength.CM);
            CheckAutoCompute();
        }

        public void SetColumnLength(double columnLength, UnitOfLength units = UnitOfLength.CM)
        {
            mColumnLength = UnitConversions.ConvertLength(columnLength, units, UnitOfLength.CM);
            CheckAutoCompute();
        }

        public void SetDeadTime(double deadTime, UnitOfTime units = UnitOfTime.Minutes)
        {
            mColumnDeadTime = UnitConversions.ConvertTime(deadTime, units, UnitOfTime.Minutes);
            CheckAutoCompute();
        }

        public void SetInterparticlePorosity(double porosity)
        {
            if (porosity is >= 0 and <= 1)
            {
                mInterparticlePorosity = porosity;
            }

            CheckAutoCompute();
        }

        public void SetParticleDiameter(double particleDiameter, UnitOfLength units = UnitOfLength.Microns)
        {
            mParticleDiameter = UnitConversions.ConvertLength(particleDiameter, units, UnitOfLength.CM);
            CheckAutoCompute();
        }

        public void SetSolventViscosity(double solventViscosity, UnitOfViscosity units = UnitOfViscosity.Poise)
        {
            mSolventViscosity = UnitConversions.ConvertViscosity(solventViscosity, units, UnitOfViscosity.Poise);
            CheckAutoCompute();
        }

        public void SetVolumetricFlowRate(double volumetricFlowRate, UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            mVolumetricFlowRate = UnitConversions.ConvertVolumetricFlowRate(volumetricFlowRate, units, UnitOfFlowRate.MLPerMin);
            CheckAutoCompute();
        }
    }
}
