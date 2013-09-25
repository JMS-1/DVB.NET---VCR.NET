using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Schnittstelle zum Zugriff auf einen eindeutigen Name.
    /// </summary>
    [
        ComImport,
        Guid( "0000000f-0000-0000-c000-000000000046" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IMoniker // : IPersist
    {
        #region IPersist

        /// <summary>
        /// Meldet die eindeutige Kennung der Implementierung des Namens.
        /// </summary>
        /// <param name="pClassID">Die gewünschte Kennung.</param>
        void GetClassID( ref Guid pClassID );

        #endregion

        /// <summary>
        /// Prüft, ob der Name verändert wurde.
        /// </summary>
        void IsDirty();

        /// <summary>
        /// Lädt den Namen aus einem Datenstrom.
        /// </summary>
        /// <param name="pstm">Der zugehörige Datenstrom.</param>
        void Load( [MarshalAs( UnmanagedType.Interface )] object pstm );

        /// <summary>
        /// Schreibt den Namen in einen Datenstrom.
        /// </summary>
        /// <param name="pstm">Der zu beschreibende Datenstrom.</param>
        /// <param name="fClearDirty">Gesetzt, wenn dieser Name nachher nicht mehr als verändert gilt.</param>
        void Save( [MarshalAs( UnmanagedType.Interface )] object pstm, int fClearDirty );

        /// <summary>
        /// Meldet die maximale Größe der Serialisierung des Names.
        /// </summary>
        /// <returns>Die maximale Größe einer Serialisierung in Bytes.</returns>
        [return: MarshalAs( UnmanagedType.I8 )]
        long GetSizeMax();

        /// <summary>
        /// Erzeugt eine COM Schnittstelle zu einer Instanz.
        /// </summary>
        /// <param name="pbc">Die Auswerteumgebung.</param>
        /// <param name="pmkToLeft">Der übergeordnete Name.</param>
        /// <param name="riidResult">Die Kennung der gewünschten COM Schnittstelle.</param>
        /// <returns>Die COM Schnittsteller einer Instanz zum Namen.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        object BindToObject( [MarshalAs( UnmanagedType.Interface )] object pbc, IMoniker pmkToLeft, ref Guid riidResult );

        /// <summary>
        /// Ermittelt eine COM Schnittstelle zu einer Speichervariante.
        /// </summary>
        /// <param name="pbc">Die Auswerteumgebung.</param>
        /// <param name="pmkToLeft">Der übergeordnete Name.</param>
        /// <param name="riid">Die Kennung der gewünschten COM Schnittstelle.</param>
        /// <returns>Die gewünschte Speicherschnittstelle zur Instanz des Namens.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        object BindToStorage( [MarshalAs( UnmanagedType.Interface )] object pbc, IMoniker pmkToLeft, ref Guid riid );

        /// <summary>
        /// Vereinfacht den Namen.
        /// </summary>
        /// <param name="pbc">Die Auswerteumgebung.</param>
        /// <param name="dwReduceHowFar">Beschreibt den Umfang der Vereinfachung.</param>
        /// <param name="ppmkToLeft">Der übergeordnete Namen.</param>
        /// <returns>Die vereinfachte Form des Namens.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IMoniker Reduce( [MarshalAs( UnmanagedType.Interface )] object pbc, uint dwReduceHowFar, ref IMoniker ppmkToLeft );

        /// <summary>
        /// Verbindet zwei Namen.
        /// </summary>
        /// <param name="pmkRight">Der untergeordnete Name.</param>
        /// <param name="fOnlyIfNotGeneric">Gesetzt, wenn es sich nicht um einen generischen Namen handelt.</param>
        /// <returns>Der zusammengesetzte Name.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IMoniker ComposeWith( IMoniker pmkRight, int fOnlyIfNotGeneric );

        /// <summary>
        /// Erzeugt eine Auflistung über die Namenshierarchie.
        /// </summary>
        /// <param name="fForward">Gesetzt, wenn die Auflistung in Vorwärtsrichtung erfolgen soll.</param>
        /// <returns>Die gewünschte Auflistung.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumMoniker Enum( int fForward );

        /// <summary>
        /// Vergleicht diesen Namen mit einem anderen.
        /// </summary>
        /// <param name="pmkOtherMoniker">Ein anderer Name.</param>
        void IsEqual( IMoniker pmkOtherMoniker );

        /// <summary>
        /// Ermittelt eine Schlüsselnummer zu diesem Namen.
        /// </summary>
        /// <returns>Die angeforderte Schlüsselnummer.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        uint Hash();

        /// <summary>
        /// Prüft, ob die zugehörige Instanz bereits aktiv ist.
        /// </summary>
        /// <param name="pbc">Die Auswertungsumgebung.</param>
        /// <param name="pmkToLeft">Der übergeordnete Name.</param>
        /// <param name="pmkNewlyRunning">Eine Referenz auf eine neu zu startende Instanz.</param>
        void IsRunning( [MarshalAs( UnmanagedType.Interface )] object pbc, IMoniker pmkToLeft, IMoniker pmkNewlyRunning );

        /// <summary>
        /// Meldet den Zeitpunkt der letzten Änderung an diesem Namen.
        /// </summary>
        /// <param name="pbc">Die Auswertungsumgebung.</param>
        /// <param name="pmkToLeft">Der übergeordnete Name.</param>
        /// <returns>Der Zeitpunkt der letzten Änderung.</returns>
        [return: MarshalAs( UnmanagedType.I8 )]
        long GetTimeOfLastChange( [MarshalAs( UnmanagedType.Interface )] object pbc, IMoniker pmkToLeft );

        /// <summary>
        /// Kehrt die Richtung der Namenshierarchie um.
        /// </summary>
        /// <returns>Der umgekehrte Name.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IMoniker Inverse();

        /// <summary>
        /// Ermittelt einen gemeinsamen Namenspräfix.
        /// </summary>
        /// <param name="pmkOther">Ein anderer Name.</param>
        /// <returns>Der gemeinsame Präfix der beiden Namen.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IMoniker CommonPrefixWith( IMoniker pmkOther );

        /// <summary>
        /// Ermittelt einen relativen Namen.
        /// </summary>
        /// <param name="pmkOther">Ein anderer Name.</param>
        /// <returns>Der relative Anteil des Namens.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IMoniker RelativePathTo( IMoniker pmkOther );

        /// <summary>
        /// Erzeugt einen Anzeigenamen zur internen Darstellung.
        /// </summary>
        /// <param name="pbc">Eine Auswerteumgebung.</param>
        /// <param name="pmkToLeft">Der übergeordnete Name.</param>
        /// <returns>Der angeforderte Anzeigename.</returns>
        [return: MarshalAs( UnmanagedType.LPWStr )]
        string GetDisplayName( [MarshalAs( UnmanagedType.Interface )] object pbc, IMoniker pmkToLeft );

        /// <summary>
        /// Intialisiert einen Namen aus einem Anzeigenamen.
        /// </summary>
        /// <param name="pbc">Eine Auswerteumgebung.</param>
        /// <param name="pmkToLeft">Der übergeordnete Name.</param>
        /// <param name="pszDisplayName">Der Anzeigename.</param>
        /// <param name="pchEaten">Die Anzahl der für die Umsetzung ausgewerteten Zeichen.</param>
        /// <returns>Ein neuer Name.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IMoniker ParseDisplayName( [MarshalAs( UnmanagedType.Interface )] object pbc, IMoniker pmkToLeft, [In, MarshalAs( UnmanagedType.LPWStr )] string pszDisplayName, out uint pchEaten );

        /// <summary>
        /// Meldet, ob es sich um einen Systemnamen handelt.
        /// </summary>
        /// <returns>Gesetzt, wenn dieser Name ein wohlbekannter systeminterner Name ist.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        uint IsSystemMoniker();
    }

    /// <summary>
    /// Hilfsmethoden zum Arbeiten mit der <see cref="IMoniker"/> Schnittstelle.
    /// </summary>
    public static class IMonikerExtensions
    {
        /// <summary>
        /// Liest eine bestimmte Eigenschaft eines eindeutigen Namens.
        /// </summary>
        /// <param name="moniker">Der eindeutige Name.</param>
        /// <param name="property">Name der gewünschten Eigenschaft.</param>
        /// <returns>Wert der Eigenschaft.</returns>
        public static string ReadProperty( this IMoniker moniker, string property )
        {
            // Validate
            if (moniker == null)
                return null;

            // Guid for reference
            Guid propBag = new Guid( "55272a00-42cb-11ce-8135-00aa004bb851" );

            // Prepare for errors
            try
            {
                // Change interface
                var props = moniker.BindToStorage( null, null, ref propBag );
                try
                {
                    // Report
                    return ((IPropertyBag) props).Get<string>( property );
                }
                finally
                {
                    // Cleanup
                    BDAEnvironment.Release( ref props );
                }
            }
            catch
            {
                // Do nothing in case of ANY error
                return null;
            }
        }

    }
}
