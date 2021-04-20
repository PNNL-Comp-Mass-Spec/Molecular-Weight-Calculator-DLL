using System;
using System.Collections.Generic;
using System.Text;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.Sequence;
using NUnit.Framework;

namespace UnitTests.FunctionalTests
{
    public class MwtAPITests : TestBase
    {
        // Ignore Spelling: Acetyl, amol, Da, fmol, fmoles, mol, ng, terminii
        // Ignore Spelling: Ala, Arg, Cys, Glu, Gly, Lys, Phe, Tyr,

        /// <summary>
        /// Initialize the Molecular Weight Calculator object
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            Initialize();
        }

        [Test]
        public void TestAccessFunctions()
        {
            // Test Abbreviations
            var abbreviationCount = mMonoisotopicMassCalculator.GetAbbreviationCount();
            for (var index = 0; index < abbreviationCount; index++)
            {
                var getSuccess = mMonoisotopicMassCalculator.GetAbbreviation(index, out var symbol, out var formula, out var charge, out var isAminoAcid, out var oneLetterSymbol, out var comment);
                Assert.True(getSuccess, "GetAbbreviation returned false");
                Assert.AreEqual(index, mMonoisotopicMassCalculator.GetAbbreviationId(symbol));

                if (index % 5 == 0)
                    Console.WriteLine("Abbreviation at index {0,-3} is {1,-6} with formula {2}", index, symbol, formula);

                var setResult = mMonoisotopicMassCalculator.SetAbbreviation(symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment);
                Assert.AreEqual(0, setResult, "SetAbbreviation returned error code {0}", setResult);
            }

            Console.WriteLine();

            // Test Caution statements
            var cautionStatementsTested = 0;
            foreach (var symbol in mMonoisotopicMassCalculator.GetCautionStatementSymbols())
            {
                var getSuccess = mMonoisotopicMassCalculator.GetCautionStatement(symbol, out var statement);
                Assert.True(getSuccess, "GetCautionStatement returned false");
                Assert.True(statement.Length > 0, "GetCautionStatement returned an empty string for symbol {0}", symbol);

                if (cautionStatementsTested % 5 == 0)
                    Console.WriteLine("Caution statement {0,-3} {1}", cautionStatementsTested + 1 + ":", statement);

                var setResult = mMonoisotopicMassCalculator.SetCautionStatement(symbol, statement);
                Assert.AreEqual(0, setResult, "SetAbbreviation returned error code {0}", setResult);

                cautionStatementsTested++;
            }

            Console.WriteLine();

            // Test Element access
            var elementCount = mMonoisotopicMassCalculator.GetElementCount();
            for (var atomicNumber = 1; atomicNumber <= elementCount; atomicNumber++)
            {
                var getSuccess = mMonoisotopicMassCalculator.GetElement((short)atomicNumber, out var symbol, out var mass, out var uncertainty, out var charge, out var isotopeCount);
                Assert.True(getSuccess, "GetElement returned false");
                Assert.AreEqual(atomicNumber, mMonoisotopicMassCalculator.GetAtomicNumber(symbol));
                Assert.True(symbol.Length > 0, "GetElement returned symbol='' for atomic number {0}", atomicNumber);
                Assert.True(isotopeCount > 0, "GetElement returned isotopeCount 0 for element {0}", symbol);

                if (atomicNumber == 1 || atomicNumber % 10 == 0)
                    Console.WriteLine("Atomic number {0,-3} is {1,-2}, {2:F3} Da", atomicNumber, symbol, mass);

                var setSuccess = mMonoisotopicMassCalculator.SetElement(symbol, mass, uncertainty, charge, false);
                Assert.True(setSuccess, "SetElement returned false");

                var getIsotopesSuccess = mMonoisotopicMassCalculator.GetElementIsotopes((short)atomicNumber, out var isotopeCount2, out var isotopeMasses, out var isotopeAbundances);
                Assert.AreEqual(isotopeCount, isotopeCount2);
                Assert.True(getIsotopesSuccess, "GetElementIsotopes returned false");
                Assert.True(isotopeCount2 > 0, "GetElementIsotopes returned isotopeCount 0 for atomic number {0}", atomicNumber);
                Assert.True(isotopeMasses.Length > 0, "GetElementIsotopes returned empty array of isotopeMasses for atomic number {0}", atomicNumber);

                if (atomicNumber % 10 == 1)
                    Console.WriteLine("  {0,-2} has {1} isotope{2}", symbol, isotopeMasses.Length, isotopeMasses.Length == 1 ? "" : "s");

                var setIsotopesSuccess = mMonoisotopicMassCalculator.SetElementIsotopes(symbol, isotopeMasses, isotopeAbundances);
                Assert.True(setIsotopesSuccess, "SetElementIsotopes returned false");
            }

            Console.WriteLine();

            // Test Message Statements access
            var messageStatementMaxId = mMonoisotopicMassCalculator.GetMessageStatementMaxId();
            var messagesFound = 0;
            for (var index = 0; index <= messageStatementMaxId; index++)
            {
                var statement = mMonoisotopicMassCalculator.GetMessageStatement(index);
                if (index == 0)
                {
                    Assert.True(statement.Length == 0, "GetMessageStatement returned a non-empty string for index 0", index);
                    continue;
                }

                if (index < 33)
                {
                    Assert.True(statement.Length > 0, "GetMessageStatement returned an empty string for index {0}", index);
                }

                if (statement.Length == 0)
                    continue;

                messagesFound++;

                if (messagesFound == 0 || messagesFound % 5 == 0)
                    Console.WriteLine("Message {0,-5} {1}", index + ":", statement);

                var setSuccess = mMonoisotopicMassCalculator.SetMessageStatement(index, statement);
                Assert.True(setSuccess, "SetMessageStatement returned false");
            }
        }

        [Test]
        public void TestMzConversion()
        {
            // Test m/z conversion
            // Switch to isotopic masses

            mMonoisotopicMassCalculator.SetElementMode(ElementMassMode.Isotopic);

            mMonoisotopicMassCalculator.Compound.SetFormula("C19H36O5NH4");
            var mass = mMonoisotopicMassCalculator.Compound.Mass;
            Console.WriteLine("Mass of " + mMonoisotopicMassCalculator.Compound.FormulaCapitalized + ": " + mass);
            for (short charge = 1; charge <= 4; charge++)
                Console.WriteLine("  m/z of " + charge + "+: " + mMonoisotopicMassCalculator.ConvoluteMass(mass, 0, charge));

            Console.WriteLine();

            mMonoisotopicMassCalculator.Compound.SetFormula("C19H36O5NH3");
            mass = mMonoisotopicMassCalculator.Compound.Mass;
            Console.WriteLine("m/z values if we first lose a hydrogen before adding a proton");
            for (short charge = 1; charge <= 4; charge++)
                Console.WriteLine("  m/z of " + charge + "+: " + mMonoisotopicMassCalculator.ConvoluteMass(mass, 0, charge));
        }

        [Test]
        public void TestCapillaryFlowFunctions()
        {
            // Test Capillary flow functions
            var capFlow = mMonoisotopicMassCalculator.CapFlow;
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

            Console.WriteLine();

            Console.WriteLine("Repeat Computations, but in a different order (should give same results)");
            Console.WriteLine("Vol flow rate:   " + capFlow.ComputeVolFlowRate(UnitOfFlowRate.NLPerMin));
            Console.WriteLine("Column ID:       " + capFlow.ComputeColumnId(UnitOfLength.Microns));
            Console.WriteLine("Back pressure:   " + capFlow.ComputeBackPressure(UnitOfPressure.Psi));
            Console.WriteLine("Column Length:   " + capFlow.ComputeColumnLength(UnitOfLength.CM));

            Console.WriteLine();

            Console.WriteLine("Old Dead time: " + capFlow.GetDeadTime(UnitOfTime.Minutes));

            capFlow.SetAutoComputeMode(AutoComputeMode.VolFlowRateUsingDeadTime);

            capFlow.SetDeadTime(25d, UnitOfTime.Minutes);
            Console.WriteLine("Dead time is now 25.0 minutes");

            Console.WriteLine("Vol flow rate: " + capFlow.GetVolFlowRate(UnitOfFlowRate.NLPerMin) + " (auto-computed since AutoComputeMode = VolFlowRateUsingDeadTime)");

            // Confirm that auto-compute worked

            Console.WriteLine("Vol flow rate: " + capFlow.ComputeVolFlowRateUsingDeadTime(out var newPressure, UnitOfFlowRate.NLPerMin, UnitOfPressure.Psi) + "  (confirmation of computed volumetric flow rate)");
            Console.WriteLine("New pressure: " + newPressure);

            Console.WriteLine();

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
            mMonoisotopicMassCalculator.SetElementMode(ElementMassMode.Isotopic);

            var peptide = mMonoisotopicMassCalculator.Peptide;
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

            fragSpectrumOptions.IonTypeOptions[(int)IonType.AIon].ShowIon = true;

            peptide.SetFragmentationSpectrumOptions(fragSpectrumOptions);

            var ionCount = peptide.GetFragmentationMasses(out var fragSpectrum);

            Console.WriteLine();
            OutputDataTable(ionCount, fragSpectrum);
        }

        [Test]
        public void TestIsotopicFunctions()
        {
            // Switch to isotopic masses
            mMonoisotopicMassCalculator.SetElementMode(ElementMassMode.Isotopic);

            // Really big formula to test with: C489 H300 F27 Fe8 N72 Ni6 O27 S9
            const short chargeState = 1;
            Console.WriteLine("Isotopic abundance test with Charge=" + chargeState);

            var formulaIn = "C1255H43O2Cl";
            var success = mMonoisotopicMassCalculator.ComputeIsotopicAbundances(ref formulaIn, chargeState, out var resultString, out var convolutedMSData2D, out var convolutedMSDataCount);
            Console.WriteLine(resultString);

            Console.WriteLine("Convert isotopic distribution to Gaussian");
            var xyVals = new List<KeyValuePair<double, double>>();
            for (var index = 0; index < convolutedMSDataCount; index++)
                xyVals.Add(new KeyValuePair<double, double>(convolutedMSData2D[index, 0], convolutedMSData2D[index, 1]));

            const int resolution = 2000;
            const double resolutionMass = 1000d;
            const int qualityFactor = 50;

            var gaussianData = mMonoisotopicMassCalculator.ConvertStickDataToGaussian2DArray(xyVals, resolution, resolutionMass, qualityFactor);

            var gaussianResults = new StringBuilder();
            gaussianResults.AppendFormat("{0}\t{1}", "m/z", "Intensity");
            gaussianResults.AppendLine();

            foreach (var point in gaussianData)
            {
                if (point.Key >= 15175d && point.Key < 15193d)
                {
                    gaussianResults.AppendFormat("{0:F3}\t{1:F3}", point.Key, point.Value);
                    gaussianResults.AppendLine();
                }
            }

            Console.WriteLine(gaussianResults.ToString());
        }

        [Test]
        public void TestIsotopicFunctions2()
        {
            // Switch to isotopic masses
            mMonoisotopicMassCalculator.SetElementMode(ElementMassMode.Isotopic);

            const short chargeState = 1;
            Console.WriteLine("Isotopic abundance test with Charge=" + chargeState + "; do not add a proton charge carrier");
            var formulaIn = "C1255H43O2Cl";
            var success = mMonoisotopicMassCalculator.ComputeIsotopicAbundances(ref formulaIn, chargeState, out var resultString, out _, out _, false);
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
