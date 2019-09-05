using System;
using MwtWinDll;

namespace MwtWinDllTestCS
{
    /*
     * This class demonstrates how to obtain the fragmentation spectrum for a peptide sequence
     * It also demonstrates how to compute the theoretical isotopic distribution for the peptide
     * Written by Matthew Monroe for PNNL in 2010
     */

    class clsFragSpecTest
    {

        MolecularWeightCalculator mMwtWin;


        public clsFragSpecTest() {
            mMwtWin = new MolecularWeightCalculator();
        }

        public clsFragSpecTest(ref MolecularWeightCalculator objMwtWin) {
            mMwtWin = objMwtWin;
        }

        public void TestAccessFunctions() {

            // Set the element mode (average, monoisotopic, or integer)
            mMwtWin.SetElementMode(MWElementAndMassRoutines.emElementModeConstants.emIsotopicMass);

            MWPeptideClass.udtFragmentationSpectrumOptionsType udtFragSpectrumOptions = default;
            udtFragSpectrumOptions.Initialize();

            // Initialize udtFragSpectrumOptions with the defaults
            udtFragSpectrumOptions = mMwtWin.Peptide.GetFragmentationSpectrumOptions();

            // Customize the options
            udtFragSpectrumOptions.DoubleChargeIonsShow = true;
            udtFragSpectrumOptions.TripleChargeIonsShow = true;
            udtFragSpectrumOptions.DoubleChargeIonsThreshold = 400;
            udtFragSpectrumOptions.TripleChargeIonsThreshold = 400;

            udtFragSpectrumOptions.IonTypeOptions[(int)MWPeptideClass.itIonTypeConstants.itAIon].ShowIon = false;
            udtFragSpectrumOptions.IonTypeOptions[(int)MWPeptideClass.itIonTypeConstants.itBIon].ShowIon = false;

            udtFragSpectrumOptions.IonTypeOptions[(int)MWPeptideClass.itIonTypeConstants.itYIon].ShowIon = true;
            udtFragSpectrumOptions.IonTypeOptions[(int)MWPeptideClass.itIonTypeConstants.itYIon].NeutralLossAmmonia = false;
            udtFragSpectrumOptions.IonTypeOptions[(int)MWPeptideClass.itIonTypeConstants.itYIon].NeutralLossPhosphate = false;
            udtFragSpectrumOptions.IonTypeOptions[(int)MWPeptideClass.itIonTypeConstants.itYIon].NeutralLossWater = false;

            udtFragSpectrumOptions.IonTypeOptions[(int)MWPeptideClass.itIonTypeConstants.itCIon].ShowIon = false;
            udtFragSpectrumOptions.IonTypeOptions[(int)MWPeptideClass.itIonTypeConstants.itZIon].ShowIon = false;

            udtFragSpectrumOptions.IntensityOptions.BYIonShoulder = 0;

            // Customize the modification symbols
            mMwtWin.Peptide.SetModificationSymbol("!", 57.02146, false, "Carbamidomethylation");
            mMwtWin.Peptide.SetModificationSymbol("+", 10.0, false, "Heavy Arg");

            string strNewSeq = "C!ETQNPVSAR+";

            // Obtain the fragmentation spectrum for a peptide

            // First define the peptide sequence
            // Need to pass "false" to parameter blnIs3LetterCode since strNewSeq is in one-letter notation
            mMwtWin.Peptide.SetSequence1LetterSymbol(strNewSeq);

            // Update the options
            mMwtWin.Peptide.SetFragmentationSpectrumOptions(udtFragSpectrumOptions);

            // Get the fragmentation masses
            MWPeptideClass.udtFragmentationSpectrumDataType[] udtFragSpectrum = null;
            mMwtWin.Peptide.GetFragmentationMasses(ref udtFragSpectrum);

            // Print the results to the console
            Console.WriteLine("Fragmentation spectrum for " + mMwtWin.Peptide.GetSequence(false, true, false, false));
            Console.WriteLine();

            Console.WriteLine("Mass     Intensity    \tSymbol");

            for (int i = 0; i < udtFragSpectrum.Length; i++) {
                Console.WriteLine(udtFragSpectrum[i].Mass.ToString("0.000") + "  " + udtFragSpectrum[i].Intensity.ToString("###0") + "        \t" + udtFragSpectrum[i].Symbol);

                // For debugging purposes, stop after displaying 20 ions
                if (i >= 30) {
                    Console.WriteLine("...");
                    break;
                }
            }

            Console.WriteLine();

            short intSuccess = 0;
            string strResults = null;
            double[,] ConvolutedMSData2D = null;
            int ConvolutedMSDataCount = 0;

            // Compute the Isotopic distribution for the peptide
            // Need to first convert to an empirical formula
            // To do this, first obtain the peptide sequence in 3-letter notation
            string strSeq3Letter = mMwtWin.Peptide.GetSequence(true, false, false, true, false);

            // Now assing to the Compound class
            mMwtWin.Compound.SetFormula(strSeq3Letter);

            // Now convert to an empirical formula
            string s = mMwtWin.Compound.ConvertToEmpirical();

            bool blnAddProtonChargeCarrier = true;
            short intChargeState = 1;
            Console.WriteLine("Isotopic abundance test with Charge=" + intChargeState);
            intSuccess = mMwtWin.ComputeIsotopicAbundances(ref s, intChargeState, ref strResults, ref ConvolutedMSData2D, ref ConvolutedMSDataCount);
            Console.WriteLine(strResults);

            blnAddProtonChargeCarrier = false;
            intChargeState = 1;
            Console.WriteLine("Isotopic abundance test with Charge=" + intChargeState + "; do not add a proton charge carrier");
            intSuccess = mMwtWin.ComputeIsotopicAbundances(ref s, intChargeState, ref strResults, ref ConvolutedMSData2D, ref ConvolutedMSDataCount, blnAddProtonChargeCarrier);
            Console.WriteLine(strResults);

        }

    }
}
