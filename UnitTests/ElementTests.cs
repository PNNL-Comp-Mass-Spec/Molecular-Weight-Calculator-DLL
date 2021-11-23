using System;
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
            ReportParseData(UnitTestWriterType.NoWriter, parseData);

            if (mCompareTextToExpected)
            {
                Assert.GreaterOrEqual(parseData.CautionDescription.IndexOf(cautionExcerpt, StringComparison.OrdinalIgnoreCase), 0,
                    "excerpt not found in reported caution statement");
            }
        }
    }
}
