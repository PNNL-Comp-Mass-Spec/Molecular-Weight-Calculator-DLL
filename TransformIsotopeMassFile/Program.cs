using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TransformIsotopeMassFile
{
    internal static class Program
    {
        private static readonly Regex mUncertaintyMatcher = new(@"(?<Value>[0-9.]+)\((?<Uncertainty>\d+)#?\)", RegexOptions.Compiled);

        private static readonly Regex mMassRangeMatcher = new(@"\[(?<StartMass>[0-9.]+),(?<EndMass>[0-9.]+)\]", RegexOptions.Compiled);

        private static readonly Regex mRadioactiveElementMass = new(@"\[(?<Mass>[0-9.]+)\]", RegexOptions.Compiled);

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

                var success = ProcessFile(args[0]);
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

        private static bool ProcessFile(string inputFilePath)
        {
            try
            {
                var inputFile = new FileInfo(inputFilePath);
                if (!inputFile.Exists)
                {
                    Console.WriteLine("File not found: " + inputFilePath);
                    return false;
                }

                if (inputFile.DirectoryName == null)
                {
                    Console.WriteLine("Unable to determine the parent directory of the input file: " + inputFilePath);
                    return false;
                }

                string outputFileName;
                string elementsFileName;
                if (inputFile.Name.EndsWith("_Linearized.txt", StringComparison.OrdinalIgnoreCase))
                {
                    var baseName = inputFile.Name.Replace("_Linearized.txt", string.Empty);
                    outputFileName = baseName + "_Tabular_WithUncertainty.txt";
                    elementsFileName = baseName + "_Elements.txt";
                }
                else
                {
                    var baseName = Path.GetFileNameWithoutExtension(inputFile.Name);
                    outputFileName = baseName + "_Tabular.txt";
                    elementsFileName = baseName + "_Elements.txt";
                }

                var outputFilePath = Path.Combine(inputFile.DirectoryName, outputFileName);
                var elementFilePath = Path.Combine(inputFile.DirectoryName, elementsFileName);

                using var reader = new StreamReader(new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                // This list has one entry for each isotope read from the input file
                var isotopes = new List<IsotopeInfo>();

                while (!reader.EndOfStream)
                {
                    var dataLine = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(dataLine) || !char.IsLetter(dataLine[0]))
                        continue;

                    if (dataLine.Trim().StartsWith("Atomic Number"))
                    {
                        var success = ReadIsotopeInfo(reader, isotopes, dataLine);

                        if (!success)
                            return false;
                    }
                }

                var writeSuccess = WriteTabularIsotopeFile(isotopes, outputFilePath);
                if (!writeSuccess)
                    return false;

                return WriteElementsFile(isotopes, elementFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ProcessFile: " + ex.Message);
                return false;
            }
        }

        private static bool GetValueAndUncertainty(string valueText, out double? numericValue, out int? uncertainty)
        {
            var match = mUncertaintyMatcher.Match(valueText);
            if (match.Success)
            {
                numericValue = double.Parse(match.Groups["Value"].Value);
                uncertainty = int.Parse(match.Groups["Uncertainty"].Value);
                return true;
            }

            uncertainty = null;

            if (double.TryParse(valueText, out var value))
            {
                numericValue = value;
                return true;
            }

            numericValue = null;
            return false;
        }

        private static bool ReadIsotopeInfo(StreamReader reader, ICollection<IsotopeInfo> isotopes, string atomicNumberLine)
        {
            try
            {
                var firstLineParts = atomicNumberLine.Split('=', 2);

                if (firstLineParts.Length < 2)
                {
                    Console.WriteLine("Error: atomic number line does not have an equals sign: " + atomicNumberLine);
                    return false;
                }

                if (!int.TryParse(firstLineParts[1], out var atomicNumber))
                {
                    Console.WriteLine("Error: integer not found after the equals sign: " + atomicNumberLine);
                    return false;
                }

                var currentIsotope = new IsotopeInfo(atomicNumber);
                isotopes.Add(currentIsotope);

                while (!reader.EndOfStream)
                {
                    var dataLine = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(dataLine) || !char.IsLetter(dataLine[0]))
                    {
                        return true;
                    }

                    var lineParts = dataLine.Split('=', 2);
                    if (lineParts.Length < 2)
                        continue;

                    var keyWord = lineParts[0].Trim();
                    var value = lineParts[1].Trim();

                    switch (keyWord)
                    {
                        case "Atomic Symbol":
                            currentIsotope.AtomicSymbol = value;
                            break;

                        case "Mass Number":
                            if (!int.TryParse(value, out var massNumber))
                            {
                                Console.WriteLine("Error for atomic number {0}: integer not found after the equals sign: {1}",
                                    currentIsotope.AtomicNumber, dataLine);
                                return false;
                            }

                            currentIsotope.MassNumber = massNumber;
                            break;

                        case "Relative Atomic Mass":
                            if (!GetValueAndUncertainty(value, out var numericValue, out var monoisotopicMassUncertainty))
                            {
                                Console.WriteLine("Error for atomic number {0}: integer not found after the equals sign: {1}",
                                    currentIsotope.AtomicNumber, dataLine);
                                return false;
                            }

                            currentIsotope.RelativeAtomicMass = numericValue;
                            currentIsotope.RelativeAtomicMassUncertainty = monoisotopicMassUncertainty;
                            break;

                        case "Isotopic Composition":
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                // This is sometimes not defined; that's OK
                                continue;
                            }

                            if (!GetValueAndUncertainty(value, out var isotopicComposition, out var compositionUncertainty))
                            {
                                Console.WriteLine("Error for atomic number {0}: integer not found after the equals sign: {1}",
                                    currentIsotope.AtomicNumber, dataLine);
                                return false;
                            }

                            currentIsotope.IsotopicComposition = isotopicComposition;
                            currentIsotope.IsotopicCompositionUncertainty = compositionUncertainty;
                            break;

                        case "Standard Atomic Weight":
                            if (string.IsNullOrWhiteSpace(value) && currentIsotope.AtomicNumber >= 95)
                            {
                                // Radioactive element without a standard atomic weight; this is OK
                                continue;
                            }

                            var rangeMatch = mMassRangeMatcher.Match(value);
                            if (rangeMatch.Success)
                            {
                                var startMass = double.Parse(rangeMatch.Groups["StartMass"].Value);
                                var endMass = double.Parse(rangeMatch.Groups["EndMass"].Value);
                                var averageMass = Math.Round((startMass + endMass) / 2, 6);
                                var averageMassUncertainty = Math.Round((endMass - startMass) / 2, 6);

                                currentIsotope.StandardAtomicWeight = averageMass;
                                currentIsotope.StandardAtomicWeightUncertainty = averageMassUncertainty;
                                continue;
                            }

                            if (GetValueAndUncertainty(value, out var standardAtomicWeight, out var standardAtomicWeightUncertainty))
                            {
                                currentIsotope.StandardAtomicWeight = standardAtomicWeight;
                                currentIsotope.StandardAtomicWeightUncertainty = standardAtomicWeightUncertainty;
                                continue;
                            }

                            var radioactiveMassMatch = mRadioactiveElementMass.Match(value);
                            if (radioactiveMassMatch.Success)
                            {
                                currentIsotope.StandardAtomicWeight = double.Parse(radioactiveMassMatch.Groups["Mass"].Value);
                                continue;
                            }

                            Console.WriteLine("Error for atomic number {0}: unrecognized value for Standard Atomic Weight: {1}",
                                currentIsotope.AtomicNumber, dataLine);
                            return false;

                        case "Notes":
                            currentIsotope.Notes = value;
                            break;

                        default:
                            Console.WriteLine("Warning: unrecognized keyword '{0}'", keyWord);
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ReadIsotopeInfo: " + ex.Message);
                return false;
            }
        }

        private static bool WriteElementsFile(IEnumerable<IsotopeInfo> isotopes, string outputFilePath)
        {

            // Keys are element name, values are the list of isotopes for that element
            var elements = new Dictionary<string, List<IsotopeInfo>>();
            foreach (var item in isotopes)
            {
                if (elements.TryGetValue(item.AtomicSymbol, out var elementIsotopes))
                {
                    elementIsotopes.Add(item);
                    continue;
                }

                elements.Add(item.AtomicSymbol, new List<IsotopeInfo> { item });
            }

            var mostAbundantIsotopeByElement = new List<IsotopeInfo>();

            foreach (var element in elements.Keys)
            {
                if (element.Equals("D") || element.Equals("T"))
                    continue;

                var elementIsotopes = elements[element];

                foreach (var item in (from isotope in elementIsotopes orderby isotope.IsotopicComposition descending select isotope).Take(1))
                {
                    mostAbundantIsotopeByElement.Add(item);
                    break;
                }
            }

            return WriteTabularIsotopeFile(mostAbundantIsotopeByElement, outputFilePath);
        }

        private static bool WriteTabularIsotopeFile(IEnumerable<IsotopeInfo> isotopes, string outputFilePath)
        {
            try
            {
                using var writer = new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite));

                var dataColumns = new List<string>
                {
                    "Atomic Number",
                    "Atomic Symbol",
                    "Mass Number",
                    "Monoisotopic Mass",
                    "Monoisotopic Mass Uncertainty",
                    "Isotopic Composition",
                    "Isotopic Composition Uncertainty",
                    "Standard Atomic Weight",
                    "Standard Weight Uncertainty",
                    "Notes"
                };

                writer.WriteLine(string.Join('\t', dataColumns));

                foreach (var isotope in isotopes)
                {
                    dataColumns.Clear();
                    dataColumns.Add(isotope.AtomicNumber.ToString());
                    dataColumns.Add(isotope.AtomicSymbol);
                    dataColumns.Add(isotope.MassNumber.ToString());
                    dataColumns.Add(NullableValueToString(isotope.RelativeAtomicMass));
                    dataColumns.Add(NullableValueToString(isotope.RelativeAtomicMassUncertainty));
                    dataColumns.Add(NullableValueToString(isotope.IsotopicComposition));
                    dataColumns.Add(NullableValueToString(isotope.IsotopicCompositionUncertainty));
                    dataColumns.Add(NullableValueToString(isotope.StandardAtomicWeight));
                    dataColumns.Add(NullableValueToString(isotope.StandardAtomicWeightUncertainty));
                    dataColumns.Add(isotope.Notes);

                    writer.WriteLine(string.Join('\t', dataColumns));
                }

                Console.WriteLine("Created file " + outputFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in WriteTabularIsotopeFile: " + ex.Message);
                return false;
            }
        }

        private static string NullableValueToString(double? nullableValue)
        {
            return nullableValue.HasValue ? nullableValue.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }
    }
}