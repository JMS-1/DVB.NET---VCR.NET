using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt die benutezrdefinierten Einstellungen.
    /// </summary>
    [DataContract]
    public class UserProfile
    {
        /// <summary>
        /// Die Anzahl von Tagen, die im Aufzeichnungsplan angezeigt werden sollen.
        /// </summary>
        [DataMember( Name = "planDays" )]
        public int DaysToShowInPlan { get; set; }

        /// <summary>
        /// Die Liste der bisher verwendeten Quellen.
        /// </summary>
        [DataMember( Name = "recentSources" )]
        public string[] RecentSources { get; set; }

        /// <summary>
        /// Die bevorzugte Auswahl der Art einer Quelle.
        /// </summary>
        [DataMember( Name = "typeFilter" )]
        public string DefaultSourceTypeSelector { get; set; }

        /// <summary>
        /// Die bevorzugte Auswahl Verschlüsselung einer Quelle.
        /// </summary>
        [DataMember( Name = "encryptionFilter" )]
        public string DefaultSourceEncryptionSelector { get; set; }

        /// <summary>
        /// Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen.
        /// </summary>
        [DataMember( Name = "languages" )]
        public bool PreferAllLanguages { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch die <i>Dolby Digital</i> Tonspur aufgezeichnet werden soll.
        /// </summary>
        [DataMember( Name = "dolby" )]
        public bool PreferDolby { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.
        /// </summary>
        [DataMember( Name = "videotext" )]
        public bool PreferVideotext { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch Untertitel aufgezeichnet werden sollen.
        /// </summary>
        [DataMember( Name = "subtitles" )]
        public bool PreferSubtitles { get; set; }

        /// <summary>
        /// Gesetzt, wenn nach dem Anlegen einer neuen Aufzeichnung aus der Programmzeitschrift
        /// heraus zur Programmzeitschrift zurück gekehrt werden soll - und nicht der aktualisierte
        /// Aufzeichnungsplan zur Anzeige kommt.
        /// </summary>
        [DataMember( Name = "backToGuide" )]
        public bool BackToGuideAfterAdd { get; set; }

        /// <summary>
        /// Die Anzahl der Zeilen auf einer Seite der Programmzeitschrift.
        /// </summary>
        [DataMember( Name = "guideRows" )]
        public int RowsInGuide { get; set; }

        /// <summary>
        /// Die Anzahl von Minuten, die eine Aufzeichnung vorzeitig beginnt, wenn sie über
        /// die Programmzeitschrift angelegt wird.
        /// </summary>
        [DataMember( Name = "guideAheadStart" )]
        public int GuideStartEarly { get; set; }

        /// <summary>
        /// Die Anzahl von Minuten, die eine Aufzeichnung verspätet endet, wenn sie über
        /// die Programmzeitschrift angelegt wird.
        /// </summary>
        [DataMember( Name = "guideBeyondEnd" )]
        public int GuideEndLate { get; set; }

        /// <summary>
        /// Die maximale Anzahl von Einträgen in der Liste der zuletzt verwendeten Quellen.
        /// </summary>
        [DataMember( Name = "recentSourceLimit" )]
        public int RowsInRecentSources { get; set; }

        /// <summary>
        /// Gesetzt, um beim Abbruch einer Aufzeichnung den Übergang in den Schlafzustand zu unterbinden.
        /// </summary>
        [DataMember( Name = "suppressHibernate" )]
        public bool NoHibernateOnAbort { get; set; }

        /// <summary>
        /// Meldet oder ändert die gespeicherten Suchen der Programmzeitschrift.
        /// </summary>
        [DataMember( Name = "guideSearches" )]
        public string GuideFavorites { get; set; }

        /// <summary>
        /// Erstellt die Informationen des aktuellen Anwenders.
        /// </summary>
        /// <returns>Die gewünschten Einstellungen.</returns>
        public static UserProfile Create()
        {
            // Report
            return
                new UserProfile
                {
                    DefaultSourceTypeSelector = UserProfileSettings.Radio ? (UserProfileSettings.Television ? "RT" : "R") : (UserProfileSettings.Television ? "T" : ""),
                    DefaultSourceEncryptionSelector = UserProfileSettings.FreeTV ? (UserProfileSettings.PayTV ? "FP" : "F") : (UserProfileSettings.PayTV ? "P" : ""),
                    RecentSources = UserProfileSettings.RecentChannels.Cast<string>().OrderBy( name => name ).ToArray(),
                    RowsInRecentSources = UserProfileSettings.MaxRecentChannels,
                    NoHibernateOnAbort = UserProfileSettings.NoHibernateOnAbort,
                    GuideFavorites = UserProfileSettings.GuideFavorites,
                    BackToGuideAfterAdd = UserProfileSettings.BackToEPG,
                    PreferSubtitles = UserProfileSettings.UseSubTitles,
                    DaysToShowInPlan = UserProfileSettings.DaysToShow,
                    GuideStartEarly = UserProfileSettings.EPGPreTime,
                    PreferAllLanguages = UserProfileSettings.UseMP2,
                    GuideEndLate = UserProfileSettings.EPGPostTime,
                    PreferVideotext = UserProfileSettings.UseTTX,
                    RowsInGuide = UserProfileSettings.EPGEntries,
                    PreferDolby = UserProfileSettings.UseAC3,
                };
        }

        /// <summary>
        /// Aktualisiert die Einstellungen des Anwenders.
        /// </summary>
        public void Update()
        {
            // Direct copy
            UserProfileSettings.NoHibernateOnAbort = NoHibernateOnAbort;
            UserProfileSettings.BackToEPG = BackToGuideAfterAdd;
            UserProfileSettings.UseSubTitles = PreferSubtitles;
            UserProfileSettings.UseMP2 = PreferAllLanguages;
            UserProfileSettings.UseTTX = PreferVideotext;
            UserProfileSettings.UseAC3 = PreferDolby;

            // A bit more work on flag groups
            switch (DefaultSourceTypeSelector ?? string.Empty)
            {
                case "R": UserProfileSettings.Radio = true; UserProfileSettings.Television = false; break;
                case "T": UserProfileSettings.Radio = false; UserProfileSettings.Television = true; break;
                case "RT": UserProfileSettings.Radio = true; UserProfileSettings.Television = true; break;
            }
            switch (DefaultSourceEncryptionSelector ?? string.Empty)
            {
                case "F": UserProfileSettings.FreeTV = true; UserProfileSettings.PayTV = false; break;
                case "P": UserProfileSettings.FreeTV = false; UserProfileSettings.PayTV = true; break;
                case "FP": UserProfileSettings.FreeTV = true; UserProfileSettings.PayTV = true; break;
            }

            // Numbers are copied after check
            if (DaysToShowInPlan >= 1)
                if (DaysToShowInPlan <= 50)
                    UserProfileSettings.DaysToShow = DaysToShowInPlan;
            if (RowsInRecentSources >= 1)
                if (RowsInRecentSources <= 50)
                    UserProfileSettings.MaxRecentChannels = RowsInRecentSources;
            if (GuideStartEarly >= 0)
                if (GuideStartEarly <= 240)
                    UserProfileSettings.EPGPreTime = GuideStartEarly;
            if (GuideEndLate >= 0)
                if (GuideEndLate <= 240)
                    UserProfileSettings.EPGPostTime = GuideEndLate;
            if (RowsInGuide >= 10)
                if (RowsInGuide <= 100)
                    UserProfileSettings.EPGEntries = RowsInGuide;

            // Store
            UserProfileSettings.Update();
        }
    }
}
