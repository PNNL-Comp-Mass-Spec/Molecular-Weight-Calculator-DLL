namespace MolecularWeightCalculator.Formula
{
    /// <summary>
    /// Data container for ElementAndMassTools
    /// </summary>
    internal class IsoResultsOverallData
    {
        public float Abundance { get; set; }
        public int Multiplicity { get; set; }

        /// <summary>
        /// Show the abundance
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0:F2}", Abundance);
        }
    }
}
