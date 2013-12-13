using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.Pipeline;
using JMS.TechnoTrend;


namespace JMS.DVB.Provider.TTBudget
{
    /// <summary>
    /// Implementiert eine DiSEqC Ansteuerung über die propertiäre Schnittstelle.
    /// </summary>
    [
        Pipeline( PipelineTypes.DiSEqC, "TechnoTrend" )
    ]
    public class NativeDiSEqC : TechnoTrendBDAAPIAccessor, IPipelineExtension
    {
        /// <summary>
        /// Sendet einen DiSEqC Befehl.
        /// </summary>
        /// <param name="device">Das gewünschte Zielgerät.</param>
        /// <param name="data">Die DiSEqC Daten.</param>
        /// <param name="bytes">Anzahl der Bytes in den Daten.</param>
        /// <param name="repeat">Anzahl von Wiederholungen.</param>
        /// <param name="tone">Burst Modus.</param>
        /// <param name="polarisation">Gewünschte Polarisation.</param>
        /// <returns></returns>
        [DllImport( "ttBdaDrvApi_Dll.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern APIErrorCodes bdaapiSetDiSEqCMsg( IntPtr device, byte[] data, byte bytes, byte repeat, byte tone, Polarisation polarisation );

        /// <summary>
        /// Erzeugt eine neue Ansteuerung.
        /// </summary>
        public NativeDiSEqC()
        {
        }

        /// <summary>
        /// Die zuletzt gesendete Meldung.
        /// </summary>
        private StandardDiSEqC m_LastDiSEqC;

        /// <summary>
        /// Führt die DiSEqC Steuerung aus.
        /// </summary>
        /// <param name="token">Die Informationen zur aktuellen Wahl der Quellgruppe.</param>
        /// <returns>Das Ergebnis der Operation.</returns>
        private PipelineResult ApplyDiSEqC( DataGraph.TuneToken token )
        {
            // See if we are fully active
            var message = (token == null) ? null : token.DiSEqCMessage;
            if (message == null)
            {
                // Shutdown connection
                Close();

                // Reset
                m_LastDiSEqC = null;

                // Next
                return PipelineResult.Continue;
            }

            // Attach to tuner
            var tuner = token.Pipeline.Graph.TunerFilter;
            if (tuner == null)
                return PipelineResult.Continue;

            // Verify that grpah is created
            if (token.Pipeline.Graph.TransportStreamAnalyser == null)
                return PipelineResult.Continue;

            // Already did it
            if (message.Equals( m_LastDiSEqC ))
                return PipelineResult.Continue;

            // Open the device
            Open( tuner );
            try
            {
                // Process
                var data = (byte[]) message.Request.Clone();
                var error = bdaapiSetDiSEqCMsg( Device, data, (byte) data.Length, (byte) message.Repeat, message.Burst, Polarisation.NotSet );
                if (error == APIErrorCodes.Success)
                    m_LastDiSEqC = message.Clone();
                else
                    throw new DVBException( error.ToString() );
            }
            catch (Exception e)
            {
                // Report
                EventLog.WriteEntry( "DVB.NET", string.Format( "TechnoTrend BDA API DiSEqC: {0}", e ), EventLogEntryType.Error );

                // Force recalibrate on next call
                m_LastDiSEqC = null;
            }

            // Next one, please
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
            if ((types & PipelineTypes.DiSEqC) != 0)
                graph.TunePipeline.AddPreProcessing( ApplyDiSEqC );
        }

        #endregion
    }
}
