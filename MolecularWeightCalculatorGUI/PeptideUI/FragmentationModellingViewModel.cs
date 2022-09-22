using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using DynamicData.Binding;
using MolecularWeightCalculator;
using MolecularWeightCalculator.EventLogging;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.Sequence;
using MolecularWeightCalculatorGUI.Utilities;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    internal class FragmentationModellingViewModel : ReactiveObject
    {
        [Obsolete("For WPF design-time use only", true)]
        public FragmentationModellingViewModel() : this(new MolecularWeightTool(ElementMassMode.Isotopic))
        {
        }

        public FragmentationModellingViewModel(MolecularWeightTool molWeight)
        {
            mwt = molWeight;
            peptide = mwt.Peptide;

            NotationOptionsSource = CollectionUtils.GetCollectionForEnum<AminoAcidNotationMode>();
            MassChargeLevelOptions = CollectionUtils.GetCollectionForEnum<MassChargeLevel>(MassChargeLevel.M);
            NTerminusGroupOptions = CollectionUtils.GetCollectionForEnum<NTerminusGroupType>();
            CTerminusGroupOptions = CollectionUtils.GetCollectionForEnum<CTerminusGroupType>();
            IonTypesList = CollectionUtils.GetCollectionForEnum<IonType>();
            ChargeThresholdOptions = Enumerable.Range(0, 16).Select(x => x * 100).ToList();
            IonMassDigitsOptions = Enumerable.Range(0, 7).ToList();

            NeutralLossIonTypes = new ObservableCollectionExtended<IonType>(new IonType[] { IonType.BIon, IonType.YIon });

            this.WhenAnyValue(x => x.ShowAIons, x => x.ShowBIons, x => x.ShowCIons, x => x.ShowYIons, x => x.ShowZIons)
                .SubscribeOnChange(x => UpdateMassesGridAndSpectrum());
            this.WhenAnyValue(x => x.NeutralLossIonTypes.Count, x => x.NeutralLossWater, x => x.NeutralLossAmmonia,
                x => x.NeutralLossPhosphate)
                .SubscribeOnChange(x => UpdateMassesGridAndSpectrum());
            this.WhenAnyValue(x => x.Show2PlusCharges, x => x.TwoPlusChargesThreshold, x => x.Show3PlusCharges,
                x => x.ThreePlusChargesThreshold)
                .SubscribeOnChange(x => UpdateMassesGridAndSpectrum());
            this.WhenAnyValue(x => x.Sequence).SubscribeOnChange(x =>
            {
                if (mDelayUpdate)
                    return;

                CheckSequenceTerminii();
                UpdatePredictedFragMasses();
            });
            this.WhenAnyValue(x => x.NTerminusGroup, x => x.CTerminusGroup, x => x.SelectedAminoAcidNotation)
                .SubscribeOnChange(x => UpdatePredictedFragMasses());
            this.WhenAnyValue(x => x.IonMassDigits).SubscribeOnChange(x => UpdateGridNumberFormat(FragmentationDataTable));
            this.WhenAnyValue(x => x.MassProtonated, x => x.MassChargeLevel).SubscribeOnChange(x => ConvertSequenceMH(true));
            this.WhenAnyValue(x => x.MassAtChargeLevel).SubscribeOnChange(x => ConvertSequenceMH(false));
            this.WhenAnyValue(x => x.ElementModeAverage, x => x.ElementModeIsotopic)
                .SubscribeOnChange(x => UpdateElementMode());

            CopyMolecularWeightCommand = ReactiveCommand.Create(CopySequenceMW);
            ShowEditResidueModificationSymbolsCommand = ReactiveCommand.Create<Window>(x =>
            {
                var vm = new AminoAcidModificationsViewModel(mwt);
                var window = new AminoAcidModificationsWindow() { DataContext = vm, Owner = x };
                window.ShowDialog();

                UpdatePredictedFragMasses();
            });

            var currentTask = "";

            try
            {
                currentTask = "Loading FragmentationModellingViewModel";

                UpdateStandardMasses();

                UpdateMassesGridAndSpectrum();
            }
            catch (Exception ex)
            {
                Logging.GeneralErrorHandler("FragmentationModellingViewModel|Constructor", new Exception("Error " + currentTask + ": " + ex.Message, ex));
            }
        }

        internal void ActivateWindow()
        {
            switch (mwt.GetElementMode())
            {
                case ElementMassMode.Average:
                    ElementModeIsotopic = false;
                    ElementModeAverage = true;
                    break;
                case ElementMassMode.Isotopic:
                    ElementModeAverage = false;
                    ElementModeIsotopic = true;
                    break;
                default:
                    mwt.SetElementMode(ElementMassMode.Isotopic);
                    ElementModeAverage = false;
                    ElementModeIsotopic = true;
                    break;
            }
        }

        private readonly MolecularWeightTool mwt;
        private ElementAndMassTools ElementAndMass => mwt.ElementAndMass;
        private readonly Peptide peptide;

        /* First: is not allowed mod symbol
         * See ElementAndMassTools.IsModSymbol()
         * Then:
         * Numbers, parentheses not allowed. TODO: Possibly allow them for side-chains
         * Spaces, dash/negative sign, decimal point (. or ,), lower and uppercase letters allowed.
         * NOTE: Beyond all of this, the parsing code ignores invalid characters.
         */
        private readonly Regex blockedCharacters = new Regex(@"[""()/:;<>=[\]{}\\|0-9]", RegexOptions.Compiled);

        //private string sequence = "Arg-His-Pro-Glu-Tyr-Ala-Val";
        private string sequence = "GlyLeuTyrProGluProThrIleAspGluMetAlaThrThrHisGluTrp";
        private AminoAcidNotationMode selectedAminoAcidNotation = AminoAcidNotationMode.ThreeLetterNotation;
        private double peptideMass = 870.43478;
        private MassChargeLevel massChargeLevel = MassChargeLevel.MPlus2H;
        private double massProtonated = 871.442056;
        private double massAtChargeLevel = 436.224666;
        private bool elementModeAverage;
        private bool elementModeIsotopic = true;
        private DataTable fragmentationDataTable = new DataTable();
        private NTerminusGroupType nTerminusGroup = NTerminusGroupType.Hydrogen;
        private CTerminusGroupType cTerminusGroup = CTerminusGroupType.Hydroxyl;
        private bool showAIons;
        private bool showBIons = true;
        private bool showCIons;
        private bool showYIons = true;
        private bool showZIons;
        private bool neutralLossWater = true;
        private bool neutralLossAmmonia;
        private bool neutralLossPhosphate;
        private bool show2PlusCharges;
        private int twoPlusChargesThreshold = 800;
        private bool show3PlusCharges;
        private int threePlusChargesThreshold = 900;
        private int ionMassDigits = 2;
        private const string SHOULDER_ION_PREFIX = "Shoulder-";

        private readonly struct FragDetailGridLocation
        {
            public FragmentationSpectrumData Details { get; }
            public int Row { get; }
            public int Col { get; }

            public FragDetailGridLocation(FragmentationSpectrumData details, int row, int col)
            {
                Details = details;
                Row = row;
                Col = col;
            }
        }

        // fragSpectrumDetails[] contains detailed information on the fragmentation spectrum data, sorted by mass
        // along with the row and column indices in FragmentationDataTable that the values in fragSpectrumDetails[] are displayed at
        private readonly List<FragDetailGridLocation> fragSpectrumDetails = new List<FragDetailGridLocation>();

        private bool mDelayUpdate;

        public IReadOnlyList<AminoAcidNotationMode> NotationOptionsSource { get; }
        public IReadOnlyList<MassChargeLevel> MassChargeLevelOptions { get; }
        public IReadOnlyList<NTerminusGroupType> NTerminusGroupOptions { get; }
        public IReadOnlyList<CTerminusGroupType> CTerminusGroupOptions { get; }
        public IReadOnlyList<IonType> IonTypesList { get; }
        public IReadOnlyList<int> ChargeThresholdOptions { get; }
        public IReadOnlyList<int> IonMassDigitsOptions { get; }

        public ReactiveCommand<RxUnit, RxUnit> CopyMolecularWeightCommand { get; }
        public ReactiveCommand<Window, RxUnit> ShowEditResidueModificationSymbolsCommand { get; }

        public string Sequence
        {
            get => sequence;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Validate the input, and drop invalid characters.
                    var x = blockedCharacters.Replace(value, "");
                    if (!value.Equals(x))
                    {
                        value = x;
                        if (value.Equals(sequence))
                        {
                            // Force UI refresh to remove invalid characters
                            this.RaisePropertyChanged();
                        }
                    }
                }

                this.RaiseAndSetIfChanged(ref sequence, value);
            }
        }

        public AminoAcidNotationMode SelectedAminoAcidNotation
        {
            get => selectedAminoAcidNotation;
            set => this.RaiseAndSetIfChanged(ref selectedAminoAcidNotation, value);
        }

        public double PeptideMass
        {
            get => peptideMass;
            set => this.RaiseAndSetIfChanged(ref peptideMass, value);
        }

        public MassChargeLevel MassChargeLevel
        {
            get => massChargeLevel;
            set => this.RaiseAndSetIfChanged(ref massChargeLevel, value);
        }

        public double MassProtonated
        {
            get => massProtonated;
            set => this.RaiseAndSetIfChanged(ref massProtonated, value);
        }

        public double MassAtChargeLevel
        {
            get => massAtChargeLevel;
            set => this.RaiseAndSetIfChanged(ref massAtChargeLevel, value);
        }

        public bool ElementModeAverage
        {
            get => elementModeAverage;
            set => this.RaiseAndSetIfChanged(ref elementModeAverage, value);
        }

        public bool ElementModeIsotopic
        {
            get => elementModeIsotopic;
            set => this.RaiseAndSetIfChanged(ref elementModeIsotopic, value);
        }

        public NTerminusGroupType NTerminusGroup
        {
            get => nTerminusGroup;
            set => this.RaiseAndSetIfChanged(ref nTerminusGroup, value);
        }

        public CTerminusGroupType CTerminusGroup
        {
            get => cTerminusGroup;
            set => this.RaiseAndSetIfChanged(ref cTerminusGroup, value);
        }

        public bool ShowAIons
        {
            get => showAIons;
            set => this.RaiseAndSetIfChanged(ref showAIons, value);
        }

        public bool ShowBIons
        {
            get => showBIons;
            set => this.RaiseAndSetIfChanged(ref showBIons, value);
        }

        public bool ShowCIons
        {
            get => showCIons;
            set => this.RaiseAndSetIfChanged(ref showCIons, value);
        }

        public bool ShowYIons
        {
            get => showYIons;
            set => this.RaiseAndSetIfChanged(ref showYIons, value);
        }

        public bool ShowZIons
        {
            get => showZIons;
            set => this.RaiseAndSetIfChanged(ref showZIons, value);
        }

        public ObservableCollectionExtended<IonType> NeutralLossIonTypes { get; }

        public bool NeutralLossWater
        {
            get => neutralLossWater;
            set => this.RaiseAndSetIfChanged(ref neutralLossWater, value);
        }

        public bool NeutralLossAmmonia
        {
            get => neutralLossAmmonia;
            set => this.RaiseAndSetIfChanged(ref neutralLossAmmonia, value);
        }

        public bool NeutralLossPhosphate
        {
            get => neutralLossPhosphate;
            set => this.RaiseAndSetIfChanged(ref neutralLossPhosphate, value);
        }

        public bool Show2PlusCharges
        {
            get => show2PlusCharges;
            set => this.RaiseAndSetIfChanged(ref show2PlusCharges, value);
        }

        public int TwoPlusChargesThreshold
        {
            get => twoPlusChargesThreshold;
            set => this.RaiseAndSetIfChanged(ref twoPlusChargesThreshold, value);
        }

        public bool Show3PlusCharges
        {
            get => show3PlusCharges;
            set => this.RaiseAndSetIfChanged(ref show3PlusCharges, value);
        }

        public int ThreePlusChargesThreshold
        {
            get => threePlusChargesThreshold;
            set => this.RaiseAndSetIfChanged(ref threePlusChargesThreshold, value);
        }

        public int IonMassDigits
        {
            get => ionMassDigits;
            set => this.RaiseAndSetIfChanged(ref ionMassDigits, value);
        }

        public DataTable FragmentationDataTable
        {
            get => fragmentationDataTable;
            private set => this.RaiseAndSetIfChanged(ref fragmentationDataTable, value);
        }

        private void CheckSequenceTerminii()
        {
            // If 3 letter codes are enabled, then checks to see if the sequence begins with H and ends with OH
            // If so, makes sure the first three letters are not an amino acid
            // If they're not, removes the H and OH and sets cboNTerminus and cboCTerminus accordingly

            mDelayUpdate = true;

            if (Sequence.Length > 3 && SelectedAminoAcidNotation == AminoAcidNotationMode.ThreeLetterNotation &&
                Sequence.StartsWith("H", StringComparison.OrdinalIgnoreCase) &&
                Sequence.EndsWith("OH", StringComparison.OrdinalIgnoreCase))
            {
                int abbrevId;
                if (char.IsLetter(Sequence[1]) && char.IsLetter(Sequence[2]))
                {
                    abbrevId = mwt.GetAbbreviationId(Sequence.Substring(0, 3));
                    if (abbrevId > 0)
                    {
                        // Matched an abbreviation; is it an amino acid?
                        mwt.GetAbbreviation(abbrevId, out _, out _, out _, out var isAminoAcid);
                        if (!isAminoAcid)
                        {
                            // Matched an abbreviation, but it's not an amino acid
                            abbrevId = 0;
                        }
                    }
                }
                else
                {
                    abbrevId = 0;
                }

                if (abbrevId == 0)
                {
                    // The first three characters do not represent a 3 letter amino acid code
                    // Remove the H and OH
                    Sequence = Sequence.Substring(1, Sequence.Length - 3).Trim('-');

                    NTerminusGroup = NTerminusGroupType.Hydrogen;
                    CTerminusGroup = CTerminusGroupType.Hydroxyl;
                }
            }

            mDelayUpdate = false;
        }

        private bool calculatingMass = false;
        private void ConvertSequenceMH(bool favorMH)
        {
            // When favorMH = true then computes MHAlt using MH
            // Otherwise, computes MH using MHAlt

            if (calculatingMass)
                return;
            calculatingMass = true;

            var charge = (int)MassChargeLevel;
            if (charge < 1)
                charge = 1;

            if (favorMH)
            {
                MassAtChargeLevel = 0d;
                if (MassProtonated > 0d)
                {
                    MassAtChargeLevel = mwt.ConvoluteMass(MassProtonated, 1, charge);
                }
            }
            else
            {
                MassProtonated = 0d;
                if (MassAtChargeLevel > 0d)
                {
                    MassProtonated = mwt.ConvoluteMass(MassAtChargeLevel, charge, 1);
                }
            }

            calculatingMass = false;
        }

        private void CopySequenceMW()
        {
            Clipboard.SetText(peptide.GetPeptideMass().ToString(), TextDataFormat.Text);
        }

        // TODO: Probably calling this many times when it doesn't need to be.
        private void DisplayPredictedIonMasses()
        {
            // Call mwt to get the predicted fragmentation spectrum masses and intensities
            // Use this data to populate FragmentationDataTable

            try
            {
                UpdateFragmentationSpectrumOptions();

                var numberFormat = $"F{IonMassDigits}";

                // The GetFragmentationMasses() function computes the masses, intensities, and symbols for the given sequence
                var fragSpectrum = peptide.GetFragmentationMasses();

                // Initialize (and clear) FragmentationDataTable
                // Examine fragSpectrum[].Symbol to make a list of the possible ion types present
                // Exclude shoulder ions and duplicates, and sort alphabetically (a, b, y, c, or z)
                var columnHeadersToAdd = fragSpectrum.Where(x => !x.IsShoulderIon).Select(x => x.SymbolGeneric).Distinct().OrderBy(x => x).ToList();

                // There are at a minimum 4 columns (#, Immon., Seq., and #C)
                // NOTE: .NET DataTable does not allow duplicate column names; using '#C' since it is from the C-Terminus anyway
                // NOTE: C#/WPF can have issues with spaces and periods in column names; using the ".Caption" value with code-behind to resolve this.
                // We'll start with # and Immon.
                var table = new DataTable();
                var seqColumn = new DataColumn("Seq", typeof(string)) { Caption = "Seq." };
                var nTermResidueNumberColumn = new DataColumn("#", typeof(int));
                var immoniumColumn = new DataColumn("Immon", typeof(FragmentationGridIon)) { Caption = "Immon." };
                var cTermResidueNumberColumn = new DataColumn("#C", typeof(int));

                table.Columns.Add(nTermResidueNumberColumn);
                table.Columns.Add(immoniumColumn);

                var yIonSymbol = peptide.LookupIonTypeString(IonType.YIon);
                var zIonSymbol = peptide.LookupIonTypeString(IonType.ZIon);

                var seqColumnAdded = false;

                // Append the items in columnHeadersToAdd[] to columnHeaders
                foreach (var header in columnHeadersToAdd)
                {
                    // Check if this column is the first y-ion or z-ion column
                    if (!seqColumnAdded && (header[0] == yIonSymbol[0] || header[0] == zIonSymbol[0]))
                    {
                        table.Columns.Add(seqColumn);
                        seqColumnAdded = true;
                    }

                    table.Columns.Add(new DataColumn(header, typeof(FragmentationGridIon)));
                }

                // If the sequence column still wasn't added, then add it now
                if (!seqColumnAdded)
                {
                    table.Columns.Add(seqColumn);
                }

                // The final column is a duplicate of the zeroth column
                table.Columns.Add(cTermResidueNumberColumn);

                // Now populate FragmentationDataTable with the data
                var residueCount = peptide.GetResidueCount();
                var use3LetterSymbol = SelectedAminoAcidNotation == AminoAcidNotationMode.ThreeLetterNotation;
                var blankRow = new object[FragmentationDataTable.Columns.Count];
                for (var residueIndex = 0; residueIndex < residueCount; residueIndex++)
                {
                    var success = peptide.GetResidue(residueIndex, out var symbol, out var residueMass, out var isModified, out _);

                    var modSymbolForThisResidue = "";
                    if (isModified)
                    {
                        peptide.GetResidueModificationIDs(residueIndex, out var modIDs);
                        foreach (var modId in modIDs)
                        {
                            peptide.GetModificationSymbol(modId, out var modSymbol, out _, out _, out _);
                            modSymbolForThisResidue += modSymbol;
                        }
                    }

                    if (!success)
                    {
                        throw new Exception("Unable to retrieve peptide residue, which is an error, and not simply corrected.");
                    }

                    // Add a new row to FragmentationDataTable
                    table.Rows.Add(blankRow); // Add the blank row first, then get it back out as a DataRow (now has connections to the columns)
                    var rowData = table.Rows[residueIndex];

                    // Add residue number, reverse residue number, and immonium ion to their respective columns
                    rowData[nTermResidueNumberColumn] = residueIndex;
                    rowData[cTermResidueNumberColumn] = residueCount - residueIndex + 1;
                    rowData[immoniumColumn] = new FragmentationGridIon(peptide.ComputeImmoniumMass(residueMass), numberFormat);

                    // Add the residue symbol
                    if (use3LetterSymbol)
                    {
                        rowData[seqColumn] = symbol + modSymbolForThisResidue;
                    }
                    else
                    {
                        var symbol1Letter = mwt.GetAminoAcidSymbolConversion(symbol, false);
                        if (string.IsNullOrWhiteSpace(symbol1Letter)) symbol1Letter = symbol;
                        if (symbol1Letter == "Xxx") symbol1Letter = "X";
                        rowData[seqColumn] = symbol1Letter + modSymbolForThisResidue;
                    }
                }

                // Initialize fragSpectrumDetails[]
                fragSpectrumDetails.Clear();
                fragSpectrumDetails.Capacity = fragSpectrum.Count;

                // Finally, step through fragSpectrum and populate FragmentationDataTable with the ion masses
                // Shoulder ion masses are not displayed, but indices are stored for all others is updated with the associated primary ion
                foreach (var ionDetails in fragSpectrum)
                {
                    var symbolGeneric = ionDetails.SymbolGeneric;
                    if (ionDetails.IsShoulderIon)
                    {
                        symbolGeneric = symbolGeneric.Replace(SHOULDER_ION_PREFIX, "");
                    }

                    var row = table.Rows[ionDetails.SourceResidueNumber];
                    var columnIndex = table.Columns[symbolGeneric].Ordinal;

                    if (!ionDetails.IsShoulderIon)
                    {
                        // Only display if not a Shoulder Ion
                        row[columnIndex] = new FragmentationGridIon(ionDetails.Mass, numberFormat);
                    }

                    // Store ionDetails with the Row Index and Column Index
                    fragSpectrumDetails.Add(new FragDetailGridLocation(ionDetails, ionDetails.SourceResidueNumber, columnIndex));
                }

                FragmentationDataTable = table;
            }
            catch (Exception ex)
            {
                Logging.GeneralErrorHandler("FragmentationModellingViewModel|DisplayPredictedIonMasses", ex);
            }
        }

        private void UpdateGridNumberFormat(DataTable grid)
        {
            var numberFormat = $"F{IonMassDigits}";
            foreach (DataColumn column in grid.Columns)
            {
                if (column.DataType == typeof(FragmentationGridIon))
                {
                    foreach (DataRow row in grid.Rows)
                    {
                        (row[column] as FragmentationGridIon)?.SetFormat(numberFormat);
                    }
                }
            }
        }

        public void PasteNewSequence(string newSequence, bool is3LetterCode)
        {
            SelectedAminoAcidNotation = is3LetterCode ? AminoAcidNotationMode.ThreeLetterNotation : AminoAcidNotationMode.OneLetterNotation;

            // Validate newSequence
            peptide.SetSequence(newSequence, default, default, is3LetterCode);

            Sequence = peptide.GetSequence(is3LetterCode);
        }

        private void UpdateElementMode()
        {
            var newWeightMode = ElementMassMode.Isotopic;

            if (ElementModeAverage)
            {
                newWeightMode = ElementMassMode.Average;
            }

            if (newWeightMode != mwt.GetElementMode())
            {
                mwt.SetElementMode(newWeightMode);
                // TODO: Also re-calculate all formulas

                UpdateStandardMasses();
            }
        }

        private void UpdateFragmentationSpectrumOptions()
        {
            if (peptide is null)
                return;

            try
            {
                // TODO: Don't need to re-assign the value...
                // Initialize to the current values
                var options = peptide.GetFragmentationSpectrumOptions();

                options.DoubleChargeIonsShow = Show2PlusCharges;
                options.TripleChargeIonsShow = Show3PlusCharges;

                options.DoubleChargeIonsThreshold = TwoPlusChargesThreshold;
                options.TripleChargeIonsThreshold = ThreePlusChargesThreshold;

                options.IonTypeOptions[(int)IonType.AIon].ShowIon = ShowAIons;
                options.IonTypeOptions[(int)IonType.BIon].ShowIon = ShowBIons;
                options.IonTypeOptions[(int)IonType.CIon].ShowIon = ShowCIons;
                options.IonTypeOptions[(int)IonType.YIon].ShowIon = ShowYIons;
                options.IonTypeOptions[(int)IonType.ZIon].ShowIon = ShowZIons;

                foreach (var ionType in Enum.GetValues(typeof(IonType)).Cast<IonType>())
                {
                    var modifyIon = NeutralLossIonTypes.Contains(ionType);

                    options.IonTypeOptions[(int)ionType].NeutralLossWater = modifyIon && NeutralLossWater;
                    options.IonTypeOptions[(int)ionType].NeutralLossAmmonia = modifyIon && NeutralLossAmmonia;
                    options.IonTypeOptions[(int)ionType].NeutralLossPhosphate = modifyIon && NeutralLossPhosphate;
                }

                // Note: 'A' ions can have ammonia and phosphate loss, but not water loss, so always set this to false
                options.IonTypeOptions[(int)IonType.AIon].NeutralLossWater = false;
            }
            catch (Exception ex)
            {
                Logging.GeneralErrorHandler("FragmentationModellingViewModel|UpdateFragmentationSpectrumOptions", ex);
            }
        }

        private void UpdateMassesGridAndSpectrum()
        {
            // Display predicted ions & intensities in grid
            DisplayPredictedIonMasses();
        }

        private void UpdatePredictedFragMasses()
        {
            // Determines the masses of the expected ions for the given sequence

            if (mDelayUpdate)
                return;

            peptide.SetSequence(Sequence, NTerminusGroup, CTerminusGroup, SelectedAminoAcidNotation == AminoAcidNotationMode.ThreeLetterNotation);

            if (peptide.GetResidueCount() > 0)
            {
                PeptideMass = peptide.GetPeptideMass();
                if (PeptideMass == 0d)
                {
                    MassProtonated = 0;
                }
                else if (NTerminusGroup == NTerminusGroupType.HydrogenPlusProton)
                {
                    // Don't need to add a proton
                    MassProtonated = PeptideMass;
                }
                else
                {
                    MassProtonated = PeptideMass + mwt.GetChargeCarrierMass();
                }
            }
            else
            {
                PeptideMass = 0d;
                MassProtonated = 0;
            }

            UpdateMassesGridAndSpectrum();
        }

        private void UpdateStandardMasses()
        {
            UpdatePredictedFragMasses();
        }
    }
}
