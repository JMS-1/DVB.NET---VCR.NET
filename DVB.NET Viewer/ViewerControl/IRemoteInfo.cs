using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.DVB.Viewer
{
	/// <summary>
	/// Diese Schnittstelle erlaubt den Zugriff auf die Einstellungen, die für eine
	/// Verbindung an einen VCR.NET Recording Service notwendig sind.
	/// </summary>
	public interface IRemoteInfo
	{
		/// <summary>
		/// Liest oder setzt den zuletzt verwendeten Sender.
		/// </summary>
		string VCRStation { get; set; }

		/// <summary>
		/// Liest oder setzt das aktive DVB.NET Geräteprofil.
		/// </summary>
		string VCRProfile { get; set; }

		/// <summary>
		/// Liest oder setzt die zuletzt verwendeten Tonspur.
		/// </summary>
		string VCRAudio { get; set; }

		/// <summary>
		/// Meldet die URL zur primären SOAP Schnittstelle des VCR.NET Recording Service.
		/// </summary>
		Uri ServerUri { get; }
	}
}
