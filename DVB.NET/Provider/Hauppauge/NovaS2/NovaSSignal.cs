using System;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Pipeline;


namespace JMS.DVB.Provider.NovaS2
{
    /// <summary>
    /// Signalinformationen für die neuere Hauppauge Nova-S Baureihe bereitstellen.
    /// </summary>
    [
        Pipeline( PipelineTypes.SignalInformation, "Hauppauge Nova-S" ),
    ]
    public class NovaSSignal : IPipelineExtension
    {
        /// <summary>
        /// Initialisiert die Instanz des Providers.
        /// </summary>
        public NovaSSignal()
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

            // See if strength information from network provider is available
            var strength = token.SignalStrength;
            if (!strength.HasValue)
                return PipelineResult.Continue;

            // Get the raw value
            var bias = (double) (short) -(strength + 1);

            // Translate
            var dBGuess = Math.Round( 5.219 + bias / 1845.0, 1 );

            // Save in bounds
            token.SignalInformation.Strength = Math.Max( 0, Math.Min( 15, dBGuess ) );

            // Next
            return PipelineResult.Continue;
        }
    }
}
