using System.Threading;
using MolecularWeightCalculator;
using MolecularWeightCalculatorGUI.CapillaryFlowUI;
using MolecularWeightCalculatorGUI.FormulaCalc;
using MolecularWeightCalculatorGUI.MassChargeConversion;
using MolecularWeightCalculatorGUI.MoleMassDilutionUI;
using MolecularWeightCalculatorGUI.PeptideUI;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI
{
    internal class MainViewModel : ReactiveObject
    {
        // TODO: Localization: https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/wpf-globalization-and-localization-overview?view=netframeworkdesktop-4.8

        public MainViewModel()
        {
            mwt = new MolecularWeightTool();
            FormulaCalc = new FormulaCalcViewModel(mwt);
            moleMassDilutionVm = new MoleMassDilutionViewModel();
            mzCalcVm = new MzCalculationsViewModel();
            aminoAcidConvertVm = new AminoAcidConverterViewModel(FormulaCalc, mwt.Peptide);
            capillaryFlowVm = new CapillaryFlowViewModel();

            ShowAboutCommand = ReactiveCommand.Create(ShowAboutWindow);
            OpenMoleMassDilutionWindowCommand = ReactiveCommand.Create(OpenMoleMassDilutionWindow);
            OpenMassChargeConversionsWindowCommand = ReactiveCommand.Create(OpenMassChargeConversionsWindow);
            OpenAminoAcidConverterWindowCommand = ReactiveCommand.Create(OpenAminoAcidConverterWindow);
            OpenCapillaryFlowWindowCommand = ReactiveCommand.Create(OpenCapillaryFlowWindow);
        }

        private readonly MolecularWeightTool mwt;
        private readonly MoleMassDilutionViewModel moleMassDilutionVm;
        private readonly MzCalculationsViewModel mzCalcVm;
        private readonly AminoAcidConverterViewModel aminoAcidConvertVm;
        private readonly CapillaryFlowViewModel capillaryFlowVm;
        private bool mainWindowVisible = true;

        public FormulaCalcViewModel FormulaCalc { get; }

        public ReactiveCommand<RxUnit, RxUnit> ShowAboutCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> OpenMoleMassDilutionWindowCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> OpenMassChargeConversionsWindowCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> OpenAminoAcidConverterWindowCommand { get; }
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

        private void OpenMoleMassDilutionWindow()
        {
            var lastFormula = FormulaCalc.LastFocusedFormula;
            lastFormula.Calculate();
            moleMassDilutionVm.Data.FormulaXaml = lastFormula.FormulaXaml;
            moleMassDilutionVm.Data.FormulaMass = lastFormula.Mass;

            //MainWindowVisible = false;
            var window = new MoleMassDilutionWindow {DataContext = moleMassDilutionVm};
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
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

        private void OpenAminoAcidConverterWindow()
        {
            //MainWindowVisible = false;
            var window = new AminoAcidConverterWindow { DataContext = aminoAcidConvertVm };
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
