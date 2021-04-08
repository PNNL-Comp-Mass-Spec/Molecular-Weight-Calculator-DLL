using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public class FormulaParser
    {
        public FormulaParser(ElementAndMassTools elementTools)
        {
            ComputationOptions = elementTools.ComputationOptions;
            mElementTools = elementTools;
        }

        internal const char EMPTY_STRING_CHAR = '~';

        private readonly ElementAndMassTools mElementTools;
        private ElementsAndAbbrevs Elements => mElementTools.Elements;
        private Messages Messages => mElementTools.Messages;

        private readonly ErrorDetails mErrorParams = new ErrorDetails();

        private ComputationStats mComputationStatsSaved = new ComputationStats();

        private string mCautionDescription;
        public FormulaOptions ComputationOptions { get; }

        // Used when computing abbreviation masses, to trigger errors when abbreviation circular references occur (rather than infinite loops/recursion)
        private class AbbrevSymbolStack
        {
            public List<short> SymbolReferenceStack { get; }

            public AbbrevSymbolStack()
            {
                SymbolReferenceStack = new List<short>(1);
            }

            /// <summary>
            /// Update the abbreviation symbol stack
            /// </summary>
            /// <param name="symbolReference"></param>
            public void Add(short symbolReference)
            {
                SymbolReferenceStack.Add(symbolReference);
            }

            /// <summary>
            /// Update the abbreviation symbol stack
            /// </summary>
            public void RemoveMostRecent()
            {
                if (SymbolReferenceStack.Count > 0)
                {
                    SymbolReferenceStack.RemoveAt(SymbolReferenceStack.Count - 1);
                }
            }

            /// <summary>
            /// Checks for presence of <paramref name="symbolReference"/>
            /// </summary>
            /// <param name="symbolReference"></param>
            /// <returns>True if <paramref name="symbolReference"/> exists in the symbol stack</returns>
            public bool IsPresent(short symbolReference)
            {
                foreach (var symbol in SymbolReferenceStack)
                {
                    if (symbol == symbolReference)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void AddToCautionDescription(string textToAdd)
        {
            if (string.IsNullOrWhiteSpace(mCautionDescription))
            {
                mCautionDescription = "";
            }

            mCautionDescription += textToAdd;
        }

        private void CheckCaution(string formulaExcerpt)
        {
            for (var length = 0; length < ElementsAndAbbrevs.MAX_ABBREV_LENGTH; length++)
            {
                if (length > formulaExcerpt.Length)
                    break;

                var test = formulaExcerpt.Substring(0, length);
                var newCaution = Messages.LookupCautionStatement(test);
                if (!string.IsNullOrEmpty(newCaution))
                {
                    AddToCautionDescription(newCaution);
                    break;
                }
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
        /// <param name="formula">Input/output: updated by ParseFormulaPublic</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(ref string formula)
        {
            return ComputeFormulaWeight(ref formula, out _);
        }

        /// <summary>
        /// Compute the weight of a formula (or abbreviation)
        /// </summary>
        /// <param name="formula">Input/output: updated by ParseFormulaPublic</param>
        /// <param name="stdDev">Computed standard deviation</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(ref string formula, out double stdDev)
        {
            var computationStats = new ComputationStats();

            ParseFormulaPublic(ref formula, computationStats, false);

            if (mErrorParams.ErrorId == 0)
            {
                stdDev = computationStats.StandardDeviation;
                return computationStats.TotalMass;
            }

            stdDev = 0;
            return -1;
        }

        /// <summary>
        /// Compute the weight of an abbreviation
        /// </summary>
        /// <param name="abbrev">Abbreviation to update with mass, stDev, and element counts</param>
        /// <param name="updateFormula">If true, also update the formula</param>
        /// <returns>True if no error, or false if an error occurred</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        internal bool ComputeAbbrevWeight(AbbrevStatsData abbrev, bool updateFormula = false)
        {
            var computationStats = new ComputationStats();
            var formula = abbrev.Formula;

            ParseFormulaPublic(ref formula, computationStats, false);

            if (mErrorParams.ErrorId == 0)
            {
                if (updateFormula)
                {
                    abbrev.Formula = formula;
                }

                abbrev.StdDev = computationStats.StandardDeviation;
                abbrev.Mass = computationStats.TotalMass;

                abbrev.ClearElements();
                for (var i = 1; i <= ElementsAndAbbrevs.ELEMENT_COUNT; i++)
                {
                    var element = computationStats.Elements[i];
                    if (element.Used)
                    {
                        abbrev.AddElement(i, element.Count, element.IsotopicCorrection);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Compute percent composition of the elements defined in <paramref name="computationStats"/>
        /// </summary>
        /// <param name="computationStats">Input/output</param>
        public void ComputePercentComposition(ComputationStats computationStats)
        {
            // Determine the number of elements in the formula
            for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
            {
                var elementPctComp = computationStats.PercentCompositions[elementIndex];
                if (computationStats.TotalMass > 0d)
                {
                    var element = Elements.ElementStats[elementIndex];
                    var elementUse = computationStats.Elements[elementIndex];

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
        /// <remarks></remarks>
        public string ConvertFormulaToEmpirical(string formula)
        {
            // TODO: Also needs to support isotopes used in abbreviations....
            var computationStats = new ComputationStats();

            // Call ParseFormulaPublic to compute the formula's mass and fill computationStats
            var mass = ParseFormulaPublic(ref formula, computationStats);

            if (mErrorParams.ErrorId == 0)
            {
                // Convert to empirical formula
                var empiricalFormula = "";
                // Carbon first, then hydrogen, then the rest alphabetically
                // ElementAlph is already sorted properly as 0:{'C',6}, 1:{'H',1}, then alphabetically
                foreach (var elementIndex in Elements.ElementAlph.Select(x => x.Value))
                {
                    // Only display the element if it's in the formula
                    var thisElementCount = mComputationStatsSaved.Elements[elementIndex].Count;
                    if (Math.Abs(thisElementCount - 1.0d) < float.Epsilon)
                    {
                        empiricalFormula += Elements.ElementStats[elementIndex].Symbol;
                    }
                    else if (thisElementCount > 0d)
                    {
                        empiricalFormula += Elements.ElementStats[elementIndex].Symbol + thisElementCount;
                    }
                }

                return empiricalFormula;
            }

            return (-1).ToString();
        }

        /// <summary>
        /// Converts <paramref name="elementCounts"/> to its corresponding empirical formula
        /// </summary>
        /// <param name="elementCounts"></param>
        /// <returns>The empirical formula, or -1 if an error</returns>
        /// <remarks></remarks>
        public string ConvertFormulaToEmpirical(IReadOnlyDictionary<int, double> elementCounts)
        {
            // TODO: Also needs to support isotopes used in abbreviations....
            if (elementCounts.Count == 0)
            {
                return (-1).ToString();
            }

            // Convert to empirical formula
            var empiricalFormula = "";
            // Carbon first, then hydrogen, then the rest alphabetically
            // ElementAlph is already sorted properly as 0:{'C',6}, 1:{'H',1}, then alphabetically
            foreach (var elementIndex in Elements.ElementAlph.Select(x => x.Value))
            {
                if (elementCounts.TryGetValue(elementIndex, out var count))
                {
                    // Only display the element if it's in the formula
                    if (Math.Abs(count - 1.0) < float.Epsilon)
                    {
                        empiricalFormula += Elements.ElementStats[elementIndex].Symbol;
                    }
                    else if (count > 0)
                    {
                        empiricalFormula += Elements.ElementStats[elementIndex].Symbol + count;
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
        /// <remarks></remarks>
        public string ExpandAbbreviationsInFormula(string formula)
        {
            var computationStats = new ComputationStats();

            // Call ExpandAbbreviationsInFormula to compute the formula's mass
            var mass = ParseFormulaPublic(ref formula, computationStats, true);

            if (mErrorParams.ErrorId == 0)
            {
                return formula;
            }

            return (-1).ToString();
        }

        public string GetCautionDescription()
        {
            return mCautionDescription;
        }

        public string GetErrorDescription()
        {
            if (mErrorParams.ErrorId != 0)
            {
                return Messages.LookupMessage(mErrorParams.ErrorId);
            }

            return "";
        }

        public int GetErrorId()
        {
            return mErrorParams.ErrorId;
        }

        public string GetErrorCharacter()
        {
            return mErrorParams.ErrorCharacter;
        }

        public int GetErrorPosition()
        {
            return mErrorParams.ErrorPosition;
        }

        /// <summary>
        /// Determines the molecular weight and elemental composition of <paramref name="formula"/>
        /// </summary>
        /// <param name="formula">Input/output: formula to parse</param>
        /// <param name="computationStats">Output: additional information about the formula</param>
        /// <param name="expandAbbreviations"></param>
        /// <param name="valueForX"></param>
        /// <returns>Computed molecular weight if no error; otherwise -1</returns>
        /// <remarks>
        /// ErrorParams will hold information on errors that occur (previous errors are cleared when this function is called)
        /// Use ComputeFormulaWeight if you only want to know the weight of a formula (it calls this function)
        /// </remarks>
        public double ParseFormulaPublic(
            ref string formula,
            ComputationStats computationStats,
            bool expandAbbreviations = false,
            double valueForX = 1)
        {
            try
            {
                // Reset ErrorParams to clear any prior errors
                mErrorParams.Reset();

                // Reset Caution Description
                mCautionDescription = "";

                if (string.IsNullOrWhiteSpace(formula))
                {
                    mComputationStatsSaved = computationStats.Clone();
                    return 0;
                }

                Console.WriteLine(formula);
                var formulaData = new FormulaParseData(formula);

                var abbrevSymbolStack = new AbbrevSymbolStack();
                formula = ParseFormulaRecursive(formula, computationStats, abbrevSymbolStack, expandAbbreviations, out var stdDevSum, out _, valueForX);

                ParseFormulaComponents(formulaData.Formula, formulaData, valueForX);
                ComputeFormulaMass(formulaData);
                if (expandAbbreviations)
                {
                    ParseFormulaExpandAbbreviations(formulaData);
                }

                formulaData.ReplaceFormulaWithCorrected();

                // Copy computationStats to mComputationStatsSaved
                mComputationStatsSaved = computationStats.Clone();

                Console.WriteLine("Old Cautions: {0}", GetCautionDescription());
                Console.WriteLine("New Cautions: {0}", formulaData.CautionDescription);

                if (mErrorParams.ErrorId == 0)
                {
                    // Compute the standard deviation
                    computationStats.StandardDeviation = Math.Sqrt(stdDevSum);

                    // Compute the total molecular weight
                    computationStats.TotalMass = 0d; // Reset total weight of compound to 0 so we can add to it
                    for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
                        // Increase total weight by multiplying the count of each element by the element's mass
                        // In addition, add in the Isotopic Correction value
                        computationStats.TotalMass = computationStats.TotalMass + Elements.ElementStats[elementIndex].Mass * computationStats.Elements[elementIndex].Count + computationStats.Elements[elementIndex].IsotopicCorrection;

                    if (!formulaData.HasError)
                    {
                        Console.WriteLine(formulaData.FormulaCorrected);
                        var stats = formulaData.Stats;

                        // Compute the standard deviation
                        stats.StandardDeviation = Math.Sqrt(formulaData.FormulaSections.First().StDevSum);

                        // Compute the total molecular weight
                        stats.TotalMass = 0d; // Reset total weight of compound to 0 so we can add to it
                        for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
                            // Increase total weight by multiplying the count of each element by the element's mass
                            // In addition, add in the Isotopic Correction value
                            stats.TotalMass = stats.TotalMass + Elements.ElementStats[elementIndex].Mass * stats.Elements[elementIndex].Count + stats.Elements[elementIndex].IsotopicCorrection;

                        Console.WriteLine("StDev: old vs. new: {0} - {1} = diff {2}", computationStats.StandardDeviation, stats.StandardDeviation, computationStats.StandardDeviation - stats.StandardDeviation);
                        Console.WriteLine("Mass: old vs. new: {0} - {1} = diff {2}", computationStats.TotalMass, stats.TotalMass, computationStats.TotalMass - stats.TotalMass);
                        Console.WriteLine("Charge: old vs. new: {0} - {1} = diff {2}", computationStats.Charge, stats.Charge, computationStats.Charge - stats.Charge);
                    }
                    else
                    {
                        Console.WriteLine("ErrorId: old vs. new: {0} - {1} = diff {2}", mErrorParams.ErrorId, formulaData.Error.ErrorId, mErrorParams.ErrorId - formulaData.Error.ErrorId);
                        Console.WriteLine("ErrorPos: old vs. new: {0} - {1} = diff {2}", mErrorParams.ErrorPosition, formulaData.Error.ErrorPosition, mErrorParams.ErrorPosition - formulaData.Error.ErrorPosition);
                        Console.WriteLine("ErrorChar: old vs. new: '{0}' - '{1}' = diff? {2}", mErrorParams.ErrorCharacter, formulaData.Error.ErrorCharacter, !string.Equals(mErrorParams.ErrorCharacter, formulaData.Error.ErrorCharacter));
                        Console.WriteLine("New Description: {0}", Messages.GetMessageStatement(formulaData.Error.ErrorId));
                    }

                    return computationStats.TotalMass;
                }
                else
                {
                    Console.WriteLine("ErrorId: old vs. new: {0} - {1} = diff {2}", mErrorParams.ErrorId, formulaData.Error.ErrorId, mErrorParams.ErrorId - formulaData.Error.ErrorId);
                    Console.WriteLine("ErrorPos: old vs. new: {0} - {1} = diff {2}", mErrorParams.ErrorPosition, formulaData.Error.ErrorPosition, mErrorParams.ErrorPosition - formulaData.Error.ErrorPosition);
                    Console.WriteLine("ErrorChar: old vs. new: '{0}' - '{1}' = diff? {2}", mErrorParams.ErrorCharacter, formulaData.Error.ErrorCharacter, !string.Equals(mErrorParams.ErrorCharacter, formulaData.Error.ErrorCharacter));
                    Console.WriteLine();
                    var oldMessage = Messages.GetMessageStatement(mErrorParams.ErrorId);
                    var newMessage = Messages.GetMessageStatement(formulaData.Error.ErrorId);
                    Console.WriteLine("Old Description: {0}", oldMessage);
                    Console.WriteLine("New Description: {0}", newMessage);
                    Console.WriteLine("Diff: {0}", oldMessage.Equals(newMessage) ? "same" : "MISMATCH!");
                    Console.WriteLine();
                    var oldMarkedFormula = "";
                    if (mErrorParams.ErrorPosition >= formula.Length)
                    {
                        oldMarkedFormula = formula + "''";
                    }
                    else if (mErrorParams.ErrorPosition == formula.Length - 1)
                    {
                        oldMarkedFormula = formula.Substring(0, mErrorParams.ErrorPosition) + "'" + formula[mErrorParams.ErrorPosition] + "'";
                    }
                    else
                    {
                        oldMarkedFormula = formula.Substring(0, mErrorParams.ErrorPosition) + "'" + formula[mErrorParams.ErrorPosition] + "'" + formula.Substring(mErrorParams.ErrorPosition + 1);
                    }
                    Console.WriteLine("Old highlight: {0}", oldMarkedFormula);
                    var newMarkedFormula = "";
                    var newFormula = formulaData.Formula;
                    var newPosition = formulaData.Error.ErrorPosition;
                    if (newPosition >= newFormula.Length)
                    {
                        newMarkedFormula = formulaData + "''";
                    }
                    else if (newPosition == newFormula.Length - 1)
                    {
                        newMarkedFormula = newFormula.Substring(0, newPosition) + "'" + newFormula[newPosition] + "'";
                    }
                    else
                    {
                        newMarkedFormula = newFormula.Substring(0, newPosition) + "'" + newFormula[newPosition] + "'" + newFormula.Substring(newPosition + 1);
                    }
                    Console.WriteLine("New highlight: {0}", newMarkedFormula);
                    Console.WriteLine("Diff: {0}", oldMarkedFormula.Equals(newMarkedFormula) ? "same" : "MISMATCH!");
                }

                return -1;
            }
            catch (Exception ex)
            {
                mElementTools.GeneralErrorHandler("FormulaParser.ParseFormulaPublic", ex);
                return -1;
            }
        }

        /// <summary>
        /// Parse a formula into its components (elements and abbreviations)
        /// </summary>
        /// <param name="formula">Formula (or segment) to parse</param>
        /// <param name="data">Formula parsing data object to track errors and subtraction information</param>
        /// <param name="valueForX">Value to use for 'X' if present</param>
        /// <param name="components">'null' for calls from other methods (this parameter is used for recursive calls)</param>
        /// <param name="prevPosition">Should only be used for recursive calls; position tracking for error reporting</param>
        /// <param name="parenDepth">Should only be used for recursive calls</param>
        private void ParseFormulaComponents(string formula, FormulaParseData data, double valueForX = 1, FormulaComponentGroup components = null, int prevPosition = 0, int parenDepth = 0)
        {
            if (formula.Contains(">"))
            {
                // Can have multiple '>' symbols in a formula (processed right-to-left), but '>' cannot occur within brackets, etc.
                var blocks = formula.Split('>');
                // Add sufficient FormulaData objects for the split.
                data.FormulaSections.AddRange(Enumerable.Range(0, blocks.Length - 1).Select(x => new FormulaData()).ToArray());

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

                    ParseFormulaComponents(block, data, valueForX, data.FormulaSections[i].Components, runningLength);
                    // Add block.Length to get the index of '>', and 1 to get the first index after it.
                    runningLength += block.Length + 1;

                    if (data.HasError)
                    {
                        break;
                    }
                }

                return;
            }

            // Formula does not contain >
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
                var isotopeText = "";
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
                        if (charAsc >= 'A' && charAsc <= 'Z' || charAsc >= 'a' && charAsc <= 'z') // Uppercase A to Z and lowercase a to z
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
                        ParseFormulaComponents(nextRemnant, data, valueForX, groupComponent, prevPosition + charIndex + 1, parenDepth + 1);
                        var groupSegmentLength = groupComponent.FormulaOriginal.Length;

                        if (charIndex + groupSegmentLength >= formula.Length)
                        {
                            // Missing closing parenthesis, highlight the opening one.
                            data.Error.SetError(3, charPosition, formula.Substring(charIndex, 1));
                            return;
                        }

                        var parenCloseChar = formula[charIndex + groupSegmentLength];
                        if (!data.HasError && (currentChar == '(' && parenCloseChar != ')' || currentChar == '{' && parenCloseChar != '}' || currentChar == '[' && parenCloseChar != ']'))
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

                        var nextChar = nextRemnant.Length > 1 ? char.ToUpper(nextRemnant[0]) : EMPTY_STRING_CHAR;
                        var nextChar2 = nextRemnant.Length > 2 ? nextRemnant[1] : EMPTY_STRING_CHAR;
                        var bracketNumLength = 0;
                        var leadingCoefficient = 0.0;
                        if (nextChar == 'X' && nextChar2 != 'e')
                        {
                            leadingCoefficient = valueForX;
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
                            // TODO: safety check for end of line
                            var bracketComponent = new FormulaComponentGroup(insideBrackets: true)
                            {
                                PrefixChars = formula.Substring(charIndex, bracketNumLength + 1),
                                LeadingCoefficient = leadingCoefficient
                            };
                            components.Add(bracketComponent);
                            component = bracketComponent;
                            ParseFormulaComponents(formula.Substring(numEndIndex + 1), data, valueForX, bracketComponent, prevPosition + numEndIndex + 1, parenDepth);
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
                        if (currentChar >= '0' && currentChar <= '9')
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
                        ParseFormulaComponents(dashRemnant, data, valueForX, dashComponent, prevPosition + charIndex + dashNumLength + 1, parenDepth);

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
                    case var @case when '0' <= @case && @case <= '9': // . or , and Numbers 0 to 9
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

                    case var case1 when 'A' <= case1 && case1 <= 'Z': // A-Z
                    case var case2 when 'a' <= case2 && case2 <= 'z': // a-z
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

                                // Found an abbreviation
                                if (isIsotope)
                                {
                                    // Cannot have isotopic mass for an abbreviation, including deuterium
                                    if (char.ToUpper(currentChar) == 'D' && nextRemnant[0] != 'y')
                                    {
                                        // Isotopic mass used for Deuterium; highlight the current character
                                        data.Error.SetError(26, charPosition, formula.Substring(charIndex, 1));
                                    }
                                    else
                                    {
                                        // Highlight the current character.
                                        data.Error.SetError(24, charPosition, formula.Substring(charIndex, 1));
                                    }
                                }
                                else
                                {
                                    // Parse abbreviation
                                    // Simply treat it like a formula surrounded by parentheses
                                    // Thus, find the number after the abbreviation, then call ParseFormulaRecursive, sending it the formula for the abbreviation
                                    // Update the abbrevSymbolStack before calling so that we can check for circular abbreviation references

                                    // Record the abbreviation length
                                    var abbrevStats = Elements.AbbrevStats[symbolReference];
                                    var abbrevSymbolLength = abbrevStats.Symbol.Length;

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
                                if (char.ToUpper(currentChar) == 'X')
                                {
                                    // X for solver but no preceding bracket, highlight the current character
                                    data.Error.SetError(18, charPosition, formula.Substring(charIndex, 1));
                                }
                                else
                                    // unmatched character, highlight it.
                                    data.Error.SetError(1, charPosition, formula.Substring(charIndex, 1));

                                break;
                        }

                        break;

                    case '^': // ^ (caret)
                        // Hitting this should not be possible, so just ignore it.
                        break;

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
        /// Compute the mass for a parsed formula
        /// </summary>
        /// <param name="formulaData">Formula parsing data with formula parsed into components</param>
        /// <param name="currentFormula">'null' for calls from other methods (this parameter is used for recursive calls)</param>
        /// <param name="currentGroup">'null' for calls from other methods (this parameter is used for recursive calls)</param>
        /// <param name="prevPosition">Should only be used for recursive calls; position tracking for error reporting</param>
        /// <param name="multiplier">Should only be used for recursive calls</param>
        private void ComputeFormulaMass(FormulaParseData formulaData, FormulaData currentFormula = null, FormulaComponentGroup currentGroup = null, int prevPosition = 0, double multiplier = 1)
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
                var previousStartPosition = formulaData.FormulaOriginal.Length;
                FormulaData previousBlock = null;
                for (var i = formulaData.FormulaSections.Count - 1; i >= 0; i--)
                {
                    var currentBlock = formulaData.FormulaSections[i];
                    var startPosition = previousStartPosition - currentBlock.Components.SymbolOriginal.Length;
                    ComputeFormulaMass(formulaData, currentBlock, currentBlock.Components, startPosition);

                    // Perform subtraction...
                    if (previousBlock != null)
                    {
                        var cStats = currentBlock.Stats;
                        var pStats = previousBlock.Stats;
                        // Update computationStats by subtracting the atom counts of the first half minus the second half
                        // If any atom counts become < 0 then, then raise an error
                        for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
                        {
                            if (pStats.Elements[elementIndex].Count <= 0)
                                continue;

                            var element = cStats.Elements[elementIndex];
                            if (Elements.ElementStats[elementIndex].Mass * element.Count + element.IsotopicCorrection >= Elements.ElementStats[elementIndex].Mass * pStats.Elements[elementIndex].Count + pStats.Elements[elementIndex].IsotopicCorrection)
                            {
                                element.Count -= pStats.Elements[elementIndex].Count;
                                if (element.Count < 0d)
                                {
                                    // This shouldn't happen
                                    Console.WriteLine(".Count is less than 0 in ParseFormulaRecursive; this shouldn't happen");
                                    element.Count = 0d;
                                }

                                if (Math.Abs(pStats.Elements[elementIndex].IsotopicCorrection) > float.Epsilon)
                                {
                                    element.IsotopicCorrection -= pStats.Elements[elementIndex].IsotopicCorrection;
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
                    ComputeFormulaMass(formulaData, currentFormula, fcg, position, multiplier * fcg.LeadingCoefficient * fcg.Count);

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
                                    case var case3 when 3 <= case3 && case3 <= 6:
                                    case var case4 when 11 <= case4 && case4 <= 14:
                                    case var case5 when 19 <= case5 && case5 <= 32:
                                    case var case6 when 37 <= case6 && case6 <= 50:
                                    case var case7 when 55 <= case7 && case7 <= 82:
                                    case var case8 when 87 <= case8 && case8 <= 109:
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

                            if (symbolReference == 6 || symbolReference == 14)
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
                            var isotope = new IsotopicAtomInfo();
                            element.Isotopes.Add(isotope);
                            isotope.Count += addCount;
                            isotope.Mass = component.Isotope;

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

                        // Store the abbreviation for the circular reference checks (if parsing an abbreviation formula)
                        formulaData.AddAbbreviationUsage(abbrevStats.Symbol, position);

                        // Use the defined charge for the abbreviation
                        // addCount is an abbreviation occurrence count
                        var addCount = multiplier * component.Count;
                        stats.Charge = (float)(stats.Charge + addCount * abbrevStats.Charge);

                        currentFormula.StDevSum += addCount * Math.Pow(abbrevStats.StdDev, 2);

                        // Add the element data from the abbreviation to the element tracking.
                        foreach (var item in abbrevStats.ElementCounts)
                        {
                            stats.Elements[item.Key].Count += addCount * item.Value;
                            stats.Elements[item.Key].IsotopicCorrection += addCount * abbrevStats.ElementIsotopicCorrection[item.Key];
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
                        abbrevFormula = ConvertFormulaToEmpirical(abbrevStats.ElementCounts);
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
        /// Determine elements in an abbreviation or elements and abbreviations in a formula
        /// Stores results in <paramref name="computationStats"/>
        /// ErrorParams will hold information on errors that occur
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="computationStats"></param>
        /// <param name="abbrevSymbolStack"></param>
        /// <param name="expandAbbreviations"></param>
        /// <param name="stdDevSum">Sum of the squares of the standard deviations</param>
        /// <param name="carbonOrSiliconReturnCount">Tracks the number of carbon and silicon atoms found; used when correcting for charge inside parentheses or inside an abbreviation</param>
        /// <param name="valueForX"></param>
        /// <param name="charCountPrior"></param>
        /// <param name="parenthMultiplier">The value to multiply all values by if inside parentheses</param>
        /// <param name="dashMultiplierPrior"></param>
        /// <param name="bracketMultiplierPrior"></param>
        /// <param name="parenthLevelPrevious"></param>
        /// <returns>Formatted formula</returns>
        private string ParseFormulaRecursive(
            string formula,
            ComputationStats computationStats,
            AbbrevSymbolStack abbrevSymbolStack,
            bool expandAbbreviations,
            out double stdDevSum,
            out int carbonOrSiliconReturnCount,
            double valueForX = 1.0d,
            int charCountPrior = 0,
            double parenthMultiplier = 1.0d,
            double dashMultiplierPrior = 1.0d,
            double bracketMultiplierPrior = 1.0d,
            short parenthLevelPrevious = 0)
        {
            // ( and ) are 40 and 41   - is 45   { and } are 123 and 125
            // Numbers are 48 to 57    . is 46
            // Uppercase letters are 65 to 90
            // Lowercase letters are 97 to 122
            // [ and ] are 91 and 93
            // ^ is 94
            // _ is 95

            stdDevSum = 0;
            carbonOrSiliconReturnCount = 0;

            try
            {
                var dashMultiplier = dashMultiplierPrior;
                var bracketMultiplier = bracketMultiplierPrior;

                // Look for the > symbol
                // If found, this means take First Part minus the Second Part
                if (formula.Contains(">"))
                {
                    formula = ParseFormulaSubtraction(formula, computationStats, abbrevSymbolStack, expandAbbreviations, out stdDevSum, valueForX, charCountPrior, parenthMultiplier, dashMultiplier, bracketMultiplier, parenthLevelPrevious);
                    return formula;
                }

                var symbolLength = 0;
                var caretPresent = false;

                var caretVal = 0.0;
                var char1 = string.Empty;

                short prevSymbolReference = 0;
                var parenthLevel = 0;

                var insideBrackets = false;

                var dashPos = -1;

                var loneCarbonOrSilicon = 0;

                // Formula does not contain >
                // Parse it
                int charIndex;
                for (charIndex = 0; charIndex < formula.Length; charIndex++)
                {
                    char1 = formula.Substring(charIndex, 1);
                    var char2 = charIndex + 1 < formula.Length ? formula.Substring(charIndex + 1, 1) : "";
                    var char3 = charIndex + 2 < formula.Length ? formula.Substring(charIndex + 2, 1) : "";
                    var charRemain = charIndex + 3 < formula.Length ? formula.Substring(charIndex + 3) : "";
                    if (ComputationOptions.CaseConversion != CaseConversionMode.ExactCase)
                        char1 = char1.ToUpper();

                    if (ComputationOptions.BracketsAsParentheses)
                    {
                        char1 = char1.Replace("[", "(").Replace("]", ")");
                    }

                    if (string.IsNullOrEmpty(char1))
                        char1 = EMPTY_STRING_CHAR.ToString();
                    if (string.IsNullOrEmpty(char2))
                        char2 = EMPTY_STRING_CHAR.ToString();
                    if (string.IsNullOrEmpty(char3))
                        char3 = EMPTY_STRING_CHAR.ToString();
                    if (string.IsNullOrEmpty(charRemain))
                        charRemain = EMPTY_STRING_CHAR.ToString();

                    var formulaExcerpt = char1 + char2 + char3 + charRemain;

                    // Check for needed caution statements
                    CheckCaution(formulaExcerpt);

                    int numLength;
                    double adjacentNum;
                    int addonCount;
                    switch (char1[0])
                    {
                        case '(':
                        case '{': // ( or {    Record its position
                            // See if a number is present just after the opening parenthesis
                            if (char.IsDigit(char2[0]) || char2 == ".")
                            {
                                // Misplaced number
                                mErrorParams.SetError(14, charIndex);
                            }

                            if (mErrorParams.ErrorId == 0)
                            {
                                // search for closing parenthesis
                                parenthLevel = 1;
                                for (var parenthClose = charIndex + 1; parenthClose < formula.Length; parenthClose++)
                                {
                                    var bracketChar = formula[parenthClose];
                                    switch (bracketChar)
                                    {
                                        case '[':
                                            if (ComputationOptions.BracketsAsParentheses)
                                                goto case '(';

                                            // Do not count the bracket
                                            break;

                                        case '(':
                                        case '{':
                                            // Another opening parentheses
                                            // increment parenthLevel
                                            parenthLevel += 1;

                                            break;

                                        case ']':
                                            if (ComputationOptions.BracketsAsParentheses)
                                                goto case ')';

                                            // Do not count the bracket
                                            break;
                                        case ')':
                                        case '}':
                                            parenthLevel -= 1;
                                            if (parenthLevel == 0)
                                            {
                                                adjacentNum = ParseNumberAtStart(formula.Substring(parenthClose + 1), out numLength, mErrorParams, charIndex + symbolLength);

                                                if (adjacentNum < 0d || numLength == 0)
                                                {
                                                    adjacentNum = 1.0d;
                                                    addonCount = 0;
                                                }
                                                else
                                                {
                                                    addonCount = numLength;
                                                }

                                                var subFormula = formula.Substring(charIndex + 1, parenthClose - (charIndex + 1));

                                                // Note, must pass parenthMultiplier * adjacentNum to preserve previous parentheses stuff
                                                var newFormula = ParseFormulaRecursive(subFormula, computationStats, abbrevSymbolStack, expandAbbreviations, out var newStdDevSum, out var carbonSiliconCount, valueForX, charCountPrior + charIndex, parenthMultiplier * adjacentNum, dashMultiplier, bracketMultiplier, (short)(parenthLevelPrevious + 1));
                                                stdDevSum += newStdDevSum;

                                                // If expanding abbreviations, then newFormula might be longer than formula, must add this onto charIndex also
                                                var expandAbbrevAdd = newFormula.Length - subFormula.Length;

                                                // Must replace the part of the formula parsed with the newFormula part, in case the formula was expanded or elements were capitalized
                                                formula = formula.Substring(0, charIndex + 1) + newFormula + formula.Substring(parenthClose);
                                                charIndex = parenthClose + addonCount + expandAbbrevAdd;

                                                // Correct charge
                                                if (carbonSiliconCount > 0)
                                                {
                                                    computationStats.Charge = (float)(computationStats.Charge - 2d * adjacentNum);
                                                    if (adjacentNum > 1d && carbonSiliconCount > 1)
                                                    {
                                                        computationStats.Charge = (float)(computationStats.Charge - 2d * (adjacentNum - 1d) * (carbonSiliconCount - 1));
                                                    }
                                                }

                                                // exit the loop;
                                                parenthClose = formula.Length;
                                            }

                                            break;
                                    }
                                }
                            }

                            if (parenthLevel > 0 && mErrorParams.ErrorId == 0)
                            {
                                // Missing closing parenthesis
                                mErrorParams.SetError(3, charIndex);
                            }

                            prevSymbolReference = 0;
                            break;

                        case ')':
                        case '}': // )    Repeat a section of a formula
                            // Should have been skipped
                            // Unmatched closing parentheses
                            mErrorParams.SetError(4, charIndex);
                            break;

                        case '-': // -
                            // Used to denote a leading coefficient
                            adjacentNum = ParseNumberAtStart(char2 + char3 + charRemain, out numLength, mErrorParams, charIndex + symbolLength);

                            if (adjacentNum > 0d)
                            {
                                dashPos = charIndex + numLength;
                                dashMultiplier = adjacentNum * dashMultiplierPrior;
                                charIndex += numLength;
                            }
                            else if (Math.Abs(adjacentNum) < float.Epsilon && numLength > 0)
                            {
                                // Cannot have 0 after a dash
                                mErrorParams.SetError(5, charIndex + 1);
                            }
                            else
                            {
                                // No number is present, that's just fine
                                // Make sure defaults are set, though
                                dashPos = -1;
                                dashMultiplier = dashMultiplierPrior;
                            }

                            prevSymbolReference = 0;
                            break;

                        case ',':
                        case '.':
                        case var @case when '0' <= @case && @case <= '9': // . or , and Numbers 0 to 9
                            // They should only be encountered as a leading coefficient
                            // Should have been bypassed when the coefficient was processed
                            if (charIndex == 0)
                            {
                                // Formula starts with a number -- multiply section by number (until next dash)
                                adjacentNum = ParseNumberAtStart(formulaExcerpt, out numLength, mErrorParams, charIndex + symbolLength);

                                if (adjacentNum >= 0d && numLength > 0)
                                {
                                    dashPos = charIndex + numLength - 1;
                                    dashMultiplier = adjacentNum * dashMultiplierPrior;
                                    charIndex += numLength - 1;
                                }
                                else
                                {
                                    // A number less then zero should have been handled by CatchParseNumError above
                                    // Make sure defaults are set, though
                                    dashPos = -1;
                                    dashMultiplier = dashMultiplierPrior;
                                }
                            }
                            else if (NumberConverter.CDblSafe(formula.Substring(charIndex - 1, 1)) > 0d)
                            {
                                // Number too large
                                mErrorParams.SetError(7, charIndex);
                            }
                            else
                            {
                                // Misplaced number
                                mErrorParams.SetError(14, charIndex);
                            }

                            prevSymbolReference = 0;
                            break;

                        case '[': // [
                            if (char2.ToUpper() == "X")
                            {
                                if (char3 == "e")
                                {
                                    adjacentNum = ParseNumberAtStart(char2 + char3 + charRemain, out numLength, mErrorParams, charIndex + symbolLength);
                                }
                                else
                                {
                                    adjacentNum = valueForX;
                                    numLength = 1;
                                }
                            }
                            else
                            {
                                adjacentNum = ParseNumberAtStart(char2 + char3 + charRemain, out numLength, mErrorParams, charIndex + symbolLength);
                            }

                            if (mErrorParams.ErrorId == 0)
                            {
                                if (insideBrackets)
                                {
                                    // No Nested brackets.
                                    mErrorParams.SetError(16, charIndex);
                                }
                                else if (adjacentNum < 0d || numLength == 0)
                                {
                                    // No number after bracket
                                    mErrorParams.SetError(12, charIndex + 1);
                                }
                                else
                                {
                                    // Coefficient for bracketed section.
                                    insideBrackets = true;
                                    bracketMultiplier = adjacentNum * bracketMultiplierPrior; // Take times bracketMultiplierPrior in case it wasn't 1 to start with
                                    charIndex += numLength;
                                }
                            }

                            prevSymbolReference = 0;
                            break;

                        case ']': // ]
                            adjacentNum = ParseNumberAtStart(char2 + char3 + charRemain, out numLength, mErrorParams, charIndex + symbolLength);

                            if (adjacentNum >= 0d && numLength > 0)
                            {
                                // Number following bracket
                                mErrorParams.SetError(11, charIndex + 1);
                            }
                            else if (insideBrackets)
                            {
                                if (dashPos >= 0)
                                {
                                    // Need to set dashPos and dashMultiplier back to defaults, since a dash number goes back to one inside brackets
                                    dashPos = -1;
                                    dashMultiplier = 1d;
                                }

                                insideBrackets = false;
                                bracketMultiplier = bracketMultiplierPrior;
                            }
                            else
                            {
                                // Unmatched bracket
                                mErrorParams.SetError(15, charIndex);
                            }

                            break;

                        case var case1 when 'A' <= case1 && case1 <= 'Z': // A-Z
                        case var case2 when 'a' <= case2 && case2 <= 'z': // a-z
                        case '+':
                        case '_': // Uppercase A to Z and lowercase a to z, and the plus (+) sign, and the underscore (_)
                            addonCount = 0;
                            adjacentNum = 0d;

                            var symbolMatchType = Elements.CheckElemAndAbbrev(formulaExcerpt, out var symbolReference);

                            switch (symbolMatchType)
                            {
                                case SymbolMatchMode.Element:
                                    // Found an element
                                    // SymbolReference is the elemental number
                                    var elementStats = Elements.ElementStats[symbolReference];
                                    symbolLength = elementStats.Symbol.Length;
                                    if (symbolLength == 0)
                                    {
                                        // No elements in ElementStats yet
                                        // Set symbolLength to 1
                                        symbolLength = 1;
                                    }
                                    // Look for number after element
                                    adjacentNum = ParseNumberAtStart(formula.Substring(charIndex + symbolLength), out numLength, mErrorParams, charIndex + symbolLength);

                                    if (adjacentNum < 0d || numLength == 0)
                                    {
                                        adjacentNum = 1d;
                                    }

                                    // Note that numLength = 0 if adjacentNum was -1 or otherwise < 0
                                    addonCount = numLength + symbolLength - 1;

                                    if (Math.Abs(adjacentNum) < float.Epsilon)
                                    {
                                        // Zero after element
                                        mErrorParams.SetError(5, charIndex + symbolLength);
                                    }
                                    else
                                    {
                                        double atomCountToAdd;
                                        if (!caretPresent)
                                        {
                                            atomCountToAdd = adjacentNum * bracketMultiplier * parenthMultiplier * dashMultiplier;
                                            var element = computationStats.Elements[symbolReference];
                                            element.Count += atomCountToAdd;
                                            element.Used = true; // Element is present tag
                                            stdDevSum += atomCountToAdd * Math.Pow(elementStats.Uncertainty, 2d);

                                            var compStats = computationStats;
                                            // Compute charge
                                            if (symbolReference == 1)
                                            {
                                                // Dealing with hydrogen
                                                switch (prevSymbolReference)
                                                {
                                                    case 1:
                                                    case var case3 when 3 <= case3 && case3 <= 6:
                                                    case var case4 when 11 <= case4 && case4 <= 14:
                                                    case var case5 when 19 <= case5 && case5 <= 32:
                                                    case var case6 when 37 <= case6 && case6 <= 50:
                                                    case var case7 when 55 <= case7 && case7 <= 82:
                                                    case var case8 when 87 <= case8 && case8 <= 109:
                                                        // Hydrogen is -1 with metals (non-halides)
                                                        compStats.Charge = (float)(compStats.Charge + atomCountToAdd * -1);
                                                        break;
                                                    default:
                                                        compStats.Charge = (float)(compStats.Charge + atomCountToAdd * elementStats.Charge);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                compStats.Charge = (float)(compStats.Charge + atomCountToAdd * elementStats.Charge);
                                            }

                                            if (symbolReference == 6 || symbolReference == 14)
                                            {
                                                // Sum up number lone C and Si (not in abbreviations)
                                                loneCarbonOrSilicon = (int)Math.Round(loneCarbonOrSilicon + adjacentNum);
                                            }
                                        }
                                        else
                                        {
                                            //caretPresent = true;
                                            // Check to make sure isotopic mass is reasonable
                                            double isoDifferenceTop = NumberConverter.CIntSafe(0.63d * symbolReference + 6d);
                                            double isoDifferenceBottom = NumberConverter.CIntSafe(0.008d * Math.Pow(symbolReference, 2d) - 0.4d * symbolReference - 6d);
                                            var caretValDifference = caretVal - symbolReference * 2;

                                            if (caretValDifference >= isoDifferenceTop)
                                            {
                                                // Probably too high isotopic mass
                                                AddToCautionDescription(Messages.LookupMessage(660) + ": " + elementStats.Symbol + " - " + caretVal + " " + Messages.LookupMessage(665) + " " + elementStats.Mass);
                                            }
                                            else if (caretVal < symbolReference)
                                            {
                                                // Definitely too low isotopic mass
                                                AddToCautionDescription(Messages.LookupMessage(670) + ": " + elementStats.Symbol + " - " + symbolReference + " " + Messages.LookupMessage(675));
                                            }
                                            else if (caretValDifference <= isoDifferenceBottom)
                                            {
                                                // Probably too low isotopic mass
                                                AddToCautionDescription(Messages.LookupMessage(662) + ": " + elementStats.Symbol + " - " + caretVal + " " + Messages.LookupMessage(665) + " " + elementStats.Mass);
                                            }

                                            // Put in isotopic correction factor
                                            atomCountToAdd = adjacentNum * bracketMultiplier * parenthMultiplier * dashMultiplier;
                                            var element = computationStats.Elements[symbolReference];
                                            // Increment element counting bin
                                            element.Count += atomCountToAdd;

                                            // Store information in .Isotopes[]
                                            // Increment the isotope counting bin
                                            var isotope = new IsotopicAtomInfo();
                                            element.Isotopes.Add(isotope);
                                            isotope.Count += atomCountToAdd;
                                            isotope.Mass = caretVal;

                                            // Add correction amount to computationStats.Elements[SymbolReference].IsotopicCorrection
                                            element.IsotopicCorrection += (caretVal * atomCountToAdd - elementStats.Mass * atomCountToAdd);

                                            // Set bit that element is present
                                            element.Used = true;

                                            // Assume no error in caret value, no need to change stdDevSum

                                            // Reset caretPresent
                                            caretPresent = false;
                                        }

                                        if (ComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                        {
                                            formula = formula.Substring(0, charIndex) + formula.Substring(charIndex, 1).ToUpper() + formula.Substring(charIndex + 1);
                                        }

                                        charIndex += addonCount;
                                    }

                                    break;

                                case SymbolMatchMode.Abbreviation:
                                    // Found an abbreviation or amino acid
                                    // SymbolReference is the abbrev or amino acid number

                                    if (abbrevSymbolStack.IsPresent(symbolReference))
                                    {
                                        // Circular Reference: Can't have an abbreviation referencing an abbreviation that depends upon it
                                        // For example, the following is impossible:  Lor = C6H5Tal and Tal = H4O2Lor
                                        // Furthermore, can't have this either: Lor = C6H5Tal and Tal = H4O2Vin and Vin = S3Lor
                                        mErrorParams.SetError(28, charIndex);
                                    }
                                    // Found an abbreviation
                                    else if (caretPresent)
                                    {
                                        // Cannot have isotopic mass for an abbreviation, including deuterium
                                        if (char1.ToUpper() == "D" && char2 != "y")
                                        {
                                            // Isotopic mass used for Deuterium
                                            mErrorParams.SetError(26, charIndex);
                                        }
                                        else
                                        {
                                            mErrorParams.SetError(24, charIndex);
                                        }
                                    }
                                    else
                                    {
                                        // Parse abbreviation
                                        // Simply treat it like a formula surrounded by parentheses
                                        // Thus, find the number after the abbreviation, then call ParseFormulaRecursive, sending it the formula for the abbreviation
                                        // Update the abbrevSymbolStack before calling so that we can check for circular abbreviation references

                                        // Record the abbreviation length
                                        var abbrevStats = Elements.AbbrevStats[symbolReference];
                                        symbolLength = abbrevStats.Symbol.Length;

                                        // Look for number after abbrev/amino
                                        adjacentNum = ParseNumberAtStart(formula.Substring(charIndex + symbolLength), out numLength, mErrorParams, charIndex + symbolLength);

                                        if (adjacentNum < 0d || numLength == 0)
                                        {
                                            adjacentNum = 1d;
                                            addonCount = symbolLength - 1;
                                        }
                                        else
                                        {
                                            addonCount = numLength + symbolLength - 1;
                                        }

                                        // Add this abbreviation symbol to the Abbreviation Symbol Stack
                                        abbrevSymbolStack.Add(symbolReference);

                                        // Compute the charge prior to calling ParseFormulaRecursive
                                        // During the call to ParseFormulaRecursive, computationStats.Charge will be
                                        // modified according to the atoms in the abbreviation's formula
                                        // This is not what we want; instead, we want to use the defined charge for the abbreviation
                                        // We'll use the atomCountToAdd variable here, though instead of an atom count, it's really an abbreviation occurrence count
                                        var atomCountToAdd = adjacentNum * bracketMultiplier * parenthMultiplier * dashMultiplier;
                                        var chargeSaved = (float)(computationStats.Charge + atomCountToAdd * abbrevStats.Charge);

                                        // When parsing an abbreviation, do not pass on the value of expandAbbreviations
                                        // This way, an abbreviation containing an abbreviation will only get expanded one level
                                        ParseFormulaRecursive(abbrevStats.Formula, computationStats, abbrevSymbolStack, false, out var abbrevStdDevSum, out _, valueForX, charCountPrior + charIndex, parenthMultiplier * adjacentNum, dashMultiplier, bracketMultiplier, parenthLevelPrevious);
                                        stdDevSum += abbrevStdDevSum;

                                        // Update the charge to chargeSaved
                                        computationStats.Charge = chargeSaved;

                                        // Remove this symbol from the Abbreviation Symbol Stack
                                        abbrevSymbolStack.RemoveMostRecent();

                                        if (mErrorParams.ErrorId == 0)
                                        {
                                            if (expandAbbreviations)
                                            {
                                                // Replace abbreviation with empirical formula
                                                var replace = abbrevStats.Formula;

                                                // Look for a number after the abbreviation or amino acid
                                                adjacentNum = ParseNumberAtStart(formula.Substring(charIndex + symbolLength), out numLength, mErrorParams, charIndex + symbolLength);

                                                if (replace.IndexOf(">", StringComparison.Ordinal) >= 0)
                                                {
                                                    // The > symbol means take First Part minus the Second Part
                                                    // If we are parsing a sub formula inside parentheses, or if there are
                                                    // symbols (elements, abbreviations, or numbers) after the abbreviation, then
                                                    // we cannot let the > symbol remain in the abbreviation
                                                    // For example, if Jk = C6H5Cl2>HCl
                                                    // and the user enters Jk2 then chooses Expand Abbreviations
                                                    // Then, naively we might replace this with (C6H5Cl2>HCl)2
                                                    // However, this will generate an error because (C6H5Cl2>HCl)2 gets split
                                                    // to (C6H5Cl2 and HCl)2 which will both generate an error
                                                    // The only option is to convert the abbreviation to its empirical formula
                                                    if (parenthLevelPrevious > 0 || parenthLevel > 0 || charIndex + symbolLength <= formula.Length)
                                                    {
                                                        replace = ConvertFormulaToEmpirical(replace);
                                                    }
                                                }

                                                if (adjacentNum < 0d || numLength == 0)
                                                {
                                                    // No number after abbreviation
                                                    formula = formula.Substring(0, charIndex) + replace + formula.Substring(charIndex + symbolLength);
                                                    symbolLength = replace.Length;
                                                    adjacentNum = 1d;
                                                    addonCount = symbolLength - 1;
                                                }
                                                else
                                                {
                                                    // Number after abbreviation -- must put abbreviation in parentheses
                                                    // Parentheses can handle integer or decimal number
                                                    replace = "(" + replace + ")";
                                                    formula = formula.Substring(0, charIndex) + replace + formula.Substring(charIndex + symbolLength);
                                                    symbolLength = replace.Length;
                                                    addonCount = numLength + symbolLength - 1;
                                                }
                                            }

                                            if (ComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                            {
                                                formula = formula.Substring(0, charIndex) + formula.Substring(charIndex, 1).ToUpper() + formula.Substring(charIndex + 1);
                                            }
                                        }
                                    }

                                    charIndex += addonCount;
                                    break;

                                default:
                                    // Element not Found
                                    if (char1.ToUpper() == "X")
                                    {
                                        // X for solver but no preceding bracket
                                        mErrorParams.SetError(18, charIndex);
                                    }
                                    else
                                    {
                                        mErrorParams.SetError(1, charIndex);
                                    }

                                    break;
                            }

                            prevSymbolReference = symbolReference;
                            break;

                        case '^': // ^ (caret)
                            adjacentNum = ParseNumberAtStart(char2 + char3 + charRemain, out numLength, mErrorParams, charIndex + symbolLength);

                            if (mErrorParams.ErrorId != 0)
                            {
                                // Problem, don't go on.
                            }
                            else
                            {
                                var nextCharIndex = charIndex + 1 + numLength;
                                var charAsc = 0;
                                if (nextCharIndex < formula.Length)
                                {
                                    charAsc = formula[nextCharIndex];
                                }

                                if (adjacentNum >= 0d && numLength > 0)
                                {
                                    if (charAsc >= 65 && charAsc <= 90 || charAsc >= 97 && charAsc <= 122) // Uppercase A to Z and lowercase a to z
                                    {
                                        caretPresent = true;
                                        caretVal = adjacentNum;
                                        charIndex += numLength;
                                    }
                                    else
                                    {
                                        // No letter after isotopic mass
                                        mErrorParams.SetError(22, charIndex + numLength + 1);
                                    }
                                }
                                else
                                {
                                    // Adjacent number is < 0 or not present
                                    // Record error
                                    caretPresent = false;
                                    if (formula.Substring(charIndex + 1, 1) == "-")
                                    {
                                        // Negative number following caret
                                        mErrorParams.SetError(23, charIndex + 1);
                                    }
                                    else
                                    {
                                        // No number following caret
                                        mErrorParams.SetError(20, charIndex + 1);
                                    }
                                }
                            }

                            break;

                        default:
                            // There shouldn't be anything else (except the ~ filler character). If there is, we'll just ignore it
                            break;
                    }

                    if (charIndex == formula.Length - 1)
                    {
                        // Need to make sure compounds are present after a leading coefficient after a dash
                        if (dashMultiplier > 0d)
                        {
                            if (charIndex != dashPos)
                            {
                                // Things went fine, no need to set anything
                            }
                            else
                            {
                                // No compounds after leading coefficient after dash
                                mErrorParams.SetError(25, charIndex);
                            }
                        }
                    }

                    if (mErrorParams.ErrorId != 0)
                    {
                        charIndex = formula.Length;
                    }
                }

                if (insideBrackets)
                {
                    if (mErrorParams.ErrorId == 0)
                    {
                        // Missing closing bracket
                        mErrorParams.SetError(13, charIndex);
                    }
                }

                if (mErrorParams.ErrorId != 0 && mErrorParams.ErrorCharacter.Length == 0)
                {
                    if (string.IsNullOrEmpty(char1))
                        char1 = EMPTY_STRING_CHAR.ToString();
                    mErrorParams.ErrorCharacter = char1;
                    mErrorParams.ErrorPosition += charCountPrior;
                }

                if (loneCarbonOrSilicon > 1)
                {
                    // Correct Charge for number of C and Si
                    computationStats.Charge -= (loneCarbonOrSilicon - 1) * 2;

                    carbonOrSiliconReturnCount = loneCarbonOrSilicon;
                }
                else
                {
                    carbonOrSiliconReturnCount = 0;
                }

                // Return formula, which is possibly now capitalized correctly
                // It will also contain expanded abbreviations
                return formula;
            }
            catch (Exception ex)
            {
                mElementTools.MwtWinDllErrorHandler("MolecularWeightCalculator_FormulaParser.ParseFormula", ex);
                mErrorParams.SetError(-10, 0);

                return formula;
            }
        }

        /// <summary>
        /// Handle formula subtraction, where the formula is in the format "formula1&gt;formula2"
        /// </summary>
        private string ParseFormulaSubtraction(
            string formula,
            ComputationStats computationStats,
            AbbrevSymbolStack abbrevSymbolStack,
            bool expandAbbreviations,
            out double stdDevSum,
            double valueForX = 1.0d,
            int charCountPrior = 0,
            double parenthMultiplier = 1.0d,
            double dashMultiplierPrior = 1.0d,
            double bracketMultiplierPrior = 1.0d,
            short parenthLevelPrevious = 0)
        {
            // Get the index of the first occurrence of >
            var minusSymbolLoc = formula.IndexOf(">", StringComparison.Ordinal);
            var computationStatsRightHalf = new ComputationStats();

            var leftHalf = formula.Substring(0, Math.Min(formula.Length, minusSymbolLoc));
            var rightHalf = formula.Substring(minusSymbolLoc + 1);

            // Parse the first half
            var newFormula = ParseFormulaRecursive(leftHalf, computationStats, abbrevSymbolStack, expandAbbreviations, out var leftStdDevSum, out _, valueForX, charCountPrior, parenthMultiplier, dashMultiplierPrior, bracketMultiplierPrior, parenthLevelPrevious);
            stdDevSum = leftStdDevSum;

            // Parse the second half
            var abbrevSymbolStackRightHalf = new AbbrevSymbolStack();
            var newFormulaRightHalf = ParseFormulaRecursive(rightHalf, computationStatsRightHalf, abbrevSymbolStackRightHalf, expandAbbreviations, out _, out _, valueForX, charCountPrior + minusSymbolLoc, parenthMultiplier, dashMultiplierPrior, bracketMultiplierPrior, parenthLevelPrevious);

            // Update formula
            formula = newFormula + ">" + newFormulaRightHalf;

            // Update computationStats by subtracting the atom counts of the first half minus the second half
            // If any atom counts become < 0 then, then raise an error
            for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
            {
                var element = computationStats.Elements[elementIndex];
                if (Elements.ElementStats[elementIndex].Mass * element.Count + element.IsotopicCorrection >= Elements.ElementStats[elementIndex].Mass * computationStatsRightHalf.Elements[elementIndex].Count + computationStatsRightHalf.Elements[elementIndex].IsotopicCorrection)
                {
                    element.Count -= computationStatsRightHalf.Elements[elementIndex].Count;
                    if (element.Count < 0d)
                    {
                        // This shouldn't happen
                        Console.WriteLine(".Count is less than 0 in ParseFormulaRecursive; this shouldn't happen");
                        element.Count = 0d;
                    }

                    if (Math.Abs(computationStatsRightHalf.Elements[elementIndex].IsotopicCorrection) > float.Epsilon)
                    {
                        element.IsotopicCorrection -= computationStatsRightHalf.Elements[elementIndex].IsotopicCorrection;
                    }
                }
                else
                {
                    // Invalid Formula; raise error
                    mErrorParams.SetError(30, minusSymbolLoc);
                }

                if (mErrorParams.ErrorId != 0)
                    break;
            }

            // Adjust the overall charge
            computationStats.Charge -= computationStatsRightHalf.Charge;

            return formula;
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
            if ((work[0] < '0' || work[0] > '9') && work[0] != ComputationOptions.DecimalSeparator && !(work[0] == '-' && allowNegative))
            {
                numLength = 0; // No number found; not really an error; just return '0', and handle that externally
                return 0;
            }

            // Start of string is a number or a decimal point, or (if allowed) a negative sign
            for (var index = 0; index < work.Length; index++)
            {
                var working = work[index];
                if (char.IsDigit(working) || working == ComputationOptions.DecimalSeparator || allowNegative && working == '-')
                {
                    foundNum += working;
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
                // more than one decPtCount
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

        internal void ResetErrorParams()
        {
            mErrorParams.Reset();
        }
    }
}
