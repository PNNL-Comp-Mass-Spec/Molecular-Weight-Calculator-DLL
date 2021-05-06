using System.ComponentModel;

namespace MolecularWeightCalculatorGUI
{
    internal enum MassChargeLevel
    {
        [Description("M (uncharged)")]
        M = 0,
        [Description("[M+H]1+")]
        MPlus1H = 1,
        [Description("[M+2H]2+")]
        MPlus2H = 2,
        [Description("[M+3H]3+")]
        MPlus3H = 3,
        [Description("[M+4H]4+")]
        MPlus4H = 4,
        [Description("[M+5H]5+")]
        MPlus5H = 5,
        [Description("[M+6H]6+")]
        MPlus6H = 6,
        [Description("[M+7H]7+")]
        MPlus7H = 7,
        [Description("[M+8H]8+")]
        MPlus8H = 8,
        [Description("[M+9H]9+")]
        MPlus9H = 9,
    }
}
