using System;
using System.Runtime.InteropServices;

using JMS.DVB;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.NoMarshalComObjects;


namespace JMS.DVB.DirectShow.Filters
{
    /// <summary>
    /// Diese Klasse übermittelt einen Rohdatenstrom in einen Direct Show
    /// Graphen.
    /// </summary>
    internal class Injector : IDisposable
    {
        /// <summary>
        /// Signatur einer Methode zur Übermittelung eines Direct Show
        /// Datenblocks in den Graphen.
        /// </summary>
        /// <param name="sample">Der Direct Show Datenblock.</param>
        public delegate void Sink( IntPtr sample );

        /// <summary>
        /// Die verwendete Direct Show Speicherinstanz.
        /// </summary>
        private NoMarshalComObjects.MemoryAllocator m_Allocator = null;

        /// <summary>
        /// Gesetzt, wenn die noch nicht in den Graphen übermittelten Rohdaten
        /// einen Synchronisationspunkt darstellen.
        /// </summary>
        private bool m_SyncWaiting = false;

        /// <summary>
        /// Anzahl der Rohdaten, die bisher entgegen genommen wurden.
        /// </summary>
        private long m_BytesReceived = 0;

        /// <summary>
        /// Auf eine <i>Stream Time</i> gesetzt, wenn die noch nicht in den
        /// Graphen übermittelten Rohdatenen eine <i>Stream Time</i> Zeitstempel
        /// besitzen.
        /// </summary>
        private long? m_SyncTime = null;

        /// <summary>
        /// Gesetzt, wenn Rohdaten in den Graphen übertragen werden sollen.
        /// </summary>
        private bool m_Running = false;

        /// <summary>
        /// Zwischenspeicher zur Entkoppelung des Graphen von der Erzeugung der
        /// Rohdaten.
        /// </summary>
        private byte[] m_Buffer;

        /// <summary>
        /// Aktueller Füllstand des Zwischenspeichers.
        /// </summary>
        private int m_Index = 0;

        /// <summary>
        /// Methode zur Übertragung der Rohdaten in den Direct Show Graphen.
        /// </summary>
        private Sink m_Sink;

        /// <summary>
        /// Art der übermittelten Daten.
        /// </summary>
        private MediaType m_Type;

        /// <summary>
        /// Gesetzt, wenn der Datentyp übertragen werden muss.
        /// </summary>
        private bool m_SendType = false;

        /// <summary>
        /// Anzahl der Direct Show Speicher, die in der Speicherverwlatungsinstanz
        /// angelegt werden sollen.
        /// </summary>
        private int m_Count;

        /// <summary>
        /// Erzeugt eine neue Vermittlungsinstanz.
        /// </summary>
        /// <param name="count">Anzahl der Direct Show Speicherblöcke, die
        /// vorzuhalten sind.</param>
        /// <param name="size">Größe eines einzelnen Direct Show Speicherblocks.</param>
        /// <param name="sink">Methode zur Übermittelung der Rohdaten in den Graphen.</param>
        /// <exception cref="ArgumentNullException">Die Übermittelungsmethode darf nicht <i>null</i>
        /// sein.</exception>
        public Injector( int count, int size, Sink sink )
            : this( count, size )
        {
            // Forward
            SetSink( sink );
        }

        /// <summary>
        /// Meldet die Anzahl der bisher erhaltenen Rohdaten.
        /// </summary>
        public long BytesReceived
        {
            get
            {
                // Report
                return m_BytesReceived;
            }
        }

        /// <summary>
        /// Initialisiert eine neue Instanz.
        /// </summary>
        /// <param name="count">Anzahl der Direct Show Speicherblöcke, die
        /// vorzuhalten sind.</param>
        /// <param name="size">Größe eines einzelnen Direct Show Speicherblocks.</param>
        protected Injector( int count, int size )
        {
            // Remember
            m_Count = count;

            // Allocate buffer
            m_Buffer = new byte[size];
        }

        /// <summary>
        /// Legt die Methode zur Übermittelung der Rohdaten in den Direct Show
        /// graphen fest.
        /// </summary>
        /// <param name="sink">Methode zur Übermittelung der Rohdaten in den Graphen.</param>
        /// <exception cref="ArgumentNullException">Die Übermittelungsmethode darf nicht <i>null</i>
        /// sein.</exception>
        protected void SetSink( Sink sink )
        {
            // Validate
            if (null == sink) throw new ArgumentNullException( "sink" );

            // Remember
            m_Sink = sink;
        }

        /// <summary>
        /// Legt das Format der Daten fest.
        /// </summary>
        /// <param name="type">Das gewünschte Datenformat.</param>
        protected void SetMediaType( MediaType type )
        {
            // Remember
            m_Type = type;
            m_SendType = true;
        }

        /// <summary>
        /// Wird aufgerufen, wenn sich die Direct Show Speicherverwaltungsinstanz geändert hat.
        /// </summary>
        /// <param name="allocator">Neue Speicherverwaltungsinstanz.</param>
        protected virtual void OnAllocatorChanged( NoMarshalComObjects.MemoryAllocator allocator )
        {
        }

        /// <summary>
        /// Aktiviert die Datenübertragung in den Direct Show Graphen.
        /// </summary>
        public virtual void Start()
        {
            // Enable forwarding packages
            m_Running = true;
        }

        /// <summary>
        /// Deaktiviert die Datenübertragung in den Direct Show Graphen.
        /// </summary>
        public virtual void Stop()
        {
            // Disable forwarding packages
            m_Running = false;
        }

        /// <summary>
        /// Löscht die zwischengespeicherten Rohdaten.
        /// </summary>
        public virtual void Clear()
        {
            // Forget all
            m_SyncWaiting = false;
            m_SyncTime = null;
            m_Index = 0;
        }

        /// <summary>
        /// Überträgt alle Daten aus dem Zwischenspeicher in den Direct Show Graphen.
        /// </summary>
        private void Flush()
        {
            // Nothing to do
            if (m_Index < 1)
                return;

            // Be safe
            try
            {
                // Allocate buffer -  may fail during shutdown so be cautious
                using (var sample = new MediaSample( m_Allocator.GetBuffer( IntPtr.Zero, IntPtr.Zero, MemBufferFlags.None ) ))
                {
                    // Set flags
                    if (m_SyncWaiting)
                        sample.IsSyncPoint = true;

                    // May want to stamp the time
                    if (m_SyncTime.HasValue)
                    {
                        // Get the time
                        long tStart = m_SyncTime.Value;

                        // Set it
                        sample.SetTime( tStart, tStart + 1 );
                    }

                    // Debug only
                    //System.Diagnostics.Debug.WriteLine( string.Format( "{0} {1} {2}", this, m_SyncTime.HasValue ? new TimeSpan( m_SyncTime.Value ) : TimeSpan.Zero, m_Index ) );

                    // Get the address of the data
                    IntPtr data = sample.BaseAddress;

                    // Fill it
                    Marshal.Copy( m_Buffer, 0, data, m_Index );

                    // Set real size
                    sample.ActualDataLength = m_Index;

                    // Set the media type
                    if (m_SendType)
                    {
                        // Once only
                        m_SendType = false;

                        // Add to sample
                        sample.MediaType = m_Type;
                    }

                    // Send to process - will COM release
                    m_Sink( sample.ComInterface );
                }
            }
            catch
            {
                // Ignore any error
            }
            finally
            {
                // Reset all - even in case of error
                m_SyncWaiting = false;
                m_SyncTime = null;
                m_Index = 0;
            }
        }

        /// <summary>
        /// Nimmt Rohdaten zur Übermittelung in den Direct Show Graphen entgegen.
        /// </summary>
        /// <param name="buffer">Ein Rohdatenblock - ist dieser Parameter <i>null</i>, 
        /// so wird das Ende des Rohdatenstroms angezeigt und die Direct Show
        /// Speicherverwaltungsinstanz freigegeben.</param>
        /// <param name="offset">Erstes zu verwendendes Byte im Rohdatenblock.</param>
        /// <param name="length">Anzahl der Bytes, die dem Rohdatenblock zu entnehmen sind.</param>
        /// <param name="sync">Gesetzt, wenn die Rohdaten einen Synchronisationspunkt im
        /// Datenstrom bezeichnen.</param>
        /// <param name="time">Gesetzt auf eine <i>Stream Time</i>, wenn die Rohdaten
        /// einen PTS getragen haben.</param>
        public void Inject( byte[] buffer, int offset, int length, bool sync, long? time )
        {
            // Force shutdown of allocator
            if (null == buffer)
            {
                // Release allocator
                if (null != m_Allocator)
                {
                    // Release memory
                    m_Allocator.Decommit();

                    // Release
                    m_Allocator.Dispose();

                    // Back to CLR
                    m_Allocator = null;

                    // Report
                    OnAllocatorChanged( null );
                }

                // Done
                return;
            }

            // Count
            m_BytesReceived += length;

            // Discard when not running
            if (!m_Running)
            {
                // Get rid of the rest
                Flush();

                // Done
                return;
            }

            // Create allocator once
            if (null == m_Allocator)
            {
                // Create allocator
                m_Allocator = new NoMarshalComObjects.MemoryAllocator();

                // Properties
                AllocatorProperties props = new AllocatorProperties();

                // Configure
                props.cbAlign = 8;
                props.cBuffers = m_Count;
                props.cbBuffer = m_Buffer.Length;

                // Store
                m_Allocator.SetProperties( props );
                m_Allocator.Commit();

                // Forward
                OnAllocatorChanged( m_Allocator );
            }

            // Flush
            if (sync || time.HasValue)
            {
                // Get rid of buffer
                Flush();

                // We are now doing a sync
                m_SyncWaiting = sync;
                m_SyncTime = time;
            }

            // All of it
            while (length > 0)
            {
                // Check for fill up
                int fill = Math.Min( m_Buffer.Length - m_Index, length );

                // Copy over
                Array.Copy( buffer, offset, m_Buffer, m_Index, fill );

                // Adjust buffer
                m_Index += fill;

                // Adjust input
                offset += fill;
                length -= fill;

                // See if we should send
                if (m_Index >= m_Buffer.Length)
                    Flush();
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz und gibt alle verbundene Ressourcen frei.
        /// </summary>
        public virtual void Dispose()
        {
            // Stop sending packaged
            Stop();
        }

        #endregion
    }
}
