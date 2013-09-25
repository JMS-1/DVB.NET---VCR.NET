using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Viewer
{
	/// <summary>
	/// Allgemeine Schnittstelle für das Positionieren in einer Datei.
	/// </summary>
	public interface IFileReplay
	{
		/// <summary>
		/// Position in der Datei verschieben.
		/// </summary>
		/// <param name="delta">Versatz in Prozent von 0.0 bis 1.0.</param>
		void MovePosition(double delta);

		/// <summary>
		/// Position in der Datei setzen.
		/// </summary>
		double Position { set; }
	}
}
