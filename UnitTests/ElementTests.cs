﻿using System;
using System.Collections.Generic;
using System.Text;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ElementTests
    {
        // ReSharper disable CommentTypo

        // To see the Console.WriteLine() results for a series of test cases for a given Test, run NUnit from the command line.  For example:
        // cd "C:\Program Files (x86)\NUnit.org\nunit-console"
        // c:nunit3-console.exe --noresult --where "method =~ /Compute*/" unittests.dll

        // Ignore Spelling: arkcas, Bpy, cd, Da, Gly, Leu, NOCH4, noresult, Nz, Pos, rkcas, sipsclarkcas, Tyr, UniMod

        // ReSharper restore CommentTypo

        private const double MATCHING_MASS_EPSILON = 0.0000001;
        private const double MATCHING_CHARGE_EPSILON = 0.05;

        /// <summary>
        /// When true, use Assert.AreEqual() to compare computed values to expected values
        /// </summary>
        private bool mCompareValuesToExpected = true;

        /// <summary>
        /// When true, use Assert statements to compare text strings to expected values
        /// </summary>
        private bool mCompareTextToExpected = true;

        private MolecularWeightTool mAverageMassCalculator;
        private MolecularWeightTool mMonoisotopicMassCalculator;

        /// <summary>
        /// Dictionary of unit test result writers
        /// </summary>
        private Dictionary<UnitTestWriterType, UnitTestResultWriter> mTestResultWriters;

        private bool mUniModHeaderWritten;

        /// <summary>
        /// Instantiate two copies of the Molecular Weight Calculator
        /// One using average masses and one using isotopic masses
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            mAverageMassCalculator = new MolecularWeightTool(ElementMassMode.Average);
            mMonoisotopicMassCalculator = new MolecularWeightTool(ElementMassMode.Isotopic);

            mTestResultWriters = new Dictionary<UnitTestWriterType, UnitTestResultWriter>();

            InitializeResultsWriter(UnitTestWriterType.ComputeMass, "UnitTestResults_ComputeMass.txt");
            InitializeResultsWriter(UnitTestWriterType.StressTest, "UnitTestResults_ComputeMassStressTest.txt");
            InitializeResultsWriter(UnitTestWriterType.ConvertToEmpirical, "UnitTestResults_ConvertToEmpirical.txt");
            InitializeResultsWriter(UnitTestWriterType.CircularReferenceHandling, "UnitTestResults_CircularReferenceHandling.txt");
            InitializeResultsWriter(UnitTestWriterType.ExpandAbbreviations, "UnitTestResults_ExpandAbbreviations.txt");
            InitializeResultsWriter(UnitTestWriterType.PercentComposition, "UnitTestResults_PercentComposition.txt");
            InitializeResultsWriter(UnitTestWriterType.UniModFormulaWriter, "UnitTestResults_UniModFormulas.txt");
            InitializeResultsWriter(UnitTestWriterType.UnitTestCaseWriter, "UnitTestCases.txt");

            mUniModHeaderWritten = false;

            mCompareValuesToExpected = true;
            mCompareTextToExpected = true;
        }

        private void InitializeResultsWriter(UnitTestWriterType writerType, string resultsFileName)
        {
            mTestResultWriters.Add(writerType, new UnitTestResultWriter(resultsFileName));
        }

        [Test]
        [TestCase("BrCH2(CH2)7CH2Br", 286.051, 283.97752578014)]
        [TestCase("FeCl3-6H2O", 270.285, 268.90488248018)]
        [TestCase("Co(Bpy)(CO)4", 327.161194, 326.98160103498)]
        [TestCase("^13C6H6-.1H2O", 85.8495, 85.848006661783)]
        [TestCase("HGlyLeuTyrOH", 351.403, 351.17942091689)]
        [TestCase("BrCH2(CH2)7CH2Br>CH8", 265.976, 263.9149255223)]
        [TestCase("C6H6-H2O-2ZnHgMg-U", 914.71191, 913.87795749141)]
        [TestCase("57FeCl3-6H2O", 271.44, 269.96994615018)]              // In VB6 "2FeCl3-6H2O" meant "2(FeCl3)-6H2O"; in C#, 2Fe gets auto-changed to ^2Fe
        [TestCase("C6H5Cl3>H3Cl2>HCl", 145.99, 144.96118046069)]
        [TestCase("D10>H10", 10.061018, 10.0627676777)]                  // In VB6, D was defined as "^2.014H"; in C#, it is "^2.0141018H"
        public void ComputeMass(string formula, double expectedAvgMass, double expectedMonoMass, double matchTolerance = MATCHING_MASS_EPSILON)
        {
            var averageMass = mAverageMassCalculator.ComputeMass(formula);
            var isotopicMass = mMonoisotopicMassCalculator.ComputeMass(formula);

            WriteUpdatedTestCase("ComputeMass", "[TestCase(\"{0}\", {1}, {2})]", formula, averageMass, isotopicMass);

            ShowAtConsoleAndLog(UnitTestWriterType.ComputeMass, string.Format(
                "{0,-22} -> {1,-20}: {2,12:F8} Da (average) and  {3,12:F8} Da (isotopic)",
                formula, mMonoisotopicMassCalculator.Compound.FormulaCapitalized, averageMass, isotopicMass));

            if (!mCompareValuesToExpected)
                return;

            Assert.AreEqual(expectedAvgMass, averageMass, matchTolerance, "Actual mass does not match expected average mass");
            Assert.AreEqual(expectedMonoMass, isotopicMass, matchTolerance, "Actual mass does not match expected isotopic mass");
        }

        // ReSharper disable StringLiteralTypo
        [Test]
        [TestCase("^13C6H6-H2O", 102.063, 102.05751487741, -6)]
        [TestCase("^13C6H6[.1H2O]", 85.8495, 85.848006661783, -6)]
        [TestCase("^13C6H6[H2O]", 102.063, 102.05751487741, -6, true)]
        [TestCase("^13C6H6[H2O]2", 120.078, 120.06807956144, -6, true)]
        [TestCase("13C6H6-6H2O", 192.15813, 192.13046829756, -6)]           // In VB6 "13C6H6" meant "13(C6H6)"; in C# it gets auto-updated to ^13.003355C6H6
        [TestCase("13(C6H6)-6H2O", 1123.572, 1122.67374061812, 222)]
        [TestCase("^13.003355C6H6-6H2O", 192.15813, 192.13046829756, -6)]
        [TestCase("13C4C2H6-6H2O", 190.17342, 190.12375829756, 0)]
        [TestCase("^13.003355C4C2H6-6H2O", 190.17342, 190.12375829756, 0)]
        [TestCase("Et2O", 74.123, 74.07316494187, 0)]
        [TestCase("DCH2", 16.0411018, 16.02975186446, 3)]
        [TestCase("CaOCH4(CH2)7Br", 250.213, 249.01669366271, 13)]
        [TestCase("canyoch4(ch2)7br", 353.12584, 351.92560796714, 13)]
        [TestCase("F(DCH2)7Br", 211.190115763, 210.12500381395, 19)]
        [TestCase("Pt(CH2)7Br-[5Ca123456789]", 24739506320.887, 24668266112.326275, 1234567936)]
        [TestCase("Pt(CH2)7Br-[5Ca123456789H]", 24739506325.927002, 24668266117.3654, 1234567936)]
        [TestCase("-[2.5CaH]", 102.715, 102.426039738075, 2.5)]
        [TestCase("^18CaH", 19.008, 19.00782503223, -1)]
        [TestCase("^23CaH", 24.008, 24.00782503223, -1)]
        [TestCase("^42CaH", 43.008, 43.00782503223, -1)]
        [TestCase("^98CaH", 99.008, 99.00782503223, -1)]
        [TestCase("(CH2)7Br-3.2CaH", 309.5682, 308.133218915956, 16.2)]
        [TestCase("[2CaOH]-K", 153.2683, 152.894367516, 3)]
        [TestCase("K-[2CaOH]", 153.2683, 152.894367516, 3)]
        [TestCase("H2Ca2KO2", 153.2683, 152.894367516, 3)]
        [TestCase("H2O^23.9Ca", 41.915, 41.91056468403, 0)]
        [TestCase("^13C6H6", 84.048, 84.04695019338, -6)]
        [TestCase("C6H6", 78.114, 78.04695019338, 8)]
        [TestCase("C6H3^19.8Ar", 94.89, 94.82347509669, 11)]
        [TestCase("C6H3^19.8arpbbb", 323.71, 324.81873831669, 19)]
        [TestCase("C6H3^19.88Ar4Pb>Ar", 321.86, 322.55774447299, 13)]
        [TestCase("C6>C4", 24.022, 24, 4)]
        [TestCase("HGly5.3Leu2.2Tyr0.03OH", 574.23788, 573.901163012462, 0)]
        [TestCase("HGly5.3leu2.2tyr0.03oh", 574.23788, 573.901163012462, 0)]
        [TestCase("HHeLiBeBCNOFNeNaMgAlSiPSClArKCaScTiVCrMnFeCoNiCuZnGaGeAsSeBrKrRbSrYZrNbMoTcRuRhPdAgCdInSnSbTeIXe", 3357.820167941, 3356.97101749416, 80)]
        [TestCase("hhelibebcnofnenamgalsipsClArKcasctivcrmnfeconicuzngageassebrkrrbsryzrnbmotcrurhpdagcdinsnsbteixe", 3617.002167941, 3616.14506348973, 83)]
        [TestCase("CsBaLaCePrNdPmSmEuGdTbDyHoErTmYbLuHfTaWReOsIrPtAuHgTlPbBiPoAtRnFrRaAcThPaUNpPuAmCmBkCfEsFmMdNoLr", 9710.68462196, 9720.269822671, 153)]
        [TestCase("csbalaceprndpmsmeugdtbdyhoertmybluhftawreosirptauhgtlpbbipoatrnfrraacthpaunppuamcmbkcfesfmmdnolr", 9710.68462196, 9720.269822671, 153)]
        [TestCase("cdinsnsbteixecsbalaceprndpm", 1832.90305196, 1835.6787480906, 22)]
        [TestCase("CdInSnSbTeIXeCsBaLaCePrNdPm", 1832.90305196, 1835.6787480906, 22)]
        [TestCase("sips cl arkcas", 277.755061998, 276.75236403697, -1)]                // Test ignoring whitespace and characters outside of a-z, A-Z, 0-9, []{}().^>
        // ReSharper restore StringLiteralTypo
        public void ComputeMassStressTest(string formula, double expectedAvgMass, double expectedMonoMass, double expectedCharge, bool bracketsAsParentheses = false)
        {
            ShowAtConsoleAndLog(UnitTestWriterType.StressTest);
            ShowAtConsoleAndLog(UnitTestWriterType.StressTest, "Formula: " + formula);
            ShowAtConsoleAndLog(UnitTestWriterType.StressTest);

            ShowAtConsoleAndLog(UnitTestWriterType.StressTest, "Average Mass:");
            mAverageMassCalculator.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var averageMass = mAverageMassCalculator.ComputeMassExtra(formula, out var parseDataAvg);

            ReportParseData(UnitTestWriterType.StressTest, parseDataAvg);
            Assert.Greater(averageMass, 0);

            var compareValues = mCompareValuesToExpected && (expectedAvgMass > 0 || expectedMonoMass > 0 || expectedCharge != 0);
            if (compareValues)
            {
                Assert.AreEqual(expectedAvgMass, averageMass, MATCHING_MASS_EPSILON, "Actual mass does not match expected average mass");
                Assert.AreEqual(expectedCharge, parseDataAvg.Charge, MATCHING_CHARGE_EPSILON, "Actual charge does not match expected charge");
            }

            ShowAtConsoleAndLog(UnitTestWriterType.StressTest);
            ShowAtConsoleAndLog(UnitTestWriterType.StressTest, "Isotopic Mass:");
            mMonoisotopicMassCalculator.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var isotopicMass = mMonoisotopicMassCalculator.ComputeMassExtra(formula, out var parseDataIso);
            ReportParseData(UnitTestWriterType.StressTest, parseDataIso);

            Assert.Greater(isotopicMass, 0);
            if (compareValues)
            {
                Assert.AreEqual(expectedMonoMass, isotopicMass, MATCHING_MASS_EPSILON, "Actual mass does not match expected isotopic mass");
                Assert.AreEqual(expectedCharge, parseDataIso.Charge, MATCHING_CHARGE_EPSILON, "Actual charge does not match expected charge");
            }

            var optionalBracketsArgument = bracketsAsParentheses ? ", true" : string.Empty;

            WriteUpdatedTestCase("ComputeMassStressTest",
                "[TestCase(\"{0}\", {1}, {2}, {3}{4})]",
                formula, averageMass, isotopicMass, parseDataIso.Charge, optionalBracketsArgument);

            // ShowAtConsoleAndLog(UnitTestWriterType.StressTest);
            // ReportParseData(UnitTestWriterType.StressTest, mMonoisotopicMassCalculator);
        }

        [Test]
        [TestCase(5, "^13C6H6-H2O0", 11, "0 directly after an element")]
        [TestCase(14, "^13C6H6[.1H2O]", 8, "Misplaced number", true)] // TODO: VB6: '[', new: '.'; - outputting the '[' or '(' might not be simple in the new
        [TestCase(12, "^13C6H6[H2O]", 7, "number must be present after a bracket")]
        [TestCase(11, "^13C6H6[.1H2O]2", 14, "follow left brackets, not right brackets")]
        [TestCase(14, "^13C6H6(.1H2O)2", 8, "Misplaced number")] // TODO: VB6: '(', new: '.'
        [TestCase(13, "^13C6H6[.1H2O", 7, "Missing closing bracket")] // TODO: VB6: '~'(end), new: '['
        [TestCase(5, "^13C6H6-0H2O", 8, "0 directly after an element or dash")]
        [TestCase(3, "^13C6H6(H2O", 7, "Missing closing parentheses")]
        [TestCase(4, "^13C6H6H2O)", 10, "Unmatched parentheses")]
        [TestCase(4, "^13C6H6(H2O}", 11, "Unmatched parentheses")] // TODO: Succeeds now, needs to fail with mismatched parentheses.
        [TestCase(30, "C6H5>H6", 4, "Invalid formula subtraction")]
        [TestCase(24, "^32PheOH", 3, "masses are not allowed for abbreviations")]
        [TestCase(12, "^13C6H6-H2O.", 11, "present after a bracket and/or after the decimal")]
        [TestCase(27, "^13C6H6.0.1H2O", 6, "more than one decimal point")]
        [TestCase(27, "^13C6H6-3.0.1H2O", 8, "more than one decimal point")]
        [TestCase(27, "^13C6H6[3.0.1H2O]", 8, "more than one decimal point")]
        [TestCase(13, "^13C6H6[3.5", 7, "missing closing bracket")]
        //[TestCase(0, "", 0, "")]
        [TestCase(30, "C6H6>", 4, "Invalid formula subtraction")] // TODO: ErrorChar: VB6 (no error), new '>'
        [TestCase(30, ">Ca", 0, "Invalid formula subtraction")]
        [TestCase(25, "^13C6H6-3", 8, "present after the leading coefficient")]
        //[TestCase(0, "", 0, "")]
        [TestCase(1, "CaNzOCH4(CH2)7Br", 3, "Unknown")] // TODO: ErrorChar capitalization?
        [TestCase(1, "CaNOCH4(CgH2)7Br", 9, "Unknown")] // TODO: ErrorChar capitalization?
        [TestCase(3, "F(D(CH2)7Br", 1, "Missing closing parentheses")]
        [TestCase(4, "FD)(CH2)7Br", 2, "Unmatched parentheses")]
        [TestCase(4, "FD(CH2}7Br", 6, "Unmatched parentheses")] // TODO: Succeeds now, needs to fail with mismatched parentheses.
        [TestCase(5, "Pt(C0H2)7Br", 4, "0 directly after an element or dash ")]
        [TestCase(5, "Pt(CH2)7Br0", 10, "0 directly after an element or dash")]
        [TestCase(11, "(CH2)7Br-[2CaH]5", 15, "Numbers should follow left brackets, not right brackets")]
        [TestCase(12, "-[CaH]", 1, "number must be present after a bracket")] // TODO: VB6: 'C', new: '['
        [TestCase(12, "[.CaH]", 0, "number must be present after a bracket")]
        [TestCase(20, "^.CaH", 0, "number following the caret")]
        [TestCase(13, "(CH2)7Br-[2CaH", 9, "Missing closing bracket")]
        [TestCase(15, "(CH2)7Br-2CaH]", 13, "unmatched bracket")]
        [TestCase(16, "[2Ca[O]H]-", 4, "nested brackets")]
        [TestCase(20, "^CaH", 0, "number following the caret")] // TODO: VB6: 'C', new: '^'
        [TestCase(20, "^23CaH^", 6, "number following the caret")] // TODO: VB6: '~'(end), new: '^'
        [TestCase(22, "^23CaH^6", 7, "present after the isotopic mass after the caret")] // TODO: VB6: '~'(end), new: '6' (best would probably be ^)
        [TestCase(23, "H2O^-23Ca", 4, "Negative isotopic masses are not allowed")]
        [TestCase(26, "C6H3^8D3", 6, "masses are not allowed for abbreviations; D is an abbreviation")]
        [TestCase(24, "C6H3^8Gly3", 6, "masses are not allowed for abbreviations")]
        [TestCase(27, "C6H3^19.88.9ArPb", 5, "more than one decimal point")]
        [TestCase(30, "C6H3^19.88Ar2Pb>Ar", 15, "Invalid formula subtraction")]
        [TestCase(30, "C6H6>Ca", 4, "Invalid formula subtraction")]
        //[TestCase(0, "", 0, "")]
        [TestCase(1, "hgly5.3leu2.2tyr0.03oh", 2, "unknown")]
        [TestCase(1, "sipsclarkcas", 7, "unknown")]
        [TestCase(1, "sips cl a rkcas", 8, "unknown")] // Whitespace maintained when reporting element/abbreviation parsing errors
        [TestCase(30, "C6H3 ^19.88Ar2Pb>Ar", 15, "Invalid formula subtraction")]
        public void ComputeMassErrorTests(int errorIdExpected, string formula, int expectedPosition, string messageExcerpt, bool bracketsAsParentheses = false)
        {
            mAverageMassCalculator.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var averageMass = mAverageMassCalculator.ComputeMassExtra(formula, out var parseData);
            ReportParseData(UnitTestWriterType.NoWriter, parseData);

            if (mCompareValuesToExpected)
            {
                Assert.AreEqual(errorIdExpected, parseData.ErrorData.ErrorId);
                Assert.AreEqual(expectedPosition, parseData.ErrorData.ErrorPosition);
            }

            if (mCompareTextToExpected)
            {
                Assert.GreaterOrEqual(
                    parseData.ErrorData.ErrorDescription.IndexOf(messageExcerpt, StringComparison.OrdinalIgnoreCase), 0,
                    "excerpt not found in reported message statement");
            }
        }

        [Test]
        [TestCase("bi", "bismuth")]
        [TestCase("bk", "berkelium")]
        [TestCase("bu", "butyl group")]
        [TestCase("cd", "cadmium")]
        [TestCase("cf", "californium")]
        [TestCase("co", "cobalt")]
        [TestCase("cs", "cesium")]
        [TestCase("cu", "copper")]
        [TestCase("dy", "dysprosium")]
        [TestCase("hf", "hafnium")]
        [TestCase("ho", "holmium")]
        [TestCase("in", "indium")]
        [TestCase("nb", "niobium")]
        [TestCase("nd", "neodymium")]
        [TestCase("ni", "nickel")]
        [TestCase("no", "nobelium")]
        [TestCase("np", "neptunium")]
        [TestCase("os", "osmium")]
        [TestCase("pd", "palladium")]
        [TestCase("ph", "phenyl")]
        [TestCase("pu", "plutonium")]
        [TestCase("py", "pyridine")]
        [TestCase("sb", "antimony")]
        [TestCase("sc", "scandium")]
        [TestCase("si", "silicon")]
        [TestCase("sn", "sulfur-nitrogen")]
        [TestCase("TI", "tritium-iodine")]
        [TestCase("yb", "yttrium-boron")]
        [TestCase("BPY", "boron-phosphorus-yttrium")]
        [TestCase("BPy", "boron-pyridine")]
        [TestCase("bpy", "bipyridine")]
        [TestCase("cys", "cysteine")]
        [TestCase("his", "histidine")]
        [TestCase("hoh", "holmium")]
        [TestCase("hyp", "hydroxyproline")]
        [TestCase("Oac", "acetate")]
        [TestCase("oac", "acetate")]
        [TestCase("Pro", "proline")]
        [TestCase("prO", "praseodymium-oxygen")]
        [TestCase("val", "valine")]
        [TestCase("vAl", "vanadium-aluminum")]
        public void ComputeMassCautionMessageTests(string formula, string cautionExcerpt)
        {
            var averageMass = mAverageMassCalculator.ComputeMassExtra(formula, out var parseData);
            ReportParseData(UnitTestWriterType.NoWriter, parseData);

            if (mCompareTextToExpected)
            {
                Assert.GreaterOrEqual(parseData.CautionDescription.IndexOf(cautionExcerpt, StringComparison.OrdinalIgnoreCase), 0,
                    "excerpt not found in reported caution statement");
            }
        }

        [Test]
        [TestCase("BrCH2(CH2)7CH2Br", "H=6.34292 (±0.0009), C=37.79011 (±0.003), Br=55.86696 (±0.002)")]
        [TestCase("FeCl3-6H2O", "H=4.47528 (±0.0006), O=35.51584 (±0.002), Cl=39.34736 (±0.006), Fe=20.66152 (±0.001)")]
        [TestCase("Co(Bpy)(CO)4", "H=2.46484 (±0.0003), C=51.39791 (±0.004), N=8.56275 (±0.0003), O=19.561 (±0.0005), Co=18.0135 (±0.0002)")]
        [TestCase("^13C6H6-.1H2O", "H=7.27972 (±0.001), C=90.85667 (±0.0004), O=1.86361 (±4E-05)")]
        [TestCase("HGlyLeuTyrOH", "H=7.17125 (±0.001), C=58.10622 (±0.005), N=11.95807 (±0.0004), O=22.76446 (±0.0006)")]
        [TestCase("BrCH2(CH2)7CH2Br>CH8", "H=3.78982 (±0.0005), C=36.12657 (±0.003), Br=60.08362 (±0.003)")]
        [TestCase("C6H5Cl3>H3Cl2>HCl", "H=2.07137 (±0.0003), C=49.36366 (±0.005), Cl=48.56497 (±0.008)")]
        [TestCase("H2C3H5C2HNO3CO2", "H=4.63097 (±0.0006), C=41.38585 (±0.004), N=8.0439 (±0.0003), O=45.93929 (±0.001)")]
        [TestCase("D", "H=100.0 (±0)")]
        [TestCase("D2CH", "H=29.5427 (±0.002), C=70.4573 (±0.007)")]
        [TestCase("D2CH^3H3", "H=53.88756 (±0.002), C=46.11244 (±0.004)")]
        [TestCase("D2C^13C5H^3H3", "H=15.4164 (±0.0002), C=84.5836 (±0.0009)")]
        public void ComputePercentCompositionTests(string formula, string expectedPercentCompositionByElement)
        {
            mAverageMassCalculator.Compound.Formula = formula;
            var percentComposition1 = mAverageMassCalculator.Compound.GetPercentCompositionForAllElements();

            mMonoisotopicMassCalculator.Compound.Formula = formula;
            var percentComposition2 = mAverageMassCalculator.Compound.GetPercentCompositionForAllElements();

            ShowAtConsoleAndLog(UnitTestWriterType.PercentComposition, formula);

            var percentCompositionData = new StringBuilder();
            foreach (var item in percentComposition1)
            {
                ShowAtConsoleAndLog(UnitTestWriterType.PercentComposition, string.Format("{0,-3} {1}", item.Key, item.Value));

                if (percentCompositionData.Length > 0)
                {
                    percentCompositionData.Append(", ");
                }

                percentCompositionData.AppendFormat("{0}={1}", item.Key, item.Value);
            }

            ShowAtConsoleAndLog(UnitTestWriterType.PercentComposition);

            if (mCompareTextToExpected && !string.IsNullOrWhiteSpace(expectedPercentCompositionByElement))
            {
                ComparePercentCompositionValues(formula, expectedPercentCompositionByElement, percentComposition1);
                ComparePercentCompositionValues(formula, expectedPercentCompositionByElement, percentComposition2);
            }

            WriteUpdatedTestCase("ComputePercentCompositionTests", "[TestCase(\"{0}\", \"{1}\")]", formula, percentCompositionData.ToString());
        }

        private static void ComparePercentCompositionValues(
            string formula,
            string expectedPercentCompositionByElement,
            IReadOnlyDictionary<string, string> computedPercentComposition)
        {
            foreach (var item in expectedPercentCompositionByElement.Split(','))
            {
                var itemParts = item.Split(new[] { '=' }, 2);

                var element = itemParts[0].Trim();
                var percentComposition = itemParts[1].Trim();

                if (!computedPercentComposition.TryGetValue(element, out var computedValue))
                {
                    Assert.Fail("Actual percent composition data does not have expected element: {0}", item);
                }

                Assert.AreEqual(computedValue, percentComposition, "Unexpected percent composition for formula {0}, element: {1}", formula, element);
            }
        }

        [Test]
        [TestCase("BrCH2(CH2)7CH2Br", "C9H18Br2")]
        [TestCase("FeCl3-6H2O", "H12Cl3FeO6")]
        [TestCase("Co(Bpy)(CO)4", "C14H8CoN2O4")]
        [TestCase("^13C6H6-.1H2O", "^13C6H6.2O0.1")]
        [TestCase("HGlyLeuTyrOH", "C17H25N3O5")]
        [TestCase("BrCH2(CH2)7CH2Br>CH8", "C8H10Br2")]
        [TestCase("C6H5Cl3>H3Cl2>HCl", "C6H3Cl2")]
        [TestCase("H2C3H5C2HNO3CO2", "C6H8NO5")]
        [TestCase("D10C6>H10", "C6", false)]        // This is not working properly; gives C6^2.0141018H10 instead of C6; even better would be C6^1.00617H10
        [TestCase("D", "^2.0141018H")]
        [TestCase("D2CH", "C^2.0141018H2H")]
        [TestCase("D2CH^3H3", "C^2.0141018H2^3H3H")]
        [TestCase("D2C^13C5H^3H3", "^13C5C^2.0141018H2^3H3H")]
        public void ConvertToEmpiricalTests(string formula, string expectedEmpirical, bool compareToExpected = true)
        {
            mAverageMassCalculator.Compound.Formula = formula;
            var empirical1 = mAverageMassCalculator.Compound.ConvertToEmpirical();

            mMonoisotopicMassCalculator.Compound.Formula = formula;
            var empirical2 = mAverageMassCalculator.Compound.ConvertToEmpirical();

            ShowAtConsoleAndLog(UnitTestWriterType.ConvertToEmpirical, string.Format("{0,-20} -> {1,-20}", formula, empirical1));

            if (mCompareTextToExpected && compareToExpected)
            {
                Assert.AreEqual(expectedEmpirical, empirical1, "Unexpected result for {0}", formula);
                Assert.AreEqual(expectedEmpirical, empirical2, "Unexpected result for {0}", formula);
            }

            var optionalCompareArgument = compareToExpected ? string.Empty : ", false";

            WriteUpdatedTestCase("ConvertToEmpiricalTests", "[TestCase(\"{0}\", \"{1}\"{2})]", formula, empirical1, optionalCompareArgument);
        }

        [Test]
        public void CircularReferenceHandlingTests()
        {
            var mwt = new MolecularWeightTool(ElementMassMode.Average);
            var mass = mwt.ComputeMassExtra("TryCo", out var parseData);

            //ReportParseData(mCircularReferenceWriter, parseData);

            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            var error = mwt.SetAbbreviation("Try", "FailH2O2", 1, false);

            // Error expected (unknown element):
            Assert.AreEqual(1, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(mCircularReferenceWriter, parseData);

            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Fail", "TryCaOH", 1, false);

            // Error expected (circular reference), A->B->A:
            Assert.AreEqual(28, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(mCircularReferenceWriter, parseData);

            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Fail", "TrierCaOH", 1, false);

            // Error expected (invalid abbreviation, due to bad dependency):
            Assert.AreEqual(32, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(mCircularReferenceWriter, parseData);

            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Trier", "TryOH", 1, false);

            // Error expected (circular reference), A->B->C->A:
            Assert.AreEqual(28, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(mCircularReferenceWriter, parseData);

            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Trier", "Trier", 1, false);

            // Error expected (circular reference), A->A:
            Assert.AreEqual(28, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(mCircularReferenceWriter, parseData);

            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Trier", "C6H12O6", 1, false);

            // No error expected:
            Assert.AreEqual(0, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            ReportParseData(UnitTestWriterType.CircularReferenceHandling, parseData);

            // No error expected:
            Assert.AreEqual(0, parseData.ErrorData.ErrorId);

            if (mCompareValuesToExpected)
            {
                Assert.AreEqual(330.188194, mass, MATCHING_MASS_EPSILON);
            }

            mass = mwt.ComputeMassExtra("TryCoFail", out parseData);
            ReportParseData(UnitTestWriterType.CircularReferenceHandling, parseData);

            // No error expected:
            Assert.AreEqual(0, parseData.ErrorData.ErrorId);

            if (mCompareValuesToExpected)
            {
                Assert.AreEqual(567.429194, mass, MATCHING_MASS_EPSILON);
            }
        }

        [Test]
        [TestCase("BrCH2(CH2)7CH2Br", "BrCH2(CH2)7CH2Br")]
        [TestCase("FeCl3-6H2O", "FeCl3-6H2O")]
        [TestCase("Co(Bpy)(CO)4", "Co(C10H8N2)(CO)4")]
        [TestCase("^13C6H6-.1H2O", "^13C6H6-.1H2O")]
        [TestCase("HGlyLeuTyrOH", "HC2H3NOC6H11NOC9H9NO2OH")]
        [TestCase("BrCH2(CH2)7CH2Br>CH8", "BrCH2(CH2)7CH2Br>CH8")]
        [TestCase("C6H5Cl3>H3Cl2>HCl", "C6H5Cl3>H3Cl2>HCl")]
        [TestCase("D10>H10", "(^2.0141018H)10>H10")]                   // In VB6, D was defined as "^2.014H"; in C#, it is "^2.0141018H"
        public void TestExpandAbbreviations(string formula, string expectedExpandedFormula)
        {
            var isotopicMass = mMonoisotopicMassCalculator.ComputeMass(formula);
            var expandedFormula = mMonoisotopicMassCalculator.Compound.ExpandAbbreviations();

            ShowAtConsoleAndLog(UnitTestWriterType.ExpandAbbreviations, string.Format(
                "{0,-20} -> {1,-23}: {2,12:F8} Da (isotopic)",
                formula, expandedFormula, isotopicMass));

            if (mCompareTextToExpected)
            {
                Assert.AreEqual(expectedExpandedFormula, expandedFormula, "New formula does not match expected");
            }

            WriteUpdatedTestCase("TestExpandAbbreviations", "[TestCase(\"{0}\", \"{1}\")]", formula, expandedFormula);
        }

        private void ReportParseData(UnitTestWriterType writerType, IFormulaParseData data)
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
        private void ReportParseData(UnitTestWriterType writerType, MolecularWeightTool mwt)
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

        private void ShowAtConsoleAndLog(UnitTestWriterType writerType, string text = "")
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

        /// <summary>
        /// Append C# code that can be used to update test cases with new masses
        /// </summary>
        /// <param name="callingMethod"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        private void WriteUpdatedTestCase(string callingMethod, string format, params object[] arg)
        {
            var testCaseCode = string.Format(format, arg);
            mTestResultWriters[UnitTestWriterType.UnitTestCaseWriter].WriteLine("{0,-30} {1}", callingMethod, testCaseCode);
        }

        [Test]
        [TestCase("S(-1)", -32.065, -31.972071, "^0H>S1", 0, 0)]
        [TestCase("H(-5) C(-1) N(-1)", -31.0571, -31.042199, "^0H>H5C1N1", 0, 0)]
        [TestCase("H(-1) N(-1) O(-1)", -31.014, -31.005814, "^0H>H1N1O1", 0, 0)]
        [TestCase("H(-2) C(-1) O(-1)", -30.026, -30.010565, "^0H>H2C1O1", 0, 0)]
        [TestCase("H(-2) C(-1) O S(-1)", -30.0922, -29.992806, "O>H2C1S1", 0, 0)]
        [TestCase("H(2) C(-5) N(2)", -30.0242, -29.978202, "H2N2>C5", 0, 0)]
        [TestCase("H(2) O(-2)", -29.9829, -29.974179, "H2>O2", 0, 0)]
        [TestCase("H(-3) C(-1) N(-1)", -29.0412, -29.026549, "^0H>H3C1N1", 0, 0)]
        [TestCase("H N(-1) O(-1)", -28.9982, -28.990164, "H>N1O1", 0, 0)]
        [TestCase("H(-4) C(-1) N(-2) O", -28.0565, -28.042534, "O>H4C1N2", 0, 0)]
        [TestCase("H(-4) C(-2)", -28.0532, -28.0313, "^0H>H4C2", 0, 0)]
        [TestCase("N(-2)", -28.0134, -28.006148, "^0H>N2", 0, 0)]
        [TestCase("C(-1) O(-1)", -28.0101, -27.994915, "^0H>C1O1", 0, 0)]
        [TestCase("H(-5) C(-1) N(-3) O(2)", -27.0717, -27.058518, "O2>H5C1N3", 0, 0)]
        [TestCase("H(-5) C(-2) N(-1) O", -27.0684, -27.047285, "O>H5C2N1", 0, 0)]
        [TestCase("H(-1) C(-1) N(-1)", -27.0253, -27.010899, "^0H>H1C1N1", 0, 0)]
        [TestCase("H(-6) C(-3) O", -26.0803, -26.052036, "O>H6C3", 0, 0)]
        [TestCase("H(-2) C(-2) O(-2) S", -25.9711, -26.033409, "S>H2C2O2", 0, 0)]
        [TestCase("H(-2) C(-2)", -26.0373, -26.01565, "^0H>H2C2", 0, 0)]
        [TestCase("H(-2) C(-3) N(2) O(-1)", -26.034, -26.004417, "N2>H2C3O1", 0, 0)]
        [TestCase("H(-7) C(-3) N(-1) S", -25.0294, -25.085779, "S>H7C3N1", 0, 0)]
        [TestCase("H(-3) C(-1) N(-3) S", -24.9896, -25.060626, "S>H3C1N3", 0, 0)]
        [TestCase("H(-3) C(-2) N(-1) O(-1) S", -24.9863, -25.049393, "S>H3C2N1O1", 0, 0)]
        [TestCase("H(4) N(-2)", -23.9816, -23.974848, "H4>N2", 0, 0)]
        [TestCase("H(-1) C(-2) N(-1) O", -23.0366, -23.015984, "O>H1C2N1", 0, 0)]
        [TestCase("H(-2) C(-2) N(-2) O(2)", -22.0519, -22.031969, "O2>H2C2N2", 0, 0)]
        [TestCase("H(-2) C S(-1)", -22.0702, -21.987721, "C>H2S1", 0, 0)]
        [TestCase("H(-4) O(-1)", -20.0312, -20.026215, "^0H>H4O1", 0, 0)]
        [TestCase("H(-5) N(-1)", -19.0464, -19.042199, "^0H>H5N1", 0, 0)]
        [TestCase("H(-1) C(-4) N O", -19.0446, -19.009836, "NO>H1C4", 0, 0)]
        [TestCase("H(3) C(-3) N", -19.0016, -18.973451, "H3N>C3", 0, 0)]
        [TestCase("H(-2) C(-4) O(2)", -18.0599, -18.025821, "O2>H2C4", 0, 0)]
        [TestCase("H(-3) 15N(-1)", -18.0239, -18.023584, " ", 0, 0)]
        [TestCase("H(-2) O(-1)", -18.0153, -18.010565, "^0H>H2O1", 0, 0)]
        [TestCase("H(-2) O S(-1)", -18.0815, -17.992806, "O>H2S1", 0, 0)]
        [TestCase("H(2) C O(-2)", -17.9722, -17.974179, "H2C>O2", 0, 0)]
        [TestCase("H(2) C S(-1)", -18.0384, -17.956421, "H2C>S1", 0, 0)]
        [TestCase("H(-3) N(-1)", -17.0305, -17.026549, "^0H>H3N1", 0, 0)]
        [TestCase("H(-3) C(-1) N O S(-1)", -17.0934, -16.997557, "NO>H3C1S1", 0, 0)]
        [TestCase("H C N(-1) O(-1)", -16.9875, -16.990164, "HC>N1O1", 0, 0)]
        [TestCase("H(-4) C(-1)", -16.0425, -16.0313, "^0H>H4C1", 0, 0)]
        [TestCase("C(-4) S", -15.9778, -16.027929, "S>C4", 0, 0)]
        [TestCase("H(-4) C(-1) O(2) S(-1)", -16.1087, -16.013542, "O2>H4C1S1", 0, 0)]
        [TestCase("O(-1)", -15.9994, -15.994915, "^0H>O1", 0, 0)]
        [TestCase("O S(-1)", -16.0656, -15.977156, "O>S1", 0, 0)]
        [TestCase("H(4) C O(-2)", -15.9563, -15.958529, "H4C>O2", 0, 0)]
        [TestCase("H(-1) N(-1)", -15.0146, -15.010899, "^0H>H1N1", 0, 0)]
        [TestCase("H(-1) C(-1) N O(-1)", -15.0113, -14.999666, "N>H1C1O1", 0, 0)]
        [TestCase("H(3) C N(-1) O(-1)", -14.9716, -14.974514, "H3C>N1O1", 0, 0)]
        [TestCase("H(-6) C(-2) O", -14.0696, -14.052036, "O>H6C2", 0, 0)]
        [TestCase("H(-2) C(-1)", -14.0266, -14.01565, "^0H>H2C1", 0, 0)]
        [TestCase("H(2) O(-1)", -13.9835, -13.979265, "H2>O1", 0, 0)]
        [TestCase("H(-7) C(-2) N(-1) O(2)", -13.0849, -13.06802, "O2>H7C2N1", 0, 0)]
        [TestCase("H(-3) C(-1) N(-1) O", -13.0418, -13.031634, "O>H3C1N1", 0, 0)]
        [TestCase("H N(-1)", -12.9988, -12.995249, "H>N1", 0, 0)]
        [TestCase("H(-4) C(-2) O", -12.0538, -12.036386, "O>H4C2", 0, 0)]
        [TestCase("C(-1) O(-2) S", -11.9445, -12.017759, "S>C1O2", 0, 0)]
        [TestCase("H(-1) C(-1) N(-1) O(-1) S", -10.9597, -11.033743, "S>H1C1N1O1", 0, 0)]
        [TestCase("H(-6) C(-3) S", -10.0147, -10.07488, "S>H6C3", 0, 0)]
        [TestCase("H(-2) C(-1) N(-2) O(2)", -10.0412, -10.031969, "O2>H2C1N2", 0, 0)]
        [TestCase("H(-2) C(-2) O", -10.0379, -10.020735, "O>H2C2", 0, 0)]
        [TestCase("H(-2) C(-3) N(2)", -10.0346, -10.009502, "N2>H2C3", 0, 0)]
        [TestCase("H(-3) C(3) N(-3)", -9.0118, -9.032697, "C3>H3N3", 0, 0)]
        [TestCase("H C(-1) N(-1) O", -9.0101, -9.000334, "HO>C1N1", 0, 0)]
        [TestCase("H(5) N(-1)", -8.967, -8.963949, "H5>N1", 0, 0)]
        [TestCase("C(-1) N(-2) O(2)", -8.0253, -8.016319, "O2>C1N2", 0, 0)]
        [TestCase("H(3) C(-3) N(3) O(-1)", -6.9876, -6.962218, "H3N3>C3O1", 0, 0)]
        [TestCase("H(2) C(-1) N(-2) S", -5.9432, -6.018427, "H2S>C1N2", 0, 0)]
        [TestCase("H(2) C(2) S(-1)", -6.0277, -5.956421, "H2C2>S1", 0, 0)]
        [TestCase("H(-3) C(-1) N(3) S(-1)", -5.0794, -4.986324, "N3>H3C1S1", 0, 0)]
        [TestCase("C O(-1)", -3.9887, -3.994915, "C>O1", 0, 0)]
        [TestCase("H(4) C(2) S(-1)", -4.0118, -3.940771, "H4C2>S1", 0, 0)]
        [TestCase("H(-1) N O S(-1)", -3.0668, -2.981907, "NO>H1S1", 0, 0)]
        [TestCase("H(3) C N S(-1)", -3.0238, -2.945522, "H3CN>S1", 0, 0)]
        [TestCase("H(-2)", -2.0159, -2.01565, "^0H>H2", 0, 0)]
        [TestCase("H(-2) O(2) S(-1)", -2.0821, -1.997892, "O2>H2S1", 0, 0)]
        [TestCase("H(2) C O(-1)", -1.9728, -1.979265, "H2C>O1", 0, 0)]
        [TestCase("H(2) C O S(-1)", -2.039, -1.961506, "H2CO>S1", 0, 0)]
        [TestCase("H(6) C(2) O(-2)", -1.9298, -1.942879, "H6C2>O2", 0, 0)]
        [TestCase("H(-3) N(-1) O", -1.0311, -1.031634, "O>H3N1", 0, 0)]
        [TestCase("H(-1)", -1.0079, -1.007825, "^0H>H1", 0, 0)]
        [TestCase("H N O(-1)", -0.9848, -0.984016, "HN>O1", 0, 0)]
        [TestCase("H(-2) 2H(2) 13C O(-1)", -0.9837, -0.979006, "D2^13.003355C>H2O1", 0, 0)]
        [TestCase("H(5) C(2) N(-1) O(-1)", -0.945, -0.958863, "H5C2>N1O1", 0, 0)]
        [TestCase("H(5) C N O(-2)", -0.9417, -0.94763, "H5CN>O2", 0, 0)]
        [TestCase("H(-4) C(-1) O", -0.0431, -0.036386, "O>H4C1", 0, 0)]
        [TestCase("H(4) C O(-1)", 0.0431, 0.036386, "H4C>O1", 0, 0)]
        [TestCase("H(-5) C(-1) N(-1) O(2)", 0.9417, 0.94763, "O2>H5C1N1", 0, 0)]
        [TestCase("H(-5) C(-2) N O", 0.945, 0.958863, "NO>H5C2", 0, 0)]
        [TestCase("H(-1) N(-1) O", 0.9848, 0.984016, "O>H1N1", 0, 0)]
        [TestCase("N(-1) 15N", 0.9934, 0.997035, "^15.000109N>N1", 0.993109, 0.997034995570001)]
        [TestCase("H(-6) C(-2) O(2)", 1.9298, 1.942879, "O2>H6C2", 0, 0)]
        [TestCase("H(-2) C(-1) O(-1) S", 2.039, 1.961506, "S>H2C1O1", 0, 0)]
        [TestCase("H(-2) C(-1) O", 1.9728, 1.979265, "O>H2C1", 0, 0)]
        [TestCase("N(-2) 15N(2)", 1.9868, 1.99407, "^15.000109N2>N2", 1.986218, 1.99406999114)]
        [TestCase("H(2) O(-2) S", 2.0821, 1.997892, "H2S>O2", 0, 0)]
        [TestCase("O(-1) 18O", 1.9998, 2.004246, "^17.999161O>O1", 2.000161, 2.00424638043)]
        [TestCase("H(2)", 2.0159, 2.01565, "H2", 2.016, 2.01565006446)]
        [TestCase("H(-3) C(-1) N(-1) S", 3.0238, 2.945522, "S>H3C1N1", 0, 0)]
        [TestCase("H N(-1) O(-1) S", 3.0668, 2.981907, "HS>N1O1", 0, 0)]
        [TestCase("H(-1) N(-1) 18O", 2.9845, 2.988261, "^17.999161O>H1N1", 0, 0)]
        [TestCase("N(-3) 15N(3)", 2.9802, 2.991105, "^15.000109N3>N3", 2.979327, 2.99110498671)]
        [TestCase("C(-3) 13C(3)", 2.978, 3.010064, "^13.003355C3>C3", 2.977065, 3.010065)]
        [TestCase("H(-3) 2H(3)", 3.0185, 3.01883, "D3>H3", 3.0183054, 3.01883030331)]
        [TestCase("H(-4) C(-2) S", 4.0118, 3.940771, "S>H4C2", 0, 0)]
        [TestCase("N(-4) 15N(4)", 3.9736, 3.98814, "^15.000109N4>N4", 3.972436, 3.98813998228)]
        [TestCase("C(-1) O", 3.9887, 3.994915, "O>C1", 0, 0)]
        [TestCase("C(-2) 13C(2) N(-2) 15N(2)", 3.9721, 4.00078, "^13.003355C2^15.000109N2>C2N2", 3.970928, 4.00077999114)]
        [TestCase("C(-3) 13C(3) N(-1) 15N", 3.9714, 4.007099, "^13.003355C3^15.000109N>C3N1", 3.970174, 4.00709999557001)]
        [TestCase("O(-2) 18O(2)", 3.9995, 4.008491, "^17.999161O2>O2", 4.000322, 4.00849276086)]
        [TestCase("C(-4) 13C(4)", 3.9706, 4.013419, "^13.003355C4>C4", 3.96942000000001, 4.01342)]
        [TestCase("H(-3) 2H(3) C(-1) 13C", 4.0111, 4.022185, "D3^13.003355C>H3C1", 4.0106604, 4.02218530331)]
        [TestCase("H(-4) 2H(4)", 4.0246, 4.025107, "D4>H4", 4.0244072, 4.02510707108)]
        [TestCase("H(-1) C(-1) N(-1) O(2)", 4.9735, 4.97893, "O2>H1C1N1", 0, 0)]
        [TestCase("C(-4) 13C(4) N(-1) 15N", 4.964, 5.010454, "^13.003355C4^15.000109N>C4N1", 4.96252900000001, 5.01045499557)]
        [TestCase("C(-5) 13C(5)", 4.9633, 5.016774, "^13.003355C5>C5", 4.96177500000002, 5.01677500000001)]
        [TestCase("H(-4) 2H(4) C(-1) 13C", 5.0173, 5.028462, "D4^13.003355C>H4C1", 5.0167622, 5.02846207108)]
        [TestCase("H(-2) C(-2) S", 6.0277, 5.956421, "S>H2C2", 0, 0)]
        [TestCase("H(-1) Li", 5.9331, 6.008178, "Li>H1", 0, 0)]
        [TestCase("C(-5) 13C(5) N(-1) 15N", 5.9567, 6.013809, "^13.003355C5^15.000109N>C5N1", 5.95488400000002, 6.01380999557001)]
        [TestCase("H(-2) C N(2) S(-1)", 5.9432, 6.018427, "CN2>H2S1", 0, 0)]
        [TestCase("C(-6) 13C(6)", 5.9559, 6.020129, "^13.003355C6>C6", 5.95413000000001, 6.02013000000001)]
        [TestCase("H(-3) C(3) N(-3) O", 6.9876, 6.962218, "C3O>H3N3", 0, 0)]
        [TestCase("C(-6) 13C(6) N(-1) 15N", 6.9493, 7.017164, "^13.003355C6^15.000109N>C6N1", 6.94723900000001, 7.01716499557001)]
        [TestCase("H(-6) 2H(6) N(-1) 15N", 7.0304, 7.034695, "D6^15.000109N>H6N1", 7.0297198, 7.03469560219)]
        [TestCase("C(-6) 13C(6) N(-2) 15N(2)", 7.9427, 8.014199, "^13.003355C6^15.000109N2>C6N2", 7.94034800000001, 8.01419999114001)]
        [TestCase("C N(2) O(-2)", 8.0253, 8.016319, "CN2>O2", 0, 0)]
        [TestCase("H(-5) N", 8.967, 8.963949, "N>H5", 0, 0)]
        [TestCase("H(-1) C N O(-1)", 9.0101, 9.000334, "CN>H1O1", 0, 0)]
        [TestCase("C(-9) 13C(9)", 8.9339, 9.030193, "^13.003355C9>C9", 8.93119500000002, 9.03019500000001)]
        [TestCase("H(3) C(-3) N(3)", 9.0118, 9.032697, "H3N3>C3", 0, 0)]
        [TestCase("C(-6) 13C(6) N(-4) 15N(4)", 9.9296, 10.008269, "^13.003355C6^15.000109N4>C6N4", 9.92656600000001, 10.00826998228)]
        [TestCase("H(2) C(3) N(-2)", 10.0346, 10.009502, "H2C3>N2", 0, 0)]
        [TestCase("H(2) C(2) O(-1)", 10.0379, 10.020735, "H2C2>O1", 0, 0)]
        [TestCase("C(-9) 13C(9) N(-1) 15N", 9.9273, 10.027228, "^13.003355C9^15.000109N>C9N1", 9.92430400000002, 10.02722999557)]
        [TestCase("H(-10) 2H(10)", 10.0616, 10.062767, "D10>H10", 10.061018, 10.0627676777)]
        [TestCase("H(6) C(3) S(-1)", 10.0147, 10.07488, "H6C3>S1", 0, 0)]
        [TestCase("H(-7) 2H(7) N(-4) 15N(4)", 11.0168, 11.032077, "D7^15.000109N4>H7N4", 11.0151486, 11.03207735667)]
        [TestCase("H C N O S(-1)", 10.9597, 11.033743, "HCNO>S1", 0, 0)]
        [TestCase("H(-9) 2H(9) N(-2) 15N(2)", 11.0423, 11.050561, "D9^15.000109N2>H9N2", 11.0411342, 11.05056090107)]
        [TestCase("H(5) C(2) N S(-1)", 11.0028, 11.070128, "H5C2N>S1", 0, 0)]
        [TestCase("C", 12.0107, 12, "C", 12.011, 12)]
        [TestCase("C O(2) S(-1)", 11.9445, 12.017759, "CO2>S1", 0, 0)]
        [TestCase("H(4) C(2) O(-1)", 12.0538, 12.036386, "H4C2>O1", 0, 0)]
        [TestCase("H(-1) C(-4) N O S", 13.0204, 12.962234, "NOS>H1C4", 0, 0)]
        [TestCase("H(-1) N", 12.9988, 12.995249, "N>H1", 0, 0)]
        [TestCase("H(3) C N O(-1)", 13.0418, 13.031634, "H3CN>O1", 0, 0)]
        [TestCase("H(7) C(2) N O(-2)", 13.0849, 13.06802, "H7C2N>O2", 0, 0)]
        [TestCase("H(-2) O", 13.9835, 13.979265, "O>H2", 0, 0)]
        [TestCase("H(2) C", 14.0266, 14.01565, "H2C", 14.027, 14.01565006446)]
        [TestCase("H(6) C(2) O(-1)", 14.0696, 14.052036, "H6C2>O1", 0, 0)]
        [TestCase("H(-3) N(-1) O(2)", 14.9683, 14.96328, "O2>H3N1", 0, 0)]
        [TestCase("H(-3) C(-1) N O", 14.9716, 14.974514, "NO>H3C1", 0, 0)]
        [TestCase("H C N(-1) O", 15.0113, 14.999666, "HCO>N1", 0, 0)]
        [TestCase("H N", 15.0146, 15.010899, "HN", 15.015, 15.01089903666)]
        [TestCase("H(-4) C(-1) O(2)", 15.9563, 15.958529, "O2>H4C1", 0, 0)]
        [TestCase("O(-1) S", 16.0656, 15.977156, "S>O1", 0, 0)]
        [TestCase("O", 15.9994, 15.994915, "O", 15.999, 15.99491461957)]
        [TestCase("H(4) C O(-2) S", 16.1087, 16.013542, "H4CS>O2", 0, 0)]
        [TestCase("C(4) S(-1)", 15.9778, 16.027929, "C4>S1", 0, 0)]
        [TestCase("2H(2) C", 16.0389, 16.028204, "D2C", 16.0392036, 16.0282036)]
        [TestCase("H(4) C", 16.0425, 16.0313, "H4C", 16.043, 16.03130012892)]
        [TestCase("H(-1) C(-1) N O", 16.9875, 16.990164, "NO>H1C1", 0, 0)]
        [TestCase("H(3) C N(-1) O(-1) S", 17.0934, 16.997557, "H3CS>N1O1", 0, 0)]
        [TestCase("H(3) N", 17.0305, 17.026549, "H3N", 17.031, 17.02654910112)]
        [TestCase("H(-1) 2H(3) C", 17.0451, 17.03448, "D3C>H1", 17.0453054, 17.03448036777)]
        [TestCase("H(-9) 2H(9) C(-6) 13C(6) N(-2) 15N(2)", 16.9982, 17.07069, "D9^13.003355C6^15.000109N2>H9C6N2", 16.9952642, 17.07069090107)]
        [TestCase("H(-2) C(-1) S", 18.0384, 17.956421, "S>H2C1", 0, 0)]
        [TestCase("H(-2) C(-1) O(2)", 17.9722, 17.974179, "O2>H2C1", 0, 0)]
        [TestCase("H(-1) F", 17.9905, 17.990578, "F>H1", 0, 0)]
        [TestCase("H(2) O", 18.0153, 18.010565, "H2O", 18.015, 18.01056468403)]
        [TestCase("H(2) C(4) O(-2)", 18.0599, 18.025821, "H2C4>O2", 0, 0)]
        [TestCase("H(-1) 2H(3) 13C", 18.0377, 18.037835, "D3^13.003355C>H1", 18.0376604, 18.03783536777)]
        [TestCase("H(-3) C(-1) N(-1) O S", 19.0232, 18.940436, "OS>H3C1N1", 0, 0)]
        [TestCase("H(-3) C(3) N(-1)", 19.0016, 18.973451, "C3>H3N1", 0, 0)]
        [TestCase("H C(4) N(-1) O(-1)", 19.0446, 19.009836, "HC4>N1O1", 0, 0)]
        [TestCase("H(-3) 2H(3) O", 19.0179, 19.013745, "D3O>H3", 19.0173054, 19.01374492288)]
        [TestCase("H(5) N", 19.0464, 19.042199, "H5N", 19.047, 19.04219916558)]
        [TestCase("C(-1) O(2)", 19.9881, 19.989829, "O2>C1", 0, 0)]
        [TestCase("C(-4) 13C(4) O", 19.97, 20.008334, "^13.003355C4O>C4", 19.96842, 20.00833461957)]
        [TestCase("H(-3) 2H(3) C(-1) 13C O", 20.0105, 20.0171, "D3^13.003355CO>H3C1", 20.0096604, 20.01709992288)]
        [TestCase("H(-2) Mg", 22.2891, 21.969392, "Mg>H2", 0, 0)]
        [TestCase("H(-1) Na", 21.9818, 21.981943, "Na>H1", 0, 0)]
        [TestCase("H(-2) C(2)", 22.0055, 21.98435, "C2>H2", 0, 0)]
        [TestCase("H(2) C(2) N(2) O(-2)", 22.0519, 22.031969, "H2C2N2>O2", 0, 0)]
        [TestCase("H C(2) N O(-1)", 23.0366, 23.015984, "HC2N>O1", 0, 0)]
        [TestCase("H(-3) Al", 23.9577, 23.958063, "Al>H3", 0, 0)]
        [TestCase("H(-4) N(2)", 23.9816, 23.974848, "N2>H4", 0, 0)]
        [TestCase("H(2) C(-5) 13C(6) N(-4) 15N(4)", 23.9561, 24.023919, "H2^13.003355C6^15.000109N4>C5N4", 23.953566, 24.02392004674)]
        [TestCase("H(-1) C N", 25.0095, 24.995249, "CN>H1", 0, 0)]
        [TestCase("H(3) C(2) N O S(-1)", 24.9863, 25.049393, "H3C2NO>S1", 0, 0)]
        [TestCase("H(3) C N(3) S(-1)", 24.9896, 25.060626, "H3CN3>S1", 0, 0)]
        [TestCase("H(7) C(3) N S(-1)", 25.0294, 25.085779, "H7C3N>S1", 0, 0)]
        [TestCase("H(2) C(3) N(-2) O", 26.034, 26.004417, "H2C3O>N2", 0, 0)]
        [TestCase("H(2) C(2)", 26.0373, 26.01565, "H2C2", 26.038, 26.01565006446)]
        [TestCase("H(2) C(2) O(2) S(-1)", 25.9711, 26.033409, "H2C2O2>S1", 0, 0)]
        [TestCase("H(6) C(3) O(-1)", 26.0803, 26.052036, "H6C3>O1", 0, 0)]
        [TestCase("H C N", 27.0253, 27.010899, "HCN", 27.026, 27.01089903666)]
        [TestCase("H(5) C(2) N O(-1)", 27.0684, 27.047285, "H5C2N>O1", 0, 0)]
        [TestCase("H(5) C N(3) O(-2)", 27.0717, 27.058518, "H5CN3>O2", 0, 0)]
        [TestCase("H(-4) O(2)", 27.967, 27.958529, "O2>H4", 0, 0)]
        [TestCase("C O", 28.0101, 27.994915, "CO", 28.01, 27.99491461957)]
        [TestCase("N(2)", 28.0134, 28.006148, "N2", 28.014, 28.00614800886)]
        [TestCase("H(4) C(2)", 28.0532, 28.0313, "H4C2", 28.054, 28.03130012892)]
        [TestCase("H(4) C N(2) O(-1)", 28.0565, 28.042534, "H4CN2>O1", 0, 0)]
        [TestCase("H(-1) 2H(3) C(-6) 13C(7) N(-4) 15N(4)", 27.9673, 28.046104, "D3^13.003355C7^15.000109N4>H1C6N4", 27.9642264, 28.04610535005)]
        [TestCase("H(-1) N O", 28.9982, 28.990164, "NO>H1", 0, 0)]
        [TestCase("H(3) C(2) N(-1) O", 29.0379, 29.015316, "H3C2O>N1", 0, 0)]
        [TestCase("H(3) C N", 29.0412, 29.026549, "H3CN", 29.042, 29.02654910112)]
        [TestCase("H(5) C(2)", 29.0611, 29.039125, "H5C2", 29.062, 29.03912516115)]
        [TestCase("H(-2) O(2)", 29.9829, 29.974179, "O2>H2", 0, 0)]
        [TestCase("H(-2) C(5) N(-2)", 30.0242, 29.978202, "C5>H2N2", 0, 0)]
        [TestCase("H(2) C O(-1) S", 30.0922, 29.992806, "H2CS>O1", 0, 0)]
        [TestCase("H(2) C O", 30.026, 30.010565, "H2CO", 30.026, 30.01056468403)]
        [TestCase("H N O", 31.014, 31.005814, "HNO", 31.014, 31.00581365623)]
        [TestCase("H(5) C N", 31.0571, 31.042199, "H5CN", 31.058, 31.04219916558)]
        [TestCase("H(-4) C(-1) O S", 32.0219, 31.935685, "OS>H4C1", 0, 0)]
        [TestCase("S", 32.065, 31.972071, "S", 32.06, 31.9720711744)]
        [TestCase("O(2)", 31.9988, 31.989829, "O2", 31.998, 31.98982923914)]
        [TestCase("C(4) O S(-1)", 31.9772, 32.022844, "C4O>S1", 0, 0)]
        [TestCase("H(4) C(5) O(-2)", 32.0865, 32.041471, "H4C5>O2", 0, 0)]
        [TestCase("2H(4) C(2)", 32.0778, 32.056407, "D4C2", 32.0784072, 32.0564072)]
        [TestCase("H(3) C(5) N(-1) O(-1)", 33.0712, 33.025486, "H3C5>N1O1", 0, 0)]
        [TestCase("H(-2) C(-1) O S", 34.0378, 33.951335, "OS>H2C1", 0, 0)]
        [TestCase("H(-1) Cl", 34.4451, 33.961028, "Cl>H1", 0, 0)]
        [TestCase("H(-2) C(-1) O(3)", 33.9716, 33.969094, "O3>H2C1", 0, 0)]
        [TestCase("H(-2) C(3)", 34.0162, 33.98435, "C3>H2", 0, 0)]
        [TestCase("H(2) S", 34.0809, 33.987721, "H2S", 34.076, 33.98772123886)]
        [TestCase("H(2) C(4) O(-1)", 34.0593, 34.020735, "H2C4>O1", 0, 0)]
        [TestCase("H(2) C(3) N(2) S(-1)", 33.9964, 34.049727, "H2C3N2>S1", 0, 0)]
        [TestCase("H(4) C(-4) 13C(6)", 34.0091, 34.051429, "H4^13.003355C6>C4", 34.00813, 34.05143012892)]
        [TestCase("2H(4) 13C(2)", 34.0631, 34.063117, "D4^13.003355C2", 34.0631172, 34.0631172)]
        [TestCase("H(-2) 2H(6) C(2)", 34.0901, 34.068961, "D6C2>H2", 34.0906108, 34.06896073554)]
        [TestCase("H(-3) C(3) N(-1) O", 35.001, 34.968366, "C3O>H3N1", 0, 0)]
        [TestCase("H C(4) N(-1)", 35.044, 35.004751, "HC4>N1", 0, 0)]
        [TestCase("C(2) N(2) O(-1)", 36.0354, 36.011233, "C2N2>O1", 0, 0)]
        [TestCase("H(4) C(-4) 13C(6) N(-2) 15N(2)", 35.9959, 36.045499, "H4^13.003355C6^15.000109N2>C4N2", 35.994348, 36.04550012006)]
        [TestCase("H(-2) 2H(6) 13C(2)", 36.0754, 36.07567, "D6^13.003355C2>H2", 36.0753208, 36.07567073554)]
        [TestCase("H(3) C(3) N O(-1)", 37.0632, 37.031634, "H3C3N>O1", 0, 0)]
        [TestCase("H(-2) Ca", 38.0621, 37.946941, "Ca>H2", 0, 0)]
        [TestCase("H(-1) K", 38.0904, 37.955882, "K>H1", 0, 0)]
        [TestCase("H(-2) C N(2)", 38.0082, 37.990498, "CN2>H2", 0, 0)]
        [TestCase("H(2) C(3)", 38.048, 38.01565, "H2C3", 38.049, 38.01565006446)]
        [TestCase("H(4) C(-4) 13C(6) N(-4) 15N(4)", 37.9827, 38.039569, "H4^13.003355C6^15.000109N4>C4N4", 37.980566, 38.0395701112)]
        [TestCase("H C(2) N", 39.036, 39.010899, "HC2N", 39.037, 39.01089903666)]
        [TestCase("C(2) O", 40.0208, 39.994915, "C2O", 40.021, 39.99491461957)]
        [TestCase("C N(2)", 40.0241, 40.006148, "CN2", 40.025, 40.00614800886)]
        [TestCase("H(4) C(3)", 40.0639, 40.0313, "H4C3", 40.065, 40.03130012892)]
        [TestCase("H(-1) N(3)", 41.0122, 41.001397, "N3>H1", 0, 0)]
        [TestCase("H(3) C(2) N", 41.0519, 41.026549, "H3C2N", 41.053, 41.02654910112)]
        [TestCase("H(7) C(3) N O(-1)", 41.095, 41.062935, "H7C3N>O1", 0, 0)]
        [TestCase("H(7) C(2) N(3) O(-2)", 41.0983, 41.074168, "H7C2N3>O2", 0, 0)]
        [TestCase("H(2) C(2) O", 42.0367, 42.010565, "H2C2O", 42.037, 42.01056468403)]
        [TestCase("H(2) C N(2)", 42.04, 42.021798, "H2CN2", 42.041, 42.02179807332)]
        [TestCase("H(6) C(3)", 42.0797, 42.04695, "H6C3", 42.081, 42.04695019338)]
        [TestCase("H(6) C(2) N(2) O(-1)", 42.083, 42.058184, "H6C2N2>O1", 0, 0)]
        [TestCase("H C N O", 43.0247, 43.005814, "HCNO", 43.025, 43.00581365623)]
        [TestCase("H N(3)", 43.028, 43.017047, "HN3", 43.029, 43.01704704552)]
        [TestCase("H(5) C(2) N", 43.0678, 43.042199, "H5C2N", 43.069, 43.04219916558)]
        [TestCase("H(-4) O(3)", 43.9664, 43.953444, "O3>H4", 0, 0)]
        [TestCase("C O(2)", 44.0095, 43.989829, "CO2", 44.009, 43.98982923914)]
        [TestCase("H(4) C(2) O(-1) S", 44.1188, 44.008456, "H4C2S>O1", 0, 0)]
        [TestCase("H(2) 13C(2) O", 44.022, 44.017274, "H2^13.003355C2O", 44.02171, 44.01727468403)]
        [TestCase("H(4) C(2) O", 44.0526, 44.026215, "H4C2O", 44.053, 44.02621474849)]
        [TestCase("H(4) C(6) S(-1)", 44.031, 44.059229, "H4C6>S1", 0, 0)]
        [TestCase("H(-1) N O(2)", 44.9976, 44.985078, "NO2>H1", 0, 0)]
        [TestCase("H(-1) 2H(3) C(2) O", 45.0552, 45.029395, "D3C2O>H1", 45.0553054, 45.02939498734)]
        [TestCase("H(2) C S", 46.0916, 45.987721, "H2CS", 46.087, 45.98772123886)]
        [TestCase("H(2) C(5) O(-1)", 46.07, 46.020735, "H2C5>O1", 0, 0)]
        [TestCase("H(-2) 2H(4) C(2) O", 46.0613, 46.035672, "D4C2O>H2", 46.0614072, 46.03567175511)]
        [TestCase("H(-2) 2H(6) C(-6) 13C(8) N(-4) 15N(4)", 46.005, 46.083939, "D6^13.003355C8^15.000109N4>H2C6N4", 46.0018868, 46.08394071782)]
        [TestCase("S(-1) Se", 46.895, 47.944449, "Se>S1", 0, 0)]
        [TestCase("O(3)", 47.9982, 47.984744, "O3", 47.997, 47.98474385871)]
        [TestCase("C(4)", 48.0428, 48, "C4", 48.044, 48)]
        [TestCase("H(2) C(-4) 13C(6) O", 47.9926, 48.030694, "H2^13.003355C6O>C4", 47.99113, 48.03069468403)]
        [TestCase("H(4) C(5) O(-1)", 48.0859, 48.036386, "H4C5>O1", 0, 0)]
        [TestCase("2H(6) C(3)", 48.1167, 48.084611, "D6C3", 48.1176108, 48.0846108)]
        [TestCase("H(3) C(5) N(-1)", 49.0706, 49.020401, "H3C5>N1", 0, 0)]
        [TestCase("H(-2) C(3) O", 50.0156, 49.979265, "C3O>H2", 0, 0)]
        [TestCase("H(2) C(4)", 50.0587, 50.01565, "H2C4", 50.06, 50.01565006446)]
        [TestCase("H(2) C(-4) 13C(6) N(-2) 15N(2) O", 49.9794, 50.024764, "H2^13.003355C6^15.000109N2O>C4N2", 49.977348, 50.02476467517)]
        [TestCase("H(2) C(3) N(2) O(-1)", 50.062, 50.026883, "H2C3N2>O1", 0, 0)]
        [TestCase("H(-3) 2H(9) C(3)", 51.1352, 51.103441, "D9C3>H3", 51.1359162, 51.10344110331)]
        [TestCase("H(-3) Fe", 52.8212, 52.911464, "Fe>H3", 0, 0)]
        [TestCase("H(7) C(3) N(3) S(-1)", 53.0428, 53.091927, "H7C3N3>S1", 0, 0)]
        [TestCase("H(-2) Fe", 53.8291, 53.919289, "Fe>H2", 0, 0)]
        [TestCase("H(-3) F(3)", 53.9714, 53.971735, "F3>H3", 0, 0)]
        [TestCase("H(2) C(3) O", 54.0474, 54.010565, "H2C3O", 54.048, 54.01056468403)]
        [TestCase("H(-3) 2H(9) 13C(3)", 54.1132, 54.113505, "D9^13.003355C3>H3", 54.1129812, 54.11350610331)]
        [TestCase("H C(6) N S(-1)", 55.0138, 55.038828, "HC6N>S1", 0, 0)]
        [TestCase("H(5) C(2) N(3) O(-1)", 55.0818, 55.053433, "H5C2N3>O1", 0, 0)]
        [TestCase("H(-2) Ni", 56.6775, 55.919696, "Ni>H2", 0, 0)]
        [TestCase("C(2) O(2)", 56.0202, 55.989829, "C2O2", 56.02, 55.98982923914)]
        [TestCase("H(4) C(3) O", 56.0633, 56.026215, "H4C3O", 56.064, 56.02621474849)]
        [TestCase("H(8) C(4)", 56.1063, 56.0626, "H8C4", 56.108, 56.06260025784)]
        [TestCase("H(3) C(2) N O", 57.0513, 57.021464, "H3C2NO", 57.052, 57.02146372069)]
        [TestCase("H(3) C N(3)", 57.0546, 57.032697, "H3CN3", 57.056, 57.03269710998)]
        [TestCase("H(3) C(6) N O(-2)", 57.0959, 57.03672, "H3C6N>O2", 0, 0)]
        [TestCase("H(7) C(3) N", 57.0944, 57.057849, "H7C3N", 57.096, 57.05784923004)]
        [TestCase("H(-2) C(5)", 58.0376, 57.98435, "C5>H2", 0, 0)]
        [TestCase("H(2) C(2) O(2)", 58.0361, 58.005479, "H2C2O2", 58.036, 58.0054793036)]
        [TestCase("H(2) C(6) O(-1)", 58.0807, 58.020735, "H2C6>O1", 0, 0)]
        [TestCase("H(4) C(2) N O", 58.0593, 58.029289, "H4C2NO", 58.06, 58.02928875292)]
        [TestCase("H(6) C(3) O", 58.0791, 58.041865, "H6C3O", 58.08, 58.04186481295)]
        [TestCase("H(5) C(2) N O(-1) S", 59.1334, 59.019355, "H5C2NS>O1", 0, 0)]
        [TestCase("H(4) 13C(3) O", 59.0412, 59.036279, "H4^13.003355C3O", 59.041065, 59.03627974849)]
        [TestCase("H 2H(3) C(3) O", 59.0817, 59.045045, "HD3C3O", 59.0823054, 59.0450450518)]
        [TestCase("H(5) C N(3)", 59.0705, 59.048347, "H5CN3", 59.072, 59.04834717444)]
        [TestCase("H(7) C(3) O", 59.0871, 59.04969, "H7C3O", 59.088, 59.04968984518)]
        [TestCase("H(4) C(2) S", 60.1182, 60.003371, "H4C2S", 60.114, 60.00337130332)]
        [TestCase("H(2) 13C(2) O(2)", 60.0214, 60.012189, "H2^13.003355C2O2", 60.02071, 60.0121893036)]
        [TestCase("H(4) C(6) O(-1)", 60.0966, 60.036386, "H4C6>O1", 0, 0)]
        [TestCase("H(4) C(6) O S(-1)", 60.0304, 60.054144, "H4C6O>S1", 0, 0)]
        [TestCase("H(-2) Zn", 63.3931, 61.913495, "Zn>H2", 0, 0)]
        [TestCase("H(-1) Cu", 62.5381, 61.921774, "Cu>H1", 0, 0)]
        [TestCase("H(2) C(5)", 62.0694, 62.01565, "H2C5", 62.071, 62.01565006446)]
        [TestCase("H(-2) 2H(6) C(3) O", 62.1002, 62.063875, "D6C3O>H2", 62.1006108, 62.06387535511)]
        [TestCase("H 2H(4) C(2) N O(-1) S", 63.158, 63.044462, "HD4C2NS>O1", 0, 0)]
        [TestCase("O(2) S", 64.0638, 63.9619, "O2S", 64.058, 63.96190041354)]
        [TestCase("O(4)", 63.9976, 63.979659, "O4", 63.996, 63.97965847828)]
        [TestCase("C(4) O", 64.0422, 63.994915, "C4O", 64.043, 63.99491461957)]
        [TestCase("H(2) C(4) O", 66.0581, 66.010565, "H2C4O", 66.059, 66.01056468403)]
        [TestCase("H(2) C(3) N(2)", 66.0614, 66.021798, "H2C3N2", 66.063, 66.02179807332)]
        [TestCase("H(-2) Cl(2)", 68.8901, 67.922055, "Cl2>H2", 0, 0)]
        [TestCase("H(4) C(4) O", 68.074, 68.026215, "H4C4O", 68.075, 68.02621474849)]
        [TestCase("H(4) C(3) N(2)", 68.0773, 68.037448, "H4C3N2", 68.079, 68.03744813778)]
        [TestCase("H(8) C(5)", 68.117, 68.0626, "H8C5", 68.119, 68.06260025784)]
        [TestCase("H(7) C(3) N(3) O(-1)", 69.1084, 69.069083, "H7C3N3>O1", 0, 0)]
        [TestCase("H(2) C(3) O(2)", 70.0468, 70.005479, "H2C3O2", 70.047, 70.0054793036)]
        [TestCase("H(6) C(4) O", 70.0898, 70.041865, "H6C4O", 70.091, 70.04186481295)]
        [TestCase("H(3) C(3) O(2)", 71.0547, 71.013304, "H3C3O2", 71.055, 71.01330433583)]
        [TestCase("H(5) C(3) N O", 71.0779, 71.037114, "H5C3NO", 71.079, 71.03711378515)]
        [TestCase("H(5) C(7) N O(-2)", 71.1225, 71.05237, "H5C7N>O2", 0, 0)]
        [TestCase("H(9) C(4) N", 71.121, 71.073499, "H9C4N", 71.123, 71.0734992945)]
        [TestCase("H(4) C(3) O(2)", 72.0627, 72.021129, "H4C3O2", 72.063, 72.02112936806)]
        [TestCase("H(4) C(7) O(-1)", 72.1073, 72.036386, "H4C7>O1", 0, 0)]
        [TestCase("2H(4) C(3) N(2)", 72.1019, 72.062555, "D4C3N2", 72.1034072, 72.06255520886)]
        [TestCase("H(-1) C(5) N", 73.0523, 72.995249, "C5N>H1", 0, 0)]
        [TestCase("H(6) C(3) S", 74.1447, 74.019021, "H6C3S", 74.141, 74.01902136778)]
        [TestCase("H(2) 2H(3) C(3) N O", 74.0964, 74.055944, "H2D3C3NO", 74.0973054, 74.05594408846)]
        [TestCase("H(4) C(2) O(-1) S(2)", 76.1838, 75.980527, "H4C2S2>O1", 0, 0)]
        [TestCase("H(4) C(2) O S", 76.1176, 75.998285, "H4C2OS", 76.113, 75.99828592289)]
        [TestCase("H(4) C(6)", 76.096, 76.0313, "H4C6", 76.098, 76.03130012892)]
        [TestCase("H(-1) Br", 78.8961, 77.910511, "Br>H1", 0, 0)]
        [TestCase("H(3) C O(2) P", 78.0071, 77.987066, "H3CO2P", 78.006761998, 77.98706633425)]
        [TestCase("H(6) C(6)", 78.1118, 78.04695, "H6C6", 78.114, 78.04695019338)]
        [TestCase("Se", 78.96, 79.91652, "Se", 78.971, 79.9165218, 0.02)]
        [TestCase("O(3) S", 80.0632, 79.956815, "O3S", 80.057, 79.95681503311)]
        [TestCase("H O(3) P", 79.9799, 79.966331, "HO3P", 79.978761998, 79.96633088936)]
        [TestCase("H(4) C(5) O", 80.0847, 80.026215, "H4C5O", 80.086, 80.02621474849)]
        [TestCase("H(4) C(4) N(2)", 80.088, 80.037448, "H4C4N2", 80.09, 80.03744813778)]
        [TestCase("H(-1) C(3) N O(2)", 81.0297, 80.985078, "C3NO2>H1", 0, 0)]
        [TestCase("H(6) C(5) O", 82.1005, 82.041865, "H6C5O", 82.102, 82.04186481295)]
        [TestCase("H(5) C(8) N S(-1)", 83.067, 83.070128, "H5C8N>S1", 0, 0)]
        [TestCase("H(3) C(7) N O(-1)", 85.106, 85.031634, "H3C7N>O1", 0, 0)]
        [TestCase("H(7) C(4) N O", 85.1045, 85.052764, "H7C4NO", 85.106, 85.05276384961)]
        [TestCase("H(7) C(3) N(3)", 85.1078, 85.063997, "H7C3N3", 85.11, 85.0639972389)]
        [TestCase("H(11) C(5) N", 85.1475, 85.089149, "H11C5N", 85.15, 85.08914935896)]
        [TestCase("H(2) C(3) O S", 86.1124, 85.982635, "H2C3OS", 86.108, 85.98263585843)]
        [TestCase("H(2) C(3) O(3)", 86.0462, 86.000394, "H2C3O3", 86.046, 86.00039392317)]
        [TestCase("H(6) C(4) O(2)", 86.0892, 86.036779, "H6C4O2", 86.09, 86.03677943252)]
        [TestCase("H C(6) N", 87.0788, 87.010899, "HC6N", 87.081, 87.01089903666)]
        [TestCase("H(5) C(3) N S", 87.1435, 87.01427, "H5C3NS", 87.14, 87.01427033998)]
        [TestCase("H(5) C(3) N O(2)", 87.0773, 87.032028, "H5C3NO2", 87.078, 87.03202840472)]
        [TestCase("H(9) C(4) N O(-1) S", 87.1866, 87.050655, "H9C4NS>O1", 0, 0)]
        [TestCase("H(9) C(4) N O", 87.1204, 87.068414, "H9C4NO", 87.122, 87.06841391407)]
        [TestCase("H(4) C(3) O S", 88.1283, 87.998285, "H4C3OS", 88.124, 87.99828592289)]
        [TestCase("H C(-9) 13C(9) O(3) P", 88.9138, 88.996524, "H^13.003355C9O3P>C9", 88.909956998, 88.99652588936)]
        [TestCase("H(3) C(2) N O(3)", 89.0501, 89.011293, "H3C2NO3", 89.05, 89.01129295983)]
        [TestCase("H(3) C(6) N", 89.0947, 89.026549, "H3C6N", 89.097, 89.02654910112)]
        [TestCase("H(6) C(7)", 90.1225, 90.04695, "H6C7", 90.125, 90.04695019338)]
        [TestCase("H(2) 2H(5) C(4) N O", 90.1353, 90.084148, "H2D5C4NO", 90.136509, 90.08414768846)]
        [TestCase("H(4) C(6) O", 92.0954, 92.026215, "H4C6O", 92.097, 92.02621474849)]
        [TestCase("H(4) C N O(2) P", 93.0217, 92.997965, "H4CNO2P", 93.021761998, 92.99796537091)]
        [TestCase("H(3) C O(3) P", 94.0065, 93.981981, "H3CO3P", 94.005761998, 93.98198095382)]
        [TestCase("H(6) C(6) O", 94.1112, 94.041865, "H6C6O", 94.113, 94.04186481295)]
        [TestCase("H N O(3) S", 95.0778, 94.967714, "HNO3S", 95.072, 94.96771406977)]
        [TestCase("H O(2) P S", 96.0455, 95.943487, "HO2PS", 96.039761998, 95.94348744419)]
        [TestCase("H(4) C(5) O(2)", 96.0841, 96.021129, "H4C5O2", 96.085, 96.02112936806)]
        [TestCase("H(8) C(6) O", 96.1271, 96.057515, "H8C6O", 96.129, 96.05751487741)]
        [TestCase("H(3) C(4) N O(2)", 97.0721, 97.016378, "H3C4NO2", 97.073, 97.01637834026)]
        [TestCase("H(11) C(6) N", 97.1582, 97.089149, "H11C6N", 97.161, 97.08914935896)]
        [TestCase("H(10) C(6) O", 98.143, 98.073165, "H10C6O", 98.145, 98.07316494187)]
        [TestCase("H(5) C(8) N O(-1)", 99.1326, 99.047285, "H5C8N>O1", 0, 0)]
        [TestCase("H(9) C(5) N O", 99.1311, 99.068414, "H9C5NO", 99.133, 99.06841391407)]
        [TestCase("H(9) C(4) N(3)", 99.1344, 99.079647, "H9C4N3", 99.137, 99.07964730336)]
        [TestCase("H(4) C(4) O(3)", 100.0728, 100.016044, "H4C4O3", 100.073, 100.01604398763)]
        [TestCase("H(8) C(4) N(2) O", 100.1191, 100.063663, "H8C4N2O", 100.121, 100.06366288627)]
        [TestCase("H(11) C(5) N O", 101.1469, 101.084064, "H11C5NO", 101.149, 101.08406397853)]
        [TestCase("H(5) C(2) As", 103.9827, 103.960719, "H5C2As", 103.983595, 103.96071973115)]
        [TestCase("H(4) C(3) O(2) S", 104.1277, 103.9932, "H4C3O2S", 104.123, 103.99320054246)]
        [TestCase("H(4) C(7) O", 104.1061, 104.026215, "H4C7O", 104.108, 104.02621474849)]
        [TestCase("H(4) 13C(4) O(3)", 104.0434, 104.029463, "H4^13.003355C4O3", 104.04242, 104.02946398763)]
        [TestCase("2H(4) C(4) O(3)", 104.0974, 104.041151, "D4C4O3", 104.0974072, 104.04115105871)]
        [TestCase("H(10) C(4) N O(2)", 104.1277, 104.071154, "H10C4NO2", 104.129, 104.07115356587)]
        [TestCase("H(3) C(2) N O S(-1) Se", 103.9463, 104.965913, "H3C2NOSe>S1", 0, 0)]
        [TestCase("H(3) C(6) N O", 105.0941, 105.021464, "H3C6NO", 105.096, 105.02146372069)]
        [TestCase("H(7) C(7) N", 105.1372, 105.057849, "H7C7N", 105.14, 105.05784923004)]
        [TestCase("H(-1) Ag", 106.8603, 105.897267, "Ag>H1", 0, 0)]
        [TestCase("H(6) C(3) O(2) S", 106.1435, 106.00885, "H6C3O2S", 106.139, 106.00885060692)]
        [TestCase("H(6) C(7) O", 106.1219, 106.041865, "H6C7O", 106.124, 106.04186481295)]
        [TestCase("H(6) C(2) N O(2) P", 107.0483, 107.013615, "H6C2NO2P", 107.048761998, 107.01361543537)]
        [TestCase("H(5) C(4) N(5) O S(-1)", 107.0504, 107.077339, "H5C4N5O>S1", 0, 0)]
        [TestCase("H(5) C(2) O P S", 108.0993, 107.979873, "H5C2OPS", 108.094761998, 107.97987295354)]
        [TestCase("H(5) C(2) O(3) P", 108.0331, 107.997631, "H5C2O3P", 108.032761998, 107.99763101828)]
        [TestCase("H(4) C(6) O(2)", 108.0948, 108.021129, "H4C6O2", 108.096, 108.02112936806)]
        [TestCase("H(4) C N O P S", 109.0873, 108.975121, "H4CNOPS", 109.082761998, 108.97512192574)]
        [TestCase("H(-1) 2H(4) C(6) N O", 109.1188, 109.046571, "D4C6NO>H1", 109.1204072, 109.04657079177)]
        [TestCase("H 2H(3) C(6) N O", 109.1205, 109.048119, "HD3C6NO", 109.1223054, 109.04811905623)]
        [TestCase("H(7) C(6) N O", 109.1259, 109.052764, "H7C6NO", 109.128, 109.05276384961)]
        [TestCase("H(3) C O(2) P S", 110.0721, 109.959137, "H3CO2PS", 110.066761998, 109.95913750865)]
        [TestCase("H(5) C(5) N O(2)", 111.0987, 111.032028, "H5C5NO2", 111.1, 111.03202840472)]
        [TestCase("H(3) 13C(6) N O", 111.05, 111.041593, "H3^13.003355C6NO", 111.05013, 111.04159372069)]
        [TestCase("H(9) C(6) N O", 111.1418, 111.068414, "H9C6NO", 111.144, 111.06841391407)]
        [TestCase("H(8) C(6) O(2)", 112.1265, 112.05243, "H8C6O2", 112.128, 112.05242949698)]
        [TestCase("H(7) C(5) N O(2)", 113.1146, 113.047679, "H7C5NO2", 113.116, 113.04767846918)]
        [TestCase("H(11) C(6) N O", 113.1576, 113.084064, "H11C6NO", 113.16, 113.08406397853)]
        [TestCase("H(2) C(4) O(4)", 114.0563, 113.995309, "H2C4O4", 114.056, 113.99530854274)]
        [TestCase("H(6) C(5) O(3)", 114.0993, 114.031694, "H6C5O3", 114.1, 114.03169405209)]
        [TestCase("H(6) C(4) N(2) O(2)", 114.1026, 114.042927, "H6C4N2O2", 114.104, 114.04292744138)]
        [TestCase("H(5) C(4) N O(3)", 115.0874, 115.026943, "H5C4NO3", 115.088, 115.02694302429)]
        [TestCase("H(5) C(8) N", 115.132, 115.042199, "H5C8N", 115.135, 115.04219916558)]
        [TestCase("H(-1) 2H(4) 13C(6) N O", 115.0747, 115.0667, "D4^13.003355C6NO>H1", 115.0745372, 115.06670079177)]
        [TestCase("H(4) C(4) O(4)", 116.0722, 116.010959, "H4C4O4", 116.072, 116.0109586072)]
        [TestCase("H(4) C(3) N O(2) P", 117.0431, 116.997965, "H4C3NO2P", 117.043761998, 116.99796537091)]
        [TestCase("H(7) C(4) N O S", 117.1695, 117.024835, "H7C4NOS", 117.166, 117.02483502401)]
        [TestCase("H(8) C(8) N", 118.1558, 118.065674, "H8C8N", 118.159, 118.06567426227)]
        [TestCase("H(2) 2H(4) C(4) N(2) O(2)", 118.1273, 118.068034, "H2D4C4N2O2", 118.1284072, 118.06803451246)]
        [TestCase("H(5) C(3) N O(2) S", 119.1423, 119.004099, "H5C3NO2S", 119.138, 119.00409957912)]
        [TestCase("H(5) C(7) N O", 119.1207, 119.037114, "H5C7NO", 119.123, 119.03711378515)]
        [TestCase("H(4) C(7) O(2)", 120.1055, 120.021129, "H4C7O2", 120.107, 120.02112936806)]
        [TestCase("H(8) C(4) O(2) S", 120.1701, 120.0245, "H8C4O2S", 120.166, 120.02450067138)]
        [TestCase("H(9) C(4) O(2) P", 120.0868, 120.034017, "H9C4O2P", 120.087761998, 120.03401652763)]
        [TestCase("H(6) 13C(4) 15N(2) O(2)", 120.0601, 120.050417, "H6^13.003355C4^15.000109N2O2", 120.059638, 120.05041743252)]
        [TestCase("H(6) C(-2) 13C(6) N(2) O(2)", 120.0586, 120.063056, "H6^13.003355C6N2O2>C2", 120.05813, 120.06305744138)]
        [TestCase("H(7) C(7) N O(-1) S", 121.2028, 121.035005, "H7C7NS>O1", 0, 0)]
        [TestCase("H(7) C(3) O(3) P", 122.0596, 122.013281, "H7C3O3P", 122.059761998, 122.01328108274)]
        [TestCase("H(6) C(7) O(2)", 122.1213, 122.036779, "H6C7O2", 122.123, 122.03677943252)]
        [TestCase("H(6) C(-2) 13C(6) 15N(2) O(2)", 122.0454, 122.057126, "H6^13.003355C6^15.000109N2O2>C2", 122.044348, 122.05712743252)]
        [TestCase("H(10) C(8) O", 122.1644, 122.073165, "H10C8O", 122.167, 122.07316494187)]
        [TestCase("H(10) C(7) N(2)", 122.1677, 122.084398, "H10C7N2", 122.171, 122.08439833116)]
        [TestCase("H(6) C(2) N O(3) P", 123.0477, 123.00853, "H6C2NO3P", 123.047761998, 123.00853005494)]
        [TestCase("H(5) C(2) O(2) P S", 124.0987, 123.974787, "H5C2O2PS", 124.093761998, 123.97478757311)]
        [TestCase("2H(5) C(7) N O", 124.1515, 124.068498, "D5C7NO", 124.153509, 124.068497624)]
        [TestCase("H(7) C(6) N O(2)", 125.1253, 125.047679, "H7C6NO2", 125.127, 125.04767846918)]
        [TestCase("H(-1) I", 125.8965, 125.896648, "I>H1", 0, 0)]
        [TestCase("H(2) 2H(6) C(4) O(2) S", 126.2071, 126.062161, "H2D6C4O2S", 126.2026108, 126.062161278)]
        [TestCase("H(14) C(8) O", 126.1962, 126.104465, "H14C8O", 126.199, 126.10446507079)]
        [TestCase("H(9) C(6) N O(2)", 127.1412, 127.063329, "H9C6NO2", 127.143, 127.06332853364)]
        [TestCase("H(13) C(7) N O", 127.1842, 127.099714, "H13C7NO", 127.187, 127.09971404299)]
        [TestCase("H(12) C(6) N(2) O", 128.1723, 128.094963, "H12C6N2O", 128.175, 128.09496301519)]
        [TestCase("H(14) C(7) N O", 128.1922, 128.107539, "H14C7NO", 128.195, 128.10753907522)]
        [TestCase("H(16) C(7) N(2)", 128.2153, 128.131349, "H16C7N2", 128.219, 128.13134852454)]
        [TestCase("H(7) C(5) N O(3)", 129.114, 129.042593, "H7C5NO3", 129.115, 129.04259308875)]
        [TestCase("H(7) C(9) N", 129.1586, 129.057849, "H7C9N", 129.162, 129.05784923004)]
        [TestCase("H(6) C(5) O(4)", 130.0987, 130.026609, "H6C5O4", 130.099, 130.02660867166)]
        [TestCase("H(2) 2H(5) C(6) N O(2)", 130.1561, 130.079062, "H2D5C6NO2", 130.157509, 130.07906230803)]
        [TestCase("H(10) 2H(3) C(7) N O", 130.2027, 130.118544, "H10D3C7NO", 130.2053054, 130.1185443463)]
        [TestCase("H(13) C(6) N O(2)", 131.1729, 131.094629, "H13C6NO2", 131.175, 131.09462866256)]
        [TestCase("H(4) C(4) O(5)", 132.0716, 132.005873, "H4C4O5", 132.071, 132.00587322677)]
        [TestCase("H(4) C(8) O(2)", 132.1162, 132.021129, "H4C8O2", 132.118, 132.02112936806)]
        [TestCase("H(8) C(9) O", 132.1592, 132.057515, "H8C9O", 132.162, 132.05751487741)]
        [TestCase("H(8) C(8) N(2)", 132.1625, 132.068748, "H8C8N2", 132.166, 132.0687482667)]
        [TestCase("H(7) C(4) N O(2) S", 133.1689, 133.019749, "H7C4NO2S", 133.165, 133.01974964358)]
        [TestCase("H(7) C(8) N O", 133.1473, 133.052764, "H7C8NO", 133.15, 133.05276384961)]
        [TestCase("H(7) 2H(6) C(7) N O", 133.2212, 133.137375, "H7D6C7NO", 133.2236108, 133.13737464961)]
        [TestCase("H(6) C(7) N(2) O", 134.1353, 134.048013, "H6C7N2O", 134.138, 134.04801282181)]
        [TestCase("H(5) C(7) N O(2)", 135.1201, 135.032028, "H5C7NO2", 135.122, 135.03202840472)]
        [TestCase("H(10) C(4) N O(2) P", 135.1015, 135.044916, "H10C4NO2P", 135.102761998, 135.04491556429)]
        [TestCase("H(4) C(3) O(4) S", 136.1265, 135.983029, "H4C3O4S", 136.121, 135.9830297816)]
        [TestCase("H(8) C(4) O S(2)", 136.2357, 136.001656, "H8C4OS2", 136.227, 136.00165722621, 0.009)]
        [TestCase("H(9) C(4) O(3) P", 136.0862, 136.028931, "H9C4O3P", 136.086761998, 136.0289311472)]
        [TestCase("H(12) C(9) O", 136.191, 136.088815, "H12C9O", 136.194, 136.08881500633)]
        [TestCase("H(4) 2H(9) C(7) N O", 136.2397, 136.156205, "H4D9C7NO", 136.2419162, 136.15620495292)]
        [TestCase("H(5) 2H(9) C(7) N O", 137.2476, 137.16403, "H5D9C7NO", 137.2499162, 137.16402998515)]
        [TestCase("H(10) C(8) O(2)", 138.1638, 138.06808, "H10C8O2", 138.166, 138.06807956144)]
        [TestCase("H(14) C(9) O", 138.2069, 138.104465, "H14C9O", 138.21, 138.10446507079)]
        [TestCase("H(7) C(2) 13C(6) N O", 139.1032, 139.072893, "H7C2^13.003355C6NO", 139.10413, 139.07289384961)]
        [TestCase("H(13) C(7) N(3)", 139.1982, 139.110947, "H13C7N3", 139.202, 139.11094743228)]
        [TestCase("H(12) C(7) N(2) O", 140.183, 140.094963, "H12C7N2O", 140.186, 140.09496301519)]
        [TestCase("H(7) C(6) N O(3)", 141.1247, 141.042593, "H7C6NO3", 141.126, 141.04259308875)]
        [TestCase("H(12) C(6) 13C N(2) O", 141.1756, 141.098318, "H12C6^13.003355CN2O", 141.178355, 141.09831801519)]
        [TestCase("H(2) 2H(6) C(4) O S(2)", 142.2727, 142.039317, "H2D6C4OS2", 142.2636108, 142.03931783283)]
        [TestCase("H(14) C(7) N(2) O", 142.1989, 142.110613, "H14C7N2O", 142.202, 142.11061307965)]
        [TestCase("H(9) C(6) N O(3)", 143.1406, 143.058243, "H9C6NO3", 143.142, 143.05824315321)]
        [TestCase("H(8) C(6) O(4)", 144.1253, 144.042259, "H8C6O4", 144.126, 144.04225873612)]
        [TestCase("H(12) C(6) 13C N 15N 18O", 144.1688, 144.099599, "H12C6^13.003355CN^15.000109N^17.999161O", 144.171625, 144.09959939119)]
        [TestCase("H(12) C(4) 13C(3) N 15N O", 144.1544, 144.102063, "H12C4^13.003355C3N^15.000109NO", 144.156174, 144.10206301076)]
        [TestCase("H(4) 2H(6) C(8) O(2)", 144.2008, 144.10574, "H4D6C8O2", 144.2026108, 144.10574016806)]
        [TestCase("H(12) C(5) 13C(2) N(2) 18O", 144.168, 144.105918, "H12C5^13.003355C2N2^17.999161O", 144.170871, 144.10591939562)]
        [TestCase("H(7) C(5) N O(2) S", 145.1796, 145.019749, "H7C5NO2S", 145.176, 145.01974964358)]
        [TestCase("H(15) C(7) 13C 15N 18O", 145.1966, 145.12, "H15C7^13.003355C^15.000109N^17.999161O", 145.199625, 145.12000048345)]
        [TestCase("H(13) 2H(2) C(7) 13C 15N O", 145.2092, 145.128307, "H13D2C7^13.003355C^15.000109NO", 145.2116676, 145.12830763856)]
        [TestCase("H(13) 2H(2) C(8) N 18O", 145.2229, 145.132163, "H13D2C8N^17.999161O", 145.2263646, 145.13216402342)]
        [TestCase("H(11) 2H(4) C(8) N O", 145.2354, 145.140471, "H11D4C8NO", 145.2384072, 145.14047117853)]
        [TestCase("H(6) C(9) O(2)", 146.1427, 146.036779, "H6C9O2", 146.145, 146.03677943252)]
        [TestCase("H(4) 2H(5) C(6) N O(3)", 148.1714, 148.089627, "H4D5C6NO3", 148.172509, 148.08962699206)]
        [TestCase("H(12) C 13C(6) 15N(2) O", 148.1257, 148.109162, "H12C^13.003355C6^15.000109N2O", 148.126348, 148.10916300633)]
        [TestCase("H(7) C(8) N S", 149.2129, 149.02992, "H7C8NS", 149.211, 149.02992040444)]
        [TestCase("H(4) C(5) N(5) O", 150.1182, 150.041585, "H4C5N5O", 150.121, 150.04158477064)]
        [TestCase("H(8) C(4) O(2) S(2)", 152.2351, 151.996571, "H8C4O2S2", 152.226, 151.99657184578, 0.0095)]
        [TestCase("H(9) C(4) O(2) P S", 152.1518, 152.006087, "H9C4O2PS", 152.147761998, 152.00608770203)]
        [TestCase("H(3) C(6) N O(2) S", 153.1585, 152.988449, "H3C6NO2S", 153.155, 152.98844951466)]
        [TestCase("H(7) C(3) O(5) P", 154.0584, 154.00311, "H7C3O5P", 154.057761998, 154.00311032188)]
        [TestCase("H(6) C(7) O(4)", 154.1201, 154.026609, "H6C7O4", 154.121, 154.02660867166)]
        [TestCase("H(10) C(7) N(2) O(2)", 154.1665, 154.074228, "H10C7N2O2", 154.169, 154.0742275703)]
        [TestCase("H(14) C(9) O(2)", 154.2063, 154.09938, "H14C9O2", 154.209, 154.09937969036)]
        [TestCase("H(14) C(8) N(2) O", 154.2096, 154.110613, "H14C8N2O", 154.213, 154.11061307965)]
        [TestCase("H(18) C(10) O", 154.2493, 154.135765, "H18C10O", 154.253, 154.13576519971)]
        [TestCase("H(5) C(6) N O(2) S", 155.1744, 155.004099, "H5C6NO2S", 155.171, 155.00409957912)]
        [TestCase("H(13) C(8) N O(2)", 155.1943, 155.094629, "H13C8NO2", 155.197, 155.09462866256)]
        [TestCase("H(-2) Br(2)", 157.7921, 155.821022, "Br2>H2", 0, 0)]
        [TestCase("H(5) C(6) O(3) P", 156.0759, 155.997631, "H5C6O3P", 156.076761998, 155.99763101828)]
        [TestCase("H(12) C(8) O(3)", 156.1791, 156.078644, "H12C8O3", 156.181, 156.07864424547)]
        [TestCase("H(12) C(6) N(4) O", 156.1857, 156.101111, "H12C6N4O", 156.189, 156.10111102405)]
        [TestCase("H(16) C(9) O(2)", 156.2221, 156.11503, "H16C9O2", 156.225, 156.11502975482)]
        [TestCase("H(7) C(6) N O(2) S", 157.1903, 157.019749, "H7C6NO2S", 157.187, 157.01974964358)]
        [TestCase("H(6) C(6) O(3) S", 158.175, 158.003765, "H6C6O3S", 158.171, 158.00376522649)]
        [TestCase("H(18) C(9) O(2)", 158.238, 158.13068, "H18C9O2", 158.241, 158.13067981928)]
        [TestCase("H(3) 13C(6) N O(2) S", 159.1144, 159.008578, "H3^13.003355C6NO2S", 159.10913, 159.00857951466)]
        [TestCase("H(9) C(6) N O(2) S", 159.2062, 159.035399, "H9C6NO2S", 159.203, 159.03539970804)]
        [TestCase("H(9) C(10) N O", 159.1846, 159.068414, "H9C10NO", 159.188, 159.06841391407)]
        [TestCase("H(2) O(6) P(2)", 159.9598, 159.932662, "H2O6P2", 159.957523996, 159.93266177872)]
        [TestCase("H(8) C(6) O(5)", 160.1247, 160.037173, "H8C6O5", 160.125, 160.03717335569)]
        [TestCase("H(12) C(6) N(2) O(3)", 160.1711, 160.084792, "H12C6N2O3", 160.173, 160.08479225433)]
        [TestCase("H(5) 13C(6) N O(2) S", 161.1303, 161.024228, "H5^13.003355C6NO2S", 161.12513, 161.02422957912)]
        [TestCase("H(7) C(5) N O(5)", 161.1128, 161.032422, "H7C5NO5", 161.113, 161.03242232789)]
        [TestCase("H(13) C(11) O", 161.2203, 161.09664, "H13C11O", 161.224, 161.09664003856)]
        [TestCase("H(10) C(9) N(2) O", 162.1885, 162.079313, "H10C9N2O", 162.192, 162.07931295073)]
        [TestCase("H(15) C(7) O(2) P", 162.1666, 162.080967, "H15C7O2P", 162.168761998, 162.08096672101)]
        [TestCase("H(18) C(8) O(3)", 162.2267, 162.125595, "H18C8O3", 162.229, 162.12559443885)]
        [TestCase("H(9) C(5) N O(3) S", 163.1949, 163.030314, "H9C5NO3S", 163.191, 163.03031432761)]
        [TestCase("H(9) C(9) N S", 163.2395, 163.04557, "H9C9NS", 163.238, 163.0455704689)]
        [TestCase("C(6) N(2) O(4)", 164.0752, 163.985807, "C6N2O4", 164.076, 163.98580648714)]
        [TestCase("H(13) C(6) O(3) P", 164.1394, 164.060231, "H13C6O3P", 164.140761998, 164.06023127612)]
        [TestCase("H(21) C(12)", 165.2951, 165.164326, "H21C12", 165.3, 165.16432567683)]
        [TestCase("H(2) C(6) N(2) O(4)", 166.0911, 166.001457, "H2C6N2O4", 166.092, 166.0014565516)]
        [TestCase("H(5) C(3) O(6) P", 168.042, 167.982375, "H5C3O6P", 168.040761998, 167.98237487699)]
        [TestCase("H(8) C(8) O(2) S", 168.2129, 168.0245, "H8C8O2S", 168.21, 168.02450067138)]
        [TestCase("H(24) C(12)", 168.319, 168.187801, "H24C12", 168.324, 168.18780077352)]
        [TestCase("H(7) C(6) N(3) O(3)", 169.1381, 169.048741, "H7C6N3O3", 169.14, 169.04874109761)]
        [TestCase("H(7) C(7) O(3) P", 170.1024, 170.013281, "H7C7O3P", 170.103761998, 170.01328108274)]
        [TestCase("H(6) C(11) O(2)", 170.1641, 170.036779, "H6C11O2", 170.167, 170.03677943252)]
        [TestCase("H(6) C(10) N(2) O", 170.1674, 170.048013, "H6C10N2O", 170.171, 170.04801282181)]
        [TestCase("H(19) C(9) N(2) O", 171.26, 171.149738, "H19C9N2O", 171.264, 171.1497382408)]
        [TestCase("H(8) C(6) N(2) S(2)", 172.2711, 172.01289, "H8C6N2S2", 172.264, 172.0128906155, 0.0075)]
        [TestCase("H(8) C(11) O(2)", 172.18, 172.05243, "H8C11O2", 172.183, 172.05242949698)]
        [TestCase("H(7) C(6) N O(2) S(-1) Se", 172.0203, 172.992127, "H7C6NO2Se>S1", 0, 0)]
        [TestCase("H(13) C(7) N(2) O(3)", 173.1897, 173.092617, "H13C7N2O3", 173.192, 173.09261728656)]
        [TestCase("H(6) C(6) O(2) S(2)", 174.2406, 173.980921, "H6C6O2S2", 174.232, 173.98092178132)]
        [TestCase("H(6) C(9) N(2) S", 174.2223, 174.025169, "H6C9N2S", 174.221, 174.02516937664)]
        [TestCase("H(16) 2H(3) C(9) N(2) O", 174.2784, 174.168569, "H16D3C9N2O", 174.2823054, 174.16856854411)]
        [TestCase("H(9) C(6) N O(3) S", 175.2056, 175.030314, "H9C6NO3S", 175.202, 175.03031432761)]
        [TestCase("H(5) C(13) N", 175.1855, 175.042199, "H5C13N", 175.19, 175.04219916558)]
        [TestCase("H(8) C(6) O(4) S", 176.1903, 176.01433, "H8C6O4S", 176.186, 176.01432991052)]
        [TestCase("H(-7) O Fe(3)", 176.4788, 176.744957, "OFe3>H7", 0, 0)]
        [TestCase("H(2) 2H(5) C(6) N O(2) S(-1) Se", 177.0511, 178.023511, "H2D5C6NO2Se>S1", 0, 0)]
        [TestCase("H(9) C(8) N O(2) S", 183.2276, 183.035399, "H9C8NO2S", 183.225, 183.03539970804)]
        [TestCase("H(4) C(7) O(4) S", 184.1693, 183.983029, "H4C7O4S", 184.165, 183.9830297816)]
        [TestCase("H(14) C(9) N O S", 184.2786, 184.07961, "H14C9NOS", 184.277, 184.07961024962)]
        [TestCase("H(20) C(10) N(2) O", 184.2786, 184.157563, "H20C10N2O", 184.283, 184.15756327303)]
        [TestCase("H(23) C(10) N(3)", 185.3097, 185.189198, "H23C10N3", 185.315, 185.18919775458)]
        [TestCase("H(7) C(7) O(4) P", 186.1018, 186.008196, "H7C7O4P", 186.102761998, 186.00819570231)]
        [TestCase("H(12) C(8) O S(2)", 188.3103, 188.032956, "H12C8OS2", 188.303, 188.03295735513)]
        [TestCase("H(20) C(14)", 188.3086, 188.156501, "H20C14", 188.314, 188.1565006446)]
        [TestCase("H(15) 2H(5) C(10) N(2) O", 189.3094, 189.188947, "H15D5C10N2O", 189.313509, 189.18894711188)]
        [TestCase("H(10) C(10) N(2) O(2)", 190.1986, 190.074228, "H10C10N2O2", 190.202, 190.0742275703)]
        [TestCase("H(8) C(6) O(3) S(2)", 192.2559, 191.991486, "H8C6O3S2", 192.247, 191.99148646535, 0.009)]
        [TestCase("H(9) C(7) N(5) O(2)", 195.1787, 195.075625, "H9C7N5O2", 195.182, 195.07562455136)]
        [TestCase("H(12) C(9) N(2) O(3)", 196.2032, 196.084792, "H12C9N2O3", 196.206, 196.08479225433)]
        [TestCase("H(14) C(9) N(3) O(2)", 196.2264, 196.108602, "H14C9N3O2", 196.23, 196.10860170365)]
        [TestCase("H(16) C(10) N(2) O(2)", 196.2462, 196.121178, "H16C10N2O2", 196.25, 196.12117776368)]
        [TestCase("H(12) C(5) N O(5) P", 197.1262, 197.04531, "H12C5NO5P", 197.126761998, 197.04530948746)]
        [TestCase("H(3) C(6) N(2) O(4) S", 199.164, 198.981352, "H3C6N2O4S", 199.16, 198.98135275823)]
        [TestCase("H(13) C(9) N O(2) S", 199.27, 199.066699, "H13C9NO2S", 199.268, 199.06669983696)]
        [TestCase("Hg", 200.59, 201.970617, "Hg", 200.592, 201.9706434)]
        [TestCase("H(7) C(4) O(3) P Cl(2)", 204.9763, 203.950987, "H7C4O3PCl2", 204.970761998, 203.95098644674, 0.0057)]
        [TestCase("H(24) C(15)", 204.3511, 204.187801, "H24C15", 204.357, 204.18780077352)]
        [TestCase("H(22) C(14) O", 206.3239, 206.167065, "H22C14O", 206.329, 206.16706532863)]
        [TestCase("H(24) C(14) O", 208.3398, 208.182715, "H24C14O", 208.345, 208.18271539309)]
        [TestCase("H(11) C(6) N O(3) S(2)", 209.2864, 209.018035, "H11C6NO3S2", 209.278, 209.01803556647)]
        [TestCase("H(10) C(6) O(4) S(2)", 210.2712, 210.00205, "H10C6O4S2", 210.262, 210.00205114938)]
        [TestCase("H(22) C(13) O(2)", 210.3126, 210.16198, "H22C13O2", 210.317, 210.1619799482)]
        [TestCase("H(26) C(14) O", 210.3556, 210.198366, "H26C14O", 210.361, 210.19836545755)]
        [TestCase("H C(6) N(3) O(6)", 211.0886, 210.986535, "HC6N3O6", 211.089, 210.98653476294)]
        [TestCase("H(9) C(5) O(7) P", 212.0945, 212.00859, "H9C5O7P", 212.093761998, 212.00858962548)]
        [TestCase("H(15) C(9) N(3) O(3)", 213.2337, 213.111341, "H15C9N3O3", 213.237, 213.11134135545)]
        [TestCase("H(14) C(9) N(2) O(4)", 214.2185, 214.095357, "H14C9N2O4", 214.221, 214.09535693836)]
        [TestCase("H(5) C(7) N O(3) S(2)", 215.2495, 214.971084, "H5C7NO3S2", 215.241, 214.97108537309, 0.0089)]
        [TestCase("H(7) C(9) N O Cl(2)", 216.064, 214.990469, "H7C9NOCl2", 216.061, 214.99046921361, 0.0035)]
        [TestCase("H(16) C(10) O(5)", 216.231, 216.099774, "H16C10O5", 216.233, 216.09977361353)]
        [TestCase("H(15) C(9) N O(5)", 217.2191, 217.095023, "H15C9NO5", 217.221, 217.09502258573)]
        [TestCase("H(20) C(8) 13C(3) 15N(2) O(2)", 217.2535, 217.156612, "H20C8^13.003355C3^15.000109N2O2", 217.256283, 217.15661288374)]
        [TestCase("H(18) 2H(2) C(10) 13C 15N(2) O(2)", 217.2805, 217.162456, "H18D2C10^13.003355C^15.000109N2O2", 217.2837766, 217.16245641928)]
        [TestCase("H(20) C(7) 13C(4) N 15N O(2)", 217.2527, 217.162932, "H20C7^13.003355C4N^15.000109NO2", 217.255529, 217.16293288817)]
        [TestCase("H(18) 2H(2) C(9) 13C(2) N 15N O(2)", 217.2797, 217.168776, "H18D2C9^13.003355C2N^15.000109NO2", 217.2830226, 217.16877642371)]
        [TestCase("H(18) 2H(2) C(8) 13C(3) N(2) O(2)", 217.279, 217.175096, "H18D2C8^13.003355C3N2O2", 217.2822686, 217.17509642814)]
        [TestCase("H(22) C(15) O", 218.3346, 218.167065, "H22C15O", 218.34, 218.16706532863)]
        [TestCase("H(13) C(12) N O(3)", 219.2365, 219.089543, "H13C12NO3", 219.24, 219.08954328213)]
        [TestCase("H(8) C(10) N(2) O(4)", 220.1815, 220.048407, "H8C10N2O4", 220.184, 220.04840674498)]
        [TestCase("H(12) C(8) O(7)", 220.1767, 220.058303, "H12C8O7", 220.177, 220.05830272375)]
        [TestCase("H(24) C(15) O", 220.3505, 220.182715, "H24C15O", 220.356, 220.18271539309)]
        [TestCase("H(5) C 13C(6) N O(3) S(2)", 221.2054, 220.991213, "H5C^13.003355C6NO3S2", 221.19513, 220.99121537309)]
        [TestCase("H(20) C(12) N(2) O(2)", 224.2994, 224.152478, "H20C12N2O2", 224.304, 224.1524778926)]
        [TestCase("H(32) C(16)", 224.4253, 224.250401, "H32C16", 224.432, 224.25040103136)]
        [TestCase("H(11) C(13) N(3) O", 225.2459, 225.090212, "H11C13N3O", 225.251, 225.09021198739)]
        [TestCase("H(15) C(10) N(3) O S", 225.3106, 225.093583, "H15C10N3OS", 225.31, 225.09358329071)]
        [TestCase("H(20) C(11) 13C N(2) O(2)", 225.2921, 225.155833, "H20C11^13.003355CN2O2", 225.296355, 225.1558328926)]
        [TestCase("H(10) C(10) O(6)", 226.1828, 226.047738, "H10C10O6", 226.184, 226.04773803972)]
        [TestCase("H(14) C(10) N(2) O(2) S", 226.2954, 226.077598, "H14C10N2O2S", 226.294, 226.07759887362, 0)]
        [TestCase("H(17) C(10) N(3) O(3)", 227.2603, 227.126991, "H17C10N3O3", 227.264, 227.12699141991)]
        [TestCase("H(29) C(14) N O", 227.3862, 227.224915, "H29C14NO", 227.392, 227.22491455867)]
        [TestCase("H(8) C(8) N O(5) P", 229.1266, 229.014009, "H8C8NO5P", 229.127761998, 229.01400935854)]
        [TestCase("H(20) C(8) 13C(4) N 15N O(2)", 229.2634, 229.162932, "H20C8^13.003355C4N^15.000109NO2", 229.266529, 229.16293288817)]
        [TestCase("H(10) C(8) N O(5) P", 231.1425, 231.02966, "H10C8NO5P", 231.143761998, 231.029659423)]
        [TestCase("H(14) C(9) N O(4) S", 232.2768, 232.064354, "H14C9NO4S", 232.274, 232.06435410833)]
        [TestCase("H(11) C(12) N O(2) S", 233.2862, 233.051049, "H11C12NO2S", 233.285, 233.0510497725)]
        [TestCase("H(14) C(9) O(7)", 234.2033, 234.073953, "H14C9O7", 234.204, 234.07395278821)]
        [TestCase("H(22) C(15) O(2)", 234.334, 234.16198, "H22C15O2", 234.339, 234.1619799482)]
        [TestCase("H(20) C(3) 13C(9) 15N(2) O(2)", 235.2201, 235.176741, "H20C3^13.003355C9^15.000109N2O2", 235.221413, 235.17674288374)]
        [TestCase("H(17) C 13C(9) N(3) O(3)", 236.1942, 236.157185, "H17C^13.003355C9N3O3", 236.195195, 236.15718641991)]
        [TestCase("H(28) C(16) O", 236.3929, 236.214016, "H28C16O", 236.399, 236.21401552201)]
        [TestCase("H(15) C(12) N O(4)", 237.2518, 237.100108, "H15C12NO4", 237.255, 237.10010796616)]
        [TestCase("H(30) C(16) O", 238.4088, 238.229666, "H30C16O", 238.415, 238.22966558647)]
        [TestCase("H(16) C(10) N(4) O S", 240.3252, 240.104482, "H16C10N4OS", 240.325, 240.10448232737)]
        [TestCase("H(16) C(16) O(2)", 240.297, 240.11503, "H16C16O2", 240.302, 240.11502975482)]
        [TestCase("H(15) C(10) N(3) O(2) S", 241.31, 241.088497, "H15C10N3O2S", 241.309, 241.08849791028)]
        [TestCase("H(14) C(9) N(4) O(4)", 242.2319, 242.101505, "H14C9N4O4", 242.235, 242.10150494722)]
        [TestCase("H(13) C(9) N(3) O(5)", 243.2166, 243.085521, "H13C9N3O5", 243.219, 243.08552053013)]
        [TestCase("H(12) C(10) O(7)", 244.1981, 244.058303, "H12C10O7", 244.199, 244.05830272375)]
        [TestCase("H(15) C(9) 13C N(2) O(5)", 244.2292, 244.101452, "H15C9^13.003355CN2O5", 244.231355, 244.10145159016)]
        [TestCase("H(28) C(13) O(4)", 248.359, 248.19876, "H28C13O4", 248.363, 248.19875938072)]
        [TestCase("H(4) C(10) N O(5) S", 250.2075, 249.981018, "H4C10NO5S", 250.204, 249.9810184056)]
        [TestCase("H(-2) I(2)", 251.7931, 251.793296, "I2>H2", 0, 0)]
        [TestCase("H(10) C(10) N(3) O(3) S", 252.2697, 252.044287, "H10C10N3O3S", 252.268, 252.0442873687)]
        [TestCase("H(12) C(9) N(6) O(3)", 252.23, 252.097088, "H12C9N6O3", 252.234, 252.09708827205)]
        [TestCase("H(12) C(11) N O Br", 254.1231, 253.010225, "H12C11NOBr", 254.127, 253.01022661076)]
        [TestCase("H(11) C(6) O(9) P", 258.1199, 258.014069, "H11C6O9P", 258.118761998, 258.01406892908)]
        [TestCase("H(14) C(10) N(2) O(6)", 258.228, 258.085186, "H14C10N2O6", 258.23, 258.0851861775)]
        [TestCase("H(18) C(10) N(4) O(2) S", 258.3405, 258.115047, "H18C10N4O2S", 258.34, 258.1150470114)]
        [TestCase("H(21) C(12) N O(5)", 259.2988, 259.141973, "H21C12NO5", 259.302, 259.14197277911)]
        [TestCase("H(17) C(18) N O", 263.3337, 263.131014, "H17C18NO", 263.34, 263.13101417191)]
        [TestCase("H(31) C(18) O", 263.4381, 263.237491, "H31C18O", 263.445, 263.2374906187)]
        [TestCase("H(24) C(20)", 264.4046, 264.187801, "H24C20", 264.412, 264.18780077352, 0.008)]
        [TestCase("H(19) C(18) N O", 265.3496, 265.146664, "H19C18NO", 265.356, 265.14666423637)]
        [TestCase("H(10) C(16) O(4)", 266.2482, 266.057909, "H10C16O4", 266.252, 266.05790880058)]
        [TestCase("H(18) C(18) O(2)", 266.3343, 266.13068, "H18C18O2", 266.34, 266.13067981928)]
        [TestCase("H(26) C(20)", 266.4204, 266.203451, "H26C20", 266.428, 266.20345083798, 0.008)]
        [TestCase("H(9) C(10) N(3) O(4) S", 267.2612, 267.031377, "H9C10N3O4S", 267.259, 267.03137695604)]
        [TestCase("H(21) C(13) N(3) O(3)", 267.3241, 267.158292, "H21C13N3O3", 267.329, 267.15829154883)]
        [TestCase("H(10) C(10) N(3) O(4) S", 268.2691, 268.039202, "H10C10N3O4S", 268.267, 268.03920198827)]
        [TestCase("H(20) C(11) 13C N(3) O(4)", 271.2976, 271.148736, "H20C11^13.003355CN3O4", 271.301355, 271.14873613617)]
        [TestCase("H(32) C(20)", 272.4681, 272.250401, "H32C20", 272.476, 272.25040103136, 0.008)]
        [TestCase("H(13) C(14) O(4) P", 276.2244, 276.055146, "H13C14O4P", 276.227761998, 276.05514589569)]
        [TestCase("H(17) C(10) N O(6) S", 279.3101, 279.077658, "H17C10NO6S", 279.307, 279.07765844416)]
        [TestCase("H(10) C(16) O(5)", 282.2476, 282.052824, "H10C16O5", 282.251, 282.05282342015)]
        [TestCase("H(11) C(15) O(6)", 287.2442, 287.055563, "H11C15O6", 287.247, 287.05556307195)]
        [TestCase("H(26) C(19) N(-2) O(4)", 290.3939, 290.176961, "H26C19O4>N2", 0, 0)]
        [TestCase("H(26) C(17) O(4)", 294.3859, 294.183109, "H26C17O4", 294.391, 294.18310931626)]
        [TestCase("H(25) C(15) N(3) O(3)", 295.3773, 295.189592, "H25C15N3O3", 295.383, 295.18959167775)]
        [TestCase("H(13) C(12) N(2) O(2) Br", 297.1478, 296.016039, "H13C12N2O2Br", 297.152, 296.01604026699)]
        [TestCase("H(13) C(10) 13C(2) N(2) O(2) Br", 299.1331, 298.022748, "H13C10^13.003355C2N2O2Br", 299.13671, 298.02275026699)]
        [TestCase("H(22) C(13) N(4) O(2) S", 298.4044, 298.146347, "H22C13N4O2S", 298.405, 298.14634714032)]
        [TestCase("H(26) C(20) O(2)", 298.4192, 298.19328, "H26C20O2", 298.426, 298.19328007712)]
        [TestCase("H(25) C(14) N(3) O(2) S", 299.4322, 299.166748, "H25C14N3O2S", 299.433, 299.16674823258)]
        [TestCase("H(8) C(4) N(5) O(7) S(2)", 302.2656, 301.986514, "H8C4N5O7S2", 302.256, 301.98651496578, 0.0099)]
        [TestCase("H(25) C(10) 13C(4) N(2) 15N O(2) S", 304.3962, 304.177202, "H25C10^13.003355C4N2^15.000109NO2S", 304.395529, 304.17720322815)]
        [TestCase("H(24) C(8) 13C(6) N(2) 15N(2) O(3)", 304.3081, 304.19904, "H24C8^13.003355C6N2^15.000109N2O3", 304.311348, 304.19904064109)]
        [TestCase("H(24) C(7) 13C(7) N(3) 15N O(3)", 304.3074, 304.20536, "H24C7^13.003355C7N3^15.000109NO3", 304.310594, 304.20536064552)]
        [TestCase("H(25) C(8) 13C(7) N 15N(2) O(3)", 304.3127, 304.207146, "H25C8^13.003355C7N^15.000109N2O3", 304.315703, 304.20714666889)]
        [TestCase("H(12) C(9) N(3) O(7) P", 305.1812, 305.041287, "H12C9N3O7P", 305.182761998, 305.04128673546)]
        [TestCase("H(15) C(10) N(3) O(6) S", 305.3076, 305.068156, "H15C10N3O6S", 305.305, 305.06815638856)]
        [TestCase("H(11) C(9) N(2) O(8) P", 306.166, 306.025302, "H11C9N2O8P", 306.166761998, 306.02530231837)]
        [TestCase("H(18) C(12) O(9)", 306.2659, 306.095082, "H18C12O9", 306.267, 306.09508215627)]
        [TestCase("H(26) C(19) N(-2) O(5)", 306.3933, 306.171876, "H26C19O5>N2", 0, 0)]
        [TestCase("H(20) C(14) N(4) O(4)", 308.333, 308.148455, "H20C14N4O4", 308.338, 308.1484551406)]
        [TestCase("H(27) C(16) N(3) O(3)", 309.4039, 309.205242, "H27C16N3O3", 309.41, 309.20524174221)]
        [TestCase("H(10) C(17) O(6)", 310.2577, 310.047738, "H10C17O6", 310.261, 310.04773803972)]
        [TestCase("H(22) C(15) N(2) O(3) S", 310.4118, 310.135113, "H22C15N2O3S", 310.412, 310.13511375103)]
        [TestCase("H(25) C(15) N(3) O(2) S", 311.4429, 311.166748, "H25C15N3O2S", 311.444, 311.16674823258)]
        [TestCase("H(24) C(15) N(2) O(3) S", 312.4277, 312.150763, "H24C15N2O3S", 312.428, 312.15076381549)]
        [TestCase("H(26) C(20) O(3)", 314.4186, 314.188195, "H26C20O3", 314.425, 314.18819469669)]
        [TestCase("H(21) C(22) P", 316.3759, 316.138088, "H21C22P", 316.383761998, 316.13808767525, 0.008)]
        [TestCase("H(28) C(20) O(3)", 316.4345, 316.203845, "H28C20O3", 316.441, 316.20384476115)]
        public void TestUniModFormulas(
            string formula,
            double avgMassUniMod,
            double monoMassUniMod,
            string expectedUpdatedFormula,
            double expectedAvgMass,
            double expectedMonoMass,
            double matchToleranceVsUniModAvg = 0.007,
            double matchToleranceVsUniModMono = 0.0005,
            double matchTolerance = MATCHING_MASS_EPSILON)
        {
            if (!mTestResultWriters[UnitTestWriterType.UniModFormulaWriter].FilePathShown)
            {
                ShowAtConsoleAndLog(UnitTestWriterType.UniModFormulaWriter);
            }

            if (!mUniModHeaderWritten)
            {
                mTestResultWriters[UnitTestWriterType.UniModFormulaWriter].WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                    "UniMod Formula", "UniMod Average Mass", "UniMod Isotopic Mass",
                    "Updated Formula", "Computed Average Mass", "Computed Monoisotopic Mass");

                mUniModHeaderWritten = true;
            }

            var averageMass = mAverageMassCalculator.ComputeMass(formula);
            var isotopicMass = mMonoisotopicMassCalculator.ComputeMass(formula);

            mTestResultWriters[UnitTestWriterType.UniModFormulaWriter].WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                formula, avgMassUniMod, monoMassUniMod, mMonoisotopicMassCalculator.Compound.FormulaCapitalized, averageMass, isotopicMass);

            Console.WriteLine("{0,-15} -> {1,-15}: {2,12:F8} Da (average) and  {3,12:F8} Da (isotopic)",
                formula, mMonoisotopicMassCalculator.Compound.FormulaCapitalized, averageMass, isotopicMass);

            var optionalMassToleranceArgument = Math.Abs(matchToleranceVsUniModAvg - 0.007) < 0.000001 ? string.Empty : ", " + matchToleranceVsUniModAvg;

            WriteUpdatedTestCase("TestUniModFormulas",
                "[TestCase(\"{0}\", {1}, {2}, \"{3}\", {4}, {5}{6})]",
                formula, avgMassUniMod, monoMassUniMod, mMonoisotopicMassCalculator.Compound.FormulaCapitalized, averageMass, isotopicMass, optionalMassToleranceArgument);

            if (mCompareTextToExpected)
            {
                Assert.AreEqual(expectedUpdatedFormula, mMonoisotopicMassCalculator.Compound.FormulaCapitalized,
                    "Capitalized formula does not match the expected value");
            }

            if (mCompareValuesToExpected)
            {
                Assert.AreEqual(expectedAvgMass, averageMass, matchTolerance, "Actual mass does not match expected average mass");
                Assert.AreEqual(expectedMonoMass, isotopicMass, matchTolerance, "Actual mass does not match expected isotopic mass");
            }

            if (isotopicMass == 0 || !mCompareValuesToExpected)
                return;

            Assert.AreEqual(monoMassUniMod, isotopicMass, matchToleranceVsUniModMono, "Computed monoisotopic mass is not within tolerance of the UniMod mass");

            if (Math.Abs(avgMassUniMod - averageMass) > matchToleranceVsUniModAvg)
            {
                if (mMonoisotopicMassCalculator.Compound.FormulaCapitalized.Contains("S"))
                {
                    // UniMod uses a different average mass for S
                    Assert.AreEqual(avgMassUniMod, averageMass, 0.015, "Computed average mass is not within tolerance of the UniMod mass (compound with sulfur)");
                    return;
                }
            }

            Assert.AreEqual(avgMassUniMod, averageMass, matchToleranceVsUniModAvg, "Computed average mass is not within tolerance of the UniMod mass");
        }
    }
}
