using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

using JMS.DVB.DeviceAccess.Enumerators;


namespace JMS.DVB.DeviceAccess.Editors
{
    /// <summary>
    /// Mit diesem Dialog werden Ger�te ausgew�hlt.
    /// </summary>
    public partial class TunerCapture : Form, IParameterEditor
    {
        /// <summary>
        /// Die aktuelle Auswahl.
        /// </summary>
        private ParameterValue m_Value;

        /// <summary>
        /// Der Name eines Parameters in einem DVB.NET Ger�teprofil, der gerade bearbeitet wird.
        /// </summary>
        private string m_Name;

        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        public TunerCapture()
        {
            // Set up
            InitializeComponent();
        }

        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        /// <param name="names">Die Liste der m�glichen Ger�te.</param>
        public TunerCapture( IEnumerable<IDeviceOrFilterInformation> names )
            : this()
        {
            // Forward
            SetSelections( names );
        }

        /// <summary>
        /// F�llt die Liste m�glicher Ger�te.
        /// </summary>
        /// <param name="infos">Die vorgegebene Liste.</param>
        private void SetSelections( IEnumerable<IDeviceOrFilterInformation> infos )
        {
            // Reload
            var sorted = new List<IDeviceOrFilterInformation>();

            // Load
            if (null != infos)
                sorted.AddRange( infos );

            // Sort by name and then by unique identifier
            sorted.Sort( ( l, r ) => { int d = l.DisplayName.CompareTo( r.DisplayName ); if (0 != d) return d; return l.UniqueName.CompareTo( r.UniqueName ); } );

            // Load
            lstDevices.Items.AddRange( sorted.ToArray() );
        }

        #region IParameterEditor Members

        /// <summary>
        /// Ruft einen �nderungsdialog auf.
        /// </summary>
        /// <param name="dialog">Der Dialog, von dem aus der Aufruf erfolgt.</param>
        /// <param name="parameterName">Der Name eines Parameters in einem DVB.NET Ger�teprofil.</param>
        /// <param name="parameterValue">Der Wert des Parameter als Anzeigewert und zu speicherndem Wert.</param>
        /// <returns>Gesetzt, wenn irgend eine Ver#nderung vorgenommen wurde.</returns>
        bool IParameterEditor.Edit( IWin32Window dialog, string parameterName, ref ParameterValue parameterValue )
        {
            // Remember
            m_Value = parameterValue;
            m_Name = parameterName;

            // Run
            if (DialogResult.OK != ShowDialog( dialog ))
                return false;

            // Copy over
            parameterValue = m_Value;

            // Done
            return true;
        }

        #endregion

        /// <summary>
        /// Bereitet diesen Dialog zur Anzeige vor.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void TunerCapture_Load( object sender, EventArgs e )
        {
            // Create name
            Text = string.Format( Text, m_Name );

            // Enable
            cmdReset.Enabled = (null != m_Value);

            // Disable selection
            lstDevices.SelectedItem = null;

            // Lookup
            if (null != m_Value)
            {
                // By unique identifier
                foreach (IDeviceOrFilterInformation info in lstDevices.Items)
                    if (Equals( m_Value.Value, info.UniqueName ))
                    {
                        // Use this
                        m_Value = new ParameterValue( info.DisplayName, info.UniqueName );

                        // Select
                        lstDevices.SelectedItem = info;

                        // Done
                        break;
                    }

                // By display name
                if (null == lstDevices.SelectedItem)
                    foreach (IDeviceOrFilterInformation info in lstDevices.Items)
                        if (Equals( m_Value.Value, info.DisplayName ))
                        {
                            // Use this
                            m_Value = new ParameterValue( info.DisplayName, info.UniqueName );

                            // Select
                            lstDevices.SelectedItem = info;

                            // Done
                            break;
                        }
            }
        }

        /// <summary>
        /// �bernimmt den ausgew�hlten Wert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdSave_Click( object sender, EventArgs e )
        {
            // Load
            var info = (IDeviceOrFilterInformation) lstDevices.SelectedItem;

            // Store
            m_Value = (info == null) ? null : new ParameterValue( info.DisplayName, info.UniqueName );
        }

        /// <summary>
        /// Die Auswahl des Ger�tes wurde ver�ndert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void lstDevices_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Load the selection
            var info = (IDeviceOrFilterInformation) lstDevices.SelectedItem;

            // Update tooltip
            tips.SetToolTip( lstDevices, (null == info) ? null : info.DisplayName );

            // Set the button
            cmdSave.Enabled = (info != m_Value);
        }

        /// <summary>
        /// Beendet die Auswahl eines ger�tes �ber einen Doppelklick.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void lstDevices_DoubleClick( object sender, EventArgs e )
        {
            // Forward
            cmdSave.PerformClick();
        }

        /// <summary>
        /// Entfernt die Ger�teauswahl vollst�ndig.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdReset_Click( object sender, EventArgs e )
        {
            // Reset value
            m_Value = null;
        }
    }

    /// <summary>
    /// Dialog zur Auswahl eines Multimedia Ger�tes.
    /// </summary>
    public class Media : TunerCapture
    {
        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        public Media()
            : base( DeviceAndFilterInformations.Cache.MediaDevices.Cast<IDeviceOrFilterInformation>() )
        {
        }
    }
}