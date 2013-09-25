using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Verwaltet grundsätzliche Informationen zum Betrieb eines DirectShow BDA Graphem zum BDA Empfang.
    /// </summary>
    public static class BDAEnvironment
    {
        /// <summary>
        /// Die eindeutige Kennung aller Filter der Art <i>Tuner</i>.
        /// </summary>
        internal static readonly Guid CategoryTunerFilter = new Guid( "71985f48-1ca1-11d3-9cc8-00c04f7971e0" );

        /// <summary>
        /// Die eindeutige Kennung aller Filter der Art <i>Capture</i>.
        /// </summary>
        internal static readonly Guid CategoryReceiverFilter = new Guid( "fd0a5af4-b41d-11d2-9c95-00c04f7971e0" );

        /// <summary>
        /// Die eindeutige Kennung der Geräteauflistungskomponente von Windows.
        /// </summary>
        public static readonly Guid SystemDeviceEnumeratorClassIdentifier = new Guid( "62be5d10-60eb-11d0-bd3b-00a0c911ce86" );

        /// <summary>
        /// Die COM Kennung eines DirectShow <i>Graph Builders</i>.
        /// </summary>
        public static readonly Guid GraphBuilderClassIdentifier = new Guid( "e436ebb3-524f-11ce-9f53-0020af0ba770" );

        /// <summary>
        /// Der COM Fehlercode zur Anzeige eines nicht verbundenen Anschlusses.
        /// </summary>
        public const Int32 NotConnected = unchecked( (Int32) 0x80040209 );

        /// <summary>
        /// Das DirectShow Format für einen Datenstrom.
        /// </summary>
        internal static readonly Guid DataFormatTypeStream = new Guid( "e436eb83-524f-11ce-9f53-0020af0ba770" );

        /// <summary>
        /// Das DirectShow Format für einen Transportstrom.
        /// </summary>
        internal static readonly Guid DataFormatMPEG2 = new Guid( "e06d8023-db46-11cf-b4d1-00805f6cbbea" );

        /// <summary>
        /// Das alternative DirectShow Format für einen Transportstrom.
        /// </summary>
        internal static readonly Guid DataFormatMPEG2BDA = new Guid( "f4aeb342-0329-4fdd-a8fd-4aff4926c978" );

        /// <summary>
        /// Ein nicht weiter definiertes Datenformat.
        /// </summary>
        public static readonly Guid DataFormatNotSpecified = new Guid( "0f6417d6-c318-11d0-a43f-00a0c9223196" );

        /// <summary>
        /// Das DirectShow Format für DVB Steuerdaten.
        /// </summary>
        internal static readonly Guid DataFormatTypeSections = new Guid( "455f176c-4b06-47ce-9aef-8caef73df7b5" );

        /// <summary>
        /// Das DirectShow Detailformat für DVB SI Tabellen.
        /// </summary>
        internal static readonly Guid DataFormatSubtypeSI = new Guid( "e9dd31a3-221d-4adb-8532-9af309c1a408" );

        /// <summary>
        /// Die alternative Art des Eingangsdatenstroms, für den Fall, dass der Filter als Demultiplexer
        /// eingesetzt wird.
        /// </summary>
        internal static readonly MediaType TransportStreamMediaType2 = new MediaType( DataFormatTypeStream, DataFormatMPEG2BDA, DataFormatNotSpecified, false, 1000 );

        /// <summary>
        /// Die Art des Eingangsdatenstroms, für den Fall, dass der Filter als Demultiplexer
        /// eingesetzt wird.
        /// </summary>
        internal static readonly MediaType TransportStreamMediaType1 = new MediaType( DataFormatTypeStream, DataFormatMPEG2, DataFormatNotSpecified, false, 1000 );

        /// <summary>
        /// Der eindeutige Name des BDA Demultiplexers von Microsoft.
        /// </summary>
        public static string MicrosoftDemultiplexerMoniker = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{AFB6C280-2C41-11D3-8A60-0000F81E0E4A}";

        /// <summary>
        /// Der Parametername für die minimale Anzahl von PAT Tabelle, die einen Wechsel der Quellgruppe als erfolgreich kennzeichnen.
        /// </summary>
        public const string MiniumPATCountName = "Tuning.MinimumPATCount";

        /// <summary>
        /// Der Parametername für die Wartezeit zwischen Wahlversuchen bei Fehlschlag des Wechselns einer Quellgruppe.
        /// </summary>
        public const string MinimumPATCountWaitName = "Tuning.MinimumPATCountWait";

        /// <summary>
        /// Die eindeutigen Namen für die Filter der einzelnen Arten von DVB Empfang.
        /// </summary>
        private static Dictionary<DVBSystemType, string> m_ProviderMap =
            new Dictionary<DVBSystemType, string>
                {
                    { DVBSystemType.Terrestrial, @"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBT Network Provider" },
                    { DVBSystemType.Satellite, @"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBS Network Provider" },
                    { DVBSystemType.Cable, @"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBC Network Provider" },
           };

        /// <summary>
        /// Meldet zu einer Art von DVB Empfang den eindeutigen Namen des zugehörigen Filters.
        /// </summary>
        /// <param name="type">Die Art des Empfangs.</param>
        /// <returns>Der eindeutige Name des Filters.</returns>
        /// <exception cref="NotSupportedException">Die angegebene Art des Empfangs wird (noch) nicht unterstützt.</exception>
        internal static string GetNetworkProviderMoniker( DVBSystemType type )
        {
            // Blind forward
            string moniker;
            if (m_ProviderMap.TryGetValue( type, out moniker ))
                return moniker;
            else
                throw new NotSupportedException( string.Format( Properties.Resources.Exception_UnsupportedDVBType, type ) );
        }

        /// <summary>
        /// Gibt eine COM Referenz ordnungsgemäß frei.
        /// </summary>
        /// <typeparam name="T">Die Art des freizugebenden Objektes.</typeparam>
        /// <param name="comObject">Die freizugebende Referenz.</param>
        public static void Release<T>( ref T comObject ) where T : class
        {
            // Load
            var instance = comObject;
            if (instance == null)
                return;

            // Forget
            comObject = null;

            // Back to COM
            if (Marshal.IsComObject( instance ))
                Marshal.ReleaseComObject( instance );
        }

        /// <summary>
        /// Verringert den Referenzzähler auf eine COM Schnittstelle.
        /// </summary>
        /// <param name="comObject">Die zu betrachtende Schnittstelle.</param>
        public static void Release( ref IntPtr comObject )
        {
            // Load
            var ptr = comObject;
            if (ptr == IntPtr.Zero)
                return;

            // Reset
            comObject = IntPtr.Zero;

            // Forward
            Marshal.Release( ptr );
        }
    }
}
