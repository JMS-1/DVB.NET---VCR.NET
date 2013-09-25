using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.DVB.Viewer
{
	/// <summary>
	/// Informationen über die Auswahl der Sender.
	/// </summary>
	public interface IChannelInfo
	{
		/// <summary>
		/// Gesetzt, wenn Radiosender in der Senderliste berücksichtigt werden sollen.
		/// </summary>
		bool UseRadio { get; }

		/// <summary>
		/// Gesetzt, wenn unverschlüsselte Sender in der Senderliste berücksichtigt werden sollen.
		/// </summary>
		bool FreeTV { get; }

		/// <summary>
		/// Gesetzt, wenn verschlüsselte Sender in der Senderliste berücksichtigt werden sollen.
		/// </summary>
		bool PayTV { get; }

		/// <summary>
		/// Gesetzt, wenn Fernsehsender in der Senderliste berücksichtigt werden sollen.
		/// </summary>
		bool UseTV { get; }
	}
}
