using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class CompoundTests
    {
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
        [TestCase("H2C3H5C2HNO3CO2", "C6H8NO5")]
        public void TestEmpiricalFormula(string formulaIn, string expectedOutput)
        {
            mMwtWinAvg.Compound.Formula = formulaIn;
            var output1 = mMwtWinAvg.Compound.ConvertToEmpirical();

            mMwtWinIso.Compound.Formula = formulaIn;
            var output2 = mMwtWinIso.Compound.ConvertToEmpirical();

            Assert.AreEqual(expectedOutput, output1);
            Assert.AreEqual(expectedOutput, output2);
        }
    }
}
