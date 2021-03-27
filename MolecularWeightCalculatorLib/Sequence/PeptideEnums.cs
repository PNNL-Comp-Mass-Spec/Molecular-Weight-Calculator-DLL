using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Sequence
{
    [Guid("D2E8E61C-ACE9-4D5F-9531-2D237DB31609"), ComVisible(true)]
    public enum CTerminusGroupType
    {
        Hydroxyl = 0,
        Amide = 1,
        None = 2
    }

    [Guid("DFBF68F5-FF11-4950-9A59-767DA221453B"), ComVisible(true)]
    public enum NTerminusGroupType
    {
        Hydrogen = 0,
        HydrogenPlusProton = 1,
        Acetyl = 2,
        PyroGlu = 3,
        Carbamyl = 4,
        // ReSharper disable once InconsistentNaming
        PTC = 5,
        None = 6
    }

    [Guid("F9B6D73E-A347-4174-97D6-9560D9F7DB66"), ComVisible(true)]
    public enum IonType
    {
        AIon = 0,
        BIon = 1,
        YIon = 2,
        CIon = 3,
        ZIon = 4
    }
}
