extern alias oldVersion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Xml;
using JMS.DVB;
using JMS.DVB.Provider.Legacy;
using JMS.DVBVCR.InstallerActions;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using legacy = oldVersion.JMS;


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
        /// Wird bei der Installation oder Reperatur aufgerufen.
        /// </summary>
        /// <param name="session">Der laufende Installationsvorgang.</param>
        /// <returns>Das Ergebnis der Erweiterung</returns>
        [CustomAction]
        public static ActionResult AsAdmin( Session session )
        {
            // Process configuration
            MigrateConfiguration( session );

            // Configuring the service is a bit optional so ignore any error
            try
            {
                // Now we can prepare the service
                using (var wmi = new ManagementObject( "Win32_Service.Name='VCR.NET Service'" ))
                {
                    // Start type
                    var autoStartMode = "1".Equals( session.CustomActionData["AUTOSTARTMODE"] );
                    var autoStartModeName = autoStartMode ? "Automatic" : "Manual";

                    // Change start type
                    wmi.InvokeMethod( "ChangeStartMode", new[] { autoStartModeName } );

                    // Check mode
                    var startService = "1".Equals( session.CustomActionData["STARTSERVICE"] );
                    if (startService)
                        using (var svc = new ServiceController( (string) wmi.GetPropertyValue( "Name" ) ))
                            if (svc.Status != ServiceControllerStatus.Running)
                            {
                                // Send start command
                                svc.Start();

                                // Wait a bit
                                svc.WaitForStatus( ServiceControllerStatus.Running, TimeSpan.FromSeconds( 10 ) );
                            }
                }
            }
            catch (Exception)
            { 
            }

            // Did it
            return ActionResult.Success;
        }

        /// <summary>
        /// Übernimmt die Konfiguration der vorhergehenden Installation.
        /// </summary>
        private static void MigrateConfiguration( Session session )
        {
            // Get the root folder of the installation
            var root = new DirectoryInfo( session.CustomActionData["ROOTDIR"] );

            // Get the path to the web.config
            var webConfig = new FileInfo( Path.Combine( root.FullName, "web.config" ) );
            if (webConfig.Exists)
            {
                // Create wrapper
                var config = new XmlDocument();

                // Load it
                config.Load( webConfig.FullName );

                // Get the section
                var compiler = (XmlElement) config.SelectSingleNode( "configuration/system.web/compilation" );

                // Update
                if (compiler != null)
                {
                    // Set the temporary directory
                    compiler.SetAttribute( "tempDirectory", Path.Combine( webConfig.DirectoryName, "AspNetTemp" ) );

                    // Store
                    config.Save( webConfig.FullName );
                }
            }

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
                {
                    // Ask user
                    using (var dialog = new ProfileInstaller( newConfiguration ))
                        dialog.ShowDialog();
                }
                else
                {
                    // All legacy profiles
                    var oldProfiles = legacy.ChannelManagement.DeviceProfile.SystemProfiles.ToDictionary( p => p.Name, ProfileManager.ProfileNameComparer );

                    // Run conversion
                    foreach (var profileNameRaw in profileNames.Split( '|' ))
                    {
                        // Get the name
                        var profileName = profileNameRaw.Trim();
                        if (ProfileManager.FindProfile( profileName ) != null)
                            continue;

                        // Load it
                        legacy.ChannelManagement.DeviceProfile oldProfile;
                        if (!oldProfiles.TryGetValue( profileName, out oldProfile ))
                            continue;

                        // Try to convert
                        try
                        {
                            // Process
                            var newProfile = ProfileTools.Convert( oldProfile );

                            // Finish
                            newProfile.MakePermanent();

                            // Refresh
                            ProfileManager.Refresh();
                        }
                        catch
                        {
                            // Ignore any error.
                        }
                    }
                }

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
        }
    }
}
