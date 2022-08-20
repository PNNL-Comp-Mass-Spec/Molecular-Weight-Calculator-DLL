using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using DynamicData.Binding;
using MolecularWeightCalculator;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.FormulaCalc
{
    internal class FormulaViewModel : ReactiveObject
    {
        private string formula;
        private double mass;
        private double standardDeviation;
        private string formulaRtf;
        private string formulaXaml;
        private DateTime lastFocusTime = DateTime.MinValue;
        private bool showPercentComposition = false;
        private string cautionText = null;
        private readonly MolecularWeightTool mWeight;
        private RichTextBox richTextBox = null;
        private ObservableAsPropertyHelper<bool> hasPercentComposition;

        public int FormulaIndex { get; }

        public string Formula
        {
            get => formula;
            set => this.RaiseAndSetIfChanged(ref formula, value);
        }

        public double Mass
        {
            get => mass;
            set => this.RaiseAndSetIfChanged(ref mass, value);
        }

        public double StandardDeviation
        {
            get => standardDeviation;
            set => this.RaiseAndSetIfChanged(ref standardDeviation, value);
        }

        public string FormulaRtf
        {
            get => formulaRtf;
            set => this.RaiseAndSetIfChanged(ref formulaRtf, value);
        }

        public string FormulaXaml
        {
            get => formulaXaml;
            set => this.RaiseAndSetIfChanged(ref formulaXaml, value);
        }

        public DateTime LastFocusTime
        {
            get => lastFocusTime;
            set => this.RaiseAndSetIfChanged(ref lastFocusTime, value);
        }

        public bool ShowPercentComposition
        {
            get => showPercentComposition;
            set => this.RaiseAndSetIfChanged(ref showPercentComposition, value);
        }

        public string CautionText
        {
            get => cautionText;
            private set => this.RaiseAndSetIfChanged(ref cautionText, value);
        }

        public int ErrorId { get; private set; }

        public string ErrorDescription { get; private set; }

        public string CautionDescription { get; private set; }

        public bool HasPercentComposition => hasPercentComposition.Value;

        public ObservableCollectionExtended<KeyValuePair<string, string>> PercentComposition { get; } =
            new ObservableCollectionExtended<KeyValuePair<string, string>>();

        public ReactiveCommand<RxUnit, bool> TogglePercentCompositionCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CalculateCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CutSelectedCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopySelectedCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> PasteCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopyFormulaCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopyFormulaRtfCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopyMolecularWeightCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopyPercentCompositionCommand { get; }

        /// <summary>
        /// Default constructor needed for WPF Design-time
        /// </summary>
        [Obsolete("For WPF design-time use only", true)]
        public FormulaViewModel() : this(1, new MolecularWeightTool())
        {
        }

        public FormulaViewModel(int index, MolecularWeightTool mwt)
        {
            FormulaIndex = index;
            Formula = "";
            FormulaRtf = "";
            mWeight = mwt;

            TogglePercentCompositionCommand = ReactiveCommand.Create(() => ShowPercentComposition = !ShowPercentComposition);
            CalculateCommand = ReactiveCommand.Create(Calculate, this.WhenAnyValue(x => x.Formula).Select(x => !string.IsNullOrWhiteSpace(x)));
            CutSelectedCommand = ReactiveCommand.Create(CutSelected);
            CopySelectedCommand = ReactiveCommand.Create(CopySelected);
            PasteCommand = ReactiveCommand.Create(Paste);
            CopyFormulaCommand = ReactiveCommand.Create(CopyFormula);
            CopyFormulaRtfCommand = ReactiveCommand.Create(CopyFormulaRtf);
            CopyMolecularWeightCommand = ReactiveCommand.Create(CopyMolecularWeight);
            CopyPercentCompositionCommand = ReactiveCommand.Create(CopyPercentComposition);

            hasPercentComposition = this.WhenAnyValue(x => x.PercentComposition.Count).Select(x => x > 0)
                .ToProperty(this, x => x.HasPercentComposition);
        }

        public FormulaViewModel(int index, MolecularWeightTool mwt, FormulaViewModel copyFrom) : this(index, mwt)
        {
            Mass = copyFrom.Mass;
            StandardDeviation = copyFrom.StandardDeviation;
            Formula = copyFrom.Formula;
            FormulaRtf = copyFrom.FormulaRtf;
            FormulaXaml = copyFrom.FormulaXaml;
            ErrorId = copyFrom.ErrorId;
            ErrorDescription = copyFrom.ErrorDescription;
            CautionDescription = copyFrom.CautionDescription;
            CautionText = copyFrom.ErrorDescription;
            PercentComposition.Load(copyFrom.PercentComposition);
        }

        public void SetRichTextBoxControl(RichTextBox rtb)
        {
            richTextBox = rtb;
        }

        public void Calculate()
        {
            if (string.IsNullOrWhiteSpace(Formula))
            {
                Clear();
                return;
            }

            var compound = mWeight.Compound;
            // Calculate mass for formula
            compound.Formula = Formula;
            Mass = compound.GetMass(false);
            StandardDeviation = compound.StandardDeviation;
            var formulaChanged = !Formula.Equals(compound.FormulaCapitalized);
            Formula = compound.FormulaCapitalized;
            var rtfChanged = !FormulaRtf.Equals(compound.FormulaRTF);
            FormulaRtf = compound.FormulaRTF;
            FormulaXaml = compound.FormulaXaml;
            ErrorId = compound.ErrorId;
            ErrorDescription = compound.ErrorDescription;
            CautionDescription = compound.CautionDescription;

            if (formulaChanged && !rtfChanged)
            {
                this.RaisePropertyChanged(nameof(FormulaRtf));
                this.RaisePropertyChanged(nameof(FormulaXaml));
            }

            if (compound.ErrorId != 0)
            {
                CautionText = ErrorDescription;
                return;
            }

            if (!string.IsNullOrWhiteSpace(CautionDescription))
            {
                CautionText = CautionDescription;
            }
            else
            {
                // Set to null to hide the tooltip
                CautionText = null;
            }

            PercentComposition.Load(compound.GetPercentCompositionForAllElements());
        }

        public void CutSelected()
        {
            richTextBox?.Cut();
        }

        public void CopySelected()
        {
            richTextBox?.Copy();
        }

        public void Paste()
        {
            richTextBox?.Paste();
        }

        public void CopyFormula()
        {
            Clipboard.SetText(Formula, TextDataFormat.Text);
        }

        public void CopyFormulaRtf()
        {
            Clipboard.SetText(FormulaRtf, TextDataFormat.Rtf);
        }

        public void CopyMolecularWeight()
        {
            // Do not copy standard deviation
            Clipboard.SetText(Mass.ToString(), TextDataFormat.Text);
        }

        public void CopyPercentComposition()
        {
            var text = "";
            foreach (var item in PercentComposition)
            {
                var split = item.Value.Split(' ');
                var pct = split[0];
                var stdDev = split[1].Trim('(', ')');
                text += $"{item.Key}\t{pct}\t{stdDev}\n";
            }

            text.TrimEnd('\n');

            Clipboard.SetText(text, TextDataFormat.Text);
        }

        public void ClearWithConfirmation(Window owner)
        {
            var result = MessageBox.Show(owner,
                "Are you sure you want to erase the current formula?",
                "Erase Current Formula", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.No)
            {
                return;
            }
            Clear();
        }

        public void Clear()
        {
            Mass = 0;
            StandardDeviation = 0;
            Formula = "";
            FormulaRtf = "";
            FormulaXaml = "";
            ErrorId = 0;
            ErrorDescription = "";
            CautionDescription = "";
            CautionText = "";
            PercentComposition.Clear();
            ShowPercentComposition = false;
        }

        public void CopyValuesFromOther(FormulaViewModel copyFrom)
        {
            Mass = copyFrom.Mass;
            StandardDeviation = copyFrom.StandardDeviation;
            Formula = copyFrom.Formula;
            FormulaRtf = copyFrom.FormulaRtf;
            FormulaXaml = copyFrom.FormulaXaml;
            ErrorId = copyFrom.ErrorId;
            ErrorDescription = copyFrom.ErrorDescription;
            CautionDescription = copyFrom.CautionDescription;
            CautionText = copyFrom.ErrorDescription;
            PercentComposition.Load(copyFrom.PercentComposition);
        }

        public void ExpandAbbreviations(Window owner)
        {
            var result = MessageBox.Show(owner,
                "Are you sure you want to convert the current formula into its empirical formula?",
                "Convert to Empirical Formula", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            var compound = mWeight.Compound;
            // Calculate mass for formula
            compound.Formula = Formula;

            compound.ExpandAbbreviations();

            Formula = compound.FormulaCapitalized;
            FormulaRtf = compound.FormulaRTF;
            FormulaXaml = compound.FormulaXaml;
        }

        public void ConvertToEmpirical(Window owner)
        {
            var result = MessageBox.Show(owner,
                "Are you sure you want to expand the abbreviations of the current formula to their elemental equivalents?",
                "Expand Abbreviations", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            var compound = mWeight.Compound;
            // Calculate mass for formula
            compound.Formula = Formula;

            compound.ConvertToEmpirical();

            Formula = compound.FormulaCapitalized;
            FormulaRtf = compound.FormulaRTF;
            FormulaXaml = compound.FormulaXaml;
        }
    }
}
