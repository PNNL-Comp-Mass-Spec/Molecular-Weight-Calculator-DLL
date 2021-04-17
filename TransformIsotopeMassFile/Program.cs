using System;
using System.IO;

namespace TransformIsotopeMassFile
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("This program reads an element information file downloaded from NIST and transforms the data into a table");
                    Console.WriteLine("Retrieve data from https://www.nist.gov/pml/atomic-weights-and-isotopic-compositions-relative-atomic-masses");
                    Console.WriteLine("Choose the 'Linearized ASCII Output' option and either 'Most common isotopes' or 'All isotopes'");
                    Console.WriteLine("Click 'Get Data' then save the results as a text file");
                    Console.WriteLine();
                    Console.WriteLine("Program written by Matthew Monroe for PNNL (Richland, WA) in 2021");

                    System.Threading.Thread.Sleep(1500);
                    return;
                }

                var inputFile = new FileInfo(args[0]);

                var processor = new IsotopeFileProcessor();
                var success = processor.ProcessFile(inputFile);

                if (!success)
                {
                    System.Threading.Thread.Sleep(1500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                System.Threading.Thread.Sleep(1500);
            }
        }
    }
}