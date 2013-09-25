using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt den Zustand eines Signals aus einem BDA DVB Empf�ngergraphen.
    /// </summary>
    public class BDASignalStatus : SignalStatus
    {
        /// <summary>
        /// Die urspr�nglich gemeldete St�rke.
        /// </summary>
        public readonly int BDAStrength;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="locked">Gesetzt, wenn das Signal eingeschwungen ist.</param>
        /// <param name="strength">Ein Mass f�r die St�rke des Signals.</param>
        /// <param name="quality">Ein Mass f�r die Qualit�t des Signals.</param>
        public BDASignalStatus( bool locked, int strength, double quality )
            : base( locked, (strength & 0xffff) / 10.0, quality )
        {
            // Remember
            BDAStrength = strength;
        }
    }
}
