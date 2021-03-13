using System;
using MwtWinDll;
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
            mMwtWinAvg = new MolecularWeightTool(ElementAndMassTools.emElementModeConstants.emAverageMass);
            mMwtWinIso = new MolecularWeightTool(ElementAndMassTools.emElementModeConstants.emIsotopicMass);
        }

        [Test]
        [TestCase("BrCH2(CH2)7CH2Br",     286.04722000, 283.97751480)]
        [TestCase("FeCl3-6H2O",           270.29478000, 268.90488320)]
        [TestCase("Co(Bpy)(CO)4",         327.15760000, 326.98160280)]
        [TestCase("^13C6H6-.1H2O",         85.84916800,  85.84800402)]
        [TestCase("HGlyLeuTyrOH",         351.39762000, 351.17941200)]
        [TestCase("BrCH2(CH2)7CH2Br>CH8", 306.12144000, 304.04011160)]
        public void ComputeMass(string formula, double expectedAvgMass, double expectedMonoMass)
        {
            var resultDaAvg = mMwtWinAvg.ComputeMass(formula);
            var resultDaIso = mMwtWinIso.ComputeMass(formula);

            Console.WriteLine("{0,-22} -> {1,12:F8} Da (average) and  {2,12:F8} Da (isotopic)",
                formula, resultDaAvg, resultDaIso);

            Assert.AreEqual(expectedAvgMass, resultDaAvg, MATCHING_MASS_EPSILON, "Actual mass does not match expected average mass");
            Assert.AreEqual(expectedMonoMass, resultDaIso, MATCHING_MASS_EPSILON, "Actual mass does not match expected isotopic mass");
        }
    }
}
