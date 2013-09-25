using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Signatur einer Methode, die aufgerufen werden soll, wenn ein Menüpunkt ausgewählt wird.
    /// </summary>
	public delegate void CallOption();

    /// <summary>
    /// Beschreibt einen Menüpunkt.
    /// </summary>
	public class OptionDisplay
	{
        /// <summary>
        /// Die Methode, die Aufgerufen werden soll, wenn ein Menüpunkt ausgewählt wird.
        /// </summary>
		public readonly CallOption Option;

        /// <summary>
        /// Der Anzeigetext des Menüpunkts.
        /// </summary>
		private string m_OptionName;

        /// <summary>
        /// Erzeugt einen neuen Menüpunkt.
        /// </summary>
        /// <param name="name">Der Anzeigetext des Menüpunkts.</param>
        /// <param name="option">Die Methode, die bei Auswahl des Menüpunkts aufgerufen werden soll.</param>
		public OptionDisplay(string name, CallOption option)
		{
			// Remember
			m_OptionName = name;
			Option = option;
		}

        /// <summary>
        /// Ermittelt den Anzeigetext dieses Menüpunktes.
        /// </summary>
        /// <returns>Der Anzeigetext des Menüpunktes.</returns>
		public override string ToString()
		{
			// Report
			return m_OptionName;
		}
	}

}
