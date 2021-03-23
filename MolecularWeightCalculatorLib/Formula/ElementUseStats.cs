using System.Collections.Generic;

namespace MolecularWeightCalculator.Formula
{
    public class ElementUseStats
    {
        /// <summary>
        /// True if the element is present
        /// </summary>
        public bool Used { get; set; }

        /// <summary>
        /// Number of atoms of this element; can have a non-integer count, e.g., C5.5
        /// </summary>
        public double Count { get; set; }

        public double IsotopicCorrection { get; set; }

        /// <summary>
        /// Specific isotopes of the atom
        /// </summary>
        public List<IsotopicAtomInfo> Isotopes { get; private set; }

        public ElementUseStats()
        {
            Used = false;
            Count = 0;
            IsotopicCorrection = 0;
            Isotopes = new List<IsotopicAtomInfo>(3);
        }

        public ElementUseStats Clone()
        {
            // Start with a shallow copy for all value members
            var cloned = (ElementUseStats) MemberwiseClone();

            // Finish with a deep copy for all reference members
            cloned.Isotopes = new List<IsotopicAtomInfo>(Isotopes.Count);
            for (var i = 0; i < Isotopes.Count; i++)
            {
                cloned.Isotopes.Add(Isotopes[i]?.Clone());
            }

            return cloned;
        }

        public override string ToString()
        {
            if (!Used)
            {
                return "unused";
            }
            return $"{Count}";
        }
    }
}