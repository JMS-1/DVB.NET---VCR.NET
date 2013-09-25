using System;


namespace JMS.DVBVCR.RecordingService.Win32Tools
{
    /// <summary>
    /// Used by the <see cref="JMS.DVBVCR.RecordingService.Win32Tools.WaitableTimer"/>
    /// to report errors.
    /// </summary>
    public class TimerException : Exception
    {
        /// <summary>
        /// Create a new instance of this exception.
        /// </summary>
        /// <param name="sReason">Some text to describe the error condition.</param>
        public TimerException( string sReason )
            : base( sReason )
        {
        }
    }
}
