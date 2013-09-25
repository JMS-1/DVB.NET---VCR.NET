using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace JMS.DVB.TS.VideoText
{
	/// <summary>
	/// Enthält Informationen zum Zeichnen eines einzelnen Videotext Zeichens.
	/// </summary>
	internal class CharDrawingContext
	{
		/// <summary>
		/// Die zugehörige Zeile.
		/// </summary>
		public readonly LineDrawingContext Line;

		/// <summary>
		/// Die aktuelle Vordergrundfarbe.
		/// </summary>
		public Brush Foreground;

		/// <summary>
		/// Die aktuelle Hintergrundfarbe.
		/// </summary>
		public Brush Background;

		/// <summary>
		/// Erzeugt eine neue Zeichenumgebung.
		/// </summary>
		/// <param name="line">Die zugehörige Zeile auf der Videotext Seite.</param>
		public CharDrawingContext(LineDrawingContext line)
		{
			// Remember
			Line = line;

			// Setup
			Foreground = LineDrawingContext.Brushes[Color.White];
			Background = null;
		}

		/// <summary>
		/// Erzeugt eine Kopie dieser Arbeitsumgebung.
		/// </summary>
		/// <returns>Eine exakte Kopie der Arbeitsumgebung.</returns>
		public CharDrawingContext Clone()
		{
			// Create clone
			CharDrawingContext clone = new CharDrawingContext(Line);

			// Copy all data
			clone.Foreground = Foreground;
			clone.Background = Background;

			// Report
			return clone;
		}
	}
}
