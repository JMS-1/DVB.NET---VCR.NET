using System;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Diese Klasse übernimmt die Planung von Aufzeichnungen.
    /// </summary>
    public partial class RecordingScheduler : IEnumerable
    {
#if !SILVERLIGHT
        /// <summary>
        /// Erlaubt eine Überwachung der internen Abläufe.
        /// </summary>
        public static readonly TraceSwitch SchedulerTrace = new TraceSwitch( Properties.SchedulerResources.Trace_DisplayName, Properties.SchedulerResources.Trace_Description );
#endif

        /// <summary>
        /// Die maximale Anzahl von Aufzeichnungen, die in einem Rutsch geplant werden.
        /// </summary>
        public static uint MaximumRecordingsInPlan = 1000;

        /// <summary>
        /// Die maximale Anzahl gleichzeitig untersucher Alternativlösungen.
        /// </summary>
        public static uint MaximumAlternativesInPlan = 1000;

        /// <summary>
        /// Alle Geräte, die bei der Planung zu berücksichtigen sind.
        /// </summary>
        internal ResourceCollection Resources { get; private set; }

        /// <summary>
        /// Alle Aufzeichungen und Aufgaben, die bei der Planung nicht berücksichtigt werden sollen.
        /// </summary>
        private HashSet<Guid> m_ForbiddenDefinitions;

        /// <summary>
        /// Methode zur Erzeugung des initialen Ablaufplans.
        /// </summary>
        private Func<SchedulePlan> m_PlanCreator;

        /// <summary>
        /// Erzeugt eine neue Planungsinstanz.
        /// </summary>
        /// <param name="resources">Die zu verwendenden Geräte.</param>
        /// <param name="forbiddenDefinitions">Alle Aufzeichnungen und Aufgaben, die nicht untersucht werden sollen.</param>
        /// <param name="planCreator">Optional eine Methode zur Erzeugung des initialen Ablaufplans.</param>
        /// <param name="comparer">Der volle Pfad zur Regeldatei.</param>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        internal RecordingScheduler( ResourceCollection resources, HashSet<Guid> forbiddenDefinitions, Func<SchedulePlan> planCreator, IComparer<SchedulePlan> comparer )
        {
            // Validate
            if (resources == null)
                throw new ArgumentNullException( "resources" );
            if (forbiddenDefinitions == null)
                throw new ArgumentNullException( "forbiddenDefinitions" );
            if (comparer == null)
                throw new ArgumentNullException( "comparer" );

            // Finish
            m_PlanCreator = planCreator ?? (Func<SchedulePlan>) (() => new SchedulePlan( Resources ));
            m_ForbiddenDefinitions = forbiddenDefinitions;
            m_comparer = comparer;
            Resources = resources;
        }

        /// <summary>
        /// Erzeugt eine neue Planungsinstanz.
        /// </summary>
        /// <param name="nameComparer">Der Algorithmus zum Vergleich von Gerätenamen.</param>
        /// <param name="rulePath">Der volle Pfad zur Regeldatei.</param>
        public RecordingScheduler( IEqualityComparer<string> nameComparer, string rulePath = null )
            : this( nameComparer, string.IsNullOrEmpty( rulePath ) ? null : File.ReadAllBytes( rulePath ) )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Planungsinstanz.
        /// </summary>
        /// <param name="nameComparer">Der Algorithmus zum Vergleich von Gerätenamen.</param>
        /// <param name="rules">Der Inhalt der Regeldatei.</param>
        public RecordingScheduler( IEqualityComparer<string> nameComparer, byte[] rules )
            : this( new ResourceCollection(), new HashSet<Guid>(), null, (rules == null) ? CustomComparer.Default( nameComparer ) : CustomComparer.Create( rules, nameComparer ) )
        {
        }

        /// <summary>
        /// Meldet ein Gerät zur Verwendung an.
        /// </summary>
        /// <param name="resource">Das gewünschte Gerät.</param>
        public void Add( IScheduleResource resource )
        {
            // Blind forward
            Resources.Add( resource );
        }

        /// <summary>
        /// Meldet komplete Entschlüsselungsregeln an.
        /// </summary>
        /// <param name="group">Eine neue Regel.</param>
        /// <exception cref="ArgumentNullException">Die Regel ist ungültig.</exception>
        public void Add( DecryptionGroup group )
        {
            // Forward
            Resources.Add( group );
        }

        #region IEnumerable Members

        /// <summary>
        /// Simuliert eine Auflistung.
        /// </summary>
        /// <returns>Die gewünschte Simulation.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // None
            return Enumerable.Empty<object>().GetEnumerator();
        }

        #endregion
    }

#if SILVERLIGHT
    /// <summary>
    /// Hilfsmethodem im Falle der Silverlight Variante.
    /// </summary>
    internal static class SilverlightExtensions
    {
        /// <summary>
        /// Ermittelt die Position eines Elementes in einer Liste.
        /// </summary>
        /// <typeparam name="T">Die Art der Elemente.</typeparam>
        /// <param name="list">Die Liste.</param>
        /// <param name="startIndex">Der erste zu untersuchende Eintrag.</param>
        /// <param name="predicate">Die zu verwendende Prüfmethode.</param>
        /// <returns>Die 0-basierte laufende Nummer des ersten Elementes, das der Prüfmethode
        /// genügt oder <i>-1</i>, wenn kein solches existiert.</returns>
        public static int FindIndex<T>( this List<T> list, int startIndex, Predicate<T> predicate )
        {
            // Validate
            if (list == null)
                throw new ArgumentNullException( "list" );
            if (predicate == null)
                throw new ArgumentNullException( "predicate" );
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException( "startIndex" );
            if (startIndex > list.Count)
                throw new ArgumentOutOfRangeException( "startIndex" );

            // Process
            for (int i = startIndex; i < list.Count; i++)
                if (predicate( list[i] ))
                    return i;

            // Not found
            return -1;
        }

        /// <summary>
        /// Ermittelt die Position eines Elementes in einer Liste.
        /// </summary>
        /// <typeparam name="T">Die Art der Elemente.</typeparam>
        /// <param name="list">Die Liste.</param>
        /// <param name="predicate">Die zu verwendende Prüfmethode.</param>
        /// <returns>Die 0-basierte laufende Nummer des ersten Elementes, das der Prüfmethode
        /// genügt oder <i>-1</i>, wenn kein solches existiert.</returns>
        public static int FindIndex<T>( this List<T> list, Predicate<T> predicate )
        {
            // Forward
            return list.FindIndex( 0, predicate );
        }
    }
#endif
}
