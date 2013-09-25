using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Beschreibt für ein Element, ob es ein- oder ausgeschlossen werden soll.
	/// </summary>
    public enum ExcludeModes
	{
		/// <summary>
		/// Das Element wird normal behandelt.
		/// </summary>
		Include,

		/// <summary>
		/// Das Element wird entfernt.
		/// </summary>
		Exclude
	}
}
