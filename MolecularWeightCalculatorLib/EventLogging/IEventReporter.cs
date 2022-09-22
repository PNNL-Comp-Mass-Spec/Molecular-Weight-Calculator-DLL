using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.EventLogging
{
    /// <summary>
    /// Interface for EventReporter; primary use is for interfaces whose implementations should inherit from EventReporter
    /// </summary>
    [ComVisible(false)]
    public interface IEventReporter
    {
        /// <summary>
        /// Debug event
        /// </summary>
        event DebugEventEventHandler DebugEvent;

        /// <summary>
        /// Error event
        /// </summary>
        event ErrorEventEventHandler ErrorEvent;

        /// <summary>
        /// Progress updated
        /// </summary>
        event ProgressUpdateEventHandler ProgressUpdate;

        /// <summary>
        /// Progress reset
        /// </summary>
        event ProgressResetEventHandler ProgressReset;

        /// <summary>
        /// Progress complete
        /// </summary>
        event ProgressCompleteEventHandler ProgressComplete;

        /// <summary>
        /// Status event
        /// </summary>
        event StatusEventEventHandler StatusEvent;

        /// <summary>
        /// Warning event
        /// </summary>
        event WarningEventEventHandler WarningEvent;

        /// <summary>
        /// Number of empty lines to write to the console before displaying a debug message
        /// This is only applicable if WriteToConsoleIfNoListener is true and the event has no listeners
        /// </summary>
        int EmptyLinesBeforeDebugMessages { get; set; }

        /// <summary>
        /// Number of empty lines to write to the console before displaying an error message
        /// This is only applicable if WriteToConsoleIfNoListener is true and the event has no listeners
        /// </summary>
        int EmptyLinesBeforeErrorMessages { get; set; }

        /// <summary>
        /// Number of empty lines to write to the console before displaying a status message
        /// This is only applicable if WriteToConsoleIfNoListener is true and the event has no listeners
        /// </summary>
        int EmptyLinesBeforeStatusMessages { get; set; }

        /// <summary>
        /// Number of empty lines to write to the console before displaying a warning message
        /// This is only applicable if WriteToConsoleIfNoListener is true and the event has no listeners
        /// </summary>
        int EmptyLinesBeforeWarningMessages { get; set; }

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write debug messages to the console if no listener
        /// </summary>
        bool SkipConsoleWriteIfNoDebugListener { get; set; }

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write errors to the console if no listener
        /// </summary>
        bool SkipConsoleWriteIfNoErrorListener { get; set; }

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write progress updates to the console if no listener
        /// </summary>
        bool SkipConsoleWriteIfNoProgressListener { get; set; }

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write status messages to the console if no listener
        /// </summary>
        bool SkipConsoleWriteIfNoStatusListener { get; set; }

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write warnings to the console if no listener
        /// </summary>
        bool SkipConsoleWriteIfNoWarningListener { get; set; }

        /// <summary>
        /// If true, and if an event does not have a listener, display the message at the console
        /// </summary>
        /// <remarks>Defaults to true. Silence individual event types using the SkipConsoleWrite properties</remarks>
        bool WriteToConsoleIfNoListener { get; set; }
    }
}
