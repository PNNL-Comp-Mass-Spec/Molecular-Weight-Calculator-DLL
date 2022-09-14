using System;
using System.Text.RegularExpressions;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    internal class EditModSymbolDetailsViewModel : ReactiveObject
    {
        [Obsolete("For WPF design-time use only", true)]
        public EditModSymbolDetailsViewModel() : this("%", 50, "Test")
        { }

        public EditModSymbolDetailsViewModel(string s, double m, string c)
        {
            Symbol = s;
            Mass = m;
            Comment = c;

            Result = EditWindowResult.Cancel;

            CloseCommand = ReactiveCommand.Create<EditWindowResult>(x => Result = x);
        }

        private readonly Regex characterWhitelist = new Regex(@"[^`~!@#$%^&*_+?']", RegexOptions.Compiled);
        private string symbol;
        private double mass;
        private string comment;

        public EditWindowResult Result { get; private set; }

        public ReactiveCommand<EditWindowResult, RxUnit> CloseCommand { get; }

        public string Symbol
        {
            get => symbol;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var x = characterWhitelist.Replace(value, "");
                    if (!value.Equals(x))
                    {
                        value = x;
                        if (value.Equals(symbol))
                        {
                            this.RaisePropertyChanged();
                        }
                    }
                }

                this.RaiseAndSetIfChanged(ref symbol, value);
            }
        }

        public double Mass
        {
            get => mass;
            set => this.RaiseAndSetIfChanged(ref mass, value);
        }

        public string Comment
        {
            get => comment;
            set => this.RaiseAndSetIfChanged(ref comment, value);
        }
    }

    internal enum EditWindowResult
    {
        Ok,
        Cancel,
        Remove
    }
}
