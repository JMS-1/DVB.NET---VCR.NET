using System;
using System.Data;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace TVBrowserPlugIn
{
	public partial class Configuration : Form
	{
		public Configuration()
		{
			InitializeComponent();
		}

		private void Configuration_Load(object sender, EventArgs e)
		{
			// Load all
			txConnection.Text = Properties.Settings.Default.TVBrowserPlugIn_VCRNETService_VCRServer30;
			txPre.Text = Properties.Settings.Default.PreTime.ToString();
			txPost.Text = Properties.Settings.Default.PostTime.ToString();
			ckOpen.Checked = Properties.Settings.Default.ShowConfirmation;

			// All mappings
			StringCollection externals = Properties.Settings.Default.ExternalNames;
			StringCollection internals = Properties.Settings.Default.VCRNETNames;

			// Fill
			if ((null != externals) && (null != internals))
				for (int i = 0, imax = Math.Min(externals.Count, internals.Count); i < imax; ++i)
				{
					// New item
					ListViewItem item = lstMappings.Items.Add(externals[i]);

					// Set name
					item.SubItems.Add(internals[i]);
				}

			// Can save?
			cmdSave.Enabled = false;
		}

		private void lstMappings_DoubleClick(object sender, EventArgs e)
		{
			// Get selection
			List<int> selections = new List<int>();

			// Fill
			foreach (int index in lstMappings.SelectedIndices) selections.Add(index);

			// Check it
			if (selections.Count < 1) return;

			// Sort
			selections.Sort();

			// Clear
			for (int i = selections.Count; i-- > 0; ) lstMappings.Items.RemoveAt(selections[i]);

			// Can save?
			CheckInput();
		}

		private void CheckInput(object sender, EventArgs e)
		{
			// Can save
			CheckInput();
		}

		private void CheckInput()
		{
			// Disable
			cmdSave.Enabled = true;

			// Test numbers
			int test;
			if (!int.TryParse(txPre.Text, out test) || (test < 0))
			{
				// Mark
				errors.SetError(txPre, Properties.Resources.BadNumber);

				// Do not save
				cmdSave.Enabled = false;
			}
			else
			{
				// Ok
				errors.SetError(txPre, null);
			}
			if (!int.TryParse(txPost.Text, out test) || (test < 0))
			{
				// Mark
				errors.SetError(txPost, Properties.Resources.BadNumber);

				// Do not save
				cmdSave.Enabled = false;
			}
			else
			{
				// Ok
				errors.SetError(txPost, null);
			}

			// Test URL
			try
			{
				// Process
				Uri uri = new Uri(txConnection.Text);

				// Ok
				errors.SetError(txConnection, null);
			}
			catch (Exception ex)
			{
				// Report
				errors.SetError(txConnection, ex.Message);

				// Do not save
				cmdSave.Enabled = false;
			}
		}

		private void cmdSave_Click(object sender, EventArgs e)
		{
			// Save all
			try
			{
				// Copy back
				Properties.Settings.Default.TVBrowserPlugIn_VCRNETService_VCRServer30 = txConnection.Text;
				Properties.Settings.Default.PreTime = int.Parse(txPre.Text);
				Properties.Settings.Default.PostTime = int.Parse(txPost.Text);
				Properties.Settings.Default.ShowConfirmation = ckOpen.Checked;

				// Lists
				StringCollection externals = null, internals = null;

				// Check mode
				if (lstMappings.Items.Count > 0)
				{
					// Create
					externals = new StringCollection();
					internals = new StringCollection();

					// Fill
					foreach (ListViewItem item in lstMappings.Items)
					{
						// Store both
						externals.Add(item.Text);
						internals.Add(item.SubItems[1].Text);
					}
				}

				// Store
				Properties.Settings.Default.ExternalNames = externals;
				Properties.Settings.Default.VCRNETNames = internals;

				// Store
				Properties.Settings.Default.Save();

				// Done
				Close();
			}
			catch (Exception ex)
			{
				// Show
				MessageBox.Show(this, ex.Message, Text);
			}
		}
	}
}