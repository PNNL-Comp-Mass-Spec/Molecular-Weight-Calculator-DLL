using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using DynamicData.Binding;
using MolecularWeightCalculator;
using MolecularWeightCalculator.Sequence;
using MolecularWeightCalculatorGUI.Utilities;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    internal class AminoAcidModificationsViewModel : ReactiveObject
    {
        [Obsolete("For WPF design-time use only", true)]
        public AminoAcidModificationsViewModel() : this(new MolecularWeightTool())
        { }

        public AminoAcidModificationsViewModel(MolecularWeightTool molWeight)
        {
            mwt = molWeight;
            this.WhenAnyValue(x => x.PhosphorylationSymbol).SubscribeOnChange(_ => ValidateNewPhosphorylationSymbol());

            OkCommand = ReactiveCommand.Create<Window>(x =>
            {
                mValueChanged = false;
                x.Close();
            });
            CancelCommand = ReactiveCommand.Create<Window>(CancelChanges);
            ResetToDefaultsCommand = ReactiveCommand.Create(ResetModificationSymbolsToDefaults);
            AddStandardModificationCommand = ReactiveCommand.Create<ModificationSymbolImmutable>(AddSelectedModificationToList);
            GridClickedCommand = ReactiveCommand.Create<Tuple<Window, ModificationSymbolEdit>>(x => HandleGridClick(x.Item1, x.Item2));

            savedModSymbols = GetCurrentModifications();
            StandardModifications = GetDefaultModifications();

            DisplayCurrentModificationSymbols();

            mValueChanged = false;
        }

        private readonly IReadOnlyList<ModificationSymbolImmutable> savedModSymbols;      // 0-based array

        private bool mValueChanged;
        private bool mDelayUpdate;
        private readonly Regex characterWhitelist = new Regex(@"[^`~!@#$%^&*_+?']", RegexOptions.Compiled);
        private MolecularWeightTool mwt;
        private string phosphorylationSymbol;
        private double phosphorylationMass = 79.9663326d;

        public string PhosphorylationSymbol
        {
            get => phosphorylationSymbol;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var x = characterWhitelist.Replace(value, "");
                    if (!value.Equals(x))
                    {
                        value = x;
                        if (value.Equals(phosphorylationSymbol))
                        {
                            this.RaisePropertyChanged();
                        }
                    }
                }

                this.RaiseAndSetIfChanged(ref phosphorylationSymbol, value);
            }
        }

        public double PhosphorylationMass
        {
            get => phosphorylationMass;
            private set => this.RaiseAndSetIfChanged(ref phosphorylationMass, value);
        }

        public IReadOnlyList<ModificationSymbolImmutable> StandardModifications { get; }

        public ObservableCollectionExtended<ModificationSymbolEdit> ModSymbols { get; } = new ObservableCollectionExtended<ModificationSymbolEdit>();

        public ReactiveCommand<Window, RxUnit> OkCommand { get; }
        public ReactiveCommand<Window, RxUnit> CancelCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> ResetToDefaultsCommand { get; }
        public ReactiveCommand<ModificationSymbolImmutable, RxUnit> AddStandardModificationCommand { get; }
        public ReactiveCommand<Tuple<Window, ModificationSymbolEdit>, RxUnit> GridClickedCommand { get; }

        private void AddSelectedModificationToList(ModificationSymbolImmutable mod)
        {
            if (string.IsNullOrWhiteSpace(mod.Symbol))
            {
                // No item selected?
                return;
            }

            // The SetModificationSymbol() will update the symbol if it exists,
            // or add a new one if it doesn't exist
            var success = mwt.Peptide.SetModificationSymbol(mod.Symbol, mod.ModificationMass, mod.IndicatesPhosphorylation, mod.Comment);
            // TODO: Handle differently: Debug.Assert(success);

            DisplayCurrentModificationSymbols();
        }

        private void DisplayCurrentModificationSymbols()
        {
            var phosphorylationSymbolFound = default(bool);

            if (mDelayUpdate)
                return;
            mDelayUpdate = true;

            PhosphorylationSymbol = "";

            var newMods = new List<ModificationSymbolEdit>();

            // Grab the Modification Symbols from objMwtWin and fill the grid
            // Place the first phosphorylation symbol found in PhosphorylationSymbol
            for (var index = 0; index < mwt.Peptide.GetModificationSymbolCount(); index++)
            {
                var success = mwt.Peptide.GetModificationSymbol(index, out var symbol, out var mass, out var phosphorylation, out var comment);
                // TODO: Handle differently: Debug.Assert(success);

                if (phosphorylation)
                {
                    if (!phosphorylationSymbolFound)
                    {
                        PhosphorylationSymbol = symbol;
                        phosphorylationSymbolFound = true;
                    }
                }
                else
                {
                    newMods.Add(new ModificationSymbolEdit(index, symbol, mass, comment));
                }
            }

            newMods.Add(new ModificationSymbolEdit(-1, "", 0.0, ""));
            ModSymbols.Load(newMods);

            mDelayUpdate = false;
        }

        private IReadOnlyList<ModificationSymbolImmutable> GetCurrentModifications()
        {
            var savedMods = mwt.Peptide.GetModificationSymbolCount();
            var saved = new List<ModificationSymbolImmutable>(savedMods);
            for (var index = 0; index < savedMods; index++)
            {
                // Note: SavedModSymbols[] is 0-based, and the modifications in objMwtWin.Peptide are 0-based
                var success = mwt.Peptide.GetModificationSymbol(index, out var symbol, out var mass, out var indicatesPhosphorylation, out var comment);
                // TODO: Handle differently: Debug.Assert(success);
                if (success)
                {
                    saved.Add(new ModificationSymbolImmutable(symbol, mass, indicatesPhosphorylation, comment));
                }
            }

            return saved;
        }

        private IReadOnlyList<ModificationSymbolImmutable> GetDefaultModifications()
        {
            // Initialize DefaultModSymbols[]
            var peptide = new Peptide(mwt.ElementAndMass);
            var modCount = peptide.GetModificationSymbolCount();
            var mods = new List<ModificationSymbolImmutable>(modCount);

            for (var index = 0; index < modCount; index++)
            {
                var success = peptide.GetModificationSymbol(index, out var modSymbol, out var modMass, out var indicatesPhosphorylation, out var comment);

                if (!indicatesPhosphorylation)
                {
                    mods.Add(new ModificationSymbolImmutable(modSymbol, modMass, indicatesPhosphorylation, comment));
                }
                else
                {
                    PhosphorylationMass = modMass;
                }
            }

            return mods;
        }

        private void HandleGridClick(Window parent, ModificationSymbolEdit mod)
        {
            // Determine which mod symbol the user clicked on
            var modSymbolId = mod.Id;
            var symbol = mod.Symbol;
            var mass = mod.ModificationMass;
            var comment = mod.Comment;

            // Display the dialog box and get user's response.
            var editDetails = new EditModSymbolDetailsViewModel(symbol, mass, comment);
            var window = new EditModSymbolDetailsWindow() { DataContext = editDetails, Owner = parent };
            window.ShowDialog();

            if (editDetails.Result != EditWindowResult.Cancel)
            {
                if (editDetails.Result == EditWindowResult.Remove)
                {
                    // Remove indicates to remove the mod symbol
                    if (modSymbolId >= 0)
                    {
                        var result = mwt.Peptide.RemoveModificationById(modSymbolId);
                        // TODO: Handle differently: Debug.Assert(result == 0);
                    }
                }
                else
                {
                    var newSymbol = editDetails.Symbol;
                    var newMass = editDetails.Mass;
                    var newComment = editDetails.Comment;

                    if (newSymbol.Length > 0)
                    {
                        if (newSymbol != symbol)
                        {
                            // User changed symbol; need to remove the old entry before adding the new one
                            mwt.Peptide.RemoveModification(symbol);
                        }
                        var success = mwt.Peptide.SetModificationSymbol(newSymbol, newMass, false, newComment);
                        // TODO: Handle differently: Debug.Assert(success);
                    }
                }

                DisplayCurrentModificationSymbols();
                mValueChanged = true;
            }
        }

        private void RemoveAllPhosphorylationSymbols()
        {
            // Remove all mod symbols with phosphorylation = True
            var index = 0;
            while (index < mwt.Peptide.GetModificationSymbolCount())
            {
                var success = mwt.Peptide.GetModificationSymbol(index, out _, out _, out var indicatesPhosphorylation, out _);
                // TODO: Handle differently: Debug.Assert(success);

                if (indicatesPhosphorylation)
                {
                    var result = mwt.Peptide.RemoveModificationById(index);
                }
                else
                {
                    index++;
                }
            }
        }

        public void ResetModificationSymbolsToDefaults()
        {
            mwt.Peptide.SetDefaultModificationSymbols();
            mValueChanged = true;

            // Update displayed mod list
            DisplayCurrentModificationSymbols();
        }

        private void RestoreSavedModSymbols()
        {
            mwt.Peptide.RemoveAllModificationSymbols();

            foreach (var mod in savedModSymbols)
            {
                // Note: SavedModSymbols[] is 0-based, and the modifications in objMwtWin.Peptide are 0-based
                var success = mwt.Peptide.SetModificationSymbol(mod.Symbol, mod.ModificationMass, mod.IndicatesPhosphorylation, mod.Comment);
                // TODO: Handle differently: Debug.Assert(success);
            }
        }

        private void ValidateNewPhosphorylationSymbol()
        {
            const double PHOSPHORYLATION_MASS = 79.9663326d;

            if (mDelayUpdate)
                return;

            var newSymbol = PhosphorylationSymbol;

            if (newSymbol.Length == 0)
            {
                RemoveAllPhosphorylationSymbols();
                mValueChanged = true;
            }
            else
            {
                // See if any of the current modification symbols match PhosphorylationSymbol
                // If they do, warn user that they will be removed
                var modSymbolID = mwt.Peptide.GetModificationSymbolId(newSymbol);

                MessageBoxResult eResponse;
                if (modSymbolID > 0)
                {
                    eResponse = MessageBox.Show("Warning, the new phosphorylation symbol is already being used for another modification.  Do you really want to use this symbol for phosphorylation and consequently remove the other definition?", "Modification Symbol Conflict", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                }
                else
                {
                    eResponse = MessageBoxResult.Yes;
                }

                if (eResponse == MessageBoxResult.Yes)
                {
                    RemoveAllPhosphorylationSymbols();
                    var success = mwt.Peptide.SetModificationSymbol(newSymbol, PHOSPHORYLATION_MASS, true, "Phosphorylation");
                    // TODO: Handle differently: Debug.Assert(success);

                    mValueChanged = true;
                }
            }

            DisplayCurrentModificationSymbols();
        }

        private void CancelChanges(Window window)
        {
            if (mValueChanged)
            {
                var eResponse = MessageBox.Show("Are you sure you want to lose all changes?", "Closing Edit Modification Symbols Window", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (eResponse == MessageBoxResult.Yes)
                {
                    RestoreSavedModSymbols();
                }
                else
                {
                    return;
                }
            }

            mValueChanged = false;
            window.Close();
        }
    }

    internal readonly struct ModificationSymbolImmutable
    {
        public string Symbol { get; }

        public double ModificationMass { get; }

        public bool IndicatesPhosphorylation { get; }

        public string Comment { get; }

        public ModificationSymbolImmutable(string symbol, double mass, bool indicatesPhosphorylation, string comment)
        {
            Symbol = symbol;
            ModificationMass = mass;
            IndicatesPhosphorylation = indicatesPhosphorylation;
            Comment = comment;
        }
    }

    internal class ModificationSymbolEdit : ReactiveObject
    {
        public ModificationSymbolEdit(int id, string s, double mass, string c)
        {
            Id = id;
            Symbol = s;
            ModificationMass = mass;
            Comment = c;
        }

        private string symbol;
        private double modificationMass;
        private string comment;

        public int Id { get; set; }

        public string Symbol
        {
            get => symbol;
            set => this.RaiseAndSetIfChanged(ref symbol, value);
        }

        public double ModificationMass
        {
            get => modificationMass;
            set => this.RaiseAndSetIfChanged(ref modificationMass, value);
        }

        public string Comment
        {
            get => comment;
            set => this.RaiseAndSetIfChanged(ref comment, value);
        }
    }

    internal class GridRowWindowDataConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value.Length < 2)
                return new Tuple<Window, ModificationSymbolEdit>(new Window(), new ModificationSymbolEdit(-1, "", 0.0, ""));

            if (!(value[0] is AminoAcidModificationsWindow aamw))
                return new Tuple<Window, ModificationSymbolEdit>(new Window(), new ModificationSymbolEdit(-1, "", 0.0, ""));

            if (!(value[1] is ModificationSymbolEdit mse))
                return new Tuple<Window, ModificationSymbolEdit>(new Window(), new ModificationSymbolEdit(-1, "", 0.0, ""));

            return new Tuple<Window, ModificationSymbolEdit>(aamw, mse);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
