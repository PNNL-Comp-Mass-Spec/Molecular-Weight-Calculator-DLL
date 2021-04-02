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

        private readonly ErrorDescription mErrorParams = new ErrorDescription();

        private ComputationStats mComputationStatsSaved = new ComputationStats();

        private string mCautionDescription;
        public FormulaOptions ComputationOptions { get; }

        internal class ErrorDescription
        {
            public int ErrorId { get; set; } // Contains the error number (used in the LookupMessage function).  In addition, if a program error occurs, ErrorParams.ErrorID = -10
            public int ErrorPosition { get; set; }
            public string ErrorCharacter { get; set; }

            public void Reset()
            {
                ErrorCharacter = "";
                ErrorId = 0;
                ErrorPosition = 0;
            }

            public override string ToString()
            {
                return "ErrorID " + ErrorId + " at " + ErrorPosition + ": " + ErrorCharacter;
            }
        }

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

        private void CatchParseNumError(double adjacentNum, int numSizing, int curCharacter, int symbolLength)
        {
            if (adjacentNum < 0d && numSizing == 0)
            {
                switch (adjacentNum)
                {
                    case -1:
                        // No number, but no error
                        // That's OK
                        break;
                    case -3:
                        // Error: No number after decimal point
                        mErrorParams.ErrorId = 12;
                        mErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                    case -4:
                        // Error: More than one decimal point
                        mErrorParams.ErrorId = 27;
                        mErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                    default:
                        // Error: General number error
                        mErrorParams.ErrorId = 14;
                        mErrorParams.ErrorPosition = curCharacter + symbolLength;
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
            var computationStats = new ComputationStats();

            // Call ParseFormulaPublic to compute the formula's mass and fill computationStats
            var mass = ParseFormulaPublic(ref formula, computationStats);

            if (mErrorParams.ErrorId == 0)
            {
                // Convert to empirical formula
                var empiricalFormula = "";
                // Carbon first, then hydrogen, then the rest alphabetically
                // This is correct to start at -2
                for (var elementIndex = -2; elementIndex < Elements.ElementAlph.Count; elementIndex++)
                {
                    int elementIndexToUse;
                    if (elementIndex == -2)
                    {
                        // Do Carbon first
                        elementIndexToUse = 6;
                    }
                    else if (elementIndex == -1)
                    {
                        // Then do Hydrogen
                        elementIndexToUse = 1;
                    }
                    else
                    {
                        // Then do the rest alphabetically
                        if (Elements.ElementAlph[elementIndex].Key == "C" || Elements.ElementAlph[elementIndex].Key == "H")
                        {
                            // Increment elementIndex when we encounter carbon or hydrogen
                            elementIndex++;
                        }

                        elementIndexToUse = Elements.ElementAlph[elementIndex].Value;
                    }

                    // Only display the element if it's in the formula
                    var thisElementCount = mComputationStatsSaved.Elements[elementIndexToUse].Count;
                    if (Math.Abs(thisElementCount - 1.0d) < float.Epsilon)
                    {
                        empiricalFormula += Elements.ElementStats[elementIndexToUse].Symbol;
                    }
                    else if (thisElementCount > 0d)
                    {
                        empiricalFormula += Elements.ElementStats[elementIndexToUse].Symbol + thisElementCount;
                    }
                }

                return empiricalFormula;
            }

            return (-1).ToString();
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
        /// Checks for presence of <paramref name="symbolReference"/> in <paramref name="abbrevSymbolStack"/>
        /// If found, returns true
        /// </summary>
        /// <param name="abbrevSymbolStack"></param>
        /// <param name="symbolReference"></param>
        /// <returns></returns>
        private bool IsPresentInAbbrevSymbolStack(AbbrevSymbolStack abbrevSymbolStack, short symbolReference)
        {
            try
            {
                var found = false;
                for (var index = 0; index < abbrevSymbolStack.SymbolReferenceStack.Count; index++)
                {
                    if (abbrevSymbolStack.SymbolReferenceStack[index] == symbolReference)
                    {
                        found = true;
                        break;
                    }
                }

                return found;
            }
            catch (Exception ex)
            {
                mElementTools.GeneralErrorHandler("IsPresentInAbbrevSymbolStack", ex);
                return false;
            }
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
                var stdDevSum = 0.0d;

                // Reset ErrorParams to clear any prior errors
                mErrorParams.Reset();

                // Reset Caution Description
                mCautionDescription = "";

                if (formula.Length > 0)
                {
                    var abbrevSymbolStack = new AbbrevSymbolStack();
                    formula = ParseFormulaRecursive(formula, computationStats, abbrevSymbolStack, expandAbbreviations, out stdDevSum, out _, valueForX);
                }

                // Copy computationStats to mComputationStatsSaved
                mComputationStatsSaved = computationStats.Clone();

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

                    return computationStats.TotalMass;
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
                                mErrorParams.ErrorId = 14;
                                mErrorParams.ErrorPosition = charIndex;
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
                                                adjacentNum = ParseNum(formula.Substring(parenthClose + 1), out numLength);
                                                CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                                if (adjacentNum < 0d)
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
                                mErrorParams.ErrorId = 3;
                                mErrorParams.ErrorPosition = charIndex;
                            }

                            prevSymbolReference = 0;
                            break;

                        case ')':
                        case '}': // )    Repeat a section of a formula
                            // Should have been skipped
                            // Unmatched closing parentheses
                            mErrorParams.ErrorId = 4;
                            mErrorParams.ErrorPosition = charIndex;
                            break;

                        case '-': // -
                            // Used to denote a leading coefficient
                            adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                            CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                            if (adjacentNum > 0d)
                            {
                                dashPos = charIndex + numLength;
                                dashMultiplier = adjacentNum * dashMultiplierPrior;
                                charIndex += numLength;
                            }
                            else if (Math.Abs(adjacentNum) < float.Epsilon)
                            {
                                // Cannot have 0 after a dash
                                mErrorParams.ErrorId = 5;
                                mErrorParams.ErrorPosition = charIndex + 1;
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
                                adjacentNum = ParseNum(formulaExcerpt, out numLength);
                                CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                if (adjacentNum >= 0d)
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
                                mErrorParams.ErrorId = 7;
                                mErrorParams.ErrorPosition = charIndex;
                            }
                            else
                            {
                                // Misplaced number
                                mErrorParams.ErrorId = 14;
                                mErrorParams.ErrorPosition = charIndex;
                            }

                            prevSymbolReference = 0;
                            break;

                        case '[': // [
                            if (char2.ToUpper() == "X")
                            {
                                if (char3 == "e")
                                {
                                    adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                                    CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);
                                }
                                else
                                {
                                    adjacentNum = valueForX;
                                    numLength = 1;
                                }
                            }
                            else
                            {
                                adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                                CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);
                            }

                            if (mErrorParams.ErrorId == 0)
                            {
                                if (insideBrackets)
                                {
                                    // No Nested brackets.
                                    mErrorParams.ErrorId = 16;
                                    mErrorParams.ErrorPosition = charIndex;
                                }
                                else if (adjacentNum < 0d)
                                {
                                    // No number after bracket
                                    mErrorParams.ErrorId = 12;
                                    mErrorParams.ErrorPosition = charIndex + 1;
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
                            adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                            CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                            if (adjacentNum >= 0d)
                            {
                                // Number following bracket
                                mErrorParams.ErrorId = 11;
                                mErrorParams.ErrorPosition = charIndex + 1;
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
                                mErrorParams.ErrorId = 15;
                                mErrorParams.ErrorPosition = charIndex;
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
                                    adjacentNum = ParseNum(formula.Substring(charIndex + symbolLength), out numLength);
                                    CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                    if (adjacentNum < 0d)
                                    {
                                        adjacentNum = 1d;
                                    }

                                    // Note that numLength = 0 if adjacentNum was -1 or otherwise < 0
                                    addonCount = numLength + symbolLength - 1;

                                    if (Math.Abs(adjacentNum) < float.Epsilon)
                                    {
                                        // Zero after element
                                        mErrorParams.ErrorId = 5;
                                        mErrorParams.ErrorPosition = charIndex + symbolLength;
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

                                    if (IsPresentInAbbrevSymbolStack(abbrevSymbolStack, symbolReference))
                                    {
                                        // Circular Reference: Can't have an abbreviation referencing an abbreviation that depends upon it
                                        // For example, the following is impossible:  Lor = C6H5Tal and Tal = H4O2Lor
                                        // Furthermore, can't have this either: Lor = C6H5Tal and Tal = H4O2Vin and Vin = S3Lor
                                        mErrorParams.ErrorId = 28;
                                        mErrorParams.ErrorPosition = charIndex;
                                    }
                                    // Found an abbreviation
                                    else if (caretPresent)
                                    {
                                        // Cannot have isotopic mass for an abbreviation, including deuterium
                                        if (char1.ToUpper() == "D" && char2 != "y")
                                        {
                                            // Isotopic mass used for Deuterium
                                            mErrorParams.ErrorId = 26;
                                            mErrorParams.ErrorPosition = charIndex;
                                        }
                                        else
                                        {
                                            mErrorParams.ErrorId = 24;
                                            mErrorParams.ErrorPosition = charIndex;
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
                                        adjacentNum = ParseNum(formula.Substring(charIndex + symbolLength), out numLength);
                                        CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                        if (adjacentNum < 0d)
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
                                                adjacentNum = ParseNum(formula.Substring(charIndex + symbolLength), out numLength);
                                                CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

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

                                                if (adjacentNum < 0d)
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
                                        mErrorParams.ErrorId = 18;
                                    }
                                    else
                                    {
                                        mErrorParams.ErrorId = 1;
                                    }

                                    mErrorParams.ErrorPosition = charIndex;
                                    break;
                            }

                            prevSymbolReference = symbolReference;
                            break;

                        case '^': // ^ (caret)
                            adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                            CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

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

                                if (adjacentNum >= 0d)
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
                                        mErrorParams.ErrorId = 22;
                                        mErrorParams.ErrorPosition = charIndex + numLength + 1;
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
                                        mErrorParams.ErrorId = 23;
                                        mErrorParams.ErrorPosition = charIndex + 1;
                                    }
                                    else
                                    {
                                        // No number following caret
                                        mErrorParams.ErrorId = 20;
                                        mErrorParams.ErrorPosition = charIndex + 1;
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
                                mErrorParams.ErrorId = 25;
                                mErrorParams.ErrorPosition = charIndex;
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
                        mErrorParams.ErrorId = 13;
                        mErrorParams.ErrorPosition = charIndex;
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
                mErrorParams.ErrorId = -10;
                mErrorParams.ErrorPosition = 0;

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
                    mErrorParams.ErrorId = 30;
                    mErrorParams.ErrorPosition = minusSymbolLoc;
                }

                if (mErrorParams.ErrorId != 0)
                    break;
            }

            // Adjust the overall charge
            computationStats.Charge -= computationStatsRightHalf.Charge;

            return formula;
        }

        /// <summary>
        /// Looks for a number and returns it if found
        /// </summary>
        /// <param name="work">Input</param>
        /// <param name="numLength">Output: length of the number</param>
        /// <param name="allowNegative"></param>
        /// <returns>
        /// Parsed number if found
        /// If not a number, returns a negative number for the error code and sets numLength = 0
        /// </returns>
        /// <remarks>
        /// Error codes:
        /// -1 = No number
        /// -2 =                                             (unused)
        /// -3 = No number at all or (more likely) no number after decimal point
        /// -4 = More than one decimal point
        /// </remarks>
        private double ParseNum(string work, out int numLength, bool allowNegative = false)
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
                numLength = 0; // No number found
                return -1;
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
                foundNum = (-3).ToString();
                numLength = 0;
            }
            else
            {
                // Check for more than one decimal point (. or ,)
                var decPtCount = foundNum.Count(c => c == ComputationOptions.DecimalSeparator);

                if (decPtCount > 1)
                {
                    // more than one decPtCount
                    foundNum = (-4).ToString();
                    numLength = 0;
                }
            }

            if (numLength < 0)
                numLength = (short)foundNum.Length;

            return NumberConverter.CDblSafe(foundNum);
        }

        internal void ResetErrorParams()
        {
            mErrorParams.Reset();
        }
    }
}
