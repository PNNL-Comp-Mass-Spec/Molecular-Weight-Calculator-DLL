using System.Runtime.InteropServices;
using MolecularWeightCalculator.CapillaryFlowTools;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator
{
    /// <summary>
    /// Compatibility/COM wrapper for CapillaryFlowTools classes
    /// </summary>
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

        // ReSharper disable once EmptyConstructor
        public CapillaryFlow()
        {
            // Ignore Spelling: acetonitrile
        }

        private readonly CapillaryFlowTools.CapillaryFlow mCapillaryFlow = new();
        private readonly MassRate mMassRate = new();
        private readonly ExtraColumnBroadening mExtraColumnBroadening = new();

        /// <summary>
        /// Computes the back pressure, stores in .BackPressure, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeBackPressure(UnitOfPressure units = UnitOfPressure.Psi)
        {
            return mCapillaryFlow.ComputeBackPressure(units);
        }

        /// <summary>
        /// Computes the column length, stores in .ColumnLength, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeColumnLength(UnitOfLength units = UnitOfLength.CM)
        {
            return mCapillaryFlow.ComputeColumnLength(units);
        }

        public double ComputeColumnVolume(UnitOfVolume units = 0)
        {
            // Computes the column volume and returns it (does not store it)
            return mCapillaryFlow.ComputeColumnVolume(units);
        }

        /// <summary>
        /// Computes the column length, stores in .ColumnLength, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeColumnId(UnitOfLength units = UnitOfLength.Microns)
        {
            return mCapillaryFlow.ComputeColumnInnerDiameter(units);
        }

        /// <summary>
        /// Computes the column dead time, stores in .ColumnDeadTime, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <param name="recalculateVolFlowRate"></param>
        public double ComputeDeadTime(UnitOfTime units = UnitOfTime.Minutes, bool recalculateVolFlowRate = true)
        {
            return mCapillaryFlow.ComputeDeadTime(units, recalculateVolFlowRate);
        }

        public double ComputeExtraColumnBroadeningResultantPeakWidth(UnitOfTime units = UnitOfTime.Seconds)
        {
            return mExtraColumnBroadening.ComputeResultantPeakWidth(units);
        }

        /// <summary>
        /// Computes the Linear velocity, stores in .LinearVelocity, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <param name="recalculateVolFlowRate"></param>
        public double ComputeLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec, bool recalculateVolFlowRate = true)
        {
            return mCapillaryFlow.ComputeLinearVelocity(units, recalculateVolFlowRate);
        }

        /// <summary>
        /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
        /// </summary>
        /// <param name="units"></param>
        public double ComputeMassFlowRate(UnitOfMassFlowRate units = UnitOfMassFlowRate.FmolPerSec)
        {
            return mMassRate.ComputeMassFlowRate(units);
        }

        /// <summary>
        /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
        /// </summary>
        /// <param name="units"></param>
        public double ComputeMassRateMolesInjected(UnitOfMolarAmount units = UnitOfMolarAmount.FemtoMoles)
        {
            return mMassRate.ComputeMolesInjected(units);
        }

        /// <summary>
        /// Computes the optimum linear velocity, based on
        /// mCapillaryFlowParameters.ParticleDiameter
        /// and mExtraColumnBroadeningParameters.DiffusionCoefficient
        /// </summary>
        /// <param name="units"></param>
        public double ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec)
        {
            return mExtraColumnBroadening.ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(mCapillaryFlow.GetParticleDiameter(UnitOfLength.CM), units);
        }

        /// <summary>
        /// Compute the viscosity of an acetonitrile / methanol solution
        /// </summary>
        /// <param name="percentAcetonitrile">Percent acetonitrile (0 to 100)</param>
        /// <param name="temperature">Temperature</param>
        /// <param name="temperatureUnits">Temperature units</param>
        /// <param name="viscosityUnits">Viscosity units</param>
        /// <returns>Computed viscosity</returns>
        public double ComputeMeCNViscosity(double percentAcetonitrile, double temperature, UnitOfTemperature temperatureUnits = UnitOfTemperature.Celsius, UnitOfViscosity viscosityUnits = UnitOfViscosity.Poise)
        {
            return CapillaryFlowTools.CapillaryFlow.ComputeMeCNViscosity(percentAcetonitrile, temperature, temperatureUnits, viscosityUnits);
        }

        /// <summary>
        /// Computes the Volumetric flow rate, stores in .VolumetricFlowRate, and returns it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            return mCapillaryFlow.ComputeVolumetricFlowRate(units);
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
            return mCapillaryFlow.ComputeVolumetricFlowRateUsingDeadTime(out newBackPressure, units, pressureUnits);
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
            return UnitConversions.ConvertConcentration(concentrationIn, currentUnits, newUnits, mMassRate.GetSampleMass());
        }

        public double ConvertDiffusionCoefficient(double diffusionCoefficientIn, UnitOfDiffusionCoefficient currentUnits, UnitOfDiffusionCoefficient newUnits)
        {
            return UnitConversions.ConvertDiffusionCoefficient(diffusionCoefficientIn, currentUnits, newUnits);
        }

        public double ConvertLength(double lengthIn, UnitOfLength currentUnits, UnitOfLength newUnits)
        {
            return UnitConversions.ConvertLength(lengthIn, currentUnits, newUnits);
        }

        public double ConvertLinearVelocity(double linearVelocityIn, UnitOfLinearVelocity currentUnits, UnitOfLinearVelocity newUnits)
        {
            return UnitConversions.ConvertLinearVelocity(linearVelocityIn, currentUnits, newUnits);
        }

        public double ConvertMassFlowRate(double massFlowRateIn, UnitOfMassFlowRate currentUnits, UnitOfMassFlowRate newUnits)
        {
            return UnitConversions.ConvertMassFlowRate(massFlowRateIn, currentUnits, newUnits);
        }

        public double ConvertMoles(double molesIn, UnitOfMolarAmount currentUnits, UnitOfMolarAmount newUnits)
        {
            return UnitConversions.ConvertMoles(molesIn, currentUnits, newUnits);
        }

        public double ConvertPressure(double pressureIn, UnitOfPressure currentUnits, UnitOfPressure newUnits)
        {
            return UnitConversions.ConvertPressure(pressureIn, currentUnits, newUnits);
        }

        public double ConvertTemperature(double temperatureIn, UnitOfTemperature currentUnits, UnitOfTemperature newUnits)
        {
            return UnitConversions.ConvertTemperature(temperatureIn, currentUnits, newUnits);
        }

        public double ConvertTime(double timeIn, UnitOfTime currentUnits, UnitOfTime newUnits)
        {
            return UnitConversions.ConvertTime(timeIn, currentUnits, newUnits);
        }

        public double ConvertViscosity(double viscosityIn, UnitOfViscosity currentUnits, UnitOfViscosity newUnits)
        {
            return UnitConversions.ConvertViscosity(viscosityIn, currentUnits, newUnits);
        }

        public double ConvertVolFlowRate(double volFlowRateIn, UnitOfFlowRate currentUnits, UnitOfFlowRate newUnits)
        {
            return UnitConversions.ConvertVolumetricFlowRate(volFlowRateIn, currentUnits, newUnits);
        }

        public double ConvertVolume(double volume, UnitOfVolume currentUnits, UnitOfVolume newUnits)
        {
            return UnitConversions.ConvertVolume(volume, currentUnits, newUnits);
        }

        /// <summary>
        /// Copy linear velocity, column length, and column inner diameter to the ExtraColumnBroadening container
        /// </summary>
        public void CopyCachedValuesToExtraColumnBroadeningContainer()
        {
            mExtraColumnBroadening.CopyCachedValuesFromCapillaryFlow(mCapillaryFlow);
        }

        public bool GetAutoComputeEnabled()
        {
            return mCapillaryFlow.GetAutoComputeEnabled();
        }

        public AutoComputeMode GetAutoComputeMode()
        {
            return mCapillaryFlow.GetAutoComputeMode();
        }

        public double GetBackPressure(UnitOfPressure units = UnitOfPressure.Psi)
        {
            return mCapillaryFlow.GetBackPressure(units);
        }

        public CapillaryType GetCapillaryType()
        {
            return mCapillaryFlow.GetCapillaryType();
        }

        public double GetColumnId(UnitOfLength units = UnitOfLength.Microns)
        {
            return mCapillaryFlow.GetColumnInnerDiameter(units);
        }

        public double GetColumnLength(UnitOfLength units = UnitOfLength.CM)
        {
            return mCapillaryFlow.GetColumnLength(units);
        }

        public double GetColumnVolume(UnitOfVolume units = UnitOfVolume.UL)
        {
            return mCapillaryFlow.GetColumnVolume(units);
        }

        public double GetDeadTime(UnitOfTime units = UnitOfTime.Minutes)
        {
            return mCapillaryFlow.GetDeadTime(units);
        }

        public double GetExtraColumnBroadeningAdditionalVarianceInSquareSeconds()
        {
            return mExtraColumnBroadening.GetAdditionalVarianceInSquareSeconds();
        }

        public double GetExtraColumnBroadeningDiffusionCoefficient(UnitOfDiffusionCoefficient units = UnitOfDiffusionCoefficient.CmSquaredPerSec)
        {
            return mExtraColumnBroadening.GetDiffusionCoefficient(units);
        }

        public double GetExtraColumnBroadeningInitialPeakWidthAtBase(UnitOfTime units = UnitOfTime.Seconds)
        {
            return mExtraColumnBroadening.GetInitialPeakWidthAtBase(units);
        }

        public double GetExtraColumnBroadeningLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.MmPerMin)
        {
            return mExtraColumnBroadening.GetLinearVelocity(units);
        }

        public double GetExtraColumnBroadeningOpenTubeId(UnitOfLength units = UnitOfLength.Microns)
        {
            return mExtraColumnBroadening.GetOpenTubeInnerDiameter(units);
        }

        public double GetExtraColumnBroadeningOpenTubeLength(UnitOfLength units = UnitOfLength.CM)
        {
            return mExtraColumnBroadening.GetOpenTubeLength(units);
        }

        public double GetExtraColumnBroadeningResultantPeakWidth(UnitOfTime units = UnitOfTime.Seconds)
        {
            return mExtraColumnBroadening.GetResultantPeakWidth(units);
        }

        public double GetExtraColumnBroadeningTemporalVarianceInSquareSeconds()
        {
            return mExtraColumnBroadening.GetTemporalVarianceInSquareSeconds();
        }

        public double GetInterparticlePorosity()
        {
            return mCapillaryFlow.GetInterparticlePorosity();
        }

        public double GetLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec)
        {
            return mCapillaryFlow.GetLinearVelocity(units);
        }

        public double GetMassRateConcentration(UnitOfConcentration units = UnitOfConcentration.MicroMolar)
        {
            return mMassRate.GetConcentration(units);
        }

        public double GetMassRateInjectionTime(UnitOfTime units = UnitOfTime.Minutes)
        {
            return mMassRate.GetInjectionTime(units);
        }

        public double GetMassFlowRate(UnitOfMassFlowRate units = UnitOfMassFlowRate.FmolPerSec)
        {
            return mMassRate.GetMassFlowRate(units);
        }

        public double GetMassRateMolesInjected(UnitOfMolarAmount units = UnitOfMolarAmount.FemtoMoles)
        {
            return mMassRate.GetMolesInjected(units);
        }

        public double GetMassRateSampleMass()
        {
            return mMassRate.GetSampleMass();
        }

        public double GetMassRateVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            return mMassRate.GetVolFlowRate(units);
        }

        public double GetParticleDiameter(UnitOfLength units = UnitOfLength.Microns)
        {
            return mCapillaryFlow.GetParticleDiameter(units);
        }

        public double GetSolventViscosity(UnitOfViscosity units = UnitOfViscosity.Poise)
        {
            return mCapillaryFlow.GetSolventViscosity(units);
        }

        public double GetVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            return mCapillaryFlow.GetVolumetricFlowRate(units);
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
            mCapillaryFlow.SetAutoComputeEnabled(autoCompute);
        }

        /// <summary>
        /// The value to compute when mAutoCompute is true
        /// </summary>
        /// <param name="autoComputeMode"></param>
        public void SetAutoComputeMode(AutoComputeMode autoComputeMode)
        {
            mCapillaryFlow.SetAutoComputeMode(autoComputeMode);
        }

        public void SetBackPressure(double backPressure, UnitOfPressure units = UnitOfPressure.Psi)
        {
            mCapillaryFlow.SetBackPressure(backPressure, units);
        }

        public void SetCapillaryType(CapillaryType capillaryType)
        {
            mCapillaryFlow.SetCapillaryType(capillaryType);
        }

        public void SetColumnId(double columnId, UnitOfLength units = UnitOfLength.Microns)
        {
            mCapillaryFlow.SetColumnInnerDiameter(columnId, units);
        }

        public void SetColumnLength(double columnLength, UnitOfLength units = UnitOfLength.CM)
        {
            mCapillaryFlow.SetColumnLength(columnLength, units);
        }

        public void SetDeadTime(double deadTime, UnitOfTime units = UnitOfTime.Minutes)
        {
            mCapillaryFlow.SetDeadTime(deadTime, units);
        }

        public void SetExtraColumnBroadeningAdditionalVariance(double additionalVarianceInSquareSeconds)
        {
            mExtraColumnBroadening.SetAdditionalVariance(additionalVarianceInSquareSeconds);
        }

        public void SetExtraColumnBroadeningDiffusionCoefficient(double diffusionCoefficient, UnitOfDiffusionCoefficient units = UnitOfDiffusionCoefficient.CmSquaredPerSec)
        {
            mExtraColumnBroadening.SetDiffusionCoefficient(diffusionCoefficient, units);
        }

        public void SetExtraColumnBroadeningInitialPeakWidthAtBase(double width, UnitOfTime units = UnitOfTime.Seconds)
        {
            mExtraColumnBroadening.SetInitialPeakWidthAtBase(width, units);
        }

        public void SetExtraColumnBroadeningLinearVelocity(double linearVelocity, UnitOfLinearVelocity units = UnitOfLinearVelocity.MmPerMin)
        {
            mExtraColumnBroadening.SetLinearVelocity(linearVelocity, units);
        }

        public void SetExtraColumnBroadeningOpenTubeId(double openTubeId, UnitOfLength units = UnitOfLength.Microns)
        {
            mExtraColumnBroadening.SetOpenTubeInnerDiameter(openTubeId, units);
        }

        public void SetExtraColumnBroadeningOpenTubeLength(double length, UnitOfLength units = UnitOfLength.CM)
        {
            mExtraColumnBroadening.SetOpenTubeLength(length, units);
        }

        public void SetInterparticlePorosity(double porosity)
        {
            mCapillaryFlow.SetInterparticlePorosity(porosity);
        }

        public void SetMassRateConcentration(double concentration, UnitOfConcentration units = UnitOfConcentration.MicroMolar)
        {
            mMassRate.SetConcentration(concentration, units);
        }

        public void SetMassRateInjectionTime(double injectionTime, UnitOfTime units = UnitOfTime.Minutes)
        {
            mMassRate.SetInjectionTime(injectionTime, units);
        }

        public void SetMassRateSampleMass(double massInGramsPerMole)
        {
            mMassRate.SetSampleMass(massInGramsPerMole);
        }

        public void SetMassRateVolFlowRate(double volFlowRate, UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            mMassRate.SetVolumetricFlowRate(volFlowRate, units);
        }

        public void SetParticleDiameter(double particleDiameter, UnitOfLength units = UnitOfLength.Microns)
        {
            mCapillaryFlow.SetParticleDiameter(particleDiameter, units);
        }

        public void SetSolventViscosity(double solventViscosity, UnitOfViscosity units = UnitOfViscosity.Poise)
        {
            mCapillaryFlow.SetSolventViscosity(solventViscosity, units);
        }

        public void SetVolFlowRate(double volFlowRate, UnitOfFlowRate units = UnitOfFlowRate.NLPerMin)
        {
            mCapillaryFlow.SetVolumetricFlowRate(volFlowRate, units);
        }
    }
}