namespace MolecularWeightCalculator.Sequence
{
    public enum CTerminusGroupType
    {
        Hydroxyl = 0,
        Amide = 1,
        None = 2
    }

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

    public enum IonType
    {
        AIon = 0,
        BIon = 1,
        YIon = 2,
        CIon = 3,
        ZIon = 4
    }
}
