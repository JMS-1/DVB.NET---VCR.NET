using System;
using System.Collections;
using System.Collections.Generic;

namespace VCRControlCenter
{
    /// <summary>
    /// Beschreibt eine Verbindung zu einem VCR.NET Recording Service.
    /// </summary>
    public class ServerInfo : IEnumerable<ProfileInfo>
    {
        /// <summary>
        /// Alle Geräteprofile dieses Servers.
        /// </summary>
        private Dictionary<string, ProfileInfo> m_Profiles = new Dictionary<string, ProfileInfo>();

        /// <summary>
        /// Der aktuelle Zustand des Servers.
        /// </summary>
        private TrayColors m_State = TrayColors.Standard;

        /// <summary>
        /// Die Versionsbezeichnung des Servers.
        /// </summary>
        private string m_Version = null;

        /// <summary>
        /// Gesetzt, wenn S3 statt S4 als Schlafzustand verwendet werden soll.
        /// </summary>
        private bool m_S3Hibernate = false;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public ServerInfo()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="state">Der aktuelle Zustand des Servers.</param>
        /// <param name="profiles">Die Liste der unterstützten DVB.NET Geräteprofile.</param>
        /// <param name="version">Die versionsbezeichnung.</param>
        /// <param name="useS3">Gesetzt, wenn für den Schlafzustand S3 und nicht S4 verwendet werden soll.</param>
        private ServerInfo( TrayColors state, Dictionary<string, ProfileInfo> profiles, string version, bool useS3 )
        {
            // Remember
            m_S3Hibernate = useS3;
            m_Version = version;
            m_State = state;

            // Create synchronized copy
            lock (profiles)
                foreach (var current in profiles)
                    m_Profiles[current.Key] = current.Value.Clone( this );
        }

        /// <summary>
        /// Fordert eine Aktualisierung der Geräteprofile an.
        /// </summary>
        public void Refresh()
        {
            // Remove all profile information
            m_Profiles.Clear();
        }

        public ServerInfo Clone()
        {
            // Create new
            return new ServerInfo( TrayColors.Red, m_Profiles, m_Version, m_S3Hibernate );
        }

        public TrayColors State
        {
            get
            {
                // Report
                return m_State;
            }
            set
            {
                // Update
                m_State = value;
            }
        }

        /// <summary>
        /// Liest oder verändert die Versionsnummer des Servers.
        /// </summary>
        public string Version
        {
            get
            {
                // Report
                return m_Version;
            }
            set
            {
                // Update
                m_Version = value;
            }
        }

        /// <summary>
        /// Meldet, ob der zugehörige VCR.NET mindestens die angegebene Version hat.
        /// </summary>
        /// <param name="major">Primäre Versionsnummer.</param>
        /// <param name="minor">Sekundäre Versionsnummer.</param>
        /// <returns>Gesetzt, wenn die Versionsnummer ermittelt werden konnte und mindestens
        /// den gewünschten Wert besitzt.</returns>
        public bool IsVersionOrLater( int major, int minor )
        {
            // Don't know anything
            if (string.IsNullOrEmpty( m_Version )) return false;

            // Split
            string[] parts = m_Version.Split( '.', ' ' );

            // Check consistency
            if (parts.Length < 2) return false;

            // Get parts
            int left, right;
            if (!int.TryParse( parts[0], out left )) return false;
            if (!int.TryParse( parts[1], out right )) return false;

            // Check consistency
            if ((left < 1) || (right < 0)) return false;

            // Check version
            return ((left > major) || ((left == major) && (right >= minor)));
        }

        /// <summary>
        /// Meldet die Informationen zu einem bestimmten Geräteprofil.
        /// </summary>
        /// <param name="profile">Der Name des Geräteprofils.</param>
        /// <returns>Das gewünschte Geräteprofil.</returns>
        public ProfileInfo this[string profile]
        {
            get
            {
                // Load
                ProfileInfo result;
                if (!m_Profiles.TryGetValue( profile, out result ))
                {
                    // Create new
                    result = new ProfileInfo( profile, this );

                    // Remember
                    m_Profiles[profile] = result;
                }

                // Report
                return result;
            }
        }

        #region IEnumerable<ProfileInfo> Members

        IEnumerator<ProfileInfo> IEnumerable<ProfileInfo>.GetEnumerator()
        {
            // Report
            return m_Profiles.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            // Report
            return m_Profiles.Values.GetEnumerator();
        }

        #endregion
    }
}
