using System;
using System.Collections.Generic;
using System.Linq;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.FormulaFinder;
using NUnit.Framework;

namespace UnitTests.FunctionalTests
{
    public class FormulaSearcherTests
    {
        private MolecularWeightTool mwtWin;
        private SearchOptions searchOptions;

        [SetUp]
        public void TestFormulaFinder()
        {
            mwtWin = new MolecularWeightTool();

            mwtWin.SetElementMode(ElementMassMode.Isotopic);

            mwtWin.FormulaFinder.CandidateElements.Clear();

            mwtWin.FormulaFinder.AddCandidateElement("C");
            mwtWin.FormulaFinder.AddCandidateElement("H");
            mwtWin.FormulaFinder.AddCandidateElement("N");
            mwtWin.FormulaFinder.AddCandidateElement("O");

            // Abbreviations are supported, for example Serine
            mwtWin.FormulaFinder.AddCandidateElement("Ser");

            searchOptions = new SearchOptions
            {
                LimitChargeRange = false,
                ChargeMin = 1,
                ChargeMax = 1,
                FindTargetMz = false
            };
        }

        [Test]
        public void FindByMass()
        {

            // Search for 200 Da, +/- 0.05 Da
            var results = mwtWin.FormulaFinder.FindMatchesByMass(200d, 0.05d, searchOptions);
            ShowFormulaFinderResults(searchOptions, results);
        }

        [Test]
        public void FindByMassPPM()
        {
            // Search for 200 Da, +/- 250 ppm
            var results = mwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(searchOptions, results, true);
        }

        [Test]
        public void FindByMassPPMChargeLimit()
        {
            searchOptions.LimitChargeRange = true;
            searchOptions.ChargeMin = -4;
            searchOptions.ChargeMax = 6;

            // Search for 200 Da, +/- 250 ppm
            var results = mwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(searchOptions, results, true);
        }

        [Test]
        public void FindByMassPPMTargetMz()
        {
            searchOptions.LimitChargeRange = true;
            searchOptions.ChargeMin = -4;
            searchOptions.ChargeMax = 6;
            searchOptions.FindTargetMz = true;

            // Search for 100 m/z, +/- 250 ppm
            var results = mwtWin.FormulaFinder.FindMatchesByMassPPM(100d, 250d, searchOptions);
            ShowFormulaFinderResults(searchOptions, results, true);
        }

        [Test]
        public void FindByPercentComposition()
        {
            mwtWin.FormulaFinder.CandidateElements.Clear();

            mwtWin.FormulaFinder.AddCandidateElement("C", 70d);
            mwtWin.FormulaFinder.AddCandidateElement("H", 10d);
            mwtWin.FormulaFinder.AddCandidateElement("N", 10d);
            mwtWin.FormulaFinder.AddCandidateElement("O", 10d);

            // Search for percent composition results, maximum mass 400 Da
            var results = mwtWin.FormulaFinder.FindMatchesByPercentComposition(400d, 1d, searchOptions);
            ShowFormulaFinderResults(searchOptions, results, false, true);
        }

        [Test]
        public void FindByMassPPMBounded()
        {
            searchOptions.SearchMode = FormulaSearchModes.Bounded;

            // Search for 200 Da, +/- 250 ppm
            var results = mwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(searchOptions, results, true);
        }

        private void ShowFormulaFinderResults(
            SearchOptions searchOptions,
            List<SearchResult> results,
            bool deltaMassIsPPM = false,
            bool percentCompositionSearch = false)
        {
            string massColumnName;
            if (deltaMassIsPPM)
            {
                massColumnName = "DeltaPPM";
            }
            else
            {
                massColumnName = "DeltaMass";
            }

            // Add columns to the table
            Console.WriteLine("{0,-15} {1,8} {2,9} {3,6} {4,9} {5}", "Formula", "Mass", massColumnName, "Charge", "M/Z", "PercentCompInfo");

            var deltaMassFormat = "0.000";
            if (deltaMassIsPPM)
            {
                deltaMassFormat = "0.0";
            }

            foreach (var result in results)
            {
                // Populates the table.

                var mz = 0.0;
                if (searchOptions.FindCharge)
                {
                    mz = result.Mz;
                }

                var percentCompInfo = string.Empty;
                if (percentCompositionSearch)
                {
                    percentCompInfo = string.Join(" ", result.PercentComposition.Select(x => $"{x.Key}={x.Value:0.00}%"));
                }

                Console.WriteLine("{0,-15} {1,8:F4} {2,9:" + deltaMassFormat + "} {3,6} {4,9:F3} {5}", result.EmpiricalFormula, result.Mass, result.DeltaMass, result.ChargeState, mz, percentCompInfo);
            }
        }
    }
}
