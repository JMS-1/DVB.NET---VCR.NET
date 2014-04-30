using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JMS.DVB;


namespace JMS.TV.Core
{
    /// <summary>
    /// Stellt den Zugriff auf <i>DVB.NET</i> Geräte bereit.
    /// </summary>
    public class StandardFeedProvider : IFeedProvider, IDisposable
    {
        /// <summary>
        /// Beschreibt ein einzelnes Gerät.
        /// </summary>
        private class StandardDevice : IDevice
        {
            /// <summary>
            /// Ein leeres Feld von Quellen.
            /// </summary>
            private static readonly SourceSelection[] _NoSources = { };

            /// <summary>
            /// Das zugehörige Geräteprofil.
            /// </summary>
            private readonly Profile m_profile;

            /// <summary>
            /// Meldet den Namen des Geräteprofils.
            /// </summary>
            public string Name { get { return m_profile.Name; } }

            /// <summary>
            /// Erstellt die Beschreibung eines Gerätes.
            /// </summary>
            /// <param name="profile">Das zugehörige Geräteprofil.</param>
            public StandardDevice( Profile profile )
            {
                m_profile = profile;
            }

            /// <summary>
            /// Wählt eine Quellgruppe an.
            /// </summary>
            /// <param name="source">Eine der Quellen der Gruppe.</param>
            /// <returns>Die Liste aller Quellen der Gruppe.</returns>
            public CancellableTask<SourceSelection[]> Activate( SourceSelection source )
            {
                // Map to indicated device
                var localSource = m_profile.FindSource( source.Source ).FirstOrDefault();
                if (localSource == null)
                    throw new ArgumentException( "bad source", "source" );

                // Create task
                return
                    CancellableTask<SourceSelection[]>.Run( cancel =>
                    {
                        // Check for cancel
                        if (cancel.IsCancellationRequested)
                            return _NoSources;

                        // Start the hardware on first call - this may take some time
                        var device = localSource.GetHardware();

                        // Check for cancel
                        if (cancel.IsCancellationRequested)
                            return _NoSources;

                        // Select the group - again this may last a bit
                        localSource.SelectGroup( device );

                        // Check for cancel
                        if (cancel.IsCancellationRequested)
                            return _NoSources;

                        // Validate
                        var groupReader = device.GroupReader;
                        if (groupReader == null)
                            return _NoSources;
                        if (!groupReader.CancellableWait( cancel ))
                            return _NoSources;

                        // Load the map
                        var groupInformation = groupReader.Result;
                        if (groupInformation == null)
                            return _NoSources;

                        // Use profile to map to full source information
                        return
                            groupInformation
                                .Sources
                                .Select( s => m_profile.FindSource( s ).FirstOrDefault() )
                                .Where( s => s != null )
                                .ToArray();
                    } );
            }

            /// <summary>
            /// Erzeugt eine Hintergrundaufgabe zum Ermitteln von Quelldaten.
            /// </summary>
            /// <param name="source">Die gewünschte Quelle.</param>
            /// <returns>Eine neue passende Hintergrundaufgabe.</returns>
            public CancellableTask<SourceInformation> GetSourceInformationAsync( SourceSelection source )
            {
                // Make sure this device can map the source
                var localSource = m_profile.FindSource( source.Source ).FirstOrDefault();
                if (localSource == null)
                    throw new ArgumentException( "bad source", "source" );
                else
                    return SourceInformationReader.GetSourceInformationAsync( localSource );
            }

            /// <summary>
            /// Entfernt alle vorgehaltenen Quelldaten.
            /// </summary>
            public void RefreshSourceInformations()
            {
                m_profile.CreateHardware().ResetInformationReaders();
            }

            /// <summary>
            /// Prüft, ob eine Quelle bekannt ist.
            /// </summary>
            /// <param name="sourceName">Der Name der Quelle.</param>
            /// <returns>Die gewünschte Quelle.</returns>
            public SourceSelection FindSource( string sourceName )
            {
                return m_profile.FindSource( sourceName ).FirstOrDefault();
            }
        }

        /// <summary>
        /// Erlaubt den Zugriff auf Geräte.
        /// </summary>
        private IDisposable m_hardware;

        /// <summary>
        /// Alle Geräteprofile.
        /// </summary>
        private readonly StandardDevice[] m_devices;

        /// <summary>
        /// Erstellt eine neue Zugriffsverwaltung.
        /// </summary>
        /// <param name="profileNames">Die Namen der zu verwendenden Geräteprofile.</param>
        public StandardFeedProvider( params string[] profileNames )
        {
            // Helper
            m_devices =
                (profileNames ?? Enumerable.Empty<string>())
                    .Select( profileName => new StandardDevice( ProfileManager.FindProfile( profileName ) ) )
                    .ToArray();

            // Check configuration
            if (m_devices.Length != new HashSet<string>( m_devices.Select( profile => profile.Name ), ProfileManager.ProfileNameComparer ).Count)
                throw new ArgumentException( "invalid profile list", "profileNames" );

            // Start hardware access
            m_hardware = HardwareManager.Open();
        }

        /// <summary>
        /// Beendet die Nutzung endgültig.
        /// </summary>
        public void Dispose()
        {
            // Cleanup once
            var hardware = Interlocked.Exchange( ref m_hardware, null );
            if (hardware != null)
                hardware.Dispose();
        }

        /// <summary>
        /// Meldet die Anzahl der zur Verfügung stehenden Geräte.
        /// </summary>
        int IFeedProvider.NumberOfDevices { get { return m_devices.Length; } }

        /// <summary>
        /// Meldet ein Gerät.
        /// </summary>
        /// <param name="index">Die 0-basierte laufenden Nummer des Gerätes.</param>
        /// <returns>Das gewünschte Gerät.</returns>
        IDevice IFeedProvider.GetDevice( int index )
        {
            return m_devices[index];
        }

        /// <summary>
        /// Ermittelt eine Quelle.
        /// </summary>
        /// <param name="sourceName">Der Name der Quelle.</param>
        /// <returns>Ein Information falls die Quelle bekannt ist.</returns>
        SourceSelection IFeedProvider.Translate( string sourceName )
        {
            // Look up for any profile
            var sources = m_devices.Select( device => device.FindSource( sourceName ) ).ToArray();

            // Only if known to all
            if (sources.Any( source => source == null ))
                return null;

            // Only if same for all
            var different = new HashSet<SourceIdentifier>( sources.Select( source => source.Source ) );
            if (different.Count == 1)
                return sources[0];
            else
                return null;
        }
    }
}
