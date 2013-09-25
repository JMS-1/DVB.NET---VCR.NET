using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Beschreibt für ein Element, welche Aktualisierung durchgeführt werden soll.
	/// </summary>
	public enum UpdateModes
	{
		/// <summary>
		/// Neue Werte ersetzen vorhandene Werte.
		/// </summary>
		Update,

		/// <summary>
		/// Neue Werte werden ignoriert.
		/// </summary>
		Keep
	}
}
