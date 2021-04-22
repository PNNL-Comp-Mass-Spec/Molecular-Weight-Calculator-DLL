using System;
using System.Collections.Generic;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;

namespace UnitTests
{
    public class TestBase
    {
        // ReSharper disable CommentTypo

        // To see the Console.WriteLine() results for a series of test cases for a given Test, run NUnit from the command line.  For example:
        // cd "C:\Program Files (x86)\NUnit.org\nunit-console"
        // c:nunit3-console.exe --noresult --where "method =~ /Compute*/" unittests.dll

        // Ignore Spelling: cd, noresult, Pos

        // ReSharper restore CommentTypo

        protected const double MATCHING_MASS_EPSILON = 0.0000001;
        protected const double MATCHING_CHARGE_EPSILON = 0.05;

        /// <summary>
        /// When true, use Assert.AreEqual() to compare computed values to expected values
        /// </summary>
        protected bool mCompareValuesToExpected = true;

        /// <summary>
        /// When true, use Assert statements to compare text strings to expected values
        /// </summary>
        protected bool mCompareTextToExpected = true;

        protected MolecularWeightTool mAverageMassCalculator;
        protected MolecularWeightTool mMonoisotopicMassCalculator;

        /// <summary>
        /// Dictionary of unit test result writers
        /// </summary>
        protected Dictionary<UnitTestWriterType, UnitTestResultWriter> mTestResultWriters;

        /// <summary>
        /// WriteUpdatedTestCase uses this list to keep track of unit test method names
        /// </summary>
        private readonly SortedSet<string> mUnitTestMethods = new();

        /// <summary>
        /// Instantiate two copies of the Molecular Weight Calculator
        /// One using average masses and one using isotopic masses
        /// </summary>
        protected void Initialize()
        {
            mAverageMassCalculator = new MolecularWeightTool(ElementMassMode.Average);
            mMonoisotopicMassCalculator = new MolecularWeightTool(ElementMassMode.Isotopic);
        }

        protected void InitializeResultsWriter(UnitTestWriterType writerType, string resultsFileName)
        {
            mTestResultWriters.Add(writerType, new UnitTestResultWriter(resultsFileName));
        }

        protected void ReportParseData(UnitTestWriterType writerType, IFormulaParseData data)
        {
            if (!string.IsNullOrWhiteSpace(data.CautionDescription))
            {
                ShowAtConsoleAndLog(writerType, string.Format("  Caution: {0}", data.CautionDescription));
                ShowAtConsoleAndLog(writerType);
            }

            if (data.ErrorData.ErrorId == 0)
            {
                ShowAtConsoleAndLog(writerType, "  " + data.Formula);

                var stats = data.Stats;
                ShowAtConsoleAndLog(writerType, string.Format("  StDev:  {0}", stats.StandardDeviation));
                ShowAtConsoleAndLog(writerType, string.Format("  Mass:   {0}", stats.TotalMass));
                ShowAtConsoleAndLog(writerType, string.Format("  Charge: {0}", stats.Charge));
            }
            else
            {
                ShowAtConsoleAndLog(writerType, "  " + data.FormulaOriginal);
                ShowAtConsoleAndLog(writerType, string.Format("  ErrorId:          {0}", data.ErrorData.ErrorId));
                ShowAtConsoleAndLog(writerType, string.Format("  ErrorPos:         {0}", data.ErrorData.ErrorPosition));
                ShowAtConsoleAndLog(writerType, string.Format("  ErrorChar:        {0}", data.ErrorData.ErrorCharacter));
                ShowAtConsoleAndLog(writerType, string.Format("  ErrorDescription: {0}", data.ErrorData.ErrorDescription));
                ShowAtConsoleAndLog(writerType);

                string markedFormula;
                var formula = data.Formula;
                var position = data.ErrorData.ErrorPosition;
                if (position >= formula.Length)
                {
                    markedFormula = formula + "''";
                }
                else if (position == formula.Length - 1)
                {
                    markedFormula = formula.Substring(0, position) + "'" + formula[position] + "'";
                }
                else
                {
                    markedFormula = formula.Substring(0, position) + "'" + formula[position] + "'" + formula.Substring(position + 1);
                }

                ShowAtConsoleAndLog(writerType, string.Format("  Highlight: {0}", markedFormula));
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        protected void ReportParseData(UnitTestWriterType writerType, MolecularWeightTool mwt)
        {
            // Use this in comparison to the other ReportParseData method... (results should be the same)
            var compound = mwt.Compound;
            if (!string.IsNullOrWhiteSpace(compound.CautionDescription))
            {
                ShowAtConsoleAndLog(writerType, string.Format("  Caution: {0}", compound.CautionDescription));
                ShowAtConsoleAndLog(writerType);
            }

            if (mwt.ErrorId == 0)
            {
                ShowAtConsoleAndLog(writerType, "  " + compound.FormulaCapitalized);

                ShowAtConsoleAndLog(writerType, string.Format("  StDev:  {0}", compound.StandardDeviation));
                ShowAtConsoleAndLog(writerType, string.Format("  Mass:   {0}", compound.GetMass(false)));
                ShowAtConsoleAndLog(writerType, string.Format("  Charge: {0}", compound.Charge));
            }
            else
            {
                ShowAtConsoleAndLog(writerType, "  " + compound.Formula);
                ShowAtConsoleAndLog(writerType, string.Format("  ErrorId:          {0}", mwt.ErrorId));
                ShowAtConsoleAndLog(writerType, string.Format("  ErrorPos:         {0}", mwt.ErrorPosition));
                ShowAtConsoleAndLog(writerType, string.Format("  ErrorChar:        {0}", mwt.ErrorCharacter));
                ShowAtConsoleAndLog(writerType, string.Format("  ErrorDescription: {0}", mwt.ErrorDescription));
                ShowAtConsoleAndLog(writerType);

                string markedFormula;
                var formula = compound.FormulaCapitalized;
                var position = mwt.ErrorPosition;
                if (position >= formula.Length)
                {
                    markedFormula = formula + "''";
                }
                else if (position == formula.Length - 1)
                {
                    markedFormula = formula.Substring(0, position) + "'" + formula[position] + "'";
                }
                else
                {
                    markedFormula = formula.Substring(0, position) + "'" + formula[position] + "'" + formula.Substring(position + 1);
                }

                ShowAtConsoleAndLog(writerType, string.Format("  Highlight: {0}", markedFormula));
            }
        }

        protected void ShowAtConsoleAndLog(UnitTestWriterType writerType, string text = "")
        {
            if (writerType != UnitTestWriterType.NoWriter)
            {
                if (!mTestResultWriters[writerType].FilePathShown)
                {
                    Console.WriteLine("Unit test results file path: ");
                    Console.WriteLine(mTestResultWriters[writerType].ResultsFile.FullName);
                    Console.WriteLine();

                    mTestResultWriters[writerType].FilePathShown = true;
                }

                if (writerType != UnitTestWriterType.UniModFormulaWriter || !string.IsNullOrWhiteSpace(text))
                {
                    mTestResultWriters[writerType].WriteLine(text);
                }
            }

            Console.WriteLine(text);
        }

        protected string ValueToString(double value, byte digitsAfterDecimal = 5)
        {
            return PRISM.StringUtilities.DblToString(value, digitsAfterDecimal);
        }

        /// <summary>
        /// Append C# code that can be used to update test cases with new masses
        /// </summary>
        /// <param name="callingMethod"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        protected void WriteUpdatedTestCase(string callingMethod, string format, params object[] arg)
        {
            if (!mUnitTestMethods.Contains(callingMethod))
            {
                // Append a blank line
                mTestResultWriters[UnitTestWriterType.UnitTestCaseWriter].WriteLine(string.Empty);

                mUnitTestMethods.Add(callingMethod);
            }

            var testCaseCode = string.Format(format, arg);
            mTestResultWriters[UnitTestWriterType.UnitTestCaseWriter].WriteLine("{0,-30} {1}", callingMethod, testCaseCode);
        }
    }
}
