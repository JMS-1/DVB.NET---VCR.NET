using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt einen Endpunkt, der Datenpakete entgegen nehmen kann.
    /// </summary>
    [
        ComImport,
        Guid( "56a8689d-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IMemInputPin
    {
        /// <summary>
        /// Ruft die zugeh�rige Speicherverwaltung ab.
        /// </summary>
        /// <returns>Die aktuelle Speicherverwaltung.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IMemAllocator GetAllocator();

        /// <summary>
        /// Legt die zu verwendende Speicherverwaltung fest.
        /// </summary>
        /// <param name="allocator">Eine Speicherverwaltung.</param>
        /// <param name="bReadOnly">Gesetzt, wenn die Speicherbl�cke nicht ver�ndert werden d�rfen.</param>
        void NotifyAllocator( IMemAllocator allocator, bool bReadOnly );

        /// <summary>
        /// Meldet die Anforderungen an die Speicherverwaltung.
        /// </summary>
        /// <param name="pProps">Die Anforderungen dieses Endpunktes.</param>
        void GetAllocatorRequirements( ref AllocatorProperties pProps );

        /// <summary>
        /// Nimmt einen einzelnen Speicherblock entgegen.
        /// </summary>
        /// <param name="sample">Die COM Schnittstelle des Speicherblocks.</param>
        void Receive( IntPtr sample );

        /// <summary>
        /// Nimmer eine Reihe von Speicherbl�cken entgegen.
        /// </summary>
        /// <param name="sampleArray">Die Liste der Speicherbl�cke.</param>
        /// <param name="sampleCount">Die Anzahl der Speicherbl�cke.</param>
        /// <param name="processed">Die Anzahl der verarbeiteten Speicherbl�cke.</param>
        void ReceiveMultiple( [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] IntPtr[] sampleArray, Int32 sampleCount, out Int32 processed );

        /// <summary>
        /// Pr�ft, ob dieser Endpunkt bereit ist, Speicherbl�cke entgegen zu nehmen.
        /// </summary>
        /// <returns>Ergebnis der Pr�fung, wobei negative Werte Fehlersituationen anzeigen.</returns>
        [PreserveSig]
        Int32 ReceiveCanBlock();
    }
}
