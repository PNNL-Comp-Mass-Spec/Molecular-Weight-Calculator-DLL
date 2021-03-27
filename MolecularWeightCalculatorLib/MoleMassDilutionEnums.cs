using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator
{
    [Guid("C4109F21-3649-44CF-BF77-8DC784C801B3"), ComVisible(true)]
    public enum AutoComputeDilutionMode
    {
        FindRequiredDilutionVolumes = 0,
        FindRequiredTotalVolume,
        FindFinalConcentration,
        FindInitialConcentration
    }

    [Guid("846B1DF9-C588-4A35-8AAE-ECAC94B9DFAC"), ComVisible(true)]
    public enum AutoComputeQuantityMode
    {
        FindAmount = 0,
        FindVolume,
        FindConcentration
    }

    [Guid("02F2CF0A-E219-48B5-8CEB-AFCACC3FBB91"), ComVisible(true)]
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

    [Guid("0FD1BC9F-19C0-4449-B639-55A20187E6E2"), ComVisible(true)]
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

    [Guid("5D63EAA6-FC5C-46F9-AD96-74DD8DF2BF44"), ComVisible(true)]
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
