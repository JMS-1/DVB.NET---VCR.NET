

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt ein Empfangssignal.
    /// </summary>
    public class SignalStatus
    {
        /// <summary>
        /// Gesetzt, wenn ein Signal bestätigt wurde.
        /// </summary>
        public readonly bool Locked;

        /// <summary>
        /// Die Stärke des Signals.
        /// </summary>
        public readonly double Strength;

        /// <summary>
        /// Die Qualität des Signals.
        /// </summary>
        public readonly double Quality;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="locked">Gesetzt, wenn das Signal bestätigt ist.</param>
        /// <param name="strength">Die Stärke des Signals.</param>
        /// <param name="quality">Die Qualität des Signals.</param>
        public SignalStatus( bool locked, double strength, double quality )
        {
            // Remember
            Locked = locked;
            Strength = strength;
            Quality = quality;
        }
    }
}
