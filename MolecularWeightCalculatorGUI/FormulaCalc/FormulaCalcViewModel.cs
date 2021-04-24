using System;
using System.Reactive.Linq;
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

            Formulas.Add(new FormulaViewModel(1, mwt) { Formula = "BrCH2(CH2)7CH2Br", Mass = 286.04722, StDev = 0.003 });
            Formulas.Add(new FormulaViewModel(2, mwt) { Formula = "FeCl3-6H2O", Mass = 270.29478, StDev = 0.003 });
            Formulas.Add(new FormulaViewModel(3, mwt) { Formula = "Co(Bpy)(CO)4", Mass = 327.1576, StDev = 0.003 });
            Formulas.Add(new FormulaViewModel(4, mwt) { Formula = "^13C6H6-.1H2O", Mass = 85.84917, StDev = 0.0002 });
            Formulas.Add(new FormulaViewModel(5, mwt) { Formula = "HGlyLeuTyrOH", Mass = 351.39762, StDev = 0.003 });
            Formulas.Add(new FormulaViewModel(6, mwt) { Formula = "BrCH2(CH2)7CH2Br>CH8", Mass = 265.973, StDev = 0.003 });

            this.WhenAnyValue(x => x.ElementModeAverage).Where(x => x).Subscribe(x => mwt.SetElementMode(ElementMassMode.Average));
            this.WhenAnyValue(x => x.ElementModeIsotopic).Where(x => x).Subscribe(x => mwt.SetElementMode(ElementMassMode.Isotopic));
            this.WhenAnyValue(x => x.ElementModeInteger).Where(x => x).Subscribe(x => mwt.SetElementMode(ElementMassMode.Integer));

            CalculateCommand = ReactiveCommand.Create(Calculate);
            NewFormulaCommand = ReactiveCommand.Create(AddNewFormula);

            Calculate();
        }

        private readonly MolecularWeightTool mwt;
        private bool elementModeAverage = true;
        private bool elementModeIsotopic;
        private bool elementModeInteger;

        public ObservableCollectionExtended<FormulaViewModel> Formulas { get; } = new ObservableCollectionExtended<FormulaViewModel>();

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

        private void Calculate()
        {
            foreach (var formula in Formulas)
            {
                formula.Calculate();
            }
        }

        private void AddNewFormula()
        {
            var newFormula = new FormulaViewModel(Formulas.Count + 1, mwt);
            Formulas.Add(newFormula);
        }
    }
}
