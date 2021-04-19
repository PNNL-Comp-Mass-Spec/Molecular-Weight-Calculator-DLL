using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TransformIsotopeMassFile
{
    internal class IsotopeFileProcessor
    {
        // Ignore Spelling: Sg, Bh, Hs, Ds, Rg, Cn, Nh, Fl, Mc, Lv, Og

        private readonly Dictionary<string, double> mConventionalElementMasses;

        private readonly Regex mUncertaintyMatcher = new(@"(?<Value>[0-9.]+)\((?<Uncertainty>\d+)#?\)", RegexOptions.Compiled);

        private readonly Regex mMassRangeMatcher = new(@"\[(?<StartMass>[0-9.]+),(?<EndMass>[0-9.]+)\]", RegexOptions.Compiled);

        private readonly Regex mRadioactiveElementMass = new(@"\[(?<Mass>[0-9.]+)\]", RegexOptions.Compiled);

        /// <summary>
        /// Converts an integer-based uncertainty value to an absolute number
        /// </summary>
        /// <param name="numericValueText"></param>
        /// <param name="uncertaintyInteger"></param>
        /// <returns>Uncertainty as a decimal number</returns>
        /// <remarks>
        /// If the input file had  "0.99636(20)"
        /// call this method with numericValueText="0.99636" and uncertaintyInteger=20
        /// This method will return 0.00020
        /// </remarks>
        private static double? ComputeAbsoluteUncertainty(string numericValueText, int uncertaintyInteger)
        {
            var decimalPointIndex = numericValueText.IndexOf('.');
            if (decimalPointIndex < 0)
            {
                Console.WriteLine("Warning: number does not have a decimal point; cannot compute the absolute uncertainty for " + numericValueText);
                return null;
            }

            var digitsAfterDecimal = numericValueText.Length - decimalPointIndex - 1;

            var uncertaintyDigitsAsText = uncertaintyInteger.ToString();
            var uncertaintyText = string.Format("0.{0}{1}",
                new string('0', digitsAfterDecimal - uncertaintyDigitsAsText.Length),
                uncertaintyDigitsAsText);

            return double.Parse(uncertaintyText);
        }

        private static IsotopeInfo FindClosestIsotope(
            string elementSymbol,
            IReadOnlyList<IsotopeInfo> elementIsotopes,
            IReadOnlyDictionary<string, double> radioactiveElementIsotopeMasses)
        {
            if (elementIsotopes.Count == 1)
            {
                return elementIsotopes[0];
            }

            if (!radioactiveElementIsotopeMasses.TryGetValue(elementSymbol, out var bestIsotopeMass))
            {
                throw new Exception(string.Format(
                    "Element {0} has more than one isotope defined, but no isotopic composition; add this element to dictionary radioactiveElementIsotopeMasses",
                    elementSymbol));
            }

            var bestIsotope = elementIsotopes[0];
            if (!bestIsotope.RelativeAtomicMass.HasValue)
            {
                throw new Exception(string.Format(
                    "Isotope {0} for {1} does not have a monoisotopic mass value stored in the RelativeAtomicMass field",
                    bestIsotope.MassNumber, bestIsotope.AtomicSymbol));
            }

            var bestMassDifference = Math.Abs(bestIsotopeMass - bestIsotope.RelativeAtomicMass.Value);

            for (var i = 1; i < elementIsotopes.Count; i++)
            {
                var isotope = elementIsotopes[i];

                if (!isotope.RelativeAtomicMass.HasValue)
                {
                    throw new Exception(string.Format(
                        "{0} isotope {1} does not have a monoisotopic mass value stored in the RelativeAtomicMass field",
                        isotope.AtomicSymbol, isotope.MassNumber));
                }

                var massDifference = Math.Abs(bestIsotopeMass - isotope.RelativeAtomicMass.Value);
                if (massDifference >= bestMassDifference)
                    continue;

                bestIsotope = isotope;
                bestMassDifference = massDifference;
            }

            return bestIsotope;
        }

        /// <summary>
        /// Check whether valueText is of the form "0.99757(16)"
        /// If it is, extract out the numeric value and convert the uncertainty to a decimal number
        /// </summary>
        /// <param name="valueText"></param>
        /// <param name="numericValue"></param>
        /// <param name="uncertainty"></param>
        /// <returns>True if valueText is a double followed by an integer in parentheses, otherwise false</returns>
        /// <remarks>
        /// If valueText is 0.99757(16), returns
        /// numericValue = 0.99757 and
        /// uncertainty  = 0.00016
        /// </remarks>
        private bool GetValueAndUncertainty(string valueText, out double? numericValue, out double? uncertainty)
        {
            var match = mUncertaintyMatcher.Match(valueText);
            if (match.Success)
            {
                var numericValueText = match.Groups["Value"].Value;
                numericValue = double.Parse(numericValueText);

                var uncertaintyDigits = int.Parse(match.Groups["Uncertainty"].Value);
                uncertainty = ComputeAbsoluteUncertainty(numericValueText, uncertaintyDigits);

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

        /// <summary>
        /// Constructor
        /// </summary>
        public IsotopeFileProcessor()
        {
            // 13 elements show a range of mass values
            // For the average mass, use the IUPAC approved conventional mass

            // The conventional mass values were obtained from Table 3
            // "Atomic weights of the elements 2013 (IUPAC Technical Report)"
            // Published in Pure and Applied Chemistry, Volume 88, Issue 3
            // https://doi.org/10.1515/pac-2015-0305

            mConventionalElementMasses = new Dictionary<string, double>
            {
                {"H", 1.008},
                {"D", 1.008},   // Isotope of hydrogen; use the mass of the most common element for the average mass (as we do for other isotopes)
                {"T", 1.008},   // Isotope of hydrogen
                {"Li", 6.94},
                {"B", 10.81},
                {"C", 12.011},
                {"N", 14.007},
                {"O", 15.999},
                {"Mg", 24.305},
                {"Si", 28.085},
                {"S", 32.06},
                {"Cl", 35.45},
                {"Br", 79.904},
                {"Tl", 204.38}
            };
        }

        private static string NullableValueToString(double? nullableValue)
        {
            return nullableValue.HasValue ? nullableValue.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        public bool ProcessFile(FileInfo inputFile, string outputFileName = "", string elementsFileName= "")
        {
            try
            {
                if (!inputFile.Exists)
                {
                    Console.WriteLine("File not found: " + inputFile.FullName);
                    return false;
                }

                if (inputFile.DirectoryName == null)
                {
                    Console.WriteLine("Unable to determine the parent directory of the input file: " + inputFile.FullName);
                    return false;
                }

                string baseName;
                string outputFileSuffix;
                if (inputFile.Name.EndsWith("_Linearized.txt", StringComparison.OrdinalIgnoreCase))
                {
                    baseName = inputFile.Name.Replace("_Linearized.txt", string.Empty);
                    outputFileSuffix = "_Tabular_WithUncertainty.txt";
                }
                else
                {
                    baseName = Path.GetFileNameWithoutExtension(inputFile.Name);
                    outputFileSuffix = "_Tabular.txt";
                }

                if (string.IsNullOrWhiteSpace(outputFileName))
                {
                    outputFileName = baseName + outputFileSuffix;
                }

                if (string.IsNullOrWhiteSpace(elementsFileName))
                {
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

        private bool ReadIsotopeInfo(StreamReader reader, ICollection<IsotopeInfo> isotopes, string atomicNumberLine)
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
                                // 13 elements show a range of mass values, e.g. [6.938,6.997]
                                // For the average mass, use the IUPAC approved conventional mass

                                var startMass = double.Parse(rangeMatch.Groups["StartMass"].Value);
                                var endMass = double.Parse(rangeMatch.Groups["EndMass"].Value);

                                if (!mConventionalElementMasses.TryGetValue(currentIsotope.AtomicSymbol, out var averageMass))
                                {
                                    Console.WriteLine("Error: unable to determine the conventional mass of {0}", currentIsotope.AtomicSymbol);

                                    // Could do this, but it's not accurate:
                                    // averageMass = Math.Round((startMass + endMass) / 2, 6);
                                    return false;
                                }

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

            // Keys in this dictionary are element symbols, values are the best isotopic mass for each one
            // This is primarily used for radioactive elements, but it is also used for Ge
            var bestIsotopeByElement = new Dictionary<string, double> {
                { "Ge", 71.92207583 },
                { "Tc", 97.9072124  },
                { "Pm", 144.9127559 },
                { "Po", 208.9824308 },
                { "At", 209.9871479 },
                { "Rn", 222.0175782 },
                { "Fr", 223.019736  },
                { "Ra", 226.0254103 },
                { "Ac", 227.0277523 },
                { "Np", 237.0481736 },
                { "Pu", 244.0642053 },
                { "Am", 243.0613813 },
                { "Cm", 247.0703541 },
                { "Bk", 247.0703073 },
                { "Cf", 251.0795886 },
                { "Es", 252.08298   },
                { "Fm", 257.0951061 },
                { "Md", 258.0984315 },
                { "No", 259.10103   },
                { "Lr", 262.10961   },
                { "Rf", 267.12179   },
                { "Db", 268.12567   },
                { "Sg", 271.13393   },
                { "Bh", 272.13826   },
                { "Hs", 270.13429   },
                { "Mt", 276.15159   },
                { "Ds", 281.16451   },
                { "Rg", 280.16514   },
                { "Cn", 285.17712   },
                { "Nh", 284.17873   },
                { "Fl", 289.19042   },
                { "Mc", 288.19274   },
                { "Lv", 293.20449   },
                { "Ts", 292.20746   },
                { "Og", 294.21392   }
            };

            var mostAbundantIsotopeByElement = new List<IsotopeInfo>();

            foreach (var elementSymbol in elements.Keys)
            {
                if (elementSymbol.Equals("D") || elementSymbol.Equals("T"))
                    continue;

                var elementIsotopes = elements[elementSymbol];
                var elementAdded = false;

                if (elementSymbol.Equals("Ge"))
                {
                    // Even though ^74Ge is more abundant than ^72Ge (36% vs. 27%), use the isotopic mass of ^72Ge
                    var bestIsotope = FindClosestIsotope(elementSymbol, elementIsotopes, bestIsotopeByElement);
                    mostAbundantIsotopeByElement.Add(bestIsotope);
                    continue;
                }

                foreach (var item in (from isotope in elementIsotopes orderby isotope.IsotopicComposition descending select isotope))
                {
                    if (item.IsotopicComposition.HasValue)
                    {
                        mostAbundantIsotopeByElement.Add(item);
                        elementAdded = true;
                        break;
                    }
                }

                if (elementAdded)
                    continue;

                // Radioactive elements do not have values defined for Isotopic Composition
                // Lookup the best isotope to use
                var bestRadioactiveIsotope = FindClosestIsotope(elementSymbol, elementIsotopes, bestIsotopeByElement);
                mostAbundantIsotopeByElement.Add(bestRadioactiveIsotope);
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
                    "Relative Atomic Mass (monoisotopic)",
                    "Relative Atomic Mass Uncertainty",
                    "Isotopic Composition",
                    "Isotopic Composition Uncertainty",
                    "Standard Atomic Weight (average mass)",
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
    }
}
