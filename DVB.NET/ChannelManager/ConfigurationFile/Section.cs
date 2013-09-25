using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace JMS.ChannelManagement.ConfigurationFile
{
    public abstract class Section
	{
		public readonly StringList Transponders = new StringList();

		private bool m_TransponderMode = false;
		private string m_SymbolicName = null;
		private string m_DisplayName = null;

		protected Section()
		{
		}

		public virtual string DisplayName
		{
			get
			{
				// Report
				return m_DisplayName;
			}
		}

		public string SymbolicName
		{
			get
			{
				// Report
				return m_SymbolicName;
			}
		}

		public virtual void SetStartToken(string token)
		{
		}

		protected virtual bool ChangeToken(string token)
		{
			// Already in transponder mode
			if (m_TransponderMode)
			{
				// Time to check rates
				Transponders.Finish();

				// Done
				return false;
			}

			// Activate
			m_TransponderMode = token.Equals("[DVB]");

			// Go on
			return m_TransponderMode;
		}

		protected virtual void Add(int index, string data)
		{
			// Check mode
			if (m_TransponderMode)
			{
				// Forward
				Transponders.Add(index, data);
			}
			else
			{
				// Load
				switch (index)
				{
					case 1: m_SymbolicName = data; break;
					case 2: m_DisplayName = data; break;
					default: throw new InvalidDataException(data);
				}
			}
		}

		protected virtual void Finish(ConfigurationCollection collection)
		{
			// Test list
			Transponders.Finish();

			// Test names
			if (null == m_SymbolicName) throw new ArgumentNullException("SymbolicName");
			if (null == m_DisplayName) throw new ArgumentNullException("DisplayName");
		}

		public string Load(StreamReader reader, ConfigurationCollection collection)
		{
			// Process
			for (; ; )
			{
				// Next line
				string line = reader.ReadLine();

				// Done
				if (null == line)
				{
					// Terminate
					Finish(collection);

					// Stop here
					return null;
				}

				// Skip comments
				if (line.StartsWith(";")) continue;

				// Normalize
				line = line.Trim();

				// Skip empty lines
				if (line.Length < 1) continue;

				// Check for next token
				if (line.StartsWith("["))
				{
					// See if we can handle it
					if (ChangeToken(line)) continue;

					// Terminate
					Finish(collection);

					// Done
					return line;
				}

				// Split line
				string[] split = line.Split('=');

				// Not possible
				if (2 != split.Length) throw new InvalidDataException(line);

				// Process
				Add(int.Parse(split[0].TrimEnd()), split[1].TrimStart());
			}
		}
	}
}
