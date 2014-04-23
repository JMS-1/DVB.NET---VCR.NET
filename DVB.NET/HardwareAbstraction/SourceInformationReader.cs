using System;
using System.Linq;
using System.Threading;
using JMS.DVB.SI;


namespace JMS.DVB
{
    /// <summary>
    /// Verwaltet die Anforderung für die Aktualisierung der Daten einer Quelle.
    /// </summary>
    public static class SourceInformationReader
    {
        /// <summary>
        /// Beginnt mit dem Auslesen der Quelldaten.
        /// </summary>
        /// <param name="source">Alle Informationen zur Quelle.</param>
        /// <returns>Die Hintergrundaufgabe zum Auslesen der Quelldaten.</returns>
        public static CancellableTask<SourceInformation> GetSourceInformationAsync( this SourceSelection source )
        {
            return source.GetHardware().GetSourceInformationAsync( source.Source, source.GetProfile() );
        }

        /// <summary>
        /// Beginnt mit dem Auslesen der Quelldaten.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <param name="hardware">Das zu verwendende Gerät.</param>
        /// <param name="profile">Optional das zu berücksichtigende Geräteprofil.</param>
        /// <returns>Die Hintergrundaufgabe zum Auslesen der Quelledaten.</returns>
        public static CancellableTask<SourceInformation> GetSourceInformationAsync( this Hardware hardware, SourceIdentifier source, Profile profile = null )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );
            if (hardware == null)
                throw new ArgumentNullException( "hardware" );

            // Start 
            return CancellableTask<SourceInformation>.Run( cancel =>
            {
                // Find the stream identifier for the service
                var pmtIdentifier = hardware.GetServicePMT( source, cancel );
                if (!pmtIdentifier.HasValue)
                    return null;

                // Wait for mapping table
                var pmtReader = hardware.GetTableAsync<PMT>( pmtIdentifier.Value );
                if (!pmtReader.CancellableWait( cancel ))
                    return null;

                // Request table
                var pmts = pmtReader.Result;
                if (pmts == null)
                    return null;

                // Create dummy
                var currentSettings = new SourceInformation { Source = source, VideoType = VideoTypes.NoVideo };

                // Process all PMT - actually should be only one
                foreach (var pmt in pmts)
                {
                    // Overwrite encryption if CA descriptor is present
                    if (pmt.Table.Descriptors != null)
                        currentSettings.IsEncrypted = pmt.Table.Descriptors.Any( descriptor => EPG.DescriptorTags.CA == descriptor.Tag );

                    // Process the program entries
                    foreach (var program in pmt.Table.ProgramEntries)
                        currentSettings.Update( program );
                }

                // Attach to the related group information
                var groupReader = hardware.GroupReader;
                if (groupReader != null)
                {
                    if (!groupReader.CancellableWait( cancel ))
                        return null;

                    // Now check the result - may want to skip
                    var groupInfo = groupReader.Result;
                    if (groupInfo != null)
                    {
                        // Find the related station information
                        var station = groupInfo.Sources.FirstOrDefault( source.Equals ) as Station;
                        if (station != null)
                        {
                            // Take data from there
                            currentSettings.Provider = station.Provider;
                            currentSettings.Name = station.Name;

                            // See if this is a service
                            currentSettings.IsService = station.IsService;

                            // Overwrite encryption if regular service entry exists
                            if (!currentSettings.IsService)
                                currentSettings.IsEncrypted = station.IsEncrypted;
                        }
                    }
                }

                // See if profile is attached
                if (profile != null)
                {
                    // Apply the modifier
                    var modifier = profile.GetFilter( currentSettings.Source );
                    if (modifier != null)
                        modifier.ApplyTo( currentSettings );
                }

                // Report
                return currentSettings;
            } );
        }
    }
}
