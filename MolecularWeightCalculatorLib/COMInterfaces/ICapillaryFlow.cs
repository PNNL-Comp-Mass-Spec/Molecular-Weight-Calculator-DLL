using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("F3F1CEDC-83C1-428A-A8A4-10EA3391D228"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface ICapillaryFlow
    {
        /// <summary>
        /// Computes the back pressure, stores in .BackPressure, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        double ComputeBackPressure(UnitOfPressure units = UnitOfPressure.Psi);

        /// <summary>
        /// Computes the column length, stores in .ColumnLength, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        double ComputeColumnLength(UnitOfLength units = UnitOfLength.CM);

        double ComputeColumnVolume(UnitOfVolume units = 0);

        /// <summary>
        /// Computes the column length, stores in .ColumnLength, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        double ComputeColumnId(UnitOfLength units = UnitOfLength.Microns);

        /// <summary>
        /// Computes the column dead time, stores in .ColumnDeadTime, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <param name="recalculateVolFlowRate"></param>
        /// <returns></returns>
        double ComputeDeadTime(UnitOfTime units = UnitOfTime.Minutes, bool recalculateVolFlowRate = true);

        double ComputeExtraColumnBroadeningResultantPeakWidth(UnitOfTime units = UnitOfTime.Seconds);

        /// <summary>
        /// Computes the Linear velocity, stores in .LinearVelocity, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <param name="recalculateVolFlowRate"></param>
        /// <returns></returns>
        double ComputeLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec, bool recalculateVolFlowRate = true);

        /// <summary>
        /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        double ComputeMassFlowRate(UnitOfMassFlowRate units = UnitOfMassFlowRate.FmolPerSec);

        /// <summary>
        /// Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        double ComputeMassRateMolesInjected(UnitOfMolarAmount units = UnitOfMolarAmount.FemtoMoles);

        /// <summary>
        /// Computes the optimum linear velocity, based on
        /// mCapillaryFlowParameters.ParticleDiameter
        /// and mExtraColumnBroadeningParameters.DiffusionCoefficient
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        double ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec);

        double ComputeMeCNViscosity(double percentAcetonitrile, double temperature, UnitOfTemperature temperatureUnits = UnitOfTemperature.Celsius, UnitOfViscosity viscosityUnits = UnitOfViscosity.Poise);

        /// <summary>
        /// Computes the Volumetric flow rate, stores in .VolumetricFlowRate, and returns it
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        double ComputeVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin);

        /// <summary>
        /// Computes the Volumetric flow rate using the dead time, stores in .VolumetricFlowRate, and returns it
        /// This requires modifying the pressure value to give the computed volumetric flow rate
        /// </summary>
        /// <param name="newBackPressure">Output: new back pressure</param>
        /// <param name="units"></param>
        /// <param name="pressureUnits"></param>
        /// <returns></returns>
        double ComputeVolFlowRateUsingDeadTime(
            out double newBackPressure,
            UnitOfFlowRate units = UnitOfFlowRate.NLPerMin,
            UnitOfPressure pressureUnits = UnitOfPressure.Psi);

        /// <summary>
        /// Convert concentration
        /// </summary>
        /// <param name="concentrationIn"></param>
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        /// <returns></returns>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        double ConvertConcentration(double concentrationIn, UnitOfConcentration currentUnits, UnitOfConcentration newUnits);

        double ConvertDiffusionCoefficient(double diffusionCoefficientIn, UnitOfDiffusionCoefficient currentUnits, UnitOfDiffusionCoefficient newUnits);
        double ConvertLength(double lengthIn, UnitOfLength currentUnits, UnitOfLength newUnits);
        double ConvertLinearVelocity(double linearVelocityIn, UnitOfLinearVelocity currentUnits, UnitOfLinearVelocity newUnits);
        double ConvertMassFlowRate(double massFlowRateIn, UnitOfMassFlowRate currentUnits, UnitOfMassFlowRate newUnits);
        double ConvertMoles(double molesIn, UnitOfMolarAmount currentUnits, UnitOfMolarAmount newUnits);
        double ConvertPressure(double pressureIn, UnitOfPressure currentUnits, UnitOfPressure newUnits);
        double ConvertTemperature(double temperatureIn, UnitOfTemperature currentUnits, UnitOfTemperature newUnits);
        double ConvertTime(double timeIn, UnitOfTime currentUnits, UnitOfTime newUnits);
        double ConvertViscosity(double viscosityIn, UnitOfViscosity currentUnits, UnitOfViscosity newUnits);
        double ConvertVolFlowRate(double volFlowRateIn, UnitOfFlowRate currentUnits, UnitOfFlowRate newUnits);
        double ConvertVolume(double volume, UnitOfVolume currentUnits, UnitOfVolume newUnits);
        bool GetAutoComputeEnabled();
        AutoComputeMode GetAutoComputeMode();
        double GetBackPressure(UnitOfPressure units = UnitOfPressure.Psi);
        CapillaryType GetCapillaryType();
        double GetColumnId(UnitOfLength units = UnitOfLength.Microns);
        double GetColumnLength(UnitOfLength units = UnitOfLength.CM);
        double GetColumnVolume(UnitOfVolume units = UnitOfVolume.UL);
        double GetDeadTime(UnitOfTime units = UnitOfTime.Minutes);
        double GetExtraColumnBroadeningAdditionalVarianceInSquareSeconds();
        double GetExtraColumnBroadeningDiffusionCoefficient(UnitOfDiffusionCoefficient units = UnitOfDiffusionCoefficient.CmSquaredPerSec);
        double GetExtraColumnBroadeningInitialPeakWidthAtBase(UnitOfTime units = UnitOfTime.Seconds);
        double GetExtraColumnBroadeningLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.MmPerMin);
        double GetExtraColumnBroadeningOpenTubeId(UnitOfLength units = UnitOfLength.Microns);
        double GetExtraColumnBroadeningOpenTubeLength(UnitOfLength units = UnitOfLength.CM);
        double GetExtraColumnBroadeningResultantPeakWidth(UnitOfTime units = UnitOfTime.Seconds);
        double GetExtraColumnBroadeningTemporalVarianceInSquareSeconds();
        double GetInterparticlePorosity();
        double GetLinearVelocity(UnitOfLinearVelocity units = UnitOfLinearVelocity.CmPerSec);
        double GetMassRateConcentration(UnitOfConcentration units = UnitOfConcentration.MicroMolar);
        double GetMassRateInjectionTime(UnitOfTime units = UnitOfTime.Minutes);
        double GetMassFlowRate(UnitOfMassFlowRate units = UnitOfMassFlowRate.FmolPerSec);
        double GetMassRateMolesInjected(UnitOfMolarAmount units = UnitOfMolarAmount.FemtoMoles);
        double GetMassRateSampleMass();
        double GetMassRateVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin);
        double GetParticleDiameter(UnitOfLength units = UnitOfLength.Microns);
        double GetSolventViscosity(UnitOfViscosity units = UnitOfViscosity.Poise);
        double GetVolFlowRate(UnitOfFlowRate units = UnitOfFlowRate.NLPerMin);
        void SetAutoComputeEnabled(bool autoCompute);
        void SetAutoComputeMode(AutoComputeMode autoComputeMode);
        void SetBackPressure(double backPressure, UnitOfPressure units = UnitOfPressure.Psi);
        void SetCapillaryType(CapillaryType capillaryType);
        void SetColumnId(double columnId, UnitOfLength units = UnitOfLength.Microns);
        void SetColumnLength(double columnLength, UnitOfLength units = UnitOfLength.CM);
        void SetDeadTime(double deadTime, UnitOfTime units = UnitOfTime.Minutes);
        void SetExtraColumnBroadeningAdditionalVariance(double additionalVarianceInSquareSeconds);
        void SetExtraColumnBroadeningDiffusionCoefficient(double diffusionCoefficient, UnitOfDiffusionCoefficient units = UnitOfDiffusionCoefficient.CmSquaredPerSec);
        void SetExtraColumnBroadeningInitialPeakWidthAtBase(double width, UnitOfTime units = UnitOfTime.Seconds);
        void SetExtraColumnBroadeningLinearVelocity(double linearVelocity, UnitOfLinearVelocity units = UnitOfLinearVelocity.MmPerMin);
        void SetExtraColumnBroadeningOpenTubeId(double openTubeId, UnitOfLength units = UnitOfLength.Microns);
        void SetExtraColumnBroadeningOpenTubeLength(double length, UnitOfLength units = UnitOfLength.CM);
        void SetInterparticlePorosity(double porosity);
        void SetMassRateConcentration(double concentration, UnitOfConcentration units = UnitOfConcentration.MicroMolar);
        void SetMassRateInjectionTime(double injectionTime, UnitOfTime units = UnitOfTime.Minutes);
        void SetMassRateSampleMass(double massInGramsPerMole);
        void SetMassRateVolFlowRate(double volFlowRate, UnitOfFlowRate units = UnitOfFlowRate.NLPerMin);
        void SetParticleDiameter(double particleDiameter, UnitOfLength units = UnitOfLength.Microns);
        void SetSolventViscosity(double solventViscosity, UnitOfViscosity units = UnitOfViscosity.Poise);
        void SetVolFlowRate(double volFlowRate, UnitOfFlowRate units = UnitOfFlowRate.NLPerMin);
    }
}