using System;
using JMS.DVB;
using System.IO;
using System.Xml;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.ChannelManagement
{
    [
        Serializable
    ]
    public class DeviceProfileReference
    {
        private bool m_IsSystem;

        public bool IsSystemProfile
        {
            get { return m_IsSystem; }
            set { m_IsSystem = value; }
        }

        private string m_Profile;

        public string ProfileName
        {
            get { return m_Profile; }
            set { m_Profile = value; }
        }

        public DeviceProfileReference()
        {
        }

        public DeviceProfileReference( string profileName, bool isSystem )
        {
            // Remember
            m_Profile = profileName;
            m_IsSystem = isSystem;
        }

        public override int GetHashCode()
        {
            // Create
            return m_IsSystem.GetHashCode() ^ ((null == m_Profile) ? 0 : m_Profile.GetHashCode());
        }

        public override bool Equals( object obj )
        {
            // Convert
            DeviceProfileReference other = obj as DeviceProfileReference;

            // Not possible
            if (null == other) return false;

            // Compare
            return (m_IsSystem == other.m_IsSystem) && (0 == string.Compare( m_Profile, other.m_Profile, true ));
        }

        [XmlIgnore]
        public DeviceProfile Profile
        {
            get
            {
                // None
                if (string.IsNullOrEmpty( m_Profile )) return null;

                // Get the directory
                DirectoryInfo profileDir = m_IsSystem ? DeviceProfile.SystemProfilePath : DeviceProfile.UserProfilePath;

                // Get the file
                FileInfo profileFile = new FileInfo( Path.Combine( profileDir.FullName, m_Profile + DeviceProfile.ProfileExtension ) );

                // None found
                if (!profileFile.Exists) return null;

                // Load
                return DeviceProfile.Load( profileFile );
            }
        }

        public IDeviceProvider Create( DeviceInformations devices )
        {
            // Forward
            return Profile.Create( devices );
        }
    }
}
