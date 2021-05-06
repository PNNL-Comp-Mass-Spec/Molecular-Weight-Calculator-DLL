using System.Reactive.Linq;
using MolecularWeightCalculator;
using ReactiveUI;

namespace MolecularWeightCalculatorGUI.MoleMassDilutionUI
{
    internal class MoleMassData : ReactiveObject
    {
        public MoleMassData()
        {
            UseFormulaMass = true;
            UseCustomMass = false;
            FormulaXaml = "";
            FormulaMass = 100;
            CustomMass = 100;
            SampleDensityGramsPerMl = 1;

            Amount = 1;
            AmountUnits = Unit.Moles;
            ConvertedAmountUnits = Unit.Grams;

            Volume = 100;
            VolumeUnits = UnitOfExtendedVolume.ML;
            Concentration = 10;
            ConcentrationUnits = UnitOfMoleMassConcentration.Molar;

            InitialConcentration = 10;
            InitialConcentrationUnits = UnitOfMoleMassConcentration.Molar;
            InitialVolume = 3;
            InitialVolumeUnits = UnitOfExtendedVolume.ML;
            FinalConcentration = 2;
            FinalConcentrationUnits = UnitOfMoleMassConcentration.Molar;
            AddedSolventVolume = 12;
            AddedSolventVolumeUnits = UnitOfExtendedVolume.ML;
            FinalVolume = 15;
            FinalVolumeUnits = UnitOfExtendedVolume.ML;

            massToUse = this
                .WhenAnyValue(x => x.UseFormulaMass, x => x.UseCustomMass, x => x.FormulaMass, x => x.CustomMass)
                .Select(x => x.Item1 ? x.Item3 : x.Item4).ToProperty(this, x => x.MassToUse);

            densityRequired = this.WhenAnyValue(x => x.AmountUnits, x => x.ConvertedAmountUnits)
                .Select(x => Unit.Liters <= x.Item1 && x.Item1 <= Unit.Pints || Unit.Liters <= x.Item2 && x.Item2 <= Unit.Pints)
                .ToProperty(this, x => x.DensityRequired);
        }

        private bool useFormulaMass;
        private bool useCustomMass;
        private string formulaXaml;
        private double formulaMass;
        private double customMass;
        private double sampleDensityGramsPerMl;
        private readonly ObservableAsPropertyHelper<double> massToUse;
        private readonly ObservableAsPropertyHelper<bool> densityRequired;
        private double amount;
        private Unit amountUnits;
        private Unit convertedAmountUnits;
        private double convertedAmount;
        private double volume;
        private UnitOfExtendedVolume volumeUnits;
        private double concentration;
        private UnitOfMoleMassConcentration concentrationUnits;
        private double initialConcentration;
        private UnitOfMoleMassConcentration initialConcentrationUnits;
        private double initialVolume;
        private UnitOfExtendedVolume initialVolumeUnits;
        private double finalConcentration;
        private UnitOfMoleMassConcentration finalConcentrationUnits;
        private double addedSolventVolume;
        private UnitOfExtendedVolume addedSolventVolumeUnits;
        private double finalVolume;
        private UnitOfExtendedVolume finalVolumeUnits;

        public bool UseFormulaMass
        {
            get => useFormulaMass;
            set => this.RaiseAndSetIfChanged(ref useFormulaMass, value);
        }

        public bool UseCustomMass
        {
            get => useCustomMass;
            set => this.RaiseAndSetIfChanged(ref useCustomMass, value);
        }

        public string FormulaXaml
        {
            get => formulaXaml;
            set => this.RaiseAndSetIfChanged(ref formulaXaml, value);
        }

        public double FormulaMass
        {
            get => formulaMass;
            set => this.RaiseAndSetIfChanged(ref formulaMass, value);
        }

        public double CustomMass
        {
            get => customMass;
            set => this.RaiseAndSetIfChanged(ref customMass, value);
        }

        public double SampleDensityGramsPerMl
        {
            get => sampleDensityGramsPerMl;
            set => this.RaiseAndSetIfChanged(ref sampleDensityGramsPerMl, value);
        }

        public double MassToUse => massToUse.Value;
        public bool DensityRequired => densityRequired.Value;

        public double Amount
        {
            get => amount;
            set => this.RaiseAndSetIfChanged(ref amount, value);
        }

        public Unit AmountUnits
        {
            get => amountUnits;
            set => this.RaiseAndSetIfChanged(ref amountUnits, value);
        }

        public Unit ConvertedAmountUnits
        {
            get => convertedAmountUnits;
            set => this.RaiseAndSetIfChanged(ref convertedAmountUnits, value);
        }

        public double ConvertedAmount
        {
            get => convertedAmount;
            set => this.RaiseAndSetIfChanged(ref convertedAmount, value);
        }

        public double Volume
        {
            get => volume;
            set => this.RaiseAndSetIfChanged(ref volume, value);
        }

        public UnitOfExtendedVolume VolumeUnits
        {
            get => volumeUnits;
            set => this.RaiseAndSetIfChanged(ref volumeUnits, value);
        }

        public double Concentration
        {
            get => concentration;
            set => this.RaiseAndSetIfChanged(ref concentration, value);
        }

        public UnitOfMoleMassConcentration ConcentrationUnits
        {
            get => concentrationUnits;
            set => this.RaiseAndSetIfChanged(ref concentrationUnits, value);
        }

        public double InitialConcentration
        {
            get => initialConcentration;
            set => this.RaiseAndSetIfChanged(ref initialConcentration, value);
        }

        public UnitOfMoleMassConcentration InitialConcentrationUnits
        {
            get => initialConcentrationUnits;
            set => this.RaiseAndSetIfChanged(ref initialConcentrationUnits, value);
        }

        public double InitialVolume
        {
            get => initialVolume;
            set => this.RaiseAndSetIfChanged(ref initialVolume, value);
        }

        public UnitOfExtendedVolume InitialVolumeUnits
        {
            get => initialVolumeUnits;
            set => this.RaiseAndSetIfChanged(ref initialVolumeUnits, value);
        }

        public double FinalConcentration
        {
            get => finalConcentration;
            set => this.RaiseAndSetIfChanged(ref finalConcentration, value);
        }

        public UnitOfMoleMassConcentration FinalConcentrationUnits
        {
            get => finalConcentrationUnits;
            set => this.RaiseAndSetIfChanged(ref finalConcentrationUnits, value);
        }

        public double AddedSolventVolume
        {
            get => addedSolventVolume;
            set => this.RaiseAndSetIfChanged(ref addedSolventVolume, value);
        }

        public UnitOfExtendedVolume AddedSolventVolumeUnits
        {
            get => addedSolventVolumeUnits;
            set => this.RaiseAndSetIfChanged(ref addedSolventVolumeUnits, value);
        }

        public double FinalVolume
        {
            get => finalVolume;
            set => this.RaiseAndSetIfChanged(ref finalVolume, value);
        }

        public UnitOfExtendedVolume FinalVolumeUnits
        {
            get => finalVolumeUnits;
            set => this.RaiseAndSetIfChanged(ref finalVolumeUnits, value);
        }
    }
}
