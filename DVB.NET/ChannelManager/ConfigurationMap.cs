using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace JMS.ChannelManagement
{
	/// <summary>
	/// 
	/// </summary>
	public class ConfigurationMap
	{
		/// <summary>
		/// 
		/// </summary>
		private Dictionary<string, ConfigurationFile.Section> m_Receivers = new Dictionary<string, ConfigurationFile.Section>();

		/// <summary>
		/// 
		/// </summary>
		internal ConfigurationMap()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public int Count
		{
			get
			{
				// Forward
				return m_Receivers.Count;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string[] ReceiverNames
		{
			get
			{
				// Create
				string[] result = new string[m_Receivers.Count];

				// Fill
				m_Receivers.Keys.CopyTo(result, 0);

				// Report
				return result;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="receiverName"></param>
		/// <returns></returns>
		internal ConfigurationFile.Section this[string receiverName]
		{
			get
			{
				// Result
				ConfigurationFile.Section result;

				// Read
				if (!m_Receivers.TryGetValue(receiverName, out result)) result = null;

				// Report
				return result;
			}
		}

		/// <summary>
		/// 
		/// </summary>
        /// <param name="sections"></param>
		internal void Load<SectionType>(List<SectionType> sections) where SectionType : ConfigurationFile.Section
		{
			// Process all
			foreach (SectionType section in sections)
			{
				// Add to map
				m_Receivers[section.DisplayName] = section;
			}
		}
	}
}
