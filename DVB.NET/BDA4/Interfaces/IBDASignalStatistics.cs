using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Meldet Informationen zum aktuellen Signal.
    /// </summary>
    [
        ComImport,
        Guid( "1347d106-cf3a-428a-a5cb-ac0d9a2a4338" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBDASignalStatistics
    {
        /// <summary>
        /// Die Stärke des Signals.
        /// </summary>
        Int32 SignalStrength { set; get; }

        /// <summary>
        /// Die Qualität des Signals.
        /// </summary>
        Int32 SignalQuality { set; get; }

        /// <summary>
        /// Meldet, ob ein Signal vorliegt.
        /// </summary>
        byte SignalPresent { set; get; }

        /// <summary>
        /// Meldet, ob das Signal eingeschwungen ist.
        /// </summary>
        byte SignalLocked { set; get; }

        /// <summary>
        /// Meldet die aktuelle Zeit.
        /// </summary>
        Int32 SampleTime { set; get; }
    }
}
