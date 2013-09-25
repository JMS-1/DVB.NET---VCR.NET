using System;

namespace JMS.DVB.Viewer
{
	/// <summary>
	/// Operationsmodi der Anwendung, die �ber Kommandozeilenparameter beim
	/// Starten ausgew�hlt werden.
	/// </summary>
	public enum StartupModes
	{
		/// <summary>
		/// Zugriff auf eine lokale DVB.NET Hardware.
		/// </summary>
		LocalDVB,

		/// <summary>
		/// Zu einem VCR.NET Recording Service verbinden.
		/// </summary>
		RemoteVCR,

		/// <summary>
		/// An einen DVB.NET / VCR.NET Transport Stream anh�ngen.
		/// </summary>
		ConnectTCP,

		/// <summary>
		/// Einen lokalen Transport Stream wiedergeben.
		/// </summary>
		PlayLocalFile,

		/// <summary>
		/// Eine VCR.NET Aufzeichnungsdatei wiedergeben.
		/// </summary>
		PlayRemoteFile,

		/// <summary>
		/// Eine laufende Aufzeichnung betrachten.
		/// </summary>
		WatchOrTimeshift,
	}
}
