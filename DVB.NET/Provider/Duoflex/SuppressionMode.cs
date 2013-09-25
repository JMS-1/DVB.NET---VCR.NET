using System;


namespace JMS.DVB.Provider.Duoflex
{
    /// <summary>
    /// Beschreibt, wie mit dem Zurücksetzen des CI/CAM umzugehen ist.
    /// </summary>
    internal enum SuppressionMode
    {
        /// <summary>
        /// Wird immer ausgeführt.
        /// </summary>
        None,

        /// <summary>
        /// Nur der initiale Aufruf wird unterdrückt.
        /// </summary>
        Startup,

        /// <summary>
        /// Wird niemals ausgeführt.
        /// </summary>
        Complete,
    }
}
