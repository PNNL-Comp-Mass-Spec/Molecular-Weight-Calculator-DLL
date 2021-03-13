namespace MolecularWeightCalculator
{
    public class FormulaFinderOptions
    {
        #region "Constants and Enums"
        public enum eSearchMode
        {
            Thorough = 0,
            Bounded = 1
        }
        #endregion

        #region "Member Variables"
        private bool mFindCharge;
        private bool mLimitChargeRange;
        private bool mFindTargetMZ;
        #endregion

        #region "Properties"

        /// <summary>
        /// When true, compute the overall charge of each compound
        /// </summary>
        /// <remarks></remarks>
        public bool FindCharge
        {
            get
            {
                return mFindCharge;
            }
            set
            {
                mFindCharge = value;

                if (mFindCharge == false)
                {
                    // Auto-disable a few options
                    mLimitChargeRange = false;
                    mFindTargetMZ = false;
                }
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
            get
            {
                return mLimitChargeRange;
            }
            set
            {
                mLimitChargeRange = value;
                if (mLimitChargeRange)
                {
                    FindCharge = true;
                }
                else
                {
                    mFindTargetMZ = false;
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
        public bool FindTargetMZ
        {
            get
            {
                return mFindTargetMZ;
            }
            set
            {
                mFindTargetMZ = value;
                if (mFindTargetMZ)
                {
                    FindCharge = true;
                    LimitChargeRange = true;
                }
            }
        }

        public eSearchMode SearchMode { get; set; }

        public bool VerifyHydrogens { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks></remarks>
        public FormulaFinderOptions()
        {
            FindCharge = true;
            LimitChargeRange = false;
            ChargeMin = -4;
            ChargeMax = 4;
            FindTargetMZ = false;
            SearchMode = eSearchMode.Thorough;
            VerifyHydrogens = true;
        }
    }
}