using System;
using JMS.DVB;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Verwaltet die gewünschten Nachbearbeitungsoperationen für ein DVB.NET
	/// Geräteprofil.
	/// </summary>
	[Serializable]
	public class PostProcessingInfo : ICloneable
	{
		/// <summary>
		/// Beschreibt einen Änderungsoperation in der Nachbearbeitungsliste.
		/// </summary>
		[Serializable]
		public class UpdateInfo
		{
			/// <summary>
			/// Der Filter zur Erkennung des Senders.
			/// </summary>
			private StationFilter m_Filter;

			/// <summary>
			/// Liest oder setzt den Filter, der die Sender erkennt, auf die
			/// die Änderung <see cref="Info"/> angewendet werden soll.
			/// </summary>
			public StationFilter Filter
			{
				get 
				{ 
					// Report
					return m_Filter; 
				}
				set 
				{ 
					// Change
					m_Filter = value; 
				}
			}

			/// <summary>
			/// Die auszuführende Veränderung am Sender.
			/// </summary>
			private StationUpdateInfo m_Info;

			/// <summary>
			/// Liest oder setzt die Informationen zu den Veränderungen, die nach
			/// einem Sendersuchlauf an einem dabei erkannten Sender auszuführen sind.
			/// </summary>
			public StationUpdateInfo Info
			{
				get 
				{ 
					// Report
					return m_Info; 
				}
				set 
				{ 
					// Change
					m_Info = value; 
				}
			}

			/// <summary>
			/// Erzeugt eine neue Änderungsbeschreibung.
			/// </summary>
			/// <remarks>
			/// Dieser Konstruktor wird für die Deserialisierung benötigt.
			/// </remarks>
			public UpdateInfo()
			{
			}

			/// <summary>
			/// Erzeugt eine neue Änderungsbeschreibung.
			/// </summary>
			/// <param name="filter">Filter zur Erkennung der Sender, auf die diese
			/// Instanz angewendet werden soll.</param>
			/// <param name="updateInfo">Die auf die erkannten Sender anzuwendende
			/// Veränderung.</param>
			public UpdateInfo(StationFilter filter, StationUpdateInfo updateInfo)
			{
				// Remember
				m_Filter = filter;
				m_Info = updateInfo;
			}
		}

		/// <summary>
		/// Liste aller Veränderungen, die nach einem Sendersuchlauf auf erkannte
		/// Sender angewendet werden sollen.
		/// </summary>
		[XmlElement("Update")]
		public readonly List<UpdateInfo> UpdateInfos = new List<UpdateInfo>();

		/// <summary>
		/// Liste aller Filter zur Erkennung der Sender, die nach einem Sendersuchlauf
		/// automatisch entfernt werden sollen.
		/// </summary>
		/// <remarks>
		/// Dieser Mechanismus wird in DVB.NET 3.5 noch nicht unterstützt.
		/// </remarks>
		[XmlElement("Discard")]
		public readonly List<StationFilter> DiscardFilters = new List<StationFilter>();

		/// <summary>
		/// Erzeugt eine neue Verwaltungsinstanz.
		/// </summary>
		public PostProcessingInfo()
		{
		}

		/// <summary>
		/// Erzeugt eine exakte Kopie dieser Verwaltungsinstanz. Die Kopie wird über 
		/// eine binäre Serialisierung und Deserialisierung erstellt und Kopie sämtliche
		/// Unterstrukturen mit.
		/// </summary>
		/// <returns></returns>
		public PostProcessingInfo Clone()
		{
			// Create serializer
			BinaryFormatter ser = new BinaryFormatter();

			// Serialize to stream
			using (MemoryStream stream = new MemoryStream())
			{
				// Store
				ser.Serialize(stream, this);

				// Reset stream
				stream.Seek(0, SeekOrigin.Begin);

				// Recreate
				return (PostProcessingInfo)ser.Deserialize(stream);
			}
		}

		/// <summary>
		/// Entfernt aus der Änderungsliste <see cref="UpdateInfos"/> alle Instanzen,
		/// die keine Veränderung bewirken würden.
		/// <seealso cref="StationUpdateInfo.IsIdentity"/>
		/// </summary>
		public void Cleanup()
		{
			// Check all update items if something will change
			for (int i = UpdateInfos.Count; i-- > 0; )
				if (UpdateInfos[i].Info.IsIdentity)
					UpdateInfos.RemoveAt(i);
		}

		/// <summary>
		/// Führt alle Änderungsoperationen nach einem Sendersuchlauf durch.
		/// </summary>
		/// <param name="newStation">Ein während des Suchlaufs ermittelter Sender.</param>
		/// <param name="oldStation">Der zugehörige Sender vor dem Suchlauf, dessen Detaildaten
		/// übernommen werden können.</param>
		/// <param name="reporter">Eine Schnittstelle zur Protokollierung von Problemen
		/// bei der Ausführung der Änderungsoperation.</param>
		public void ProcessUpdate(Station newStation, Station oldStation, IPostprocessingReport reporter)
		{
			// Process all
            foreach (UpdateInfo info in UpdateInfos)
                if (info.Filter.Match(oldStation))
                    info.Info.UpdateStation(oldStation, newStation, reporter);
		}

		#region ICloneable Members

		/// <summary>
		/// Erzeugt eine exakte Kopie dieser Verwaltungsinstanz.
		/// <seealso cref="Clone"/>
		/// </summary>
		/// <returns>Eine Kopie dieser Verwaltungsinstanz, wobei auch für alle
		/// Unterstrukturen eine Kopie erstellt wird.</returns>
		object ICloneable.Clone()
		{
			// Forward
			return Clone();
		}

		#endregion
	}
}
