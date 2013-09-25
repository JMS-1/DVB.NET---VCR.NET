using System;
using JMS.DVB;
using System.Text;
using System.Collections;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Beschreibt die Kennung eines erkannten Dienstes.
    /// </summary>
    [Serializable]
    public class ServiceIdentifier : Identifier
    {
        /// <summary>
        /// Die wirkliche Kennung.
        /// </summary>
        public Identifier RealIdentifier;

        /// <summary>
        /// Erzeugt eine neue Kennung.
        /// </summary>
        /// <remarks>Dieser Konstruktor wird für die Serialisierung benötigt.</remarks>
        public ServiceIdentifier()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Kennung.
        /// </summary>
        /// <param name="networkID">Die Netzwerkkennung.</param>
        /// <param name="transportID">Die Kennung des Transport Streams.</param>
        /// <param name="serviceID">Die eindeutige Nummer des Programms.</param>
        /// <param name="id">Die tatsächliche Kennung.</param>
        public ServiceIdentifier( ushort networkID, ushort transportID, ushort serviceID, Identifier id )
            : base( networkID, transportID, serviceID )
        {
            // Remember
            RealIdentifier = id;
        }
    }

    /// <summary>
    /// This class is attached to the EPG and locates all service channels
    /// related to a given channel.
    /// </summary>
    public class ServiceParser
    {
        /// <summary>
        /// Allow other clients to post process the EPG section.
        /// </summary>
        public Parser.SectionFoundHandler PostProcessor = null;

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
        private Hashtable m_ServiceNames = new Hashtable();

        /// <summary>
        /// Helper instance for EPG parsing.
        /// </summary>
        private Parser m_EPGParser = null;

        /// <summary>
        /// Related hardware device.
        /// </summary>
        private IDeviceProvider DVBDevice;

        /// <summary>
        /// Synchronize access to currently shown station.
        /// </summary>
        private object m_SyncStation = new object();

        /// <summary>
        /// The currently show station.
        /// </summary>
        private Identifier Portal;

        /// <summary>
        /// Create a new instance and start EPG parsing on PID <i>0x12</i>.
        /// </summary>
        /// <param name="device">Related hardware device.</param>
        /// <param name="portal">The currently show station.</param>
        public ServiceParser( IDeviceProvider device, Identifier portal )
        {
            // Remember
            DVBDevice = device;
            Portal = portal;

            // Create EPG parser
            m_EPGParser = new Parser( DVBDevice );

            // Attach handler
            m_EPGParser.SectionFound += AddSection;
        }

        /// <summary>
        /// Create a new instance and start EPG parsing on PID <i>0x12</i>.
        /// </summary>
        /// <param name="portal">The currently show station.</param>
        public ServiceParser( Identifier portal )
        {
            // Remember
            Portal = portal;

            // Create EPG parser
            m_EPGParser = new Parser( null );

            // Attach handler
            m_EPGParser.SectionFound += AddSection;
        }

        /// <summary>
        /// Get the current station.
        /// </summary>
        private Identifier CurrentPortal
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
        /// <param name="station">The new station to focus upon.</param>
        public void ChangeStation( Station station )
        {
            // Forward
            ChangeStation( (Identifier) station );
        }

        /// <summary>
        /// Change the active station.
        /// </summary>
        /// <remarks>
        /// This allows the client to keep the EPG filter installed
        /// when changing stations.
        /// </remarks>
        /// <param name="station">The new station to focus upon.</param>
        public void ChangeStation( Identifier station )
        {
            // Update
            lock (m_SyncStation)
                Portal = station;

            // Clear
            lock (m_ServiceNames)
                m_ServiceNames.Clear();
        }

        /// <summary>
        /// Parse some EPG information and try to extract the data of the current
        /// service group.
        /// </summary>
        /// <param name="section">Currently parsed SI table.</param>
        public void AddSection( Section section )
        {
            // Test
            Tables.EIT eit = section.Table as Tables.EIT;

            // Not us
            if (null != eit)
                foreach (EventEntry evt in eit.Entries)
                {
                    // What to add
                    ArrayList ids = new ArrayList(), names = new ArrayList();

                    // Make sure that this is us
                    bool found = false;

                    // Run over
                    foreach (Descriptor descr in evt.Descriptors)
                    {
                        // Check type
                        Descriptors.Linkage info = descr as Descriptors.Linkage;
                        if (null == info) 
                            continue;

                        // Check type (PREMIERE)
                        if (176 != info.LinkType) 
                            continue;

                        // Create identifier
                        Identifier id = new Identifier( info.OriginalNetworkIdentifier, info.TransportStreamIdentifier, info.ServiceIdentifier );

                        // Try to locate the related station
                        Station real = DVBDevice.FindStation( id );
                        if (null == real)
                        {
                            // Could be service channel
                            id = new ServiceIdentifier( info.ServiceIdentifier, 0xffff, info.ServiceIdentifier, id );
                            real = DVBDevice.FindStation( id );

                            // Try again
                            if (null == real) 
                                continue;
                        }

                        // Check the first one
                        if (!found) 
                            found = Equals( id, CurrentPortal );

                        // Remember
                        names.Add( string.Format( "{0},{1}", names.Count, CodePage.GetString( info.PrivateData ) ) );
                        ids.Add( id );
                    }

                    // Register
                    if (found)
                        lock (m_ServiceNames)
                            for (int i = ids.Count; i-- > 0; )
                                m_ServiceNames[ids[i]] = names[i];
                }

            // Load post processor
            Parser.SectionFoundHandler postProcessor = PostProcessor;

            // Forward
            if (null != postProcessor) 
                postProcessor( section );
        }

        /// <summary>
        /// Report the list of services found in the current group.
        /// </summary>
        public Hashtable ServiceMap
        {
            get
            {
                // Create new
                Hashtable map = new Hashtable();

                // Synchronize
                lock (m_ServiceNames)
                    foreach (DictionaryEntry ent in m_ServiceNames)
                        map[ent.Key] = ent.Value;

                // Report
                return map;
            }
        }
    }
}
