using System.Windows.Forms;


namespace JMS.DVB.Favorites
{
    /// <summary>
    /// Erlaubt die Manipulat der Konfiguration.
    /// </summary>
    public partial class ConfigurationDialog : Form
    {
        /// <summary>
        /// Erstellt einen neuen Konfigurationsdialog.
        /// </summary>
        /// <param name="selector">Die zugehörige Auswahlkomponente.</param>
        public ConfigurationDialog( ChannelSelector selector )
        {
            // Fill self
            InitializeComponent();

            // Fill inner
            configuationControl.Initialize( selector );
        }

        private void configuationControl_ConfigurationFinished( DialogResult result )
        {
            // Take over
            DialogResult = result;

            // Done
            Close();
        }
    }
}