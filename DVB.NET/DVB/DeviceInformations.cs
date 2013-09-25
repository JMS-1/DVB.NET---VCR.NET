using System;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using System.Threading;
using System.Collections;
using System.Windows.Forms;

namespace JMS.DVB
{
    public class DeviceInformations : IEnumerable
    {
        private const string PathKey = "Path";

        public readonly Hashtable Settings;

        private XmlDocument m_File = new XmlDocument();
        private Hashtable m_Devices = new Hashtable();

        public DeviceInformations()
            : this( new Hashtable() )
        {
        }

        public DeviceInformations( Hashtable settings )
        {
            // Remember
            Settings = settings;

            // Get the root
            string root = RunTimeLoader.RootDirectory.FullName;

            // Check for override
            if (Settings.ContainsKey( PathKey ))
            {
                // Merge
                root = Path.Combine( root, (string) Settings[PathKey] );

                // Remove
                Settings.Remove( PathKey );
            }

            // Attach to the provider configuration file
            FileInfo path = new FileInfo( Path.Combine( root, "DVBNETProviders.xml" ) );

            // Process
            if (path.Exists)
            {
                // Load the DOM from file
                m_File.Load( path.FullName );
            }
            else
            {
                // Me
                Type me = GetType();

                // Load the DOM from resource
                using (Stream providers = me.Assembly.GetManifestResourceStream( me.Namespace + ".DVBNETProviders.xml" ))
                {
                    // Load
                    m_File.Load( providers );
                }
            }

            // Verify
            if (!m_File.DocumentElement.Name.Equals( "DVBNETProviders" ))
                throw new ArgumentException( "bad provider definition", "file" );
            if (!Equals( m_File.DocumentElement.GetAttribute( "SchemaVersion" ), "3.9" ))
                throw new ArgumentException( "invalid schema version", "file" );

            // All providers
            foreach (XmlElement provider in m_File.DocumentElement.SelectNodes( "DVBNETProvider" ))
            {
                // Create new
                DeviceInformation info = new DeviceInformation( provider );

                // Register
                m_Devices[info.UniqueIdentifier] = info;
            }
        }

        public int Count
        {
            get
            {
                // Forward
                return m_Devices.Count;
            }
        }

        public IEnumerable DeviceNames
        {
            get
            {
                // Report
                return m_Devices.Keys;
            }
        }

        public DeviceInformation this[string uniqueIdentifier]
        {
            get
            {
                // Load
                DeviceInformation device = (DeviceInformation) m_Devices[uniqueIdentifier];

                // Validate
                if (null == device) throw new ArgumentException( "no device " + uniqueIdentifier, "uniqueIdentifier" );

                // Report
                return device;
            }
        }

        public IDeviceProvider Create( DeviceInformation provider )
        {
            // Forward
            return provider.Create( Settings );
        }

        public IDeviceProvider Create( DeviceInformation provider, Hashtable parameterOverwrites )
        {
            // Clone hashtable
            Hashtable settings = new Hashtable( Settings );

            // Merge
            foreach (DictionaryEntry parameter in parameterOverwrites)
            {
                // Merge in
                settings[parameter.Key] = parameter.Value;
            }

            // Forward
            return provider.Create( settings );
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            // Forward
            return m_Devices.Values.GetEnumerator();
        }

        #endregion
    }
}
