using System.Collections.Generic;
using MolecularWeightCalculator;
using NUnit.Framework;

namespace UnitTests
{
    public class MoleMassDilutionTests : TestBase
    {
        // ReSharper disable once CommentTypo
        // Ignore Spelling: Conc

        private MoleMassDilution mMoleMassConverter;

        /// <summary>
        /// Initialize the Molecular Weight Calculator objects and results writers
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            Initialize();
            mMoleMassConverter = new MoleMassDilution();

            mTestResultWriters = new Dictionary<UnitTestWriterType, UnitTestResultWriter>();

            InitializeResultsWriter(UnitTestWriterType.UnitTestCaseWriter, "UnitTestCases_MoleMassDilution.txt");
        }

        [Test]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 0)]
        public void TestComputeFinalConcentration(
            double initialConcentration, UnitOfMoleMassConcentration initialConcentrationUnits,
            double stockSolutionVolume, UnitOfExtendedVolume stockSolutionVolumeUnits,
            UnitOfMoleMassConcentration finalConcentrationUnits,
            double dilutingSolventVolume, UnitOfExtendedVolume dilutingSolventVolumeUnits,
            double expectedResult)
        {
            var totalFinalVolume = AddVolumes(stockSolutionVolume, stockSolutionVolumeUnits, dilutingSolventVolume, dilutingSolventVolumeUnits);

            SetDilutionValues(
                initialConcentration, initialConcentrationUnits, stockSolutionVolume, stockSolutionVolumeUnits,
                1, UnitOfMoleMassConcentration.Molar, dilutingSolventVolume, dilutingSolventVolumeUnits,
                totalFinalVolume, UnitOfExtendedVolume.L,

            var result = mMoleMassConverter.ComputeDilutionFinalConcentration(finalConcentrationUnits);

            WriteUpdatedTestCase("TestComputeFinalConc",
              "[TestCase(" +
              "{0}, UnitOfMoleMassConcentration.{1}, " +
              "{2}, UnitOfExtendedVolume.{3}, " +
              "UnitOfMoleMassConcentration.{4}, " +
              "{5}, UnitOfExtendedVolume.{6}, " +
              "{9})]",
              initialConcentration, initialConcentrationUnits,
              stockSolutionVolume, stockSolutionVolumeUnits,
              finalConcentrationUnits,
              dilutingSolventVolume, dilutingSolventVolumeUnits,
              ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 0)]
        public void TestComputeInitialConcentration(
            UnitOfMoleMassConcentration initialConcentrationUnits,
            double stockSolutionVolume, UnitOfExtendedVolume stockSolutionVolumeUnits,
            double finalConcentration, UnitOfMoleMassConcentration finalConcentrationUnits,
            double dilutingSolventVolume, UnitOfExtendedVolume dilutingSolventVolumeUnits,
            double expectedResult)
        {
            var totalFinalVolume = AddVolumes(stockSolutionVolume, stockSolutionVolumeUnits, dilutingSolventVolume, dilutingSolventVolumeUnits);

            SetDilutionValues(
                1, UnitOfMoleMassConcentration.Molar, stockSolutionVolume, stockSolutionVolumeUnits,
                finalConcentration, finalConcentrationUnits, dilutingSolventVolume, dilutingSolventVolumeUnits,
                totalFinalVolume, UnitOfExtendedVolume.L,

            var result = mMoleMassConverter.ComputeDilutionInitialConcentration(initialConcentrationUnits);

            WriteUpdatedTestCase("TestComputeInitialConc",
               "[TestCase(" +
               "UnitOfMoleMassConcentration.{0}, " +
               "{1}, UnitOfExtendedVolume.{2}, " +
               "{3}, UnitOfMoleMassConcentration.{4}, " +
               "{5}, UnitOfExtendedVolume.{6}, " +
               "{9})]",
               initialConcentrationUnits,
               stockSolutionVolume, stockSolutionVolumeUnits,
               finalConcentration, finalConcentrationUnits,
               dilutingSolventVolume, dilutingSolventVolumeUnits,
               ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 0, 0)]
        public void TestComputeRequiredStockAndDilutingSolventVolumes(
            double initialConcentration, UnitOfMoleMassConcentration initialConcentrationUnits,
            UnitOfExtendedVolume stockSolutionVolumeUnits,
            double finalConcentration, UnitOfMoleMassConcentration finalConcentrationUnits,
            UnitOfExtendedVolume dilutingSolventVolumeUnits,
            double totalFinalVolume, UnitOfExtendedVolume totalFinalVolumeUnits,
            double expectedStockSolutionVolume,
            double expectedDilutionSolventVolume)
        {
            SetDilutionValues(
                initialConcentration, initialConcentrationUnits, 1, UnitOfExtendedVolume.ML,
                finalConcentration, finalConcentrationUnits, 1, UnitOfExtendedVolume.ML,
                totalFinalVolume, totalFinalVolumeUnits,

            var computedStockSolutionVolume = mMoleMassConverter.ComputeDilutionRequiredStockAndDilutingSolventVolumes(
                out var computedDilutingSolventVolume, stockSolutionVolumeUnits, dilutingSolventVolumeUnits);

            WriteUpdatedTestCase("TestComputeRequiredSolventVolumes",
               "[TestCase(" +
               "{0}, UnitOfMoleMassConcentration.{1}, " +
               "UnitOfExtendedVolume.{2}, " +
               "{3}, UnitOfMoleMassConcentration.{4}, " +
               "UnitOfExtendedVolume.{5}, " +
               "{6}, UnitOfExtendedVolume.{7}, " +
               "{8}, {9})]",
               initialConcentration, initialConcentrationUnits,
               stockSolutionVolumeUnits,
               finalConcentration, finalConcentrationUnits,
               dilutingSolventVolumeUnits,
               totalFinalVolume, totalFinalVolumeUnits,
               ValueToString(computedStockSolutionVolume, 5),
               ValueToString(computedDilutingSolventVolume, 5));

            if (mCompareValuesToExpected && expectedStockSolutionVolume > 0)
            {
                Assert.AreEqual(expectedStockSolutionVolume, computedStockSolutionVolume, 0.00001);
            }

            if (mCompareValuesToExpected && expectedDilutionSolventVolume > 0)
            {
                Assert.AreEqual(expectedDilutionSolventVolume, computedDilutingSolventVolume, 0.00001);
            }
        }

        [Test]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 0, 0)]
        public void TestComputeDilutionTotalVolume(
            double initialConcentration, UnitOfMoleMassConcentration initialConcentrationUnits,
            double stockSolutionVolume, UnitOfExtendedVolume stockSolutionVolumeUnits,
            double finalConcentration, UnitOfMoleMassConcentration finalConcentrationUnits,
            UnitOfExtendedVolume dilutingSolventVolumeUnits,
            UnitOfExtendedVolume totalFinalVolumeUnits,
            double expectedTotalFinalVolume,
            double expectedDilutionSolventVolume)
        {
            SetDilutionValues(
                initialConcentration, initialConcentrationUnits, stockSolutionVolume, stockSolutionVolumeUnits,
                finalConcentration, finalConcentrationUnits,
                1, UnitOfExtendedVolume.ML,
                1, UnitOfExtendedVolume.ML);

            var computedTotalFinalVolume = mMoleMassConverter.ComputeDilutionTotalVolume(
                out var computedDilutingSolventVolume, totalFinalVolumeUnits, dilutingSolventVolumeUnits);

            WriteUpdatedTestCase("TestComputeDilutionTotalVolume",
                "[TestCase(" +
                "{0}, UnitOfMoleMassConcentration.{1}, " +
                "{2}, UnitOfExtendedVolume.{3}, " +
                "{4}, UnitOfMoleMassConcentration.{5}, " +
                "UnitOfExtendedVolume.{6}, " +
                "UnitOfExtendedVolume.{7}, " +
                "{8}, {9})]",
                initialConcentration, initialConcentrationUnits,
                stockSolutionVolume, stockSolutionVolumeUnits,
                finalConcentration, finalConcentrationUnits,
                dilutingSolventVolumeUnits,
                totalFinalVolumeUnits,
                ValueToString(computedTotalFinalVolume, 5),
                ValueToString(computedDilutingSolventVolume, 5));

            if (mCompareValuesToExpected && expectedTotalFinalVolume > 0)
            {
                Assert.AreEqual(expectedTotalFinalVolume, computedTotalFinalVolume, 0.00001);
            }

            if (mCompareValuesToExpected && expectedDilutionSolventVolume > 0)
            {
                Assert.AreEqual(expectedDilutionSolventVolume, computedDilutingSolventVolume, 0.00001);
            }
        }

        [Test]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Moles, 0)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Millimoles, 0)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Moles, 0)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Grams, 0)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Pounds, 0)]
        [TestCase(8, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 320, 0.75, Unit.MicroLiters, 0)]
        [TestCase(8, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 320, 1, Unit.MicroLiters, 0)]
        [TestCase(8, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 320, 1.3, Unit.MicroLiters, 0)]
        public void TestComputeQuantityAmount(
            double concentration, UnitOfMoleMassConcentration concentrationUnits,
            double volume, UnitOfExtendedVolume volumeUnits,
            double massInGramsPerMole, double densityInGramsPerML,
            Unit quantityUnit, double expectedResult)
        {
            mMoleMassConverter.SetQuantityConcentration(concentration, concentrationUnits);
            mMoleMassConverter.SetQuantityVolume(volume, volumeUnits);

            mMoleMassConverter.SetSampleMass(massInGramsPerMole);
            mMoleMassConverter.SetSampleDensity(densityInGramsPerML);

            var result = mMoleMassConverter.ComputeQuantityAmount(quantityUnit);

            WriteUpdatedTestCase("TestComputeQuantityAmount",
                "[TestCase({0}, UnitOfMoleMassConcentration.{1}, {2}, UnitOfExtendedVolume.{3}, {4}, {5}, Unit.{6}, {7})]",
                concentration, concentrationUnits, volume, volumeUnits,
                massInGramsPerMole, densityInGramsPerML, quantityUnit,
                ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(500, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.Molar, 0)]
        [TestCase(500, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.MilliMolar, 0)]
        [TestCase(500, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.MgPerDL, 0)]
        [TestCase(500, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.UgPerUL, 0)]
        [TestCase(750, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 0.8, UnitOfMoleMassConcentration.Molar, 0)]
        [TestCase(750, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.Molar, 0)]
        [TestCase(750, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1.25, UnitOfMoleMassConcentration.Molar, 0)]
        [TestCase(5, Unit.Liters, 100, UnitOfExtendedVolume.ML, 300, 1.1, UnitOfMoleMassConcentration.Molar, 0)]
        [TestCase(5, Unit.Kilograms, 100, UnitOfExtendedVolume.ML, 300, 1.1, UnitOfMoleMassConcentration.Molar, 0)]
        public void TestComputeQuantityConcentration(
            double amount, Unit amountUnits,
            double volume, UnitOfExtendedVolume volumeUnits,
            double massInGramsPerMole, double densityInGramsPerML,
            UnitOfMoleMassConcentration concentrationUnit, double expectedResult)
        {
            mMoleMassConverter.SetQuantityAmount(amount, amountUnits);
            mMoleMassConverter.SetQuantityVolume(volume, volumeUnits);

            mMoleMassConverter.SetSampleMass(massInGramsPerMole);
            mMoleMassConverter.SetSampleDensity(densityInGramsPerML);

            var result = mMoleMassConverter.ComputeQuantityConcentration(concentrationUnit);

            WriteUpdatedTestCase("TestComputeQuantityConc",
                "[TestCase({0}, Unit.{1}, {2}, UnitOfExtendedVolume.{3}, {4}, {5}, UnitOfMoleMassConcentration.{6}, {7})]",
                amount, amountUnits, volume, volumeUnits,
                massInGramsPerMole, densityInGramsPerML, concentrationUnit,
                ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(5, Unit.Kilograms, 3, UnitOfMoleMassConcentration.Molar, 250, 1, UnitOfExtendedVolume.ML, 0)]
        [TestCase(5, Unit.Kilograms, 3, UnitOfMoleMassConcentration.Molar, 250, 1, UnitOfExtendedVolume.L, 0)]
        [TestCase(5, Unit.Moles, 3, UnitOfMoleMassConcentration.Molar, 250, 0.8, UnitOfExtendedVolume.L, 0)]
        [TestCase(5, Unit.Moles, 3, UnitOfMoleMassConcentration.Molar, 250, 1, UnitOfExtendedVolume.L, 0)]
        [TestCase(5, Unit.Moles, 3, UnitOfMoleMassConcentration.Molar, 250, 1.2, UnitOfExtendedVolume.L, 0)]
        [TestCase(5, Unit.Moles, 3, UnitOfMoleMassConcentration.Molar, 300, 1, UnitOfExtendedVolume.L, 0)]
        [TestCase(5, Unit.Moles, 3, UnitOfMoleMassConcentration.Molar, 350, 1, UnitOfExtendedVolume.L, 0)]
        public void TestComputeQuantityVolume(
            double amount, Unit amountUnits,
            double concentration, UnitOfMoleMassConcentration concentrationUnits,
            double massInGramsPerMole, double densityInGramsPerML,
            UnitOfExtendedVolume volumeUnit, double expectedResult)
        {
            mMoleMassConverter.SetQuantityAmount(amount, amountUnits);
            mMoleMassConverter.SetQuantityConcentration(concentration, concentrationUnits);

            mMoleMassConverter.SetSampleMass(massInGramsPerMole);
            mMoleMassConverter.SetSampleDensity(densityInGramsPerML);

            var result = mMoleMassConverter.ComputeQuantityVolume(volumeUnit);

            WriteUpdatedTestCase("TestComputeQuantityVolume",
                "[TestCase({0}, Unit.{1}, {2}, UnitOfMoleMassConcentration.{3}, {4}, {5} UnitOfExtendedVolume.{6}, {7})]",
                amount, amountUnits, concentration, concentrationUnits,
                massInGramsPerMole, densityInGramsPerML, volumeUnit,
                ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(5, Unit.Liters, 250, 1, Unit.Gallons, 0)]
        [TestCase(5, Unit.Kilograms, 250, 1, Unit.Pounds, 0)]
        [TestCase(5, Unit.Kilograms, 250, 1, Unit.MilliLiters, 0)]
        [TestCase(5, Unit.Kilograms, 250, 1, Unit.Moles, 0)]
        [TestCase(5, Unit.Kilograms, 350, 1, Unit.Moles, 0)]
        [TestCase(5, Unit.Kilograms, 450, 1, Unit.Moles, 0)]
        [TestCase(5, Unit.Kilograms, 325, 0.8, Unit.Moles, 0)]
        [TestCase(5, Unit.Kilograms, 325, 1, Unit.Moles, 0)]
        [TestCase(5, Unit.Kilograms, 325, 1.2, Unit.Moles, 0)]
        public void TestConvertAmount(
            double amountIn, Unit currentUnits,
            double massInGramsPerMole, double densityInGramsPerML,
            Unit newUnits, double expectedResult)
        {
            mMoleMassConverter.SetSampleMass(massInGramsPerMole);
            mMoleMassConverter.SetSampleDensity(densityInGramsPerML);

            var result = mMoleMassConverter.ConvertAmount(amountIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertAmount",
                "[TestCase({0}, Unit.{1}, Unit.{2}, {3})]",
                amountIn, currentUnits, newUnits, ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 150, UnitOfMoleMassConcentration.MilliMolar, 0)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 250, UnitOfMoleMassConcentration.MilliMolar, 0)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 350, UnitOfMoleMassConcentration.MilliMolar, 0)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 250, UnitOfMoleMassConcentration.NanoMolar, 0)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 250, UnitOfMoleMassConcentration.MgPerML, 0)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 250, UnitOfMoleMassConcentration.Molar, 0)]
        public void ConvertConcentration(
            double concentrationIn, UnitOfMoleMassConcentration currentUnits,
            double massInGramsPerMole,
            UnitOfMoleMassConcentration newUnits, double expectedResult)
        {
            mMoleMassConverter.SetSampleMass(massInGramsPerMole);

            var result = mMoleMassConverter.ConvertConcentration(concentrationIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertConcentration",
                "[TestCase({0}, UnitOfMoleMassConcentration.{1}, {2}, UnitOfMoleMassConcentration.{3}, {4})]",
                concentrationIn, currentUnits, massInGramsPerMole,
                newUnits, ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        private double AddVolumes(
            double volume1, UnitOfExtendedVolume units1,
            double volume2, UnitOfExtendedVolume units2,
            UnitOfExtendedVolume finalVolumeUnits = UnitOfExtendedVolume.L)
        {
            var totalVolume = mMoleMassConverter.ConvertVolumeExtended(volume1, units1, UnitOfExtendedVolume.L) +
                              mMoleMassConverter.ConvertVolumeExtended(volume2, units2, UnitOfExtendedVolume.L);

            return mMoleMassConverter.ConvertVolumeExtended(totalVolume, UnitOfExtendedVolume.L, finalVolumeUnits);
        }

        private void SetDilutionValues(
            double initialConcentration, UnitOfMoleMassConcentration initialConcentrationUnits,
            double stockSolutionVolume, UnitOfExtendedVolume stockSolutionVolumeUnits,
            double finalConcentration, UnitOfMoleMassConcentration finalConcentrationUnits,
            double dilutingSolventVolume, UnitOfExtendedVolume dilutingSolventVolumeUnits,
            double totalFinalVolume, UnitOfExtendedVolume totalFinalVolumeUnits)
        {
            mMoleMassConverter.SetDilutionInitialConcentration(initialConcentration, initialConcentrationUnits);
            mMoleMassConverter.SetDilutionVolumeStockSolution(stockSolutionVolume, stockSolutionVolumeUnits);
            mMoleMassConverter.SetDilutionFinalConcentration(finalConcentration, finalConcentrationUnits);
            mMoleMassConverter.SetDilutionVolumeDilutingSolvent(dilutingSolventVolume, dilutingSolventVolumeUnits);
            mMoleMassConverter.SetDilutionTotalFinalVolume(totalFinalVolume, totalFinalVolumeUnits);
        }
    }
}
