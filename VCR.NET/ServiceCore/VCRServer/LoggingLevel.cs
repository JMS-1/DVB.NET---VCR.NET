

namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Used to select which information should be reported to 
    /// the Windows event log.
    /// </summary>
    public enum LoggingLevel
    {
        /// <summary>
        /// All events are logged.
        /// </summary>
        Full,

        /// <summary>
        /// The events for the Windows service control will
        /// not be reported.
        /// </summary>
        Schedules,

        /// <summary>
        /// Only problems will be reported.
        /// </summary>
        Errors,

        /// <summary>
        /// Only report security issues.
        /// </summary>
        Security
    }
}
