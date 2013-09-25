using System;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Pipeline;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.Provider.NovaS2
{
    /// <summary>
    /// Ansteuerung der DVB-S2 Quellgruppen für die Hauppauge Nova-HD-S2.
    /// </summary>
    [
        Pipeline( PipelineTypes.DVBS2, "Hauppauge Nova-HD-S2" ),
    ]
    public class NovaHDS2Modulation : IPipelineExtension
    {
        /// <summary>
        /// Die Erweiterungseigenschaft, die für die DiSEqC Steuerung verwendet werden soll.
        /// </summary>
        private static readonly Guid BdaTunerExtensionProperties = new Guid( "faa8f3e5-31d4-4e41-88ef-00a0c9f21fc7" );

        /// <summary>
        /// Initialisiert die Instanz des Providers.
        /// </summary>
        public NovaHDS2Modulation()
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
            if ((types & PipelineTypes.DVBS2) != 0)
                graph.TunePipeline.AddPreProcessing( ActivateS2 );
        }

        #endregion

        /// <summary>
        /// Aktiviert die Auswahl einer DVB-S2 Quellgruppe.
        /// </summary>
        /// <param name="tune">Der aktuelle Auswahlvorgang.</param>
        /// <returns>Beschreibt, wie die Bearbeitung weiter fortgeführt werden soll.</returns>
        private static PipelineResult ActivateS2( DataGraph.TuneToken tune )
        {
            // Graph is starting or resetting
            var group = (tune == null) ? null : tune.SourceGroup as SatelliteGroup;
            if (group == null)
                return PipelineResult.Continue;

            // Attach to tuner
            var tuner = tune.Pipeline.Graph.TunerFilter;
            if (tuner == null)
                return PipelineResult.Continue;

            // Verify that grpah is created
            if (tune.Pipeline.Graph.TransportStreamAnalyser == null)
                return PipelineResult.Continue;

            // Not DVB-S2
            if (!group.UsesS2Modulation)
                return PipelineResult.Continue;

            // Attach to the locator of the tune request
            var locator = tune.TuneRequest.Locator;
            try
            {
                // Apply modulation
                switch (group.Modulation)
                {
                    case SatelliteModulations.QPSK: locator.Modulation = (ModulationType) (ModulationType.AnalogFrequency + 5); break;
                    case SatelliteModulations.PSK8: locator.Modulation = (ModulationType) (ModulationType.AnalogFrequency + 6); break;
                    case SatelliteModulations.QAM16: locator.Modulation = ModulationType.QAM16; break;
                }

                // Store back
                tune.TuneRequest.Locator = locator;
            }
            finally
            {
                // Cleanup properly
                BDAEnvironment.Release( ref locator );
            }

            // Attach to the one input pin of the tuner
            using (var input = tuner.GetSinglePin( PinDirection.Input ))
            {
                // Start with roll-off
                using (var propertySet = KsPropertySet.Create<RollOff>( input.Interface ))
                    if (propertySet != null)
                    {
                        // Get the property and node to use
                        var roNode = KsPNode.Create( BdaTunerExtensionProperties, BDATunerExtensions.RollOff, BDANodes.Tuner );

                        // Update
                        if (propertySet.DoesSupport( roNode, PropertySetSupportedTypes.Set ))
                            switch (group.RollOff)
                            {
                                case S2RollOffs.Alpha35: propertySet.Set( roNode, RollOff.Offset35 ); break;
                                case S2RollOffs.Alpha25: propertySet.Set( roNode, RollOff.Offset25 ); break;
                                case S2RollOffs.Alpha20: propertySet.Set( roNode, RollOff.Offset20 ); break;
                                default: propertySet.Set( roNode, RollOff.NotSet ); break;
                            }
                    }

                // Continue with pilot
                using (var propertySet = KsPropertySet.Create<Pilot>( input.Interface ))
                    if (propertySet != null)
                    {
                        // Get the property and node to use
                        var piNode = KsPNode.Create( BdaTunerExtensionProperties, BDATunerExtensions.Pilot, BDANodes.Tuner );

                        // Update
                        if (propertySet.DoesSupport( piNode, PropertySetSupportedTypes.Set ))
                            propertySet.Set( piNode, Pilot.NotSet );
                    }
            }

            // Normal
            return PipelineResult.Continue;
        }
    }
}
