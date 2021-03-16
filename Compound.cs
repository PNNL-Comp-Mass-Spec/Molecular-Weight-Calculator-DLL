﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MolecularWeightCalculator
{
    public class Compound
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: Compound

        // The compound class can be used to represent a compound
        // Use the Formula Property to enter the compound's formula
        // Use ErrorDescription and CautionDescription to see if there are any problems with the formula
        // Custom abbreviations can be defined using the SetAbbreviationInternal() function in ElementAndMassRoutines()
        // Note that the standard amino acids and 16 other abbreviations are defined by default (see MemoryLoadAbbreviations())

        // Use the Mass Property to get the mass of the compound

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2002
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

        public Compound() : base()
        {
            ElementAndMassRoutines = new ElementAndMassTools();
            InitializeClass();
        }

        public Compound(ElementAndMassTools objElementAndMassTools) : base()
        {
            ElementAndMassRoutines = objElementAndMassTools;
            InitializeClass();
        }

        private string mStrFormula;
        private string mStrFormattedFormula;
        private double mValueForX; // The value to assign to x when present after a square bracket.
        // For example, in C6H6[xBr] if x = 1, then the formula is treated like C6H6Br
        // If x = 2, then the formula is treated like C6H6Br2

        private ElementAndMassTools.udtComputationStatsType mComputationStats;

        private readonly ElementAndMassTools ElementAndMassRoutines;

        public string ConvertToEmpirical()
        {
            // Converts mStrFormula to its empirical formula and returns the result
            var strResult = ElementAndMassRoutines.ConvertFormulaToEmpirical(mStrFormula);
            UpdateErrorAndCaution();

            if (string.IsNullOrEmpty(ErrorDescription))
            {
                mStrFormula = strResult;
                mStrFormattedFormula = strResult;
                return strResult;
            }

            return ErrorDescription;
        }

        public bool ElementPresent(short elementID)
        {
            // Returns True if the element is present
            if (elementID >= 1 && elementID <= ElementAndMassTools.ELEMENT_COUNT)
            {
                return mComputationStats.Elements[elementID].Used;
            }

            return false;
        }

        public string ExpandAbbreviations()
        {
            // Expands abbreviations in mStrFormula and returns the result
            var strResult = ElementAndMassRoutines.ExpandAbbreviationsInFormula(mStrFormula);
            UpdateErrorAndCaution();

            if (string.IsNullOrEmpty(ErrorDescription))
            {
                mStrFormula = strResult;
                mStrFormattedFormula = strResult;
                return strResult;
            }

            return ErrorDescription;
        }

        public double GetAtomCountForElement(short intElementID)
        {
            // Return the number of atoms of a given element that are present in the formula
            // Note that the number of atoms is not necessarily an integer (e.g. C5.5)

            if (intElementID >= 1 && intElementID <= ElementAndMassTools.ELEMENT_COUNT)
            {
                return mComputationStats.Elements[intElementID].Count;
            }

            return 0d;
        }

        public double GetPercentCompositionForElement(short intElementID)
        {
            // Returns the percent composition for element
            // Returns -1 if an invalid ID

            if (intElementID >= 1 && intElementID <= ElementAndMassTools.ELEMENT_COUNT)
            {
                return mComputationStats.PercentCompositions[intElementID].PercentComposition;
            }

            return -1;
        }

        public string GetPercentCompositionForElementAsString(short elementId)
        {
            return GetPercentCompositionForElementAsString(elementId, true);
        }

        public string GetPercentCompositionForElementAsString(short elementId, bool blnIncludeStandardDeviation)
        {
            // Returns the percent composition and standard deviation for element
            // Returns "" if an invalid ID

            if (elementId >= 1 && elementId <= ElementAndMassTools.ELEMENT_COUNT)
            {
                var compStats = mComputationStats.PercentCompositions[elementId];
                var strElementSymbol = ElementAndMassRoutines.GetElementSymbolInternal(elementId) + ":";
                var strPctComposition = ElementAndMassRoutines.ReturnFormattedMassAndStdDev(compStats.PercentComposition, compStats.StdDeviation, blnIncludeStandardDeviation, true);
                if (compStats.PercentComposition < 10d)
                {
                    strPctComposition = " " + strPctComposition;
                }

                return ElementAndMassRoutines.SpacePad(strElementSymbol, 4) + strPctComposition;
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
            // Returns the percent composition for all elements in strPctCompositionsOneBased

            var percentCompositionByElement = new Dictionary<string, string>();

            try
            {
                ElementAndMassRoutines.ComputePercentComposition(ref mComputationStats);

                for (short elementId = 1; elementId <= ElementAndMassTools.ELEMENT_COUNT; elementId++)
                {
                    if (mComputationStats.PercentCompositions[elementId].PercentComposition > 0d)
                    {
                        string percentCompositionAndStDev = ElementAndMassRoutines.ReturnFormattedMassAndStdDev(
                            mComputationStats.PercentCompositions[elementId].PercentComposition,
                            mComputationStats.PercentCompositions[elementId].StdDeviation);

                        string elementSymbol = ElementAndMassRoutines.GetElementSymbolInternal(elementId);

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

        public short GetUsedElementCount()
        {
            // Returns the number of unique elements present in mStrFormula

            // Determine # of elements in formula
            short intTotalElements = 0;
            for (int intElementIndex = 1; intElementIndex <= ElementAndMassTools.ELEMENT_COUNT; intElementIndex++)
            {
                // Increment .TotalElements if element is present
                if (mComputationStats.Elements[intElementIndex].Used)
                {
                    intTotalElements = (short)(intTotalElements + 1);
                }
            }

            return intTotalElements;
        }

        private void InitializeClass()
        {
            mStrFormula = "";
            ValueForX = 1.0d;
        }

        public int SetFormula(string strNewFormula)
        {
            // Provides an alternate method for setting the formula
            // Returns ErrorID (0 if no error)

            Formula = strNewFormula;

            return ErrorID;
        }

        private void UpdateErrorAndCaution()
        {
            CautionDescription = ElementAndMassRoutines.GetCautionDescription();
            ErrorDescription = ElementAndMassRoutines.GetErrorDescription();
            ErrorID = ElementAndMassRoutines.GetErrorID();
        }

        private void UpdateMass()
        {
            mStrFormattedFormula = mStrFormula;

            // mStrFormattedFormula is passed ByRef
            // If gComputationOptions.CaseConversion = ccConvertCaseUp then mStrFormattedFormula is properly capitalized
            // The mass of the compound is stored in mComputationStats.TotalMass
            ElementAndMassRoutines.ParseFormulaPublic(ref mStrFormattedFormula, ref mComputationStats, false, mValueForX);

            ElementAndMassRoutines.ComputePercentComposition(ref mComputationStats);

            UpdateErrorAndCaution();
        }

        public bool XIsPresentAfterBracket()
        {
            if (ElementAndMassRoutines.gComputationOptions.BracketsAsParentheses)
            {
                // Treating brackets as parentheses, therefore an x after a bracket isn't allowed
                return false;
            }

            var intCharLoc = (short)mStrFormattedFormula.ToLower().IndexOf("[x", StringComparison.OrdinalIgnoreCase);
            if (intCharLoc >= 0)
            {
                if (mStrFormattedFormula.Substring(intCharLoc + 1, 1) != "e")
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        public string CautionDescription { get; private set; }

        public float Charge
        {
            get => mComputationStats.Charge;
            set => mComputationStats.Charge = value;
        }

        public string ErrorDescription { get; private set; }

        public int ErrorID { get; private set; }

        public string Formula
        {
            get => mStrFormula;
            set
            {
                mStrFormula = value;

                // Recompute the mass for this formula
                // Updates Error and Caution statements if there is a problem
                UpdateMass();
            }
        }

        public string FormulaCapitalized => mStrFormattedFormula;

        public string FormulaRTF => ElementAndMassRoutines.PlainTextToRtfInternal(FormulaCapitalized, false);

        public double Mass => get_Mass(true);

        public double get_Mass(bool blnRecomputeMass)
        {
            if (blnRecomputeMass)
                UpdateMass();
            return mComputationStats.TotalMass;
        }

        public string MassAndStdDevString => get_MassAndStdDevString(true);

        public string get_MassAndStdDevString(bool blnRecomputeMass)
        {
            if (blnRecomputeMass)
                UpdateMass();

            return ElementAndMassRoutines.ReturnFormattedMassAndStdDev(mComputationStats.TotalMass, mComputationStats.StandardDeviation);
        }

        public double StandardDeviation => mComputationStats.StandardDeviation;

        public double ValueForX
        {
            get => mValueForX;
            set
            {
                if (value >= 0d)
                    mValueForX = value;
            }
        }
    }
}