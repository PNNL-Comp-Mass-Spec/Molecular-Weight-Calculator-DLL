using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.Formula
{
    [Guid("4631BE93-0F20-4E9C-96CF-BC0FBA38BF4E"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class Compound : ICompound
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: Compound

        // The compound class can be used to represent a compound
        // Use the Formula Property to enter the compound's formula
        // Use ErrorDescription and CautionDescription to see if there are any problems with the formula
        // Custom abbreviations can be defined using the SetAbbreviation() method in ElementAndMassTools
        // Note that the standard amino acids and 16 other abbreviations are defined by default (see MemoryLoadAbbreviations())

        // Use the Mass Property to get the mass of the compound

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2002
        // Converted to C# by Bryson Gibbons in 2021
        // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
        // Website: https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL and https://omics.pnl.gov/
        // -------------------------------------------------------------------------------
        //
        // Licensed under the Apache License, Version 2.0; you may not use this file except
        // in compliance with the License.  You may obtain a copy of the License at
        // http://www.apache.org/licenses/LICENSE-2.0
        //
        // Notice: This computer software was prepared by Battelle Memorial Institute,
        // hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
        // Department of Energy (DOE).  All rights in the computer software are reserved
        // by DOE on behalf of the United States Government and the Contractor as
        // provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY
        // WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS
        // SOFTWARE.  This notice including this sentence must appear on any copies of
        // this computer software.

        // Ignore Spelling: Bryson, interop

        public Compound(ElementAndMassTools elementAndMassTools = null)
        {
            mElementAndMassRoutines = elementAndMassTools ?? new ElementAndMassTools();

            mFormula = string.Empty;
            ValueForUnknown = 1.0d;
        }

        private string mFormula;

        private string mFormattedFormula;

        /// <summary>
        /// The value to assign to ? when present after a square bracket.
        /// </summary>
        /// <remarks>
        /// For example, in C6H6[?Br] if ? = 1, then the formula is treated like C6H6Br
        /// If ? = 2, then the formula is treated like C6H6Br2
        /// </remarks>
        private double mValueForUnknown;

        private ComputationStats mComputationStats = new();

        private readonly ElementAndMassTools mElementAndMassRoutines;

        public string ConvertToEmpirical()
        {
            // Converts mFormula to its empirical formula and returns the result
            var result = mElementAndMassRoutines.Parser.ConvertFormulaToEmpirical(mFormula);
            UpdateErrorAndCaution();

            if (string.IsNullOrEmpty(ErrorDescription))
            {
                mFormula = result;
                mFormattedFormula = result;
                return result;
            }

            return ErrorDescription;
        }

        public bool ElementPresent(short atomicNumber)
        {
            // Returns True if the element is present
            if (atomicNumber is >= 1 and <= ElementsAndAbbrevs.ELEMENT_COUNT)
            {
                return mComputationStats.Elements[atomicNumber].Used;
            }

            return false;
        }

        public string ExpandAbbreviations()
        {
            // Expands abbreviations in mFormula and returns the result
            var result = mElementAndMassRoutines.Parser.ExpandAbbreviationsInFormula(mFormula);
            UpdateErrorAndCaution();

            if (string.IsNullOrEmpty(ErrorDescription))
            {
                mFormula = result;
                mFormattedFormula = result;
                return result;
            }

            return ErrorDescription;
        }

        public double GetAtomCountForElement(short atomicNumber)
        {
            // Return the number of atoms of a given element that are present in the formula
            // Note that the number of atoms is not necessarily an integer (e.g. C5.5)

            if (atomicNumber is >= 1 and <= ElementsAndAbbrevs.ELEMENT_COUNT)
            {
                return mComputationStats.Elements[atomicNumber].Count;
            }

            return 0d;
        }

        public double GetPercentCompositionForElement(short atomicNumber)
        {
            // Returns the percent composition for element
            // Returns -1 if an invalid ID

            if (atomicNumber is >= 1 and <= ElementsAndAbbrevs.ELEMENT_COUNT)
            {
                return mComputationStats.PercentCompositions[atomicNumber].PercentComposition;
            }

            return -1;
        }

        /// <summary>
        /// Get the percent composition for the element, as a string
        /// </summary>
        /// <param name="atomicNumber"></param>
        /// <param name="includeStandardDeviation"></param>
        /// <returns></returns>
        public string GetPercentCompositionForElementAsString(short atomicNumber, bool includeStandardDeviation = true)
        {
            // Returns the percent composition and standard deviation for element
            // Returns "" if an invalid ID

            if (atomicNumber is >= 1 and <= ElementsAndAbbrevs.ELEMENT_COUNT)
            {
                var compStats = mComputationStats.PercentCompositions[atomicNumber];
                var elementSymbol = mElementAndMassRoutines.Elements.GetElementSymbol(atomicNumber) + ":";
                var pctComposition = mElementAndMassRoutines.ReturnFormattedMassAndStdDev(compStats.PercentComposition, compStats.StdDeviation, includeStandardDeviation, true);
                if (compStats.PercentComposition < 10d)
                {
                    pctComposition = " " + pctComposition;
                }

                return mElementAndMassRoutines.SpacePad(elementSymbol, 4) + pctComposition;
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the percent composition for all elements in an empirical formula
        /// </summary>
        /// <returns>
        /// Dictionary of percent composition values
        /// Keys are element symbols; values are the percent composition
        /// </returns>
        public Dictionary<string, string> GetPercentCompositionForAllElements()
        {
            // Returns the percent composition for all elements in pctCompositionsOneBased

            var percentCompositionByElement = new Dictionary<string, string>();

            try
            {
                mElementAndMassRoutines.Parser.ComputePercentComposition(mComputationStats);

                for (var atomicNumber = 1; atomicNumber <= ElementsAndAbbrevs.ELEMENT_COUNT; atomicNumber++)
                {
                    if (mComputationStats.PercentCompositions[atomicNumber].PercentComposition > 0d)
                    {
                        var percentCompositionAndStDev = mElementAndMassRoutines.ReturnFormattedMassAndStdDev(
                            mComputationStats.PercentCompositions[atomicNumber].PercentComposition,
                            mComputationStats.PercentCompositions[atomicNumber].StdDeviation);

                        var elementSymbol = mElementAndMassRoutines.Elements.GetElementSymbol((short)atomicNumber);

                        if (!percentCompositionByElement.ContainsKey(elementSymbol))
                        {
                            percentCompositionByElement.Add(elementSymbol, percentCompositionAndStDev);
                        }
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error occurred while copying percent composition values.  Probably an uninitialized array.");
            }

            return percentCompositionByElement;
        }

        /// <summary>
        /// Get the percent composition for all elements in an empirical formula. This implementation is specifically for COM interop support
        /// </summary>
        /// <returns>
        /// 2D array of percent composition values; first dimension is element symbols, second dimension is percent compositions
        /// </returns>
        public string[,] GetPercentCompositionForAllElements2DArray()
        {
            var data = GetPercentCompositionForAllElements();
            var array = new string[data.Count, 2];
            var counter = 0;

            foreach (var entry in data)
            {
                array[counter, 0] = entry.Key;
                array[counter, 1] = entry.Value;
                counter++;
            }

            return array;
        }

        public short GetUsedElementCount()
        {
            // Returns the number of unique elements present in mFormula

            // Determine # of elements in formula
            short totalElements = 0;
            for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
            {
                // Increment .TotalElements if element is present
                if (mComputationStats.Elements[elementIndex].Used)
                {
                    totalElements++;
                }
            }

            return totalElements;
        }

        public int SetFormula(string newFormula)
        {
            // Provides an alternate method for setting the formula
            // Returns ErrorID (0 if no error)

            Formula = newFormula;

            return ErrorId;
        }

        private void UpdateErrorAndCaution()
        {
            CautionDescription = FormulaParseData.CautionDescription ?? string.Empty;
            ErrorDescription = FormulaParseData.ErrorData.ErrorDescription ?? string.Empty;
            ErrorId = FormulaParseData.ErrorData.ErrorId;
        }

        private void UpdateMass()
        {
            mFormattedFormula = mFormula;

            FormulaParseData = mElementAndMassRoutines.Parser.ParseFormula(mFormattedFormula, false, mValueForUnknown);

            mFormattedFormula = FormulaParseData.Formula;
            mComputationStats = FormulaParseData.Stats;

            mElementAndMassRoutines.Parser.ComputePercentComposition(mComputationStats);

            UpdateErrorAndCaution();
        }

        public bool XIsPresentAfterBracket()
        {
            if (mElementAndMassRoutines.ComputationOptions.BracketsAsParentheses)
            {
                // Treating brackets as parentheses, therefore an x after a bracket isn't allowed
                return false;
            }

            var charLoc = (short)mFormattedFormula.ToLower().IndexOf("[x", StringComparison.OrdinalIgnoreCase);
            if (charLoc >= 0)
            {
                return mFormattedFormula[charLoc + 1] != 'e';
            }

            return false;
        }

        public string CautionDescription { get; private set; }

        public float Charge
        {
            get => mComputationStats.Charge;
            set => mComputationStats.Charge = value;
        }

        public string ErrorDescription { get; private set; } = string.Empty;

        public int ErrorId { get; private set; }

        public string Formula
        {
            get => mFormula;
            set
            {
                mFormula = value;

                // Recompute the mass for this formula
                // Updates Error and Caution statements if there is a problem
                UpdateMass();
            }
        }

        public string FormulaCapitalized => mFormattedFormula;

        [ComVisible(false)] public IFormulaParseData FormulaParseData { get; private set; } = new FormulaParseData("");

        // ReSharper disable once InconsistentNaming
        public string FormulaRTF => mElementAndMassRoutines.PlainTextToRtf(FormulaCapitalized, false, errorData: FormulaParseData.ErrorData);

        public double Mass => GetMass();

        public double GetMass(bool recomputeMass = true)
        {
            if (recomputeMass)
                UpdateMass();

            return mComputationStats.TotalMass;
        }

        public string MassAndStdDevString => GetMassAndStdDevString();

        public string GetMassAndStdDevString(bool recomputeMass = true)
        {
            if (recomputeMass)
                UpdateMass();

            return mElementAndMassRoutines.ReturnFormattedMassAndStdDev(mComputationStats.TotalMass, mComputationStats.StandardDeviation);
        }

        public double StandardDeviation => mComputationStats.StandardDeviation;

        public double ValueForUnknown
        {
            get => mValueForUnknown;
            set
            {
                if (value >= 0d)
                    mValueForUnknown = value;
            }
        }
    }
}