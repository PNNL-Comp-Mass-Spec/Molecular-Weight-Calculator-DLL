using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.FormulaFinder
{
    [Guid("D35BAE50-BD22-4CD0-8BFC-A8358F71C802"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class SearchOptions : IFormulaFinderSearchOptions
    {
        private bool mFindCharge;
        private bool mLimitChargeRange;
        private bool mFindTargetMz;

        /// <summary>
        /// When true, compute the overall charge of each compound
        /// </summary>
        public bool FindCharge
        {
            get => mFindCharge;
            set
            {
                mFindCharge = value;

                if (mFindCharge)
                    return;

                // Auto-disable a few options
                mLimitChargeRange = false;
                mFindTargetMz = false;
            }
        }

        /// <summary>
        /// When true, filter the results by ChargeMin and ChargeMax
        /// </summary>
        /// <remarks>
        /// Setting this to True auto-sets FindCharge to true
        /// Setting this to False auto-sets FindTargetMZ to false</remarks>
        public bool LimitChargeRange
        {
            get => mLimitChargeRange;
            set
            {
                mLimitChargeRange = value;
                if (mLimitChargeRange)
                {
                    FindCharge = true;
                }
                else
                {
                    mFindTargetMz = false;
                }
            }
        }

        /// <summary>
        /// When LimitChargeRange is true, results will be limited to the range ChargeMin to ChargeMax
        /// </summary>
        /// <remarks>Negative values are allowed</remarks>
        public int ChargeMin { get; set; }

        /// <summary>
        /// When LimitChargeRange is true, results will be limited to the range ChargeMin to ChargeMax
        /// </summary>
        /// <remarks>Negative values are allowed</remarks>
        public int ChargeMax { get; set; }

        /// <summary>
        /// Set to true to search for a target m/z value instead of a target mass
        /// </summary>
        /// <remarks>Setting this to True auto-sets FindCharge and LimitChargeRange to True</remarks>
        public bool FindTargetMz
        {
            get => mFindTargetMz;
            set
            {
                mFindTargetMz = value;
                if (mFindTargetMz)
                {
                    FindCharge = true;
                    LimitChargeRange = true;
                }
            }
        }

        public FormulaSearchModes SearchMode { get; set; }

        public bool VerifyHydrogens { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchOptions()
        {
            FindCharge = true;
            LimitChargeRange = false;
            ChargeMin = -4;
            ChargeMax = 4;
            FindTargetMz = false;
            SearchMode = FormulaSearchModes.Thorough;
            VerifyHydrogens = true;
        }
    }
}