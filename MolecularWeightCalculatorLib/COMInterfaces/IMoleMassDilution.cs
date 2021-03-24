using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("38C1972F-7B50-4AFC-AA8A-379892CF6881"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IMoleMassDilution
    {
        /// <summary>
        /// Computes the Final Concentration, storing in .FinalConcentration, and returning it
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        double ComputeDilutionFinalConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar);

        /// <summary>
        /// Computes the Initial Concentration, storing in .InitialConcentration, and returning it
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        double ComputeDilutionInitialConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar);

        /// <summary>
        /// Computes the required dilution volumes using initial concentration, final concentration
        /// and total final volume, storing in .StockSolutionVolume and .DilutingSolventVolume,
        /// and returning .StockSolutionVolume
        /// </summary>
        /// <param name="newDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="stockSolutionUnits"></param>
        /// <param name="dilutingSolventUnits"></param>
        /// <returns></returns>
        double ComputeDilutionRequiredStockAndDilutingSolventVolumes(
            out double newDilutingSolventVolume,
            UnitOfExtendedVolume stockSolutionUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume dilutingSolventUnits = UnitOfExtendedVolume.ML);

        /// <summary>
        /// Compute the total volume following the dilution, storing in .TotalFinalVolume, and returning it
        /// </summary>
        /// <param name="newDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="totalVolumeUnits"></param>
        /// <param name="dilutingSolventUnits"></param>
        /// <returns></returns>
        double ComputeDilutionTotalVolume(
            out double newDilutingSolventVolume,
            UnitOfExtendedVolume totalVolumeUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume dilutingSolventUnits = UnitOfExtendedVolume.ML);

        /// <summary>
        /// Computes mQuantity.Amount using mQuantity.Volume and mQuantity.Concentration, storing the result in mQuantity.Amount
        /// </summary>
        /// <param name="units"></param>
        /// <returns>mQuantity.Amount, with the specified units</returns>
        double ComputeQuantityAmount(Unit units = Unit.Moles);

        /// <summary>
        /// Computes mQuantity.Concentration using mQuantity.Amount and mQuantity.Volume, storing the result in mQuantity.Concentration
        /// </summary>
        /// <param name="units"></param>
        /// <returns>mQuantity.Concentration, with the specified units</returns>
        double ComputeQuantityConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar);

        /// <summary>
        /// Computes mQuantity.Volume using mQuantity.Amount and mQuantity.Concentration, storing the result in mQuantity.Volume
        /// </summary>
        /// <param name="units"></param>
        /// <returns>mQuantity.Volume, with the specified units</returns>
        double ComputeQuantityVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.L);

        /// <summary>
        /// This function uses .SampleMass and .SampleDensity if the units are mass and/or volume-based
        /// </summary>
        /// <param name="amountIn"></param>
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        /// <returns></returns>
        double ConvertAmount(double amountIn, Unit currentUnits, Unit newUnits);

        /// <summary>
        /// Convert concentration
        /// </summary>
        /// <param name="concentrationIn"></param>
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        /// <returns></returns>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        double ConvertConcentration(double concentrationIn, UnitOfMoleMassConcentration currentUnits, UnitOfMoleMassConcentration newUnits);

        double ConvertVolumeExtended(double volume, UnitOfExtendedVolume currentUnits, UnitOfExtendedVolume newUnits);
        bool GetAutoComputeDilutionEnabled();
        AutoComputeDilutionMode GetAutoComputeDilutionMode();
        bool GetAutoComputeQuantityEnabled();
        AutoComputeQuantityMode GetAutoComputeQuantityMode();
        double GetDilutionFinalConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar);
        double GetDilutionInitialConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar);
        double GetDilutionTotalFinalVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML);
        double GetDilutionVolumeDilutingSolvent(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML);
        double GetDilutionVolumeStockSolution(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML);
        double GetQuantityAmount(Unit units = Unit.Moles);
        double GetQuantityConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar);
        double GetQuantityVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML);
        double GetSampleDensity();
        double GetSampleMass();
        void SetAutoComputeDilutionEnabled(bool autoCompute);
        void SetAutoComputeDilutionMode(AutoComputeDilutionMode autoComputeMode);
        void SetAutoComputeQuantityEnabled(bool autoCompute);
        void SetAutoComputeQuantityMode(AutoComputeQuantityMode autoComputeMode);
        void SetDilutionFinalConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar);
        void SetDilutionInitialConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar);
        void SetDilutionTotalFinalVolume(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML);
        void SetDilutionVolumeDilutingSolvent(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML);
        void SetDilutionVolumeStockSolution(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML);
        void SetQuantityAmount(double amount, Unit units = Unit.Moles);
        void SetQuantityConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar);
        void SetQuantityVolume(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML);
        void SetSampleDensity(double densityInGramsPerML);
        void SetSampleMass(double massInGramsPerMole);
        short AmountsUnitListCount { get; }
        short AmountsUnitListVolumeIndexStart { get; }
        short AmountsUnitListVolumeIndexEnd { get; }
    }
}