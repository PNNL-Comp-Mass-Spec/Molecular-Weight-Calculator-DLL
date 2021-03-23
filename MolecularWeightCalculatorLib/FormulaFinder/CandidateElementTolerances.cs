namespace MolecularWeightCalculator.FormulaFinder
{
    /// <summary>
    /// Search tolerances for each element
    /// </summary>
    /// <remarks>
    /// Target percent composition values must be between 0 and 100; they are only used when calling FindMatchesByPercentComposition
    /// MinimumCount and MaximumCount are only used when the search mode is Bounded; they are ignored for Thorough search
    /// </remarks>
    public class CandidateElementTolerances
    {
        public double TargetPercentComposition { get; set; }
        public int MinimumCount { get; set; }
        public int MaximumCount { get; set; }
    }
}