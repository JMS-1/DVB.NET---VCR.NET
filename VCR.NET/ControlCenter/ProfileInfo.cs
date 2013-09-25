using System;
using System.Text;
using System.Collections.Generic;

namespace VCRControlCenter
{
    /// <summary>
    /// Enth�lt die Daten zu einem einzelnen DVB.NET Ger�teprofil.
    /// </summary>
    public class ProfileInfo
    {
        /// <summary>
        /// Der Name des Ger�teprofils.
        /// </summary>
        public readonly string Profile;

        /// <summary>
        /// Informationen zur aktuellen Aufzeichnung.
        /// </summary>
        private VCRNETRestProxy.Current[] m_Current = null;

        /// <summary>
        /// Informationen zum zugeh�rigen VCR.NET Recording Service.
        /// </summary>
        public readonly ServerInfo ServerInfo;

        /// <summary>
        /// Erzeugt eine neuen Informationsinstanz zu einem DVB.NET Ger�teprofil.
        /// </summary>
        /// <param name="profile">Der Name des Profils.</param>
        /// <param name="server">Der zugeh�rige VCR.NET Recording Service.</param>
        public ProfileInfo( string profile, ServerInfo server )
        {
            // Remeber
            ServerInfo = server;
            Profile = profile;
        }

        /// <summary>
        /// Liest oder setzt die aktuelle Aufzeichnung. Beim Setzen einer aktuellen
        /// Aufzeichnung wird der <see cref="ServerInfo.State"/> automatisch
        /// auf <see cref="TrayColors">Blue</see> gesetzt.
        /// </summary>
        public VCRNETRestProxy.Current[] CurrentRecordings
        {
            get
            {
                // Report
                return m_Current;
            }
            set
            {
                // Remember
                m_Current = value;

                // Report
                ServerInfo.State = TrayColors.Blue;
            }
        }

        /// <summary>
        /// Erzeugt eine Kopie dieser Profilinformationen - dabei wird
        /// automatisch die <see cref="CurrentRecording"/> in 
        /// <see cref="PreviousRecording"/> �bertragen und gel�scht.
        /// </summary>
        /// <param name="server">Der zugeh�rige VCR.NET Recording Service.</param>
        /// <returns>Eine Kopie der Informationen zu diesem DVB.NET Ger�teprofil.</returns>
        public ProfileInfo Clone( ServerInfo server )
        {
            // Create new
            return new ProfileInfo( Profile, server );
        }
    }
}
