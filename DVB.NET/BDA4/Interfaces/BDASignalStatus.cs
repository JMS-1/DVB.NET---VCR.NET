using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt den Zustand eines Signals aus einem BDA DVB Empfängergraphen.
    /// </summary>
    public class BDASignalStatus : SignalStatus
    {
        /// <summary>
        /// Die ursprünglich gemeldete Stärke.
        /// </summary>
        public readonly int BDAStrength;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="locked">Gesetzt, wenn das Signal eingeschwungen ist.</param>
        /// <param name="strength">Ein Mass für die Stärke des Signals.</param>
        /// <param name="quality">Ein Mass für die Qualität des Signals.</param>
        public BDASignalStatus( bool locked, int strength, double quality )
            : base( locked, (strength & 0xffff) / 10.0, quality )
        {
            // Remember
            BDAStrength = strength;
        }
    }
}
