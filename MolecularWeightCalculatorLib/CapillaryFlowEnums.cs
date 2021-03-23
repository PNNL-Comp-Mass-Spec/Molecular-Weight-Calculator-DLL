using System.Diagnostics.CodeAnalysis;

namespace MolecularWeightCalculator
{
    public enum CapillaryType
    {
        OpenTubularCapillary = 0,
        PackedCapillary
    }

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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum UnitOfLength
    {
        M = 0,
        CM,
        MM,
        Microns,
        Inches
    }

    public enum UnitOfViscosity
    {
        Poise = 0,
        CentiPoise
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum UnitOfFlowRate
    {
        MLPerMin = 0,
        ULPerMin,
        NLPerMin
    }

    public enum UnitOfLinearVelocity
    {
        CmPerHr = 0,
        MmPerHr,
        CmPerMin,
        MmPerMin,
        CmPerSec,
        MmPerSec
    }

    public enum UnitOfTime
    {
        Hours = 0,
        Minutes,
        Seconds
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum UnitOfVolume
    {
        ML = 0,
        UL,
        NL,
        PL
    }

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

    public enum UnitOfTemperature
    {
        Celsius = 0,
        Kelvin,
        Fahrenheit
    }

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

    public enum UnitOfDiffusionCoefficient
    {
        CmSquaredPerHr = 0,
        CmSquaredPerMin,
        CmSquaredPerSec
    }

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
