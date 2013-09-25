using System;
using System.Linq;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt eine geplante Aufzeichnung.
    /// </summary>
    public static class RecordingDefinition
    {
        /// <summary>
        /// Beschreibt eine Aufzeichnung, die nur ein einziges Mal ausgeführt wird.
        /// </summary>
        /// <typeparam name="UserDataType">Vom Benutzer zusätzlich bereitgestellte Informationen.</typeparam>
        private class _OneOffItem<UserDataType> : IRecordingDefinition<UserDataType>
        {
            /// <summary>
            /// Eine leere Liste von Zeitpunkten.
            /// </summary>
            protected static readonly SuggestedPlannedTime[] NoTimes = { };

            /// <summary>
            /// Eine eindeutige Identifikation der Aktion.
            /// </summary>
            public Guid UniqueIdentifier { get; private set; }

            /// <summary>
            /// Optional eine Liste von Geräten, die für eine Aufzeichnung erlaubt sind.
            /// </summary>
            public IScheduleResource[] Resources { get; private set; }

            /// <summary>
            /// Ein Anzeigename zur Identifikation der Aktion.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Vom Anwender der Schnittstelle zusätzlich bereitgestellte Daten.
            /// </summary>
            public UserDataType Context { get; private set; }

            /// <summary>
            /// Die einzige geplante Aufzeichnungszeit.
            /// </summary>
            private PlannedTime m_Plan;

            /// <summary>
            /// Die einzige geplante Aufzeichnungszeit.
            /// </summary>
            protected PlannedTime Plan
            {
                get
                {
                    // Report
                    return m_Plan;
                }
            }

            /// <summary>
            /// Die zugehörige Quelle.
            /// </summary>
            public IScheduleSource Source { get; private set; }

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="context">Vom Anwender der Schnittstelle zusätzlich bereitgestellte Daten.</param>
            /// <param name="name">Eine Name zur Identifikation der Aufzeichnung.</param>
            /// <param name="uniqueIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
            /// <param name="resources">Die Liste aller Geräte, die verwendet werden dürfen.</param>
            /// <param name="source">Die gewünschte Quelle.</param>
            /// <param name="start">Der Beginn der Aufzeichnung.</param>
            /// <param name="duration">Die Dauer der Aufzeichnung.</param>
            public _OneOffItem( UserDataType context, string name, Guid uniqueIdentifier, IScheduleResource[] resources, IScheduleSource source, DateTime start, TimeSpan duration )
            {
                // Validate
                if (source == null)
                    throw new ArgumentNullException( "source" );
                if (start.Year < 2000)
                    throw new ArgumentOutOfRangeException( "start", string.Format( Properties.InterfaceResources.Exception_StartYear, start ) );
                if (duration.TotalSeconds <= 0)
                    throw new ArgumentOutOfRangeException( "duration", string.Format( Properties.InterfaceResources.Exception_Duration, duration ) );
                if (resources != null)
                    if (resources.Any( r => r == null ))
                        throw new ArgumentNullException( "resources" );

                // Create compound information
                m_Plan = new PlannedTime { Start = start, Duration = duration };

                // Remember simple information
                Resources = resources ?? new IScheduleResource[0];
                UniqueIdentifier = uniqueIdentifier;
                Context = context;
                Source = source;
                Name = name;
            }

            /// <summary>
            /// Meldet alle geplanten Aufzeichnungszeiten.
            /// </summary>
            /// <param name="minTime">Aufzeichnunge, die vor diesem Zeitpunkt enden, brauchen nicht in
            /// der Auflistung zu erscheinen. Es handelt sich um eine optionale Optimierung.</param>
            /// <returns>Eine Liste aller geplanten Zeiten.</returns>
            public virtual IEnumerable<SuggestedPlannedTime> GetTimes( DateTime minTime )
            {
                // Only one
                if (m_Plan.End <= minTime)
                    return NoTimes;
                else
                    return new SuggestedPlannedTime[] { m_Plan };
            }
        }

        /// <summary>
        /// Beschreibt eine sich wiederholende Aufzeichnung.
        /// </summary>
        /// <typeparam name="UserDataType">Vom Benutzer zusätzlich bereitgestellte Informationen.</typeparam>
        private class _RepeatingItem<UserDataType> : _OneOffItem<UserDataType>
        {
            /// <summary>
            /// Der Tag der letzten Aufzeichnung.
            /// </summary>
            private DateTime m_End;

            /// <summary>
            /// Alle Wochentage, an denen eine Aufzeichnung erlaubt ist.
            /// </summary>
            private HashSet<DayOfWeek> m_Pattern;

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="context">Vom Anwender der Schnittstelle zusätzlich bereitgestellte Daten.</param>
            /// <param name="name">Eine Name zur Identifikation der Aufzeichnung.</param>
            /// <param name="uniqueIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
            /// <param name="resources">Die Liste aller Geräte, die verwendet werden dürfen.</param>
            /// <param name="source">Die gewünschte Quelle.</param>
            /// <param name="start">Der Beginn der Aufzeichnung.</param>
            /// <param name="end">Der Tag der letzten Aufzeichnung.</param>
            /// <param name="duration">Die Dauer der Aufzeichnung.</param>
            /// <param name="pattern">Die Liste der Tage, an denen die Aufzeichnung stattfinden soll.</param>
            public _RepeatingItem( UserDataType context, string name, Guid uniqueIdentifier, IScheduleResource[] resources, IScheduleSource source, DateTime start, DateTime end, TimeSpan duration, IEnumerable<DayOfWeek> pattern )
                : base( context, name, uniqueIdentifier, resources, source, start, duration )
            {
                // Unmap
                var localStartDate = start.ToLocalTime().Date;
                var days = pattern.ToArray();

                // Remember
                m_Pattern = new HashSet<DayOfWeek>( days );
                m_End = end;

                // Validate
                if (end.TimeOfDay != TimeSpan.Zero)
                    throw new ArgumentException( string.Format( Properties.InterfaceResources.Exception_EndNotDay, end ), "end" );
                if (end < localStartDate)
                    throw new ArgumentException( string.Format( Properties.InterfaceResources.Exception_EndDay, localStartDate, end ), "end" );
                if (m_Pattern.Count < 1)
                    throw new ArgumentException( Properties.InterfaceResources.Exception_EmptyPattern, "pattern" );
                if (m_Pattern.Count != days.Length)
                    throw new ArgumentException( Properties.InterfaceResources.Exception_BadPattern, "pattern" );
            }

            /// <summary>
            /// Meldet alle geplanten Aufzeichnungszeiten.
            /// </summary>
            /// <param name="minTime">Aufzeichnunge, die vor diesem Zeitpunkt enden, brauchen nicht in
            /// der Auflistung zu erscheinen. Es handelt sich um eine optionale Optimierung.</param>
            /// <returns>Eine Liste aller geplanten Zeiten.</returns>
            public override IEnumerable<SuggestedPlannedTime> GetTimes( DateTime minTime )
            {
                // Get the recording time
                var next = Plan.Start;

                // Adjust a bit to avoid race conditions
                if (next < minTime)
                    next = minTime.ToLocalTime().Date.AddDays( -2 ) + next.ToLocalTime().TimeOfDay;
                else
                    next = next.ToLocalTime();

                // Process
                for (; ; next = next.AddDays( 1 ))
                {
                    // Adjust until day is valid
                    while (!m_Pattern.Contains( next.DayOfWeek ))
                        next = next.AddDays( 1 );

                    // Done if end is reached
                    if (next.Date > m_End)
                        yield break;

                    // Create plan
                    var planned = new PlannedTime { Start = next.ToUniversalTime(), Duration = Plan.Duration };

                    // Report if limit reached
                    if (planned.End > minTime)
                        yield return planned;
                }
            }
        }

        /// <summary>
        /// Erzeugt eine einmalige Aufzeichnung.
        /// </summary>
        /// <param name="context">Vom Anwender zusätzlich bereitgestellte Daten.</param>
        /// <param name="name">Eine Name zur Identifikation der Aufzeichnung.</param>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="resources">Die Liste aller Geräte, die verwendet werden dürfen.</param>
        /// <param name="source">Die zu verwendende Quelle.</param>
        /// <param name="start">Der Beginn der Aufzeichnung in UTC / GMT Notation.</param>
        /// <param name="duration">Die Dauer der Aufzeichnung.</param>
        /// <typeparam name="UserDataType">Vom Benutzer zusätzlich bereitgestellte Informationen.</typeparam>
        /// <returns>Die Beschreibung der Aufzeichnung.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Der Startzeitpunkt liegt vor dem Jahr 2000 oder
        /// die Dauer ist nicht positiv.</exception>
        public static IRecordingDefinition<UserDataType> Create<UserDataType>( UserDataType context, string name, Guid uniqueIdentifier, IScheduleResource[] resources, IScheduleSource source, DateTime start, TimeSpan duration )
        {
            // Use the simple implementation
            return new _OneOffItem<UserDataType>( context, name, uniqueIdentifier, resources, source, start, duration );
        }

        /// <summary>
        /// Erzeugt eine sich wiederholende Aufzeichnung.
        /// </summary>
        /// <param name="context">Vom Anwender zusätzlich bereitgestellte Daten.</param>
        /// <param name="name">Eine Name zur Identifikation der Aufzeichnung.</param>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="resources">Die Liste aller Geräte, die verwendet werden dürfen.</param>
        /// <param name="source">Die zu verwendende Quelle.</param>
        /// <param name="start">Der Beginn der Aufzeichnung in UTC / GMT Notation.</param>
        /// <param name="duration">Die Dauer der Aufzeichnung.</param>
        /// <param name="end">Der Tag in der lokalen Zeitzone, an dem die Aufzeichnung letztmalig ausgeführt werden soll.</param>
        /// <param name="dayPattern">Die Tage in der lokalen Zeitzone, an denen eine Aufzeichnung stattfinden soll.</param>
        /// <typeparam name="UserDataType">Vom Benutzer zusätzlich bereitgestellte Informationen.</typeparam>
        /// <returns>Die Beschreibung der Aufzeichnung.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Der Startzeitpunkt liegt vor dem Jahr 2000 oder
        /// die Dauer ist nicht positiv.</exception>
        public static IRecordingDefinition<UserDataType> Create<UserDataType>( UserDataType context, string name, Guid uniqueIdentifier, IScheduleResource[] resources, IScheduleSource source, DateTime start, TimeSpan duration, DateTime end, params DayOfWeek[] dayPattern )
        {
            // Use the repeating implementation which will do some more tests
            return new _RepeatingItem<UserDataType>( context, name, uniqueIdentifier, resources, source, start, end, duration, dayPattern ?? Enumerable.Empty<DayOfWeek>() );
        }
    }
}
