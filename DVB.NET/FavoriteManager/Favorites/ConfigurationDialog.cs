using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JMS.DVB.Favorites
{
	public partial class ConfigurationDialog : Form
	{
		public ConfigurationDialog(ChannelSelector selector)
		{
			// Fill self
			InitializeComponent();

			// Fill inner
			configuationControl.Initialize(selector);
		}

		private void configuationControl_ConfigurationFinished(DialogResult result)
		{
			// Take over
			DialogResult = result;

			// Done
			Close();
		}
	}
}