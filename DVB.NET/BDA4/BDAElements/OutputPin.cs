using System;
using System.Threading;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess.BDAElements
{
    /// <summary>
    /// Diese Klasse beschreibt einen Austrittspfad aus einem Filter. Instanzen
    /// werden mit dem Eingangspfad eines anderen Filters verbunden und übertragen Samples
    /// unverändert.
    /// </summary>
    public class OutputPin : TypedComIdentity<IPin>, IPin
    {
        /// <summary>
        /// Signatur der <see cref="IMemInputPin.ReceiveMultiple"/> Methode.
        /// </summary>
        /// <param name="classPointer">COM Schnittstelle, deren Methode aufgerufen wird.</param>
        /// <param name="sample">COM Schnittstelle zum Speicherbereich.</param>
        private delegate void MediaSampleSink( IntPtr classPointer, IntPtr sample );

        /// <summary>
        /// Signatur der <see cref="IMemInputPin.NotifyAllocator"/> Methode.
        /// </summary>
        /// <param name="classPointer">COM Schnitstelle, deren Methode aufgerufen wird.</param>
        /// <param name="allocator">COM Schnittstelle zur Speicherverwaltung.</param>
        /// <param name="bReadOnly">Gesetzt, wenn die Speicherblöcke nicht verändert werden dürfen.</param>
        private delegate void NotifyAllocatorSink( IntPtr classPointer, IntPtr allocator, bool bReadOnly );

        /// <summary>
        /// Format dr Verbindung zum nächsten Filter.
        /// </summary>
        private MediaType m_ConnectType = new MediaType();

        /// <summary>
        /// Eingangsseite des verbundenen Filters.
        /// </summary>
        private IntPtr m_Connected = IntPtr.Zero;

        /// <summary>
        /// Referenz zur <see cref="IMemInputPin.ReceiveMultiple"/> Methode.
        /// </summary>
        private volatile MediaSampleSink m_MemSink = null;

        /// <summary>
        /// Referenz zur <see cref="IMemInputPin.NotifyAllocator"/> Methode.
        /// </summary>
        private NotifyAllocatorSink m_NotifySink = null;

        /// <summary>
        /// COM Schnittstelle zum verbundenen Filter.
        /// </summary>
        private IntPtr m_MemPin = IntPtr.Zero;

        /// <summary>
        /// Unterstützte Datenformate.
        /// </summary>
        private MediaType[] m_Types;

        /// <summary>
        /// Der Filter, zu dem dieser Pin gehört.
        /// </summary>
        private TypedComIdentity<IBaseFilter> m_Filter;

        /// <summary>
        /// Name dieses Pins.
        /// </summary>
        private string m_Name;

        /// <summary>
        /// Die Anzahl der entgegengenommenen Pakete.
        /// </summary>
        public long SamplesReceived = 0;

        /// <summary>
        /// Erzeugt einen neuen Pin.
        /// </summary>
        /// <param name="filter">Zugehöriger Filter.</param>
        /// <param name="name">Der eigene Name.</param>
        /// <param name="mediaType">Das zu verwendende Datenformat.</param>
        public OutputPin( TypedComIdentity<IBaseFilter> filter, string name, MediaType mediaType )
        {
            // Remember
            m_Types = new MediaType[] { mediaType };
            m_Filter = filter;
            m_Name = name;
        }

        /// <summary>
        /// Fordert zum Weiterreichen eines Samples auf.
        /// </summary>
        /// <param name="sample">Die COM Schnittstelle eines Samples.</param>
        public void Receive( IntPtr sample )
        {
            // Count
            Interlocked.Increment( ref SamplesReceived );

            // Attach to sink
            var memPin = m_MemPin;
            var sink = m_MemSink;

            // Send
            if (sink != null)
                if (memPin != IntPtr.Zero)
                    sink( memPin, sample );
        }

        /// <summary>
        /// Verbindet diesen Ausgang mit dem Eingang eines beliebigen Filters.
        /// </summary>
        /// <param name="receiver">Der Filter, der die erzeugten Daten verarbeiten soll.</param>
        /// <param name="mediaType">Die Art der zu verwendenden Daten.</param>
        public void Connect( TypedComIdentity<IBaseFilter> receiver, MediaType mediaType )
        {
            // Attach to the one input pin
            var to = receiver.GetSinglePin( PinDirection.Input );
            try
            {
                // Process
                Connect( to.Interface, mediaType.GetReference() );
            }
            finally
            {
                // Cleanup
                BDAEnvironment.Release( ref to );
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Gibt alle Ressourcen zu diesem Instanz frei.
        /// <seealso cref="Disconnect"/>
        /// </summary>
        public override void Dispose()
        {
            // Disconnect
            Disconnect();

            // Forward
            base.Dispose();
        }

        #endregion

        #region IPin Members

        /// <summary>
        /// Leitet die Daten zu einem Endpunkt um.
        /// </summary>
        /// <param name="receivePin">Der Endpunkt, der die Daten empfangen soll.</param>
        /// <param name="mediaType">Die Art der zu übertragenden Daten.</param>
        public void Connect( IntPtr receivePin, IntPtr mediaType )
        {
            // Inform partner
            var receiver = Marshal.GetObjectForIUnknown( receivePin );
            try
            {
                // Change type
                var pin = (IPin) receiver;

                // We can choose
                if (mediaType == IntPtr.Zero)
                {
                    // Use ours
                    mediaType = m_Types[0].GetReference();

                    // Ask partner
                    if (pin.QueryAccept( mediaType ) != 0)
                        throw new COMException( "mediaType", unchecked( (int) 0x80040207 ) );
                }

                // Forward
                ReceiveConnection( receivePin, mediaType );

                // Forward
                pin.ReceiveConnection( Interface, mediaType );
            }
            finally
            {
                // Cleanup
                BDAEnvironment.Release( ref receiver );
            }
        }

        /// <summary>
        /// Legt die Speicherverwaltung fest.
        /// </summary>
        /// <param name="allocator">Die zu verwendende Speicherverwaltung.</param>
        public void SetMemAllocator( TypedComIdentity<IMemAllocator> allocator )
        {
            // Nothing to do
            if (allocator == null)
                return;
            if (m_MemPin == IntPtr.Zero)
                return;
            if (m_NotifySink == null)
                return;

            // Forward
            m_NotifySink( m_MemPin, allocator.Interface, false );
        }

        /// <summary>
        /// Legt die Speicherverwaltung fest.
        /// </summary>
        /// <param name="allocator">Die zu verwendende Speicherverwaltung.</param>
        public void SetMemAllocator( IntPtr allocator )
        {
            // Nothing to do
            if (m_MemPin == IntPtr.Zero)
                return;
            if (m_NotifySink == null)
                return;
            if (allocator == IntPtr.Zero)
                return;

            // Forward
            m_NotifySink( m_MemPin, allocator, false );
        }

        /// <summary>
        /// Meldet, dass nun eine Verbindung zu einem anderen Endpunkt festgelegt wurde.
        /// </summary>
        /// <param name="connector">Ein anderer Endpunkt.</param>
        /// <param name="mediaType">Die Art der Daten, die zwischen den Endpunkten ausgetauscht werden.</param>
        public void ReceiveConnection( IntPtr connector, IntPtr mediaType )
        {
            // Free
            Disconnect();

            // Remember
            if (connector != null)
                Marshal.AddRef( connector );
            m_Connected = connector;

            // Clone the media type
            m_ConnectType.Dispose();
            m_ConnectType = new MediaType( mediaType );

            // Attach to raw COM interface
            m_MemPin = ComIdentity.QueryInterface( m_Connected, typeof( IMemInputPin ) );

            // Get the delegate for the calls - dirty but we have to avoid automatic marshalling inside the graph
            IntPtr comFunctionTable = Marshal.ReadIntPtr( m_MemPin );
            IntPtr receiveSingle = Marshal.ReadIntPtr( comFunctionTable, 24 );
            IntPtr notifyAllocator = Marshal.ReadIntPtr( comFunctionTable, 16 );
            m_MemSink = (MediaSampleSink) Marshal.GetDelegateForFunctionPointer( receiveSingle, typeof( MediaSampleSink ) );
            m_NotifySink = (NotifyAllocatorSink) Marshal.GetDelegateForFunctionPointer( notifyAllocator, typeof( NotifyAllocatorSink ) );
        }

        /// <summary>
        /// Trennt den Endpunkt von anderen.
        /// </summary>
        public void Disconnect()
        {
            // Detach all helper
            m_NotifySink = null;
            m_MemSink = null;

            // Release
            BDAEnvironment.Release( ref m_MemPin );
            BDAEnvironment.Release( ref m_Connected );

            // Reset
            m_ConnectType.Dispose();
            m_ConnectType = new MediaType();
        }

        /// <summary>
        /// Prüft, ob dieser Endpunkt mit einem anderen verbunden ist.
        /// </summary>
        /// <param name="other">Der andere Endpunkt.</param>
        /// <returns>Ergebnis der Prüfung.</returns>
        public Int32 ConnectedTo( ref IntPtr other )
        {
            // Not connected
            if (m_Connected == IntPtr.Zero)
                return BDAEnvironment.NotConnected;

            // Add reference
            Marshal.AddRef( m_Connected );

            // Remember
            other = m_Connected;

            // Done
            return 0;
        }

        /// <summary>
        /// Meldet die Daten, die dieser Endpunkt erzeugt.
        /// </summary>
        /// <param name="mediaType">Das Format der Daten.</param>
        public void ConnectionMediaType( ref RawMediaType mediaType )
        {
            // Copy
            m_ConnectType.CopyTo( ref mediaType );
        }

        /// <summary>
        /// Meldet Informationen zu diesem Endpunkt.
        /// </summary>
        /// <param name="info">Die gewünschten Informationen.</param>
        public void QueryPinInfo( ref PinInfo info )
        {
            // Create new
            info.Direction = PinDirection.Output;
            info.Filter = m_Filter.AddRef();
            info.Name = QueryId();
        }

        /// <summary>
        /// Meldet die Richtung, dieses Endpunktes.
        /// </summary>
        /// <returns>Die verwendete Richtung.</returns>
        public PinDirection QueryDirection()
        {
            // Always output
            return PinDirection.Output;
        }

        /// <summary>
        /// Meldet den eindeutigen Namen dieses Endpunktes.
        /// </summary>
        /// <returns>Der angeforderte Name.</returns>
        public string QueryId()
        {
            // Static
            return m_Name;
        }

        /// <summary>
        /// Prüft, ob dieser Endpunkt ein bestimmtes Datenformat unterstützt.
        /// </summary>
        /// <param name="mediaType">Das zu prüfende Datenformat.</param>
        /// <returns>Das Ergebnis der Prüfung, negativ im Fall einer Fehlersituation.</returns>
        public Int32 QueryAccept( IntPtr mediaType )
        {
            // Always
            return 0;
        }

        /// <summary>
        /// Meldet alle Datentypen dieses Endpunktes.
        /// </summary>
        /// <returns>Eine Auflistung über die Datentypen.</returns>
        public IEnumMediaTypes EnumMediaTypes()
        {
            // Create
            return new TypeEnum( m_Types );
        }

        /// <summary>
        /// Erfragt interne Verbindungen dieses Endpunktes.
        /// </summary>
        /// <param name="pin">Ein anderer Endpunkt.</param>
        /// <param name="pinIndex">Die laufende Nummer für die Prüfung.</param>
        public void QueryInternalConnections( out IPin pin, ref uint pinIndex )
        {
            // None
            pinIndex = 0;
            pin = null;
        }

        /// <summary>
        /// Meldet das Ende des Datenflusses durch diesen Endpunkt.
        /// </summary>
        public void EndOfStream()
        {
        }

        /// <summary>
        /// Beginnt mit dem Entleeren aller vorgehaltenen Daten.
        /// </summary>
        public void BeginFlush()
        {
        }

        /// <summary>
        /// Beendet das Entleeren aller vorgehaltenen Daten.
        /// </summary>
        public void EndFlush()
        {
        }

        /// <summary>
        /// Zeigt einen neuen Datenbereich an.
        /// </summary>
        /// <param name="tStart">Anfangszeitpunkt.</param>
        /// <param name="tStop">Endzeitpunkt.</param>
        /// <param name="dRate">Datenrate.</param>
        public void NewSegment( long tStart, long tStop, double dRate )
        {
        }

        #endregion
    }
}
