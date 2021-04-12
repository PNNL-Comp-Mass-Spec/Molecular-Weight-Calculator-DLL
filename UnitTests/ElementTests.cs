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
        [TestCase("BrCH2(CH2)7CH2Br",     286.04722000, 283.97751480)]
        [TestCase("FeCl3-6H2O",           270.29478000, 268.90488320)]
        [TestCase("Co(Bpy)(CO)4",         327.15760000, 326.98160280)]
        [TestCase("^13C6H6-.1H2O",         85.84916800,  85.84800402)]
        [TestCase("HGlyLeuTyrOH",         351.39762000, 351.17941200)]
        [TestCase("BrCH2(CH2)7CH2Br>CH8", 265.97300000, 263.91491800)]
        [TestCase("C6H6-H2O-2ZnHgMg-U",   914.72602000, 913.87795180)]
        [TestCase("2FeCl3-6H2O",          432.49788000, 429.74638120)]
        [TestCase("C6H5Cl3>H3Cl2>HCl",    145.99342000, 144.96117980)]
        public void ComputeMass(string formula, double expectedAvgMass, double expectedMonoMass)
        {
            var resultDaAvg = mMwtWinAvg.ComputeMass(formula);
            var resultDaIso = mMwtWinIso.ComputeMass(formula);

            Console.WriteLine("{0,-22} -> {1,12:F8} Da (average) and  {2,12:F8} Da (isotopic)",
                formula, resultDaAvg, resultDaIso);

            Assert.AreEqual(expectedAvgMass, resultDaAvg, MATCHING_MASS_EPSILON, "Actual mass does not match expected average mass");
            Assert.AreEqual(expectedMonoMass, resultDaIso, MATCHING_MASS_EPSILON, "Actual mass does not match expected isotopic mass");
        }

        [Test]
        [TestCase("^13C6H6-H2O", 102.06292, 102.0575118, -6)]
        [TestCase("^13C6H6[.1H2O]", 85.849168, 85.84800402, -6)]
        [TestCase("^13C6H6[H2O]", 102.06292, 102.0575118, -6, true)]
        [TestCase("^13C6H6[H2O]2", 120.0782, 120.068076, -6, true)]
        [TestCase("13C6H6-6H2O", 1123.5456, 1122.673704, 222)]
        [TestCase("Et2O", 74.1216, 74.073161, 0)]
        [TestCase("DCH2", 16.04058, 16.0296492, 3)]
        //[TestCase("", 0, 0, 0)]
        [TestCase("CaOCH4(CH2)7Br", 250.20992, 249.0166848, 13)] // TODO: stdDev difference (-8.67e-19) (but it's insignificant)
        [TestCase("canyoch4(ch2)7br", 353.12251, 351.9256148, 13)] // TODO: stdDev difference (-8.67e-19) (but it's insignificant)
        [TestCase("F(DCH2)7Br", 211.1864632, 210.1242836, 19)]
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
            Console.WriteLine("Average Mass:");
            mMwtWinAvg.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var resultDaAvg = mMwtWinAvg.ComputeMassExtra(formula, out var parseDataAvg);
            ReportParseData(parseDataAvg);
            Assert.Greater(resultDaAvg, 0);
            Assert.AreEqual(expectedAvgMass, resultDaAvg, MATCHING_MASS_EPSILON, "Actual mass does not match expected average mass");
            Assert.AreEqual(expectedCharge, parseDataAvg.Charge, MATCHING_CHARGE_EPSILON, "Actual charge does not match expected charge");

            Console.WriteLine("");
            Console.WriteLine("Isotopic Mass:");
            mMwtWinIso.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var resultDaIso = mMwtWinIso.ComputeMassExtra(formula, out var parseDataIso);
            ReportParseData(parseDataIso);
            Assert.Greater(resultDaIso, 0);
            Assert.AreEqual(expectedMonoMass, resultDaIso, MATCHING_MASS_EPSILON, "Actual mass does not match expected isotopic mass");
            Assert.AreEqual(expectedCharge, parseDataIso.Charge, MATCHING_CHARGE_EPSILON, "Actual charge does not match expected charge");
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
        [TestCase("D","^2.014H")]
        [TestCase("D2CH", "C^2.014H2H")]
        [TestCase("D2CH^3H3", "C^2.014H2^3H3H")]
        [TestCase("D2C^13C5H^3H3", "^13C5C^2.014H2^3H3H")]
        public void ConvertToEmpiricalTests(string formula, string expectedEmpirical)
        {
            mMwtWinAvg.Compound.Formula = formula;
            var empirical = mMwtWinAvg.Compound.ConvertToEmpirical();
            Assert.AreEqual(expectedEmpirical, empirical);
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
    }
}
