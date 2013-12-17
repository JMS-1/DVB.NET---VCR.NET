using System;
using System.Runtime.InteropServices;
using System.Security;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// Steuert den Empfang eines einzelnen Nutzdatenstroms.
    /// </summary>
    public class DVBRawFilter : IDisposable
    {
        /// <summary>
        /// Erstellt eine Empfangsinstanz.
        /// </summary>
        /// <param name="instance">Das zugehörige C++ Objekt.</param>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??0CDVBTSFilter@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBTSFilter_Construct( IntPtr instance );

        /// <summary>
        /// Vernichtet eine Empfangsinstanz.
        /// </summary>
        /// <param name="instance">Das zugehörige C++ Objekt.</param>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??1CDVBTSFilter@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBTSFilter_Destruct( IntPtr instance );

        /// <summary>
        /// Startet den Empfang.
        /// </summary>
        /// <param name="instance">Das zugehörige C++ Objekt.</param>
        /// <param name="type">Die Art des Nutzdatenstroms.</param>
        /// <param name="streamIdentifier">Die Datenstromkennung des zu empfangenden Datenstroms.</param>
        /// <param name="match">Das Vergleichsmuster.</param>
        /// <param name="mask">Die Auswahl der Vergleichsbits.</param>
        /// <param name="matchSize">Die Größe des Vergleichsmusters.</param>
        /// <returns>Ergebnis des Vorgangs.</returns>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetFilter@CDVBTSFilter@@QAE?AW4DVB_ERROR@@W4FILTERTYPE@1@GPAE1E@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBTSFilter_SetFilter( IntPtr instance, FilterType type, UInt16 streamIdentifier, byte[] match, byte[] mask, byte matchSize );

        /// <summary>
        /// Beendet denh Empfang.
        /// </summary>
        /// <param name="instance">Das zugehörige C++ Objekt.</param>
        /// <returns>Ergebnis des Vorgangs.</returns>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?ResetFilter@CDVBTSFilter@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBTSFilter_ResetFilter( IntPtr instance );

        /// <summary>
        /// Signatur einer Empfangsmethode.
        /// </summary>
        /// <param name="buffer">Ein Speicherbereich mit Daten.</param>
        /// <param name="bytes">Die Anzahl der empfangenen Daten.</param>
        private delegate void InternalDataArrivalHandler( IntPtr buffer, Int32 bytes );

        /// <summary>
        /// Die zugehörige C++ Instanz.
        /// </summary>
        private ClassHolder m_Class = null;

        /// <summary>
        /// Die Größe des zu verwendenden Zwischenspeichers.
        /// </summary>
        public PipeSize UseExplicitBuffer = PipeSize.None;

        /// <summary>
        /// Die zugehörige Schnittstelle zum Gerät.
        /// </summary>
        private DVBFrontend m_Frontend = null;

        /// <summary>
        /// Die aktuelle Verarbeitungsmethode.
        /// </summary>
        private volatile Action<byte[]> m_DataHandler;

        /// <summary>
        /// Die zugehörige Datenstromkennung.
        /// </summary>
        private readonly ushort m_streamIdentifier = 0;

        /// <summary>
        /// Erstellt einen neuen Empfang.
        /// </summary>
        /// <param name="streamIdentifier">Die Datenstromkennung.</param>
        /// <param name="frontend">Das zugehörige Gerät.</param>
        internal DVBRawFilter( ushort streamIdentifier, DVBFrontend frontend )
        {
            // Remember
            m_streamIdentifier = streamIdentifier;
            FilterType = FilterType.None;
            m_Frontend = frontend;
        }

        /// <summary>
        /// Zerstört die Empfangsinstanz.
        /// </summary>
        ~DVBRawFilter()
        {
            // Forward
            Dispose();
        }

        /// <summary>
        /// Beendet den Empfang endgültig.
        /// </summary>
        public void Dispose()
        {
            // Be safe
            try
            {
                // Clean up
                InternalStop( true );
            }
            catch (Exception e)
            {
                // Avoid any error while cleaning up
                System.Diagnostics.Debug.WriteLine( e );
            }

            // Remove from garbage collection
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Legt das Ziel des Datenempfangs fest.
        /// </summary>
        /// <param name="dataSink">Die Methode zur Bearbeitung aller Daten.</param>
        public void SetTarget( Action<byte[]> dataSink )
        {
            // Stop all current processings
            InternalStop( false );

            // Create helper
            m_Class = new ClassHolder( LegacySize.CDVBTSFilter );

            // Construct 
            CDVBTSFilter_Construct( m_Class.ClassPointer );

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( CDVBTSFilter_Destruct );

            // Overload virtual function
            m_Class[1] = new InternalDataArrivalHandler( OnDataArrival );

            // Remember
            m_DataHandler = dataSink;
        }

        /// <summary>
        /// Beendet den Empfang kurzzeitig.
        /// </summary>
        private void Suspend()
        {
            // Check operation
            DVBException.ThrowOnError( CDVBTSFilter_ResetFilter( m_Class.ClassPointer ), "Could not stop the Filter" );
        }

        /// <summary>
        /// Beendet den Empfang.
        /// </summary>
        /// <param name="remove">Gesetzt, wenn auch eine Abmeldung bei dem zugehörige Gerät erwünscht ist.</param>
        private void InternalStop( bool remove )
        {
            // Remove from frontend
            if (remove)
                if (m_Frontend != null)
                {
                    // Remove
                    m_Frontend.RemoveFilter( m_streamIdentifier );

                    // Forget
                    m_Frontend = null;
                }

            // Discard registered handler
            m_DataHandler = null;

            // Nothing to do
            if (m_Class == null)
                return;

            // With cleanup
            try
            {
                // Terminate
                Suspend();
            }
            finally
            {
                // Remove C++ instance
                m_Class.Dispose();

                // Detach from object
                m_Class = null;
            }
        }

        /// <summary>
        /// Beendet den Empfang.
        /// </summary>
        public void Stop()
        {
            // Forward
            InternalStop( true );
        }

        /// <summary>
        /// Nimmt Daten vom Gerät entgegen.
        /// </summary>
        /// <param name="buffer">Der Speicher mit Daten.</param>
        /// <param name="bytes">Die Anzahl der empfangenen Bytes.</param>
        private void OnDataArrival( IntPtr buffer, Int32 bytes )
        {
            // Synchronize
            lock (this)
            {
                // Already done
                if (m_Class == null)
                    return;

                // Load handler
                var std = m_DataHandler;
                if (std == null)
                    return;

                // The data
                var aData = new byte[bytes];

                // Convert
                Marshal.Copy( buffer, aData, 0, bytes );

                // Forward
                std( aData );
            }
        }

        /// <summary>
        /// Aktiviert den Empfang.
        /// </summary>
        /// <param name="pattern">Das Vergleichsmuster für den Empfang.</param>
        /// <param name="mask">Die Auswahl der Vergleichbits.</param>
        public void Start( byte[] pattern, byte[] mask )
        {
            // Check size
            if ((pattern == null) != (mask == null))
                throw new ArgumentException( "Filter Data and Mask do not match" );
            if (pattern != null)
            {
                // Validate
                if (pattern.Length != mask.Length)
                    throw new ArgumentException( "Filter Data and Mask are not of same Size" );
                if (pattern.Length > 255)
                    throw new ArgumentException( "Filter Data and Mask are too large - a Maximum of 255 Bytes is allowed" );
            }

            // Length to send
            var bLenParam = (byte) ((pattern == null) ? 0 : pattern.Length);

            // Piping
            if (FilterType == FilterType.Piping)
            {
                // Validate
                if (pattern != null)
                    throw new ArgumentException( "Piping does not allow Filter Data and Mask" );

                // Set buffer parameter
                bLenParam = (byte) ((UseExplicitBuffer != PipeSize.None) ? UseExplicitBuffer : PipeSize.ThirtyTwo);
            }

            // Try to start the current filter
            DVBException.ThrowOnError( CDVBTSFilter_SetFilter( m_Class.ClassPointer, FilterType, m_streamIdentifier, pattern, mask, bLenParam ), "Could not filter" );
        }

        /// <summary>
        /// Aktiviert den Empfang.
        /// </summary>
        /// <param name="pattern">Das Vergleichsmuster für den Empfang.</param>
        public void Start( byte[] pattern )
        {
            // Short cut
            if (pattern != null)
                if (pattern.Length < 1)
                    pattern = null;

            // New array
            byte[] mask = null;

            // Fill
            if (pattern != null)
            {
                // Allocate
                mask = new byte[pattern.Length];

                // Fill it
                for (int ix = mask.Length; ix-- > 0; )
                    mask[ix] = 255;
            }

            // Forward
            Start( pattern, mask );
        }

        /// <summary>
        /// Aktiviert den Empfang.
        /// </summary>
        public void Start()
        {
            // Forward
            Start( null, null );
        }

        /// <summary>
        /// Die Art des Filters.
        /// </summary>
        public FilterType FilterType { get; set; }
    }
}