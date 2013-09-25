using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace JMS.DVB
{
    /// <summary>
    /// Base class for any transponder description.
    /// </summary>
    [Serializable]
    public abstract class Transponder
    {
        /// <summary>
        /// Detailinformationen für das Anwählen dieses Transponders. Diese Informationen
        /// werden nicht in der Senderliste gespeichert.
        /// </summary>
        public class TuningExtension
        {
            /// <summary>
            /// Der eindeutige Name dieser Information.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Die erweiterten Informationen.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Erzeugt neue Detailinformationen.
            /// </summary>
            public TuningExtension()
            {
            }
        }

        /// <summary>
        /// Liest oder setzt Detailinformation für das Anwählen des Transponders.
        /// </summary>
        [XmlIgnore]
        public TuningExtension[] TuningExtensions { get; set; }

        /// <summary>
        /// All stations in this transponder.
        /// </summary>
        [XmlIgnore]
        private List<Station> m_Stations = new List<Station>();

        /// <summary>
        /// Initialize the instance.
        /// </summary>
        protected Transponder()
        {
        }

        /// <summary>
        /// Register a <see cref="Station"/> inside this transponder.
        /// </summary>
        /// <param name="station">The station to register.</param>
        public void AddStation( Station station )
        {
            // Validate
            if (null == station) throw new ArgumentNullException( "station" );

            // See if it exists
            int i = m_Stations.IndexOf( station );

            // Check mode
            if (i < 0)
            {
                // Register new
                m_Stations.Add( station );
            }
            else
            {
                // Replace existing
                m_Stations[i] = station;
            }
        }

        /// <summary>
        /// Remove a <see cref="Station"/> from this transponder.
        /// </summary>
        /// <param name="station">The station to register.</param>
        public void RemoveStation( Station station )
        {
            // Validate
            if (null == station) throw new ArgumentNullException( "station" );

            // Process
            m_Stations.Remove( station );
        }

        /// <summary>
        /// Versucht auf Basis der Sender auf diesem Transponder die originale Netzwerkkennung
        /// und die Kennung des <i>Transport Streams</i> zu ermitteln. 
        /// </summary>
        /// <returns>Die ermittelten Kennungen oder <i>null</i>, wenn dies nicht möglich war.</returns>
        public Identifier DeriveDefaultIdentifier()
        {
            // Result
            Identifier res = null;

            // Check all stations
            foreach (Identifier station in m_Stations)
                if (0xffff != station.TransportStreamIdentifier)
                {
                    // Create
                    Identifier test = new Identifier( station.NetworkIdentifier, station.TransportStreamIdentifier, 0 );

                    // Check mode
                    if (null == res)
                    {
                        // Use first as is
                        res = test;
                    }
                    else if (!res.Equals( test ))
                    {
                        // Mismatch - can not derive
                        return null;
                    }
                }

            // Report
            return res;
        }

        /// <summary>
        /// Get all stations in this transponder.
        /// </summary>
        [XmlElement( "Station" )]
        public Station[] Stations
        {
            get
            {
                // Create
                return m_Stations.ToArray();
            }
            set
            {
                // Reset
                m_Stations.Clear();

                // Fill
                if (null != value) m_Stations.AddRange( value );
            }
        }

        /// <summary>
        /// Ermittelt den Namen dieses Transponders.
        /// </summary>
        /// <param name="format">Unterstützt wird <i>0</i> für eine kurze Darstellung und
        /// <i>1</i> für eine volle Darstellung.</param>
        /// <returns>Der Name dieses Transponders.</returns>
        public abstract string ToString( string format );

        /// <summary>
        /// Ermittelt den Namen dieses Transponders.
        /// </summary>
        /// <returns>Der Name dieses Transponders.</returns>
        public override string ToString()
        {
            // Forward
            return ToString( "0" );
        }

        /// <summary>
        /// Erzeugt eine Kopie dieser Instanz. Die Implementierung ist recht langsam und sollte nur
        /// in GUI Szenarien verwendet werden.
        /// </summary>
        /// <param name="withStations">Gesetzt, wenn auch alle Sender mit kopiert werden sollen.</param>
        /// <returns>Eine Kopie dieser Instanz.</returns>
        public Transponder Clone( bool withStations )
        {
            // Helper
            List<Station> stations = m_Stations;

            // Load
            if (!withStations) m_Stations = new List<Station>();

            // Safe clone
            try
            {
                // Create helper
                using (MemoryStream stream = new MemoryStream())
                {
                    // Create serializer
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Serialize
                    formatter.Serialize( stream, this );

                    // Reset
                    stream.Seek( 0, SeekOrigin.Begin );

                    // Deserialize
                    return (Transponder) formatter.Deserialize( stream );
                }
            }
            finally
            {
                // Restore
                m_Stations = stations;
            }
        }
    }
}
