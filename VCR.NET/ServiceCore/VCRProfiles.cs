using System;
using System.Collections.Generic;
using System.Linq;
using JMS.DVB;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Verwaltet die Geräteprofile des VCR.NET Recording Service.
    /// </summary>
    public static class VCRProfiles
    {
        /// <summary>
        /// Der aktuelle Konfigurationsbestand.
        /// </summary>
        private class _State
        {
            /// <summary>
            /// Alle Geräteprofile, die der VCR.NET Recording Service verwenden darf.
            /// </summary>
            public Profile[] Profiles = new Profile[0];

            /// <summary>
            /// Die Geräteprofile zum schnellen Zugriff über den Namen.
            /// </summary>
            public Dictionary<string, Profile> ProfileMap = new Dictionary<string, Profile>( ProfileManager.ProfileNameComparer );

            /// <summary>
            /// Verwaltet alle Quellen aller zur Verfügung stehenden Geräteprofile nach dem eindeutigen
            /// Bezeichner der Quelle.
            /// </summary>
            public Dictionary<string, Dictionary<string, SourceSelection>> SourceBySelectionMap = new Dictionary<string, Dictionary<string, SourceSelection>>( StringComparer.InvariantCultureIgnoreCase );

            /// <summary>
            /// Ermittelt zu einer Quelle den aktuellen eindeutigen Namen.
            /// </summary>
            public Dictionary<string, string> UniqueNameBySelectionMap = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );

            /// <summary>
            /// Verwaltet alle Quellen aller zur Verfügung stehenden Geräteprofile nach der DVB
            /// Kennung.
            /// </summary>
            public Dictionary<string, Dictionary<SourceIdentifier, SourceSelection>> SourceByIdentifierMap = new Dictionary<string, Dictionary<SourceIdentifier, SourceSelection>>( StringComparer.InvariantCultureIgnoreCase );

            /// <summary>
            /// Erzeugt einen neuen Bestand.
            /// </summary>
            public _State()
            {
            }
        }

        /// <summary>
        /// Beschreibt die aktuell gültige Konfiguration.
        /// </summary>
        private static volatile _State CurrentState = new _State();

        /// <summary>
        /// Lädt alle Profile erneut.
        /// </summary>
        internal static void Reset()
        {
            // Report
            Tools.ExtendedLogging( "Reloading Profile List" );

            // Forward to profile manager
            ProfileManager.Refresh();

            // Create a new state
            var newState = new _State();

            // List of profiles
            var profiles = new List<Profile>();

            // Load the setting
            var profileNames = VCRConfiguration.Current.ProfileNames;
            if (!string.IsNullOrEmpty( profileNames ))
                foreach (var profileName in profileNames.Split( '|' ))
                {
                    // Try to locate
                    var profile = ProfileManager.FindProfile( profileName.Trim() );
                    if (profile == null)
                    {
                        // This is not goot
                        VCRServer.LogError( Properties.Resources.InternalError_NoProfile, profileName.Trim() );

                        // Next
                        continue;
                    }

                    // Report
                    Tools.ExtendedLogging( "Using Profile {0}", profile.Name );

                    // Remember
                    profiles.Add( profile );

                    // Create the dictionary of sources
                    var sourcesByIdentifier = new Dictionary<SourceIdentifier, SourceSelection>();
                    var sourcesByKey = new Dictionary<string, SourceSelection>();

                    // Load by name
                    foreach (var byDisplayName in profile.AllSourcesByDisplayName)
                        sourcesByKey[byDisplayName.DisplayName] = byDisplayName;

                    // Remember it
                    newState.SourceByIdentifierMap[profile.Name] = sourcesByIdentifier;
                    newState.SourceBySelectionMap[profile.Name] = sourcesByKey;

                    // Load list
                    foreach (var source in sourcesByKey.Values)
                    {
                        // Just remember by identifier
                        sourcesByIdentifier[source.Source] = source;

                        // Correct back the name
                        source.DisplayName = ((Station) source.Source).FullName;
                    }
                }

            // Fill it
            foreach (var profileMap in newState.SourceBySelectionMap.Values)
                foreach (var mapEntry in profileMap)
                    newState.UniqueNameBySelectionMap[mapEntry.Value.SelectionKey] = mapEntry.Key;

            // Add all qualified names to allow semi-legacy clients to do a unique lookup
            foreach (var profileMap in newState.SourceBySelectionMap.Values)
                foreach (var source in profileMap.ToList())
                    if (!source.Key.Equals( source.Value.DisplayName ))
                    {
                        // Unmap the station
                        var station = (Station) source.Value.Source;

                        // Enter special notation
                        profileMap[string.Format( "{0} {1} [{2}]", station.Name, station.ToStringKey(), station.Provider )] = source.Value;
                    }

            // Use all
            newState.Profiles = profiles.ToArray();
            newState.ProfileMap = profiles.ToDictionary( profile => profile.Name, newState.ProfileMap.Comparer );

            // Report
            Tools.ExtendedLogging( "Activating new Profile Set" );

            // Use the new state
            CurrentState = newState;
        }

        /// <summary>
        /// Meldet das erste zu verwendende Geräteprofil.
        /// </summary>
        public static Profile DefaultProfile
        {
            get
            {
                // Attach to array
                var profiles = CurrentState.Profiles;
                if (profiles.Length > 0)
                    return profiles[0];
                else
                    return null;
            }
        }

        /// <summary>
        /// Ermittelt eine Quelle nach ihrem Namen.
        /// </summary>
        /// <param name="profileName">Das zu verwendende Geräteprofil.</param>
        /// <param name="name">Der Anzeigename der Quelle.</param>
        /// <returns>Die eidneutige Auswahl der Quelle oder <i>null</i>.</returns>
        public static SourceSelection FindSource( string profileName, string name )
        {
            // No source
            if (string.IsNullOrEmpty( name ))
                return null;

            // No profile
            var state = CurrentState;
            if (string.IsNullOrEmpty( profileName ))
                if (state.Profiles.Length < 1)
                    return null;
                else
                    profileName = state.Profiles[0].Name;

            // Map to use
            Dictionary<string, SourceSelection> sources;
            if (!state.SourceBySelectionMap.TryGetValue( profileName, out sources ))
                return null;

            // Ask map
            SourceSelection source;
            if (!sources.TryGetValue( name, out source ))
                return null;

            // Report
            return source;
        }

        /// <summary>
        /// Ermittelt eine Quelle nach ihrer eindeutigen Kennung.
        /// </summary>
        /// <param name="profileName">Das zu verwendende Geräteprofil.</param>
        /// <param name="source">Die gewünschte Kennung.</param>
        /// <returns>Die eidneutige Auswahl der Quelle oder <i>null</i>.</returns>
        public static SourceSelection FindSource( string profileName, SourceIdentifier source )
        {
            // No source
            if (source == null)
                return null;

            // No profile
            var state = CurrentState;
            if (string.IsNullOrEmpty( profileName ))
                if (state.Profiles.Length < 1)
                    return null;
                else
                    profileName = state.Profiles[0].Name;

            // Find the map
            Dictionary<SourceIdentifier, SourceSelection> map;
            if (!state.SourceByIdentifierMap.TryGetValue( profileName, out map ))
                return null;

            // Find the source
            SourceSelection found;
            if (!map.TryGetValue( source, out found ))
                return null;
            else
                return found;
        }

        /// <summary>
        /// Ermittelt die aktuelle Konfiguration einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die aktuelle Auswahl oder <i>null</i>.</returns>
        public static SourceSelection FindSource( SourceSelection source )
        {
            // Never
            if (source == null)
                return null;
            if (source.Source == null)
                return null;

            // Find the source
            return FindSource( source.ProfileName, source.Source );
        }

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public static IEnumerable<SourceSelection> GetSources( string profileName )
        {
            // Forward
            return GetSources( profileName, (Func<SourceSelection, bool>) null );
        }

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profile">Der Name des Geräteprofils.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public static IEnumerable<SourceSelection> GetSources( Profile profile )
        {
            // Forward
            return GetSources( profile, (Func<SourceSelection, bool>) null );
        }

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="predicate">Methode, die prüft, ob eine Quelle gemeldet werden soll.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public static IEnumerable<SourceSelection> GetSources( string profileName, Func<SourceSelection, bool> predicate )
        {
            // Forward
            return GetSources( FindProfile( profileName ), predicate );
        }

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="predicate">Methode, die prüft, ob eine Quelle gemeldet werden soll.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public static IEnumerable<SourceSelection> GetSources( string profileName, Func<Station, bool> predicate )
        {
            // Forward
            if (null == predicate)
                return GetSources( FindProfile( profileName ) );
            else
                return GetSources( FindProfile( profileName ), s => predicate( (Station) s.Source ) );
        }

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="predicate">Methode, die prüft, ob eine Quelle gemeldet werden soll.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public static IEnumerable<SourceSelection> GetSources( Profile profile, Func<SourceSelection, bool> predicate )
        {
            // Resolve
            if (null == profile)
                yield break;

            // Load the map
            Dictionary<SourceIdentifier, SourceSelection> map;
            if (!CurrentState.SourceByIdentifierMap.TryGetValue( profile.Name, out map ))
                yield break;

            // Use state
            foreach (SourceSelection source in map.Values)
                if ((null == predicate) || predicate( source ))
                    yield return source;
        }

        /// <summary>
        /// Ermittelt ein Geräteprofil.
        /// </summary>
        /// <param name="name">Der Name des Geräteprofils oder <i>null</i> für das
        /// bevorzugte Profil.</param>
        /// <returns>Das gewünschte Geräteprofil.</returns>
        public static Profile FindProfile( string name )
        {
            // Use default
            if (string.IsNullOrEmpty( name ))
                return DefaultProfile;

            // Read out
            Profile profile;
            return CurrentState.ProfileMap.TryGetValue( name, out profile ) ? profile : null;
        }

        /// <summary>
        /// Meldet die Namen alle aktivierten Geräteprofile, das bevorzugte Profil immer zuerst.
        /// </summary>
        public static IEnumerable<string> ProfileNames { get { return CurrentState.Profiles.Select( p => p.Name ); } }

        /// <summary>
        /// Ermittelt den eindeutigen Namen einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Der eindeutige Name oder <i>null</i>, wenn die Quelle nicht
        /// bekannt ist..</returns>
        public static string GetUniqueName( SourceSelection source )
        {
            // Map to current
            var active = FindSource( source );
            if (active == null)
                return null;

            // Find the name
            string name;
            if (!CurrentState.UniqueNameBySelectionMap.TryGetValue( active.SelectionKey, out name ))
                return null;

            // Report it
            return name;
        }
    }

    /// <summary>
    /// Einige Hilfsmethoden zum Arbeiten mit Listen von Quellen.
    /// </summary>
    public static class ProfileExtensions
    {
        /// <summary>
        /// Ermittelt den eindeutigen Namen einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Der eindeutige Name oder <i>null</i>, wenn die Quelle nicht
        /// bekannt ist.</returns>
        public static string GetUniqueName( this SourceSelection source )
        {
            // Forward
            return VCRProfiles.GetUniqueName( source );
        }
    }
}
