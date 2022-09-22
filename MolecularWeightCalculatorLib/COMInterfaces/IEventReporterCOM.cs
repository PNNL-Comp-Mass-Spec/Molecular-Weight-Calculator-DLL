using System;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.EventLogging;

namespace MolecularWeightCalculator.COMInterfaces
{
    /// <summary>
    /// Event interface to expose events provided by <see cref="IEventReporter"/> to COM
    /// </summary>
    [Guid("3A7F7DB1-0E9D-4E59-BD03-979F54378537"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), ComVisible(true)]
    internal interface IEventReporterCOM
    {
        /// <summary>
        /// Debug event
        /// </summary>
        void DebugEvent(string message);

        /// <summary>
        /// Error event
        /// </summary>
        void ErrorEvent(string message, Exception ex);

        /// <summary>
        /// Progress updated
        /// </summary>
        void ProgressUpdate(string progressMessage, float percentComplete);

        /// <summary>
        /// Progress reset
        /// </summary>
        void ProgressReset();

        /// <summary>
        /// Progress complete
        /// </summary>
        void ProgressComplete();

        /// <summary>
        /// Status event
        /// </summary>
        void StatusEvent(string message);

        /// <summary>
        /// Warning event
        /// </summary>
        void WarningEvent(string message);
    }
}
