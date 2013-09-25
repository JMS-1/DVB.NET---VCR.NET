using System;
using System.Runtime.InteropServices;

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Einstellungen für die Auswahl eines Zwischenspeichers.
    /// </summary>
    [Flags]
    public enum MemBufferFlags
    {
        /// <summary>
        /// Keine besonderen Informationen.
        /// </summary>
        None = 0,

        /// <summary>
        /// Der vorherige Block wurde übersprungen.
        /// </summary>
        PreviousFrameSkipped = 1,

        /// <summary>
        /// Dieser Block ist kein Synchronisationspunkt.
        /// </summary>
        NotASyncPoint = 2,

        /// <summary>
        /// Warten bei der Übertragung ist nicht notwendig.
        /// </summary>
        NoWait = 4,

        /// <summary>
        /// Der DirectDraw Zeichenbereich muss nicht gesperrt werden.
        /// </summary>
        NoDDSurfaceLock = 8
    }

    /// <summary>
    /// Schnittstelle für eine Speicherverwaltung.
    /// </summary>
    [
        ComImport,
        Guid( "56a8689c-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IMemAllocator
    {
        /// <summary>
        /// Legt die Eigenschaften der Speicherverwaltung fest.
        /// </summary>
        /// <param name="request">Die neuen Eigenschaften.</param>
        /// <param name="actual">Die bisherigen Eigenschaften.</param>
        void SetProperties( ref AllocatorProperties request, ref AllocatorProperties actual );

        /// <summary>
        /// Meldet die Eigenschaften der Speicherverwaltung.
        /// </summary>
        /// <param name="properties">Die aktuellen Eigenschaften.</param>
        void GetProperties( ref AllocatorProperties properties );

        /// <summary>
        /// Reserviert den benötigten Speicher.
        /// </summary>
        void Commit();

        /// <summary>
        /// Gibt den reservierten Speicher wieder frei.
        /// </summary>
        void Decommit();

        /// <summary>
        /// Meldet einen Speicherblock.
        /// </summary>
        /// <param name="sample">Der neue Speicherblock.</param>
        /// <param name="startTime">Startzeitpunkt.</param>
        /// <param name="endTime">Endzeitpunkt.</param>
        /// <param name="flags">Besonderheiten des Speicherblocks.</param>
        void GetBuffer( out IntPtr sample, IntPtr startTime, IntPtr endTime, MemBufferFlags flags );

        /// <summary>
        /// Gibt einen Speicherblock wieder frei.
        /// </summary>
        /// <param name="buffer"></param>
        void ReleaseBuffer( IntPtr buffer );
    }
}
