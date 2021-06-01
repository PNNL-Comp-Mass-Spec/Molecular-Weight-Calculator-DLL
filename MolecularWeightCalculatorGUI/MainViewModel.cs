﻿using System.Threading;
using System.Windows;
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

            ShowAboutCommand = ReactiveCommand.Create<Window>(ShowAboutWindow);
            OpenMoleMassDilutionWindowCommand = ReactiveCommand.Create<Window>(OpenMoleMassDilutionWindow);
            OpenMassChargeConversionsWindowCommand = ReactiveCommand.Create<Window>(OpenMassChargeConversionsWindow);
            OpenAminoAcidConverterWindowCommand = ReactiveCommand.Create<Window>(OpenAminoAcidConverterWindow);
            OpenCapillaryFlowWindowCommand = ReactiveCommand.Create<Window>(OpenCapillaryFlowWindow);
        }

        private readonly MolecularWeightTool mwt;
        private readonly MoleMassDilutionViewModel moleMassDilutionVm;
        private readonly MzCalculationsViewModel mzCalcVm;
        private readonly AminoAcidConverterViewModel aminoAcidConvertVm;
        private readonly CapillaryFlowViewModel capillaryFlowVm;
        private bool mainWindowVisible = true;

        public FormulaCalcViewModel FormulaCalc { get; }

        public ReactiveCommand<Window, RxUnit> ShowAboutCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenMoleMassDilutionWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenMassChargeConversionsWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenAminoAcidConverterWindowCommand { get; }
        public ReactiveCommand<Window, RxUnit> OpenCapillaryFlowWindowCommand { get; }

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

        private void OpenAminoAcidConverterWindow(Window parent)
        {
            //MainWindowVisible = false;
            var window = new AminoAcidConverterWindow { DataContext = aminoAcidConvertVm, Owner = parent };
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
    }
}
