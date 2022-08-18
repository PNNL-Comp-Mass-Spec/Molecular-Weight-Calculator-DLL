using System;
using MolecularWeightCalculator.FormulaFinder;
using ReactiveUI;

namespace MolecularWeightCalculatorGUI.FormulaFinder
{
    internal class ElementConfiguration : ReactiveObject
    {
        [Obsolete("For WPF Design time use only", true)]
        public ElementConfiguration() : this(0, "Example")
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index">Element index</param>
        /// <param name="caption"></param>
        /// <param name="fixedElement">Set to 'True' if the element is fixed/unchangeable. 'True' also hides the weight input display</param>
        /// <param name="atomicNumber">If <paramref name="fixedElement"/> is true, the atomic number of the element this references; not used otherwise</param>
        public ElementConfiguration(int index, string caption, bool fixedElement = false, int atomicNumber = 0)
        {
            Index = index;
            Caption = caption;
            FixedElement = fixedElement;
            if (fixedElement)
            {
                FixedAtomicNumber = atomicNumber;
                Use = true;
                Percent = 10;
                weightText = "Fixed";
                if (atomicNumber == 6)
                {
                    Percent = 70;
                }
            }

            // automatically clear out parsed data when WeightText is updated...
            this.WhenAnyValue(x => x.WeightText).Subscribe(x => ClearParsedData());
        }

        private int min = 0;
        private int max = 10;
        private bool use;
        private double percent;
        private string weightText = "";
        private string lastParsedWeightText = null;

        public int Index { get; }
        public string Caption { get; }
        public bool FixedElement { get; }
        public bool ShowWeight => !FixedElement;
        public int FixedAtomicNumber { get; }

        public int Min
        {
            get => min;
            set => this.RaiseAndSetIfChanged(ref min, value);
        }

        public int Max
        {
            get => max;
            set => this.RaiseAndSetIfChanged(ref max, value);
        }

        public bool Use
        {
            get => use;
            set => this.RaiseAndSetIfChanged(ref use, value);
        }

        public double Percent
        {
            get => percent;
            set => this.RaiseAndSetIfChanged(ref percent, value);
        }

        // TODO: Don't allow null?
        public string WeightText
        {
            get => weightText;
            set
            {
                if (!FixedElement)
                {
                    this.RaiseAndSetIfChanged(ref weightText, value);
                }
                else
                {
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsParsed { get; private set; }
        public double Mass { get; private set; }
        public float Charge { get; private set; }
        public string MatchSymbol { get; private set; }

        public void ClearParsedData()
        {
            if (IsParsed && !string.Equals(lastParsedWeightText, WeightText))
            {
                IsParsed = false;
                Mass = 0;
                Charge = 0;
                MatchSymbol = "";
            }
        }

        public void SetParsedData(double weight, float charge, string matchSymbol, string newWeightText = "")
        {
            if (!string.IsNullOrWhiteSpace(newWeightText))
            {
                lastParsedWeightText = newWeightText; // Prevent accidental clearing of parsed values
                WeightText = newWeightText; // TODO: Should this be wrapped in RxApp.MainThreadScheduler?
            }
            else
            {
                lastParsedWeightText = WeightText;
            }

            Mass = weight;
            Charge = charge;
            MatchSymbol = matchSymbol;
            IsParsed = true;
        }

        public CandidateElement GetCandidateElement(FormulaSearchModes searchMode, double percentTolerance)
        {
            var symbol = FixedElement ? MatchSymbol : WeightText;
            var minCount = min;
            var maxCount = max;
            if (searchMode == FormulaSearchModes.Thorough)
            {
                minCount = 0;
                maxCount = int.MaxValue;
            }
            return new CandidateElement(symbol, MatchSymbol, Mass, Charge, minCount, maxCount, Percent, percentTolerance);
        }
    }
}
