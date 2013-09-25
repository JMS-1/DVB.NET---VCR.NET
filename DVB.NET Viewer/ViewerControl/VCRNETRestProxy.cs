using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Führt Aufrufe an die <i>LIVE</i> Schnittstelle des <i>VCR.NET Recording Service</i> aus.
    /// </summary>
    public static class VCRNETRestProxy
    {
        /// <summary>
        /// Beschreibt den aktuellen Zustand.
        /// </summary>
        public class Status
        {
            /// <summary>
            /// Das aktuelle Ziel der Nutzdaten.
            /// </summary>
            public string target { get; set; }

            /// <summary>
            /// Die aktuelle Quelle.
            /// </summary>
            public string source { get; set; }

            /// <summary>
            /// Alle auf der aktuellen Quellgruppe verfügbaren Dienste.
            /// </summary>
            public Service[] services { get; set; }
        }

        /// <summary>
        /// Beschreibt einen Dienst.
        /// </summary>
        public class Service
        {
            /// <summary>
            /// Der Name des Dienstes.
            /// </summary>
            public string nameWithIndex { get; set; }

            /// <summary>
            /// Die eindeutige Kennung.
            /// </summary>
            public string source { get; set; }
        }

        /// <summary>
        /// Beschreibt eine mögliche Datenquelle.
        /// </summary>
        public class Source
        {
            /// <summary>
            /// Der Anzeigename der Quelle.
            /// </summary>
            public string nameWithProvider { get; set; }

            /// <summary>
            /// Gesetzt, wenn die Quelle verschlüsselt ist.
            /// </summary>
            public bool encrypted { get; set; }

            /// <summary>
            /// Die eindeutige Kennung der Quelle.
            /// </summary>
            public string source { get; set; }
        }

        /// <summary>
        /// Beschreibt eine aktuelle oder geplante Aktivität auf einem Gerät.
        /// </summary>
        public class Current
        {
            /// <summary>
            /// Das zugehörige Geräteprofil.
            /// </summary>
            public string device { get; set; }

            /// <summary>
            /// Der Name der Aktivität.
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// Die laufende Nummer des zugehörigen Datenstroms.
            /// </summary>
            public int streamIndex { get; set; }

            /// <summary>
            /// Für gerade aktive Aufzeichnungen gesetzt.
            /// </summary>
            public Guid? referenceId { get; set; }

            /// <summary>
            /// Gesetzt, wenn es sich um eine aktive Aufzeichnung oder Aufgabe handelt.
            /// </summary>
            public bool IsActive { get { return referenceId.HasValue; } }

            /// <summary>
            /// Die zugehörige Quelle, sofern bekannt.
            /// </summary>
            public string source { get; set; }

            /// <summary>
            /// Alle Dateien zu dieser Aufzeichnung.
            /// </summary>
            public string[] files { get; set; }

            /// <summary>
            /// Der Startzeitpunkt der Aufzeichnung.
            /// </summary>
            public DateTime? start { get; set; }

            /// <summary>
            /// Die Laufzeit der Aufzeichnung in Sekunden.
            /// </summary>
            public int duration { get; set; }

            /// <summary>
            /// Meldet den Endzeitpunkt der Aufzeichnung.
            /// </summary>
            public DateTime EndsAt { get { return start.Value.AddSeconds( duration ); } }

            /// <summary>
            /// Die Netzwerkadresse, an die gerade die Aufzeichnungsdaten versendet werden.
            /// </summary>
            public string streamTarget { get; set; }
        }

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
        /// Die Serialisierungskomponente.
        /// </summary>
        private static readonly JsonSerializer s_Converter = new JsonSerializer();

        /// <summary>
        /// Rekonstruiert eine <i>JSON</i> Antwort.
        /// </summary>
        /// <typeparam name="TResult">Die gewünschte Art des Ergebnisses.</typeparam>
        /// <param name="result">Der laufende Zugriff.</param>
        /// <returns>Das Ergebnis.</returns>
        private static TResult Deserialize<TResult>( IAsyncResult result )
        {
            // Attach to the request
            var request = (WebRequest) result.AsyncState;

            // Load the response
            using (var response = request.EndGetResponse( result ))
            using (var status = response.GetResponseStream())
            using (var reader = new StreamReader( status ))
                return (TResult) s_Converter.Deserialize( reader, typeof( TResult ) );
        }

        /// <summary>
        /// Führt eine Anfrage aus und wartet auf das Ergebnis.
        /// </summary>
        /// <typeparam name="TResult">Die Art des Ergebnisses.</typeparam>
        /// <param name="request">Die Anfrage.</param>
        /// <returns>Das gewünschte Ergebnis.</returns>
        private static TResult BeginRequestAndWait<TResult>( Action<Action<TResult>, Action<Exception>> request )
        {
            // Result
            var result = default( TResult );
            Exception exception = null;

            // Create synchronizer
            var sync = new object();

            // Protect
            Monitor.Enter( sync );

            // Start
            request( data => { result = data; lock (sync) Monitor.Pulse( sync ); }, error => { exception = error; lock (sync) Monitor.Pulse( sync ); } );

            // Wait if not failed in preparation
            if (exception == null)
                Monitor.Wait( sync );

            // Fire error
            if (exception != null)
                throw exception;

            // Report
            return result;
        }

        /// <summary>
        /// Sendet eine Zugriff ab, der als Ergebnis einen neuen Zustand liefert.
        /// </summary>
        /// <typeparam name="TResult">Die Art des Ergebnisses.</typeparam>
        /// <param name="uriFactory">Methode zur Erzeugung der Adresse des Web Dienstes.</param>
        /// <param name="method">Die gewünschte Aufrufmethode.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird im Fehlerfall aufgerufen.</param>
        /// <param name="data">An den Server zu übertragende Daten.</param>
        private static void BeginRequest<TResult>( Func<string> uriFactory, string method, Action<TResult> success, Action<Exception> failure, object data = null )
        {
            // Default
            if (failure == null)
                failure = e => { };

            // Be safe
            try
            {
                // Create request
                var uri = new Uri( uriFactory() );
                var request = WebRequest.Create( uri );

                // Configure
                request.UseDefaultCredentials = true;
                request.Method = method;

                // Process
                AsyncCallback responseProcessor =
                    args =>
                    {
                        // Process
                        try
                        {
                            // Generate response
                            var status = Deserialize<TResult>( args );

                            // Report
                            if (success != null)
                                success( status );
                        }
                        catch (Exception e)
                        {
                            // Forward
                            failure( e );
                        }
                    };

                // Check mode
                if (data == null)
                {
                    // No content at all
                    request.ContentLength = 0;

                    // Direct processing
                    request.BeginGetResponse( responseProcessor, request );
                }
                else
                {
                    // Finish
                    request.ContentType = "application/json";

                    // Wait for request stream
                    request.BeginGetRequestStream(
                        args =>
                        {
                            // Process
                            try
                            {
                                // Fill
                                using (var stream = request.EndGetRequestStream( args ))
                                using (var writer = new StreamWriter( stream ))
                                    s_Converter.Serialize( writer, data );

                                // Forward
                                request.BeginGetResponse( responseProcessor, request );
                            }
                            catch (Exception e)
                            {
                                // Forward
                                failure( e );
                            }
                        }, request );
                }
            }
            catch (Exception e)
            {
                // Fire
                failure( e );
            }
        }

        /// <summary>
        /// Aktiviert das Versenden von Daten.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Das zu verwendende Geräteprofil.</param>
        /// <param name="target">Die Adresse, an die alle Daten gesendet werden sollen.</param>
        public static Status ConnectSync( string endPoint, string profileName, string target )
        {
            // Use helper
            return BeginRequestAndWait<Status>( ( success, failure ) => Connect( endPoint, profileName, target, success, failure ) );
        }

        /// <summary>
        /// Aktiviert das Versenden von Daten.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Das zu verwendende Geräteprofil.</param>
        /// <param name="target">Die Adresse, an die alle Daten gesendet werden sollen.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird im Fehlerfall aufgerufen.</param>
        private static void Connect( string endPoint, string profileName, string target, Action<Status> success, Action<Exception> failure )
        {
            // Use helper
            BeginRequest( () => string.Format( "{0}{1}?target={2}", endPoint, profileName, target ), "POST", success, failure );
        }

        /// <summary>
        /// Beginnt einen neuen Zugriff zum Auslesen der Quellen.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="includeTV">Gesetzt, um alle Fernsehsender einzuschliessen.</param>
        /// <param name="includeRadio">Gesetzt, um alle Radiosender einzuschliessen.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird bei Fehlern aufgerufen.</param>
        private static void ReadSources( string endPoint, string profileName, bool includeTV, bool includeRadio, Action<Source[]> success, Action<Exception> failure )
        {
            // Forward
            BeginRequest( () => string.Format( "{0}{1}?tv={2}&radio={3}", endPoint, profileName, includeTV, includeRadio ), "GET", success, failure );
        }

        /// <summary>
        /// Ermittelt alle Quellen.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="includeTV">Gesetzt, um alle Fernsehsender einzuschliessen.</param>
        /// <param name="includeRadio">Gesetzt, um alle Radiosender einzuschliessen.</param>
        /// <returns>Die Liste der Quellen.</returns>
        public static Source[] ReadSourcesSync( string endPoint, string profileName, bool includeTV, bool includeRadio )
        {
            // Forward
            return BeginRequestAndWait<Source[]>( ( success, failure ) => ReadSources( endPoint, profileName, includeTV, includeRadio, success, failure ) );
        }

        /// <summary>
        /// Ermittelt einen aktuellen Zustand.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird bei Fehlern aufgerufen.</param>
        public static void GetStatus( string endPoint, string profileName, Action<Status> success, Action<Exception> failure )
        {
            // Use helper
            BeginRequest( () => string.Format( "{0}{1}", endPoint, profileName ), "GET", success, failure );
        }

        /// <summary>
        /// Ermittelt einen aktuellen Zustand.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <returns>Der aktuelle Zustand.</returns>
        public static Status GetStatusSync( string endPoint, string profileName )
        {
            // Use helper
            return BeginRequestAndWait<Status>( ( success, failure ) => GetStatus( endPoint, profileName, success, failure ) );
        }

        /// <summary>
        /// Beendet die Sitzung.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird bei Fehlern aufgerufen.</param>
        private static void Disconnect( string endPoint, string profileName, Action<Status> success, Action<Exception> failure )
        {
            // Use helper
            BeginRequest( () => string.Format( "{0}{1}", endPoint, profileName ), "DELETE", success, failure );
        }

        /// <summary>
        /// Beendet die Sitzung.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <returns>Der neue Zustand.</returns>
        public static Status DisconnectSync( string endPoint, string profileName )
        {
            // Use helper
            return BeginRequestAndWait<Status>( ( success, failure ) => Disconnect( endPoint, profileName, success, failure ) );
        }

        /// <summary>
        /// Wählt einen neuen Sender aus.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="source">Die eindeutige Kennung des Senders.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird bei Fehlern aufgerufen.</param>
        private static void Tune( string endPoint, string profileName, string source, Action<Status> success, Action<Exception> failure )
        {
            // Use helper
            BeginRequest( () => string.Format( "{0}{1}?source={2}", endPoint, profileName, source ), "PUT", success, failure );
        }

        /// <summary>
        /// Wählt einen neuen Sender aus.
        /// </summary>
        /// <param name="endPoint">Der zu verwendende <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="source">Die eindeutige Kennung des Senders.</param>
        /// <returns>Der neue Zustand.</returns>
        public static Status TuneSync( string endPoint, string profileName, string source )
        {
            // Use helper
            return BeginRequestAndWait<Status>( ( success, failure ) => Tune( endPoint, profileName, source, success, failure ) );
        }

        /// <summary>
        /// Ändert den Netzwerkversand.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Das betroffene Gerät.</param>
        /// <param name="source">Die Quelle.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="target">Die neue Zieladresse für den Netzwerkversand.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird bei Fehlern aufgerufen.</param>
        private static void SetStreamTarget( string endPoint, string profileName, string source, Guid scheduleIdentifier, string target, Action<object> success, Action<Exception> failure )
        {
            // Use helper
            BeginRequest<object>( () => string.Format( "{0}/plan/{1}?source={2}&scheduleIdentifier={3:N}&target={4}", endPoint, profileName, source, scheduleIdentifier, target ), "POST", success, failure );
        }

        /// <summary>
        /// Ändert den Netzwerkversand.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Das betroffene Gerät.</param>
        /// <param name="source">Die Quelle.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="target">Die neue Zieladresse für den Netzwerkversand.</param>
        public static void SetStreamTargetSync( string endPoint, string profileName, string source, Guid scheduleIdentifier, string target )
        {
            // Use helper
            BeginRequestAndWait<object>( ( success, failure ) => SetStreamTarget( endPoint, profileName, source, scheduleIdentifier, target, success, failure ) );
        }

        /// <summary>
        /// Ermittelt alle Geräteprofile.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird bei Fehlern aufgerufen.</param>
        private static void GetProfiles( string endPoint, Action<ProfileInfo[]> success, Action<Exception> failure )
        {
            // Use helper
            BeginRequest( () => string.Format( "{0}/profile", endPoint ), "GET", success, failure );
        }

        /// <summary>
        /// Ermittelt alle Geräteprofile.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <returns>Die gewünschte Liste.</returns>
        public static ProfileInfo[] GetProfilesSync( string endPoint )
        {
            // Use helper
            return BeginRequestAndWait<ProfileInfo[]>( ( success, failure ) => GetProfiles( endPoint, success, failure ) );
        }

        /// <summary>
        /// Ermittelt die aktuellen und anstehenden Aktivitäten aller Geräteprofile.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird bei Fehlern aufgerufen.</param>
        public static void GetActivities( string endPoint, Action<Current[]> success, Action<Exception> failure )
        {
            // Use helper
            BeginRequest( () => string.Format( "{0}/plan", endPoint ), "GET", success, failure );
        }

        /// <summary>
        /// Ermittelt die aktuellen und anstehenden Aktivitäten aller Geräteprofile.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <returns>Die gewünschte Liste.</returns>
        public static Current[] GetActivitiesSync( string endPoint )
        {
            // Use helper
            return BeginRequestAndWait<Current[]>( ( success, failure ) => GetActivities( endPoint, success, failure ) );
        }

        /// <summary>
        /// Meldet alle Aktivitäten eines bestimmten Geräteprofils.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Profils.</param>
        /// <returns>Die gewünschte Liste</returns>
        public static List<Current> GetActivitiesForProfile( string endPoint, string profileName )
        {
            // Forward
            return
                GetActivitiesSync( endPoint )
                    .Where( activity => activity.IsActive )
                    .Where( activity => activity.streamIndex >= 0 )
                    .Where( activity => ProfileManager.ProfileNameComparer.Equals( activity.device, profileName ) )
                    .ToList();
        }

        /// <summary>
        /// Ermittelt die erste Aktivität zu einem Geräteprofil.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Der Name des Profils.</param>
        /// <returns>Die erste Aktivität, sofern eine solche existiert.</returns>
        public static Current GetFirstActivityForProfile( string endPoint, string profileName )
        {
            // Forward
            return
                GetActivitiesSync( endPoint )
                    .FirstOrDefault( activity => ProfileManager.ProfileNameComparer.Equals( activity.device, profileName ) );
        }

        /// <summary>
        /// Legt eine neue Aufzeichnung an.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="job">Die Daten zum Auftrag.</param>
        /// <param name="schedule">Die Daten zur Aufzeichnung.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird bei Fehlern aufgerufen.</param>
        private static void CreateNew( string endPoint, Job job, Schedule schedule, Action<string> success, Action<Exception> failure )
        {
            // Use helper
            BeginRequest( () => string.Format( "{0}/edit", endPoint ), "POST", success, failure, new JobScheduleData { job = job, schedule = schedule } );
        }

        /// <summary>
        /// Legt eine neue Aufzeichnung an.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="job">Die Daten zum Auftrag.</param>
        /// <param name="schedule">Die Daten zur Aufzeichnung.</param>
        /// <returns>Die eindeutige Kennung der neuen Aufzeichnung.</returns>
        public static string CreateNewSync( string endPoint, Job job, Schedule schedule )
        {
            // Use helper
            return BeginRequestAndWait<string>( ( success, failure ) => CreateNew( endPoint, job, schedule, success, failure ) );
        }

        /// <summary>
        /// Fordert ein Stück einer Datei an.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        /// <param name="offset">Das erste Byte, das gesendet werden soll.</param>
        /// <param name="length">Die Anzahl der zu sendenden Bytes.</param>
        /// <param name="target">Der Rechner, an den die Daten zu senden sind.</param>
        /// <param name="port">Der Port, an dem die Daten empfangen werden sollen.</param>
        /// <param name="success">Wird im Erfolgsfall aufgerufen.</param>
        /// <param name="failure">Wird bei Fehlern aufgerufen.</param>
        public static void RequestFilePart( string endPoint, string path, long offset, int length, string target, ushort port, Action<long> success, Action<Exception> failure )
        {
            // Use helper
            BeginRequest( () => string.Format( "{0}/file?path={1}&offset={2}&length={3}&target={4}&port={5}", endPoint, path, offset, length, target, port ), "GET", success, failure );
        }
    }
}
