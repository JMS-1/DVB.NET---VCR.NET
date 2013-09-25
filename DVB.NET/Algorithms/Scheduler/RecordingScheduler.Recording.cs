using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    partial class RecordingScheduler
    {
        /// <summary>
        /// Verwaltet eine einzelne Aufzeichnung.
        /// </summary>
        private class _Schedule
        {
            /// <summary>
            /// Der geplante Zeitpunkt.
            /// </summary>
            public IScheduleDefinition Definition { get; private set; }

            /// <summary>
            /// Enthält die eindeutigen Kennungen aller Geräte, die benutzt werden dürfen.
            /// </summary>
            public IScheduleResource[] AllowedResources { get; private set; }

            /// <summary>
            /// Alle Ausnahmeregeln, geordnet nach dem Datum.
            /// </summary>
            private Dictionary<DateTime, PlanException> m_Exceptions;

            /// <summary>
            /// Die aktuelle Abfrage der Aufzeichnungsdaten.
            /// </summary>
            private IEnumerator<SuggestedPlannedTime> m_Scan;

            /// <summary>
            /// Die nächste Ausführungszeit.
            /// </summary>
            private SuggestedPlannedTime m_Current;

            /// <summary>
            /// Aufzeichnungen, die vor diesem Zeitpunkt enden, werden nicht berücksichtigt.
            /// </summary>
            private DateTime m_MinTime = DateTime.MinValue;

            /// <summary>
            /// Erzeugt eine neue Verwaltung.
            /// </summary>
            /// <param name="definition">Die Definition der Aufzeichnung.</param>
            /// <param name="exceptions">Alle Ausnahmen zur Aufzeichnung.</param>
            /// <exception cref="ArgumentNullException">Es wurde keine Aufzeichnung angegeben.</exception>
            public _Schedule( IScheduleDefinition definition, IEnumerable<PlanException> exceptions = null )
            {
                // Validate
                if (definition == null)
                    throw new ArgumentNullException( "plan" );

                // Remember
                Definition = definition;

                // Default
                if (exceptions == null)
                    exceptions = Enumerable.Empty<PlanException>();

                // Validate exceptions
                foreach (var exception in exceptions)
                    if (exception.ExceptionDate.TimeOfDay != TimeSpan.Zero)
                        throw new ArgumentException( string.Format( Properties.SchedulerResources.Exception_NotAPureDate, exception.ExceptionDate ), "exceptions" );

                // Cross validate
                foreach (var exception in exceptions.GroupBy( e => e.ExceptionDate ))
                    if (exception.Count() > 1)
                        throw new ArgumentException( string.Format( Properties.SchedulerResources.Exception_DuplicateDate, exception.Key ), "exceptions" );

                // Order plan
                m_Exceptions = exceptions.ToDictionary( e => e.ExceptionDate );
            }

            /// <summary>
            /// Meldet das aktuell gültige Element.
            /// </summary>
            /// <exception cref="InvalidOperationException">Es existiert kein gültiger Zeitpunkt mehr.</exception>
            public SuggestedPlannedTime Current
            {
                get
                {
                    // Not allowed
                    if (m_Scan == null)
                        return null;
                    else
                        return m_Current;
                }
            }

            /// <summary>
            /// Ermittelt zu allen erlaubten Geräten die eindeutigen Kennungen.
            /// </summary>
            /// <param name="scheduler">Die zugehörige Planungsinstanz.</param>
            /// <returns>Gesetzt, wenn mindestens ein Gerät erlaubt ist.</returns>
            public bool PrepareResourceMap( RecordingScheduler scheduler )
            {
                // Reset
                AllowedResources = null;

                // Check mode
                var resources = Definition.Resources;
                if (resources != null)
                    if (resources.Length > 0)
                        AllowedResources = Definition.Resources.Where( r => (r != null) && scheduler.Resources.Contains( r ) ).ToArray();

                // Create full map
                if (AllowedResources == null)
                    AllowedResources = scheduler.Resources.ToArray();

                // Report
                return (AllowedResources.Length > 0);
            }

            /// <summary>
            /// Prüft, ob ein weiterer Zeitpunkt vorliegt.
            /// </summary>
            public void MoveNext()
            {
                // As long as necessary
                for (; ; )
                {
                    // Ask base
                    if (m_Scan != null)
                        if (!m_Scan.MoveNext())
                            m_Scan = null;

                    // Report
                    if (m_Scan == null)
                        break;

                    // Load what base gives us
                    m_Current = m_Scan.Current;

                    // Load to check - normally we will not change it
                    var planned = m_Current.Planned;

                    // Find exception to apply         
                    PlanException exception;
                    if (m_Exceptions.TryGetValue( planned.Start.ToLocalTime().Date, out exception ))
                    {
#if !SILVERLIGHT
                        // Report
                        if (RecordingScheduler.SchedulerTrace.TraceVerbose)
                            Trace.TraceInformation( Properties.SchedulerResources.Trace_Exception, planned.Start, planned.Duration, exception.StartDelta, exception.DurationDelta );
#endif

                        // Change
                        planned =
                            new PlannedTime
                            {
                                Duration = planned.Duration + exception.DurationDelta,
                                Start = planned.Start + exception.StartDelta,
                            };

                        // Write back
                        m_Current.Planned = planned;
                    }

                    // Found it
                    if (planned.End > m_MinTime)
                        if (planned.Duration.TotalSeconds > 0)
                            break;
                }
            }

            /// <summary>
            /// Beginnt die Auflistung erneut.
            /// </summary>
            /// <param name="minTime">Aufzeichnungen, die vor diesem Zeitpunkt enden, werden nicht berücksichtigt.</param>
            public void Reset( DateTime minTime )
            {
                // Remember
                m_MinTime = minTime;

                // If we use exceptions better allow for some moderate correction
                if (m_Exceptions.Count > 0)
                    minTime = minTime.AddDays( -1 );

                // Ask planned item
                m_Scan = Definition.GetTimes( minTime ).GetEnumerator();
            }
        }

        /// <summary>
        /// Verwaltet eine einzelne Aufzeichnung.
        /// </summary>
        private class _Recording : _Schedule
        {
            /// <summary>
            /// Der geplante Zeitpunkt.
            /// </summary>
            public new IRecordingDefinition Definition
            {
                get
                {
                    // Report
                    return (IRecordingDefinition) base.Definition;
                }
            }

            /// <summary>
            /// Erzeugt eine neue Verwaltung.
            /// </summary>
            /// <param name="plan">Die Definition der Aufzeichnung.</param>
            /// <param name="exceptions">Alle Ausnahmen zur Aufzeichnung.</param>
            public _Recording( IRecordingDefinition plan, IEnumerable<PlanException> exceptions )
                : base( plan, exceptions )
            {
            }
        }

        /// <summary>
        /// Alle verwalteten Aufzeichnungen.
        /// </summary>
        private List<_Recording> m_PlanItems = new List<_Recording>();

        /// <summary>
        /// Meldet eine Aufzeichnung zur Planung an.
        /// </summary>
        /// <param name="definition">Die gewünschte Aufzeichnung.</param>
        /// <param name="exceptions">Die Ausnahmeregeln zur Aufzeichnung.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Aufzeichnung angegeben.</exception>
        public void Add( IRecordingDefinition definition, params PlanException[] exceptions )
        {
            // Forward
            m_PlanItems.Add( new _Recording( definition, exceptions ) );
        }
    }
}
