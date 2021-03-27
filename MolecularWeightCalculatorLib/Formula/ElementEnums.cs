using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [Guid("D2E4106D-07BC-46C4-9FAA-7B286E8527AA"), ComVisible(true)]
    public enum ElementMassMode
    {
        Average = 1,
        Isotopic = 2,
        Integer = 3
    }

    [Guid("3BF63FE7-A86E-4C78-9A86-D3762F7292D4"), ComVisible(true)]
    public enum StdDevMode
    {
        Short = 0,
        Scientific = 1,
        Decimal = 2
    }

    [Guid("D9D6E8B4-1990-46F2-B838-B86E447C1D20"), ComVisible(true)]
    public enum CaseConversionMode
    {
        ConvertCaseUp = 0,
        ExactCase = 1,
        SmartCase = 2
    }
}
