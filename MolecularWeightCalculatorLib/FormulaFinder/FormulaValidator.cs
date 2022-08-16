using System;
using System.Collections.Generic;

namespace MolecularWeightCalculator.FormulaFinder
{
    /// <summary>
    /// Object for storing element counts for verifying hydrogens and correcting/verifying charge
    /// </summary>
    internal class FormulaValidator
    {
        public FormulaValidator(IEnumerable<ElementCount> formulaElements)
        {
            // Determine number of C, Si, N, P, O, S, Cl, I, F, Br and H atoms
            foreach (var element in formulaElements)
            {
                switch (element.Symbol)
                {
                    case "C":
                        C += element.Count;
                        break;
                    case "Si":
                        Si += element.Count;
                        break;
                    case "N":
                        N += element.Count;
                        break;
                    case "P":
                        P += element.Count;
                        break;
                    case "O":
                        O += element.Count;
                        break;
                    case "S":
                        S += element.Count;
                        break;
                    case "Cl":
                        Cl += element.Count;
                        break;
                    case "I":
                        I += element.Count;
                        break;
                    case "F":
                        F += element.Count;
                        break;
                    case "Br":
                        Br += element.Count;
                        break;
                    case "H":
                        H += element.Count;
                        break;
                    default:
                        Other += element.Count;
                        break;
                }
            }
        }

        public readonly int H;
        public readonly int C;
        public readonly int Si;
        public readonly int N;
        public readonly int P;
        public readonly int O;
        public readonly int S;
        public readonly int Cl;
        public readonly int I;
        public readonly int F;
        public readonly int Br;
        public readonly int Other;

        public bool HydrogensValid()
        {
            var maxHydrogens = 0;
            // Compute maximum number of hydrogens
            if (Si == 0 && C == 0 && N == 0 && P == 0 && Other == 0 && (O > 0 || S > 0))
            {
                // Only O and S
                maxHydrogens = 3;
            }
            else
            {
                // Formula is: [#C*2 + 3 - (2 if N or P present)] + [#N + 3 - (1 if C or Si present)] + [#other elements * 4 + 3], where we assume other elements can have a coordination number of up to 7
                if (C > 0 || Si > 0)
                {
                    maxHydrogens += (C + Si) * 2 + 3;
                    // if (N > 0 || P > 0) maxHydrogens -= 2
                }

                if (N > 0 || P > 0)
                {
                    maxHydrogens += (N + P) + 3;
                    // if (C > 0 || Si > 0) maxHydrogens -= 1
                }

                // Correction for carbon contribution
                // Combine the above two commented-out if's into:
                if ((N > 0 || P > 0) && (C > 0 || Si > 0))
                    maxHydrogens -= 3;

                if (Other > 0)
                    maxHydrogens += Other * 4 + 3;
            }

            // correct for if H only
            if (maxHydrogens < 3)
                maxHydrogens = 3;

            // correct for halogens
            maxHydrogens -= F - Cl - Br - I;

            // correct for negative maxHydrogens
            if (maxHydrogens < 0)
                maxHydrogens = 0;

            // Verify H's
            return H <= maxHydrogens;
        }

        /// <summary>
        /// Correct charge using rules for an empirical formula
        /// </summary>
        /// <param name="totalCharge"></param>
        /// <param name="limitChargeRange"></param>
        /// <param name="minCharge"></param>
        /// <param name="maxCharge"></param>
        /// <param name="hydrogenCharge"></param>
        /// <returns>Corrected charge</returns>
        public bool CorrectChargeEmpirical(ref double totalCharge, bool limitChargeRange, int minCharge, int maxCharge, double hydrogenCharge)
        {
            var chargeOk = false;
            var correctedCharge = totalCharge;

            // Correct charge using rules for an empirical formula
            if (C + Si >= 1)
            {
                if (H > 0 && Math.Abs(hydrogenCharge - 1) < float.Epsilon)
                {
                    // Since carbon or silicon are present, assume the hydrogens should be negative
                    // Subtract H*2 since hydrogen is assigned a +1 charge if ElementStats[1].Charge = 1
                    correctedCharge -= H * 2;
                }
                // Correct for number of C and Si atoms
                if (C + Si > 1)
                {
                    correctedCharge -= (C + Si - 1) * 2;
                }
            }

            if (N + P > 0 && C > 0)
            {
                // Assume 2 hydrogens around each Nitrogen or Phosphorus, thus add back +2 for each H
                // First, decrease the number of halogens by the number of hydrogens & halogens taken up by the carbons
                // Determine # of H taken up by all the carbons in a compound without N or P, then add back 1 H for each N and P
                var numHalogens = H + F + Cl + Br + I;
                numHalogens += -(C * 2 + 2) + N + P;

                if (numHalogens >= 0)
                {
                    for (var i = 0; i < N + P; i++)
                    {
                        correctedCharge += 2d;
                        numHalogens--;
                        if (numHalogens <= 0)
                        {
                            break;
                        }

                        correctedCharge += 2d;
                        numHalogens--;
                        if (numHalogens <= 0)
                            break;
                    }
                }
            }

            totalCharge = correctedCharge;

            if (limitChargeRange)
            {
                // Make sure totalCharge is within the specified range
                if (correctedCharge >= minCharge && correctedCharge <= maxCharge)
                {
                    // Charge is within range
                    chargeOk = true;
                }
                else
                {
                    chargeOk = false;
                }
            }
            else
            {
                chargeOk = true;
            }

            return chargeOk;
        }
    }
}
