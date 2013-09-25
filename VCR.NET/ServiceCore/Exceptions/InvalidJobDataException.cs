using System;
using System.Runtime.Serialization;
using JMS.DVBVCR.RecordingService.Persistence;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Used to report configuration errors in <see cref="VCRJob"/>
    /// and <see cref="VCRSchedule"/>.
    /// </summary>
    [Serializable]
    public class InvalidJobDataException : Exception
    {
        /// <summary>
        /// Create a new instance of this exception.
        /// </summary>
        /// <param name="reason">Describes the reason for this exception.</param>
        public InvalidJobDataException( string reason )
            : base( reason )
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public InvalidJobDataException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}
