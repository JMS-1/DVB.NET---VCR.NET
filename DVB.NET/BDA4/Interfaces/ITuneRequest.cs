using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt die Anfrage zum Wechseln einer Quellgruppe.
    /// </summary>
    [
        ComImport,
        Guid( "07ddc146-fc3d-11d2-9d8c-00c04f72d980" ),
        InterfaceType( ComInterfaceType.InterfaceIsIDispatch )
    ]
    public interface ITuneRequest
    {
        /// <summary>
        /// Der zugehörige Namensraum.
        /// </summary>
        ITuningSpace TuningSpace { get; }

        /// <summary>
        /// Die verwendeten Komponenten.
        /// </summary>
        IComponents Components { get; }

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Anfrage.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        ITuneRequest Clone();

        /// <summary>
        /// Die zugehörige Beschreibung der Quellgruppe.
        /// </summary>
        ILocator Locator { get; set; }

        /// <summary>
        /// Die originale Netzwerkkennung.
        /// </summary>
        int ONID { get; set; }

        /// <summary>
        /// Die Kennung des Transportstroms und damit der Quellgruppe.
        /// </summary>
        int TSID { get; set; }

        /// <summary>
        /// Die Kennung einer Quelle innerhalb der Quellgruppe.
        /// </summary>
        int SID { get; set; }
    }
}
