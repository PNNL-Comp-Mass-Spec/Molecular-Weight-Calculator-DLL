using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Data;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.Tools;
using MolecularWeightCalculatorGUI.Plotting;
using ReactiveUI;
using Unit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.IsotopicDistribution
{
    internal class IsotopicDistributionViewModel : ReactiveObject
    {
        private const int MAX_SERIES_COUNT = 2;
        private const int GAUSSIAN_CONVERSION_DATACOUNT_WARNING_THRESHOLD = 1000;

        [Obsolete("For WPF designer use only", true)]
        public IsotopicDistributionViewModel() : this(new MolecularWeightTool())
        { }

        public IsotopicDistributionViewModel(MolecularWeightTool mwt)
        {
            this.mwt = mwt ?? new MolecularWeightTool();

            Spectrum.SetSeriesCount(2);
            Spectrum.SetSeriesPlotMode(1, PlotMode.Lines, true);

            plotUpdatesDisabled = false;

            convolutedMSData = new List<MassAbundanceImmutable>();
            comparisonListData = new List<XYPointImmutable>();

            //PopulateComboBox(PlotType, true, "Sticks to Zero|Gaussian Peaks", IsotopicPlotMode.Gaussian);    // 15140
            //PopulateComboBox(ComparisonListPlotType, true, "Sticks to Zero|Gaussian Peaks|Lines Between Points", IsotopicPlotMode.SticksToZero);    // 15145
            IsotopicPlotModesComparison = Enum.GetValues(typeof(IsotopicPlotMode)).Cast<IsotopicPlotMode>().ToList();
            IsotopicPlotModes = IsotopicPlotModesComparison.Where(x => x != IsotopicPlotMode.ContinuousData).ToList();

            ChargeState = 1;

            Results = "";

            ComputeCommand = ReactiveCommand.Create(StartIsotopicDistributionCalcs);
            CopyCommand = ReactiveCommand.Create(CopyResults);
            // TODO: Close spectrum window (if separate): CloseCommand = ReactiveCommand.Create(objSpectrum.ShowSpectrum);
            CloseCommand = ReactiveCommand.Create(() => {});
            PasteComparisonIonListCommand = ReactiveCommand.Create(ComparisonIonListPaste);
            ClearComparisonIonListCommand = ReactiveCommand.Create(ComparisonIonListClear);

            this.WhenAnyValue(x => x.AddProtonChargeCarrier).Subscribe(_ => StartIsotopicDistributionCalcs());
            this.WhenAnyValue(x => x.Formula).Subscribe(_ => StartIsotopicDistributionCalcs());
            this.WhenAnyValue(x => x.ChargeState).Subscribe(_ => StartIsotopicDistributionCalcs());
            this.WhenAnyValue(x => x.PlotResults).Throttle(TimeSpan.FromMilliseconds(500)).Where(x => x).Subscribe(_ => PlotIsotopicDistribution(true));
            this.WhenAnyValue(x => x.ComparisonListPlotType, x => x.PlotType, x => x.AutoLabelPeaks, x => x.ComparisonListPlotColor, x => x.PlotColor).Throttle(TimeSpan.FromMilliseconds(500)).Subscribe(_ => PlotIsotopicDistribution(true));
            this.WhenAnyValue(x => x.EffectiveResolution, x => x.EffectiveResolutionMass, x => x.GaussianQualityFactor).Throttle(TimeSpan.FromMilliseconds(500)).Subscribe(_ => PlotIsotopicDistribution(true));

            //formulaCapitalized = this.WhenAnyValue(x => x.Formula).Select(x =>
            //{
            //    if (string.IsNullOrWhiteSpace(x))
            //    {
            //        return "";
            //    }
            //
            //    var data = objMwtWin.ElementAndMass.Parser.ParseFormula(x);
            //
            //    // TODO: Improve error handling (right now probably just returning an empty string)
            //    if (data.ErrorData.ErrorId != 0)
            //    {
            //        Results = data.ErrorData.ErrorDescription;
            //        return "";
            //    }
            //
            //    return data.Formula;
            //}).ToProperty(this, x => x.FormulaCapitalized);

            formulaRtf = this.WhenAnyValue(x => x.FormulaCapitalized).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => this.mwt.ElementAndMass.PlainTextToRtf(x, false)).ToProperty(this, x => x.FormulaRtf);
            formulaXaml = this.WhenAnyValue(x => x.FormulaCapitalized).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => this.mwt.ElementAndMass.PlainTextToXaml(x, false)).ToProperty(this, x => x.FormulaXaml);

            initializing = false;
        }

        private IReadOnlyList<MassAbundanceImmutable> convolutedMSData;

        private readonly List<XYPointImmutable> comparisonListData;  // 0-based array

        private double crcForPredictedSpectrumSaved;
        private double crcForComparisonIonListSaved;

        private bool mUserWarnedLargeDataSetForGaussianConversion;
        private bool plotUpdatesDisabled;

        private string formula = "";
        //private readonly ObservableAsPropertyHelper<string> formulaCapitalized;
        private string formulaCapitalized = "";
        private readonly ObservableAsPropertyHelper<string> formulaRtf;
        private readonly ObservableAsPropertyHelper<string> formulaXaml;
        private int chargeState = 1;
        private bool addProtonChargeCarrier = true;
        private string results = "";

        private bool plotResults = true;
        private bool autoLabelPeaks = false;
        private Color plotColor = Colors.Blue;
        private IsotopicPlotMode plotType = IsotopicPlotMode.Gaussian;
        private int effectiveResolution = 5000;
        private int effectiveResolutionMass = 1000;
        private int gaussianQualityFactor = 50;
        private Color comparisonListPlotColor = Colors.Red;
        private IsotopicPlotMode comparisonListPlotType = IsotopicPlotMode.SticksToZero;
        private bool comparisonListNormalize = true;
        private int comparisonListDataPoints = 0;
        private readonly MolecularWeightTool mwt;

        // Set to true here, then all of the constructor setup occurs, then we permanently set it to false.
        // ReSharper disable once MemberInitializerValueIgnored
        private readonly bool initializing = true;

        public IReadOnlyList<IsotopicPlotMode> IsotopicPlotModes { get; }
        public IReadOnlyList<IsotopicPlotMode> IsotopicPlotModesComparison { get; }

        public string Formula
        {
            get => formula;
            set => this.RaiseAndSetIfChanged(ref formula, value);
        }

        public string FormulaCapitalized
        {
            get => formulaCapitalized;
            set => this.RaiseAndSetIfChanged(ref formulaCapitalized, value);
        }

        public string FormulaRtf => formulaRtf.Value;
        public string FormulaXaml => formulaXaml.Value;

        public int ChargeState
        {
            get => chargeState;
            set => this.RaiseAndSetIfChanged(ref chargeState, value);
        }

        public bool AddProtonChargeCarrier
        {
            get => addProtonChargeCarrier;
            set => this.RaiseAndSetIfChanged(ref addProtonChargeCarrier, value);
        }

        public string Results
        {
            get => results;
            set => this.RaiseAndSetIfChanged(ref results, value);
        }

        public bool PlotResults
        {
            get => plotResults;
            set => this.RaiseAndSetIfChanged(ref plotResults, value);
        }

        public bool AutoLabelPeaks
        {
            get => autoLabelPeaks;
            set => this.RaiseAndSetIfChanged(ref autoLabelPeaks, value);
        }

        public Color PlotColor
        {
            get => plotColor;
            set => this.RaiseAndSetIfChanged(ref plotColor, value);
        }

        public IsotopicPlotMode PlotType
        {
            get => plotType;
            set => this.RaiseAndSetIfChanged(ref plotType, value);
        }

        public int EffectiveResolution
        {
            get => effectiveResolution;
            set => this.RaiseAndSetIfChanged(ref effectiveResolution, value);
        }

        public int EffectiveResolutionMass
        {
            get => effectiveResolutionMass;
            set => this.RaiseAndSetIfChanged(ref effectiveResolutionMass, value);
        }

        public int GaussianQualityFactor
        {
            get => gaussianQualityFactor;
            set => this.RaiseAndSetIfChanged(ref gaussianQualityFactor, value);
        }

        public Color ComparisonListPlotColor
        {
            get => comparisonListPlotColor;
            set => this.RaiseAndSetIfChanged(ref comparisonListPlotColor, value);
        }

        public IsotopicPlotMode ComparisonListPlotType
        {
            get => comparisonListPlotType;
            set => this.RaiseAndSetIfChanged(ref comparisonListPlotType, value);
        }

        public bool ComparisonListNormalize
        {
            get => comparisonListNormalize;
            set => this.RaiseAndSetIfChanged(ref comparisonListNormalize, value);
        }

        public int ComparisonListDataPoints
        {
            get => comparisonListDataPoints;
            set => this.RaiseAndSetIfChanged(ref comparisonListDataPoints, value);
        }

        public PlotViewModel Spectrum { get; } = new PlotViewModel();

        public ReactiveCommand<Unit, Unit> ComputeCommand { get; }
        public ReactiveCommand<Unit, Unit> CopyCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        public ReactiveCommand<Unit, Unit> PasteComparisonIonListCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearComparisonIonListCommand { get; }

        public void ResetOptions()
        {
            chargeState = 1;
            addProtonChargeCarrier = true;
            results = "";

            plotResults = true;
            autoLabelPeaks = false;
            plotColor = Colors.Blue;
            plotType = IsotopicPlotMode.Gaussian;
            effectiveResolution = 5000;
            effectiveResolutionMass = 1000;
            gaussianQualityFactor = 50;
            comparisonListPlotColor = Colors.Red;
            comparisonListPlotType = IsotopicPlotMode.SticksToZero;
            comparisonListNormalize = true;
            comparisonListDataPoints = 0;
        }

        private void ComparisonIonListClear()
        {
            comparisonListData.Clear();
            UpdateComparisonListStatus();

            PlotIsotopicDistribution();
        }

        //private bool QueryExistingIonsInList()
        //{
        //    if (comparisonListData.Count > 0)
        //    {
        //        const string message = "Ions are already present in the ion list.  Replace with new ions?";
        //        var response = MessageBox.Show(message, "Replace Existing Ions", MessageBoxButton.YesNoCancel, MessageBoxImage.Information, MessageBoxResult.Cancel);
        //        return response == MessageBoxResult.Yes;
        //    }
        //
        //    return true;
        //}

        private void ComparisonIonListPaste()
        {
            // Input: from clipboard, a list of mass/intensity pairs, one per line
            // mass and intensity separated by space, tab, or comma (except if comma is the current culture's decimal separator - NOTE: Add support for semicolon)
            try
            {
                ////    // Warn user about replacing data
                ////    if (!QueryExistingIonsInList()) return;

                // Grab text from clipboard

                var clipText = Clipboard.GetText(TextDataFormat.Text);

                if (clipText.Length == 0)
                {
                    MessageBox.Show("The clipboard is empty.  No ions to paste.", "No ions", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Determine if commas are used for the decimal point in this locale
                bool allowCommaDelimiter;
                if (MolecularWeightTool.DetermineDecimalPoint() == ',')
                {
                    allowCommaDelimiter = false;
                }
                else
                {
                    allowCommaDelimiter = true;
                }

                // First Determine number of data points in clipText
                // This isn't necessary (while it is necessary in frmFragmentationModelling.PasteIonMatchList),
                //  but it doesn't take that much time and allows this code to be nearly identical to the frmFragmentationModelling code

                // Initialize maximumIntensity
                var maximumIntensity = double.MinValue;

                // Construct Delimiter List: Contains a space, Tab, and possibly comma
                var delimiters = new char[] { ' ', '\t', ',', ';' };
                if (!allowCommaDelimiter)
                {
                    delimiters = new char[] { ' ', '\t', ';' };
                }

                // Split all values by line, which also removes any newline characters, and remove any resulting empty entries
                var lines = clipText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var loaded = new List<XYPoint>(lines.Length + 2);
                foreach (var line in lines)
                {
                    var pair = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    if (pair.Length >= 2 && double.TryParse(pair[0], out var mass) &&
                        double.TryParse(pair[1], out var intensity) && intensity != 0)
                    {
                        loaded.Add(new XYPoint(mass, intensity));
                        maximumIntensity = Math.Max(maximumIntensity, intensity);
                    }
                }

                comparisonListData.Clear();
                if (loaded.Count > 0)
                {
                    // Initialize IonMatchList
                    comparisonListData.Capacity = loaded.Count;
                    if (maximumIntensity != 0 && ComparisonListNormalize)
                    {
                        // Normalize the Comparison ion list to 100
                        comparisonListData.AddRange(loaded.Select(xy => xy.ToReadOnly(x => x, y => y / maximumIntensity * 100)));
                    }
                    else
                    {
                        comparisonListData.AddRange(loaded.Select(x => x.ToReadOnly()));
                    }

                    PlotIsotopicDistribution(false);
                }
                else
                {
                    MessageBox.Show("No valid ions were found on the clipboard.  A valid ion list is a list of mass and intensity pairs, separated by commas, tabs, or spaces.  One mass/intensity pair should be present per line.", "No ions", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateComparisonListStatus();
            }
            catch (Exception ex)
            {
                UpdateComparisonListStatus();
                mwt.ElementAndMass.GeneralErrorHandler("IsotopicDistribution|ComparisonIonListPaste", ex);
            }
        }

        private void CopyResults()
        {
            Clipboard.Clear();
            Clipboard.SetText(Results, TextDataFormat.Text);
        }

        public void EnableDisablePlotUpdates(bool disableUpdating)
        {
            plotUpdatesDisabled = disableUpdating;
            if (!disableUpdating)
            {
                PlotIsotopicDistribution(true);
            }
        }

        private readonly bool[] plotSeriesModeSet = new bool[MAX_SERIES_COUNT + 1];
        private bool isPlotUpdating = false;

        private void PlotIsotopicDistribution(bool ignoreCRC = false)
        {
            if (initializing || plotUpdatesDisabled) { return; }

            try
            {
                // TODO: Maybe ignore these checks?
                // Compute a CRC value for the current convolutedMSData[] array and compare to the previously computed value
                double predictedSpectrumCRC = 0;
                double dblMassTimesIntensity;
                for (var i = 0; i < convolutedMSData.Count; i++)
                {
                    dblMassTimesIntensity = Math.Abs(i * convolutedMSData[i].Mass * convolutedMSData[i].Abundance);
                    if (dblMassTimesIntensity > 0)
                    {
                        predictedSpectrumCRC += Math.Log(dblMassTimesIntensity);
                    }
                }

                var plotUpdateNeeded = false;
                // If the new CRC is different than the old one then re-plot the spectrum
                if (Math.Abs(predictedSpectrumCRC - crcForPredictedSpectrumSaved) > float.Epsilon)
                {
                    crcForPredictedSpectrumSaved = predictedSpectrumCRC;
                    plotUpdateNeeded = true;
                }

                // Also compute a CRC value for the Comparison Ion List and compare to the previously computed value
                predictedSpectrumCRC = 0;
                for (var i = 0; i < comparisonListData.Count; i++)
                {
                    dblMassTimesIntensity = Math.Abs(i * comparisonListData[i].X * comparisonListData[i].Y);
                    if (dblMassTimesIntensity > 0)
                    {
                        predictedSpectrumCRC += Math.Log(dblMassTimesIntensity);
                    }
                }

                // If the new CRC is different than the old one then re-plot the spectrum
                if (Math.Abs(predictedSpectrumCRC - crcForComparisonIonListSaved) > float.Epsilon)
                {
                    crcForComparisonIonListSaved = predictedSpectrumCRC;
                    plotUpdateNeeded = true;
                }

                if (ignoreCRC)
                {
                    plotUpdateNeeded = true;
                }

                if (!plotUpdateNeeded)
                {
                    return;
                }

                if (isPlotUpdating)
                {
                    return;
                }

                isPlotUpdating = true;

                var data = new List<XYPointImmutable>();       // 0-based array
                if (convolutedMSData.Count > 0)
                {
                    data.Capacity = convolutedMSData.Count;

                    // Now fill data[]
                    // ConvolutedMSData[] is 0-based, and data[] is 0-based
                    data.AddRange(convolutedMSData.Select(x => new XYPointImmutable(x.Mass, x.Abundance)));
                }

                Spectrum.ClearAnnotations();

                // First plot the theoretical isotopic distribution
                var seriesNumber = 1;
                var legendCaption = Formula;
                PlotIsotopicDistributionWork(seriesNumber, data, legendCaption, PlotType, PlotColor);

                // Now plot the comparison ion list (will clear series 2 if list is empty)
                seriesNumber = 2;
                legendCaption = "Comparison List";
                var plotMode = ComparisonListPlotType;

                // Check for large number of data points when Gaussian mode is enabled
                var skipPlottingComparison = false;
                if (plotMode == IsotopicPlotMode.Gaussian && comparisonListData.Count > GAUSSIAN_CONVERSION_DATACOUNT_WARNING_THRESHOLD)
                {
                    if (!mUserWarnedLargeDataSetForGaussianConversion)
                    {
                        var message = "Warning, the ion comparison list contains a large number of data points.  Do you really want to convert each point to a Gaussian curve?";
                        var response = MessageBox.Show(message, "Large Data Point Count", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);

                        if (response == MessageBoxResult.Yes)
                        {
                            mUserWarnedLargeDataSetForGaussianConversion = true;
                        }
                        else if (response == MessageBoxResult.Cancel)
                        {
                            skipPlottingComparison = true;
                        }
                        else
                        {
                            // Switch to Continuous Data mode
                            plotMode = IsotopicPlotMode.ContinuousData;
                            ComparisonListPlotType = plotMode;
                        }
                    }
                }

                if (!skipPlottingComparison)
                {
                    PlotIsotopicDistributionWork(seriesNumber, comparisonListData, legendCaption, plotMode, ComparisonListPlotColor);
                }

                Spectrum.UpdatePlot();
            }
            catch (Exception ex)
            {
                mwt.ElementAndMass.GeneralErrorHandler("IsotopicDistribution|PlotIsotopicDistribution", ex);
            }

            isPlotUpdating = false;
        }

        private void PlotIsotopicDistributionWork(int seriesNumber, List<XYPointImmutable> data, string legendCaption, IsotopicPlotMode isoPlotMode, Color seriesColor)
        {
            try
            {
                // Save the original stick list for annotations
                var origData = data;
                if (isoPlotMode == IsotopicPlotMode.Gaussian)
                {
                    // Note that xVals[] and yVals[] will be replaced with Gaussian representations of the peaks by the following function
                    data = Gaussian.ConvertStickDataToGaussian2DArray(data, EffectiveResolution, EffectiveResolutionMass, GaussianQualityFactor, false, mwt.ElementAndMass);
                }

                var plotMode = isoPlotMode == IsotopicPlotMode.SticksToZero ? PlotMode.StickToZero : PlotMode.Lines;

                if (Spectrum.GetSeriesCount() < seriesNumber)
                {
                    Spectrum.SetSeriesCount(seriesNumber);
                }

                if (!plotSeriesModeSet[seriesNumber])
                {
                    Spectrum.SetSeriesPlotMode(seriesNumber, PlotMode.StickToZero, false);
                    plotSeriesModeSet[seriesNumber] = true;
                }

                Spectrum.ClearData(seriesNumber);
                if (data.Count > 0)
                {
                    // Normal data
                    Spectrum.SetSeriesColor(seriesNumber, seriesColor);
                    Spectrum.SetDataXvsY(seriesNumber, data, legendCaption);
                    Spectrum.SetSeriesPlotMode(seriesNumber, plotMode, false);

                    if (AutoLabelPeaks)
                    {
                        // TODO: Filter this list based on values in the Gaussian data
                        // TODO: Best results would include some peak detection (Magnitude-Concavity)
                        // TODO: Simple results: hide if the y-value in the Gaussian data is more than 'x' more than the original value (from peak combining), otherwise update with the y-value from the Gaussian data
                        Spectrum.SetAnnotations(origData);

                        // Add additional space to the Y-axis to display the annotations in the default zoom mode
                        Spectrum.SetMaxY(origData.Max(x => x.Y) + 30);
                    }

                    Spectrum.GetRangeX(out var xMinimum, out var xMaximum);

                    if (data[0].X < xMinimum || data[0].X > xMaximum ||
                        data[data.Count - 1].X < xMinimum || data[data.Count - 1].X > xMaximum)
                    {
                        Spectrum.ResetZoomOnNextUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                mwt.ElementAndMass.GeneralErrorHandler("IsotopicDistribution|PlotIsotopicDistributionWork", ex);
            }
        }

        public void StartIsotopicDistributionCalcs()
        {
            try
            {
                var formulaCopy = Formula;

                if (string.IsNullOrWhiteSpace(formulaCopy))
                {
                    return;
                }

                var headerIsotopicAbundance = "Isotopic Abundances for";
                string headerMass;
                if (ChargeState == 0)
                {
                    headerMass = "Neutral Mass";
                }
                else
                {
                    headerMass = "Mass/Charge";
                }

                var headerFraction = "Fraction";
                var headerIntensity = "Intensity";

                // Make sure we're using isotopic masses
                mwt.SetElementMode(ElementMassMode.Isotopic);

                // Note: strFormula is passed ByRef
                convolutedMSData = mwt.ElementAndMass.ComputeIsotopicAbundances(ref formulaCopy, (short)ChargeState, out var isotopeTable, addProtonChargeCarrier, headerIsotopicAbundance, headerMass, headerFraction, headerIntensity);
                if (convolutedMSData.Count > 0)
                {
                    // Update rtfFormula with the capitalized formula
                    FormulaCapitalized = formulaCopy;

                    if (PlotResults)
                    {
                        PlotIsotopicDistribution();
                    }
                }

                Results = isotopeTable;

                Formula = formulaCopy;
            }
            catch (Exception ex)
            {
                mwt.ElementAndMass.GeneralErrorHandler("IsotopicDistribution|StartIsotopicDistributionCalcs", ex);
            }
        }

        private void UpdateComparisonListStatus()
        {
            ComparisonListDataPoints = comparisonListData.Count;
        }
    }
}
