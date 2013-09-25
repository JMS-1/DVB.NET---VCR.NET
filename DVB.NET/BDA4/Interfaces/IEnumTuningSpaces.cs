using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Listet Namensr�ume f�r Quellgruppen.
    /// </summary>
    [
        ComImport,
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
        Guid( "8b8eb248-fc2b-11d2-9d8c-00c04f72d980" )
    ]
    public interface IEnumTuningSpaces
    {
        /// <summary>
        /// Ermittelt die n�chsten Namensr�ume der Auflistung.
        /// </summary>
        /// <param name="spaces">Die Anzahl der auszulesenden Namensr�ume.</param>
        /// <param name="spaceArray">Ein Speicherbereich f�r die Ablage der Namensr�ume.</param>
        /// <param name="fetched">Die Anzahl der ermittelten Namensr�ume.</param>
        /// <returns></returns>
        [PreserveSig]
        Int32 Next( uint spaces, IntPtr spaceArray, out uint fetched );

        /// <summary>
        /// �berspringt Namensr�ume in der Auflistung.
        /// </summary>
        /// <param name="spaces">Die Anzahl der zu �berspringenden Namensr�ume.</param>
        void Skip( uint spaces );

        /// <summary>
        /// Beginnt die Auflistung erneut mit dem ersten Namensraum.
        /// </summary>
        void Reset();

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Auflistung im gleichen Abfragestand.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumTuningSpaces Clone();
    }

}
