using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Erlaubt Feineinstellungen für den DVB-S Empfang.
    /// </summary>
    [
        ComImport,
        Guid( "71985f47-1ca1-11d3-9cc8-00c04f7971e0" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBDAFrequencyFilter
    {
        /// <summary>
        /// Meldet oder setzt den automatischen Einschwingvorgang.
        /// </summary>
        UInt32 Autotune { set; get; }

        /// <summary>
        /// Meldet oder setzt die Frequenz.
        /// </summary>
        UInt32 Frequency { set; get; }

        /// <summary>
        /// Meldet oder setzt die Polarisations des Signals.
        /// </summary>
        Polarisation SignalPolarisation { set; get; }

        /// <summary>
        /// Meldet oder setzt den Empfangsbereich.
        /// </summary>
        UInt32 Range { set; get; }

        /// <summary>
        /// Meldet oder setzt die Bandbreite.
        /// </summary>
        UInt32 Bandwidth { set; get; }

        /// <summary>
        /// Meldet oder setzt den Frequenzfaktor.
        /// </summary>
        UInt32 FrequencyMultiplier { set; get; }
    }
}
