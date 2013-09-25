using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Hilfsklasse für die Protokollierung von Fehlern in das Ereignisprotokoll von
    /// Windows.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Schreibt einen Eintrag in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="message">Der Text zum Eintrag.</param>
        public static void Log( string message )
        {
            // Forward
            Log( EventLogEntryType.Information, message );
        }

        /// <summary>
        /// Schreibt einen Eintrag in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="message">Der Text zum Eintrag.</param>
        public static void LogError( string message )
        {
            // Forward
            Log( EventLogEntryType.Error, message );
        }

        /// <summary>
        /// Schreibt einen Eintrag in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="message">Der Text zum Eintrag.</param>
        public static void LogWarning( string message )
        {
            // Forward
            Log( EventLogEntryType.Warning, message );
        }

        /// <summary>
        /// Schreibt einen Eintrag in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="format">Das Format zum Erstellen der Nachricht.</param>
        /// <param name="args">Die Parameter zur Aufbau der Nachricht.</param>
        public static void Log( string format, params object[] args )
        {
            // Forward
            Log( EventLogEntryType.Information, format, args );
        }

        /// <summary>
        /// Schreibt einen Eintrag in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="format">Das Format zum Erstellen der Nachricht.</param>
        /// <param name="args">Die Parameter zur Aufbau der Nachricht.</param>
        public static void LogError( string format, params object[] args )
        {
            // Forward
            Log( EventLogEntryType.Error, format, args );
        }


        /// <summary>
        /// Schreibt einen Eintrag in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="format">Das Format zum Erstellen der Nachricht.</param>
        /// <param name="args">Die Parameter zur Aufbau der Nachricht.</param>
        public static void LogWarning( string format, params object[] args )
        {
            // Forward
            Log( EventLogEntryType.Warning, format, args );
        }


        /// <summary>
        /// Schreibt einen Eintrag in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="format">Das Format zum Erstellen der Nachricht.</param>
        /// <param name="type">Die Art des Fehlers.</param>
        /// <param name="args">Die Parameter zur Aufbau der Nachricht.</param>
        public static void Log( EventLogEntryType type, string format, params object[] args )
        {
            // Be safe
            try
            {
                // Forward
                Log( type, string.Format( format, args ) );
            }
            catch
            {
                // Ignore any error
            }
        }

        /// <summary>
        /// Schreibt einen Eintrag in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="message">Der Text zum Eintrag.</param>
        /// <param name="type">Die Art des Fehlers.</param>
        public static void Log( EventLogEntryType type, string message )
        {
            // Be safe
            try
            {
                // Standard way
                EventLog.WriteEntry( "DVB.NET", message, type );
            }
            catch
            {
                // Be fully safe
                try
                {
                    // Get the path
                    string path = Path.Combine( Path.GetTempPath(), "DVBNET Fallback Log.log" );

                    // Write the message
                    using (StreamWriter writer = new StreamWriter( path, true, Encoding.GetEncoding( 1252 ) ))
                        writer.WriteLine( "{0}\t{1}\t{2}", DateTime.Now, type, message );
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Meldete einen Fehler in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="e">Der aufgetretene Fehler, <i>null</i> is erlaubt.</param>
        public static void Log( Exception e )
        {
            // Process
            if (null != e)
                Log( EventLogEntryType.Error, e.Message );
        }

        /// <summary>
        /// Meldete einen Fehler in das Ereignisprotokoll von Windows.
        /// </summary>
        /// <param name="e">Der aufgetretene Fehler, <i>null</i> is erlaubt.</param>
        public static void LogAsWarning( Exception e )
        {
            // Process
            if (null != e)
                Log( EventLogEntryType.Warning, e.Message );
        }
    }
}
