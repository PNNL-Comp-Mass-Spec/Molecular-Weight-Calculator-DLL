namespace MolecularWeightCalculator.Data
{
    public class MassAbundance
    {
        public double Mass { get; set; }
        public double Abundance { get; set; }

        public MassAbundance(double mass, double abundance)
        {
            Mass = mass;
            Abundance = abundance;
        }

        public MassAbundanceImmutable ToReadOnly()
        {
            return new MassAbundanceImmutable(Mass, Abundance);
        }

        /// <summary>
        /// Show the mass and abundance values
        /// </summary>
        public override string ToString()
        {
            return $"{Mass:F2}, {Abundance:F2}";
        }
    }
}
