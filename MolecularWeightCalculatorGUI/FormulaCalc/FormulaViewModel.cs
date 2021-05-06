using System;
using System.Collections.Generic;
using DynamicData.Binding;
using MolecularWeightCalculator;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.FormulaCalc
{
    internal class FormulaViewModel : ReactiveObject
    {
        private string formula;
        private double mass;
        private double stDev;
        private string formulaRtf;
        private string formulaXaml;
        private DateTime lastFocusTime = DateTime.MinValue;
        private bool showPercentComposition = false;
        private readonly MolecularWeightTool mWeight;

        public int FormulaIndex { get; }

        public string Formula
        {
            get => formula;
            set => this.RaiseAndSetIfChanged(ref formula, value);
        }

        public double Mass
        {
            get => mass;
            set => this.RaiseAndSetIfChanged(ref mass, value);
        }

        public double StDev
        {
            get => stDev;
            set => this.RaiseAndSetIfChanged(ref stDev, value);
        }

        public string FormulaRtf
        {
            get => formulaRtf;
            set => this.RaiseAndSetIfChanged(ref formulaRtf, value);
        }

        public string FormulaXaml
        {
            get => formulaXaml;
            set => this.RaiseAndSetIfChanged(ref formulaXaml, value);
        }

        public DateTime LastFocusTime
        {
            get => lastFocusTime;
            set => this.RaiseAndSetIfChanged(ref lastFocusTime, value);
        }

        public bool ShowPercentComposition
        {
            get => showPercentComposition;
            set => this.RaiseAndSetIfChanged(ref showPercentComposition, value);
        }

        public ObservableCollectionExtended<KeyValuePair<string, string>> PercentComposition { get; } =
            new ObservableCollectionExtended<KeyValuePair<string, string>>();

        public ReactiveCommand<RxUnit, bool> TogglePercentCompositionCommand { get; }

        /// <summary>
        /// Default constructor needed for WPF Design-time
        /// </summary>
        [Obsolete("For WPF design-time use only", true)]
        public FormulaViewModel() : this(1, new MolecularWeightTool())
        {
        }

        public FormulaViewModel(int index, MolecularWeightTool mwt)
        {
            FormulaIndex = index;
            Formula = "";
            FormulaRtf = "";
            mWeight = mwt;

            TogglePercentCompositionCommand = ReactiveCommand.Create(() => ShowPercentComposition = !ShowPercentComposition);
        }

        public void Calculate()
        {
            var compound = mWeight.Compound;
            // Calculate mass for formula
            compound.Formula = Formula;
            Mass = compound.GetMass(false);
            StDev = compound.StandardDeviation;
            Formula = compound.FormulaCapitalized;
            FormulaRtf = compound.FormulaRTF;
            FormulaXaml = compound.FormulaXaml;
            PercentComposition.Load(compound.GetPercentCompositionForAllElements());
        }
    }
}
