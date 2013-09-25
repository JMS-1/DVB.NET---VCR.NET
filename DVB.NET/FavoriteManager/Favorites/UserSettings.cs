using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB.Favorites
{
	[Serializable]
	public class UserSettings
	{
		public bool EnableShortCuts = false;

		public string PreferredLanguage = "Deutsch";

		public bool PreferAC3 = false;

		public string[] FavoriteChannels = { };

		public UserSettings()
		{
		}

		private static string DefaultPath
		{
			get
			{
				// Attach to user settings directory
				string userDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

				// Append
				FileInfo file = new FileInfo(Path.Combine(userDir, @"JMS\DVB.NET Favorites.xml"));

				// Create directory
				if (!file.Directory.Exists) file.Directory.Create();

				// Report
				return file.FullName;
			}
		}

		public void Save()
		{
			// Open file
			using (FileStream file = new FileStream(DefaultPath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				// Create serializer
				XmlSerializer serializer = new XmlSerializer(GetType());

				// Create settings
				XmlWriterSettings settings = new XmlWriterSettings();

				// Configure settings
				settings.Encoding = Encoding.Unicode;
				settings.Indent = true;

				// Process
				using (XmlWriter writer = XmlWriter.Create(file, settings)) serializer.Serialize(writer, this);
			}
		}

		public static UserSettings Load()
		{
			try
			{
				// Open file
				using (FileStream file = new FileStream(DefaultPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					// Create serializer
					XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));

					// Load
					return (UserSettings)serializer.Deserialize(file);
				}
			}
			catch
			{
				// Create a new one on every error
				return new UserSettings();
			}
		}
	}
}
