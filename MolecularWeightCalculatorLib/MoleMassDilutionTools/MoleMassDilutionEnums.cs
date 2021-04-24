using System.ComponentModel;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace MolecularWeightCalculator
{
    [Guid("C4109F21-3649-44CF-BF77-8DC784C801B3"), ComVisible(true)]
    public enum AutoComputeDilutionMode
    {
        [Description("Find Solution Volumes")]
        FindRequiredDilutionVolumes = 0,
        [Description("Find Solvent and Final Volumes")]
        FindRequiredTotalVolume,
        [Description("Find Final Concentration")]
        FindFinalConcentration,
        [Description("Find Initial Concentration")]
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
        [Description("Moles")]
        Moles = 0,
        [Description("milliMoles")]
        Millimoles,
        [Description("microMoles")]
        MicroMoles,
        [Description("nanoMoles")]
        NanoMoles,
        [Description("picoMoles")]
        PicoMoles,
        [Description("femtoMoles")]
        FemtoMoles,
        [Description("attoMoles")]
        AttoMoles,
        [Description("Kilograms")]
        Kilograms,
        [Description("Grams")]
        Grams,
        [Description("Milligrams")]
        Milligrams,
        [Description("Micrograms")]
        Micrograms,
        [Description("Pounds")]
        Pounds,
        [Description("Ounces")]
        Ounces,
        [Description("Liters")]
        Liters,
        [Description("Deciliters")]
        DeciLiters,
        [Description("Milliliters")]
        MilliLiters,
        [Description("Microliters")]
        MicroLiters,
        [Description("Nanoliters")]
        NanoLiters,
        [Description("Picoliters")]
        PicoLiters,
        [Description("Gallons")]
        Gallons,
        [Description("Quarts")]
        Quarts,
        [Description("Pints")]
        Pints
    }

    [Guid("0FD1BC9F-19C0-4449-B639-55A20187E6E2"), ComVisible(true)]
    public enum UnitOfExtendedVolume
    {
        [Description("Liters")]
        L = 0,
        [Description("Deciliters")]
        DL,
        [Description("Milliliters")]
        ML,
        [Description("Microliters")]
        UL,
        [Description("Nanoliters")]
        NL,
        [Description("Picoliters")]
        PL,
        [Description("Gallons")]
        Gallons,
        [Description("Quarts")]
        Quarts,
        [Description("Pints")]
        Pints
    }

    [Guid("5D63EAA6-FC5C-46F9-AD96-74DD8DF2BF44"), ComVisible(true)]
    public enum UnitOfMoleMassConcentration
    {
        [Description("Molar")]
        Molar = 0,
        [Description("milliMolar")]
        MilliMolar,
        [Description("microMolar")]
        MicroMolar,
        [Description("nanoMolar")]
        NanoMolar,
        [Description("picoMolar")]
        PicoMolar,
        [Description("femtoMolar")]
        FemtoMolar,
        [Description("attoMolar")]
        AttoMolar,
        [Description("mg/dL")]
        MgPerDL,
        [Description("mg/mL")]
        MgPerML,
        [Description("ug/mL")]
        UgPerML,
        [Description("ng/mL")]
        NgPerML,
        [Description("ug/uL")]
        UgPerUL,
        [Description("ng/uL")]
        NgPerUL
    }
}
