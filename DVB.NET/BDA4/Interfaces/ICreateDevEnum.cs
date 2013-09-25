using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Schnittstelle zum Erzeugen einer Auflistung über eine Sorte von Filtern
    /// </summary>
    [
        ComImport,
        Guid( "29840822-5b84-11d0-bd3b-00a0c911ce86" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface ICreateDevEnum
    {
        /// <summary>
        /// Erzeugt eine Auflistung über eine Art von Filtern.
        /// </summary>
        /// <param name="deviceClass">Die gewünschte Art der Filter.</param>
        /// <param name="monikers">Die angeforderte Auflistung.</param>
        /// <param name="flags">Feineinstellungen zum Abruf der Auflistung.</param>
        /// <returns>Ergebnis der Erzeugung, negativ im Falle einer Fehlersituation.</returns>
        [PreserveSig]
        Int32 CreateClassEnumerator( ref Guid deviceClass, out IEnumMoniker monikers, UInt32 flags );
    }
}
