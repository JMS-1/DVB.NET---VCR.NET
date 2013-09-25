using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Wird von Objekten angeboten, die eine Speicherung in einen Datenstrom erlauben.
    /// </summary>
    [
        ComImport,
        Guid( "00000109-0000-0000-c000-000000000046" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IPersistStream // : IPersist
    {
        /// <summary>
        /// Meldet eine eindeutige Kennung für das erzeugte Datenformat.
        /// </summary>
        /// <param name="classID">Die angeforderte Kennung.</param>
        void GetClassID( out Guid classID );

        /// <summary>
        /// Meldet, ob der interne Zustand des Objektes verändert wurde.
        /// </summary>
        /// <returns>Das Ergebnis der Prüfung, negative Werte deuten auf eine Fehlersituation hin.</returns>
        [PreserveSig]
        Int32 IsDirty();

        /// <summary>
        /// Rekonstruiert den internen Zustand eines Objektes aus einem Datenstrom.
        /// </summary>
        /// <param name="stream">Der zu verwendende Datenstrom.</param>
        void Load( IStream stream );

        /// <summary>
        /// Schreibt den internen Zustand eines Objektes in einen Datenstrom.
        /// </summary>
        /// <param name="stream">Der zu befüllende Datenstrom.</param>
        /// <param name="clearDirty">Gesetzt, wenn der interne Zustand danach als unverändert markiert werden soll.</param>
        void Save( IStream stream, bool clearDirty );

        /// <summary>
        /// Meldet die maximale Größe des internen Zustandes.
        /// </summary>
        /// <returns>Die maximale Größe des Zustands.</returns>
        [return: MarshalAs( UnmanagedType.U8 )]
        UInt64 GetSizeMax();
    }
}
