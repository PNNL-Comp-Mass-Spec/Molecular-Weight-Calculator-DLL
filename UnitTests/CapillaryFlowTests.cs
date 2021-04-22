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
        [TestCase(50, 30, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0)]
        [TestCase(50, 30, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Torr, 0)]
        [TestCase(50, 30, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Pascals, 0)]
        [TestCase(50, 50, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0)]
        [TestCase(50, 300, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0)]
        [TestCase(30, 50, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0)]
        [TestCase(40, 50, 0.0089, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0)]
        [TestCase(40, 50, 0.0080, 1000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0)]
        [TestCase(75, 30, 0.0089, 750, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0)]
        [TestCase(75, 30, 0.0089, 1250, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0)]
        [TestCase(75, 30, 0.0089, 2000, CapillaryType.OpenTubularCapillary, 0, UnitOfPressure.Psi, 0)]
        [TestCase(50, 30, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 0)]
        [TestCase(50, 30, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Torr, 0)]
        [TestCase(50, 30, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Pascals, 0)]
        [TestCase(50, 50, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 0)]
        [TestCase(50, 300, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 0)]
        [TestCase(30, 50, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 0)]
        [TestCase(40, 50, 0.0089, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 0)]
        [TestCase(40, 50, 0.0080, 125, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 0)]
        [TestCase(75, 30, 0.0089, 25, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 0)]
        [TestCase(75, 30, 0.0089, 75, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 0)]
        [TestCase(75, 30, 0.0089, 150, CapillaryType.PackedCapillary, 5, UnitOfPressure.Psi, 0)]
        [TestCase(75, 30, 0.0089, 75, CapillaryType.PackedCapillary, 3, UnitOfPressure.Psi, 0)]
        [TestCase(75, 30, 0.0089, 75, CapillaryType.PackedCapillary, 4, UnitOfPressure.Psi, 0)]
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
        [TestCase(50, 30, 0.0089, 600, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 0)]
        [TestCase(50, 30, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 0)]
        [TestCase(150, 10, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 0)]
        [TestCase(150, 20, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 0)]
        [TestCase(3000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 0)]
        [TestCase(3500, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 0)]
        [TestCase(3000, 125, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 0)]
        [TestCase(3000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 3, UnitOfLength.CM, 0)]
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
        [TestCase(50, 75, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.NL, 0)]
        [TestCase(50, 50, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.NL, 0)]
        [TestCase(50, 50, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.UL, 0)]
        [TestCase(100, 75, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.NL, 0)]
        [TestCase(100, 125, CapillaryType.OpenTubularCapillary, 0, UnitOfVolume.NL, 0)]
        [TestCase(50, 75, CapillaryType.PackedCapillary, 5, UnitOfVolume.NL, 0)]
        [TestCase(50, 50, CapillaryType.PackedCapillary, 5, UnitOfVolume.NL, 0)]
        [TestCase(100, 75, CapillaryType.PackedCapillary, 5, UnitOfVolume.NL, 0)]
        [TestCase(100, 125, CapillaryType.PackedCapillary, 5, UnitOfVolume.NL, 0)]
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
        [TestCase(50, 50, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 0)]
        [TestCase(50, 50, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.M, 0)]
        [TestCase(75, 50, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 0)]
        [TestCase(50, 90, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 0)]
        [TestCase(50, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfLength.CM, 0)]
        [TestCase(3000, 50, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 0)]
        [TestCase(3000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 0)]
        [TestCase(4000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 0)]
        [TestCase(3000, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfLength.CM, 0)]
        [TestCase(3000, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfLength.CM, 0)]
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
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Seconds, 0)]
        [TestCase(75, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(50, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(50, 50, 75, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(50, 80, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(3000, 50, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 0)]
        [TestCase(4000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 0)]
        [TestCase(3000, 75, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfTime.Minutes, 0)]
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
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Seconds, 0)]
        [TestCase(75, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(50, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(50, 50, 75, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(50, 80, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfTime.Minutes, 0)]
        [TestCase(3000, 50, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 0)]
        [TestCase(4000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 0)]
        [TestCase(3000, 75, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfTime.Minutes, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfTime.Minutes, 0)]
        public void TestComputeExtraColumnBroadening(
            double pressurePSI, double columnLengthCm, double columnIdMicrons,
            double viscosityPoise, double volFlowRateNanoLitersPerMinute,
            CapillaryType capillaryType, double particleDiameterMicrons,
            UnitOfTime peakWidthUnits, double expectedResult)
        {
            SetCapillaryValues(
                pressurePSI, columnLengthCm, columnIdMicrons, viscosityPoise,
                volFlowRateNanoLitersPerMinute, capillaryType, particleDiameterMicrons);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeExtraColumnBroadeningResultantPeakWidth(peakWidthUnits);

            WriteUpdatedTestCase("TestComputeExtraColumnBroadening",
                "[TestCase({0}, {1}, {2}, {3}, {4}, CapillaryType.{5}, {6}, UnitOfTime.{7}, {8})]",
                pressurePSI, columnLengthCm, columnIdMicrons,
                viscosityPoise, volFlowRateNanoLitersPerMinute,
                capillaryType, particleDiameterMicrons,
                peakWidthUnits, ValueToString(result));

            Console.WriteLine("{0} {1} wide eluted peak", result, peakWidthUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(75, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(50, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(50, 50, 75, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(50, 80, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(3000, 50, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(4000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(3000, 75, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfLinearVelocity.CmPerSec, 0)]
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
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(75, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(50, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(50, 50, 75, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(50, 80, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(3000, 50, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(4000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(3000, 75, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfLinearVelocity.CmPerSec, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfLinearVelocity.CmPerSec, 0)]
        public void TestComputeOptimumLinearVelocity(
            double pressurePSI, double columnLengthCm, double columnIdMicrons,
            double viscosityPoise, double volFlowRateNanoLitersPerMinute,
            CapillaryType capillaryType, double particleDiameterMicrons,
            UnitOfLinearVelocity linearVelocityUnits, double expectedResult)
        {
            SetCapillaryValues(
                pressurePSI, columnLengthCm, columnIdMicrons, viscosityPoise,
                volFlowRateNanoLitersPerMinute, capillaryType, particleDiameterMicrons);

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(linearVelocityUnits);

            WriteUpdatedTestCase("TestComputeOptimumLinearVelocity",
                "[TestCase({0}, {1}, {2}, {3}, {4}, CapillaryType.{5}, {6}, UnitOfLinearVelocity.{7}, {8})]",
                pressurePSI, columnLengthCm, columnIdMicrons,
                viscosityPoise, volFlowRateNanoLitersPerMinute,
                capillaryType, particleDiameterMicrons,
                linearVelocityUnits, ValueToString(result));

            Console.WriteLine("{0} {1} optimum linear velocity", result, linearVelocityUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(50, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(75, 50, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(75, 100, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(50, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(500, 90, 75, 0.0089, 900, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.MLPerMin, 0)]
        [TestCase(50, 50, 75, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(50, 80, 50, 0.0089, 200, CapillaryType.OpenTubularCapillary, 0, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(3000, 50, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(4000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 5, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(3000, 75, 75, 0.0089, 500, CapillaryType.PackedCapillary, 5, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(3000, 75, 75, 0.0089, 300, CapillaryType.PackedCapillary, 4, UnitOfFlowRate.NLPerMin, 0)]
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
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerMin, 0)]
        [TestCase(750, 0.4, UnitOfConcentration.MilliMolar, 300, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerMin, 0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerMin, 0)]
        [TestCase(250, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerMin, 0)]
        [TestCase(500, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerMin, 0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 600, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerMin, 0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 60, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerMin, 0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 60, UnitOfTime.Seconds, UnitOfMassFlowRate.FmolPerSec, 0)]
        public void TestComputeMassFlowRate(
            double massInGramsPerMole,
            double concentration, UnitOfConcentration concentrationUnits,
            double volFlowRate, UnitOfFlowRate volFlowRateUnits,
            double injectionTime, UnitOfTime injectionTimeUnits,
            UnitOfMassFlowRate massFlowRateUnits, double expectedResult)
        {
            mMonoisotopicMassCalculator.CapFlow.SetMassRateSampleMass(massInGramsPerMole);
            mMonoisotopicMassCalculator.CapFlow.SetMassRateConcentration(concentration, concentrationUnits);
            mMonoisotopicMassCalculator.CapFlow.SetVolFlowRate(volFlowRate, volFlowRateUnits);
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
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.FemtoMoles, 0)]
        [TestCase(750, 0.4, UnitOfConcentration.MilliMolar, 300, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.FemtoMoles, 0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.FemtoMoles, 0)]
        [TestCase(250, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.FemtoMoles, 0)]
        [TestCase(500, 200, UnitOfConcentration.MicroMolar, 200, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.FemtoMoles, 0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 600, UnitOfFlowRate.NLPerMin, 30, UnitOfTime.Seconds, UnitOfMolarAmount.FemtoMoles, 0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 60, UnitOfTime.Seconds, UnitOfMolarAmount.FemtoMoles, 0)]
        [TestCase(750, 200, UnitOfConcentration.MicroMolar, 300, UnitOfFlowRate.NLPerMin, 60, UnitOfTime.Seconds, UnitOfMolarAmount.PicoMoles, 0)]
        public void TestComputeMassRateMolesInjected(
            double massInGramsPerMole,
            double concentration, UnitOfConcentration concentrationUnits,
            double volFlowRate, UnitOfFlowRate volFlowRateUnits,
            double injectionTime, UnitOfTime injectionTimeUnits,
            UnitOfMolarAmount molarAmountUnits, double expectedResult)
        {
            mMonoisotopicMassCalculator.CapFlow.SetMassRateSampleMass(massInGramsPerMole);
            mMonoisotopicMassCalculator.CapFlow.SetMassRateConcentration(concentration, concentrationUnits);
            mMonoisotopicMassCalculator.CapFlow.SetVolFlowRate(volFlowRate, volFlowRateUnits);
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
        [TestCase(25, 30, UnitOfTemperature.Celsius, UnitOfViscosity.Poise, 0.00880)]
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
        [TestCase(50, 30, 25, 0.0089, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(50, 30, 25, 0.0089, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(75, 30, 35, 0.0089, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(50, 50, 15, 0.0089, UnitOfFlowRate.NLPerMin, 0)]
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

            var result = mMonoisotopicMassCalculator.CapFlow.ComputeVolFlowRateUsingDeadTime(out var newBackPressurePsi, flowRateUnits, UnitOfPressure.Psi);

            WriteUpdatedTestCase("TestComputeVolFlowRateUsingDeadTimeOpenTubular",
                "[TestCase({0}, {1}, {2}, {3}, UnitOfFlowRate.{4}, {5})]",
                columnLengthCm, columnIdMicrons, deadTimeSeconds, viscosityPoise,
                flowRateUnits, ValueToString(result));

            Console.WriteLine("{0} cm, {1} um i.d. open-tubular column with a {2} second dead time has {3} {4}",
                columnLengthCm, columnIdMicrons, deadTimeSeconds, result, flowRateUnits);

            if (mCompareValuesToExpected && expectedResult > 0)
            {
                Assert.AreEqual(expectedResult, result, 0.00001);
            }
        }

        [Test]
        [TestCase(50, 75, 3, 0.0089, 5, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(50, 75, 3, 0.0089, 5, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(50, 75, 3, 0.0099, 5, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(50, 75, 3, 0.0075, 5, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(50, 30, 6, 0.0089, 5, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(75, 30, 8, 0.0089, 5, UnitOfFlowRate.NLPerMin, 0)]
        [TestCase(50, 50, 4, 0.0089, 5, UnitOfFlowRate.NLPerMin, 0)]
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
        [TestCase(5, UnitOfConcentration.Molar, UnitOfConcentration.MilliMolar, 0)]
        [TestCase(250, UnitOfConcentration.UgPerML, UnitOfConcentration.Molar, 0)]
        [TestCase(5, UnitOfConcentration.MgPerML, UnitOfConcentration.MilliMolar, 0)]
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
        [TestCase(100, UnitOfDiffusionCoefficient.CmSquaredPerSec, UnitOfDiffusionCoefficient.CmSquaredPerMin, 0)]
        [TestCase(5000, UnitOfDiffusionCoefficient.CmSquaredPerHr, UnitOfDiffusionCoefficient.CmSquaredPerSec, 0)]
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
        [TestCase(80, UnitOfLength.CM, UnitOfLength.M, 0)]
        [TestCase(80, UnitOfLength.CM, UnitOfLength.Inches, 0)]
        [TestCase(500, UnitOfLength.Inches, UnitOfLength.M, 0)]
        [TestCase(2, UnitOfLength.MM, UnitOfLength.Microns, 0)]
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
        [TestCase(200, UnitOfLinearVelocity.MmPerSec, UnitOfLinearVelocity.CmPerMin, 0)]
        [TestCase(500, UnitOfLinearVelocity.CmPerHr, UnitOfLinearVelocity.MmPerMin, 0)]
        [TestCase(8, UnitOfLinearVelocity.MmPerSec, UnitOfLinearVelocity.MmPerMin, 0)]
        [TestCase(8, UnitOfLinearVelocity.MmPerSec, UnitOfLinearVelocity.MmPerHr, 0)]
        [TestCase(8, UnitOfLinearVelocity.MmPerSec, UnitOfLinearVelocity.CmPerMin, 0)]
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
        [TestCase(250, UnitOfMassFlowRate.FmolPerMin, UnitOfMassFlowRate.FmolPerSec, 0)]
        [TestCase(75, UnitOfMassFlowRate.PmolPerSec, UnitOfMassFlowRate.MolesPerMin, 0)]
        [TestCase(75, UnitOfMassFlowRate.FmolPerSec, UnitOfMassFlowRate.PmolPerMin, 0)]
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
        [TestCase(5, UnitOfMolarAmount.MilliMoles, UnitOfMolarAmount.NanoMoles, 0)]
        [TestCase(750, UnitOfMolarAmount.PicoMoles, UnitOfMolarAmount.NanoMoles, 0)]
        [TestCase(5.87, UnitOfMolarAmount.NanoMoles, UnitOfMolarAmount.PicoMoles, 0)]
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
        [TestCase(760, UnitOfPressure.Torr, UnitOfPressure.Atmospheres, 0)]
        [TestCase(760, UnitOfPressure.Torr, UnitOfPressure.Bar, 0)]
        [TestCase(760, UnitOfPressure.Torr, UnitOfPressure.KiloPascals, 0)]
        [TestCase(5320, UnitOfPressure.Pascals, UnitOfPressure.KiloPascals, 0)]
        [TestCase(3, UnitOfPressure.Atmospheres, UnitOfPressure.Psi, 0)]
        [TestCase(760, UnitOfPressure.Torr, UnitOfPressure.DynesPerSquareCm, 0)]
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
        [TestCase(100, UnitOfTemperature.Celsius, UnitOfTemperature.Fahrenheit, 0)]
        [TestCase(100, UnitOfTemperature.Celsius, UnitOfTemperature.Kelvin, 0)]
        [TestCase(0, UnitOfTemperature.Celsius, UnitOfTemperature.Fahrenheit, 0)]
        [TestCase(0, UnitOfTemperature.Celsius, UnitOfTemperature.Kelvin, 0)]
        [TestCase(-80, UnitOfTemperature.Celsius, UnitOfTemperature.Kelvin, 0)]
        [TestCase(150, UnitOfTemperature.Kelvin, UnitOfTemperature.Celsius, 0)]
        [TestCase(150, UnitOfTemperature.Kelvin, UnitOfTemperature.Fahrenheit, 0)]
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
        [TestCase(60, UnitOfTime.Seconds, UnitOfTime.Minutes, 1)]
        [TestCase(2, UnitOfTime.Minutes, UnitOfTime.Seconds, 120)]
        [TestCase(1.5, UnitOfTime.Hours, UnitOfTime.Minutes, 90)]
        [TestCase(18, UnitOfTime.Minutes, UnitOfTime.Hours, 0)]
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
        [TestCase(0.0089, UnitOfViscosity.Poise, UnitOfViscosity.CentiPoise, 0)]
        [TestCase(0.89, UnitOfViscosity.CentiPoise, UnitOfViscosity.Poise, 0)]
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
        [TestCase(250, UnitOfFlowRate.NLPerMin, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(1.53, UnitOfFlowRate.MLPerMin, UnitOfFlowRate.ULPerMin, 0)]
        [TestCase(18, UnitOfFlowRate.ULPerMin, UnitOfFlowRate.NLPerMin, 0)]
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
        [TestCase(3.2, UnitOfVolume.ML, UnitOfVolume.UL, 0)]
        [TestCase(803, UnitOfVolume.NL, UnitOfVolume.UL, 0)]
        [TestCase(0.03, UnitOfVolume.UL, UnitOfVolume.PL, 0)]
        [TestCase(850, UnitOfVolume.PL, UnitOfVolume.NL, 0)]
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
    }
}
