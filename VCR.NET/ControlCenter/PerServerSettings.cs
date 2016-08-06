using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;


namespace VCRControlCenter
{
    public class PerServerSettings
    {
        /// <summary>
        /// Ermittelt die Netzwerkinformationen.
        /// </summary>
        /// <param name="table">Ein vorbereiteter Speicherbereich.</param>
        /// <param name="size">Die Größe des Speicherbereiches.</param>
        /// <param name="order">Gesetzt, wenn die Einträge sortiert werden sollen.</param>
        /// <returns>Ein Fehlercode.</returns>
        [DllImport( "Iphlpapi.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern UInt32 GetIpNetTable( IntPtr table, ref UInt32 size, bool order );

        /// <summary>
        /// Informationen über eine Adresse.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct IpNetRow
        {
            /// <summary>
            /// Die Größe in Bytes.
            /// </summary>
            public static readonly int SizeOf = Marshal.SizeOf( typeof( IpNetRow ) );

            /// <summary>
            /// Die laufende Nummer des Eintrags.
            /// </summary>
            private UInt32 m_index;

            /// <summary>
            /// Die Anzahl der Adressbytes.
            /// </summary>
            private UInt32 m_physicalAddressBytes;

            /// <summary>
            /// Die physikalische Adresse.
            /// </summary>
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 8 )]
            private byte[] m_physicalAddress;

            /// <summary>
            /// Meldet die physikalische Adresse.
            /// </summary>
            public byte[] MAC { get { return m_physicalAddress.Take( checked((int)m_physicalAddressBytes) ).ToArray(); } }

            /// <summary>
            /// Die Netzwerkadresse.
            /// </summary>
            private UInt32 m_ip4Address;

            /// <summary>
            /// Die Art der Adresse.
            /// </summary>
            private UInt32 m_type;

            /// <summary>
            /// Prüft, ob dieser Eintrag zu einer Adresse passt.
            /// </summary>
            /// <param name="addresses">Eine Liste von Adressen.</param>
            /// <returns>Gesetzt, wenn mindestens eine davon passt.</returns>
            public bool Matches( IEnumerable<IPAddress> addresses )
            {
                // None
                if (addresses == null)
                    return false;
                else
                    return addresses.Any( new IPAddress( m_ip4Address ).Equals );
            }
        }

        public string ServerName { get; set; }

        public ushort ServerPort { get; set; }

        public int RefreshInterval { get; set; }

        public string WakeUpBroadcast { get; set; }

        public byte[] MAC { get; set; }

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
            public IEnumerable<ProfileInfo> Profiles { get { return (IEnumerable<ProfileInfo>)m_Info; } }

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
        /// Ermittelt die MAC des Rechners, sofern dies nicht nich geschehen ist.
        /// </summary>
        public void DetectMAC()
        {
            // Already did it
            if (MAC != null)
                return;
            if (IsLocal)
                return;

            // Ignore any error
            try
            {
                // Resolve server
                var server = Dns.GetHostEntry( ServerName );
                if (server == null)
                    return;

                // Get size of address table
                UInt32 size = 0;
                if (GetIpNetTable( IntPtr.Zero, ref size, true ) != 122)
                    return;
                if (size < 4)
                    return;

                // Load address table
                var table = Marshal.AllocHGlobal( checked((int)size) );
                try
                {
                    // Get the table
                    if (GetIpNetTable( table, ref size, true ) != 0)
                        return;

                    // Get the number of entries in the table
                    var count = checked((UInt32)Marshal.ReadInt32( table, 0 ));

                    // Process entries
                    for (int offset = 4; count-- > 0; offset += IpNetRow.SizeOf)
                    {
                        // Unmarshal from native representation
                        var data = (IpNetRow)Marshal.PtrToStructure( table + offset, typeof( IpNetRow ) );

                        // See if this is it
                        if (data.Matches( server.AddressList ))
                        {
                            // Remember
                            MAC = data.MAC;

                            // Write back
                            Properties.Settings.Default.Save();

                            // Done
                            return;
                        }
                    }
                }
                finally
                {
                    // Cleanup
                    Marshal.FreeHGlobal( table );
                }
            }
            catch (Exception)
            {
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
        public string EndPoint => $"http://{ServerName}:{ServerPort}/VCR.NET";
    }
}
