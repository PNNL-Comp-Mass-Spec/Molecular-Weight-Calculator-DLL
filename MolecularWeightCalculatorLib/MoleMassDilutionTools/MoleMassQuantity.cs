using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.MoleMassDilutionTools
{
    [ComVisible(false)]
    public class MoleMassQuantity
    {
        public MoleMassQuantity()
        {
            SetAutoComputeEnabled(false);

            SetAutoComputeMode(AutoComputeQuantityMode.FindConcentration);

            // ReSharper disable RedundantArgumentDefaultValue

            SetAmount(1, Unit.Moles);
            SetVolume(100, UnitOfExtendedVolume.ML);
            SetConcentration(1, UnitOfMoleMassConcentration.Molar);

            // ReSharper restore RedundantArgumentDefaultValue

            // Recompute
            ComputeAmount();
        }

        /// <summary>
        /// Amount, in moles
        /// </summary>
        private double mAmount;

        /// <summary>
        /// Volume, in L
        /// </summary>
        private double mVolume;

        /// <summary>
        /// Concentration, in molarity
        /// </summary>
        private double mConcentration;

        /// <summary>
        /// Sample mass, in g
        /// </summary>
        private double mSampleMass;

        /// <summary>
        /// Sample density, in g/mL
        /// </summary>
        private double mSampleDensity;

        /// <summary>
        /// When true, automatically compute quantities whenever any value changes
        /// </summary>
        private bool mAutoCompute;

        /// <summary>
        /// The value to compute when mAutoCompute is true
        /// </summary>
        private AutoComputeQuantityMode mAutoComputeMode;

        /// <summary>
        /// Checks if AutoCompute is enabled
        /// If yes, calls the appropriate function
        /// </summary>
        private void CheckAutoCompute()
        {
            if (mAutoCompute)
            {
                switch (mAutoComputeMode)
                {
                    case AutoComputeQuantityMode.FindVolume:
                        ComputeVolume();
                        break;
                    case AutoComputeQuantityMode.FindConcentration:
                        ComputeConcentration();
                        break;
                    default:
                        // Includes FindAmount
                        ComputeAmount();
                        break;
                }
            }
        }

        /// <summary>
        /// Computes Amount using Volume and Concentration, storing the result in Amount
        /// </summary>
        /// <param name="units"></param>
        /// <returns>Amount, with the specified units</returns>
        public double ComputeAmount(Unit units = Unit.Moles)
        {
            mAmount = mConcentration * mVolume;

            return UnitConversions.ConvertAmount(mAmount, Unit.Moles, units, mSampleMass, mSampleDensity);
        }

        /// <summary>
        /// Computes Concentration using Amount and Volume, storing the result in Concentration
        /// </summary>
        /// <param name="units"></param>
        /// <returns>Concentration, with the specified units</returns>
        public double ComputeConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            if (Math.Abs(mVolume) > float.Epsilon)
            {
                mConcentration = mAmount / mVolume;
            }
            else
            {
                mConcentration = 0;
            }

            return UnitConversions.ConvertConcentration(mConcentration, UnitOfMoleMassConcentration.Molar, units, mSampleMass);
        }

        /// <summary>
        /// Computes Volume using Amount and Concentration, storing the result in Volume
        /// </summary>
        /// <param name="units"></param>
        /// <returns>Volume, with the specified units</returns>
        public double ComputeVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.L)
        {
            if (Math.Abs(mConcentration) > float.Epsilon)
            {
                mVolume = mAmount / mConcentration;
            }
            else
            {
                mVolume = 0;
            }

            return UnitConversions.ConvertVolumeExtended(mVolume, UnitOfExtendedVolume.L, units);
        }

        // Get Methods
        // These retrieve the most recently computed value
        // If mAutoCompute = False, must manually call a Compute Sub to recompute the value

        public bool GetAutoComputeEnabled()
        {
            return mAutoCompute;
        }

        public AutoComputeQuantityMode GetAutoComputeMode()
        {
            return mAutoComputeMode;
        }

        public double GetAmount(Unit units = Unit.Moles)
        {
            return UnitConversions.ConvertAmount(mAmount, Unit.Moles, units, mSampleMass, mSampleDensity);
        }

        public double GetConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return UnitConversions.ConvertConcentration(mConcentration, UnitOfMoleMassConcentration.Molar, units, mSampleMass);
        }

        public double GetVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return UnitConversions.ConvertVolumeExtended(mVolume, UnitOfExtendedVolume.L, units);
        }

        public double GetSampleDensity()
        {
            return mSampleDensity;
        }

        public double GetSampleMass()
        {
            return mSampleMass;
        }

        /// <summary>
        /// Updates the auto-compute mode for quantity-related values
        /// </summary>
        /// <param name="autoCompute"></param>
        /// <remarks>
        /// When true, quantity-related values will be auto-computed based on mAutoComputeMode
        /// When false, you must manually call a Compute method to re-compute the value
        /// </remarks>
        public void SetAutoComputeEnabled(bool autoCompute)
        {
            mAutoCompute = autoCompute;
        }

        /// <summary>
        /// Auto-compute mode for quantity-related values
        /// </summary>
        /// <param name="autoComputeMode"></param>
        public void SetAutoComputeMode(AutoComputeQuantityMode autoComputeMode)
        {
            mAutoComputeMode = autoComputeMode;
        }

        public void SetAmount(double amount, Unit units = Unit.Moles)
        {
            mAmount = UnitConversions.ConvertAmount(amount, units, Unit.Moles, mSampleMass, mSampleDensity);
            CheckAutoCompute();
        }

        public void SetConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            mConcentration = UnitConversions.ConvertConcentration(concentration, units, UnitOfMoleMassConcentration.Molar, mSampleMass);
            CheckAutoCompute();
        }

        public void SetVolume(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mVolume = UnitConversions.ConvertVolumeExtended(volume, units, UnitOfExtendedVolume.L);
        }

        // ReSharper disable once InconsistentNaming
        public void SetSampleDensity(double densityInGramsPerML)
        {
            if (densityInGramsPerML >= 0)
            {
                mSampleDensity = densityInGramsPerML;
            }
            else
            {
                mSampleDensity = 0;
            }

            CheckAutoCompute();
        }

        public void SetSampleMass(double massInGrams)
        {
            if (massInGrams >= 0)
            {
                mSampleMass = massInGrams;
            }
            else
            {
                mSampleMass = 0;
            }

            CheckAutoCompute();
        }
    }
}
