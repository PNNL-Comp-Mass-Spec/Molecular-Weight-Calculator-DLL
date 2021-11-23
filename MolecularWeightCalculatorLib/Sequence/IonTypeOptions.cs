using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Sequence
{
    /// <summary>
    /// MS/MS fragmentation ion type options
    /// </summary>
    [ComVisible(false)]
    public class IonTypeOptions
    {
        public bool ShowIon { get; set; }
        public bool NeutralLossWater { get; set; }
        public bool NeutralLossAmmonia { get; set; }
        public bool NeutralLossPhosphate { get; set; }
    }
}