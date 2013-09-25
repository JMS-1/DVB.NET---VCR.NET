using System;
using System.Collections.Generic;

using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Simuliert ein Gerät.
    /// </summary>
    internal class ResourceMock : ScheduleResource<ResourceMock, SourceMock>
    {
        /// <summary>
        /// Simuliert Gerät 1 ohne Verschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource Device1 =
            new ResourceMock( "dev1" )
                {
                    Sources =
                        {
                            SourceMock.Source1Group1Free,
                            SourceMock.Source2Group1Free,
                            SourceMock.Source3Group1Free,
                            SourceMock.Source4Group1Pay,
                            SourceMock.Source1Group2Free,
                            SourceMock.Source2Group2Free,
                            SourceMock.Source3Group2Free,
                            SourceMock.Source4Group2Pay,
                            SourceMock.Source1Group3Free,
                        }
                };

        /// <summary>
        /// Simuliert Gerät 2 mit Verschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource Device2 =
            new ResourceMock( "dev2" )
                {
                    Sources =
                        {
                            SourceMock.Source1Group1Free,
                            SourceMock.Source2Group1Free,
                            SourceMock.Source3Group1Free,
                            SourceMock.Source4Group1Pay,
                            SourceMock.Source5Group1Pay,
                            SourceMock.Source1Group2Free,
                            SourceMock.Source2Group2Free,
                            SourceMock.Source3Group2Free,
                            SourceMock.Source4Group2Pay,
                            SourceMock.Source1Group3Free,
                        }
                }.SetEncryptionLimit( 1 );

        /// <summary>
        /// Simuliert Gerät 3 mit Verschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource Device3 =
            new ResourceMock( "dev3" )
            {
                Sources =
                        {
                            SourceMock.Source1Group1Free,
                            SourceMock.Source2Group1Free,
                            SourceMock.Source3Group1Free,
                            SourceMock.Source4Group1Pay,
                            SourceMock.Source1Group2Free,
                            SourceMock.Source2Group2Free,
                            SourceMock.Source3Group2Free,
                            SourceMock.Source4Group2Pay,
                        }
            }.SetEncryptionLimit( 1 );

        /// <summary>
        /// Alle Quellen, die dieses Gerät kennt.
        /// </summary>
        public List<IScheduleSource> Sources = new List<IScheduleSource>();

        /// <summary>
        /// Erzeugt eine neue Simulation.
        /// </summary>
        /// <param name="name">Der Name des simulierten Gerätes.</param>
        private ResourceMock( string name )
        {
            // Remember
            Name = name;
        }

        /// <summary>
        /// Erzeugt einen Anzeigenamen zu Testzwecken.
        /// </summary>
        /// <returns>Der Name des simulierten Gerätes.</returns>
        public override string ToString()
        {
            // Report
            return Name;
        }

        /// <summary>
        /// Legt die maximale Anzahl gleichzeitig zu entschlüsselnder Quellen fest.
        /// </summary>
        /// <param name="number">Die gewünschte Anzahl.</param>
        /// <returns>Diese Instanz.</returns>
        public ResourceMock SetEncryptionLimit( int number )
        {
            // Update
            Decryption = new DecryptionLimits { MaximumParallelSources = number };

            // Make it fluent
            return this;
        }

        /// <summary>
        /// Legt die maximale Anzahl gleichzeitig aufnehmbarer Quellen fest.
        /// </summary>
        /// <param name="limit">Die maximale Anzahl der Quellen.</param>
        /// <returns>Diese Instanz.</returns>
        public ResourceMock SetSourceLimit( int limit )
        {
            // Update
            SourceLimit = limit;

            // Make it fluent
            return this;
        }

        /// <summary>
        /// Legt die Priorität des Gerätes fest.
        /// </summary>
        /// <param name="priority">Die gewünschte Priorität.</param>
        /// <returns>Diese Instanz.</returns>
        public ResourceMock SetPriority( int priority )
        {
            // Update
            AbsolutePriority = priority;

            // Make it fluent
            return this;
        }

        /// <summary>
        /// Erzeugt eine neue Simulation.
        /// </summary>
        /// <param name="name">Der Name des simulierten Gerätes.</param>
        /// <param name="sources">Alle Quellen des simulierten Gerätes.</param>
        /// <returns>Die angeforderte Suimulation.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Name angegeben.</exception>
        public static ResourceMock Create( string name, params SourceMock[] sources )
        {
            // Forward
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );

            // Create
            var resource = new ResourceMock( name );

            // Fill
            if (sources != null)
                resource.Sources.AddRange( sources );

            // Report
            return resource;
        }

        /// <summary>
        /// Prüft, ob eine bestimmte Quelle über dieses Gerät angesprochen werden kann.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Gesetzt, wenn die Quelle angesprochen werden kann.</returns>
        protected override bool TestAccess( SourceMock source )
        {
            // Check by name
            return (Sources.FindIndex( s => string.Equals( ((SourceMock) s).Name, source.Name ) ) >= 0);
        }
    }
}
