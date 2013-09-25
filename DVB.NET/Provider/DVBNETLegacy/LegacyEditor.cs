extern alias oldVersion;

using System;
using System.Xml;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

using JMS.DVB.Editors;

using legacy = oldVersion::JMS;


namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Dialog zur Pflege der Parameter einer alten (vor 3.9) DVB.NET Geräteabstraktion.
    /// </summary>
    public partial class LegacyEditor : UserControl, IHardwareEditor
    {
        /// <summary>
        /// Alle Parameter des aktuellen Gerätetyps.
        /// </summary>
        private List<ParameterItem> m_Parameters = new List<ParameterItem>();

        /// <summary>
        /// Liest oder setzt das zu pflegende Geräteprofil.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        public LegacyEditor()
        {
            // Copy settings from designer
            InitializeComponent();

            // Fill in
            foreach (legacy.DVB.DeviceInformation device in ProfileTools.LegacyDevices)
            {
                // Remember
                selDevice.Items.Add( device );

                // Load stable list
                XmlElement[] paramList = device.Parameters.Cast<XmlElement>().ToArray();

                // Update editor types
                foreach (XmlElement param in paramList)
                {
                    // Load editor
                    string editor = param.GetAttribute( "editor" );

                    // Not set
                    if (string.IsNullOrEmpty( editor ))
                        continue;

                    // Unsupported or changed editors
                    switch (editor)
                    {
                        case "JMS.DVB.Provider.BDA.Editors.Frontend, JMS.DVB.Provider.BDA": param.ParentNode.RemoveChild( param ); break;
                    }
                }
            }
        }

        /// <summary>
        /// Meldet, ob die aktuellen Eingaben zulässig sind und ein Speichern möglich wäre.
        /// </summary>
        bool IHardwareEditor.IsValid
        {
            get
            {
                // Check it
                return m_Parameters.All( p => p.IsValid );
            }
        }

        /// <summary>
        /// Kopiert die aktuelle Eingabe in das aktuelle Geräteprofil.
        /// </summary>
        void IHardwareEditor.UpdateProfile()
        {
            // Get the device selected
            legacy.DVB.DeviceInformation device = CurrentDevice;

            // Wipe out all
            Profile.DeviceAspects.Clear();
            Profile.Parameters.Clear();

            // Done
            if (null == device)
                return;

            // Set legacy driver
            Profile.DeviceAspects.Add( new DeviceAspect { Value = CurrentDevice.DriverType } );

            // Find the moniker flag
            var ignore = m_Parameters.FirstOrDefault( i => (i.Value != null) && Equals( i.Text, "IgnoreMoniker" ) );

            // See if we are allowed to use monikers
            bool allowMoniker;
            if (ignore == null)
                allowMoniker = true;
            else if (bool.TryParse( ignore.Value.DisplayText, out allowMoniker ))
                allowMoniker = !allowMoniker;
            else
                allowMoniker = true;

            // All parameters
            foreach (ParameterItem item in m_Parameters)
                if (item.IsEditable)
                {
                    // Get the value
                    var value = item.Value;
                    if (null == value)
                        continue;

                    // Store the value as is
                    this[item.Text] = value.DisplayText;

                    // Add moniker variant
                    if (allowMoniker)
                        if (!Equals( value.DisplayText, value.Value ))
                            this[item.Text + "Moniker"] = value.Value;
                }
                else
                {
                    // Use default
                    this[item.Text] = item.SubItems[1].Text;
                }
        }

        /// <summary>
        /// Liest oder setzt die aktuell ausgewählte Implementierung.
        /// </summary>
        private legacy.DVB.DeviceInformation CurrentDevice
        {
            get
            {
                // Report
                return (legacy.DVB.DeviceInformation) selDevice.SelectedItem;
            }
            set
            {
                // Update
                selDevice.SelectedItem = value;

                // Refresh GUI
                selDevice_SelectionChangeCommitted( selDevice, EventArgs.Empty );
            }
        }

        /// <summary>
        /// Liest oder setzt einen Parameter im aktuellen Geräteprofil.
        /// </summary>
        /// <param name="parameterName">Der Name des Parameters.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Name angegeben.</exception>
        private string this[string parameterName]
        {
            get
            {
                // Validate
                if (string.IsNullOrEmpty( parameterName ))
                    throw new ArgumentNullException( "parameterName" );

                // Find the parameter
                ProfileParameter parameter = Profile.Parameters.Find( p => parameterName.Equals( p.Name ) );

                // Report
                if (null == parameter)
                    return null;
                else
                    return parameter.Value;
            }
            set
            {
                // Validate
                if (string.IsNullOrEmpty( parameterName ))
                    throw new ArgumentNullException( "parameterName" );

                // Remove the parameter
                Profile.Parameters.RemoveAll( p => parameterName.Equals( p.Name ) );

                // Add a new one
                if (null != value)
                    Profile.Parameters.Add( new ProfileParameter { Name = parameterName, Value = value } );
            }
        }

        /// <summary>
        /// Wird aktiviert, sobald eine Geräteimplementierung aufgerufen wird.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selDevice_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Clear all lists
            lstParams.Items.Clear();
            lstCards.Items.Clear();
            m_Parameters.Clear();

            // Skip
            if (null == CurrentDevice)
                return;

            // Fill
            lstCards.Items.AddRange( CurrentDevice.Names );

            // Find all parameters
            foreach (XmlElement param in CurrentDevice.Parameters)
            {
                // Get the values
                string display = this[param.Name];
                string store = display;

                // Special
                if (null != display)
                {
                    // Check for moniker representation
                    string moniker = this[param.Name + "Moniker"];

                    // Use this
                    if (null != moniker)
                        store = moniker;
                }

                // Create the item
                ParameterItem item = new ParameterItem( param, display, store );

                // Remember in general list
                m_Parameters.Add( item );

                // Add it to the list
                lstParams.Items.Add( item );
            }

            // Finsish GUI
            foreach (ColumnHeader header in lstParams.Columns)
            {
                // Header only
                header.AutoResize( ColumnHeaderAutoResizeStyle.HeaderSize );

                // Remember
                int width1 = header.Width;

                // Content
                header.AutoResize( ColumnHeaderAutoResizeStyle.ColumnContent );

                // Remember
                int width2 = header.Width;

                // Reset
                header.AutoResize( ColumnHeaderAutoResizeStyle.None );

                // Fix it
                header.Width = Math.Max( width1, width2 );
            }
        }

        /// <summary>
        /// Aktiviert die Bearbeitung eines einzelnen Parameters.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void lstParams_DoubleClick( object sender, EventArgs e )
        {
            // Check
            if (1 != lstParams.SelectedItems.Count)
                return;

            // Get the selection
            ParameterItem param = (ParameterItem) lstParams.SelectedItems[0];

            // Process
            if (param.IsEditable)
                param.Edit( ParentForm );
        }

        /// <summary>
        /// Bereitet die Anzeige des Dialogs vor.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void LegacyEditor_Load( object sender, EventArgs e )
        {
            // Find the legacy provider
            DeviceAspect type = Profile.DeviceAspects.Find( a => string.IsNullOrEmpty( a.Aspekt ) );

            // Find the device
            if (null != type)
                foreach (legacy.DVB.DeviceInformation device in selDevice.Items)
                    if (Equals( type.Value, device.DriverType ))
                    {
                        // Select
                        selDevice.SelectedItem = device;

                        // Force refresh
                        selDevice_SelectionChangeCommitted( selDevice, EventArgs.Empty );

                        // Done
                        break;
                    }
        }
    }
}
