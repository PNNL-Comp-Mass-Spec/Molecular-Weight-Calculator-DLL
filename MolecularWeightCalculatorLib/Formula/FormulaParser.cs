using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public class FormulaParser
    {
        // Ignore Spelling: Alph, UniMod

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elementTools"></param>
        public FormulaParser(ElementAndMassTools elementTools)
        {
            ComputationOptions = elementTools.ComputationOptions;
            mElementTools = elementTools;
        }

        internal const char EMPTY_STRING_CHAR = '~';

        private readonly ElementAndMassTools mElementTools;
        private ElementsAndAbbrevs Elements => mElementTools.Elements;
        private Messages Messages => mElementTools.Messages;

        private FormulaParseData mLastFormulaParsed = new("");
        public FormulaOptions ComputationOptions { get; }

        private static readonly Regex mElementMatcher = new(@"(?<LeadingWhitespaceOrIsotope>^|\s+|\^[0-9.]+)(?<Element>[a-z]+)\((?<ElementCount>[0-9]+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex mIsotopeMatcher = new(@"(?<LeadingWhitespace>^|\s+)(?<IsotopeMass>\d+)(?<Element>[a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex mNegativeCountMatcher = new(@"(?<Element>[a-z]+)\((?<ElementCount>-\d+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Used when computing abbreviation masses, to trigger errors when abbreviation circular references occur (rather than infinite loops/recursion)
        private class AbbrevSymbolStack
        {
            private List<string> SymbolStack { get; }

            public AbbrevSymbolStack()
            {
                SymbolStack = new List<string>(1);
            }

            /// <summary>
            /// Update the abbreviation symbol stack
            /// </summary>
            /// <param name="symbol"></param>
            public void Add(string symbol)
            {
                SymbolStack.Add(symbol);
            }

            /// <summary>
            /// Update the abbreviation symbol stack
            /// </summary>
            public void RemoveMostRecent()
            {
                if (SymbolStack.Count > 0)
                {
                    SymbolStack.RemoveAt(SymbolStack.Count - 1);
                }
            }

            /// <summary>
            /// Checks for presence of <paramref name="symbol"/>
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns>True if <paramref name="symbol"/> exists in the symbol stack</returns>
            public bool IsPresent(string symbol)
            {
                foreach (var symbolStored in SymbolStack)
                {
                    if (symbolStored.Equals(symbol, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void CheckCaution(string formulaExcerpt, FormulaParseData data)
        {
            for (var length = 0; length < ElementsAndAbbrevs.MAX_ABBREV_LENGTH; length++)
            {
                if (length > formulaExcerpt.Length)
                    break;

                var test = formulaExcerpt.Substring(0, length);
                var newCaution = Messages.LookupCautionStatement(test);
                if (!string.IsNullOrEmpty(newCaution))
                {
                    data.CautionDescriptionList.Add(newCaution);
                    break;
                }
            }
        }

        /// <summary>
        /// Compute the weight of a formula (or abbreviation)
        /// </summary>
        /// <param name="formula">Input</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(string formula)
        {
            return ComputeFormulaWeight(ref formula);
        }

        /// <summary>
        /// Compute the weight of a formula (or abbreviation)
        /// </summary>
        /// <param name="formula">Input</param>
        /// <param name="stdDev">Computed standard deviation</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(string formula, out double stdDev)
        {
            return ComputeFormulaWeight(ref formula, out stdDev);
        }

        /// <summary>
        /// Compute the weight of a formula (or abbreviation)
        /// </summary>
        /// <param name="formula">Input/output: updated by ParseFormula</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(ref string formula)
        {
            return ComputeFormulaWeight(ref formula, out _);
        }

        /// <summary>
        /// Compute the weight of a formula (or abbreviation)
        /// </summary>
        /// <param name="formula">Input/output: updated by ParseFormula</param>
        /// <param name="stdDev">Computed standard deviation</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(ref string formula, out double stdDev)
        {
            var data = (FormulaParseData)ParseFormula(formula, false);

            if (data.Error.ErrorId == 0)
            {
                stdDev = data.Stats.StandardDeviation;
                return data.Stats.TotalMass;
            }

            stdDev = 0;
            return -1;
        }

        /// <summary>
        /// Compute the weight of an abbreviation
        /// </summary>
        /// <param name="abbrev">Abbreviation to update with mass, stDev, and element counts</param>
        /// <returns>ErrorId: 0 if no error.</returns>
        internal int ComputeAbbrevWeight(AbbrevStatsData abbrev)
        {
            var formula = abbrev.Formula;

            var data = (FormulaParseData)ParseFormula(formula, false);

            // Set the used abbreviations here (regardless of success) for circular reference checks
            abbrev.SetUsedAbbreviations(data.AbbreviationUsage.Keys.ToList());

            if (data.AbbreviationUsage.Count > 0)
            {
                // Check for abbreviation circular references
                // Add self first (because somebody could try an immediate circular reference too)
                var abbrevSymbolStack = new AbbrevSymbolStack();
                abbrevSymbolStack.Add(abbrev.Symbol);

                var position = CheckForAbbreviationCircularRefs(data.AbbreviationUsage, abbrevSymbolStack);
                if (position >= 0)
                {
                    // Highlight the start of the abbreviation
                    data.Error.SetError(28, position, formula.Substring(position, 1));
                }
            }

            if (data.Error.ErrorId == 0)
            {
                var computationStats = data.Stats;

                abbrev.StdDev = computationStats.StandardDeviation;
                abbrev.Mass = computationStats.TotalMass;
                abbrev.AddUsedElements(computationStats.Elements);
            }

            return data.Error.ErrorId;
        }

        /// <summary>
        /// Check for circular references between abbreviations.
        /// </summary>
        /// <param name="abbrevsToCheck">Key is abbreviation symbol, value is index where it first occurred (or '0' on recursive calls)</param>
        /// <param name="abbrevSymbolStack"></param>
        /// <returns>-1 if no circular reference, or &gt;=0 index in formula if one exists.</returns>
        private int CheckForAbbreviationCircularRefs(IReadOnlyDictionary<string, int> abbrevsToCheck, AbbrevSymbolStack abbrevSymbolStack)
        {
            if (abbrevsToCheck.Count == 0)
            {
                return -1;
            }

            var countToCheck = abbrevsToCheck.Count;
            var countChecked = 0;

            // Don't use the master symbols list here; it may not be updated yet.
            foreach (var storedAbbrev in Elements.AbbrevStats)
            {
                foreach (var abbrev in abbrevsToCheck)
                {
                    if (abbrev.Key.Equals(storedAbbrev.Symbol, StringComparison.OrdinalIgnoreCase))
                    {
                        if (abbrevSymbolStack.IsPresent(abbrev.Key))
                        {
                            return abbrev.Value;
                        }

                        abbrevSymbolStack.Add(abbrev.Key);
                        var position = CheckForAbbreviationCircularRefs(storedAbbrev.AbbreviationsUsed.ToDictionary(x => x, _ => 0), abbrevSymbolStack);
                        if (position >= 0)
                        {
                            return abbrev.Value;
                        }
                        abbrevSymbolStack.RemoveMostRecent();

                        countChecked++;
                        break;
                    }
                }

                if (countChecked >= countToCheck)
                {
                    break;
                }
            }

            return -1;
        }

        /// <summary>
        /// Compute percent composition of the elements defined in <paramref name="computationStats"/>
        /// </summary>
        /// <param name="computationStats">Input/output</param>
        public void ComputePercentComposition(ComputationStats computationStats)
        {
            // Determine the number of elements in the formula
            for (var atomicNumber = 1; atomicNumber <= ElementsAndAbbrevs.ELEMENT_COUNT; atomicNumber++)
            {
                var elementPctComp = computationStats.PercentCompositions[atomicNumber];
                if (computationStats.TotalMass > 0d)
                {
                    var element = Elements.ElementStats[atomicNumber];
                    var elementUse = computationStats.Elements[atomicNumber];

                    var elementTotalMass = elementUse.Count * element.Mass + elementUse.IsotopicCorrection;

                    // Percent is the percent composition
                    var percentComp = elementTotalMass / computationStats.TotalMass * 100.0d;
                    elementPctComp.PercentComposition = percentComp;

                    // Calculate standard deviation
                    double stdDeviation;
                    if (Math.Abs(elementUse.IsotopicCorrection - 0d) < float.Epsilon)
                    {
                        // No isotopic mass correction factor exists
                        stdDeviation = percentComp * Math.Sqrt(Math.Pow(element.Uncertainty / element.Mass, 2d) + Math.Pow(computationStats.StandardDeviation / computationStats.TotalMass, 2d));
                    }
                    else
                    {
                        // Isotopic correction factor exists, assume no error in it
                        stdDeviation = percentComp * Math.Sqrt(Math.Pow(computationStats.StandardDeviation / computationStats.TotalMass, 2d));
                    }

                    if (Math.Abs(elementTotalMass - computationStats.TotalMass) < float.Epsilon && Math.Abs(percentComp - 100d) < float.Epsilon)
                    {
                        stdDeviation = 0d;
                    }

                    elementPctComp.StdDeviation = stdDeviation;
                }
                else
                {
                    elementPctComp.PercentComposition = 0d;
                    elementPctComp.StdDeviation = 0d;
                }
            }
        }

        /// <summary>
        /// Converts <paramref name="formula"/> to its corresponding empirical formula
        /// </summary>
        /// <param name="formula"></param>
        /// <returns>The empirical formula, or -1 if an error</returns>
        public string ConvertFormulaToEmpirical(string formula)
        {
            // Call ParseFormula to compute the formula's mass and fill computationStats
            var data = (FormulaParseData)ParseFormula(formula);

            if (data.Error.ErrorId == 0)
            {
                var stats = data.Stats;
                var elementsUsed = new Dictionary<int, IElementUseStats>();
                for (var i = 1; i <= ElementsAndAbbrevs.ELEMENT_COUNT; i++)
                {
                    if (stats.Elements[i].Used)
                    {
                        elementsUsed.Add(i, stats.Elements[i]);
                    }
                }

                return ConvertFormulaToEmpirical(elementsUsed);
            }

            return (-1).ToString();
        }

        /// <summary>
        /// Converts <paramref name="elementCounts"/> to its corresponding empirical formula
        /// </summary>
        /// <param name="elementCounts">Dictionary where keys are atomic number and values are statistics for that element</param>
        /// <returns>The empirical formula, or -1 if an error</returns>
        public string ConvertFormulaToEmpirical(IReadOnlyDictionary<int, IElementUseStats> elementCounts)
        {
            if (elementCounts.Count == 0)
            {
                return (-1).ToString();
            }

            // Convert to empirical formula
            var empiricalFormula = string.Empty;

            // Carbon first, then hydrogen, then the rest alphabetically
            // ElementAlph is already sorted properly as 0:{'C',6}, 1:{'H',1}, then alphabetically
            foreach (var atomicNumber in Elements.ElementAlph.Select(x => x.Value))
            {
                if (elementCounts.TryGetValue(atomicNumber, out var stats))
                {
                    // Only display the element if it's in the formula
                    var elementCount = stats.Count;
                    var isotopesCount = 0.0;
                    foreach (var isotope in stats.IsotopesUsed.OrderBy(x => x.Mass))
                    {
                        isotopesCount += isotope.Count;
                        if (Math.Abs(isotope.Count - 1.0) < float.Epsilon)
                        {
                            empiricalFormula += $"^{isotope.Mass}{Elements.ElementStats[atomicNumber].Symbol}";
                        }
                        else if (isotope.Count > 0)
                        {
                            empiricalFormula += $"^{isotope.Mass}{Elements.ElementStats[atomicNumber].Symbol}{isotope.Count}";
                        }
                    }

                    elementCount -= isotopesCount;
                    if (Math.Abs(elementCount - 1.0) < float.Epsilon)
                    {
                        empiricalFormula += Elements.ElementStats[atomicNumber].Symbol;
                    }
                    else if (elementCount > 0)
                    {
                        empiricalFormula += Elements.ElementStats[atomicNumber].Symbol + elementCount;
                    }
                }
            }

            return empiricalFormula;
        }

        /// <summary>
        /// Expands abbreviations in formula to their elemental equivalent
        /// </summary>
        /// <param name="formula"></param>
        /// <returns>Returns the result, or -1 if an error</returns>
        public string ExpandAbbreviationsInFormula(string formula)
        {
            // Call ExpandAbbreviationsInFormula to compute the formula's mass
            var data = (FormulaParseData)ParseFormula(formula, true);

            if (data.Error.ErrorId == 0)
            {
                return data.Formula;
            }

            return (-1).ToString();
        }

        [Obsolete("Use value from IFormulaParseData.CautionDescription instead")]
        public string GetCautionDescription()
        {
            return mLastFormulaParsed.CautionDescription;
        }

        [Obsolete("Use value from IFormulaParseData.ErrorData.ErrorDescription instead")]
        public string GetErrorDescription()
        {
            if (mLastFormulaParsed.HasError)
            {
                return Messages.LookupMessage(mLastFormulaParsed.Error.ErrorId);
            }

            return string.Empty;
        }

        // TODO: [Obsolete("Use value from IFormulaParseData.ErrorData.ErrorId instead")]
        public int GetErrorId()
        {
            return mLastFormulaParsed.Error.ErrorId;
        }

        // TODO: [Obsolete("Use value from IFormulaParseData.ErrorData.ErrorCharacter instead")]
        public string GetErrorCharacter()
        {
            return mLastFormulaParsed.Error.ErrorCharacter;
        }

        // TODO: [Obsolete("Use value from IFormulaParseData.ErrorData.ErrorPosition instead")]
        public int GetErrorPosition()
        {
            return mLastFormulaParsed.Error.ErrorPosition;
        }

        /// <summary>
        /// Determines the molecular weight and elemental composition of <paramref name="formula"/>
        /// </summary>
        /// <param name="formula">Input/output: formula to parse</param>
        /// <param name="computationStats">Output: additional information about the formula</param>
        /// <param name="expandAbbreviations"></param>
        /// <param name="valueForUnknown"></param>
        /// <returns>Computed molecular weight if no error; otherwise -1</returns>
        /// <remarks>
        /// ErrorParams will hold information on errors that occur (previous errors are cleared when this function is called)
        /// Use ComputeFormulaWeight if you only want to know the weight of a formula (it calls this function)
        /// </remarks>
        [Obsolete("Change to use ParseFormula instead.")]
        public double ParseFormula(
            ref string formula,
            out ComputationStats computationStats,
            bool expandAbbreviations = false,
            double valueForUnknown = 1)
        {
            var data = ParseFormula(formula, expandAbbreviations, valueForUnknown);

            formula = data.Formula;
            computationStats = data.Stats;

            return data.Stats.TotalMass;
        }

        /// <summary>
        /// Determines the molecular weight and elemental composition of <paramref name="formula"/>
        /// </summary>
        /// <param name="formula">Input/output: formula to parse</param>
        /// <param name="expandAbbreviations"></param>
        /// <param name="valueForUnknown"></param>
        /// <returns>Computed molecular weight if no error; otherwise -1</returns>
        /// <remarks>
        /// ErrorParams will hold information on errors that occur (previous errors are cleared when this function is called)
        /// Use ComputeFormulaWeight if you only want to know the weight of a formula (it calls this function)
        /// </remarks>
        public IFormulaParseData ParseFormula(
            string formula,
            bool expandAbbreviations = false,
            double valueForUnknown = 1)
        {
            var formulaData = new FormulaParseData(formula ?? string.Empty);

            try
            {
                if (string.IsNullOrWhiteSpace(formula))
                {
                    return formulaData;
                }

                // Parse the formula into individual elements and abbreviations
                ParseFormulaComponents(formulaData.FormulaOriginal, formulaData, valueForUnknown);

                // Replace the working formula with a cleaned formula (removes whitespace and unsupported characters, unless an error occurred when parsing)
                formulaData.ReplaceFormulaWithCorrected();

                // Compute the mass of the formula (also determines the total element counts (and isotopic correction) of the formula
                ComputeFormulaMass(formulaData);
                if (expandAbbreviations)
                {
                    ParseFormulaExpandAbbreviations(formulaData);
                    // Replace the working formula with the updated formula (no abbreviations)
                    formulaData.ReplaceFormulaWithCorrected();
                }

                mLastFormulaParsed = formulaData;

                if (formulaData.HasError)
                {
                    formulaData.Error.ErrorDescription = Messages.LookupMessage(formulaData.Error.ErrorId);
                }
            }
            catch (Exception ex)
            {
                mElementTools.GeneralErrorHandler("FormulaParser.ParseFormula", ex);
            }

            return formulaData;
        }

        /// <summary>
        /// Parse a formula into its components (elements and abbreviations)
        /// </summary>
        /// <param name="formula">Formula (or segment) to parse</param>
        /// <param name="data">Formula parsing data object to track errors and subtraction information</param>
        /// <param name="valueForUnknown">Value to use for '?' if present</param>
        /// <param name="components">'null' for calls from other methods (this parameter is used for recursive calls)</param>
        /// <param name="prevPosition">Should only be used for recursive calls; position tracking for error reporting</param>
        /// <param name="parenDepth">Should only be used for recursive calls</param>
        private void ParseFormulaComponents(string formula, FormulaParseData data, double valueForUnknown = 1, FormulaComponentGroup components = null, int prevPosition = 0, int parenDepth = 0)
        {
            // If the formula contains UniMod style element counts, convert them
            formula = ConvertFormulaNotation(formula);

            if (formula.Contains(">"))
            {
                // Formula has the formula subtraction symbol
                // Can have multiple '>' symbols in a formula (processed right-to-left), but '>' cannot occur within brackets, etc.
                var blocks = formula.Split('>');

                // Add sufficient FormulaData objects for the split.
                data.FormulaSections.AddRange(Enumerable.Range(0, blocks.Length - 1).Select(_ => new FormulaData()).ToArray());

                var runningLength = 0;
                for (var i = 0; i < blocks.Length; i++)
                {
                    var block = blocks[i];
                    if (string.IsNullOrWhiteSpace(block))
                    {
                        // Empty subtraction block
                        var position = Math.Min(runningLength, formula.Length - 1);
                        data.Error.SetError(30, position, formula.Substring(position, 1));
                        break;
                    }

                    ParseFormulaComponents(block, data, valueForUnknown, data.FormulaSections[i].Components, runningLength);
                    // Add block.Length to get the index of '>', and 1 to get the first index after it.
                    runningLength += block.Length + 1;

                    if (data.HasError)
                    {
                        break;
                    }
                }

                return;
            }

            // Formula does not contain the formula subtraction symbol (>)

            // Parse it
            // set components variable (if null)
            components ??= data.Components;

            // Determine the components of the formula.
            // A component might be a single element or a compound grouped by (), {}, or [], or a couple other cases.
            // Each iteration processes one component, using recursion if the component is a compound/group
            for (var charIndex = 0; charIndex < formula.Length; charIndex++)
            {
                var currentChar = formula[charIndex];
                var nextRemnant = charIndex + 1 < formula.Length ? formula.Substring(charIndex + 1) : "";
                var charPosition = prevPosition + charIndex;

                var isIsotope = false;
                var isotope = 0.0;
                var isotopeText = string.Empty;
                if (currentChar == '^') // ^ (caret)
                {
                    // Handle ^ first, separately, since the goal is to handle a single component with each iteration of the loop.
                    var caretNum = ParseNumberAtStart(nextRemnant, out var caretNumLength, data.Error, charPosition + 1);

                    // ErrorID 12: used if there is only a decimal point. Treat this as an isotope error instead (ErrorID 20, set below)
                    if (data.HasError && data.Error.ErrorId != 12)
                        return;

                    var nextCharIndex = charIndex + 1 + caretNumLength;
                    var charAsc = 0;
                    if (nextCharIndex < formula.Length)
                    {
                        charAsc = formula[nextCharIndex];
                    }

                    if (caretNum >= 0d && caretNumLength > 0)
                    {
                        if (charAsc is >= 'A' and <= 'Z' or >= 'a' and <= 'z') // Uppercase A to Z and lowercase a to z
                        {
                            isIsotope = true;
                            isotope = caretNum;
                            isotopeText = formula.Substring(charIndex, caretNumLength + 1);
                            charIndex += caretNumLength + 1;

                            currentChar = formula[charIndex];
                            nextRemnant = charIndex + 1 < formula.Length ? formula.Substring(charIndex + 1) : "";
                        }
                        else
                        {
                            // No letter after isotopic mass
                            // Highlights the last digit of the number
                            data.Error.SetError(22, charPosition + caretNumLength, formula.Substring(charIndex + caretNumLength, 1));
                        }
                    }
                    else
                    {
                        // Adjacent number is < 0 or not present
                        // Record error
                        if (charIndex < formula.Length - 1 && formula.Substring(charIndex + 1, 1) == "-")
                        {
                            // Negative number following caret
                            // Highlights the negative sign
                            data.Error.SetError(23, charPosition + 1, formula.Substring(charIndex + 1, 1));
                        }
                        else
                        {
                            // No number following caret
                            // Highlights the caret
                            data.Error.SetError(20, charPosition, formula.Substring(charIndex, 1));
                        }
                    }

                    if (data.HasError)
                        return;

                    charPosition = prevPosition + charIndex;
                }

                var currentRemnant = formula.Substring(charIndex);

                if (ComputationOptions.CaseConversion != CaseConversionMode.ExactCase)
                {
                    currentRemnant = char.ToUpper(currentChar) + nextRemnant;
                }

                // Check for needed caution statements
                CheckCaution(currentRemnant, data);
                FormulaComponent component = null;

                // Take action based on currentChar
                switch (currentChar)
                {
                    case '[' when ComputationOptions.BracketsAsParentheses:
                    case '(':
                    case '{': // ( or {    Record its position
                        var groupComponent = new FormulaComponentGroup
                        {
                            PrefixChars = currentChar.ToString()
                        };
                        components.Add(groupComponent);
                        component = groupComponent;
                        ParseFormulaComponents(nextRemnant, data, valueForUnknown, groupComponent, prevPosition + charIndex + 1, parenDepth + 1);
                        var groupSegmentLength = groupComponent.FormulaOriginal.Length;

                        if (charIndex + groupSegmentLength >= formula.Length)
                        {
                            // Missing closing parenthesis, highlight the opening one.
                            data.Error.SetError(3, charPosition, formula.Substring(charIndex, 1));
                            return;
                        }

                        var parenCloseChar = formula[charIndex + groupSegmentLength];
                        if (!data.HasError && (
                            currentChar == '(' && parenCloseChar != ')' ||
                            currentChar == '{' && parenCloseChar != '}' ||
                            currentChar == '[' && parenCloseChar != ']'))
                        {
                            // Mismatched parentheses/curly braces: highlight the closing parenthesis/curly brace
                            data.Error.SetError(4, charPosition + groupSegmentLength, parenCloseChar.ToString());
                        }

                        groupComponent.SuffixChars += parenCloseChar;

                        charIndex += groupSegmentLength;

                        break;

                    case ']' when ComputationOptions.BracketsAsParentheses:
                    case ')':
                    case '}': // )    Repeat a section of a formula
                        if (parenDepth == 0)
                        {
                            // Should have been skipped
                            // Unmatched closing parenthesis, highlight it.
                            data.Error.SetError(4, charPosition, formula.Substring(charIndex, 1));
                        }
                        else
                        {
                            // End of formula group; return to caller to process the closing parenthesis and continue
                            charIndex = formula.Length;
                        }

                        break;

                    case '[': // [
                        if (components.InsideBrackets)
                        {
                            // No Nested brackets, highlight this opening bracket
                            data.Error.SetError(16, charPosition, formula.Substring(charIndex, 1));
                        }

                        var nextChar = nextRemnant.Length > 1 ? nextRemnant[0] : EMPTY_STRING_CHAR;
                        int bracketNumLength;
                        double leadingCoefficient;
                        if (nextChar == '?')
                        {
                            leadingCoefficient = valueForUnknown;
                            bracketNumLength = 1;
                        }
                        else
                        {
                            leadingCoefficient = ParseNumberAtStart(nextRemnant, out bracketNumLength, data.Error, charPosition + 1);
                        }

                        // ErrorID 12: used if there is only a decimal point. Treat this as a bracket error instead (same error ID, different position/char, set below)
                        if (data.HasError && data.Error.ErrorId != 12)
                            return;

                        if (leadingCoefficient <= 0d || bracketNumLength == 0)
                        {
                            // No number following opening bracket, or a negative number there
                            // Highlight the opening bracket.
                            data.Error.SetError(12, charPosition, formula.Substring(charIndex, 1));
                        }
                        else
                        {
                            // Coefficient for bracketed section.
                            var numEndIndex = charIndex + bracketNumLength;
                            var bracketComponent = new FormulaComponentGroup(insideBrackets: true)
                            {
                                PrefixChars = formula.Substring(charIndex, bracketNumLength + 1),
                                LeadingCoefficient = leadingCoefficient
                            };
                            components.Add(bracketComponent);
                            component = bracketComponent;
                            // formula.Substring(formula.Length) returns "", so we shouldn't encounter an error with this
                            ParseFormulaComponents(formula.Substring(numEndIndex + 1), data, valueForUnknown, bracketComponent, prevPosition + numEndIndex + 1, parenDepth);
                            var bracketSegmentLength = bracketComponent.FormulaOriginal.Length;

                            if (charIndex + bracketSegmentLength >= formula.Length)
                            {
                                // Missing closing bracket, highlight the opening bracket
                                data.Error.SetError(13, charPosition, formula.Substring(charIndex, 1));
                            }
                            else
                            {
                                charIndex += bracketSegmentLength;
                                bracketComponent.SuffixChars += formula[charIndex];
                            }
                        }

                        break;

                    case ']': // ]
                        if (components.InsideBrackets)
                        {
                            // End of block; return to caller to process the closing bracket and continue
                            charIndex = formula.Length;
                        }
                        else
                        {
                            // Unmatched closing bracket, highlight the opening bracket
                            data.Error.SetError(15, charPosition, formula.Substring(charIndex, 1));
                        }

                        break;

                    case '\u00B7': // Unicode "Middle Dot"
                    case '\u2019': // Unicode "Bullet Operator"
                    case '\u2022': // Unicode "Bullet"
                    case '\u2027': // Unicode "Hyphenation Point"
                    case '\u22C5': // Unicode "Dot Operator"
                    case '-': // -
                        // NOTE: Also handles leading coefficients at the beginning of the formula that don't start with '-'
                        if (components.RightOfDash)
                        {
                            // Return to caller for handling of the dash component as another component on the same level (not multiplicative)
                            return;
                        }

                        // Used to denote a leading coefficient
                        var dashLength = 1;
                        var numRemnant = nextRemnant;
                        if (currentChar is >= '0' and <= '9')
                        {
                            // handling fall-through from a leading coefficient number
                            numRemnant = currentRemnant;
                            dashLength = 0;
                        }

                        var dashValue = ParseNumberAtStart(numRemnant, out var dashNumLength, data.Error, charPosition + 1);

                        if (data.HasError)
                        {
                            return;
                        }

                        // recurse to process the remnant.
                        var dashComponent = new FormulaComponentGroup(rightOfDash: true)
                        {
                            PrefixChars = formula.Substring(charIndex, dashNumLength + dashLength)
                        };

                        var dashRemnant = numRemnant;
                        if (dashNumLength > 0)
                        {
                            if (Math.Abs(dashValue) < float.Epsilon)
                            {
                                // Cannot have 0 after a dash, highlight the 0
                                data.Error.SetError(5, charPosition + 1, nextRemnant.Substring(0, 1));
                            }

                            dashComponent.LeadingCoefficient = dashValue;
                            dashRemnant = numRemnant.Substring(dashNumLength);
                        }

                        components.Add(dashComponent);
                        component = dashComponent;

                        // Parse anything remaining; don't handle closing parenthesis here.
                        ParseFormulaComponents(dashRemnant, data, valueForUnknown, dashComponent, prevPosition + charIndex + dashNumLength + 1, parenDepth);

                        if (dashComponent.Components.Count == 0 && !data.HasError)
                        {
                            // No compounds after leading coefficient after dash, highlight the first digit in the number
                            data.Error.SetError(25, charPosition + 1, formula.Substring(charIndex + 1, dashNumLength));
                        }

                        // Subtract 1 for correctness here.
                        charIndex += dashComponent.FormulaOriginal.Length - 1;
                        break;

                    case ',':
                    case '.':
                    case >= '0' and <= '9': // . or , and Numbers 0 to 9
                        // They should only be encountered as a leading coefficient
                        // Should have been bypassed when the coefficient was processed
                        if (charIndex == 0 && prevPosition == 0)
                        {
                            // Handle the exact same as a '-'.
                            goto case '-';
                        }
                        else if (charIndex > 1 && NumberConverter.CDblSafe(formula.Substring(charIndex - 1, 1)) > 0d)
                        {
                            // Number too large (long), highlight the current character
                            data.Error.SetError(7, charPosition, formula.Substring(charIndex, 1));
                        }
                        else
                        {
                            // Misplaced number, highlight the current character
                            data.Error.SetError(14, charPosition, formula.Substring(charIndex, 1));
                        }

                        break;

                    case >= 'A' and <= 'Z':
                    case >= 'a' and <= 'z':
                    case '+':
                    case '_': // Uppercase A to Z and lowercase a to z, and the plus (+) sign, and the underscore (_)
                        var symbolMatchType = Elements.CheckElemAndAbbrev(currentRemnant, out var symbolReference);
                        switch (symbolMatchType)
                        {
                            case SymbolMatchMode.Element:
                                // Found an element
                                // SymbolReference is the elemental number
                                var elementStats = Elements.ElementStats[symbolReference];
                                var elementSymbolLength = elementStats.Symbol.Length;
                                if (elementSymbolLength == 0)
                                {
                                    // No elements in ElementStats yet
                                    // Set symbolLength to 1
                                    elementSymbolLength = 1;
                                }

                                var elementComponent = new FormulaComponent(true, symbolReference);
                                components.Add(elementComponent);
                                component = elementComponent;
                                if (isIsotope)
                                {
                                    elementComponent.PrefixChars = isotopeText;
                                    elementComponent.Isotope = isotope;
                                }

                                var formulaSymbol = formula.Substring(charIndex, elementSymbolLength);
                                elementComponent.SymbolOriginal = formulaSymbol;

                                if (ComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                {
                                    formulaSymbol = char.ToUpper(formulaSymbol[0]) + (formulaSymbol.Length > 1 ? formulaSymbol.Substring(1) : "");
                                }

                                elementComponent.SymbolCorrected = formulaSymbol;

                                charIndex += elementSymbolLength - 1;

                                break;

                            case SymbolMatchMode.Abbreviation:
                                // Found an abbreviation or amino acid
                                // SymbolReference is the abbrev or amino acid number
                                // Record the abbreviation length
                                var abbrevStats = Elements.AbbrevStats[symbolReference];
                                var abbrevSymbolLength = abbrevStats.Symbol.Length;

                                // Store the abbreviation for the circular reference checks (if parsing an abbreviation formula)
                                // Need to store this regardless of parsing success to prevent infinite loops if a circular reference does occur.
                                data.AddAbbreviationUsage(abbrevStats.Symbol, prevPosition + charIndex);

                                if (isIsotope && char.ToUpper(currentChar) == 'D' && nextRemnant[0] != 'y')
                                {
                                    // Isotopic mass used for Deuterium; highlight the current character/abbreviation
                                    data.Error.SetError(26, charPosition, formula.Substring(charIndex, abbrevSymbolLength));
                                }
                                else if (isIsotope)
                                {
                                    // Cannot have isotopic mass for an abbreviation; highlight the current character/abbreviation.
                                    data.Error.SetError(24, charPosition, formula.Substring(charIndex, abbrevSymbolLength));
                                }
                                else if (abbrevStats.InvalidSymbolOrFormula)
                                {
                                    // Cannot use an invalid abbreviation. Should only occur when parsing abbreviations
                                    data.Error.SetError(32, charPosition, formula.Substring(charIndex, abbrevSymbolLength));
                                }
                                else if (abbrevStats.ElementsUsed.Count == 0)
                                {
                                    // Cannot return success if the abbreviation has not yet been parsed, so set an error
                                    data.Error.SetError(31, charPosition, formula.Substring(charIndex, abbrevSymbolLength));
                                }
                                else
                                {
                                    // Parse abbreviation
                                    // Simply treat it like a formula surrounded by parentheses
                                    // Thus, find the number after the abbreviation, then call ParseFormulaRecursive, sending it the formula for the abbreviation
                                    // Update the abbrevSymbolStack before calling so that we can check for circular abbreviation references

                                    var abbrevComponent = new FormulaComponent(false, symbolReference);
                                    components.Add(abbrevComponent);
                                    component = abbrevComponent;

                                    var abbrevSymbol = formula.Substring(charIndex, abbrevSymbolLength);
                                    abbrevComponent.SymbolOriginal = abbrevSymbol;

                                    if (ComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                    {
                                        abbrevSymbol = char.ToUpper(abbrevSymbol[0]) + abbrevSymbol.Substring(1);
                                    }

                                    abbrevComponent.SymbolCorrected = abbrevSymbol;

                                    charIndex += abbrevSymbolLength - 1;
                                }

                                break;

                            default:
                                // Element not Found
                                // Unmatched character, highlight it.
                                data.Error.SetError(1, charPosition, formula.Substring(charIndex, 1));

                                break;
                        }

                        break;

                    case '^': // ^ (caret)
                        // Hitting this should not be possible, so just ignore it.
                        break;

                    case '?':
                        // ? for solver but no preceding bracket, highlight the current character
                        data.Error.SetError(18, charPosition, formula.Substring(charIndex, 1));
                        break;

                    // ReSharper disable once RedundantEmptySwitchSection
                    default:
                        // There shouldn't be anything else (except the ~ filler character). If there is, we'll just ignore it
                        break;
                }

                if (data.HasError)
                {
                    return;
                }

                if (component != null && formula.Length > charIndex + 1)
                {
                    var possibleNumber = formula.Substring(charIndex + 1);
                    charPosition = prevPosition + charIndex + 1;
                    var groupValue = ParseNumberAtStart(possibleNumber, out var groupNumLength, data.Error, charPosition);

                    if (groupNumLength > 0)
                    {
                        if (component.SuffixChars == "]" && !ComputationOptions.BracketsAsParentheses)
                        {
                            // Number following bracket, highlight the number
                            data.Error.SetError(11, charPosition, formula.Substring(charIndex + 1, 1));
                        }
                        else if (Math.Abs(groupValue) < float.Epsilon)
                        {
                            // Zero after element or abbreviation, highlight the 0
                            data.Error.SetError(5, charPosition, formula.Substring(charIndex + 1, 1));
                        }

                        component.Count = groupValue;
                        component.SuffixChars += formula.Substring(charIndex + 1, groupNumLength);
                        charIndex += groupNumLength;
                    }
                }

                if (data.HasError)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Computes the mass for the formula, storing it in <paramref name="formulaData"/>.Stats
        /// </summary>
        /// <param name="formulaData">Formula parsing data with formula parsed into components</param>
        private void ComputeFormulaMass(FormulaParseData formulaData)
        {
            if (formulaData.HasError)
            {
                return;
            }

            DetermineTotalElementCounts(formulaData);

            if (formulaData.HasError)
            {
                return;
            }

            var stats = formulaData.Stats;

            // Compute the standard deviation
            stats.StandardDeviation = Math.Sqrt(formulaData.FormulaSections.First().StDevSum);

            // Compute the total molecular weight
            stats.TotalMass = 0d; // Reset total weight of compound to 0 so we can add to it
            for (var atomicNumber = 1; atomicNumber <= ElementsAndAbbrevs.ELEMENT_COUNT; atomicNumber++)
            {
                // Increase total weight by multiplying the count of each element by the element's mass
                // In addition, add in the Isotopic Correction value
                stats.TotalMass = stats.TotalMass + Elements.ElementStats[atomicNumber].Mass * stats.Elements[atomicNumber].Count + stats.Elements[atomicNumber].IsotopicCorrection;
            }
        }

        /// <summary>
        /// Convert a formula from UniMod style to the style recognized by this software
        /// </summary>
        /// <param name="formula"></param>
        /// <returns>Updated formula</returns>
        /// <remarks>
        /// Examples:
        /// H(4) 13C(3) O             -> H4 ^13C3 O
        /// H(3) C(6) N O             -> H3 C6 N O
        /// H(12) C(6) 13C N 15N 18O  -> H12 C6 ^13C N ^15N ^18O
        /// </remarks>
        private static string ConvertFormulaNotation(string formula)
        {
            var negativeCountMatches = mNegativeCountMatcher.Matches(formula);

            string updatedFormula;

            if (negativeCountMatches.Count > 0)
            {
                var formulaToSubtract = new StringBuilder();
                foreach (Match item in negativeCountMatches)
                {
                    var elementCount = int.Parse(item.Groups["ElementCount"].Value);
                    formulaToSubtract.AppendFormat("{0}{1}", item.Groups["Element"].Value, Math.Abs(elementCount));
                }

                var updatedFormula1 = mNegativeCountMatcher.Replace(formula, string.Empty);

                if (string.IsNullOrWhiteSpace(updatedFormula1))
                {
                    // This modification only subtracts a mass
                    return "^0H>" + formulaToSubtract;
                }

                updatedFormula = updatedFormula1 + ">" + formulaToSubtract;
            }
            else
            {
                updatedFormula = formula;
            }

            var updatedFormula2 = mIsotopeMatcher.Match(updatedFormula).Success ?
                                      mIsotopeMatcher.Replace(updatedFormula, @"${LeadingWhitespace}^${IsotopeMass}${Element}") :
                                      updatedFormula;

            return mElementMatcher.Match(updatedFormula2).Success ?
                                      mElementMatcher.Replace(updatedFormula2, @"${LeadingWhitespaceOrIsotope}${Element}${ElementCount}") :
                                      updatedFormula2;

        }

        /// <summary>
        /// Determines the total element counts for a parsed formula, as well as the total difference in mass for an element when an isotope is specified.
        /// </summary>
        /// <param name="formulaData">Formula parsing data with formula parsed into components</param>
        /// <param name="currentFormula">'null' for calls from other methods (this parameter is used for recursive calls)</param>
        /// <param name="currentGroup">'null' for calls from other methods (this parameter is used for recursive calls)</param>
        /// <param name="prevPosition">Should only be used for recursive calls; position tracking for error reporting</param>
        /// <param name="multiplier">Should only be used for recursive calls</param>
        private void DetermineTotalElementCounts(FormulaParseData formulaData, FormulaData currentFormula = null, FormulaComponentGroup currentGroup = null, int prevPosition = 0, double multiplier = 1)
        {
            if (formulaData.HasError)
            {
                // Just exit if an error has occurred parsing this formula
                return;
            }

            // Only enter this block if there is subtraction present, and it's not a recursive call.
            if (currentFormula == null && formulaData.FormulaSections.Count > 1)
            {
                // Handle subtraction; process right-to-left
                var previousStartPosition = formulaData.Formula.Length;
                FormulaData previousBlock = null;
                for (var i = formulaData.FormulaSections.Count - 1; i >= 0; i--)
                {
                    var currentBlock = formulaData.FormulaSections[i];
                    var startPosition = previousStartPosition - currentBlock.Components.SymbolOriginal.Length;
                    DetermineTotalElementCounts(formulaData, currentBlock, currentBlock.Components, startPosition);

                    // Perform subtraction...
                    if (previousBlock != null)
                    {
                        var cStats = currentBlock.Stats;
                        var pStats = previousBlock.Stats;
                        // Update computationStats by subtracting the atom counts of the first half minus the second half
                        // If any atom counts become < 0 then, then raise an error
                        for (var atomicNumber = 1; atomicNumber <= ElementsAndAbbrevs.ELEMENT_COUNT; atomicNumber++)
                        {
                            if (pStats.Elements[atomicNumber].Count == 0)
                                continue;

                            var element = cStats.Elements[atomicNumber];
                            if (Elements.ElementStats[atomicNumber].Mass * element.Count + element.IsotopicCorrection >=
                                Elements.ElementStats[atomicNumber].Mass * pStats.Elements[atomicNumber].Count + pStats.Elements[atomicNumber].IsotopicCorrection)
                            {
                                element.Used = true;
                                element.Count -= pStats.Elements[atomicNumber].Count;
                                if (element.Count < 0d)
                                {
                                    // This shouldn't happen
                                    Console.WriteLine(".Count is less than 0 in DetermineTotalElementCounts; this shouldn't happen");
                                    element.Count = 0d;
                                }

                                if (Math.Abs(pStats.Elements[atomicNumber].IsotopicCorrection) > float.Epsilon)
                                {
                                    element.IsotopicCorrection -= pStats.Elements[atomicNumber].IsotopicCorrection;
                                }
                            }
                            else
                            {
                                // Invalid Formula; raise error
                                // Highlight the subtraction symbol (easy to report the whole subtraction string too)
                                formulaData.Error.SetError(30, previousStartPosition, ">");
                                return;
                            }

                            if (formulaData.Error.ErrorId != 0)
                                break;
                        }

                        // Adjust the overall charge
                        cStats.Charge -= pStats.Charge;
                    }

                    previousStartPosition = startPosition - 1; // subtract 1 for '>'
                    previousBlock = currentBlock;
                }

                return;
            }

            // Null checks
            currentFormula ??= formulaData.FormulaSections.First();
            currentGroup ??= currentFormula.Components;

            var position = prevPosition;
            var prevElementSymbolReference = 0;
            var stats = currentFormula.Stats;
            var loneCarbonOrSilicon = 0;
            foreach (var component in currentGroup.Components)
            {
                if (component is FormulaComponentGroup fcg)
                {
                    // Recurse, and calculate
                    // Need to properly deal with the "carbonSiliconCount" and charge.
                    DetermineTotalElementCounts(formulaData, currentFormula, fcg, position, multiplier * fcg.LeadingCoefficient * fcg.Count);

                    // Correct charge
                    if (fcg.LoneCarbonSiliconCount > 0)
                    {
                        stats.Charge = (float)(stats.Charge - 2d * fcg.Count);
                        if (fcg.Count > 1d && fcg.LoneCarbonSiliconCount > 1)
                        {
                            stats.Charge = (float)(stats.Charge - 2d * (fcg.Count - 1d) * (fcg.LoneCarbonSiliconCount - 1));
                        }
                    }
                }
                else
                {
                    var symbolReference = component.SymbolReference;
                    // Calculate
                    if (component.IsElement)
                    {
                        var elementStats = Elements.ElementStats[symbolReference];
                        var element = stats.Elements[component.SymbolReference];
                        var addCount = multiplier * component.Count;
                        element.Count += addCount; // Increment element counting bin
                        element.Used = true; // Element is present tag
                        if (Math.Abs(component.Isotope) < double.Epsilon)
                        {
                            // No isotope specified
                            currentFormula.StDevSum += addCount * Math.Pow(elementStats.Uncertainty, 2);

                            // Compute charge
                            if (symbolReference == 1)
                            {
                                // Dealing with hydrogen
                                // Switch on the atomic numbers for metals
                                switch (prevElementSymbolReference)
                                {
                                    case 1:
                                    case >= 3 and <= 6:
                                    case >= 11 and <= 14:
                                    case >= 19 and <= 32:
                                    case >= 37 and <= 50:
                                    case >= 55 and <= 82:
                                    case >= 87 and <= 109:
                                        // Hydrogen is -1 with metals (non-halides)
                                        stats.Charge = (float)(stats.Charge + addCount * -1);
                                        break;

                                    default:
                                        stats.Charge = (float)(stats.Charge + addCount * elementStats.Charge);
                                        break;
                                }
                            }
                            else
                            {
                                stats.Charge = (float)(stats.Charge + addCount * elementStats.Charge);
                            }

                            if (symbolReference is 6 or 14)
                            {
                                // Sum up number lone C and Si (not in abbreviations)
                                loneCarbonOrSilicon = (int)Math.Round(loneCarbonOrSilicon + component.Count);
                            }
                        }
                        else
                        {
                            // Check to make sure isotopic mass is reasonable
                            double isoDifferenceTop = NumberConverter.CIntSafe(0.63d * symbolReference + 6d);
                            double isoDifferenceBottom = NumberConverter.CIntSafe(0.008d * Math.Pow(symbolReference, 2d) - 0.4d * symbolReference - 6d);
                            var caretValDifference = component.Isotope - symbolReference * 2;

                            if (caretValDifference >= isoDifferenceTop)
                            {
                                // Probably too high isotopic mass
                                formulaData.CautionDescriptionList.Add(Messages.LookupMessage(660) + ": " + elementStats.Symbol + " - " + component.Isotope + " " + Messages.LookupMessage(665) + " " + elementStats.Mass);
                            }
                            else if (component.Isotope < symbolReference)
                            {
                                // Definitely too low isotopic mass
                                formulaData.CautionDescriptionList.Add(Messages.LookupMessage(670) + ": " + elementStats.Symbol + " - " + symbolReference + " " + Messages.LookupMessage(675));
                            }
                            else if (caretValDifference <= isoDifferenceBottom)
                            {
                                // Probably too low isotopic mass
                                formulaData.CautionDescriptionList.Add(Messages.LookupMessage(662) + ": " + elementStats.Symbol + " - " + component.Isotope + " " + Messages.LookupMessage(665) + " " + elementStats.Mass);
                            }

                            // Put in isotopic correction factor

                            // Store information in .Isotopes[]
                            // Increment the isotope counting bin
                            element.AddIsotope(component.Isotope, addCount);

                            // Add correction amount to computationStats.Elements[SymbolReference].IsotopicCorrection
                            element.IsotopicCorrection += component.Isotope * addCount - elementStats.Mass * addCount;

                            // Assume no error in caret value, no need to change stdDevSum
                        }

                        prevElementSymbolReference = symbolReference;
                    }
                    else
                    {
                        // Handle Abbreviation
                        prevElementSymbolReference = 0;

                        // Record the abbreviation length
                        var abbrevStats = Elements.AbbrevStats[symbolReference];

                        // Use the defined charge for the abbreviation
                        // addCount is an abbreviation occurrence count
                        var addCount = multiplier * component.Count;
                        stats.Charge = (float)(stats.Charge + addCount * abbrevStats.Charge);

                        currentFormula.StDevSum += addCount * Math.Pow(abbrevStats.StdDev, 2);

                        // Add the element data from the abbreviation to the element tracking.
                        foreach (var item in abbrevStats.ElementsUsed)
                        {
                            stats.Elements[item.Key].Used = true;
                            stats.Elements[item.Key].Count += addCount * item.Value.Count;
                            stats.Elements[item.Key].IsotopicCorrection += addCount * item.Value.IsotopicCorrection;
                            foreach (var isotope in item.Value.IsotopesUsed)
                            {
                                stats.Elements[item.Key].AddIsotope(isotope.Mass, addCount * isotope.Count);
                            }
                        }
                    }
                }

                position += component.FormulaOriginal.Length;
            }

            if (loneCarbonOrSilicon > 1)
            {
                // Correct Charge for number of C and Si
                stats.Charge -= (loneCarbonOrSilicon - 1) * 2;
                currentGroup.LoneCarbonSiliconCount = loneCarbonOrSilicon;
            }
            else
            {
                currentGroup.LoneCarbonSiliconCount = 0;
            }
        }

        /// <summary>
        /// Expand abbreviations in the formula
        /// </summary>
        /// <param name="data">Formula parsing data object with formula parsed into components</param>
        /// <param name="components">'null' for calls from other methods (this parameter is used for recursive calls)</param>
        private void ParseFormulaExpandAbbreviations(FormulaParseData data, FormulaComponentGroup components = null)
        {
            if (data.HasError)
            {
                // Just exit if an error has occurred parsing this formula
                return;
            }

            if (data.FormulaSections.Count > 1 && components == null)
            {
                foreach (var section in data.FormulaSections)
                {
                    ParseFormulaExpandAbbreviations(data, section.Components);
                }

                return;
            }

            // Set value to the first components object if null
            components ??= data.Components;

            foreach (var component in components.Components)
            {
                if (component is FormulaComponentGroup fcg)
                {
                    ParseFormulaExpandAbbreviations(data, fcg);
                }
                else if (!component.IsElement)
                {
                    // Component is not an element, so it should be an abbreviation
                    // Expand the abbreviation.
                    var abbrevStats = Elements.AbbrevStats[component.SymbolReference];
                    var abbrevFormula = abbrevStats.Formula;
                    if (abbrevFormula.Contains(">"))
                    {
                        // Use empirical formula instead of formula with subtraction
                        abbrevFormula = ConvertFormulaToEmpirical(abbrevStats.ElementsUsed);
                    }

                    if (Math.Abs(component.Count - 1) >= double.Epsilon)
                    {
                        // There is a leading coefficient or non-'1' count; surround the formula with parentheses to maintain accuracy
                        abbrevFormula = "(" + abbrevFormula + ")";
                    }

                    // Store to the "corrected" symbol
                    component.SymbolCorrected = abbrevFormula;
                }
            }
        }

        /// <summary>
        /// Parse a number and log any errors to <paramref name="error"/>
        /// </summary>
        /// <param name="work">string for extracting a number; will extract all numerical characters from the start of the string, support the decimal separator</param>
        /// <param name="numLength">Length of the number string found at the start of <paramref name="work"/>; 0 if no number found or an error occurred</param>
        /// <param name="error"><see cref="ErrorDetails"/> object for storing any error that occurs</param>
        /// <param name="currentIndex">index of the first character of <paramref name="work"/> in the larger string being parsed, for reporting error location in <paramref name="error"/></param>
        /// <param name="allowNegative">If true, a leading '-' will not be treated as an error/no number found.</param>
        /// <returns>
        /// Parsed number if found
        /// If not a number, returns '0', with an error code set and numLength = 0</returns>
        private double ParseNumberAtStart(string work, out int numLength, ErrorDetails error, int currentIndex, bool allowNegative = false)
        {
            if (ComputationOptions.DecimalSeparator == default(char))
            {
                ComputationOptions.DecimalSeparator = MolecularWeightTool.DetermineDecimalPoint();
            }

            // Set numLength to -1 for now
            // If it doesn't get set to 0 (due to an error), it will get set to the
            // length of the matched number before exiting the sub
            numLength = -1;
            var foundNum = string.Empty;

            if (string.IsNullOrEmpty(work))
                work = EMPTY_STRING_CHAR.ToString();

            if (work[0] is < '0' or > '9' &&
                work[0] != ComputationOptions.DecimalSeparator &&
                !(work[0] == '-' && allowNegative))
            {
                numLength = 0; // No number found; not really an error; just return '0', and handle that externally
                return 0;
            }

            // Start of string is a number or a decimal point, or (if allowed) a negative sign
            foreach (var character in work)
            {
                if (char.IsDigit(character) ||
                    character == ComputationOptions.DecimalSeparator ||
                    allowNegative && character == '-')
                {
                    foundNum += character;
                }
                else
                {
                    break;
                }
            }

            if (foundNum.Length == 0 || foundNum == ComputationOptions.DecimalSeparator.ToString())
            {
                // No number at all or (more likely) no number after decimal point
                numLength = 0;
                // highlight the decimal point
                error.SetError(12, currentIndex, work.Substring(0, 1));
                return 0;
            }

            // Check for more than one decimal point (. or ,)
            var decPtCount = foundNum.Count(c => c == ComputationOptions.DecimalSeparator);

            if (decPtCount > 1)
            {
                // more than one decimal point count
                numLength = 0;
                // Error: More than one decimal point, highlight the whole bad number
                error.SetError(27, currentIndex, foundNum);
                return 0;
            }

            // TODO: Also check for more than one '-' when allowNegative is true.

            if (double.TryParse(foundNum, out var num))
            {
                numLength = foundNum.Length;
                return num;
            }

            // Error: General number error, highlight the entire failed string.
            error.SetError(14, currentIndex, foundNum);
            numLength = 0;

            return 0;
        }
    }
}
