using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Signatur einer Methode, die aufgerufen werden soll, wenn ein Men�punkt ausgew�hlt wird.
    /// </summary>
	public delegate void CallOption();

    /// <summary>
    /// Beschreibt einen Men�punkt.
    /// </summary>
	public class OptionDisplay
	{
        /// <summary>
        /// Die Methode, die Aufgerufen werden soll, wenn ein Men�punkt ausgew�hlt wird.
        /// </summary>
		public readonly CallOption Option;

        /// <summary>
        /// Der Anzeigetext des Men�punkts.
        /// </summary>
		private string m_OptionName;

        /// <summary>
        /// Erzeugt einen neuen Men�punkt.
        /// </summary>
        /// <param name="name">Der Anzeigetext des Men�punkts.</param>
        /// <param name="option">Die Methode, die bei Auswahl des Men�punkts aufgerufen werden soll.</param>
		public OptionDisplay(string name, CallOption option)
		{
			// Remember
			m_OptionName = name;
			Option = option;
		}

        /// <summary>
        /// Ermittelt den Anzeigetext dieses Men�punktes.
        /// </summary>
        /// <returns>Der Anzeigetext des Men�punktes.</returns>
		public override string ToString()
		{
			// Report
			return m_OptionName;
		}
	}

}
