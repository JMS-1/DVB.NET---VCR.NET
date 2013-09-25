using System;
using System.Threading;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Pipeline;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.Provider.NovaS2
{
    /// <summary>
    /// DiSEqC Anbindung für die Hauppauge Nova-S Reihe der neueren Baureihe (nicht TechnoTrend Budget Line)
    /// </summary>
    [
        Pipeline( PipelineTypes.DiSEqC, "Hauppauge Nova-S" ),
    ]
    public class NovaDiSEqC : IPipelineExtension
    {
        /// <summary>
        /// Die Erweiterungseigenschaft, die für die DiSEqC Steuerung verwendet werden soll.
        /// </summary>
        private static readonly Guid BdaTunerExtensionProperties = new Guid( "faa8f3e5-31d4-4e41-88ef-00a0c9f21fc7" );

        /// <summary>
        /// Initialisiert die Instanz des Providers.
        /// </summary>
        public NovaDiSEqC()
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
            if ((types & PipelineTypes.DiSEqC) != 0)
                graph.TunePipeline.AddPostProcessing( ApplyDiSEqC );
        }

        #endregion

        /// <summary>
        /// Die zuletzt gesendete DiSEqC Steuermeldung.
        /// </summary>
        private StandardDiSEqC m_LastDiSEqC;

        /// <summary>
        /// Übernimmt die Ansteuerung einer Antenne.
        /// </summary>
        /// <param name="tune">Der aktuelle Änderungswunsch.</param>
        /// <returns>Beschreibt, wie weiter fortzufahren ist.</returns>
        private PipelineResult ApplyDiSEqC( DataGraph.TuneToken tune )
        {
            // Not active
            var diseqc = (tune == null) ? null : tune.DiSEqCMessage;
            if (diseqc == null)
            {
                // Reset request - or first call at all
                m_LastDiSEqC = null;

                // Next
                return PipelineResult.Continue;
            }

            // Attach to tuner
            var tuner = tune.Pipeline.Graph.TunerFilter;
            if (tuner == null)
                return PipelineResult.Continue;

            // Verify that grpah is created
            if (tune.Pipeline.Graph.TransportStreamAnalyser == null)
                return PipelineResult.Continue;

            // Request the message to send
            if (diseqc.Equals( m_LastDiSEqC ))
                return PipelineResult.Continue;

            // Attach to the one input pin of the tuner
            using (var input = tuner.GetSinglePin( PinDirection.Input ))
            using (var propertySet = KsPropertySet.Create<NovaDiSEqCMessage>( input.Interface ))
                if (propertySet != null)
                {
                    // Create the identifier of the property to use
                    var nodeReference = KsPNode.Create( BdaTunerExtensionProperties, BDATunerExtensions.DiSEqC, BDANodes.Tuner );

                    // Check for support of the property
                    if (!propertySet.DoesSupport( nodeReference, PropertySetSupportedTypes.Set ))
                        return PipelineResult.Continue;

                    // Create structures
                    var message = new NovaDiSEqCMessage();

                    // Create a copy
                    var command = (byte[]) diseqc.Request.Clone();

                    // As long as necessary
                    for (int nCount = diseqc.Repeat; nCount-- > 0; Thread.Sleep( 120 ))
                        try
                        {
                            // Prepare the message
                            message.Request = new byte[151];
                            message.Response = new byte[9];

                            // Fill the message
                            command.CopyTo( message.Request, 0 );

                            // Set the lengths
                            message.RequestLength = (uint) command.Length;
                            message.ResponseLength = 0;

                            // Configure
                            message.ToneBurstModulation = (ToneBurstModulationModes) diseqc.Burst;
                            message.ResponseMode = DiSEqCReceiveModes.NoReply;
                            message.DiSEqCVersion = DiSEqCVersions.Version1;
                            message.AmplitudeAttenuation = 3;
                            message.LastMessage = true;

                            // Send the message
                            propertySet.Set( nodeReference, message );

                            // Set repeat flag
                            if (command.Length > 0)
                                command[0] |= 1;
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

            // Done
            return PipelineResult.Continue;
        }
    }
}
