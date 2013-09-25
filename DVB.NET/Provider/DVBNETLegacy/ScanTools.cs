extern alias oldVersion;

using System;

using legacy = oldVersion.JMS.DVB;


namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Hilfsmethoden zum Umwandeln der DVB.NET Sendersuchlaufinformationen vor Version 3.9
    /// in einen aktuellen Stand.
    /// </summary>
    public static class ScanTools
    {        
        /// <summary>
        /// Wandelt sämliche Sendersuchlaufinformationen um.
        /// </summary>
        public static ScanLocations ConvertAll()
        {
            // Create new
            ScanLocations locations = new ScanLocations();

            // Load the old configurations
            ChannelManagement.ConfigurationFile.ConfigurationCollection list = new ChannelManagement.ConfigurationFile.ConfigurationCollection();

            // Process all DVB-S entries
            foreach (ChannelManagement.ConfigurationFile.SatelliteSection section in list.DVBSReceivers)
            {
                // Convert
                SatelliteScanLocation dvbs = section.FromLegacy();

                // Remember
                if (null != dvbs)
                    locations.Locations.Add( dvbs );
            }

            // Process all DVB-C entries
            foreach (ChannelManagement.ConfigurationFile.CableSection section in list.DVBCReceivers)
            {
                // Convert
                CableScanLocation dvbc = section.FromLegacy();

                // Remember
                if (null != dvbc)
                    locations.Locations.Add( dvbc );
            }

            // Process all DVB-T entries
            foreach (ChannelManagement.ConfigurationFile.TerrestrialSection section in list.DVBTReceivers)
            {
                // Convert
                TerrestrialScanLocation dvbt = section.FromLegacy();

                // Remember
                if (null != dvbt)
                    locations.Locations.Add( dvbt );
            }

            // Report
            return locations;
        }

        /// <summary>
        /// Wandelt eine DVB-S Sendersuchlaufbeschreibung in die neue Notation um.
        /// </summary>
        /// <param name="section">Die alte Notation der Beschreibung.</param>
        /// <returns>Die neue Notation der Beschreibung.</returns>
        public static SatelliteScanLocation FromLegacy( this ChannelManagement.ConfigurationFile.SatelliteSection section )
        {
            // Read the key
            string pos = section.SymbolicName;
            if ((null == pos) || (4 != pos.Length))
                return null;

            // Try to parse to number
            ushort uPos;
            if (!ushort.TryParse( pos, out uPos ))
                return null;

            // Check range
            if ((uPos < 0) || (uPos >= 3600))
                return null;

            // Get position mode
            bool west = (uPos > 1800);

            // Correct
            if (west)
                uPos = (ushort) (3600 - uPos);

            // Recreate
            string orbitalPosition = uPos.ToString( "0000" );

            // Create location
            SatelliteScanLocation location = new SatelliteScanLocation { DisplayName = section.DisplayName, AutoConvert = true };

            // Process all transponders
            foreach (string transponderKey in section.Transponders)
                if (!string.IsNullOrEmpty( transponderKey ))
                {
                    // Try to recreate
                    var transponder = legacy.Satellite.SatelliteChannel.Create( transponderKey, 0 );
                    if (null == transponder)
                        continue;

                    // Convert
                    SatelliteGroup group = transponder.FromLegacy();
                    if (null == group)
                        continue;

                    // Finish
                    group.OrbitalPosition = orbitalPosition;
                    group.IsWestPosition = west;

                    // Remember
                    location.SourceGroups.Add( group );
                }

            // Report
            if (location.SourceGroups.Count < 1)
                return null;
            else
                return location;
        }

        /// <summary>
        /// Wandelt eine DVB-C Sendersuchlaufbeschreibung in die neue Notation um.
        /// </summary>
        /// <param name="section">Die alte Notation der Beschreibung.</param>
        /// <returns>Die neue Notation der Beschreibung.</returns>
        public static CableScanLocation FromLegacy( this ChannelManagement.ConfigurationFile.CableSection section )
        {
            // Create location
            CableScanLocation location = new CableScanLocation { DisplayName = section.SymbolicName, AutoConvert = true };

            // Process all frequencies
            foreach (string settings in section.Transponders)
                foreach (string rate in section.SymbolRates)
                {
                    // Split
                    string[] rateinfo = rate.Split( ',' );

                    // Check it
                    if (3 != rateinfo.Length)
                        continue;
                    if (!Equals( rateinfo[2], "1" ))
                        continue;

                    // Try to read symbol rate
                    uint symrate;
                    if (!uint.TryParse( rateinfo[0].Replace( ".", string.Empty ), out symrate ))
                        continue;

                    // Try to read QAM
                    int rawQAM;
                    if (!int.TryParse( rateinfo[1], out rawQAM ) || (rawQAM < 0))
                        continue;

                    // Split
                    string[] transinfo = settings.Split( ',' );

                    // Check it
                    if (3 != transinfo.Length)
                        continue;

                    // Try to read frequency
                    uint freq;
                    if (!uint.TryParse( transinfo[1].Replace( ".", string.Empty ), out freq ))
                        continue;

                    // Try to read bandwidth
                    int rawBand;
                    if (!int.TryParse( transinfo[2], out rawBand ) || (rawBand < 0))
                        continue;

                    // Create transponder
                    var transponder = new legacy.Cable.CableChannel( freq, legacy.SpectrumInversion.Auto, symrate, (legacy.Cable.Qam) rawQAM, (legacy.BandwidthType) rawBand );

                    // Remap
                    CableGroup group = transponder.FromLegacy();
                    if (null == group)
                        return null;

                    // Remember
                    location.SourceGroups.Add( group );
                }

            // Report
            if (location.SourceGroups.Count < 1)
                return null;
            else
                return location;
        }

        /// <summary>
        /// Wandelt eine DVB-T Sendersuchlaufbeschreibung in die neue Notation um.
        /// </summary>
        /// <param name="section">Die alte Notation der Beschreibung.</param>
        /// <returns>Die neue Notation der Beschreibung.</returns>
        public static TerrestrialScanLocation FromLegacy( this ChannelManagement.ConfigurationFile.TerrestrialSection section )
        {
            // Create location
            TerrestrialScanLocation location = new TerrestrialScanLocation { DisplayName = section.DisplayName, AutoConvert = true };

            // Process all frequencies
            foreach (string settings in section.Transponders)
            {
                // Split
                string[] transinfo = settings.Split( ',' );

                // Check it
                if (5 != transinfo.Length)
                    continue;

                // Try to read frequency
                uint freq;
                if (!uint.TryParse( transinfo[1].Replace( ".", string.Empty ), out freq ))
                    continue;

                // Try to read inversion
                uint inversion;
                if (!uint.TryParse( transinfo[2], out inversion ))
                    continue;

                // Try to read bandwidth
                int rawBand;
                if (!int.TryParse( transinfo[4], out rawBand ) || (rawBand < 0))
                    continue;

                // Create transponder
                var transponder = new legacy.Terrestrial.TerrestrialChannel( freq, (1 == inversion) ? legacy.SpectrumInversion.On : legacy.SpectrumInversion.Off, false, (legacy.BandwidthType) rawBand );

                // Remap
                TerrestrialGroup group = transponder.FromLegacy();
                if (null == group)
                    return null;

                // Remember
                location.SourceGroups.Add( group );
            }

            // Report
            if (location.SourceGroups.Count < 1)
                return null;
            else
                return location;
        }
    }
}
