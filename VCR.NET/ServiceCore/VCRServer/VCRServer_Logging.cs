using System;
using System.Diagnostics;


namespace JMS.DVBVCR.RecordingService
{
    partial class VCRServer
    {
        /// <summary>
        /// Ereignisprotokoll für alle Meldungen, die mit Aufzeichnungen in Verbindung stehen.
        /// </summary>
        public static readonly EventLog EventLog = new EventLog( "Application", ".", "VCR.NET Recording Service" );

        /// <summary>
        /// Trägt eine <see cref="Exception"/> ins Ereignisprotokoll ein, wenn die Konfiguration
        /// des VCR.NET Recording Service die Protokollierung von Fehlern gestattet.
        /// </summary>
        /// <param name="e">Abgefangener Fehler, eingetragen wird 
        /// <see cref="Exception.ToString"/>.</param>
        public static void Log( Exception e )
        {
            // Forwatd
            LogError( "{0}", e );
        }

        /// <summary>
        /// Trägt eine Fehlermeldung ins Ereignisprotokoll ein, wenn die Konfiguration
        /// des VCR.NET Recording Service die Protokollierung von Fehlern gestattet.
        /// </summary>
        /// <param name="format">Format für den Aufbau der Fehlermeldung.</param>
        /// <param name="args">Parameter für den Aufbau der Fehlermeldung.</param>
        public static void LogError( string format, params object[] args )
        {
            // Forward
            Log( LoggingLevel.Errors, EventLogEntryType.Error, format, args );
        }

        /// <summary>
        /// Trägt eine Meldung ins Ereignisprotokoll ein, wenn die Schwere der Meldung
        /// gemäß der Konfiguration des VCR.NET Recording Service eine Protokollierung
        /// gestattet.
        /// </summary>
        /// <param name="level">Schwere der Meldung.<seealso cref="ShouldLog"/></param>
        /// <param name="format">Format für den Aufbau der Meldung.</param>
        /// <param name="args">Parameter für den Aufbau der Meldung.</param>
        public static void Log( LoggingLevel level, string format, params object[] args )
        {
            // Forward
            Log( level, EventLogEntryType.Information, format, args );
        }

        /// <summary>
        /// Check if an event of the indicated level should be reported to
        /// the event log of Windows.
        /// </summary>
        /// <param name="reportLevel">Some logging level.</param>
        /// <returns>Set, if the logging level configured requires
        /// the event to be logged.</returns>
        public static bool ShouldLog( LoggingLevel reportLevel )
        {
            // Respect the implied ordering
            return (reportLevel >= VCRConfiguration.Current.LoggingLevel);
        }

        /// <summary>
        /// Trägt eine Meldung ins Ereignisprotokoll ein, wenn die Schwere der Meldung
        /// gemäß der Konfiguration des VCR.NET Recording Service eine Protokollierung
        /// gestattet.
        /// </summary>
        /// <param name="level">Schwere der Meldung.<seealso cref="ShouldLog"/></param>
        /// <param name="entryType">Art des Eintrags ins Ereignisprotokoll.</param>
        /// <param name="format">Format für den Aufbau der Meldung.</param>
        /// <param name="args">Parameter für den Aufbau der Meldung.</param>
        public static void Log( LoggingLevel level, EventLogEntryType entryType, string format, params object[] args )
        {
            // Nothing more to do
            if (!ShouldLog( level ))
                return;

            // Report
            EventLog.WriteEntry( string.Format( format, args ), entryType );
        }
    }
}
