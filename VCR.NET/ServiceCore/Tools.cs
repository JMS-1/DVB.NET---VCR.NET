using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using JMS.DVB;
using Microsoft.Win32;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Allgemeine Hilfsfunktionen.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Die Zeitbasis für alle Zeitangaben.
        /// </summary>
        public static long UnixTimeBias = new DateTime( 1970, 1, 1 ).Ticks;

        /// <summary>
        /// Umrechnungsfaktor für Zeitangaben.
        /// </summary>
        public static int UnixTimeFactor = 10000;

        /// <summary>
        /// Die Methode, mit der ein Schlafzustand ausgelöst werden soll.
        /// </summary>
        public static Func<bool, bool> SendSystemToSleep = useSuspend => Application.SetSuspendState( useSuspend ? PowerState.Suspend : PowerState.Hibernate, false, false );

        /// <summary>
        /// Der Pfad zur Anwendung.
        /// </summary>
        /// <remarks>
        /// Dieser Pfad wird verwendet, um abhängige Dateien relativ zum
        /// VCR.NET Recording Service zu finden. Er kann in Testprogrammen
        /// verändert werden.
        /// </remarks>
        public static string ExecutablePath { get { return RunTimeLoader.ExecutablePath.FullName; } }

        /// <summary>
        /// Die Konfiguration zur aktuellen Anwendung.
        /// </summary>
        public static volatile Configuration m_ApplicationConfiguration;

        /// <summary>
        /// Die Konfiguration zur aktuellen Anwendung.
        /// </summary>
        public static Configuration ApplicationConfiguration { get { return m_ApplicationConfiguration; } }

        /// <summary>
        /// Synchronisiert Einträge in das spezielle Protokoll.
        /// </summary>
        public static object m_LoggingLock = new object();

        /// <summary>
        /// Wird gesetzt, wenn der Start mit der /Console Option erfolgt.
        /// </summary>
        public static bool DebugMode = false;

        /// <summary>
        /// Wird aktiviert, wenn Protokollinformationen in das Fenster des Debuggers geschrieben werden sollen.
        /// </summary>
        public static bool EnableTracing = false;

        /// <summary>
        /// Der Registryeintrag für die erweiterte Protokollierung in eine Datei.
        /// </summary>
        private static RegistryKey m_ExtendedLogging;

        /// <summary>
        /// Eine Kurzbezeichnung für die aktuelle Laufzeitumgebung für Protokolleinträge.
        /// </summary>
        public static string DomainName;

        /// <summary>
        /// Initialisiert die statischen Variablen.
        /// </summary>
        static Tools()
        {
            // Time to load configuration
            RefreshConfiguration();
        }

        /// <summary>
        /// Gibt eine Fehlermeldung in eine Datei aus.
        /// </summary>
        /// <param name="context">Ursache des Fehlers.</param>
        /// <param name="e">Abgefangene <see cref="Exception"/>.</param>
        static public void LogException( string context, Exception e )
        {
            // Be safe
            try
            {
                // Special - may generate stack overflow!
                //Tools.ExtendedLogging( "LogException({0}) {1}", context, e );

                // Create path name
                var temp = Path.Combine( Path.GetTempPath(), "VCR.NET Recording Service.jlg" );

                // Open stream
                using (var writer = new StreamWriter( temp, true, Encoding.Unicode ))
                {
                    // Report
                    writer.WriteLine( "{0} Fatal Abort at {2}: {1}", DateTime.Now, e, context );
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Protokolliert besondere Ereignisse.
        /// </summary>
        /// <param name="format">Format für eine Meldung.</param>
        /// <param name="args">Parameter zum Format für die Meldung.</param>
        public static void ExtendedLogging( string format, params object[] args )
        {
            // Log all errors a special way
            try
            {
                // Get the path
                var path = ExtendedLogPath;
                if (path == null)
                    if (!EnableTracing)
                        return;

                // Forward
                InternalLog( path, string.Format( format, args ) );
            }
            catch (Exception e)
            {
                // Report
                LogException( "ExtendedLogging", e );
            }
        }

        /// <summary>
        /// Meldet den aktuellen Pfad zur erweiterten Protokollierung.
        /// </summary>
        private static string ExtendedLogPath
        {
            get
            {
                // Attach once
                if (m_ExtendedLogging == null)
                    m_ExtendedLogging = Registry.LocalMachine.OpenSubKey( @"SOFTWARE\JMS\VCR.NET", false );
                if (m_ExtendedLogging == null)
                    return null;

                // Load the path
                var path = m_ExtendedLogging.GetValue( @"ExtendedLogPath" ) as string;
                if (string.IsNullOrEmpty( path ))
                    return null;
                else
                    return path;
            }
        }

        /// <summary>
        /// Protokolliert besondere Ereignisse.
        /// </summary>
        /// <param name="message">Die Meldung.</param>
        public static void ExtendedLogging( string message )
        {
            // Log all errors a special way
            try
            {
                // Get the path
                var path = ExtendedLogPath;
                if (path == null)
                    if (!EnableTracing)
                        return;

                // Forward
                InternalLog( path, message );
            }
            catch (Exception e)
            {
                // Report
                LogException( "ExtendedLogging", e );
            }
        }

        /// <summary>
        /// Protokolliert besondere Ereignisse.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Protokolldatei oder <i>null</i>.</param>
        /// <param name="message">Die Meldung.</param>
        private static void InternalLog( string path, string message )
        {
            // Log all errors a special way
            try
            {
                // Create the message
                message = $"{DateTime.Now} on {Thread.CurrentThread.ManagedThreadId}@{DomainName}: {message}";

                // Report to trace
                if (EnableTracing)
                    Debug.WriteLine( message );

                // Fully synchonized
                if (path != null)
                    lock (m_LoggingLock)
                        using (var writer = new StreamWriter( path, true, Encoding.Unicode ))
                            writer.WriteLine( message );
            }
            catch (Exception e)
            {
                // Report
                LogException( "ExtendedLogging", e );
            }
        }

        /// <summary>
        /// Lädt die Konfigurationsdatei dieser Anwendung.
        /// </summary>
        public static void RefreshConfiguration()
        {
            // Just load
            RefreshConfiguration( ConfigurationManager.OpenExeConfiguration( ExecutablePath ) );
        }

        /// <summary>
        /// Aktiviert eine Konfiguration.
        /// </summary>
        /// <param name="configuration">Die zu verwendende Konfiguration.</param>
        public static void RefreshConfiguration( Configuration configuration )
        {
            // Just load
            m_ApplicationConfiguration = configuration;
        }

        /// <summary>
        /// Erzeugt das Verzeichnis zu einer Datei.
        /// </summary>
        /// <param name="fullPath">Voller Pfad, kann auch leer oder <i>null</i> sein.</param>
        static public void CreateDir( string fullPath )
        {
            // Create it
            if (!String.IsNullOrEmpty( fullPath ))
                Directory.CreateDirectory( Path.GetDirectoryName( fullPath ) );
        }

        /// <summary>
        /// Liefert das Verzeichnis, in dem die Anwendungsdatei des VCR.NET
        /// Recording Service liegt.
        /// <seealso cref="ExecutablePath"/>
        /// </summary>
        public static DirectoryInfo ApplicationDirectory { get { return RunTimeLoader.ExecutablePath.Directory; } }

        /// <summary>
        /// Ermittelt einen Zeitwert aus der Windows Registrierung.
        /// </summary>
        /// <param name="name">Der Name des Wertes.</param>
        /// <returns>Der aktuelle Wert oder <i>null</i>, wenn dieser nicht existiert oder
        /// ungültig ist.</returns>
        internal static DateTime? GetRegistryTime( string name )
        {
            // Try to load
            try
            {
                // Read it
                var value = (string) VCRServer.ServiceRegistry.GetValue( name );
                if (string.IsNullOrEmpty( value ))
                    return null;

                // To to convert
                DateTime result;
                if (DateTime.TryParseExact( value, "u", null, DateTimeStyles.None, out result ))
                    return DateTime.SpecifyKind( result, DateTimeKind.Utc );
            }
            catch
            {
                // Ignore any error
            }

            // Discard
            SetRegistryTime( name, null );

            // Not known
            return null;
        }

        /// <summary>
        /// Aktualisiert einen Zeitwert in der Windows Registrierung.
        /// </summary>
        /// <param name="name">Der Name des Wertes.</param>
        /// <param name="value">Der neue Wert oder <i>null</i>, wenn dieser entfernt
        /// werden soll.</param>
        internal static void SetRegistryTime( string name, DateTime? value )
        {
            // Always be safe
            try
            {
                // Check mode
                if (value.HasValue)
                    VCRServer.ServiceRegistry.SetValue( name, value.Value.ToString( "u" ) );
                else
                    VCRServer.ServiceRegistry.DeleteValue( name, false );
            }
            catch (Exception e)
            {
                // Report
                VCRServer.Log( e );
            }
        }

        /// <summary>
        /// Startet eine einzelne Erweiterung.
        /// </summary>
        /// <param name="extension">Die gewünschte Erweiterung.</param>
        /// <param name="environment">Parameter für die Erweiterung.</param>
        /// <returns>Der gestartete Prozess oder <i>null</i>, wenn ein Start nicht möglich war.</returns>
        public static Process RunExtension( FileInfo extension, Dictionary<string, string> environment )
        {
            // Be safe
            try
            {
                // Create the start record
                var info =
                    new ProcessStartInfo
                    {
                        WorkingDirectory = extension.DirectoryName,
                        FileName = extension.FullName,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        ErrorDialog = false,
                    };

                // Overwrite as wanted
                if (environment != null)
                    foreach (var env in environment)
                        info.EnvironmentVariables.Add( env.Key.Substring( 1, env.Key.Length - 2 ), env.Value );

                // Log
                VCRServer.Log( LoggingLevel.Full, Properties.Resources.RunExtension, extension.FullName );

                // Start the process
                return Process.Start( info );
            }
            catch (Exception e)
            {
                // Report
                VCRServer.Log( e );
            }

            // Report
            return null;
        }

        /// <summary>
        /// Ermittelt eine Liste von vollen Stunden eines Tages aus der Konfiguration.
        /// </summary>
        /// <param name="hours">Die durch Komma getrennte Liste von vollen Stunden.</param>
        /// <returns>Die Liste der Stunden.</returns>
        public static IEnumerable<uint> GetHourList( string hours )
        {
            // None at all
            if (string.IsNullOrEmpty( hours ))
                yield break;

            // Process all
            uint hour;
            foreach (var hourAsString in hours.Split( ',' ))
                if (uint.TryParse( hourAsString.Trim(), out hour ))
                    if ((hour >= 0) && (hour <= 23))
                        yield return hour;
        }

        /// <summary>
        /// Ermittelt zu einem letzten Ausführungszeitpunkt und einer Liste von erlaubten Stunden
        /// den nächsten ausführungszeitpunkt für eine periodische Aktualisierung.
        /// </summary>
        /// <param name="lastTime">Der Zeitpunkt, an dem letztmalig eine Aktualisierung ausgeführt wurde.</param>
        /// <param name="hourSkip">Die Anzahl von Stunden, die auf jeden Fall übersprungen werden sollen.</param>
        /// <param name="hourList">Die Liste der erlaubten vollen Stunden.</param>
        /// <returns>Der gewünschte Zeitpunkt oder <i>null</i>.</returns>
        public static DateTime? GetNextTime( DateTime lastTime, uint hourSkip, IEnumerable<uint> hourList )
        {
            // Create dictionary from hours
            var hours = hourList.ToDictionary( h => (int) h );
            if (hours.Count < 1)
                return null;

            // Advance to the next hour
            var nextRun = lastTime.Date.AddHours( lastTime.Hour + hourSkip );

            // Test all
            for (int it = 48; it-- > 0; nextRun = nextRun.AddHours( 1 ))
                if (hours.ContainsKey( nextRun.ToLocalTime().Hour ))
                    return nextRun;

            // Must be a configuration error
            return null;
        }

        /// <summary>
        /// Ermittelte alle Erweiterungen eine bestimmten Art.
        /// </summary>
        /// <param name="extensionType">Die gewünschte Art von Erweiterungen.</param>
        /// <returns>Alle Erweiterungen der gewünschten Art.</returns>
        public static IEnumerable<FileInfo> GetExtensions( string extensionType )
        {
            // Get the path
            var root = new DirectoryInfo( Path.Combine( ApplicationDirectory.Parent.FullName, "Server Extensions" ) );
            if (!root.Exists)
                yield break;

            // Get the path
            var extensionDir = new DirectoryInfo( Path.Combine( root.FullName, extensionType ) );
            if (!extensionDir.Exists)
                yield break;

            // Get all files
            foreach (var file in extensionDir.GetFiles( "*.bat" ))
                if (StringComparer.InvariantCultureIgnoreCase.Equals( file.Extension, ".bat" ))
                    yield return file;
        }

        /// <summary>
        /// Startet aller Erweiterung einer bestimmten Art.
        /// </summary>
        /// <param name="extensionType">Die gewünschte Art von Erweiterungen.</param>
        /// <param name="environment">Aller Parameter für die Erweiterungen.</param>
        /// <returns>Die Prozessinformationen zu allen gestarten Erweiterungen.</returns>
        public static IEnumerable<Process> RunExtensions( string extensionType, Dictionary<string, string> environment )
        {
            // Forward
            return RunExtensions( GetExtensions( extensionType ), environment );
        }

        /// <summary>
        /// Startet aller Erweiterung einer bestimmten Art.
        /// </summary>
        /// <param name="extensionType">Die gewünschte Art von Erweiterungen.</param>
        /// <param name="environment">Aller Parameter für die Erweiterungen.</param>
        /// <returns>Die Prozessinformationen zu allen gestarten Erweiterungen.</returns>
        public static void RunSynchronousExtensions( string extensionType, Dictionary<string, string> environment )
        {
            // Forward
            foreach (var process in RunExtensions( GetExtensions( extensionType ), environment ))
                if (process != null)
                    try
                    {
                        // Wait on done
                        try
                        {
                            // Do it
                            process.WaitForExit();
                        }
                        finally
                        {
                            // Get rid
                            process.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        // Report and catch all!
                        VCRServer.Log( e );
                    }
        }

        /// <summary>
        /// Startet eine Liste von Erweiterungen.
        /// </summary>
        /// <param name="extensions">Die zu startenden Erweiterungen.</param>
        /// <param name="environment">Aller Parameter für die Erweiterungen.</param>
        /// <returns>Die Prozessinformationen zu allen gestarten Erweiterungen.</returns>
        public static IEnumerable<Process> RunExtensions( IEnumerable<FileInfo> extensions, Dictionary<string, string> environment )
        {
            // Process all
            foreach (var running in extensions.Select( extension => RunExtension( extension, environment ) ).Where( process => process != null ))
            {
                // Report
                Tools.ExtendedLogging( "Started Extension Process {0}", running.Id );

                // Remember if sucessfully started
                yield return running;
            }
        }
    }
}
