namespace TransformIsotopeMassFile
{
    internal class IsotopeInfo
    {
        /// <summary>
        /// Atomic number
        /// </summary>
        public int AtomicNumber { get; }

        /// <summary>
        /// Atomic symbol
        /// </summary>
        public string AtomicSymbol { get; set; }

        /// <summary>
        /// Mass number
        /// </summary>
        /// <remarks>
        /// Total number of protons and neutrons
        /// </remarks>
        public int MassNumber { get; set; }

        /// <summary>
        /// Monoisotopic mass
        /// </summary>
        public double? RelativeAtomicMass { get; set; }

        /// <summary>
        /// Uncertainty in RelativeAtomicMass
        /// </summary>
        public double? RelativeAtomicMassUncertainty { get; set; }

        /// <summary>
        /// Natural abundance of this isotope (as a number between 0 and 1)
        /// </summary>
        /// <remarks>
        /// Natural isotope abundance
        /// </remarks>
        public double? IsotopicComposition { get; set; }

        /// <summary>
        /// Uncertainty in IsotopicComposition
        /// </summary>
        public double? IsotopicCompositionUncertainty { get; set; }

        /// <summary>
        /// Average mass
        /// </summary>
        /// <remarks>
        /// When the input file shows a range (e.g. [10.806,10.821] for Boron)
        /// this program stores the average of the two listed numbers as the standard atomic weight
        /// </remarks>
        public double? StandardAtomicWeight { get; set; }

        /// <summary>
        /// Uncertainty in StandardAtomicWeight
        /// </summary>
        public double? StandardAtomicWeightUncertainty { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        /// <remarks>
        /// g: Geological materials are known in which the element has an isotopic composition outside the limits for normal material
        /// m: Modified isotopic compositions may be found in commercially available material because the material has been subjected to an undisclosed or inadvertent isotopic fractionation
        /// r: Range in isotopic composition of normal terrestrial material prevents a more precise standard atomic weight being given
        /// </remarks>
        public string Notes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public IsotopeInfo(int atomicNumber)
        {
            AtomicNumber = atomicNumber;
            AtomicSymbol = string.Empty;
            Notes = string.Empty;
        }
    }
}
