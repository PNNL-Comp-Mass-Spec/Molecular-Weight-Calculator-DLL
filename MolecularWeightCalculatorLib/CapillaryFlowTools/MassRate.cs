using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.CapillaryFlowTools
{
    /// <summary>
    /// Mass flow rate tools
    /// </summary>
    [ComVisible(false)]
    public class MassRate
    {
        public MassRate()
        {
            // ReSharper disable RedundantArgumentDefaultValue

            SetConcentration(1, UnitOfConcentration.MicroMolar);
            SetVolumetricFlowRate(600, UnitOfFlowRate.NLPerMin);
            SetInjectionTime(5, UnitOfTime.Minutes);

            // ReSharper restore RedundantArgumentDefaultValue

            // Recompute
            ComputeValues();
        }

        /// <summary>
        /// Units: Molar
        /// </summary>
        private double mSampleConcentration;

        /// <summary>
        /// Units: g/mole
        /// </summary>
        private double mSampleMass;

        /// <summary>
        /// Units: mL/min
        /// </summary>
        private double mVolumetricFlowRate;

        /// <summary>
        /// Units: min
        /// </summary>
        private double mInjectionTime;

        /// <summary>
        /// Units: Moles/min
        /// </summary>
        private double mMassFlowRate;

        /// <summary>
        /// Units: moles
        /// </summary>
        private double mMolesInjected;

        /// <summary>
        /// Computes the MassFlowRate and Moles injected based on stored values for sample concentration, volumetric flow rate, and injection time
        /// Stores the computed values in mMassFlowRate and mMolesInjected
        /// Returns mass flow rate
        /// </summary>
        /// <param name="units"></param>
        public double ComputeMassFlowRate(UnitOfMassFlowRate units = UnitOfMassFlowRate.FmolPerSec)
        {
            ComputeValues();
            return GetMassFlowRate(units);
        }

        /// <summary>
        /// Computes the MassFlowRate and Moles injected based on stored values for sample concentration, volumetric flow rate, and injection time
        /// Stores the computed values in mMassFlowRate and mMolesInjected
        /// Returns moles injected
        /// </summary>
        /// <param name="units"></param>
        public double ComputeMolesInjected(UnitOfMolarAmount units = UnitOfMolarAmount.FemtoMoles)
        {
            ComputeValues();
            return GetMolesInjected(units);
        }

        private void ComputeValues()
        {
            // Compute mass flow rate in moles/min
            mMassFlowRate = mSampleConcentration * mVolumetricFlowRate / 1000.0;

            // Compute moles injected in moles
            mMolesInjected = mMassFlowRate * mInjectionTime;
        }

        public double GetConcentration(UnitOfConcentration units = UnitOfConcentration.MicroMolar)
        {
            return UnitConversions.ConvertConcentration(mSampleConcentration, UnitOfConcentration.Molar, units, mSampleMass);
        }

        public double GetInjectionTime(UnitOfTime units = UnitOfTime.Minutes)
        {
            return UnitConversions.ConvertTime(mInjectionTime, UnitOfTime.Minutes, units);
        }

        public double GetMassFlowRate(UnitOfMassFlowRate units = UnitOfMassFlowRate.FmolPerSec)
        {
            return UnitConversions.ConvertMassFlowRate(mMassFlowRate, UnitOfMassFlowRate.MolesPerMin, units);
        }

        public double GetMolesInjected(UnitOfMolarAmount units = UnitOfMolarAmount.FemtoMoles)
        {
            return UnitConversions.ConvertMoles(mMolesInjected, UnitOfMolarAmount.Moles, units);
        }

        public double GetSampleMass()
        {
            return mSampleMass;
        }

        public double GetVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            return UnitConversions.ConvertVolumetricFlowRate(mVolumetricFlowRate, UnitOfFlowRate.MLPerMin, units);
        }

        public void SetConcentration(double concentration, UnitOfConcentration units = UnitOfConcentration.MicroMolar)
        {
            mSampleConcentration = UnitConversions.ConvertConcentration(concentration, units, UnitOfConcentration.Molar, mSampleMass);
            ComputeValues();
        }

        public void SetInjectionTime(double injectionTime, UnitOfTime units = UnitOfTime.Minutes)
        {
            mInjectionTime = UnitConversions.ConvertTime(injectionTime, units, UnitOfTime.Minutes);
            ComputeValues();
        }

        public void SetSampleMass(double massInGramsPerMole)
        {
            if (massInGramsPerMole >= 0)
            {
                mSampleMass = massInGramsPerMole;
            }
            else
            {
                mSampleMass = 0;
            }

            ComputeValues();
        }

        public void SetVolumetricFlowRate(double volumetricFlowRate, UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            mVolumetricFlowRate = UnitConversions.ConvertVolumetricFlowRate(volumetricFlowRate, units, UnitOfFlowRate.MLPerMin);
            ComputeValues();
        }
    }
}