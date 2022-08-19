using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DynamicData;
using DynamicData.Binding;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.FormulaFinder;
using MolecularWeightCalculatorGUI.IsotopicDistribution;
using MolecularWeightCalculatorGUI.Utilities;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.FormulaFinder
{
    internal class FormulaFinderViewModel : ReactiveObject
    {
        [Obsolete("For WPF design time use only", true)]
        public FormulaFinderViewModel() : this(new MolecularWeightTool())
        { }

        public FormulaFinderViewModel(MolecularWeightTool objMwt, IsotopicDistributionViewModel isoDistVm = null)
        {
            mwt = objMwt;

            isoDistributionVm = isoDistVm ?? new IsotopicDistributionViewModel(objMwt);

            Options = new FormulaFinderOptionsViewModel(this);

            // ReSharper disable once RedundantExplicitArraySize
            var elements = new ElementConfiguration[]
            {
                new ElementConfiguration(0, "Carbon"  , true, 6),
                new ElementConfiguration(1, "Hydrogen", true, 1),
                new ElementConfiguration(2, "Nitrogen", true, 7),
                new ElementConfiguration(3, "Oxygen"  , true, 8),
                new ElementConfiguration(4, "Custom1_"),
                new ElementConfiguration(5, "Custom2_"),
                new ElementConfiguration(6, "Custom3_"),
                new ElementConfiguration(7, "Custom4_"),
                new ElementConfiguration(8, "Custom5_"),
                new ElementConfiguration(9, "Custom6_"),
            };
            finderElements = elements;
            Elements = new ReadOnlyObservableCollection<ElementConfiguration>(new ObservableCollection<ElementConfiguration>(finderElements));

            var sortObservable = Options.WhenAnyValue(x => x.SortResults, x => x.ResultsSortMode).Select(x =>
            {
                if (!x.Item1)
                {
                    return new FinderResult.NoSort();
                }

                return (IComparer<FinderResult>) (x.Item2 switch
                {
                    SearchResultsSortMode.SortByFormula => new FinderResult.FormulaSort(),
                    SearchResultsSortMode.SortByMWT => new FinderResult.MolecularWeightSort(),
                    SearchResultsSortMode.SortByDeltaMass => new FinderResult.DeltaMassSort(),
                    SearchResultsSortMode.SortByCharge => new FinderResult.ChargeSort(),
                    SearchResultsSortMode.SortByMZ => new FinderResult.MzSort(),
                    _ => new FinderResult.MzSort()
                });
            });

            finderResults.Connect().Sort(sortObservable).ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var finderResultsBound).Subscribe();
            FinderResults = finderResultsBound;
            //FinderResults = new ReadOnlyObservableCollection<IFinderResult>(new ObservableCollection<IFinderResult>());

            showMinMaxColumns = Options.WhenAnyValue(x => x.SearchMode)
                .Select(x => x == FormulaSearchModes.Bounded).ToProperty(this, x => x.ShowMinMaxColumns);
            isNotCalculating = this.WhenAnyValue(x => x.IsCalculating).Select(x => !x)
                .ToProperty(this, x => x.IsNotCalculating);

            OpenFormulaFinderOptionsCommand = ReactiveCommand.Create<Window>(x =>
            {
                // TODO: Prevent showing multiple? How, by storing a window reference?
                var window = new FormulaFinderOptionsWindow() { DataContext = Options, Owner = x };
                window.Show();
            });

            // Seems convoluted, but is the recommendation from https://www.reactiveui.net/docs/handbook/collections/
            elementChecked = Elements.ToObservableChangeSet().AutoRefresh(x => x.Use).ToCollection()
                .Select(x => x.Any(y => y.Use)).ToProperty(this, x => x.ElementChecked);

            Elements.ToObservableChangeSet().AutoRefresh(x => x.Use).Subscribe();
            Elements.ToObservableChangeSet().WhenPropertyChanged(x => x.Use).Subscribe(x => UpdateMinMax(x.Sender));

            // If hydrogen is present, then show verify hydrogens box
            this.WhenAnyValue(x => x.finderElements[1].Use).Subscribe(x => Options.CanVerifyHydrogens = x);
            this.WhenAnyValue(x => x.PpmMode).SubscribeOnChange(x => PpmModeChangeUpdate());
            this.WhenAnyValue(x => x.TargetMolMass).Subscribe(x => UpdateMinMax());

            CopyAsRtfCommand = ReactiveCommand.Create(CopyResultsAsRtf);
            DisplayIsotopicAbundanceCommand = ReactiveCommand.Create<Window>(x => { DisplayIsoAbundanceForCurrent(false); }, this.WhenAnyValue(x => x.IsCalculating).Select(x => !x));
            CalculateCommand = ReactiveCommand.CreateFromTask(Calculate, this.WhenAnyValue(x => x.ElementChecked, x => x.IsCalculating).Select(x => x.Item1 && !x.Item2));
            AbortCommand = ReactiveCommand.Create(AbortCalculations);
            PrintCommand = ReactiveCommand.Create(Print, this.WhenAnyValue(x => x.IsCalculating).Select(x => !x));
            CopyResultsCommand = ReactiveCommand.Create(CopyResults);
            CopySelectedResultsCommand = ReactiveCommand.Create(CopySelectedResults);
            CloseCommand = ReactiveCommand.Create<Window>(x => x.Close(), this.WhenAnyValue(x => x.IsCalculating).Select(x => !x));
        }

        private readonly MolecularWeightTool mwt;
        private FormulaSearcher Finder => mwt.FormulaFinder;
        private ElementAndMassTools ElementAndMass => mwt.ElementAndMass;
        private Messages Messages => ElementAndMass.Messages;

        private ElementMassMode elementMode;
        private string progressStatus;
        private string completionNote;
        private bool completionWarning;
        private string inputErrorText = "";

        private bool calculationsAborted = false;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private readonly IsotopicDistributionViewModel isoDistributionVm;

        private int maxHits = 1000;
        private readonly ObservableAsPropertyHelper<bool> showMinMaxColumns;
        private readonly IReadOnlyList<ElementConfiguration> finderElements;
        private double percentMaxMass = 400;
        private double targetMolMass = 200;
        private double massTolerance = 0.05;
        private double percentTolerance = 1;
        private bool ppmMode;
        private bool showDeltaMass = true;
        private bool isCalculating;
        private readonly ObservableAsPropertyHelper<bool> isNotCalculating;
        private bool showWaitCursor;
        private bool showAppStartingCursor;
        private readonly ObservableAsPropertyHelper<bool> elementChecked;
        private readonly SourceList<FinderResult> finderResults = new SourceList<FinderResult>();
        private FinderResult selectedResult;

        public bool ElementChecked => elementChecked.Value;

        public FormulaFinderOptionsViewModel Options { get; }

        public ReadOnlyObservableCollection<ElementConfiguration> Elements { get; }

        public ElementConfiguration Element00 => Elements[0];
        public ElementConfiguration Element01 => Elements[1];
        public ElementConfiguration Element02 => Elements[2];
        public ElementConfiguration Element03 => Elements[3];
        public ElementConfiguration Element04 => Elements[4];
        public ElementConfiguration Element05 => Elements[5];
        public ElementConfiguration Element06 => Elements[6];
        public ElementConfiguration Element07 => Elements[7];
        public ElementConfiguration Element08 => Elements[8];
        public ElementConfiguration Element09 => Elements[9];

        public ElementMassMode ElementMode
        {
            get => elementMode;
            set => this.RaiseAndSetIfChanged(ref elementMode, value);
        }

        public string ProgressStatus
        {
            get => progressStatus;
            set => this.RaiseAndSetIfChanged(ref progressStatus, value);
        }

        public string CompletionNote
        {
            get => completionNote;
            set => this.RaiseAndSetIfChanged(ref completionNote, value);
        }

        public bool CompletionWarning
        {
            get => completionWarning;
            set => this.RaiseAndSetIfChanged(ref completionWarning, value);
        }

        public string InputErrorText
        {
            get => inputErrorText;
            set => this.RaiseAndSetIfChanged(ref inputErrorText, value);
        }

        public int MaxHits
        {
            get => maxHits;
            set => this.RaiseAndSetIfChanged(ref maxHits, value);
        }

        public bool ShowMinMaxColumns => showMinMaxColumns.Value;

        public double TargetMolMass
        {
            get => targetMolMass;
            set => this.RaiseAndSetIfChanged(ref targetMolMass, value);
        }

        public double PercentMaxMass
        {
            get => percentMaxMass;
            set => this.RaiseAndSetIfChanged(ref percentMaxMass, value);
        }

        public double MassTolerance
        {
            get => massTolerance;
            set => this.RaiseAndSetIfChanged(ref massTolerance, value);
        }

        public double PercentTolerance
        {
            get => percentTolerance;
            set => this.RaiseAndSetIfChanged(ref percentTolerance, value);
        }

        public bool PpmMode
        {
            get => ppmMode;
            set => this.RaiseAndSetIfChanged(ref ppmMode, value);
        }

        public bool ShowDeltaMass
        {
            get => showDeltaMass;
            set => this.RaiseAndSetIfChanged(ref showDeltaMass, value);
        }

        public ReadOnlyObservableCollection<FinderResult> FinderResults { get; }

        public FinderResult SelectedResult
        {
            get => selectedResult;
            set => this.RaiseAndSetIfChanged(ref selectedResult, value);
        }

        public ObservableCollectionExtended<FinderResult> SelectedResultSet { get; } = new ObservableCollectionExtended<FinderResult>();

        public bool ShowWaitCursor
        {
            get => showWaitCursor;
            set => this.RaiseAndSetIfChanged(ref showWaitCursor, value);
        }

        public bool ShowAppStartingCursor
        {
            get => showAppStartingCursor;
            set => this.RaiseAndSetIfChanged(ref showAppStartingCursor, value);
        }

        public bool IsCalculating
        {
            get => isCalculating;
            private set => this.RaiseAndSetIfChanged(ref isCalculating, value);
        }

        public bool IsNotCalculating => isNotCalculating.Value;

        public ReactiveCommand<Window, RxUnit> OpenFormulaFinderOptionsCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopyAsRtfCommand { get; }
        public ReactiveCommand<Window, RxUnit> DisplayIsotopicAbundanceCommand { get; }
        public ReactiveCommand<RxUnit, string> CalculateCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> AbortCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> PrintCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopyResultsCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> CopySelectedResultsCommand { get; }
        public ReactiveCommand<Window, RxUnit> CloseCommand { get; }

        private void CopySelectedResults()
        {
            // TODO: Used for right-click menu Cut and Copy
            // Copy text from rtfResults to the clipboard
            if (FinderResults.Count == 0)
            {
                return;
            }

            IEnumerable<FinderResult> results = SelectedResultSet;
            if (SelectedResultSet.Count == 0)
            {
                results = FinderResults;
            }

            var copyText = "";
            foreach (var result in results)
            {
                if (string.IsNullOrWhiteSpace(copyText))
                {
                    copyText += result.DisplayRtfPrefix + result.DisplayRtfNoDocument;
                }
                else
                {
                    copyText += @"\par " + result.DisplayRtfNoDocument;
                }
            }

            copyText += "}";

            // Copy corrected text to Clipboard.
            Clipboard.Clear();
            Clipboard.SetText(copyText, TextDataFormat.Rtf);
        }

        private void DisplayIsoAbundanceForCurrent(bool blnFromRtfResults = false)
        {
            if (FinderResults.Count == 0)
            {
                MessageBox.Show("The results box is empty.", "Nothing to Display", MessageBoxButton.OK);
            }
            else if (SelectedResult == null)
            {
                MessageBox.Show("No result selected", "Nothing to Display", MessageBoxButton.OK);
            }
            else
            {
                ShowWaitCursor = true;

                isoDistributionVm.Spectrum.ResetZoomOnNextUpdate();
                // TODO: Invalid; next line may not work either...
                // TODO: isoDistributionVm.FormulaRtf = strWorking;
                isoDistributionVm.Formula = SelectedResult.EmpiricalFormula;
                isoDistributionVm.PlotResults = true;
                isoDistributionVm.StartIsotopicDistributionCalcs();
                // TODO: Show window if not shown; however, need to add some checks and capability to bring another window to the front...

                ShowWaitCursor = false;

                if (blnFromRtfResults)
                {
                    // TODO: UI Effect... rtfResults.SetFocus();
                }
            }
        }

        private bool ParseElement(ElementConfiguration element)
        {
            // TODO: Do this on weight entry?
            // Returns the weight of the element on elementIndex, along with setting the symbol and charge

            if (element.IsParsed)
            {
                return true;
            }

            if (element.FixedElement)
            {
                var result = mwt.GetElement(element.FixedAtomicNumber, out var symbol, out var mass, out _, out var charge, out _);
                // TODO: Throw exception on false; but, this would be a developer error, not user error
                Debug.Assert(result);

                element.SetParsedData(mass, charge, symbol, symbol);
                return true;
            }

            const string customElementPhrase = "Custom";
            const string numOrElementPhrase = "# or Element or Abbrev.";

            // Working with custom elements
            if (string.IsNullOrWhiteSpace(element.WeightText) || element.WeightText == numOrElementPhrase)
            {
                // The custom element phrase is present; don't try to match it to an element
                element.WeightText = numOrElementPhrase;
                return false;
            }

            if (!double.TryParse(element.WeightText.Trim(), out var weight))
            {
                // A single element or abbreviation was entered

                // Convert input to default format of first letter capitalized and rest lowercase
                element.WeightText = element.WeightText.Substring(0, 1).ToUpper() + element.WeightText.Substring(1);
                var work = element.WeightText;

                // See if this is an element
                var symbolReference = mwt.GetAtomicNumber(work);
                if (symbolReference >= 1)
                {
                    // Found an element
                    var success = mwt.GetElement(symbolReference, out var symbol, out var mass, out _, out var charge, out _);
                    // TODO: This should never "assert false", since we were able to retrieve the atomic number
                    Debug.Assert(success);

                    // Re-assign matched element or abbreviation to properly capitalize
                    element.SetParsedData(mass, charge, symbol, symbol);
                    return true;
                }

                // See if this is an abbreviation
                symbolReference = mwt.GetAbbreviationId(work);
                if (symbolReference >= 1)
                {
                    // Found a normal abbreviation
                    var success = mwt.GetAbbreviation(symbolReference, out var symbol, out var formula, out var charge, out _);

                    // Re-assign matched element or abbreviation to properly capitalize
                    var mass = mwt.ComputeMass(formula);
                    element.SetParsedData(mass, charge, symbol, symbol);
                    return true;
                }

                // The text doesn't match a symbol or abbreviation, and is not a number; that's an error...
                element.WeightText = numOrElementPhrase;
                return false;
            }

            var matchSymbol = customElementPhrase.Substring(0, 1) + (element.Index - 3) + "_";
            element.SetParsedData(weight, 0, matchSymbol);
            return true;
        }

        private string ParseElements(List<CandidateElement> candidateElementsStats)
        {
            // Determine which elements are checked and add to candidateElementsStats list
            foreach (var element in finderElements.Where(x => x.Use))
            {
                if (!element.FixedElement)
                {
                    // Parse the element, and populate the values in the ElementConfiguration object
                    // Working with custom elements
                    if (!double.TryParse(element.WeightText, out var elementWeight))
                    {
                        // A single element or abbreviation was entered
                        // Convert input to default format of first letter capitalized and rest lowercase
                        element.WeightText = element.WeightText.Substring(0, 1).ToUpper() + element.WeightText.Substring(1);
                        foreach (var c in element.WeightText)
                        {
                            var testChar = char.ToUpper(c);
                            if ((testChar < 'A' || testChar > 'Z') && testChar != '+' && testChar != '_')
                            {
                                return Messages.LookupMessage(775);
                            }
                        }

                        if (string.IsNullOrWhiteSpace(element.WeightText))
                        {
                            // Too short
                            return Messages.LookupMessage(780);
                        }

                        // Returns false if failed to parse element weight; sets weight, charge, and match symbol
                        if (!ParseElement(element)) // calls element.SetParsedData(...)
                        {
                            // Element or abbreviation not found
                            return Messages.LookupMessage(785) + ": " + element.WeightText + "  " + Messages.LookupMessage(790);

                        }

                        // No problems, store symbol (below)
                    }
                    else
                    {
                        // Custom element, only weight given so charge is 0
                        element.SetParsedData(elementWeight, 0, "C" + (element.Index - 3) + "_");
                    }
                }
                else
                {
                    // Parse the element, and populate the values in the ElementConfiguration object
                    ParseElement(element); // calls element.SetParsedData(...)
                }

                candidateElementsStats.Add(element.GetCandidateElement(Options.SearchMode, PercentTolerance));
            }

            return "";
        }

        private string ValidateSettings()
        {
            // Check for Checked elements
            var message = "";
            if (!finderElements.Any(x => x.Use))
            {
                return Messages.LookupMessage(700) + "\r\n";
            }

            // Check for valid information in text boxes
            if (MaxHits <= 0)
            {
                message += Messages.LookupMessage(705) + "\r\n";
            }
            else if (MaxHits > FormulaSearcher.MAXIMUM_ALLOWED_RESULTS_TO_FIND)
            {
                message += Messages.LookupMessage(710, " " + FormulaSearcher.MAXIMUM_ALLOWED_RESULTS_TO_FIND) + "\r\n";
            }

            foreach (var element in finderElements.Where(x => x.Use))
            {
                // Check for reasonable numbers in Min/Max boxes
                if (element.Min < 0)
                {
                    message += Messages.LookupMessage(715) + "\r\n";
                }
                else if (element.Min > element.Max)
                {
                    message += Messages.LookupMessage(720) + "\r\n";
                }
                else if (element.Max > 65025)
                {
                    message += Messages.LookupMessage(725) + "\r\n";
                }

                // Initialize custom elements
                if (!double.TryParse(element.WeightText, out var elementWeight))
                {
                    if (element.WeightText == "# or Element or Abbrev." || string.IsNullOrWhiteSpace(element.WeightText))
                        message += Messages.LookupMessage(730) + "\r\n";
                }
                else if (elementWeight <= 0)
                {
                    message += Messages.LookupMessage(735) + "\r\n";
                }
            }

            if (Options.MatchMolecularWeight)
            {
                // Matching Molecular Weights
                if (TargetMolMass <= 0)
                    message += Messages.LookupMessage(745) + "\r\n";
                if (MassTolerance < 0)
                    message += Messages.LookupMessage(750) + "\r\n";
            }
            else
            {
                // Matching Percent Compositions

                // Confirm that PercentMaxWeight is Valid
                if (PercentMaxMass <= 0)
                    message += Messages.LookupMessage(760) + "\r\n";

                var percentCompositionSum = default(double);
                foreach (var element in finderElements.Where(x => x.Use))
                {
                    if (element.Percent <= 0)
                        message += Messages.LookupMessage(770) + "\r\n";

                    percentCompositionSum += element.Percent;
                }

                if (string.IsNullOrEmpty(message))
                {
                    if (percentCompositionSum > 100 + PercentTolerance || percentCompositionSum < 100 - PercentTolerance)
                    {
                        message = $"The sum of the percent compositions is not 100% ({percentCompositionSum:F2}%)";
                        var eResponse = MessageBox.Show(message + "  Continue with calculation?", "Caution", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        if (eResponse == MessageBoxResult.Yes)
                        {
                            message = "";
                        }
                        else
                        {
                            InputErrorText = message;
                            return message;
                        }
                    }
                }
            }

            return message;
        }

        public async Task<string> Calculate()
        {
            if (IsCalculating)
                return "";

            CompletionNote = "";
            CompletionWarning = false;
            InputErrorText = "";

            calculationsAborted = false;
            cancellationToken = new CancellationTokenSource();

            try
            {
                var messages = ValidateSettings();
                if (!string.IsNullOrEmpty(messages))
                {
                    messages = messages.Trim();
                    InputErrorText = messages;
                    return messages;
                }

                // Fill working variables with correct values

                var candidateElementsStats = new List<CandidateElement>();

                var errorText = ParseElements(candidateElementsStats);
                if (!string.IsNullOrWhiteSpace(errorText))
                {
                    errorText = errorText.Trim();
                    InputErrorText = errorText;
                    return errorText;
                }

                // Change mouse pointer to hourglass with arrow, so user can still multitask
                ShowAppStartingCursor = true;

                // Delete old Results
                finderResults.Clear();

                IsCalculating = true;

                // Make the percent completed box visible
                ProgressStatus = "Starting search...";

                // Reorder candidateElementsStats list in order from heaviest to lightest element
                // Greatly speeds up the recursive routine
                candidateElementsStats.Sort((x, y) => y.Mass.CompareTo(x.Mass));

                Finder.MaximumHits = MaxHits;

                FinderResult.SetRichTextFont(mwt.RtfFontName, mwt.RtfFontSize);

                Finder.ProgressReporter = new Progress<double>(percent =>
                    RxApp.MainThreadScheduler.Schedule(() => ProgressStatus = $"Searching, {percent:F0}% Completed"));

                // Run the heavy processing in a separate thread
                await Task.Factory.StartNew(() =>
                {
                    List<SearchResult> searchResults = null;
                    if (Options.MatchMolecularWeight)
                    {
                        // Matching Molecular Weights
                        var toleranceDa = MassTolerance;
                        if (PpmMode)
                        {
                            // Convert the PPM tolerance to mass tolerance (Da)
                            toleranceDa = Math.Abs(TargetMolMass * MassTolerance / 1000000d);
                        }

                        searchResults = Finder.FindMatchesByMass(TargetMolMass, toleranceDa,
                            Options.GetSearchOptions(), candidateElementsStats, PpmMode);
                    }
                    else
                    {
                        // Matching Percent Compositions
                        searchResults = Finder.FindMatchesByPercentComposition(PercentMaxMass,
                            Options.GetSearchOptions(), candidateElementsStats);
                    }

                    RxApp.MainThreadScheduler.Schedule(() => ProgressStatus = "Search Completed, formatting results...");

                    // Configure an IEnumerable to wrap/parse results...
                    var results = searchResults.Select(x => new FinderResult(x, Options.FindMz, ShowDeltaMass));

                    // Sorting handled by observable collection presenting the results to the view
                    // Now, put them into the FinderResults list
                    finderResults.Edit(list =>
                    {
                        list.Clear();
                        list.AddRange(results);
                    });
                }, cancellationToken.Token, TaskCreationOptions.HideScheduler, TaskScheduler.Default);

                // Show abort messages if necessary
                if (calculationsAborted)
                {
                    CompletionNote = "Calculations Interrupted";
                    CompletionWarning = true;
                }
                else if (finderResults.Count >= MaxHits)
                {
                    CompletionNote = "Maximum number of hits reached; calculations stopped.";
                    CompletionWarning = true;
                }
                else
                {
                    ProgressStatus = "Done";
                    CompletionNote = "100% Completed";
                }

                IsCalculating = false;
            }
            catch (Exception ex)
            {
                ElementAndMass.GeneralErrorHandler("FormulaFinderViewModel|Calculate", ex);
            }

            // Change mouse pointer to default
            ShowAppStartingCursor = false;

            return "";
        }

        public void LoadDynamicTextCaptions()
        {
            for (var intIndex = 4; intIndex < finderElements.Count; intIndex++)
            {
                var value = finderElements[intIndex].WeightText;
                // Look for custom weight starting with a #, or containing a space,
                // or starting with Ý (Russian abbreviation)
                // If found, clear to let the overlay provide the prompt text
                if (value.StartsWith("#") || value.Contains(" ") || value == "Ý")
                {
                    finderElements[intIndex].WeightText = "";
                }
            }
        }

        private void UpdateMinMax(ElementConfiguration element = null)
        {
            if (!Options.AutoSetBounds || !(TargetMolMass > 0))
            {
                return;
            }

            // Only update Min and Max if user wishes.
            IReadOnlyList<ElementConfiguration> items = Elements;
            if (element != null)
            {
                items = new[] { element };
            }

            foreach (var item in items)
            {
                // Find the mass for each element
                // Set the maximum search value for each element to a reasonable value,
                // based on it's mass, the search mass, and the search tolerance
                // For example, if the search mass is 200 and the tolerance is 0.5,
                // then for Nitrogen (mass=14), the max atom count is set to 15 since
                // 15 * 14= 210 (a little more than 200)
                var matched = ParseElement(item);
                var elementMass = item.Mass;
                if (item.FixedAtomicNumber == 1 && Options.VerifyHydrogens)
                {
                    elementMass *= 2;
                }

                if (!matched || elementMass <= 0)
                {
                    item.Max = 20;
                }
                else if (Options.MatchMolecularWeight)
                {
                    if (!Options.FindTargetMz)
                    {
                        item.Max = (int)Math.Round((TargetMolMass + MassTolerance) / elementMass, MidpointRounding.AwayFromZero) + 1;
                    }
                    else
                    {
                        var multiplier = Math.Max(Math.Abs(Options.ChargeMin), Math.Abs(Options.ChargeMax));
                        item.Max = (int)Math.Round((TargetMolMass + MassTolerance) / elementMass, MidpointRounding.AwayFromZero) * multiplier + 1;
                    }
                }
                else if (Options.SearchMode == FormulaSearchModes.Thorough)
                {
                    item.Max = (int)Math.Round((PercentMaxMass + MassTolerance) / elementMass, MidpointRounding.AwayFromZero) + 1;
                }
                else
                {
                    // leave item.Max unchanged
                }
            }
        }

        private void PpmModeChangeUpdate()
        {
            if (!PpmMode)
            {
                // now turning false, convert weight tolerance to regular
                double temp;
                var ppm = MassTolerance;
                if (TargetMolMass == 0)
                {
                    temp = 0.5d;
                }
                else
                {
                    temp = Math.Abs(TargetMolMass * ppm / 1000000d);
                }
                MassTolerance = temp;
            }
            else
            {
                // now turning true, convert weight tolerance to ppm
                double ppm;
                var temp = MassTolerance;
                // Original formula was ppm = Abs((1 - (TargetMolWeight + temp) / TargetMolWeight) * 1000000#)
                // Simplified using Derive to get:
                if (TargetMolMass == 0)
                {
                    ppm = 100d;
                }
                else
                {
                    ppm = Math.Abs(1000000d * temp / TargetMolMass);
                }
                MassTolerance = ppm;
            }
        }

        private void AbortCalculations()
        {
            Finder.AbortProcessingNow();
            if (!calculationsAborted)
            {
                cancellationToken.Cancel();
            }
            calculationsAborted = true;
        }

        private void CopyResults()
        {
            if (FinderResults.Count == 0)
            {
                MessageBox.Show("The results box is empty.", "Nothing to Copy", MessageBoxButton.OK);
            }
            else
            {
                Clipboard.Clear();
                var copyText = "";
                foreach (var result in FinderResults)
                {
                    // MW not found, copy line to clipboard without any tabs
                    copyText += "\r\n" + result.DisplayText;
                }
                Clipboard.SetText(copyText, TextDataFormat.Text);
            }
        }

        private void CopyResultsAsRtf()
        {
            if (FinderResults.Count == 0)
            {
                MessageBox.Show("The results box is empty.", "Nothing to Copy", MessageBoxButton.OK);
            }
            else
            {
                var resultsRtf = "";
                foreach (var result in FinderResults)
                {
                    if (string.IsNullOrWhiteSpace(resultsRtf))
                    {
                        resultsRtf = result.DisplayRtfPrefix + result.DisplayRtfNoDocument;
                    }
                    else
                    {
                        resultsRtf += @"\par " + result.DisplayRtfNoDocument;
                    }
                }

                resultsRtf += "}";

                Clipboard.Clear();
                Clipboard.SetText(resultsRtf, TextDataFormat.Rtf);
            }
        }

        private void Print()
        {
            if (IsCalculating)
            {
                // Do nothing; originally this triggered an abort...
                return;
            }

            if (FinderResults.Count == 0)
            {
                MessageBox.Show("The results box is empty.", "Nothing to Print", MessageBoxButton.OK);
            }
            else
            {
                // Print the results
                var response = MessageBox.Show("Are you sure you want to print the current result(s)?", "Printing", MessageBoxButton.YesNo);

                if (response == MessageBoxResult.Yes)
                {
                    var doc = new FlowDocument();
                    var par = new Paragraph();
                    doc.Blocks.Add(par);
                    foreach (var result in FinderResults)
                    {
                        par.Inlines.Add(result.DisplayText);
                    }

                    var printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Formula Finder Results");
                    }
                }
            }
        }

        internal void WindowActivated()
        {
            // TODO: UI Effect: FinderResults.FontName = objMwtWin.RtfFontName;
            // TODO: UI Effect: FinderResults.FontSize = objMwtWin.RtfFontSize;

            // TODO: UI Effect: PossiblyHideMainWindow();

            // Display correct weight mode phrase
            ElementMode = mwt.GetElementMode();

            LoadDynamicTextCaptions();
        }
    }
}
