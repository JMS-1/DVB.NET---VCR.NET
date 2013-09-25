using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.ChannelManagement
{
    /// <summary>
    /// Beschreibt die benutzerspezifischen Vorgaben.
    /// </summary>
	[
		Serializable,
		XmlRoot("UserProfile")
	]
	public class UserProfile
	{
        /// <summary>
        /// Der XML Namensraum, der für die serialisierte Form verwendet wird.
        /// </summary>
		private const string ProfileNamespace = "http://jochen-manns.de/DVB.NET/Profiles/User";

        /// <summary>
        /// Die Datei, aus der diese Vorgaben entnommen wurden.
        /// </summary>
		private FileInfo m_File;

        /// <summary>
        /// Die bevorzugte Benutzersprache.
        /// </summary>
		public string PreferredLanguage { get; set; }

	    /// <summary>
	    /// Das zugehörige Geräteprofil.
	    /// </summary>
		public DeviceProfileReference Profile { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob die Auswahl des Geräteprofils immer angezeigt werden soll.
        /// </summary>
		public bool AllwaysShowDialog { get; set; }
	
        /// <summary>
        /// Erzeugt neue Vorgaben.
        /// </summary>
		public UserProfile()
		{
		}

        /// <summary>
        /// Spichert die Vorgaben ab.
        /// </summary>
		public void Save()
		{
			// Open and forward
			using (FileStream stream = new FileStream(m_File.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				// Create configuration
				XmlWriterSettings settings = new XmlWriterSettings();

				// Fill configuration
				settings.Encoding = Encoding.Unicode;
				settings.Indent = true;

				// Create serializer
				XmlSerializer serializer = new XmlSerializer(GetType(), ProfileNamespace);

				// Process
				using (XmlWriter writer = XmlWriter.Create(stream, settings))
				{
					// Store
					serializer.Serialize(writer, this);
				}
			}
		}

        /// <summary>
        /// Lädt die Vorgaben des aktuellen Anwenders.
        /// </summary>
        /// <returns>Die Vorgaben für diesen Anwender.</returns>
		public static UserProfile Load()
		{
			// Get the user profile directory
			DirectoryInfo profileDir = DeviceProfile.UserProfilePath;

			// Create
			profileDir.Create();

			// Get the file name
			FileInfo profile = new FileInfo(Path.Combine(profileDir.FullName, "UserProfile.dup"));

			// The new profile
			UserProfile settings;

			// Load or create
			if (profile.Exists)
			{
				// Open
				using (FileStream stream = new FileStream(profile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					// Create serializer
					XmlSerializer serializer = new XmlSerializer(typeof(UserProfile), ProfileNamespace);

					// Process
					settings = (UserProfile)serializer.Deserialize(stream);
				}
			}
			else
			{
				// Create new
				settings = new UserProfile();
			}

			// Remember root
			settings.m_File = profile;

			// Report
			return settings;
		}
	}
}
