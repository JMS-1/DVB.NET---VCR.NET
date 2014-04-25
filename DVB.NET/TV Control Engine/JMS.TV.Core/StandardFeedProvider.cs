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
        /// Erlaubt den Zugriff auf Geräte.
        /// </summary>
        private IDisposable m_hardware;

        /// <summary>
        /// Alle Geräteprofile.
        /// </summary>
        private readonly Profile[] m_profiles;

        /// <summary>
        /// Erstellt eine neue Zugriffsverwaltung.
        /// </summary>
        /// <param name="profileNames">Die Namen der zu verwendenden Geräteprofile.</param>
        public StandardFeedProvider( params string[] profileNames )
        {
            // Helper
            m_profiles = (profileNames ?? Enumerable.Empty<string>()).Select( ProfileManager.FindProfile ).ToArray();

            // Check configuration
            if (m_profiles.Length != new HashSet<string>( m_profiles.Select( profile => profile.Name ), ProfileManager.ProfileNameComparer ).Count)
                throw new ArgumentException( "inbalid profile list", "profileNames" );

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
        int IFeedProvider.NumberOfDevices { get { return m_profiles.Length; } }

        /// <summary>
        /// Öffnet ein Gerät.
        /// </summary>
        /// <param name="index">Die laufende Nummer des Gerätes.</param>
        void IFeedProvider.AllocateDevice( int index )
        {
            // First call will load DVB.NET driver
            HardwareManager.OpenHardware( m_profiles[index] );
        }

        /// <summary>
        /// Wählt eine Quellgruppe an.
        /// </summary>
        /// <param name="index">Die laufende Nummer des Gerätes.</param>
        /// <param name="source">Eine der Quellen der Gruppe.</param>
        /// <returns>Die Liste aller Quellen der Gruppe.</returns>
        CancellableTask<SourceSelection[]> IFeedProvider.Activate( int index, SourceSelection source )
        {
            // Map to indicated device
            var profile = m_profiles[index];
            var localSource = profile.FindSource( source.Source ).FirstOrDefault();
            if (localSource == null)
                throw new ArgumentException( "bad source", "source" );

            // Select the group
            localSource.SelectGroup();

            // Retrieve the group information
            var device = localSource.GetHardware();
            var groupReader = device.GroupReader;

            // Create task
            return
                CancellableTask<SourceSelection[]>.Run( cancel =>
                {
                    // Validate
                    if (groupReader == null)
                        return new SourceSelection[0];
                    if (!groupReader.CancellableWait( cancel ))
                        return new SourceSelection[0];

                    // Load the map
                    var groupInformation = groupReader.Result;
                    if (groupInformation == null)
                        return new SourceSelection[0];

                    // Use profile to map to full source information
                    return
                        groupInformation
                            .Sources
                            .Select( s => profile.FindSource( s ).FirstOrDefault() )
                            .Where( s => s != null )
                            .ToArray();
                } );
        }

        /// <summary>
        /// Ermittelt eine Quelle.
        /// </summary>
        /// <param name="sourceName">Der Name der Quelle.</param>
        /// <returns>Ein Information falls die Quelle bekannt ist.</returns>
        SourceSelection IFeedProvider.Translate( string sourceName )
        {
            // Look up for any profile
            var sources = m_profiles.Select( profile => profile.FindSource( sourceName ).FirstOrDefault() ).ToArray();

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

        /// <summary>
        /// Erzeugt eine Hintergrundaufgabe zum Ermitteln von Quelldaten.
        /// </summary>
        /// <param name="index">Die laufende Nummer eines Gerätes.</param>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Eine neue passende Hintergrundaufgabe.</returns>
        CancellableTask<SourceInformation> IFeedProvider.GetSourceInformationAsync( int index, SourceSelection source )
        {
            // Map to indicated device
            var profile = m_profiles[index];
            var localSource = profile.FindSource( source.Source ).FirstOrDefault();
            if (localSource == null)
                throw new ArgumentException( "bad source", "source" );

            // Start task
            return SourceInformationReader.GetSourceInformationAsync( localSource );
        }

        /// <summary>
        /// Entfernt alle vorgehaltenen Quelldaten.
        /// </summary>
        /// <param name="index">Die laufende Nummer des zu verwendenden Gerätes.</param>
        void IFeedProvider.RefreshSourceInformations( int index )
        {
            m_profiles[index].CreateHardware().ResetInformationReaders();
        }
    }
}
