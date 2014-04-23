extern alias oldVersion;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;

using JMS.DVB.SI;
using JMS.DVB.SI.ProgramGuide;

using legacy = oldVersion.JMS.DVB.EPG;


namespace JMS.DVB.CardServer
{
    partial class InMemoryCardServer
    {
        /// <summary>
        /// Ein leeres Feld von Zeichenketten.
        /// </summary>
        private static readonly string[] EmptyStringArray = { };

        /// <summary>
        /// Wird gesetzt, während die Programmzeitschrift aktualisiert wird.
        /// </summary>
        public double? EPGProgress { get; private set; }

        /// <summary>
        /// Die zusätzlichen Erweiterungen, die bei der Aktualisierung der Programmzeitschrift berücksichtigt werden sollen.
        /// </summary>
        private EPGExtensions m_EPGExtensions = EPGExtensions.None;

        /// <summary>
        /// Alle Quellgruppen (Transponder), die bei der Aktualisierung der Programmzeitschrift anzusteuern
        /// sind.
        /// </summary>
        private Dictionary<GroupKey, bool> m_EPGGroups = new Dictionary<GroupKey, bool>();

        /// <summary>
        /// Alle Quellen, zu denen Daten in die Programmzeitschrift aufgenommen werden sollen. 
        /// </summary>
        private Dictionary<SourceIdentifier, SourceSelection> m_EPGSources = new Dictionary<SourceIdentifier, SourceSelection>();

        /// <summary>
        /// Die Liste der zu bearbeitenden Quellgruppen (Transponder) für die Aktualisierung der
        /// Programmzeitschrift.
        /// </summary>
        private List<GroupKey> m_EPGPending;

        /// <summary>
        /// Der Zeitpunkt, an dem bei der Aktualisierung der Programmzeitschrift letztmalig die Quellgruppe
        /// gewechselt wurde.
        /// </summary>
        private DateTime m_EPGLastTune;

        /// <summary>
        /// Alle bisher ermittelten Daten zur Programmzeitschrift.
        /// </summary>
        private static Dictionary<SourceIdentifier, Dictionary<DateTime, ProgramGuideItem>> m_EPGItems = new Dictionary<SourceIdentifier, Dictionary<DateTime, ProgramGuideItem>>();

        /// <summary>
        /// Die Anzahl der bisher ermittelten Einträge für die Programmzeitschrift.
        /// </summary>
        private volatile int m_EPGItemCount;

        /// <summary>
        /// Die Anzahl der Einträge bei der letzten Prüfung.
        /// </summary>
        private int m_EPGLastItemCount = -1;

        /// <summary>
        /// Der Zeitpunkt, wann zum letzen Mal auf die Anzahl der Einträge geprüft wurde.
        /// </summary>
        private DateTime m_EPGLastItemCheck;

        /// <summary>
        /// In diesem Zeitabstand (in Sekunden) wird geprüft, ob die Sammlung auf einer Quellgruppe vorzeitig
        /// beendet werden soll.
        /// </summary>
        private int? EPGItemCountCheckInterval = null;

        /// <summary>
        /// Beginnt mit der Sammlung der Daten für die elektronische Programmzeitschrift
        /// (EPG).
        /// </summary>
        /// <param name="device">Das zu verwendende DVB.NET Gerät.</param>
        /// <param name="sources">Die zu berücksichtigenden Quellen.</param>
        /// <param name="extensions">Spezielle Zusatzfunktionalitäten der Sammlung.</param>
        private void StartEPGCollection( Hardware device, SourceIdentifier[] sources, EPGExtensions extensions )
        {
            // Check mode
            if (EPGProgress.HasValue)
                CardServerException.Throw( new EPGActiveFault() );
            if (m_ScanProgress >= 0)
                CardServerException.Throw( new SourceUpdateActiveFault() );

            // Reset lists
            m_EPGSources.Clear();
            m_EPGGroups.Clear();

            // Load all identifiers to scan
            if (null != sources)
                foreach (SourceIdentifier source in sources)
                    AddEPGSource( source );

            // Add specials
            if (0 != (extensions & EPGExtensions.PREMIEREDirect))
                if (AddEPGGroup( DirectCIT.TriggerSource ).Length < 1)
                    extensions &= ~EPGExtensions.PREMIEREDirect;
            if (0 != (extensions & EPGExtensions.PREMIERESport))
                if (AddEPGGroup( SportCIT.TriggerSource ).Length < 1)
                    extensions &= ~EPGExtensions.PREMIERESport;
            if (0 != (extensions & EPGExtensions.FreeSatUK))
                if (AddEPGGroup( EIT.FreeSatEPGTriggerSource ).Length < 1)
                    extensions &= ~EPGExtensions.FreeSatUK;

            // Stop all
            RemoveAll();

            // Prepare
            m_EPGPending = new List<GroupKey>( m_EPGGroups.Keys );
            m_EPGLastItemCheck = DateTime.MaxValue;
            m_EPGLastTune = DateTime.MinValue;
            m_EPGLastItemCount = -1;
            m_EPGItems.Clear();
            m_EPGItemCount = 0;

            // Mark as active
            m_EPGExtensions = extensions;
            EPGProgress = 0;

            // Enforce start
            CollectProgramGuide( device );
        }

        /// <summary>
        /// Wird aufgerufen, wenn ein Eintrag für die Programmzeitschrift erkannt wurde.
        /// </summary>
        /// <param name="epg">Ein Element der Programmzeitschrift.</param>
        private void OnStandardEPGEvent( EIT epg )
        {
            // Ignore any error
            try
            {
                // Attach to source
                SourceIdentifier source = epg.Source;

                // See if we are interested
                if (!m_EPGSources.ContainsKey( source ))
                    return;

                // Process all events
                foreach (legacy.EventEntry epgEntry in epg.Table.Entries)
                    AddGuideItem( source, epgEntry.EventIdentifier, epgEntry.StartTime, epgEntry.Duration, epgEntry.Descriptors );
            }
            catch
            {
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn ein Eintrag für die Programmzeitschrift der PREMIERE Dienste
        /// erkannt wurde.
        /// </summary>
        /// <param name="epg">Ein Element der Programmzeitschrift.</param>
        private void OnPremiereEPGEvent( CIT epg )
        {
            // Ignore any error
            try
            {
                // Start processing the data
                foreach (legacy.Descriptor descriptor in epg.Table.Descriptors)
                {
                    // Check for schedule information
                    legacy.Descriptors.ContentTransmissionPremiere schedule = descriptor as legacy.Descriptors.ContentTransmissionPremiere;
                    if (null == schedule)
                        continue;

                    // Create identifier
                    SourceIdentifier source = new SourceIdentifier { Network = schedule.OriginalNetworkIdentifier, TransportStream = schedule.TransportStreamIdentifier, Service = schedule.ServiceIdentifier };

                    // Process all start times
                    foreach (DateTime start in schedule.StartTimes)
                        AddGuideItem( source, epg.Table.Identifier, start, epg.Table.Duration, epg.Table.Descriptors );
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Erzeugt die Beschreibung eines Eintrags aus der Programmzeitschrift und vermerkt diese
        /// in der Gesamtliste.
        /// </summary>
        /// <remarks>Wird auf einem separaten <see cref="Thread"/> ausgeführt und
        /// muss alle Zugriff auf die statischen Daten geeignet synchronisieren.</remarks>
        /// <param name="source">Die Quelle, zu der die Information gehört.</param>
        /// <param name="identifier">Die eindeutige Kennung der Sendung.</param>
        /// <param name="startTime">Anfangszeitpunkt in UTC / GMT.</param>
        /// <param name="duration">Dauer der zugehörigen Sendung.</param>
        /// <param name="descriptors">Ergänzende Beschreibungen zur Sendung.</param>
        /// <returns>Die neu erzeugte Beschreibungsinstanz.</returns>
        private void AddGuideItem( SourceIdentifier source, uint identifier, DateTime startTime, TimeSpan duration, legacy.Descriptor[] descriptors )
        {
            // First create it
            ProgramGuideItem info = CreateGuideItem( source, identifier, startTime, duration, descriptors );
            if (null == info)
                return;

            // Must synchronize
            lock (m_EPGItems)
            {
                // Attach to the source list
                Dictionary<DateTime, ProgramGuideItem> list;
                if (!m_EPGItems.TryGetValue( source, out list ))
                {
                    // Create new
                    list = new Dictionary<DateTime, ProgramGuideItem>();

                    // Add it
                    m_EPGItems[source] = list;
                }

                // Old count
                int before = list.Count;

                // Remember
                list[startTime] = info;

                // New count
                m_EPGItemCount += (list.Count - before);
            }
        }

        /// <summary>
        /// Erzeugt die Beschreibung eines Eintrags aus der Programmzeitschrift.
        /// </summary>
        /// <remarks>Wird auf einem separaten <see cref="Thread"/> ausgeführt und
        /// muss alle Zugriff auf die statischen Daten geeignet synchronisieren.</remarks>
        /// <param name="source">Die Quelle, zu der die Information gehört.</param>
        /// <param name="identifier">Die eindeutige Kennung der Sendung.</param>
        /// <param name="startTime">Anfangszeitpunkt in UTC / GMT.</param>
        /// <param name="duration">Dauer der zugehörigen Sendung.</param>
        /// <param name="descriptors">Ergänzende Beschreibungen zur Sendung.</param>
        /// <returns>Die neu erzeugte Beschreibungsinstanz.</returns>
        private ProgramGuideItem CreateGuideItem( SourceIdentifier source, uint identifier, DateTime startTime, TimeSpan duration, legacy.Descriptor[] descriptors )
        {
            // Descriptors we can have
            legacy.Descriptors.ParentalRating rating = null;
            legacy.Descriptors.ShortEvent shortEvent = null;
            //

            // Collector
            var exEvents = new List<legacy.Descriptors.ExtendedEvent>();
            var categories = new HashSet<ContentCategory>();

            // Check all descriptors
            foreach (var descr in descriptors)
                if (descr.IsValid)
                {
                    // Check type
                    if (shortEvent == null)
                    {
                        // Read
                        shortEvent = descr as legacy.Descriptors.ShortEvent;

                        // Done for now
                        if (null != shortEvent)
                            continue;
                    }
                    if (rating == null)
                    {
                        // Read
                        rating = descr as legacy.Descriptors.ParentalRating;

                        // Done for now
                        if (null != rating)
                            continue;
                    }

                    // Event
                    var exEvent = descr as legacy.Descriptors.ExtendedEvent;
                    if (exEvent != null)
                    {
                        // Remember
                        exEvents.Add( exEvent );

                        // Next
                        continue;
                    }

                    // Check for content information
                    var content = descr as legacy.Descriptors.Content;
                    if (content != null)
                    {
                        // Process
                        if (content.Categories != null)
                            foreach (var singleCategory in content.Categories)
                                categories.Add( Event.GetContentCategory( singleCategory ) );

                        // Next
                        continue;
                    }
                }

            // Data
            string name = null, description = null, language = null, shortDescription = null;

            // Take the best we got
            if (exEvents.Count > 0)
            {
                // Text builder
                StringBuilder text = new StringBuilder();

                // Process all
                foreach (legacy.Descriptors.ExtendedEvent exEvent in exEvents)
                {
                    // Normal
                    if (null == name)
                        name = exEvent.Name;
                    if (null == language)
                        language = exEvent.Language;

                    // Merge
                    if (exEvent.Text != null)
                        text.Append( exEvent.Text );
                }

                // Use
                description = text.ToString();
            }

            // Try short event
            if (shortEvent != null)
            {
                // Remember
                if (string.IsNullOrEmpty( shortEvent.Name ))
                    shortDescription = shortEvent.Text;
                else if (string.IsNullOrEmpty( shortEvent.Text ))
                    shortDescription = shortEvent.Name;
                else
                    shortDescription = string.Format( "{0} ({1})", shortEvent.Name, shortEvent.Text );

                // Read
                if (null == name)
                    name = shortEvent.Name;
                if (null == description)
                    description = shortEvent.Text;
                if (null == language)
                    language = shortEvent.Language;
            }

            // Not possible
            if (string.IsNullOrEmpty( name ))
                return null;

            // Defaults
            if (shortDescription == null)
                shortDescription = string.Empty;

            // Defaults
            if (string.IsNullOrEmpty( description ))
                description = "-";
            if (null == language)
                language = string.Empty;

            // Create event
            return new ProgramGuideItem
            {
                Ratings = ((rating == null) ? null : rating.Ratings) ?? EmptyStringArray,
                Duration = (int) duration.TotalSeconds,
                ShortDescription = shortDescription,
                Content = categories.ToArray(),
                Description = description,
                Identifier = identifier,
                Language = language,
                Start = startTime,
                Source = source,
                Name = name
            };
        }

        /// <summary>
        /// Steuerung der Aktivierung der Programmzeitschrift.
        /// </summary>
        /// <param name="device">Das aktuell verwendete DVB.NET Gerät.</param>
        private void CollectProgramGuide( Hardware device )
        {
            // Not necessary
            if (!EPGProgress.HasValue)
                return;

            // Be safe
            try
            {
                // Load interval once
                if (!EPGItemCountCheckInterval.HasValue)
                    try
                    {
                        // Load setting
                        string interval = ConfigurationManager.AppSettings["CardServerProgramGuideCountCheckInterval"];

                        // Load data
                        uint value;
                        if (!string.IsNullOrEmpty( interval ))
                            if (uint.TryParse( interval, out value ))
                                if ((value > 0) && (value < int.MaxValue))
                                    EPGItemCountCheckInterval = (int) value;
                    }
                    finally
                    {
                        // Load default
                        if (!EPGItemCountCheckInterval.HasValue)
                            EPGItemCountCheckInterval = 10;
                    }

                // Time since we last checked the item count
                TimeSpan countDelta = DateTime.UtcNow - m_EPGLastItemCheck;

                // Must recheck
                if (countDelta.TotalSeconds >= EPGItemCountCheckInterval.Value)
                    if (m_EPGLastItemCount == m_EPGItemCount)
                    {
                        // Early stop
                        m_EPGLastTune = DateTime.MinValue;
                    }
                    else
                    {
                        // Remember item count
                        m_EPGLastItemCheck = DateTime.UtcNow;
                        m_EPGLastItemCount = m_EPGItemCount;
                    }

                // Read the interval since the last tune
                TimeSpan delta = DateTime.UtcNow - m_EPGLastTune;

                // Check for change interval
                if (delta.TotalSeconds < 60)
                    return;

                // Always shut down receivers
                device.SelectGroup( null, null );

                // Get the current state
                int total = m_EPGGroups.Count, left = m_EPGPending.Count;

                // Set the progress value
                if (total < 1)
                    EPGProgress = 1;
                else
                    EPGProgress = 1.0 * (total - left) / total;

                // See if we are fully done
                if (left < 1)
                    return;

                // Load next
                GroupKey next = m_EPGPending[0];

                // Remove from list
                m_EPGPending.RemoveAt( 0 );

                // Tune to transponder
                device.SelectGroup( next.Location, next.Group );

                // See if there is something on this group
                if (null == device.GetGroupInformation( 5000 ))
                {
                    // Push back
                    m_EPGPending.Add( next );

                    // Next after a short delay
                    return;
                }

                // Add standard EPG
                device.AddProgramGuideConsumer( OnStandardEPGEvent );

                // Check PREMIERE Direct
                if (0 != (m_EPGExtensions & EPGExtensions.PREMIEREDirect))
                    foreach (SourceSelection selection in Profile.FindSource( DirectCIT.TriggerSource ))
                        if (Equals( selection.Location, device.CurrentLocation ))
                            if (Equals( selection.Group, device.CurrentGroup ))
                            {
                                // Register
                                device.SetConsumerState( device.AddConsumer<DirectCIT>( OnPremiereEPGEvent ), true );

                                // Did it
                                break;
                            }

                // Check PREMIERE Sport
                if (0 != (m_EPGExtensions & EPGExtensions.PREMIERESport))
                    foreach (SourceSelection selection in Profile.FindSource( SportCIT.TriggerSource ))
                        if (Equals( selection.Location, device.CurrentLocation ))
                            if (Equals( selection.Group, device.CurrentGroup ))
                            {
                                // Register
                                device.SetConsumerState( device.AddConsumer<SportCIT>( OnPremiereEPGEvent ), true );

                                // Did it
                                break;
                            }

                // Check FreeSat UK
                if (0 != (m_EPGExtensions & EPGExtensions.FreeSatUK))
                    foreach (SourceSelection selection in Profile.FindSource( EIT.FreeSatEPGTriggerSource ))
                        if (Equals( selection.Location, device.CurrentLocation ))
                            if (Equals( selection.Group, device.CurrentGroup ))
                            {
                                // Register
                                Guid consumerId = device.AddConsumer<EIT>( EIT.FreeSatEPGPID, OnStandardEPGEvent );
                                device.SetConsumerState( consumerId, true );

                                // Did it
                                break;
                            }

                // Reset time
                m_EPGLastTune = DateTime.UtcNow;

                // Remember item count
                m_EPGLastItemCheck = DateTime.UtcNow;
                m_EPGLastItemCount = m_EPGItemCount;
            }
            catch
            {
                // Just go on
            }
        }

        /// <summary>
        /// Meldet eine Quelle zur Sammlung der Daten aus der Programmzeitschrift an.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        private void AddEPGSource( SourceIdentifier source )
        {
            // Process
            foreach (SourceSelection selection in AddEPGGroup( source ))
            {
                // First only
                m_EPGSources[source] = selection;

                // Done
                break;
            }
        }

        /// <summary>
        /// Meldet die Gruppe (Transponder) einer Quelle zur Sammlung der
        /// Programmzeitschrift an.
        /// </summary>
        /// <param name="source">Die betroffene Quelle.</param>
        /// <returns>Meldet alle passenden Quellen.</returns>
        private SourceSelection[] AddEPGGroup( SourceIdentifier source )
        {
            // Resolve
            SourceSelection[] selections = Profile.FindSource( source );

            // Process
            foreach (SourceSelection selection in selections)
                m_EPGGroups[new GroupKey( selection )] = true;

            // Report
            return selections;
        }

        /// <summary>
        /// Beginnt mit der Sammlung der Daten für die elektronische Programmzeitschrift
        /// (EPG).
        /// </summary>
        /// <param name="sources">Die zu berücksichtigenden Quellen.</param>
        /// <param name="extensions">Spezielle Zusatzfunktionalitäten der Sammlung.</param>
        protected override void OnStartEPGCollection( SourceIdentifier[] sources, EPGExtensions extensions )
        {
            // Process
            Start( device => { StartEPGCollection( device, sources, extensions ); } );
        }

        /// <summary>
        /// Meldet alle gesammelten Informationen der Programmzeitschrift.
        /// </summary>
        private ProgramGuideItem[] CreateGuideItems()
        {
            // Result
            List<ProgramGuideItem> schedules = new List<ProgramGuideItem>();

            // Lock out - actually this should not be necessary
            lock (m_EPGItems)
            {
                // Loop over all we found
                foreach (Dictionary<DateTime, ProgramGuideItem> list in m_EPGItems.Values)
                {
                    // Get all dates for one source
                    List<DateTime> dates = new List<DateTime>( list.Keys );

                    // Sort ascending
                    dates.Sort();

                    // Next allowed date
                    DateTime allowed = DateTime.MinValue;

                    // Process all dates
                    foreach (DateTime start in dates)
                    {
                        // Attach to the item
                        ProgramGuideItem info = list[start];

                        // Start is hidden
                        if (start < allowed)
                        {
                            // End is hidden - discard all
                            if (info.End <= allowed)
                                continue;

                            // Adjust duration
                            info.Duration = (int) (info.End - allowed).TotalSeconds;

                            // Adjust start
                            info.Start = allowed;
                        }

                        // Remember it
                        schedules.Add( info );

                        // Set the end as the new allowed point
                        allowed = info.End;
                    }
                }

                // Wipe out
                m_EPGItems.Clear();
            }

            // Report
            return schedules.ToArray();
        }

        /// <summary>
        /// Beendet die Aktualisierung der Programmzeitschrift.
        /// </summary>
        protected override void OnEndEPGCollection()
        {
            // Process
            Start( device =>
            {
                // Check mode
                if (!EPGProgress.HasValue)
                    CardServerException.Throw( new EPGNotActiveFault() );

                // Disable all consumers
                device.SelectGroup( null, null );

                // Preserve some memory
                m_EPGPending.Clear();
                m_EPGSources.Clear();
                m_EPGGroups.Clear();

                // Terminate
                EPGProgress = null;

                // Process result - will reset the list
                return CreateGuideItems();
            } );
        }
    }
}
