using System;
using System.Text;
using System.Collections.Generic;

using JMS.DVB.SI;


namespace JMS.DVB
{
    /// <summary>
    /// This class is attached to the EPG and locates all service channels
    /// related to a given channel.
    /// </summary>
    public class ServiceParser
    {
        /// <summary>
        /// Currently link descriptors use Windows encoding.
        /// </summary>
        /// <remarks>
        /// At least the german PayTV station PREMIERE.
        /// </remarks>
        private static Encoding CodePage = Encoding.GetEncoding( 1252 );

        /// <summary>
        /// All services found related to the current station.
        /// </summary>
        private Dictionary<Station, string> m_ServiceNames = new Dictionary<Station, string>();

        /// <summary>
        /// Synchronize access to currently shown station.
        /// </summary>
        private object m_SyncStation = new object();

        /// <summary>
        /// The currently show station.
        /// </summary>
        private SourceIdentifier Portal;

        /// <summary>
        /// Das zu verwendende DVB.NET Geräteprofil.
        /// </summary>
        private Profile Profile;

        /// <summary>
        /// Create a new instance and start EPG parsing on PID <i>0x12</i>.
        /// </summary>
        /// <param name="profile">Related hardware device.</param>
        /// <param name="portal">The currently show station.</param>
        public ServiceParser( Profile profile, SourceIdentifier portal )
        {
            // Remember
            Profile = profile;
            Portal = portal;

            // Register
            HardwareManager.OpenHardware( Profile ).AddProgramGuideConsumer( TableFound );
        }

        /// <summary>
        /// Stellt den Datenempfang ein.
        /// </summary>
        public void Disable()
        {
            // Unregister
            HardwareManager.OpenHardware( Profile ).RemoveProgramGuideConsumer( TableFound );
        }

        /// <summary>
        /// Get the current station.
        /// </summary>
        private SourceIdentifier CurrentPortal
        {
            get
            {
                // Report
                lock (m_SyncStation)
                    return Portal;
            }
        }

        /// <summary>
        /// Change the active station.
        /// </summary>
        /// <remarks>
        /// This allows the client to keep the EPG filter installed
        /// when changing stations.
        /// </remarks>
        /// <param name="source">The new station to focus upon.</param>
        public void ChangeStation( SourceIdentifier source )
        {
            // Update
            lock (m_SyncStation)
                Portal = source;

            // Clear
            lock (m_ServiceNames)
                m_ServiceNames.Clear();
        }

        /// <summary>
        /// Parse some EPG information and try to extract the data of the current
        /// service group.
        /// </summary>
        /// <param name="table">Currently parsed SI table.</param>
        public void TableFound( EIT table )
        {
            // Test
            var eit = table.Table;

            // Not us
            if (null != eit)
                foreach (var evt in eit.Entries)
                    if (evt.Status == EPG.EventStatus.Running)
                    {
                        // What to add
                        var ids = new List<Station>();
                        var names = new List<string>();

                        // Make sure that this is us
                        bool found = false;

                        // Run over
                        foreach (var descr in evt.Descriptors)
                        {
                            // Check type
                            var info = descr as EPG.Descriptors.Linkage;
                            if (null == info)
                                continue;

                            // Check type (PREMIERE)
                            if (176 != info.LinkType)
                                continue;

                            // Create identifier
                            var id = new SourceIdentifier { Network = info.OriginalNetworkIdentifier, TransportStream = info.TransportStreamIdentifier, Service = info.ServiceIdentifier };

                            // Lookup in profile
                            SourceSelection[] sources = Profile.FindSource( id );
                            if (sources.Length < 1)
                                continue;

                            // Check the first one to see if this is us
                            if (!found)
                                found = Equals( id, CurrentPortal );

                            // Remember
                            names.Add( string.Format( "{0},{1}", names.Count, CodePage.GetString( info.PrivateData ) ) );
                            ids.Add( (Station) sources[0].Source );
                        }

                        // Register
                        if (found)
                            lock (m_ServiceNames)
                                for (int i = ids.Count; i-- > 0; )
                                    m_ServiceNames[ids[i]] = names[i];
                    }
        }

        /// <summary>
        /// Report the list of services found in the current group.
        /// </summary>
        public Dictionary<Station, string> ServiceMap
        {
            get
            {
                // Synchronize and clone
                lock (m_ServiceNames)
                    return new Dictionary<Station, string>( m_ServiceNames );
            }
        }
    }
}
