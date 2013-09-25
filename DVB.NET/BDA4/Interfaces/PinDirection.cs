using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Meldet die möglichen Art eines Endpunktes.
    /// </summary>
    public enum PinDirection
    {
        /// <summary>
        /// Der Endpunkt verbraucht Daten.
        /// </summary>
        Input = 0,

        /// <summary>
        /// Der Endpunkt erzeugt Daten.
        /// </summary>
        Output = 1,
    }
}
