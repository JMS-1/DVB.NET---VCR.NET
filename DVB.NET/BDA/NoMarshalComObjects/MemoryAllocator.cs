using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.NoMarshalComObjects;


namespace JMS.DVB.DirectShow.NoMarshalComObjects
{
    /// <summary>
    /// Vermittelt den Zugriff auf eine <i>Direct Show</i> Speicherverwaltung
    /// ohne Rücksicht auf COM Apartment Regeln.
    /// </summary>
    internal class MemoryAllocator : NoMarshalBase<IMemAllocator>
    {
        /// <summary>
        /// Signatur einer Methode zur Festlegung der Eigenschaften der Speicherverwaltung.
        /// </summary>
        /// <param name="comObject">Die eigentliche Speicherverwaltungsinstanz.</param>
        /// <param name="request">Die angeforderten Einstellung.</param>
        /// <param name="actual">Die nun tatsächlich verwendeten Einstellungen.</param>
        /// <returns>Der COM Fehlercode.</returns>
        private delegate Int32 SetPropertiesSignature( IntPtr comObject, ref AllocatorProperties request, ref AllocatorProperties actual );

        /// <summary>
        /// Signature der Methode zum Aktivieren der Speicherverwaltung.
        /// </summary>
        /// <param name="comObject">Die eigentliche Speicherverwaltungsinstanz.</param>
        /// <returns>Der COM Fehlercode.</returns>
        private delegate Int32 CommitSignature( IntPtr comObject );

        /// <summary>
        /// Signature der Methode zum Deaktivieren der Speicherverwaltung.
        /// </summary>
        /// <param name="comObject">Die eigentliche Speicherverwaltungsinstanz.</param>
        /// <returns>Der COM Fehlercode.</returns>
        private delegate Int32 DecommitSignature( IntPtr comObject );

        /// <summary>
        /// Signature der Methode zum Abruf eines neuen Speicherblocks.
        /// </summary>
        /// <summary>
        /// Signature der Methode zum Deaktivieren der Speicherverwaltung.
        /// </summary>
        /// <param name="comObject">Die eigentliche Speicherverwaltungsinstanz.</param>
        /// <param name="sample">Der angeforderte Speicherbereich.</param>
        /// <param name="startTime">Die Startzeit für die Daten im Speicher - <see cref="IntPtr.Zero"/>, falls nicht bekannt.</param>
        /// <param name="endTime">Die Startzeit für die Daten im Speicher - <see cref="IntPtr.Zero"/>, falls nicht bekannt.</param>
        /// <param name="flags">Sonderwünsche bezüglich des angeforderten Speicherbereichs.</param>
        /// <returns>Der COM Fehlercode.</returns>
        private delegate Int32 GetBufferSignature( IntPtr comObject, out IntPtr sample, IntPtr startTime, IntPtr endTime, MemBufferFlags flags );

        /// <summary>
        /// Methode zum Verändern der Konfiguration der Speicherverwaltung.
        /// </summary>
        private SetPropertiesSignature m_SetProperties;

        /// <summary>
        /// Methode zum Aktivieren der Speicherverwaltung.
        /// </summary>
        private CommitSignature m_Commit;

        /// <summary>
        /// Methode zum Deaktivieren der Speicherverwaltung.
        /// </summary>
        private DecommitSignature m_Decommit;

        /// <summary>
        /// Methode zum Anfordern eines neuen Speicherbereichs.
        /// </summary>
        private GetBufferSignature m_GetBuffer;

        /// <summary>
        /// Erzeugt einen neue Speicherverwaltung.
        /// </summary>
        public MemoryAllocator()
            : base( Activator.CreateInstance( Type.GetTypeFromCLSID( Constants.CLSID_MemoryAllocator ) ), true )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="allocator">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        public MemoryAllocator( IntPtr allocator )
            : base( allocator )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="allocator">Die COM Referenz auf das <i>Direct Show</i> Objekt. Der
        /// Aufrufer hat für die Freigabe zu sorgen.</param>
        public MemoryAllocator( object allocator )
            : base( allocator )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="allocator">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        /// <param name="autoRelease">Gesetzt, wenn die Referenz nach der Übergabe freigegeben werden soll.</param>
        public MemoryAllocator( object allocator, bool autoRelease )
            : base( allocator, autoRelease )
        {
        }

        /// <summary>
        /// Erzeugt alle notwendigen Methodensignaturen.
        /// </summary>
        protected override void OnCreateDelegates()
        {
            // Alle Signaturen erfragen
            m_SetProperties = CreateDelegate<SetPropertiesSignature>( 0 );
            m_Commit = CreateDelegate<CommitSignature>( 2 );
            m_Decommit = CreateDelegate<DecommitSignature>( 3 );
            m_GetBuffer = CreateDelegate<GetBufferSignature>( 4 );
        }

        /// <summary>
        /// Verändert die Konfiguration der Speicherverwaltung.
        /// </summary>
        /// <param name="request">Die gewünschte Konfiguration.</param>
        /// <returns>Die tatsächlich ab sofort verwendete Konfiguration.</returns>
        /// <exception cref="COMException">Reicht Fehlermeldung des COM Objektes durch.</exception>
        public AllocatorProperties SetProperties( AllocatorProperties request )
        {
            // Helper
            AllocatorProperties actual = new AllocatorProperties();

            // Process
            Int32 hResult = m_SetProperties( ComInterface, ref request, ref actual );
            if (hResult < 0)
                throw new COMException( "MemoryAllocator.SetProperties", hResult );

            // Report result
            return actual;
        }

        /// <summary>
        /// Aktiviert die Speicherverwaltung.
        /// </summary>
        /// <exception cref="COMException">Reicht Fehlermeldung des COM Objektes durch.</exception>
        public void Commit()
        {
            // Process
            Int32 hResult = m_Commit( ComInterface );
            if (hResult < 0)
                throw new COMException( "MemoryAllocator.Commit", hResult );
        }

        /// <summary>
        /// Deaktiviert die Speicherverwaltung.
        /// </summary>
        /// <exception cref="COMException">Reicht Fehlermeldung des COM Objektes durch.</exception>
        public void Decommit()
        {
            // Process
            Int32 hResult = m_Decommit( ComInterface );
            if (hResult < 0)
                throw new COMException( "MemoryAllocator.Decommit", hResult );
        }

        /// <summary>
        /// Ermittelt einen neuen Speicherbereich.
        /// </summary>
        /// <param name="startTime">Die Startzeit für die Daten im Speicher - <see cref="IntPtr.Zero"/>, falls nicht bekannt.</param>
        /// <param name="endTime">Die Startzeit für die Daten im Speicher - <see cref="IntPtr.Zero"/>, falls nicht bekannt.</param>
        /// <param name="flags">Sonderwünsche bezüglich des angeforderten Speicherbereichs.</param>
        /// <returns>Der neue Speicherbereich.</returns>
        /// <exception cref="COMException">Reicht Fehlermeldung des COM Objektes durch.</exception>
        public IntPtr GetBuffer( IntPtr startTime, IntPtr endTime, MemBufferFlags flags )
        {
            // The new memory
            IntPtr sample;

            // Process
            Int32 hResult = m_GetBuffer( ComInterface, out sample, startTime, endTime, flags );
            if (hResult < 0)
                throw new COMException( "MemoryAllocator.GetBuffer", hResult );

            // Report
            return sample;
        }
    }
}
