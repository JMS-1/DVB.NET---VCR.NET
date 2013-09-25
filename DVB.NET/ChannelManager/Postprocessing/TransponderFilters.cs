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
	/// Prüft die Art eines Transponders zu einem Sender.
	/// </summary>
	[Serializable]
	public class ByReceiverType : StationFilter
	{
		/// <summary>
		/// Die benötigte Art des Transponders.
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
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
		/// </remarks>
		public ByReceiverType()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="type">Die gewünschte Art des Transponders.</param>
		public ByReceiverType(FrontendType type)
		{
			// Remember
			m_Type = type;
		}

		/// <summary>
		/// Meldet für einen Sender, ob dieser zu einem Transponder einer
		/// bestimmten Art gehört.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>Gesetzt, wenn der zugehörige Transponder der gewünschten Art
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
		/// Die gewünschte Frequenz.
		/// </summary>
		private uint m_Frequency;

		/// <summary>
		/// Liest oder setzt die gewünschte Empfangsfrequenz.
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
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
		/// </remarks>
		public ByFrequency()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="frequency">Die gewünschte Frequenz.</param>
		public ByFrequency(uint frequency)
		{
			// Remember
			m_Frequency = frequency;
		}

		/// <summary>
		/// Meldet, ob ein Sender auf einer bestimmten Frequenz empfangen wird.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>Gesetzt, wenn der Sender auf der gewünschten Frequenz empfangen wird.</returns>
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
		/// Die gewünschte Inversion.
		/// </summary>
		private SpectrumInversion m_Inversion;

		/// <summary>
		/// Liest oder setzt die gwünschte spektrale Inversion.
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
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
		/// </remarks>
		public BySpectrumInversion()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="inversion">Die gewünschte spektrale Inversion.</param>
		public BySpectrumInversion(SpectrumInversion inversion)
		{
			// Remember
			m_Inversion = inversion;
		}

		/// <summary>
		/// Meldet, ob ein Sender mit einer bestimmten spektralen Inversion empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>Gesetzt, wenn der Sender mit der gewünschten Inversion empfangen wurde.</returns>
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
	/// Prüft für DVB-S Sender, ob diese über eine bestimmte Antenne empfnagen wurden.
	/// </summary>
	[Serializable]
	public class ByLNBIndex : StationFilter
	{
		/// <summary>
		/// 0-basierter Index der gewünschten Antenne.
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
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
		/// </remarks>
		public ByLNBIndex() 
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="index">Der gewünschte 0-basierte Index der Antenne.</param>
		public ByLNBIndex(int index)
		{
			// Remember
			m_Index = index;
		}

		/// <summary>
		/// Meldet, ob ein Sender ein DVB-S Sender ist und dieser über eine bestimmte
		/// Antenne empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>Gesetzt für DVB-S Sender, die über die gwünschte Antenne empfangen werden.</returns>
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
		/// Die gewünschte Polarisation.
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
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
		/// </remarks>
		public ByPolarisation()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instanz.
		/// </summary>
		/// <param name="mode">Die gewünschte Polarisation.</param>
		public ByPolarisation(PowerMode mode)
		{
			// Remember
			m_Mode = mode;
		}

		/// <summary>
		/// Meldet, ob ein Sender ein DVB-S Sender ist, der auf einer bestimmten
		/// Polarisation empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>Gesetzt für DVB-S Sender, die auf der gewünschten Polarisation empfangen
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
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
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
		/// Meldet, ob ein Sender über DVB-S oder DVB-S2 empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>Gesetzt für DVB-S Sender, die in der gewünschten Modulation
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
	/// Kann für DVB-C Sender nach der Modulation unterscheiden.
	/// </summary>
	[Serializable]
	public class ByDVBCModulation : StationFilter
	{
		/// <summary>
		/// Die gewünschte Modulation.
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
		/// Dieser Konstruktor wird für die Deserialisierung benötigt.
		/// </remarks>
		public ByDVBCModulation()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Instzanz.
		/// </summary>
		/// <param name="modulation">Die gewünschte Modulation.</param>
		public ByDVBCModulation(Qam modulation)
		{
			// Remember
			m_Modulation = modulation;
		}

		/// <summary>
		/// Meldet, ob ein Sender ein DVB-C Sender ist, der mit einer bestimmten
		/// Modulation empfangen wurde.
		/// </summary>
		/// <param name="station">Der zu prüfende Sender.</param>
		/// <returns>gesetzt für DVB-C Sender, die in der gewünschten Modulation
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
        /// Der gewünschte Transponder.
        /// </summary>
        private Transponder m_Transponder;

        /// <summary>
        /// Liest oder setzt den gewünschten Transponder.
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
        /// Dieser Konstruktor wird für die Deserialisierung benötigt.
        /// </remarks>
        public ByTransponder()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="transponder">Der gewünschte Transponder.</param>
        public ByTransponder(Transponder transponder)
        {
            //  Must create a clone with no stations
            m_Transponder = transponder.Clone(false);
        }

        /// <summary>
        /// Meldet, ob ein Sender zum gewünschten Transponder gehört.
        /// </summary>
        /// <param name="station">Der zu prüfende Sender.</param>
        /// <returns>Gesetzt, wenn der Sender auf dem gewünschten Transponder gefunden wurde.</returns>
        public override bool Match(Station station)
        {
            // Direct
            return Equals(station.Transponder, m_Transponder);
        }
    }
}
