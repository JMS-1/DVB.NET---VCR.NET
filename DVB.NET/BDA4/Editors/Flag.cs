using System;
using System.Windows.Forms;


namespace JMS.DVB.DeviceAccess.Editors
{
    /// <summary>
    /// Erlaubt die Pflege eines Wahrheitswertes.
    /// </summary>
    public partial class Flag : Form, IParameterEditor
    {
        /// <summary>
        /// Der Name des Parameters.
        /// </summary>
        private string m_Name = null;

        /// <summary>
        /// Der aktuelle Wert in Textform.
        /// </summary>
        private ParameterValue m_Value = null;
                
        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        public Flag()
        {
            // Set up
            InitializeComponent();
        }

        #region IParameterEditor Members

        /// <summary>
        /// Ruft einen Änderungsdialog auf.
        /// </summary>
        /// <param name="dialog">Der Dialog, von dem aus der Aufruf erfolgt.</param>
        /// <param name="parameterName">Der Name eines Parameters in einem DVB.NET Geräteprofil.</param>
        /// <param name="parameterValue">Der Wert des Parameter als Anzeigewert und zu speicherndem Wert.</param>
        /// <returns>Gesetzt, wenn irgend eine Ver#nderung vorgenommen wurde.</returns>
        bool IParameterEditor.Edit( IWin32Window dialog, string parameterName, ref ParameterValue parameterValue )
        {
            // Remember context
            m_Name = parameterName;
            m_Value = parameterValue;

            // Run
            if (DialogResult.OK != ShowDialog( dialog )) return false;

            // Update
            parameterValue = m_Value;

            // Report
            return true;
        }

        #endregion

        /// <summary>
        /// Wird bei Aktivierung des Dialogs einmalig aufgerufen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void Flag_Load( object sender, EventArgs e )
        {
            // Load setting
            ckSetting.Text = m_Name;
            ckSetting.Checked = (m_Value != null) && bool.Parse( m_Value.DisplayText );
        }

        /// <summary>
        /// Der Wahrheitswert wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ckSetting_CheckedChanged( object sender, EventArgs e )
        {
            // Load the value
            var value = (m_Value != null) && bool.Parse( m_Value.DisplayText );

            // Enabled button
            cmdSave.Enabled = (ckSetting.Checked != value);
        }

        /// <summary>
        /// Der neue Wahrheitswert soll gespeichert werden.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdSave_Click( object sender, EventArgs e )
        {
            // Convert
            var asString = ckSetting.Checked.ToString();

            // Update
            m_Value = new ParameterValue( asString );
        }
    }
}