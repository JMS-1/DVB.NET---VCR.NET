using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace TVBrowserPlugIn
{
    public partial class ChooseMapping : Form
    {
        public string MappedName = null;

        public ChooseMapping( string channelName, IEnumerable<string> channels )
        {
            // Core
            InitializeComponent();

            // Update
            Text = string.Format( Text, channelName );

            // Fill
            selStation.Items.AddRange( channels.ToArray() );
        }

        private void ChooseMapping_Load( object sender, EventArgs e )
        {

        }

        private void selStation_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Enable
            cmdChoose.Enabled = (selStation.SelectedItem != null);
        }

        private void cmdChoose_Click( object sender, EventArgs e )
        {
            // Remember
            MappedName = (string) selStation.SelectedItem;
        }
    }
}