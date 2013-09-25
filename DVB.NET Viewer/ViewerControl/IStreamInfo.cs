using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Viewer
{
	/// <summary>
	/// Vermittelt den Zugriff auf die Konfiguration für den Netzwerkversand
	/// eines Transport Streams (lokale DVB.NET Hardware) oder den zu
	/// verwendende lokalen TCP/IP Endpunkt.
	/// </summary>
	public interface IStreamInfo
	{
		/// <summary>
		/// Meldet den zu verwendenden UDP Port.
		/// </summary>
		ushort BroadcastPort { get; }

		/// <summary>
		/// Meldet die zu verwendende Multicast Adresse.
		/// </summary>
		string BroadcastIP { get; }
	}
}
