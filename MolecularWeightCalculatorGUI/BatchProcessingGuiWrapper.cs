using System;
using System.Threading.Tasks;
using System.Windows;
using MolecularWeightCalculator;
using MolecularWeightCalculatorGUI.Utilities;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI
{
    /// <summary>
    /// Simple wrapper to keep UI-specific properties and base classes out of <see cref="BatchProcessing"/>
    /// </summary>
    internal class BatchProcessingGuiWrapper : ReactiveObject, IProgressViewModel
    {
        public BatchProcessingGuiWrapper(MolecularWeightTool mwt)
        {
            batchProcessing = new BatchProcessing(mwt);

            AbortCommand = ReactiveCommand.Create(() => { batchProcessing.AbortProcessing = true; });
        }

        private readonly BatchProcessing batchProcessing;
        private bool showProgress;
        private double progress;
        private string status;
        private string subStatus;

        public bool ShowProgress
        {
            get => showProgress;
            set => this.RaiseAndSetIfChanged(ref showProgress, value);
        }

        public double Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
        }

        public string Status
        {
            get => status;
            set => this.RaiseAndSetIfChanged(ref status, value);
        }

        public string SubStatus
        {
            get => subStatus;
            set => this.RaiseAndSetIfChanged(ref subStatus, value);
        }

        public ReactiveCommand<RxUnit, RxUnit> AbortCommand { get; }

        public bool AbortProcessing
        {
            get => batchProcessing.AbortProcessing;
            set => batchProcessing.AbortProcessing = value;
        }

        public async Task BatchProcessTextFile(Window parent)
        {
            // the action passed via the constructor runs in this synchronization context, not in the context where .Report is called.
            var progressReporter = new Progress<ProgressValues>(data =>
            {
                ShowProgress = data.ShowProgress;
                Progress = data.Progress;
                Status = data.Status;
                SubStatus = data.SubStatus;
            });

            await batchProcessing.BatchProcessTextFile(parent, progressReporter);
        }
    }
}
