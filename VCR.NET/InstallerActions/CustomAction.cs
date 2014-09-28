using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using JMS.DVB;
using JMS.DVBVCR.InstallerActions;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;


namespace InstallerActions
{
    /// <summary>
    /// Enthält die Einsprungpunkte für die Installationserweiterungen.
    /// </summary>
    public class CustomActions
    {
        /// <summary>
        /// Übernimmt das dynamische Laden der DVB.NET Laufzeitumgebung.
        /// </summary>
        static CustomActions()
        {
            // Start dynamic loading
            RunTimeLoader.Startup();
        }

        /// <summary>
        /// Der Konfigurationspfad des Dienstes in der Windows Registrierung.
        /// </summary>
        private static string ConfigRoot
        {
            get
            {
                // Report
                return @"SYSTEM\CurrentControlSet\Services\VCR.NET Service";
            }
        }

        /// <summary>
        /// Der Pfad zu den Parametern des Dienstes in der Registry.
        /// </summary>
        private static string ConfigPath
        {
            get
            {
                // Construct
                return ConfigRoot + @"\Parameters";
            }
        }

        /// <summary>
        /// Aktualisiert die Konfiguration des Dienstes.
        /// </summary>
        /// <param name="session">Der aktuelle Installationsstand.</param>
        /// <returns>Das Ergebnis der Konfiguration.</returns>
        [CustomAction]
        public static ActionResult MigrateConfiguration( Session session )
        {
            // Get the root folder of the installation
            var root = new DirectoryInfo( session.CustomActionData["ROOTDIR"] );

            // Path to service
            var servicePath = Path.Combine( root.FullName, @"bin\JMS.DVBVCR.RecordingService.exe" );

            // Attach to helper files
            var newConfigurationPath = new FileInfo( servicePath + @".config" );
            var oldConfigurationPath = new FileInfo( servicePath + @".config.cpy" );

            // Temporary configuration
            FileInfo tempConfig = null;

            // Only if copy exists
            if (!oldConfigurationPath.Exists)
            {
                // Check for legacy version
                var oldLegacyPath = new FileInfo( Path.Combine( oldConfigurationPath.Directory.Parent.FullName, oldConfigurationPath.Name ) );

                // Check this
                if (!oldLegacyPath.Exists)
                {
                    // Create scratch file
                    tempConfig = new FileInfo( Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString() ) );

                    // Copy over
                    newConfigurationPath.CopyTo( tempConfig.FullName );

                    // Use
                    oldConfigurationPath = tempConfig;
                }
                else
                {
                    // Be safe
                    try
                    {
                        // Try to move in place
                        oldLegacyPath.MoveTo( oldConfigurationPath.FullName );
                    }
                    catch
                    {
                        // Ignore on any error
                    }
                }
            }

            // Cleanup
            try
            {
                // Helpers
                var newConfiguration = new XmlDocument();
                var oldConfiguration = new XmlDocument();

                // Always load the new configuration
                newConfiguration.Load( newConfigurationPath.FullName );

                // See if there is some update configuration
                try
                {
                    // Load as XML
                    oldConfiguration.Load( oldConfigurationPath.FullName );
                }
                catch
                {
                    // Ignore any error
                }

                // Result
                var src = new List<XmlElement>();
                var dst = new List<XmlElement>();

                // Flags
                bool epgEnabled = false, psiEnabled = false;

                // Compare all settings
                foreach (XmlElement setting in oldConfiguration.SelectNodes( "configuration/appSettings/add" ))
                {
                    // Special handling for some settings
                    var key = setting.GetAttribute( "key" );
                    switch (key)
                    {
                        case "ScanInterval": psiEnabled = !Equals( setting.GetAttribute( "value" ), "0" ); break;
                        case "EPGDuration": epgEnabled = !Equals( setting.GetAttribute( "value" ), "0" ); break;
                    }

                    // Locate in new configuration
                    var newSetting = (XmlElement) newConfiguration.SelectSingleNode( "configuration/appSettings/add[@key='" + key + "']" );
                    if (null == newSetting)
                        continue;

                    // Check mode
                    if (Equals( setting.GetAttribute( "value" ), newSetting.GetAttribute( "value" ) ))
                        continue;

                    // Remember
                    src.Add( setting );
                    dst.Add( newSetting );
                }

                // Copy over
                for (var ix = src.Count; ix-- > 0; )
                {
                    // Load
                    var oldSetting = src[ix];
                    var newSetting = dst[ix];

                    // Copy over
                    newSetting.SetAttribute( "value", oldSetting.GetAttribute( "value" ) );
                }

                // Read the profiles in use
                var profiles = (XmlElement) newConfiguration.SelectSingleNode( "configuration/appSettings/add[@key='Profiles']" );
                var profileNames = profiles.GetAttribute( "value" );

                // See if use should choose profile
                if (string.IsNullOrEmpty( profileNames ))
                    using (var dialog = new ProfileInstaller( newConfiguration ))
                        dialog.ShowDialog();

                // Save the configuration
                newConfiguration.Save( newConfigurationPath.FullName );

                // Process all known profiles
                if (epgEnabled || psiEnabled)
                {
                    // Installation time
                    var stamp = DateTime.UtcNow.ToString( "u" );

                    // for now mark all known profiles
                    using (var reg = Registry.LocalMachine.CreateSubKey( ConfigPath ))
                        foreach (var profile in ProfileManager.AllProfiles)
                        {
                            // Disable automatic scans and regard all current as actual
                            if (epgEnabled)
                                reg.SetValue( string.Format( "LastEPGRun {0}", profile.Name ), stamp );
                            if (psiEnabled)
                                reg.SetValue( string.Format( "LastPSIRun {0}", profile.Name ), stamp );
                        }
                }
            }
            finally
            {
                // Must cleanup temp
                if (tempConfig != null)
                    if (tempConfig.Exists)
                        tempConfig.Delete();
            }

            // Did it
            return ActionResult.Success;
        }
    }
}
