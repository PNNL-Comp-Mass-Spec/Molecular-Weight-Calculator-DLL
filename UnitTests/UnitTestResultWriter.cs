using System.IO;
using JetBrains.Annotations;

namespace UnitTests
{
    public enum UnitTestWriterType
    {
        NoWriter = 0,
        ComputeMass = 1,
        StressTest = 2,
        ConvertToEmpirical = 3,
        CircularReferenceHandling = 4,
        ExpandAbbreviations = 5,
        PercentComposition = 6,
        UniModFormulaWriter = 7,
        UnitTestCaseWriter = 8
    }

    public class UnitTestResultWriter
    {
        /// <summary>
        /// Set to true once the results file path has been shown at the console
        /// </summary>
        public bool FilePathShown { get; set; }

        /// <summary>
        /// Unit test results file info
        /// </summary>
        public FileInfo ResultsFile { get; }

        /// <summary>
        /// Unit test results writer
        /// </summary>
        private StreamWriter Writer { get; set; }

        private void InitializeWriter()
        {
            Writer = new StreamWriter(new FileStream(ResultsFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                AutoFlush = true
            };
        }

        /// <summary>
        /// Append a line to the results file
        /// </summary>
        /// <param name="value"></param>
        public void WriteLine(string value)
        {
            if (Writer == null)
            {
                InitializeWriter();
            }

            Writer.WriteLine(value);
        }

        /// <summary>
        /// Append a line to the results file
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [StringFormatMethod("format")]
        public void WriteLine(string format, params object[] args)
        {
            if (Writer == null)
            {
                InitializeWriter();
            }

            Writer.WriteLine(format, args);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resultsFileName"></param>
        public UnitTestResultWriter(string resultsFileName)
        {
            ResultsFile = new FileInfo(resultsFileName);

            Writer = null;

            FilePathShown = false;
        }
    }
}
