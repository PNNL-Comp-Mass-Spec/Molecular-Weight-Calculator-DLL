using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.FormulaFinder
{
    /// <summary>
    /// Search tolerances for each element
    /// </summary>
    /// <remarks>
    /// Target percent composition values must be between 0 and 100; they are only used when calling FindMatchesByPercentComposition
    /// MinimumCount and MaximumCount are only used when the search mode is Bounded; they are ignored for Thorough search
    /// </remarks>
    [Guid("91FCC514-4908-4F0C-8B7C-CB4D20803A81"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class CandidateElementTolerances : ICandidateElementTolerances
    {
        public double TargetPercentComposition { get; set; }
        public int MinimumCount { get; set; }
        public int MaximumCount { get; set; }
    }
}
