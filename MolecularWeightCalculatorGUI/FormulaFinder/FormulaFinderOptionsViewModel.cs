using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using MolecularWeightCalculator.FormulaFinder;
using ReactiveUI;

namespace MolecularWeightCalculatorGUI.FormulaFinder
{
    internal class FormulaFinderOptionsViewModel : ReactiveObject
    {
        public FormulaFinderOptionsViewModel(FormulaFinderViewModel ffvm)
        {
            FormulaSearchModeOptions = Enum.GetValues(typeof(FormulaSearchModes)).Cast<FormulaSearchModes>().ToList();
            resultsSortModeBackingList.AddRange(Enum.GetValues(typeof(SearchResultsSortMode)).Cast<SearchResultsSortMode>());

            resultsSortModeBackingList.Connect().Sort(Comparer<SearchResultsSortMode>.Default).Bind(out var tempList).Subscribe();
            ResultSortModeOptions = tempList;

            allowChanges = ffvm.WhenAnyValue(x => x.IsCalculating).Select(x => !x)
                .ToProperty(this, x => x.AllowChanges, true, true);

            allowLimitChargeRange = this.WhenAnyValue(x => x.FindCharge, x => x.AllowChanges)
                .Select(x => x.Item1 && x.Item2).ToProperty(this, x => x.AllowLimitChargeRange);

            enableVerifyHydrogens = this.WhenAnyValue(x => x.CanVerifyHydrogens, x => x.AllowChanges)
                .Select(x => x.Item1 && x.Item2).ToProperty(this, x => x.EnableVerifyHydrogens);

            // If SortResults is checked, then show sort mode
            // If FindCharge is checked and LimitChargeRange is checked then show the Min and Max labels and boxes
            this.WhenAnyValue(x => x.FindCharge).Where(x => !x).Subscribe(x =>
            {
                LimitChargeRange = false;
                FindMz = false;
            });
            this.WhenAnyValue(x => x.MatchMolecularWeight).Where(x => !x).Subscribe(x => FindMz = false);
            this.WhenAnyValue(x => x.FindMz, x => x.LimitChargeRange).Where(x => !x.Item1 || !x.Item2)
                .Subscribe(x => FindTargetMz = false);

            // ChargeRange: Make sure the values are not equal, and a valid range
            this.WhenAnyValue(x => x.ChargeMin).Subscribe(x =>
                ChargeMax = x > ChargeMax ? Math.Min(x + 1, 20) : ChargeMax);
            this.WhenAnyValue(x => x.ChargeMax).Subscribe(x =>
                ChargeMin = x < ChargeMin ? Math.Max(x - 1, -20) : ChargeMin);
            this.WhenAnyValue(x => x.CanVerifyHydrogens).Where(x => !x)
                .Subscribe(x => VerifyHydrogens = false);

            // Update the values in the ResultsSortModes combo box
            this.WhenAnyValue(x => x.FindCharge, x => x.FindMz).Subscribe(x => UpdateSortOptions());

            // Show or hide the FindMz and LimitChargeRange buttons depending on FindCharge's value; FindMz is invalid in 'MatchPercentComposition' mode
            allowFindMz = this.WhenAnyValue(x => x.FindCharge, x => x.MatchMolecularWeight, x => x.AllowChanges)
                .Select(x => x.Item1 && x.Item2 && x.Item3).ToProperty(this, x => x.AllowFindMz);
            // If LimitChargeRange is checked and FindMz is checked then show FindTargetMz
            findMzLimitedChargeRange = this.WhenAnyValue(x => x.FindMz, x => x.LimitChargeRange, x => x.AllowChanges)
                .Select(x => x.Item1 && x.Item2 && x.Item3).ToProperty(this, x => x.FindMzLimitedChargeRange);
            // If FormulaSearchMode is bounded search, then show the auto adjust min and max checkbox
            enableAutoSetBounds = this.WhenAnyValue(x => x.SearchMode, x => x.AllowChanges)
                .Select(x => x.Item1 == FormulaSearchModes.Bounded && x.Item2).ToProperty(this, x => x.EnableAutoSetBounds);
        }

        private readonly SourceList<SearchResultsSortMode> resultsSortModeBackingList = new SourceList<SearchResultsSortMode>();

        private bool findCharge = true;
        private bool limitChargeRange;
        private int chargeMin = -4;
        private int chargeMax = 4;
        private readonly ObservableAsPropertyHelper<bool> allowFindMz;
        private bool findMz;
        private bool findTargetMz = false;
        private readonly ObservableAsPropertyHelper<bool> findMzLimitedChargeRange;
        private bool sortResults = true;
        private bool verifyHydrogens = true;
        private bool canVerifyHydrogens = true;
        private SearchResultsSortMode resultsSortMode;
        private FormulaSearchModes searchMode = FormulaSearchModes.Thorough;
        private bool autoSetBounds;
        private readonly ObservableAsPropertyHelper<bool> enableAutoSetBounds;
        private bool matchMolecularWeight = true;
        private bool matchPercentCompositions;
        private readonly ObservableAsPropertyHelper<bool> allowChanges;
        private readonly ObservableAsPropertyHelper<bool> allowLimitChargeRange;
        private readonly ObservableAsPropertyHelper<bool> enableVerifyHydrogens;

        public IReadOnlyList<FormulaSearchModes> FormulaSearchModeOptions { get; }
        public ReadOnlyObservableCollection<SearchResultsSortMode> ResultSortModeOptions { get; }

        public bool FindCharge
        {
            get => findCharge;
            set => this.RaiseAndSetIfChanged(ref findCharge, value);
        }

        public bool LimitChargeRange
        {
            get => limitChargeRange;
            set => this.RaiseAndSetIfChanged(ref limitChargeRange, value);
        }

        public int ChargeMin
        {
            get => chargeMin;
            set => this.RaiseAndSetIfChanged(ref chargeMin, value);
        }

        public int ChargeMax
        {
            get => chargeMax;
            set => this.RaiseAndSetIfChanged(ref chargeMax, value);
        }

        public bool AllowFindMz => allowFindMz.Value;

        public bool FindMz
        {
            get => findMz;
            set => this.RaiseAndSetIfChanged(ref findMz, value);
        }

        public bool FindTargetMz
        {
            get => findTargetMz;
            set => this.RaiseAndSetIfChanged(ref findTargetMz, value);
        }

        public bool FindMzLimitedChargeRange => findMzLimitedChargeRange.Value;

        public bool SortResults
        {
            get => sortResults;
            set => this.RaiseAndSetIfChanged(ref sortResults, value);
        }

        public bool VerifyHydrogens
        {
            get => verifyHydrogens;
            set => this.RaiseAndSetIfChanged(ref verifyHydrogens, value);
        }

        public bool CanVerifyHydrogens
        {
            get => canVerifyHydrogens;
            set => this.RaiseAndSetIfChanged(ref canVerifyHydrogens, value);
        }

        public SearchResultsSortMode ResultsSortMode
        {
            get => resultsSortMode;
            set => this.RaiseAndSetIfChanged(ref resultsSortMode, value);
        }

        public FormulaSearchModes SearchMode
        {
            get => searchMode;
            set => this.RaiseAndSetIfChanged(ref searchMode, value);
        }

        public bool AutoSetBounds
        {
            get => autoSetBounds;
            set => this.RaiseAndSetIfChanged(ref autoSetBounds, value);
        }

        public bool EnableAutoSetBounds => enableAutoSetBounds.Value;

        public bool MatchMolecularWeight
        {
            get => matchMolecularWeight;
            set => this.RaiseAndSetIfChanged(ref matchMolecularWeight, value);
        }

        public bool MatchPercentCompositions
        {
            get => matchPercentCompositions;
            set => this.RaiseAndSetIfChanged(ref matchPercentCompositions, value);
        }

        public bool EnableVerifyHydrogens => enableVerifyHydrogens.Value;
        public bool AllowLimitChargeRange => allowLimitChargeRange.Value;
        public bool AllowChanges => allowChanges.Value;

        private void UpdateSortOptions()
        {
            // Update the values in the ResultSortMode combo box
            resultsSortModeBackingList.Edit(list =>
            {
                if (list.Contains(SearchResultsSortMode.SortByCharge))
                {
                    if (!FindCharge)
                    {
                        list.Remove(SearchResultsSortMode.SortByCharge);
                    }
                }
                else if (FindCharge)
                {
                    list.Add(SearchResultsSortMode.SortByCharge);
                }

                if (list.Contains(SearchResultsSortMode.SortByMZ))
                {
                    if (!FindMz)
                    {
                        list.Remove(SearchResultsSortMode.SortByMZ);
                    }
                }
                else if (FindMz)
                {
                    list.Add(SearchResultsSortMode.SortByMZ);
                }
            });

            if ((!FindCharge && ResultsSortMode == SearchResultsSortMode.SortByCharge) ||
                (!FindMz && ResultsSortMode == SearchResultsSortMode.SortByMZ))
            {
                ResultsSortMode = SearchResultsSortMode.SortByFormula;
            }
        }

        public SearchOptions GetSearchOptions()
        {
            return new SearchOptions
            {
                FindCharge = FindCharge,
                LimitChargeRange = LimitChargeRange,
                ChargeMin = ChargeMin,
                ChargeMax = ChargeMax,
                FindTargetMz = FindTargetMz,
                SearchMode = SearchMode,
                VerifyHydrogens = VerifyHydrogens
            };
        }
    }
}
