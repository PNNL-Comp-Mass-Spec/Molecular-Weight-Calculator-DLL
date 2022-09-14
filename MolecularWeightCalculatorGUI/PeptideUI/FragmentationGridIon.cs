using ReactiveUI;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    internal class FragmentationGridIon : ReactiveObject
    {
        public FragmentationGridIon()
        {
            Display = "0";
            Value = 0;
        }

        public FragmentationGridIon(double value, string formatString, bool isMatched = false)
        {
            Value = value;
            Display = value.ToString(formatString);
            matched = isMatched;
        }

        private bool matched = false;
        private bool matchedShoulder = false;
        private bool errorMatched = false;

        public string Display { get; private set; }

        public double Value { get; }

        public bool Matched
        {
            get => matched;
            private set => this.RaiseAndSetIfChanged(ref matched, value);
        }

        public bool MatchedShoulder
        {
            get => matchedShoulder;
            private set => this.RaiseAndSetIfChanged(ref matchedShoulder, value);
        }

        public bool ErrorMatched
        {
            get => errorMatched;
            private set => this.RaiseAndSetIfChanged(ref errorMatched, value);
        }

        public void SetFormat(string formatString)
        {
            Display = Value.ToString(formatString);
            this.RaisePropertyChanged(nameof(Display));
        }

        public void SetMatched()
        {
            Matched = true;
            MatchedShoulder = false;
            ErrorMatched = false;
        }

        public void SetMatchedShoulder()
        {
            // Only change to yellow if currently white
            if (!Matched && !ErrorMatched)
            {
                MatchedShoulder = true;
            }
        }

        public void SetErrorMatched()
        {
            Matched = false;
            MatchedShoulder = false;
            ErrorMatched = true;
        }

        public void ResetMatched()
        {
            Matched = false;
            MatchedShoulder = false;
            ErrorMatched = false;
        }

        public override string ToString()
        {
            return Display;
        }
    }
}
