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
        /// Meldet eine eindeutige Kennung f�r das erzeugte Datenformat.
        /// </summary>
        /// <param name="classID">Die angeforderte Kennung.</param>
        void GetClassID( out Guid classID );

        /// <summary>
        /// Meldet, ob der interne Zustand des Objektes ver�ndert wurde.
        /// </summary>
        /// <returns>Das Ergebnis der Pr�fung, negative Werte deuten auf eine Fehlersituation hin.</returns>
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
        /// <param name="stream">Der zu bef�llende Datenstrom.</param>
        /// <param name="clearDirty">Gesetzt, wenn der interne Zustand danach als unver�ndert markiert werden soll.</param>
        void Save( IStream stream, bool clearDirty );

        /// <summary>
        /// Meldet die maximale Gr��e des internen Zustandes.
        /// </summary>
        /// <returns>Die maximale Gr��e des Zustands.</returns>
        [return: MarshalAs( UnmanagedType.U8 )]
        UInt64 GetSizeMax();
    }
}
