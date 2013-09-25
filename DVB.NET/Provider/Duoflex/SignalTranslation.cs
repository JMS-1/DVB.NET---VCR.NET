using System;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Pipeline;


namespace JMS.DVB.Provider.Duoflex
{
    /// <summary>
    /// Diese Klasse unterstützt die Bereitstellung von Signalinformationen.
    /// </summary>
    [
        Pipeline( PipelineTypes.SignalInformation, "DuoFlex" ),
    ]
    public class SignalTranslation : IPipelineExtension
    {
        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        public SignalTranslation()
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

            // Translate
            var dBGuess = Math.Round( 10.25 - token.SignalInformation.Strength.Value / 960.0, 1 );

            // Save in bounds
            token.SignalInformation.Strength = Math.Max( 0, Math.Min( 15, dBGuess ) );

            // Next
            return PipelineResult.Continue;
        }
    }
}
