using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator
{
    [ComVisible(true)]
    public enum AutoComputeDilutionMode
    {
        FindRequiredDilutionVolumes = 0,
        FindRequiredTotalVolume,
        FindFinalConcentration,
        FindInitialConcentration
    }

    [ComVisible(true)]
    public enum AutoComputeQuantityMode
    {
        FindAmount = 0,
        FindVolume,
        FindConcentration
    }

    [ComVisible(true)]
    public enum Unit
    {
        Moles = 0,
        Millimoles,
        MicroMoles,
        NanoMoles,
        PicoMoles,
        FemtoMoles,
        AttoMoles,
        Kilograms,
        Grams,
        Milligrams,
        Micrograms,
        Pounds,
        Ounces,
        Liters,
        DeciLiters,
        MilliLiters,
        MicroLiters,
        NanoLiters,
        PicoLiters,
        Gallons,
        Quarts,
        Pints
    }

    [ComVisible(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum UnitOfExtendedVolume
    {
        L = 0,
        DL,
        ML,
        UL,
        NL,
        PL,
        Gallons,
        Quarts,
        Pints
    }

    [ComVisible(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum UnitOfMoleMassConcentration
    {
        Molar = 0,
        MilliMolar,
        MicroMolar,
        NanoMolar,
        PicoMolar,
        FemtoMolar,
        AttoMolar,
        MgPerDL,
        MgPerML,
        UgPerML,
        NgPerML,
        UgPerUL,
        NgPerUL
    }
}
