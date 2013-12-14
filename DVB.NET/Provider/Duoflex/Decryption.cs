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
        private DataGraph m_dataGraph;

        /// <summary>
        /// Die laufende Nummer unseres Filters.
        /// </summary>
        private int m_filterIndex = -1;

        /// <summary>
        /// Gesetzt, sobald das CAM einmal zurückgesetzt wurde.
        /// </summary>
        private bool m_hasBeenReset;

        /// <summary>
        /// Gesetzt, wenn bei Änderungen des Datenstroms kurzzeitig die Entschlüsselung
        /// deaktiviert werden soll.
        /// </summary>
        private bool m_disableOnChange;

        /// <summary>
        /// Die Wartezeit in Sekunden vor der Übergabe einer Entschlüsselung nach einer Veränderung
        /// der Senderdaten.
        /// </summary>
        private int m_changeDelay;

        /// <summary>
        /// Die Art, wie das CI/CAM Zurücksetzen behandelt werden soll.
        /// </summary>
        private SuppressionMode m_suppress;

        /// <summary>
        /// Zählt die Aufrufe der Entschlüsselungsmethode <see cref="Decrypt(DataGraph.DecryptToken)"/>.
        /// </summary>
        private int m_changeCounter;

        /// <summary>
        /// Steuert den Zugriff auf die Hardware.
        /// </summary>
        private readonly object m_deviceAccess = new object();

        /// <summary>
        /// Entschlüsselt eine einzelne Quelle.
        /// </summary>
        /// <param name="service">Die Informationen zur Quelle.</param>
        /// <param name="graph">Der zu verwendende Graph.</param>
        public void Decrypt( ushort service, DataGraph graph )
        {
            // Check COM interface
            var controlPtr = ComIdentity.QueryInterface( graph.AdditionalFilters[m_filterIndex].Interface, typeof( KsControl.Interface ) );
            if (controlPtr != IntPtr.Zero)
                using (var control = new KsControl( controlPtr ))
                    control.SetServices( service );
        }

        /// <summary>
        /// Aktiviert die Entschlüsselung einer Quelle.
        /// </summary>
        /// <param name="token">Informationen zur gewählten Quelle.</param>
        public PipelineResult Decrypt( DataGraph.DecryptToken token )
        {
            // Load graph
            if (token != null)
                m_dataGraph = token.Pipeline.Graph;

            // Get unique call identifier
            var callIdentifier = Interlocked.Increment( ref m_changeCounter );

            // See if we can do anything
            if (m_dataGraph == null)
                return PipelineResult.Continue;
            if (m_filterIndex < 0)
                return PipelineResult.Continue;
            if (m_filterIndex >= m_dataGraph.AdditionalFilters.Count)
                return PipelineResult.Continue;

            // Deactivate if CAM reset is forbidden
            var sources = (token == null) ? null : token.Sources;
            var noSources = (sources == null) || (sources.Length < 1);
            if ((noSources && (m_suppress != SuppressionMode.Complete)) || !m_hasBeenReset)
                lock (m_deviceAccess)
                {
                    // Check COM interface
                    var controlPtr = ComIdentity.QueryInterface( m_dataGraph.AdditionalFilters[m_filterIndex].Interface, typeof( KsControl.Interface ) );
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
                        m_hasBeenReset = true;
                    }
                }

            // Start processor
            token.WaitForPMTs(
                ( pmt, first ) =>
                {
                    // See if we are still allowed to process and do so
                    lock (m_deviceAccess)
                    {
                        // No longer current
                        if (Thread.VolatileRead( ref m_changeCounter ) != callIdentifier)
                            return false;

                        // Try reset
                        if (!first)
                            if (m_disableOnChange)
                                Decrypt( 0, m_dataGraph );

                        // Wait for it
                        if (m_changeDelay > 0)
                            Thread.Sleep( m_changeDelay );

                        // Regular
                        Decrypt( pmt.ProgramNumber, m_dataGraph );
                    }

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
                m_suppress = AdditionalFilterSelector.GetSuppression( PipelineTypes.CICAM, profile.Parameters );
                m_hasBeenReset = (m_suppress != SuppressionMode.None);

                // Analyse settings
                if (!bool.TryParse( profile.Parameters.GetParameter( PipelineTypes.CICAM, AdditionalFilterSelector.CancelEncryptionOnChangedStream ), out m_disableOnChange ))
                    m_disableOnChange = false;
                if (!int.TryParse( profile.Parameters.GetParameter( PipelineTypes.CICAM, AdditionalFilterSelector.DelayOnChangedStream ), out m_changeDelay ))
                    m_changeDelay = 0;

                // Register in pipeline
                graph.DecryptionPipeline.AddPostProcessing( Decrypt );

                // Check for additional filter
                var filterDisplayName = profile.Parameters.GetParameter( PipelineTypes.CICAM, AdditionalFilterSelector.FilterName );
                if (!string.IsNullOrEmpty( filterDisplayName ))
                {
                    // Load moniker - if available
                    var filterMoniker = profile.Parameters.GetParameter( PipelineTypes.CICAM, AdditionalFilterSelector.FilterMoniker );

                    // Remember position of our extension
                    m_filterIndex = graph.AdditionalFilterInformations.Count;

                    // Register in graph
                    graph.AdditionalFilterInformations.Add( DeviceAndFilterInformations.Cache.AllFilters.FindFilter( filterDisplayName, filterMoniker ) );
                }
            }
        }

        #endregion
    }
}
