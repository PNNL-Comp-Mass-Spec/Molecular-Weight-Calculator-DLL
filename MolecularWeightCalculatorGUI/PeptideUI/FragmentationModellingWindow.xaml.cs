﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

            if (DataContext is FragmentationModellingViewModel viewModel)
            {
                viewModel.NeutralLossIonTypes.RemoveMany(e.RemovedItems.Cast<object>().Where(x => x is IonType).Cast<IonType>());
                var added = e.AddedItems.Cast<object>().Where(x => x is IonType).Cast<IonType>().Except(viewModel.NeutralLossIonTypes).ToList();
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
    }
}