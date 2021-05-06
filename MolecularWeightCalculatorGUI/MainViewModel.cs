using System.Threading;
using MolecularWeightCalculator;
using MolecularWeightCalculatorGUI.CapillaryFlowUI;
using MolecularWeightCalculatorGUI.FormulaCalc;
using MolecularWeightCalculatorGUI.MassChargeConversion;
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
            mzCalcVm = new MzCalculationsViewModel();
            capillaryFlowVm = new CapillaryFlowViewModel();

            ShowAboutCommand = ReactiveCommand.Create(ShowAboutWindow);
            OpenMassChargeConversionsWindowCommand = ReactiveCommand.Create(OpenMassChargeConversionsWindow);
            OpenCapillaryFlowWindowCommand = ReactiveCommand.Create(OpenCapillaryFlowWindow);
        }

        private readonly MolecularWeightTool mwt;
        private readonly MzCalculationsViewModel mzCalcVm;
        private readonly CapillaryFlowViewModel capillaryFlowVm;
        private bool mainWindowVisible = true;

        public FormulaCalcViewModel FormulaCalc { get; }

        public ReactiveCommand<RxUnit, RxUnit> ShowAboutCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> OpenMassChargeConversionsWindowCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> OpenCapillaryFlowWindowCommand { get; }

        public bool MainWindowVisible
        {
            get => mainWindowVisible;
            private set => this.RaiseAndSetIfChanged(ref mainWindowVisible, value);
        }

        private void ShowAboutWindow()
        {
            var window = new AboutWindow();
            window.ShowDialog();
        }

        private void OpenMassChargeConversionsWindow()
        {
            //MainWindowVisible = false;
            var window = new MzCalculationsWindow { DataContext = mzCalcVm };
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
        }

        private void OpenCapillaryFlowWindow()
        {
            //MainWindowVisible = false;
            var window = new CapillaryFlowWindow { DataContext = capillaryFlowVm };
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
        }
    }
}
