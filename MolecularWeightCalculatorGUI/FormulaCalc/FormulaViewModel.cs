using System;
using MolecularWeightCalculator;
using ReactiveUI;

namespace MolecularWeightCalculatorGUI.FormulaCalc
{
    internal class FormulaViewModel : ReactiveObject
    {
        private string formula;
        private double mass;
        private double stDev;
        private string formulaRtf;
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
        }
    }
}
