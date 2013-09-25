using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.Persistence;


namespace JMS.DVBVCR.RecordingService.LegacyUpgrades.Pre39
{
    /// <summary>
    /// Diese Klasse enthält nur noch die Daten für die Konvertierung aus
    /// einer VCR.NET Version vor 3.9.
    /// </summary>
    [Serializable]
    [XmlType( "VCRJob" )]
    public class Job_Pre39
    {
        /// <summary>
        /// Dateiendung für Aufträge im XML Serialisierungsformat.
        /// </summary>
        public const string FileSuffix = ".vjb";

        /// <summary>
        /// Aufzeichnungen zu diesem Auftrag.
        /// </summary>
        [XmlElement( "Schedule" )]
        public Schedule_Pre39[] Schedules { get; set; }

        /// <summary>
        /// Verzeichnis, in dem Aufzeichnungsdateien abgelegt werden sollen.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string Directory { get; set; }

        /// <summary>
        /// Eindeutige Kennung des Auftrags.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string UniqueID { get; set; }

        /// <summary>
        /// Dateiformat für Aufzeichnungen.
        /// </summary>
        /// <remarks>
        /// Seit VCR.NET 2.7 wird das Dateiformat ignoriert.
        /// </remarks>
        public int FileFormat { get; set; }

        /// <summary>
        /// Sender für alle Aufzeichnungen sofern nicht pro Aufzeichnung überschrieben.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string Station { get; set; }

        /// <summary>
        /// DVB.NET Geräteprofil zur <see cref="Station"/>.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist,
        /// oder für eine Aufzeichnung überschrieben wird.
        /// </remarks>
        [XmlElement( IsNullable = true )]
        public string Profile { get; set; }

        /// <summary>
        /// Dienstekennung zur <see cref="Station"/>.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist,
        /// oder für eine Aufzeichnung überschrieben wird.
        /// </remarks>
        [XmlElement( IsNullable = true )]
        public string NVOD { get; set; }

        /// <summary>
        /// Name des Auftrags.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string Name { get; set; }

        /// <summary>
        /// Aktiviert die Aufzeichnung des AC3 (Dolby Digital) Tons.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist,
        /// oder für eine Aufzeichnung überschrieben wird.
        /// </remarks>
        public bool AC3Recording { get; set; }

        /// <summary>
        /// Aktiviert die Aufzeichnung von DVB Unterttiteln.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist,
        /// oder für eine Aufzeichnung überschrieben wird.
        /// </remarks>
        public bool WithSubtitles { get; set; }

        /// <summary>
        /// Aktiviert die Aufzeichnung aller MP2 Tonspuren.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist,
        /// oder für eine Aufzeichnung überschrieben wird.
        /// </remarks>
        public bool AllLanguages { get; set; }

        /// <summary>
        /// Aktiviert die Aufzeichung des Videotext Signals.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist,
        /// oder für eine Aufzeichnung überschrieben wird.
        /// </remarks>
        public bool WithTeleText { get; set; }

        /// <summary>
        /// Die zugehörige Datei.
        /// </summary>
        [XmlIgnore]
        public FileInfo File { get; private set; }

        /// <summary>
        /// Die volle Bezeichnung der Quelle.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Wandelt diesen Auftrag in die neue Darstellung um.
        /// </summary>
        /// <returns>Die neue Darstellung des Auftrags.</returns>
        internal VCRJob ToJob()
        {
            // Create new
            var job =
                new VCRJob
                {
                    UniqueID = UniqueID.ToUniqueIdentifier(),
                    Directory = Directory,
                    Name = Name,
                };

            // See if a source is defined
            if (!string.IsNullOrEmpty( Station ))
            {
                // Find selection
                job.Source = Station.ToSelection( Profile );

                // Update streams
                job.Streams = Schedule_Pre39.CreateStreams( Station, AllLanguages, AC3Recording, WithSubtitles, WithTeleText );
            }
            else
            {
                // Must preserve profile name
                job.Source = new SourceSelection { ProfileName = Profile };
            }

            // Process all schedules
            if (null != Schedules)
                foreach (var schedule in Schedules)
                    job.Schedules.Add( schedule.ToSchedule( this ) );

            // Report
            return job;
        }

        /// <summary>
        /// Aktualisiert alle Aufträge in einem Verzeichnis.
        /// </summary>
        /// <param name="directory">Das zu bearbeitende Verzeichnis.</param>
        public static void UpgradeAndDelete( DirectoryInfo directory )
        {
            // Report
            Tools.ExtendedLogging( "Upgrading {0}", directory.FullName );

            // Process
            foreach (var job in Load( directory ))
                try
                {
                    // Report
                    VCRServer.Log( LoggingLevel.Full, Properties.Resources.Information_JobUpgrade, job.File.FullName );

                    // Store
                    job.ToJob().Save( directory );

                    // Delete self
                    job.File.Delete();
                }
                catch (Exception e)
                {
                    // Report
                    VCRServer.Log( e );
                }
        }

        /// <summary>
        /// Ermittelt alle alten Repräsentationen in einem Verzeichnis.
        /// </summary>
        /// <param name="directory">Das zu bearbeitende Verzeichnis.</param>
        /// <returns>Alle alten Aufträge.</returns>
        public static IEnumerable<Job_Pre39> Load( DirectoryInfo directory )
        {
            // Process
            foreach (var file in directory.GetFiles( "*" + FileSuffix ))
            {
                // File to load
                var old = SerializationTools.Load<Job_Pre39>( file );
                if (null == old)
                    continue;

                // Attach file
                old.File = file;

                // Report
                yield return old;
            }
        }
    }
}
