using System;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Hilfsklasse zur Implementierung der <see cref="IScheduleResource"/> Schnittstelle.
    /// </summary>
    /// <typeparam name="ResourceType">Die konkrete Implementierung.</typeparam>
    /// <typeparam name="SourceType">Die Art der verwendeten Quellen.</typeparam>
    public abstract class ScheduleResource<ResourceType, SourceType> : IScheduleResource
        where ResourceType : class, IScheduleResource
        where SourceType : class, IScheduleSource
    {
        /// <summary>
        /// Meldet die absolute Prioriät des Gerätes. Größere Werte veranlassen die Lastverteilung, das
        /// Gerät bevorzugt zu verwenden.
        /// </summary>
        public int AbsolutePriority { get; protected set; }

        /// <summary>
        /// Beschreibt die Grenzwerte der Entschlüsselung für dieses individuelle Gerät.
        /// </summary>
        public DecryptionLimits Decryption { get; protected set; }

        /// <summary>
        /// Die maximale Anzahl unterschiedlicher Quellen, die von diesem Geräte gleichzeitig
        /// bearbeitet werden können.
        /// </summary>
        public int SourceLimit { get; protected set; }

        /// <summary>
        /// Meldet den Namen der Ressource.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Erzeugt eine neue Implementierung.
        /// </summary>
        protected ScheduleResource()
        {
        }

        /// <summary>
        /// Prüft, ob eine bestimmte Quelle über dieses Gerät angesprochen werden kann.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Gesetzt, wenn die Quelle angesprochen werden kann.</returns>
        protected abstract bool TestAccess( SourceType source );

        /// <summary>
        /// Prüft, ob eine bestimmte Quelle über dieses Gerät angesprochen werden kann.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Gesetzt, wenn die Quelle angesprochen werden kann.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        /// <exception cref="ArgumentException">Die Quelle passt nicht zu dieser Art von Gerät.</exception>
        public bool CanAccess( IScheduleSource source )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );

            // Check type
            var typedSource = source as SourceType;
            if (typedSource == null)
                throw new ArgumentException( source.GetType().FullName, "source" );

            // Report
            return TestAccess( typedSource );
        }
    }
}
