extern alias oldVersion;

using System;
using System.Xml;

using legacy = oldVersion.JMS.DVB;


namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Hilfsklasse zur Nutzung der Geräteprofile vor DVB.NET 3.9.
    /// </summary>
    public static class ProfileTools
    {
        /// <summary>
        /// Enthält die Beschreibung zu allen bekannten Geräten der alten DVB.NET Version.
        /// </summary>
        internal static readonly legacy.DeviceInformations LegacyDevices = new legacy.DeviceInformations();

        /// <summary>
        /// Liest alle alten Geräteprofile und wandelt sie in die neue Form um. Dabei
        /// erfolgt eine automatische Anmeldung im <see cref="ProfileManager"/>.
        /// </summary>
        /// <remarks>
        /// Es erfolgt keine Speicherung der Profile. Sie werden alle als 
        /// <see cref="Profile.VolatileName"/> gekennzeichnet.
        /// </remarks>
        public static void ConvertAll()
        {
            // Load all profiles
            foreach (var profile in ChannelManagement.DeviceProfile.SystemProfiles)
            {
                // Convert the profile
                Profile newProfile = Convert( profile );

                // Add to profile manager
                if (null == ProfileManager.FindProfile( newProfile.Name ))
                    ProfileManager.AddVolatileProfile( newProfile );
            }
        }

        /// <summary>
        /// Wandelt ein altes Geräteprofil in ein neues um.
        /// </summary>
        /// <param name="profile">Das Geräteprofil in der alten Darstellung.</param>
        /// <returns>Das Geräteprofil in der neuen Darstellung.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein altes Profil angegeben.</exception>
        /// <exception cref="ArgumentException">Der DVB Typ des Profils konnte nicht ermittelt werden.</exception>
        public static Profile Convert( ChannelManagement.DeviceProfile profile )
        {
            // Valdiate
            if (null == profile)
                throw new ArgumentNullException( "profile" );

            // Profile to use
            Profile newProfile;

            // Check type
            switch (profile.RealChannels.FrontendType)
            {
                case legacy.FrontendType.Terrestrial: newProfile = ConvertTerrestrial( profile ); newProfile.HardwareType = "JMS.DVB.Provider.Legacy.DVBTLegacy, JMS.DVB.Provider.Legacy"; break;
                case legacy.FrontendType.Satellite: newProfile = ConvertSatellite( profile ); newProfile.HardwareType = "JMS.DVB.Provider.Legacy.DVBSLegacy, JMS.DVB.Provider.Legacy"; break;
                case legacy.FrontendType.Cable: newProfile = ConvertCable( profile ); newProfile.HardwareType = "JMS.DVB.Provider.Legacy.DVBCLegacy, JMS.DVB.Provider.Legacy"; break;
                default: throw new ArgumentException( profile.Channels.FrontendType.ToString(), "profile" );
            }

            // Fill in common data
            newProfile.VolatileName = profile.Name;

            // Attach to the indicated device
            var device = LegacyDevices[profile.Device];

            // Lookup device
            newProfile.DeviceAspects.Add( new DeviceAspect { Value = device.DriverType } );

            // Load default parameters
            foreach (XmlNode parameter in device.Parameters)
                if (!string.IsNullOrEmpty( parameter.InnerText ))
                {
                    // Create the new parameter
                    ProfileParameter newParameter = new ProfileParameter { Name = parameter.Name, Value = parameter.InnerText };

                    // Add to list
                    newProfile.Parameters.Add( newParameter );
                }

            // Process all parameters set
            foreach (var parameter in profile.Parameters)
            {
                // Find parameter
                var newParameter = newProfile.Parameters.Find( p => p.Name.Equals( parameter.Name ) );

                // Create the new parameter
                if (null == newParameter)
                    newProfile.Parameters.Add( newParameter = new ProfileParameter { Name = parameter.Name } );

                // Set the value
                newParameter.Value = parameter.Value;
            }

            // Report
            return newProfile;
        }

        /// <summary>
        /// Wandelt ein altes DVB-S Geräteprofil in die neue Repräsenation um.
        /// </summary>
        /// <param name="profile">Das alte Geräteprofil.</param>
        /// <returns>Die zugehörige neue Repräsentation.</returns>
        private static SatelliteProfile ConvertSatellite( ChannelManagement.DeviceProfile profile )
        {
            // Create the brand new profile
            SatelliteProfile newProfile = new SatelliteProfile();

            // See if this is a refernce only profile
            if (profile.ShareChannelFile != null)
            {
                // Just set the reference
                newProfile.UseSourcesFrom = profile.ShareChannelFile.ProfileName;

                // Done
                return newProfile;
            }

            // Attach to configuration
            var channels = profile.Channels;
            if (null == channels)
                return newProfile;

            // Start to convert all locations
            foreach (var diseqc in channels.Receivers)
            {
                // Convert to location
                SatelliteLocation location = diseqc.LNB.FromLegacy();

                // Remember
                newProfile.GroupLocations.Add( location );
            }

            // Scan for all sources
            if (null != channels.Channels)
                foreach (var transponder in channels.Channels.Transponders)
                {
                    // Check the type
                    var channel = transponder as legacy.Satellite.SatelliteChannel;
                    if (null == channel)
                        continue;

                    // Create group location
                    SatelliteGroup newGroup = channel.FromLegacy();
                    if (null == newGroup)
                        continue;

                    // Remember
                    if ((channel.LNBIndex >= 0) && (channel.LNBIndex < newProfile.GroupLocations.Count))
                        newProfile.GroupLocations[channel.LNBIndex].SourceGroups.Add( newGroup );

                    // Fill in all sources
                    AddStations( newGroup, channel );
                }

            // Wipe out all empty groups
            foreach (SatelliteLocation location in newProfile.GroupLocations)
                location.SourceGroups.RemoveAll( g => g.Sources.Count < 1 );

            // Wipe out all locations without sources
            newProfile.GroupLocations.RemoveAll( l => l.SourceGroups.Count < 1 );

            // Process the scan templates
            if (null != profile.ScanTemplate)
                if (null != profile.ScanTemplate.Receivers)
                    foreach (var template in profile.ScanTemplate.Receivers)
                    {
                        // Create the new template
                        ScanTemplate<SatelliteLocation> newTemplate = new ScanTemplate<SatelliteLocation> { Location = template.LNB.FromLegacy() };

                        // Validate
                        if (null == newTemplate.Location)
                            continue;

                        // Search all
                        if (null != template.TargetNames)
                            foreach (string satName in template.TargetNames)
                            {
                                // Slow lookup - remember, one time conversion only
                                SatelliteScanLocation scanLocation = SatelliteScanLocation.Find( l => Equals( l.DisplayName, satName ) );

                                // Remember
                                if (null != scanLocation)
                                    newTemplate.ScanLocations.Add( scanLocation.UniqueName );
                            }

                        // Add it
                        if (newTemplate.ScanLocations.Count > 0)
                            newProfile.TypedScanLocations.Add( newTemplate );
                    }

            // Report
            return newProfile;
        }

        /// <summary>
        /// Wandelt ein altes DVB-C Geräteprofil in die neue Repräsenation um.
        /// </summary>
        /// <param name="profile">Das alte Geräteprofil.</param>
        /// <returns>Die zugehörige neue Repräsentation.</returns>
        private static CableProfile ConvertCable( ChannelManagement.DeviceProfile profile )
        {
            // Create the brand new profile
            CableProfile newProfile = new CableProfile();

            // Add the dummy location
            newProfile.GroupLocations.Add( new CableLocation() );

            // See if this is a refernce only profile
            if (profile.ShareChannelFile != null)
            {
                // Just set the reference
                newProfile.UseSourcesFrom = profile.ShareChannelFile.ProfileName;

                // Done
                return newProfile;
            }

            // Attach to configuration
            var channels = profile.Channels;
            if (null == channels)
                return newProfile;

            // Attach to the location
            CableLocation location = (CableLocation) newProfile.Locations[0];

            // Scan for all sources
            if (null != channels.Channels)
                foreach (var transponder in channels.Channels.Transponders)
                {
                    // Check the type
                    var channel = transponder as legacy.Cable.CableChannel;
                    if (null == channel)
                        continue;

                    // Create group location
                    CableGroup newGroup = channel.FromLegacy();
                    if (null == newGroup)
                        continue;

                    // Remember
                    location.Groups.Add( newGroup );

                    // Fill in all sources
                    AddStations( newGroup, channel );
                }

            // Wipe out all empty groups
            location.SourceGroups.RemoveAll( g => g.Sources.Count < 1 );

            // Create the scan template
            ScanTemplate<CableLocation> newTemplate = new ScanTemplate<CableLocation> { Location = new CableLocation() };

            // Process the scan templates
            if (null != profile.ScanTemplate)
                if (null != profile.ScanTemplate.Receivers)
                    foreach (var template in profile.ScanTemplate.Receivers)
                    {
                        // Search all
                        if (null != template.TargetNames)
                            foreach (string terName in template.TargetNames)
                            {
                                // Slow lookup - remember, one time conversion only
                                CableScanLocation scanLocation = CableScanLocation.Find( l => Equals( l.DisplayName, terName ) );

                                // Remember
                                if (null != scanLocation)
                                    newTemplate.ScanLocations.Add( scanLocation.UniqueName );
                            }
                    }

            // Add it
            if (newTemplate.ScanLocations.Count > 0)
                newProfile.TypedScanLocations.Add( newTemplate );

            // Report
            return newProfile;
        }

        /// <summary>
        /// Überträgt alle Sender als Quellen und korrigiert die eindeutigen Kennungen der Dienste.
        /// </summary>
        /// <param name="group">Eine Quellgruppe (Transponder).</param>
        /// <param name="transponder">Die alte Repräsentation als Transponder.</param>
        private static void AddStations( SourceGroup group, legacy.Transponder transponder )
        {
            // Process all stations
            foreach (var station in transponder.Stations)
            {
                // Convert
                Station newStation = station.FromLegacy();
                if (null == newStation)
                    continue;

                // Remember
                group.Sources.Add( newStation );
            }

            // Find the first non service
            Station nonService = (Station) group.Sources.Find( g => !((Station) g).IsService );

            // Update all services
            if (null == nonService)
                return;

            // Update
            group.Sources.ForEach( g =>
            {
                // Only for services
                if (!((Station) g).IsService)
                    return;

                // Use same parameters as non-service source in same group
                g.Network = nonService.Network;
                g.TransportStream = nonService.TransportStream;
            } );
        }

        /// <summary>
        /// Wandelt ein altes DVB-T Geräteprofil in die neue Repräsenation um.
        /// </summary>
        /// <param name="profile">Das alte Geräteprofil.</param>
        /// <returns>Die zugehörige neue Repräsentation.</returns>
        private static TerrestrialProfile ConvertTerrestrial( ChannelManagement.DeviceProfile profile )
        {
            // Create the brand new profile
            TerrestrialProfile newProfile = new TerrestrialProfile();

            // Add the dummy location
            newProfile.GroupLocations.Add( new TerrestrialLocation() );

            // See if this is a refernce only profile
            if (profile.ShareChannelFile != null)
            {
                // Just set the reference
                newProfile.UseSourcesFrom = profile.ShareChannelFile.ProfileName;

                // Done
                return newProfile;
            }

            // Attach to configuration
            var channels = profile.Channels;
            if (null == channels)
                return newProfile;

            // Attach to the location
            TerrestrialLocation location = (TerrestrialLocation) newProfile.Locations[0];

            // Scan for all sources
            if (null != channels.Channels)
                foreach (var transponder in channels.Channels.Transponders)
                {
                    // Check the type
                    var channel = transponder as legacy.Terrestrial.TerrestrialChannel;
                    if (null == channel)
                        continue;

                    // Create group location
                    TerrestrialGroup newGroup = channel.FromLegacy();
                    if (null == newGroup)
                        continue;

                    // Remember
                    location.Groups.Add( newGroup );

                    // Fill in all sources
                    AddStations( newGroup, channel );
                }

            // Wipe out all empty groups
            location.SourceGroups.RemoveAll( g => g.Sources.Count < 1 );

            // Create the scan template
            ScanTemplate<TerrestrialLocation> newTemplate = new ScanTemplate<TerrestrialLocation>() { Location = new TerrestrialLocation() };

            // Process the scan templates
            if (null != profile.ScanTemplate)
                if (null != profile.ScanTemplate.Receivers)
                    foreach (var template in profile.ScanTemplate.Receivers)
                    {
                        // Search all
                        if (null != template.TargetNames)
                            foreach (string terName in template.TargetNames)
                            {
                                // Slow lookup - remember, one time conversion only
                                TerrestrialScanLocation scanLocation = TerrestrialScanLocation.Find( l => Equals( l.DisplayName, terName ) );

                                // Remember
                                if (null != scanLocation)
                                    newTemplate.ScanLocations.Add( scanLocation.UniqueName );
                            }
                    }

            // Add it
            if (newTemplate.ScanLocations.Count > 0)
                newProfile.TypedScanLocations.Add( newTemplate );

            // Report
            return newProfile;
        }
    }
}
