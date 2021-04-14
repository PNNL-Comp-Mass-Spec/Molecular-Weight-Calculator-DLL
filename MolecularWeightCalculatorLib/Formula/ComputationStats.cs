using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public class ComputationStats
    {
        /// <summary>
        /// Element statistics
        /// </summary>
        /// <remarks>
        /// 1-based array, ranging from 1 to ELEMENT_COUNT
        /// </remarks>
        public ElementUseStats[] Elements { get; private set; }

        public double TotalMass { get; set; }

        /// <summary>
        /// Percent composition info
        /// </summary>
        /// <remarks>
        /// 1-based array, ranging from 1 to ELEMENT_COUNT
        /// </remarks>
        public PercentCompositionInfo[] PercentCompositions { get; private set; }

        public float Charge { get; set; }

        public double StandardDeviation { get; set; }

        public ComputationStats()
        {
            const int ElementCount = ElementsAndAbbrevs.ELEMENT_COUNT + 1;

            Charge = 0;
            StandardDeviation = 0;
            TotalMass = 0;
            Elements = new ElementUseStats[ElementCount];
            PercentCompositions = new PercentCompositionInfo[ElementCount];

            for (var i = 0; i < ElementCount; i++)
            {
                Elements[i] = new ElementUseStats();
                PercentCompositions[i] = new PercentCompositionInfo();
            }
        }

        public ComputationStats Clone()
        {
            // Start with a shallow copy for all value members
            // NOTE: can't use this when trying to create deep copies of lists, when the object reference is read only
            var cloned = (ComputationStats)MemberwiseClone();

            // Finish with a deep copy for all reference members
            cloned.Elements = new ElementUseStats[Elements.Length];
            cloned.PercentCompositions = new PercentCompositionInfo[PercentCompositions.Length];

            // Assume Elements and PercentCompositions are always the same length
            for (var i = 0; i < Elements.Length; i++)
            {
                cloned.Elements[i] = Elements[i]?.Clone();
                cloned.PercentCompositions[i] = PercentCompositions[i]?.Clone();
            }

            return cloned;
        }

        public override string ToString()
        {
            return $"{TotalMass:F2}";
        }
    }
}