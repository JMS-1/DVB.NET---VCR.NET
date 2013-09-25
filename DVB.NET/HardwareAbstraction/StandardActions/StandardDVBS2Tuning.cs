using System;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.Pipeline;


namespace JMS.DVB.StandardActions
{
    /// <summary>
    /// Nutzt die Standardschnittstelle <see cref="IBDADigitalDemodulator2"/> zur Ansteuerung von
    /// DVB-S2 Quellgruppen.
    /// </summary>
    [
        Pipeline( PipelineTypes.DVBS2, "Microsoft BDA" )
    ]
    public class StandardDVBS2Tuning : IPipelineExtension
    {
        /// <summary>
        /// Erzeugt eine neue Implementierung.
        /// </summary>
        public StandardDVBS2Tuning()
        {
        }

        /// <summary>
        /// Setzt die DVB-S2 Empfangsparameter.
        /// </summary>
        /// <param name="tune">Informationen zur aktuellen Auswahl der Quellgruppe.</param>
        /// <returns>Beschreibt, wie weiter vorzugehen ist.</returns>
        private static PipelineResult TuneDVBS2( DataGraph.TuneToken tune )
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

            // Attach to the BDA interface
            var config = tuner.GetDigitalDemodulator();
            if (config == null)
                return PipelineResult.Continue;

            // Requires cleanup
            try
            {
                // Change interface
                var config2 = config as IBDADigitalDemodulator2;
                if (config2 == null)
                    return PipelineResult.Continue;

                // Pilot to use
                var pilot = PilotMode.NotSet;

                // Roll-Off to use
                var rollOff = RollOff.NotDefined;
                switch (group.RollOff)
                {
                    case S2RollOffs.Alpha20: rollOff = RollOff.Twenty; break;
                    case S2RollOffs.Alpha25: rollOff = RollOff.TwentyFive; break;
                    case S2RollOffs.Alpha35: rollOff = RollOff.ThirtyFive; break;
                    default: rollOff = RollOff.NotSet; break;
                }

                // Modulation to use
                var modulation = ModulationType.NotDefined;
                switch (group.Modulation)
                {
                    case SatelliteModulations.QPSK: modulation = ModulationType.NBCQPSK; break;
                    case SatelliteModulations.PSK8: modulation = ModulationType.NBC8PSK; break;
                    case SatelliteModulations.QAM16: modulation = ModulationType.QAM16; break;
                }

                // Update conditionally
                if (modulation != ModulationType.NotDefined)
                    config2.SetModulation( ref modulation );

                // Update unconditionally
                config2.SetRollOff( ref rollOff );
                config2.SetPilot( ref pilot );
            }
            finally
            {
                // Back to COM
                BDAEnvironment.Release( ref config );
            }

            // Normal
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
            if ((types & PipelineTypes.DVBS2) != 0)
                graph.TunePipeline.AddPreProcessing( TuneDVBS2 );
        }

        #endregion
    }
}
