using System;

namespace MolecularWeightCalculator.EventLogging
{
    // Singleton class to handle logging to file
    internal class Logging : EventReporter
    {
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

        internal static void MwtWinDllErrorHandler(string callingProcedure, Exception ex)
        {
            Logger.MwtWinDllErrorHandlerInternal(callingProcedure, ex);
        }

        private Logging()
        {
            mMessages = new Messages();

            mLogFolderPath = string.Empty;
            mLogFilePath = string.Empty;
        }

        private readonly Messages mMessages;
        private string mLogFilePath;
        private System.IO.StreamWriter mLogFile;

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
                var errorFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "ErrorLog.txt");

                // Open the file and append a new error entry
                using var outFile = new System.IO.StreamWriter(errorFilePath, true);
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
                    mLogFilePath = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    mLogFilePath += "_log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                    try
                    {
                        mLogFolderPath ??= string.Empty;

                        if (mLogFolderPath.Length > 0)
                        {
                            // Create the log folder if it doesn't exist
                            if (!System.IO.Directory.Exists(mLogFolderPath))
                            {
                                System.IO.Directory.CreateDirectory(mLogFolderPath);
                            }
                        }
                    }
                    catch
                    {
                        mLogFolderPath = string.Empty;
                    }

                    if (mLogFolderPath.Length > 0)
                    {
                        mLogFilePath = System.IO.Path.Combine(mLogFolderPath, mLogFilePath);
                    }

                    var openingExistingFile = System.IO.File.Exists(mLogFilePath);

                    mLogFile = new System.IO.StreamWriter(new System.IO.FileStream(mLogFilePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read))
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

        private void MwtWinDllErrorHandlerInternal(string callingProcedure, Exception ex)
        {
            string message;

            if (ex is OverflowException)
            {
                message = mMessages.LookupMessage(590);

                OnErrorEvent(mMessages.LookupMessage(350) + ": " + message);
                LogMessageInternal(message, MessageType.Error);
            }
            else
            {
                message = mMessages.LookupMessage(600) + ": " + ex.Message + Environment.NewLine + " (" + callingProcedure + " handler)";
                message += Environment.NewLine + mMessages.LookupMessage(605);

                OnErrorEvent(mMessages.LookupMessage(350) + ": " + message);

                // Call GeneralErrorHandler so that the error gets logged to ErrorLog.txt
                // Note that GeneralErrorHandler will call LogMessage
                GeneralErrorHandlerInternal(callingProcedure, ex);
            }
        }
    }
}
