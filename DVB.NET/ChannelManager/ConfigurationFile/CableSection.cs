using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace JMS.ChannelManagement.ConfigurationFile
{
    public class CableSection : Section
	{
		public readonly StringList SymbolRates = new StringList();

		private bool m_PassThrough = true;
		private bool m_RateMode = false;

		public CableSection()
		{
		}

		protected override void Finish(ConfigurationCollection collection)
		{
			// Self
			SymbolRates.Finish();

			// Base class
			base.Finish(collection);

			// Store
			collection.DVBCReceivers.Add(this);
		}

		protected override bool ChangeToken(string token)
		{
			// See if we should test
			if (!m_RateMode)
			{
				// Set up
				m_PassThrough = false;

				// Test
				m_RateMode = token.Equals("[SYMBOLRATES]");

				// Finish
				return m_RateMode;
			}

			// Recover
			m_PassThrough = true;

			// Use base class
			return base.ChangeToken(token);
		}

		protected override void Add(int index, string data)
		{
			// Check mode
			if (m_PassThrough)
			{
				// Base class must handle
				base.Add(index, data);
			}
			else
			{
				// Forward
				SymbolRates.Add(index, data);
			}
		}
	}
}
