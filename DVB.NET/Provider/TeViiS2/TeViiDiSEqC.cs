using System;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.Pipeline;


namespace JMS.DVB.Provider.TeViiS2
{
    /// <summary>
    /// DiSEqC Steuerung für die TeVii DVB-S2 Geräte, zu denen auch die TechnoTrend S2-4600 gehört.
    /// </summary>
    [Pipeline( PipelineTypes.DiSEqC, "TeVii S2 / S2-4600" )]
    public class TeViiDiSEqC : IPipelineExtension
    {
        /// <summary>
        /// Die eindeutige Kennung des Erweiterungssatzes.
        /// </summary>
        private static readonly Guid TunerExtensionPropertiesS2 = new Guid( "16faac60-9022-d420-1520-d9eb716f6ec9" );

        /// <summary>
        /// Die eindeutige Kennung des Erweiterungssatzes.
        /// </summary>
        private static readonly Guid TunerExtensionPropertiesS1 = new Guid( "faa8f3e5-9022-d420-88ef-d9eb716f6ec9" );

        /// <summary>
        /// Die zuletzt gesendete DiSEqC Steuermeldung.
        /// </summary>
        private StandardDiSEqC m_LastDiSEqC;

        /// <summary>
        /// Führt die DiSEqC Steuerung aus.
        /// </summary>
        /// <param name="token">Die aktuellen Informationen zum Wechsel der Quellgruppe.</param>
        /// <returns>Meldet, wie die weitere Abarbeitung zu erfolgen hat.</returns>
        private PipelineResult ApplyDiSEqC( DataGraph.TuneToken token )
        {
            // Not active
            var diseqc = (token == null) ? null : token.DiSEqCMessage;
            if (diseqc == null)
            {
                // Reset request - or first call at all
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

            // Request the message to send
            if (diseqc.Equals( m_LastDiSEqC ))
                return PipelineResult.Continue;

            // Attach to the one input pin of the tuner
            using (var input = tuner.GetSinglePin( PinDirection.Input ))
            using (var propertySet = KsPropertySet.Create<DiSEqCMessage>( input.Interface ))
                if (propertySet != null)
                {
                    // Create the identifier of the property to use
                    var nodeReference = KsPNode.Create( TunerExtensionPropertiesS2, TunerExtensionProperties.DiSEqC, 0 );

                    // Check for support of the property
                    if (!propertySet.DoesSupport( nodeReference, PropertySetSupportedTypes.Set ))
                        return PipelineResult.Continue;

                    // Create structures
                    var message = new DiSEqCMessage();

                    // Create a copy
                    var command = (byte[]) diseqc.Request.Clone();

                    // As long as necessary
                    try
                    {
                        // Prepare the message
                        message.Request = new byte[151];

                        // Fill the message
                        command.CopyTo( message.Request, 0 );

                        // Configure
                        message.RequestLength = (byte) command.Length;
                        message.LastMessage = true;
                        message.Power = 0;

                        // Send the message
                        propertySet.Set( nodeReference, message );
                    }
                    catch
                    {
                        // Reset
                        m_LastDiSEqC = null;

                        // Forward
                        throw;
                    }

                    // Remember
                    m_LastDiSEqC = diseqc.Clone();
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
