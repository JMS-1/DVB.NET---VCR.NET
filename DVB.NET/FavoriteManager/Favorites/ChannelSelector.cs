using System;
using System.Data;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace JMS.DVB.Favorites
{
	public delegate void ChannelSelectionHandler(ChannelSelector selector, string channelName, object context);
	public delegate void ServiceSelectionHandler(ChannelSelector selector, string serviceName, object context);
	public delegate void AudioSelectionHandler(ChannelSelector selector, string audioName);
	public delegate void ConfigurationChangeHandler(ChannelSelector selector);
	public delegate void KeyPreviewHandler(KeyPressEventArgs args);

	public partial class ChannelSelector : UserControl, IMessageFilter
	{
		internal const string IndexFormat = "[{0}] {1}";

		internal readonly UserSettings Configuration;

		private object m_FilterCountLock = new object();
		private int m_FilterCount = 0;

		private Dictionary<string, ServiceItem> m_Services = new Dictionary<string, ServiceItem>();
		private Dictionary<string, ChannelItem> m_Channels = new Dictionary<string, ChannelItem>();
		private Dictionary<string, AudioItem> m_Tracks = new Dictionary<string, AudioItem>();
		private DateTime m_LastKey = DateTime.MinValue;
		private int m_IndexChars = int.MaxValue;
		private bool m_ChannelsLoaded = false;
		private bool m_ServiceLoaded = false;
		private string m_PrimaryTrack = null;
		private bool m_AudioLoaded = false;
		private int m_IndexCollected = 0;
		private bool m_ShowAll = true;
		private int m_IndexCount = 0;
		private bool m_Embedded;
	
		public event ConfigurationChangeHandler ConfigurationChanged;
		public event ChannelSelectionHandler ChannelSelected;
		public event ServiceSelectionHandler ServiceSelected;
		public event AudioSelectionHandler TrackSelected;
		public event KeyPreviewHandler KeyPreview;

		public ChannelSelector()
			: this(false)
		{
		}

		public ChannelSelector(bool embedded)
		{
			// Remember
			m_Embedded = embedded;

			// Load configuration
			Configuration = UserSettings.Load();

			// Configure
			InitializeComponent();

			// Nothing more to do
			if (m_Embedded) return;

			// Configure to handek model code
			Application.EnterThreadModal += new EventHandler(Application_EnterThreadModal);
			Application.LeaveThreadModal += new EventHandler(Application_LeaveThreadModal);

			// Register self
			Application.AddMessageFilter(this);
		}

		void Application_LeaveThreadModal(object sender, EventArgs e)
		{
			// Enable
			EnableMessageFilter();
		}

		void Application_EnterThreadModal(object sender, EventArgs e)
		{
			// Lock out
			DisableMessageFilter();
		}

		public void AddChannel(string displayName)
		{
			// Forward
			AddChannel(displayName, null);
		}

		public void AddChannel(string displayName, object context)
		{
			// Create and remember
			m_Channels[displayName] = new ChannelItem(displayName, context);

			// Mark as changed
			m_ChannelsLoaded = false;
		}

		public void AddService(string serviceName)
		{
			// Forward
			AddService(serviceName, null);
		}

		public void AddService(string serviceName, object context)
		{
			// Create and remember
			m_Services[serviceName] = new ServiceItem(serviceName, context);

			// Mark as changed
			m_ServiceLoaded = false;
		}

		public void AddAudio(string audioName, bool primary)
		{
			// Create and remember
			m_Tracks[audioName] = new AudioItem(audioName);

			// Remember primary
			if (primary) m_PrimaryTrack = audioName;

			// Mark as changed
			m_AudioLoaded = false;
		}

		public List<ChannelItem> LoadChannelList()
		{
			// Load all
			FillChannelList();

			// Create temporary list
			List<ChannelItem> channels = new List<ChannelItem>();

			// Load from selection list
			foreach (ChannelItem channel in selChannel.Items) channels.Add(channel);

			// Report
			return channels;
		}

		public void FillChannelList()
		{
			// Already did it
			if (m_ChannelsLoaded) return;

			// Once only
			m_ChannelsLoaded = true;

			// Reset index
			foreach (ChannelItem channel in m_Channels.Values) channel.Index = null;

			// Create temporary list
			List<ChannelItem> channels = new List<ChannelItem>();

			// Check mode
			if (ShortCutsEnabled)
			{
				// Favorites only
				if (null != Configuration.FavoriteChannels)
					foreach (string favorite in Configuration.FavoriteChannels)
					{
						// Try to find
						ChannelItem channel;
						if (!m_Channels.TryGetValue(favorite, out channel)) continue;

						// Use
						channels.Add(channel);
					}
			}
			else
			{
				// All
				channels.AddRange(m_Channels.Values);

				// Sort
				channels.Sort();
			}

			// Reset index chars
			m_LastKey = DateTime.MinValue;
			m_IndexChars = int.MaxValue;

			// Add shortcut information
			if (m_Embedded || ShortCutsEnabled)
			{
				// Calculate
				m_IndexChars = channels.Count.ToString().Length;

				// Create format pattern
				string format = new string('0', m_IndexChars);

				// Set index on all
				for (int i = channels.Count; i-- > 0; )
				{
					// Load
					ChannelItem channel = channels[i];

					// Set index
					channel.Index = (i + 1).ToString(format);
				}
			}

			// Add to list
			selChannel.Items.Clear();
			selChannel.Items.AddRange(channels.ToArray());

			// Enable
			selChannel.Enabled = (selChannel.Items.Count > 0);
		}

		public void FillServiceList()
		{
			// Already did it
			if (m_ServiceLoaded) return;

			// Once only
			m_ServiceLoaded = true;

			// Create temporary list
			List<ServiceItem> services = new List<ServiceItem>(m_Services.Values);

			// Reset index
			foreach (ServiceItem service in services) service.Index = '\0';

			// Sort
			if (!m_Embedded) services.Sort();

			// Add shortcut information
			if (m_Embedded || ShortCutsEnabled)
			{
				// Get the starting character
				int baseChar = 'Z' - services.Count + 1;

				// Set index on all
				for (int i = services.Count; i-- > 0; )
				{
					// Load
					ServiceItem service = services[i];

					// Set index
					service.Index = (char)(baseChar + i);
				}
			}

			// Add to list
			selService.Items.Clear();
			selService.Items.AddRange(services.ToArray());

			// Enable
			selService.Enabled = (selService.Items.Count > 0);
		}

		public void FillAudioList()
		{
			// Already did it
			if (m_AudioLoaded) return;

			// Once only
			m_AudioLoaded = true;

			// Create temporary list
			List<AudioItem> tracks = new List<AudioItem>(m_Tracks.Values);

			// Reset index
			foreach (AudioItem audio in tracks) audio.Index = '\0';

			// Sort
			tracks.Sort();

			// Add shortcut information
			if (m_Embedded || ShortCutsEnabled)
				for (int i = tracks.Count; i-- > 0; )
				{
					// Load
					AudioItem audio = tracks[i];

					// Set index
					audio.Index = (char)('A' + i);
				}

			// Add to list
			selAudio.Items.Clear();
			selAudio.Items.AddRange(tracks.ToArray());

			// Enable
			selAudio.Enabled = (selAudio.Items.Count > 1);
		}

		public bool SelectAudio(string audioName)
		{
			// Make sure that list is filled
			FillAudioList();

			// Item to find
			AudioItem audio = null;
			
			// Default
			if (string.IsNullOrEmpty(audioName)) audio = GetPreferredAudioFromList();

			// Try lookup
			if (null == audio)
			{
				// Use preferred track
				if (string.IsNullOrEmpty(audioName)) audioName = m_PrimaryTrack;

				// Default
				if (!string.IsNullOrEmpty(audioName))
					if (!m_Tracks.TryGetValue(audioName, out audio))
						audio = null;
			}

			// Use first
			if ((null == audio) && (selAudio.Items.Count > 0)) audio = (AudioItem)selAudio.Items[0];

			// Use it
			selAudio.SelectedItem = audio;

			// Report
			selAudio_SelectionChangeCommitted(selAudio, EventArgs.Empty);

			// Report
			return (null != audio);
		}

		public string GetPreferredAudio()
		{
			// Make sure that list is filled
			FillAudioList();

			// Item to find
			AudioItem audio = GetPreferredAudioFromList();

			// Use preferred track
			if (null == audio)
				if (!string.IsNullOrEmpty(m_PrimaryTrack))
					if (!m_Tracks.TryGetValue(m_PrimaryTrack, out audio))
						audio = null;

			// Use first
			if ((null == audio) && (selAudio.Items.Count > 0)) audio = (AudioItem)selAudio.Items[0];

			// Report
			return (null == audio) ? null : audio.AudioName;
		}

		private void ScanTracks(bool ac3, ref AudioItem any, ref AudioItem hit)
		{
			// Test for AC3
			string testAC3 = " (AC3) [";

			// Create the key
			string key = Configuration.PreferredLanguage + (ac3 ? testAC3 : " [");

			// Already failed
			bool failed = false;

			// Test all
			foreach (KeyValuePair<string, AudioItem> test in m_Tracks)
			{
				// Check mode
				bool isAC3 = (test.Key.IndexOf(testAC3) >= 0);

				// Skip
				if (isAC3 != ac3) continue;

				// Set any
				if ((null == any) || (test.Value.Index < any.Index)) any = test.Value;

				// Match
				if (!test.Key.StartsWith(key)) continue;

				// Test
				failed = (null != hit);

				// Remember
				hit = test.Value;
			}

			// Wipe out
			if (failed) hit = null;
		}

		private AudioItem GetPreferredAudioFromList()
		{
			// Make sure that list is filled
			FillAudioList();

			// Check default
			if (string.IsNullOrEmpty(Configuration.PreferredLanguage)) return null;

			// Check for exact match
			AudioItem audio;
			if (m_Tracks.TryGetValue(Configuration.PreferredLanguage, out audio))
				if (null != audio)
					return audio;

			// Find
			AudioItem bestMP2 = null, anyMP2 = null;
			ScanTracks(false, ref anyMP2, ref bestMP2);

			// Check modes
			if (Configuration.PreferAC3)
			{
				// Find
				AudioItem bestAC3 = null, anyAC3 = null;
				ScanTracks(true, ref anyAC3, ref bestAC3);

				// Check for AC3 hit
				if (null != bestAC3) return bestAC3;

				// At least prefer AC3
				if (null != anyAC3) return anyAC3;
			}

			// Check for MP2 hit
			if (null != bestMP2) return bestMP2;

			// Use any
			return anyMP2;
		}

		public bool SelectService(string serviceName)
		{
			// Make sure that list is filled
			FillServiceList();

			// Read the item
			ServiceItem service;
			if (string.IsNullOrEmpty(serviceName) || !m_Services.TryGetValue(serviceName, out service)) service = null;

			// Use it
			selService.SelectedItem = service;

			// Report
			selService_SelectionChangeCommitted(selService, EventArgs.Empty);

			// Report
			return (null != service);
		}

		public bool SelectChannel(string displayName)
		{
			// Make sure that list is filled
			FillChannelList();

			// Read the item
			ChannelItem channel;
			if (string.IsNullOrEmpty(displayName) || !m_Channels.TryGetValue(displayName, out channel)) channel = null;

			// Use it
			selChannel.SelectedItem = channel;

			// Report
			selChannel_SelectionChangeCommitted(selChannel, EventArgs.Empty);

			// Report
			return (null != channel);
		}

		public void ShowConfiguration(IWin32Window owner)
		{
			// Create dialog
			using (ConfigurationDialog dialog = new ConfigurationDialog(this))
			{
				// Nothing happend
				if (DialogResult.OK != dialog.ShowDialog(owner)) return;

				// Reset
				Clear();

				// Forward
				OnConfigurationChanged();
			}
		}

		private void cmdConfig_DoubleClick(object sender, EventArgs e)
		{
			// Forward
			ShowConfiguration(this);
		}

		protected virtual void OnChannelSelected(string channelName, object context)
		{
			// Report
			if (null != ChannelSelected) ChannelSelected(this, channelName, context);
		}

		protected virtual void OnServiceSelected(string serviceName, object context)
		{
			// Report
			if (null != ServiceSelected) ServiceSelected(this, serviceName, context);
		}

		protected virtual void OnAudioSelected(string audioName)
		{
			// Report
			if (null != TrackSelected) TrackSelected(this, audioName);
		}

		private void selChannel_SelectionChangeCommitted(object sender, EventArgs e)
		{
			// Get the item
			ChannelItem channel = CurrentChannel;

			// None
			if (null == channel) return;

			// Clear helper lists
			ClearAudioAndService();

			// Forward
			OnChannelSelected(channel.ChannelName, channel.Context);
		}

		public ComboBox ChannelList
		{
			get
			{
				// Make sure that list is filled
				FillChannelList();

				// Report
				return selChannel;
			}
		}

		public ComboBox ServiceList
		{
			get
			{
				// Make sure that list is filled
				FillServiceList();

				// Report
				return selService;
			}
		}

		public ComboBox AudioList
		{
			get
			{
				// Make sure that list is filled
				FillAudioList();

				// Report
				return selAudio;
			}
		}

		public void ClearAll()
		{
			// Channels
			m_Channels.Clear();

			// Forwad
			Clear();
		}

		public void Clear()
		{
			// Force reload
			m_ChannelsLoaded = false;

			// Clear secondary lists
			selChannel.Items.Clear();

			// Disable GUI
			selChannel.Enabled = false;

			// Reset counters
			m_LastKey = DateTime.MinValue;
			m_IndexChars = int.MaxValue;

			// Forward
			ClearAudioAndService();
		}

		public void ClearAudioAndService()
		{
			// Clear helper lists
			m_PrimaryTrack = null;
			m_Tracks.Clear();

			// Clear secondary lists
			selAudio.Items.Clear();

			// Disable GUI
			selAudio.Enabled = false;

			// Forward
			ClearService();
		}

		public void ClearService()
		{
			// Clear helper list
			m_Services.Clear();

			// Clear secondary list
			selService.Items.Clear();

			// Disable GUI
			selService.Enabled = false;
		}

		private ChannelItem CurrentChannel
		{
			get
			{
				// Get the item
				return (ChannelItem)selChannel.SelectedItem;
			}
		}

		private ServiceItem CurrentService
		{
			get
			{
				// Get the item
				return (ServiceItem)selService.SelectedItem;
			}
		}

		private AudioItem CurrentAudio
		{
			get
			{
				// Get the item
				return (AudioItem)selAudio.SelectedItem;
			}
		}

		private void selAudio_SelectionChangeCommitted(object sender, EventArgs e)
		{
			// Get the item
			AudioItem audio = CurrentAudio;

			// None
			if (null == audio) return;

			// Forward
			OnAudioSelected(audio.AudioName);
		}

		private void selService_SelectionChangeCommitted(object sender, EventArgs e)
		{
			// Get the item
			ServiceItem service = CurrentService;

			// None
			if (null == service) return;

			// Forward
			OnServiceSelected(service.ServiceName, service.Context);
		}

		public void FinishSelection(ComboBox selection)
		{
			// Call notifications
			if (selection == selChannel)
				selChannel_SelectionChangeCommitted(selChannel, EventArgs.Empty);
			else if (selection == selAudio)
				selAudio_SelectionChangeCommitted(selAudio, EventArgs.Empty);
			else if (selection == selService)
				selService_SelectionChangeCommitted(selService, EventArgs.Empty);
		}

		public void AnalyseStandardKey(Keys key)
		{
			// Forward
			AnalyseStandardKey((char)key);
		}

		public void AnalyseStandardKey(char key)
		{
			// Forward
			AnalyseKey(new KeyPressEventArgs(key));
		}

		private void AnalyseKey(KeyPressEventArgs e)
		{
			// We are inactive
			if (!Enabled) return;

			// Not enabled
			if (!m_Embedded)
				if (!ShortCutsEnabled)
					return;

			// One of our lists is open
			if (selChannel.DroppedDown || selAudio.DroppedDown || selService.DroppedDown) return;

			// Check for digit
			if (selChannel.Enabled)
				if (char.IsDigit(e.KeyChar))
				{
					// How long since the last key pressed
					TimeSpan delta = DateTime.UtcNow - m_LastKey;

					// Reset all
					if (delta.TotalSeconds >= 2)
					{
						// Reset
						m_IndexCollected = 0;
						m_IndexCount = 0;
					}

					// Collect
					m_IndexCollected = 10 * m_IndexCollected + (e.KeyChar - '0');

					// Check end
					if (++m_IndexCount == m_IndexChars)
					{
						// Start from scratch
						m_LastKey = DateTime.MinValue;

						// Process
						if ((m_IndexCollected > 0) && (m_IndexCollected <= selChannel.Items.Count))
						{
							// Load the item
							selChannel.SelectedItem = selChannel.Items[m_IndexCollected - 1];

							// Force update
							selChannel_SelectionChangeCommitted(selChannel, EventArgs.Empty);
						}
					}
					else
					{
						// Must go on
						m_LastKey = DateTime.UtcNow;
					}

					// Eaten
					e.Handled = true;

					// Done 
					return;
				}
				else if ('-' == e.KeyChar)
				{
					// Check
					if (selChannel.SelectedIndex > 0)
					{
						// Change
						selChannel.SelectedIndex -= 1;

						// Force update
						selChannel_SelectionChangeCommitted(selChannel, EventArgs.Empty);
					}

					// Eaten
					e.Handled = true;

					// Done 
					return;
				}
				else if ('+' == e.KeyChar)
				{
					// Check
					if (selChannel.SelectedIndex < (selChannel.Items.Count - 1))
					{
						// Change
						selChannel.SelectedIndex += 1;

						// Force update
						selChannel_SelectionChangeCommitted(selChannel, EventArgs.Empty);
					}

					// Eaten
					e.Handled = true;

					// Done 
					return;
				}

			// The char
			char test = char.ToUpper(e.KeyChar);

			// Check audio
			if (selAudio.Enabled)
				foreach (AudioItem audio in selAudio.Items)
					if (audio.Index == test)
					{
						// We did it
						e.Handled = true;

						// Select
						selAudio.SelectedItem = audio;

						// Fire
						selAudio_SelectionChangeCommitted(selAudio, EventArgs.Empty);

						// Done
						return;
					}

			// Check service
			if (selService.Enabled)
				foreach (ServiceItem service in selService.Items)
					if (service.Index == test)
					{
						// We did it
						e.Handled = true;

						// Select
						selService.SelectedItem = service;

						// Fire
						selService_SelectionChangeCommitted(selService, EventArgs.Empty);

						// Done
						return;
					}

			// List to update
			ComboBox list = null;

			// Specials
			switch (test)
			{
				case 'K': list = selChannel; break;
				case 'L': list = selAudio; break;
				case 'M': list = selService; break;
			}

			// Process
			if ((null != list) && list.Enabled)
			{
				// Toggle
				list.DroppedDown = true;

				// Set focus
				list.Focus();

				// Mark end
				e.Handled = true;

				// Done
				return;
			}
		}

		protected virtual void OnConfigurationChanged()
		{
			// Forward
			if (null != ConfigurationChanged) ConfigurationChanged(this);
		}

		public ChannelItem[] Channels
		{
			get
			{
				// Create
				ChannelItem[] result = new ChannelItem[m_Channels.Count];

				// Fill
				m_Channels.Values.CopyTo(result, 0);

				// Report
				return result;
			}
		}

		protected virtual void OnKeyPreview(KeyPressEventArgs args)
		{
			// Process self
			AnalyseKey(args);

			// Done
			if (args.Handled) return;

			// Fire
			if (null != KeyPreview) KeyPreview(args);
		}

		public bool MessageFilterEnabled
		{
			get
			{
				// Report
				lock (m_FilterCountLock) return (0 == m_FilterCount);
			}
		}

		public void DisableMessageFilter()
		{
			// Easy
			lock (m_FilterCountLock) ++m_FilterCount;
		}

		public void EnableMessageFilter()
		{
			// Protect
			lock (m_FilterCountLock)
			{
				// Test
				if (0 == m_FilterCount) throw new InvalidOperationException();

				// Change
				--m_FilterCount;
			}
		}

		public void DisableFavorites()
		{
			// Remember
			m_ShowAll = true;
		}

		public void EnableFavorites()
		{
			// Remember
			m_ShowAll = false;
		}

		private bool ShortCutsEnabled
		{
			get
			{
				// Report
				return (Configuration.EnableShortCuts && !m_ShowAll);
			}
		}

		#region IMessageFilter Members

		bool IMessageFilter.PreFilterMessage(ref Message m)
		{
			// Not us
			if (0x0102 != m.Msg) return false;

			// Already gone
			if (IsDisposed) return false;

			// Chief disabled events
			if (!MessageFilterEnabled) return false;

			// We are busy
			if (selChannel.DroppedDown || selAudio.DroppedDown || selService.DroppedDown) return false;

			// Create command
			KeyPressEventArgs args = new KeyPressEventArgs((char)m.WParam);

			// Reset
			args.Handled = false;

			// Process
			OnKeyPreview(args);

			// Report
			return args.Handled;
		}

		#endregion
	}
}
