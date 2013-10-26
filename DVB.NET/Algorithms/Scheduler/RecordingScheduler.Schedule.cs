using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    partial class RecordingScheduler
    {
        /// <summary>
        /// Der zu verwendende Vergleichsalgorithmus für Aufzeichnunspläne.
        /// </summary>
        private readonly IComparer<SchedulePlan> m_comparer;

        /// <summary>
        /// Verwaltet eine Liste von Aufzeichnungen.
        /// </summary>
        private class _ScheduleList
        {
            /// <summary>
            /// Alle noch verfügbaren Aufzeichnungen.
            /// </summary>
            private List<_Recording> m_Items;

            /// <summary>
            /// Die laufende Nummer der aktuell betrachteten Aufzeichnung.
            /// </summary>
            private int m_CurrentIndex;

            /// <summary>
            /// Die aktuell betrachtete Aufzeichnung.
            /// </summary>
            public _Recording Current { get; private set; }

            /// <summary>
            /// Erzeugt eine neue Verwaltung.
            /// </summary>
            /// <param name="schedules">Alle bekannten Aufzeichnungen.</param>
            /// <param name="minTime">Der früheste Zeitpunkt für den eine Planung vorgenommen werden soll.</param>
            public _ScheduleList( IEnumerable<_Recording> schedules, DateTime minTime )
            {
                // Process
                m_Items =
                    schedules
                        .Select( schedule =>
                            {
                                // Set scope
                                schedule.Reset( minTime );

                                // Move to first
                                schedule.MoveNext();

                                // Report
                                return schedule;
                            } )
                        .Where( item => item.Current != null )
                        .ToList();
            }

            /// <summary>
            /// Ermittelt den nächsten zu bearbeitenden Eintrag.
            /// </summary>
            /// <returns>Gesetzt, wenn ein solcher Eintrag existiert.</returns>
            public bool MoveNext()
            {
                // Advance the recording we reported on the previous iteration
                if (Current != null)
                {
                    // Advance
                    Current.MoveNext();

                    // Did it all
                    if (Current.Current == null)
                        m_Items.RemoveAt( m_CurrentIndex );

                    // Reset
                    Current = null;
                }

                // We are done
                if (m_Items.Count < 1)
                    return false;

                // Reset
                m_CurrentIndex = 0;

                // Locate the earliest schedule
                for (int i = 1, imax = m_Items.Count; i < imax; i++)
                    if (m_Items[i].Current.Planned.Start < m_Items[m_CurrentIndex].Current.Planned.Start)
                        m_CurrentIndex = i;

                // Remember
                Current = m_Items[m_CurrentIndex];

                // Report
                return true;
            }
        }

        /// <summary>
        /// Meldet alle Aufzeichnungen ab einem bestimmten Zeitpunkt.
        /// </summary>
        /// <param name="minTime">Alle Aufzeichnungen, die vor diesem Zeitpunkt enden, werden
        /// nicht berücksichtigt. Die Angabe erfolgt in UTC / GMT Notation.</param>
        /// <returns>Alle Aufzeichnungen.</returns>
        private IEnumerable<ScheduleInfo> GetSchedulesForRecordings( DateTime minTime )
        {
            // All items to process
            var items = new _ScheduleList( m_PlanItems.Where( r => !m_ForbiddenDefinitions.Contains( r.Definition.UniqueIdentifier ) ), minTime );

            // Create the plans to extend
            var plans = new List<SchedulePlan> { m_PlanCreator() };
            var steps = 0;

            // As long as necessary
            while (items.MoveNext())
            {
                // Load the item
                var candidate = items.Current;
                var candiateTime = candidate.Current;
                var planned = candiateTime.Planned;

#if !SILVERLIGHT
                // Report
                if (SchedulerTrace.TraceVerbose)
                    Trace.TraceInformation( Properties.SchedulerResources.Trace_Candidate, candidate.Definition.Source, planned.Start, planned.Duration );
#endif

                // Get the current end of plans and see if we can dump the state - this may increase performance
                var planStart = plans.SelectMany( p => p.Resources ).Min( r => (DateTime?) r.PlanStart );
                var planEnd = plans.SelectMany( p => p.Resources ).Max( r => (DateTime?) r.PlanEnd );
                var canEndPlan = planEnd.HasValue && (planEnd.Value != DateTime.MinValue) && (planned.Start >= planEnd.Value);
                var mustEndPlan = planStart.HasValue && (planStart.Value != DateTime.MaxValue) && planEnd.HasValue && (planEnd.Value != DateTime.MinValue) && ((planEnd.Value - planStart.Value).TotalDays > 2);

                // Count this effort
                if ((++steps > MaximumRecordingsInPlan) || canEndPlan || mustEndPlan || (plans.Count > MaximumAlternativesInPlan))
                {
                    // Find best plan
                    var best = SchedulePlan.FindBest( plans, m_comparer );

                    // Report
                    foreach (var info in Dump( best ))
                        yield return info;

                    // Reset
                    plans.Clear();
                    plans.Add( best.Restart( planned.Start ) );

                    // Reset
                    steps = 1;
                }

#if !SILVERLIGHT
                // Report
                if (SchedulerTrace.TraceVerbose)
                    Trace.TraceInformation( Properties.SchedulerResources.Trace_PlanCount, plans.Count );
#endif

                // All plans to extend
                var allPlans = plans.ToArray();

                // Discard list
                plans.Clear();

                // We collect all plans forcing to start the recording late separatly
                var latePlans = new List<SchedulePlan>();

                // Iterate over all plans and try to add the current candidate - in worst case this will multiply possible plans by the number of resources available
                foreach (var plan in allPlans)
                    for (int i = plan.Resources.Length; i-- > 0; )
                    {
                        // Clone of plan must be recreated for each resource because test is allowed to modify it
                        var clone = plan.Clone();

                        // Remember the time we tried
                        SuggestedPlannedTime plannedTime = planned;

                        // See if resource can handle this
                        if (clone.Resources[i].Add( candidate.Definition, plannedTime, minTime ))
                            if (plannedTime.Planned.Start == planned.Start)
                            {
                                // Add to the full plan
                                plans.Add( clone );

                                // Ignore all late plans to follow
                                latePlans = null;
                            }
                            else if (latePlans != null)
                            {
                                // Remember in case we do not find a plan which starts in time
                                latePlans.Add( clone );
                            }
                    }

                // Merge
                if (latePlans != null)
                    plans.AddRange( latePlans );

                // Must reset if the recording could not be scheduled at all
                if (plans.Count < 1)
                {
                    // Report
                    yield return new ScheduleInfo( candidate.Definition, null, planned, false );

                    // Restore the original plans since we did nothing at all
                    plans.AddRange( allPlans );
                }
            }

            // Send all we found 
            foreach (var info in Dump( SchedulePlan.FindBest( plans, m_comparer ) ))
                yield return info;
        }

        /// <summary>
        /// Gibt einen Plan aus.
        /// </summary>
        /// <param name="plan">Der zu verwendende Plan.</param>
        /// <returns>Alle Aufzeichnungen, geordnet erst nach Zeit und dann nach der Priorität des Gerätes.</returns>
        private IEnumerable<ScheduleInfo> Dump( SchedulePlan plan )
        {
            // Skip
            if (plan == null)
                yield break;

#if !SILVERLIGHT
            // Dump decryption allocation
            if (SchedulerTrace.TraceVerbose)
                foreach (var decryption in plan.DecryptionCounters.Values)
                    if (decryption.IsEnabled)
                        Trace.TraceInformation( Properties.SchedulerResources.Trace_Decryption, decryption );
#endif

            // Artifical dump
            foreach (var info in plan.GetRecordings())
                if (!m_ForbiddenDefinitions.Contains( info.Definition.UniqueIdentifier ))
                {
#if !SILVERLIGHT
                    // Trace
                    if (SchedulerTrace.TraceInfo)
                    {
                        // Check type
                        var definition = info.Definition as IRecordingDefinition;

                        // Report
                        Trace.TraceInformation
                            (
                                info.StartsLate ? Properties.SchedulerResources.Trace_LateItem : Properties.SchedulerResources.Trace_NormalItem,
                                info.Resource,
                                (definition == null) ? null : definition.Source,
                                info.Time.Start,
                                info.Time.Duration
                            );
                    }
#endif

                    // Report
                    yield return info;
                }
        }
    }
}
