using System;


namespace JMS.DVB.Provider.FireDTV
{
    /// <summary>
    /// Detailbefehl für Entschlüsselungsanforderungen.
    /// </summary>
    internal enum PMTCommands
    {
        /// <summary>
        /// Nicht gesetzt.
        /// </summary>
        None,

        /// <summary>
        /// Entschlüsselung aktivieren.
        /// </summary>
        Descramble,

        /// <summary>
        /// Entschlüsselung nicht starten, Interaktion mit dem Anwender möglich.
        /// </summary>
        MMI,

        /// <summary>
        /// Entschlüsselung nicht starten, Interaktion mit dem Anwender notwendig.
        /// </summary>
        Query,

        /// <summary>
        /// Entschlüsselung beenden.
        /// </summary>
        NotSelected
    }
}
