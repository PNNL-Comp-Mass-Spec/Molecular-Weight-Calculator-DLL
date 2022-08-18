using System.Windows;
using WpfExtras;

namespace MolecularWeightCalculatorGUI.FormulaFinder
{
    /// <summary>
    /// Binding proxy to help with DataGrid/ItemsControl binding to base DataContext
    /// </summary>
    /// <remarks>https://thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/</remarks>
    internal class FormulaFinderBindingProxy : BindingProxy<FormulaFinderViewModel>
    {
        protected override BindingProxy<FormulaFinderViewModel> CreateNewInstance()
        {
            return new FormulaFinderBindingProxy();
        }

        /// <summary>
        /// Data object for binding
        /// </summary>
        public override FormulaFinderViewModel Data
        {
            get => (FormulaFinderViewModel)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <summary>
        /// DependencyProperty definition for Data
        /// </summary>
        public new static readonly DependencyProperty DataProperty = BindingProxy<FormulaFinderViewModel>.DataProperty.AddOwner(typeof(FormulaFinderBindingProxy));
    }
}
