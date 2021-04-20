using System;
using System.Collections.Generic;
using System.Diagnostics;
using MolecularWeightCalculator;
using NUnit.Framework;

namespace UnitTests.FunctionalTests
{
    public class PeptideTests
    {
        private readonly IReadOnlyDictionary<int, string> expectedFragments;

        public PeptideTests()
        {
            expectedFragments = new Dictionary<int, string>()
            {
                {1, "MMK"},
                {2, "ANVTK"},
                {3, "K"},
                {4, "TLNEGLGLLER"},
                {5, "VIPSR"},
                {6, "SSNPLLTALK"},
                {7, "VETSEGGLTLSGTNLEIDLSCFVPAEVQQPENFVVPAHLFAQIVR"},
                {8, "NLGGELVELELSGQELSVR"},
                {9, "SGGSDFK"},
                {10, "LQTGDIEAYPPLSFPAQADVSLDGGELSR"},
                {11, "AFSSVR"},
                {12, "YAASNEAFQAVFR"},
                {13, "GIK"},
                {14, "LEHHGESAR"},
                {15, "VVASDGYR"},
                {16, "VAIR"},
                {17, "DFPASGDGK"},
                {18, "NLIIPAR"},
                {19, "SVDELIR"},
                {20, "VLK"},
                {21, "DGEAR"},
                {22, "FTYGDGMLTVTTDR"},
                {23, "VK"},
                {24, "MNLK"},
                {25, "LLDGDFPDYER"},
                {26, "VIPK"},
                {27, "DIK"},
                {28, "LQVTLPATALK"},
                {29, "EAVNR"},
                {30, "VAVLADK"},
                {31, "NANNR"},
                {32, "VEFLVSEGTLR"},
                {33, "LAAEGDYGR"},
                {34, "AQDTLSVTQGGTEQAMSLAFNAR"},
                {35, "HVLDALGPIDGDAELLFSGSTSPAIFR"},
                {36, "AR"},
                {37, "R"},
                {38, "WGR"},
                {39, "R"},
                {40, "VYGGHGHAAR"},
                {41, "LR"},
                {42, "GLLRPLR"},
                {43, "GMSALAHHPESSPPLEPRPEFA"},
                {44, ""},
            };
        }

        [Test]
        public void TestTrypticName()
        {
            const short dimChunk = 1000;

            const short iterationsToRun = 5;
            const short minProteinLength = 50;
            const short maxProteinLength = 200;
            const string possibleResidues = "ACDEFGHIKLMNPQRSTVWY";

            string peptideFragMwtWin;
            const int matchCount = 0;

            var mAverageMassCalculator = new MolecularWeightTool();

            int mwtWinDimCount = dimChunk;
            var peptideNameMwtWin = new string[mwtWinDimCount + 1];

            // Bigger protein
            var protein = "MMKANVTKKTLNEGLGLLERVIPSRSSNPLLTALKVETSEGGLTLSGTNLEIDLSCFVPAEVQQPENFVVPAHLFAQIVRNLGGELVELELSGQELSVRSGGSDFKLQTGDIEAYPPLSFPAQADVSLDGGELSRAFSSVRYAASNEAFQAVFRGIKLEHHGESARVVASDGYRVAIRDFPASGDGKNLIIPARSVDELIRVLKDGEARFTYGDGMLTVTTDRVKMNLKLLDGDFPDYERVIPKDIKLQVTLPATALKEAVNRVAVLADKNANNRVEFLVSEGTLRLAAEGDYGRAQDTLSVTQGGTEQAMSLAFNARHVLDALGPIDGDAELLFSGSTSPAIFRARRWGRRVYGGHGHAARLRGLLRPLRGMSALAHHPESSPPLEPRPEFA";

            Console.WriteLine("Testing GetTrypticNameMultipleMatches() function");
            Console.WriteLine("MatchList for NL: " + mAverageMassCalculator.Peptide.GetTrypticNameMultipleMatches(protein, "NL", matchCount));
            Console.WriteLine("MatchCount = " + matchCount);

            Console.WriteLine(string.Empty);
            Console.WriteLine("Testing GetTrypticPeptideByFragmentNumber function");
            for (var index = 1; index <= 43; index++)
            {
                peptideFragMwtWin = mAverageMassCalculator.Peptide.GetTrypticPeptideByFragmentNumber(protein, (short)index, out var residueStart, out var residueEnd);

                if (peptideFragMwtWin.Length > 1)
                {
                    // Make sure residueStart and residueEnd are correct
                    // Do this using .GetTrypticNameMultipleMatches()
                    var peptideName = mAverageMassCalculator.Peptide.GetTrypticNameMultipleMatches(protein, protein.Substring(residueStart, Math.Min(residueEnd - residueStart + 1, protein.Length - residueStart)));
                    Assert.IsTrue(peptideName.IndexOf("t" + index, StringComparison.Ordinal) >= 0, $"Tryptic Peptide t{index} not found in string \"{peptideName}\"");
                }
            }

            Console.WriteLine("Check of GetTrypticPeptideByFragmentNumber Complete");
            Console.WriteLine(string.Empty);

            Console.WriteLine("Test tryptic digest of: " + protein);
            var fragIndex = 1;
            do
            {
                peptideFragMwtWin = mAverageMassCalculator.Peptide.GetTrypticPeptideByFragmentNumber(protein, (short)fragIndex, out _, out _);
                Console.WriteLine("Tryptic fragment " + fragIndex + ": " + peptideFragMwtWin);
                Assert.AreEqual(peptideFragMwtWin, expectedFragments[fragIndex], "Fragment did not match expected sequence");
                fragIndex++;
            }
            while (peptideFragMwtWin.Length > 0);

            Console.WriteLine(string.Empty);
            var random = new Random();
            for (var multipleIteration = 1; multipleIteration <= iterationsToRun; multipleIteration++)
            {
                // Generate random protein
                var proteinLengthRand = random.Next(maxProteinLength - minProteinLength + 1) + minProteinLength;

                protein = string.Empty;
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
                        peptideNameMwtWin[mwtWinResultCount] = mAverageMassCalculator.Peptide.GetTrypticName(protein, peptideResidues, out _, out _, true);

                        mwtWinResultCount++;
                        if (mwtWinResultCount > mwtWinDimCount)
                        {
                            mwtWinDimCount += dimChunk;
                            Array.Resize(ref peptideNameMwtWin, mwtWinDimCount + 1);
                        }
                    }
                }

                sw.Stop();
                var mwtWinWorkTime = sw.ElapsedMilliseconds;
                Console.WriteLine();
                Console.WriteLine("Processing time (" + mwtWinResultCount + " peptides) = " + mwtWinWorkTime + " msec");
            }

            Console.WriteLine("Check of Tryptic Sequence functions Complete");
        }
    }
}
