using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DynamicData;
using MolecularWeightCalculator.Sequence;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    /// <summary>
    /// Interaction logic for FragmentationModellingWindow.xaml
    /// </summary>
    public partial class FragmentationModellingWindow : Window
    {
        public FragmentationModellingWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CanClose(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NeutralLossIonTypes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initializingSelection)
            {
                return;
            }

            if (DataContext is FragmentationModellingViewModel viewModel)
            {
                viewModel.NeutralLossIonTypes.RemoveMany(e.RemovedItems.Cast<object>().OfType<IonType>());
                var added = e.AddedItems.Cast<object>().OfType<IonType>().Except(viewModel.NeutralLossIonTypes).ToList();
                viewModel.NeutralLossIonTypes.AddRange(added);
            }
        }

        private bool initializingSelection = false;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            initializingSelection = true;
            if (e.NewValue is FragmentationModellingViewModel viewModel)
            {
                NeutralLossIons.SelectedItems.Clear();
                foreach (var ion in viewModel.NeutralLossIonTypes)
                {
                    NeutralLossIons.SelectedItems.Add(ion);
                }
            }

            initializingSelection = false;
        }

        private void DataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (sender is DataGrid dg && dg.ItemsSource is DataView dv)
            {
                // Prefer the caption for the displayed column header
                e.Column.Header = dv.Table.Columns[e.PropertyName].Caption;
            }

            if (e.PropertyType == typeof(FragmentationGridIon))
            {
                var converter = new DataRowViewIonValueConverter();
                e.Column = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)Resources["IonTemplate"],
                    // Support built-in copy (paste is blocked)
                    ClipboardContentBinding = new Binding()
                    {
                        Converter = converter,
                        BindsDirectlyToSource = true,
                        RelativeSource = RelativeSource.Self,
                    },
                    Header = e.Column.Header,
                    HeaderTemplate = e.Column.HeaderTemplate,
                    HeaderStringFormat = e.Column.HeaderStringFormat,
                    SortMemberPath = e.PropertyName
                };
            }
            else if (e.Column is DataGridTextColumn dgtc)
            {
                var style = (Style)Resources["RightAlignCell"];
                if (e.Column.Header.ToString().StartsWith("seq", StringComparison.OrdinalIgnoreCase))
                {
                    style = (Style)Resources["CenterAlignCell"];
                }

                dgtc.ElementStyle = style;
            }
        }

        private void FragmentationModellingWindow_OnActivated(object sender, EventArgs e)
        {
            if (DataContext is FragmentationModellingViewModel fmvm)
            {
                fmvm.ActivateWindow();
            }
        }

        private void FragmentationModellingWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // TODO: Load initial size, then change 'SizeToContent' to 'Manual'
            // TODO: Also need to do something similar when the ion list is shown/hidden...
            Width = ActualWidth;
            Height = ActualHeight;
            SizeToContent = SizeToContent.Manual;
        }
    }
}
