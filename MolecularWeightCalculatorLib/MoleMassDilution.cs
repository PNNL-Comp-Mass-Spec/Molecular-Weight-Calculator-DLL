using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;
using MolecularWeightCalculator.MoleMassDilutionTools;

namespace MolecularWeightCalculator
{
    /// <summary>
    /// Compatibility/COM wrapper for MoleMassDilutionTools classes
    /// </summary>
    [Guid("FD3FADF4-C3E4-4C4C-AF7A-AF7018FC3DBB"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class MoleMassDilution : IMoleMassDilution
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: MoleMassDilution

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2002
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
        public MoleMassDilution()
        {
        }

        private readonly MoleMassQuantity mQuantity = new();
        private readonly MoleMassDilutionTools.MoleMassDilution mDilution = new();

        /// <summary>
        /// Computes the Final Concentration, storing in .FinalConcentration, and returning it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeDilutionFinalConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return mDilution.ComputeFinalConcentration(units);
        }

        /// <summary>
        /// Computes the Initial Concentration, storing in .InitialConcentration, and returning it
        /// </summary>
        /// <param name="units"></param>
        public double ComputeDilutionInitialConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return mDilution.ComputeInitialConcentration(units);
        }

        /// <summary>
        /// Computes the required dilution volumes using initial concentration, final concentration
        /// and total final volume, storing in .StockSolutionVolume and .DilutingSolventVolume,
        /// and returning .StockSolutionVolume
        /// </summary>
        /// <param name="newDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="stockSolutionUnits"></param>
        /// <param name="dilutingSolventUnits"></param>
        public double ComputeDilutionRequiredStockAndDilutingSolventVolumes(
            out double newDilutingSolventVolume,
            UnitOfExtendedVolume stockSolutionUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume dilutingSolventUnits = UnitOfExtendedVolume.ML)
        {
            return mDilution.ComputeRequiredStockAndDilutingSolventVolumes(out newDilutingSolventVolume, stockSolutionUnits, dilutingSolventUnits);
        }

        /// <summary>
        /// Compute the total volume following the dilution, storing in .TotalFinalVolume, and returning it
        /// </summary>
        /// <param name="newDilutingSolventVolume">Output: diluting solvent volume</param>
        /// <param name="totalVolumeUnits"></param>
        /// <param name="dilutingSolventUnits"></param>
        /// <remarks>
        /// If mDilutionValues.InitialConcentration is less than mDilutionValues.FinalConcentration, this represents evaporation or sublimation
        /// In this case, the computed value for mDilutionValues.TotalFinalVolume represents the volume that the solution must be reduced to in order to obtain the final concentration
        /// The computed value for newDilutingSolventVolume will be -1 when initialConcentration is less than finalConcentration
        /// </remarks>
        public double ComputeDilutionTotalVolume(
            out double newDilutingSolventVolume,
            UnitOfExtendedVolume totalVolumeUnits = UnitOfExtendedVolume.ML,
            UnitOfExtendedVolume dilutingSolventUnits = UnitOfExtendedVolume.ML)
        {
            return mDilution.ComputeTotalVolume(out newDilutingSolventVolume, totalVolumeUnits, dilutingSolventUnits);
        }

        /// <summary>
        /// Computes mQuantity.Amount using mQuantity.Volume and mQuantity.Concentration, storing the result in mQuantity.Amount
        /// </summary>
        /// <param name="units"></param>
        /// <returns>mQuantity.Amount, with the specified units</returns>
        public double ComputeQuantityAmount(Unit units = Unit.Moles)
        {
            return mQuantity.ComputeAmount(units);
        }

        /// <summary>
        /// Computes mQuantity.Concentration using mQuantity.Amount and mQuantity.Volume, storing the result in mQuantity.Concentration
        /// </summary>
        /// <param name="units"></param>
        /// <returns>mQuantity.Concentration, with the specified units</returns>
        public double ComputeQuantityConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return mQuantity.ComputeConcentration(units);
        }

        /// <summary>
        /// Computes mQuantity.Volume using mQuantity.Amount and mQuantity.Concentration, storing the result in mQuantity.Volume
        /// </summary>
        /// <param name="units"></param>
        /// <returns>mQuantity.Volume, with the specified units</returns>
        public double ComputeQuantityVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.L)
        {
            return mQuantity.ComputeVolume(units);
        }

        /// <summary>
        /// This function uses .SampleMass and .SampleDensity if the units are mass and/or volume-based
        /// </summary>
        /// <param name="amountIn"></param>
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        public double ConvertAmount(double amountIn, Unit currentUnits, Unit newUnits)
        {
            return UnitConversions.ConvertAmount(amountIn, currentUnits, newUnits, mQuantity.GetSampleMass(), mQuantity.GetSampleDensity());
        }

        /// <summary>
        /// Convert concentration
        /// </summary>
        /// <param name="concentrationIn"></param>
        /// <param name="currentUnits"></param>
        /// <param name="newUnits"></param>
        /// <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
        public double ConvertConcentration(double concentrationIn, UnitOfMoleMassConcentration currentUnits, UnitOfMoleMassConcentration newUnits)
        {
            return UnitConversions.ConvertConcentration(concentrationIn, currentUnits, newUnits, mQuantity.GetSampleMass());
        }

        public double ConvertVolumeExtended(double volume, UnitOfExtendedVolume currentUnits, UnitOfExtendedVolume newUnits)
        {
            return UnitConversions.ConvertVolumeExtended(volume, currentUnits, newUnits);
        }

        // Get Methods
        // These retrieve the most recently computed value
        // If mAutoComputeDilution = False, must manually call a Compute Sub to recompute the value
        // Similarly, if mAutoComputeQuantity = False, must manually call a Compute Sub to recompute the value

        public bool GetAutoComputeDilutionEnabled()
        {
            return mDilution.GetAutoComputeEnabled();
        }

        public AutoComputeDilutionMode GetAutoComputeDilutionMode()
        {
            return mDilution.GetAutoComputeMode();
        }

        public bool GetAutoComputeQuantityEnabled()
        {
            return mQuantity.GetAutoComputeEnabled();
        }

        public AutoComputeQuantityMode GetAutoComputeQuantityMode()
        {
            return mQuantity.GetAutoComputeMode();
        }

        public double GetDilutionFinalConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return mDilution.GetFinalConcentration(units);
        }

        public double GetDilutionInitialConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return mDilution.GetInitialConcentration(units);
        }

        public double GetDilutionTotalFinalVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return mDilution.GetTotalFinalVolume(units);
        }

        public double GetDilutionVolumeDilutingSolvent(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return mDilution.GetVolumeDilutingSolvent(units);
        }

        public double GetDilutionVolumeStockSolution(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return mDilution.GetVolumeStockSolution(units);
        }

        public double GetQuantityAmount(Unit units = Unit.Moles)
        {
            return mQuantity.GetAmount(units);
        }

        public double GetQuantityConcentration(UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            return mQuantity.GetConcentration(units);
        }

        public double GetQuantityVolume(UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            return mQuantity.GetVolume(units);
        }

        public double GetSampleDensity()
        {
            return mQuantity.GetSampleDensity();
        }

        public double GetSampleMass()
        {
            return mQuantity.GetSampleMass();
        }

        /// <summary>
        /// Updates the auto-compute mode for dilution-related values
        /// </summary>
        /// <param name="autoCompute"></param>
        /// <remarks>
        /// When true, dilution-related values will be auto-computed based on mAutoComputeDilutionMode
        /// When false, you must manually call a Compute method to re-compute the value
        /// </remarks>
        public void SetAutoComputeDilutionEnabled(bool autoCompute)
        {
            mDilution.SetAutoComputeEnabled(autoCompute);
        }

        /// <summary>
        /// Auto-compute mode for dilution-related values
        /// </summary>
        /// <param name="autoComputeMode"></param>
        public void SetAutoComputeDilutionMode(AutoComputeDilutionMode autoComputeMode)
        {
            mDilution.SetAutoComputeMode(autoComputeMode);
        }

        /// <summary>
        /// Updates the auto-compute mode for quantity-related values
        /// </summary>
        /// <param name="autoCompute"></param>
        /// <remarks>
        /// When true, quantity-related values will be auto-computed based on mAutoComputeQuantityMode
        /// When false, you must manually call a Compute method to re-compute the value
        /// </remarks>
        public void SetAutoComputeQuantityEnabled(bool autoCompute)
        {
            mQuantity.SetAutoComputeEnabled(autoCompute);
        }

        /// <summary>
        /// Auto-compute mode for quantity-related values
        /// </summary>
        /// <param name="autoComputeMode"></param>
        public void SetAutoComputeQuantityMode(AutoComputeQuantityMode autoComputeMode)
        {
            mQuantity.SetAutoComputeMode(autoComputeMode);
        }

        public void SetDilutionFinalConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            mDilution.SetFinalConcentration(concentration, units);
        }

        public void SetDilutionInitialConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            mDilution.SetInitialConcentration(concentration, units);
        }

        public void SetDilutionTotalFinalVolume(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mDilution.SetTotalFinalVolume(volume, units);
        }

        public void SetDilutionVolumeDilutingSolvent(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mDilution.SetVolumeDilutingSolvent(volume, units);
        }

        public void SetDilutionVolumeStockSolution(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mDilution.SetVolumeStockSolution(volume, units);
        }

        public void SetQuantityAmount(double amount, Unit units = Unit.Moles)
        {
            mQuantity.SetAmount(amount, units);
        }

        public void SetQuantityConcentration(double concentration, UnitOfMoleMassConcentration units = UnitOfMoleMassConcentration.Molar)
        {
            mQuantity.SetConcentration(concentration, units);
        }

        public void SetQuantityVolume(double volume, UnitOfExtendedVolume units = UnitOfExtendedVolume.ML)
        {
            mQuantity.SetVolume(volume, units);
        }

        // ReSharper disable once InconsistentNaming
        public void SetSampleDensity(double densityInGramsPerML)
        {
            mQuantity.SetSampleDensity(densityInGramsPerML);
        }

        public void SetSampleMass(double massInGrams)
        {
            mQuantity.SetSampleMass(massInGrams);
            mDilution.SetSampleMass(massInGrams);
        }

        public short AmountsUnitListCount => UnitConversions.AmountsUnitListCount;

        public short AmountsUnitListVolumeIndexStart => UnitConversions.AmountsUnitListVolumeIndexStart;

        public short AmountsUnitListVolumeIndexEnd => UnitConversions.AmountsUnitListVolumeIndexEnd;
    }
}