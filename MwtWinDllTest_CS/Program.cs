using System;
using System.Collections.Generic;
using System.Text;

namespace MwtWinDllTestCS
{
    /*
     * This is a very simple C# application that shows how to use MwtWinDll.dll 
     * Written by Matthew Monroe for PNNL in 2010
     */

    class Program
    {
        static void Main(string[] args) {

            MwtWinDll.MolecularWeightCalculator objMwtWin;

            // Instantiate the Molecular Weight Calculator
            objMwtWin = new MwtWinDll.MolecularWeightCalculator();

            string sTestFormula;
            double dMass;

            // Set the element mode (average, monoisotopic, or integer)
            objMwtWin.SetElementMode(MwtWinDll.MWElementAndMassRoutines.emElementModeConstants.emAverageMass);

            // Can simply compute the mass of a formula using ComputeMass
            sTestFormula = "C6H6";
            dMass = objMwtWin.ComputeMass(sTestFormula);

            Console.WriteLine("Mass of " + sTestFormula + " is " + dMass.ToString());
            Console.WriteLine();

            // If we want to do more complex operations, need to fill mMwtWin.Compound with valid info
            // Then, can read out values from it
            objMwtWin.Compound.SetFormula("Cl2PhH4OH");

            if (objMwtWin.Compound.ErrorDescription.Length > 0) {
                    Console.WriteLine("Error: " + objMwtWin.Compound.ErrorDescription);
            }
            else {
                 Console.WriteLine("Formula:           " + objMwtWin.Compound.FormulaCapitalized);
                 Console.WriteLine("Expand Abbrev:     " + objMwtWin.Compound.ExpandAbbreviations());
                 Console.WriteLine("Empirical Formula: " + objMwtWin.Compound.ConvertToEmpirical());
                 // Console.WriteLine("FormulaRTF: " + objMwtWin.Compound.FormulaRTF);
                 Console.WriteLine();
                 Console.WriteLine("Mass:              " + objMwtWin.Compound.Mass);
                 Console.WriteLine("Mass with StDev:   " + objMwtWin.Compound.MassAndStdDevString);
                 Console.WriteLine();

                 objMwtWin.Compound.SetFormula("Cl2PhH4OH");
                 Console.WriteLine("Formula:            " + objMwtWin.Compound.FormulaCapitalized);
                 Console.WriteLine("CautionDescription: " + objMwtWin.Compound.CautionDescription);
                 Console.WriteLine();

                 sTestFormula = "^13c2c4h6fe";

                 objMwtWin.Compound.SetFormula(sTestFormula);
                 Console.WriteLine(sTestFormula + " auto-capitalizes to " + objMwtWin.Compound.FormulaCapitalized);
                 Console.WriteLine("Mass: " + objMwtWin.Compound.Mass);

            }

            Console.WriteLine();
            Console.WriteLine();

            clsFragSpecTest objFragTest = new clsFragSpecTest(ref objMwtWin);
            objFragTest.TestAccessFunctions();

            Console.WriteLine("Press any key to continue");
            Console.ReadKey(true);
        }

    }
}