namespace MolecularWeightCalculator.Formula
{
    /// <summary>
    /// Data container for ElementAndMassTools
    /// </summary>
    internal class IsoResultsByElement
    {
        /// <summary>
        /// Index of element in ElementStats[] array; look in ElementStats[] to get information on its isotopes
        /// </summary>
        public int AtomicNumber { get; }

        /// <summary>
        /// True if this class is tracking stats related to an isotope explicitly listed in the formula, e.g. ^13C6H6
        /// </summary>
        public bool ExplicitIsotope { get; }

        /// <summary>
        /// Explicit mass
        /// </summary>
        /// <remarks>
        /// User-supplied isotopic mass, e.g. 13 for ^13C6H6
        /// </remarks>
        public double ExplicitMass { get; }

        /// <summary>
        /// Number of atoms of this element in the formula being parsed
        /// </summary>
        public int AtomCount { get; }

        /// <summary>
        /// Number of masses in MassAbundances; changed at times for data filtering purposes
        /// </summary>
        public int ResultsCount { get; set; }

        /// <summary>
        /// Starting mass of the results for this element
        /// </summary>
        public int StartingResultsMass { get; set; }

        /// <summary>
        /// Abundance of each mass, starting with StartingResultsMass; 0-based array (can't change to a list)
        /// </summary>
        public float[] MassAbundances { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="atomicNumber"></param>
        /// <param name="atomCount"></param>
        /// <param name="explicitMass"></param>
        /// <param name="explicitIsotope">True if this class is tracking stats related to an isotope explicitly listed in the formula, e.g. ^13C6H6</param>
        public IsoResultsByElement(int atomicNumber, int atomCount, double explicitMass, bool explicitIsotope = false)
        {
            AtomicNumber = atomicNumber;
            AtomCount = atomCount;
            ExplicitMass = explicitMass;
            ExplicitIsotope = explicitIsotope;

            ResultsCount = 0;
            MassAbundances = new float[1];
        }

        public void SetArraySize(int count)
        {
            MassAbundances = new float[count];
        }

        /// <summary>
        /// Show the element atomic number and atom count
        /// </summary>
        public override string ToString()
        {
            return string.Format("Element {0}: {1} atoms", AtomicNumber, AtomCount);
        }
    }
}
