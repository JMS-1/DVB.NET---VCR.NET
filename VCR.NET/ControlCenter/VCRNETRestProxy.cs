using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;


namespace VCRControlCenter
{
    /// <summary>
    /// Führt Aufrufe an die <i>LIVE</i> Schnittstelle des <i>VCR.NET Recording Service</i> aus.
    /// </summary>
    public static class VCRNETRestProxy
    {
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
            /// Der Startzeitpunkt in ISO Notation.
            /// </summary>
            public DateTime? start { get; set; }

            /// <summary>
            /// Die laufende Nummer des zugehörigen Datenstroms.
            /// </summary>
            public int streamIndex { get; set; }

            /// <summary>
            /// Das aktuelle Ziel des Netzwerkversands.
            /// </summary>
            public string streamTarget { get; set; }

            /// <summary>
            /// Für gerade aktive Aufzeichnungen gesetzt.
            /// </summary>
            public Guid? referenceId { get; set; }

            /// <summary>
            /// Gesetzt, wenn es sich um eine aktive Aufzeichnung oder Aufgabe handelt.
            /// </summary>
            public bool IsActive { get { return referenceId.HasValue; } }

            /// <summary>
            /// Der zum betrachten reservierte TCP/IP Port.
            /// </summary>
            public ushort ReservedPort { get; set; }

            /// <summary>
            /// Die zugehörige Quelle, sofern bekannt.
            /// </summary>
            public string source { get; set; }
        }

        /// <summary>
        /// Meldet Versionsinformationen.
        /// </summary>
        public class ServiceInformation
        {
            /// <summary>
            /// Die Zeichenkette mit den Versionsdaten.
            /// </summary>
            public string version { get; set; }

            /// <summary>
            /// Gesetzt, wenn ein Schlafzustand nicht ausgelöst wurde, weil noch ein Anwender aktiv ist.
            /// </summary>
            public bool hibernationPending { get; set; }

            /// <summary>
            /// Gesetzt, wenn noch Erweiterungen aktiv sind.
            /// </summary>
            public bool extensionsRunning { get; set; }

            /// <summary>
            /// Die Namen aller Geräteprofile.
            /// </summary>
            public string[] profileNames { get; set; }

            /// <summary>
            /// Die minimale Verweildauer (in Minuten) im Schlafzustand.
            /// </summary>
            public uint sleepMinimum { get; set; }
        }

        /// <summary>
        /// Die Serialisierungskomponente.
        /// </summary>
        private static readonly JsonSerializer s_Deserializer = new JsonSerializer();

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <typeparam name="TResult">Die Art des erwarteten Ergebnisses.</typeparam>
        /// <param name="uri">Die aufzurufende Adresse.</param>
        /// <param name="method">Die Methode zum Aufruf.</param>
        /// <returns>Das Ergebnis des Aufrufs.</returns>
        private static TResult CallServer<TResult>( string uri, string method = "GET" )
        {
            // Be safe
            try
            {
                // Create request
                var request = WebRequest.Create( uri );

                // Configure
                request.UseDefaultCredentials = true;
                request.ContentLength = 0;
                request.Timeout = 10000;
                request.Method = method;

                // Load the response
                var response = request.GetResponse();
                try
                {
                    // Process
                    using (var status = response.GetResponseStream())
                    using (var reader = new StreamReader( status ))
                        return (TResult) s_Deserializer.Deserialize( reader, typeof( TResult ) );
                }
                finally
                {
                    // Done
                    response.Close();
                }
            }
            catch (Exception e)
            {
                // See if this is a timeout
                var webException = e as WebException;
                if (webException != null)
                    if (webException.Status == WebExceptionStatus.Timeout)
                        VCRNETControl.Log( "Timeout calling {1} ({0})", method, uri );

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Ermittelt die aktuellen und anstehenden Aktivitäten aller Geräteprofile.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <returns>Die gewünschte Liste.</returns>
        public static Current[] GetActivities( string endPoint )
        {
            // Use helper
            return CallServer<Current[]>( string.Format( "{0}/plan", endPoint ) );
        }

        /// <summary>
        /// Ermittelt die aktuellen Versionsinformationen.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <returns>Die gewünschten Versionsinformationen.</returns>
        public static ServiceInformation GetInformation( string endPoint )
        {
            // Use helper
            return CallServer<ServiceInformation>( string.Format( "{0}/info", endPoint ) );
        }

        /// <summary>
        /// Ändert den Netzwerkversand.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        /// <param name="profileName">Das betroffene Gerät.</param>
        /// <param name="source">Die Quelle.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="target">Die neue Zieladresse für den Netzwerkversand.</param>
        public static void SetStreamTarget( string endPoint, string profileName, string source, Guid scheduleIdentifier, string target )
        {
            // Use helper
            CallServer<object>( string.Format( "{0}/plan/{1}?source={2}&scheduleIdentifier={3:N}&target={4}", endPoint, profileName, source, scheduleIdentifier, target ), "POST" );
        }

        /// <summary>
        /// Vergißt, dass eigentlich ein Schlafzustand ausgelöst werden sollte.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        public static void ResetPendingHibernation( string endPoint )
        {
            // Use helper
            CallServer<object>( string.Format( "{0}/hibernate?reset", endPoint ), "POST" );
        }

        /// <summary>
        /// Versucht, den Übergang in den Schlafzustand auszulösen.
        /// </summary>
        /// <param name="endPoint">Die Verbindung zum <i>VCR.NET Recording Service</i>.</param>
        public static void TryHibernate( string endPoint )
        {
            // Use helper
            CallServer<object>( string.Format( "{0}/hibernate?hibernate", endPoint ), "POST" );
        }
    }
}
