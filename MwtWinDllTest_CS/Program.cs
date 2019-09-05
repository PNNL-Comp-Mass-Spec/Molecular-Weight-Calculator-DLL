using System;
using MwtWinDll;

namespace MwtWinDllTestCS
{
    /*
     * This is a very simple C# application that shows how to use MwtWinDll.dll
     * Written by Matthew Monroe for PNNL in 2010
     */

    class Program
    {
        static void Main(string[] args) {

            // Instantiate the Molecular Weight Calculator
            var mwtWin = new MolecularWeightCalculator();

            // Set the element mode (average, monoisotopic, or integer)
            mwtWin.SetElementMode(MWElementAndMassRoutines.emElementModeConstants.emAverageMass);

            // Can simply compute the mass of a formula using ComputeMass
            var testFormula = "C6H6";
            var formulaMass = mwtWin.ComputeMass(testFormula);

            Console.WriteLine("Mass of " + testFormula + " is " + formulaMass);
            Console.WriteLine();

            // If we want to do more complex operations, need to fill mMwtWin.Compound with valid info
            // Then, can read out values from it
            mwtWin.Compound.SetFormula("Cl2PhH4OH");

            if (mwtWin.Compound.ErrorDescription.Length > 0) {
                    Console.WriteLine("Error: " + mwtWin.Compound.ErrorDescription);
            }
            else {
                 Console.WriteLine("Formula:           " + mwtWin.Compound.FormulaCapitalized);
                 Console.WriteLine("Expand Abbrev:     " + mwtWin.Compound.ExpandAbbreviations());
                 Console.WriteLine("Empirical Formula: " + mwtWin.Compound.ConvertToEmpirical());
                 // Console.WriteLine("FormulaRTF: " + mwtWin.Compound.FormulaRTF);
                 Console.WriteLine();
                 Console.WriteLine("Mass:              " + mwtWin.Compound.Mass);
                 Console.WriteLine("Mass with StDev:   " + mwtWin.Compound.MassAndStdDevString);
                 Console.WriteLine();

                 mwtWin.Compound.SetFormula("Cl2PhH4OH");
                 Console.WriteLine("Formula:            " + mwtWin.Compound.FormulaCapitalized);
                 Console.WriteLine("CautionDescription: " + mwtWin.Compound.CautionDescription);
                 Console.WriteLine();

                 testFormula = "^13c2c4h6fe";

                 mwtWin.Compound.SetFormula(testFormula);
                 Console.WriteLine(testFormula + " auto-capitalizes to " + mwtWin.Compound.FormulaCapitalized);
                 Console.WriteLine("Mass: " + mwtWin.Compound.Mass);

            }

            Console.WriteLine();
            Console.WriteLine("Percent composition");
            var percentCompositionByElement = mwtWin.Compound.GetPercentCompositionForAllElements();

            foreach (var item in percentCompositionByElement)
            {
                Console.WriteLine(item.Key + ": " + item.Value);
            }

            Console.WriteLine();
            Console.WriteLine();

            var objFragTest = new clsFragSpecTest(ref mwtWin);
            objFragTest.TestAccessFunctions();

        }

    }
}