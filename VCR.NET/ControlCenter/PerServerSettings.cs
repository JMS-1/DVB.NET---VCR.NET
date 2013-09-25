using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;


namespace VCRControlCenter
{
    public class PerServerSettings
    {
        private string m_Server;

        public string ServerName
        {
            get { return m_Server; }
            set { m_Server = value; }
        }

        private ushort m_Port;

        public ushort ServerPort
        {
            get { return m_Port; }
            set { m_Port = value; }
        }

        private int m_Interval;

        public int RefreshInterval
        {
            get { return m_Interval; }
            set { m_Interval = value; }
        }

        private bool m_Extensions = true;

        public bool RunExtensions
        {
            get { return m_Extensions; }
            set { m_Extensions = value; }
        }

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
                SubItems.Add( Settings.RunExtensions ? Properties.Resources.Yes : Properties.Resources.No );
            }

            public IEnumerable<ProfileInfo> Profiles
            {
                get
                {
                    // Report
                    return (IEnumerable<ProfileInfo>) m_Info;
                }
            }

            public ServerInfo ServerInfo
            {
                get
                {
                    // Report a clone
                    return m_Info.Clone();
                }
                set
                {
                    // Use the new one
                    m_Info = value;
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

        public PerServerSettings()
        {
        }

        public PerServerView View
        {
            get
            {
                // Create once
                if (null == m_View) m_View = new PerServerView( this );

                // Report
                return m_View;
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
                    var serverEntry = Dns.GetHostEntry( m_Server );

                    // Process
                    m_Local = (0 == string.Compare( serverEntry.HostName, selfEntry.HostName, true ));
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
