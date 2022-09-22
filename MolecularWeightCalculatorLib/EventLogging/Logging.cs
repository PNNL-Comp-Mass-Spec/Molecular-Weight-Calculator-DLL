using System;
using System.IO;

namespace MolecularWeightCalculator.EventLogging
{
    // Singleton class to handle logging to file
    internal class Logging : IDisposable
    {
        // TODO: Change this class to use a writable path if the current path is not writable?
        // TODO: Probably should only do that for executables that start with "MolecularWeightCalculator".
        public static Logging Logger { get; } = new Logging();

        internal static Messages Messages => Logger.mMessages;

        public static string LogFilePath => Logger.mLogFilePath;

        /// <summary>
        /// Log file folder
        /// If blank, mOutputFolderPath will be used
        /// If mOutputFolderPath is also blank,  the log is created in the same folder as the executing assembly
        /// </summary>
        public static string LogFolderPath
        {
            get => Logger.mLogFolderPath;
            set => Logger.mLogFolderPath = value;
        }

        public static bool LogMessagesToFile
        {
            get => Logger.mLogMessagesToFile;
            set => Logger.mLogMessagesToFile = value;
        }

        internal enum MessageType
        {
            Normal = 0,
            Error = 1,
            Warning = 2
        }

        public static string GeneralErrorHandler(string callingProcedure, Exception ex)
        {
            return Logger.GeneralErrorHandlerInternal(callingProcedure, ex);
        }

        internal static void LogMessage(string message, MessageType messageType = MessageType.Normal)
        {
            Logger.LogMessageInternal(message, messageType);
        }

        private Logging()
        {
            mMessages = new Messages();

            mLogFolderPath = string.Empty;
            mLogFilePath = string.Empty;
        }

        ~Logging()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                GC.SuppressFinalize(this);
                mLogFile?.Dispose();
                disposed = true;
            }
        }

        private bool disposed = false;
        private readonly Messages mMessages;
        private string mLogFilePath;
        private StreamWriter mLogFile;

        /// <summary>
        /// Log file folder
        /// If blank, mOutputFolderPath will be used
        /// If mOutputFolderPath is also blank,  the log is created in the same folder as the executing assembly
        /// </summary>
        private string mLogFolderPath;

        private bool mLogMessagesToFile;

        private string GeneralErrorHandlerInternal(string callingProcedure, Exception ex)
        {
            var message = "Error in " + callingProcedure + ": ";
            if (!string.IsNullOrEmpty(ex.Message))
            {
                message += Environment.NewLine + ex.Message;
            }

            LogMessageInternal(message, MessageType.Error);

            try
            {
                var errorFilePath = Path.Combine(Environment.CurrentDirectory, "ErrorLog.txt");

                // Open the file and append a new error entry
                using var outFile = new StreamWriter(errorFilePath, true);
                outFile.WriteLine(DateTime.Now + " -- " + message + Environment.NewLine);
            }
            catch
            {
                // Ignore errors here
            }

            return message;
        }

        private void LogMessageInternal(string message, MessageType messageType = MessageType.Normal)
        {
            // Note that CleanupFilePaths() will update OutputFolderPath, which is used here if mLogFolderPath is blank
            // Thus, be sure to call CleanupFilePaths (or update mLogFolderPath) before the first call to LogMessage

            if (mLogFile == null && mLogMessagesToFile)
            {
                try
                {
                    mLogFilePath = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    mLogFilePath += "_log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                    try
                    {
                        mLogFolderPath ??= string.Empty;

                        if (mLogFolderPath.Length > 0)
                        {
                            // Create the log folder if it doesn't exist
                            if (!Directory.Exists(mLogFolderPath))
                            {
                                Directory.CreateDirectory(mLogFolderPath);
                            }
                        }
                    }
                    catch
                    {
                        mLogFolderPath = string.Empty;
                    }

                    if (mLogFolderPath.Length > 0)
                    {
                        mLogFilePath = Path.Combine(mLogFolderPath, mLogFilePath);
                    }

                    var openingExistingFile = File.Exists(mLogFilePath);

                    mLogFile = new StreamWriter(new FileStream(mLogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        AutoFlush = true
                    };

                    if (!openingExistingFile)
                    {
                        mLogFile.WriteLine("Date" + "\t" +
                            "Type" + "\t" +
                            "Message");
                    }
                }
                catch
                {
                    // Error creating the log file; set mLogMessagesToFile to false so we don't repeatedly try to create it
                    mLogMessagesToFile = false;
                }
            }

            var messageTypeText = messageType switch
            {
                MessageType.Normal => "Normal",
                MessageType.Error => "Error",
                MessageType.Warning => "Warning",
                _ => "Unknown"
            };

            if (mLogFile == null)
            {
                Console.WriteLine(messageTypeText + "\t" + message);
            }
            else
            {
                mLogFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt") + "\t" +
                    messageTypeText + "\t" + message);
            }
        }
    }
}
