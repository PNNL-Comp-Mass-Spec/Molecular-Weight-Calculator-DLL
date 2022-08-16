using System.Runtime.InteropServices;
using MolecularWeightCalculator.FormulaFinder;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("B3B405D3-0E3D-4B3C-9D01-54FD01326A37"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IFormulaFinderSearchOptions
    {
        /// <summary>
        /// When true, compute the overall charge of each compound
        /// </summary>
        bool FindCharge { get; set; }

        /// <summary>
        /// When true, filter the results by ChargeMin and ChargeMax
        /// </summary>
        /// <remarks>
        /// Setting this to True auto-sets FindCharge to true
        /// Setting this to False auto-sets FindTargetMZ to false</remarks>
        bool LimitChargeRange { get; set; }

        /// <summary>
        /// When LimitChargeRange is true, results will be limited to the range ChargeMin to ChargeMax
        /// </summary>
        /// <remarks>Negative values are allowed</remarks>
        int ChargeMin { get; set; }

        /// <summary>
        /// When LimitChargeRange is true, results will be limited to the range ChargeMin to ChargeMax
        /// </summary>
        /// <remarks>Negative values are allowed</remarks>
        int ChargeMax { get; set; }

        /// <summary>
        /// Set to true to search for a target m/z value instead of a target mass
        /// </summary>
        /// <remarks>Setting this to True auto-sets FindCharge and LimitChargeRange to True</remarks>
        bool FindTargetMz { get; set; }

        FormulaSearchModes SearchMode { get; set; }
        bool VerifyHydrogens { get; set; }
    }
}
