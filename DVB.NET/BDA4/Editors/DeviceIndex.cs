using System;
using System.Windows.Forms;


namespace JMS.DVB.DeviceAccess.Editors
{
    /// <summary>
    /// Erlaubt die Bearbeitung einer laufenden Nummer.
    /// </summary>
    public partial class DeviceIndex : Form, IParameterEditor
    {
        /// <summary>
        /// Der Name des Parameters, der gepflegt wird.
        /// </summary>
        private string m_Name = null;

        /// <summary>
        /// Der urspr�ngliche Wert des Parameters als Zeichenkette.
        /// </summary>
        private ParameterValue m_Value = null;

        /// <summary>
        /// Erzeugt ein neues Formular.
        /// </summary>
        public DeviceIndex()
        {
            // Set up
            InitializeComponent();
        }

        #region IParameterEditor Members

        /// <summary>
        /// Bietet einen Parameter zum �ndern an.
        /// </summary>
        /// <param name="dialog">Das �bergeordnete Fenster.</param>
        /// <param name="parameterName">Der Name des Parameters.</param>
        /// <param name="parameterValue">Der Wert des Parameters als Zeichenkette.</param>
        /// <returns>Gesetzt, wenn der Wert ver�ndert wurde.</returns>
        public virtual bool Edit( IWin32Window dialog, string parameterName, ref ParameterValue parameterValue )
        {
            // Remember context
            m_Name = parameterName;
            m_Value = parameterValue;

            // Run
            if (ShowDialog( dialog ) != DialogResult.OK)
                return false;

            // Update
            parameterValue = m_Value;

            // Report
            return true;
        }

        #endregion

        /// <summary>
        /// �bernimmt den aktuellen Wert des Parameters.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void DeviceIndex_Load( object sender, EventArgs e )
        {
            // Load setting
            selIndex.Value = (m_Value == null) ? 0 : int.Parse( m_Value.DisplayText );
        }

        /// <summary>
        /// Der Anwender m�chte einen ver�nderten Wert �bernehmen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdSave_Click( object sender, EventArgs e )
        {
            // Convert
            var asString = selIndex.Value.ToString();

            // Update
            m_Value = new ParameterValue( asString );
        }

        /// <summary>
        /// Der Anwender hat den Wert ver�ndert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selIndex_ValueChanged( object sender, EventArgs e )
        {
            // Convert
            var value = (m_Value == null) ? 0 : int.Parse( m_Value.DisplayText );

            // Enabled button
            cmdSave.Enabled = (selIndex.Value != value);
        }
    }
}