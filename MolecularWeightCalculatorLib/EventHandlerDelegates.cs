using System.Runtime.InteropServices;

namespace MolecularWeightCalculator
{
    [ComVisible(false)]
    public delegate void ProgressResetEventHandler();

    /// <summary>
    /// Progress changed event
    /// </summary>
    /// <param name="taskDescription"></param>
    /// <param name="percentComplete">Ranges from 0 to 100, but can contain decimal percentage values</param>
    [ComVisible(false)]
    public delegate void ProgressChangedEventHandler(string taskDescription, float percentComplete);     // PercentComplete ranges from 0 to 100, but can contain decimal percentage values

    [ComVisible(false)]
    public delegate void ProgressCompleteEventHandler();

    [ComVisible(false)]
    public delegate void MessageEventEventHandler(string message);

    [ComVisible(false)]
    public delegate void ErrorEventEventHandler(string errorMessage);

    [ComVisible(false)]
    public delegate void WarningEventEventHandler(string warningMessage);
}
