using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Schnittstelle zur Feinsteuerung der Tonerzeugung.
    /// </summary>
    [
        ComImport,
        Guid( "56a868b3-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIDispatch )
    ]
    internal interface IBasicAudio
    {
        /// <summary>
        /// Liest oder setzt die aktuelle Lautstärke.
        /// </summary>
        int Volume { get; set; }

        /// <summary>
        /// Liest oder setzte den aktuellen Versatz der Stereoanteile.
        /// </summary>
        int Balance { get; set; }
    }
}
