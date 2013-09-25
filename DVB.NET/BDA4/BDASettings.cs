using System;
using System.IO;
using System.Diagnostics;
using System.Configuration;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Verwaltet die statischen Einstellungen, die über die <i>appSettings</i> der Anwendung 
    /// eingestellt sind.
    /// </summary>
    public static class BDASettings
    {
        /// <summary>
        /// Schalter zur Protokollierung von elementaren Operationen.
        /// </summary>
        public static readonly BooleanSwitch BDATraceSwitch = new BooleanSwitch( "BDATrace", "Reports low level BDA Operations" );

        /// <summary>
        /// Beschreibt, wie auf das Vorhandensein eines Signals geprüft werden soll.
        /// </summary>
        private static bool? s_FastTune;

        /// <summary>
        /// Meldet, ob die BDA Protokollierung aktiviert ist.
        /// </summary>
        public static bool BDALoggingEnabled
        {
            get
            {
                // Ask setting
                bool enabled;
                if (bool.TryParse( ConfigurationManager.AppSettings["BDALogging"], out enabled ))
                    return enabled;
                else
                    return false;
            }
        }

        /// <summary>
        /// Meldet den Pfad zur BDA Protokolldatei, sofern <see cref="BDALoggingEnabled"/> aktiviert wurde.
        /// </summary>
        public static FileInfo BDALogPath
        {
            get
            {
                // Construct
                if (BDALoggingEnabled)
                    return new FileInfo( Path.Combine( Path.GetTempPath(), string.Format( "DVBNETGraph_{0}.log", Guid.NewGuid().ToString( "N" ) ) ) );
                else
                    return null;
            }
        }

        /// <summary>
        /// Meldet, ob der BDA Graph registriert werden soll und dann mit dem GraphEditor extern betrachtet
        /// und manipuliert werden kann.
        /// </summary>
        public static bool ShouldRegisterBDAGraph
        {
            get
            {
                // Load and test
                bool register;
                if (bool.TryParse( ConfigurationManager.AppSettings["BDARegisterGraph"], out register ))
                    return register;
                else
                    return false;
            }
        }

        /// <summary>
        /// Meldet den BDA Graphen zum externen Zugriff an, sofern <see cref="ShouldRegisterBDAGraph"/> aktiv ist.
        /// </summary>
        /// <param name="graph">Der anzumeldende Graph.</param>
        /// <param name="force">Meldet den Graphen auch dann an, wenn <see cref="ShouldRegisterBDAGraph"/> inaktiv ist.</param>
        /// <returns>Informationen zur Deregistrierung.</returns>
        public static ROTRegister RegisterBDAGRaph( object graph, bool force )
        {
            // Validate
            if (graph == null)
                throw new ArgumentNullException( "graph" );

            // Check mode
            if (!force)
                if (!ShouldRegisterBDAGraph)
                    return null;

            // Do it
            return new ROTRegister( graph );
        }

        /// <summary>
        /// Meldet das Verzeichnis, in dem Rohdaten zu Testzwecken abzulegen sind.
        /// </summary>
        public static DirectoryInfo DumpDirectory
        {
            get
            {
                // See if we should dump
                var dumpDir = ConfigurationManager.AppSettings["TSDumpDirectory"];
                if (string.IsNullOrEmpty( dumpDir ))
                    return null;

                // Safe create
                try
                {
                    // The normal path
                    return new DirectoryInfo( dumpDir );
                }
                catch
                {
                    // Ignore any error
                    return null;
                }
            }
        }

        /// <summary>
        /// Meldet, ob statistische Informationen für den Datenstrom erzeugt werden sollen.
        /// </summary>
        public static bool GenerateTSStatistics
        {
            get
            {
                // Check it
                bool enabled;
                if (bool.TryParse( ConfigurationManager.AppSettings["TSStatistics"], out enabled ))
                    return enabled;
                else
                    return false;
            }
        }

        /// <summary>
        /// Meldet, ob nach einem Wechsel der Quellgruppe direkt auf das Vorhandensein eines Signals geprüft
        /// werden soll.
        /// </summary>
        public static bool FastTune
        {
            get
            {
                // Per application overwrite used
                if (s_FastTune.HasValue)
                    return s_FastTune.Value;

                // Test
                bool fastTune;
                if (bool.TryParse( ConfigurationManager.AppSettings["FastTune"], out fastTune ))
                    return fastTune;
                else
                    return false;
            }
            set
            {
                // Just remember
                s_FastTune = value;
            }
        }
    }
}
