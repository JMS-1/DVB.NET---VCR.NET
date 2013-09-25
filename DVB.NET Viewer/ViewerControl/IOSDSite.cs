using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.DVB.Viewer
{
	/// <summary>
	/// Beschreibt die Kommunikation eines OSD Bildes mit der zugehörigen
	/// Anwendung.
	/// </summary>
	public interface IOSDSite : IWin32Window
	{
		/// <summary>
		/// Aktiviert ein OSD Bild - alle Angaben sind in Prozent von 0.0 bis 1.0;
		/// </summary>
		/// <param name="bitmap">Das Bild.</param>
		/// <param name="left">Linke obere Ecke.</param>
		/// <param name="top">Linke obere Ecke.</param>
		/// <param name="right">Rechte untere Ecke.</param>
		/// <param name="bottom">Rechte untere Ecke.</param>
		/// <param name="alpha">Transparenz des Bildes - <i>null</i>, wenn eine Darstellung ohne DirectShow Overlay erfolgen soll.</param>
		/// <param name="transparent">Transparenzfarbe.</param>
        /// <param name="mode">Die Art der angezeigten Daten.</param>
		void Show(Bitmap bitmap, double left, double top, double right, double bottom, double? alpha, Color? transparent, OSDShowMode mode);

		/// <summary>
		/// Blendet das OSD aus.
		/// </summary>
		void Hide();

		/// <summary>
		/// Gesetzt, wenn das OSD über das DirectShow Overlay angezeigt werden kann.
		/// </summary>
		bool UsesOverlay { get; }
	}
}
