using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator
{
    [ComVisible(true)]
    public enum CapillaryType
    {
        OpenTubularCapillary = 0,
        PackedCapillary
    }

    [ComVisible(true)]
    public enum UnitOfPressure
    {
        Psi = 0,
        Pascals,
        KiloPascals,
        Atmospheres,
        Bar,
        Torr,
        DynesPerSquareCm
    }

    [ComVisible(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum UnitOfLength
    {
        M = 0,
        CM,
        MM,
        Microns,
        Inches
    }

    [ComVisible(true)]
    public enum UnitOfViscosity
    {
        Poise = 0,
        CentiPoise
    }

    [ComVisible(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum UnitOfFlowRate
    {
        MLPerMin = 0,
        ULPerMin,
        NLPerMin
    }

    [ComVisible(true)]
    public enum UnitOfLinearVelocity
    {
        CmPerHr = 0,
        MmPerHr,
        CmPerMin,
        MmPerMin,
        CmPerSec,
        MmPerSec
    }

    [ComVisible(true)]
    public enum UnitOfTime
    {
        Hours = 0,
        Minutes,
        Seconds
    }

    [ComVisible(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum UnitOfVolume
    {
        ML = 0,
        UL,
        NL,
        PL
    }

    [ComVisible(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum UnitOfConcentration
    {
        Molar = 0,
        MilliMolar,
        MicroMolar,
        NanoMolar,
        PicoMolar,
        FemtoMolar,
        AttoMolar,
        MgPerML,
        UgPerML,
        NgPerML,
        UgPerUL,
        NgPerUL
    }

    [ComVisible(true)]
    public enum UnitOfTemperature
    {
        Celsius = 0,
        Kelvin,
        Fahrenheit
    }

    [ComVisible(true)]
    public enum UnitOfMassFlowRate
    {
        PmolPerMin = 0,
        FmolPerMin,
        AmolPerMin,
        PmolPerSec,
        FmolPerSec,
        AmolPerSec,
        MolesPerMin
    }

    [ComVisible(true)]
    public enum UnitOfMolarAmount
    {
        Moles = 0,
        MilliMoles,
        MicroMoles,
        NanoMoles,
        PicoMoles,
        FemtoMoles,
        AttoMoles
    }

    [ComVisible(true)]
    public enum UnitOfDiffusionCoefficient
    {
        CmSquaredPerHr = 0,
        CmSquaredPerMin,
        CmSquaredPerSec
    }

    [ComVisible(true)]
    public enum AutoComputeMode
    {
        BackPressure = 0,
        ColumnId,
        ColumnLength,
        DeadTime,
        LinearVelocity,
        VolFlowRate,
        VolFlowRateUsingDeadTime
    }
}
