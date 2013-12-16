using System;
using System.Linq;
using JMS.DVB.SI;


namespace JMS.DVB
{
    /// <summary>
    /// Verwaltet die Anforderung für die Aktualisierung der Daten einer Quelle.
    /// </summary>
    public class SourceInformationReader : IDisposable
    {
        /// <summary>
        /// Die zugehörige Anfrage für die aktuelle SI Tabelle.
        /// </summary>
        private IAsynchronousTableReader<PMT> m_TableReader;

        /// <summary>
        /// Die ursprünglichen Daten des Senders.
        /// </summary>
        public SourceIdentifier Source { get; private set; }

        /// <summary>
        /// Die neuen Daten des Senders.
        /// </summary>
        private SourceInformation m_CurrentSettings;

        /// <summary>
        /// Das Gerät, über das die Aktualisierung ausgeführt wird.
        /// </summary>
        private Hardware m_Hardware;

        /// <summary>
        /// Optional die Senderliste, der die Quelle entnommen wurde.
        /// </summary>
        private Profile m_Profile;

        /// <summary>
        /// Erzeugt eine neue Aktualisierungsverwaltung.
        /// </summary>
        /// <param name="source">Die Quelle, deren Daten ermittelt werden sollen.</param>
        /// <param name="hardware">Das zu verwendende Gerät.</param>
        /// <param name="profile">Optional die Senderliste, aus der die Quelle entnommen wurde.</param>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        /// <exception cref="ArgumentException">Die Quelle ist auf der aktuell gewählten Quellgruppe
        /// (Transponder) nicht verfügbar.</exception>
        public SourceInformationReader( SourceIdentifier source, Hardware hardware, Profile profile )
        {
            // Validate
            if (null == source)
                throw new ArgumentNullException( "source" );
            if (null == hardware)
                throw new ArgumentNullException( "hardware" );

            // Find the station in the current 
            ushort? pmt = hardware.GetServicePMT( source );
            if (!pmt.HasValue)
                return;

            // Create waiter
            m_TableReader = hardware.BeginGetTable<PMT>( pmt.Value );

            // Remember all
            m_Hardware = hardware;
            m_Profile = profile;
            Source = source;
        }

        /// <summary>
        /// Wartet auf das Eintreffen der gewünschten Daten.
        /// </summary>
        /// <returns>Die neuen Daten des Senders oder <i>null</i>, wenn keine gefunden wurden.</returns>
        public SourceInformation Wait()
        {
            // Forward
            return Wait( 5000 );
        }

        /// <summary>
        /// Wartet auf das Eintreffen der gewünschten Daten.
        /// </summary>
        /// <param name="milliseconds">Die maximale Wartezeit in Millisekunden.</param>
        /// <returns>Die neuen Daten des Senders oder <i>null</i>, wenn keine gefunden wurden.</returns>
        public SourceInformation Wait( int milliseconds )
        {
            // Not started
            if (null == m_TableReader)
                return null;

            // Already did it
            if (null != m_CurrentSettings)
                return m_CurrentSettings;

            // Wait for table
            PMT[] pmts = m_TableReader.WaitForTables( milliseconds );
            if (null == pmts)
                return null;

            // Create dummy
            m_CurrentSettings = new SourceInformation { Source = Source, VideoType = VideoTypes.NoVideo };

            // Process all PMT - actually should be only one
            foreach (PMT pmt in pmts)
            {
                // Overwrite encryption if CA descriptor is present
                if (null != pmt.Table.Descriptors)
                    m_CurrentSettings.IsEncrypted = pmt.Table.Descriptors.Any( d => EPG.DescriptorTags.CA == d.Tag );

                // Process the program entries
                foreach (var program in pmt.Table.ProgramEntries)
                    m_CurrentSettings.Update( program );
            }

            // Attach to the related group information
            GroupInformation groupInfo = m_Hardware.GetGroupInformation();
            if (null != groupInfo)
            {
                // Find the related station information
                Station station = (Station) groupInfo.Sources.Find( s => s.Equals( Source ) );
                if (null != station)
                {
                    // Take data from there
                    m_CurrentSettings.Provider = station.Provider;
                    m_CurrentSettings.Name = station.Name;

                    // See if this is a service
                    m_CurrentSettings.IsService = station.IsService;

                    // Overwrite encryption if regular service entry exists
                    if (!m_CurrentSettings.IsService)
                        m_CurrentSettings.IsEncrypted = station.IsEncrypted;
                }
            }

            // See if profile is attached
            if (null != m_Profile)
            {
                // Read the modifier
                SourceModifier modifier = m_Profile.GetFilter( m_CurrentSettings.Source );

                // Appliy fixed values
                if (null != modifier)
                    modifier.ApplyTo( m_CurrentSettings );
            }

            // Report
            return m_CurrentSettings;
        }

        #region IDisposable Members

        /// <summary>
        /// Gibt alle verwendeten Ressourcen frei.
        /// </summary>
        public void Dispose()
        {
            // Check waiter
            if (null != m_TableReader)
                try
                {
                    // Forward
                    m_TableReader.Dispose();
                }
                finally
                {
                    // Forget
                    m_TableReader = null;
                }
        }

        #endregion
    }
}
