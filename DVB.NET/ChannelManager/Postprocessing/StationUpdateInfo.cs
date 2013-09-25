using System;
using JMS.DVB;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Beschreibt, in welchem Umfang die Daten eines Senders ver�ndert werden d�rfen.
	/// </summary>
	[Serializable]
	public class StationUpdateInfo
	{
		/// <summary>
		/// Beschreibt, wie die Sprachinformation zu einer einzelnen Tonspur aktualisiert
		/// werden soll.
		/// </summary>
		[Serializable]
		public class LanguageUpdate
		{
			/// <summary>
			/// Der PID des Tondatenstroms.
			/// </summary>
			private ushort m_PID = 0;

			/// <summary>
			/// Liest oder setzt den PID des Tondatenstroms.
			/// </summary>
			[XmlAttribute]
			public ushort PID
			{
				get 
				{ 
					// Report
					return m_PID; 
				}
				set 
				{ 
					// Update
					m_PID = value; 
				}
			}

			/// <summary>
			/// Beschreibt, in welchem Umfang die Sprachinformationen ver�ndert werden d�rfen.
			/// </summary>
			private UpdateModes m_Mode = UpdateModes.Update;

			/// <summary>
			/// Liest oder legt fest, wie die Sprachinformationen ver�ndert werden
			/// d�rfen.
			/// </summary>
			[XmlText]
			public UpdateModes UpdateMode
			{
				get 
				{ 
					// Report
					return m_Mode; 
				}
				set 
				{ 
					// Update
					m_Mode = value; 
				}
			}

			/// <summary>
			/// Erzeugt eine neue, uninitialisierte Instanz.
			/// </summary>
			/// <remarks>
			/// Nicht initialisierte Instanzen werden bei der Deserialisierung ben�tigt.
			/// </remarks>
			public LanguageUpdate()
			{
			}

			/// <summary>
			/// Erzeugt eine neue Instanz.
			/// </summary>
			/// <param name="PID">Der PID des Tondatenstroms.</param>
			/// <param name="languageMode">Die erlaubten Ver�nderungen f�r die Sprachinformationen.</param>
			public LanguageUpdate(ushort PID, UpdateModes languageMode)
			{
				// Remember
				m_PID = PID;
				m_Mode = languageMode;
			}
		}

		/// <summary>
		/// Hilfsklasse zum eleganteren Zugriff auf die Tonspuren.
		/// </summary>
		public class _AudioAccessor
		{
			/// <summary>
			/// Die Liste aller �nderungsinformationen f�r Tonspuren eines Senders.
			/// </summary>
			private List<LanguageUpdate> m_List;

			/// <summary>
			/// Erzeugt eine neue Instanz.
			/// </summary>
			/// <param name="list">Die Liste der �nderungsinformationen.</param>
			public _AudioAccessor(List<LanguageUpdate> list)
			{
				// Remember
				m_List = list;
			}

			/// <summary>
			/// Ermittelt die Konfiguration zu einer Tonspur.
			/// </summary>
			/// <param name="PID">Kennung der Tonspur.</param>
			/// <returns>Informationsinstanz oder <i>null</i>.</returns>
			private LanguageUpdate FindAudioMode(ushort PID)
			{
				// Find
				foreach (LanguageUpdate info in m_List)
					if (info.PID == PID)
						return info;

				// None found
				return null;
			}

			/// <summary>
			/// Liest oder setzt die �nderungsinformationen f�r eine Tonspur.
			/// </summary>
			/// <param name="pid">Datenstromkennung der Tonspur.</param>
			public AudioUpdateModes this[ushort pid]
			{
				get
				{
					// Find the item
					LanguageUpdate update = FindAudioMode(pid);

					// None
					if (null == update) return AudioUpdateModes.Update;

					// Fully protected
					if (UpdateModes.Keep == update.UpdateMode) return AudioUpdateModes.KeepLanguage;

					// Only PID is preserved
					return AudioUpdateModes.Keep;
				}
				set
				{
					// Find the item
					LanguageUpdate update = FindAudioMode(pid);

					// Must create
					if (null == update)
					{
						// Will be deleted right away
						if (AudioUpdateModes.Update == value) return;

						// Create
						update = new LanguageUpdate(pid, UpdateModes.Update);

						// Enter to list
						m_List.Add(update);
					}

					// Check new value
					switch (value)
					{
						case AudioUpdateModes.Update: m_List.Remove(update); break;
						case AudioUpdateModes.Keep: update.UpdateMode = UpdateModes.Update; break;
						case AudioUpdateModes.KeepLanguage: update.UpdateMode = UpdateModes.Keep; break;
					}
				}
			}
		}

		/// <summary>
		/// Erlaubt den indizierten Zugriff auf die Tonspuren.
		/// </summary>
		[NonSerialized]
		private _AudioAccessor m_AudioMode = null;

		/// <summary>
		/// Erlaubt den indizierten Zugriff auf die Tonspuren.
		/// </summary>
		[XmlIgnore]
		public _AudioAccessor AudioMode
		{
			get
			{
				// Create once
				if (null == m_AudioMode) m_AudioMode = new _AudioAccessor(AudioModes);

				// Report
				return m_AudioMode;
			}
		}

		/// <summary>
		/// Ver�nderungsoptionen f�r das Bildsignal.
		/// </summary>
		private VideoUpdateModes m_Video = VideoUpdateModes.Update;

		/// <summary>
		/// Liest oder legt fest, in welchem Umfang die Informationen zum Bild
		/// aktualisiert werden d�rfen.
		/// </summary>
		[XmlAttribute("Video"), DefaultValue(VideoUpdateModes.Update)]
		public VideoUpdateModes VideoMode
		{
			get 
			{ 
				// Report
				return m_Video; 
			}
			set 
			{ 
				// Change
				m_Video = value; 
			}
		}

		/// <summary>
		/// Ver�nderungsoptionen f�r den Videotext.
		/// </summary>
		private UpdateModes m_TTX = UpdateModes.Update;

		/// <summary>
		/// Liest oder legt fest, in welchem Umgang die Informationen zum
		/// Videotext aktualisiert werden d�rfen.
		/// </summary>
		[XmlElement("VideoText"), DefaultValue(UpdateModes.Update)]
		public UpdateModes VideoTextMode
		{
			get 
			{ 
				// Report
				return m_TTX; 
			}
			set 
			{ 
				// Change
				m_TTX = value; 
			}
		}

        /// <summary>
        /// Ver�nderungsoptionen f�r die DVB Untertitel.
        /// </summary>
        private UpdateModes m_DVBSub = UpdateModes.Update;

        /// <summary>
        /// Liest oder legt fest, in welchem Umgang die Informationen zu den DVB
        /// Untertiteln aktualisiert werden d�rfen.
        /// </summary>
        [XmlElement("DVBSubTitles"), DefaultValue(UpdateModes.Update)]
        public UpdateModes DVBSubtitleMode
        {
            get
            {
                // Report
                return m_DVBSub;
            }
            set
            {
                // Change
                m_DVBSub = value;
            }
        }
        
        /// <summary>
		/// Ver�nderungsinformationen f�r die Zeitbasis.
		/// </summary>
		private UpdateModes m_PCR = UpdateModes.Update;

		/// <summary>
		/// Liest oder legt fest, in welchem Umgang die Informationen zur
		/// Zeitbasis (PCR) aktualisiert werden d�rfen.
		/// </summary>
		[XmlElement("Clock"), DefaultValue(UpdateModes.Update)]
		public UpdateModes PCRMode
		{
			get 
			{ 
				// Report
				return m_PCR; 
			}
			set 
			{ 
				// Change
				m_PCR = value; 
			}
		}

		/// <summary>
		/// Ver�nderungsoptionen f�r die Verschl�sselung.
		/// </summary>
		private UpdateModes m_Enc = UpdateModes.Update;

		/// <summary>
		/// Liest oder legt fest, in welchem Umgang die Informationen zur
		/// Verschl�sselung aktualisiert werden d�rfen.
		/// </summary>
		[XmlAttribute("Encryption"), DefaultValue(UpdateModes.Update)]
		public UpdateModes EncryptionMode
		{
			get 
			{ 
				// Report
				return m_Enc; 
			}
			set 
			{ 
				// Update
				m_Enc = value; 
			}
		}

        /// <summary>
        /// Pr�foption f�r den Ein- oder Ausschluss.
        /// </summary>
        private ExcludeModes m_Exclude = ExcludeModes.Include;

        /// <summary>
        /// Liest oder legt fest, ob ein Element als Ganzes ein- oder
        /// ausgeschlossen werden soll.
        /// </summary>
        [XmlAttribute("Exclusion"), DefaultValue(ExcludeModes.Include)]
        public ExcludeModes ExclusionMode
        {
            get
            {
                // Report
                return m_Exclude;
            }
            set
            {
                // Update
                m_Exclude = value;
            }
        }

		/// <summary>
		/// Ver�nderungsoptionen f�r den Sendernamen.
		/// </summary>
		private UpdateModes m_Name = UpdateModes.Update;

		/// <summary>
		/// Liest oder legt fest, in welchem Umgang der Sendername aktualisiert werden darf.
		/// </summary>
		[XmlElement("Name"), DefaultValue(UpdateModes.Update)]
		public UpdateModes StationNameMode
		{
			get 
			{ 
				// Report
				return m_Name; 
			}
			set 
			{ 
				// Update
				m_Name = value; 
			}
		}

		/// <summary>
		/// Ver�nderungsoptionen f�r den Dienstnamen.
		/// </summary>
		private UpdateModes m_Transponder = UpdateModes.Update;

		/// <summary>
		/// Liest oder legt fest, in welchem Umgang der Name des Dienstanbieters aktualisiert werden darf.
		/// </summary>
		[XmlElement("Service"), DefaultValue(UpdateModes.Update)]
		public UpdateModes TransponderNameMode
		{
			get 
			{ 
				// Report
				return m_Transponder; 
			}
			set 
			{ 
				// Update
				m_Transponder = value; 
			}
		}

		/// <summary>
		/// Beschreibt alle Tonspuren mit besonderen �nderungsoptionen.
		/// </summary>
		[XmlElement("Audio")]
		public readonly List<LanguageUpdate> AudioModes = new List<LanguageUpdate>();

		/// <summary>
		/// Spezielle Zusatzinformationen, die am Sender vermerkt werden sollen.
		/// </summary>
		[XmlElement("Setting")]
		public readonly List<Station.CustomData> CustomSettings = new List<Station.CustomData>();

		/// <summary>
		/// Erzeugt eine neue Informationsinstanz.
		/// </summary>
		public StationUpdateInfo()
		{
		}

		/// <summary>
		/// F�hrt alle erlaubten Aktualisierungen durch und �bernimmt die gew�nschten
		/// Werte aus den vorherigen Senderinformationen.
		/// </summary>
		/// <param name="oldStation">Bisherige Senderinformationen.</param>
		/// <param name="newStation">Neue Senderinformationen.</param>
		/// <param name="reporter">Schnittstelle zur Erzeugung von Meldungen.</param>
		public void UpdateStation(Station oldStation, Station newStation, IPostprocessingReport reporter)
		{
			// Start
			using (reporter.BeginUpdate(newStation))
			{
				// Simple replaces
				if (UpdateModes.Keep == m_Name) newStation.Name = oldStation.Name;
				if (UpdateModes.Keep == m_Transponder) newStation.TransponderName = oldStation.TransponderName;
				if (UpdateModes.Keep == m_Enc) newStation.Encrypted = oldStation.Encrypted;
				if (UpdateModes.Keep == m_PCR) newStation.PCRPID = oldStation.PCRPID;
				if (UpdateModes.Keep == m_TTX) newStation.TTXPID = oldStation.TTXPID;
                
                // List relpaces
                if (UpdateModes.Keep == m_DVBSub)
                {
                    // Wipe out
                    newStation.DVBSubtitles.Clear();

                    // Load all
                    newStation.DVBSubtitles.AddRange(oldStation.DVBSubtitles);
                }

				// Video
				if (VideoUpdateModes.Update != m_Video) newStation.VideoPID = oldStation.VideoPID;
				if (VideoUpdateModes.KeepType == m_Video) newStation.VideoType = oldStation.VideoType;

				// Custom settings
				foreach (Station.CustomData custom in CustomSettings) newStation.SetCustomSetting(custom.Key, custom.Value);

				// Audio
				foreach (LanguageUpdate info in AudioModes)
				{
					// Must exist in old station
					AudioInfo oldAudio = oldStation.FindAudioInfo(info.PID);
					if (null == oldAudio) continue;

					// May exist in new station
					AudioInfo newAudio = newStation.FindAudioInfo(info.PID);

					// Check mode
					if ((null == newAudio) || (UpdateModes.Keep == info.UpdateMode))
					{
						// Full replace to keep the track and the language
						newStation.UpdateAudioMap(newAudio, oldAudio);
					}
				}
			}
		}

		/// <summary>
		/// Ermittelt eine erweiterte Einstellung.
		/// </summary>
		/// <param name="key">Eindeutiger Name der Einstellung.</param>
		/// <returns>Die gew�nschte Einstellung oder <i>null</i>.</returns>
		public string GetCustomSetting(string key)
		{
			// Forward
			return Station.GetCustomSetting(CustomSettings, key);
		}

		/// <summary>
		/// Ermittelt eine erweiterte Einstellung.
		/// </summary>
		/// <typeparam name="T">Gew�nschter Datentyp.</typeparam>
		/// <param name="key">Eindeutiger Name der Einstellung.</param>
		/// <returns>Die gew�nschte Einstellung oder <i>null</i>.</returns>
		public T? GetCustomSetting<T>(string key) where T : struct
		{
			// Forward
			return Station.GetCustomSetting<T>(CustomSettings, key);
		}

		/// <summary>
		/// Ver�ndert eine erweiterte Einstellung.
		/// </summary>
		/// <typeparam name="T">Gew�nschter Datentyp.</typeparam>
		/// <param name="key">Eindeutiger Name der Einstellung.</param>
		/// <param name="value">Der neue Wert der Einstellung.</param>
		public void SetCustomSetting<T>(string key, T? value) where T : struct
		{
			// Forward
			Station.SetCustomSetting(CustomSettings, key, value);
		}

		/// <summary>
		/// Ver�ndert eine erweiterte Einstellung.
		/// </summary>
		/// <param name="key">Eindeutiger Name der Einstellung.</param>
		/// <param name="value">Der neue Wert der Einstellung.</param>
		public void SetCustomSetting(string key, string value)
		{
			// Forward
			Station.SetCustomSetting(CustomSettings, key, value);
		}

		/// <summary>
		/// Meldet, ob diese �nderungsinformation �berhaupt keine Ver�nderung bewirkt.
		/// </summary>
		public bool IsIdentity
		{
			get
			{
				// Check all
				if (m_Enc != UpdateModes.Update) return false;
				if (m_Name != UpdateModes.Update) return false;
				if (m_PCR != UpdateModes.Update) return false;
				if (m_Transponder != UpdateModes.Update) return false;
				if (m_TTX != UpdateModes.Update) return false;
                if (m_DVBSub != UpdateModes.Update) return false;
				if (m_Video != VideoUpdateModes.Update) return false;
                if (m_Exclude != ExcludeModes.Include) return false;
				if (AudioModes.Count > 0) return false;
				if (CustomSettings.Count > 0) return false;
				
				// Changes nothing
				return true;
			}
		}
	}
}
