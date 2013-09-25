using System;
using System.Runtime.Serialization;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.ProgramGuide;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt die Daten eines Auftrags.
    /// </summary>
    [DataContract]
    [Serializable]
    public class EditJob
    {
        /// <summary>
        /// Der Name des Auftrags.
        /// </summary>
        [DataMember( Name = "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Das Aufzeichnungsverzeichnis zum Auftrag.
        /// </summary>
        [DataMember( Name = "directory" )]
        public string RecordingDirectory { get; set; }

        /// <summary>
        /// Das für die Auswahl der Quelle verwendete Gerät.
        /// </summary>
        [DataMember( Name = "device" )]
        public string Profile { get; set; }

        /// <summary>
        /// Die Quelle, von der aufgezeichnet werden soll.
        /// </summary>
        [DataMember( Name = "sourceName" )]
        public string Source { get; set; }

        /// <summary>
        /// Gesetzt, wenn die Aufzeichnung auf jeden Fall auf dem für die Auswahl der Quelle
        /// verwendetem Geräte ausgeführt werden soll.
        /// </summary>
        [DataMember( Name = "lockedToDevice" )]
        public bool UseProfileForRecording { get; set; }

        /// <summary>
        /// Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.
        /// </summary>
        [DataMember( Name = "allLanguages" )]
        public bool AllLanguages { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch die <i>Dolby Digital</i> Tonspur aufgezeichnet werden soll.
        /// </summary>
        [DataMember( Name = "includeDolby" )]
        public bool DolbyDigital { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.
        /// </summary>
        [DataMember( Name = "withVideotext" )]
        public bool Videotext { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch alle DVB Untertitel aufgezeichnet werden sollen.
        /// </summary>
        [DataMember( Name = "withSubtitles" )]
        public bool DVBSubtitles { get; set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="job">Der konkrete Auftag.</param>
        /// <param name="guide">Ein Eintrag der Programmzeitschrift.</param>
        /// <param name="profile">Vorgabe für das Geräteprofil.</param>
        /// <returns>Die zugehörige Beschreibung.</returns>
        public static EditJob Create( VCRJob job, ProgramGuideEntry guide, string profile )
        {
            // Process
            if (job == null)
            {
                // No hope
                if (guide == null)
                    return null;

                // Create from program guide            
                return
                    new EditJob
                    {
                        Source = ServerRuntime.VCRServer.GetUniqueName( new SourceSelection { ProfileName = profile, Source = guide.Source } ),
                        DVBSubtitles = UserProfileSettings.UseSubTitles,
                        DolbyDigital = UserProfileSettings.UseAC3,
                        AllLanguages = UserProfileSettings.UseMP2,
                        Videotext = UserProfileSettings.UseTTX,
                        UseProfileForRecording = false,
                        Name = guide.Name.MakeValid(),
                        Profile = profile,
                    };
            }

            // Optionen ermitteln
            var streams = job.Streams;
            var sourceName = ServerRuntime.VCRServer.GetUniqueName( job.Source );

            // Report            
            return
                new EditJob
                {
                    UseProfileForRecording = !job.AutomaticResourceSelection,
                    DolbyDigital = streams.GetUsesDolbyAudio(),
                    AllLanguages = streams.GetUsesAllAudio(),
                    DVBSubtitles = streams.GetUsesSubtitles(),
                    Videotext = streams.GetUsesVideotext(),
                    RecordingDirectory = job.Directory,
                    Profile = job.Source.ProfileName,
                    Source = sourceName,
                    Name = job.Name,
                };
        }

        /// <summary>
        /// Erstellt einen passenden Auftrag für die persistente Ablage.
        /// </summary>
        /// <returns>Der zugehörige Auftrag.</returns>
        public VCRJob CreateJob()
        {
            // Forward
            return CreateJob( Guid.NewGuid() );
        }

        /// <summary>
        /// Erstellt einen passenden Auftrag für die persistente Ablage.
        /// </summary>
        /// <param name="jobIdentifier">Die eindeutige Kennung des Auftrags.</param>
        /// <returns>Der zugehörige Auftrag.</returns>
        public VCRJob CreateJob( Guid jobIdentifier )
        {
            // Create core
            var job =
                new VCRJob
                    {
                        AutomaticResourceSelection = !UseProfileForRecording,
                        Directory = RecordingDirectory,
                        UniqueID = jobIdentifier,
                        Name = Name,
                    };

            // Check source
            var profile = Profile;
            if (string.IsNullOrEmpty( profile ))
                return job;

            // Get the name of the source
            var sourceName = Source;
            if (string.IsNullOrEmpty( sourceName ))
            {
                // Create profile reference
                job.Source = new SourceSelection { ProfileName = profile };

                // Done
                return job;
            }

            // Locate the source
            job.Source = ServerRuntime.VCRServer.FindSource( profile, sourceName );
            if (job.Source == null)
                return job;

            // Configure streams
            job.Streams = new StreamSelection();

            // Set all - oder of audio settings is relevant, dolby MUST come last
            job.Streams.SetUsesAllAudio( AllLanguages );
            job.Streams.SetUsesDolbyAudio( DolbyDigital );
            job.Streams.SetUsesSubtitles( DVBSubtitles );
            job.Streams.SetUsesVideotext( Videotext );
            job.Streams.ProgramGuide = true;

            // Report
            return job;
        }
    }
}
