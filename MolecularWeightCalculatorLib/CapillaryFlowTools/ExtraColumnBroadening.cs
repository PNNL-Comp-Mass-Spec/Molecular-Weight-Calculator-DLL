using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.CapillaryFlowTools
{
    [ComVisible(false)]
    public class ExtraColumnBroadening
    {
        public ExtraColumnBroadening()
        {
            // Ignore Spelling: acetonitrile, dynes, ng, ug

            // Recompute
            ComputeValues();
        }

        /// <summary>
        /// Units: cm/min
        /// </summary>
        private double mLinearVelocity;

        /// <summary>
        /// Units: cm^2/sec
        /// </summary>
        private double mDiffusionCoefficient = 0.000005;

        /// <summary>
        /// Units: cm
        /// </summary>
        private double mOpenTubeLength;

        /// <summary>
        /// Units: cm
        /// </summary>
        private double mOpenTubeInnerDiameter;

        /// <summary>
        /// Units: sec
        /// </summary>
        private double mInitialPeakWidth;

        /// <summary>
        /// Units: sec^2
        /// </summary>
        private double mTemporalVariance;

        /// <summary>
        /// Units: sec^2
        /// </summary>
        private double mAdditionalTemporalVariance;

        /// <summary>
        /// Units: sec
        /// </summary>
        private double mResultantPeakWidth;


        public double ComputeResultantPeakWidth(UnitOfTime units = UnitOfTime.Seconds)
        {
            ComputeValues();

            return GetResultantPeakWidth(units);
        }

        private void ComputeValues()
        {
            if (Math.Abs(mLinearVelocity) > float.Epsilon && Math.Abs(mDiffusionCoefficient) > float.Epsilon)
            {
                // Temporal variance, in sec^2
                mTemporalVariance = Math.Pow(mOpenTubeInnerDiameter, 2) * mOpenTubeLength / (96 * mDiffusionCoefficient * mLinearVelocity / 60.0);
            }
            else
            {
                mTemporalVariance = 0;
            }

            var initialPeakVariance = Math.Pow(mInitialPeakWidth / 4.0, 2);

            var sumOfVariances = initialPeakVariance + mTemporalVariance + mAdditionalTemporalVariance;

            if (sumOfVariances >= 0)
            {
                // ResultantPeakWidth at the base = 4 sigma  and  sigma = SquareRoot(Total_Variance)
                mResultantPeakWidth = 4 * Math.Sqrt(sumOfVariances);
            }
            else
            {
                mResultantPeakWidth = 0;
            }
        }

        /// <summary>
        /// Computes the optimum linear velocity, based on <paramref name="particleDiameter"/> and <see cref="mDiffusionCoefficient"/>
        /// </summary>
        /// <param name="particleDiameter"></param>
        /// <param name="units"></param>
        public double ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(double particleDiameter, UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec)
        {
            var optimumLinearVelocity = 0.0;

            if (Math.Abs(particleDiameter) > float.Epsilon)
            {
                optimumLinearVelocity = 3 * mDiffusionCoefficient / particleDiameter;

                optimumLinearVelocity = UnitConversions.ConvertLinearVelocity(optimumLinearVelocity, UnitOfLinearVelocity.CmPerSec, units);
            }

            return optimumLinearVelocity;
        }

        /// <summary>
        /// Copy linear velocity, column length, and column inner diameter to the ExtraColumnBroadening container
        /// </summary>
        public void CopyCachedValuesFromCapillaryFlow(CapillaryFlow capillaryFlowData)
        {
            mLinearVelocity = capillaryFlowData.GetLinearVelocity(UnitOfLinearVelocity.CmPerMin);
            mOpenTubeLength = capillaryFlowData.GetColumnLength(UnitOfLength.CM);
            mOpenTubeInnerDiameter = capillaryFlowData.GetColumnInnerDiameter(UnitOfLength.CM);
        }

        public double GetAdditionalVarianceInSquareSeconds()
        {
            return mAdditionalTemporalVariance;
        }

        public double GetDiffusionCoefficient(UnitOfDiffusionCoefficient units = UnitOfDiffusionCoefficient.CmSquaredPerSec)
        {
            return UnitConversions.ConvertDiffusionCoefficient(mDiffusionCoefficient, UnitOfDiffusionCoefficient.CmSquaredPerSec, units);
        }

        public double GetInitialPeakWidthAtBase(UnitOfTime units = UnitOfTime.Seconds)
        {
            return UnitConversions.ConvertTime(mInitialPeakWidth, UnitOfTime.Seconds, units);
        }

        public double GetLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.MmPerMin)
        {
            return UnitConversions.ConvertLinearVelocity(mLinearVelocity, UnitOfLinearVelocity.CmPerMin, units);
        }

        public double GetOpenTubeInnerDiameter(UnitOfLength units = UnitOfLength.Microns)
        {
            return UnitConversions.ConvertLength(mOpenTubeInnerDiameter, UnitOfLength.CM, units);
        }

        public double GetOpenTubeLength(UnitOfLength units = UnitOfLength.CM)
        {
            return UnitConversions.ConvertLength(mOpenTubeLength, UnitOfLength.CM, units);
        }

        public double GetResultantPeakWidth(UnitOfTime units = UnitOfTime.Seconds)
        {
            return UnitConversions.ConvertTime(mResultantPeakWidth, UnitOfTime.Seconds, units);
        }

        public double GetTemporalVarianceInSquareSeconds()
        {
            return mTemporalVariance;
        }

        public void SetAdditionalVariance(double additionalVarianceInSquareSeconds)
        {
            mAdditionalTemporalVariance = additionalVarianceInSquareSeconds;
            ComputeValues();
        }

        public void SetDiffusionCoefficient(double diffusionCoefficient, UnitOfDiffusionCoefficient units = UnitOfDiffusionCoefficient.CmSquaredPerSec)
        {
            mDiffusionCoefficient = UnitConversions.ConvertDiffusionCoefficient(diffusionCoefficient, units, UnitOfDiffusionCoefficient.CmSquaredPerSec);
            ComputeValues();
        }

        public void SetInitialPeakWidthAtBase(double width, UnitOfTime units = UnitOfTime.Seconds)
        {
            mInitialPeakWidth = UnitConversions.ConvertTime(width, units, UnitOfTime.Seconds);
            ComputeValues();
        }

        public void SetLinearVelocity(double linearVelocity, UnitOfLinearVelocity units = UnitOfLinearVelocity.MmPerMin)
        {
            mLinearVelocity = UnitConversions.ConvertLinearVelocity(linearVelocity, units, UnitOfLinearVelocity.CmPerMin);
            ComputeValues();
        }

        public void SetOpenTubeInnerDiameter(double openTubeInnerDiameter, UnitOfLength units = UnitOfLength.Microns)
        {
            mOpenTubeInnerDiameter = UnitConversions.ConvertLength(openTubeInnerDiameter, units, UnitOfLength.CM);
            ComputeValues();
        }

        public void SetOpenTubeLength(double length, UnitOfLength units = UnitOfLength.CM)
        {
            mOpenTubeLength = UnitConversions.ConvertLength(length, units, UnitOfLength.CM);
            ComputeValues();
        }
    }
}