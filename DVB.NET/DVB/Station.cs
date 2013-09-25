using System;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
	/// <summary>
	/// Base class describing a single station.
	/// </summary>
	[Serializable]
	public class Station : Identifier
	{
		/// <summary>
		/// Beschreibt Sonderdaten, die für einen Sender zu vermerken sind.
		/// </summary>
		[Serializable]
		public class CustomData
		{
			/// <summary>
			/// Der eindeutige Name eines Wertes.
			/// </summary>
			private string m_Key;

			/// <summary>
			/// Liest oder setzt den eindeutigen Namen eines Wertes.
			/// </summary>
			[XmlAttribute("key")]
			public string Key
			{
				get { return m_Key; }
				set { m_Key = value; }
			}

			/// <summary>
			/// Der zu verwendende Wert.
			/// </summary>
			private string m_Data;

			/// <summary>
			/// Liest oder setzt den zu verwendenden Wert.
			/// </summary>
			[XmlText]
			public string Value
			{
				get { return m_Data; }
				set { m_Data = value; }
			}

			/// <summary>
			/// Erzeugt eine uninitialisierte Instanz.
			/// </summary>
			/// <remarks>
			/// Nicht initialisierte Instanzen werden für die Deserialisierung benötigt.
			/// </remarks>
			public CustomData()
			{
			}

			/// <summary>
			/// Erzeugt eine neue Instanz.
			/// </summary>
			/// <param name="key">Eindeutiger Name des Wertes.</param>
			/// <param name="value">Der zu speichernde Wert.</param>
			public CustomData(string key, string value)
			{
				// Remember
				m_Key = key;
				m_Data = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private static Dictionary<string, string> m_LanguageMap = new Dictionary<string, string>();

		/// <summary>
		/// 
		/// </summary>
		private static Dictionary<string, string> m_LanguageMapEx = new Dictionary<string, string>();

		/// <summary>
		/// The transponder this station belongs to.
		/// </summary>
		[XmlIgnore]
		public Transponder Transponder;

		/// <summary>
		/// The name of the transponder.
		/// </summary>
		public string TransponderName;

		/// <summary>
		/// The name of this station realtive to the transponder.
		/// </summary>
		[XmlAttribute]
		public string Name;

		/// <summary>
		/// The video stream identifier.
		/// </summary>
		[XmlElement("Video")]
		public ushort VideoPID;

		/// <summary>
		/// The stream identifier of the primary audio signal.
		/// </summary>
		[XmlElement("Audio")]
		public ushort AudioPID;

		/// <summary>
		/// Stream identifier for the Dolby Digital (AC3) audio signal.
		/// </summary>
		[XmlElement("AC3")]
		public ushort AC3PID;

		/// <summary>
		/// Program clock reference stream identifier.
		/// </summary>
		[XmlElement("PCR")]
		public ushort PCRPID;

		/// <summary>
		/// Stream identifier for video text data.
		/// </summary>
		[XmlElement("TTX")]
		public ushort TTXPID;

		/// <summary>
		/// Maps audio names to the related stream identifiers.
		/// </summary>
		[XmlIgnore]
		private Dictionary<string, AudioInfo> m_AudioMap = new Dictionary<string, AudioInfo>();

        /// <summary>
        /// Contains all DVB subtitle information for this station.
        /// </summary>
        public readonly List<DVBSubtitleInfo> DVBSubtitles = new List<DVBSubtitleInfo>();

		/// <summary>
		/// See if this station send encrypted.
		/// </summary>
		public bool Encrypted;

		/// <summary>
		/// The type of the video stream - 255 if not known.
		/// </summary>
		public byte VideoType = 255;

		/// <summary>
		/// Additional settings not related to core DVB.NET functionality.
		/// </summary>
		[XmlElement("Setting")]
		public readonly List<CustomData> CustomSettings = new List<CustomData>();

		/// <summary>
		/// Create an empty instance used for serialization.
		/// </summary>
		public Station()
		{
		}

		/// <summary>
		/// Initialize the instance.
		/// </summary>
		/// <param name="transponder">The transponder this station is bound to.</param>
		/// <param name="networkID">The original network identifier.</param>
		/// <param name="transportID">The transport strea, identifier.</param>
		/// <param name="serviceID">The service identifier.</param>
		/// <param name="name">Name of the station as shown to the user.</param>
		/// <param name="video">Video PID.</param>
		/// <param name="audio">Audio PID.</param>
		/// <param name="ac3">AC3 PID.</param>
		/// <param name="ttx">TeleText PID.</param>
		/// <param name="pcr">Program Clock Reference PID.</param>
		/// <param name="audioMap">Maps audio names to the related stream identifiers.</param>
		/// <param name="transponderName">Name of the transponder.</param>
		/// <param name="encrypted">Set to create an encrypted station.</param>
		public Station(Transponder transponder, ushort networkID, ushort transportID, ushort serviceID, string name, string transponderName, ushort video, ushort audio, Dictionary<string, AudioInfo> audioMap, ushort pcr, ushort ac3, ushort ttx, bool encrypted)
			: base(networkID, transportID, serviceID)
		{
			// Must hava a transponder and a name
			if (null == transponder) throw new ArgumentNullException("transponder");
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			// Remember
			TransponderName = transponderName;
			Transponder = transponder;
			Encrypted = encrypted;
			VideoPID = video;
			AudioPID = audio;
			AC3PID = ac3;
			PCRPID = pcr;
			TTXPID = ttx;
			Name = name;

			// Load audio map
			if (null != audioMap) m_AudioMap = new Dictionary<string, AudioInfo>(audioMap);

			// Link into transponder
			Transponder.AddStation(this);
		}

		/// <summary>
		/// Initialize the instance.
		/// </summary>
		/// <param name="transponder">The transponder this station is bound to.</param>
		/// <param name="networkID">The original network identifier.</param>
		/// <param name="transportID">The transport strea, identifier.</param>
		/// <param name="serviceID">The service identifier.</param>
		/// <param name="name">Name of the station as shown to the user.</param>
		/// <param name="video">Video PID.</param>
		/// <param name="audio">Audio PID.</param>
		/// <param name="ac3">AC3 PID.</param>
		/// <param name="ttx">TeleText PID.</param>
		/// <param name="pcr">Program Clock Reference PID.</param>
		/// <param name="audioMap">Maps audio names to the related stream identifiers.</param>
		/// <param name="transponderName">Name of the transponder.</param>
		/// <param name="encrypted">Set to create an encrypted station.</param>
		public Station(Transponder transponder, ushort networkID, ushort transportID, ushort serviceID, string name, string transponderName, ushort video, ushort audio, ushort pcr, ushort ac3, ushort ttx, Hashtable audioMap, bool encrypted)
			: this(transponder, networkID, transportID, serviceID, name, transponderName, video, audio, null, pcr, ac3, ttx, encrypted)
		{
			// No audio map at all
			if (null == audioMap) return;

			// Fill the audio map
			foreach (DictionaryEntry ent in audioMap)
			{
				// Create item
				AudioInfo info = new AudioInfo((ushort)ent.Value, (string)ent.Key, false);

				// Remember it
				m_AudioMap[info.Name] = info;
			}
		}

		/// <summary>
		/// Clone a station an assign it to a new transponder.
		/// </summary>
		/// <param name="transponder">The target transponder.</param>
		/// <param name="other">The prototype station.</param>
		public Station(Transponder transponder, Station other)
			: base(other.NetworkIdentifier, other.TransportStreamIdentifier, other.ServiceIdentifier)
		{
			// Must hava a transponder and a name
			if (null == transponder) throw new ArgumentNullException("transponder");

			// Remember
			m_AudioMap = new Dictionary<string, AudioInfo>(other.m_AudioMap);
            DVBSubtitles.AddRange(other.DVBSubtitles);
			TransponderName = other.TransponderName;
			VideoType = other.VideoType;
			Encrypted = other.Encrypted;
			Transponder = transponder;
			VideoPID = other.VideoPID;
			AudioPID = other.AudioPID;
			PCRPID = other.PCRPID;
			TTXPID = other.TTXPID;
			AC3PID = other.AC3PID;
			Name = other.Name;

			// Link into transponder
			Transponder.AddStation(this);
		}

		/// <summary>
		/// Ermittelt eine Sonderkonfiguration.
		/// </summary>
		/// <param name="list">Liste aller Sonderkonfigurationen.</param>
		/// <param name="key">Eindeutiger Name der Sonderkonfiguration.</param>
		/// <returns>Eine vorhandene Sonderkonfiguration oder <i>null</i>.</returns>
		private static CustomData FindCustomSetting(List<CustomData> list, string key)
		{
			// Process
			return list.Find(data => Equals(data.Key, key));
		}

		/// <summary>
		/// Ermittelt den Wert einer Sonderkonfiguration.
		/// </summary>
		/// <param name="list">Liste aller Sonderkonfigurationen.</param>
		/// <param name="key">Eindeutiger Name der Sonderkonfiguration.</param>
		/// <returns>Wert der Sonderkonfiguration oder <i>null</i>.</returns>
		public static string GetCustomSetting(List<CustomData> list, string key)
		{
			// Retrieve it
			CustomData data = FindCustomSetting(list, key);

			// Report
			return (null == data) ? null : data.Value;
		}

		/// <summary>
		/// Ermittelt den Wert einer Sonderkonfiguration.
		/// </summary>
		/// <typeparam name="T">Datentyp der Sonderkonfiguration.</typeparam>
		/// <param name="list">Liste aller Sonderkonfigurationen.</param>
		/// <param name="key">Eindeutiger Name der Sonderkonfiguration.</param>
		/// <returns>Wert der Sonderkonfiguration oder <i>null</i>.</returns>
		public static T? GetCustomSetting<T>(List<CustomData> list, string key) where T : struct
		{
			// Retrieve it
			string data = GetCustomSetting(list, key);

			// None
			if (null == data) return null;

			// Use late binding to convert
			return (T)typeof(T).InvokeMember("Parse", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { data });
		}

		/// <summary>
		/// Ändert eine Sonderkonfiguration oder legt eine neue an, wenn noch keine
		/// vorhanden ist.
		/// </summary>
		/// <typeparam name="T">Datentyp der Sonderkonfiguration.</typeparam>
		/// <param name="list">Liste aller Sonderkonfigurationen.</param>
		/// <param name="key">Eindeutiger Name der Sonderkonfiguration.</param>
		/// <param name="value">Neuer Wert der Sonderkonfiguration. Ist dieser <i>null</i>,
		/// so wird die Sonderkonfiguration entfernt.</param>
		public static void SetCustomSetting<T>(List<CustomData> list, string key, T? value) where T : struct
		{
			// Forward
			SetCustomSetting(list, key, value.HasValue ? value.Value.ToString() : null);
		}

		/// <summary>
		/// Ändert eine Sonderkonfiguration oder legt eine neue an, wenn noch keine
		/// vorhanden ist.
		/// </summary>
		/// <param name="list">Liste aller Sonderkonfigurationen.</param>
		/// <param name="key">Eindeutiger Name der Sonderkonfiguration.</param>
		/// <param name="value">Neuer Wert der Sonderkonfiguration. Ist dieser <i>null</i>,
		/// so wird die Sonderkonfiguration entfernt.</param>
		public static void SetCustomSetting(List<CustomData> list, string key, string value)
		{
			// Retrieve it
			CustomData data = FindCustomSetting(list, key);

			// None
			if (null == data)
			{
				// No data to be set
				if (null == value) return;

				// Create new
				list.Add(new CustomData(key, value));
			}
			else if (null == value)
			{
				// Discard
				list.Remove(data);
			}
			else
			{
				// Update
				data.Value = value;
			}
		}

		/// <summary>
		/// Ermittelt den Wert einer Sonderkonfiguration zu diesem Sender.
		/// </summary>
		/// <typeparam name="T">Datentyp der Sonderkonfiguration.</typeparam>
		/// <param name="key">Eindeutiger Name der Sonderkonfiguration.</param>
		/// <returns>Wert der Sonderkonfiguration oder <i>null</i>.</returns>
		public T? GetCustomSetting<T>(string key) where T : struct
		{
			// Forward
			return GetCustomSetting<T>(CustomSettings, key);
		}

		/// <summary>
		/// Ermittelt den Wert einer Sonderkonfiguration zu diesem Sender.
		/// </summary>
		/// <param name="key">Eindeutiger Name der Sonderkonfiguration.</param>
		/// <returns>Wert der Sonderkonfiguration oder <i>null</i>.</returns>
		public string GetCustomSetting(string key)
		{
			// Forward
			return GetCustomSetting(CustomSettings, key);
		}

		/// <summary>
		/// Ändert eine Sonderkonfiguration zu diesem Sender oder legt eine neue an, wenn noch keine
		/// vorhanden ist.
		/// </summary>
		/// <typeparam name="T">Datentyp der Sonderkonfiguration.</typeparam>
		/// <param name="key">Eindeutiger Name der Sonderkonfiguration.</param>
		/// <param name="value">Neuer Wert der Sonderkonfiguration. Ist dieser <i>null</i>,
		/// so wird die Sonderkonfiguration entfernt.</param>
		public void SetCustomSetting<T>(string key, T? value) where T : struct
		{
			// Forward
			SetCustomSetting(CustomSettings, key, value);
		}

		/// <summary>
		/// Ändert eine Sonderkonfiguration zu diesem Sender oder legt eine neue an, wenn noch keine
		/// vorhanden ist.
		/// </summary>
		/// <param name="key">Eindeutiger Name der Sonderkonfiguration.</param>
		/// <param name="value">Neuer Wert der Sonderkonfiguration. Ist dieser <i>null</i>,
		/// so wird die Sonderkonfiguration entfernt.</param>
		public void SetCustomSetting(string key, string value)
		{
			// Forward
			SetCustomSetting(CustomSettings, key, value);
		}

		private void SetAudioInfos(bool ac3, AudioInfo[] infos)
		{
			// Create new map
			Dictionary<string, AudioInfo> audioMap = new Dictionary<string, AudioInfo>();

			// Copy all of alternate type
			foreach (KeyValuePair<string, AudioInfo> audio in m_AudioMap)
				if (ac3 != audio.Value.AC3)
					audioMap[audio.Key] = audio.Value;

			// Add new
			if (null != infos)
				foreach (AudioInfo audio in infos)
					audioMap[audio.Name] = new AudioInfo(audio.PID, audio.Name, ac3);

			// Update map
			m_AudioMap = audioMap;	
		}

		private AudioInfo[] GetAudioInfos(bool ac3)
		{
			// Create
			List<AudioInfo> result = new List<AudioInfo>();

			// Fill
			foreach (AudioInfo audio in m_AudioMap.Values)
				if (ac3 == audio.AC3)
					result.Add(audio);

			// Report
			return result.ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement("AudioInfo")]
		public AudioInfo[] AudioInfos
		{
			get
			{
				// Forward
				return GetAudioInfos(false);
			}
			set
			{
				// Update
				SetAudioInfos(false, value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement("AC3Info")]
		public AudioInfo[] AC3Infos
		{
			get
			{
				// Forward
				return GetAudioInfos(true);
			}
			set
			{
				// Update
				SetAudioInfos(true, value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="audioPID"></param>
		/// <returns></returns>
		public string FindISOLanguage(ushort audioPID)
		{
			// Loop over
			foreach (AudioInfo audio in m_AudioMap.Values)
				if (audioPID == audio.PID)
					return FindISOLanguage(audio.Name);

			// Not found
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="audioName"></param>
		/// <returns></returns>
		public static string FindISOLanguage(string audioName)
		{
			// Not possible
			if (string.IsNullOrEmpty(audioName)) return null;

			// Split off
			string[] parts = audioName.Split(' ', '[');

			// Find it
			string shortName;
			if (LanguageMap.TryGetValue(parts[0], out shortName)) return shortName;

			// Find it
			if (EnglishLanguageMap.TryGetValue(parts[0], out shortName)) return shortName;

			// Not found
			return parts[0];
		}

		/// <summary>
		/// Report the current mapping of native names to ISO names.
		/// </summary>
		public static Dictionary<string, string> LanguageMap
		{
			get
			{
				// Lock the map
				LoadLanguageMap();

				// Report
				return m_LanguageMap;
			}
		}

		/// <summary>
		/// Report the current mapping of native names to ISO names.
		/// </summary>
		public static Dictionary<string, string> EnglishLanguageMap
		{
			get
			{
				// Lock the map
				LoadLanguageMap();

				// Report
				return m_LanguageMapEx;
			}
		}

		/// <summary>
		/// Fill the language mappings once.
		/// </summary>
		private static void LoadLanguageMap()
		{
			// Lock the map and load
			lock (m_LanguageMap)
				if (m_LanguageMap.Count < 1)
					foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
					{
						// Primary
						m_LanguageMap[info.NativeName] = info.ThreeLetterISOLanguageName;

						// Extended
						m_LanguageMapEx[info.EnglishName] = info.ThreeLetterISOLanguageName;
					}
		}

		/// <summary>
		/// Report the name for alternate audio streams.
		/// <seealso cref="FindAudio"/>
		/// </summary>
		[XmlIgnore, Obsolete("This Property has been deprecated, please use AudioMap. Is will only report MP2 Audio Streams.")]
		public string[] AudioNames
		{
			get
			{
				// Create helper
				List<string> names = new List<string>();

				// Fill
				foreach (KeyValuePair<string, AudioInfo> audio in m_AudioMap)
					if (!audio.Value.AC3)
						names.Add(audio.Key);

				// Report
				return names.ToArray();
			}
		}

		public void UpdateAudioMap(AudioInfo oldInfo, AudioInfo newInfo)
		{
			// Remove old - null means a new item should be added
			if (null != oldInfo) m_AudioMap.Remove(oldInfo.Name);

			// Add new
			m_AudioMap[newInfo.Name] = newInfo;
		}

		[XmlIgnore]
		public AudioInfo[] AudioMap
		{
			get
			{
				// Create helper
				AudioInfo[] result = new AudioInfo[m_AudioMap.Count];

				// Fill
				m_AudioMap.Values.CopyTo(result, 0);

				// Report
				return result;
			}
		}

		/// <summary>
		/// Locate an audio stream identifier by its name.
		/// <seealso cref="AudioNames"/>
		/// </summary>
		/// <param name="audioName">Some valid name of an audio stream.</param>
		/// <returns>The corresponding audio stream identifier.</returns>
		[Obsolete("This Method has been deprecated, please use FindAudioInfo.")]
		public ushort FindAudio(string audioName)
		{
			// Report
			return m_AudioMap[audioName].PID;
		}

		public AudioInfo FindAudioInfo(string audioName)
		{
			// Forward
			return m_AudioMap[audioName];
		}

		public AudioInfo FindAudioInfo(ushort PID)
		{
			// Search
			foreach (AudioInfo audio in m_AudioMap.Values)
				if (PID == audio.PID)
					return audio;

			// Not found
			return null;
		}

		/// <summary>
		/// Construct a full (unique) name from the name of the station
		/// and the name of the transponder.
		/// </summary>
		[XmlIgnore]
		public string FullName
		{
			get
			{
				// Construct
				return Name + " [" + TransponderName + "]";
			}
		}

		/// <summary>
		/// Copy all relevant SI data from a more current version of
		/// the station data.
		/// </summary>
		/// <param name="other">The other data from a current scan.</param>
		public void UpdateSIData(Station other)
		{
            // Reset
            DVBSubtitles.Clear();

			// Process
            DVBSubtitles.AddRange(other.DVBSubtitles);
			AudioInfos = other.AudioInfos;
			Encrypted = other.Encrypted;
			VideoType = other.VideoType;
			AudioPID = other.AudioPID;
			AC3Infos = other.AC3Infos;
			VideoPID = other.VideoPID;
			AC3PID = other.AC3PID;
			PCRPID = other.PCRPID;
			TTXPID = other.TTXPID;
		}
	}
}
