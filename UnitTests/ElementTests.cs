using System;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ElementTests
    {
        //
        // To see the Console.Writeline() results for a series of test cases for a given Test, run NUnit from the command line.  For example:
        // cd "C:\Program Files (x86)\NUnit.org\nunit-console"
        // c:nunit3-console.exe --noresult --where "method =~ /Compute*/" unittests.dll
        //

        // Ignore Spelling: Bpy, cd, Da, Gly, Leu, Pos, Tyr, UniMod

        private const double MATCHING_MASS_EPSILON = 0.0000001;
        private const double MATCHING_CHARGE_EPSILON = 0.05;

        private MolecularWeightTool mMwtWinAvg;
        private MolecularWeightTool mMwtWinIso;

        /// <summary>
        /// Instantiate two copies of the Molecular Weight Calculator
        /// One using average masses and one using isotopic masses
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            mMwtWinAvg = new MolecularWeightTool(ElementMassMode.Average);
            mMwtWinIso = new MolecularWeightTool(ElementMassMode.Isotopic);
        }

        [Test]
        [TestCase("BrCH2(CH2)7CH2Br", 286.04722000, 283.97751480)]
        [TestCase("FeCl3-6H2O", 270.29478000, 268.90488320)]
        [TestCase("Co(Bpy)(CO)4", 327.15760000, 326.98160280)]
        [TestCase("^13C6H6-.1H2O", 85.84916800, 85.84800402)]
        [TestCase("HGlyLeuTyrOH", 351.39762000, 351.17941200)]
        [TestCase("BrCH2(CH2)7CH2Br>CH8", 265.97300000, 263.91491800)]
        [TestCase("C6H6-H2O-2ZnHgMg-U", 914.72602000, 913.87795180)]
        [TestCase("57FeCl3-6H2O", 271.44978, 269.96994420)]              // In VB6 "2FeCl3-6H2O" meant "2(FeCl3)-6H2O"; in C#, 2Fe gets auto-changed to ^2Fe
        [TestCase("C6H5Cl3>H3Cl2>HCl", 145.99342000, 144.96117980)]
        [TestCase("D10>H10", 10.061618, 10.062772)]                      // In VB6, D was defined as "^2.014H"; in C#, it is "^2.0141018H"
        public void ComputeMass(string formula, double expectedAvgMass, double expectedMonoMass, double matchTolerance = MATCHING_MASS_EPSILON)
        {
            var resultDaAvg = mMwtWinAvg.ComputeMass(formula);
            var resultDaIso = mMwtWinIso.ComputeMass(formula);

            if (!mMwtWinIso.Compound.FormulaCapitalized.Equals(formula))
            {
                Console.WriteLine("Original formula:");
                Console.WriteLine(formula);
            }

            Console.WriteLine("{0,-22} -> {1,12:F8} Da (average) and  {2,12:F8} Da (isotopic)",
                mMwtWinIso.Compound.FormulaCapitalized, resultDaAvg, resultDaIso);

            Assert.AreEqual(expectedAvgMass, resultDaAvg, matchTolerance, "Actual mass does not match expected average mass");
            Assert.AreEqual(expectedMonoMass, resultDaIso, matchTolerance, "Actual mass does not match expected isotopic mass");
        }

        [Test]
        [TestCase("^13C6H6-H2O", 102.06292, 102.0575118, -6)]
        [TestCase("^13C6H6[.1H2O]", 85.849168, 85.84800402, -6)]
        [TestCase("^13C6H6[H2O]", 102.06292, 102.0575118, -6, true)]
        [TestCase("^13C6H6[H2O]2", 120.0782, 120.068076, -6, true)]
        [TestCase("13C6H6-6H2O", 192.15945, 192.13046280, -6)]          // In VB6 "13C6H6" meant "13(C6H6)"; in C# it gets auto-updated to ^13.003355C6H6
        [TestCase("13(C6H6)-6H2O", 1123.5456, 1122.673704, 222)]
        [TestCase("^13.003355C6H6-6H2O", 192.15945, 192.13046280, -6)]
        [TestCase("13C4C2H6-6H2O", 190.17414, 190.1237528, 0)]
        [TestCase("^13.003355C4C2H6-6H2O", 190.17414, 190.1237528, 0)]
        [TestCase("Et2O", 74.1216, 74.073161, 0)]
        [TestCase("DCH2", 16.04068180, 16.029751, 3)]
        //[TestCase("", 0, 0, 0)]
        [TestCase("CaOCH4(CH2)7Br", 250.20992, 249.0166848, 13)] // TODO: stdDev difference (-8.67e-19) (but it's insignificant)
        [TestCase("canyoch4(ch2)7br", 353.12251, 351.9256148, 13)] // TODO: stdDev difference (-8.67e-19) (but it's insignificant)
        [TestCase("F(DCH2)7Br", 211.1871758, 210.1249962, 19)]
        [TestCase("Pt(CH2)7Br-[5Ca123456789]", 24739506320.878063, 24668266196.894161, 1234567936)]
        [TestCase("Pt(CH2)7Br-[5Ca123456789H]", 24739506325.917763, 24668266201.933285, 1234567936)]
        [TestCase("-[2.5CaH]", 102.71485, 102.426039, 2.5)]
        [TestCase("^18CaH", 19.00794, 19.0078246, -1)]
        [TestCase("^23CaH", 24.00794, 24.0078246, -1)]
        [TestCase("^42CaH", 43.00794, 43.0078246, -1)]
        [TestCase("^98CaH", 99.00794, 99.0078246, -1)]
        [TestCase("(CH2)7Br-3.2CaH", 309.565068, 308.13321031, 16.2)]
        [TestCase("[2CaOH]-K", 153.26898, 152.8943692, 3)]
        [TestCase("K-[2CaOH]", 153.26898, 152.8943692, 3)]
        [TestCase("H2Ca2KO2", 153.26898, 152.8943692, 3)]
        [TestCase("H2O^23.9Ca", 41.91528, 41.9105642, 0)]
        [TestCase("^13C6H6", 84.04764, 84.0469476, -6)]
        [TestCase("C6H6", 78.11184, 78.0469476, 8)]
        [TestCase("C6H3^19.8Ar", 94.88802, 94.8234738, 11)]
        [TestCase("C6H3^19.8arpbbb", 323.71002, 324.8187248, 19)]
        [TestCase("C6H3^19.88Ar4Pb>Ar", 321.86002, 322.5577318, 13)]
        [TestCase("C6>C4", 24.0214, 24, 4)]
        //[TestCase("", 0, 0, 0)]
        [TestCase("HGly5.3Leu2.2Tyr0.03OH", 574.229583, 573.901147922, 0)]
        [TestCase("HGly5.3leu2.2tyr0.03oh", 574.229583, 573.901147922, 0)]
        [TestCase("HHeLiBeBCNOFNeNaMgAlSiPSClArKCaScTiVCrMnFeCoNiCuZnGaGeAsSeBrKrRbSrYZrNbMoTcRuRhPdAgCdInSnSbTeIXe", 3357.7013952, 3357.0639367, 80)]
        [TestCase("hhelibebcnofnenamgalsipsClArKcasctivcrmnfeconicuzngageassebrkrrbsryzrnbmotcrurhpdagcdinsnsbteixe", 3586.7961552, 3596.0659477, 88)]
        [TestCase("CsBaLaCePrNdPmSmEuGdTbDyHoErTmYbLuHfTaWReOsIrPtAuHgTlPbBiPoAtRnFrRaAcThPaUNpPuAmCmBkCfEsFmMdNoLr", 9710.62118, 9728.379096, 154)]
        [TestCase("csbalaceprndpmsmeugdtbdyhoertmybluhftawreosirptauhgtlpbbipoatrnfrraacthpaunppuamcmbkcfesfmmdnolr", 9710.62118, 9728.379096, 154)]
        [TestCase("cdinsnsbteixecsbalaceprndpm", 1832.80777, 1835.765967, 22)]
        [TestCase("CdInSnSbTeIXeCsBaLaCePrNdPm", 1832.80777, 1835.765967, 22)]
        [TestCase("sips cl arkcas", 277.768261, 276.75237, -1)] // Ignoring whitespace and characters outside of a-z, A-Z, 0-9, []{}().^>
        public void ComputeMassStressTest(string formula, double expectedAvgMass, double expectedMonoMass, double expectedCharge, bool bracketsAsParentheses = false)
        {
            Console.WriteLine("Formula: " + formula);
            Console.WriteLine();

            Console.WriteLine("Average Mass:");
            mMwtWinAvg.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var resultDaAvg = mMwtWinAvg.ComputeMassExtra(formula, out var parseDataAvg);
            ReportParseData(parseDataAvg);
            Assert.Greater(resultDaAvg, 0);

            var compareValues = expectedAvgMass > 0 || expectedMonoMass > 0 || expectedCharge != 0;
            if (compareValues)
            {
                Assert.AreEqual(expectedAvgMass, resultDaAvg, MATCHING_MASS_EPSILON, "Actual mass does not match expected average mass");
                Assert.AreEqual(expectedCharge, parseDataAvg.Charge, MATCHING_CHARGE_EPSILON, "Actual charge does not match expected charge");
            }

            Console.WriteLine("");
            Console.WriteLine("Isotopic Mass:");
            mMwtWinIso.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var resultDaIso = mMwtWinIso.ComputeMassExtra(formula, out var parseDataIso);
            ReportParseData(parseDataIso);

            Assert.Greater(resultDaIso, 0);
            if (compareValues)
            {
                Assert.AreEqual(expectedMonoMass, resultDaIso, MATCHING_MASS_EPSILON, "Actual mass does not match expected isotopic mass");
                Assert.AreEqual(expectedCharge, parseDataIso.Charge, MATCHING_CHARGE_EPSILON, "Actual charge does not match expected charge");
            }

            Console.WriteLine();
            ReportParseData(mMwtWinIso);
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
            mMwtWinAvg.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var resultDaAvg = mMwtWinAvg.ComputeMassExtra(formula, out var parseData);
            ReportParseData(parseData);
            Assert.AreEqual(errorIdExpected, parseData.ErrorData.ErrorId);
            Assert.AreEqual(expectedPosition, parseData.ErrorData.ErrorPosition);
            Assert.GreaterOrEqual(parseData.ErrorData.ErrorDescription.IndexOf(messageExcerpt, StringComparison.OrdinalIgnoreCase), 0, "excerpt not found in reported message statement");
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
            var resultDaAvg = mMwtWinAvg.ComputeMassExtra(formula, out var parseData);
            ReportParseData(parseData);

            Assert.GreaterOrEqual(parseData.CautionDescription.IndexOf(cautionExcerpt, StringComparison.OrdinalIgnoreCase), 0, "excerpt not found in reported caution statement");
        }

        [Test]
        [TestCase("BrCH2(CH2)7CH2Br", "C9H18Br2")]
        [TestCase("FeCl3-6H2O", "H12Cl3FeO6")]
        [TestCase("Co(Bpy)(CO)4", "C14H8CoN2O4")]
        [TestCase("^13C6H6-.1H2O", "^13C6H6.2O0.1")]
        [TestCase("HGlyLeuTyrOH", "C17H25N3O5")]
        [TestCase("BrCH2(CH2)7CH2Br>CH8", "C8H10Br2")]
        [TestCase("C6H5Cl3>H3Cl2>HCl", "C6H3Cl2")]
        [TestCase("D10C6>H10", "C6", false)]        // This is not working properly; gives C6^2.0141018H10 instead of C6; even better would be C6^1.00617H10
        [TestCase("D", "^2.0141018H")]
        [TestCase("D2CH", "C^2.0141018H2H")]
        [TestCase("D2CH^3H3", "C^2.0141018H2^3H3H")]
        [TestCase("D2C^13C5H^3H3", "^13C5C^2.0141018H2^3H3H")]
        public void ConvertToEmpiricalTests(string formula, string expectedEmpirical, bool compareToExpected = true)
        {
            mMwtWinAvg.Compound.Formula = formula;
            var empirical = mMwtWinAvg.Compound.ConvertToEmpirical();

            if (compareToExpected)
                Assert.AreEqual(expectedEmpirical, empirical, "Unexpected result for {0}", formula);
        }

        [Test]
        public void CircularReferenceHandlingTests()
        {
            var mwt = new MolecularWeightTool(ElementMassMode.Average);
            var mass = mwt.ComputeMassExtra("TryCo", out var parseData);
            //ReportParseData(parseData);
            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            var error = mwt.SetAbbreviation("Try", "FailH2O2", 1, false);
            // Error expected (unknown element):
            Assert.AreEqual(1, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(parseData);
            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Fail", "TryCaOH", 1, false);
            // Error expected (circular reference), A->B->A:
            Assert.AreEqual(28, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(parseData);
            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Fail", "TrierCaOH", 1, false);
            // Error expected (invalid abbreviation, due to bad dependency):
            Assert.AreEqual(32, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(parseData);
            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Trier", "TryOH", 1, false);
            // Error expected (circular reference), A->B->C->A:
            Assert.AreEqual(28, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(parseData);
            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Trier", "Trier", 1, false);
            // Error expected (circular reference), A->A:
            Assert.AreEqual(28, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            //ReportParseData(parseData);
            // Error expected (unknown element):
            Assert.AreEqual(1, parseData.ErrorData.ErrorId);

            error = mwt.SetAbbreviation("Trier", "C6H12O6", 1, false);
            // No error expected:
            Assert.AreEqual(0, error);

            mass = mwt.ComputeMassExtra("TryCo", out parseData);
            ReportParseData(parseData);
            // No error expected:
            Assert.AreEqual(0, parseData.ErrorData.ErrorId);
            Assert.AreEqual(330.1891, mass, MATCHING_MASS_EPSILON);

            mass = mwt.ComputeMassExtra("TryCoFail", out parseData);
            ReportParseData(parseData);
            // No error expected:
            Assert.AreEqual(0, parseData.ErrorData.ErrorId);
            Assert.AreEqual(567.43032, mass, MATCHING_MASS_EPSILON);
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
            var resultDaIso = mMwtWinIso.ComputeMass(formula);
            var expandedFormula = mMwtWinIso.Compound.ExpandAbbreviations();

            Console.WriteLine("{0,-15} -> {1,-15}: {2,12:F8} Da (isotopic)",
                formula, expandedFormula, resultDaIso);

            Assert.AreEqual(expectedExpandedFormula, expandedFormula, "New formula does not match expected");
        }

        private void ReportParseData(IFormulaParseData data)
        {
            if (!string.IsNullOrWhiteSpace(data.CautionDescription))
            {
                Console.WriteLine("Cautions: {0}", data.CautionDescription);
                Console.WriteLine();
            }

            if (data.ErrorData.ErrorId == 0)
            {
                Console.WriteLine(data.Formula);
                var stats = data.Stats;

                Console.WriteLine("StDev:  {0}", stats.StandardDeviation);
                Console.WriteLine("Mass:   {0}", stats.TotalMass);
                Console.WriteLine("Charge: {0}", stats.Charge);
            }
            else
            {
                Console.WriteLine(data.FormulaOriginal);
                Console.WriteLine("ErrorId:          {0}", data.ErrorData.ErrorId);
                Console.WriteLine("ErrorPos:         {0}", data.ErrorData.ErrorPosition);
                Console.WriteLine("ErrorChar:        {0}", data.ErrorData.ErrorCharacter);
                Console.WriteLine("ErrorDescription: {0}", data.ErrorData.ErrorDescription);
                Console.WriteLine();
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
                Console.WriteLine("Highlight: {0}", markedFormula);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void ReportParseData(MolecularWeightTool mwt)
        {
            // Use this in comparison to the other ReportParseData method... (results should be the same)
            var compound = mwt.Compound;
            if (!string.IsNullOrWhiteSpace(compound.CautionDescription))
            {
                Console.WriteLine("Cautions: {0}", compound.CautionDescription);
                Console.WriteLine();
            }

            if (mwt.ErrorId == 0)
            {
                Console.WriteLine(compound.FormulaCapitalized);

                Console.WriteLine("StDev:  {0}", compound.StandardDeviation);
                Console.WriteLine("Mass:   {0}", compound.GetMass(false));
                Console.WriteLine("Charge: {0}", compound.Charge);
            }
            else
            {
                Console.WriteLine(compound.Formula);
                Console.WriteLine("ErrorId:          {0}", mwt.ErrorId);
                Console.WriteLine("ErrorPos:         {0}", mwt.ErrorPosition);
                Console.WriteLine("ErrorChar:        {0}", mwt.ErrorCharacter);
                Console.WriteLine("ErrorDescription: {0}", mwt.ErrorDescription);
                Console.WriteLine();
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
                Console.WriteLine("Highlight: {0}", markedFormula);
            }
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
        [TestCase("N(-1) 15N", 0.9934, 0.997035, "^15.000109N>N1", 0.993369, 0.997035)]
        [TestCase("H(-6) C(-2) O(2)", 1.9298, 1.942879, "O2>H6C2", 0, 0)]
        [TestCase("H(-2) C(-1) O(-1) S", 2.039, 1.961506, "S>H2C1O1", 0, 0)]
        [TestCase("H(-2) C(-1) O", 1.9728, 1.979265, "O>H2C1", 0, 0)]
        [TestCase("N(-2) 15N(2)", 1.9868, 1.99407, "^15.000109N2>N2", 1.986738, 1.99407)]
        [TestCase("H(2) O(-2) S", 2.0821, 1.997892, "H2S>O2", 0, 0)]
        [TestCase("O(-1) 18O", 1.9998, 2.004246, "^17.999161O>O1", 1.999761, 2.004246)]
        [TestCase("H(2)", 2.0159, 2.01565, "H2", 2.01588, 2.0156492)]
        [TestCase("H(-3) C(-1) N(-1) S", 3.0238, 2.945522, "S>H3C1N1", 0, 0)]
        [TestCase("H N(-1) O(-1) S", 3.0668, 2.981907, "HS>N1O1", 0, 0)]
        [TestCase("H(-1) N(-1) 18O", 2.9845, 2.988261, "^17.999161O>H1N1", 0, 0)]
        [TestCase("N(-3) 15N(3)", 2.9802, 2.991105, "^15.000109N3>N3", 2.980107, 2.991105)]
        [TestCase("C(-3) 13C(3)", 2.978, 3.010064, "^13.003355C3>C3", 2.977965, 3.010065)]
        [TestCase("H(-3) 2H(3)", 3.0185, 3.01883, "D3>H3", 3.0184854, 3.0188316)]
        [TestCase("H(-4) C(-2) S", 4.0118, 3.940771, "S>H4C2", 0, 0)]
        [TestCase("N(-4) 15N(4)", 3.9736, 3.98814, "^15.000109N4>N4", 3.973476, 3.98814)]
        [TestCase("C(-1) O", 3.9887, 3.994915, "O>C1", 0, 0)]
        [TestCase("C(-2) 13C(2) N(-2) 15N(2)", 3.9721, 4.00078, "^13.003355C2^15.000109N2>C2N2", 3.972048, 4.00078)]
        [TestCase("C(-3) 13C(3) N(-1) 15N", 3.9714, 4.007099, "^13.003355C3^15.000109N>C3N1", 3.971334, 4.0071)]
        [TestCase("O(-2) 18O(2)", 3.9995, 4.008491, "^17.999161O2>O2", 3.999522, 4.008492)]
        [TestCase("C(-4) 13C(4)", 3.9706, 4.013419, "^13.003355C4>C4", 3.97062, 4.01342)]
        [TestCase("H(-3) 2H(3) C(-1) 13C", 4.0111, 4.022185, "D3^13.003355C>H3C1", 4.0111404, 4.0221866)]
        [TestCase("H(-4) 2H(4)", 4.0246, 4.025107, "D4>H4", 4.0246472, 4.0251088)]
        [TestCase("H(-1) C(-1) N(-1) O(2)", 4.9735, 4.97893, "O2>H1C1N1", 0, 0)]
        [TestCase("C(-4) 13C(4) N(-1) 15N", 4.964, 5.010454, "^13.003355C4^15.000109N>C4N1", 4.963989, 5.010455)]
        [TestCase("C(-5) 13C(5)", 4.9633, 5.016774, "^13.003355C5>C5", 4.96327500000001, 5.01677500000001)]
        [TestCase("H(-4) 2H(4) C(-1) 13C", 5.0173, 5.028462, "D4^13.003355C>H4C1", 5.0173022, 5.0284638)]
        [TestCase("H(-2) C(-2) S", 6.0277, 5.956421, "S>H2C2", 0, 0)]
        [TestCase("H(-1) Li", 5.9331, 6.008178, "Li>H1", 0, 0)]
        [TestCase("C(-5) 13C(5) N(-1) 15N", 5.9567, 6.013809, "^13.003355C5^15.000109N>C5N1", 5.95664400000001, 6.01381000000001)]
        [TestCase("H(-2) C N(2) S(-1)", 5.9432, 6.018427, "CN2>H2S1", 0, 0)]
        [TestCase("C(-6) 13C(6)", 5.9559, 6.020129, "^13.003355C6>C6", 5.95593000000001, 6.02013000000001)]
        [TestCase("H(-3) C(3) N(-3) O", 6.9876, 6.962218, "C3O>H3N3", 0, 0)]
        [TestCase("C(-6) 13C(6) N(-1) 15N", 6.9493, 7.017164, "^13.003355C6^15.000109N>C6N1", 6.94929900000001, 7.01716500000001)]
        [TestCase("H(-6) 2H(6) N(-1) 15N", 7.0304, 7.034695, "D6^15.000109N>H6N1", 7.0303398, 7.0346982)]
        [TestCase("C(-6) 13C(6) N(-2) 15N(2)", 7.9427, 8.014199, "^13.003355C6^15.000109N2>C6N2", 7.94266800000001, 8.01420000000001)]
        [TestCase("C N(2) O(-2)", 8.0253, 8.016319, "CN2>O2", 0, 0)]
        [TestCase("H(-5) N", 8.967, 8.963949, "N>H5", 0, 0)]
        [TestCase("H(-1) C N O(-1)", 9.0101, 9.000334, "CN>H1O1", 0, 0)]
        [TestCase("C(-9) 13C(9)", 8.9339, 9.030193, "^13.003355C9>C9", 8.93389500000001, 9.03019500000001)]
        [TestCase("H(3) C(-3) N(3)", 9.0118, 9.032697, "H3N3>C3", 0, 0)]
        [TestCase("C(-6) 13C(6) N(-4) 15N(4)", 9.9296, 10.008269, "^13.003355C6^15.000109N4>C6N4", 9.92940600000001, 10.00827)]
        [TestCase("H(2) C(3) N(-2)", 10.0346, 10.009502, "H2C3>N2", 0, 0)]
        [TestCase("H(2) C(2) O(-1)", 10.0379, 10.020735, "H2C2>O1", 0, 0)]
        [TestCase("C(-9) 13C(9) N(-1) 15N", 9.9273, 10.027228, "^13.003355C9^15.000109N>C9N1", 9.92726400000001, 10.02723)]
        [TestCase("H(-10) 2H(10)", 10.0616, 10.062767, "D10>H10", 10.061618, 10.062772)]
        [TestCase("H(6) C(3) S(-1)", 10.0147, 10.07488, "H6C3>S1", 0, 0)]
        [TestCase("H(-7) 2H(7) N(-4) 15N(4)", 11.0168, 11.032077, "D7^15.000109N4>H7N4", 11.0166086, 11.0320804)]
        [TestCase("H C N O S(-1)", 10.9597, 11.033743, "HCNO>S1", 0, 0)]
        [TestCase("H(-9) 2H(9) N(-2) 15N(2)", 11.0423, 11.050561, "D9^15.000109N2>H9N2", 11.0421942, 11.0505648)]
        [TestCase("H(5) C(2) N S(-1)", 11.0028, 11.070128, "H5C2N>S1", 0, 0)]
        [TestCase("C", 12.0107, 12, "C", 12.0107, 12)]
        [TestCase("C O(2) S(-1)", 11.9445, 12.017759, "CO2>S1", 0, 0)]
        [TestCase("H(4) C(2) O(-1)", 12.0538, 12.036386, "H4C2>O1", 0, 0)]
        [TestCase("H(-1) C(-4) N O S", 13.0204, 12.962234, "NOS>H1C4", 0, 0)]
        [TestCase("H(-1) N", 12.9988, 12.995249, "N>H1", 0, 0)]
        [TestCase("H(3) C N O(-1)", 13.0418, 13.031634, "H3CN>O1", 0, 0)]
        [TestCase("H(7) C(2) N O(-2)", 13.0849, 13.06802, "H7C2N>O2", 0, 0)]
        [TestCase("H(-2) O", 13.9835, 13.979265, "O>H2", 0, 0)]
        [TestCase("H(2) C", 14.0266, 14.01565, "H2C", 14.02658, 14.0156492)]
        [TestCase("H(6) C(2) O(-1)", 14.0696, 14.052036, "H6C2>O1", 0, 0)]
        [TestCase("H(-3) N(-1) O(2)", 14.9683, 14.96328, "O2>H3N1", 0, 0)]
        [TestCase("H(-3) C(-1) N O", 14.9716, 14.974514, "NO>H3C1", 0, 0)]
        [TestCase("H C N(-1) O", 15.0113, 14.999666, "HCO>N1", 0, 0)]
        [TestCase("H N", 15.0146, 15.010899, "HN", 15.01468, 15.0108986)]
        [TestCase("H(-4) C(-1) O(2)", 15.9563, 15.958529, "O2>H4C1", 0, 0)]
        [TestCase("O(-1) S", 16.0656, 15.977156, "S>O1", 0, 0)]
        [TestCase("O", 15.9994, 15.994915, "O", 15.9994, 15.994915)]
        [TestCase("H(4) C O(-2) S", 16.1087, 16.013542, "H4CS>O2", 0, 0)]
        [TestCase("C(4) S(-1)", 15.9778, 16.027929, "C4>S1", 0, 0)]
        [TestCase("2H(2) C", 16.0389, 16.028204, "D2C", 16.0389036, 16.0282036)]
        [TestCase("H(4) C", 16.0425, 16.0313, "H4C", 16.04246, 16.0312984)]
        [TestCase("H(-1) C(-1) N O", 16.9875, 16.990164, "NO>H1C1", 0, 0)]
        [TestCase("H(3) C N(-1) O(-1) S", 17.0934, 16.997557, "H3CS>N1O1", 0, 0)]
        [TestCase("H(3) N", 17.0305, 17.026549, "H3N", 17.03056, 17.0265478)]
        [TestCase("H(-1) 2H(3) C", 17.0451, 17.03448, "D3C>H1", 17.0450654, 17.0344808)]
        [TestCase("H(-9) 2H(9) C(-6) 13C(6) N(-2) 15N(2)", 16.9982, 17.07069, "D9^13.003355C6^15.000109N2>H9C6N2", 16.9981242, 17.0706948)]
        [TestCase("H(-2) C(-1) S", 18.0384, 17.956421, "S>H2C1", 0, 0)]
        [TestCase("H(-2) C(-1) O(2)", 17.9722, 17.974179, "O2>H2C1", 0, 0)]
        [TestCase("H(-1) F", 17.9905, 17.990578, "F>H1", 0, 0)]
        [TestCase("H(2) O", 18.0153, 18.010565, "H2O", 18.01528, 18.0105642)]
        [TestCase("H(2) C(4) O(-2)", 18.0599, 18.025821, "H2C4>O2", 0, 0)]
        [TestCase("H(-1) 2H(3) 13C", 18.0377, 18.037835, "D3^13.003355C>H1", 18.0377204, 18.0378358)]
        [TestCase("H(-3) C(-1) N(-1) O S", 19.0232, 18.940436, "OS>H3C1N1", 0, 0)]
        [TestCase("H(-3) C(3) N(-1)", 19.0016, 18.973451, "C3>H3N1", 0, 0)]
        [TestCase("H C(4) N(-1) O(-1)", 19.0446, 19.009836, "HC4>N1O1", 0, 0)]
        [TestCase("H(-3) 2H(3) O", 19.0179, 19.013745, "D3O>H3", 19.0178854, 19.0137466)]
        [TestCase("H(5) N", 19.0464, 19.042199, "H5N", 19.04644, 19.042197)]
        [TestCase("C(-1) O(2)", 19.9881, 19.989829, "O2>C1", 0, 0)]
        [TestCase("C(-4) 13C(4) O", 19.97, 20.008334, "^13.003355C4O>C4", 19.97002, 20.008335)]
        [TestCase("H(-3) 2H(3) C(-1) 13C O", 20.0105, 20.0171, "D3^13.003355CO>H3C1", 20.0105404, 20.0171016)]
        [TestCase("H(-2) Mg", 22.2891, 21.969392, "Mg>H2", 0, 0)]
        [TestCase("H(-1) Na", 21.9818, 21.981943, "Na>H1", 0, 0)]
        [TestCase("H(-2) C(2)", 22.0055, 21.98435, "C2>H2", 0, 0)]
        [TestCase("H(2) C(2) N(2) O(-2)", 22.0519, 22.031969, "H2C2N2>O2", 0, 0)]
        [TestCase("H C(2) N O(-1)", 23.0366, 23.015984, "HC2N>O1", 0, 0)]
        [TestCase("H(-3) Al", 23.9577, 23.958063, "Al>H3", 0, 0)]
        [TestCase("H(-4) N(2)", 23.9816, 23.974848, "N2>H4", 0, 0)]
        [TestCase("H(2) C(-5) 13C(6) N(-4) 15N(4)", 23.9561, 24.023919, "H2^13.003355C6^15.000109N4>C5N4", 23.955986, 24.0239192)]
        [TestCase("H(-1) C N", 25.0095, 24.995249, "CN>H1", 0, 0)]
        [TestCase("H(3) C(2) N O S(-1)", 24.9863, 25.049393, "H3C2NO>S1", 0, 0)]
        [TestCase("H(3) C N(3) S(-1)", 24.9896, 25.060626, "H3CN3>S1", 0, 0)]
        [TestCase("H(7) C(3) N S(-1)", 25.0294, 25.085779, "H7C3N>S1", 0, 0)]
        [TestCase("H(2) C(3) N(-2) O", 26.034, 26.004417, "H2C3O>N2", 0, 0)]
        [TestCase("H(2) C(2)", 26.0373, 26.01565, "H2C2", 26.03728, 26.0156492)]
        [TestCase("H(2) C(2) O(2) S(-1)", 25.9711, 26.033409, "H2C2O2>S1", 0, 0)]
        [TestCase("H(6) C(3) O(-1)", 26.0803, 26.052036, "H6C3>O1", 0, 0)]
        [TestCase("H C N", 27.0253, 27.010899, "HCN", 27.02538, 27.0108986)]
        [TestCase("H(5) C(2) N O(-1)", 27.0684, 27.047285, "H5C2N>O1", 0, 0)]
        [TestCase("H(5) C N(3) O(-2)", 27.0717, 27.058518, "H5CN3>O2", 0, 0)]
        [TestCase("H(-4) O(2)", 27.967, 27.958529, "O2>H4", 0, 0)]
        [TestCase("C O", 28.0101, 27.994915, "CO", 28.0101, 27.994915)]
        [TestCase("N(2)", 28.0134, 28.006148, "N2", 28.01348, 28.006148)]
        [TestCase("H(4) C(2)", 28.0532, 28.0313, "H4C2", 28.05316, 28.0312984)]
        [TestCase("H(4) C N(2) O(-1)", 28.0565, 28.042534, "H4CN2>O1", 0, 0)]
        [TestCase("H(-1) 2H(3) C(-6) 13C(7) N(-4) 15N(4)", 27.9673, 28.046104, "D3^13.003355C7^15.000109N4>H1C6N4", 27.9671264, 28.0461058)]
        [TestCase("H(-1) N O", 28.9982, 28.990164, "NO>H1", 0, 0)]
        [TestCase("H(3) C(2) N(-1) O", 29.0379, 29.015316, "H3C2O>N1", 0, 0)]
        [TestCase("H(3) C N", 29.0412, 29.026549, "H3CN", 29.04126, 29.0265478)]
        [TestCase("H(5) C(2)", 29.0611, 29.039125, "H5C2", 29.0611, 29.039123)]
        [TestCase("H(-2) O(2)", 29.9829, 29.974179, "O2>H2", 0, 0)]
        [TestCase("H(-2) C(5) N(-2)", 30.0242, 29.978202, "C5>H2N2", 0, 0)]
        [TestCase("H(2) C O(-1) S", 30.0922, 29.992806, "H2CS>O1", 0, 0)]
        [TestCase("H(2) C O", 30.026, 30.010565, "H2CO", 30.02598, 30.0105642)]
        [TestCase("H N O", 31.014, 31.005814, "HNO", 31.01408, 31.0058136)]
        [TestCase("H(5) C N", 31.0571, 31.042199, "H5CN", 31.05714, 31.042197)]
        [TestCase("H(-4) C(-1) O S", 32.0219, 31.935685, "OS>H4C1", 0, 0)]
        [TestCase("S", 32.065, 31.972071, "S", 32.066, 31.972072)]
        [TestCase("O(2)", 31.9988, 31.989829, "O2", 31.9988, 31.98983)]
        [TestCase("C(4) O S(-1)", 31.9772, 32.022844, "C4O>S1", 0, 0)]
        [TestCase("H(4) C(5) O(-2)", 32.0865, 32.041471, "H4C5>O2", 0, 0)]
        [TestCase("2H(4) C(2)", 32.0778, 32.056407, "D4C2", 32.0778072, 32.0564072)]
        [TestCase("H(3) C(5) N(-1) O(-1)", 33.0712, 33.025486, "H3C5>N1O1", 0, 0)]
        [TestCase("H(-2) C(-1) O S", 34.0378, 33.951335, "OS>H2C1", 0, 0)]
        [TestCase("H(-1) Cl", 34.4451, 33.961028, "Cl>H1", 0, 0)]
        [TestCase("H(-2) C(-1) O(3)", 33.9716, 33.969094, "O3>H2C1", 0, 0)]
        [TestCase("H(-2) C(3)", 34.0162, 33.98435, "C3>H2", 0, 0)]
        [TestCase("H(2) S", 34.0809, 33.987721, "H2S", 34.08188, 33.9877212)]
        [TestCase("H(2) C(4) O(-1)", 34.0593, 34.020735, "H2C4>O1", 0, 0)]
        [TestCase("H(2) C(3) N(2) S(-1)", 33.9964, 34.049727, "H2C3N2>S1", 0, 0)]
        [TestCase("H(4) C(-4) 13C(6)", 34.0091, 34.051429, "H4^13.003355C6>C4", 34.00909, 34.0514284)]
        [TestCase("2H(4) 13C(2)", 34.0631, 34.063117, "D4^13.003355C2", 34.0631172, 34.0631172)]
        [TestCase("H(-2) 2H(6) C(2)", 34.0901, 34.068961, "D6C2>H2", 34.0901308, 34.0689616)]
        [TestCase("H(-3) C(3) N(-1) O", 35.001, 34.968366, "C3O>H3N1", 0, 0)]
        [TestCase("H C(4) N(-1)", 35.044, 35.004751, "HC4>N1", 0, 0)]
        [TestCase("C(2) N(2) O(-1)", 36.0354, 36.011233, "C2N2>O1", 0, 0)]
        [TestCase("H(4) C(-4) 13C(6) N(-2) 15N(2)", 35.9959, 36.045499, "H4^13.003355C6^15.000109N2>C4N2", 35.995828, 36.0454984)]
        [TestCase("H(-2) 2H(6) 13C(2)", 36.0754, 36.07567, "D6^13.003355C2>H2", 36.0754408, 36.0756716)]
        [TestCase("H(3) C(3) N O(-1)", 37.0632, 37.031634, "H3C3N>O1", 0, 0)]
        [TestCase("H(-2) Ca", 38.0621, 37.946941, "Ca>H2", 0, 0)]
        [TestCase("H(-1) K", 38.0904, 37.955882, "K>H1", 0, 0)]
        [TestCase("H(-2) C N(2)", 38.0082, 37.990498, "CN2>H2", 0, 0)]
        [TestCase("H(2) C(3)", 38.048, 38.01565, "H2C3", 38.04798, 38.0156492)]
        [TestCase("H(4) C(-4) 13C(6) N(-4) 15N(4)", 37.9827, 38.039569, "H4^13.003355C6^15.000109N4>C4N4", 37.982566, 38.0395684)]
        [TestCase("H C(2) N", 39.036, 39.010899, "HC2N", 39.03608, 39.0108986)]
        [TestCase("C(2) O", 40.0208, 39.994915, "C2O", 40.0208, 39.994915)]
        [TestCase("C N(2)", 40.0241, 40.006148, "CN2", 40.02418, 40.006148)]
        [TestCase("H(4) C(3)", 40.0639, 40.0313, "H4C3", 40.06386, 40.0312984)]
        [TestCase("H(-1) N(3)", 41.0122, 41.001397, "N3>H1", 0, 0)]
        [TestCase("H(3) C(2) N", 41.0519, 41.026549, "H3C2N", 41.05196, 41.0265478)]
        [TestCase("H(7) C(3) N O(-1)", 41.095, 41.062935, "H7C3N>O1", 0, 0)]
        [TestCase("H(7) C(2) N(3) O(-2)", 41.0983, 41.074168, "H7C2N3>O2", 0, 0)]
        [TestCase("H(2) C(2) O", 42.0367, 42.010565, "H2C2O", 42.03668, 42.0105642)]
        [TestCase("H(2) C N(2)", 42.04, 42.021798, "H2CN2", 42.04006, 42.0217972)]
        [TestCase("H(6) C(3)", 42.0797, 42.04695, "H6C3", 42.07974, 42.0469476)]
        [TestCase("H(6) C(2) N(2) O(-1)", 42.083, 42.058184, "H6C2N2>O1", 0, 0)]
        [TestCase("H C N O", 43.0247, 43.005814, "HCNO", 43.02478, 43.0058136)]
        [TestCase("H N(3)", 43.028, 43.017047, "HN3", 43.02816, 43.0170466)]
        [TestCase("H(5) C(2) N", 43.0678, 43.042199, "H5C2N", 43.06784, 43.042197)]
        [TestCase("H(-4) O(3)", 43.9664, 43.953444, "O3>H4", 0, 0)]
        [TestCase("C O(2)", 44.0095, 43.989829, "CO2", 44.0095, 43.98983)]
        [TestCase("H(4) C(2) O(-1) S", 44.1188, 44.008456, "H4C2S>O1", 0, 0)]
        [TestCase("H(2) 13C(2) O", 44.022, 44.017274, "H2^13.003355C2O", 44.02199, 44.0172742)]
        [TestCase("H(4) C(2) O", 44.0526, 44.026215, "H4C2O", 44.05256, 44.0262134)]
        [TestCase("H(4) C(6) S(-1)", 44.031, 44.059229, "H4C6>S1", 0, 0)]
        [TestCase("H(-1) N O(2)", 44.9976, 44.985078, "NO2>H1", 0, 0)]
        [TestCase("H(-1) 2H(3) C(2) O", 45.0552, 45.029395, "D3C2O>H1", 45.0551654, 45.0293958)]
        [TestCase("H(2) C S", 46.0916, 45.987721, "H2CS", 46.09258, 45.9877212)]
        [TestCase("H(2) C(5) O(-1)", 46.07, 46.020735, "H2C5>O1", 0, 0)]
        [TestCase("H(-2) 2H(4) C(2) O", 46.0613, 46.035672, "D4C2O>H2", 46.0613272, 46.035673)]
        [TestCase("H(-2) 2H(6) C(-6) 13C(8) N(-4) 15N(4)", 46.005, 46.083939, "D6^13.003355C8^15.000109N4>H2C6N4", 46.0048468, 46.0839416)]
        [TestCase("S(-1) Se", 46.895, 47.944449, "Se>S1", 0, 0)]
        [TestCase("O(3)", 47.9982, 47.984744, "O3", 47.9982, 47.984745)]
        [TestCase("C(4)", 48.0428, 48, "C4", 48.0428, 48)]
        [TestCase("H(2) C(-4) 13C(6) O", 47.9926, 48.030694, "H2^13.003355C6O>C4", 47.99261, 48.0306942)]
        [TestCase("H(4) C(5) O(-1)", 48.0859, 48.036386, "H4C5>O1", 0, 0)]
        [TestCase("2H(6) C(3)", 48.1167, 48.084611, "D6C3", 48.1167108, 48.0846108)]
        [TestCase("H(3) C(5) N(-1)", 49.0706, 49.020401, "H3C5>N1", 0, 0)]
        [TestCase("H(-2) C(3) O", 50.0156, 49.979265, "C3O>H2", 0, 0)]
        [TestCase("H(2) C(4)", 50.0587, 50.01565, "H2C4", 50.05868, 50.0156492)]
        [TestCase("H(2) C(-4) 13C(6) N(-2) 15N(2) O", 49.9794, 50.024764, "H2^13.003355C6^15.000109N2O>C4N2", 49.979348, 50.0247642)]
        [TestCase("H(2) C(3) N(2) O(-1)", 50.062, 50.026883, "H2C3N2>O1", 0, 0)]
        [TestCase("H(-3) 2H(9) C(3)", 51.1352, 51.103441, "D9C3>H3", 51.1351962, 51.1034424)]
        [TestCase("H(-3) Fe", 52.8212, 52.911464, "Fe>H3", 0, 0)]
        [TestCase("H(7) C(3) N(3) S(-1)", 53.0428, 53.091927, "H7C3N3>S1", 0, 0)]
        [TestCase("H(-2) Fe", 53.8291, 53.919289, "Fe>H2", 0, 0)]
        [TestCase("H(-3) F(3)", 53.9714, 53.971735, "F3>H3", 0, 0)]
        [TestCase("H(2) C(3) O", 54.0474, 54.010565, "H2C3O", 54.04738, 54.0105642)]
        [TestCase("H(-3) 2H(9) 13C(3)", 54.1132, 54.113505, "D9^13.003355C3>H3", 54.1131612, 54.1135074)]
        [TestCase("H C(6) N S(-1)", 55.0138, 55.038828, "HC6N>S1", 0, 0)]
        [TestCase("H(5) C(2) N(3) O(-1)", 55.0818, 55.053433, "H5C2N3>O1", 0, 0)]
        [TestCase("H(-2) Ni", 56.6775, 55.919696, "Ni>H2", 0, 0)]
        [TestCase("C(2) O(2)", 56.0202, 55.989829, "C2O2", 56.0202, 55.98983)]
        [TestCase("H(4) C(3) O", 56.0633, 56.026215, "H4C3O", 56.06326, 56.0262134)]
        [TestCase("H(8) C(4)", 56.1063, 56.0626, "H8C4", 56.10632, 56.0625968)]
        [TestCase("H(3) C(2) N O", 57.0513, 57.021464, "H3C2NO", 57.05136, 57.0214628)]
        [TestCase("H(3) C N(3)", 57.0546, 57.032697, "H3CN3", 57.05474, 57.0326958)]
        [TestCase("H(3) C(6) N O(-2)", 57.0959, 57.03672, "H3C6N>O2", 0, 0)]
        [TestCase("H(7) C(3) N", 57.0944, 57.057849, "H7C3N", 57.09442, 57.0578462)]
        [TestCase("H(-2) C(5)", 58.0376, 57.98435, "C5>H2", 0, 0)]
        [TestCase("H(2) C(2) O(2)", 58.0361, 58.005479, "H2C2O2", 58.03608, 58.0054792)]
        [TestCase("H(2) C(6) O(-1)", 58.0807, 58.020735, "H2C6>O1", 0, 0)]
        [TestCase("H(4) C(2) N O", 58.0593, 58.029289, "H4C2NO", 58.0593, 58.0292874)]
        [TestCase("H(6) C(3) O", 58.0791, 58.041865, "H6C3O", 58.07914, 58.0418626)]
        [TestCase("H(5) C(2) N O(-1) S", 59.1334, 59.019355, "H5C2NS>O1", 0, 0)]
        [TestCase("H(4) 13C(3) O", 59.0412, 59.036279, "H4^13.003355C3O", 59.041225, 59.0362784)]
        [TestCase("H 2H(3) C(3) O", 59.0817, 59.045045, "HD3C3O", 59.0817454, 59.045045)]
        [TestCase("H(5) C N(3)", 59.0705, 59.048347, "H5CN3", 59.07062, 59.048345)]
        [TestCase("H(7) C(3) O", 59.0871, 59.04969, "H7C3O", 59.08708, 59.0496872)]
        [TestCase("H(4) C(2) S", 60.1182, 60.003371, "H4C2S", 60.11916, 60.0033704)]
        [TestCase("H(2) 13C(2) O(2)", 60.0214, 60.012189, "H2^13.003355C2O2", 60.02139, 60.0121892)]
        [TestCase("H(4) C(6) O(-1)", 60.0966, 60.036386, "H4C6>O1", 0, 0)]
        [TestCase("H(4) C(6) O S(-1)", 60.0304, 60.054144, "H4C6O>S1", 0, 0)]
        [TestCase("H(-2) Zn", 63.3931, 61.913495, "Zn>H2", 0, 0)]
        [TestCase("H(-1) Cu", 62.5381, 61.921774, "Cu>H1", 0, 0)]
        [TestCase("H(2) C(5)", 62.0694, 62.01565, "H2C5", 62.06938, 62.0156492)]
        [TestCase("H(-2) 2H(6) C(3) O", 62.1002, 62.063875, "D6C3O>H2", 62.1002308, 62.0638766)]
        [TestCase("H 2H(4) C(2) N O(-1) S", 63.158, 63.044462, "HD4C2NS>O1", 0, 0)]
        [TestCase("O(2) S", 64.0638, 63.9619, "O2S", 64.0648, 63.961902)]
        [TestCase("O(4)", 63.9976, 63.979659, "O4", 63.9976, 63.97966)]
        [TestCase("C(4) O", 64.0422, 63.994915, "C4O", 64.0422, 63.994915)]
        [TestCase("H(2) C(4) O", 66.0581, 66.010565, "H2C4O", 66.05808, 66.0105642)]
        [TestCase("H(2) C(3) N(2)", 66.0614, 66.021798, "H2C3N2", 66.06146, 66.0217972)]
        [TestCase("H(-2) Cl(2)", 68.8901, 67.922055, "Cl2>H2", 0, 0)]
        [TestCase("H(4) C(4) O", 68.074, 68.026215, "H4C4O", 68.07396, 68.0262134)]
        [TestCase("H(4) C(3) N(2)", 68.0773, 68.037448, "H4C3N2", 68.07734, 68.0374464)]
        [TestCase("H(8) C(5)", 68.117, 68.0626, "H8C5", 68.11702, 68.0625968)]
        [TestCase("H(7) C(3) N(3) O(-1)", 69.1084, 69.069083, "H7C3N3>O1", 0, 0)]
        [TestCase("H(2) C(3) O(2)", 70.0468, 70.005479, "H2C3O2", 70.04678, 70.0054792)]
        [TestCase("H(6) C(4) O", 70.0898, 70.041865, "H6C4O", 70.08984, 70.0418626)]
        [TestCase("H(3) C(3) O(2)", 71.0547, 71.013304, "H3C3O2", 71.05472, 71.0133038)]
        [TestCase("H(5) C(3) N O", 71.0779, 71.037114, "H5C3NO", 71.07794, 71.037112)]
        [TestCase("H(5) C(7) N O(-2)", 71.1225, 71.05237, "H5C7N>O2", 0, 0)]
        [TestCase("H(9) C(4) N", 71.121, 71.073499, "H9C4N", 71.121, 71.0734954)]
        [TestCase("H(4) C(3) O(2)", 72.0627, 72.021129, "H4C3O2", 72.06266, 72.0211284)]
        [TestCase("H(4) C(7) O(-1)", 72.1073, 72.036386, "H4C7>O1", 0, 0)]
        [TestCase("2H(4) C(3) N(2)", 72.1019, 72.062555, "D4C3N2", 72.1019872, 72.0625552)]
        [TestCase("H(-1) C(5) N", 73.0523, 72.995249, "C5N>H1", 0, 0)]
        [TestCase("H(6) C(3) S", 74.1447, 74.019021, "H6C3S", 74.14574, 74.0190196)]
        [TestCase("H(2) 2H(3) C(3) N O", 74.0964, 74.055944, "H2D3C3NO", 74.0964254, 74.0559436)]
        [TestCase("H(4) C(2) O(-1) S(2)", 76.1838, 75.980527, "H4C2S2>O1", 0, 0)]
        [TestCase("H(4) C(2) O S", 76.1176, 75.998285, "H4C2OS", 76.11856, 75.9982854)]
        [TestCase("H(4) C(6)", 76.096, 76.0313, "H4C6", 76.09596, 76.0312984)]
        [TestCase("H(-1) Br", 78.8961, 77.910511, "Br>H1", 0, 0)]
        [TestCase("H(3) C O(2) P", 78.0071, 77.987066, "H3CO2P", 78.007081, 77.9870668)]
        [TestCase("H(6) C(6)", 78.1118, 78.04695, "H6C6", 78.11184, 78.0469476)]
        [TestCase("Se", 78.96, 79.91652, "Se", 78.96, 79.916521)]
        [TestCase("O(3) S", 80.0632, 79.956815, "O3S", 80.0642, 79.956817)]
        [TestCase("H O(3) P", 79.9799, 79.966331, "HO3P", 79.979901, 79.9663326)]
        [TestCase("H(4) C(5) O", 80.0847, 80.026215, "H4C5O", 80.08466, 80.0262134)]
        [TestCase("H(4) C(4) N(2)", 80.088, 80.037448, "H4C4N2", 80.08804, 80.0374464)]
        [TestCase("H(-1) C(3) N O(2)", 81.0297, 80.985078, "C3NO2>H1", 0, 0)]
        [TestCase("H(6) C(5) O", 82.1005, 82.041865, "H6C5O", 82.10054, 82.0418626)]
        [TestCase("H(5) C(8) N S(-1)", 83.067, 83.070128, "H5C8N>S1", 0, 0)]
        [TestCase("H(3) C(7) N O(-1)", 85.106, 85.031634, "H3C7N>O1", 0, 0)]
        [TestCase("H(7) C(4) N O", 85.1045, 85.052764, "H7C4NO", 85.10452, 85.0527612)]
        [TestCase("H(7) C(3) N(3)", 85.1078, 85.063997, "H7C3N3", 85.1079, 85.0639942)]
        [TestCase("H(11) C(5) N", 85.1475, 85.089149, "H11C5N", 85.14758, 85.0891446)]
        [TestCase("H(2) C(3) O S", 86.1124, 85.982635, "H2C3OS", 86.11338, 85.9826362)]
        [TestCase("H(2) C(3) O(3)", 86.0462, 86.000394, "H2C3O3", 86.04618, 86.0003942)]
        [TestCase("H(6) C(4) O(2)", 86.0892, 86.036779, "H6C4O2", 86.08924, 86.0367776)]
        [TestCase("H C(6) N", 87.0788, 87.010899, "HC6N", 87.07888, 87.0108986)]
        [TestCase("H(5) C(3) N S", 87.1435, 87.01427, "H5C3NS", 87.14454, 87.014269)]
        [TestCase("H(5) C(3) N O(2)", 87.0773, 87.032028, "H5C3NO2", 87.07734, 87.032027)]
        [TestCase("H(9) C(4) N O(-1) S", 87.1866, 87.050655, "H9C4NS>O1", 0, 0)]
        [TestCase("H(9) C(4) N O", 87.1204, 87.068414, "H9C4NO", 87.1204, 87.0684104)]
        [TestCase("H(4) C(3) O S", 88.1283, 87.998285, "H4C3OS", 88.12926, 87.9982854)]
        [TestCase("H C(-9) 13C(9) O(3) P", 88.9138, 88.996524, "H^13.003355C9O3P>C9", 88.913796, 88.9965276)]
        [TestCase("H(3) C(2) N O(3)", 89.0501, 89.011293, "H3C2NO3", 89.05016, 89.0112928)]
        [TestCase("H(3) C(6) N", 89.0947, 89.026549, "H3C6N", 89.09476, 89.0265478)]
        [TestCase("H(6) C(7)", 90.1225, 90.04695, "H6C7", 90.12254, 90.0469476)]
        [TestCase("H(2) 2H(5) C(4) N O", 90.1353, 90.084148, "H2D5C4NO", 90.135329, 90.0841472)]
        [TestCase("H(4) C(6) O", 92.0954, 92.026215, "H4C6O", 92.09536, 92.0262134)]
        [TestCase("H(4) C N O(2) P", 93.0217, 92.997965, "H4CNO2P", 93.021761, 92.9979654)]
        [TestCase("H(3) C O(3) P", 94.0065, 93.981981, "H3CO3P", 94.006481, 93.9819818)]
        [TestCase("H(6) C(6) O", 94.1112, 94.041865, "H6C6O", 94.11124, 94.0418626)]
        [TestCase("H N O(3) S", 95.0778, 94.967714, "HNO3S", 95.07888, 94.9677156)]
        [TestCase("H O(2) P S", 96.0455, 95.943487, "HO2PS", 96.046501, 95.9434896)]
        [TestCase("H(4) C(5) O(2)", 96.0841, 96.021129, "H4C5O2", 96.08406, 96.0211284)]
        [TestCase("H(8) C(6) O", 96.1271, 96.057515, "H8C6O", 96.12712, 96.0575118)]
        [TestCase("H(3) C(4) N O(2)", 97.0721, 97.016378, "H3C4NO2", 97.07216, 97.0163778)]
        [TestCase("H(11) C(6) N", 97.1582, 97.089149, "H11C6N", 97.15828, 97.0891446)]
        [TestCase("H(10) C(6) O", 98.143, 98.073165, "H10C6O", 98.143, 98.073161)]
        [TestCase("H(5) C(8) N O(-1)", 99.1326, 99.047285, "H5C8N>O1", 0, 0)]
        [TestCase("H(9) C(5) N O", 99.1311, 99.068414, "H9C5NO", 99.1311, 99.0684104)]
        [TestCase("H(9) C(4) N(3)", 99.1344, 99.079647, "H9C4N3", 99.13448, 99.0796434)]
        [TestCase("H(4) C(4) O(3)", 100.0728, 100.016044, "H4C4O3", 100.07276, 100.0160434)]
        [TestCase("H(8) C(4) N(2) O", 100.1191, 100.063663, "H8C4N2O", 100.1192, 100.0636598)]
        [TestCase("H(11) C(5) N O", 101.1469, 101.084064, "H11C5NO", 101.14698, 101.0840596)]
        [TestCase("H(5) C(2) As", 103.9827, 103.960719, "H5C2As", 103.9827, 103.960719)]
        [TestCase("H(4) C(3) O(2) S", 104.1277, 103.9932, "H4C3O2S", 104.12866, 103.9932004)]
        [TestCase("H(4) C(7) O", 104.1061, 104.026215, "H4C7O", 104.10606, 104.0262134)]
        [TestCase("H(4) 13C(4) O(3)", 104.0434, 104.029463, "H4^13.003355C4O3", 104.04338, 104.0294634)]
        [TestCase("2H(4) C(4) O(3)", 104.0974, 104.041151, "D4C4O3", 104.0974072, 104.0411522)]
        [TestCase("H(10) C(4) N O(2)", 104.1277, 104.071154, "H10C4NO2", 104.12774, 104.07115)]
        [TestCase("H(3) C(2) N O S(-1) Se", 103.9463, 104.965913, "H3C2NOSe>S1", 0, 0)]
        [TestCase("H(3) C(6) N O", 105.0941, 105.021464, "H3C6NO", 105.09416, 105.0214628)]
        [TestCase("H(7) C(7) N", 105.1372, 105.057849, "H7C7N", 105.13722, 105.0578462)]
        [TestCase("H(-1) Ag", 106.8603, 105.897267, "Ag>H1", 0, 0)]
        [TestCase("H(6) C(3) O(2) S", 106.1435, 106.00885, "H6C3O2S", 106.14454, 106.0088496)]
        [TestCase("H(6) C(7) O", 106.1219, 106.041865, "H6C7O", 106.12194, 106.0418626)]
        [TestCase("H(6) C(2) N O(2) P", 107.0483, 107.013615, "H6C2NO2P", 107.048341, 107.0136146)]
        [TestCase("H(5) C(4) N(5) O S(-1)", 107.0504, 107.077339, "H5C4N5O>S1", 0, 0)]
        [TestCase("H(5) C(2) O P S", 108.0993, 107.979873, "H5C2OPS", 108.100261, 107.979873)]
        [TestCase("H(5) C(2) O(3) P", 108.0331, 107.997631, "H5C2O3P", 108.033061, 107.997631)]
        [TestCase("H(4) C(6) O(2)", 108.0948, 108.021129, "H4C6O2", 108.09476, 108.0211284)]
        [TestCase("H(4) C N O P S", 109.0873, 108.975121, "H4CNOPS", 109.088361, 108.9751224)]
        [TestCase("H(-1) 2H(4) C(6) N O", 109.1188, 109.046571, "D4C6NO>H1", 109.1188072, 109.0465716)]
        [TestCase("H 2H(3) C(6) N O", 109.1205, 109.048119, "HD3C6NO", 109.1205854, 109.048119)]
        [TestCase("H(7) C(6) N O", 109.1259, 109.052764, "H7C6NO", 109.12592, 109.0527612)]
        [TestCase("H(3) C O(2) P S", 110.0721, 109.959137, "H3CO2PS", 110.073081, 109.9591388)]
        [TestCase("H(5) C(5) N O(2)", 111.0987, 111.032028, "H5C5NO2", 111.09874, 111.032027)]
        [TestCase("H(3) 13C(6) N O", 111.05, 111.041593, "H3^13.003355C6NO", 111.05009, 111.0415928)]
        [TestCase("H(9) C(6) N O", 111.1418, 111.068414, "H9C6NO", 111.1418, 111.0684104)]
        [TestCase("H(8) C(6) O(2)", 112.1265, 112.05243, "H8C6O2", 112.12652, 112.0524268)]
        [TestCase("H(7) C(5) N O(2)", 113.1146, 113.047679, "H7C5NO2", 113.11462, 113.0476762)]
        [TestCase("H(11) C(6) N O", 113.1576, 113.084064, "H11C6NO", 113.15768, 113.0840596)]
        [TestCase("H(2) C(4) O(4)", 114.0563, 113.995309, "H2C4O4", 114.05628, 113.9953092)]
        [TestCase("H(6) C(5) O(3)", 114.0993, 114.031694, "H6C5O3", 114.09934, 114.0316926)]
        [TestCase("H(6) C(4) N(2) O(2)", 114.1026, 114.042927, "H6C4N2O2", 114.10272, 114.0429256)]
        [TestCase("H(5) C(4) N O(3)", 115.0874, 115.026943, "H5C4NO3", 115.08744, 115.026942)]
        [TestCase("H(5) C(8) N", 115.132, 115.042199, "H5C8N", 115.13204, 115.042197)]
        [TestCase("H(-1) 2H(4) 13C(6) N O", 115.0747, 115.0667, "D4^13.003355C6NO>H1", 115.0747372, 115.0667016)]
        [TestCase("H(4) C(4) O(4)", 116.0722, 116.010959, "H4C4O4", 116.07216, 116.0109584)]
        [TestCase("H(4) C(3) N O(2) P", 117.0431, 116.997965, "H4C3NO2P", 117.043161, 116.9979654)]
        [TestCase("H(7) C(4) N O S", 117.1695, 117.024835, "H7C4NOS", 117.17052, 117.0248332)]
        [TestCase("H(8) C(8) N", 118.1558, 118.065674, "H8C8N", 118.15586, 118.0656708)]
        [TestCase("H(2) 2H(4) C(4) N(2) O(2)", 118.1273, 118.068034, "H2D4C4N2O2", 118.1273672, 118.0680344)]
        [TestCase("H(5) C(3) N O(2) S", 119.1423, 119.004099, "H5C3NO2S", 119.14334, 119.004099)]
        [TestCase("H(5) C(7) N O", 119.1207, 119.037114, "H5C7NO", 119.12074, 119.037112)]
        [TestCase("H(4) C(7) O(2)", 120.1055, 120.021129, "H4C7O2", 120.10546, 120.0211284)]
        [TestCase("H(8) C(4) O(2) S", 120.1701, 120.0245, "H8C4O2S", 120.17112, 120.0244988)]
        [TestCase("H(9) C(4) O(2) P", 120.0868, 120.034017, "H9C4O2P", 120.086821, 120.0340144)]
        [TestCase("H(6) 13C(4) 15N(2) O(2)", 120.0601, 120.050417, "H6^13.003355C4^15.000109N2O2", 120.060078, 120.0504156)]
        [TestCase("H(6) C(-2) 13C(6) N(2) O(2)", 120.0586, 120.063056, "H6^13.003355C6N2O2>C2", 120.05865, 120.0630556)]
        [TestCase("H(7) C(7) N O(-1) S", 121.2028, 121.035005, "H7C7NS>O1", 0, 0)]
        [TestCase("H(7) C(3) O(3) P", 122.0596, 122.013281, "H7C3O3P", 122.059641, 122.0132802)]
        [TestCase("H(6) C(7) O(2)", 122.1213, 122.036779, "H6C7O2", 122.12134, 122.0367776)]
        [TestCase("H(6) C(-2) 13C(6) 15N(2) O(2)", 122.0454, 122.057126, "H6^13.003355C6^15.000109N2O2>C2", 122.045388, 122.0571256)]
        [TestCase("H(10) C(8) O", 122.1644, 122.073165, "H10C8O", 122.1644, 122.073161)]
        [TestCase("H(10) C(7) N(2)", 122.1677, 122.084398, "H10C7N2", 122.16778, 122.084394)]
        [TestCase("H(6) C(2) N O(3) P", 123.0477, 123.00853, "H6C2NO3P", 123.047741, 123.0085296)]
        [TestCase("H(5) C(2) O(2) P S", 124.0987, 123.974787, "H5C2O2PS", 124.099661, 123.974788)]
        [TestCase("2H(5) C(7) N O", 124.1515, 124.068498, "D5C7NO", 124.151549, 124.068498)]
        [TestCase("H(7) C(6) N O(2)", 125.1253, 125.047679, "H7C6NO2", 125.12532, 125.0476762)]
        [TestCase("H(-1) I", 125.8965, 125.896648, "I>H1", 0, 0)]
        [TestCase("H(2) 2H(6) C(4) O(2) S", 126.2071, 126.062161, "H2D6C4O2S", 126.2080908, 126.062162)]
        [TestCase("H(14) C(8) O", 126.1962, 126.104465, "H14C8O", 126.19616, 126.1044594)]
        [TestCase("H(9) C(6) N O(2)", 127.1412, 127.063329, "H9C6NO2", 127.1412, 127.0633254)]
        [TestCase("H(13) C(7) N O", 127.1842, 127.099714, "H13C7NO", 127.18426, 127.0997088)]
        [TestCase("H(12) C(6) N(2) O", 128.1723, 128.094963, "H12C6N2O", 128.17236, 128.0949582)]
        [TestCase("H(14) C(7) N O", 128.1922, 128.107539, "H14C7NO", 128.1922, 128.1075334)]
        [TestCase("H(16) C(7) N(2)", 128.2153, 128.131349, "H16C7N2", 128.21542, 128.1313416)]
        [TestCase("H(7) C(5) N O(3)", 129.114, 129.042593, "H7C5NO3", 129.11402, 129.0425912)]
        [TestCase("H(7) C(9) N", 129.1586, 129.057849, "H7C9N", 129.15862, 129.0578462)]
        [TestCase("H(6) C(5) O(4)", 130.0987, 130.026609, "H6C5O4", 130.09874, 130.0266076)]
        [TestCase("H(2) 2H(5) C(6) N O(2)", 130.1561, 130.079062, "H2D5C6NO2", 130.156129, 130.0790622)]
        [TestCase("H(10) 2H(3) C(7) N O", 130.2027, 130.118544, "H10D3C7NO", 130.2027454, 130.1185404)]
        [TestCase("H(13) C(6) N O(2)", 131.1729, 131.094629, "H13C6NO2", 131.17296, 131.0946238)]
        [TestCase("H(4) C(4) O(5)", 132.0716, 132.005873, "H4C4O5", 132.07156, 132.0058734)]
        [TestCase("H(4) C(8) O(2)", 132.1162, 132.021129, "H4C8O2", 132.11616, 132.0211284)]
        [TestCase("H(8) C(9) O", 132.1592, 132.057515, "H8C9O", 132.15922, 132.0575118)]
        [TestCase("H(8) C(8) N(2)", 132.1625, 132.068748, "H8C8N2", 132.1626, 132.0687448)]
        [TestCase("H(7) C(4) N O(2) S", 133.1689, 133.019749, "H7C4NO2S", 133.16992, 133.0197482)]
        [TestCase("H(7) C(8) N O", 133.1473, 133.052764, "H7C8NO", 133.14732, 133.0527612)]
        [TestCase("H(7) 2H(6) C(7) N O", 133.2212, 133.137375, "H7D6C7NO", 133.2212308, 133.137372)]
        [TestCase("H(6) C(7) N(2) O", 134.1353, 134.048013, "H6C7N2O", 134.13542, 134.0480106)]
        [TestCase("H(5) C(7) N O(2)", 135.1201, 135.032028, "H5C7NO2", 135.12014, 135.032027)]
        [TestCase("H(10) C(4) N O(2) P", 135.1015, 135.044916, "H10C4NO2P", 135.101501, 135.044913)]
        [TestCase("H(4) C(3) O(4) S", 136.1265, 135.983029, "H4C3O4S", 136.12746, 135.9830304)]
        [TestCase("H(8) C(4) O S(2)", 136.2357, 136.001656, "H8C4OS2", 136.23772, 136.0016558)]
        [TestCase("H(9) C(4) O(3) P", 136.0862, 136.028931, "H9C4O3P", 136.086221, 136.0289294)]
        [TestCase("H(12) C(9) O", 136.191, 136.088815, "H12C9O", 136.19098, 136.0888102)]
        [TestCase("H(4) 2H(9) C(7) N O", 136.2397, 136.156205, "H4D9C7NO", 136.2397162, 136.1562036)]
        [TestCase("H(5) 2H(9) C(7) N O", 137.2476, 137.16403, "H5D9C7NO", 137.2476562, 137.1640282)]
        [TestCase("H(10) C(8) O(2)", 138.1638, 138.06808, "H10C8O2", 138.1638, 138.068076)]
        [TestCase("H(14) C(9) O", 138.2069, 138.104465, "H14C9O", 138.20686, 138.1044594)]
        [TestCase("H(7) C(2) 13C(6) N O", 139.1032, 139.072893, "H7C2^13.003355C6NO", 139.10325, 139.0728912)]
        [TestCase("H(13) C(7) N(3)", 139.1982, 139.110947, "H13C7N3", 139.19834, 139.1109418)]
        [TestCase("H(12) C(7) N(2) O", 140.183, 140.094963, "H12C7N2O", 140.18306, 140.0949582)]
        [TestCase("H(7) C(6) N O(3)", 141.1247, 141.042593, "H7C6NO3", 141.12472, 141.0425912)]
        [TestCase("H(12) C(6) 13C N(2) O", 141.1756, 141.098318, "H12C6^13.003355CN2O", 141.175715, 141.0983132)]
        [TestCase("H(2) 2H(6) C(4) O S(2)", 142.2727, 142.039317, "H2D6C4OS2", 142.2746908, 142.039319)]
        [TestCase("H(14) C(7) N(2) O", 142.1989, 142.110613, "H14C7N2O", 142.19894, 142.1106074)]
        [TestCase("H(9) C(6) N O(3)", 143.1406, 143.058243, "H9C6NO3", 143.1406, 143.0582404)]
        [TestCase("H(8) C(6) O(4)", 144.1253, 144.042259, "H8C6O4", 144.12532, 144.0422568)]
        [TestCase("H(12) C(6) 13C N 15N 18O", 144.1688, 144.099599, "H12C6^13.003355CN^15.000109N^17.999161O", 144.168845, 144.0995942)]
        [TestCase("H(12) C(4) 13C(3) N 15N O", 144.1544, 144.102063, "H12C4^13.003355C3N^15.000109NO", 144.154394, 144.1020582)]
        [TestCase("H(4) 2H(6) C(8) O(2)", 144.2008, 144.10574, "H4D6C8O2", 144.2007708, 144.1057392)]
        [TestCase("H(12) C(5) 13C(2) N(2) 18O", 144.168, 144.105918, "H12C5^13.003355C2N2^17.999161O", 144.168131, 144.1059142)]
        [TestCase("H(7) C(5) N O(2) S", 145.1796, 145.019749, "H7C5NO2S", 145.18062, 145.0197482)]
        [TestCase("H(15) C(7) 13C 15N 18O", 145.1966, 145.12, "H15C7^13.003355C^15.000109N^17.999161O", 145.196625, 145.119994)]
        [TestCase("H(13) 2H(2) C(7) 13C 15N O", 145.2092, 145.128307, "H13D2C7^13.003355C^15.000109NO", 145.2091876, 145.1283024)]
        [TestCase("H(13) 2H(2) C(8) N 18O", 145.2229, 145.132163, "H13D2C8N^17.999161O", 145.2229246, 145.1321584)]
        [TestCase("H(11) 2H(4) C(8) N O", 145.2354, 145.140471, "H11D4C8NO", 145.2354872, 145.1404668)]
        [TestCase("H(6) C(9) O(2)", 146.1427, 146.036779, "H6C9O2", 146.14274, 146.0367776)]
        [TestCase("H(4) 2H(5) C(6) N O(3)", 148.1714, 148.089627, "H4D5C6NO3", 148.171409, 148.0896264)]
        [TestCase("H(12) C 13C(6) 15N(2) O", 148.1257, 148.109162, "H12C^13.003355C6^15.000109N2O", 148.125728, 148.1091582)]
        [TestCase("H(7) C(8) N S", 149.2129, 149.02992, "H7C8NS", 149.21392, 149.0299182)]
        [TestCase("H(4) C(5) N(5) O", 150.1182, 150.041585, "H4C5N5O", 150.11836, 150.0415834)]
        [TestCase("H(8) C(4) O(2) S(2)", 152.2351, 151.996571, "H8C4O2S2", 152.23712, 151.9965708)]
        [TestCase("H(9) C(4) O(2) P S", 152.1518, 152.006087, "H9C4O2PS", 152.152821, 152.0060864)]
        [TestCase("H(3) C(6) N O(2) S", 153.1585, 152.988449, "H3C6NO2S", 153.15956, 152.9884498)]
        [TestCase("H(7) C(3) O(5) P", 154.0584, 154.00311, "H7C3O5P", 154.058441, 154.0031102)]
        [TestCase("H(6) C(7) O(4)", 154.1201, 154.026609, "H6C7O4", 154.12014, 154.0266076)]
        [TestCase("H(10) C(7) N(2) O(2)", 154.1665, 154.074228, "H10C7N2O2", 154.16658, 154.074224)]
        [TestCase("H(14) C(9) O(2)", 154.2063, 154.09938, "H14C9O2", 154.20626, 154.0993744)]
        [TestCase("H(14) C(8) N(2) O", 154.2096, 154.110613, "H14C8N2O", 154.20964, 154.1106074)]
        [TestCase("H(18) C(10) O", 154.2493, 154.135765, "H18C10O", 154.24932, 154.1357578)]
        [TestCase("H(5) C(6) N O(2) S", 155.1744, 155.004099, "H5C6NO2S", 155.17544, 155.004099)]
        [TestCase("H(13) C(8) N O(2)", 155.1943, 155.094629, "H13C8NO2", 155.19436, 155.0946238)]
        [TestCase("H(-2) Br(2)", 157.7921, 155.821022, "Br2>H2", 0, 0)]
        [TestCase("H(5) C(6) O(3) P", 156.0759, 155.997631, "H5C6O3P", 156.075861, 155.997631)]
        [TestCase("H(12) C(8) O(3)", 156.1791, 156.078644, "H12C8O3", 156.17908, 156.0786402)]
        [TestCase("H(12) C(6) N(4) O", 156.1857, 156.101111, "H12C6N4O", 156.18584, 156.1011062)]
        [TestCase("H(16) C(9) O(2)", 156.2221, 156.11503, "H16C9O2", 156.22214, 156.1150236)]
        [TestCase("H(7) C(6) N O(2) S", 157.1903, 157.019749, "H7C6NO2S", 157.19132, 157.0197482)]
        [TestCase("H(6) C(6) O(3) S", 158.175, 158.003765, "H6C6O3S", 158.17604, 158.0037646)]
        [TestCase("H(18) C(9) O(2)", 158.238, 158.13068, "H18C9O2", 158.23802, 158.1306728)]
        [TestCase("H(3) 13C(6) N O(2) S", 159.1144, 159.008578, "H3^13.003355C6NO2S", 159.11549, 159.0085798)]
        [TestCase("H(9) C(6) N O(2) S", 159.2062, 159.035399, "H9C6NO2S", 159.2072, 159.0353974)]
        [TestCase("H(9) C(10) N O", 159.1846, 159.068414, "H9C10NO", 159.1846, 159.0684104)]
        [TestCase("H(2) O(6) P(2)", 159.9598, 159.932662, "H2O6P2", 159.959802, 159.9326652)]
        [TestCase("H(8) C(6) O(5)", 160.1247, 160.037173, "H8C6O5", 160.12472, 160.0371718)]
        [TestCase("H(12) C(6) N(2) O(3)", 160.1711, 160.084792, "H12C6N2O3", 160.17116, 160.0847882)]
        [TestCase("H(5) 13C(6) N O(2) S", 161.1303, 161.024228, "H5^13.003355C6NO2S", 161.13137, 161.024229)]
        [TestCase("H(7) C(5) N O(5)", 161.1128, 161.032422, "H7C5NO5", 161.11282, 161.0324212)]
        [TestCase("H(13) C(11) O", 161.2203, 161.09664, "H13C11O", 161.22032, 161.0966348)]
        [TestCase("H(10) C(9) N(2) O", 162.1885, 162.079313, "H10C9N2O", 162.18858, 162.079309)]
        [TestCase("H(15) C(7) O(2) P", 162.1666, 162.080967, "H15C7O2P", 162.166561, 162.080962)]
        [TestCase("H(18) C(8) O(3)", 162.2267, 162.125595, "H18C8O3", 162.22672, 162.1255878)]
        [TestCase("H(9) C(5) N O(3) S", 163.1949, 163.030314, "H9C5NO3S", 163.1959, 163.0303124)]
        [TestCase("H(9) C(9) N S", 163.2395, 163.04557, "H9C9NS", 163.2405, 163.0455674)]
        [TestCase("C(6) N(2) O(4)", 164.0752, 163.985807, "C6N2O4", 164.07528, 163.985808)]
        [TestCase("H(13) C(6) O(3) P", 164.1394, 164.060231, "H13C6O3P", 164.139381, 164.0602278)]
        [TestCase("H(21) C(12)", 165.2951, 165.164326, "H21C12", 165.29514, 165.1643166)]
        [TestCase("H(2) C(6) N(2) O(4)", 166.0911, 166.001457, "H2C6N2O4", 166.09116, 166.0014572)]
        [TestCase("H(5) C(3) O(6) P", 168.042, 167.982375, "H5C3O6P", 168.041961, 167.982376)]
        [TestCase("H(8) C(8) O(2) S", 168.2129, 168.0245, "H8C8O2S", 168.21392, 168.0244988)]
        [TestCase("H(24) C(12)", 168.319, 168.187801, "H24C12", 168.31896, 168.1877904)]
        [TestCase("H(7) C(6) N(3) O(3)", 169.1381, 169.048741, "H7C6N3O3", 169.1382, 169.0487392)]
        [TestCase("H(7) C(7) O(3) P", 170.1024, 170.013281, "H7C7O3P", 170.102441, 170.0132802)]
        [TestCase("H(6) C(11) O(2)", 170.1641, 170.036779, "H6C11O2", 170.16414, 170.0367776)]
        [TestCase("H(6) C(10) N(2) O", 170.1674, 170.048013, "H6C10N2O", 170.16752, 170.0480106)]
        [TestCase("H(19) C(9) N(2) O", 171.26, 171.149738, "H19C9N2O", 171.26004, 171.1497304)]
        [TestCase("H(8) C(6) N(2) S(2)", 172.2711, 172.01289, "H8C6N2S2", 172.2732, 172.0128888)]
        [TestCase("H(8) C(11) O(2)", 172.18, 172.05243, "H8C11O2", 172.18002, 172.0524268)]
        [TestCase("H(7) C(6) N O(2) S(-1) Se", 172.0203, 172.992127, "H7C6NO2Se>S1", 0, 0)]
        [TestCase("H(13) C(7) N(2) O(3)", 173.1897, 173.092617, "H13C7N2O3", 173.1898, 173.0926128)]
        [TestCase("H(6) C(6) O(2) S(2)", 174.2406, 173.980921, "H6C6O2S2", 174.24264, 173.9809216)]
        [TestCase("H(6) C(9) N(2) S", 174.2223, 174.025169, "H6C9N2S", 174.22342, 174.0251676)]
        [TestCase("H(16) 2H(3) C(9) N(2) O", 174.2784, 174.168569, "H16D3C9N2O", 174.2785254, 174.168562)]
        [TestCase("H(9) C(6) N O(3) S", 175.2056, 175.030314, "H9C6NO3S", 175.2066, 175.0303124)]
        [TestCase("H(5) C(13) N", 175.1855, 175.042199, "H5C13N", 175.18554, 175.042197)]
        [TestCase("H(8) C(6) O(4) S", 176.1903, 176.01433, "H8C6O4S", 176.19132, 176.0143288)]
        [TestCase("H(-7) O Fe(3)", 176.4788, 176.744957, "OFe3>H7", 0, 0)]
        [TestCase("H(2) 2H(5) C(6) N O(2) S(-1) Se", 177.0511, 178.023511, "H2D5C6NO2Se>S1", 0, 0)]
        [TestCase("H(9) C(8) N O(2) S", 183.2276, 183.035399, "H9C8NO2S", 183.2286, 183.0353974)]
        [TestCase("H(4) C(7) O(4) S", 184.1693, 183.983029, "H4C7O4S", 184.17026, 183.9830304)]
        [TestCase("H(14) C(9) N O S", 184.2786, 184.07961, "H14C9NOS", 184.2796, 184.0796054)]
        [TestCase("H(20) C(10) N(2) O", 184.2786, 184.157563, "H20C10N2O", 184.27868, 184.157555)]
        [TestCase("H(23) C(10) N(3)", 185.3097, 185.189198, "H23C10N3", 185.30984, 185.1891878)]
        [TestCase("H(7) C(7) O(4) P", 186.1018, 186.008196, "H7C7O4P", 186.101841, 186.0081952)]
        [TestCase("H(12) C(8) O S(2)", 188.3103, 188.032956, "H12C8OS2", 188.31228, 188.0329542)]
        [TestCase("H(20) C(14)", 188.3086, 188.156501, "H20C14", 188.3086, 188.156492)]
        [TestCase("H(15) 2H(5) C(10) N(2) O", 189.3094, 189.188947, "H15D5C10N2O", 189.309489, 189.188941)]
        [TestCase("H(10) C(10) N(2) O(2)", 190.1986, 190.074228, "H10C10N2O2", 190.19868, 190.074224)]
        [TestCase("H(8) C(6) O(3) S(2)", 192.2559, 191.991486, "H8C6O3S2", 192.25792, 191.9914858)]
        [TestCase("H(9) C(7) N(5) O(2)", 195.1787, 195.075625, "H9C7N5O2", 195.17886, 195.0756214)]
        [TestCase("H(12) C(9) N(2) O(3)", 196.2032, 196.084792, "H12C9N2O3", 196.20326, 196.0847882)]
        [TestCase("H(14) C(9) N(3) O(2)", 196.2264, 196.108602, "H14C9N3O2", 196.22648, 196.1085964)]
        [TestCase("H(16) C(10) N(2) O(2)", 196.2462, 196.121178, "H16C10N2O2", 196.24632, 196.1211716)]
        [TestCase("H(12) C(5) N O(5) P", 197.1262, 197.04531, "H12C5NO5P", 197.126281, 197.0453072)]
        [TestCase("H(3) C(6) N(2) O(4) S", 199.164, 198.981352, "H3C6N2O4S", 199.1651, 198.9813538)]
        [TestCase("H(13) C(9) N O(2) S", 199.27, 199.066699, "H13C9NO2S", 199.27106, 199.0666958)]
        [TestCase("Hg", 200.59, 201.970617, "Hg", 200.59, 201.970632)]
        [TestCase("H(7) C(4) O(3) P Cl(2)", 204.9763, 203.950987, "H7C4O3PCl2", 204.975741, 203.9509862, 0.00057)]
        [TestCase("H(24) C(15)", 204.3511, 204.187801, "H24C15", 204.35106, 204.1877904)]
        [TestCase("H(22) C(14) O", 206.3239, 206.167065, "H22C14O", 206.32388, 206.1670562)]
        [TestCase("H(24) C(14) O", 208.3398, 208.182715, "H24C14O", 208.33976, 208.1827054)]
        [TestCase("H(11) C(6) N O(3) S(2)", 209.2864, 209.018035, "H11C6NO3S2", 209.28848, 209.0180336)]
        [TestCase("H(10) C(6) O(4) S(2)", 210.2712, 210.00205, "H10C6O4S2", 210.2732, 210.00205)]
        [TestCase("H(22) C(13) O(2)", 210.3126, 210.16198, "H22C13O2", 210.31258, 210.1619712)]
        [TestCase("H(26) C(14) O", 210.3556, 210.198366, "H26C14O", 210.35564, 210.1983546)]
        [TestCase("H C(6) N(3) O(6)", 211.0886, 210.986535, "HC6N3O6", 211.08876, 210.9865366)]
        [TestCase("H(9) C(5) O(7) P", 212.0945, 212.00859, "H9C5O7P", 212.094521, 212.0085894)]
        [TestCase("H(15) C(9) N(3) O(3)", 213.2337, 213.111341, "H15C9N3O3", 213.23382, 213.111336)]
        [TestCase("H(14) C(9) N(2) O(4)", 214.2185, 214.095357, "H14C9N2O4", 214.21854, 214.0953524)]
        [TestCase("H(5) C(7) N O(3) S(2)", 215.2495, 214.971084, "H5C7NO3S2", 215.25154, 214.971086)]
        [TestCase("H(7) C(9) N O Cl(2)", 216.064, 214.990469, "H7C9NOCl2", 216.06342, 214.9904672, 0.000585)]
        [TestCase("H(16) C(10) O(5)", 216.231, 216.099774, "H16C10O5", 216.23104, 216.0997686)]
        [TestCase("H(15) C(9) N O(5)", 217.2191, 217.095023, "H15C9NO5", 217.21914, 217.095018)]
        [TestCase("H(20) C(8) 13C(3) 15N(2) O(2)", 217.2535, 217.156612, "H20C8^13.003355C3^15.000109N2O2", 217.253483, 217.156605)]
        [TestCase("H(18) 2H(2) C(10) 13C 15N(2) O(2)", 217.2805, 217.162456, "H18D2C10^13.003355C^15.000109N2O2", 217.2804966, 217.1624494)]
        [TestCase("H(20) C(7) 13C(4) N 15N O(2)", 217.2527, 217.162932, "H20C7^13.003355C4N^15.000109NO2", 217.252769, 217.162925)]
        [TestCase("H(18) 2H(2) C(9) 13C(2) N 15N O(2)", 217.2797, 217.168776, "H18D2C9^13.003355C2N^15.000109NO2", 217.2797826, 217.1687694)]
        [TestCase("H(18) 2H(2) C(8) 13C(3) N(2) O(2)", 217.279, 217.175096, "H18D2C8^13.003355C3N2O2", 217.2790686, 217.1750894)]
        [TestCase("H(22) C(15) O", 218.3346, 218.167065, "H22C15O", 218.33458, 218.1670562)]
        [TestCase("H(13) C(12) N O(3)", 219.2365, 219.089543, "H13C12NO3", 219.23656, 219.0895388)]
        [TestCase("H(8) C(10) N(2) O(4)", 220.1815, 220.048407, "H8C10N2O4", 220.1816, 220.0484048)]
        [TestCase("H(12) C(8) O(7)", 220.1767, 220.058303, "H12C8O7", 220.17668, 220.0583002)]
        [TestCase("H(24) C(15) O", 220.3505, 220.182715, "H24C15O", 220.35046, 220.1827054)]
        [TestCase("H(5) C 13C(6) N O(3) S(2)", 221.2054, 220.991213, "H5C^13.003355C6NO3S2", 221.20747, 220.991216)]
        [TestCase("H(20) C(12) N(2) O(2)", 224.2994, 224.152478, "H20C12N2O2", 224.29948, 224.15247)]
        [TestCase("H(32) C(16)", 224.4253, 224.250401, "H32C16", 224.42528, 224.2503872)]
        [TestCase("H(11) C(13) N(3) O", 225.2459, 225.090212, "H11C13N3O", 225.24606, 225.0902076)]
        [TestCase("H(15) C(10) N(3) O S", 225.3106, 225.093583, "H15C10N3OS", 225.31172, 225.093578)]
        [TestCase("H(20) C(11) 13C N(2) O(2)", 225.2921, 225.155833, "H20C11^13.003355CN2O2", 225.292135, 225.155825)]
        [TestCase("H(10) C(10) O(6)", 226.1828, 226.047738, "H10C10O6", 226.1828, 226.047736)]
        [TestCase("H(14) C(10) N(2) O(2) S", 226.2954, 226.077598, "H14C10N2O2S", 226.29644, 226.0775944, 0.00)]
        [TestCase("H(17) C(10) N(3) O(3)", 227.2603, 227.126991, "H17C10N3O3", 227.2604, 227.1269852)]
        [TestCase("H(29) C(14) N O", 227.3862, 227.224915, "H29C14NO", 227.3862, 227.2249024)]
        [TestCase("H(8) C(8) N O(5) P", 229.1266, 229.014009, "H8C8NO5P", 229.126621, 229.0140088)]
        [TestCase("H(20) C(8) 13C(4) N 15N O(2)", 229.2634, 229.162932, "H20C8^13.003355C4N^15.000109NO2", 229.263469, 229.162925)]
        [TestCase("H(10) C(8) N O(5) P", 231.1425, 231.02966, "H10C8NO5P", 231.142501, 231.029658)]
        [TestCase("H(14) C(9) N O(4) S", 232.2768, 232.064354, "H14C9NO4S", 232.2778, 232.0643504)]
        [TestCase("H(11) C(12) N O(2) S", 233.2862, 233.051049, "H11C12NO2S", 233.28728, 233.0510466)]
        [TestCase("H(14) C(9) O(7)", 234.2033, 234.073953, "H14C9O7", 234.20326, 234.0739494)]
        [TestCase("H(22) C(15) O(2)", 234.334, 234.16198, "H22C15O2", 234.33398, 234.1619712)]
        [TestCase("H(20) C(3) 13C(9) 15N(2) O(2)", 235.2201, 235.176741, "H20C3^13.003355C9^15.000109N2O2", 235.220113, 235.176735)]
        [TestCase("H(17) C 13C(9) N(3) O(3)", 236.1942, 236.157185, "H17C^13.003355C9N3O3", 236.194295, 236.1571802)]
        [TestCase("H(28) C(16) O", 236.3929, 236.214016, "H28C16O", 236.39292, 236.2140038)]
        [TestCase("H(15) C(12) N O(4)", 237.2518, 237.100108, "H15C12NO4", 237.25184, 237.100103)]
        [TestCase("H(30) C(16) O", 238.4088, 238.229666, "H30C16O", 238.4088, 238.229653)]
        [TestCase("H(16) C(10) N(4) O S", 240.3252, 240.104482, "H16C10N4OS", 240.3264, 240.1044766)]
        [TestCase("H(16) C(16) O(2)", 240.297, 240.11503, "H16C16O2", 240.29704, 240.1150236)]
        [TestCase("H(15) C(10) N(3) O(2) S", 241.31, 241.088497, "H15C10N3O2S", 241.31112, 241.088493)]
        [TestCase("H(14) C(9) N(4) O(4)", 242.2319, 242.101505, "H14C9N4O4", 242.23202, 242.1015004)]
        [TestCase("H(13) C(9) N(3) O(5)", 243.2166, 243.085521, "H13C9N3O5", 243.21674, 243.0855168)]
        [TestCase("H(12) C(10) O(7)", 244.1981, 244.058303, "H12C10O7", 244.19808, 244.0583002)]
        [TestCase("H(15) C(9) 13C N(2) O(5)", 244.2292, 244.101452, "H15C9^13.003355CN2O5", 244.229235, 244.101447)]
        [TestCase("H(28) C(13) O(4)", 248.359, 248.19876, "H28C13O4", 248.35902, 248.1987488)]
        [TestCase("H(4) C(10) N O(5) S", 250.2075, 249.981018, "H4C10NO5S", 250.2085, 249.9810194)]
        [TestCase("H(-2) I(2)", 251.7931, 251.793296, "I2>H2", 0, 0)]
        [TestCase("H(10) C(10) N(3) O(3) S", 252.2697, 252.044287, "H10C10N3O3S", 252.27082, 252.044285)]
        [TestCase("H(12) C(9) N(6) O(3)", 252.23, 252.097088, "H12C9N6O3", 252.23022, 252.0970842)]
        [TestCase("H(12) C(11) N O Br", 254.1231, 253.010225, "H12C11NOBr", 254.12312, 253.0102202)]
        [TestCase("H(11) C(6) O(9) P", 258.1199, 258.014069, "H11C6O9P", 258.119901, 258.0140686)]
        [TestCase("H(14) C(10) N(2) O(6)", 258.228, 258.085186, "H14C10N2O6", 258.22804, 258.0851824)]
        [TestCase("H(18) C(10) N(4) O(2) S", 258.3405, 258.115047, "H18C10N4O2S", 258.34168, 258.1150408)]
        [TestCase("H(21) C(12) N O(5)", 259.2988, 259.141973, "H21C12NO5", 259.29888, 259.1419656)]
        [TestCase("H(17) C(18) N O", 263.3337, 263.131014, "H17C18NO", 263.33372, 263.1310072)]
        [TestCase("H(31) C(18) O", 263.4381, 263.237491, "H31C18O", 263.43814, 263.2374776)]
        [TestCase("H(24) C(20)", 264.4046, 264.187801, "H24C20", 264.40456, 264.1877904)]
        [TestCase("H(19) C(18) N O", 265.3496, 265.146664, "H19C18NO", 265.3496, 265.1466564)]
        [TestCase("H(10) C(16) O(4)", 266.2482, 266.057909, "H10C16O4", 266.2482, 266.057906)]
        [TestCase("H(18) C(18) O(2)", 266.3343, 266.13068, "H18C18O2", 266.33432, 266.1306728)]
        [TestCase("H(26) C(20)", 266.4204, 266.203451, "H26C20", 266.42044, 266.2034396)]
        [TestCase("H(9) C(10) N(3) O(4) S", 267.2612, 267.031377, "H9C10N3O4S", 267.26228, 267.0313754)]
        [TestCase("H(21) C(13) N(3) O(3)", 267.3241, 267.158292, "H21C13N3O3", 267.32426, 267.1582836)]
        [TestCase("H(10) C(10) N(3) O(4) S", 268.2691, 268.039202, "H10C10N3O4S", 268.27022, 268.0392)]
        [TestCase("H(20) C(11) 13C N(3) O(4)", 271.2976, 271.148736, "H20C11^13.003355CN3O4", 271.297675, 271.148729)]
        [TestCase("H(32) C(20)", 272.4681, 272.250401, "H32C20", 272.46808, 272.2503872)]
        [TestCase("H(13) C(14) O(4) P", 276.2244, 276.055146, "H13C14O4P", 276.224381, 276.0551428)]
        [TestCase("H(17) C(10) N O(6) S", 279.3101, 279.077658, "H17C10NO6S", 279.31112, 279.0776542)]
        [TestCase("H(10) C(16) O(5)", 282.2476, 282.052824, "H10C16O5", 282.2476, 282.052821)]
        [TestCase("H(11) C(15) O(6)", 287.2442, 287.055563, "H11C15O6", 287.24424, 287.0555606)]
        [TestCase("H(26) C(19) N(-2) O(4)", 290.3939, 290.176961, "H26C19O4>N2", 0, 0)]
        [TestCase("H(26) C(17) O(4)", 294.3859, 294.183109, "H26C17O4", 294.38594, 294.1830996)]
        [TestCase("H(25) C(15) N(3) O(3)", 295.3773, 295.189592, "H25C15N3O3", 295.37742, 295.189582)]
        [TestCase("H(13) C(12) N(2) O(2) Br", 297.1478, 296.016039, "H13C12N2O2Br", 297.1479, 296.0160338)]
        [TestCase("H(13) C(10) 13C(2) N(2) O(2) Br", 299.1331, 298.022748, "H13C10^13.003355C2N2O2Br", 299.13321, 298.0227438)]
        [TestCase("H(22) C(13) N(4) O(2) S", 298.4044, 298.146347, "H22C13N4O2S", 298.40554, 298.1463392)]
        [TestCase("H(26) C(20) O(2)", 298.4192, 298.19328, "H26C20O2", 298.41924, 298.1932696)]
        [TestCase("H(25) C(14) N(3) O(2) S", 299.4322, 299.166748, "H25C14N3O2S", 299.43332, 299.166739)]
        [TestCase("H(8) C(4) N(5) O(7) S(2)", 302.2656, 301.986514, "H8C4N5O7S2", 302.26782, 301.9865158)]
        [TestCase("H(25) C(10) 13C(4) N(2) 15N O(2) S", 304.3962, 304.177202, "H25C10^13.003355C4N2^15.000109NO2S", 304.397309, 304.177194)]
        [TestCase("H(24) C(8) 13C(6) N(2) 15N(2) O(3)", 304.3081, 304.19904, "H24C8^13.003355C6N2^15.000109N2O3", 304.308188, 304.1990314)]
        [TestCase("H(24) C(7) 13C(7) N(3) 15N O(3)", 304.3074, 304.20536, "H24C7^13.003355C7N3^15.000109NO3", 304.307474, 304.2053514)]
        [TestCase("H(25) C(8) 13C(7) N 15N(2) O(3)", 304.3127, 304.207146, "H25C8^13.003355C7N^15.000109N2O3", 304.312743, 304.207137)]
        [TestCase("H(12) C(9) N(3) O(7) P", 305.1812, 305.041287, "H12C9N3O7P", 305.181361, 305.0412852)]
        [TestCase("H(15) C(10) N(3) O(6) S", 305.3076, 305.068156, "H15C10N3O6S", 305.30872, 305.068153)]
        [TestCase("H(11) C(9) N(2) O(8) P", 306.166, 306.025302, "H11C9N2O8P", 306.166081, 306.0253016)]
        [TestCase("H(18) C(12) O(9)", 306.2659, 306.095082, "H18C12O9", 306.26592, 306.0950778)]
        [TestCase("H(26) C(19) N(-2) O(5)", 306.3933, 306.171876, "H26C19O5>N2", 0, 0)]
        [TestCase("H(20) C(14) N(4) O(4)", 308.333, 308.148455, "H20C14N4O4", 308.33316, 308.148448)]
        [TestCase("H(27) C(16) N(3) O(3)", 309.4039, 309.205242, "H27C16N3O3", 309.404, 309.2052312)]
        [TestCase("H(10) C(17) O(6)", 310.2577, 310.047738, "H10C17O6", 310.2577, 310.047736)]
        [TestCase("H(22) C(15) N(2) O(3) S", 310.4118, 310.135113, "H22C15N2O3S", 310.41286, 310.1351062)]
        [TestCase("H(25) C(15) N(3) O(2) S", 311.4429, 311.166748, "H25C15N3O2S", 311.44402, 311.166739)]
        [TestCase("H(24) C(15) N(2) O(3) S", 312.4277, 312.150763, "H24C15N2O3S", 312.42874, 312.1507554)]
        [TestCase("H(26) C(20) O(3)", 314.4186, 314.188195, "H26C20O3", 314.41864, 314.1881846)]
        [TestCase("H(21) C(22) P", 316.3759, 316.138088, "H21C22P", 316.375901, 316.1380796)]
        [TestCase("H(28) C(20) O(3)", 316.4345, 316.203845, "H28C20O3", 316.43452, 316.2038338)]
        public void TestUniModFormulas(
            string formula,
            double avgMassUniMod,
            double monoMassUniMod,
            string expectedUpdatedFormula,
            double expectedAvgMass,
            double expectedMonoMass,
            double matchToleranceVsUniModAvg = 0.0005,
            double matchToleranceVsUniModMono = 0.0005,
            double matchTolerance = MATCHING_MASS_EPSILON)
        {
            var resultDaAvg = mMwtWinAvg.ComputeMass(formula);
            var resultDaIso = mMwtWinIso.ComputeMass(formula);

            // Uncomment to write the data to disk
            // using (var writer = new StreamWriter(new FileStream(@"C:\Temp\UnimodMasses.txt", FileMode.Append, FileAccess.Write, FileShare.Read)))
            // {
            //     writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
            //         formula, avgMassUniMod, monoMassUniMod, expectedUpdatedFormula, resultDaAvg, resultDaIso);
            // }

            Console.WriteLine("{0,-15} -> {1,-15}: {2,12:F8} Da (average) and  {3,12:F8} Da (isotopic)",
                formula, mMwtWinIso.Compound.FormulaCapitalized, resultDaAvg, resultDaIso);

            Assert.AreEqual(expectedUpdatedFormula, mMwtWinIso.Compound.FormulaCapitalized, "Capitalized formula does not match the expected value");

            Assert.AreEqual(expectedAvgMass, resultDaAvg, matchTolerance, "Actual mass does not match expected average mass");
            Assert.AreEqual(expectedMonoMass, resultDaIso, matchTolerance, "Actual mass does not match expected isotopic mass");

            if (resultDaIso == 0)
                return;

            Assert.AreEqual(monoMassUniMod, resultDaIso, matchToleranceVsUniModMono, "Computed monoisotopic mass is not within tolerance of the UniMod mass");

            if (Math.Abs(avgMassUniMod - resultDaAvg) > matchToleranceVsUniModAvg)
            {
                if (mMwtWinIso.Compound.FormulaCapitalized.Contains("S"))
                {
                    // UniMod uses a slightly different average mass for S
                    Assert.AreEqual(avgMassUniMod, resultDaAvg, 0.0025, "Computed average mass is not within tolerance of the UniMod mass (compound with sulfur)");
                    return;
                }
            }

            Assert.AreEqual(avgMassUniMod, resultDaAvg, matchToleranceVsUniModAvg, "Computed average mass is not within tolerance of the UniMod mass");
        }
    }
}
