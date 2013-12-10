using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EPGReader
{
    public partial class EPGDisplay : Form
    {
        public EPGDisplay( EPGEntry entry )
        {
            // Set up
            InitializeComponent();

            // Load
            Text = entry.SubItems[3].Text;
            lbName.Text = Text;
            lbTime.Text = string.Format( "{0} - {1}", entry.CompareData[1], entry.CompareData[2] );
            lbDescription.Text = entry.SubItems[4].Text;

            // Check for identifier
            var identifier = entry.SubItems[5].Text;
            if (!string.IsNullOrEmpty( identifier ))
                Text += string.Format( " [{0}]", identifier );
        }

        private void EPGDisplay_Load( object sender, EventArgs e )
        {

        }

        private void cmdCopy_Click( object sender, EventArgs e )
        {
            // Load
            Clipboard.Clear();
            Clipboard.SetText( string.Format( "{0}\r\n{1}\r\n{2}\r\n", lbName.Text, lbTime.Text, lbDescription.Text ) );

            // Done
            Close();
        }
    }
}