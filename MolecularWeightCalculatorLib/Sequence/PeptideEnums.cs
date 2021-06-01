using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Sequence
{
    [Guid("D2E8E61C-ACE9-4D5F-9531-2D237DB31609"), ComVisible(true)]
    public enum CTerminusGroupType
    {
        [Description("OH (hydroxide)")]
        Hydroxyl = 0,
        [Description("NH2 (amide)")]
        Amide = 1,
        [Description("(none)")]
        None = 2
    }

    [Guid("DFBF68F5-FF11-4950-9A59-767DA221453B"), ComVisible(true)]
    public enum NTerminusGroupType
    {
        [Description("H (hydrogen)")]
        Hydrogen = 0,
        [Description("HH+ (protonated)")]
        HydrogenPlusProton = 1,
        [Description("C2OH3 (acetyl)")]
        Acetyl = 2,
        [Description("C5O2NH6 (pyroglu)")]
        PyroGlu = 3,
        [Description("CONH2 (carbamyl)")]
        Carbamyl = 4,
        // ReSharper disable once InconsistentNaming
        [Description("C7H6NS (PTC)")]
        PTC = 5,
        [Description("(none)")]
        None = 6
    }

    [Guid("F9B6D73E-A347-4174-97D6-9560D9F7DB66"), ComVisible(true)]
    public enum IonType
    {
        [Description("A")]
        AIon = 0,
        [Description("B")]
        BIon = 1,
        [Description("Y")]
        YIon = 2,
        [Description("C")]
        CIon = 3,
        [Description("Z")]
        ZIon = 4
    }
}
