using System;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class MassTests : TestBase
    {
        // Ignore Spelling: Da

        /// <summary>
        /// Initialize the Molecular Weight Calculator object
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            Initialize();
        }

        [Test]
        [TestCase(243.000, 1, 0,   241.99272351)]
        [TestCase(243.000, 1, 1,   243.00000000)]
        [TestCase(243.000, 1, 2,   122.00363825)]
        [TestCase(243.000, 1, 3,    81.67151766)]
        [TestCase(243.000, 1, 4,    61.50545737)]
        [TestCase(243.000, 4, 7,   139.28883278)]
        [TestCase(243.000, 4, 4,   243.00000000)]
        [TestCase(243.000, 4, 2,   484.99272351)]
        [TestCase(243.000, 4, 1,   968.97817053)]
        [TestCase(243.000, 0, 4,    61.75727649)]
        [TestCase(1530.000, 3, 2, 2294.49636176)]
        [TestCase(1530.000, 3, 5,  918.40291060)]
        [TestCase(1530.000, 0, 3,  511.00727649)]
        [TestCase(1530.000, 1, 3,  510.67151766)]
        public void ConvoluteMass(double currentMz, short currentCharge, short targetCharge, double expectedMz)
        {
            var resultMH = mMonoisotopicMassCalculator.ConvoluteMass(currentMz, currentCharge);
            var resultMz = mMonoisotopicMassCalculator.ConvoluteMass(currentMz, currentCharge, targetCharge);

            Console.WriteLine("{0,8:F3} m/z, charge {1,2}+ -> {2,2}+ is {3,10:F8} m/z",
                currentMz, currentCharge, targetCharge, resultMz);

            if (targetCharge == 1)
            {
                Assert.AreEqual(resultMH, resultMz, MATCHING_MASS_EPSILON,
                                "resultMH and resultMz computed conflicting values");
            }

            Assert.AreEqual(expectedMz, resultMz, MATCHING_MASS_EPSILON, "Actual m/z does not match expected m/z");
        }

        [Test]
        [TestCase(500.000, 1, 501.00739000, 501.00727649)]
        [TestCase(500.000, 2, 251.00739000, 251.00727649)]
        [TestCase(500.000, 3, 167.67405667, 167.67394316)]
        [TestCase(750.400, 2, 376.20739000, 376.20727649)]
        [TestCase(750.400, 3, 251.14072333, 251.14060982)]
        [TestCase(750.400, 4, 188.60739000, 188.60727649)]
        [TestCase(1595.987, 1, 1596.99439000, 1596.99427649)]
        [TestCase(1595.987, 2, 799.00089000, 799.00077649)]
        [TestCase(1595.987, 3, 533.00305667, 533.00294316)]
        [TestCase(14565.636, 5, 2914.13459000, 2914.13447649)]
        [TestCase(14565.636, 10, 1457.57099000, 1457.57087649)]
        [TestCase(14565.636, 15, 972.04979000, 972.04967649)]
        public void MonoisotopicMassConversion(double monoMass, short targetCharge, double expectedMzAvg, double expectedMzIso)
        {
            var resultMzAvg = mAverageMassCalculator.MonoMassToMz(monoMass, targetCharge);
            var resultMzIso = mMonoisotopicMassCalculator.MonoMassToMz(monoMass, targetCharge);

            Console.WriteLine("{0,8:F3} Da -> {1,2}+ is {2,10:F8} m/z average mass, and {3,10:F8} m/z isotopic mass",
                monoMass, targetCharge, resultMzAvg, resultMzIso);

            Assert.AreEqual(expectedMzAvg, resultMzAvg, MATCHING_MASS_EPSILON, "Actual average-mass m/z does not match expected m/z");
            Assert.AreEqual(expectedMzIso, resultMzIso, MATCHING_MASS_EPSILON, "Actual isotopic-mass m/z does not match expected m/z");
        }
    }
}
