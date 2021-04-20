using System;
using System.Collections.Generic;
using System.Text;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using NUnit.Framework;

namespace UnitTests
{
    public class CompoundTests : TestBase
    {
        // ReSharper disable CommentTypo

        // Ignore Spelling: arkcas, Bpy, Da, Gly, Leu, Nz, rkcas, sipsclarkcas, Tyr

        // ReSharper restore CommentTypo

        /// <summary>
        /// Initialize the Molecular Weight Calculator objects and results writers
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            Initialize();

            mTestResultWriters = new Dictionary<UnitTestWriterType, UnitTestResultWriter>();

            InitializeResultsWriter(UnitTestWriterType.ComputeMass, "UnitTestResults_ComputeMass.txt");
            InitializeResultsWriter(UnitTestWriterType.StressTest, "UnitTestResults_ComputeMassStressTest.txt");
            InitializeResultsWriter(UnitTestWriterType.ConvertToEmpirical, "UnitTestResults_ConvertToEmpirical.txt");
            InitializeResultsWriter(UnitTestWriterType.CircularReferenceHandling, "UnitTestResults_CircularReferenceHandling.txt");
            InitializeResultsWriter(UnitTestWriterType.ExpandAbbreviations, "UnitTestResults_ExpandAbbreviations.txt");
            InitializeResultsWriter(UnitTestWriterType.PercentComposition, "UnitTestResults_PercentComposition.txt");
            InitializeResultsWriter(UnitTestWriterType.UnitTestCaseWriter, "UnitTestCases_Compound.txt");
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
    }
}
