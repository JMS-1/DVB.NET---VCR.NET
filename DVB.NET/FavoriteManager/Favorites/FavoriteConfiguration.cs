using System;
using System.Data;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;

namespace JMS.DVB.Favorites
{
	public delegate void ConfigurationFinishHandler(DialogResult result);

	public partial class FavoriteConfiguration : UserControl
	{
		private UserSettings m_Configuration = null;

		public event ConfigurationFinishHandler ConfigurationFinished;
 
		public FavoriteConfiguration()
		{
			// Initialize
			InitializeComponent();
		}

		public void Initialize(ChannelSelector selector)
		{
			// Remember
			m_Configuration = selector.Configuration;

			// All favorites
			Dictionary<string, bool> favorites = new Dictionary<string, bool>();

			// Fill favorites
			if (null != m_Configuration.FavoriteChannels)
				foreach (string favorite in m_Configuration.FavoriteChannels)
				{
					// Remember
					favorites[favorite] = true;

					// Add
					lstFavorites.Items.Add(favorite);
				}

			// Fill channels
			foreach (ChannelItem channel in selector.Channels)
				if (!favorites.ContainsKey(channel.ChannelName))
					lstAll.Items.Add(channel.ChannelName);

			// Fill language list
			foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
			{
				// Skip all sub-languages
				if (info.NativeName.IndexOf(" (") >= 0) continue;

				// Add to map
				selLanguage.Items.Add(info.NativeName);
			}

			// Copy over
			selLanguage.Text = m_Configuration.PreferredLanguage;
			ckUse.Checked = m_Configuration.EnableShortCuts;
			ckAC3.Checked = m_Configuration.PreferAC3;

			// Disable
			cmdSave.Enabled = false;
		}

		private void FavoriteConfiguration_Load(object sender, EventArgs e)
		{
			// Configure
			pnlLists_SizeChanged(this, EventArgs.Empty);
		}

		private void pnlLists_SizeChanged(object sender, EventArgs e)
		{
			// Disable
			lstAll.IntegralHeight = false;
			lstFavorites.IntegralHeight = false;

			// Process
			lstFavorites.Location = new Point(0, 0);
			lstFavorites.Width = pnlLists.Width / 2 - 20;
			lstFavorites.Height = pnlLists.Height;

			// Other
			lstAll.Location = new Point(lstFavorites.Left + lstFavorites.Width + 10, 0);
			lstAll.Width = lstFavorites.Width;
			lstAll.Height = lstFavorites.Height;

			// Enable
			lstAll.IntegralHeight = true;
			lstFavorites.IntegralHeight = true;
		}

		private void lstAll_DoubleClick(object sender, EventArgs e)
		{
			// Get the item
			string selected = (string)lstAll.SelectedItem;

			// None
			if (null == selected) return;

			// Remove in here
			lstAll.Items.RemoveAt(lstAll.SelectedIndex);

			// See if there is a selection
			if (lstFavorites.SelectedIndex < 0)
			{
				// Move over to the end
				lstFavorites.Items.Add(selected);
			}
			else
			{
				// Move in
				lstFavorites.Items.Insert(lstFavorites.SelectedIndex + 1, selected);
			}

			// Always select in list
			lstFavorites.SelectedItem = selected;

			// Enable
			cmdSave.Enabled = true;
		}

		private void lstFavorites_DoubleClick(object sender, EventArgs e)
		{
			// Get the item
			string selected = (string)lstFavorites.SelectedItem;

			// None
			if (null == selected) return;

			// Remove in here
			lstFavorites.Items.RemoveAt(lstFavorites.SelectedIndex);

			// Move over
			lstAll.Items.Add(selected);

			// Always select
			lstAll.SelectedItem = selected;

			// Enable
			cmdSave.Enabled = true;
		}

		private void lstFavorites_KeyUp(object sender, KeyEventArgs e)
		{
			// Get the index
			int i = lstFavorites.SelectedIndex;

			// Not possible
			if (i < 0) return;

			// Selection
			if (Keys.Return == e.KeyCode)
			{
				// Did it
				e.Handled = true;

				// Make it a double click
				lstFavorites_DoubleClick(lstFavorites, EventArgs.Empty);

				// Done
				return;
			}

			// Load
			string selected = (string)lstFavorites.Items[i];

			// Check the mode
			if (e.KeyCode == Keys.Up)
			{
				// Not possible
				if (i < 1) return;

				// Simpy move selection
				if (e.Shift)
				{
					// Move up
					lstFavorites.SelectedIndex = --i;

					// We did it
					e.Handled = true;

					// Done
					return;
				}

				// Remove
				lstFavorites.Items.RemoveAt(i);

				// Correct
				--i;
			}
			else if (e.KeyCode == Keys.Down)
			{
				// Not possible
				if (i >= (lstFavorites.Items.Count - 1)) return;

				// Simpy move selection
				if (e.Shift)
				{
					// Move up
					lstFavorites.SelectedIndex = ++i;

					// We did it
					e.Handled = true;

					// Done
					return;
				}

				// Remove
				lstFavorites.Items.RemoveAt(i);

				// Correct
				++i;
			}
			else
			{
				// Done
				return;
			}

			// Insert
			lstFavorites.Items.Insert(i, selected);

			// Reselect
			lstFavorites.SelectedIndex = i;

			// Done
			e.Handled = true;

			// Enable
			cmdSave.Enabled = true;
		}

		private void lstFavorites_KeyDown(object sender, KeyEventArgs e)
		{
			// Done
			if ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down)) e.Handled = true;
		}

		private void selLanguage_SelectionChangeCommitted(object sender, EventArgs e)
		{
			// Enable
			cmdSave.Enabled = true;
		}

		private void ckUse_CheckedChanged(object sender, EventArgs e)
		{
			// Enable
			cmdSave.Enabled = true;
		}

		private void cmdSave_Click(object sender, EventArgs e)
		{
			try
			{
				// Take over
				m_Configuration.PreferredLanguage = selLanguage.Text;
				m_Configuration.PreferAC3 = ckAC3.Checked;
				m_Configuration.EnableShortCuts = ckUse.Checked;
				m_Configuration.FavoriteChannels = new string[lstFavorites.Items.Count];
				
				// Fill
				lstFavorites.Items.CopyTo(m_Configuration.FavoriteChannels, 0);

				// Save
				m_Configuration.Save();

				// Disable
				cmdSave.Enabled = false;

				// Done
				OnFinished(DialogResult.OK);
			}
			catch (Exception ex)
			{
				// Report
				MessageBox.Show(this, ex.Message, Properties.Resources.SaveMessage);
			}
		}

		protected virtual void OnFinished(DialogResult result)
		{
			// Fire
			if (null != ConfigurationFinished) ConfigurationFinished(result);
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			// Report
			OnFinished(DialogResult.Cancel);
		}

		private void lstAll_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Check the key
			if ('\r' != e.KeyChar) return;
			
			// We did it
			e.Handled = true;

			// Forward
			lstAll_DoubleClick(lstAll, EventArgs.Empty);
		}
	}
}
