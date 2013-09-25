using System;
using JMS.DVB;
using System.Text;
using System.Collections.Generic;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Beschreibt die m�glichen Meldungstypen w�hrend der Nachbearbeitung
	/// eines Sendersuchlaufs.
	/// </summary>
	public enum PostprocessingReportModes
	{
		/// <summary>
		/// Nur eine Information.
		/// </summary>
		Information,

		/// <summary>
		/// Eine Warnung, deren Ursache gekl�rt werden sollte, durch die aber
		/// vermutlich kein Fehlverhalten ausgel�st wird.
		/// </summary>
		Warning,

		/// <summary>
		/// Ein Fehler, durch den vermutlich ein fehlverhalten ausgel�st wird.
		/// </summary>
		Error
	}

	/// <summary>
	/// Die Schnittstelle, um w�hrend der Nachbearbeitung eines Sendersuchlaufs 
	/// Meldungen zu protokollieren.
	/// </summary>
	public interface IPostprocessingReport
	{
		/// <summary>
		/// Beginnt die Aktualisierung eines Senders.
		/// </summary>
		/// <param name="station">Der betroffene Sender.</param>
		/// <returns>Muss vom Aufrufer freigegeben werden, wenn der Sender aktualisiert 
		/// wurde.</returns>
		IDisposable BeginUpdate(Station station);

		/// <summary>
		/// Erzeugt eine Meldung w�hrend einer Aktualisierung.
		/// </summary>
		/// <param name="mode">Art der Meldung.</param>
		/// <param name="format">Formatierungsinformation.</param>
		/// <param name="args">Parameter f�r die Formatierung.</param>
		void Report(PostprocessingReportModes mode, string format, params object[] args);
	}
}
