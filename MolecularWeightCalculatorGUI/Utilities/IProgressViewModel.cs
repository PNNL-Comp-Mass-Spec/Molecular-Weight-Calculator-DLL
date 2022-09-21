using System;
using System.ComponentModel;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.Utilities
{
    // TODO: Abort click support? Not going to implement "pause"
    /* Original Progress window info:
     * progress bar
     * status
     * sub-status
     * "Overall progress"
     * "0.37 : 0.37 min. elapsed/remaining"
     * Button: "Click to Pause"
     * "(Press Escape to abort)"
     * progress bar for sub-task (was only used by frmCWSpectrum.frm for saving data to disk)
     * Also had tie-ins to cancel the current task if the dialog window was closed, but only after the user had already clicked it once, at least 20 seconds earlier
     */
    internal interface IProgressViewModel : INotifyPropertyChanged
    {
        bool ShowProgress { get; }
        double Progress { get; }
        string Status { get; }
        string SubStatus { get; }
        ReactiveCommand<RxUnit, RxUnit> AbortCommand { get; }
    }

    /// <summary>
    /// Class just to provide a non-specific implementation of <see cref="IProgressViewModel"/>
    /// </summary>
    internal class DevProgressViewModel : IProgressViewModel
    {
        [Obsolete("For WPF design-time use only", true)]
        public DevProgressViewModel()
        {
            AbortCommand = ReactiveCommand.Create(() => { });
        }

#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
        public bool ShowProgress => true;
        public double Progress => 35;
        public string Status => "Main Status";
        public string SubStatus => "Additional Status";
        public ReactiveCommand<RxUnit, RxUnit> AbortCommand { get; }
    }

    internal struct ProgressValues
    {
        public bool ShowProgress { get; set; }
        public double Progress { get; set; }
        public string Status { get; set; }
        public string SubStatus { get; set; }

        public ProgressValues(bool showProgress)
        {
            ShowProgress = showProgress;
            Progress = 0;
            Status = "";
            SubStatus = "";
        }
    }
}
