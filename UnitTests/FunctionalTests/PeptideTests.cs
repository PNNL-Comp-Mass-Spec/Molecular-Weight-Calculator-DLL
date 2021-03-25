using System;
using System.Diagnostics;
using MolecularWeightCalculator;
using NUnit.Framework;

namespace UnitTests.FunctionalTests
{
    public class PeptideTests
    {
        [Test]
        public void TestTrypticName()
        {
            const short dimChunk = 1000;

            const short iterationsToRun = 5;
            const short minProteinLength = 50;
            const short maxProteinLength = 200;
            const string possibleResidues = "ACDEFGHIKLMNPQRSTVWY";

            string peptideFragMwtWin;
            const int matchCount = default(int);

            var mMwtWin = new MolecularWeightTool();

            int mwtWinDimCount = dimChunk;
            var peptideNameMwtWin = new string[mwtWinDimCount + 1];

            // Bigger protein
            var protein = "MMKANVTKKTLNEGLGLLERVIPSRSSNPLLTALKVETSEGGLTLSGTNLEIDLSCFVPAEVQQPENFVVPAHLFAQIVRNLGGELVELELSGQELSVRSGGSDFKLQTGDIEAYPPLSFPAQADVSLDGGELSRAFSSVRYAASNEAFQAVFRGIKLEHHGESARVVASDGYRVAIRDFPASGDGKNLIIPARSVDELIRVLKDGEARFTYGDGMLTVTTDRVKMNLKLLDGDFPDYERVIPKDIKLQVTLPATALKEAVNRVAVLADKNANNRVEFLVSEGTLRLAAEGDYGRAQDTLSVTQGGTEQAMSLAFNARHVLDALGPIDGDAELLFSGSTSPAIFRARRWGRRVYGGHGHAARLRGLLRPLRGMSALAHHPESSPPLEPRPEFA";

            Console.WriteLine("Testing GetTrypticNameMultipleMatches() function");
            Console.WriteLine("MatchList for NL: " + mMwtWin.Peptide.GetTrypticNameMultipleMatches(protein, "NL", matchCount));
            Console.WriteLine("MatchCount = " + matchCount);

            Console.WriteLine(string.Empty);
            Console.WriteLine("Testing GetTrypticPeptideByFragmentNumber function");
            for (var index = 1; index <= 43; index++)
            {
                peptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(protein, (short)index, out var residueStart, out var residueEnd);

                if (peptideFragMwtWin.Length > 1)
                {
                    // Make sure residueStart and residueEnd are correct
                    // Do this using .GetTrypticNameMultipleMatches()
                    var peptideName = mMwtWin.Peptide.GetTrypticNameMultipleMatches(protein, protein.Substring(residueStart, Math.Min(residueEnd - residueStart + 1, protein.Length - residueStart)));
                    Assert.IsTrue(peptideName.IndexOf("t" + index, StringComparison.Ordinal) >= 0, $"Tryptic Peptide t{index} not found in string \"{peptideName}\"");
                }
            }

            Console.WriteLine("Check of GetTrypticPeptideByFragmentNumber Complete");
            Console.WriteLine(string.Empty);

            Console.WriteLine("Test tryptic digest of: " + protein);
            var fragIndex = 1;
            do
            {
                peptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(protein, (short)fragIndex, out _, out _);
                Console.WriteLine("Tryptic fragment " + fragIndex + ": " + peptideFragMwtWin);
                fragIndex += 1;
            }
            while (peptideFragMwtWin.Length > 0);

            Console.WriteLine(string.Empty);
            var random = new Random();
            for (var multipleIteration = 1; multipleIteration <= iterationsToRun; multipleIteration++)
            {
                // Generate random protein
                var proteinLengthRand = random.Next(maxProteinLength - minProteinLength + 1) + minProteinLength;

                protein = "";
                for (var residueRand = 0; residueRand < proteinLengthRand; residueRand++)
                {
                    var newResidue = possibleResidues.Substring(random.Next(possibleResidues.Length), 1);
                    protein += newResidue;
                }

                Console.WriteLine("Iteration: " + multipleIteration + " = " + protein);

                var mwtWinResultCount = 0;
                Debug.Write("Starting residue is ");
                var sw = Stopwatch.StartNew();
                for (var residueStart = 0; residueStart < protein.Length; residueStart++)
                {
                    if (residueStart % 10 == 0)
                    {
                        Debug.Write(residueStart + ", ");
                    }

                    for (var residueEnd = 0; residueEnd < protein.Length - residueStart; residueEnd++)
                    {
                        if (residueEnd - residueStart > 50)
                        {
                            break;
                        }

                        var peptideResidues = protein.Substring(residueStart, residueEnd);
                        peptideNameMwtWin[mwtWinResultCount] = mMwtWin.Peptide.GetTrypticName(protein, peptideResidues, out _, out _, true);

                        mwtWinResultCount += 1;
                        if (mwtWinResultCount > mwtWinDimCount)
                        {
                            mwtWinDimCount += dimChunk;
                            Array.Resize(ref peptideNameMwtWin, mwtWinDimCount + 1);
                        }
                    }
                }

                sw.Stop();
                var mwtWinWorkTime = sw.ElapsedMilliseconds;
                Console.WriteLine("");
                Console.WriteLine("Processing time (" + mwtWinResultCount + " peptides) = " + mwtWinWorkTime + " msec");
            }

            Console.WriteLine("Check of Tryptic Sequence functions Complete");
        }
    }
}
