using System;
using System.IO;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.ChannelManagement.ConfigurationFile
{
    public class ConfigurationCollection
    {
        /// <summary>
        /// Der Name der primären Konfigurationsdatei für den Sendersuchlauf.
        /// </summary>
        public const string ConfigFileName = "ScanConfig.ini";

        private static Dictionary<string, Type> m_TokenMap = new Dictionary<string, Type>();

        public readonly List<TerrestrialSection> DVBTReceivers = new List<TerrestrialSection>();
        public readonly List<SatelliteSection> DVBSReceivers = new List<SatelliteSection>();
        public readonly List<CableSection> DVBCReceivers = new List<CableSection>();

        static ConfigurationCollection()
        {
            // Load types
            m_TokenMap["[TERTYPE]"] = typeof(TerrestrialSection);
            m_TokenMap["[SATTYPE]"] = typeof(SatelliteSection);
            m_TokenMap["[SCAN]"] = typeof(TerrestrialSection);
            m_TokenMap["[CABTYPE]"] = typeof(CableSection);
        }

        public ConfigurationCollection()
        {
            // Attach to current application
            FileInfo appPath = new FileInfo(Application.ExecutablePath);

            // Check for given name
            string configRelPath = ConfigurationManager.AppSettings["DVBNETScanConfig"];

            // Use default
            if (string.IsNullOrEmpty(configRelPath))
                if (string.IsNullOrEmpty(ReceiverConfiguration.ScanConfiguration))
                    configRelPath = @"Receivers\" + ConfigFileName;
                else
                    configRelPath = ReceiverConfiguration.ScanConfiguration;

            // Attach to file
            FileInfo config = new FileInfo(Path.Combine(appPath.DirectoryName, configRelPath));

            // Attach to the configuration directory
            DirectoryInfo configDir = config.Directory;

            // Check mode
            if (config.Exists)
            {
                // From file
                Load(config.FullName);
            }
            else
            {
                // Self
                Type me = GetType();

                // Get the namespace
                string ns = me.Namespace;

                // Parent namespace
                ns = ns.Substring(0, ns.LastIndexOf('.'));

                // Load from ressource
                using (Stream file = me.Assembly.GetManifestResourceStream(ns + "." + ConfigFileName))
                {
                    // From stream
                    Load(file);
                }
            }

            // Merge all other configurations files
            if (configDir.Exists)
                foreach (FileInfo configFile in configDir.GetFiles("*.ini"))
                    if (0 != string.Compare(configFile.Name, ConfigFileName, true))
                        Load(configFile.FullName);
        }

        public ConfigurationCollection(string path)
        {
            // Can forward as is
            Load(path);
        }

        public ConfigurationCollection(Stream stream)
        {
            // Can forward as is
            Load(stream);
        }

        private void Load(string path)
        {
            // Open the file
            using (StreamReader reader = new StreamReader(path, Encoding.GetEncoding(1252)))
            {
                // Forward
                Load(reader);
            }
        }

        private void Load(Stream stream)
        {
            // Open the file
            using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(1252)))
            {
                // Forward
                Load(reader);
            }
        }

        private void Load(StreamReader reader)
        {
            // Next token
            string token = null;

            // Skip until first section
            do
            {
                // Read
                token = reader.ReadLine();

                // Done
                if (null == token) return;
            }
            while (!token.StartsWith("["));

            // Process as long as possible
            while (null != token)
            {
                // Token to type
                Section section = (Section)Activator.CreateInstance(m_TokenMap[token]);

                // Configure
                section.SetStartToken(token);

                // Process
                token = section.Load(reader, this);
            }
        }
    }
}
