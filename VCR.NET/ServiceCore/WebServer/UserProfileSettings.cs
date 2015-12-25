using System.Collections.Specialized;
using System.Web;
using System.Web.Profile;


namespace JMS.DVBVCR.RecordingService.WebServer
{
    /// <summary>
    /// Kapselt die benutzerspezifischen Einstellungen.
    /// </summary>
    public static class UserProfileSettings
    {
        /// <summary>
        /// Meldet die Liste der zuletzt verwendenden Sendern.
        /// </summary>
        public static StringCollection RecentChannels => (StringCollection) Profile["RecentChannels"];

        /// <summary>
        /// Meldet die maximal erlaubte Anzahl von Sendern in der Liste zuletzt verwendeter Sender.
        /// </summary>
        public static int MaxRecentChannels
        {
            get { return (int) Profile["MaxRecentChannels"]; }
            set { Profile["MaxRecentChannels"] = value; }
        }

        /// <summary>
        /// Meldet das aktuelle Benutzerprofil.
        /// </summary>
        private static ProfileBase Profile => HttpContext.Current.Profile;

        /// <summary>
        /// Begrenzt die Anzahl der zuletzt verwendeten Quellen auf die Höchstgrenze.
        /// </summary>
        /// <returns>Gesetzt, wenn eine Reduktion vorgenommen wurde.</returns>
        private static bool LimitChannels()
        {
            // Check delta
            int delta = (RecentChannels.Count - MaxRecentChannels);
            if (delta < 1)
                return false;

            // Cut off
            while (delta-- > 0)
                RecentChannels.RemoveAt( MaxRecentChannels );

            // Did it
            return true;
        }

        /// <summary>
        /// Fügt eine Quelle zur Liste der zuletzt verwendeten Quellen hinzu. Ist der
        /// Eintrag bereits vorhanden, so wird er an den Anfang der Liste verschoben.
        /// </summary>
        /// <param name="station">Der Name der Quelle.</param>
        public static void AddRecentChannel( string station )
        {
            // Not allowed
            if (string.IsNullOrEmpty( station ))
                return;

            // Remove first
            RecentChannels.Remove( station );

            // Append to head
            RecentChannels.Insert( 0, station );

            // Correct and save
            Update();
        }

        /// <summary>
        /// Aktualisiert das Benutzerprofil auf der Festplatte.
        /// </summary>
        public static void Update()
        {
            // Correct
            LimitChannels();

            // Store
            Profile.Save();
        }

        /// <summary>
        /// Prüft die Voreinstellung für den Schlafzustand nach einem Abbruch.
        /// </summary>
        public static bool NoHibernateOnAbort
        {
            get { return (bool) Profile["NoHibernateOnAbort"]; }
            set { Profile["NoHibernateOnAbort"] = value; }
        }

        /// <summary>
        /// Gesetzt, wen verschlüsselte Sender berücksichtigt werden sollen.
        /// </summary>
        public static bool PayTV
        {
            get { return (bool) Profile["PayTV"]; }
            set { Profile["PayTV"] = value; }
        }

        /// <summary>
        /// Meldet, ob Fernsehsender berücksichtigt werden sollen.
        /// </summary>
        public static bool Radio
        {
            get { return (bool) Profile["Radio"]; }
            set { Profile["Radio"] = value; }
        }

        /// <summary>
        /// Meldet, ob unverschlüsselte Sender berücksichtigt werden sollen.
        /// </summary>
        public static bool FreeTV
        {
            get { return (bool) Profile["FreeTV"]; }
            set { Profile["FreeTV"] = value; }
        }

        /// <summary>
        /// Meldet, ob Radiosender berücksichtigt werden sollen.
        /// </summary>
        public static bool Television
        {
            get { return (bool) Profile["Television"]; }
            set { Profile["Television"] = value; }
        }

        /// <summary>
        /// Meldet, ob nach einer Programmierung über die Programmzeitschrift zu dieser zurück gekehrt werden soll.
        /// </summary>
        public static bool BackToEPG
        {
            get { return (bool) Profile["BackToEPG"]; }
            set { Profile["BackToEPG"] = value; }
        }

        /// <summary>
        /// Meldet, ob der Videotext mit aufgezeichnet werden soll.
        /// </summary>
        public static bool UseTTX
        {
            get { return (bool) Profile["UseTTX"]; }
            set { Profile["UseTTX"] = value; }
        }

        /// <summary>
        /// Meldet, ob die <i>Dolby Digital</i> Tonspur mit aufgezeichnet werden soll.
        /// </summary>
        public static bool UseAC3
        {
            get { return (bool) Profile["UseAC3"]; }
            set { Profile["UseAC3"] = value; }
        }

        /// <summary>
        /// Meldet, ob alle Sprachen aufzeichnet werden sollen.
        /// </summary>
        public static bool UseMP2
        {
            get { return (bool) Profile["UseMP2"]; }
            set { Profile["UseMP2"] = value; }
        }

        /// <summary>
        /// Meldet, ob auch DVB Untertitel aufgezeichnet werden sollen.
        /// </summary>
        public static bool UseSubTitles
        {
            get { return (bool) Profile["UseSubTitles"]; }
            set { Profile["UseSubTitles"] = value; }
        }

        /// <summary>
        /// Meldet die Nachlaufzeit bei Programmierung über die Programmzeitschrift.
        /// </summary>
        /// <seealso cref="EPGPreTime"/>
        public static int EPGPostTime
        {
            get { return (int) Profile["EPGPostTime"]; }
            set { Profile["EPGPostTime"] = value; }
        }

        /// <summary>
        /// Meldet die Vorlaufzeit bei Programmierung über die Programmzeitschrift.
        /// </summary>
        /// <seealso cref="EPGPostTime"/>
        public static int EPGPreTime
        {
            get { return (int) Profile["EPGPreTime"]; }
            set { Profile["EPGPreTime"] = value; }
        }

        /// <summary>
        /// Meldet die Anzahl von Einträge der Programmzeitschrift pro Seite.
        /// </summary>
        public static int EPGEntries
        {
            get { return (int) Profile["EPGEntries"]; }
            set { Profile["EPGEntries"] = value; }
        }

        /// <summary>
        /// Meldet die Anzahl von Tagen, die im Aufzeichnungsplan pro Seite angezeigt werden sollen.
        /// </summary>
        public static int DaysToShow
        {
            get { return (int) Profile["DaysToShow"]; }
            set { Profile["DaysToShow"] = value; }
        }

        /// <summary>
        /// Meldet oder ändert die gespeicherten Suchen der Programmzeichschrift.
        /// </summary>
        public static string GuideFavorites
        {
            get { return (string) Profile["GuideFavorites"]; }
            set { Profile["GuideFavorites"] = value; }
        }
    }
}