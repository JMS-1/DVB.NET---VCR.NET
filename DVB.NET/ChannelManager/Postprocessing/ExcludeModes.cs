using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Beschreibt f�r ein Element, welche Aktualisierung durchgef�hrt werden soll.
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
