using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Beschreibt f�r eine Tonspur, welche Aktualisierung durchgef�hrt werden sollen.
	/// </summary>
	public enum AudioUpdateModes
	{
		/// <summary>
		/// Eine normale Aktualisierung durchf�hren.
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
