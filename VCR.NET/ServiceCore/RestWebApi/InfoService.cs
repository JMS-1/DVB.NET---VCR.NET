using System;
using System.Runtime.Serialization;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt die aktuelle Version des VCR.NET Recording Service.
    /// </summary>
    [DataContract]
    [Serializable]
    public class InfoService
    {
        /// <summary>
        /// Die Zeichenkette mit den Versionsdaten.
        /// </summary>
        [DataMember( Name = "version" )]
        public string Version { get; set; }

        /// <summary>
        /// Die Zeichenkette mit den Versionsdaten.
        /// </summary>
        [DataMember( Name = "msiVersion" )]
        public string InstalledVersion { get; set; }

        /// <summary>
        /// Gesetzt, wenn mindestens ein Gerät in Verwendung ist.
        /// </summary>
        [DataMember( Name = "active" )]
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gesetzt, wenn der aktuelle Anwender ein Administrator ist.
        /// </summary>
        [DataMember( Name = "userIsAdmin" )]
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Gesetzt, wenn es möglich ist, die Liste der Quellen zu aktualisieren.
        /// </summary>
        [DataMember( Name = "canScan" )]
        public bool SourceScanEnabled { get; set; }

        /// <summary>
        /// Gesetzt, wenn es möglich ist, die Programmzeitschriften zu aktualisieren.
        /// </summary>
        [DataMember( Name = "hasGuides" )]
        public bool GuideUpdateEnabled { get; set; }

        /// <summary>
        /// Gesetzt, wenn ein Schlafzustand eigentlich ausgelöst werden sollte.
        /// </summary>
        [DataMember( Name = "hibernationPending" )]
        public bool HasPendingHibernation { get; set; }

        /// <summary>
        /// Gesetzt, wenn noch Erweiterungen aktiv sind.
        /// </summary>
        [DataMember( Name = "extensionsRunning" )]
        public bool HasPendingExtensions { get; set; }

        /// <summary>
        /// Die Namen aller Geräteprofile.
        /// </summary>
        [DataMember( Name = "profileNames" )]
        public string[] ProfilesNames { get; set; }

        /// <summary>
        /// Die minimale Verweildauer (in Minuten) im Schlafzustand.
        /// </summary>
        [DataMember( Name = "sleepMinimum" )]
        public uint MinimumHibernationDelay { get; set; }
    }
}
