using MolecularWeightCalculator;
using MolecularWeightCalculatorGUI.FormulaCalc;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI
{
    internal class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            mwt = new MolecularWeightTool();
            FormulaCalc = new FormulaCalcViewModel(mwt);

            ShowAboutCommand = ReactiveCommand.Create(ShowAboutWindow);
        }

        private readonly MolecularWeightTool mwt;

        public FormulaCalcViewModel FormulaCalc { get; }

        public ReactiveCommand<RxUnit, RxUnit> ShowAboutCommand { get; }

        private void ShowAboutWindow()
        {
            var window = new AboutWindow();
            window.ShowDialog();
        }
    }
}
