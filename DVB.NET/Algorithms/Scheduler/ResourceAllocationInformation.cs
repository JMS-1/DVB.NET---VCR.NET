using System;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt eine zugeteilte Aufzeichnung.
    /// </summary>
    internal class ResourceAllocationInformation : IResourceAllocationInformation
    {
        /// <summary>
        /// Meldet das zu verwendende Gerät.
        /// </summary>
        public IScheduleResource Resource
        {
            get
            {
                // The one and only
                return Resources[0];
            }
        }

        /// <summary>
        /// Optional die Quelle, die aufgezeichnet werden soll.
        /// </summary>
        public IScheduleSource Source { get; private set; }

        /// <summary>
        /// Eine eindeutige Identifikation der Aktion.
        /// </summary>
        public Guid UniqueIdentifier { get; private set; }

        /// <summary>
        /// Ein Anzeigename zur Identifikation der Aktion.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Die geplante Laufzeit.
        /// </summary>
        public PlannedTime Time { get; private set; }

        /// <summary>
        /// Optional eine Liste von Geräten, die für eine Aufzeichnung erlaubt sind.
        /// </summary>
        public IScheduleResource[] Resources { get; private set; }

        /// <summary>
        /// Meldet alle geplanten Aufzeichnungszeiten.
        /// </summary>
        /// <param name="minTime">Aufzeichnunge, die vor diesem Zeitpunkt enden, brauchen nicht in
        /// der Auflistung zu erscheinen. Es handelt sich um eine optionale Optimierung.</param>
        /// <returns>Eine Liste aller geplanten Zeiten.</returns>
        IEnumerable<SuggestedPlannedTime> IScheduleDefinition.GetTimes( DateTime minTime )
        {
            // Static
            throw new NotImplementedException( "IRecordingDefinition.Resources" );
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="resource">Das zugehörige Gerät.</param>
        /// <param name="source">Optional die zu verwendende Quelle.</param>
        /// <param name="identifier">Die eindeutige Kennung der zugehörigen Aufzeichnung oder Aufgabe.</param>
        /// <param name="name">Der Name der Aufzeichnung.</param>
        /// <param name="start">Der Startzeitpunkt.</param>
        /// <param name="duration">Die Dauer der Aufzeichnung.</param>
        /// <exception cref="ArgumentOutOfRangeException">Die Dauer der Aufzeichnung ist negativ.</exception>
        public ResourceAllocationInformation( IScheduleResource resource, IScheduleSource source, Guid identifier, string name, DateTime start, TimeSpan duration )
        {
            // Validate
            if (duration.TotalSeconds <= 0)
                throw new ArgumentOutOfRangeException( "duration" );

            // Remember all
            Time = new PlannedTime { Start = start, Duration = duration };
            Resources = new[] { resource };
            UniqueIdentifier = identifier;
            Source = source;
            Name = name;
        }
    }
}
