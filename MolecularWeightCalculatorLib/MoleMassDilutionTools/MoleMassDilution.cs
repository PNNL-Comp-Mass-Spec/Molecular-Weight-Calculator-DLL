using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.MoleMassDilutionTools
{
    [ComVisible(false)]
    public class MoleMassDilution
    {
        public MoleMassDilution()
        {
            SetAutoComputeEnabled(false);

            SetAutoComputeMode(AutoComputeDilutionMode.FindRequiredDilutionVolumes);

            // ReSharper disable RedundantArgumentDefaultValue

            SetInitialConcentration(10, UnitOfMoleMassConcentration.Molar);
            SetVolumeStockSolution(3, UnitOfExtendedVolume.ML);
            SetFinalConcentration(2, UnitOfMoleMassConcentration.Molar);
            SetVolumeDilutingSolvent(12, UnitOfExtendedVolume.ML);
            SetTotalFinalVolume(15, UnitOfExtendedVolume.ML);

            // ReSharper restore RedundantArgumentDefaultValue

            // Recompute
            ComputeRequiredStockAndDilutingSolventVolumes(out _);
        }

        /// <summary>
        /// Initial concentration, in molarity
        /// </summary>
        private double mInitialConcentration;

        /// <summary>
        /// Stock solution volume, in L
        /// </summary>
        private double mStockSolutionVolume;

        /// <summary>
        /// Final concentration, in molarity
        /// </summary>
        private double mFinalConcentration;

        /// <summary>
        /// Diluting solvent volume, in L
        /// </summary>
        private double mDilutingSolventVolume;

        /// <summary>
        /// Total final volume, in L
        /// </summary>
        private double mTotalFinalVolume;

        /// <summary>
        /// Sample mass, in g; required for calculating certain concentration units
        /// </summary>
        private double mSampleMass;

        /// <summary>
        /// When true, automatically compute dilution results whenever any value changes
        /// </summary>
        private bool mAutoCompute;

        /// <summary>
        /// The value to compute when mAutoCompute is true
        /// </summary>
        private AutoComputeDilutionMode mAutoComputeMode;

        /// <summary>
        /// Checks if AutoCompute is enabled
        /// If yes, calls the appropriate method
        /// </summary>
        private void CheckAutoCompute()
        {
            if (mAutoCompute)
            {
                switch (mAutoComputeMode)
                {
                    case AutoComputeDilutionMode.FindRequiredTotalVolume:
                        ComputeTotalVolume(out _);
                        break;
                    case AutoComputeDilutionMode.FindFinalConcentration:
                        ComputeFinalConcentration();
                        break;
                    case AutoComputeDilutionMode.FindInitialConcentration:
                        ComputeInitialConcentration();
                        break;
                    default:
                        // Includes FindRequiredDilutionVolumes
                        ComputeRequiredStockAndDilutingSolventVolumes(out _);
                        break;
                }
            }
        }

        /// <summary>
        /// Computes the Final Concentration, storing in .FinalConcentration, and returning it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeFinalConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            if (Math.Abs(mTotalFinalVolume) > float.Epsilon)
            {
                mFinalConcentration = mInitialConcentration * mStockSolutionVolume / mTotalFinalVolume;
            }
            else
            {
                mTotalFinalVolume = 0;
            }

            mDilutingSolventVolume = mTotalFinalVolume - mStockSolutionVolume;
            if (mDilutingSolventVolume < 0)
                mDilutingSolventVolume = -1;

            return UnitConversions.ConvertConcentration(mFinalConcentration, UnitOfMoleMassConcentration.Molar, units, mSampleMass);
        }

        /// <summary>
        /// Computes the Initial Concentration, storing in .InitialConcentration, and returning it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeInitialConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            if (Math.Abs(mStockSolutionVolume) > float.Epsilon)
            {
                mInitialConcentration = mFinalConcentration * mTotalFinalVolume / mStockSolutionVolume;
            }
            else
            {
                mInitialConcentration = 0;
            }

            mDilutingSolventVolume = mTotalFinalVolume - mStockSolutionVolume;
            if (mDilutingSolventVolume < 0)
                mDilutingSolventVolume = -1;

            return UnitConversions.ConvertConcentration(mInitialConcentration, UnitOfMoleMassConcentration.Molar, units, mSampleMass);
        }

        /// <summary>
        /// Computes the required dilution volumes using initial concentration, final concentration
        /// and total final volume, storing in .StockSolutionVolume and .DilutingSolventVolume,
        /// and returning .StockSolutionVolume
        /// </summary>
        /// <param name="newDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="stockSolutionUnits"></param>
        /// <param name="dilutingSolventUnits"></param>
        public double ComputeRequiredStockAndDilutingSolventVolumes(
            out double newDilutingSolventVolume,
            UnitOfExtendedVolume stockSolutionUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume dilutingSolventUnits = UnitOfExtendedVolume.ML)
        {
            if (Math.Abs(mInitialConcentration) > float.Epsilon)
            {
                mStockSolutionVolume = mFinalConcentration * mTotalFinalVolume / mInitialConcentration;
            }
            else
            {
                mStockSolutionVolume = 0;
            }

            mDilutingSolventVolume = mTotalFinalVolume - mStockSolutionVolume;

            if (mDilutingSolventVolume < 0)
            {
                mDilutingSolventVolume = -1;
                mStockSolutionVolume = -1;
                newDilutingSolventVolume = -1;
                return -1;
            }

            newDilutingSolventVolume = UnitConversions.ConvertVolumeExtended(mDilutingSolventVolume, UnitOfExtendedVolume.L, dilutingSolventUnits);

            return UnitConversions.ConvertVolumeExtended(mStockSolutionVolume, UnitOfExtendedVolume.L, stockSolutionUnits);
        }

        /// <summary>
        /// Compute the total volume following the dilution, storing in .TotalFinalVolume, and returning it
        /// </summary>
        /// <param name="newDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="totalVolumeUnits"></param>
        /// <param name="dilutingSolventUnits"></param>
        /// <remarks>
        /// If InitialConcentration is less than FinalConcentration, this represents evaporation or sublimation
        /// In this case, the computed value for TotalFinalVolume represents the volume that the solution must be reduced to in order to obtain the final concentration
        /// The computed value for newDilutingSolventVolume will be -1 when initialConcentration is less than finalConcentration
        /// </remarks>
        public double ComputeTotalVolume(
            out double newDilutingSolventVolume,
            UnitOfExtendedVolume totalVolumeUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume dilutingSolventUnits = UnitOfExtendedVolume.ML)
        {
            if (mInitialConcentration > 0 && mFinalConcentration > 0)
            {
                mTotalFinalVolume = mInitialConcentration * mStockSolutionVolume / mFinalConcentration;
                if (mTotalFinalVolume < 0)
                {
                    mTotalFinalVolume = 0;
                }
            }
            else
            {
                mTotalFinalVolume = 0;
            }

            mDilutingSolventVolume = mTotalFinalVolume - mStockSolutionVolume;
            if (mDilutingSolventVolume < 0)
            {
                mDilutingSolventVolume = -1;
                newDilutingSolventVolume = -1;
            }
            else
            {
                newDilutingSolventVolume = UnitConversions.ConvertVolumeExtended(mDilutingSolventVolume, UnitOfExtendedVolume.L, dilutingSolventUnits);
            }

            return UnitConversions.ConvertVolumeExtended(mTotalFinalVolume, UnitOfExtendedVolume.L, totalVolumeUnits);
        }

        // Get Methods
        // These retrieve the most recently computed value
        // If mAutoCompute = False, must manually call a Compute Sub to recompute the value

        public bool GetAutoComputeEnabled()
        {
            return mAutoCompute;
        }

        public AutoComputeDilutionMode GetAutoComputeMode()
        {
            return mAutoComputeMode;
        }

        public double GetFinalConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return UnitConversions.ConvertConcentration(mFinalConcentration, UnitOfMoleMassConcentration.Molar, units, mSampleMass);
        }

        public double GetInitialConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return UnitConversions.ConvertConcentration(mInitialConcentration, UnitOfMoleMassConcentration.Molar, units, mSampleMass);
        }

        public double GetSampleMass()
        {
            return mSampleMass;
        }

        public double GetTotalFinalVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return UnitConversions.ConvertVolumeExtended(mTotalFinalVolume, UnitOfExtendedVolume.L, units);
        }

        public double GetVolumeDilutingSolvent(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return UnitConversions.ConvertVolumeExtended(mDilutingSolventVolume, UnitOfExtendedVolume.L, units);
        }

        public double GetVolumeStockSolution(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return UnitConversions.ConvertVolumeExtended(mStockSolutionVolume, UnitOfExtendedVolume.L, units);
        }

        /// <summary>
        /// Updates the auto-compute mode for dilution-related values
        /// </summary>
        /// <param name="autoCompute"></param>
        /// <remarks>
        /// When true, dilution-related values will be auto-computed based on mAutoComputeMode
        /// When false, you must manually call a Compute method to re-compute the value
        /// </remarks>
        public void SetAutoComputeEnabled(bool autoCompute)
        {
            mAutoCompute = autoCompute;
        }

        /// <summary>
        /// Auto-compute mode for dilution-related values
        /// </summary>
        /// <param name="autoComputeMode"></param>
        public void SetAutoComputeMode(AutoComputeDilutionMode autoComputeMode)
        {
            mAutoComputeMode = autoComputeMode;
        }

        public void SetFinalConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            mFinalConcentration = UnitConversions.ConvertConcentration(concentration, units, UnitOfMoleMassConcentration.Molar, mSampleMass);
            CheckAutoCompute();
        }

        public void SetInitialConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            mInitialConcentration = UnitConversions.ConvertConcentration(concentration, units, UnitOfMoleMassConcentration.Molar, mSampleMass);
            CheckAutoCompute();
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

            CheckAutoCompute();
        }

        public void SetTotalFinalVolume(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mTotalFinalVolume = UnitConversions.ConvertVolumeExtended(volume, units, UnitOfExtendedVolume.L);
            CheckAutoCompute();
        }

        public void SetVolumeDilutingSolvent(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mDilutingSolventVolume = UnitConversions.ConvertVolumeExtended(volume, units, UnitOfExtendedVolume.L);
            CheckAutoCompute();
        }

        public void SetVolumeStockSolution(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mStockSolutionVolume = UnitConversions.ConvertVolumeExtended(volume, units, UnitOfExtendedVolume.L);
            CheckAutoCompute();
        }
    }
}
