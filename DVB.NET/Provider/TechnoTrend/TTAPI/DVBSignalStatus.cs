

namespace JMS.TechnoTrend
{
    /// <summary>
    /// Beschreibt ein Empfangssignal.
    /// </summary>
    public class DVBSignalStatus
    {
        /// <summary>
        /// Gesetzt, wenn ein Signal best�tigt wurde.
        /// </summary>
        public readonly bool Locked;

        /// <summary>
        /// Die Signalst�rke.
        /// </summary>
        public readonly double Level;

        /// <summary>
        /// Die Qualit�t des Signals.
        /// </summary>
        public readonly double Quality;

        /// <summary>
        /// Die Fehlerrate.
        /// </summary>
        public readonly double ErrorRate;

        /// <summary>
        /// Die St�rke des Signals.
        /// </summary>
        public readonly double Strength;

        /// <summary>
        /// Das S/N Verh�ltnis des Signals.
        /// </summary>
        public readonly double SignalNoise;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="locked">Gesetzt, wenn ein Signal best�tigt wurde.</param>
        /// <param name="level">Die Signalst�rke.</param>
        /// <param name="quality">Die Qualit�t des Signals.</param>
        /// <param name="BER">Die Fehlerrate.</param>
        /// <param name="strength">Die St�rke des Signals.</param>
        /// <param name="rawQuality"></param>
        public DVBSignalStatus( bool locked, double level, double quality, double BER, double strength, double rawQuality )
        {
            // Remember
            SignalNoise = rawQuality;
            Strength = strength;
            Quality = quality;
            ErrorRate = BER;
            Locked = locked;
            Level = level;
        }
    }
}
