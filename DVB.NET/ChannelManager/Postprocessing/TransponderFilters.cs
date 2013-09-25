using System;
using JMS.DVB;
using System.Text;
using JMS.DVB.Cable;
using JMS.DVB.Satellite;
using JMS.DVB.Terrestrial;
using System.Collections.Generic;

namespace JMS.ChannelManagement.Postprocessing
{
	/// <summary>
	/// Pr�ft die Art eines Transponders zu einem Sender.
	/// </summary>
	[Serializable]
	public class ByReceiverType : StationFilter
	{
		/// <summary>
		/// Die ben�tigte Art des Transponders.
		/// </summary>
		private FrontendType m_Type = FrontendType.Unknown;

		/// <summary>
		/// Liest oder setzt die Art des Transponders.
		/// </summary>
		public FrontendType Type
		{
			get 
			{ 
				// Report
				return m_Type; 
			}
			set 
			{ 
				// Update
				m_Type = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public ByReceiverType()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="type">Die gew�nschte Art des Transponders.</param>
		public ByReceiverType(FrontendType type)
		{
			// Remember
			m_Type = type;
		}

		/// <summary>
		/// Meldet f�r einen Sender, ob dieser zu einem Transponder einer
		/// bestimmten Art geh�rt.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>Gesetzt, wenn der zugeh�rige Transponder der gew�nschten Art
		/// entspricht.</returns>
		public override bool Match(Station station)
		{
			// Check type
			switch (Type)
			{
				case FrontendType.Unknown: return true;
				case FrontendType.Cable: return (station.Transponder.GetType() == typeof(CableChannel));
				case FrontendType.Satellite: return (station.Transponder.GetType() == typeof(SatelliteChannel));
				case FrontendType.Terrestrial: return (station.Transponder.GetType() == typeof(TerrestrialChannel));
			}

			// Unknown
			return false;
		}
	}

	/// <summary>
	/// Erkennt Sender, die auf einer bestimmten Frequenz empfangen werden.
	/// </summary>
	[Serializable]
	public class ByFrequency : StationFilter
	{
		/// <summary>
		/// Die gew�nschte Frequenz.
		/// </summary>
		private uint m_Frequency;

		/// <summary>
		/// Liest oder setzt die gew�nschte Empfangsfrequenz.
		/// </summary>
		public uint Frequency
		{
			get 
			{ 
				// Report
				return m_Frequency; 
			}
			set 
			{ 
				// Change
				m_Frequency = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public ByFrequency()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="frequency">Die gew�nschte Frequenz.</param>
		public ByFrequency(uint frequency)
		{
			// Remember
			m_Frequency = frequency;
		}

		/// <summary>
		/// Meldet, ob ein Sender auf einer bestimmten Frequenz empfangen wird.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>Gesetzt, wenn der Sender auf der gew�nschten Frequenz empfangen wird.</returns>
		public override bool Match(Station station)
		{
			// Attach to channel
			Channel channel = station.Transponder as Channel;

			// Something weired
			if (null == channel) return false;

			// Compare
			return (channel.Frequency == m_Frequency);
		}
	}

	/// <summary>
	/// Erkennt Sender nach ihrer spektralen Inversion.
	/// </summary>
	[Serializable]
	public class BySpectrumInversion : StationFilter
	{
		/// <summary>
		/// Die gew�nschte Inversion.
		/// </summary>
		private SpectrumInversion m_Inversion;

		/// <summary>
		/// Liest oder setzt die gw�nschte spektrale Inversion.
		/// </summary>
		public SpectrumInversion Inversion
		{
			get 
			{ 
				// Report
				return m_Inversion; 
			}
			set 
			{ 
				// Update
				m_Inversion = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public BySpectrumInversion()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="inversion">Die gew�nschte spektrale Inversion.</param>
		public BySpectrumInversion(SpectrumInversion inversion)
		{
			// Remember
			m_Inversion = inversion;
		}

		/// <summary>
		/// Meldet, ob ein Sender mit einer bestimmten spektralen Inversion empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>Gesetzt, wenn der Sender mit der gew�nschten Inversion empfangen wurde.</returns>
		public override bool Match(Station station)
		{
			// Attach to channel
			Channel channel = station.Transponder as Channel;

			// Something weired
			if (null == channel) return false;

			// Compare
			return (channel.SpectrumInversion == m_Inversion);
		}
	}

	/// <summary>
	/// Pr�ft f�r DVB-S Sender, ob diese �ber eine bestimmte Antenne empfnagen wurden.
	/// </summary>
	[Serializable]
	public class ByLNBIndex : StationFilter
	{
		/// <summary>
		/// 0-basierter Index der gew�nschten Antenne.
		/// </summary>
		private int m_Index = -1;

		/// <summary>
		/// Liest oder setzt den 0-basierten Index der Antenne.
		/// </summary>
		public int Index
		{
			get 
			{ 
				// Report
				return m_Index; 
			}
			set 
			{ 
				// Update
				m_Index = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public ByLNBIndex() 
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="index">Der gew�nschte 0-basierte Index der Antenne.</param>
		public ByLNBIndex(int index)
		{
			// Remember
			m_Index = index;
		}

		/// <summary>
		/// Meldet, ob ein Sender ein DVB-S Sender ist und dieser �ber eine bestimmte
		/// Antenne empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>Gesetzt f�r DVB-S Sender, die �ber die gw�nschte Antenne empfangen werden.</returns>
		public override bool Match(Station station)
		{
			// Change type
			SatelliteChannel channel = station.Transponder as SatelliteChannel;

			// Process
			return (null != channel) && (m_Index == channel.LNBIndex);
		}
	}

	/// <summary>
	/// Unterscheidet DVB-S Sender nach der Polarisation, auf der sie empfangen werden.
	/// </summary>
	[Serializable]
	public class ByPolarisation : StationFilter
	{
		/// <summary>
		/// Die gew�nschte Polarisation.
		/// </summary>
		private PowerMode m_Mode;

		/// <summary>
		/// Liest oder setzt die Polarisation.
		/// </summary>
		public PowerMode Polarisation
		{
			get 
			{ 
				// Report
				return m_Mode; 
			}
			set 
			{ 
				// Change
				m_Mode = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public ByPolarisation()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="mode">Die gew�nschte Polarisation.</param>
		public ByPolarisation(PowerMode mode)
		{
			// Remember
			m_Mode = mode;
		}

		/// <summary>
		/// Meldet, ob ein Sender ein DVB-S Sender ist, der auf einer bestimmten
		/// Polarisation empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>Gesetzt f�r DVB-S Sender, die auf der gew�nschten Polarisation empfangen
		/// werden.</returns>
		public override bool Match(Station station)
		{
			// Change type
			SatelliteChannel channel = station.Transponder as SatelliteChannel;

			// Process
			return (null != channel) && (channel.Power == m_Mode);
		}
	}

	/// <summary>
	/// Erkennt DVB-S2 Sender.
	/// </summary>
	[Serializable]
	public class ByDVBSModulation : StationFilter
	{
		/// <summary>
		/// Gesetzt, wenn DVB-S2 und nicht DVB-S Sender erkannt werden sollen.
		/// </summary>
		private bool m_S2;

		/// <summary>
		/// Liest oder setzt, welche Sender erkannt werden sollen.
		/// </summary>
		public bool IsS2Modulated
		{
			get 
			{ 
				// Report
				return m_S2; 
			}
			set 
			{ 
				// Change
				m_S2 = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public ByDVBSModulation()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="isS2Modulated">Gesetzt, wenn DVB-S2 und nicht DVB-S Sender
		/// erkannt werden sollen.</param>
		public ByDVBSModulation(bool isS2Modulated)
		{
			// Remember
			m_S2 = isS2Modulated;
		}

		/// <summary>
		/// Meldet, ob ein Sender �ber DVB-S oder DVB-S2 empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>Gesetzt f�r DVB-S Sender, die in der gew�nschten Modulation
		/// empfangen werden.</returns>
		public override bool Match(Station station)
		{
			// Change type
			SatelliteChannel channel = station.Transponder as SatelliteChannel;

			// Process
			return (null != channel) && (channel.S2Modulation == m_S2);
		}
	}

	/// <summary>
	/// Kann f�r DVB-C Sender nach der Modulation unterscheiden.
	/// </summary>
	[Serializable]
	public class ByDVBCModulation : StationFilter
	{
		/// <summary>
		/// Die gew�nschte Modulation.
		/// </summary>
		private Qam m_Modulation;

		/// <summary>
		/// Liest oder setzt die Modulation.
		/// </summary>
		public Qam Modulation
		{
			get 
			{ 
				// Report
				return m_Modulation; 
			}
			set 
			{ 
				// Update
				m_Modulation = value; 
			}
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <remarks>
		/// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
		/// </remarks>
		public ByDVBCModulation()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instzanz.
		/// </summary>
		/// <param name="modulation">Die gew�nschte Modulation.</param>
		public ByDVBCModulation(Qam modulation)
		{
			// Remember
			m_Modulation = modulation;
		}

		/// <summary>
		/// Meldet, ob ein Sender ein DVB-C Sender ist, der mit einer bestimmten
		/// Modulation empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu pr�fende Sender.</param>
		/// <returns>gesetzt f�r DVB-C Sender, die in der gew�nschten Modulation
		/// empfangen werden.</returns>
		public override bool Match(Station station)
		{
			// Change type
			CableChannel channel = station.Transponder as CableChannel;

			// Process
			return (null != channel) && (channel.QAM == m_Modulation);
		}
	}

    /// <summary>
    /// Erkennt, ob sich ein Sender auf einem bestimmten Transponder befindet.
    /// </summary>
    [Serializable]
    public class ByTransponder : StationFilter
    {
        /// <summary>
        /// Der gew�nschte Transponder.
        /// </summary>
        private Transponder m_Transponder;

        /// <summary>
        /// Liest oder setzt den gew�nschten Transponder.
        /// </summary>
        public Transponder Transponder
        {
            get
            {
                // Report
                return m_Transponder;
            }
            set
            {
                // Remember
                m_Transponder = value;
            }
        }

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <remarks>
        /// Dieser Konstruktor wird f�r die Deserialisierung ben�tigt.
        /// </remarks>
        public ByTransponder()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="transponder">Der gew�nschte Transponder.</param>
        public ByTransponder(Transponder transponder)
        {
            //  Must create a clone with no stations
            m_Transponder = transponder.Clone(false);
        }

        /// <summary>
        /// Meldet, ob ein Sender zum gew�nschten Transponder geh�rt.
        /// </summary>
        /// <param name="station">Der zu pr�fende Sender.</param>
        /// <returns>Gesetzt, wenn der Sender auf dem gew�nschten Transponder gefunden wurde.</returns>
        public override bool Match(Station station)
        {
            // Direct
            return Equals(station.Transponder, m_Transponder);
        }
    }
}
