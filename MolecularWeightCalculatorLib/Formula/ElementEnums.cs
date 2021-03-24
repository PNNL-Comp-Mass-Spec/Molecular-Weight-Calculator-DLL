using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(true)]
    public enum ElementMassMode
    {
        Average = 1,
        Isotopic = 2,
        Integer = 3
    }

    [ComVisible(true)]
    public enum StdDevMode
    {
        Short = 0,
        Scientific = 1,
        Decimal = 2
    }

    [ComVisible(true)]
    public enum CaseConversionMode
    {
        ConvertCaseUp = 0,
        ExactCase = 1,
        SmartCase = 2
    }
}
