using System;
using System.Collections.Generic;
using System.Windows.Forms;
using JMS.DVB.DeviceAccess.Enumerators;
using JMS.DVB.Editors;


namespace JMS.DVB.Provider.Duoflex
{
    /// <summary>
    /// Erlaubt dem Anwender die Auswahl eines zusätzlichen <i>DirectShow</i> Filters,
    /// der in den Graphen geladen wird.
    /// </summary>
    public partial class AdditionalFilterSelector : Form
    {
        /// <summary>
        /// Der Name des Parameters mit dem eindeutigen Namen.
        /// </summary>
        public const string FilterMoniker = "DDAdditionalFilterMoniker";

        /// <summary>
        /// Der Name des Parameters mit dem Anzeigenamen.
        /// </summary>
        public const string FilterName = "DDAdditionalFilterName";

        /// <summary>
        /// Der Name des Parameters mit dem Auswahlverfahren.
        /// </summary>
        public const string IgnoreMoniker = "DDResolveByName";

        /// <summary>
        /// Der Name des Parameters zur Einstellung der CI/CAM Initialisierung.
        /// </summary>
        public const string ResetSuppressionMode = "DDSuppressCAMReset";

        /// <summary>
        /// Der Name des Parameters, der für eine kurzzeitige Deaktivierung der Entschlüsselung bei
        /// Änderungen der Senderdaten sorgt.
        /// </summary>
        public const string CancelEncryptionOnChangedStream = "DDCancelOnChange";

        /// <summary>
        /// Die Zeitverzögerung in Sekunden, bevor eine auf eine Änderung der Senderdaten an
        /// die Entschlüsselung weitergegeben wird.
        /// </summary>
        public const string DelayOnChangedStream = "DDDelayOnChange";

        /// <summary>
        /// Erzeugt ein neues Auswahlfenster.
        /// </summary>
        /// <param name="parameters">Die aktuelle Wahl der Parameter.</param>
        public AdditionalFilterSelector( List<ProfileParameter> parameters )
        {
            // Load designer stuff
            InitializeComponent();

            // Load the supression mode
            switch (GetSuppression( null, parameters ))
            {
                case SuppressionMode.Complete: ckReset.Checked = true; ckResetTune.Checked = false; break;
                case SuppressionMode.Startup: ckReset.Checked = true; break;
                default: ckResetTune.Enabled = false; break;
            }

            // Load flag
            bool disableOnChange;
            if (bool.TryParse( parameters.GetParameter( CancelEncryptionOnChangedStream ), out disableOnChange ))
                ckDisableOnChange.Checked = disableOnChange;
            else
                ckDisableOnChange.Checked = false;

            // Load delay
            int delay;
            if (int.TryParse( parameters.GetParameter( DelayOnChangedStream ), out delay ))
                udChangeDelay.Value = Math.Max( Math.Min( delay, udChangeDelay.Maximum ), udChangeDelay.Minimum );
            else
                udChangeDelay.Value = udChangeDelay.Minimum;

            // Load
            foreach (var information in DeviceAndFilterInformations.Cache.AllFilters)
                selFilter.Items.Add( new DeviceSelector( information ) );

            // Check for done
            selFilter.Enabled = (selFilter.Items.Count > 0);
            if (!selFilter.Enabled)
                return;

            // Forget about sorting
            selFilter.Sorted = false;

            // Add a null option
            selFilter.Items.Insert( 0, Properties.Resources.Select_NoDevice );

            // Pre select
            selFilter.SelectedIndex = 0;

            // Load name
            var displayName = parameters.GetParameter( FilterName );
            if (string.IsNullOrEmpty( displayName ))
                return;

            // Load flag
            bool flag;
            if (bool.TryParse( parameters.GetParameter( IgnoreMoniker ), out flag ))
                ckDisplayOnly.Checked = flag;

            // Load moniker
            var moniker = flag ? null : parameters.GetParameter( FilterMoniker );

            // Try to re-select
            DeviceSelector.Select( selFilter, displayName, moniker );
        }

        /// <summary>
        /// Ermittelt die Art der Unterdrückung der CI/CAM Initialisierung.
        /// </summary>
        /// <param name="type">Die Art der Erweiterung, die im Namen zu berücksichtigen ist.</param>
        /// <param name="parameters">Die Liste aller Parameter.</param>
        /// <returns>Der aktuell gültige Parameter.</returns>
        internal static SuppressionMode GetSuppression( PipelineTypes? type, IEnumerable<ProfileParameter> parameters )
        {
            // Load native
            string setting;
            if (type.HasValue)
                setting = parameters.GetParameter( type.Value, ResetSuppressionMode );
            else
                setting = parameters.GetParameter( ResetSuppressionMode );

            // Convert
            if (!string.IsNullOrEmpty( setting ))
                try
                {
                    // Convert
                    return (SuppressionMode) Enum.Parse( typeof( SuppressionMode ), setting );
                }
                catch
                {
                    // Ignore any error - would prefer a TryParse...
                }

            // Default
            return SuppressionMode.None;
        }

        /// <summary>
        /// Aktualisiert die Einstellungen.
        /// </summary>
        /// <param name="parameters">Die Liste der Einstellungen.</param>
        public void Update( List<ProfileParameter> parameters )
        {
            // Reset
            parameters.Clear();

            // Get the suppression mode
            if (ckReset.Checked)
            {
                // Load the mode
                var mode = ckResetTune.Checked ? SuppressionMode.Startup : SuppressionMode.Complete;

                // Create parameter
                parameters.Add( new ProfileParameter( ResetSuppressionMode, mode ) );
            }

            // Copy change behaviour
            if (ckDisableOnChange.Checked)
                parameters.Add( new ProfileParameter( CancelEncryptionOnChangedStream, true ) );

            // Copy delay
            var delay = (int) udChangeDelay.Value;
            if (delay > 0)
                parameters.Add( new ProfileParameter( DelayOnChangedStream, delay ) );

            // Check selection
            var selected = selFilter.SelectedItem as DeviceSelector;
            if (selected == null)
                return;

            // Always add displayname and flag
            parameters.Add( new ProfileParameter( FilterName, selected.Information.DisplayName ) );
            parameters.Add( new ProfileParameter( IgnoreMoniker, ckDisplayOnly.Checked ) );

            // Add moniker as well
            if (!ckDisplayOnly.Checked)
                parameters.Add( new ProfileParameter( FilterMoniker, selected.Information.UniqueName ) );
        }

        /// <summary>
        /// Der Anwender hat die Auswahl für das CAM Reset verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ckReset_CheckedChanged( object sender, EventArgs e )
        {
            // Modify partner
            ckResetTune.Enabled = ckReset.Checked;
        }
    }
}
