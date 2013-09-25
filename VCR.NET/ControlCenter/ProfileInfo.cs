using System;
using System.Text;
using System.Collections.Generic;

namespace VCRControlCenter
{
    /// <summary>
    /// Enthält die Daten zu einem einzelnen DVB.NET Geräteprofil.
    /// </summary>
    public class ProfileInfo
    {
        /// <summary>
        /// Der Name des Geräteprofils.
        /// </summary>
        public readonly string Profile;

        /// <summary>
        /// Informationen zur aktuellen Aufzeichnung.
        /// </summary>
        private VCRNETRestProxy.Current[] m_Current = null;

        /// <summary>
        /// Informationen zum zugehörigen VCR.NET Recording Service.
        /// </summary>
        public readonly ServerInfo ServerInfo;

        /// <summary>
        /// Erzeugt eine neuen Informationsinstanz zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profile">Der Name des Profils.</param>
        /// <param name="server">Der zugehörige VCR.NET Recording Service.</param>
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
        /// <see cref="PreviousRecording"/> übertragen und gelöscht.
        /// </summary>
        /// <param name="server">Der zugehörige VCR.NET Recording Service.</param>
        /// <returns>Eine Kopie der Informationen zu diesem DVB.NET Geräteprofil.</returns>
        public ProfileInfo Clone( ServerInfo server )
        {
            // Create new
            return new ProfileInfo( Profile, server );
        }
    }
}
