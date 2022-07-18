using System.ComponentModel;

namespace MolecularWeightCalculatorGUI.IsotopicDistribution
{
    internal enum IsotopicPlotMode
    {
        [Description("Sticks to Zero")]
        SticksToZero = 0,
        [Description("Gaussian Peaks")]
        Gaussian = 1,
        [Description("Lines Between Points")]
        ContinuousData = 2,  // Used for raw, uncentered data
    }
}
