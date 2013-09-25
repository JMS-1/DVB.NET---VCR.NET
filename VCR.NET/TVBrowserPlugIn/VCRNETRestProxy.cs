using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;


namespace TVBrowserPlugIn
{
    /// <summary>
    /// Führt Aufrufe an die <i>LIVE</i> Schnittstelle des <i>VCR.NET Recording Service</i> aus.
    /// </summary>
    public static class VCRNETRestProxy
    {
        /// <summary>
        /// Meldet ein Geräteprofil.
        /// </summary>
        public class ProfileInfo
        {
            /// <summary>
            /// Der Name des Profils.
            /// </summary>
            public string name { get; set; }
        }

        /// <summary>
        /// Beschreibt eine Quelle.
        /// </summary>
        public class ProfileSource
        {
            /// <summary>
            /// Der Name der Quelle.
            /// </summary>
            public string nameWithProvider { get; set; }
        }

        /// <summary>
        /// Beschreibt einen Auftrag.
        /// </summary>
        public class Job
        {
            /// <summary>
            /// Der Name des Auftrags.
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// Das für die Auswahl der Quelle verwendete Gerät.
            /// </summary>
            public string device { get; set; }

            /// <summary>
            /// Die Quelle, von der aufgezeichnet werden soll.
            /// </summary>
            public string sourceName { get; set; }

            /// <summary>
            /// Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.
            /// </summary>
            public bool allLanguages { get; set; }

            /// <summary>
            /// Gesetzt, wenn auch die <i>Dolby Digital</i> Tonspur aufgezeichnet werden soll.
            /// </summary>
            public bool includeDolby { get; set; }

            /// <summary>
            /// Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.
            /// </summary>
            public bool withVideotext { get; set; }

            /// <summary>
            /// Gesetzt, wenn auch alle DVB Untertitel aufgezeichnet werden sollen.
            /// </summary>
            public bool withSubtitles { get; set; }
        }

        /// <summary>
        /// Beschreibt eine Aufzeichnung.
        /// </summary>
        public class Schedule
        {
            /// <summary>
            /// Der optionale Name der Aufzeichnung.
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// Der Zeitpunkt, an dem die erste Aufzeichnung stattfinden soll.
            /// </summary>
            public DateTime firstStart { get; set; }

            /// <summary>
            /// Das Datum der letzten Ausführung.
            /// </summary>
            public DateTime lastDay { get; set; }

            /// <summary>
            /// Die Dauer der Aufzeichnung.
            /// </summary>
            public int duration { get; set; }
        }

        /// <summary>
        /// Beschreibt die Daten einer neuen Aufzeichnung.
        /// </summary>
        private class JobScheduleData
        {
            /// <summary>
            /// Der zugehörige Auftrag.
            /// </summary>
            public Job job { get; set; }

            /// <summary>
            /// Die zugehörige Aufzeichnung.
            /// </summary>
            public Schedule schedule { get; set; }
        }

        /// <summary>
        /// Beschreibt eine bekannte Aufzeichnung.
        /// </summary>
        public class ScheduleInfo
        {
            /// <summary>
            /// Der Startzeitpunkt der Aufzeichnung.
            /// </summary>
            public DateTime start { get; set; }

            /// <summary>
            /// Die Tage, an denen die Aufzeichnung wiederholt werden soll.
            /// </summary>
            public int repeatPattern { get; set; }

            /// <summary>
            /// Die Dauer der Aufzeichnung.
            /// </summary>
            public int duration { get; set; }

            /// <summary>
            /// Der eindeutige Name der Aufzeichnung.
            /// </summary>
            public string id { get; set; }
        }

        /// <summary>
        /// Beschreibt einen bekannten Auftrag.
        /// </summary>
        public class JobInfo
        {
            /// <summary>
            /// Die einzelnen Aufzeichnungen des Auftrags.
            /// </summary>
            public ScheduleInfo[] schedules { get; set; }

            /// <summary>
            /// Der Name des Geräteprofils.
            /// </summary>
            public string device { get; set; }

            /// <summary>
            /// Der Name des Quelle.
            /// </summary>
            public string source { get; set; }
        }

        /// <summary>
        /// Die Serialisierungskomponente.
        /// </summary>
        private static readonly JsonSerializer s_Converter = new JsonSerializer();

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <typeparam name="TResult">Die Art des erwarteten Ergebnisses.</typeparam>
        /// <param name="uri">Die aufzurufende Adresse.</param>
        /// <param name="method">Die zu verwendende Protokollmethode.</param>
        /// <param name="postData">Mit zu übertragende Daten.</param>
        /// <returns>Das Ergebnis des Aufrufs.</returns>
        private static TResult CallServer<TResult>( string uri, string method = "GET", object postData = null )
        {
            // Create request
            var request = WebRequest.Create( uri );

            // Configure
            request.UseDefaultCredentials = true;
            request.Method = method;

            // Send post data
            if (postData != null)
            {
                // Set the type
                request.ContentType = "application/json";

                // Fill in
                using (var stream = request.GetRequestStream())
                using (var writer = new StreamWriter( stream ))
                    s_Converter.Serialize( writer, postData );
            }

            // Load the response
            var response = request.GetResponse();
            try
            {
                // Process
                using (var status = response.GetResponseStream())
                using (var reader = new StreamReader( status ))
                    return (TResult) s_Converter.Deserialize( reader, typeof( TResult ) );
            }
            finally
            {
                // Done
                response.Close();
            }
        }

        /// <summary>
        /// Ermittelt alle Geräteprofile.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <returns>Die gewünschte Liste.</returns>
        public static ProfileInfo[] GetProfiles( string endPoint )
        {
            // Use helper
            return CallServer<ProfileInfo[]>( string.Format( "{0}/profile", endPoint ) );
        }

        /// <summary>
        /// Ermittelt alle Quellen zu einem Geräteprofil.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <returns>Alle Quellen des Profils.</returns>
        public static ProfileSource[] GetSources( string endPoint, string profileName )
        {
            // Use helper
            return CallServer<ProfileSource[]>( string.Format( "{0}/profile/{1}", endPoint, profileName ) );
        }

        /// <summary>
        /// Ermittelt alle Aufträge.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <returns>Die Liste der Aufträge.</returns>
        public static JobInfo[] GetJobs( string endPoint )
        {
            // Use helper
            return CallServer<JobInfo[]>( string.Format( "{0}/info?jobs", endPoint ) );
        }

        /// <summary>
        /// Entfernt eine Aufzeichnung.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="scheduleIdentifier">Der eindeutige Name der Aufzeichnung.</param>
        public static void Delete( string endPoint, string scheduleIdentifier )
        {
            // Use helper
            CallServer<JobInfo[]>( string.Format( "{0}/edit/{1}", endPoint, scheduleIdentifier ), "DELETE" );
        }

        /// <summary>
        /// Legt eine neue Aufzeichnung an.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="job">Die Daten zum Auftrag.</param>
        /// <param name="schedule">Die Daten zur Aufzeichnung.</param>
        /// <returns>Die eindeutige Kennung der neuen Aufzeichnung.</returns>
        public static string CreateNew( string endPoint, Job job, Schedule schedule )
        {
            // Use helper
            return CallServer<string>( string.Format( "{0}/edit", endPoint ), "POST", new JobScheduleData { job = job, schedule = schedule } );
        }
    }
}
