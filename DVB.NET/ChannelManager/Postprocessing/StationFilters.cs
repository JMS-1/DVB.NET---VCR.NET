using System;
using JMS.DVB;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Ein Filter zur exakten Erkennung eines Sender.
	/// </summary>
	[Serializable]
	public class ByStationId : StationFilter
	{
		/// <summary>
		/// Die eindeutige Kennung des Senders.
		/// </summary>
		private Identifier m_Id;

		/// <summary>
		/// Liest oder setzt die eindeutige Kennung des Senders.
		/// </summary>
		public Identifier Identifier
		{
			get 
			{ 
				// Report
				return m_Id; 
			}
			set 
			{ 
				// Change
				m_Id = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public ByStationId()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="id">Die gew�nschte eindeutige Kennung.</param>
		public ByStationId(Identifier id)
		{
			// Remember
			m_Id = id;
		}

		/// <summary>
		/// Meldet, ob ein Sender die gew�nschte eindeutige Kennung besitzt.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>Gesetzt, wenn der Sender die gew�nschte eindeutige Kennung besitzt.</returns>
		public override bool Match(Station station)
		{
			// Direct
			return Equals(m_Id, station);
		}
	}

	/// <summary>
	/// Erkennt, ob ein Sender verschl�sselt oder unverschl�sselt ist.
	/// </summary>
	[Serializable]
	public class ByEncryption : StationFilter
	{
		/// <summary>
		/// Der gew�nschte Verschl�sselungsmodus.
		/// </summary>
		private bool m_Encrypted;

		/// <summary>
		/// Liest oder setzt den gew�nschten Verschl�sselungsmodus des Senders.
		/// </summary>
		public bool IsEncrypted
		{
			get 
			{ 
				// Report
				return m_Encrypted; 
			}
			set 
			{ 
				// Update
				m_Encrypted = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public ByEncryption()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="isEncrypted">Gesetzt, wenn dieser Filter verschl�sselte
		/// Sender erkennen soll.</param>
		public ByEncryption(bool isEncrypted)
		{
			// Remember
			m_Encrypted = isEncrypted;
		}

		/// <summary>
		/// Meldet, ob ein Sender den gew�nschten Verschl�sselungsmodus besitzt.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>Gesetzt, wenn der Sender den gew�nschten Verschl�sselungsmodus
		/// besitzt.</returns>
		public override bool Match(Station station)
		{
			// Direct
			return (station.Encrypted == m_Encrypted);
		}
	}

	/// <summary>
	/// Ein Filter zur Unterscheidung zwischen Radio- und Fernsehsendern.
	/// </summary>
	[Serializable]
	public class ByServiceType : StationFilter
	{
		/// <summary>
		/// Gesetzt, wenn der Filter Radiosender erkennen soll.
		/// </summary>
		private bool m_Radio;

		/// <summary>
		/// Liest oder setzt, ob der Filter Radio- und nicht Fernsehsender
		/// erkennen soll.
		/// </summary>
		public bool IsRadio
		{
			get 
			{ 
				// Report
				return m_Radio; 
			}
			set 
			{ 
				// Update
				m_Radio = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public ByServiceType()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="isRadio">Gesetzt, wenn der Filter Radio- und nicht Fernsehsender
		/// erkennen soll.</param>
		public ByServiceType(bool isRadio)
		{
			// Remember
			m_Radio = isRadio;
		}

		/// <summary>
		/// Meldet, ob ein Sender dem gew�nschten Typus entspricht.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>Gesetzt, wenn der Sender dem gew�nschten Typus entspricht.</returns>
		public override bool Match(Station station)
		{
			// Direct
			return ((0 == station.VideoPID) == m_Radio);
		}
	}
}
