using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.ChannelManagement.ConfigurationFile
{
    public class TerrestrialSection : Section
	{
		private bool m_TypeMode = false;
		
		public TerrestrialSection()
		{
		}

		protected override void Finish(ConfigurationCollection collection)
		{
			// Base class can do all of it
			base.Finish(collection);

			// Register
			collection.DVBTReceivers.Add(this);
		}

		public override void SetStartToken(string token)
		{
			// Check it
			m_TypeMode = token.Equals("[TERTYPE]");
		}

		protected override bool ChangeToken(string token)
		{
			// Check mode
			if (!m_TypeMode)
			{
				// Check it
				m_TypeMode = token.Equals("[TERTYPE]");

				// Done
				if (m_TypeMode) return true;
			}

			// Forward to base class
			return base.ChangeToken(token);
		}

		protected override void Add(int index, string data)
		{
			// Can be processed by base class
			if (m_TypeMode) base.Add(index, data);
		}

		public override string DisplayName
		{
			get
			{
				// Merge
				return base.DisplayName + ", " + base.SymbolicName;
			}
		}
	}
}
