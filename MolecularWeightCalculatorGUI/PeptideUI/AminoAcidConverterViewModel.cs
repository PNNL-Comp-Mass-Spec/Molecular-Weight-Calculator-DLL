﻿using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Binding;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.Sequence;
using MolecularWeightCalculatorGUI.FormulaCalc;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    internal class AminoAcidConverterViewModel : ReactiveObject
    {
        [Obsolete("For WPF design-time use only", true)]
        public AminoAcidConverterViewModel() : this(new FormulaCalcViewModel(), new Peptide(new ElementAndMassTools()))
        {
        }

        public AminoAcidConverterViewModel(FormulaCalcViewModel formulaList, Peptide peptide)
        {
            OneLetterSequence = "GLY";
            SpaceEvery10Residues = true;
            SeparateResiduesWithDash = true;
            peptideTools = peptide;

            formulas = formulaList;
            AvailableFormulaDisplays = new ObservableCollectionExtended<int>(formulas.Formulas.Select(x => x.FormulaIndex));
            AvailableFormulaDisplays.Add(AvailableFormulaDisplays.Max() + 1);

            formulas.Formulas.WhenAnyValue(x => x.Count).Subscribe(x =>
            {
                var ids = formulas.Formulas.Select(y => y.FormulaIndex).ToList();
                ids.Add(ids.Max() + 1);
                AvailableFormulaDisplays.Load(ids);

                SelectedFormulaDisplay = AvailableFormulaDisplays.Max();
            });

            SelectedFormulaDisplay = AvailableFormulaDisplays.Max();

            ConvertOneToThreeCommand = ReactiveCommand.Create(ConvertOneToThree);
            ConvertThreeToOneCommand     = ReactiveCommand.Create(ConvertThreeToOne);
            ModelFragmentationCommand    = ReactiveCommand.Create(OpenModelFragmentationWindow);
            CopySequenceToFormulaCommand = ReactiveCommand.Create(() => CopySequenceToFormula(SelectedFormulaDisplay));

            ConvertOneToThree();
        }

        private readonly FormulaCalcViewModel formulas;
        private readonly Peptide peptideTools;
        private string oneLetterSequence;
        private string threeLetterSequence;
        private bool spaceEvery10Residues;
        private bool separateResiduesWithDash;
        private int selectedFormulaDisplay;

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
        public ReactiveCommand<RxUnit, RxUnit> ModelFragmentationCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopySequenceToFormulaCommand { get; }

        private void ConvertOneToThree()
        {
            peptideTools.SetSequence(OneLetterSequence, is3LetterCode: false);
            ThreeLetterSequence = peptideTools.GetSequence(true, false, SeparateResiduesWithDash);
        }

        private void ConvertThreeToOne()
        {
            peptideTools.SetSequence(ThreeLetterSequence);
            OneLetterSequence = peptideTools.GetSequence(false, SpaceEvery10Residues, false);
        }

        private void OpenModelFragmentationWindow()
        {
            // TODO: !!!!
        }

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
    }
}
