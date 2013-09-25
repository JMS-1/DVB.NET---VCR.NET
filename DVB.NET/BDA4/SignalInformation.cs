using System;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Beschreibt technische Daten zu einem empfangenen Signal.
    /// </summary>
    public class SignalInformation
    {
        /// <summary>
        /// Meldet oder legt fest, ob der Tuner auf die Frequenz eingeschwungen
        /// ist.
        /// </summary>
        public bool? Locked { get; set; }

        /// <summary>
        /// Meldet die Signalstärkein dB.
        /// </summary>
        public double? Strength { get; set; }

        /// <summary>
        /// Meldet die Signalqualität in %.
        /// </summary>
        public double? Quality { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public SignalInformation()
        {
        }
    }
}
