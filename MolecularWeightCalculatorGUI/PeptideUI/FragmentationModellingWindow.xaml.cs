using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        private void NeutralLossIonTypes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initializingSelection)
            {
                return;
            }

            if (DataContext is FragmentationModellingViewModel fmvm)
            {
                fmvm.NeutralLossIonTypes.RemoveMany(e.RemovedItems.Cast<object>().Where(x => x is IonType).Cast<IonType>());
                var added = e.AddedItems.Cast<object>().Where(x => x is IonType).Cast<IonType>().Except(fmvm.NeutralLossIonTypes).ToList();
                fmvm.NeutralLossIonTypes.AddRange(added);
            }
        }

        private bool initializingSelection = false;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            initializingSelection = true;
            if (e.NewValue is FragmentationModellingViewModel fmvm)
            {
                NeutralLossIons.SelectedItems.Clear();
                foreach (var ion in fmvm.NeutralLossIonTypes)
                {
                    NeutralLossIons.SelectedItems.Add(ion);
                }
            }

            initializingSelection = false;
        }
    }
}
