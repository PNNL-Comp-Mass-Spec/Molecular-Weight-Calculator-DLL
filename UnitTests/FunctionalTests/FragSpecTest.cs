using System;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.Sequence;
using NUnit.Framework;

namespace UnitTests.FunctionalTests
{
    /*
     * This class demonstrates how to obtain the fragmentation spectrum for a peptide sequence
     * It also demonstrates how to compute the theoretical isotopic distribution for the peptide
     * Written by Matthew Monroe for PNNL in 2010
     */
    [TestFixture]
    public class FragSpecTest
    {
        // Ignore Spelling: frag, Arg

        private MolecularWeightTool mMwtWin;

        [OneTimeSetUp]
        public void Setup()
        {
            mMwtWin = new MolecularWeightTool(ElementMassMode.Isotopic);
        }

        [Test]
        public void TestAccessFunctions()
        {
            // Set the element mode (average, monoisotopic, or integer)
            mMwtWin.SetElementMode(ElementMassMode.Isotopic);

            // Initialize fragSpectrumOptions with the defaults
            var fragSpectrumOptions = mMwtWin.Peptide.GetFragmentationSpectrumOptions();

            // Customize the options
            fragSpectrumOptions.DoubleChargeIonsShow = true;
            fragSpectrumOptions.TripleChargeIonsShow = true;
            fragSpectrumOptions.DoubleChargeIonsThreshold = 400;
            fragSpectrumOptions.TripleChargeIonsThreshold = 400;

            fragSpectrumOptions.IonTypeOptions[(int)IonType.AIon].ShowIon = false;
            fragSpectrumOptions.IonTypeOptions[(int)IonType.BIon].ShowIon = false;

            fragSpectrumOptions.IonTypeOptions[(int)IonType.YIon].ShowIon = true;
            fragSpectrumOptions.IonTypeOptions[(int)IonType.YIon].NeutralLossAmmonia = false;
            fragSpectrumOptions.IonTypeOptions[(int)IonType.YIon].NeutralLossPhosphate = false;
            fragSpectrumOptions.IonTypeOptions[(int)IonType.YIon].NeutralLossWater = false;

            fragSpectrumOptions.IonTypeOptions[(int)IonType.CIon].ShowIon = false;
            fragSpectrumOptions.IonTypeOptions[(int)IonType.ZIon].ShowIon = false;

            fragSpectrumOptions.IntensityOptions.BYIonShoulder = 0;

            // Customize the modification symbols
            mMwtWin.Peptide.SetModificationSymbol("!", 57.02146, false, "Carbamidomethylation");
            mMwtWin.Peptide.SetModificationSymbol("+", 10.0, false, "Heavy Arg");

            // ReSharper disable once StringLiteralTypo
            const string newSeq = "C!ETQNPVSAR+";

            // Obtain the fragmentation spectrum for a peptide

            // First define the peptide sequence
            mMwtWin.Peptide.SetSequence1LetterSymbol(newSeq);

            // Update the options
            mMwtWin.Peptide.SetFragmentationSpectrumOptions(fragSpectrumOptions);

            // Get the fragmentation masses
            mMwtWin.Peptide.GetFragmentationMasses(out var fragSpectrum);

            // Print the results to the console
            Console.WriteLine("Fragmentation spectrum for " + mMwtWin.Peptide.GetSequence(false, true, false, false));
            Console.WriteLine();

            Console.WriteLine("Mass     Intensity    \tSymbol");

            for (var i = 0; i < fragSpectrum.Length; i++)
            {
                Console.WriteLine(fragSpectrum[i].Mass.ToString("0.000") + "  " + fragSpectrum[i].Intensity.ToString("###0") + "        \t" + fragSpectrum[i].Symbol);

                // For debugging purposes, stop after displaying 20 ions
                if (i >= 30)
                {
                    Console.WriteLine("...");
                    break;
                }
            }

            Console.WriteLine();

            // Compute the Isotopic distribution for the peptide
            // Need to first convert to an empirical formula
            // To do this, first obtain the peptide sequence in 3-letter notation
            var seq3Letter = mMwtWin.Peptide.GetSequence(true, false, false, true, false);

            // Now assign to the Compound class
            mMwtWin.Compound.SetFormula(seq3Letter);

            // Now convert to an empirical formula
            var s = mMwtWin.Compound.ConvertToEmpirical();

            short chargeState = 1;
            Console.WriteLine("Isotopic abundance test with Charge=" + chargeState);
            mMwtWin.ComputeIsotopicAbundances(ref s, chargeState, out var results, out _, out _);
            Console.WriteLine(results);

            const bool addProtonChargeCarrier = false;
            chargeState = 1;
            Console.WriteLine("Isotopic abundance test with Charge=" + chargeState + "; do not add a proton charge carrier");
            mMwtWin.ComputeIsotopicAbundances(ref s, chargeState, out results, out _, out _, addProtonChargeCarrier);
            Console.WriteLine(results);
        }
    }
}
