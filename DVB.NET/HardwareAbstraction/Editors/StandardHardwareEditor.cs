using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Enumerators;


namespace JMS.DVB.Editors
{
    /// <summary>
    /// Erlaubt die Pflege der Parameter für ein DVB.NET 4.0 BDA Gerät.
    /// </summary>
    public partial class StandardHardwareEditor : UserControl, IHardwareEditor
    {
        /// <summary>
        /// Verwaltet die Oberflächenelemente für eine Art von Erweiterung.
        /// </summary>
        private class ProviderUI
        {
            /// <summary>
            /// Die Liste mit allen Erweiterungen dieser Art.
            /// </summary>
            public ComboBox Selector { get; private set; }

            /// <summary>
            /// Die Schaltfläche für die Detailinformationen zu dieser Art.
            /// </summary>
            public Button Details { get; private set; }

            /// <summary>
            /// Alle zugehörigen Parameter für diese eine Art von Erweiterung.
            /// </summary>
            public readonly List<ProfileParameter> Parameters = new List<ProfileParameter>();

            /// <summary>
            /// Die eindeutige Kennung aller Parameter dieser Art.
            /// </summary>
            private string m_TypePrefix;

            /// <summary>
            /// Erzeugt eine neue Verwaltung.
            /// </summary>
            /// <param name="type">Die konkrete Art der Erweiterung.</param>
            /// <param name="selector">Die Liste mit allen Erweiterungen.</param>
            /// <param name="details">Die Schaltfläche für die Pflege der Parameter.</param>
            public ProviderUI( PipelineTypes type, ComboBox selector, Button details )
            {
                // Remember
                m_TypePrefix = ProfileParameter.GetPrefixForExtensionParameter( type );
                Selector = selector;
                Details = details;
            }

            /// <summary>
            /// Initialisiert die Liste der Parameter aus einem Geräteprofil.
            /// </summary>
            /// <param name="profile">Das zu verwendende Geräteprofil.</param>
            public void Fill( Profile profile )
            {
                // All parameters for this type of extension
                Parameters.AddRange(
                    profile
                        .Parameters
                        .Where( p => !string.IsNullOrEmpty( p.Name ) && p.Name.StartsWith( m_TypePrefix ) )
                        .Select( p => new ProfileParameter( p.Name.Substring( m_TypePrefix.Length ), p.Value ) ) );
            }

            /// <summary>
            /// Aktualisiert ein Geräteprofil.
            /// </summary>
            /// <param name="profile">Das zu befüllende Profil.</param>
            public void Update( Profile profile )
            {
                // All parameters with prefix
                profile.Parameters.AddRange(
                    Parameters
                        .Select( p => new ProfileParameter( string.Format( "{0}{1}", m_TypePrefix, p.Name ), p.Value ) ) );
            }
        }

        /// <summary>
        /// Der Name des Parameters zur Unterdrückung des eindeutigen Namens.
        /// </summary>
        public const string Parameter_NoMoniker = "IgnoreMoniker";

        /// <summary>
        /// Zu allen Arten von Erweiterungen die Namen der zugehörigen Klassen.
        /// </summary>
        private static readonly Dictionary<PipelineTypes, List<ProviderSelector>> s_Types = new Dictionary<PipelineTypes, List<ProviderSelector>>();

        /// <summary>
        /// Führt globale Initialisierungen aus.
        /// </summary>
        static StandardHardwareEditor()
        {
            // Self
            RegisterPipelineAssembly( Assembly.GetExecutingAssembly() );

            // Attach to adapter directory
            var dir = RunTimeLoader.AdapterPath;
            if (!dir.Exists)
                return;

            // Process all potenial assemblies
            foreach (var file in dir.GetFiles( "*.dll" ))
                try
                {
                    // Load it
                    var ext = Assembly.LoadFrom( file.FullName );
                    if (ext != null)
                        RegisterPipelineAssembly( ext );
                }
                catch
                {
                    // Ignore the error
                }
        }

        /// <summary>
        /// Durchsucht eine Bilbiothek nach Aktionen.
        /// </summary>
        /// <param name="assembly">Eine Bibliothek.</param>
        private static void RegisterPipelineAssembly( Assembly assembly )
        {
            // Check all types
            foreach (var type in assembly.GetExportedTypes())
            {
                // Check attribute
                var attributes = type.GetCustomAttributes( typeof( PipelineAttribute ), true );
                if (attributes == null)
                    continue;

                // Process all
                foreach (PipelineAttribute attribute in attributes)
                    foreach (PipelineTypes pipeline in Enum.GetValues( typeof( PipelineTypes ) ))
                        if ((attribute.Types & pipeline) != 0)
                        {
                            // Load list
                            List<ProviderSelector> types;
                            if (!s_Types.TryGetValue( pipeline, out types ))
                                s_Types.Add( pipeline, types = new List<ProviderSelector>() );

                            // Remember
                            types.Add( new ProviderSelector( type, attribute ) );
                        }
            }
        }

        /// <summary>
        /// Liest oder setzt das zu verändernde Geräteprofil.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Vermerkt zu jeder Art von Erweiterung die zugehörige Liste.
        /// </summary>
        private Dictionary<PipelineTypes, ProviderUI> m_ProviderSelection = new Dictionary<PipelineTypes, ProviderUI>();

        /// <summary>
        /// Erzeugt ein neues Eingabefeld.
        /// </summary>
        public StandardHardwareEditor()
        {
            // Load designer stuff
            InitializeComponent();

            // Fill device lists
            Fill( selCapture, DeviceAndFilterInformations.Cache.CaptureFilters );
            Fill( selWakeup, DeviceAndFilterInformations.Cache.MediaDevices );
            Fill( selTuner, DeviceAndFilterInformations.Cache.TunerFilters );

            // Fill pipeline actions
            Fill( selSignal, cmdEditSig, PipelineTypes.SignalInformation );
            Fill( selDiSEqC, cmdEditDiSEqC, PipelineTypes.DiSEqC );
            Fill( selCICAM, cmdEditCAM, PipelineTypes.CICAM );
            Fill( selDVBS2, cmdEditS2, PipelineTypes.DVBS2 );
        }

        /// <summary>
        /// Füllt eine Auswahlliste mit Erweiterungen.
        /// </summary>
        /// <param name="target">Die zu befüllende Liste.</param>
        /// <param name="details">Die Schaltfläche zur Pflege der Detailparameter.</param>
        /// <param name="type">Die Art der Erweiterung.</param>
        private void Fill( ComboBox target, Button details, PipelineTypes type )
        {
            // Remember
            m_ProviderSelection.Add( type, new ProviderUI( type, target, details ) );

            // Connect
            details.Tag = type;
            target.Tag = type;

            // Request
            List<ProviderSelector> providers;
            if (s_Types.TryGetValue( type, out providers ))
                target.Items.AddRange( providers.ToArray() );

            // Check for done
            target.Enabled = (target.Items.Count > 0);
            details.Enabled = false;
            if (!target.Enabled)
                return;

            // Forget about sorting
            target.Sorted = false;

            // Add a null option
            target.Items.Insert( 0, Properties.Resources.Select_NoAction );

            // Pre select
            target.SelectedIndex = 0;
        }

        /// <summary>
        /// Füllt eine Auswahlliste mit Geräten oder Filtern.
        /// </summary>
        /// <typeparam name="T">Die konkrete Art der Information - letztlich identisch behandelt.</typeparam>
        /// <param name="target">Die zu befüllende Auswahlliste.</param>
        /// <param name="informations">Die bereitgestellten Informationen.</param>
        private void Fill<T>( ComboBox target, IEnumerable<T> informations ) where T : IDeviceOrFilterInformation
        {
            // Load
            foreach (var information in informations)
                target.Items.Add( new DeviceSelector( information ) );

            // Check for done
            target.Enabled = (target.Items.Count > 0);
            if (!target.Enabled)
                return;

            // Forget about sorting
            target.Sorted = false;

            // Add a null option
            target.Items.Insert( 0, Properties.Resources.Select_NoDevice );

            // Pre select
            target.SelectedIndex = 0;
        }

        #region IHardwareEditor Members

        /// <summary>
        /// Gesetzt, wenn ein Speichern möglich ist.
        /// </summary>
        bool IHardwareEditor.IsValid
        {
            get
            {
                // For now
                return (selTuner.SelectedItem is DeviceSelector);
            }
        }

        /// <summary>
        /// Aktualisiert das Geräteprofil.
        /// </summary>
        void IHardwareEditor.UpdateProfile()
        {
            // Reset profile
            Profile.DeviceAspects.Clear();
            Profile.Parameters.Clear();
            Profile.Pipeline.Clear();

            // Load pipeline
            foreach (var selection in m_ProviderSelection)
            {
                // Attach to the item
                var provider = selection.Value.Selector.SelectedItem as ProviderSelector;
                if (provider != null)
                    Profile.Pipeline.Add( new PipelineItem { SupportedOperations = selection.Key, ComponentType = provider.TypeName } );
            }

            // Flags and values
            Profile.Parameters.Add( new ProfileParameter( Parameter_NoMoniker, ckNoMoniker.Checked ) );
            Profile.Parameters.Add( new ProfileParameter( Hardware.Parameter_EnableWakeup, ckWakeup.Checked ) );
            Profile.Parameters.Add( new ProfileParameter( BDAEnvironment.MiniumPATCountName, (uint) selPATCount.Value ) );
            Profile.Parameters.Add( new ProfileParameter( BDAEnvironment.MinimumPATCountWaitName, (uint) selPATDelay.Value ) );

            // Tuner
            var device = selTuner.SelectedItem as DeviceSelector;
            if (device != null)
            {
                // Always store display name
                Profile.DeviceAspects.Add( new DeviceAspect { Aspekt = Hardware.Aspect_TunerName, Value = device.Information.DisplayName } );

                // May store moniker
                if (!ckNoMoniker.Checked)
                    Profile.DeviceAspects.Add( new DeviceAspect { Aspekt = Hardware.Aspect_TunerMoniker, Value = device.Information.UniqueName } );
            }

            // Capture
            device = selCapture.SelectedItem as DeviceSelector;
            if (device != null)
            {
                // Always store display name
                Profile.DeviceAspects.Add( new DeviceAspect { Aspekt = Hardware.Aspect_CaptureName, Value = device.Information.DisplayName } );

                // May store moniker
                if (!ckNoMoniker.Checked)
                    Profile.DeviceAspects.Add( new DeviceAspect { Aspekt = Hardware.Aspect_CaptureMoniker, Value = device.Information.UniqueName } );
            }

            // Wakeup
            if (ckWakeup.Checked)
            {
                // Attach to the selection
                device = selWakeup.SelectedItem as DeviceSelector;
                if (device != null)
                {
                    // Always store display name
                    Profile.Parameters.Add( new ProfileParameter( Hardware.Parameter_WakeupDevice, device.Information.DisplayName ) );

                    // May store moniker
                    if (!ckNoMoniker.Checked)
                        Profile.Parameters.Add( new ProfileParameter( Hardware.Parameter_WakeupMoniker, device.Information.UniqueName ) );
                }
            }

            // Extension data
            foreach (var provider in m_ProviderSelection.Values)
            {
                // Find current selection
                var selected = provider.Selector.SelectedItem as ProviderSelector;
                if (selected == null)
                    continue;

                // Fill it
                provider.Update( Profile );
            }
        }

        #endregion

        /// <summary>
        /// Lädt die Aktionen in das Formular.
        /// </summary>
        /// <param name="pipeline">Die Liste der Aktionen.</param>
        private void LoadPipeline( List<PipelineItem> pipeline )
        {
            // Process all
            foreach (var action in pipeline)
                foreach (PipelineTypes type in Enum.GetValues( typeof( PipelineTypes ) ))
                    if ((action.SupportedOperations & type) != 0)
                    {
                        // Forward to helper
                        ProviderUI information;
                        if (m_ProviderSelection.TryGetValue( type, out information ))
                            ProviderSelector.Select( information.Selector, action.ComponentType, this );
                    }
        }

        /// <summary>
        /// Lädt die Auswahl der Geräte.
        /// </summary>
        /// <param name="parameters">Die Parameter des Geräteprofils.</param>
        /// <param name="aspects">Die Konfiguration des Gerätes.</param>
        private void LoadDevices( List<ProfileParameter> parameters, List<DeviceAspect> aspects )
        {
            // Select flag
            bool suppressMoniker;
            if (bool.TryParse( parameters.GetParameter( Parameter_NoMoniker ), out suppressMoniker ))
                ckNoMoniker.Checked = suppressMoniker;

            // Load flag
            bool doWakeup;
            if (bool.TryParse( parameters.GetParameter( Hardware.Parameter_EnableWakeup ), out doWakeup ))
                ckWakeup.Checked = doWakeup;

            // Load settings
            DeviceSelector.Select( selWakeup, doWakeup ? parameters.GetParameter( Hardware.Parameter_WakeupDevice ) : null, (suppressMoniker || !doWakeup) ? null : parameters.GetParameter( Hardware.Parameter_WakeupMoniker ) );
            DeviceSelector.Select( selCapture, aspects.GetDeviceAspect( Hardware.Aspect_CaptureName ), suppressMoniker ? null : aspects.GetDeviceAspect( Hardware.Aspect_CaptureMoniker ) );
            DeviceSelector.Select( selTuner, aspects.GetDeviceAspect( Hardware.Aspect_TunerName ), suppressMoniker ? null : aspects.GetDeviceAspect( Hardware.Aspect_TunerMoniker ) );

            // Load numbers
            uint n;
            if (uint.TryParse( parameters.GetParameter( BDAEnvironment.MiniumPATCountName ), out n ))
                if (n >= selPATCount.Minimum)
                    if (n <= selPATCount.Maximum)
                        selPATCount.Value = n;
            if (uint.TryParse( parameters.GetParameter( BDAEnvironment.MinimumPATCountWaitName ), out n ))
                if (n >= selPATDelay.Minimum)
                    if (n <= selPATDelay.Maximum)
                        selPATDelay.Value = n;
        }

        /// <summary>
        /// Wird einmalig zur Anzeige des Formulars aufgerufen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void StandardHardwareEditor_Load( object sender, EventArgs e )
        {
            // See if we can translate from previous version
            try
            {
                // Check special lists
                selDiSEqC.Enabled = (Profile is SatelliteProfile);
                selDVBS2.Enabled = selDiSEqC.Enabled;

                // Check for conversion
                using (var hardware = Hardware.Create( Profile, true ))
                    if (hardware != null)
                    {
                        // Load from hardware
                        LoadPipeline( hardware.EffectivePipeline );
                        LoadDevices( hardware.EffectiveProfileParameters, hardware.EffectiveDeviceAspects );

                        // Done
                        return;
                    }
            }
            catch
            {
            }

            // Load from profile
            LoadPipeline( Profile.Pipeline );
            LoadDevices( Profile.Parameters, Profile.DeviceAspects );
        }

        /// <summary>
        /// Der Anwender hat die Auswahl einer Erweiterung verändert.
        /// </summary>
        /// <param name="sender">Die veränderte Liste.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void PipelineSelectionChanged( object sender, EventArgs e )
        {
            // Forward
            PipelineSelectionChanged( (ComboBox) sender, false );
        }

        /// <summary>
        /// Der Anwender hat die Auswahl einer Erweiterung verändert.
        /// </summary>
        /// <param name="list">Die veränderte Auswahl.</param>
        /// <param name="startup">Gesetzt, wenn die Auswahl erstmalig geladen wird.</param>
        internal void PipelineSelectionChanged( ComboBox list, bool startup )
        {
            // Attach to the selected item
            var selected = list.SelectedItem as ProviderSelector;
            var type = (PipelineTypes) list.Tag;
            var info = m_ProviderSelection[type];
            var details = info.Details;

            // Change selection
            details.Enabled = (selected != null) && selected.Attribute.HasAdditionalParameters;

            // Prepare parameters
            if (startup)
                info.Fill( Profile );
            else
                info.Parameters.Clear();
        }

        /// <summary>
        /// Ruft die Pflege der Parameter einer Erweiterung auf.
        /// </summary>
        /// <param name="sender">Die Schaltfläche zur Erweiterung.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void PipelineParameterEdit( object sender, EventArgs e )
        {
            // Attach to the selection
            var button = (Button) sender;
            var type = (PipelineTypes) button.Tag;
            var info = m_ProviderSelection[type];
            var selected = info.Selector.SelectedItem as ProviderSelector;
            if (selected == null)
                return;

            // Be safe
            try
            {
                // Load editor component
                var editor = (IPipelineParameterExtension) Activator.CreateInstance( selected.Type );

                // Run it
                editor.Edit( this, info.Parameters, type );
            }
            catch
            {
                // Silent discard
            }
        }
    }
}
