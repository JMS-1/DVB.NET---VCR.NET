using System;
using System.Collections.Generic;
using System.IO;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Hilfsmethoden zum Erstellen von Vergleichsalgorithmen.
    /// </summary>
    public static class CustomComparer
    {
        /// <summary>
        /// Der Standardvergleichsalgorithmus für Pläne.
        /// </summary>
        /// <param name="nameComparer">Der Algorithmus zum Vergleich von Gerätenamen.</param>
        internal static IComparer<SchedulePlan> Default( IEqualityComparer<string> nameComparer )
        {
            // Forward
            return Create( Properties.Files.DefaultPlanComparer, nameComparer );
        }

        /// <summary>
        /// Kehrt eine Ordnung um.
        /// </summary>
        /// <typeparam name="TEntity">Die Art der zu vergleichenden Klasse.</typeparam>
        private class InverseComparer<TEntity> : IComparer<TEntity>
        {
            /// <summary>
            /// Der eigebtliche Vergleich, dessen Ergebnis umgekehrt werden soll.
            /// </summary>
            private readonly IComparer<TEntity> m_positiveComparer;

            /// <summary>
            /// Erstellt einen neuen Vergleich.
            /// </summary>
            /// <param name="comparer">Der ursprüngliche Vergleichsalgorithmus, dessen Ergebnis umgekehrt werden soll.</param>
            public InverseComparer( IComparer<TEntity> comparer )
            {
                // Validate
                if (comparer == null)
                    throw new ArgumentNullException( "comparer" );

                // Remember
                m_positiveComparer = comparer;
            }

            /// <summary>
            /// Führt einen Vergleich aus.
            /// </summary>
            /// <param name="first">Die erste Instanz.</param>
            /// <param name="second">Die zweite Instanz.</param>
            /// <returns>Das umgekehrte Ergebnis des Originalvergleichs.</returns>
            public int Compare( TEntity first, TEntity second )
            {
                // Forward
                return -m_positiveComparer.Compare( first, second );
            }
        }

        /// <summary>
        /// Kombiniert eine Liste von Vergleichen.
        /// </summary>
        /// <typeparam name="TEntity">Die Art der zu vergleichenden Klasse.</typeparam>
        private class CompoundComparer<TEntity> : IComparer<TEntity>
        {
            /// <summary>
            /// Alle individuellen Vergleiche.
            /// </summary>
            public readonly List<IComparer<TEntity>> Comparers = new List<IComparer<TEntity>>();

            /// <summary>
            /// Erstellt eine neue Liste.
            /// </summary>
            /// <param name="comparers">Die einzelnen Algorithmen.</param>
            public CompoundComparer( IEnumerable<IComparer<TEntity>> comparers = null )
            {
                // Nothing to do
                if (comparers == null)
                    return;

                // Fill in 
                foreach (var comparer in comparers)
                    if (comparer == null)
                        throw new ArgumentNullException( "comparers" );
                    else
                        Comparers.Add( comparer );
            }

            /// <summary>
            /// Führt einen Vergleich aus.
            /// </summary>
            /// <param name="first">Die erste Instanz.</param>
            /// <param name="second">Die zweite Instanz.</param>
            /// <returns>Positiv, wenn die erste Instanz besser passt.</returns>
            public int Compare( TEntity first, TEntity second )
            {
                // Forward
                foreach (var comparer in Comparers)
                {
                    // Process
                    var delta = comparer.Compare( first, second );
                    if (delta != 0)
                        return delta;
                }

                // Same
                return 0;
            }
        }

        /// <summary>
        /// Vergleicht zwei Pläne auf Basis einzelner Geräte.
        /// </summary>
        private class ByResourcePriorityComparer : IComparer<SchedulePlan>
        {
            /// <summary>
            /// Gesetzt, wenn das Gerät mit der höchsten Priorittät zuerst untersucht werden soll.
            /// </summary>
            private readonly bool m_highestFirst;

            /// <summary>
            /// Die Methode zum Vergleichen zweier Geräte.
            /// </summary>
            private readonly IComparer<ResourcePlan> m_resourceComparer;

            /// <summary>
            /// Erstellt einen neuen Vergleich.
            /// </summary>
            /// <param name="descending">Gesetzt, wenn Geräte mit höherer Priorität bevorzugt verwendet werden sollen.</param>
            /// <param name="comparer">Die Vergleichsmethode für einzelne Geräte.</param>
            public ByResourcePriorityComparer( bool descending, IComparer<ResourcePlan> comparer )
            {
                // Validate
                if (comparer == null)
                    throw new ArgumentNullException( "comparer" );

                // Remember
                m_resourceComparer = comparer;
                m_highestFirst = descending;
            }

            /// <summary>
            /// Vergliecht zwei Pläne über die enthaltenen Geräte.
            /// </summary>
            /// <param name="firstPlan">Der erste Plan.</param>
            /// <param name="secondPlan">Der zweite Plan.</param>
            /// <returns>Gesetzt, wenn der erste Plan nach allen Regeln eine bessere Lösung bietet.</returns>
            public int Compare( SchedulePlan firstPlan, SchedulePlan secondPlan )
            {
                // Get resource lists
                var firstResources = firstPlan.Resources;
                var secondResources = secondPlan.Resources;

                // Internal cross check
                if (firstResources.Length != secondResources.Length)
                    throw new InvalidOperationException( "number of resources" );

                // Get bounds
                var index = m_highestFirst ? (firstResources.Length - 1) : 0;
                var step = m_highestFirst ? -1 : +1;

                // Check by resource - starting with the highest priority
                for (var n = firstResources.Length; n-- > 0; index += step)
                {
                    // Process
                    var delta = m_resourceComparer.Compare( firstResources[index], secondResources[index] );
                    if (delta != 0)
                        return delta;
                }

                // No difference
                return 0;
            }
        }

        /// <summary>
        /// Erstellt zu einem Vergleichsalgorithmus eine Umkehrung.
        /// </summary>
        /// <typeparam name="TEntity">Die Art der zu vergleichenden Klasse.</typeparam>
        /// <param name="comparer">Der ursprüngliche Vergleichsalgorithmus.</param>
        /// <returns>Der neue Algorithmus.</returns>
        internal static IComparer<TEntity> Invert<TEntity>( this IComparer<TEntity> comparer )
        {
            // Report
            return new InverseComparer<TEntity>( comparer );
        }

        /// <summary>
        /// Vergleicht zwei Pläne auf Basis einzelner Geräte.
        /// </summary>
        /// <param name="comparer">Die Vergleichsmethode für einzelne Geräte.</param>
        /// <param name="descending">Gesetzt, wenn Geräte mit höherer Priorität bevorzugt verwendet werden sollen.</param>
        internal static IComparer<SchedulePlan> ByPriority( this IComparer<ResourcePlan> comparer, bool descending )
        {
            // Forward
            return new ByResourcePriorityComparer( descending, comparer );
        }

        /// <summary>
        /// Kombiniert eine Liste von Vergleichsmethoden.
        /// </summary>
        /// <typeparam name="TEntity">Die zu vergleichende Klasse.</typeparam>
        /// <param name="comparers">Die Liste der Algorithmen.</param>
        /// <returns>Ein kombinierter Vergleich.</returns>
        internal static IComparer<TEntity> Combine<TEntity>( params IComparer<TEntity>[] comparers )
        {
            // Create
            return new CompoundComparer<TEntity>( comparers );
        }

        /// <summary>
        /// Erstellt einen neuen Vergleichsalgorithmus auf Basis einer Dateibeschreibung.
        /// </summary>
        /// <param name="fileContents">Der tatsächliche Inhalt der Datei.</param>
        /// <param name="nameComparer">Der Algorithmus zum Vegleich von Gerätenamen.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        internal static IComparer<SchedulePlan> Create( byte[] fileContents, IEqualityComparer<string> nameComparer )
        {
            // Validate
            if (fileContents == null)
                throw new ArgumentNullException( "fileContents" );

            // Result
            var resourceComparer = default( CompoundComparer<ResourcePlan> );
            var planComparer = new CompoundComparer<SchedulePlan>();

            // Create reader
            using (var stream = new MemoryStream( fileContents, false ))
            using (var reader = new StreamReader( stream, true ))
                for (string line; (line = reader.ReadLine()) != null; )
                {
                    // Check for comment and empty line
                    var dataLength = line.IndexOf( '#' );
                    var data = line.Substring( 0, (dataLength < 0) ? line.Length : dataLength ).Trim();
                    if (string.IsNullOrEmpty( data ))
                        continue;

                    // Check operation
                    var parts = data.Split( ':' );
                    if (parts.Length != 2)
                        throw new InvalidDataException( string.Format( Properties.SchedulerResources.Exception_BadScheduleFile, data ) );

                    // Check mode
                    if (resourceComparer == null)
                    {
                        // New rule to use
                        var rule = default( IComparer<SchedulePlan> );

                        // Check for supported operations
                        switch (parts[0])
                        {
                            case "StartTime": planComparer.Comparers.Add( SchedulePlan.CompareByOverlappingStart( parts[1], nameComparer ) ); continue;
                            case "ParallelSource": rule = SchedulePlan.CompareByParallelSourceTime; break;
                            case "ResourceCount": rule = SchedulePlan.CompareByResourceCount; break;
                            case "TotalCut": rule = SchedulePlan.CompareByTotalCut; break;
                            case "ByPriority":
                                {
                                    // Create inner
                                    resourceComparer = new CompoundComparer<ResourcePlan>();

                                    // Check mode
                                    switch (parts[1])
                                    {
                                        case "Ascending": rule = resourceComparer.ByPriority( false ); break;
                                        case "Descending": rule = resourceComparer.ByPriority( true ); break;
                                        default: throw new InvalidDataException( string.Format( Properties.SchedulerResources.Exception_UnknownOrder, parts[0], parts[1] ) );
                                    }

                                    // Done
                                    break;
                                }
                        }

                        // Validate
                        if (rule == null)
                            throw new InvalidDataException( string.Format( Properties.SchedulerResources.Exception_UnknownProperty, parts[0] ) );

                        // Process adaption
                        if (resourceComparer == null)
                            switch (parts[1])
                            {
                                case "Min": rule = Invert( rule ); break;
                                case "Max": break;
                                default: throw new InvalidDataException( string.Format( Properties.SchedulerResources.Exception_UnknownOrder, parts[0], parts[1] ) );
                            }

                        // Remember
                        planComparer.Comparers.Add( rule );
                    }
                    else
                    {
                        // New rule to use
                        var rule = default( IComparer<ResourcePlan> );

                        // Check for supported operations
                        switch (parts[0])
                        {
                            case "RecordingCount": rule = ResourcePlan.CompareByRecordingCount; break;
                            case "SourceCount": rule = ResourcePlan.CompareBySourceCount; break;
                            case "ByPriority":
                                {
                                    // Check mode
                                    switch (parts[1])
                                    {
                                        case "End": resourceComparer = null; break;
                                        default: throw new InvalidDataException( Properties.SchedulerResources.Exception_ResourcesNotTerminated );
                                    }

                                    // Done
                                    break;
                                }
                        }

                        // Validate
                        if (rule != null)
                        {
                            // May invert
                            switch (parts[1])
                            {
                                case "Min": rule = Invert( rule ); break;
                                case "Max": break;
                                default: throw new InvalidDataException( string.Format( Properties.SchedulerResources.Exception_UnknownOrder, parts[0], parts[1] ) );
                            }

                            // Remember
                            resourceComparer.Comparers.Add( rule );
                        }
                        else if (resourceComparer != null)
                            throw new InvalidDataException( string.Format( Properties.SchedulerResources.Exception_UnknownProperty, parts[0] ) );
                    }
                }

            // Validate
            if (resourceComparer != null)
                throw new InvalidDataException( Properties.SchedulerResources.Exception_ResourcesNotTerminated );

            // Report
            return planComparer;
        }
    }
}
