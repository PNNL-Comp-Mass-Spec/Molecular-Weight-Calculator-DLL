using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using DynamicData.Binding;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.Sequence;
using MolecularWeightCalculatorGUI.FormulaCalc;
using MolecularWeightCalculatorGUI.Utilities;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    internal class AminoAcidConverterViewModel : ReactiveObject
    {
        [Obsolete("For WPF design-time use only", true)]
        public AminoAcidConverterViewModel() : this(new FormulaCalcViewModel(), new Peptide(new ElementAndMassTools()), new SwitchElementModesViewModel(), new FragmentationModellingViewModel())
        {
        }

        public AminoAcidConverterViewModel(FormulaCalcViewModel formulaList, Peptide peptide, SwitchElementModesViewModel switchModesVm, FragmentationModellingViewModel fragModelling)
        {
            formulas = formulaList;
            peptideTools = peptide;
            switchElementModesVm = switchModesVm;
            fragModellingVm = fragModelling;

            OneLetterSequence = "GLY";
            SpaceEvery10Residues = true;
            SeparateResiduesWithDash = true;

            AvailableFormulaDisplays = new ObservableCollectionExtended<int>(formulas.Formulas.Select(x => x.FormulaIndex));
            AvailableFormulaDisplays.Add(AvailableFormulaDisplays.Max() + 1);

            formulas.Formulas.WhenAnyValue(x => x.Count).Subscribe(x =>
            {
                var ids = formulas.Formulas.Select(y => y.FormulaIndex).ToList();
                ids.Add(ids.Max() + 1);
                AvailableFormulaDisplays.Load(ids);

                SelectFirstEmptyFormulaOrNew();
            });

            SelectFirstEmptyFormulaOrNew();

            ConvertOneToThreeCommand = ReactiveCommand.Create(ConvertOneToThree);
            ConvertThreeToOneCommand = ReactiveCommand.Create(ConvertThreeToOne);
            ModelFragmentationCommand = ReactiveCommand.Create<Window>(OpenModelFragmentationWindow);
            CopySequenceToFormulaCommand = ReactiveCommand.Create(() => CopySequenceToFormula(SelectedFormulaDisplay));

            ConvertOneToThree();

            this.WhenAnyValue(x => x.OneLetterSequence).Subscribe(x => ValidateOneLetterSequence());
            this.WhenAnyValue(x => x.ThreeLetterSequence).Subscribe(x => ValidateThreeLetterSequence());
        }

        private readonly FormulaCalcViewModel formulas;
        private readonly Peptide peptideTools;
        private readonly SwitchElementModesViewModel switchElementModesVm;
        private readonly FragmentationModellingViewModel fragModellingVm;
        private string oneLetterSequence;
        private string threeLetterSequence;
        private bool spaceEvery10Residues;
        private bool separateResiduesWithDash;
        private int selectedFormulaDisplay;
        private bool suppressOneLetterSequenceValidation = false;
        private bool suppressThreeLetterSequenceValidation = false;

        // Valid characters in the sequence: space, period/comma (decimal point), A-Z, a-z; strip any other character
        private readonly Regex validMatch = new Regex("^[A-Za-z., \\-]*$", RegexOptions.Compiled);

        public string OneLetterSequence
        {
            get => oneLetterSequence;
            set => this.RaiseAndSetIfChanged(ref oneLetterSequence, value);
        }

        public string ThreeLetterSequence
        {
            get => threeLetterSequence;
            set => this.RaiseAndSetIfChanged(ref threeLetterSequence, value);
        }

        public bool SpaceEvery10Residues
        {
            get => spaceEvery10Residues;
            set => this.RaiseAndSetIfChanged(ref spaceEvery10Residues, value);
        }

        public bool SeparateResiduesWithDash
        {
            get => separateResiduesWithDash;
            set => this.RaiseAndSetIfChanged(ref separateResiduesWithDash, value);
        }

        public ObservableCollectionExtended<int> AvailableFormulaDisplays { get; }

        public int SelectedFormulaDisplay
        {
            get => selectedFormulaDisplay;
            set => this.RaiseAndSetIfChanged(ref selectedFormulaDisplay, value);
        }

        public ReactiveCommand<RxUnit, RxUnit> ConvertOneToThreeCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> ConvertThreeToOneCommand { get; }
        public ReactiveCommand<Window, RxUnit> ModelFragmentationCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopySequenceToFormulaCommand { get; }

        /// <summary>
        /// Convert a string of amino acid abbreviations to their 3 letter abbreviation
        /// </summary>
        private void ConvertOneToThree()
        {
            peptideTools.SetSequence(OneLetterSequence, is3LetterCode: false);
            suppressThreeLetterSequenceValidation = true;
            ThreeLetterSequence = peptideTools.GetSequence(true, false, SeparateResiduesWithDash);
            suppressThreeLetterSequenceValidation = false;
        }

        /// <summary>
        /// Convert a string of amino acid abbreviations from their 3 letter abbreviation
        /// </summary>
        private void ConvertThreeToOne()
        {
            peptideTools.SetSequence(ThreeLetterSequence);
            suppressOneLetterSequenceValidation = true;
            OneLetterSequence = peptideTools.GetSequence(false, SpaceEvery10Residues, false);
            suppressOneLetterSequenceValidation = false;
        }

        /// <summary>
        /// Copy 3 letter sequence to the peptide sequence fragmentation modeller
        /// </summary>
        /// <param name="parent"></param>
        private void OpenModelFragmentationWindow(Window parent)
        {
            // Check or change mode
            switchElementModesVm.ShowWindow(true, parent);

            fragModellingVm.PasteNewSequence(ThreeLetterSequence, true);

            // TODO: Don't open new window if a window is already open...
            var window = new FragmentationModellingWindow() { DataContext = fragModellingVm };
            window.Show();
        }

        /// <summary>
        /// Copy 3 letter sequence to requested formula on the main formula display
        /// </summary>
        /// <param name="targetFormulaId"></param>
        private void CopySequenceToFormula(int targetFormulaId)
        {
            if (targetFormulaId > formulas.Formulas.Max(x => x.FormulaIndex))
            {
                formulas.AddNewFormula();
            }

            var maxExisting = formulas.Formulas.Max(x => x.FormulaIndex);
            if (targetFormulaId > maxExisting)
            {
                targetFormulaId = maxExisting;
            }

            var target = formulas.Formulas.First(x => x.FormulaIndex == targetFormulaId);
            target.Formula = ThreeLetterSequence;
            target.Calculate();
        }

        private void ValidateOneLetterSequence()
        {
            if (suppressOneLetterSequenceValidation)
                return;

            suppressOneLetterSequenceValidation = true;
            OneLetterSequence = ValidateSequence(OneLetterSequence);
            suppressOneLetterSequenceValidation = false;
        }

        private void ValidateThreeLetterSequence()
        {
            if (suppressThreeLetterSequenceValidation)
                return;

            suppressThreeLetterSequenceValidation = true;
            ThreeLetterSequence = ValidateSequence(ThreeLetterSequence);
            suppressThreeLetterSequenceValidation = false;
        }

        /// <summary>
        /// Make sure characters in an Amino-acid containing textbox are valid
        /// </summary>
        /// <param name="newSequence"></param>
        /// <returns>Cleaned sequence</returns>
        private string ValidateSequence(string newSequence)
        {
            // Valid characters in the sequence: space, period/comma (decimal point), A-Z, a-z; strip any other character
            if (string.IsNullOrWhiteSpace(newSequence))
            {
                return "";
            }

            if (validMatch.IsMatch(newSequence))
            {
                return newSequence;
            }

            var cleaned = new StringBuilder(newSequence.Length);
            foreach (var c in newSequence)
            {
                switch (c)
                {
                    case ' ': // Spaces are allowed
                    case ',': // Decimal point (. or ,) is allowed (as a separator)
                    case '.':
                    case '-': // Allow separation with dashes
                    case >= 'A' and <= 'Z': // Characters are allowed
                    case >= 'a' and <= 'z':
                        cleaned.Append(c);
                        break;
                    default: break;
                }
            }

            return cleaned.ToString();
        }

        /// <summary>
        /// Examine the formulas on frmMain, looking for the first blank one
        /// </summary>
        private void SelectFirstEmptyFormulaOrNew()
        {
            var index = -1;
            foreach (var formula in formulas.Formulas)
            {
                if (string.IsNullOrWhiteSpace(formula.Formula))
                {
                    index = formula.FormulaIndex;
                    break;
                }
            }

            if (index <= 0)
            {
                index = formulas.Formulas.Max(x => x.FormulaIndex) + 1;
            }

            SelectedFormulaDisplay = index;
        }

        public void WindowActivated()
        {
            SelectFirstEmptyFormulaOrNew();
        }
    }
}
