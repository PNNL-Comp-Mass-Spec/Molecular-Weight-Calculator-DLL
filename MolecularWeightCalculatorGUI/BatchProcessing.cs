using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.FormulaFinder;
using MolecularWeightCalculator.Sequence;
using MolecularWeightCalculatorGUI.Utilities;

namespace MolecularWeightCalculatorGUI
{
    public class BatchProcessing
    {
        private const string COMMENT_CHAR = ";";
        public BatchProcessing(MolecularWeightTool molWt)
        {
            mwt = molWt;

            Finder.WarningEvent += Finder_WarningEvent;
            Finder.ErrorEvent += Finder_ErrorEvent;
            Finder.MessageEvent += Finder_MessageEvent;

            compound = new Compound(mwt.ElementAndMass);
        }

        private readonly MolecularWeightTool mwt;
        private Messages Messages => mwt.ElementAndMass.Messages;
        private FormulaSearcher Finder => mwt.FormulaFinder;
        private readonly Compound compound;
        private ProgressValues progressData;
        private CancellationTokenSource cancelTokenSource;

        public bool AbortProcessing
        {
            get => cancelTokenSource.IsCancellationRequested;
            set
            {
                if (value && !cancelTokenSource.IsCancellationRequested)
                {
                    cancelTokenSource.Cancel();
                }
            }
        }

        private bool stdDevModeEnabled = false; // TODO: gBlnShowStdDevWithMass;

        // Set default output options
        private bool verboseMode = true;
        private string delimiter = "\t";
        private bool showWeight = true;
        private bool showInputSequence = true;
        private bool showSourceFormula = true;
        private bool echoComments = false;
        private bool showCapitalizedFormula = false;
        private bool convertToEmpiricalFormula = false;
        private bool oneLetterPeptideWeightMode = false;
        private bool separateWithDash = false;
        private bool aminoAcidConvert1To3 = false;
        private bool aminoAcidConvert3To1 = false;
        private bool spaceEvery10 = false;
        private bool expandAbbreviations = false;
        private double finderWeightTolerance = -1d;

        // Default AA weight mode prefix and suffix
        private string peptideWeightModePrefixFormula = "H";     // Atoms that make up the prefix group for Peptide weight mode; default is H
        private string peptideWeightModeSuffixFormula = "OH";    // Atoms that make up the suffix group for Peptide weight mode; default is OH

        // Isotopic Distribution Options
        private int isotopicDistributionChargeState = 1;
        private string headerIsotopicAbundance = "Isotopic Abundances for";
        private string headerMass = "Mass/Charge";
        private string headerFraction = "Fraction";
        private string headerIntensity = "Intensity";
        private bool addProtonChargeCarrier = true;

        private bool formulaFinderMode = false;
        private bool formulaFinderModeHasCustomMass = false;
        private bool isotopicDistributionMode = false;

        private void ResetValues()
        {
            cancelTokenSource = new CancellationTokenSource();
            // Make sure we abort FormulaSearcher processing as well when we abort processing.
            cancelTokenSource.Token.Register(() => Finder.AbortProcessingNow());

            verboseMode = true;
            stdDevModeEnabled = false; // TODO: gBlnShowStdDevWithMass;

            // Set default output options
            delimiter = "\t";
            showWeight = true;
            showInputSequence = true;
            showSourceFormula = true;
            echoComments = false;
            showCapitalizedFormula = false;
            convertToEmpiricalFormula = false;
            oneLetterPeptideWeightMode = false;
            separateWithDash = false;
            aminoAcidConvert1To3 = false;
            aminoAcidConvert3To1 = false;
            spaceEvery10 = false;
            expandAbbreviations = false;
            finderWeightTolerance = -1d;

            // Default AA weight mode prefix and suffix
            peptideWeightModePrefixFormula = "H"; // Atoms that make up the prefix group for Peptide weight mode; default is H
            peptideWeightModeSuffixFormula = "OH"; // Atoms that make up the suffix group for Peptide weight mode; default is OH

            // Isotopic Distribution Options
            isotopicDistributionChargeState = 1;
            headerIsotopicAbundance = "Isotopic Abundances for";
            headerMass = "Mass/Charge";
            headerFraction = "Fraction";
            headerIntensity = "Intensity";
            addProtonChargeCarrier = true;

            formulaFinderMode = false;
            formulaFinderModeHasCustomMass = false;
            isotopicDistributionMode = false;
        }

        /// <summary>
        /// Returns True if text starts with ; or '
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsComment(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            var trimmed = text.Trim();
            return trimmed.StartsWith(COMMENT_CHAR) || trimmed.StartsWith("'");
        }

        internal async Task BatchProcessTextFile(Window parent, IProgress<ProgressValues> progress)
        {
            progress ??= new Progress<ProgressValues>(x => { });
            progress.Report(new ProgressValues(false));

            // Use Open dialog to choose file
            var dlg = new OpenFileDialog
            {
                Title = "Select File",
                Filter = "Text Files|*.txt",
            };

            var result = dlg.ShowDialog(parent);

            if (!(result ?? false) || string.IsNullOrWhiteSpace(dlg.FileName))
            {
                // No file selected (or other error)
                return;
            }

            var inputFileName = dlg.FileName;
            var outputFileName = inputFileName + ".out.txt";

            if (File.Exists(outputFileName) /* TODO: && (frmProgramPreferences.optExitConfirmation(exmEscapeKeyConfirmExit).value == true || frmProgramPreferences.optExitConfirmation(exmIgnoreEscapeKeyConfirmExit).value == true) */ )
            {
                var response = MessageBox.Show(parent, outputFileName + ": " + Messages.LookupMessage(490), Messages.LookupMessage(500), MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (response != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                await Task.Factory.StartNew(() => BatchProcessTextFileWork(inputFileName, outputFileName, progress));
            }
            catch (Exception ex)
            {
                var message = Messages.LookupMessage(510) + ": " + inputFileName + " or " + outputFileName;
                message = message + "\r\n" + ex.Message;
                message = message + "\r\n" + Messages.LookupMessage(515);
                MessageBox.Show(parent, message, Messages.LookupMessage(350), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void BatchProcessTextFile(string inputFileName, string outputFileName, bool overwriteWithoutAsking)
        {
            // Use Open dialog to choose file
            if (!File.Exists(inputFileName))
            {
                Console.WriteLine("{0}: {1}", Messages.LookupMessage(480), Messages.LookupMessage(470, " (" + inputFileName + ")"));
                return;
            }

            if (string.IsNullOrWhiteSpace(outputFileName) || outputFileName == inputFileName)
                outputFileName = inputFileName + ".out.txt";

            if (!overwriteWithoutAsking && File.Exists(outputFileName))
            {
                var response = MessageBox.Show(outputFileName + ": " + Messages.LookupMessage(490), Messages.LookupMessage(500), MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (response != MessageBoxResult.Yes)
                    return;
                Console.WriteLine("{0}: {1}", Messages.LookupMessage(500), outputFileName + ": " + Messages.LookupMessage(490));
                Console.Write("Enter 'y' to confirm, anything else to cancel: ");
                var resp = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(resp) && !resp.StartsWith("Y", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            try
            {
                BatchProcessTextFileWork(inputFileName, outputFileName, new Progress<ProgressValues>(x =>
                {
                    if (x.ShowProgress)
                    {
                        Console.Write("\r{0:F1}% Complete; {1} - {2}", x.Progress, x.Status, x.SubStatus);
                    }
                }));

                Console.WriteLine("\rCompleted batch analysis.");
            }
            catch (Exception ex)
            {
                var message = Messages.LookupMessage(510) + ": " + inputFileName + " or " + outputFileName;
                message = message + "\r\n" + ex.Message;
                message = message + "\r\n" + Messages.LookupMessage(515);
                Console.WriteLine(message);
            }
        }

        private void BatchProcessTextFileWork(string inputFileName, string outputFileName, IProgress<ProgressValues> progress)
        {
            progressData = new ProgressValues(false);
            ResetValues();
            finderMessages.Clear();

            var linesRead = 0;
            var formulasProcessed = 0;

            StreamReader fileInput = null;
            StreamWriter fileOutput = null;

            // Save the current standard deviation mode and retrieve the current state of displaying the StdDev Mode
            var elementModeSaved = mwt.GetElementMode();
            var stdDevModeSaved = mwt.StdDevMode;

            progressData.ShowProgress = true;
            progress.Report(progressData);
            try
            {
                // Open the file for input
                fileInput = new StreamReader(new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                fileOutput = new StreamWriter(new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite));

                progressData.Status = "Batch analyzing";
                progress.Report(progressData);

                // Get the number of lines in the file with a no-processing read of all lines
                var totalLines = 0d;
                while (fileInput.ReadLine() != null)
                {
                    totalLines++;
                }

                // Reset fileInput to the start of the file
                fileInput.BaseStream.Seek(0, SeekOrigin.Begin);
                fileInput.DiscardBufferedData();

                // Note: 'is { } workText' (not-null pattern) is recommended instead of 'is string workText', because the return value is known to be a string.
                while (fileInput.ReadLine() is { } workText)
                {
                    linesRead++;

                    if (string.IsNullOrWhiteSpace(workText) || IsComment(workText))
                    {
                        if (echoComments)
                        {
                            fileOutput.WriteLine(workText);
                        }
                    }
                    else
                    {

                        // Examine workText to see if it contains an = sign
                        // If so, then it is a batch processing Command
                        var matchIndex = workText.IndexOf("=");
                        if (matchIndex > 0)
                        {
                            // Command Found
                            var command = workText.Substring(0, matchIndex).Trim().ToUpper();
                            var settings = workText.Substring(matchIndex + 1);

                            ParseCommand(command, settings, fileOutput, progress);
                        }
                        else
                        {
                            if (cancelTokenSource.IsCancellationRequested)
                            {
                                fileOutput.WriteLine(COMMENT_CHAR + " Error: Processing aborted");

                                break;
                            }

                            formulasProcessed++;
                            if (formulaFinderMode)
                            {
                                // Using Formula Finder
                                ProcessFormulaFinder(workText, fileOutput, progress);
                            }
                            else
                            {
                                // Not formula finder mode
                                var processed = Process(workText);

                                // Write outLine to the output file
                                fileOutput.WriteLine(processed);
                            }
                        }
                    }

                    if (formulasProcessed % 50 == 0 || totalLines < 75)
                    {
                        progressData.Progress = linesRead / totalLines * 100.0;
                        progressData.SubStatus = "Processed " + formulasProcessed + " formulas";
                        progress.Report(progressData);
                    }
                }
            }
            finally
            {
                progressData.ShowProgress = false;
                progress.Report(progressData);

                // Restore the standard deviation mode to the saved value
                mwt.StdDevMode = stdDevModeSaved;
                mwt.SetElementMode(elementModeSaved);

                fileInput?.Close();
                fileOutput?.Close();
            }
        }

        private void ParseCommand(string command, string settings, StreamWriter fileOutput, IProgress<ProgressValues> progress)
        {
            var settingsUpperCase = settings.ToUpper();

            switch (command)
            {
                case "VERBOSEMODE":
                    verboseMode = ParseBooleanSetting(settingsUpperCase);
                    if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Verbose mode is now on");

                    break;

                case "DELIMETER":
                case "DELIMITER":
                    if (settings.StartsWith("<") && settings.EndsWith(">"))
                    {
                        // Special delimiter
                        switch (settings.Trim('<', '>').ToUpper())
                        {
                            case "TAB":
                                delimiter = "\t";
                                if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Delimiter now a Tab");

                                break;

                            case "SPACE":
                                delimiter = " ";
                                if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Delimiter now a Space");

                                break;

                            case "ENTER":
                            case "CRLF":
                                delimiter = "\r\n";
                                if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Delimiter now a Carriage Return (Enter)");

                                break;

                            default:
                                fileOutput.WriteLine(COMMENT_CHAR + " Unknown delimiter code: " + settings + " -- Should be one of the following: <TAB>, <SPACE>, <ENTER>, <CRLF>");

                                break;
                        }
                    }
                    else if (settings.Length > 0)
                    {
                        // Normal text delimiter (one or more characters)
                        delimiter = settings;
                        if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Delimiter now " + delimiter);
                    }
                    else
                    {
                        if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Delimiter reset to default (Tab)");

                        delimiter = "\t";
                    }

                    break;

                case "ECHOCOMMENTS":
                    echoComments = ParseBooleanSetting(settingsUpperCase, fileOutput, "Will now write comments present in the source file to the output file", "Comments found in the source file will not be written to the output file");
                    break;

                case "MW":
                    if (formulaFinderMode)
                    {
                        formulaFinderMode = false;
                    }
                    isotopicDistributionMode = false;
                    showWeight = true;
                    aminoAcidConvert3To1 = false;
                    aminoAcidConvert1To3 = false;
                    convertToEmpiricalFormula = false;
                    expandAbbreviations = false;
                    oneLetterPeptideWeightMode = false;
                    if (verboseMode)
                    {
                        fileOutput.WriteLine("");
                        fileOutput.WriteLine(COMMENT_CHAR + " Normal Molecular Weight Mode Enabled (other modes turned Off)");
                    }

                    break;

                case "CAPITALIZED":
                    showCapitalizedFormula = ParseBooleanSetting(settingsUpperCase, fileOutput, "Source formula will be displayed with proper capitalization", "Source formula will be displayed exactly as found in the input file");

                    if (showCapitalizedFormula)
                    {
                        showSourceFormula = true;
                    }
                    break;

                case "MWSHOWSOURCEFORMULA":
                    showSourceFormula = ParseBooleanSetting(settingsUpperCase, fileOutput, "Display of source formula is now On", "Display of source formula is now Off");
                    break;

                case "EMPIRICAL":
                    if (verboseMode) fileOutput.WriteLine("");

                    // Enable conversion of formulas to their empirical formulas
                    convertToEmpiricalFormula = ParseBooleanSetting(settingsUpperCase, fileOutput, "Converting formulas to empirical formulas now On", "Converting formulas to empirical formulas now Off");

                    break;

                case "EXPANDABBREVIATIONS":
                    if (verboseMode) fileOutput.WriteLine("");

                    // Enable expansion of abbreviations
                    expandAbbreviations = ParseBooleanSetting(settingsUpperCase, fileOutput, "Abbreviation expansion now On", "Abbreviation expansion now Off");

                    break;

                case "SHOWWEIGHT":
                    // Show molecular weight
                    showWeight = ParseBooleanSetting(settingsUpperCase, fileOutput, "Will display the molecular weight (mass) of each formula", "Will not display the molecular weight (mass) of each formula");
                    break;

                case "STDDEVMODE":
                    switch (settingsUpperCase)
                    {
                        case "SHORT":
                            mwt.StdDevMode = StdDevMode.Short;
                            if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Standard deviation mode using Short display");

                            stdDevModeEnabled = true;
                            break;

                        case "SCIENTIFIC":
                            mwt.StdDevMode = StdDevMode.Scientific;
                            if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Standard deviation mode using Scientific display");

                            stdDevModeEnabled = true;
                            break;

                        case "DECIMAL":
                            mwt.StdDevMode = StdDevMode.Decimal;
                            if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Standard deviation mode using Decimal display");

                            stdDevModeEnabled = true;
                            break;

                        case "OFF":
                            stdDevModeEnabled = false;
                            if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Standard deviations will not be displayed");

                            break;

                        default:
                            fileOutput.WriteLine(COMMENT_CHAR + " Warning: Invalid standard deviation mode: " + settingsUpperCase + " -- Should be one of the following: SHORT, SCIENTIFIC, DECIMAL, OFF");

                            break;
                    }

                    break;

                case "WEIGHTMODE":
                    switch (settingsUpperCase)
                    {
                        case "AVERAGE":
                            mwt.SetElementMode(ElementMassMode.Average);
                            if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Average Weight Mode Enabled");

                            break;

                        case "ISOTOPIC":
                            mwt.SetElementMode(ElementMassMode.Isotopic);
                            if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Isotopic Weight Mode Enabled");

                            break;

                        case "INTEGER":
                            mwt.SetElementMode(ElementMassMode.Integer);
                            if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Integer Weight Mode Enabled");

                            break;

                        default:
                            fileOutput.WriteLine(COMMENT_CHAR + " Warning: Invalid elemental weight mode: " + settingsUpperCase + " -- Should be one of the following: AVERAGE, ISOTOPIC, INTEGER");

                            break;
                    }

                    break;

                case "ONELETTERPEPTIDEWEIGHTMODE":
                    // Show molecular weight
                    if (verboseMode)
                    {
                        fileOutput.WriteLine("");
                    }

                    oneLetterPeptideWeightMode = ParseBooleanSetting(settingsUpperCase, fileOutput, "One letter Amino Acid weight mode: input formulas are assumed to be peptides in one-letter notation", " ");
                    if (oneLetterPeptideWeightMode)
                    {
                        if (formulaFinderMode)
                        {
                            formulaFinderMode = false;
                        }

                        isotopicDistributionMode = false;
                        showWeight = true;
                        aminoAcidConvert3To1 = false;
                        aminoAcidConvert1To3 = false;
                        convertToEmpiricalFormula = false;
                        expandAbbreviations = false;
                    }

                    break;

                case "PEPTIDEWEIGHTMODEPEPTIDEPREFIX":
                    peptideWeightModePrefixFormula = settings;
                    break;

                case "PEPTIDEWEIGHTMODEPEPTIDESUFFIX":
                    peptideWeightModeSuffixFormula = settings;
                    break;

                case "AACONVERT3TO1":
                    // Treat settings as an amino acid with 3 letter symbols
                    // Convert to 1 letter symbols
                    if (verboseMode) fileOutput.WriteLine("");

                    aminoAcidConvert3To1 = ParseBooleanSetting(settingsUpperCase, fileOutput, "3 letter to 1 letter amino acid symbol conversion now On", "3 letter to 1 letter amino acid symbol conversion now Off");

                    aminoAcidConvert1To3 = false;
                    formulaFinderMode = false;
                    isotopicDistributionMode = false;
                    break;

                case "AACONVERT1TO3":
                    // Treat settings as an amino acid with 1 letter symbols
                    // Convert to 3 letter symbols
                    if (verboseMode) fileOutput.WriteLine("");

                    aminoAcidConvert1To3 = ParseBooleanSetting(settingsUpperCase, fileOutput, "1 letter to 3 letter amino acid symbol conversion now On", "1 letter to 3 letter amino acid symbol conversion Off");

                    aminoAcidConvert3To1 = false;
                    formulaFinderMode = false;
                    isotopicDistributionMode = false;
                    break;

                case "AASPACEEVERY10":
                    spaceEvery10 = ParseBooleanSetting(settingsUpperCase, fileOutput, "Will add a space every 10 amino acids", "Will not add a space every 10 amino acids");
                    break;

                case "AA1TO3USEDASH":
                    separateWithDash = ParseBooleanSetting(settingsUpperCase, fileOutput, "Will separate residues with a dash", "Will not separate residues with a dash");
                    break;

                case "AASHOWSEQUENCEBEINGCONVERTED":
                    showInputSequence = ParseBooleanSetting(settingsUpperCase, fileOutput, "Will show sequence being converted, in addition to the converted sequence", "Will only show the converted sequence, not the sequence being converted");
                    break;

                case "FF":
                    if (!formulaFinderMode)
                    {
                        formulaFinderMode = true;
                        progressData.Status = "Batch analyzing";
                        progress.Report(progressData);
                    }

                    // Examine settings to see if any elements/abbreviations are specified
                    // If yes, construct an array of the elements/abbreviations
                    // Once constructed, select the appropriate elements on the form

                    if (settings.Length > 0)
                    {
                        var potentialElements = settings.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (potentialElements.Length > 0)
                        {
                            // Uncheck all of the currently selected search elements on frmFinder
                            Finder.CandidateElements.Clear();
                            formulaFinderModeHasCustomMass = false;

                            var customElementIndex = 3;
                            foreach (var potentialElement in potentialElements)
                            {
                                switch (potentialElement.Trim().ToUpper())
                                {
                                    case "C":
                                        Finder.AddCandidateElement("C");
                                        break;

                                    case "H":
                                        Finder.AddCandidateElement("H");
                                        break;

                                    case "N":
                                        Finder.AddCandidateElement("N");
                                        break;

                                    case "O":
                                        Finder.AddCandidateElement("O");
                                        break;

                                    default:
                                        if (customElementIndex < 9)
                                        {
                                            if (double.TryParse(potentialElement, out var mass))
                                            {
                                                customElementIndex++;
                                                // TODO: If this is used, then there also needs to be some report of which placeholder corresponds to which mass
                                                Finder.AddCandidateElement(potentialElement);
                                                formulaFinderModeHasCustomMass = true;
                                            }
                                            else
                                            {
                                                // See if customElementFormula is valid by checking if it has a weight of 0 or more
                                                var customElementFormula = potentialElement.Substring(0, 1).ToUpper() + potentialElement.Substring(1);

                                                compound.Formula = customElementFormula;
                                                if (compound.ErrorId == 0)
                                                {
                                                    customElementIndex++;
                                                    Finder.AddCandidateElement(compound.FormulaCapitalized);
                                                }
                                                else
                                                {
                                                    fileOutput.WriteLine(COMMENT_CHAR + " Error: Invalid formula or abbreviation: " + customElementFormula);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            fileOutput.WriteLine(COMMENT_CHAR + " Error: Too many custom search elements (" + potentialElements.Length + ")");
                                        }

                                        break;
                                }
                            }
                        }
                    }


                    if (verboseMode)
                    {
                        var searchElementsSelected = string.Join(", ", Finder.CandidateElements.Keys);
                        fileOutput.WriteLine("");
                        fileOutput.WriteLine(COMMENT_CHAR + " Formula Finder Mode Enabled.  Search elements/abbreviations: " + searchElementsSelected);
                    }

                    break;

                case "MAXHITS":
                    if (double.TryParse(settings, out var valueDblMaxHits))
                    {
                        var newMaxHits = (int)valueDblMaxHits;
                        if (newMaxHits >= 1 && newMaxHits <= 15000)
                        {
                            Finder.MaximumHits = newMaxHits;
                            if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " FF Maximum Hits set to " + newMaxHits);

                        }
                    }

                    break;

                case "TOLERANCE":
                    if (double.TryParse(settings, out var valueDblTol))
                    {
                        finderWeightTolerance = valueDblTol;
                        if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " FF Tolerance set to " + valueDblTol);

                    }

                    break;

                case "ISOTOPICDISTRIBUTION":
                    // Isotopic Distribution calculations
                    if (verboseMode) fileOutput.WriteLine("");

                    isotopicDistributionMode = ParseBooleanSetting(settingsUpperCase, fileOutput, "Isotopic Distribution calculations now On", "Isotopic Distribution calculations now Off");
                    showWeight = !isotopicDistributionMode;

                    formulaFinderMode = false;
                    aminoAcidConvert3To1 = false;
                    aminoAcidConvert1To3 = false;
                    convertToEmpiricalFormula = false;
                    expandAbbreviations = false;
                    oneLetterPeptideWeightMode = false;
                    break;

                case "ISOTOPICDISTRIBUTIONCHARGE":
                    // Charge state for Isotopic Distribution calculations
                    if (double.TryParse(settings.Trim(), out var valueDblIdc))
                    {
                        isotopicDistributionChargeState = (int)valueDblIdc;
                        if (verboseMode) fileOutput.WriteLine(COMMENT_CHAR + " Isotopic Distribution charge set to " + isotopicDistributionChargeState);
                    }

                    break;

                case "ISOTOPICDISTRIBUTIONADDPROTON":
                    // Whether or not to add a proton whem charge is >=1 during Isotopic Distribution calculations
                    if (verboseMode) fileOutput.WriteLine("");

                    addProtonChargeCarrier = ParseBooleanSetting(settingsUpperCase, fileOutput, "Isotopic Distribution calculations will add a proton when charge is >= 1", "Isotopic Distribution calculations will not add a proton when charge is >= 1");

                    break;

                default:
                    fileOutput.WriteLine(COMMENT_CHAR + " Error: Unknown Command: " + command);

                    break;
            }
        }

        private string Process(string workText)
        {
            var outLine = "";
            if (aminoAcidConvert3To1)
            {
                // Convert 3 letter to 1 letter amino acids
                mwt.Peptide.SetSequence(workText);
                if (showInputSequence)
                    outLine = workText + delimiter;
                outLine += mwt.Peptide.GetSequence(false, spaceEvery10, false, false, true);
            }
            else if (aminoAcidConvert1To3)
            {
                // Convert 1 letter to 3 letter amino acids
                mwt.Peptide.SetSequence(workText, NTerminusGroupType.Hydrogen, CTerminusGroupType.Hydroxyl, false);
                if (showInputSequence)
                    outLine = workText + delimiter;
                outLine += mwt.Peptide.GetSequence(true, spaceEvery10, separateWithDash, false, true);
            }
            else if (isotopicDistributionMode)
            {
                if (isotopicDistributionChargeState < 1)
                    isotopicDistributionChargeState = 1;

                var success = mwt.ComputeIsotopicAbundances(ref workText, isotopicDistributionChargeState, out var results, out _, out _, addProtonChargeCarrier, headerIsotopicAbundance, headerMass, headerFraction, headerIntensity);
                var errorId = mwt.ErrorId;

                if (success)
                    // Formula parsed successfully
                    outLine = results;
                else if (errorId != -1)
                    outLine = Messages.LookupMessage(350) + ": " + Messages.LookupMessage(errorId);
            }
            else
            {
                // Compute the mass of a formula or of a 1-letter peptide sequence
                if (oneLetterPeptideWeightMode)
                {
                    // Convert workText to 3-letter notation, then add
                    // peptideWeightModePrefixFormula and peptideWeightModeSuffixFormula

                    mwt.Peptide.SetSequence(workText, NTerminusGroupType.Hydrogen, CTerminusGroupType.Hydroxyl, false);
                    var peptide3Letter = mwt.Peptide.GetSequence(true, false, false, false, false);
                    peptide3Letter = peptideWeightModePrefixFormula + "-" + peptide3Letter + "-" + peptideWeightModeSuffixFormula;

                    compound.Formula = peptide3Letter;
                }
                else
                    compound.Formula = workText;

                if (compound.ErrorId == 0)
                {
                    if (!(showCapitalizedFormula || showSourceFormula || convertToEmpiricalFormula || expandAbbreviations || showWeight))
                        // Need to make sure blnShowWeight = True so that something gets displayed
                        showWeight = true;

                    if (showSourceFormula)
                    {
                        if (showCapitalizedFormula)
                            outLine = compound.FormulaCapitalized;
                        else
                            outLine = workText;
                    }
                    else
                        outLine = "";

                    if (convertToEmpiricalFormula)
                    {
                        if (outLine.Length > 0)
                            outLine += delimiter;
                        outLine += compound.ConvertToEmpirical();
                    }
                    else if (expandAbbreviations)
                    {
                        if (outLine.Length > 0)
                            outLine += delimiter;
                        outLine += compound.ExpandAbbreviations();
                    }

                    if (showWeight)
                    {
                        if (outLine.Length > 0)
                            outLine += delimiter;
                        if (stdDevModeEnabled)
                            outLine += compound.MassAndStdDevString;
                        else
                            outLine += compound.Mass;
                    }
                }
                else
                    // If an error, output the error to the file
                    outLine = workText + delimiter + Messages.LookupMessage(compound.ErrorId);
            }

            return outLine;
        }

        private void ProcessFormulaFinder(string workText, StreamWriter fileOutput, IProgress<ProgressValues> progress)
        {
            // Clear error messages from the previous run
            finderMessages.Clear();
            // Only parse line if it contains a number
            if (double.TryParse(workText.Trim(), out var targetMass))
            {
                // Make sure at least one element is checked
                if (Finder.CandidateElements.Count == 0)
                {
                    fileOutput.WriteLine(COMMENT_CHAR + " Warning: Formula Finder Mode is Enabled, but no elements were chosen.");

                    fileOutput.WriteLine(COMMENT_CHAR + "          Will use C, H, N, and O by default");

                    Finder.AddCandidateElement("C");
                    Finder.AddCandidateElement("H");
                    Finder.AddCandidateElement("N");
                    Finder.AddCandidateElement("O");
                }

                if (targetMass > 0)
                {
                    progressData.SubStatus = $"Calculating potential formulas for target mass {targetMass:0.###}";
                    progress.Report(progressData);
                    fileOutput.WriteLine(COMMENT_CHAR + " FF Searching: " + targetMass);

                    // Start the Formula Finder in batch mode
                    var finderResults = Finder.FindMatchesByMass(targetMass, finderWeightTolerance, out var symbolMasses);

                    var errorFound = false;
                    foreach (var entry in finderMessages)
                    {
                        fileOutput.WriteLine(COMMENT_CHAR + entry);
                        if (entry.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase))
                        {
                            errorFound = true;
                        }
                    }

                    if (formulaFinderModeHasCustomMass)
                    {
                        fileOutput.WriteLine("Formula symbol masses: {0}", string.Join(", ", symbolMasses.Select(x => $"{x.Key}={x.Value:0.###}")));
                    }

                    // Now print the results
                    var linesOutput = 0;
                    if (!errorFound || finderResults.Count > 0)
                    {
                        linesOutput++;
                        fileOutput.WriteLine("Compounds found: {0}", finderResults.Count);
                    }

                    foreach (var result in finderResults)
                    {
                        linesOutput++;
                        fileOutput.WriteLine("{0}\tMW={1:F3}\tdm={2:F3}", result.EmpiricalFormula, result.Mass, result.DeltaMass);
                    }

                    // Require at least 2 lines of output
                    while (linesOutput < 2)
                    {
                        fileOutput.WriteLine();
                        linesOutput++;
                    }

                    fileOutput.WriteLine("");

                }
                else
                {
                    fileOutput.WriteLine(COMMENT_CHAR + " Error: Cannot use Formula Finder with a search value of 0");
                }

            }
            else
            {
                fileOutput.WriteLine(COMMENT_CHAR + " Error: Formula Finder Mode is Enabled, but number not found: " + workText);
            }
        }

        private readonly List<string> finderMessages = new List<string>();

        private void Finder_MessageEvent(string message)
        {
            finderMessages.Add("Note: " + message);
        }

        private void Finder_ErrorEvent(string errorMessage)
        {
            finderMessages.Add("Error: " + errorMessage);
        }

        private void Finder_WarningEvent(string warningMessage)
        {
            finderMessages.Add("Warning: " + warningMessage);
        }

        private bool ParseBooleanSetting(string settingValue, StreamWriter fileOutput = null, string verboseTextTrue = "", string verboseTextFalse = "")
        {
            var doOutput = verboseMode && fileOutput != null;
            switch (settingValue)
            {
                case "TRUE":
                case "1":
                case "ON":
                    if (doOutput && !string.IsNullOrWhiteSpace(verboseTextTrue)) fileOutput.WriteLine(COMMENT_CHAR + " " + verboseTextTrue);

                    return true;

                default:
                    if (doOutput && !string.IsNullOrWhiteSpace(verboseTextFalse)) fileOutput.WriteLine(COMMENT_CHAR + " " + verboseTextFalse);

                    return false;
            }
        }
    }
}
