using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations.MWT;

namespace MolecularWeightCalculator.EventLogging
{
    /// <summary>
    /// Debug event
    /// </summary>
    /// <param name="message">Debug message</param>
    public delegate void DebugEventEventHandler(string message);

    /// <summary>
    /// Error event
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="ex">Exception</param>
    public delegate void ErrorEventEventHandler(string message, Exception ex);

    /// <summary>
    /// Progress updated
    /// </summary>
    /// <param name="progressMessage"></param>
    /// <param name="percentComplete">Value between 0 and 100</param>
    public delegate void ProgressUpdateEventHandler(string progressMessage, float percentComplete);

    /// <summary>
    /// Progress complete
    /// </summary>
    public delegate void ProgressResetEventHandler();

    /// <summary>
    /// Progress reset
    /// </summary>
    public delegate void ProgressCompleteEventHandler();

    /// <summary>
    /// Status event
    /// </summary>
    /// <param name="message">Status message</param>
    public delegate void StatusEventEventHandler(string message);

    /// <summary>
    /// Warning event
    /// </summary>
    /// <param name="message">Warning message</param>
    public delegate void WarningEventEventHandler(string message);

    /// <summary>
    /// This class implements various status events, including status, debug, error, and warning
    /// </summary>
    // Many similarities to https://github.com/PNNL-Comp-Mass-Spec/PRISM-Class-Library/blob/master/PRISM/Logging/EventNotifier.cs
    // Could have just used that, but I want to keep the external dependencies of this library to 'none', if possible, and I also don't want conflicts with that class
    [ComVisible(false)]
    public abstract class EventReporter : IEventReporter
    {
        /// <summary>
        /// Debug event
        /// </summary>
        public event DebugEventEventHandler DebugEvent;

        /// <summary>
        /// Error event
        /// </summary>
        public event ErrorEventEventHandler ErrorEvent;

        /// <summary>
        /// Progress updated
        /// </summary>
        public event ProgressUpdateEventHandler ProgressUpdate;

        /// <summary>
        /// Progress reset
        /// </summary>
        public event ProgressResetEventHandler ProgressReset;

        /// <summary>
        /// Progress complete
        /// </summary>
        public event ProgressCompleteEventHandler ProgressComplete;

        /// <summary>
        /// Status event
        /// </summary>
        public event StatusEventEventHandler StatusEvent;

        /// <summary>
        /// Warning event
        /// </summary>
        public event WarningEventEventHandler WarningEvent;

        /// <summary>
        /// Number of empty lines to write to the console before displaying a debug message
        /// This is only applicable if WriteToConsoleIfNoListener is true and the event has no listeners
        /// </summary>
        public int EmptyLinesBeforeDebugMessages { get; set; } = 1;

        /// <summary>
        /// Number of empty lines to write to the console before displaying an error message
        /// This is only applicable if WriteToConsoleIfNoListener is true and the event has no listeners
        /// </summary>
        public int EmptyLinesBeforeErrorMessages { get; set; } = 1;

        /// <summary>
        /// Number of empty lines to write to the console before displaying a status message
        /// This is only applicable if WriteToConsoleIfNoListener is true and the event has no listeners
        /// </summary>
        public int EmptyLinesBeforeStatusMessages { get; set; } = 0;

        /// <summary>
        /// Number of empty lines to write to the console before displaying a warning message
        /// This is only applicable if WriteToConsoleIfNoListener is true and the event has no listeners
        /// </summary>
        public int EmptyLinesBeforeWarningMessages { get; set; } = 1;

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write debug messages to the console if no listener
        /// </summary>
        public bool SkipConsoleWriteIfNoDebugListener { get; set; }

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write errors to the console if no listener
        /// </summary>
        public bool SkipConsoleWriteIfNoErrorListener { get; set; }

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write progress updates to the console if no listener
        /// </summary>
        public bool SkipConsoleWriteIfNoProgressListener { get; set; }

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write status messages to the console if no listener
        /// </summary>
        public bool SkipConsoleWriteIfNoStatusListener { get; set; }

        /// <summary>
        /// If WriteToConsoleIfNoListener is true, optionally set this to true to not write warnings to the console if no listener
        /// </summary>
        public bool SkipConsoleWriteIfNoWarningListener { get; set; }

        /// <summary>
        /// If true, and if an event does not have a listener, display the message at the console
        /// </summary>
        /// <remarks>Defaults to true. Silence individual event types using the SkipConsoleWrite properties</remarks>
        public bool WriteToConsoleIfNoListener { get; set; } = true;

        // ReSharper disable UnusedMember.Global

        /// <summary>
        /// True if the Debug event has any listeners
        /// </summary>
        protected bool HasEventListenerDebug => DebugEvent != null;

        /// <summary>
        /// True if the Error event has any listeners
        /// </summary>
        protected bool HasEventListenerError => ErrorEvent != null;

        /// <summary>
        /// True if the ProgressUpdate event has any listeners
        /// </summary>
        protected bool HasEventListenerProgressUpdate => ProgressUpdate != null;

        /// <summary>
        /// True if the StatusEvent event has any listeners
        /// </summary>
        protected bool HasEventListenerStatusEvent => StatusEvent != null;

        /// <summary>
        /// True if the WarningEvent event has any listeners
        /// </summary>
        protected bool HasEventListenerWarningEvent => WarningEvent != null;

        // ReSharper restore UnusedMember.Global

        /// <summary>
        /// Report a debug message
        /// </summary>
        /// <param name="message">Debug message</param>
        protected void OnDebugEvent(string message)
        {
            if (DebugEvent == null && WriteToConsoleIfNoListener && !SkipConsoleWriteIfNoDebugListener)
            {
                ConsoleWriteEmptyLines(EmptyLinesBeforeDebugMessages);
                Console.WriteLine(message);
            }

            DebugEvent?.Invoke(message);
        }

        private void ConsoleWriteEmptyLines(int count)
        {
            for (var i = 0; i < count; i++)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Report a debug message
        /// </summary>
        /// <param name="format">Debug message format string</param>
        /// <param name="args">string format arguments</param>
        [StringFormatMethod("format")]
        protected void OnDebugEvent(string format, params object[] args)
        {
            OnDebugEvent(string.Format(format, args));
        }

        /// <summary>
        /// Report an error
        /// </summary>
        /// <param name="message">Error message</param>
        protected void OnErrorEvent(string message)
        {
            if (ErrorEvent == null && WriteToConsoleIfNoListener && !SkipConsoleWriteIfNoErrorListener)
            {
                ConsoleWriteEmptyLines(EmptyLinesBeforeErrorMessages);
                Console.WriteLine(message);
            }

            ErrorEvent?.Invoke(message, null);
        }

        /// <summary>
        /// Report an error
        /// </summary>
        /// <param name="format">Error message format string</param>
        /// <param name="args">string format arguments</param>
        [StringFormatMethod("format")]
        protected void OnErrorEvent(string format, params object[] args)
        {
            OnErrorEvent(string.Format(format, args));
        }

        /// <summary>
        /// Report an error
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Exception (allowed to be null)</param>
        protected void OnErrorEvent(string message, Exception ex)
        {
            if (ErrorEvent == null && WriteToConsoleIfNoListener && !SkipConsoleWriteIfNoErrorListener)
            {
                ConsoleWriteEmptyLines(EmptyLinesBeforeErrorMessages);

                var formattedMessage = message;
                if (ex != null && !message.Contains(ex.Message))
                {
                    formattedMessage += ": " + ex.Message;
                }

                Console.WriteLine(formattedMessage);

                if (ex != null)
                {
                    Console.WriteLine();
                    // TODO: Could make this fancier, but that's more complicated
                    Console.WriteLine(ex.StackTrace);
                }
            }

            ErrorEvent?.Invoke(message, ex);
        }

        /// <summary>
        /// Report an error
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="format">Error message format string</param>
        /// <param name="args">string format arguments</param>
        [StringFormatMethod("format")]
        protected void OnErrorEvent(Exception ex, string format, params object[] args)
        {
            OnErrorEvent(string.Format(format, args), ex);
        }

        /// <summary>
        /// Progress update
        /// </summary>
        /// <param name="progressMessage">Progress message</param>
        /// <param name="percentComplete">Value between 0 and 100</param>
        protected void OnProgressUpdate(string progressMessage, float percentComplete)
        {
            if (ProgressUpdate == null && WriteToConsoleIfNoListener && !SkipConsoleWriteIfNoProgressListener)
            {
                Console.WriteLine("{0:F2}%: {1}", percentComplete, progressMessage);
            }

            ProgressUpdate?.Invoke(progressMessage, percentComplete);
        }

        /// <summary>
        /// Progress update
        /// </summary>
        /// <param name="percentComplete">Value between 0 and 100</param>
        /// <param name="format">Progress message format string</param>
        /// <param name="args">string format arguments</param>
        [StringFormatMethod("format")]
        // ReSharper disable once UnusedMember.Global
        protected void OnProgressUpdate(float percentComplete, string format, params object[] args)
        {
            OnProgressUpdate(string.Format(format, args), percentComplete);
        }

        /// <summary>
        /// Progress reset
        /// </summary>
        protected void OnProgressReset()
        {
            ProgressReset?.Invoke();
        }

        /// <summary>
        /// Progress complete
        /// </summary>
        protected void OnProgressComplete()
        {
            ProgressComplete?.Invoke();
        }

        /// <summary>
        /// Report a status message
        /// </summary>
        /// <param name="message">Status message</param>
        protected void OnStatusEvent(string message)
        {
            if (StatusEvent == null && WriteToConsoleIfNoListener && !SkipConsoleWriteIfNoStatusListener)
            {
                ConsoleWriteEmptyLines(EmptyLinesBeforeStatusMessages);
                Console.WriteLine(message);
            }

            StatusEvent?.Invoke(message);
        }

        /// <summary>
        /// Report a status message
        /// </summary>
        /// <param name="format">Status message format string</param>
        /// <param name="args">string format arguments</param>
        [StringFormatMethod("format")]
        protected void OnStatusEvent(string format, params object[] args)
        {
            OnStatusEvent(string.Format(format, args));
        }

        /// <summary>
        /// Report a warning
        /// </summary>
        /// <param name="message">Warning message</param>
        protected void OnWarningEvent(string message)
        {
            if (WarningEvent == null && WriteToConsoleIfNoListener && !SkipConsoleWriteIfNoWarningListener)
            {
                ConsoleWriteEmptyLines(EmptyLinesBeforeWarningMessages);
                Console.WriteLine(message);
            }

            WarningEvent?.Invoke(message);
        }

        /// <summary>
        /// Report a warning
        /// </summary>
        /// <param name="format">Warning message format string</param>
        /// <param name="args">string format arguments</param>
        [StringFormatMethod("format")]
        protected void OnWarningEvent(string format, params object[] args)
        {
            OnWarningEvent(string.Format(format, args));
        }

        /// <summary>
        /// Use this method to chain events between classes
        /// </summary>
        /// <param name="sourceClass"></param>
        protected void RegisterEvents(IEventReporter sourceClass)
        {
            sourceClass.DebugEvent += OnDebugEvent;
            sourceClass.StatusEvent += OnStatusEvent;
            sourceClass.ErrorEvent += OnErrorEvent;
            sourceClass.WarningEvent += OnWarningEvent;
            sourceClass.ProgressUpdate += OnProgressUpdate;
            sourceClass.ProgressReset += OnProgressReset;
            sourceClass.ProgressComplete += OnProgressComplete;
        }
    }
}
