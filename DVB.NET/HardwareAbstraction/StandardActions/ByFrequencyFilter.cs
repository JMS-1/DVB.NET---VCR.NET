using System;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Pipeline;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.StandardActions
{
    /// <summary>
    /// Implementiert die de facto Standardansteuerung für DiSEqC über den <see cref="IBDAFrequencyFilter"/>.
    /// </summary>
    [
        Pipeline( PipelineTypes.DiSEqC, "Using FrequencyFilter / InputRange" )
    ]
    public class ByFrequencyFilter : IPipelineExtension
    {
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

            // Initial
            uint positionOption;
            switch (location.LNB)
            {
                case DiSEqCLocations.DiSEqC1: positionOption = 0x0000; break;
                case DiSEqCLocations.DiSEqC2: positionOption = 0x0001; break;
                case DiSEqCLocations.DiSEqC3: positionOption = 0x0100; break;
                case DiSEqCLocations.DiSEqC4: positionOption = 0x0101; break;
                default: positionOption = uint.MaxValue; break;
            }

            // Request interface
            var filter = tuner.GetFrequencyFilter();
            if (filter != null)
            {
                // Do it the more or less modern way
                try
                {
                    // Load
                    filter.Range = positionOption;
                }
                finally
                {
                    // Cleanup
                    BDAEnvironment.Release( ref filter );
                }
            }
            else
            {
                // Attach to the tuning space
                var space = token.TuneRequest.TuningSpace;
                try
                {
                    // Store
                    ((IDVBSTuningSpace) space).InputRange = positionOption.ToString();
                }
                finally
                {
                    // Cleanup
                    BDAEnvironment.Release( ref space );
                }
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
