using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator
{
    [Guid("9DACDA73-0875-41D8-AECA-90651C7DC510"), ComVisible(true)]
    public enum CapillaryType
    {
        OpenTubularCapillary = 0,
        PackedCapillary
    }

    [Guid("ED5D8D28-FE6D-4FE4-B0F6-1F521971A3D1"), ComVisible(true)]
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

    [Guid("76D1C162-CAE8-4921-9928-DF229775B629"), ComVisible(true)]
    public enum UnitOfLength
    {
        M = 0,
        CM,
        MM,
        Microns,
        Inches
    }

    [Guid("FDB804A9-0A8C-469E-86CB-3040A7B5116C"), ComVisible(true)]
    public enum UnitOfViscosity
    {
        Poise = 0,
        CentiPoise
    }

    [Guid("88C1C17B-88A2-4B1F-B739-C78FD27D6FEB"), ComVisible(true)]
    public enum UnitOfFlowRate
    {
        MLPerMin = 0,
        ULPerMin,
        NLPerMin
    }

    [Guid("5191EEFB-1AB7-452D-BF6D-D8F6139B53DB"), ComVisible(true)]
    public enum UnitOfLinearVelocity
    {
        CmPerHr = 0,
        MmPerHr,
        CmPerMin,
        MmPerMin,
        CmPerSec,
        MmPerSec
    }

    [Guid("94F70EBF-8E33-42A2-83C0-F98BD138078E"), ComVisible(true)]
    public enum UnitOfTime
    {
        Hours = 0,
        Minutes,
        Seconds
    }

    [Guid("97E85E7B-3319-4FC3-87E5-60D3B5A28B91"), ComVisible(true)]
    public enum UnitOfVolume
    {
        ML = 0,
        UL,
        NL,
        PL
    }

    [Guid("1DAEC84C-DD67-4E62-9975-53B74C22BF3C"), ComVisible(true)]
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

    [Guid("19EFF331-E247-4733-BF5C-F9E87285DF87"), ComVisible(true)]
    public enum UnitOfTemperature
    {
        Celsius = 0,
        Kelvin,
        Fahrenheit
    }

    [Guid("5726243F-E12C-409E-AC8F-1095436C397D"), ComVisible(true)]
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

    [Guid("0E982894-E072-4122-930D-B5E4583BB31E"), ComVisible(true)]
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

    [Guid("D00EA5CC-DC9C-44CE-A96F-649793625D1B"), ComVisible(true)]
    public enum UnitOfDiffusionCoefficient
    {
        CmSquaredPerHr = 0,
        CmSquaredPerMin,
        CmSquaredPerSec
    }

    [Guid("DE5DBA1F-A19C-4EB2-AFF4-7F3770ECFB5F"), ComVisible(true)]
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
