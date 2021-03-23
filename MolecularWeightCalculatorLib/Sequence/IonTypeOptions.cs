namespace MolecularWeightCalculator.Sequence
{
    public class IonTypeOptions
    {
        // Note: A ions can have ammonia and phosphate loss, but not water loss, so this is set to false by default
        public bool ShowIon { get; set; }
        public bool NeutralLossWater { get; set; }
        public bool NeutralLossAmmonia { get; set; }
        public bool NeutralLossPhosphate { get; set; }
    }
}