using System;
using System.Collections.Generic;
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
    [XmlType( "VCRSchedule" )]
    public class Schedule_Pre39
    {
        /// <summary>
        /// Zeigt an, dass kein Enddatum für diese Aufzeichnung definiert wurde.
        /// </summary>
        public static readonly DateTime NoEndIndicator = new DateTime( 2999, 12, 31 );

        /// <summary>
        /// Zeigt an, dass <see cref="FirstStart"/> als erster Zeitpunkt der Aufzeichnung verwendet werden soll.
        /// </summary>
        private static readonly DateTime NoStartNotSet = new DateTime( 2003, 1, 1, 0, 0, 0, DateTimeKind.Utc );

        /// <summary>
        /// Für sich wiederholende Aufzeichnungen das früheste Datum, an dem die
        /// nächste Aufzeichnung stattfinden darf.
        /// </summary>
        public DateTime NoStartBefore { get; set; }

        /// <summary>
        /// Datum und Uhrzeit, wann die Aufzeichnung das erste Mal ausgeführt werden soll.
        /// </summary>
        public DateTime FirstStart { get; set; }

        /// <summary>
        /// Datum, an dem die Aufzeichnung das letzte Mal ausgeführt werden soll.
        /// </summary>
        /// <remarks>
        /// Das Datum wird ignoriert, wenn die Aufzeichnung keine Wiederholungen verwendet.
        /// </remarks>
        public DateTime LastDay { get; set; }

        /// <summary>
        /// Eindeutige Kennung der Aufzeichnung.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string UniqueID { get; set; }

        /// <summary>
        /// Optionaler Sender zu Aufzeichnung.
        /// </summary>
        /// <remarks>
        /// Ist der Sender nicht gesetzt, wird die Einstellung des zugehörigen
        /// Auftrags verwendet.
        /// </remarks>
        [XmlElement( IsNullable = true )]
        public string Station { get; set; }

        /// <summary>
        /// Gewünschter NVOD Dienst zur <see cref="Station"/>.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist.
        /// In diesem Fall wird der entsprechende Wert des Auftrags verwendet.
        /// </remarks>
        [XmlElement( IsNullable = true )]
        public string NVOD { get; set; }

        /// <summary>
        /// Dauer der Aufzeichung in Minuten.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Optionaler Name der Aufzeichnung.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string Name { get; set; }

        /// <summary>
        /// Maske zur Definition des Wiederholungsinterfalls.
        /// </summary>
        /// <remarks>
        /// Jedes Bit entspricht einem Wochentag aus <see cref="VCRDay"/>.
        /// </remarks>
        public int Days { get; set; }

        /// <summary>
        /// Aktiviert die Aufzeichnung der AC3 (Dolby Digital) Tonspur.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist.
        /// In diesem Fall wird der entsprechende Wert des Auftrags verwendet.
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
        /// Aufzeichnung aller MP2 Tonspuren aktivieren.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist.
        /// In diesem Fall wird der entsprechende Wert des Auftrags verwendet.
        /// </remarks>
        public bool AllLanguages { get; set; }

        /// <summary>
        /// Aufzeichnung von Videotext Daten aktivieren.
        /// </summary>
        /// <remarks>
        /// Diese Eigenschaft wird ignoriert, wenn <see cref="Station"/> nicht gesetzt ist.
        /// In diesem Fall wird der entsprechende Wert des Auftrags verwendet.
        /// </remarks>
        public bool WithTeleText { get; set; }

        /// <summary>
        /// Die volle Bezeichnung der Quelle.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Alle Ausnahmeregeln für eine sich wiederholende Aufzeichnung.
        /// </summary>
        [XmlElement( "Exception" )]
        public readonly List<VCRScheduleException> Exceptions = new List<VCRScheduleException>();

        /// <summary>
        /// Erzeugt eine neue Instanz einer Aufzeichnung.
        /// </summary>
        public Schedule_Pre39()
        {
            // Setup fields
            NoStartBefore = NoStartNotSet;
        }

        /// <summary>
        /// Wandelt diese Aufzeichnung in die aktuelle Darstellung um.
        /// </summary>
        /// <param name="job">Der zugehörige Auftrag.</param>
        /// <returns>Die aktuelle Darstellung.</returns>
        internal VCRSchedule ToSchedule( Job_Pre39 job )
        {
            // Create
            VCRSchedule schedule = new VCRSchedule();

            // Conditional - we are now using Nullable<T>
            if (0 != Days)
                schedule.Days = (VCRDay) Days;
            if (NoEndIndicator != LastDay)
                schedule.LastDay = LastDay;
            if (NoStartNotSet != NoStartBefore)
                schedule.NoStartBefore = NoStartBefore;

            // Copy standard settings
            schedule.UniqueID = UniqueID.ToUniqueIdentifier();
            schedule.Exceptions.AddRange( Exceptions );
            schedule.FirstStart = FirstStart;
            schedule.Duration = Duration;
            schedule.Name = Name;

            // See if we have to convert the station
            if (!string.IsNullOrEmpty( Station ))
            {
                // Convert station
                schedule.Source = Station.ToSelection( job.Profile );

                // Convert streams
                schedule.Streams = CreateStreams( Station, AllLanguages, AC3Recording, WithSubtitles, WithTeleText );
            }

            // Report
            return schedule;
        }

        /// <summary>
        /// Erzeugt die Auswahl der aufzuzeichnenden Datenspuren.
        /// </summary>
        /// <param name="station">Der Name des Senders, in dem möglicherweise eine
        /// explizite Sprachauswahl verborgen ist.</param>
        /// <param name="allAudio">Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.</param>
        /// <param name="withAC3">Gesetzt, wenn auch AC3 Tonspuren aufgezeichnet werden sollen.</param>
        /// <param name="withSub">Gesetzt, wenn auch DVB Untertitel aufgezeichnet werden sollen.</param>
        /// <param name="withTTX">Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.</param>
        /// <returns>Die Beschreibung der Datenströme.</returns>
        public static StreamSelection CreateStreams( string station, bool allAudio, bool withAC3, bool withSub, bool withTTX )
        {
            // Create new
            StreamSelection streams = new StreamSelection { Videotext = withTTX, ProgramGuide = true };

            // Language dependencies
            streams.AC3Tracks.LanguageMode = withAC3 ? (allAudio ? LanguageModes.All : LanguageModes.Primary) : LanguageModes.Selection;
            streams.SubTitles.LanguageMode = withSub ? LanguageModes.All : LanguageModes.Selection;
            streams.MP2Tracks.LanguageMode = allAudio ? LanguageModes.All : LanguageModes.Primary;

            // No more checks necessary
            if (allAudio)
                return streams;

            // Get the audio
            int audio = station.IndexOf( "] - " );
            if (audio < 0)
                return streams;

            // Cut off
            string audioName = station.Substring( audio + 4 );

            // Find the end
            int end = audioName.LastIndexOf( '[' );
            if (end < 0)
                return streams;

            // Get the name
            audioName = audioName.Substring( 0, end ).Trim();

            // Use it
            streams.MP2Tracks.LanguageMode = LanguageModes.Selection;
            streams.MP2Tracks.Languages.Add( audioName );

            // Report
            return streams;
        }
    }

    /// <summary>
    /// Hilfsklasse zur Aktualisierung von Datenstrukturen.
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// Wandelt eine Senderbezeichnung in eine eindeutige Quellauswahl um.
        /// </summary>
        /// <param name="station">Der Name des Senders.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <returns>Die entsprechende Auswahl.</returns>
        public static SourceSelection ToSelection( this string station, string profile )
        {
            // See if there is audio appended
            int audio = station.IndexOf( "] - " );
            if (audio >= 0)
                station = station.Substring( 0, audio + 1 );

            // Attach to profile
            if (string.IsNullOrEmpty( profile ))
            {
                // Attach to the default
                Profile defaultProfile = VCRProfiles.DefaultProfile;

                // Use it
                if (null != defaultProfile)
                    profile = defaultProfile.Name;
            }

            // See if we have a profile
            if (!string.IsNullOrEmpty( profile ))
            {
                // Ask helper
                SourceSelection source = VCRProfiles.FindSource( profile, station );
                if (null != source)
                    return source;
            }

            // Create placeholder
            return new SourceSelection { DisplayName = station, ProfileName = profile };
        }

        /// <summary>
        /// Versucht, eine Zeichenkette in eine eindeutige Kennung zu wandeln.
        /// </summary>
        /// <param name="guid">Die eindeutige Kennung in der Textdarstellung.</param>
        /// <returns>Die eindeutige Kennung oder <i>null</i>, wenn eine Umwandlung nicht 
        /// möglich war.</returns>
        public static Guid? ToUniqueIdentifier( this string guid )
        {
            // Save parse
            Guid identifier;
            if (Guid.TryParse( guid, out identifier ))
                return identifier;
            else
                return null;
        }
    }
}
