﻿using System;
using System.Collections.Generic;
using System.Text;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.Sequence;
using NUnit.Framework;

namespace UnitTests.FunctionalTests
{
    public class MwtAPITests
    {
        private MolecularWeightTool mMwtWin;

        [OneTimeSetUp]
        public void Setup()
        {
            mMwtWin = new MolecularWeightTool();
        }

        [Test]
        public void TestAccessFunctions()
        {
            // Test Abbreviations
            var abbreviationCount = mMwtWin.GetAbbreviationCount();
            for (var index = 0; index < abbreviationCount; index++)
            {
                var result = mMwtWin.GetAbbreviation(index, out var symbol, out var formula, out var charge, out var isAminoAcid, out var oneLetterSymbol, out var comment);
                Assert.AreEqual(0, result);
                Assert.AreEqual(index, mMwtWin.GetAbbreviationId(symbol));

                result = mMwtWin.SetAbbreviation(symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment);
                Assert.AreEqual(0, result);
            }

            // Test Caution statements
            foreach (var symbol in mMwtWin.GetCautionStatementSymbols())
            {
                var result = mMwtWin.GetCautionStatement(symbol, out var statement);
                Assert.AreEqual(0, result);

                result = mMwtWin.SetCautionStatement(symbol, statement);
                Assert.AreEqual(0, result);
            }

            // Test Element access
            var elementCount = mMwtWin.GetElementCount();
            for (var atomicNumber = 1; atomicNumber <= elementCount; atomicNumber++)
            {
                var result = mMwtWin.GetElement((short)atomicNumber, out var symbol, out var mass, out var uncertainty, out var charge, out var isotopeCount);
                Assert.AreEqual(0, result);
                Assert.AreEqual(atomicNumber, mMwtWin.GetAtomicNumber(symbol));

                result = mMwtWin.SetElement(symbol, mass, uncertainty, charge, false);
                Assert.AreEqual(0, result);

                result = mMwtWin.GetElementIsotopes((short)atomicNumber, out var isotopeCount2, out var isotopeMasses, out var isotopeAbundances);
                Assert.AreEqual(isotopeCount, isotopeCount2);
                Assert.AreEqual(0, result);

                result = mMwtWin.SetElementIsotopes(symbol, isotopeCount, isotopeMasses, isotopeAbundances);
                Assert.AreEqual(0, result);
            }

            // Test Message Statements access
            var messageStatementMaxId = mMwtWin.GetMessageStatementMaxId();
            for (var index = 0; index <= messageStatementMaxId; index++)
            {
                var statement = mMwtWin.GetMessageStatement(index);

                var result = mMwtWin.SetMessageStatement(index, statement);
            }
        }

        [Test]
        public void TestMzConversion()
        {
            // Test m/z conversion
            // Switch to isotopic masses

            mMwtWin.SetElementMode(ElementMassMode.Isotopic);

            mMwtWin.Compound.SetFormula("C19H36O5NH4");
            var mass = mMwtWin.Compound.Mass;
            Console.WriteLine("Mass of " + mMwtWin.Compound.FormulaCapitalized + ": " + mass);
            for (short charge = 1; charge <= 4; charge++)
                Console.WriteLine("  m/z of " + charge + "+: " + mMwtWin.ConvoluteMass(mass, 0, charge));

            Console.WriteLine("");

            mMwtWin.Compound.SetFormula("C19H36O5NH3");
            mass = mMwtWin.Compound.Mass;
            Console.WriteLine("m/z values if we first lose a hydrogen before adding a proton");
            for (short charge = 1; charge <= 4; charge++)
                Console.WriteLine("  m/z of " + charge + "+: " + mMwtWin.ConvoluteMass(mass, 0, charge));
        }

        [Test]
        public void TestCapillaryFlowFunctions()
        {
            // Test Capillary flow functions
            var capFlow = mMwtWin.CapFlow;
            capFlow.SetAutoComputeEnabled(false);
            capFlow.SetBackPressure(2000d, UnitOfPressure.Psi);
            capFlow.SetColumnLength(40d, UnitOfLength.CM);
            capFlow.SetColumnId(50d, UnitOfLength.Microns);
            capFlow.SetSolventViscosity(0.0089d, UnitOfViscosity.Poise);
            capFlow.SetInterparticlePorosity(0.33d);
            capFlow.SetParticleDiameter(2d, UnitOfLength.Microns);
            capFlow.SetAutoComputeEnabled(true);

            Console.WriteLine("Check capillary flow calculations");
            Console.WriteLine("Linear Velocity: " + capFlow.ComputeLinearVelocity(UnitOfLinearVelocity.CmPerSec));
            Console.WriteLine("Vol flow rate:   " + capFlow.ComputeVolFlowRate(UnitOfFlowRate.NLPerMin) + "  (newly computed)");

            Console.WriteLine("Vol flow rate:   " + capFlow.GetVolFlowRate());
            Console.WriteLine("Back pressure:   " + capFlow.ComputeBackPressure(UnitOfPressure.Psi));
            Console.WriteLine("Column Length:   " + capFlow.ComputeColumnLength(UnitOfLength.CM));
            Console.WriteLine("Column ID:       " + capFlow.ComputeColumnId(UnitOfLength.Microns));
            Console.WriteLine("Column Volume:   " + capFlow.ComputeColumnVolume(UnitOfVolume.NL));
            Console.WriteLine("Dead time:       " + capFlow.ComputeDeadTime(UnitOfTime.Seconds));

            Console.WriteLine("");

            Console.WriteLine("Repeat Computations, but in a different order (should give same results)");
            Console.WriteLine("Vol flow rate:   " + capFlow.ComputeVolFlowRate(UnitOfFlowRate.NLPerMin));
            Console.WriteLine("Column ID:       " + capFlow.ComputeColumnId(UnitOfLength.Microns));
            Console.WriteLine("Back pressure:   " + capFlow.ComputeBackPressure(UnitOfPressure.Psi));
            Console.WriteLine("Column Length:   " + capFlow.ComputeColumnLength(UnitOfLength.CM));

            Console.WriteLine("");

            Console.WriteLine("Old Dead time: " + capFlow.GetDeadTime(UnitOfTime.Minutes));

            capFlow.SetAutoComputeMode(AutoComputeMode.VolFlowRateUsingDeadTime);

            capFlow.SetDeadTime(25d, UnitOfTime.Minutes);
            Console.WriteLine("Dead time is now 25.0 minutes");

            Console.WriteLine("Vol flow rate: " + capFlow.GetVolFlowRate(UnitOfFlowRate.NLPerMin) + " (auto-computed since AutoComputeMode = VolFlowRateUsingDeadTime)");

            // Confirm that auto-compute worked

            Console.WriteLine("Vol flow rate: " + capFlow.ComputeVolFlowRateUsingDeadTime(out var newPressure, UnitOfFlowRate.NLPerMin, UnitOfPressure.Psi) + "  (confirmation of computed volumetric flow rate)");
            Console.WriteLine("New pressure: " + newPressure);

            Console.WriteLine("");

            // Can set a new back pressure, but since auto-compute is on, and the
            // auto-compute mode is VolFlowRateUsingDeadTime, the pressure will get changed back to
            // the pressure needed to give a vol flow rate matching the dead time
            capFlow.SetBackPressure(2000d);
            Console.WriteLine("Pressure set to 2000 psi, but auto-compute mode is VolFlowRateUsingDeadTime, so pressure");
            Console.WriteLine("  was automatically changed back to pressure needed to give vol flow rate matching dead time");
            Console.WriteLine("Pressure is now: " + capFlow.GetBackPressure(UnitOfPressure.Psi) + " psi (thus, not 2000 as one might expect)");

            capFlow.SetAutoComputeMode(AutoComputeMode.VolFlowRate);
            Console.WriteLine("Changed auto-compute mode to VolFlowRate.  Can now set pressure to 2000 and it will stick; plus, vol flow rate gets computed.");

            capFlow.SetBackPressure(2000d, UnitOfPressure.Psi);

            // Calling GetVolFlowRate will get the new computed vol flow rate (since auto-compute is on)
            Console.WriteLine("Vol flow rate: " + capFlow.GetVolFlowRate());

            capFlow.SetMassRateSampleMass(1000d);
            capFlow.SetMassRateConcentration(1d, UnitOfConcentration.MicroMolar);
            capFlow.SetMassRateVolFlowRate(600d, UnitOfFlowRate.NLPerMin);
            capFlow.SetMassRateInjectionTime(5d, UnitOfTime.Minutes);

            Console.WriteLine("Mass flow rate: " + capFlow.GetMassFlowRate(UnitOfMassFlowRate.FmolPerSec) + " fmol/sec");
            Console.WriteLine("Moles injected: " + capFlow.GetMassRateMolesInjected(UnitOfMolarAmount.FemtoMoles) + " fmoles");

            capFlow.SetMassRateSampleMass(1234d);
            capFlow.SetMassRateConcentration(1d, UnitOfConcentration.NgPerML);

            Console.WriteLine("Computing mass flow rate for compound weighing 1234 g/mol and at 1 ng/mL concentration");
            Console.WriteLine("Mass flow rate: " + capFlow.GetMassFlowRate(UnitOfMassFlowRate.AmolPerMin) + " amol/min");
            Console.WriteLine("Moles injected: " + capFlow.GetMassRateMolesInjected(UnitOfMolarAmount.FemtoMoles) + " fmoles");

            capFlow.SetExtraColumnBroadeningLinearVelocity(4d, UnitOfLinearVelocity.CmPerMin);
            capFlow.SetExtraColumnBroadeningDiffusionCoefficient(0.0003d, UnitOfDiffusionCoefficient.CmSquaredPerMin);
            capFlow.SetExtraColumnBroadeningOpenTubeLength(5d, UnitOfLength.CM);
            capFlow.SetExtraColumnBroadeningOpenTubeId(250d, UnitOfLength.Microns);
            capFlow.SetExtraColumnBroadeningInitialPeakWidthAtBase(30d, UnitOfTime.Seconds);

            Console.WriteLine("Computing broadening for 30 second wide peak through a 250 um open tube that is 5 cm long (4 cm/min)");
            Console.WriteLine(capFlow.GetExtraColumnBroadeningResultantPeakWidth(UnitOfTime.Seconds).ToString());
        }

        [Test]
        public void TestPeptideFunctions()
        {
            // Switch to isotopic masses
            mMwtWin.SetElementMode(ElementMassMode.Isotopic);

            var peptide = mMwtWin.Peptide;
            peptide.SetSequence1LetterSymbol("K.AC!YEFGHRKACY*EFGHRK.G");
            // .SetSequence1LetterSymbol("K.ACYEFGHRKACYEFGHRK.G")

            // Can change the terminii to various standard groups
            peptide.SetNTerminusGroup(NTerminusGroupType.Carbamyl);
            peptide.SetCTerminusGroup(CTerminusGroupType.Amide);

            // Can change the terminii to any desired elements
            peptide.SetNTerminus("C2OH3"); // Acetyl group
            peptide.SetCTerminus("NH2"); // Amide group

            // Can mark third residue, Tyr, as phosphorylated
            peptide.SetResidue(3, "Tyr", true, true);

            // Can define that the * modification equals 15
            peptide.SetModificationSymbol("*", 15d, false, "");

            const string newSeq = "Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg*-Lys-Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg-Lys";
            Console.WriteLine(newSeq);
            peptide.SetSequence(newSeq);

            peptide.SetSequence("K.TQPLE*VK.-", NTerminusGroupType.HydrogenPlusProton, CTerminusGroupType.Hydroxyl, is3LetterCode: false);

            Console.WriteLine(peptide.GetSequence(true, false, true, false));
            Console.WriteLine(peptide.GetSequence(false, true, false, false));
            Console.WriteLine(peptide.GetSequence(true, false, true, true));

            peptide.SetCTerminusGroup(CTerminusGroupType.None);
            Console.WriteLine(peptide.GetSequence(true, false, true, true));

            var fragSpectrumOptions = peptide.GetFragmentationSpectrumOptions();

            fragSpectrumOptions.DoubleChargeIonsShow = true;
            fragSpectrumOptions.DoubleChargeIonsThreshold = 300f;
            fragSpectrumOptions.IntensityOptions.BYIonShoulder = 0d;

            fragSpectrumOptions.TripleChargeIonsShow = true;
            fragSpectrumOptions.TripleChargeIonsThreshold = 400f;

            fragSpectrumOptions.IonTypeOptions[(int) IonType.AIon].ShowIon = true;

            peptide.SetFragmentationSpectrumOptions(fragSpectrumOptions);

            var ionCount = peptide.GetFragmentationMasses(out var fragSpectrum);

            Console.WriteLine();
            OutputDataTable(ionCount, fragSpectrum);
        }

        [Test]
        public void TestIsotopicFunctions()
        {
            // Switch to isotopic masses
            mMwtWin.SetElementMode(ElementMassMode.Isotopic);

            // Really big formula to test with: C489 H300 F27 Fe8 N72 Ni6 O27 S9
            const short chargeState = 1;
            Console.WriteLine("Isotopic abundance test with Charge=" + chargeState);

            var formulaIn = "C1255H43O2Cl";
            var success = mMwtWin.ComputeIsotopicAbundances(ref formulaIn, chargeState, out var resultString, out var convolutedMSData2D, out var convolutedMSDataCount);
            Console.WriteLine(resultString);

            Console.WriteLine("Convert isotopic distribution to Gaussian");
            var xyVals = new List<KeyValuePair<double, double>>();
            for (var index = 0; index < convolutedMSDataCount; index++)
                xyVals.Add(new KeyValuePair<double, double>(convolutedMSData2D[index, 0], convolutedMSData2D[index, 1]));

            const int resolution = 2000;
            const double resolutionMass = 1000d;
            const int qualityFactor = 50;

            var gaussianData = mMwtWin.ConvertStickDataToGaussian2DArray(xyVals, resolution, resolutionMass, qualityFactor);

            var gaussianResults = new StringBuilder();
            gaussianResults.AppendLine("m/z" + "\t" + "Intensity");
            foreach (var point in gaussianData)
            {
                if (point.Key >= 15175d && point.Key < 15193d)
                {
                    gaussianResults.AppendLine(point.Key.ToString("0.000") + "\t" + point.Value.ToString("0.000"));
                }
            }

            Console.WriteLine(gaussianResults.ToString());
        }

        [Test]
        public void TestIsotopicFunctions2()
        {
            // Switch to isotopic masses
            mMwtWin.SetElementMode(ElementMassMode.Isotopic);

            const short chargeState = 1;
            Console.WriteLine("Isotopic abundance test with Charge=" + chargeState + "; do not add a proton charge carrier");
            var formulaIn = "C1255H43O2Cl";
            var success = mMwtWin.ComputeIsotopicAbundances(ref formulaIn, chargeState, out var resultString, out _, out _, false);
            Console.WriteLine(resultString);
        }

        private void OutputDataTable(int ionCount, FragmentationSpectrumData[] fragSpectrum)
        {
            // Output a table of data to the console

            // Create three columns, and add them to the table.
            Console.WriteLine("{0,-9} {1,9} {2,-10}", "Mass", "Intensity", "Symbol");

            // Append rows to the table.
            for (var index = 0; index < ionCount; index++)
            {
                // Populates the table.
                Console.WriteLine("{0,9:F5} {1,9} {2,-10}", fragSpectrum[index].Mass, fragSpectrum[index].Intensity, fragSpectrum[index].Symbol);
            }
        }
    }
}
