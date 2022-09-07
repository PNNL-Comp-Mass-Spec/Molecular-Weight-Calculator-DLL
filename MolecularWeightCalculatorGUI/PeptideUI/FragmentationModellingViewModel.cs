using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using DynamicData.Binding;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.Sequence;
using MolecularWeightCalculatorGUI.Utilities;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    internal class FragmentationModellingViewModel : ReactiveObject
    {
        [Obsolete("For WPF design-time use only", true)]
        public FragmentationModellingViewModel() : this(new MolecularWeightTool(ElementMassMode.Isotopic))
        {
        }

        public FragmentationModellingViewModel(MolecularWeightTool molWeight)
        {
            mwt = molWeight;
            peptide = mwt.Peptide;

            NotationOptionsSource = CollectionUtils.GetCollectionForEnum<AminoAcidNotationMode>();
            MassChargeLevelOptions = CollectionUtils.GetCollectionForEnum<MassChargeLevel>(MassChargeLevel.M);
            NTerminusGroupOptions = CollectionUtils.GetCollectionForEnum<NTerminusGroupType>();
            CTerminusGroupOptions = CollectionUtils.GetCollectionForEnum<CTerminusGroupType>();
            IonTypesList = CollectionUtils.GetCollectionForEnum<IonType>();
            ChargeThresholdOptions = Enumerable.Range(0, 16).Select(x => x * 100).ToList();
            IonMassDigitsOptions = Enumerable.Range(0, 7).ToList();

            NeutralLossIonTypes = new ObservableCollectionExtended<IonType>(new IonType[] { IonType.BIon, IonType.YIon });

            this.WhenAnyValue(x => x.ShowAIons, x => x.ShowBIons, x => x.ShowCIons, x => x.ShowYIons, x => x.ShowZIons)
                .SubscribeOnChange(x => UpdateShownIons());
            this.WhenAnyValue(x => x.NeutralLossIonTypes.Count, x => x.NeutralLossWater, x => x.NeutralLossAmmonia,
                x => x.NeutralLossPhosphate).SubscribeOnChange(x => UpdateNeutralLossIons());
            this.WhenAnyValue(x => x.Show2PlusCharges, x => x.TwoPlusChargesThreshold, x => x.Show3PlusCharges,
                x => x.ThreePlusChargesThreshold).SubscribeOnChange(x => UpdateIonCharges());
            this.WhenAnyValue(x => x.Sequence, x => x.NTerminusGroup, x => x.CTerminusGroup,
                x => x.SelectedAminoAcidNotation).SubscribeOnChange(x => UpdateSequence());
            this.WhenAnyValue(x => x.IonMassDigits).SubscribeOnChange(x => UpdateFragments());
            this.WhenAnyValue(x => x.MassChargeLevel).SubscribeOnChange(x => UpdateMasses());
            this.WhenAnyValue(x => x.ElementModeAverage, x => x.ElementModeIsotopic)
                .SubscribeOnChange(x => UpdateElementMode());

            CopyMolecularWeightCommand = ReactiveCommand.Create(CopySequenceMW);

            UpdateShownIons(false);
            UpdateNeutralLossIons(false);
            UpdateIonCharges(false);
            UpdateSequence(false);
            UpdateFragments();
        }

        internal void ActivateWindow()
        {
            switch (mwt.GetElementMode())
            {
                case ElementMassMode.Average:
                    ElementModeIsotopic = false;
                    ElementModeAverage = true;
                    break;
                case ElementMassMode.Isotopic:
                    ElementModeAverage = false;
                    ElementModeIsotopic = true;
                    break;
                default:
                    // TODO: Coerce Isotopic Mode when opening...
                    mwt.SetElementMode(ElementMassMode.Isotopic);
                    ElementModeAverage = false;
                    ElementModeIsotopic = true;
                    break;
            }
        }

        private readonly MolecularWeightTool mwt;
        private readonly Peptide peptide;
        private string sequence = "Arg-His-Pro-Glu-Tyr-Ala-Val";
        private AminoAcidNotationMode selectedAminoAcidNotation = AminoAcidNotationMode.ThreeLetterNotation;
        private double mass = 870.43478;
        private MassChargeLevel massChargeLevel = MassChargeLevel.MPlus2H;
        private double massProtonated = 871.442056;
        private double massAtChargeLevel = 436.224666;
        private bool elementModeAverage;
        private bool elementModeIsotopic = true;
        private DataTable fragmentationDataTable = null;
        private NTerminusGroupType nTerminusGroup = NTerminusGroupType.Hydrogen;
        private CTerminusGroupType cTerminusGroup = CTerminusGroupType.Hydroxyl;
        private bool showAIons;
        private bool showBIons = true;
        private bool showCIons;
        private bool showYIons = true;
        private bool showZIons;
        private bool neutralLossWater = true;
        private bool neutralLossAmmonia;
        private bool neutralLossPhosphate;
        private bool show2PlusCharges;
        private int twoPlusChargesThreshold = 800;
        private bool show3PlusCharges;
        private int threePlusChargesThreshold = 900;
        private int ionMassDigits = 2;

        public IReadOnlyList<AminoAcidNotationMode> NotationOptionsSource { get; }
        public IReadOnlyList<MassChargeLevel> MassChargeLevelOptions { get; }
        public IReadOnlyList<NTerminusGroupType> NTerminusGroupOptions { get; }
        public IReadOnlyList<CTerminusGroupType> CTerminusGroupOptions { get; }
        public IReadOnlyList<IonType> IonTypesList { get; }
        public IReadOnlyList<int> ChargeThresholdOptions { get; }
        public IReadOnlyList<int> IonMassDigitsOptions { get; }

        public ReactiveCommand<RxUnit, RxUnit> CopyMolecularWeightCommand { get; }
        public string Sequence
        {
            get => sequence;
            set => this.RaiseAndSetIfChanged(ref sequence, value);
        }

        public AminoAcidNotationMode SelectedAminoAcidNotation
        {
            get => selectedAminoAcidNotation;
            set => this.RaiseAndSetIfChanged(ref selectedAminoAcidNotation, value);
        }

        public double Mass
        {
            get => mass;
            set => this.RaiseAndSetIfChanged(ref mass, value);
        }

        public MassChargeLevel MassChargeLevel
        {
            get => massChargeLevel;
            set => this.RaiseAndSetIfChanged(ref massChargeLevel, value);
        }

        public double MassProtonated
        {
            get => massProtonated;
            set => this.RaiseAndSetIfChanged(ref massProtonated, value);
        }

        public double MassAtChargeLevel
        {
            get => massAtChargeLevel;
            set => this.RaiseAndSetIfChanged(ref massAtChargeLevel, value);
        }

        public bool ElementModeAverage
        {
            get => elementModeAverage;
            set => this.RaiseAndSetIfChanged(ref elementModeAverage, value);
        }

        public bool ElementModeIsotopic
        {
            get => elementModeIsotopic;
            set => this.RaiseAndSetIfChanged(ref elementModeIsotopic, value);
        }

        public DataTable FragmentationDataTable
        {
            get => fragmentationDataTable;
            private set => this.RaiseAndSetIfChanged(ref fragmentationDataTable, value);
        }

        public NTerminusGroupType NTerminusGroup
        {
            get => nTerminusGroup;
            set => this.RaiseAndSetIfChanged(ref nTerminusGroup, value);
        }

        public CTerminusGroupType CTerminusGroup
        {
            get => cTerminusGroup;
            set => this.RaiseAndSetIfChanged(ref cTerminusGroup, value);
        }

        public bool ShowAIons
        {
            get => showAIons;
            set => this.RaiseAndSetIfChanged(ref showAIons, value);
        }

        public bool ShowBIons
        {
            get => showBIons;
            set => this.RaiseAndSetIfChanged(ref showBIons, value);
        }

        public bool ShowCIons
        {
            get => showCIons;
            set => this.RaiseAndSetIfChanged(ref showCIons, value);
        }

        public bool ShowYIons
        {
            get => showYIons;
            set => this.RaiseAndSetIfChanged(ref showYIons, value);
        }

        public bool ShowZIons
        {
            get => showZIons;
            set => this.RaiseAndSetIfChanged(ref showZIons, value);
        }

        public ObservableCollectionExtended<IonType> NeutralLossIonTypes { get; }

        public bool NeutralLossWater
        {
            get => neutralLossWater;
            set => this.RaiseAndSetIfChanged(ref neutralLossWater, value);
        }

        public bool NeutralLossAmmonia
        {
            get => neutralLossAmmonia;
            set => this.RaiseAndSetIfChanged(ref neutralLossAmmonia, value);
        }

        public bool NeutralLossPhosphate
        {
            get => neutralLossPhosphate;
            set => this.RaiseAndSetIfChanged(ref neutralLossPhosphate, value);
        }

        public bool Show2PlusCharges
        {
            get => show2PlusCharges;
            set => this.RaiseAndSetIfChanged(ref show2PlusCharges, value);
        }

        public int TwoPlusChargesThreshold
        {
            get => twoPlusChargesThreshold;
            set => this.RaiseAndSetIfChanged(ref twoPlusChargesThreshold, value);
        }

        public bool Show3PlusCharges
        {
            get => show3PlusCharges;
            set => this.RaiseAndSetIfChanged(ref show3PlusCharges, value);
        }

        public int ThreePlusChargesThreshold
        {
            get => threePlusChargesThreshold;
            set => this.RaiseAndSetIfChanged(ref threePlusChargesThreshold, value);
        }

        public int IonMassDigits
        {
            get => ionMassDigits;
            set => this.RaiseAndSetIfChanged(ref ionMassDigits, value);
        }

        private void UpdateShownIons(bool updateFragments = true)
        {
            var options = peptide.GetFragmentationSpectrumOptions();
            options.IonTypeOptions[(int)IonType.AIon].ShowIon = ShowAIons;
            options.IonTypeOptions[(int)IonType.BIon].ShowIon = ShowBIons;
            options.IonTypeOptions[(int)IonType.CIon].ShowIon = ShowCIons;
            options.IonTypeOptions[(int)IonType.YIon].ShowIon = ShowYIons;
            options.IonTypeOptions[(int)IonType.ZIon].ShowIon = ShowZIons;

            if (updateFragments)
            {
                UpdateFragments();
            }
        }

        private void UpdateNeutralLossIons(bool updateFragments = true)
        {
            var options = peptide.GetFragmentationSpectrumOptions();

            foreach (var ionType in IonTypesList)
            {
                var settings = options.IonTypeOptions[(int)ionType];
                if (NeutralLossIonTypes.Contains(ionType))
                {
                    settings.NeutralLossWater = NeutralLossWater;
                    settings.NeutralLossAmmonia = NeutralLossAmmonia;
                    settings.NeutralLossPhosphate = NeutralLossPhosphate;
                }
                else
                {
                    settings.NeutralLossWater = false;
                    settings.NeutralLossAmmonia = false;
                    settings.NeutralLossPhosphate = false;
                }
            }

            if (updateFragments)
            {
                UpdateFragments();
            }
        }

        private void UpdateIonCharges(bool updateFragments = true)
        {
            var options = peptide.GetFragmentationSpectrumOptions();
            options.DoubleChargeIonsShow = Show2PlusCharges;
            options.DoubleChargeIonsThreshold = TwoPlusChargesThreshold;
            options.TripleChargeIonsShow = Show3PlusCharges;
            options.TripleChargeIonsThreshold = ThreePlusChargesThreshold;

            if (updateFragments)
            {
                UpdateFragments();
            }
        }

        private void UpdateSequence(bool updateFragments = true)
        {
            // TODO: Where to set this?
            var options = peptide.GetFragmentationSpectrumOptions();
            options.IntensityOptions.BYIonShoulder = 0;

            peptide.SetSequence(Sequence, NTerminusGroup, CTerminusGroup, SelectedAminoAcidNotation == AminoAcidNotationMode.ThreeLetterNotation);
            UpdateMasses();

            if (updateFragments)
            {
                UpdateFragments();
            }
        }

        private void UpdateMasses()
        {
            Mass = peptide.GetPeptideMass();
            MassProtonated = mwt.ConvoluteMass(Mass, 0, 1);
            MassAtChargeLevel = mwt.ConvoluteMass(Mass, 0, (short) MassChargeLevel);
        }

        private void UpdateElementMode()
        {
            if (ElementModeAverage)
            {
                mwt.SetElementMode(ElementMassMode.Average);
            }
            else
            {
                mwt.SetElementMode(ElementMassMode.Isotopic);
            }

            UpdateSequence();
        }

        private void UpdateFragments()
        {
            var fragData = peptide.GetFragmentationMasses();

            // Process the fragments for the display table
            var numberFormat = $"F{IonMassDigits}";
            var table = new DataTable();
            var ionTypeHeaders = fragData.Select(x => x.SymbolGeneric).Distinct().OrderBy(x => x).ToList();
            table.Columns.Add("#");
            table.Columns.Add("Immon");

            var ionSymbolY = peptide.LookupIonTypeString(IonType.YIon);
            var ionSymbolZ = peptide.LookupIonTypeString(IonType.ZIon);

            var seqColumnAdded = false;

            foreach (var header in ionTypeHeaders)
            {
                if (!seqColumnAdded && (header[0] == ionSymbolY[0] || header[0] == ionSymbolZ[0]))
                {
                    table.Columns.Add("Seq");
                    seqColumnAdded = true;
                }

                table.Columns.Add(header);
            }

            if (!seqColumnAdded)
            {
                table.Columns.Add("Seq");
            }

            table.Columns.Add("#C");

            // Populate data
            var residueCount = peptide.GetResidueCount();
            var dataBlank = new object[table.Columns.Count];
            for (var i = 0; i < residueCount; i++)
            {
                table.Rows.Add(dataBlank);
                var row = table.Rows[i];
                row["#"] = i + 1;
                row["#C"] = residueCount - i;
                if (peptide.GetResidue(i, out var residueSymbol, out var residueMass, out _, out _))
                {
                    row["Immon"] = peptide.ComputeImmoniumMass(residueMass).ToString(numberFormat);
                    if (SelectedAminoAcidNotation == AminoAcidNotationMode.ThreeLetterNotation)
                    {
                        row["Seq"] = residueSymbol;
                    }
                    else
                    {
                        var oneLetter = mwt.GetAminoAcidSymbolConversion(residueSymbol, false);
                        if (string.IsNullOrWhiteSpace(oneLetter)) oneLetter = residueSymbol;
                        if (oneLetter == "Xxx") oneLetter = "X";

                        row["Seq"] = oneLetter;
                    }
                }
            }

            // TODO: Figure out how to do cell highlighting for matched ions...
            foreach (var entry in fragData)
            {
                var row = table.Rows[entry.SourceResidueNumber];
                row[entry.SymbolGeneric] = entry.Mass.ToString(numberFormat);
            }

            FragmentationDataTable = table;
        }

        private void CopySequenceMW()
        {
            Clipboard.SetText(peptide.GetPeptideMass().ToString(), TextDataFormat.Text);
        }
    }
}
