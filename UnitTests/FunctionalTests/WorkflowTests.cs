using System;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using NUnit.Framework;

namespace UnitTests.FunctionalTests
{
    /// <summary>
    /// This was originally a very simple C# application that showed how to use massCalculatorDll.dll
    /// Converted to a "Unit Test" to make it easier to run and inspect results from Visual Studio.
    /// </summary>
    /// <remarks>
    /// Written by Matthew Monroe for PNNL in 2010
    /// </remarks>
    public class WorkflowTests
    {
        [Test]
        [TestCase(ElementMassMode.Average)]
        [TestCase(ElementMassMode.Isotopic)]
        public void TestMassCalculatorFeatures(ElementMassMode elementMode)
        {
            // Instantiate the Molecular Weight Calculator
            var massCalculator = new MolecularWeightTool(elementMode);
            var elementModeDescription = elementMode switch
            {
                ElementMassMode.Average => "Average",
                ElementMassMode.Isotopic => "Monoisotopic",
                ElementMassMode.Integer => "Integer",
                _ => "Unknown",
            };

            // Set the element mode (average, monoisotopic, or integer)
            massCalculator.SetElementMode(elementMode);

            // Can simply compute the mass of a formula using ComputeMass
            var testFormula = "C6H6";
            var formulaMass = massCalculator.ComputeMass(testFormula);

            Console.WriteLine("{0} mass of {1} is {2}", elementModeDescription, testFormula, formulaMass);
            Console.WriteLine();

            // If we want to do more complex operations, need to fill massCalculator.Compound with valid info
            // Then, can read out values from it
            massCalculator.Compound.SetFormula("Cl2PhH4OH");

            if (massCalculator.Compound.ErrorDescription.Length > 0)
            {
                Console.WriteLine("Error: " + massCalculator.Compound.ErrorDescription);
            }
            else
            {
                Console.WriteLine("Formula:           " + massCalculator.Compound.FormulaCapitalized);
                Console.WriteLine("Expand Abbrev:     " + massCalculator.Compound.ExpandAbbreviations());
                Console.WriteLine("Empirical Formula: " + massCalculator.Compound.ConvertToEmpirical());
                // Console.WriteLine("FormulaRTF: " + massCalculator.Compound.FormulaRTF);
                Console.WriteLine();
                Console.WriteLine("{0,-18} {1}", elementModeDescription + " Mass:", massCalculator.Compound.Mass);
                Console.WriteLine("Mass with StDev:   " + massCalculator.Compound.MassAndStdDevString);
                Console.WriteLine();

                massCalculator.Compound.SetFormula("Cl2PhH4OH");
                Console.WriteLine("Formula:            " + massCalculator.Compound.FormulaCapitalized);
                Console.WriteLine("CautionDescription: " + massCalculator.Compound.CautionDescription);
                Console.WriteLine();

                // ReSharper disable once StringLiteralTypo
                massCalculator.Peptide.SetSequence1LetterSymbol("FEQDGENYTGTIDGNMGAYAR");

                var oneLetterSequence = massCalculator.Peptide.GetSequence(false);
                var threeLetterSequence = massCalculator.Peptide.GetSequence(true);

                var unchargedMass = massCalculator.Peptide.GetPeptideMass();
                var twoPlusMz = massCalculator.ConvoluteMass(unchargedMass, 0, 2);
                var threePlusMz = massCalculator.ConvoluteMass(twoPlusMz, 2, 3);

                Console.WriteLine("Peptide:            " + oneLetterSequence);
                Console.WriteLine("3 letter notation:  " + threeLetterSequence);
                Console.WriteLine("{0,-18}  {1}", elementModeDescription + " Mass:", unchargedMass);
                Console.WriteLine("m/z for 2+ ion:     " + twoPlusMz);
                Console.WriteLine("m/z for 3+ ion:     " + threePlusMz);

                massCalculator.Compound.SetFormula(threeLetterSequence);
                Console.WriteLine("Empirical Formula:  " + massCalculator.Compound.ConvertToEmpirical());

                Console.WriteLine();

                testFormula = "^13c2c4h6fe";

                massCalculator.Compound.SetFormula(testFormula);
                Console.WriteLine(testFormula + " auto-capitalizes to " + massCalculator.Compound.FormulaCapitalized);
                Console.WriteLine("{0,-18}  {1}", elementModeDescription + " Mass:", massCalculator.Compound.Mass);

                Console.WriteLine();
                Console.WriteLine("Percent composition");
                var percentCompositionByElement = massCalculator.Compound.GetPercentCompositionForAllElements();

                foreach (var item in percentCompositionByElement)
                {
                    Console.WriteLine(item.Key + ": " + item.Value);
                }
            }
        }
    }
}
