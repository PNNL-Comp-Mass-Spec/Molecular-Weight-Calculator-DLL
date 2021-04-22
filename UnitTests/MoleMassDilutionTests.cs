using System.Collections.Generic;
using MolecularWeightCalculator;
using NUnit.Framework;

namespace UnitTests
{
    public class MoleMassDilutionTests : TestBase
    {
        // Ignore Spelling: Conc

        private MoleMassDilution mMoleMassConverter;

        /// <summary>
        /// Initialize the Mole/Mass converter
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            mMoleMassConverter = new MoleMassDilution();

            mTestResultWriters = new Dictionary<UnitTestWriterType, UnitTestResultWriter>();

            InitializeResultsWriter(UnitTestWriterType.UnitTestCaseWriter, "UnitTestCases_MoleMassDilution.txt");
        }

        [Test]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 250, 2.0)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 350, 2.0)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, UnitOfMoleMassConcentration.Molar, 8, UnitOfExtendedVolume.ML, 250, 2.72727)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 6, UnitOfExtendedVolume.ML, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 250, 3.33333)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 250, 0.008)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 350, 0.00571)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, UnitOfMoleMassConcentration.MgPerML, 12, UnitOfExtendedVolume.ML, 250, 2.0)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, UnitOfMoleMassConcentration.MgPerML, 12, UnitOfExtendedVolume.ML, 350, 2.0)]
        public void TestComputeFinalConcentration(
            double initialConcentration, UnitOfMoleMassConcentration initialConcentrationUnits,
            double stockSolutionVolume, UnitOfExtendedVolume stockSolutionVolumeUnits,
            UnitOfMoleMassConcentration finalConcentrationUnits,
            double dilutingSolventVolume, UnitOfExtendedVolume dilutingSolventVolumeUnits,
            double massInGramsPerMole,
            double expectedResult)
        {
            var totalFinalVolume = AddVolumes(stockSolutionVolume, stockSolutionVolumeUnits, dilutingSolventVolume, dilutingSolventVolumeUnits);

            SetDilutionValues(
                initialConcentration, initialConcentrationUnits, stockSolutionVolume, stockSolutionVolumeUnits,
                1, UnitOfMoleMassConcentration.Molar, dilutingSolventVolume, dilutingSolventVolumeUnits,
                totalFinalVolume, UnitOfExtendedVolume.L,
                massInGramsPerMole);

            var result = mMoleMassConverter.ComputeDilutionFinalConcentration(finalConcentrationUnits);

            WriteUpdatedTestCase("TestComputeFinalConc",
              "[TestCase(" +
              "{0}, UnitOfMoleMassConcentration.{1}, " +
              "{2}, UnitOfExtendedVolume.{3}, " +
              "UnitOfMoleMassConcentration.{4}, " +
              "{5}, UnitOfExtendedVolume.{6}, " +
              "{7}, {8})]",
              initialConcentration, initialConcentrationUnits,
              stockSolutionVolume, stockSolutionVolumeUnits,
              finalConcentrationUnits,
              dilutingSolventVolume, dilutingSolventVolumeUnits,
              massInGramsPerMole,
              ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 250, 10)]
        [TestCase(UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 350, 10)]
        [TestCase(UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 3, UnitOfMoleMassConcentration.Molar, 15, UnitOfExtendedVolume.ML, 250, 18.0)]
        [TestCase(UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 250, 10)]
        [TestCase(UnitOfMoleMassConcentration.Molar, 8, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, 12, UnitOfExtendedVolume.ML, 250, 5.0)]
        [TestCase(UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, 12, UnitOfExtendedVolume.ML, 250, 2.5)]
        [TestCase(UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, 12, UnitOfExtendedVolume.ML, 350, 3.5)]
        [TestCase(UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MgPerML, 12, UnitOfExtendedVolume.ML, 250, 10)]
        [TestCase(UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MgPerML, 12, UnitOfExtendedVolume.ML, 350, 10)]
        public void TestComputeInitialConcentration(
            UnitOfMoleMassConcentration initialConcentrationUnits,
            double stockSolutionVolume, UnitOfExtendedVolume stockSolutionVolumeUnits,
            double finalConcentration, UnitOfMoleMassConcentration finalConcentrationUnits,
            double dilutingSolventVolume, UnitOfExtendedVolume dilutingSolventVolumeUnits,
            double massInGramsPerMole,
            double expectedResult)
        {
            var totalFinalVolume = AddVolumes(stockSolutionVolume, stockSolutionVolumeUnits, dilutingSolventVolume, dilutingSolventVolumeUnits);

            SetDilutionValues(
                1, UnitOfMoleMassConcentration.Molar, stockSolutionVolume, stockSolutionVolumeUnits,
                finalConcentration, finalConcentrationUnits, dilutingSolventVolume, dilutingSolventVolumeUnits,
                totalFinalVolume, UnitOfExtendedVolume.L,
                massInGramsPerMole);

            var result = mMoleMassConverter.ComputeDilutionInitialConcentration(initialConcentrationUnits);

            WriteUpdatedTestCase("TestComputeInitialConc",
               "[TestCase(" +
               "UnitOfMoleMassConcentration.{0}, " +
               "{1}, UnitOfExtendedVolume.{2}, " +
               "{3}, UnitOfMoleMassConcentration.{4}, " +
               "{5}, UnitOfExtendedVolume.{6}, " +
               "{7}, {8})]",
               initialConcentrationUnits,
               stockSolutionVolume, stockSolutionVolumeUnits,
               finalConcentration, finalConcentrationUnits,
               dilutingSolventVolume, dilutingSolventVolumeUnits,
               massInGramsPerMole,
               ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 250, 3.0, 12)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 350, 3.0, 12)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 5, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 250, 7.5, 7.5)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 15, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 250, -1.0, -1.0)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 250, 0.75, 14.25)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 350, 1.05, 13.95)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MgPerML, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 250, 3.0, 12)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MgPerML, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 350, 3.0, 12)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 250, 0.75, 14.25)]
        [TestCase(10, UnitOfMoleMassConcentration.MgPerML, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, 15, UnitOfExtendedVolume.ML, 350, 1.05, 13.95)]
        public void TestComputeRequiredStockAndDilutingSolventVolumes(
            double initialConcentration, UnitOfMoleMassConcentration initialConcentrationUnits,
            UnitOfExtendedVolume stockSolutionVolumeUnits,
            double finalConcentration, UnitOfMoleMassConcentration finalConcentrationUnits,
            UnitOfExtendedVolume dilutingSolventVolumeUnits,
            double totalFinalVolume, UnitOfExtendedVolume totalFinalVolumeUnits,
            double massInGramsPerMole,
            double expectedStockSolutionVolume,
            double expectedDilutionSolventVolume)
        {
            SetDilutionValues(
                initialConcentration, initialConcentrationUnits, 1, UnitOfExtendedVolume.ML,
                finalConcentration, finalConcentrationUnits, 1, UnitOfExtendedVolume.ML,
                totalFinalVolume, totalFinalVolumeUnits,
                massInGramsPerMole);

            var computedStockSolutionVolume = mMoleMassConverter.ComputeDilutionRequiredStockAndDilutingSolventVolumes(
                out var computedDilutingSolventVolume, stockSolutionVolumeUnits, dilutingSolventVolumeUnits);

            WriteUpdatedTestCase("TestComputeRequiredSolventVolumes",
               "[TestCase(" +
               "{0}, UnitOfMoleMassConcentration.{1}, " +
               "UnitOfExtendedVolume.{2}, " +
               "{3}, UnitOfMoleMassConcentration.{4}, " +
               "UnitOfExtendedVolume.{5}, " +
               "{6}, UnitOfExtendedVolume.{7}, " +
               "{8}, {9}, {10})]",
               initialConcentration, initialConcentrationUnits,
               stockSolutionVolumeUnits,
               finalConcentration, finalConcentrationUnits,
               dilutingSolventVolumeUnits,
               totalFinalVolume, totalFinalVolumeUnits,
               massInGramsPerMole,
               ValueToString(computedStockSolutionVolume),
               ValueToString(computedDilutingSolventVolume));

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
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 15, 12)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 350, 15, 12)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 6, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 30, 24)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 4, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 7.5, 4.5)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.Molar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 7.5, 4.5)]
        [TestCase(5, UnitOfMoleMassConcentration.MilliMolar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 7.5, 4.5)]
        [TestCase(5, UnitOfMoleMassConcentration.MilliMolar, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 350, 7.5, 4.5)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 1, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 10, 9.0)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 2, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 20, 18)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 30, 27)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 1, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 350, 7.14286, 6.14286)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 1, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 10, 9.0)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 1, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 500, 5.0, 4.0)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 1, UnitOfExtendedVolume.ML, 2, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 650, 3.84615, 2.84615)]
        [TestCase(5, UnitOfMoleMassConcentration.MilliMolar, 3, UnitOfExtendedVolume.ML, 8, UnitOfMoleMassConcentration.MilliMolar, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 1.875, -1.0)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 3, UnitOfExtendedVolume.ML, 6, UnitOfMoleMassConcentration.MgPerML, UnitOfExtendedVolume.ML, UnitOfExtendedVolume.ML, 250, 2.5, -1.0)]
        public void TestComputeDilutionTotalVolume(
            double initialConcentration, UnitOfMoleMassConcentration initialConcentrationUnits,
            double stockSolutionVolume, UnitOfExtendedVolume stockSolutionVolumeUnits,
            double finalConcentration, UnitOfMoleMassConcentration finalConcentrationUnits,
            UnitOfExtendedVolume dilutingSolventVolumeUnits,
            UnitOfExtendedVolume totalFinalVolumeUnits,
            double massInGramsPerMole,
            double expectedTotalFinalVolume,
            double expectedDilutionSolventVolume)
        {
            // Note: if initialConcentration is less than finalConcentration, this represents evaporation or sublimation
            // In this case, totalFinalVolume represents the volume that the solution must be reduced to in order to obtain the final concentration
            // computedDilutingSolventVolume will be -1 when initialConcentration < finalConcentration

            SetDilutionValues(
                initialConcentration, initialConcentrationUnits, stockSolutionVolume, stockSolutionVolumeUnits,
                finalConcentration, finalConcentrationUnits,
                1, UnitOfExtendedVolume.ML,
                1, UnitOfExtendedVolume.ML,
                massInGramsPerMole);

            var computedTotalFinalVolume = mMoleMassConverter.ComputeDilutionTotalVolume(
                out var computedDilutingSolventVolume, totalFinalVolumeUnits, dilutingSolventVolumeUnits);

            WriteUpdatedTestCase("TestComputeDilutionTotalVolume",
                "[TestCase(" +
                "{0}, UnitOfMoleMassConcentration.{1}, " +
                "{2}, UnitOfExtendedVolume.{3}, " +
                "{4}, UnitOfMoleMassConcentration.{5}, " +
                "UnitOfExtendedVolume.{6}, " +
                "UnitOfExtendedVolume.{7}, " +
                "{8}, {9}, {10})]",
                initialConcentration, initialConcentrationUnits,
                stockSolutionVolume, stockSolutionVolumeUnits,
                finalConcentration, finalConcentrationUnits,
                dilutingSolventVolumeUnits, totalFinalVolumeUnits,
                massInGramsPerMole,
                ValueToString(computedTotalFinalVolume),
                ValueToString(computedDilutingSolventVolume));

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
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Moles, 1.0)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Millimoles, 1000)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Moles, 1.0)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Grams, 250)]
        [TestCase(10, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 250, 1, Unit.Pounds, 0.55116)]
        [TestCase(8, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 320, 0.75, Unit.MicroLiters, 341333.33333)]
        [TestCase(8, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 320, 1, Unit.MicroLiters, 256000)]
        [TestCase(8, UnitOfMoleMassConcentration.Molar, 100, UnitOfExtendedVolume.ML, 320, 1.3, Unit.MicroLiters, 196923.07692)]
        public void TestComputeQuantityAmount(
            double concentration, UnitOfMoleMassConcentration concentrationUnits,
            double volume, UnitOfExtendedVolume volumeUnits,
            double massInGramsPerMole, double densityInGramsPerML,
            Unit quantityUnit, double expectedResult)
        {
            mMoleMassConverter.SetSampleMass(massInGramsPerMole);
            mMoleMassConverter.SetSampleDensity(densityInGramsPerML);

            mMoleMassConverter.SetQuantityConcentration(concentration, concentrationUnits);
            mMoleMassConverter.SetQuantityVolume(volume, volumeUnits);

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
        [TestCase(500, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.Molar, 5.0)]
        [TestCase(500, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.MilliMolar, 5000)]
        [TestCase(500, Unit.Millimoles, 500, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.MgPerML, 250)]
        [TestCase(500, Unit.Millimoles, 500, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.MgPerDL, 25000.0)]
        [TestCase(500, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.UgPerUL, 1250)]
        [TestCase(750, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 0.8, UnitOfMoleMassConcentration.Molar, 7.5)]
        [TestCase(750, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.Molar, 7.5)]
        [TestCase(750, Unit.Millimoles, 100, UnitOfExtendedVolume.ML, 250, 1.25, UnitOfMoleMassConcentration.Molar, 7.5)]
        [TestCase(5, Unit.Liters, 100, UnitOfExtendedVolume.ML, 250, 1, UnitOfMoleMassConcentration.Molar, 200)]
        [TestCase(5, Unit.Liters, 100, UnitOfExtendedVolume.ML, 250, 1.2, UnitOfMoleMassConcentration.Molar, 240)]
        [TestCase(5, Unit.Liters, 100, UnitOfExtendedVolume.ML, 250, 0.9, UnitOfMoleMassConcentration.Molar, 180)]
        [TestCase(5, Unit.Liters, 100, UnitOfExtendedVolume.ML, 350, 1, UnitOfMoleMassConcentration.Molar, 142.85714)]
        [TestCase(5, Unit.Kilograms, 100, UnitOfExtendedVolume.ML, 350, 1, UnitOfMoleMassConcentration.Molar, 142.85714)]
        [TestCase(5, Unit.Kilograms, 100, UnitOfExtendedVolume.ML, 350, 1.1, UnitOfMoleMassConcentration.Molar, 142.85714)]
        public void TestComputeQuantityConcentration(
            double amount, Unit amountUnits,
            double volume, UnitOfExtendedVolume volumeUnits,
            double massInGramsPerMole, double densityInGramsPerML,
            UnitOfMoleMassConcentration concentrationUnit, double expectedResult)
        {
            mMoleMassConverter.SetSampleMass(massInGramsPerMole);
            mMoleMassConverter.SetSampleDensity(densityInGramsPerML);

            mMoleMassConverter.SetQuantityAmount(amount, amountUnits);
            mMoleMassConverter.SetQuantityVolume(volume, volumeUnits);

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
        [TestCase(5, Unit.Kilograms, 4, UnitOfMoleMassConcentration.Molar, 250, 1, UnitOfExtendedVolume.ML, 5000)]
        [TestCase(5, Unit.Kilograms, 4, UnitOfMoleMassConcentration.Molar, 250, 1, UnitOfExtendedVolume.L, 5.0)]
        [TestCase(5, Unit.Moles, 4, UnitOfMoleMassConcentration.Molar, 250, 0.8, UnitOfExtendedVolume.L, 1.25)]
        [TestCase(5, Unit.Moles, 4, UnitOfMoleMassConcentration.Molar, 250, 1, UnitOfExtendedVolume.L, 1.25)]
        [TestCase(5, Unit.Moles, 4, UnitOfMoleMassConcentration.Molar, 250, 1.2, UnitOfExtendedVolume.L, 1.25)]
        [TestCase(5, Unit.Moles, 4, UnitOfMoleMassConcentration.Molar, 300, 1, UnitOfExtendedVolume.L, 1.25)]
        [TestCase(5, Unit.Moles, 4, UnitOfMoleMassConcentration.Molar, 350, 1, UnitOfExtendedVolume.L, 1.25)]
        [TestCase(5, Unit.Kilograms, 4, UnitOfMoleMassConcentration.Molar, 250, 0.8, UnitOfExtendedVolume.L, 5.0)]
        [TestCase(5, Unit.Kilograms, 4, UnitOfMoleMassConcentration.Molar, 250, 1, UnitOfExtendedVolume.L, 5.0)]
        [TestCase(5, Unit.Kilograms, 4, UnitOfMoleMassConcentration.Molar, 250, 1.2, UnitOfExtendedVolume.L, 5.0)]
        [TestCase(5, Unit.Kilograms, 4, UnitOfMoleMassConcentration.Molar, 300, 1, UnitOfExtendedVolume.L, 4.16667)]
        [TestCase(5, Unit.Kilograms, 4, UnitOfMoleMassConcentration.Molar, 350, 1, UnitOfExtendedVolume.L, 3.57143)]
        [TestCase(5, Unit.MilliLiters, 4, UnitOfMoleMassConcentration.Molar, 250, 0.8, UnitOfExtendedVolume.L, 0.004)]
        [TestCase(5, Unit.MilliLiters, 4, UnitOfMoleMassConcentration.Molar, 250, 1, UnitOfExtendedVolume.L, 0.005)]
        [TestCase(5, Unit.MilliLiters, 4, UnitOfMoleMassConcentration.Molar, 250, 1.2, UnitOfExtendedVolume.L, 0.006)]
        [TestCase(5, Unit.MilliLiters, 4, UnitOfMoleMassConcentration.Molar, 300, 1, UnitOfExtendedVolume.L, 0.00417)]
        [TestCase(5, Unit.MilliLiters, 4, UnitOfMoleMassConcentration.Molar, 350, 1, UnitOfExtendedVolume.L, 0.00357)]
        public void TestComputeQuantityVolume(
            double amount, Unit amountUnits,
            double concentration, UnitOfMoleMassConcentration concentrationUnits,
            double massInGramsPerMole, double densityInGramsPerML,
            UnitOfExtendedVolume volumeUnit, double expectedResult)
        {
            mMoleMassConverter.SetSampleMass(massInGramsPerMole);
            mMoleMassConverter.SetSampleDensity(densityInGramsPerML);

            mMoleMassConverter.SetQuantityAmount(amount, amountUnits);
            mMoleMassConverter.SetQuantityConcentration(concentration, concentrationUnits);

            var result = mMoleMassConverter.ComputeQuantityVolume(volumeUnit);

            WriteUpdatedTestCase("TestComputeQuantityVolume",
                "[TestCase(" +
                "{0}, Unit.{1}, " +
                "{2}, UnitOfMoleMassConcentration.{3}, " +
                "{4}, {5}, " +
                "UnitOfExtendedVolume.{6}, {7})]",
                amount, amountUnits,
                concentration, concentrationUnits,
                massInGramsPerMole, densityInGramsPerML,
                volumeUnit,
                ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(5, Unit.Liters, 250, 1, Unit.Gallons, 1.32086)]
        [TestCase(5, Unit.Kilograms, 250, 1, Unit.Pounds, 11.02311)]
        [TestCase(5, Unit.Kilograms, 250, 1, Unit.MilliLiters, 5000)]
        [TestCase(5, Unit.Kilograms, 250, 1, Unit.Moles, 20)]
        [TestCase(5, Unit.Kilograms, 350, 1, Unit.Moles, 14.28571)]
        [TestCase(5, Unit.Kilograms, 450, 1, Unit.Moles, 11.11111)]
        [TestCase(5, Unit.Kilograms, 325, 0.8, Unit.Moles, 15.38462)]
        [TestCase(5, Unit.Kilograms, 325, 1, Unit.Moles, 15.38462)]
        [TestCase(5, Unit.Kilograms, 325, 1.2, Unit.Moles, 15.38462)]
        [TestCase(5, Unit.MilliLiters, 325, 0.8, Unit.Millimoles, 12.30769)]
        [TestCase(5, Unit.MilliLiters, 325, 1, Unit.Millimoles, 15.38462)]
        [TestCase(5, Unit.MilliLiters, 325, 1.2, Unit.Millimoles, 18.46154)]
        public void TestConvertAmount(
            double amountIn, Unit currentUnits,
            double massInGramsPerMole, double densityInGramsPerML,
            Unit newUnits, double expectedResult)
        {
            mMoleMassConverter.SetSampleMass(massInGramsPerMole);
            mMoleMassConverter.SetSampleDensity(densityInGramsPerML);

            var result = mMoleMassConverter.ConvertAmount(amountIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertAmount",
                "[TestCase(" +
                "{0}, Unit.{1}, " +
                "{2}, {3}, " +
                "Unit.{4}, {5})]",
                amountIn, currentUnits,
                massInGramsPerMole, densityInGramsPerML,
                newUnits, ValueToString(result));

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 150, UnitOfMoleMassConcentration.MilliMolar, 5000)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 250, UnitOfMoleMassConcentration.MilliMolar, 5000)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 350, UnitOfMoleMassConcentration.MilliMolar, 5000)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 150, UnitOfMoleMassConcentration.MgPerML, 750)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 250, UnitOfMoleMassConcentration.MgPerML, 1250)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 350, UnitOfMoleMassConcentration.MgPerML, 1750)]
        [TestCase(0.895, UnitOfMoleMassConcentration.MicroMolar, 250, UnitOfMoleMassConcentration.NanoMolar, 895)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 250, UnitOfMoleMassConcentration.MgPerML, 1250)]
        [TestCase(5, UnitOfMoleMassConcentration.Molar, 250, UnitOfMoleMassConcentration.UgPerUL, 1250)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 250, UnitOfMoleMassConcentration.MilliMolar, 20)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 350, UnitOfMoleMassConcentration.MilliMolar, 14.28571)]
        [TestCase(5, UnitOfMoleMassConcentration.MgPerML, 450, UnitOfMoleMassConcentration.MilliMolar, 11.11111)]
        public void ConvertConcentration(
            double concentrationIn, UnitOfMoleMassConcentration currentUnits,
            double massInGramsPerMole,
            UnitOfMoleMassConcentration newUnits, double expectedResult)
        {
            mMoleMassConverter.SetSampleMass(massInGramsPerMole);

            var result = mMoleMassConverter.ConvertConcentration(concentrationIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertConcentration",
                "[TestCase(" +
                "{0}, UnitOfMoleMassConcentration.{1}, " +
                "{2}, " +
                "UnitOfMoleMassConcentration.{3}, {4})]",
                concentrationIn, currentUnits,
                massInGramsPerMole,
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
            double totalFinalVolume, UnitOfExtendedVolume totalFinalVolumeUnits,
            double massInGramsPerMole)
        {
            mMoleMassConverter.SetSampleMass(massInGramsPerMole);
            mMoleMassConverter.SetDilutionInitialConcentration(initialConcentration, initialConcentrationUnits);
            mMoleMassConverter.SetDilutionVolumeStockSolution(stockSolutionVolume, stockSolutionVolumeUnits);
            mMoleMassConverter.SetDilutionFinalConcentration(finalConcentration, finalConcentrationUnits);
            mMoleMassConverter.SetDilutionVolumeDilutingSolvent(dilutingSolventVolume, dilutingSolventVolumeUnits);
            mMoleMassConverter.SetDilutionTotalFinalVolume(totalFinalVolume, totalFinalVolumeUnits);
        }
    }
}
