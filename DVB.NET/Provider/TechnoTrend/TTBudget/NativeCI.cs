using System;
using System.Linq;
using System.Security;
using System.Threading;
using System.Configuration;
using System.Runtime.InteropServices;

using JMS.TechnoTrend;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Pipeline;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.Provider.TTBudget
{
    /// <summary>
    /// Implementiert eine <i>Common Interface</i> Ansteuerung über die propertiäre Schnittstelle.
    /// </summary>
    [
        Pipeline( PipelineTypes.CICAM, "TechnoTrend" )
    ]
    public class NativeCI : TechnoTrendBDAAPIAccessor, IPipelineExtension
    {
        /// <summary>
        /// Aktiviert die Nutzung des <i>Common Interfaces</i>.
        /// </summary>
        /// <param name="device">Das aktuelle Gerät.</param>
        /// <returns>Ergebnis der Ausführung.</returns>
        [DllImport( "ttBdaDrvApi_Dll.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern APIErrorCodes bdaapiOpenCIWithoutPointer( IntPtr device );

        /// <summary>
        /// Beendet die Nutzung des <i>Common Interfaces</i>.
        /// </summary>
        /// <param name="device">Das aktuelle Gerät.</param>
        /// <returns>Ergebnis der Ausführung.</returns>
        [DllImport( "ttBdaDrvApi_Dll.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern APIErrorCodes bdaapiCloseCI( IntPtr device );

        /// <summary>
        /// Aktiviert die Entschlüsselung eines Senders.
        /// </summary>
        /// <param name="device">Das zugehörige Gerät.</param>
        /// <param name="buffer">Ein Zwischenspeicher mit der PMT des Senders.</param>
        /// <param name="length">Die Größe der PMT in Bytes.</param>
        /// <returns>Ergebnis der Ausführung.</returns>
        [DllImport( "ttBdaDrvApi_Dll.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern APIErrorCodes bdaapiCIReadPSIFastWithPMT( IntPtr device, byte[] buffer, UInt16 length );

        /// <summary>
        /// Gesetzt, sobald die Verbindung zum <i>Common Interface</i> aufgesetzt wurde.
        /// </summary>
        private bool m_CIOpen = false;

        /// <summary>
        /// Verzögerung in Millisekunden nach Öffnen des CI.
        /// </summary>
        private int m_OpenDelay = 2000;

        /// <summary>
        /// Verzögerung in Millisekunden vor Aktivierung der Verschlüsselung
        /// </summary>
        private int m_TuneDelay = 0;

        /// <summary>
        /// Erzeugt eine neue Ansteuerung.
        /// </summary>
        public NativeCI()
        {
            // Load settings
            var openDelay = ConfigurationManager.AppSettings["CIOpenDelay"];
            var tuneDelay = ConfigurationManager.AppSettings["CISwitchDelay"];

            // Try to read
            if (string.IsNullOrEmpty( openDelay ) || !int.TryParse( openDelay, out m_OpenDelay )) m_OpenDelay = 2000;
            if (string.IsNullOrEmpty( tuneDelay ) || !int.TryParse( tuneDelay, out m_TuneDelay )) m_TuneDelay = 0;
        }

        /// <summary>
        /// Stellt die Verbindung zur properitären Schnittstelle her.
        /// </summary>
        /// <param name="tuner">Der Filter zur Hardware.</param>
        protected override void Open( TypedComIdentity<IBaseFilter> tuner )
        {
            // Did it all
            if (m_CIOpen)
                return;

            // Forward
            base.Open( tuner );

            // Process
            var error = bdaapiOpenCIWithoutPointer( Device );
            if (APIErrorCodes.Success != error)
                throw new DVBException( error.ToString() );

            // Remember
            m_CIOpen = true;

            // Just wait a bit
            if (m_OpenDelay > 0)
                Thread.Sleep( m_OpenDelay );
        }

        /// <summary>
        /// Beendet die Nutzung des <i>Common Interface</i>.
        /// </summary>
        private void CloseCI()
        {
            // Terminate once
            if (m_CIOpen)
                try
                {
                    // Process - ignore any error
                    bdaapiCloseCI( Device );
                }
                finally
                {
                    // We are shut down
                    m_CIOpen = false;
                }
        }

        /// <summary>
        /// Beendet die Nutzung des <i>Common Interface</i>.
        /// </summary>
        protected override void Close()
        {
            // Use helper
            CloseCI();

            // Forward
            base.Close();
        }

        /// <summary>
        /// Zählt die Aufrufe der Entschlüsselungsmethode.
        /// </summary>
        private int m_ChangeCounter;

        /// <summary>
        /// Der zuletzt beobachtete BDA Graph.
        /// </summary>
        private DataGraph m_DataGraph;

        /// <summary>
        /// Wird zur eigentlichen Steuerung der Entschlüsselung aufgerufen.
        /// </summary>
        /// <param name="pmt">Die Informationen zur Quelle.</param>
        private void Decrypt( EPG.Tables.PMT pmt )
        {
            // Connect once
            Open( m_DataGraph.TunerFilter );

            // Just wait a bit
            if (m_TuneDelay > 0)
                Thread.Sleep( m_TuneDelay );

            // Create full section and process
            var table = pmt.Section.CreateSITable().Skip( 1 ).ToArray();
            var error = bdaapiCIReadPSIFastWithPMT( Device, table, (ushort) table.Length );
            if (error != APIErrorCodes.Success)
                throw new DVBException( error.ToString() );
        }

        /// <summary>
        /// Aktiviert die Entschlüsselung einer Quelle.
        /// </summary>
        /// <param name="token">Informationen zur gewählten Quelle.</param>
        /// <exception cref="NotSupportedException">Aktuell kann immer nur eine Quelle entschlüsselt werden.</exception>
        private PipelineResult Decrypt( DataGraph.DecryptToken token )
        {
            // Load graph
            if (token != null)
                m_DataGraph = token.Pipeline.Graph;

            // Get unique call identifier
            var callIdentifier = Interlocked.Increment( ref m_ChangeCounter );

            // Check mode of operation
            var sources = (token == null) ? null : token.Sources;
            if ((sources == null) || (sources.Length < 1))
            {
                // Shutdown
                if (sources == null)
                    Close();

                // Next
                return PipelineResult.Continue;
            }

            // Check request
            if (sources.Length != 1)
                throw new NotSupportedException( Properties.Resources.Exception_DecryptSingle );

            // Wait for PMT
            token.WaitForPMT( sources[0], pmt =>
                {
                    // See if we are still allowed to process
                    if (Thread.VolatileRead( ref m_ChangeCounter ) == callIdentifier)
                        Decrypt( pmt );
                } );

            // Next
            return PipelineResult.Continue;
        }

        #region IPipelineExtension Members

        /// <summary>
        /// Aktualisiert die Aktionslisten des DVB Empfangs.
        /// </summary>
        /// <param name="graph">Der DirectShow Graph für den DVB Empfang.</param>
        /// <param name="profile">Das verwendete Geräteprofil.</param>
        /// <param name="types">Die gewünschte Aktivierung.</param>
        void IPipelineExtension.Install( DataGraph graph, Profile profile, PipelineTypes types )
        {
            // Validate
            if (graph == null)
                throw new ArgumentNullException( "graph" );
            if (profile == null)
                throw new ArgumentNullException( "profile" );

            // Check supported types
            if ((types & PipelineTypes.CICAM) != 0)
                graph.DecryptionPipeline.AddPostProcessing( Decrypt );
        }

        #endregion
    }
}
