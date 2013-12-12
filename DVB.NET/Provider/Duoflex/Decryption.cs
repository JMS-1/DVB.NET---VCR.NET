using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Enumerators;
using JMS.DVB.DeviceAccess.Pipeline;


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
        /// Zählt die Aufrufe der Entschlüsselungsmethode <see cref="Decrypt(DataGraph.DecryptToken)"/>.
        /// </summary>
        private int m_ChangeCounter;

        /// <summary>
        /// Steuert den Zugriff auf die Hardware.
        /// </summary>
        private readonly object m_deviceAccess = new object();

        /// <summary>
        /// Entschlüsselt eine einzelne Quelle.
        /// </summary>
        /// <param name="pmt">Die Informationen zur Quelle.</param>
        private void Decrypt( EPG.Tables.PMT pmt )
        {
            // Check COM interface
            var controlPtr = ComIdentity.QueryInterface( m_DataGraph.AdditionalFilters[m_FilterIndex].Interface, typeof( KsControl.Interface ) );
            if (controlPtr != IntPtr.Zero)
                using (var control = new KsControl( controlPtr ))
                    control.SetServices( pmt.ProgramNumber );
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

            // Get unique call identifier
            var callIdentifier = Interlocked.Increment( ref m_ChangeCounter );

            // See if we can do anything
            if (m_DataGraph == null)
                return PipelineResult.Continue;
            if (m_FilterIndex < 0)
                return PipelineResult.Continue;
            if (m_FilterIndex >= m_DataGraph.AdditionalFilters.Count)
                return PipelineResult.Continue;

            // Deactivate if CAM reset is forbidden
            var sources = (token == null) ? null : token.Sources;
            var noSources = (sources == null) || (sources.Length < 1);
            if ((noSources && (m_Suppress != SuppressionMode.Complete)) || !m_HasBeenReset)
                lock (m_deviceAccess)
                {
                    // Check COM interface
                    var controlPtr = ComIdentity.QueryInterface( m_DataGraph.AdditionalFilters[m_FilterIndex].Interface, typeof( KsControl.Interface ) );
                    if (controlPtr == IntPtr.Zero)
                        return PipelineResult.Continue;

                    // Process
                    using (var control = new KsControl( controlPtr ))
                    {
                        // Report
                        if (BDASettings.BDATraceSwitch.Enabled)
                            Trace.WriteLine( Properties.Resources.Trace_ResetCAM, BDASettings.BDATraceSwitch.DisplayName );

                        // Reset the CAM
                        control.Reset();

                        // We did it once
                        m_HasBeenReset = true;
                    }
                }

            // Start processor
            token.WaitForPMTs(
                pmt =>
                {
                    // See if we are still allowed to process and do so
                    lock (m_deviceAccess)
                        if (Thread.VolatileRead( ref m_ChangeCounter ) == callIdentifier)
                            Decrypt( pmt );
                        else
                            return false;

                    // Next
                    return true;
                }, sources );

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
