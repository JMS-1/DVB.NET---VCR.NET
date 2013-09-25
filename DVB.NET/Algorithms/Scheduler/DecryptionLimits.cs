using System;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt die Einschränkungen bezüglicher einer Entschlüsselung.
    /// </summary>
    public struct DecryptionLimits
    {
        /// <summary>
        /// Die maximale Anzahl von Quellen, die gleichzeitig entschlüssel werden können.
        /// </summary>
        public int MaximumParallelSources;

        /// <summary>
        /// Meldet eine Anzeigetext zu Testzwecken.
        /// </summary>
        /// <returns>Informationen zur Konfiguration dieser Instanz.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "CI<{0}", MaximumParallelSources + 1 );
        }
    }
}
