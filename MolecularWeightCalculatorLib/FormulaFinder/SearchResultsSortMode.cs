using System.ComponentModel;

namespace MolecularWeightCalculator.FormulaFinder
{
    public enum SearchResultsSortMode
    {
        [Description("Sort By Formula")]
        SortByFormula = 0,
        [Description("Sort by Charge")]
        SortByCharge = 1,
        [Description("Sort By MWT")]
        SortByMWT = 2,
        [Description("Sort by m/z")]
        SortByMZ = 3,
        [Description("Sort By Abs(Delta Mass)")]
        SortByDeltaMass = 4,
    }
}
