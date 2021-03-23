using System;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;

namespace MwtWinDllTestCS
{
    /// <summary>
    /// This is a very simple C# application that shows how to use MwtWinDll.dll
    /// </summary>
    /// <remarks>
    /// Written by Matthew Monroe for PNNL in 2010
    /// </remarks>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Instantiate the Molecular Weight Calculator
            var mwtWin = new MolecularWeightTool();

            TestMwtWinFeatures(mwtWin, ElementAndMassTools.ElementMassMode.Average);

            Console.WriteLine();
            Console.WriteLine("Percent composition");
            var percentCompositionByElement = mwtWin.Compound.GetPercentCompositionForAllElements();

            foreach (var item in percentCompositionByElement)
            {
                Console.WriteLine(item.Key + ": " + item.Value);
            }

            Console.WriteLine();
            TestMwtWinFeatures(mwtWin, ElementAndMassTools.ElementMassMode.Isotopic);

            Console.WriteLine();
            Console.WriteLine();

            var fragTest = new FragSpecTest(ref mwtWin);
            fragTest.TestAccessFunctions();

            Console.WriteLine();
        }

        private static void TestMwtWinFeatures(
            MolecularWeightTool mwtWin,
            ElementAndMassTools.ElementMassMode elementMode)
        {
            var elementModeDescription = elementMode switch
            {
                ElementAndMassTools.ElementMassMode.Average => "Average",
                ElementAndMassTools.ElementMassMode.Isotopic => "Monoisotopic",
                ElementAndMassTools.ElementMassMode.Integer => "Integer",
                _ => "Unknown",
            };

            // Set the element mode (average, monoisotopic, or integer)
            mwtWin.SetElementMode(elementMode);

            // Can simply compute the mass of a formula using ComputeMass
            var testFormula = "C6H6";
            var formulaMass = mwtWin.ComputeMass(testFormula);

            Console.WriteLine("{0} mass of {1} is {2}", elementModeDescription, testFormula, formulaMass);
            Console.WriteLine();

            // If we want to do more complex operations, need to fill mMwtWin.Compound with valid info
            // Then, can read out values from it
            mwtWin.Compound.SetFormula("Cl2PhH4OH");

            if (mwtWin.Compound.ErrorDescription.Length > 0)
            {
                Console.WriteLine("Error: " + mwtWin.Compound.ErrorDescription);
            }
            else
            {
                Console.WriteLine("Formula:           " + mwtWin.Compound.FormulaCapitalized);
                Console.WriteLine("Expand Abbrev:     " + mwtWin.Compound.ExpandAbbreviations());
                Console.WriteLine("Empirical Formula: " + mwtWin.Compound.ConvertToEmpirical());
                // Console.WriteLine("FormulaRTF: " + mwtWin.Compound.FormulaRTF);
                Console.WriteLine();
                Console.WriteLine("{0,-18} {1}", elementModeDescription + " Mass:", mwtWin.Compound.Mass);
                Console.WriteLine("Mass with StDev:   " + mwtWin.Compound.MassAndStdDevString);
                Console.WriteLine();

                mwtWin.Compound.SetFormula("Cl2PhH4OH");
                Console.WriteLine("Formula:            " + mwtWin.Compound.FormulaCapitalized);
                Console.WriteLine("CautionDescription: " + mwtWin.Compound.CautionDescription);
                Console.WriteLine();

                // ReSharper disable once StringLiteralTypo
                mwtWin.Peptide.SetSequence1LetterSymbol("FEQDGENYTGTIDGNMGAYAR");

                var oneLetterSequence = mwtWin.Peptide.GetSequence(false);
                var threeLetterSequence = mwtWin.Peptide.GetSequence(true);

                var unchargedMass = mwtWin.Peptide.GetPeptideMass();
                var twoPlusMz = mwtWin.ConvoluteMass(unchargedMass, 0, 2);
                var threePlusMz = mwtWin.ConvoluteMass(twoPlusMz, 2, 3);

                Console.WriteLine("Peptide:            " + oneLetterSequence);
                Console.WriteLine("3 letter notation:  " + threeLetterSequence);
                Console.WriteLine("{0,-18}  {1}", elementModeDescription + " Mass:", unchargedMass);
                Console.WriteLine("m/z for 2+ ion:     " + twoPlusMz);
                Console.WriteLine("m/z for 3+ ion:     " + threePlusMz);

                mwtWin.Compound.SetFormula(threeLetterSequence);
                Console.WriteLine("Empirical Formula:  " + mwtWin.Compound.ConvertToEmpirical());

                Console.WriteLine();

                testFormula = "^13c2c4h6fe";

                mwtWin.Compound.SetFormula(testFormula);
                Console.WriteLine(testFormula + " auto-capitalizes to " + mwtWin.Compound.FormulaCapitalized);
                Console.WriteLine("{0,-18}  {1}", elementModeDescription + " Mass:", mwtWin.Compound.Mass);
            }
        }
    }
}