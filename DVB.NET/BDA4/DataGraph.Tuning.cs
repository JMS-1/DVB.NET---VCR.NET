using System;

using JMS.DVB.DeviceAccess.Pipeline;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess
{
    partial class DataGraph
    {
        /// <summary>
        /// Enthält alle Informationen, die während einer Auswahl einer Quellgruppe benötigt werden.
        /// </summary>
        public partial class TuneToken : PipelineToken<TuneToken>, IDisposable
        {
            /// <summary>
            /// Die Anfrage zum Wechseln der Quellgruppe.
            /// </summary>
            private ITuneRequest m_Request;

            /// <summary>
            /// Meldet die Anfrage zum Wechseln der Quellgruppe.
            /// </summary>
            public ITuneRequest TuneRequest
            {
                get
                {
                    // Report
                    return m_Request;
                }
            }

            /// <summary>
            /// Der Ursprung der Quellegruppe - zurzeit nur für den Satellitenempfang von Bedeutung.
            /// </summary>
            public GroupLocation GroupLocation { get; private set; }

            /// <summary>
            /// Die neu anzuwählende Quellgruppe.
            /// </summary>
            public SourceGroup SourceGroup { get; private set; }

            /// <summary>
            /// Für den Fall des Empfangs über Satellite die zugehörige DiSEqC Steuermeldung.
            /// </summary>
            public StandardDiSEqC DiSEqCMessage { get; private set; }

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="pipeline">Die zugehörige Gesamtliste aller Aktionen gleicher Art.</param>
            /// <param name="location">Der Ursprung für die angegebene Quellgruppe.</param>
            /// <param name="group">Die neue Quellgruppe.</param>
            private TuneToken( ActionPipeline<TuneToken> pipeline, GroupLocation location, SourceGroup group )
                : base( pipeline )
            {
                // Remember
                GroupLocation = location;
                SourceGroup = group;
            }

            /// <summary>
            /// Erzeugt ein neue Zustandsinformation.
            /// </summary>
            /// <param name="pipeline">Die zugehörige Gesamtliste aller Aktionen gleicher Art.</param>
            /// <param name="location">Der Ursprung für die angegebene Quellgruppe.</param>
            /// <param name="group">Die neue Quellgruppe.</param>
            /// <returns>Die gewünschte Information.</returns>
            /// <exception cref="ArgumentNullException">Es wurde kein Graph übergeben.</exception>
            internal static TuneToken Create( ActionPipeline<TuneToken> pipeline, GroupLocation location, SourceGroup group )
            {
                // Create new
                var token = new TuneToken( pipeline, location, group );

                // Configure
                IDVBTuningSpace2 tuningSpace;
                Action<ILocator> initializer;
                switch (pipeline.Graph.DVBType)
                {
                    case DVBSystemType.Terrestrial: tuningSpace = token.PrepareTerrestrial( out initializer ); break;
                    case DVBSystemType.Satellite: tuningSpace = token.PrepareSatellite( out initializer ); break;
                    case DVBSystemType.Cable: tuningSpace = token.PrepareCable( out initializer ); break;
                    default: throw new NotImplementedException( pipeline.Graph.DVBType.ToString() );
                }

                // With cleanup
                try
                {
                    // Create a new tune request
                    token.CreateTuneRequest( tuningSpace, initializer );
                }
                finally
                {
                    // No longer used
                    BDAEnvironment.Release( ref tuningSpace );
                }

                // Report
                return token;
            }

            /// <summary>
            /// Erzeugt die eigentliche Anfrage zum Wechseln der Quellgruppe.
            /// </summary>
            /// <param name="tuningSpace">Der zu verwendende Namensraum.</param>
            /// <param name="initializer">Methode zur Übertragung der aktuellen Quellgruppenparameter.</param>
            private void CreateTuneRequest( IDVBTuningSpace2 tuningSpace, Action<ILocator> initializer )
            {
                // Create from tuning space
                m_Request = tuningSpace.CreateTuneRequest();

                // Reset internally - just in case
                m_Request.ONID = -1;
                m_Request.TSID = -1;
                m_Request.SID = -1;

                // Attach to the default locator
                var @default = tuningSpace.DefaultLocator;
                try
                {
                    // Clone the default locator
                    var locator = @default.Clone();
                    try
                    {
                        // Set it up
                        if (initializer != null)
                            initializer( locator );

                        // Attach locator
                        m_Request.Locator = locator;
                    }
                    finally
                    {
                        // Detach from clone
                        BDAEnvironment.Release( ref locator );
                    }
                }
                finally
                {
                    // Detach
                    BDAEnvironment.Release( ref @default );
                }
            }

            #region IDisposable Members

            /// <summary>
            /// Beendet die Nutzung dieser Instanz endgültig.
            /// </summary>
            public void Dispose()
            {
                // Detach all
                BDAEnvironment.Release( ref m_Request );
            }

            #endregion
        }

        /// <summary>
        /// Die für die Auswahl der Quellgruppe verantwortliche Aktionslist.
        /// </summary>
        public ActionPipeline<TuneToken> TunePipeline { get; private set; }

        /// <summary>
        /// Wechselt die Quellgruppe.
        /// </summary>
        /// <param name="token">Die Informationen zur neuen Quellgruppe.</param>
        /// <returns>Meldet, wie die Bearbeitung fortgesetzt werden soll.</returns>
        private PipelineResult SetTuneRequest( TuneToken token )
        {
            // Ignore termination
            if (token == null)
                return PipelineResult.Continue;

            // Connect the request to the network provider
            using (var provider = NetworkProvider.MarshalToManaged())
                ((ITuner) provider.Object).TuneRequest = token.TuneRequest;

            // Now run post processings
            return PipelineResult.Continue;
        }
    }
}
