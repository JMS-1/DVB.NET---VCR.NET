using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.DVB.Viewer
{
	/// <summary>
	/// Informationen �ber die Auswahl der Sender.
	/// </summary>
	public interface IChannelInfo
	{
		/// <summary>
		/// Gesetzt, wenn Radiosender in der Senderliste ber�cksichtigt werden sollen.
		/// </summary>
		bool UseRadio { get; }

		/// <summary>
		/// Gesetzt, wenn unverschl�sselte Sender in der Senderliste ber�cksichtigt werden sollen.
		/// </summary>
		bool FreeTV { get; }

		/// <summary>
		/// Gesetzt, wenn verschl�sselte Sender in der Senderliste ber�cksichtigt werden sollen.
		/// </summary>
		bool PayTV { get; }

		/// <summary>
		/// Gesetzt, wenn Fernsehsender in der Senderliste ber�cksichtigt werden sollen.
		/// </summary>
		bool UseTV { get; }
	}
}
