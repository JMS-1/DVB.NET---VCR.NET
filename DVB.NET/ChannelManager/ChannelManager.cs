using System;
using JMS.DVB;
using System.IO;
using System.Xml;
using JMS.DVB.EPG;
using System.Text;
using JMS.DVB.Satellite;
using System.Reflection;
using JMS.DVB.EPG.Tables;
using System.Collections;
using System.Configuration;
using System.Xml.Serialization;
using System.Collections.Generic;
using JMS.ChannelManagement.Postprocessing;

using Log = JMS.DVB.EPG.Tools;

namespace JMS.ChannelManagement
{
    /// <summary>
    /// Hold all channels found by the channel list providers.
    /// </summary>
    [Serializable]
    [XmlType( "Channels" )]
    public class ChannelManager
    {
        /// <summary>
        /// Empty <see cref="Station"/> <see cref="Array"/>.
        /// </summary>
        private static Station[] NoStation = { };

        /// <summary>
        /// All transponders known for all stations managed.
        /// </summary>
        [XmlIgnore]
        private Dictionary<Transponder, Transponder> m_Transponders = new Dictionary<Transponder, Transponder>();

        /// <summary>
        /// All stations indexed by name.
        /// </summary>
        /// <remarks>
        /// Each entry is an <see cref="Array"/> since names may
        /// not be unique between different transponders.
        /// </remarks>
        [XmlIgnore]
        private Dictionary<string, List<Station>> m_Stations = new Dictionary<string, List<Station>>( StringComparer.InvariantCultureIgnoreCase );

        /// <summary>
        /// All stations indexed by the unique network identification.
        /// </summary>
        [XmlIgnore]
        private Hashtable m_ByNetwork = new Hashtable();

        /// <summary>
        /// The overall DISEqC configuration related with these stations.
        /// </summary>
        [XmlIgnore]
        internal DiSEqCConfiguration m_DiSEqCConfiguration = new DiSEqCConfiguration();

        /// <summary>
        /// Create a new channel manager and dynamically load it from all providers.
        /// </summary>
        /// <remarks>
        /// Provider assemblies will be put in the 
        /// directory where the calling <see cref="Assembly"/> is located. This code will try to load
        /// all files in there as an <see cref="Assembly"/>. For all <see cref="Assembly.GetExportedTypes"/>
        /// implementing <see cref="IChannelListProvider"/> an instance is created and
        /// <see cref="IChannelListProvider.Load"/> invoked.
        /// </remarks>
        public ChannelManager()
        {
            // Attach to the path of the calling assembly
            FileInfo caller = new FileInfo( Assembly.GetCallingAssembly().CodeBase.Substring( 8 ).Replace( '/', '\\' ) );

            // Attach to the provider directory
            DirectoryInfo pDir = caller.Directory;

            // Scan all
            foreach (FileInfo pFile in pDir.GetFiles())
                try
                {
                    // As assembly - may fail
                    Assembly pProv = Assembly.LoadFrom( pFile.FullName );

                    // Process all exported types
                    foreach (Type pType in pProv.GetExportedTypes())
                    {
                        // Skip
                        if (!pType.IsClass || !typeof( IChannelListProvider ).IsAssignableFrom( pType )) continue;

                        // Create the instance
                        IChannelListProvider pInstance = (IChannelListProvider) Activator.CreateInstance( pType );

                        // Run on us
                        pInstance.Load( this );
                    }
                }
                catch
                {
                    // Ignore all errors
                }

            // Be safe
            try
            {
                // Try to load the DiSEqC configuration from file
                string sConfigFile = (string) ConfigurationManager.AppSettings["DiSEqCConfiguration"];

                // The easy way
                if ((null != sConfigFile) && (sConfigFile.Length > 0))
                {
                    // Full disabled
                    if (sConfigFile.Equals( "*" )) return;

                    // Load
                    m_DiSEqCConfiguration.Load( sConfigFile );

                    // Done so far
                    return;
                }

                // Check for registry provider
                string sConfigProvider = (string) ConfigurationManager.AppSettings["DiSEqCProvider"];

                // Not set - using default provider
                if (null == sConfigProvider) return;

                // Create the instance
                IDiSEqCRegistryProvider pDiSEqCProv = (IDiSEqCRegistryProvider) Activator.CreateInstance( Type.GetType( sConfigProvider ) );

                // Process
                pDiSEqCProv.Fill( m_DiSEqCConfiguration );
            }
            catch
            {
                // Ignore all errors
            }
        }

        /// <summary>
        /// Create a new channel manager and load it from the indicated provider.
        /// </summary>
        /// <param name="provider">Channel list and DISEqC provider class.</param>
        public ChannelManager( Type provider )
        {
            // Allow load-nothing option
            if (null == provider) return;

            // Create the instance
            IChannelListProvider instance = (IChannelListProvider) Activator.CreateInstance( provider );

            // Test the instance
            IDiSEqCRegistryProvider pDiSEqCProv = instance as IDiSEqCRegistryProvider;

            // Be safe
            try
            {
                // Run on us
                instance.Load( this );

                // Process
                if (null != pDiSEqCProv) pDiSEqCProv.Fill( m_DiSEqCConfiguration );
            }
            catch
            {
                // Ignore all errors
            }
        }

        /// <summary>
        /// Serialize the channel list to a stream.
        /// </summary>
        /// <param name="stream">Target stream.</param>
        public void Save( Stream stream )
        {
            // Create serializer
            XmlSerializer serializer = new XmlSerializer( GetType(), "http://jochen-manns.de/DVB.NET/Channels" );

            // Create settings
            XmlWriterSettings settings = new XmlWriterSettings();

            // Configure settings
            settings.Encoding = Encoding.Unicode;
            settings.Indent = true;

            // Create writer and process
            using (XmlWriter writer = XmlWriter.Create( stream, settings )) serializer.Serialize( writer, this );
        }

        /// <summary>
        /// Serialize the channel list to a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="mode">Mode to open the file.</param>
        public void Save( string path, FileMode mode )
        {
            // Forward
            using (FileStream stream = new FileStream( path, mode, FileAccess.Write, FileShare.None )) Save( stream );
        }

        /// <summary>
        /// Load a channel list from a file.
        /// </summary>
        /// <param name="path">Full path to the file.</param>
        /// <param name="diseqc">The related DiSEqC configuration.</param>
        /// <returns>The reconstructed channel manager instance.</returns>
        public static ChannelManager Load( string path, DiSEqCConfiguration diseqc )
        {
            // Forward
            using (FileStream stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read )) return Load( stream, diseqc );
        }

        /// <summary>
        /// Load a channel list from a stream.
        /// </summary>
        /// <param name="stream">The stream holding the instance.</param>
        /// <param name="diseqc">The related DiSEqC configuration.</param>
        /// <returns>The reconstructed channel manager instance.</returns>
        public static ChannelManager Load( Stream stream, DiSEqCConfiguration diseqc )
        {
            // Create deserializer
            XmlSerializer deserializer = new XmlSerializer( typeof( ChannelManager ), "http://jochen-manns.de/DVB.NET/Channels" );

            // Process
            ChannelManager result = (ChannelManager) deserializer.Deserialize( stream );

            // Connect
            result.m_DiSEqCConfiguration = diseqc;

            // Report
            return result;
        }

        /// <summary>
        /// Add a new transponder to the channel management.
        /// </summary>
        /// <param name="transponder">Some transponder.</param>
        /// <returns>The parameter or the transponder instance already
        /// registered.</returns>
        public Transponder RegisterTransponder( Transponder transponder )
        {
            // Must by synchronized
            lock (this)
            {
                // Load
                Transponder known;
                if (m_Transponders.TryGetValue( transponder, out known )) return known;

                // Remember
                m_Transponders[transponder] = transponder;

                // Return new one
                return transponder;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            // Reset internal state
            m_Stations = new Dictionary<string, List<Station>>( StringComparer.InvariantCultureIgnoreCase );
            m_Transponders = new Dictionary<Transponder, Transponder>();
            m_ByNetwork = new Hashtable();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Cleanup()
        {
            // All transponders
            List<Transponder> transponders = new List<Transponder>( m_Transponders.Keys );

            // Remove all with no stations
            foreach (Transponder transponder in transponders)
                if (transponder.Stations.Length < 1)
                    m_Transponders.Remove( transponder );
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                // Report
                return m_ByNetwork.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement( typeof( JMS.DVB.Terrestrial.TerrestrialChannel ) )]
        [XmlElement( typeof( JMS.DVB.Satellite.SatelliteChannel ) )]
        [XmlElement( typeof( JMS.DVB.Cable.CableChannel ) )]
        public Transponder[] TransponderList
        {
            get
            {
                // Time to cleanup duplicate names - this method is typically only called during serialisation
                CleanupDuplicates();

                // Result
                List<Transponder> transponders = new List<Transponder>();

                // Copy over
                transponders.AddRange( m_Transponders.Keys );

                // Report
                return transponders.ToArray();
            }
            set
            {
                // Reset
                Clear();

                // Fill
                if (null == value) return;

                // Process all
                foreach (Transponder transponder in value)
                {
                    // Tranponder first
                    Transponder registered = RegisterTransponder( transponder );

                    // Duplicates are not allowed
                    if (registered != transponder) throw new InvalidDataException();

                    // All station
                    foreach (Station station in transponder.Stations)
                    {
                        // Connect
                        station.Transponder = transponder;

                        // Remember
                        AddStation( station );
                    }
                }
            }
        }

        /// <summary>
        /// Register a single station in the channel management.
        /// </summary>
        /// <remarks>
        /// If there is already some station with the same network
        /// <see cref="Identifier"/> it will be replaced.
        /// </remarks>
        /// <param name="station">The station to add.</param>
        public void AddStation( Station station )
        {
            // Use as key
            string key = station.Name;

            // Must be synchronized
            lock (this)
            {
                // Add any key only once
                if (m_ByNetwork.Contains( station )) return;

                // Add by identifier
                m_ByNetwork[station] = station;

                // Check for lookup by name
                List<Station> list;

                // Check mode
                if (!m_Stations.TryGetValue( key, out list ))
                {
                    // Create new
                    list = new List<Station>();

                    // Remember
                    m_Stations[key] = list;
                }

                // Extend
                list.Add( station );
            }
        }

        /// <summary>
        /// Report all stations in this channel.
        /// </summary>
        [XmlIgnore]
        public IEnumerable Stations
        {
            get
            {
                // Report
                return m_ByNetwork.Values;
            }
        }

        /// <summary>
        /// Report all transponders for this channel.
        /// </summary>
        [XmlIgnore]
        public IEnumerable Transponders
        {
            get
            {
                // Report
                return m_Transponders.Values;
            }
        }

        [XmlIgnore]
        public Transponder[] ActiveTransponders
        {
            get
            {
                // Result
                List<Transponder> list = new List<Transponder>();

                // Fill
                foreach (Transponder transponder in m_Transponders.Values)
                    if (transponder.Stations.Length > 0)
                        list.Add( transponder );

                // Report
                return list.ToArray();
            }
        }

        /// <summary>
        /// Lookup a station by name.
        /// </summary>
        /// <param name="stationName">The name of the station.</param>
        /// <returns>If the number of elements is not one the station is
        /// either not known at all or the name is used in different
        /// transponders.</returns>
        public Station[] Find( string stationName )
        {
            // Forward
            return Find( stationName, null );
        }

        /// <summary>
        /// Lookup a station by name relatvive to a transponder.
        /// </summary>
        /// <param name="stationName">The name of the station.</param>
        /// <param name="transponderName">Optional name of a transponder.</param>
        /// <returns>If the number of elements is not one the station is
        /// either not known at all or the name is used in different
        /// transponders.</returns>
        public Station[] Find( string stationName, string transponderName )
        {
            // Retrieve
            List<Station> stations;

            // Empty
            if (!m_Stations.TryGetValue( stationName, out stations )) return NoStation;

            // Done - we do not clone and expect wellbehaved clients
            if (string.IsNullOrEmpty( transponderName )) return stations.ToArray();

            // Result
            List<Station> ret = new List<Station>( stations );

            // Must match
            ret.RemoveAll( station => !transponderName.Equals( station.TransponderName ) );

            // Report
            return ret.ToArray();
        }

        /// <summary>
        /// Lookup a station by its unique network identification.
        /// </summary>
        /// <param name="networkKey">The unique identification.</param>
        /// <returns>Can be <i>null</i> if there is no station for the indicated
        /// identification.</returns>
        public Station Find( Identifier networkKey )
        {
            // Forward as is
            return (Station) m_ByNetwork[networkKey];
        }

        /// <summary>
        /// The related DiSEqC configuration.
        /// </summary>
        public DiSEqCConfiguration DiSEqCConfiguration
        {
            get
            {
                // Report
                return m_DiSEqCConfiguration;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        private Station RemoveStation( Station station )
        {
            // See if station exists
            Station previous = (Station) m_ByNetwork[station];

            // See if station exists
            if (null == previous) return null;

            // Remove from map
            m_ByNetwork.Remove( previous );

            // Create lookup key
            string key = previous.Name;

            // Attach to list
            List<Station> list;
            if (m_Stations.TryGetValue( key, out list ))
            {
                // Remove
                list.Remove( previous );

                // Wipe out
                if (list.Count < 1) m_Stations.Remove( key );
            }

            // Report
            return previous;
        }

        private void CleanupDuplicates()
        {
            // All clashes
            List<Station> clashes = new List<Station>(), process = new List<Station>(), update = new List<Station>();
            List<string> emptyLists = new List<string>();

            // Resolve name clashed - this method is typically only called during serialisation
            foreach (KeyValuePair<string, List<Station>> current in m_Stations)
            {
                // Attach
                List<Station> list = current.Value;

                // Skip
                if (list.Count < 2) continue;

                // Reset
                process.Clear();

                // Create a copy
                process.AddRange( list );

                // Process all
                while (process.Count > 1)
                {
                    // Load the first
                    Station left = process[0];

                    // Remove
                    process.RemoveAt( 0 );

                    // Reset
                    clashes.Clear();

                    // Extract all clashes
                    for (int i = process.Count; i-- > 0; )
                    {
                        // Attach 
                        Station right = process[i];

                        // Check for full name clash
                        if (0 == string.Compare( left.TransponderName, right.TransponderName, true ))
                        {
                            // Remove from processing list
                            process.RemoveAt( i );

                            // Remember
                            clashes.Add( right );
                        }
                    }

                    // Nothing to do
                    if (clashes.Count < 1) continue;

                    // Add primary
                    clashes.Add( left );

                    // Sort by transponder
                    clashes.Sort( ( l, r ) => l.CompareTo( r ) );

                    // Update names
                    for (int i = 0; i < clashes.Count; )
                    {
                        // Load the station
                        Station clash = clashes[i++];

                        // Update the name
                        clash.Name = string.Format( "{0} #{1}", clash.Name, i );

                        // Cleanup
                        list.Remove( clash );
                    }

                    // Remember for update
                    update.AddRange( clashes );
                }

                // Check 
                if (list.Count < 1) emptyLists.Add( current.Key );
            }

            // Remove all empty lists
            foreach (string emptyList in emptyLists) m_Stations.Remove( emptyList );

            // Update all
            foreach (Station station in update)
            {
                // Discard
                m_ByNetwork.Remove( station );

                // Re-add with new name
                AddStation( station );
            }
        }
    }
}
