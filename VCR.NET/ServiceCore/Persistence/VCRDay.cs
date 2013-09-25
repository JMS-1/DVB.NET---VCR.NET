using System;
using System.Xml.Serialization;


namespace JMS.DVBVCR.RecordingService.Persistence
{
    /// <summary>
    /// Each numeration entry defines a bit position to be used in <see cref="VCRSchedule.Days"/>.
    /// </summary>
    [Flags]
    [XmlType( "VCRDays" )]
    public enum VCRDay
    {
        /// <summary>
        /// Monday is bit 0.
        /// </summary>
        Monday = 0x01,

        /// <summary>
        /// Tuesday is bit 1.
        /// </summary>
        Tuesday = 0x02,

        /// <summary>
        /// Wednesday is bit 2.
        /// </summary>
        Wednesday = 0x04,

        /// <summary>
        /// Thursday is bit 3.
        /// </summary>
        Thursday = 0x08,

        /// <summary>
        /// Friday is bit 4.
        /// </summary>
        Friday = 0x10,

        /// <summary>
        /// Saturday is bit 5.
        /// </summary>
        Saturday = 0x20,

        /// <summary>
        /// Sunday is bit 6.
        /// </summary>
        Sunday = 0x40
    }
}
