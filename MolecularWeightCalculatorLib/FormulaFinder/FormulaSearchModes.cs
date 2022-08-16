using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.FormulaFinder
{
    [Guid("4B7F5830-621E-454E-B8D5-FD1EA01F30C2"), ComVisible(true)]
    public enum FormulaSearchModes
    {
        [Description("Thorough Search")]
        Thorough = 0,
        [Description("Bounded Search")]
        Bounded = 1
    }
}
