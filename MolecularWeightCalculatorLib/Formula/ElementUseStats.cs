using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public class ElementUseStats : IElementUseStats
    {
        // Ignore Spelling: Memberwise

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
        public IReadOnlyList<IsotopicAtomInfo> Isotopes => isotopes;

        public IReadOnlyList<IIsotopicAtomInfo> IsotopesUsed => isotopes;

        private readonly List<IsotopicAtomInfo> isotopes;

        public ElementUseStats()
        {
            Used = false;
            Count = 0;
            IsotopicCorrection = 0;
            isotopes = new List<IsotopicAtomInfo>(0);
        }

        public ElementUseStats Clone()
        {
            // Start with a shallow copy for all value members
            //var cloned = (ElementUseStats) MemberwiseClone();// Nice, but also copies the read-only "isotopes" object...
            var cloned = new ElementUseStats()
            {
                Used = Used,
                Count = Count,
                IsotopicCorrection = IsotopicCorrection,
            };

            // Finish with a deep copy for all reference members
            cloned.isotopes.Capacity = Isotopes.Count;
            foreach (var isotope in Isotopes)
            {
                cloned.isotopes.Add(isotope.Clone());
            }

            return cloned;
        }

        /// <summary>
        /// Add an isotopic mass and occurrence count; will merge counts when the masses are close enough
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="count"></param>
        public void AddIsotope(double mass, double count)
        {
            var found = false;
            foreach (var isotope in isotopes)
            {
                // Comparison: double.Epsilon is really, really small;
                // this number is smaller than the least-significant digit of any stored mass or isotope,
                // and probably still smaller than any precision anyone who is using this program cares about.
                if (Math.Abs(isotope.Mass - mass) < 0.000000000001)
                {
                    found = true;
                    isotope.Count += count;
                    break;
                }
            }

            if (!found)
            {
                isotopes.Add(new IsotopicAtomInfo { Mass = mass, Count = count });
            }
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

    [ComVisible(false)]
    public interface IElementUseStats
    {

        /// <summary>
        /// Number of atoms of this element; can have a non-integer count, e.g., C5.5
        /// </summary>
        public double Count { get; set; }

        public double IsotopicCorrection { get; set; }

        /// <summary>
        /// Specific isotopes of the atom
        /// </summary>
        public IReadOnlyList<IIsotopicAtomInfo> IsotopesUsed { get; }
    }
}