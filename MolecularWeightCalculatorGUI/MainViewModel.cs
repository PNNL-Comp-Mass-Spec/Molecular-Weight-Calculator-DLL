﻿using System.Windows;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.Sequence;
using MolecularWeightCalculatorGUI.CapillaryFlowUI;
using MolecularWeightCalculatorGUI.FormulaCalc;
using MolecularWeightCalculatorGUI.FormulaFinder;
using MolecularWeightCalculatorGUI.IsotopicDistribution;
using MolecularWeightCalculatorGUI.MassChargeConversion;
using MolecularWeightCalculatorGUI.MoleMassDilutionUI;
using MolecularWeightCalculatorGUI.PeptideUI;
using MolecularWeightCalculatorGUI.Utilities;
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
            formulaFinderVm = new FormulaFinderViewModel(mwt);
            switchElementModesVm = new SwitchElementModesViewModel(mwt); // TODO: synchronize the element mode when it changes...
            fragModellingVm = new FragmentationModellingViewModel(mwt);
            aminoAcidConvertVm = new AminoAcidConverterViewModel(FormulaCalc, new Peptide(mwt.ElementAndMass), switchElementModesVm, fragModellingVm);
            capillaryFlowVm = new CapillaryFlowViewModel();
            isotopicDistributionVm = new IsotopicDistributionViewModel(mwt);

            ShowAboutCommand = ReactiveCommand.Create<Window>(ShowAboutWindow);
            OpenMoleMassDilutionWindowCommand = ReactiveCommand.Create<Window>(OpenMoleMassDilutionWindow);
            OpenMassChargeConversionsWindowCommand = ReactiveCommand.Create<Window>(OpenMassChargeConversionsWindow);
            OpenFormulaFinderWindowCommand = ReactiveCommand.Create<Window>(OpenFormulaFinderWindow);
            OpenAminoAcidConverterWindowCommand = ReactiveCommand.Create<Window>(OpenAminoAcidConverterWindow);
            OpenFragmentationModellingWindowCommand = ReactiveCommand.Create<Window>(OpenFragmentationModellingWindow);
            OpenCapillaryFlowWindowCommand = ReactiveCommand.Create<Window>(OpenCapillaryFlowWindow);
            OpenIsotopicDistributionWindowCommand = ReactiveCommand.Create<Window>(OpenIsotopicDistributionWindow);
            OpenSelectedIsotopicDistributionWindowCommand = ReactiveCommand.Create<Window>(OpenSelectedIsotopicDistributionWindow);

            mwt.SetElementMode(ElementMassMode.Isotopic);
            FormulaCalc.ElementModeIsotopic = true;
        }

        private readonly MolecularWeightTool mwt;
        private readonly MoleMassDilutionViewModel moleMassDilutionVm;
        private readonly MzCalculationsViewModel mzCalcVm;
        private readonly FormulaFinderViewModel formulaFinderVm;
        private readonly SwitchElementModesViewModel switchElementModesVm;
        private readonly AminoAcidConverterViewModel aminoAcidConvertVm;
        private readonly FragmentationModellingViewModel fragModellingVm;
        private readonly CapillaryFlowViewModel capillaryFlowVm;
        private readonly IsotopicDistributionViewModel isotopicDistributionVm;
        private bool mainWindowVisible = true;

        public FormulaCalcViewModel FormulaCalc { get; }

        public ReactiveCommand<Window, RxUnit> ShowAboutCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenMoleMassDilutionWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenMassChargeConversionsWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenFormulaFinderWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenAminoAcidConverterWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenFragmentationModellingWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenCapillaryFlowWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenIsotopicDistributionWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenSelectedIsotopicDistributionWindowCommand { get; }

        public bool MainWindowVisible
        {
            get => mainWindowVisible;
            private set => this.RaiseAndSetIfChanged(ref mainWindowVisible, value);
        }

        private void ShowAboutWindow(Window parent)
        {
            var window = new AboutWindow { Owner = parent };
            window.ShowDialog();
        }

        private void OpenMoleMassDilutionWindow(Window parent)
        {
            var lastFormula = FormulaCalc.LastFocusedFormula;
            lastFormula.Calculate();
            moleMassDilutionVm.Data.FormulaXaml = lastFormula.FormulaXaml;
            moleMassDilutionVm.Data.FormulaMass = lastFormula.Mass;

            //MainWindowVisible = false;
            var window = new MoleMassDilutionWindow { DataContext = moleMassDilutionVm, Owner = parent };
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
        }

        private void OpenMassChargeConversionsWindow(Window parent)
        {
            //MainWindowVisible = false;
            var window = new MzCalculationsWindow { DataContext = mzCalcVm, Owner = parent };
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
        }

        private void OpenFormulaFinderWindow(Window parent)
        {
            // Check or change mode
            switchElementModesVm.ShowWindow(false, parent);
            //MainWindowVisible = false;
            formulaFinderVm.ElementMode = mwt.GetElementMode();
            var window = new FormulaFinderWindow() { DataContext = formulaFinderVm, Owner = parent };
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
        }

        private void OpenAminoAcidConverterWindow(Window parent)
        {
            //MainWindowVisible = false;
            var window = new AminoAcidConverterWindow { DataContext = aminoAcidConvertVm, Owner = parent };
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
        }

        private void OpenFragmentationModellingWindow(Window parent)
        {
            // Check or change mode
            switchElementModesVm.ShowWindow(true, parent);
            //MainWindowVisible = false;
            var window = new FragmentationModellingWindow() { DataContext = fragModellingVm, Owner = parent };
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
        }

        private void OpenCapillaryFlowWindow(Window parent)
        {
            //MainWindowVisible = false;
            var window = new CapillaryFlowWindow { DataContext = capillaryFlowVm, Owner = parent };
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
        }

        private void OpenIsotopicDistributionWindow(Window parent)
        {
            //MainWindowVisible = false;
            var window = new IsotopicDistributionWindow() { DataContext = isotopicDistributionVm, Owner = parent };
            // TODO: ShowDialog() breaks this.WhenAnyValue(...)
            //window.ShowDialog();
            window.Show();
            //MainWindowVisible = true;
        }

        private void OpenSelectedIsotopicDistributionWindow(Window parent)
        {
            isotopicDistributionVm.Formula = FormulaCalc.LastFocusedFormula.Formula;
            OpenIsotopicDistributionWindow(parent);
        }
    }
}
