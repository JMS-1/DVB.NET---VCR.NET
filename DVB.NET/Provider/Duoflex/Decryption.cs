using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Pipeline;
using JMS.DVB.DeviceAccess.Enumerators;


namespace JMS.DVB.Provider.Duoflex
{
    /// <summary>
    /// Diese Klasse übernimmt die Ansteuerung der Entschlüsselung für Geräte der Firma <i>Digital
    /// Devices</i>.
    /// </summary>
    [
        Pipeline( PipelineTypes.CICAM, "Digital Devices", HasAdditionalParameters = true )
    ]
    public class Decryption : IPipelineParameterExtension
    {
        /// <summary>
        /// Der zuletzt beobachtete BDA Graph.
        /// </summary>
        private DataGraph m_DataGraph;

        /// <summary>
        /// Die laufende Nummer unseres Filters.
        /// </summary>
        private int m_FilterIndex = -1;

        /// <summary>
        /// Gesetzt, sobald das CAM einmal zurückgesetzt wurde.
        /// </summary>
        private bool m_HasBeenReset;

        /// <summary>
        /// Die Art, wie das CI/CAM Zurücksetzen behandelt werden soll.
        /// </summary>
        private SuppressionMode m_Suppress;

        /// <summary>
        /// Erzeugt eine neue Erweiterung.
        /// </summary>
        public Decryption()
        {
        }

        /// <summary>
        /// Aktiviert die Entschlüsselung einer Quelle.
        /// </summary>
        /// <param name="token">Informationen zur gewählten Quelle.</param>
        public PipelineResult Decrypt( DataGraph.DecryptToken token )
        {
            // Load graph
            if (token != null)
                m_DataGraph = token.Pipeline.Graph;

            // Attach to source list
            var sources = (token == null) ? null : token.Sources;
            var reset = (sources == null) || (sources.Length < 1);

            // See if we can do anything
            if (m_DataGraph != null)
                if (m_FilterIndex >= 0)
                    if (m_DataGraph.AdditionalFilters.Count > m_FilterIndex)
                    {
                        // Check COM interface
                        var controlPtr = ComIdentity.QueryInterface( m_DataGraph.AdditionalFilters[m_FilterIndex].Interface, typeof( KsControl.Interface ) );
                        if (controlPtr != IntPtr.Zero)
                            using (var control = new KsControl( controlPtr ))
                            {
                                // Deactive if forbiddem
                                if (reset)
                                    reset = (m_Suppress != SuppressionMode.Complete);

                                // Reset the CAM
                                if (reset || !m_HasBeenReset)
                                {
                                    // Report
                                    if (BDASettings.BDATraceSwitch.Enabled)
                                        Trace.WriteLine( Properties.Resources.Trace_ResetCAM, BDASettings.BDATraceSwitch.DisplayName );

                                    // Process
                                    control.Reset();

                                    // We did it
                                    m_HasBeenReset = true;
                                }

                                // Start decryption
                                if (sources != null)
                                    foreach (var source in sources)
                                        control.SetServices( source.Service );
                            }
                    }

            // Next
            return PipelineResult.Continue;
        }

        #region IPipelineParameterExtension Members

        /// <summary>
        /// Pflegt die zusätzlichen Parameter dieser Erweiterung.
        /// </summary>
        /// <param name="parent">Das übergeordnete Oberflächenelement.</param>
        /// <param name="parameters">Die Liste der Parameter.</param>
        /// <param name="type">Die Art der Erweiterung, deren Parameter gepflegt werden sollen.</param>
        void IPipelineParameterExtension.Edit( Control parent, List<ProfileParameter> parameters, PipelineTypes type )
        {
            // Show dialog
            using (var dialog = new AdditionalFilterSelector( parameters ))
                if (dialog.ShowDialog( parent ) == DialogResult.OK)
                    dialog.Update( parameters );
        }

        #endregion

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
            if ((types & PipelineTypes.CICAM) != 0)
            {
                // Load static settings
                m_Suppress = AdditionalFilterSelector.GetSuppression( PipelineTypes.CICAM, profile.Parameters );
                m_HasBeenReset = (m_Suppress != SuppressionMode.None);

                // Register in pipeline
                graph.DecryptionPipeline.AddPostProcessing( Decrypt );

                // Check for additional filter
                var filterDisplayName = profile.Parameters.GetParameter( PipelineTypes.CICAM, AdditionalFilterSelector.FilterName );
                if (!string.IsNullOrEmpty( filterDisplayName ))
                {
                    // Load moniker - if available
                    var filterMoniker = profile.Parameters.GetParameter( PipelineTypes.CICAM, AdditionalFilterSelector.FilterMoniker );

                    // Remember position of our extension
                    m_FilterIndex = graph.AdditionalFilterInformations.Count;

                    // Register in graph
                    graph.AdditionalFilterInformations.Add( DeviceAndFilterInformations.Cache.AllFilters.FindFilter( filterDisplayName, filterMoniker ) );
                }
            }
        }

        #endregion
    }
}
