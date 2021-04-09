using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal class ErrorDetails : IErrorData
    {
        /// <summary>
        /// Contains the error number (used in the LookupMessage function).  In addition, if a program error occurs, ErrorParams.ErrorID = -10
        /// </summary>
        public int ErrorId { get; set; }
        public int ErrorPosition { get; set; }
        public string ErrorCharacter { get; set; }
        public string ErrorDescription { get; set; }

        public void SetError(int errorId, int errorPosition, string errorCharacter = "")
        {
            ErrorId = errorId;
            ErrorPosition = errorPosition;
            ErrorCharacter = errorCharacter;
            ErrorDescription = "";
        }

        public void Reset()
        {
            ErrorCharacter = "";
            ErrorId = 0;
            ErrorPosition = 0;
            ErrorDescription = "";
        }

        public override string ToString()
        {
            return "ErrorID " + ErrorId + " at " + ErrorPosition + ": " + ErrorCharacter;
        }
    }

    /// <summary>
    /// Holds parsing data for a formula, including separate objects for different sections for subtraction.
    /// </summary>
    [ComVisible(false)]
    internal class FormulaParseData : IFormulaParseData
    {
        /// <summary>
        /// The updated formula (updated after parsing formula components)
        /// </summary>
        public string Formula { get; private set; }

        /// <summary>
        /// The original or user-supplied formula
        /// </summary>
        public string FormulaOriginal { get; }

        /// <summary>
        /// The corrected formula (appropriate capitalization, expanded abbreviations, etc.). May not be correct when an error occurs.
        /// </summary>
        public string FormulaCorrected => string.Join(">", FormulaSections.Select(x => x.Components.FormulaCorrected));

        /// <summary>
        /// Computed formula mass
        /// </summary>
        public double Mass => Stats.TotalMass;

        /// <summary>
        /// Computed standard deviation of the formula mass
        /// </summary>
        public double StandardDeviation => Stats.StandardDeviation;

        /// <summary>
        /// Computed charge
        /// </summary>
        public float Charge => Stats.Charge;

        /// <summary>
        /// Computation stats for the first <see cref="FormulaData"/> object in <see cref="FormulaSections"/>
        /// </summary>
        public ComputationStats Stats => FormulaSections.First().Stats;

        /// <summary>
        /// Component tree for the first <see cref="FormulaData"/> object in <see cref="FormulaSections"/>
        /// </summary>
        internal FormulaComponentGroup Components => FormulaSections.First().Components;

        /// <summary>
        /// Sections of the formula. Only one object, unless there are '&gt;' symbols in the formula for formula subtraction.
        /// </summary>
        internal List<FormulaData> FormulaSections { get; } = new List<FormulaData>(1);

        /// <summary>
        /// The list of caution notes for the formula, as a comma+space-separated string.
        /// </summary>
        public string CautionDescription => string.Join("", CautionDescriptionList);

        /// <summary>
        /// List of caution notes for the formula.
        /// </summary>
        public List<string> CautionDescriptionList { get; } = new List<string>();

        /// <summary>
        /// Error data object for the formula parsing
        /// </summary>
        public ErrorDetails Error { get; } = new ErrorDetails();

        public IErrorData ErrorData => Error;

        /// <summary>
        /// True if an error is logged in <see cref="Error"/>
        /// </summary>
        public bool HasError => Error.ErrorId != 0;

        /// <summary>
        /// The original formula, as parsed into components.
        /// </summary>
        public string ComponentsFormula => string.Join(">", FormulaSections);

        /// <summary>
        /// Usage of abbreviations. Key is abbreviation symbol, value is index of first usage. Used for checking for circular references among abbreviations.
        /// </summary>
        internal Dictionary<string, int> AbbreviationUsage { get; } = new Dictionary<string, int>();

        public FormulaParseData(string formula)
        {
            FormulaOriginal = formula;
            Formula = formula;
            FormulaSections.Add(new FormulaData());
        }

        private void UpdateFormula()
        {
            var corrected = FormulaCorrected;
            if (FormulaOriginal.Equals(corrected, StringComparison.OrdinalIgnoreCase))
            {
                // No changes whatsoever beyond maybe capitalization, just use the corrected
                Formula = corrected;
                return;
            }

            if (FormulaOriginal.StartsWith(corrected, StringComparison.OrdinalIgnoreCase))
            {
                // Likely encountered an error, but the only changes before the error were maybe capitalization
                Formula = corrected + FormulaOriginal.Substring(corrected.Length);
                return;
            }

            // Changes that are not due to capitalization. Could be due to '0's or whitespace added or removed
            var correctedPosition = 0;
            var output = "";

            for (var i = 0; i < FormulaOriginal.Length; i++)
            {
                if (correctedPosition >= corrected.Length)
                {
                    output += FormulaOriginal.Substring(i);
                    break;
                }

                if (FormulaOriginal.Substring(i, 1).Equals(corrected.Substring(correctedPosition, 1), StringComparison.OrdinalIgnoreCase))
                {
                    output += corrected[correctedPosition];
                    correctedPosition++;
                }
                else if (char.IsWhiteSpace(FormulaOriginal[i]) || FormulaOriginal[i] == '0')
                {
                    // Whitespace or '0' in original was removed, keep it
                    output += FormulaOriginal[i];
                }
                else if (char.IsWhiteSpace(corrected[correctedPosition]) || corrected[correctedPosition] == '0')
                {
                    // Add whitespace or '0', skip it, and repeat i.
                    correctedPosition++;
                    i--;
                }
            }

            Formula = output;
        }

        /// <summary>
        /// Replaces <see cref="Formula"/> with the corrected formula (if no parsing errors occurred); otherwise does a smart replace of the formula
        /// </summary>
        public void ReplaceFormulaWithCorrected()
        {
            if (HasError)
            {
                UpdateFormula();
                return;
            }

            Formula = FormulaCorrected;
        }

        internal void AddAbbreviationUsage(string abbreviation, int position)
        {
            if (!AbbreviationUsage.ContainsKey(abbreviation))
            {
                AbbreviationUsage.Add(abbreviation, position);
            }
        }
    }

    /// <summary>
    /// Data for a section of a formula. Usually only one of these object for a formula; the main exception is with subtraction.
    /// </summary>
    [ComVisible(false)]
    internal class FormulaData
    {
        /// <summary>
        /// Computation stats for this section of the formula
        /// </summary>
        public ComputationStats Stats { get; } = new ComputationStats();

        /// <summary>
        /// Component breakdown for this section of the formula
        /// </summary>
        internal FormulaComponentGroup Components { get; } = new FormulaComponentGroup();

        /// <summary>
        /// Standard deviation (really 'variance') sum for this section of the formula
        /// </summary>
        public double StDevSum { get; set; } = 0;

        public override string ToString()
        {
            return Components.ToString();
        }
    }

    [ComVisible(false)]
    internal class FormulaComponent
    {
        /// <summary>
        /// Formula, non-corrected.
        /// </summary>
        public virtual string FormulaOriginal => PrefixChars + SymbolOriginal + SuffixChars;

        /// <summary>
        /// Corrected formula for component
        /// </summary>
        public virtual string FormulaCorrected => PrefixChars + SymbolCorrected + SuffixChars;

        /// <summary>
        /// Symbol, as it appears in the supplied formula
        /// </summary>
        public virtual string SymbolOriginal { get; set; } = "";

        /// <summary>
        /// Symbol, as stored in MWT (could also be expanded abbreviation)
        /// </summary>
        public virtual string SymbolCorrected { get; set; } = "";

        /// <summary>
        /// Element isotope (displayed as a superscript prefix, with '^')
        /// </summary>
        public double Isotope { get; set; }

        /// <summary>
        /// Element/component count (usually displayed as a subscript suffix)
        /// </summary>
        public double Count { get; set; }

        /// <summary>
        /// True if this component is an element (used in conjunction with <see cref="SymbolReference"/>)
        /// </summary>
        public bool IsElement { get; }

        /// <summary>
        /// SymbolReference index (used in conjunction with <see cref="IsElement"/> to look up elements or abbreviations)
        /// </summary>
        public int SymbolReference { get; }

        /// <summary>
        /// Prefix characters (leading coefficient or isotope, also includes opening parenthesis/brace/bracket)
        /// </summary>
        public string PrefixChars { get; set; }

        /// <summary>
        /// Suffix characters (component count, also includes closing parenthesis/brace/bracket)
        /// </summary>
        public string SuffixChars { get; set; }

        public FormulaComponent(bool isElement = false, int symbolReference = -1)
        {
            Isotope = 0;
            Count = 1;
            IsElement = isElement;
            SymbolReference = symbolReference;
            PrefixChars = "";
            SuffixChars = "";
        }

        public override string ToString()
        {
            return FormulaOriginal;
        }
    }

    [ComVisible(false)]
    internal class FormulaComponentGroup : FormulaComponent
    {
        private readonly List<FormulaComponent> components = new List<FormulaComponent>();

        /// <summary>
        /// List of components in this group
        /// </summary>
        public IReadOnlyList<FormulaComponent> Components => components;

        /// <summary>
        /// Leading coefficient for the element/component (prefix, primarily used with hydrates)
        /// </summary>
        public double LeadingCoefficient { get; set; }

        /// <summary>
        /// Tracks if this component group or a parent group is enclosed by square brackets (that are not being treated as parentheses)
        /// </summary>
        public bool InsideBrackets { get; private set; }

        /// <summary>
        /// If true, this is a component group immediately following a 'dash'; stored to prevent a subsequent 'dash' at the same level from being included in this component group.
        /// </summary>
        public bool RightOfDash { get; }

        /// <summary>
        /// Count of Carbon or Silicon atoms in this immediate group that are not part of an abbreviation. Does not include Carbon or Silicon atom counts for sub-groups.
        /// </summary>
        public int LoneCarbonSiliconCount { get; set; }

        /// <summary>
        /// Symbol (actually formula), as it appears in the supplied formula
        /// </summary>
        public override string SymbolOriginal
        {
            get => string.Join("", Components.Select(x => x.FormulaOriginal));
            set { } // No "set" allowed.
        }

        /// <summary>
        /// Corrected formula for component
        /// </summary>
        public override string SymbolCorrected
        {
            get => string.Join("", Components.Select(x => x.FormulaCorrected));
            set { } // No "set" allowed.
        }

        public FormulaComponentGroup(bool insideBrackets = false, bool rightOfDash = false)
        {
            LeadingCoefficient = 1;
            InsideBrackets = insideBrackets;
            RightOfDash = rightOfDash;
            LoneCarbonSiliconCount = 0;
        }

        /// <summary>
        /// Add a component to this component group. Also handles cascading <see cref="InsideBrackets"/> to child components.
        /// </summary>
        /// <param name="component"></param>
        public void Add(FormulaComponent component)
        {
            if (component is FormulaComponentGroup fcg)
            {
                // brackets inside brackets isn't allowed. But, only update if InsideBrackets is true (i.e., don't accidentally change it to false).
                if (InsideBrackets)
                {
                    fcg.InsideBrackets = InsideBrackets;
                }

                // "a-2b-3c" means "a AND 2b AND 3c", not "a AND 2(b AND 3c)". But - allow specifying "a-2(b-3c)"
                // To do that, we can't forward RightOfDash to sub-components
                //fcg.RightOfDash = RightOfDash;
            }

            components.Add(component);
        }

        public override string ToString()
        {
            return FormulaOriginal;
        }
    }
}
