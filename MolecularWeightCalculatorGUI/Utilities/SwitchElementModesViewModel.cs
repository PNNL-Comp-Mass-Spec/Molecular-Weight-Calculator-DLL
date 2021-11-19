using System;
using System.Windows;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using ReactiveUI;

namespace MolecularWeightCalculatorGUI.Utilities
{
    internal class SwitchElementModesViewModel : ReactiveObject
    {
        // Ignore Spelling: Daltons, Gly, Leu, Modelling, Tyr

        [Obsolete("For WPF design-time use only", true)]
        public SwitchElementModesViewModel() : this(new MolecularWeightTool())
        {
        }

        public SwitchElementModesViewModel(MolecularWeightTool molWeight)
        {
            mwt = molWeight;
        }

        private const string FormulaFinderTitle = "Formula Finder";
        private const string FormulaFinderMessage1 = "The typical use of the Formula Finder feature is for when the monoisotopic mass (weight) of a compound is known (typically determined by Mass Spectrometry) and potential matching compounds are to be searched for.";
        private const string FormulaFinderMessage2 = "For example, a mass of 16.0312984 Daltons is measured for a compound containing Carbon and Hydrogen, and the possible empirical formula is desired.  Performing the search, with a weight tolerance of 5000 ppm results in three compounds, H2N, CH4, and O.  Within 500 ppm only CH4 matches, which is the correct match.";

        private const string FragmentationModellingTitle = "Peptide Sequence Fragmentation Modelling";
        private const string FragmentationModellingMessage1 = "The typical use of the Fragmentation Modelling feature is for predicting the masses expected to be observed with a Mass Spectrometer when a peptide is ionized, enters the instrument, and fragments along the peptide backbone.";
        private const string FragmentationModellingMessage2 = "The peptide typically fragments at each amide bond.  For example, the peptide Gly-Leu-Tyr will form the fragments Gly-Leu, Leu-Tyr, Gly, Leu, and Tyr.  Additionally, the cleavage of the amide bond can occur at differing locations, resulting in varying weights.";

        private const string ChangeModesMessage = "To correctly use this feature, the program must be set to Isotopic Weight mode.  This can be done manually by choosing Edit Elements Table under the File menu, or the program can automatically switch to this mode for you.";

        private readonly MolecularWeightTool mwt;
        private string windowTitle = FormulaFinderTitle;
        private string message1 = FormulaFinderMessage1;
        private string message2 = FormulaFinderMessage2;
        private string message3 = ChangeModesMessage;
        private bool switchToIsotopic = true;
        private bool switchToIsotopicAuto;
        private bool keepAverage;
        private bool stopShowingWarning;

        public string WindowTitle
        {
            get => windowTitle;
            set => this.RaiseAndSetIfChanged(ref windowTitle, value);
        }

        public string Message1
        {
            get => message1;
            set => this.RaiseAndSetIfChanged(ref message1, value);
        }

        public string Message2
        {
            get => message2;
            set => this.RaiseAndSetIfChanged(ref message2, value);
        }

        public string Message3
        {
            get => message3;
            set => this.RaiseAndSetIfChanged(ref message3, value);
        }

        public bool SwitchToIsotopic
        {
            get => switchToIsotopic;
            set => this.RaiseAndSetIfChanged(ref switchToIsotopic, value);
        }

        public bool SwitchToIsotopicAuto
        {
            get => switchToIsotopicAuto;
            set => this.RaiseAndSetIfChanged(ref switchToIsotopicAuto, value);
        }

        public bool KeepAverage
        {
            get => keepAverage;
            set => this.RaiseAndSetIfChanged(ref keepAverage, value);
        }

        public bool StopShowingWarning
        {
            get => stopShowingWarning;
            set => this.RaiseAndSetIfChanged(ref stopShowingWarning, value);
        }

        public void ShowWindow(bool isFragmentationModelling, Window parent)
        {
            if (mwt.GetElementMode() == ElementMassMode.Isotopic)
            {
                // No Change needed
                return;
            }

            if (StopShowingWarning && !SwitchToIsotopicAuto)
            {
                // Never warn, just leave alone
                return;
            }

            if (SwitchToIsotopicAuto)
            {
                // Always change
                mwt.SetElementMode(ElementMassMode.Isotopic);
                return;
            }

            SetWindowMode(isFragmentationModelling);
            var window = new SwitchElementModesWindow { DataContext = this, Owner = parent};
            window.ShowDialog();

            if (SwitchToIsotopic || SwitchToIsotopicAuto)
            {
                // Set once
                mwt.SetElementMode(ElementMassMode.Isotopic);
            }
        }

        public void SetWindowMode(bool isFragmentationModelling)
        {
            if (isFragmentationModelling)
            {
                windowTitle = FragmentationModellingTitle;
                message1 = FragmentationModellingMessage1;
                message2 = FragmentationModellingMessage2;
                message3 = ChangeModesMessage;
            }
            else
            {
                windowTitle = FormulaFinderTitle;
                message1 = FormulaFinderMessage1;
                message2 = FormulaFinderMessage2;
                message3 = ChangeModesMessage;
            }
        }
    }
}
