using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("E57F9856-E7CF-42D4-A978-5877C2B758D7"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface ICandidateElementTolerances
    {
        double TargetPercentComposition { get; set; }
        int MinimumCount { get; set; }
        int MaximumCount { get; set; }
    }
}