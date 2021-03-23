using System.Diagnostics.CodeAnalysis;

namespace MolecularWeightCalculator
{
    public enum AutoComputeDilutionMode
    {
        FindRequiredDilutionVolumes = 0,
        FindRequiredTotalVolume,
        FindFinalConcentration,
        FindInitialConcentration
    }

    public enum AutoComputeQuantityMode
    {
        FindAmount = 0,
        FindVolume,
        FindConcentration
    }

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
