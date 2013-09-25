using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    partial class RecordingScheduler
    {
        /// <summary>
        /// Verwaltet eine einzelne Aufgabe.
        /// </summary>
        private class _Task : _Schedule
        {
            /// <summary>
            /// Erzeugt eine neue Verwaltung.
            /// </summary>
            /// <param name="definition">Die zugehörige Beschreibung der Aufgabenzeiten.</param>
            /// <param name="minTime">Der Zeitpunkt, an dem die Aufgabe das nächste Mal ausgeführt werden soll.</param>
            public _Task( IScheduleDefinition definition, DateTime minTime )
                : base( definition )
            {
                // Start at once
                Reset( minTime );

                // Move to the first
                MoveNext();
            }
        }

        /// <summary>
        /// Diese Methode verteilt Aufgaben an Geräte, die eine Zeitlang nicht in Benutzung sind.
        /// </summary>
        /// <param name="resource">Das zu verwendende Gerät.</param>
        /// <param name="startFree">Der Anfang des unbenutzten Bereiches.</param>
        /// <param name="endFree">Das Ende des unbenutzten Bereiches.</param>
        /// <param name="tasks">Die Liste der Aufgaben.</param>
        /// <returns>Alle Aufgaben, die dem Gerät in der unbenutzten Zeit zugeordnet wurden.</returns>
        private IEnumerable<ScheduleInfo> DispatchTasksForResource( IScheduleResource resource, DateTime startFree, DateTime endFree, List<_Task> tasks )
        {
            // See if there is room left to run a task
            while ((tasks.Count > 0) && (endFree > startFree))
            {
                // Load duration
                var duration = endFree - startFree;

                // Best task
                var tiBest = -1;

                // From all tasks fitting choose the one which starts first
                for (var ti = 0; ti < tasks.Count; ti++)
                {
                    // See if resource could be used
                    var task = tasks[ti];
                    if (!task.AllowedResources.Contains( resource, ReferenceComparer<IScheduleResource>.Default ))
                        continue;

                    // Not enough time - skip
                    var planned = task.Current.Planned;
                    if (planned.Duration.Ticks <= 0)
                        continue;
                    if (planned.Duration > duration)
                        continue;

                    // Will not end before resource is reused
                    if (planned.End > endFree)
                        continue;

                    // Check for best
                    if (tiBest < 0)
                        tiBest = ti;
                    else if (planned.Start < tasks[tiBest].Current.Planned.Start)
                        tiBest = ti;
                }

                // Found nothing
                if (tiBest < 0)
                    break;

                // Reload
                var bestTask = tasks[tiBest];
                var bestPlan = bestTask.Current;

                // Check mode
                var startsLate = bestPlan.Planned.Start < startFree;
                if (startsLate)
                    bestPlan.Planned.Start = startFree;

                // Set the new free area
                startFree = bestPlan.Planned.End;

#if !SILVERLIGHT
                // Trace
                if (SchedulerTrace.TraceInfo)
                    Trace.TraceInformation
                        (
                            startsLate ? Properties.SchedulerResources.Trace_LateTask : Properties.SchedulerResources.Trace_NormalTask,
                            resource,
                            bestPlan.Planned.Start,
                            bestPlan.Planned.Duration
                        );
#endif

                // Report
                yield return new ScheduleInfo( bestTask.Definition, resource, bestPlan.Planned, startsLate );

                // Advance
                bestTask.MoveNext();

                // Get rid of it
                if (bestTask.Current == null)
                    tasks.RemoveAt( tiBest );
            }
        }

        /// <summary>
        /// Führt ausstehende Aufgaben aus.
        /// </summary>
        /// <param name="allocationPerResource">Vermerkt, ab wann ein Gerät für Aufgaben verfügbar ist.</param>
        /// <param name="tasks">Die Liste aller noch ausstehenden Aufgaben.</param>
        /// <returns>Alle geplanten Aufgaben.</returns>
        private IEnumerable<ScheduleInfo> DispatchTasks( Dictionary<IScheduleResource, DateTime> allocationPerResource, List<_Task> tasks )
        {
            // Process all remaining resources until no task is exists to use it            
            while ((allocationPerResource.Count > 0) && (tasks.Count > 0))
            {
                // Find the earliest end time and all resources offering it
                var bestEnd = allocationPerResource.Values.Min();
                var resources = new HashSet<IScheduleResource>( allocationPerResource.Where( p => p.Value == bestEnd ).Select( p => p.Key ), allocationPerResource.Comparer );

                // For each task see if at least one is interested in doing something
                for (int ti = 0, timax = tasks.Count; ti < timax; ti++)
                {
                    // Load the task
                    var task = tasks[ti];
                    var resource = task.AllowedResources.FirstOrDefault( r => resources.Contains( r ) );

                    // No of the resources available can be used
                    if (resource == null)
                        continue;

                    // Load the suggested time
                    var time = task.Current;

                    // May move a bit in the future
                    var startsLate = (time.Planned.Start < bestEnd);
                    if (startsLate)
                        time.Planned.Start = bestEnd;

                    // Update the end 
                    allocationPerResource[resource] = time.Planned.End;

#if !SILVERLIGHT
                    // Trace
                    if (SchedulerTrace.TraceInfo)
                        Trace.TraceInformation
                            (
                                startsLate ? Properties.SchedulerResources.Trace_LateTask : Properties.SchedulerResources.Trace_NormalTask,
                                resource,
                                time.Planned.Start,
                                time.Planned.Duration
                            );
#endif

                    // Report
                    yield return new ScheduleInfo( task.Definition, resource, time.Planned, startsLate );

                    // Next
                    task.MoveNext();

                    // Discard
                    if (task.Current == null)
                        tasks.RemoveAt( ti );

                    // At least we did anything so better remove nothing at all
                    resources.Clear();

                    // Do not inspect other tasks
                    break;
                }

                // If we did nothing cleanup resources
                foreach (var resource in resources)
                    allocationPerResource.Remove( resource );
            }
        }

        /// <summary>
        /// Meldet alle Aufzeichnungen ab einem bestimmten Zeitpunkt.
        /// </summary>
        /// <param name="minTime">Alle Aufzeichnungen, die vor diesem Zeitpunkt enden, werden
        /// nicht berücksichtigt. Die Angabe erfolgt in UTC / GMT Notation.</param>
        /// <param name="maintainanceTasks">Alle zusätzlichen Hintergrundaufgaben, die auszuführend sind, wenn
        /// auf einem Geräte keine Aufzeichnungen mehr ausgeführt werden. Ist diese nicht leet, so kann nicht 
        /// garantiert werden, dass die gemeldeten Aufzeichnung gerätübergreifend nach dem Beginn sortiert sind - 
        /// pro Gerät bleibt diese Ordnung immer erhalten.</param>
        /// <returns>Alle Aufzeichnungen.</returns>
        public IEnumerable<IScheduleInformation> GetSchedules( DateTime minTime, params IScheduleDefinition[] maintainanceTasks )
        {
            // Convert tasks
            var tasks =
                (maintainanceTasks ?? Enumerable.Empty<IScheduleDefinition>())
                    .Where( t => !m_ForbiddenDefinitions.Contains( t.UniqueIdentifier ) )
                    .Select( t => new _Task( t, minTime ) )
                    .Where( t => t.Current != null )
                    .Where( s => s.PrepareResourceMap( this ) )
                    .ToList();

            // For all devices in use the last time we reported the end of a recording
            var lastEnds = Resources.ToDictionary( minTime );

            // See if there are any predefined allocations for the resources
            foreach (var resource in m_PlanCreator().Resources)
            {
                // If planned end is above what we think just reserve the time
                var planEnd = resource.PlanEnd;
                if (planEnd > minTime)
                    lastEnds[resource.Resource] = planEnd;
            }

            // Forward
            foreach (var recording in GetSchedulesForRecordings( minTime ))
            {
                // Can only check times if we succeeded in assigning a device
                if (recording.Resource != null)
                {
                    // Get the end of the unused time space
                    var endFree = recording.Time.Start;

                    // Process all tasks
                    foreach (var resource in lastEnds)
                        if (resource.Value < endFree)
                            foreach (var taskSchedule in DispatchTasksForResource( resource.Key, resource.Value, endFree, tasks ))
                                yield return taskSchedule;

                    // Update the allocation information on the device
                    lastEnds[recording.Resource] = recording.Time.End;
                }

                // Report
                yield return recording;
            }

            // Process all remaining resources until no task is exists to use it            
            foreach (var taskSchedule in DispatchTasks( lastEnds, tasks ))
                yield return taskSchedule;
        }
    }
}
