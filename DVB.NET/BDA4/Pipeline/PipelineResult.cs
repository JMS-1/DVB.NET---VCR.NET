using System;


namespace JMS.DVB.DeviceAccess.Pipeline
{
    /// <summary>
    /// Beschreibt das Ergebnis eines einzelnen Ausführungsschrittes.
    /// </summary>
    public enum PipelineResult
    {
        /// <summary>
        /// Fährt mit dem nächsten Schritt fort.
        /// </summary>
        Continue,

        /// <summary>
        /// Beendet die Ausführung mit dem aktuellen Schritt ohne eine Fehlermeldung.
        /// </summary>
        Terminate,
    }
}
