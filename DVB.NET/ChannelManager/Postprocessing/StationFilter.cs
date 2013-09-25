using System;
using JMS.DVB;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Basisklasse zur Implementierung von Nachbearbeitungsfiltern.
	/// </summary>
	/// <remarks>
	/// Ein Nachbearbeitungsfilter wird auf einen Sender angewendet und meldet,
	/// ob dieser in den Zuständigkeitsbereich des Filters fällt.
	/// </remarks>
	[
		Serializable,
		XmlInclude(typeof(UnionFilters)),
		XmlInclude(typeof(IntersectionFilters)),
		XmlInclude(typeof(ByStationId)),
		XmlInclude(typeof(ByReceiverType)),
		XmlInclude(typeof(ByLNBIndex)),
		XmlInclude(typeof(ByFrequency)),
		XmlInclude(typeof(BySpectrumInversion)),
		XmlInclude(typeof(ByEncryption)),
		XmlInclude(typeof(ByServiceType)),
		XmlInclude(typeof(ByDVBCModulation)),
		XmlInclude(typeof(ByDVBSModulation)),
		XmlInclude(typeof(ByPolarisation)),
		XmlInclude(typeof(ByTransponder))
	]
	public abstract class StationFilter
	{
		/// <summary>
		/// Initialisiert eine neue Instanz.
		/// </summary>
		protected StationFilter()
		{
		}

		/// <summary>
		/// Meldet, ob ein Sender in den Zuständigkeitsbereich des Filters fällt.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>Gesetzt, wenn der Filter den Sender erkennt.</returns>
		public abstract bool Match(Station station);
	}
}
