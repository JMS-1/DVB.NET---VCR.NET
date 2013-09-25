using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Beschreibt ein Verzeichnis in einem Speichermedium.
    /// </summary>
    [
        ComImport,
        Guid( "0000000b-0000-0000-c000-000000000046" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IStorage
    {
        /// <summary>
        /// Erzeugt eine neue Datei im Verzeichnis.
        /// </summary>
        /// <param name="pwcsName">Der Name der Datei.</param>
        /// <param name="grfMode">Die Art des Zugriffs auf die Datei.</param>
        /// <param name="reserved1">(Reserviert)</param>
        /// <param name="reserved2">(Reserviert)</param>
        /// <returns>Die neue Datei als Datenstrom.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IStream CreateStream( string pwcsName, UInt32 grfMode, UInt32 reserved1, UInt32 reserved2 );

        /// <summary>
        /// Öffnet eine existierende Datei.
        /// </summary>
        /// <param name="pwcsName">Der Name der Datei.</param>
        /// <param name="reserved1">(Reserviert)</param>
        /// <param name="grfMode">Die Art des Zugriffs auf die Datei.</param>
        /// <param name="reserved2">(Reserviert)</param>
        /// <returns>Die neue Datei als Datenstrom.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IStream OpenStream( string pwcsName, IntPtr reserved1, UInt32 grfMode, UInt32 reserved2 );

        /// <summary>
        /// Legt ein Unterverzeichnis an.
        /// </summary>
        /// <param name="pwcsName">Der Name des Unterverzeichnisses.</param>
        /// <param name="grfMode">Die Art des Zugriffs auf das Verzeichnis.</param>
        /// <param name="reserved1">(Reserviert)</param>
        /// <param name="reserved2">(Reserviert)</param>
        /// <returns>Die COM Schnittstelle des neuen Verzeichnisses.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IStorage CreateStorage( string pwcsName, UInt32 grfMode, UInt32 reserved1, UInt32 reserved2 );

        /// <summary>
        /// Öffnet ein Unterverzeichnis.
        /// </summary>
        /// <param name="pwcsName">Der Name des Unterverzeichnisses.</param>
        /// <param name="pstgPriority">Die Prioriät des Verzeichnisses.</param>
        /// <param name="grfMode">Die Art des Zugriffs auf das Verzeichnis.</param>
        /// <param name="snbExclude">Informationen zum Ausschluss beim Zugriff.</param>
        /// <param name="reserved">(Reserviert)</param>
        /// <returns>Die Schnittstelle des neuen Verzeichnisses.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IStorage OpenStorage( string pwcsName, IStorage pstgPriority, UInt32 grfMode, IntPtr snbExclude, UInt32 reserved );

        /// <summary>
        /// Kopiert ein Verzeichnis.
        /// </summary>
        /// <param name="ciidExclude">Anzahl der Ausschlussschnittstellen.</param>
        /// <param name="rgiidExclude">Liste mit eindeutigen Kennungen ausgeschlossener Schnittstellen.</param>
        /// <param name="snbExclude">Weitere Informationen zum Ausschluss.</param>
        /// <param name="pstgDest">Das Zielverzeichnis.</param>
        void CopyTo( UInt32 ciidExclude, ref Guid rgiidExclude, IntPtr snbExclude, IStorage pstgDest );

        /// <summary>
        /// Verschiebt einen Eintrag des Verzeichnisses.
        /// </summary>
        /// <param name="pwcsName">Der Name des Eintrags.</param>
        /// <param name="pstgDest">Das Zielverzeichnis.</param>
        /// <param name="pwcsNewName">Der neue Name des Eintrags im Zielverzeichnis.</param>
        /// <param name="grfFlags">Die Art des Zugriffs.</param>
        void MoveElementTo( string pwcsName, IStorage pstgDest, string pwcsNewName, UInt32 grfFlags );

        /// <summary>
        /// Schliesst Änderungen an dem Verzeichnis ab.
        /// </summary>
        /// <param name="grfCommitFlags">Detailinformationen zum Abschluss der Operation.</param>
        void Commit( UInt32 grfCommitFlags );

        /// <summary>
        /// Macht Änderungen am Verzeichnis rückgängig.
        /// </summary>
        void Revert();

        /// <summary>
        /// Listet alle Einträge in diesem Verzeichnis.
        /// </summary>
        /// <param name="reserved1">(Reserviert)</param>
        /// <param name="reserved2">(Reserviert)</param>
        /// <param name="reserved3">(Reserviert)</param>
        /// <returns>Eine Auflistung über alle Einträge.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        object EnumElements( UInt32 reserved1, IntPtr reserved2, UInt32 reserved3 );

        /// <summary>
        /// Entfernt einen Eintrag aus diesem Verzeichnis.
        /// </summary>
        /// <param name="pwcsName">Der Name des Eintrags.</param>
        void DestroyElement( string pwcsName );

        /// <summary>
        /// Benennt einen Eintrag in diesem Verzeichnis um.
        /// </summary>
        /// <param name="pwcsOldName">Der bisherige Name.</param>
        /// <param name="pwcsNewName">Der neue Name.</param>
        void RenameElement( string pwcsOldName, string pwcsNewName );

        /// <summary>
        /// Vermerkt Zeitinformationen zu einem Eintrag.
        /// </summary>
        /// <param name="pwcsName">Der Name des betroffenen Eintrags.</param>
        /// <param name="pctime">Der Zeitpunkt der Erzeugung.</param>
        /// <param name="patime">Der Zeitpunkt des letzten Zugriff.</param>
        /// <param name="pmtime">Der Zeitpunkt der letzten Veränderung.</param>
        void SetElementTimes( string pwcsName, ref long pctime, ref long patime, ref long pmtime );

        /// <summary>
        /// Legt die Klasse dieses Verzeichnisses fest.
        /// </summary>
        /// <param name="clsid">Die eindeutige Kennung der Klasse.</param>
        void SetClass( ref Guid clsid );

        /// <summary>
        /// Setzt Zustandsinformationen.
        /// </summary>
        /// <param name="grfStateBits">Der neue Zustand.</param>
        /// <param name="grfMask">Legt fest, welche Detailaspekete verändert werden sollen.</param>
        void SetStateBits( UInt32 grfStateBits, UInt32 grfMask );

        /// <summary>
        /// Meldet den Zustand dieses Verzeichnisses.
        /// </summary>
        /// <param name="pstatstg">Der Zustand des Verzeichnisses.</param>
        /// <param name="grfStatFlag">Detailinformationen zum Zugriff auf das Verzeichnis.</param>
        void Stat( IntPtr pstatstg, UInt32 grfStatFlag );
    }
}
