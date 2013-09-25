using System;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.Pipeline;


namespace JMS.DVB.StandardActions
{
    /// <summary>
    /// Implementiert die Ansteuerung für DiSEqC über die <see cref="IBDADiseqCommand"/> Schnittstelle.
    /// </summary>
    [
        Pipeline( PipelineTypes.DiSEqC, "Microsoft BDA" )
    ]
    public class StandardDiSEqCControl : IPipelineExtension
    {
        /// <summary>
        /// Erzeugt eine neue Implementierung.
        /// </summary>
        public StandardDiSEqCControl()
        {
        }

        /// <summary>
        /// Führt die DiSEqC Steuerung aus.
        /// </summary>
        /// <param name="token">Die aktuellen Informationen zum Wechsel der Quellgruppe.</param>
        /// <returns>Meldet, wie die weitere Abarbeitung zu erfolgen hat.</returns>
        private static PipelineResult ApplyDiSEqC( DataGraph.TuneToken token )
        {
            // Check mode
            var location = (token == null) ? null : token.GroupLocation as SatelliteLocation;
            if (location == null)
                return PipelineResult.Continue;

            // Load tuner
            var tuner = token.Pipeline.Graph.TunerFilter;
            if (tuner == null)
                return PipelineResult.Continue;

            // Verify that grpah is created
            if (token.Pipeline.Graph.TransportStreamAnalyser == null)
                return PipelineResult.Continue;

            // Check support
            var diseqc = tuner.GetOutputControlNode<IBDADiseqCommand>( 0 );
            if (diseqc == null)
                return PipelineResult.Continue;

            // Requires cleanup
            try
            {
                // Enable
                diseqc.Enable = true;

                // Set mode
                switch (location.LNB)
                {
                    case DiSEqCLocations.BurstOff: diseqc.UseToneBurst = false; break;
                    case DiSEqCLocations.BurstOn: diseqc.UseToneBurst = true; break;
                    case DiSEqCLocations.DiSEqC1: diseqc.LNBSource = 1; break;
                    case DiSEqCLocations.DiSEqC2: diseqc.LNBSource = 2; break;
                    case DiSEqCLocations.DiSEqC3: diseqc.LNBSource = 3; break;
                    case DiSEqCLocations.DiSEqC4: diseqc.LNBSource = 4; break;
                    case DiSEqCLocations.None: diseqc.Enable = false; break;
                }
            }
            finally
            {
                // Cleanup
                BDAEnvironment.Release( ref diseqc );
            }

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
            if ((types & PipelineTypes.DiSEqC) != 0)
                graph.TunePipeline.AddPreProcessing( ApplyDiSEqC );
        }

        #endregion
    }
}
