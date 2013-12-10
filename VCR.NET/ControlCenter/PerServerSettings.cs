using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;


namespace VCRControlCenter
{
    public class PerServerSettings
    {
        public string ServerName { get; set; }

        public ushort ServerPort { get; set; }

        public int RefreshInterval { get; set; }

        public string WakeUpBroadcast { get; set; }

        public class PerServerView : ListViewItem
        {
            public readonly PerServerSettings Settings;

            private ServerInfo m_Info = new ServerInfo();

            private DateTime m_LastProcessed = DateTime.MinValue;
            private bool m_Processing = false;

            public PerServerView( PerServerSettings settings )
            {
                // Remember
                Settings = settings;

                // Create view
                Refresh();
            }

            public void Refresh()
            {
                // Forward
                m_Info.Refresh();

                // Clear sub-items
                SubItems.Clear();

                // Load
                Text = Settings.ServerName;
                SubItems.Add( Settings.ServerPort.ToString() );
                SubItems.Add( Settings.RefreshInterval.ToString() );
                SubItems.Add( Settings.SubNetAddress.ToString() );
            }

            /// <summary>
            /// Meldet die Liste aller Geräte.
            /// </summary>
            public IEnumerable<ProfileInfo> Profiles { get { return (IEnumerable<ProfileInfo>) m_Info; } }

            /// <summary>
            /// Meldet den aktuellen Zustand des Dienstes.
            /// </summary>
            public TrayColors State { get { return m_Info.State; } }

            /// <summary>
            /// Meldet oder ändert die Zustandsverwaltung.
            /// </summary>
            public ServerInfo ServerInfo { get { return m_Info.Clone(); } set { m_Info = value; } }

            /// <summary>
            /// Meldet, ob der zugehörige VCR.NET mindestens die angegebene Version hat.
            /// </summary>
            /// <param name="major">Primäre Versionsnummer.</param>
            /// <param name="minor">Sekundäre Versionsnummer.</param>
            /// <returns>Gesetzt, wenn die Versionsnummer ermittelt werden konnte und mindestens
            /// den gewünschten Wert besitzt.</returns>
            public bool IsVersionOrLater( int major, int minor )
            {
                // Forward
                return m_Info.IsVersionOrLater( major, minor );
            }

            public DateTime LastProcessed
            {
                get
                {
                    // Report
                    return m_LastProcessed;
                }
            }

            public bool IsProcessing
            {
                get
                {
                    // Report
                    return m_Processing;
                }
            }

            public void StartProcessing()
            {
                // Update
                m_Processing = true;
            }

            public void ResetProcessing()
            {
                // Update
                if (!m_Processing) m_LastProcessed = DateTime.MinValue;
            }

            public void EndProcessing()
            {
                // Update
                m_LastProcessed = DateTime.UtcNow;
                m_Processing = false;
            }
        }

        [NonSerialized]
        private PerServerView m_View = null;

        [NonSerialized]
        private bool? m_Local = null;

        public PerServerView View
        {
            get
            {
                // Create once
                if (m_View == null)
                    m_View = new PerServerView( this );

                // Report
                return m_View;
            }
        }

        /// <summary>
        /// Meldet das Subnetz des Rechners.
        /// </summary>
        public IPAddress SubNetAddress
        {
            get
            {
                // Try to convert
                IPAddress address;
                if (!string.IsNullOrEmpty( WakeUpBroadcast ))
                    if (IPAddress.TryParse( WakeUpBroadcast, out address ))
                        return address;

                // Fallback
                return IPAddress.Broadcast;
            }
        }

        /// <summary>
        /// Meldet, ob es sich bei dem hier konfigurierten Rechner um das lokale System handelt.
        /// </summary>
        public bool IsLocal
        {
            get
            {
                // Report if already checked
                if (m_Local.HasValue)
                    return m_Local.Value;

                // Check mode
                try
                {
                    // Load entrys
                    var selfEntry = Dns.GetHostEntry( Dns.GetHostName() );
                    var serverEntry = Dns.GetHostEntry( ServerName );

                    // Process
                    m_Local = StringComparer.InvariantCultureIgnoreCase.Equals( serverEntry.HostName, selfEntry.HostName );
                }
                catch
                {
                    // No
                    m_Local = false;
                }

                // Report
                return m_Local.Value;
            }
        }

        /// <summary>
        /// Die Adresse des REST Web Dienstes.
        /// </summary>
        public string EndPoint { get { return string.Format( "http://{0}:{1}/VCR.NET", ServerName, ServerPort ); } }
    }
}
