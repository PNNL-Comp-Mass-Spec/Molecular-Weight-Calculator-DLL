using System.ComponentModel;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace MolecularWeightCalculator
{
    [Guid("9DACDA73-0875-41D8-AECA-90651C7DC510"), ComVisible(true)]
    public enum CapillaryType
    {
        [Description("Open Tubular Capillary")]
        OpenTubularCapillary = 0,
        [Description("Packed Capillary")]
        PackedCapillary
    }

    [Guid("ED5D8D28-FE6D-4FE4-B0F6-1F521971A3D1"), ComVisible(true)]
    public enum UnitOfPressure
    {
        [Description("psi")]
        Psi = 0,
        [Description("Pascals")]
        Pascals,
        [Description("kiloPascals")]
        KiloPascals,
        [Description("Atmospheres")]
        Atmospheres,
        [Description("Bar")]
        Bar,
        [Description("Torr (mm Hg)")]
        Torr,
        [Description("dynes/cm^2")]
        DynesPerSquareCm
    }

    [Guid("76D1C162-CAE8-4921-9928-DF229775B629"), ComVisible(true)]
    public enum UnitOfLength
    {
        [Description("m")]
        M = 0,
        [Description("cm")]
        CM,
        [Description("mm")]
        MM,
        [Description("um")]
        Microns,
        [Description("inches")]
        Inches
    }

    [Guid("FDB804A9-0A8C-469E-86CB-3040A7B5116C"), ComVisible(true)]
    public enum UnitOfViscosity
    {
        [Description("Poise [g/(cm-sec)]")]
        Poise = 0,
        [Description("centiPoise")]
        CentiPoise
    }

    [Guid("88C1C17B-88A2-4B1F-B739-C78FD27D6FEB"), ComVisible(true)]
    public enum UnitOfFlowRate
    {
        [Description("mL/min")]
        MLPerMin = 0,
        [Description("uL/min")]
        ULPerMin,
        [Description("nL/min")]
        NLPerMin
    }

    [Guid("5191EEFB-1AB7-452D-BF6D-D8F6139B53DB"), ComVisible(true)]
    public enum UnitOfLinearVelocity
    {
        [Description("cm/hr")]
        CmPerHr = 0,
        [Description("mm/hr")]
        MmPerHr,
        [Description("cm/min")]
        CmPerMin,
        [Description("mm/min")]
        MmPerMin,
        [Description("cm/sec")]
        CmPerSec,
        [Description("mm/sec")]
        MmPerSec
    }

    [Guid("94F70EBF-8E33-42A2-83C0-F98BD138078E"), ComVisible(true)]
    public enum UnitOfTime
    {
        [Description("hours")]
        Hours = 0,
        [Description("minutes")]
        Minutes,
        [Description("seconds")]
        Seconds
    }

    [Guid("97E85E7B-3319-4FC3-87E5-60D3B5A28B91"), ComVisible(true)]
    public enum UnitOfVolume
    {
        [Description("mL")]
        ML = 0,
        [Description("uL")]
        UL,
        [Description("nL")]
        NL,
        [Description("pL")]
        PL
    }

    [Guid("1DAEC84C-DD67-4E62-9975-53B74C22BF3C"), ComVisible(true)]
    public enum UnitOfConcentration
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

    [Guid("19EFF331-E247-4733-BF5C-F9E87285DF87"), ComVisible(true)]
    public enum UnitOfTemperature
    {
        [Description("Celsius")]
        Celsius = 0,
        [Description("Kelvin")]
        Kelvin,
        [Description("Fahrenheit")]
        Fahrenheit
    }

    [Guid("5726243F-E12C-409E-AC8F-1095436C397D"), ComVisible(true)]
    public enum UnitOfMassFlowRate
    {
        [Description("pmol/min")]
        PmolPerMin = 0,
        [Description("fmol/min")]
        FmolPerMin,
        [Description("amol/min")]
        AmolPerMin,
        [Description("pmol/sec")]
        PmolPerSec,
        [Description("fmol/sec")]
        FmolPerSec,
        [Description("amol/sec")]
        AmolPerSec,
        [Description("mol/min")]
        MolesPerMin
    }

    [Guid("0E982894-E072-4122-930D-B5E4583BB31E"), ComVisible(true)]
    public enum UnitOfMolarAmount
    {
        [Description("Moles")]
        Moles = 0,
        [Description("milliMoles")]
        MilliMoles,
        [Description("microMoles")]
        MicroMoles,
        [Description("nanoMoles")]
        NanoMoles,
        [Description("picoMoles")]
        PicoMoles,
        [Description("femtoMoles")]
        FemtoMoles,
        [Description("attoMoles")]
        AttoMoles
    }

    [Guid("D00EA5CC-DC9C-44CE-A96F-649793625D1B"), ComVisible(true)]
    public enum UnitOfDiffusionCoefficient
    {
        [Description("cm^2/hr")]
        CmSquaredPerHr = 0,
        [Description("cm^2/min")]
        CmSquaredPerMin,
        [Description("cm^2/sec")]
        CmSquaredPerSec
    }

    [Guid("DE5DBA1F-A19C-4EB2-AFF4-7F3770ECFB5F"), ComVisible(true)]
    public enum AutoComputeMode
    {
        [Description("Find Back Pressure")]
        BackPressure = 0,
        [Description("Find Inner Diameter")]
        ColumnId,
        [Description("Find Column Length")]
        ColumnLength,
        [Description("Find Dead Time")]
        DeadTime,
        [Description("Find Linear Velocity")]
        LinearVelocity,
        [Description("Find Volumetric Flow rate")]
        VolFlowRate,
        [Description("Find Flow Rate using Dead Time")]
        VolFlowRateUsingDeadTime
    }
}
