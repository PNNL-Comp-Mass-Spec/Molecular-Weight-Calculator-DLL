using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ElementTests : TestBase
    {
        // Ignore Spelling: cd, dy, nb, nd, ni, np, os, ph, pu, py, sb, sc, si, sn, yb, bpy, cys, hoh, hyp, oac
        // Ignore Spelling: bipyridine, cysteine, Da, hydroxyproline, proline, valine

        /// <summary>
        /// Initialize the Molecular Weight Calculator objects
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            Initialize();
        }

        [Test]
        [TestCase("bi", "bismuth")]
        [TestCase("bk", "berkelium")]
        [TestCase("bu", "butyl group")]
        [TestCase("cd", "cadmium")]
        [TestCase("cf", "californium")]
        [TestCase("co", "cobalt")]
        [TestCase("cs", "cesium")]
        [TestCase("cu", "copper")]
        [TestCase("dy", "dysprosium")]
        [TestCase("hf", "hafnium")]
        [TestCase("ho", "holmium")]
        [TestCase("in", "indium")]
        [TestCase("nb", "niobium")]
        [TestCase("nd", "neodymium")]
        [TestCase("ni", "nickel")]
        [TestCase("no", "nobelium")]
        [TestCase("np", "neptunium")]
        [TestCase("os", "osmium")]
        [TestCase("pd", "palladium")]
        [TestCase("ph", "phenyl")]
        [TestCase("pu", "plutonium")]
        [TestCase("py", "pyridine")]
        [TestCase("sb", "antimony")]
        [TestCase("sc", "scandium")]
        [TestCase("si", "silicon")]
        [TestCase("sn", "sulfur-nitrogen")]
        [TestCase("TI", "tritium-iodine")]
        [TestCase("yb", "yttrium-boron")]
        [TestCase("BPY", "boron-phosphorus-yttrium")]
        [TestCase("BPy", "boron-pyridine")]
        [TestCase("bpy", "bipyridine")]
        [TestCase("cys", "cysteine")]
        [TestCase("his", "histidine")]
        [TestCase("hoh", "holmium")]
        [TestCase("hyp", "hydroxyproline")]
        [TestCase("Oac", "acetate")]
        [TestCase("oac", "acetate")]
        [TestCase("Pro", "proline")]
        [TestCase("prO", "praseodymium-oxygen")]
        [TestCase("val", "valine")]
        [TestCase("vAl", "vanadium-aluminum")]
        public void ComputeMassCautionMessageTests(string formula, string cautionExcerpt)
        {
            var averageMass = mAverageMassCalculator.ComputeMassExtra(formula, out var parseData);

            Console.WriteLine("Mass of {0}: {1:F3} Da", formula, averageMass);

            ReportParseData(UnitTestWriterType.NoWriter, parseData);

            if (mCompareTextToExpected)
            {
                Assert.GreaterOrEqual(parseData.CautionDescription.IndexOf(cautionExcerpt, StringComparison.OrdinalIgnoreCase), 0,
                    "excerpt not found in reported caution statement");
            }
        }

        [Test]
        [TestCase("C", false, "12;98.5", "13.05;1.25", "14.10;0.25")]
        [TestCase("N", false, "14; 0.985", "15.07; .0125", "16.12; .0025")]
        [TestCase("P", true, "31; 95", "32; 1.1")]
        public void TestSetElementIsotopes(string elementSymbol, bool errorExpected, params string[] newIsotopes)
        {
            mMonoisotopicMassCalculator.GetElementIsotopes(
                elementSymbol,
                out var originalIsotopeCount,
                out var originalIsotopeMasses,
                out var originalIsotopeAbundances);

            Console.WriteLine("Original isotopes of {0}", elementSymbol);
            for (var i = 0; i < originalIsotopeCount; i++)
            {
                Console.WriteLine("{0:F3}: {1:F3}%", originalIsotopeMasses[i], originalIsotopeAbundances[i] * 100);
            }

            Console.WriteLine();

            var isotopeMasses = new List<double>();
            var isotopeAbundances = new List<float>();
            var abundanceSum = 0.0;

            foreach (var item in newIsotopes)
            {
                var isotopeParts = item.Split(new[] { ';' }, 2);

                if (!double.TryParse(isotopeParts[0], out var isotopeMass))
                {
                    Assert.Fail("Unable to parse the mass value from {0}", isotopeParts[0]);
                }

                if (!float.TryParse(isotopeParts[1], out var isotopeAbundance))
                {
                    Assert.Fail("Unable to parse the abundance value from {0}", isotopeParts[1]);
                }

                isotopeMasses.Add(isotopeMass);
                isotopeAbundances.Add(isotopeAbundance);

                abundanceSum += isotopeAbundance;
            }

            if (Math.Abs(abundanceSum - 1) < 0.0001)
            {
                // Isotope abundances were specified as values between 0 and 1; leave as is
                Console.WriteLine("Isotope abundance sum: {0:F3}", abundanceSum);
            }
            else if (Math.Abs(abundanceSum - 100) < 0.01)
            {
                Console.WriteLine("Isotope abundance sum: {0:F3}", abundanceSum);
                Console.WriteLine("Converting to values between 0 and 1");

                for (var i = 0; i < isotopeAbundances.Count; i++)
                {
                    isotopeAbundances[i] /= 100f;
                }
            }
            else
            {
                var message = string.Format(
                    "The sum of the isotope abundances should be 1 if using fractions, or 100 if using percentages; " +
                    "the actual sum is {0:F3}", abundanceSum);

                if (errorExpected)
                {
                    Console.WriteLine("{0} (this was expected)", message);
                    return;
                }

                Assert.Fail(message);
            }

            mMonoisotopicMassCalculator.SetElementIsotopes(elementSymbol, isotopeMasses, isotopeAbundances);

            mMonoisotopicMassCalculator.GetElementIsotopes(
                elementSymbol,
                out var updatedIsotopeCount,
                out var updatedIsotopeMasses,
                out var updatedIsotopeAbundances);

            Console.WriteLine();
            Console.WriteLine("Updated isotopes of {0}", elementSymbol);
            for (var i = 0; i < updatedIsotopeCount; i++)
            {
                Console.WriteLine("{0:F3}: {1:F3}%", updatedIsotopeMasses[i], updatedIsotopeAbundances[i] * 100);

                Assert.AreEqual(isotopeMasses[i], updatedIsotopeMasses[i], "Isotope mass mismatch");
                Assert.AreEqual(isotopeAbundances[i], updatedIsotopeAbundances[i], "Isotope relative abundance mismatch");
            }
        }
    }
}
