using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using DynamicData;
using DynamicData.Binding;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.FormulaCalc
{
    internal class FormulaCalcViewModel : ReactiveObject
    {
        [Obsolete("For WPF Design-time use only", true)]
        public FormulaCalcViewModel() : this(new MolecularWeightTool())
        {
        }

        public FormulaCalcViewModel(MolecularWeightTool molWeight)
        {
            mwt = molWeight;

            // Set the font to Calibri, which supports Typography.Variations; Arial does not, so subscript and superscript don't appear.
            //mwt.RtfFontName = "Calibri"; // Variations Support: Calibri, Cambria, Segoe UI
            mwt.RtfFontName = "Cambria";
            mwt.RtfFontSize = 10;

            Formulas.Add(new FormulaViewModel(1, mwt) { Formula = "BrCH2(CH2)7CH2Br", Mass = 286.04722, StandardDeviation = 0.003, LastFocusTime = DateTime.Now });
            Formulas.Add(new FormulaViewModel(2, mwt) { Formula = "FeCl3-6H2O", Mass = 270.29478, StandardDeviation = 0.003 });
            Formulas.Add(new FormulaViewModel(3, mwt) { Formula = "Co(Bpy)(CO)4", Mass = 327.1576, StandardDeviation = 0.003 });
            Formulas.Add(new FormulaViewModel(4, mwt) { Formula = "^13C6H6-.1H2O", Mass = 85.84917, StandardDeviation = 0.0002 });
            Formulas.Add(new FormulaViewModel(5, mwt) { Formula = "HGlyLeuTyrOH", Mass = 351.39762, StandardDeviation = 0.003 });
            Formulas.Add(new FormulaViewModel(6, mwt) { Formula = "BrCH2(CH2)7CH2Br>CH8", Mass = 265.973, StandardDeviation = 0.003 });

            this.WhenAnyValue(x => x.ElementModeAverage).Where(x => x).Subscribe(x => mwt.SetElementMode(ElementMassMode.Average));
            this.WhenAnyValue(x => x.ElementModeIsotopic).Where(x => x).Subscribe(x => mwt.SetElementMode(ElementMassMode.Isotopic));
            this.WhenAnyValue(x => x.ElementModeInteger).Where(x => x).Subscribe(x => mwt.SetElementMode(ElementMassMode.Integer));

            var formulaChangedCheck = Formulas.ToObservableChangeSet().AutoRefresh(x => x.Formula).ToCollection();

            CalculateCommand = ReactiveCommand.Create(Calculate, formulaChangedCheck.Select(x => x.Any(y => !string.IsNullOrWhiteSpace(y.Formula))));
            NewFormulaCommand = ReactiveCommand.Create(AddNewFormula, formulaChangedCheck.Select(x => !x.Any(y => string.IsNullOrWhiteSpace(y.Formula))));

            Calculate();

            var time = DateTime.Now;
            // Set up the default "focused" order, from top to bottom
            foreach (var formula in Formulas)
            {
                formula.LastFocusTime = time;
                time = time.AddSeconds(-10);
            }
        }

        private readonly MolecularWeightTool mwt;
        private bool elementModeAverage = true;
        private bool elementModeIsotopic;
        private bool elementModeInteger;
        private string cautionText = "";

        public ObservableCollectionExtended<FormulaViewModel> Formulas { get; } = new ObservableCollectionExtended<FormulaViewModel>();

        public FormulaViewModel LastFocusedFormula => Formulas.OrderByDescending(x => x.LastFocusTime).First();

        public ReactiveCommand<RxUnit, RxUnit> CalculateCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> NewFormulaCommand { get; }

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

        public bool ElementModeInteger
        {
            get => elementModeInteger;
            set => this.RaiseAndSetIfChanged(ref elementModeInteger, value);
        }

        public string CautionText
        {
            get => cautionText;
            set => this.RaiseAndSetIfChanged(ref cautionText, value);
        }

        private void Calculate()
        {
            CautionText = "";
            foreach (var formula in Formulas)
            {
                formula.Calculate();

                if (string.IsNullOrWhiteSpace(CautionText) && !string.IsNullOrWhiteSpace(formula.CautionText))
                {
                    CautionText = formula.CautionText;
                }
            }
        }

        public void AddNewFormula()
        {
            if (Formulas.Any(x => string.IsNullOrWhiteSpace(x.Formula)))
            {
                return;
            }

            var newFormula = new FormulaViewModel(Formulas.Count + 1, mwt);
            Formulas.Add(newFormula);
        }

        public void AddDuplicateFormula()
        {
            if (string.IsNullOrWhiteSpace(LastFocusedFormula.Formula))
            {
                return;
            }

            foreach (var formula in Formulas)
            {
                if (string.IsNullOrWhiteSpace(formula.Formula))
                {
                    // Fill the empty formula
                    formula.CopyValuesFromOther(LastFocusedFormula);
                    return;
                }
            }

            var newFormula = new FormulaViewModel(Formulas.Count + 1, mwt, LastFocusedFormula);
            Formulas.Add(newFormula);
        }

        public void RemoveFormula(Window owner)
        {
            var result = MessageBox.Show(owner,
                "Are you sure you want to remove the current formula?",
                "Remove Formula", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            var found = false;
            for (var i = 0; i < Formulas.Count - 1; i++)
            {
                found = found || Formulas[i].FormulaIndex == LastFocusedFormula.FormulaIndex;
                if (found)
                {
                    Formulas[i].CopyValuesFromOther(Formulas[i + 1]);
                }
            }

            if (Formulas.Count > 1)
            {
                Formulas.RemoveAt(Formulas.Count - 1);
            }
        }

        public void ClearAll(Window owner)
        {
            var result = MessageBox.Show(owner,
                "Are you sure you want to erase all the formulas?",
                "Erase all Formulas", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            foreach (var formula in Formulas)
            {
                formula.Clear();
            }
        }
    }
}
