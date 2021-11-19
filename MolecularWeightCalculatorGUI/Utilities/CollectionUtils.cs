using System;
using System.Collections.Generic;
using System.Linq;

namespace MolecularWeightCalculatorGUI.Utilities
{
    internal static class CollectionUtils
    {
        public static IReadOnlyList<T> GetCollectionForEnum<T>(params T[] exclusions) where T : Enum
        {
            var items = Enum.GetValues(typeof(T)).Cast<T>();
            if (exclusions?.Length > 0)
            {
                items = items.Except(exclusions);
            }

            return new List<T>(items);
        }
    }
}
