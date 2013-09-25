using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Viewer
{
	/// <summary>
	/// Informationen zu Einstellungen bei Zugriff auf eine lokale DVB.NET Hardware.
	/// </summary>
	public interface ILocalInfo
	{
		/// <summary>
		/// Liest oder setzt das Aufzeichnungsverzeichnis.
		/// </summary>
		string RecordingDirectory { get; set; }

		/// <summary>
		/// Liest oder setzt den zuletzt verwendeten Sender.
		/// </summary>
		string LocalStation { get; set; }

		/// <summary>
		/// Liest oder setzt die zuletzt verwendet Tonspur.
		/// </summary>
		string LocalAudio { get; set; }
	}
}
