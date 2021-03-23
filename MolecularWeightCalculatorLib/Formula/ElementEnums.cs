namespace MolecularWeightCalculator.Formula
{
    public enum ElementMassMode
    {
        Average = 1,
        Isotopic = 2,
        Integer = 3
    }

    public enum StdDevMode
    {
        Short = 0,
        Scientific = 1,
        Decimal = 2
    }

    public enum CaseConversionMode
    {
        ConvertCaseUp = 0,
        ExactCase = 1,
        SmartCase = 2
    }
}
