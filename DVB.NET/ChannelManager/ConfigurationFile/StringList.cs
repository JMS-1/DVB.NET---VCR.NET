using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace JMS.ChannelManagement.ConfigurationFile
{
    public class StringList : IEnumerable
	{
		private string[] m_Strings = null;

		public StringList()
		{
		}

		public void Add(int index, string data)
		{
			// Check mode
			if (null == m_Strings)
			{
				// Must be index 0
				if (0 != index) throw new ArgumentOutOfRangeException("index");

				// Create
				m_Strings = new string[int.Parse(data)];
			}
			else
			{
				// Load
				m_Strings[index - 1] = data;
			}
		}

		public void Finish()
		{
			// Overall
			if (null == m_Strings) throw new ArgumentNullException("StringList");

			// Test all
			for (int i = m_Strings.Length; i-- > 0; )
				if (null == m_Strings[i])
					throw new InvalidDataException(i.ToString());
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			// Forward
			return m_Strings.GetEnumerator();
		}

		#endregion
	}
}
