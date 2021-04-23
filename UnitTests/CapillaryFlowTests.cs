using System;
using System.Collections.Generic;
using MolecularWeightCalculator;
using NUnit.Framework;

namespace UnitTests
{
    public class CapillaryFlowTests : TestBase
    {
        // Ignore Spelling: acetonitrile

        /// <summary>
        /// Initialize the Molecular Weight Calculator objects and results writers
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            Initialize();

            mTestResultWriters = new Dictionary<UnitTestWriterType, UnitTestResultWriter>();

            InitializeResultsWriter(UnitTestWriterType.UnitTestCaseWriter, "UnitTestCases_CapillaryFlow.txt");
        }

        [Test]
        [TestCase(50, 30, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 54.10842)]
        [TestCase(50, 30, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Torr, 2798.22105)]
        [TestCase(50, 30, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Pascals, 373064.42628)]
        [TestCase(50, 50, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 7.01245)]
        [TestCase(50, 300, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0.00541)]
        [TestCase(30, 50, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 4.20747)]
        [TestCase(40, 50, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 5.60996)]
        [TestCase(40, 50, 0.0080, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 5.04266)]
        [TestCase(75, 30, 0.0089, 750, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 60.87198)]
        [TestCase(75, 30, 0.0089, 1250, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 101.45329)]
        [TestCase(75, 30, 0.0089, 2000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 162.32527)]
        [TestCase(50, 30, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 7704.10941)]
        [TestCase(50, 30, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Torr, 398418.58251)]
        [TestCase(50, 30, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Pascals, 53117962.25692)]
        [TestCase(50, 50, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 2773.47939)]
        [TestCase(50, 300, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 77.04109)]
        [TestCase(30, 50, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 1664.08763)]
        [TestCase(40, 50, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 2218.78351)]
        [TestCase(40, 50, 0.0080, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 1994.41214)]
        [TestCase(75, 30, 0.0089, 25, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 2311.23282)]
        [TestCase(75, 30, 0.0089, 75, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 6933.69847)]
        [TestCase(75, 30, 0.0089, 150, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 13867.39693)]
        [TestCase(75, 30, 0.0089, 75, CapillaryType.PackedCapillary, 3, UnitOfPressure.Psi, 19260.27352)]
        [TestCase(75, 30, 0.0089, 75, CapillaryType.PackedCapillary, 4, UnitOfPressure.Psi, 10833.90385)]
        public void TestComputeBackPressure(
            double columnLengthCm, double columnIdMicrons,
            double viscosityPoise, double volFlowRateNanoLitersPerMinute,
            CapillaryType capillaryType, double particleDiameterMicrons,
            UnitOfPressure pressureUnits, double expectedResult)
        {
            SetCapillaryValues(
                0, columnLengthCm, columnIdMicrons, viscosityPoise,
                volFlowRateNanoLitersPerMinute, capillaryType, particleDiameterMicrons);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeBackPressure(pressureUnits);

            WriteUpdatedTestCase("TestComputeBackPressure",
                "[TestCase({0}, {1}, {2:F4}, {3}, CapillaryType.{4}, {5}, UnitOfPressure.{6}, {7})]",
                columnLengthCm, columnIdMicrons, viscosityPoise, volFlowRateNanoLitersPerMinute,
                capillaryType, particleDiameterMicrons, pressureUnits,
                ValueToString(result));

            Console.WriteLine("{0} {1} back pressure", result, pressureUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 30, 0.0089, 600, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 77.00588)]
        [TestCase(50, 30, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 51.33725)]
        [TestCase(150, 10, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 1.90138)]
        [TestCase(150, 20, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 30.42208)]
        [TestCase(3000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 50.70346)]
        [TestCase(3500, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 59.15404)]
        [TestCase(3000, 125, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 140.84294)]
        [TestCase(3000, 125, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.M, 1.408429)]
        [TestCase(3000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 3, UnitOfLength.CM, 18.25325)]
        public void TestComputeColumnLength(
            double pressurePSI, double columnIdMicrons,
            double viscosityPoise, double volFlowRateNanoLitersPerMinute,
            CapillaryType capillaryType, double particleDiameterMicrons,
            UnitOfLength lengthUnits, double expectedResult)
        {
            SetCapillaryValues(
                pressurePSI, 0, columnIdMicrons, viscosityPoise,
                volFlowRateNanoLitersPerMinute, capillaryType, particleDiameterMicrons);

            if (particleDiameterMicrons > 0)
            {
                mMonoisotopicMassCalculator.CapFlow.SetParticleDiameter(particleDiameterMicrons);
            }

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeColumnLength(lengthUnits);

            WriteUpdatedTestCase("TestComputeColumnLength",
                "[TestCase({0}, {1}, {2}, {3}, CapillaryType.{4}, {5}, UnitOfLength.{6}, {7})]",
                pressurePSI, columnIdMicrons,
                viscosityPoise, volFlowRateNanoLitersPerMinute,
                capillaryType, particleDiameterMicrons,
                lengthUnits, ValueToString(result));

            Console.WriteLine("{0} {1} long column", result, lengthUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 75, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.NL, 2208.93233)]
        [TestCase(50, 50, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.NL, 981.7477)]
        [TestCase(50, 50, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.UL, 0.98175)]
        [TestCase(100, 75, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.NL, 4417.86467)]
        [TestCase(100, 125, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.UL, 12.27185)]
        [TestCase(50, 75, CapillaryType.PackedCapillary, 5, UnitOfVolume.NL, 883.57293)]
        [TestCase(50, 50, CapillaryType.PackedCapillary, 5, UnitOfVolume.NL, 392.69908)]
        [TestCase(100, 75, CapillaryType.PackedCapillary, 5, UnitOfVolume.NL, 1767.14587)]
        [TestCase(100, 125, CapillaryType.PackedCapillary, 5, UnitOfVolume.NL, 4908.73852)]
        public void TestComputeColumnVolume(
            double columnLengthCm, double columnIdMicrons,
            CapillaryType capillaryType, double particleDiameterMicrons,
            UnitOfVolume volumeUnits, double expectedResult)
        {
            SetCapillaryValues(
                0, columnLengthCm, columnIdMicrons, 0,
                0, capillaryType, particleDiameterMicrons);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeColumnVolume(volumeUnits);

            WriteUpdatedTestCase("TestComputeColumnVolume",
                "[TestCase({0}, {1}, CapillaryType.{2}, {3}, UnitOfVolume.{4}, {5})]",
                columnLengthCm, columnIdMicrons,
                capillaryType, particleDiameterMicrons,
                volumeUnits, ValueToString(result));

            Console.WriteLine("{0} {1} column volume", result, volumeUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 50, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.Microns, 29.8027)]
        [TestCase(50, 80, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.Microns, 33.51858)]
        [TestCase(75, 50, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.Microns, 26.92978)]
        [TestCase(50, 90, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.Microns, 34.52023)]
        [TestCase(50, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.Microns, 20.46221)]
        [TestCase(3000, 50, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.Microns, 74.47791)]
        [TestCase(3000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.Microns, 91.21644)]
        [TestCase(4000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.Microns, 78.99575)]
        [TestCase(3000, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfLength.MM, 0.11776)]
        [TestCase(3000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfLength.Microns, 114.02054)]
        public void TestComputeColumnId(
            double pressurePSI, double columnLengthCm,
            double viscosityPoise, double volFlowRateNanoLitersPerMinute,
            CapillaryType capillaryType, double particleDiameterMicrons,
            UnitOfLength columnIdUnits, double expectedResult)
        {
            SetCapillaryValues(
                pressurePSI, columnLengthCm, 0, viscosityPoise,
                volFlowRateNanoLitersPerMinute, capillaryType, particleDiameterMicrons);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeColumnId(columnIdUnits);

            WriteUpdatedTestCase("TestComputeColumnId",
                "[TestCase({0}, {1}, {2}, {3}, CapillaryType.{4}, {5}, UnitOfLength.{6}, {7})]",
                pressurePSI, columnLengthCm,
                viscosityPoise, volFlowRateNanoLitersPerMinute,
                capillaryType, particleDiameterMicrons,
                columnIdUnits, ValueToString(result));

            Console.WriteLine("{0} {1} column i.d.", result, columnIdUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0.0612)]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Seconds, 3.67171)]
        [TestCase(75, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0.0408)]
        [TestCase(50, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0.19827)]
        [TestCase(50, 50, 75, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0.0612)]
        [TestCase(50, 80, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0.35248)]
        [TestCase(3000, 50, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 2.90438)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 6.53486)]
        [TestCase(4000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 4.90114)]
        [TestCase(3000, 75, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 6.53486)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfTime.Minutes, 10.21071)]
        public void TestComputeDeadTime(
            double pressurePSI, double columnLengthCm, double columnIdMicrons,
            double viscosityPoise, double volFlowRateNanoLitersPerMinute,
            CapillaryType capillaryType, double particleDiameterMicrons,
            UnitOfTime deadTimeUnits, double expectedResult)
        {
            SetCapillaryValues(
                pressurePSI, columnLengthCm, columnIdMicrons, viscosityPoise,
                volFlowRateNanoLitersPerMinute, capillaryType, particleDiameterMicrons);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeDeadTime(deadTimeUnits);

            WriteUpdatedTestCase("TestComputeDeadTime",
                "[TestCase({0}, {1}, {2}, {3}, {4}, CapillaryType.{5}, {6}, UnitOfTime.{7}, {8})]",
                pressurePSI, columnLengthCm, columnIdMicrons,
                viscosityPoise, volFlowRateNanoLitersPerMinute,
                capillaryType, particleDiameterMicrons,
                deadTimeUnits, ValueToString(result));

            Console.WriteLine("{0} {1} dead time", result, deadTimeUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, 30, UnitOfTime.Seconds, UnitOfTime.Minutes, 0.50191)]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, 30, UnitOfTime.Seconds, UnitOfTime.Seconds, 30.11452)]
        [TestCase(75, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, 30, UnitOfTime.Seconds, UnitOfTime.Minutes, 0.50127)]
        [TestCase(50, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, 30, UnitOfTime.Seconds, UnitOfTime.Seconds, 30.36949)]
        [TestCase(50, 50, 75, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, 30, UnitOfTime.Seconds, UnitOfTime.Seconds, 30.11452)]
        [TestCase(50, 80, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, 30, UnitOfTime.Seconds, UnitOfTime.Seconds, 30.29231)]
        [TestCase(3000, 50, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, 30, UnitOfTime.Seconds, UnitOfTime.Seconds, 35.02489)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, 30, UnitOfTime.Seconds, UnitOfTime.Seconds, 40.43725)]
        [TestCase(4000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, 30, UnitOfTime.Seconds, UnitOfTime.Seconds, 38.09696)]
        [TestCase(3000, 75, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, 30, UnitOfTime.Seconds, UnitOfTime.Minutes, 0.67395)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, 30, UnitOfTime.Seconds, UnitOfTime.Seconds, 45.26263)]
        public void TestComputeExtraColumnBroadening(
            double pressurePSI, double columnLengthCm, double columnIdMicrons,
            double viscosityPoise, double volFlowRateNanoLitersPerMinute,
            CapillaryType capillaryType, double particleDiameterMicrons,
            double initialPeakWidthAtBase, UnitOfTime initialPeakWidthUnits,
            UnitOfTime finalPeakWidthUnits, double expectedResult)
        {
            SetCapillaryValues(
                pressurePSI, columnLengthCm, columnIdMicrons, viscosityPoise,
                volFlowRateNanoLitersPerMinute, capillaryType, particleDiameterMicrons);

            mMonoisotopicMassCalculator.CapFlow.ComputeLinearVelocity();
            mMonoisotopicMassCalculator.CapFlow.CopyCachedValuesToExtraColumnBroadeningContainer();

            mMonoisotopicMassCalculator.CapFlow.SetExtraColumnBroadeningInitialPeakWidthAtBase(initialPeakWidthAtBase);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeExtraColumnBroadeningResultantPeakWidth(finalPeakWidthUnits);

            WriteUpdatedTestCase("TestComputeExtraColumnBroadening",
                "[TestCase({0}, {1}, {2}, {3}, {4}, CapillaryType.{5}, {6}, {7}, UnitOfTime.{8}, UnitOfTime.{9}, {10})]",
                pressurePSI, columnLengthCm, columnIdMicrons,
                viscosityPoise, volFlowRateNanoLitersPerMinute,
                capillaryType, particleDiameterMicrons,
                initialPeakWidthAtBase, initialPeakWidthUnits,
                finalPeakWidthUnits, ValueToString(result));

            Console.WriteLine("{0} {1} wide peak will broaden to {2:F3} {3} when it elutes",
                initialPeakWidthAtBase, TrimFromEnd(initialPeakWidthUnits, 1).ToLower(),
                result, finalPeakWidthUnits.ToString().ToLower());

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 13.61763)]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 13.61763)]
        [TestCase(75, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 20.42644)]
        [TestCase(50, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 7.56535)]
        [TestCase(50, 50, 75, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 13.61763)]
        [TestCase(50, 80, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 3.78267)]
        [TestCase(3000, 50, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.CmPerSec, 0.28692)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.MmPerSec, 1.91282)]
        [TestCase(4000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.MmPerSec, 2.55043)]
        [TestCase(3000, 75, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.MmPerSec, 1.91282)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfLinearVelocity.MmPerSec, 1.2242)]
        public void TestComputeLinearVelocity(
            double pressurePSI, double columnLengthCm, double columnIdMicrons,
            double viscosityPoise, double volFlowRateNanoLitersPerMinute,
            CapillaryType capillaryType, double particleDiameterMicrons,
            UnitOfLinearVelocity linearVelocityUnits, double expectedResult)
        {
            SetCapillaryValues(
                pressurePSI, columnLengthCm, columnIdMicrons, viscosityPoise,
                volFlowRateNanoLitersPerMinute, capillaryType, particleDiameterMicrons);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeLinearVelocity(linearVelocityUnits);

            WriteUpdatedTestCase("TestComputeLinearVelocity",
                "[TestCase({0}, {1}, {2}, {3}, {4}, CapillaryType.{5}, {6}, UnitOfLinearVelocity.{7}, {8})]",
                pressurePSI, columnLengthCm, columnIdMicrons,
                viscosityPoise, volFlowRateNanoLitersPerMinute,
                capillaryType, particleDiameterMicrons,
                linearVelocityUnits, ValueToString(result));

            Console.WriteLine("{0} {1} linear velocity", result, linearVelocityUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(5, 0.000004, UnitOfLinearVelocity.CmPerSec, 0.024)]
        [TestCase(5, 0.000005, UnitOfLinearVelocity.CmPerSec, 0.03)]
        [TestCase(5, 0.000006, UnitOfLinearVelocity.CmPerSec, 0.036)]
        [TestCase(5, 0.000005, UnitOfLinearVelocity.MmPerSec, 0.3)]
        [TestCase(4, 0.000005, UnitOfLinearVelocity.CmPerSec, 0.0375)]
        [TestCase(2, 0.000005, UnitOfLinearVelocity.CmPerSec, 0.075)]
        public void TestComputeOptimumLinearVelocity(
            double particleDiameterMicrons, double diffusionCoefficient,
            UnitOfLinearVelocity linearVelocityUnits, double expectedResult)
        {
            mMonoisotopicMassCalculator.CapFlow.SetParticleDiameter(particleDiameterMicrons);
            mMonoisotopicMassCalculator.CapFlow.SetExtraColumnBroadeningDiffusionCoefficient(diffusionCoefficient);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(linearVelocityUnits);

            WriteUpdatedTestCase("TestComputeOptimumLinearVelocity",
                "[TestCase({0}, {1}, UnitOfLinearVelocity.{2}, {3})]",
                particleDiameterMicrons, diffusionCoefficient, linearVelocityUnits, ValueToString(result));

            Console.WriteLine("{0} {1} optimum linear velocity", result, linearVelocityUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 36.09651)]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.NLPerMin, 36096.5059)]
        [TestCase(75, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 54.14476)]
        [TestCase(75, 100, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 27.07238)]
        [TestCase(50, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 20.05361)]
        [TestCase(500, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.MLPerMin, 0.20054)]
        [TestCase(50, 50, 75, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 36.09651)]
        [TestCase(50, 80, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 4.45636)]
        [TestCase(3000, 50, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfFlowRate.NLPerMin, 304.22076)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfFlowRate.NLPerMin, 202.81384)]
        [TestCase(4000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfFlowRate.NLPerMin, 270.41845)]
        [TestCase(3000, 75, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfFlowRate.NLPerMin, 202.81384)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfFlowRate.NLPerMin, 129.80086)]
        public void TestComputeVolFlowRate(
            double pressurePSI, double columnLengthCm, double columnIdMicrons,
            double viscosityPoise, double volFlowRateNanoLitersPerMinute,
            CapillaryType capillaryType, double particleDiameterMicrons,
            UnitOfFlowRate flowRateUnits, double expectedResult)
        {
            SetCapillaryValues(
                pressurePSI, columnLengthCm, columnIdMicrons, viscosityPoise,
                volFlowRateNanoLitersPerMinute, capillaryType, particleDiameterMicrons);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeVolFlowRate(flowRateUnits);

            WriteUpdatedTestCase("TestComputeVolFlowRate",
                "[TestCase({0}, {1}, {2}, {3}, {4}, CapillaryType.{5}, {6}, UnitOfFlowRate.{7}, {8})]",
                pressurePSI, columnLengthCm, columnIdMicrons,
                viscosityPoise, volFlowRateNanoLitersPerMinute,
                capillaryType, particleDiameterMicrons,
                flowRateUnits, ValueToString(result));

            Console.WriteLine("{0} {1} volumetric flow rate", result, flowRateUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.PmolPerMin, 60)]
        [TestCase(750, 0.4, UnitOfConcentration.MilliMolar, 300, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.PmolPerMin, 120)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.PmolPerMin, 40.0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerMin, 40000.0)]
        [TestCase(250, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.PmolPerMin, 40.0)]
        [TestCase(500, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.PmolPerMin, 40.0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 600, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.PmolPerMin, 120)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 60, UnitOfTime.Seconds, UnitOfMassFlowRate.PmolPerMin, 60)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 60, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerSec, 1000)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 60, UnitOfTime.Seconds, UnitOfMassFlowRate.PmolPerSec, 1.0)]
        public void TestComputeMassFlowRate(
            double massInGramsPerMole,
            double concentration, UnitOfConcentration concentrationUnits,
            double volFlowRate, UnitOfFlowRate volFlowRateUnits,
            double injectionTime, UnitOfTime injectionTimeUnits,
            UnitOfMassFlowRate massFlowRateUnits, double expectedResult)
        {
            mMonoisotopicMassCalculator.CapFlow.SetMassRateSampleMass(massInGramsPerMole);
            mMonoisotopicMassCalculator.CapFlow.SetMassRateConcentration(concentration, concentrationUnits);
            mMonoisotopicMassCalculator.CapFlow.SetMassRateVolFlowRate(volFlowRate, volFlowRateUnits);
            mMonoisotopicMassCalculator.CapFlow.SetMassRateInjectionTime(injectionTime, injectionTimeUnits);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeMassFlowRate(massFlowRateUnits);

            WriteUpdatedTestCase("TestComputeMassFlowRate",
                "[TestCase({0}, {1}, UnitOfConcentration.{2}, {3}, UnitOfFlowRate.{4}, {5}, UnitOfTime.{6}, UnitOfMassFlowRate.{7}, {8})]",
                massInGramsPerMole, concentration, concentrationUnits,
                volFlowRate, volFlowRateUnits,
                injectionTime, injectionTimeUnits,
                massFlowRateUnits, ValueToString(result));

            Console.WriteLine("{0} {1} mass flow rate", result, massFlowRateUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.PicoMoles, 30)]
        [TestCase(750, 0.4, UnitOfConcentration.MilliMolar, 300, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.PicoMoles, 60)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.PicoMoles, 20.0)]
        [TestCase(250, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.PicoMoles, 20.0)]
        [TestCase(500, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.PicoMoles, 20.0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 600, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.PicoMoles, 60)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 60, UnitOfTime.Seconds, UnitOfMolarAmount.PicoMoles, 60)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 60, UnitOfTime.Seconds, UnitOfMolarAmount.PicoMoles, 60)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 1, UnitOfTime.Seconds, UnitOfMolarAmount.FemtoMoles, 1000.0)]
        public void TestComputeMassRateMolesInjected(
            double massInGramsPerMole,
            double concentration, UnitOfConcentration concentrationUnits,
            double volFlowRate, UnitOfFlowRate volFlowRateUnits,
            double injectionTime, UnitOfTime injectionTimeUnits,
            UnitOfMolarAmount molarAmountUnits, double expectedResult)
        {
            mMonoisotopicMassCalculator.CapFlow.SetMassRateSampleMass(massInGramsPerMole);
            mMonoisotopicMassCalculator.CapFlow.SetMassRateConcentration(concentration, concentrationUnits);
            mMonoisotopicMassCalculator.CapFlow.SetMassRateVolFlowRate(volFlowRate, volFlowRateUnits);
            mMonoisotopicMassCalculator.CapFlow.SetMassRateInjectionTime(injectionTime, injectionTimeUnits);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeMassRateMolesInjected(molarAmountUnits);

            WriteUpdatedTestCase("TestComputeMassRateMolesInjected",
                "[TestCase({0}, {1}, UnitOfConcentration.{2}, {3}, UnitOfFlowRate.{4}, {5}, UnitOfTime.{6}, UnitOfMolarAmount.{7}, {8})]",
                massInGramsPerMole, concentration, concentrationUnits,
                volFlowRate, volFlowRateUnits,
                injectionTime, injectionTimeUnits,
                molarAmountUnits, ValueToString(result));

            Console.WriteLine("{0} {1} injected", result, molarAmountUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(5, 30, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00801)]
        [TestCase(15, 30, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00839)]
        [TestCase(25, 30, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.0088)]
        [TestCase(50, 30, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00989)]
        [TestCase(15, 25, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00916)]
        [TestCase(25, 35, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00808)]
        [TestCase(50, 40, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00835)]
        [TestCase(75, 40, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00937)]
        [TestCase(25, 23, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00996)]
        [TestCase(25, 296.15, UnitOfTemperature.Kelvin, UnitOfViscosity.Poise, 0.00993)]
        [TestCase(25, 73.4, UnitOfTemperature.Fahrenheit, UnitOfViscosity.Poise, 0.00996)]
        [TestCase(20, 23, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00972)]
        [TestCase(20, 23, UnitOfTemperature.Celsius, UnitOfViscosity.CentiPoise, 0.97227)]
        public void TestComputeMeCNViscosity(
            double percentAcetonitrile,
            double temperature,
            UnitOfTemperature temperatureUnits,
            UnitOfViscosity viscosityUnits,
            double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ComputeMeCNViscosity(percentAcetonitrile, temperature, temperatureUnits, viscosityUnits);

            WriteUpdatedTestCase("TestComputeMeCNViscosity",
                "[TestCase({0}, {1}, UnitOfTemperature.{2}, UnitOfViscosity.{3}, {4})]",
                percentAcetonitrile, temperature, temperatureUnits,
                viscosityUnits, ValueToString(result));

            var temperatureSymbol = temperatureUnits switch
            {
                UnitOfTemperature.Celsius => 'C',
                UnitOfTemperature.Kelvin => 'K',
                UnitOfTemperature.Fahrenheit => 'F',
                _ => '?'
            };

            Console.WriteLine("Viscosity of {0}% acetonitrile in methanol at {1} deg {2} is {3} {4}",
                percentAcetonitrile, temperature, temperatureSymbol,
                ValueToString(result), viscosityUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 30, 25, 0.0089, UnitOfFlowRate.NLPerMin, 848.23002)]
        [TestCase(50, 30, 25, 0.0089, UnitOfFlowRate.ULPerMin, 0.84823)]
        [TestCase(75, 30, 35, 0.0089, UnitOfFlowRate.NLPerMin, 908.81787)]
        [TestCase(50, 50, 15, 0.0089, UnitOfFlowRate.NLPerMin, 3926.99082)]
        public void TestComputeVolFlowRateUsingDeadTimeOpenTubular(
            double columnLengthCm,
            double columnIdMicrons,
            double deadTimeSeconds,
            double viscosityPoise,
            UnitOfFlowRate flowRateUnits,
            double expectedResult)
        {
            mMonoisotopicMassCalculator.CapFlow.SetColumnLength(columnLengthCm);
            mMonoisotopicMassCalculator.CapFlow.SetColumnId(columnIdMicrons);
            mMonoisotopicMassCalculator.CapFlow.SetDeadTime(deadTimeSeconds, UnitOfTime.Seconds);
            mMonoisotopicMassCalculator.CapFlow.SetSolventViscosity(viscosityPoise);
            mMonoisotopicMassCalculator.CapFlow.SetCapillaryType(CapillaryType.OpenTubularCapillary);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeVolFlowRateUsingDeadTime(out var newBackPressurePsi, flowRateUnits);

            WriteUpdatedTestCase("TestComputeVolFlowRateUsingDeadTimeOpenTubular",
                "[TestCase({0}, {1}, {2}, {3}, UnitOfFlowRate.{4}, {5})]",
                columnLengthCm, columnIdMicrons, deadTimeSeconds, viscosityPoise,
                flowRateUnits, ValueToString(result));

            Console.WriteLine("{0} cm, {1} um i.d. open-tubular column with a {2} second dead time has flow {3:F2} {4}, and back pressure {5:F1} {6}",
                columnLengthCm, columnIdMicrons, deadTimeSeconds, result, flowRateUnits, newBackPressurePsi, UnitOfPressure.Psi);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 75, 3, 0.0089, 5, UnitOfFlowRate.NLPerMin, 294.52431)]
        [TestCase(50, 75, 3, 0.0089, 5, UnitOfFlowRate.ULPerMin, 0.29452)]
        [TestCase(50, 75, 3, 0.0099, 5, UnitOfFlowRate.ULPerMin, 0.29452)]
        [TestCase(50, 75, 3, 0.0075, 5, UnitOfFlowRate.ULPerMin, 0.29452)]
        [TestCase(50, 30, 6, 0.0089, 5, UnitOfFlowRate.NLPerMin, 23.56194)]
        [TestCase(75, 30, 8, 0.0089, 5, UnitOfFlowRate.NLPerMin, 26.50719)]
        [TestCase(50, 50, 4, 0.0089, 5, UnitOfFlowRate.NLPerMin, 98.17477)]
        public void TestComputeVolFlowRateUsingDeadTimePackedCapillary(
            double columnLengthCm,
            double columnIdMicrons,
            double deadTimeMinutes,
            double viscosityPoise,
            double particleDiameterMicrons,
            UnitOfFlowRate flowRateUnits,
            double expectedResult)
        {
            SetCapillaryValues(
                0, columnLengthCm, columnIdMicrons, viscosityPoise,
                0, CapillaryType.PackedCapillary, particleDiameterMicrons);

            mMonoisotopicMassCalculator.CapFlow.SetDeadTime(deadTimeMinutes);
            mMonoisotopicMassCalculator.CapFlow.SetInterparticlePorosity(0.4d);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeVolFlowRateUsingDeadTime(out var newBackPressurePsi, flowRateUnits, UnitOfPressure.Psi);

            WriteUpdatedTestCase("TestComputeVolFlowRateUsingDeadTimePackedCapillary",
                "[TestCase({0}, {1}, {2}, {3:F4}, {4}, UnitOfFlowRate.{5}, {6})]",
                columnLengthCm, columnIdMicrons, deadTimeMinutes, viscosityPoise,
                particleDiameterMicrons, flowRateUnits, ValueToString(result));

            Console.WriteLine("{0} cm, {1} um i.d. packed capillary with {2} um particles and a {3} minute dead time has {4} {5}",
                columnLengthCm, columnIdMicrons, particleDiameterMicrons, deadTimeMinutes, result, flowRateUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(5, UnitOfConcentration.Molar, UnitOfConcentration.MilliMolar, 5000)]
        [TestCase(250, UnitOfConcentration.UgPerML, UnitOfConcentration.Molar, 3.33333E-04)]
        [TestCase(5, UnitOfConcentration.MgPerML, UnitOfConcentration.MilliMolar, 6.66667)]
        public void TestConvertConcentration(double concentrationIn, UnitOfConcentration currentUnits, UnitOfConcentration newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertConcentration(concentrationIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertConcentration",
                "[TestCase({0}, UnitOfConcentration.{1}, UnitOfConcentration.{2}, {3})]",
                concentrationIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", concentrationIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(100, UnitOfDiffusionCoefficient.CmSquaredPerSec, UnitOfDiffusionCoefficient.CmSquaredPerMin, 6000)]
        [TestCase(5000, UnitOfDiffusionCoefficient.CmSquaredPerHr, UnitOfDiffusionCoefficient.CmSquaredPerSec, 1.38889)]
        public void TestConvertDiffusionCoefficient(double diffusionCoefficientIn, UnitOfDiffusionCoefficient currentUnits, UnitOfDiffusionCoefficient newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertDiffusionCoefficient(diffusionCoefficientIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertDiffusionCoefficient",
                "[TestCase({0}, UnitOfDiffusionCoefficient.{1}, UnitOfDiffusionCoefficient.{2}, {3})]",
                diffusionCoefficientIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", diffusionCoefficientIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(80, UnitOfLength.CM, UnitOfLength.M, 0.8)]
        [TestCase(80, UnitOfLength.CM, UnitOfLength.Inches, 31.49606)]
        [TestCase(500, UnitOfLength.Inches, UnitOfLength.M, 12.7)]
        [TestCase(2, UnitOfLength.MM, UnitOfLength.Microns, 2000)]
        public void TestConvertLength(double lengthIn, UnitOfLength currentUnits, UnitOfLength newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertLength(lengthIn, currentUnits, newUnits);

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", lengthIn, currentUnits, result, newUnits);

            WriteUpdatedTestCase("TestConvertLength",
                "[TestCase({0}, UnitOfLength.{1}, UnitOfLength.{2}, {3})]",
                lengthIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", lengthIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(200, UnitOfLinearVelocity.MmPerSec, UnitOfLinearVelocity.CmPerMin, 1200)]
        [TestCase(500, UnitOfLinearVelocity.CmPerHr, UnitOfLinearVelocity.MmPerMin, 83.33333)]
        [TestCase(8, UnitOfLinearVelocity.MmPerSec, UnitOfLinearVelocity.MmPerMin, 480)]
        [TestCase(8, UnitOfLinearVelocity.MmPerSec, UnitOfLinearVelocity.MmPerHr, 28800)]
        [TestCase(8, UnitOfLinearVelocity.MmPerSec, UnitOfLinearVelocity.CmPerMin, 48)]
        public void TestConvertLinearVelocity(double linearVelocityIn, UnitOfLinearVelocity currentUnits, UnitOfLinearVelocity newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertLinearVelocity(linearVelocityIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertLinearVelocity",
                "[TestCase({0}, UnitOfLinearVelocity.{1}, UnitOfLinearVelocity.{2}, {3})]",
                linearVelocityIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", linearVelocityIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(250, UnitOfMassFlowRate.FmolPerMin, UnitOfMassFlowRate.FmolPerSec, 4.16667)]
        [TestCase(2000, UnitOfMassFlowRate.PmolPerSec, UnitOfMassFlowRate.MolesPerMin, 1.2E-07)]
        [TestCase(75, UnitOfMassFlowRate.FmolPerSec, UnitOfMassFlowRate.PmolPerMin, 4.5)]
        public void TestConvertMassFlowRate(double massFlowRateIn, UnitOfMassFlowRate currentUnits, UnitOfMassFlowRate newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertMassFlowRate(massFlowRateIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertMassFlowRate",
                "[TestCase({0}, UnitOfMassFlowRate.{1}, UnitOfMassFlowRate.{2}, {3})]",
                massFlowRateIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", massFlowRateIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(5, UnitOfMolarAmount.MilliMoles, UnitOfMolarAmount.MicroMoles, 5000)]
        [TestCase(5, UnitOfMolarAmount.MilliMoles, UnitOfMolarAmount.NanoMoles, 5000000)]
        [TestCase(750, UnitOfMolarAmount.PicoMoles, UnitOfMolarAmount.NanoMoles, 0.75)]
        [TestCase(5.87, UnitOfMolarAmount.NanoMoles, UnitOfMolarAmount.PicoMoles, 5870)]
        public void TestConvertMoles(double molesIn, UnitOfMolarAmount currentUnits, UnitOfMolarAmount newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertMoles(molesIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertMoles",
                "[TestCase({0}, UnitOfMolarAmount.{1}, UnitOfMolarAmount.{2}, {3})]",
                molesIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", molesIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(760, UnitOfPressure.Torr, UnitOfPressure.Atmospheres, 1.0)]
        [TestCase(760, UnitOfPressure.Torr, UnitOfPressure.Bar, 1.01325)]
        [TestCase(760, UnitOfPressure.Torr, UnitOfPressure.KiloPascals, 101.32472)]
        [TestCase(5320, UnitOfPressure.Pascals, UnitOfPressure.KiloPascals, 5.32)]
        [TestCase(3, UnitOfPressure.Atmospheres, UnitOfPressure.Psi, 44.08785)]
        [TestCase(760, UnitOfPressure.Torr, UnitOfPressure.DynesPerSquareCm, 1013247.2)]
        public void TestConvertPressure(double pressureIn, UnitOfPressure currentUnits, UnitOfPressure newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertPressure(pressureIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertPressure",
                "[TestCase({0}, UnitOfPressure.{1}, UnitOfPressure.{2}, {3})]",
                pressureIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", pressureIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(100, UnitOfTemperature.Celsius, UnitOfTemperature.Fahrenheit, 212)]
        [TestCase(100, UnitOfTemperature.Celsius, UnitOfTemperature.Kelvin, 373)]
        [TestCase(0, UnitOfTemperature.Celsius, UnitOfTemperature.Fahrenheit, 32)]
        [TestCase(0, UnitOfTemperature.Celsius, UnitOfTemperature.Kelvin, 273)]
        [TestCase(-80, UnitOfTemperature.Celsius, UnitOfTemperature.Kelvin, 193)]
        [TestCase(150, UnitOfTemperature.Kelvin, UnitOfTemperature.Celsius, -273)]
        [TestCase(150, UnitOfTemperature.Kelvin, UnitOfTemperature.Fahrenheit, -459.4)]
        public void TestConvertTemperature(double temperatureIn, UnitOfTemperature currentUnits, UnitOfTemperature newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertTemperature(temperatureIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertTemperature",
                "[TestCase({0}, UnitOfTemperature.{1}, UnitOfTemperature.{2}, {3})]",
                temperatureIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", temperatureIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(60, UnitOfTime.Seconds, UnitOfTime.Minutes, 1.0)]
        [TestCase(2, UnitOfTime.Minutes, UnitOfTime.Seconds, 120)]
        [TestCase(1.5, UnitOfTime.Hours, UnitOfTime.Minutes, 90)]
        [TestCase(18, UnitOfTime.Minutes, UnitOfTime.Hours, 0.3)]
        public void TestConvertTime(double timeIn, UnitOfTime currentUnits, UnitOfTime newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertTime(timeIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertTime",
                "[TestCase({0}, UnitOfTime.{1}, UnitOfTime.{2}, {3})]",
                timeIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", timeIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(0.0089, UnitOfViscosity.Poise, UnitOfViscosity.CentiPoise, 0.89)]
        [TestCase(0.89, UnitOfViscosity.CentiPoise, UnitOfViscosity.Poise, 0.0089)]
        public void TestConvertViscosity(double viscosityIn, UnitOfViscosity currentUnits, UnitOfViscosity newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertViscosity(viscosityIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertViscosity",
                "[TestCase({0}, UnitOfViscosity.{1}, UnitOfViscosity.{2}, {3})]",
                viscosityIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", viscosityIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(250, UnitOfFlowRate.NLPerMin, UnitOfFlowRate.ULPerMin, 0.25)]
        [TestCase(1.53, UnitOfFlowRate.MLPerMin, UnitOfFlowRate.ULPerMin, 1530)]
        [TestCase(18, UnitOfFlowRate.ULPerMin, UnitOfFlowRate.NLPerMin, 18000)]
        public void TestConvertVolFlowRate(double volFlowRateIn, UnitOfFlowRate currentUnits, UnitOfFlowRate newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertVolFlowRate(volFlowRateIn, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertVolFlowRate",
                "[TestCase({0}, UnitOfFlowRate.{1}, UnitOfFlowRate.{2}, {3})]",
                volFlowRateIn, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", volFlowRateIn, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(3.2, UnitOfVolume.ML, UnitOfVolume.UL, 3200)]
        [TestCase(803, UnitOfVolume.NL, UnitOfVolume.UL, 0.803)]
        [TestCase(0.03, UnitOfVolume.UL, UnitOfVolume.PL, 30000)]
        [TestCase(850, UnitOfVolume.PL, UnitOfVolume.NL, 0.85)]
        public void TestConvertVolume(double volume, UnitOfVolume currentUnits, UnitOfVolume newUnits, double expectedResult)
        {
            var result = mMonoisotopicMassCalculator.CapFlow.ConvertVolume(volume, currentUnits, newUnits);

            WriteUpdatedTestCase("TestConvertVolume",
                "[TestCase({0}, UnitOfVolume.{1}, UnitOfVolume.{2}, {3})]",
                volume, currentUnits, newUnits, ValueToString(result));

            Console.WriteLine("{0,-3} {1,-15} => {2,-3} {3,-15}", volume, currentUnits, result, newUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        private void SetCapillaryValues(
            double pressurePSI, double columnLengthCm, double columnIdMicrons, double viscosityPoise,
            double volFlowRateNanoLitersPerMinute, CapillaryType capillaryType, double particleDiameterMicrons)
        {
            if (pressurePSI > 0)
                mMonoisotopicMassCalculator.CapFlow.SetBackPressure(pressurePSI);

            if (columnLengthCm > 0)
                mMonoisotopicMassCalculator.CapFlow.SetColumnLength(columnLengthCm);

            if (columnIdMicrons > 0)
                mMonoisotopicMassCalculator.CapFlow.SetColumnId(columnIdMicrons);

            if (viscosityPoise > 0)
                mMonoisotopicMassCalculator.CapFlow.SetSolventViscosity(viscosityPoise);

            if (volFlowRateNanoLitersPerMinute > 0)
                mMonoisotopicMassCalculator.CapFlow.SetVolFlowRate(volFlowRateNanoLitersPerMinute);

            mMonoisotopicMassCalculator.CapFlow.SetCapillaryType(capillaryType);

            if (particleDiameterMicrons > 0)
                mMonoisotopicMassCalculator.CapFlow.SetParticleDiameter(particleDiameterMicrons);
        }

        /// <summary>
        /// Remove the specified number of characters from the end of the string
        /// </summary>
        /// <param name="stringOrEnum"></param>
        /// <param name="numberOfCharacters"></param>
        /// <returns>Truncated string</returns>
        private string TrimFromEnd<T>(T stringOrEnum, int numberOfCharacters)
        {
            var value = stringOrEnum.ToString();

            return numberOfCharacters >= value.Length ?
                       string.Empty :
                       value.Substring(0, value.Length - numberOfCharacters);
        }
    }
}
