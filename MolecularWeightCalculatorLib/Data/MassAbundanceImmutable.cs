using System.Collections.Generic;

namespace MolecularWeightCalculator.Data
{
    public readonly struct MassAbundanceImmutable
    {
        public double Mass { get; }
        public double Abundance { get; }

        public MassAbundanceImmutable(double mass, double abundance)
        {
            Mass = mass;
            Abundance = abundance;
        }

        public MassAbundanceImmutable(KeyValuePair<double, double> pair)
        {
            Mass = pair.Key;
            Abundance = pair.Value;
        }

        public MassAbundance ToMassAbundance()
        {
            return new MassAbundance(Mass, Abundance);
        }

        public KeyValuePair<double, double> ToKeyValuePair()
        {
            return new KeyValuePair<double, double>(Mass, Abundance);
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
