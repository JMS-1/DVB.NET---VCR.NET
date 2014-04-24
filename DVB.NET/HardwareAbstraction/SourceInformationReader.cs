using System;
using System.Linq;
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
        /// <param name="device">Das zu verwendende Gerät.</param>
        /// <param name="profile">Optional das zu berücksichtigende Geräteprofil.</param>
        /// <returns>Die Hintergrundaufgabe zum Auslesen der Quelledaten.</returns>
        public static CancellableTask<SourceInformation> GetSourceInformationAsync( this Hardware device, SourceIdentifier source, Profile profile = null )
        {
            // Validate
            if (device == null)
                throw new ArgumentNullException( "no hardware to use", "device" );
            if (source == null)
                throw new ArgumentException( "no source to get information for", "source" );

            // Attach to tasks
            var patReader = device.AssociationTableReader;
            var groupReader = device.GroupReader;

            // Start 
            return
                CancellableTask<SourceInformation>.Run( cancel =>
                {
                    // Check tasks
                    if (groupReader == null)
                        return null;
                    if (patReader == null)
                        return null;

                    // Wait on tasks
                    if (!groupReader.CancellableWait( cancel ))
                        return null;
                    if (!patReader.CancellableWait( cancel ))
                        return null;

                    // Get the current group information
                    var groupInfo = groupReader.Result;
                    if (groupInfo == null)
                        return null;

                    // See if group exists
                    if (!groupInfo.Sources.Any( source.Equals ))
                        return null;

                    // Find the stream identifier for the service
                    var pmtIdentifier = patReader.Result.FindService( source.Service );
                    if (!pmtIdentifier.HasValue)
                        return null;

                    // Wait for mapping table
                    var pmtReader = device.GetTableAsync<PMT>( pmtIdentifier.Value );
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
