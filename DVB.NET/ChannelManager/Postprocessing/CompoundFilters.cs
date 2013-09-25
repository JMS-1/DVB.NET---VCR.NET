using System;
using JMS.DVB;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Instanzen dieser Klasse kombinieren mehrere andere Nachbearbeitungsfilter.
	/// </summary>
	[Serializable]
	public abstract class CompoundFilter: StationFilter
	{
		/// <summary>
		/// Die Liste der untergeordneten Filter.
		/// </summary>
		private List<StationFilter> m_Filters = new List<StationFilter>();

		/// <summary>
		/// Meldet die Liste der verwalteten Filter.
		/// </summary>
		public List<StationFilter> Filters
		{
			get 
			{ 
				// Report
				return m_Filters; 
			}
		} 

		/// <summary>
		/// Initialisiert eine Verwaltungsinstanz.
		/// </summary>
		/// <param name="filters">Liste der zu verwaltenden Filter.</param>
		protected CompoundFilter(StationFilter[] filters)
		{
			// Remember
			if (null != filters) Filters.AddRange(filters);
		}

		/// <summary>
		/// Initialisiert eine Verwaltungsinstanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
		/// </remarks>
		protected CompoundFilter()
		{
		}
	}

	/// <summary>
	/// Filter dieser Art erkennen einen Sender als Treffer an, wenn mindestens
	/// einer der untergeordneten Filter dies tut.
	/// </summary>
	[
		Serializable,
		XmlType("Union")
	]
	public class UnionFilters: CompoundFilter
	{
		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="filters">Liste der untergeordneten Filter.</param>
		public UnionFilters(params StationFilter[] filters)
			: base(filters)
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
		/// </remarks>
		public UnionFilters()
		{
		}

		/// <summary>
		/// Prüft, ob ein Sender von mindestens einem der verwalteten Filter
		/// als Treffer erkannt wird.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>Gesetzt, wenn mindestens ein Filter den Sender als
		/// Treffer erkennt.</returns>
		public override bool Match(Station station)
		{
			// Test all
			foreach (StationFilter filter in Filters)
				if (filter.Match(station))
					return true;

			// No match
			return false;
		}
	}

	/// <summary>
	/// Filter dieser Art erkennen einen Sender nur dann als Treffer an, wenn
	/// alle untergeordneten Filter dies tun.
	/// </summary>
	[
		Serializable,
		XmlType("Intersection")
	]
	public class IntersectionFilters : CompoundFilter
	{
		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="filters">Liste der untergeordneten Filter.</param>
		public IntersectionFilters(params StationFilter[] filters)
			: base(filters)
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
		/// </remarks>
		public IntersectionFilters()
		{
		}

		/// <summary>
		/// Prüft, ob ein Sender von allen verwalteten Filtern
		/// als Treffer erkannt wird.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>Gesetzt, wenn alle Filter den Sender als
		/// Treffer erkennen.</returns>
		public override bool Match(Station station)
		{
			// Test all
			foreach (StationFilter filter in Filters)
				if (!filter.Match(station))
					return false;

			// No match
			return true;
		}
	}
}
