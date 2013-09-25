using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt einen Namensraum für den Satellitenempfang.
    /// </summary>
    [
        ComImport,
        Guid( "cdf7be60-d954-42fd-a972-78971958e470" ),
        InterfaceType( ComInterfaceType.InterfaceIsIDispatch )
    ]
    public interface IDVBSTuningSpace // : IDVBTuningSpace2    
    {
        #region IDVBTuningSpace2

        #region IDVBTuningSpace

        #region ITuningSpace

        /// <summary>
        /// Der eindeutige Name.
        /// </summary>
        string UniqueName { [return: MarshalAs( UnmanagedType.BStr )] get; [param: MarshalAs( UnmanagedType.BStr )] set; }

        /// <summary>
        /// Der Anzeigename.
        /// </summary>
        string FriendlyName { [return: MarshalAs( UnmanagedType.BStr )] get; [param: In, MarshalAs( UnmanagedType.BStr )] set; }

        /// <summary>
        /// Die eindeutige Kennung.
        /// </summary>
        string CLSID { [return: MarshalAs( UnmanagedType.BStr )] get; }

        /// <summary>
        /// Die Art des Netzwerks.
        /// </summary>
        string NetworkType { [return: MarshalAs( UnmanagedType.BStr )] get; [param: In, MarshalAs( UnmanagedType.BStr )] set; }

        /// <summary>
        /// Die Art des Netzwerks als eindeutige Kennung.
        /// </summary>
        Guid _NetworkType { get; set; }

        /// <summary>
        /// Erzeugt eine neue Quellgruppenwechselanfrage.
        /// </summary>
        /// <returns>Die gewünschte Beschreibung der Quellgruppe.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        ITuneRequest CreateTuneRequest();

        /// <summary>
        /// Meldet eine Auflistung über alle Kategorien.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumGUID EnumCategoryGUIDs();

        /// <summary>
        /// Meldet eine Auflistung über alle Geräte.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumMoniker EnumDeviceMonikers();

        /// <summary>
        /// Meldet die bevorzugten Komponentenarten.
        /// </summary>
        IComponentTypes DefaultPreferredComponentTypes { get; set; }

        /// <summary>
        /// Die Frequenzabbildung.
        /// </summary>
        string FrequencyMapping { [return: MarshalAs( UnmanagedType.BStr )] get; [param: MarshalAs( UnmanagedType.BStr )] set; }

        /// <summary>
        /// Die Standardinformationen zum Wechsel einer Quellgruppe.
        /// </summary>
        ILocator DefaultLocator { get; set; }

        /// <summary>
        /// Erzeugt eine exakte Kopie dieses Namensraums.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        ITuningSpace Clone();

        #endregion

        /// <summary>
        /// Die Art des Empfangs.
        /// </summary>
        DVBSystemType SystemType { get; set; }

        #endregion

        /// <summary>
        /// Die eindeutige Nummer des Netzwerks.
        /// </summary>
        int NetworkID { get; set; }

        #endregion

        /// <summary>
        /// Die untere Frequenz.
        /// </summary>
        int LowOscillator { get; set; }

        /// <summary>
        /// Die obere Frequenz.
        /// </summary>
        int HighOscillator { get; set; }

        /// <summary>
        /// Die Wechselfrequenz.
        /// </summary>
        int LNBSwitch { get; set; }

        /// <summary>
        /// Der Empfangsbereich.
        /// </summary>
        string InputRange { [return: MarshalAs( UnmanagedType.BStr )] get; [param: MarshalAs( UnmanagedType.BStr )] set; }

        /// <summary>
        /// Die verwendete spektrale Inversion.
        /// </summary>
        SpectralInversion SpectralInversion { get; set; }
    }
}
