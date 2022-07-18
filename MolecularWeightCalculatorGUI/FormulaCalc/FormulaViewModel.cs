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
        private string cautionText = null;
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

        public string CautionText
        {
            get => cautionText;
            private set => this.RaiseAndSetIfChanged(ref cautionText, value);
        }

        public int ErrorId { get; private set; }

        public string ErrorDescription { get; private set; }

        public string CautionDescription { get; private set; }

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
            var formulaChanged = !Formula.Equals(compound.FormulaCapitalized);
            Formula = compound.FormulaCapitalized;
            var rtfChanged = !FormulaRtf.Equals(compound.FormulaRTF);
            FormulaRtf = compound.FormulaRTF;
            FormulaXaml = compound.FormulaXaml;
            ErrorId = compound.ErrorId;
            ErrorDescription = compound.ErrorDescription;
            CautionDescription = compound.CautionDescription;

            if (formulaChanged && !rtfChanged)
            {
                this.RaisePropertyChanged(nameof(FormulaRtf));
                this.RaisePropertyChanged(nameof(FormulaXaml));
            }

            if (compound.ErrorId != 0)
            {
                CautionText = ErrorDescription;
                return;
            }

            if (!string.IsNullOrWhiteSpace(CautionDescription))
            {
                CautionText = CautionDescription;
            }
            else
            {
                // Set to null to hide the tooltip
                CautionText = null;
            }

            PercentComposition.Load(compound.GetPercentCompositionForAllElements());
        }
    }
}
