using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using MolecularWeightCalculator.FormulaFinder;
using ReactiveUI;

namespace MolecularWeightCalculatorGUI.FormulaFinder
{
    internal class FinderResult
    {
        // Static Properties and methods

        public static string RichTextFontName { get; private set; } = "Arial";
        public static int RichTextFontSize { get; private set; } = 10;
        private static FontFamily richTextFontFamily;
        private static double richTextFontSizePixels;
        private static string rtfPrefix;

        static FinderResult()
        {
            SetRichTextFont(RichTextFontName, RichTextFontSize);
        }

        public static void SetRichTextFont(string fontName, int fontSize)
        {
            if (string.IsNullOrEmpty(fontName))
            {
                fontName = "Arial";
            }

            if (fontSize < 4)
            {
                fontSize = 4;
            }

            RichTextFontName = fontName;
            RichTextFontSize = fontSize;
            richTextFontFamily = new FontFamily(fontName);
            // WPF default is convert from points to pixels with 96.0 / 72.0. See System.Windows.LengthConverter.
            richTextFontSizePixels = 96.0 / 72.0 * (fontSize + 2);
            // ReSharper disable StringLiteralTypo
            //rtfPrefix = @"{\rtf1\ansi\ansicpg1252\nouicompat\deflang1033\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Calibri;}{\f2\fnil\fcharset0 Cambria;}{\f3\fnil\fcharset0 " + RichTextFontName + @";}}\pard\plain\f3\fs" + (int)(RichTextFontSize * 2.25d) + " ";
            // Note: RTF Font size is in half-points, so this math turns "10pt" into 12.5 point size
            rtfPrefix = @"{\rtf1\ansi\ansicpg1252\nouicompat\deflang1033\deff0{\fonttbl{\f0\fnil\fcharset0 " + RichTextFontName + @";}}\pard\plain\f0\fs" + (int)(RichTextFontSize * 2.25d) + " ";
            // ReSharper restore StringLiteralTypo
        }

        // Instance properties and methods

        public FinderResult(SearchResult searchResult, bool findMz, bool showDeltaMass)
        {
            result = searchResult;

            SetDisplayText(findMz, showDeltaMass, out var formulaText, out var statsText, out var pctCompText);
            FormulaAndChargeText = formulaText;
            FormulaStatsText = statsText;
            PercentCompositionText = pctCompText;
        }

        private readonly SearchResult result;

        public IReadOnlyList<ElementCount> CountsByElement => result.CountsByElement;
        public string EmpiricalFormula => result.EmpiricalFormula;
        public string SortKey => result.SortKey;
        public double Mass => result.Mass;
        public int ChargeState => result.ChargeState;

        // Note: Added checks to avoid bad
        public double Mz => result.Mz;
        public double DeltaMass => result.DeltaMass;
        public IReadOnlyList<ElementPercent> PercentComposition => result.PercentComposition;

        public string DisplayText => FormulaAndChargeText + FormulaStatsText + (!string.IsNullOrWhiteSpace(PercentCompositionText) ? $"\n{PercentCompositionText}" : "");
        public string FormulaAndChargeText { get; }
        public string FormulaStatsText { get; }
        public string PercentCompositionText { get; }

        public string DisplayRtf => rtfPrefix + DisplayRtfNoDocument + "}";
        public string DisplayRtfPrefix => rtfPrefix;
        // Generate on request - spread the processing load to only when needed
        public string DisplayRtfNoDocument => GetDisplayTextRtf();
        // Generate on request (from the UI) - supports ListBox virtualization, avoids thread issues, and doesn't bog down 5000+ results processing
        public TextBlock DisplayWpf => GetDisplayTextWpf();

        private void SetDisplayText(bool findMz, bool showDeltaMass, out string formulaText, out string statsText, out string pctCompText)
        {
            const string mw = "MW";
            // Display dm result in 0.000 notation rather than exponential
            var deltaMassFormat = "#0.0#######";
            var deltaMassPpmFormat = "#0.0";
            var mzFormat = "#0.000";

#pragma warning disable CS0162
            if ( /* TODO: frmProgramPreferences.optStdDevType(1).value == true */ false)
            {
                // Use scientific notation instead
                deltaMassFormat = "0.0#######E+00";
                deltaMassPpmFormat = "0.0####E+00";
                mzFormat = "0.0######E+00";
            }
#pragma warning restore CS0162

            formulaText = EmpiricalFormula;
            statsText = "";
            var mz = "";
            if (ChargeState != 0)
            {
                formulaText += "^" + ChargeState;

                if (findMz)
                {
                    // Add m/z value
                    mz = $"\tm/z {Mz.ToString(mzFormat)}";
                }
            }

            statsText += $"\t{mw}={Mass}";

            if (!showDeltaMass || Math.Abs(DeltaMass) <= 0.00000000001)
            {
                // dblTargetMass <= 0 means matching percent compositions, so don't want to add dm= to line
                // Don't add difference in mass (dm) result since standard deviation mode is off
            }
            else if (!result.DeltaMassIsPPM)
            {
                statsText += $"\tdm={DeltaMass.ToString(deltaMassFormat)}";
            }
            else
            {
                statsText += $"\tdm={DeltaMass.ToString(deltaMassPpmFormat)} ppm";
            }

            statsText += mz;

            pctCompText = "";
            if (PercentComposition.Count > 0)
            {
                // Add % info
                pctCompText = " has " + string.Join(" ", PercentComposition.Select(x => x.ToString()));
            }
        }

        private string GetDisplayTextRtf()
        {
            var rtf = ConvertResultToRtf() + FormulaStatsText;
            if (!string.IsNullOrWhiteSpace(PercentCompositionText))
            {
                rtf += @"\par " + PercentCompositionText;
            }

            return rtf;
        }

        private TextBlock GetDisplayTextWpf()
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                // Prevent an exception, since a TextBlock cannot be created in MTA
                // Doesn't guarantee no exception, because calling this on a non-UI STA thread and showing it in the UI would probably trigger a different exception.
                return null;
            }

            // NOTE: All UI-displayed objects must be created on an STA thread
            var runs = ConvertResultToWpf();
            runs.Add(new Run(FormulaStatsText));
            if (!string.IsNullOrWhiteSpace(PercentCompositionText))
            {
                runs.Add(new Run($"\n {PercentCompositionText}"));
            }

            var displayWpf = new TextBlock()
            {
                FontFamily = richTextFontFamily,
                FontSize = richTextFontSizePixels,
            };
            displayWpf.Inlines.AddRange(runs);

            return displayWpf;
        }

        private string ConvertResultToRtf()
        {
            // Converts plain text to formatted rtf text.
            // Rtf string must begin with {{\fonttbl{\f0\fcharset0\fprq2 Times New Roman;}}\pard\plain\fs25
            // and must end with }

            var rtf = "";

            foreach (var item in CountsByElement)
            {
                //if (item.Symbol.StartsWith("^"))
                //{
                //    // TODO: Handle super scripting the isotope for an element?
                //}
                rtf += item.Symbol;

                if (item.Count > 1)
                {
                    rtf += $@"{{\sub {item.Count}" + @"}";
                }
            }

            if (result.ChargeState != 0)
            {
                // Conditional format string: before semicolon is formatting for positive values, after semicolon for negative values
                rtf += $@"{{\super {result.ChargeState:#0.###+;#0.###-}" + @"}"; // Add closing bracket separately, since it tries to include it in the format string.
            }

            return rtf;
        }

        private List<Inline> ConvertResultToWpf()
        {
            // Do not use both BaselineAlignment and Typography.Variants.
            var runs = new List<Inline>();

            foreach (var item in CountsByElement)
            {
                //if (item.Symbol.StartsWith("^"))
                //{
                //    // TODO: Handle super scripting the isotope for an element?
                //}
                runs.Add(new Run(item.Symbol));

                if (item.Count > 1)
                {
                    var countRun = new Run(item.Count.ToString()) { Typography = { Variants = FontVariants.Subscript } };
                    runs.Add(countRun);
                }
            }

            if (ChargeState != 0)
            {
                // Conditional format string: before semicolon is formatting for positive values, after semicolon for negative values
                // TODO: Consider using unicode code for +/- superscripted, since there seem to be some issues with the variants?...
                var charge = new Run($"{result.ChargeState:#0.###+;#0.###-}") { Typography = { Variants = FontVariants.Superscript } };
                runs.Add(charge);
            }

            return runs;
        }

        public override string ToString()
        {
            return $"{EmpiricalFormula}^{ChargeState}";
        }

        public class FormulaSort : IComparer<FinderResult>
        {
            public int Compare(FinderResult x, FinderResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return string.Compare(x.SortKey, y.SortKey, StringComparison.Ordinal); // Ordinal sort, because we want to sort by binary values
            }
        }

        public class NoSort : IComparer<FinderResult>
        {
            // Note: This will keep the existing order for stable sorts; Nothing guaranteed for unstable sorts (list.Sort())
            public int Compare(FinderResult x, FinderResult y)
            {
                return 0;
            }
        }

        public class MolecularWeightSort : IComparer<FinderResult>
        {
            public int Compare(FinderResult x, FinderResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Mass.CompareTo(y.Mass);
            }
        }

        public class DeltaMassSort : IComparer<FinderResult>
        {
            public int Compare(FinderResult x, FinderResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.DeltaMass.CompareTo(y.DeltaMass);
            }
        }

        public class ChargeSort : IComparer<FinderResult>
        {
            public int Compare(FinderResult x, FinderResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.ChargeState.CompareTo(y.ChargeState);
            }
        }

        public class MzSort : IComparer<FinderResult>
        {
            public int Compare(FinderResult x, FinderResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Mz.CompareTo(y.Mz);
            }
        }
    }
}
