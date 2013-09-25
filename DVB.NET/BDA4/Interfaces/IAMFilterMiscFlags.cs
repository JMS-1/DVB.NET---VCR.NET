using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Meldet spezielle Eigenschaften eines Filters.
    /// </summary>
    [
        ComImport,
        Guid( "2dd74950-a890-11d1-abe8-00a0c905f375" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IAMFilterMiscFlags
    {
        /// <summary>
        /// Ruft die speziellen Eigenschaften ab.
        /// </summary>
        /// <returns>Die gewünschten Informationen oder ein negative Wert, wenn ein Fehler aufgetreten ist.</returns>
        [PreserveSig]
        UInt32 GetMiscFlags();
    }

    /// <summary>
    /// Die möglichen speziellen Informationen eines Filters.
    /// </summary>
    [Flags]
    public enum FilterMiscFlags
    {
        /// <summary>
        /// Es handelt sich um eine Darstellungskomponente (Ende des Graphen).
        /// </summary>
        Renderer = 0x00000001,

        /// <summary>
        /// Es handelt sich im eine Quellkomponente (Ursprung des Graphen).
        /// </summary>
        Source = 0x00000002,
    }
}
