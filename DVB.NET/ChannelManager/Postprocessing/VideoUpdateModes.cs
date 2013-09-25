using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Die Möglichkeiten für die Aktualisierung der Bildinformationen zu einem Sender.
	/// </summary>
	public enum VideoUpdateModes
	{
		/// <summary>
		/// Neue Werte ersetzen vorhandene Werte.
		/// </summary>
		Update,

		/// <summary>
		/// Neue Werte werden ignoriert.
		/// </summary>
		Keep,

		/// <summary>
		/// Der PID des Bilddatenstroms kann durch einen neuen Wert ersetzt werden, nicht
		/// aber die Art des Bildsignals - MPEG-2 oder HDTV.
		/// </summary>
		KeepType
	}
}
