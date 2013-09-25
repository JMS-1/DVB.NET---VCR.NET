namespace JMS.DVB.DirectShow.Filters
{
    partial class TSFilter
    {
        /// <summary>
        /// Der Wert der Referenzuhr im Graphen beim Starten des Graphen.
        /// </summary>
        private long? m_TimeOnStart;

        /// <summary>
        /// Synchronisiert den Zugriff auf <see cref="m_Correction"/>.
        /// </summary>
        private object m_CorrectionLock = new object();

        /// <summary>
        /// Der Korrekturwert für das Umrechnen der Zeitbasis.
        /// </summary>
        private volatile object m_Correction;

        /// <summary>
        /// Rechnet die Zeitbasis um.
        /// </summary>
        /// <param name="pts">Die Zeitbasis aus dem Datenstrom, eventuell um Nulldurchgänge
        /// korrigiert.</param>
        /// <param name="firstTime">Gesetzt, wenn es sich um die erste Zeitübertragung handelt..</param>
        /// <returns></returns>
        internal long? GetStreamTime( long pts, bool firstTime )
        {
            // Load the correction
            long? correction = (long?) m_Correction;

            // Must start it
            if (!correction.HasValue)
                lock (m_CorrectionLock)
                {
                    // Load again
                    correction = (long?) m_Correction;

                    // Test again
                    if (!correction.HasValue)
                    {
                        // Only possible if first packet is received
                        if (!firstTime)
                            return null;

                        // Load the clock
                        long? clock0 = m_TimeOnStart;

                        // None
                        if (!clock0.HasValue)
                            return null;

                        // Get the current system clock
                        long? clock = SystemClock;

                        // None
                        if (!clock.HasValue)
                            return null;

                        // Get the relative time from the start of the graph
                        long clockDelta = clock.Value - clock0.Value;

                        // Running back in time
                        if (clockDelta < 0)
                            return null;

                        // Start with the pts we got - will be the bias for all time stamps reported in the future
                        long pts0 = pts;

                        // Make it relative to the start clock
                        pts0 -= clockDelta;

                        // Correct by the user given delay - defaults to DisplayGraph.DefaultAVDelay and must be large enough to support HDTV
                        pts0 -= AVDelay * 10000;

                        // Load to current
                        correction = pts0;

                        // Remember
                        m_Correction = correction;
                    }
                }

            // Just correct
            return pts - correction.Value;
        }
    }
}
