using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace JMS.DVBVCR.RecordingService.Status
{
    /// <summary>
    /// Used to report some settings to the client.
    /// </summary>
    [Serializable]
    [XmlType( "VCRSettings" )]
    public class Settings
    {
        /// <summary>
        /// The profiles that can be used.
        /// </summary>
        public readonly List<string> Profiles = new List<string>();

        /// <summary>
        /// Set if the VCR.NET Recording Service is allowed to hibernate the system.
        /// </summary>
        public bool MayHibernateSystem = false;

        /// <summary>
        /// Set to use S3 hibernation mode.
        /// </summary>
        public bool UseStandByForHibernation = false;

        /// <summary>
        /// Gesetzt, wenn eigentlich der Übergang in den Schlafzustand ansteht.
        /// </summary>
        public bool HasPendingHibernation { get; set; }

        /// <summary>
        /// Die minimale Wertezeit im Schlafzustand (in Minuten).
        /// </summary>
        public uint MinimumHibernationDelay { get; set; }
    }
}
