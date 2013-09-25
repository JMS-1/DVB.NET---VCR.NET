using System;
using JMS.DVB;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.ChannelManagement
{
    [
        Serializable
    ]
    public class ProfileParameter
    {
        private string m_Name;

        [XmlAttribute( "id" )]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        private string m_Value;

        [XmlText]
        public string Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public ProfileParameter()
            : this( null, null )
        {
        }

        public ProfileParameter( string name )
            : this( name, null )
        {
        }

        public ProfileParameter( string name, string value )
        {
            // Remember
            m_Name = name;
            m_Value = value;
        }
    }

    [
        Serializable,
        XmlRoot( "DVBNETProfile" )
    ]
    public class DeviceProfile : IDeviceProfile
    {
        public const string ProfileExtension = ".ddp";

        private const string ProfileNamespace = "http://jochen-manns.de/DVB.NET/Profiles";

        [XmlIgnore]
        private FileInfo m_File = null;

        private string m_Device;

        public string Device
        {
            get { return m_Device; }
            set { m_Device = value; }
        }

        [XmlElement( "Parameter" )]
        public readonly List<ProfileParameter> Parameters = new List<ProfileParameter>();

        private DeviceProfileReference m_SharedProfile;

        public DeviceProfileReference ShareChannelFile
        {
            get { return m_SharedProfile; }
            set { m_SharedProfile = value; }
        }

        private ReceiverConfiguration m_Channels;

        public ReceiverConfiguration Channels
        {
            get { return m_Channels; }
            set { m_Channels = value; }
        }

        private ReceiverConfiguration m_Template;

        public ReceiverConfiguration ScanTemplate
        {
            get { return m_Template; }
            set { m_Template = value; }
        }

        private string m_DisplayFormat = null;

        private Postprocessing.PostProcessingInfo m_Scripts = new Postprocessing.PostProcessingInfo();

        public Postprocessing.PostProcessingInfo Scripts
        {
            get { return m_Scripts; }
            set { m_Scripts = value; }
        }

        public DeviceProfile()
        {
        }

        public void Save()
        {
            // Save self
            Save( m_File.FullName );
        }

        public void Save( FileInfo file )
        {
            // Process
            Save( file.FullName );

            // Remember
            m_File = file;
        }

        public void Save( Stream stream )
        {
            // Create configuration
            XmlWriterSettings settings = new XmlWriterSettings();

            // Fill configuration
            settings.Encoding = Encoding.Unicode;
            settings.Indent = true;

            // Create serializer
            XmlSerializer serializer = new XmlSerializer( GetType(), ProfileNamespace );

            // Process
            using (XmlWriter writer = XmlWriter.Create( stream, settings ))
            {
                // Store
                serializer.Serialize( writer, this );
            }
        }

        public void Save( string path, FileMode mode )
        {
            // Open and forward
            using (FileStream stream = new FileStream( path, mode, FileAccess.Write, FileShare.None )) Save( stream );
        }

        public void Save( string path )
        {
            // Forward
            Save( path, FileMode.Create );
        }

        public void Delete()
        {
            // Process
            m_File.Delete();

            // Forget
            m_File = null;
        }

        public static DeviceProfile Load( Stream stream )
        {
            // Create serializer
            XmlSerializer serializer = new XmlSerializer( typeof( DeviceProfile ), ProfileNamespace );

            // Process
            return (DeviceProfile) serializer.Deserialize( stream );
        }

        public static DeviceProfile Load( string path )
        {
            // Open and forward
            using (FileStream stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read )) return Load( stream );
        }

        public static DeviceProfile Load( FileInfo file )
        {
            // Forward
            DeviceProfile profile = Load( file.FullName );

            // Remember root
            profile.m_File = file;

            // Report
            return profile;
        }

        private static DirectoryInfo CreatePath( Environment.SpecialFolder folder )
        {
            // Create
            return new DirectoryInfo( Path.Combine( Environment.GetFolderPath( folder ), "DVBNETProfiles" ) );
        }

        public static DirectoryInfo UserProfilePath
        {
            get
            {
                // Forward
                return CreatePath( Environment.SpecialFolder.LocalApplicationData );
            }
        }

        public static DirectoryInfo SystemProfilePath
        {
            get
            {
                // Forward
                return CreatePath( Environment.SpecialFolder.CommonApplicationData );
            }
        }

        private static DeviceProfile[] GetProfiles( DirectoryInfo profileDir )
        {
            // Result
            List<DeviceProfile> profiles = new List<DeviceProfile>();

            // Process all
            if (profileDir.Exists)
                foreach (FileInfo file in profileDir.GetFiles())
                    if (0 == string.Compare( file.Extension, ProfileExtension, true ))
                        try
                        {
                            // Load
                            profiles.Add( Load( file ) );
                        }
                        catch
                        {
                            // Ignore any error
                        }

            // Report
            return profiles.ToArray();
        }

        public static DeviceProfile[] UserProfiles
        {
            get
            {
                // Forward
                return GetProfiles( UserProfilePath );
            }
        }

        public static DeviceProfile[] SystemProfiles
        {
            get
            {
                // Forward
                return GetProfiles( SystemProfilePath );
            }
        }

        public override string ToString()
        {
            // Report
            return string.IsNullOrEmpty( m_DisplayFormat ) ? Name : string.Format( m_DisplayFormat, Name );
        }

        public void SetDisplayFormat( string displayFormat )
        {
            // Remember
            m_DisplayFormat = displayFormat;
        }

        [XmlIgnore]
        public string Name
        {
            get
            {
                // Report
                return (null == m_File) ? null : Path.GetFileNameWithoutExtension( m_File.FullName );
            }
        }

        private ProfileParameter FindParameter( string name )
        {
            // Lookup
            foreach (ProfileParameter parameter in Parameters)
                if (parameter.Name.Equals( name ))
                    return parameter;

            // None
            return null;
        }

        [XmlIgnore]
        public string this[string name]
        {
            get
            {
                // Load
                ProfileParameter parameter = FindParameter( name );

                // Report
                return (null == parameter) ? null : parameter.Value;
            }
            set
            {
                // Load
                ProfileParameter parameter = FindParameter( name );

                // Create
                if (null == parameter)
                {
                    // Create new
                    parameter = new ProfileParameter( name );

                    // Remember
                    Parameters.Add( parameter );
                }

                // Store
                parameter.Value = value;
            }
        }

        [XmlIgnore]
        public bool IsSystemProfile
        {
            get
            {
                // Report
                return (null != m_File) && (0 == string.Compare( m_File.Directory.FullName, SystemProfilePath.FullName, true ));
            }
        }

        [XmlIgnore]
        public DeviceProfileReference SelfReference
        {
            get
            {
                // Report
                return new DeviceProfileReference( Name, IsSystemProfile );
            }
        }

        public IDeviceProvider Create( DeviceInformations devices )
        {
            // Create parameter collection
            Hashtable parameters = new Hashtable();

            // Fill it
            foreach (ProfileParameter parameter in Parameters)
            {
                // Store
                parameters[parameter.Name] = parameter.Value;
            }

            // Forward
            IDeviceProvider device = devices.Create( devices[m_Device], parameters );

            // Attach us
            device.Profile = this;

            // Done
            return device;
        }

        [XmlIgnore]
        public ReceiverConfiguration RealChannels
        {
            get
            {
                // We have it
                if (null == m_SharedProfile) return m_Channels;

                // Be aware of cylic references
                Dictionary<DeviceProfileReference, bool> processed = new Dictionary<DeviceProfileReference, bool>();

                // Process
                for (DeviceProfileReference profile = m_SharedProfile; ; )
                {
                    // Check against cycles
                    if (processed.ContainsKey( profile )) break;

                    // Set lock
                    processed[profile] = true;

                    // Resolve
                    DeviceProfile next = profile.Profile;

                    // Next
                    profile = next.m_SharedProfile;

                    // We are done
                    if (null == profile) return next.m_Channels;
                }

                // Not found
                return null;
            }
        }
    }
}
