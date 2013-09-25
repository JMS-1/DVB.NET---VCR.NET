using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Beschreibt für eine Tonspur, welche Aktualisierung durchgeführt werden sollen.
	/// </summary>
	public enum AudioUpdateModes
	{
		/// <summary>
		/// Eine normale Aktualisierung durchführen.
		/// </summary>
		Update,

		/// <summary>
		/// Die Sprachinformationen werden aktualisiert.
		/// </summary>
		Keep,

		/// <summary>
		/// Die Sprachinformationen werden beibehalten.
		/// </summary>
		KeepLanguage
	}
}
