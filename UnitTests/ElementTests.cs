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
        [TestCase("^13C6H6-H2O")]
        [TestCase("^13C6H6[.1H2O]", false)]
        [TestCase("^13C6H6[H2O]2", true)]
        [TestCase("^13C6H6[H2O]", true)]
        [TestCase("13C6H6-6H2O")]
        [TestCase("Et2O")]
        //[TestCase("")]
        [TestCase("CaOCH4(CH2)7Br")] // TODO: stdDev difference (-8.67e-19) (but it's insignificant)
        [TestCase("canyoch4(ch2)7br")] // TODO: stdDev difference (-8.67e-19) (but it's insignificant)
        [TestCase("F(DCH2)7Br")]
        [TestCase("Pt(CH2)7Br-[5Ca123456789]")]
        [TestCase("Pt(CH2)7Br-[5Ca123456789H]")]
        [TestCase("-[2.5CaH]", false)]
        [TestCase("^42CaH")]
        [TestCase("^98CaH")]
        [TestCase("^18CaH")]
        [TestCase("(CH2)7Br-3.2CaH")]
        [TestCase("[2CaOH]-K")]
        [TestCase("K-[2CaOH]")]
        [TestCase("H2Ca2KO2")]
        [TestCase("H2O^23.9Ca")]
        [TestCase("^13C6H6")]
        [TestCase("C6H6")]
        [TestCase("C6H3^19.8Ar")]
        [TestCase("C6H3^19.8arpbbb")]
        [TestCase("C6H3^19.88Ar4Pb>Ar")]
        [TestCase("C6>C4")]
        //[TestCase("")]
        [TestCase("DCH2")]
        [TestCase("^23CaH")]
        //[TestCase("")]
        [TestCase("C6>C4")]
        [TestCase("HGly5.3Leu2.2Tyr0.03OH")]
        [TestCase("HGly5.3leu2.2tyr0.03oh")]
        [TestCase("HHeLiBeBCNOFNeNaMgAlSiPSClArKCaScTiVCrMnFeCoNiCuZnGaGeAsSeBrKrRbSrYZrNbMoTcRuRhPdAgCdInSnSbTeIXe")]
        [TestCase("CsBaLaCePrNdPmSmEuGdTbDyHoErTmYbLuHfTaWReOsIrPtAuHgTlPbBiPoAtRnFrRaAcThPaUNpPuAmCmBkCfEsFmMdNoLr")]
        [TestCase("hhelibebcnofnenamgalsipsClArKcasctivcrmnfeconicuzngageassebrkrrbsryzrnbmotcrurhpdagcdinsnsbteixe")]
        [TestCase("csbalaceprndpmsmeugdtbdyhoertmybluhftawreosirptauhgtlpbbipoatrnfrraacthpaunppuamcmbkcfesfmmdnolr")]
        [TestCase("cdinsnsbteixecsbalaceprndpm")]
        [TestCase("CdInSnSbTeIXeCsBaLaCePrNdPm")]
        public void ComputeMassStressTest(string formula, bool bracketsAsParentheses = false)
        {
            mMwtWinAvg.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var resultDaAvg = mMwtWinAvg.ComputeMassExtra(formula, out var parseData);
            ReportParseData(parseData);
            Assert.Greater(resultDaAvg, 0);
        }

        [Test]
        [TestCase("^13C6H6-H2O0")]
        [TestCase("^13C6H6[.1H2O]", "", true)] // TODO: Error position/char (7/('['->'(') -> 8/'.') - outputting the '[' or '(' might not be simple in the new                                                                         // TODO: VB6: '[', new: '.'; - outputting the '[' or '(' might not be simple in the new
        [TestCase("^13C6H6[H2O]")]
        [TestCase("^13C6H6[.1H2O]2")]
        [TestCase("^13C6H6(.1H2O)2")]                                                                          // TODO: VB6: '(', new: '.'
        [TestCase("^13C6H6[.1H2O")]                                                                            // TODO: VB6: '~'(end), new: '['
        [TestCase("^13C6H6-0H2O")]
        [TestCase("^13C6H6(H2O")]
        [TestCase("^13C6H6H2O)")]
        [TestCase("^13C6H6(H2O}")] // TODO: Succeeds now, needs to fail with mismatched parentheses.
        [TestCase("C6H5>H6")]
        [TestCase("^32PheOH", "24: isotope on abbreviation")]
        [TestCase("^13C6H6-H2O.")]
        [TestCase("^13C6H6.0.1H2O", "27: multiple decimals in number")]
        [TestCase("^13C6H6-3.0.1H2O", "27: multiple decimals in number")]
        [TestCase("^13C6H6[3.0.1H2O]", "27: multiple decimals in number")]
        //[TestCase("")]
        [TestCase("C6H6>")]                                                                                     // TODO: ErrorChar: VB6 (no error), new '>'
        [TestCase(">Ca")]
        [TestCase("^13C6H6-3")]
        //[TestCase("")]
        [TestCase("CaNzOCH4(CH2)7Br")] // TODO: ErrorChar capitalization?
        [TestCase("CaNOCH4(CgH2)7Br")] // TODO: ErrorChar capitalization?
        [TestCase("F(D(CH2)7Br")]
        [TestCase("FD)(CH2)7Br")]
        [TestCase("FD(CH2}7Br")] // TODO: Succeeds now, needs to fail with mismatched parentheses.
        [TestCase("Pt(C0H2)7Br")]
        [TestCase("Pt(CH2)7Br0")]
        [TestCase("(CH2)7Br-[2CaH]5")]
        [TestCase("-[CaH]")]                                                                                   // TODO: VB6: 'C', new: '['
        [TestCase("[.CaH]")]
        [TestCase("^.CaH")]
        [TestCase("(CH2)7Br-[2CaH")]
        [TestCase("(CH2)7Br-2CaH]")]
        [TestCase("[2Ca[O]H]-")]
        [TestCase("^CaH")] // TODO: Error position/char (1/'^'  -> 1/'C')                                                                                                             // TODO: VB6: 'C', new: '^'
        [TestCase("^23CaH^")] // ErrorPosition: 7 -> 6                                                                                                                                // TODO: VB6: '~'(end), new: '^'
        [TestCase("^23CaH^6")] // TODO: Error position/char (8/'^' -> 7/'6') (old position is high by 2, should be 6)                                                                 // TODO: VB6: '~'(end), new: '6' (best would probably be ^)
        [TestCase("H2O^-23Ca")]
        [TestCase("C6H3^8D3")]
        [TestCase("C6H3^8Gly3")]
        [TestCase("C6H3^19.88.9ArPb")]
        [TestCase("C6H3^19.88Ar2Pb>Ar")]
        [TestCase("C6H6>Ca")]
        //[TestCase("")]
        [TestCase("hgly5.3leu2.2tyr0.03oh")]
        [TestCase("sipsclarkcas")]

        public void ComputeMassErrorTests(string formula, string note = "", bool bracketsAsParentheses = false)
        {
            mMwtWinAvg.BracketsTreatedAsParentheses = bracketsAsParentheses;
            var resultDaAvg = mMwtWinAvg.ComputeMassExtra(formula, out var parseData);
            ReportParseData(parseData);
            Assert.AreNotEqual(0, parseData.ErrorData.ErrorId);
        }

        [Test]
        [TestCase("bi")]
        [TestCase("bk")]
        [TestCase("bu")]
        [TestCase("cd")]
        [TestCase("cf")]
        [TestCase("co")]
        [TestCase("cs")]
        [TestCase("cu")]
        [TestCase("dy")]
        [TestCase("hf")]
        [TestCase("ho")]
        [TestCase("in")]
        [TestCase("nb")]
        [TestCase("nd")]
        [TestCase("ni")]
        [TestCase("no")]
        [TestCase("np")]
        [TestCase("os")]
        [TestCase("pd")]
        [TestCase("ph")]
        [TestCase("pu")]
        [TestCase("py")]
        [TestCase("sb")]
        [TestCase("sc")]
        [TestCase("si")]
        [TestCase("sn")]
        [TestCase("TI")]
        [TestCase("yb")]
        [TestCase("BPY")]
        [TestCase("BPy")]
        [TestCase("bpy")]
        [TestCase("cys")]
        [TestCase("his")]
        [TestCase("hoh")]
        [TestCase("hyp")]
        [TestCase("Oac")]
        [TestCase("oac")]
        [TestCase("Pro")]
        [TestCase("prO")]
        [TestCase("val")]
        [TestCase("vAl")]

        public void ComputeMassCautionMessageTests(string formula)
        {
            var resultDaAvg = mMwtWinAvg.ComputeMassExtra(formula, out var parseData);
            ReportParseData(parseData);
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
                Console.WriteLine(data.FormulaCorrected);
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
