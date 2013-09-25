using System;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Pipeline;


namespace JMS.DVB.Provider.TTBudget
{
    /// <summary>
    /// Diese Erweiterung stellt Informationen zur Signalstärke und -qualität zur Verfügung.
    /// </summary>
    [
        Pipeline( PipelineTypes.SignalInformation, "TechnoTrend" )
    ]
    public class SignalExtension : IPipelineExtension
    {
        /// <summary>
        /// Erzeugt eine neue Erweiterung.
        /// </summary>
        public SignalExtension()
        {
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
            if ((types & PipelineTypes.SignalInformation) != 0)
                graph.SignalPipeline.AddPostProcessing( TransformSignal );
        }

        #endregion

        /// <summary>
        /// Berechnet die Signalstärke.
        /// </summary>
        /// <param name="token">Der aktuelle Zustand der Berechnung.</param>
        /// <returns>Informationen zur Fortführung der Operation.</returns>
        private static PipelineResult TransformSignal( DataGraph.SignalToken token )
        {
            // Shutdown call - ignore
            if (token == null)
                return PipelineResult.Continue;

            // Nothing we can do
            if (!token.SignalInformation.Strength.HasValue)
                return PipelineResult.Continue;

            // Make sure that bounds are respected
            token.SignalInformation.Strength = Math.Max( 0, Math.Min( 15, token.SignalInformation.Strength.Value ) );

            // Next
            return PipelineResult.Continue;
        }
    }
}
