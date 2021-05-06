using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using MolecularWeightCalculator;
using MolecularWeightCalculator.MoleMassDilutionTools;
using ReactiveUI;
using MoleMassDilution = MolecularWeightCalculator.MoleMassDilutionTools.MoleMassDilution;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.MoleMassDilutionUI
{
    internal class MoleMassDilutionViewModel : ReactiveObject
    {
        // TODO: Requires a reference to a mass/molecular weight (may need to re-design the UI, since other conversions do not use the mass: False (find concentration may use it, same with dilution))

        public MoleMassDilutionViewModel()
        {
            Data = new MoleMassData();

            AmountUnitOptions = Enum.GetValues(typeof(Unit)).Cast<Unit>().ToList();
            VolumeUnitOptions = Enum.GetValues(typeof(UnitOfExtendedVolume)).Cast<UnitOfExtendedVolume>().ToList();
            ConcentrationUnitOptions = Enum.GetValues(typeof(UnitOfMoleMassConcentration)).Cast<UnitOfMoleMassConcentration>().ToList();

            Data.WhenAnyValue(x => x.AmountUnits).Subscribe(x => Data.Amount = mmQuantity.GetAmount(x));
            Data.WhenAnyValue(x => x.VolumeUnits).Subscribe(x => Data.Volume = mmQuantity.GetVolume(x));
            Data.WhenAnyValue(x => x.ConcentrationUnits).Subscribe(x => Data.Concentration = mmQuantity.GetConcentration(x));

            // Convert Amount
            Data.WhenAnyValue(x => x.Amount, x => x.AmountUnits, x => x.ConvertedAmountUnits, x => x.MassToUse, x => x.SampleDensityGramsPerMl)
                .Subscribe(x => ConvertAmount());

            // Find Concentration
            Data.WhenAnyValue(x => x.Amount, x => x.AmountUnits).Subscribe(x => mmQuantity.SetAmount(x.Item1, x.Item2));
            Data.WhenAnyValue(x => x.Volume, x => x.VolumeUnits).Subscribe(x => mmQuantity.SetVolume(x.Item1, x.Item2));
            Data.WhenAnyValue(x => x.Concentration, x => x.ConcentrationUnits).Subscribe(x => mmQuantity.SetConcentration(x.Item1, x.Item2));

            FindAmountCommand = ReactiveCommand.Create(FindQuantityAmount);
            FindVolumeCommand = ReactiveCommand.Create(FindQuantityVolume);
            FindConcentrationCommand = ReactiveCommand.Create(FindQuantityConcentration);

            // Dilution Calculations
            DilutionFindModeOptions = Enum.GetValues(typeof(AutoComputeDilutionMode)).Cast<AutoComputeDilutionMode>().ToList();
            VolumeUnitOptions = Enum.GetValues(typeof(UnitOfExtendedVolume)).Cast<UnitOfExtendedVolume>().ToList();
            ConcentrationUnitOptions = Enum.GetValues(typeof(UnitOfMoleMassConcentration)).Cast<UnitOfMoleMassConcentration>().ToList();

            Data.WhenAnyValue(x => x.MassToUse).Subscribe(x =>
            {
                mmQuantity.SetSampleMass(x);
                mmDilution.SetSampleMass(x);
            });

            Data.WhenAnyValue(x => x.SampleDensityGramsPerMl).Subscribe(x => mmQuantity.SetSampleDensity(x));

            LinkConcentrations = false;
            LinkDilutionVolumeUnits = true;
            DilutionFindMode = AutoComputeDilutionMode.FindRequiredDilutionVolumes;

            this.WhenAnyValue(x => x.LinkConcentrations).Where(x => x).Subscribe(x =>
            {
                Data.InitialConcentration = Data.Concentration;
                Data.InitialConcentrationUnits = Data.ConcentrationUnits;
            });
            Data.WhenAnyValue(x => x.Concentration).Subscribe(x => Data.InitialConcentration = x);
            Data.WhenAnyValue(x => x.InitialConcentration).Subscribe(x => Data.Concentration = x);
            Data.WhenAnyValue(x => x.ConcentrationUnits).Subscribe(x => Data.InitialConcentrationUnits = x);
            Data.WhenAnyValue(x => x.InitialConcentrationUnits).Subscribe(x => Data.ConcentrationUnits = x);

            this.WhenAnyValue(x => x.LinkDilutionVolumeUnits).Where(x => x).Subscribe(x => UpdateLinkedVolumes(Data.InitialVolumeUnits));
            Data.WhenAnyValue(x => x.InitialVolumeUnits).Subscribe(UpdateLinkedVolumes);
            Data.WhenAnyValue(x => x.AddedSolventVolumeUnits).Subscribe(UpdateLinkedVolumes);
            Data.WhenAnyValue(x => x.FinalVolumeUnits).Subscribe(UpdateLinkedVolumes);

            initialConcentrationReadOnly = this.WhenAnyValue(x => x.DilutionFindMode).Select(x => x == AutoComputeDilutionMode.FindInitialConcentration).ToProperty(this, x => x.InitialConcentrationReadOnly);
            initialVolumeReadOnly = this.WhenAnyValue(x => x.DilutionFindMode).Select(x => x == AutoComputeDilutionMode.FindRequiredDilutionVolumes).ToProperty(this, x => x.InitialVolumeReadOnly);
            finalConcentrationReadOnly = this.WhenAnyValue(x => x.DilutionFindMode).Select(x => x == AutoComputeDilutionMode.FindFinalConcentration).ToProperty(this, x => x.FinalConcentrationReadOnly);
            addedSolventVolumeReadOnly = this.WhenAnyValue(x => x.DilutionFindMode).Select(x => x == AutoComputeDilutionMode.FindRequiredDilutionVolumes || x == AutoComputeDilutionMode.FindRequiredTotalVolume).ToProperty(this, x => x.AddedSolventVolumeReadOnly);
            finalVolumeReadOnly = this.WhenAnyValue(x => x.DilutionFindMode).Select(x => x == AutoComputeDilutionMode.FindRequiredTotalVolume).ToProperty(this, x => x.FinalVolumeReadOnly);

            // Re-calculate whenever any units are updated
            Data.WhenAnyValue(x => x.InitialConcentrationUnits, x => x.InitialVolumeUnits,
                    x => x.FinalConcentrationUnits, x => x.AddedSolventVolumeUnits, x => x.FinalVolumeUnits)
                .Subscribe(x => Calculate());

            // Otherwise, update only when an editable property is updated.
            Data.WhenAnyValue(x => x.InitialConcentration, x => x.FinalConcentration, x => x.FinalVolume)
                .Where(x => DilutionFindMode == AutoComputeDilutionMode.FindRequiredDilutionVolumes).Subscribe(x => Calculate());
            Data.WhenAnyValue(x => x.InitialConcentration, x => x.InitialVolume, x => x.FinalConcentration)
                .Where(x => DilutionFindMode == AutoComputeDilutionMode.FindRequiredTotalVolume).Subscribe(x => Calculate());
            Data.WhenAnyValue(x => x.InitialConcentration, x => x.InitialVolume, x => x.AddedSolventVolume, x => x.FinalVolume)
                .Where(x => DilutionFindMode == AutoComputeDilutionMode.FindFinalConcentration).Subscribe(x => Calculate());
            Data.WhenAnyValue(x => x.InitialVolume, x => x.FinalConcentration, x => x.AddedSolventVolume, x => x.FinalVolume)
                .Where(x => DilutionFindMode == AutoComputeDilutionMode.FindInitialConcentration).Subscribe(x => Calculate());

            Calculate();
        }

        private readonly MoleMassQuantity mmQuantity = new MoleMassQuantity();
        private readonly MoleMassDilution mmDilution = new MoleMassDilution();
        private bool linkConcentrations;
        private bool linkDilutionVolumeUnits;
        private AutoComputeDilutionMode dilutionFindMode;
        private readonly ObservableAsPropertyHelper<bool> initialConcentrationReadOnly;
        private readonly ObservableAsPropertyHelper<bool> initialVolumeReadOnly;
        private readonly ObservableAsPropertyHelper<bool> finalConcentrationReadOnly;
        private readonly ObservableAsPropertyHelper<bool> addedSolventVolumeReadOnly;
        private readonly ObservableAsPropertyHelper<bool> finalVolumeReadOnly;

        public MoleMassData Data { get; }

        public IReadOnlyList<Unit> AmountUnitOptions { get; }
        public IReadOnlyList<UnitOfExtendedVolume> VolumeUnitOptions { get; }
        public IReadOnlyList<UnitOfMoleMassConcentration> ConcentrationUnitOptions { get; }
        public IReadOnlyList<AutoComputeDilutionMode> DilutionFindModeOptions { get; }

        public ReactiveCommand<RxUnit, RxUnit> FindAmountCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> FindVolumeCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> FindConcentrationCommand { get; }

        public bool LinkConcentrations
        {
            get => linkConcentrations;
            set => this.RaiseAndSetIfChanged(ref linkConcentrations, value);
        }

        public bool LinkDilutionVolumeUnits
        {
            get => linkDilutionVolumeUnits;
            set => this.RaiseAndSetIfChanged(ref linkDilutionVolumeUnits, value);
        }

        public AutoComputeDilutionMode DilutionFindMode
        {
            get => dilutionFindMode;
            set => this.RaiseAndSetIfChanged(ref dilutionFindMode, value);
        }
        public bool InitialConcentrationReadOnly => initialConcentrationReadOnly.Value;
        public bool InitialVolumeReadOnly => initialVolumeReadOnly.Value;
        public bool FinalConcentrationReadOnly => finalConcentrationReadOnly.Value;
        public bool AddedSolventVolumeReadOnly => addedSolventVolumeReadOnly.Value;
        public bool FinalVolumeReadOnly => finalVolumeReadOnly.Value;

        public void ConvertAmount()
        {
            Data.ConvertedAmount = UnitConversions.ConvertAmount(Data.Amount, Data.AmountUnits, Data.ConvertedAmountUnits, Data.MassToUse, Data.SampleDensityGramsPerMl);
        }

        public void FindQuantityAmount()
        {
            Data.Amount = mmQuantity.ComputeAmount(Data.AmountUnits);
        }

        public void FindQuantityVolume()
        {
            Data.Volume = mmQuantity.ComputeVolume(Data.VolumeUnits);
        }

        public void FindQuantityConcentration()
        {
            Data.Concentration = mmQuantity.ComputeConcentration(Data.ConcentrationUnits);
        }

        private void UpdateLinkedVolumes(UnitOfExtendedVolume newUnit)
        {
            if (!LinkDilutionVolumeUnits)
            {
                return;
            }

            Data.InitialVolumeUnits = newUnit;
            Data.AddedSolventVolumeUnits = newUnit;
            Data.FinalVolumeUnits = newUnit;
        }

        private void Calculate()
        {
            switch (DilutionFindMode)
            {
                case AutoComputeDilutionMode.FindRequiredDilutionVolumes:
                    mmDilution.SetInitialConcentration(Data.InitialConcentration, Data.InitialConcentrationUnits);
                    mmDilution.SetFinalConcentration(Data.FinalConcentration, Data.FinalConcentrationUnits);
                    mmDilution.SetTotalFinalVolume(Data.FinalVolume, Data.FinalVolumeUnits);
                    Data.InitialVolume = mmDilution.ComputeRequiredStockAndDilutingSolventVolumes(out var addedSolventVolume, Data.InitialVolumeUnits, Data.AddedSolventVolumeUnits);
                    Data.AddedSolventVolume = addedSolventVolume;
                    break;
                case AutoComputeDilutionMode.FindRequiredTotalVolume:
                    mmDilution.SetInitialConcentration(Data.InitialConcentration, Data.InitialConcentrationUnits);
                    mmDilution.SetVolumeStockSolution(Data.InitialVolume, Data.InitialVolumeUnits);
                    mmDilution.SetFinalConcentration(Data.FinalConcentration, Data.FinalConcentrationUnits);
                    Data.FinalVolume = mmDilution.ComputeTotalVolume(out addedSolventVolume, Data.FinalVolumeUnits, Data.AddedSolventVolumeUnits);
                    Data.AddedSolventVolume = addedSolventVolume;
                    break;
                case AutoComputeDilutionMode.FindFinalConcentration:
                    mmDilution.SetInitialConcentration(Data.InitialConcentration, Data.InitialConcentrationUnits);
                    mmDilution.SetVolumeStockSolution(Data.InitialVolume, Data.InitialVolumeUnits);
                    mmDilution.SetVolumeDilutingSolvent(Data.AddedSolventVolume, Data.AddedSolventVolumeUnits);
                    mmDilution.SetTotalFinalVolume(Data.FinalVolume, Data.FinalVolumeUnits);
                    Data.FinalConcentration = mmDilution.ComputeFinalConcentration(Data.FinalConcentrationUnits);
                    break;
                case AutoComputeDilutionMode.FindInitialConcentration:
                    mmDilution.SetVolumeStockSolution(Data.InitialVolume, Data.InitialVolumeUnits);
                    mmDilution.SetFinalConcentration(Data.FinalConcentration, Data.FinalConcentrationUnits);
                    mmDilution.SetVolumeDilutingSolvent(Data.AddedSolventVolume, Data.AddedSolventVolumeUnits);
                    mmDilution.SetTotalFinalVolume(Data.FinalVolume, Data.FinalVolumeUnits);
                    Data.InitialConcentration = mmDilution.ComputeInitialConcentration(Data.InitialConcentrationUnits);
                    break;
            }
        }
    }
}
