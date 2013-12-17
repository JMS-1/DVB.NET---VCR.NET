

namespace JMS.TechnoTrend
{
    /// <summary>
    /// Beschreibt ein Empfangssignal.
    /// </summary>
    public class DVBSignalStatus
    {
        /// <summary>
        /// Gesetzt, wenn ein Signal bestätigt wurde.
        /// </summary>
        public readonly bool Locked;

        /// <summary>
        /// Die Signalstärke.
        /// </summary>
        public readonly double Level;

        /// <summary>
        /// Die Qualität des Signals.
        /// </summary>
        public readonly double Quality;

        /// <summary>
        /// Die Fehlerrate.
        /// </summary>
        public readonly double ErrorRate;

        /// <summary>
        /// Die Stärke des Signals.
        /// </summary>
        public readonly double Strength;

        /// <summary>
        /// Das S/N Verhältnis des Signals.
        /// </summary>
        public readonly double SignalNoise;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="locked">Gesetzt, wenn ein Signal bestätigt wurde.</param>
        /// <param name="level">Die Signalstärke.</param>
        /// <param name="quality">Die Qualität des Signals.</param>
        /// <param name="BER">Die Fehlerrate.</param>
        /// <param name="strength">Die Stärke des Signals.</param>
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
