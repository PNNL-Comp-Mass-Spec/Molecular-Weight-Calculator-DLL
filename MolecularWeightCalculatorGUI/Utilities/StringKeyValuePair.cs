using System;
using System.Collections.Generic;

namespace MolecularWeightCalculatorGUI.Utilities
{
    /// <summary>
    /// Simple wrapper class to improve use of a KeyValuePair(string, string) in XAML
    /// </summary>
    internal class StringKeyValuePair
    {
        [Obsolete("Only for WPF design-time use", true)]
        public StringKeyValuePair() : this(new KeyValuePair<string, string>("Key", "Value"))
        {
        }

        public StringKeyValuePair(KeyValuePair<string, string> pair)
        {
            kvp = pair;
        }

        private readonly KeyValuePair<string, string> kvp;

        public string Key => kvp.Key;
        public string Value => kvp.Value;
    }
}
