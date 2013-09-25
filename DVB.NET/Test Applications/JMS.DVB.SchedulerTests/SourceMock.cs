using System;

using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Simuliert eine Quelle.
    /// </summary>
    internal class SourceMock : IScheduleSource
    {
        /// <summary>
        /// Die erste Quellgruppe.
        /// </summary>
        public static readonly Guid SourceGroup1 = Guid.NewGuid();

        /// <summary>
        /// Die zweite Quellgruppe.
        /// </summary>
        public static readonly Guid SourceGroup2 = Guid.NewGuid();

        /// <summary>
        /// Die dritte Quellgruppe.
        /// </summary>
        public static readonly Guid SourceGroup3 = Guid.NewGuid();

        /// <summary>
        /// Die erste unverschlüsselte Quelle der ersten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source1Group1Free = new SourceMock( "S1.G1", false, SourceGroup1 );

        /// <summary>
        /// Die erste verschlüsselte Quelle der ersten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source1Group1Pay = new SourceMock( "S1.G1", true, SourceGroup1 );

        /// <summary>
        /// Die zweite unverschlüsselte Quelle der ersten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source2Group1Free = new SourceMock( "S2.G1", false, SourceGroup1 );

        /// <summary>
        /// Die dritte unverschlüsselte Quelle der ersten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source3Group1Free = new SourceMock( "S3.G1", false, SourceGroup1 );

        /// <summary>
        /// Die zweite verschlüsselte Quelle der ersten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source4Group1Pay = new SourceMock( "S4.G1", true, SourceGroup1 );

        /// <summary>
        /// Die dritte verschlüsselte Quelle der ersten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source5Group1Pay = new SourceMock( "S5.G1", true, SourceGroup1 );

        /// <summary>
        /// Die erste unverschlüsselte Quelle der zweiten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source1Group2Free = new SourceMock( "S1.G2", false, SourceGroup2 );

        /// <summary>
        /// Die zweite unverschlüsselte Quelle der zweiten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source2Group2Free = new SourceMock( "S2.G2", false, SourceGroup2 );

        /// <summary>
        /// Die dritte unverschlüsselte Quelle der zweiten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source3Group2Free = new SourceMock( "S3.G2", false, SourceGroup2 );

        /// <summary>
        /// Die erste verschlüsselte Quelle der zweiten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source4Group2Pay = new SourceMock( "S4.G2", true, SourceGroup2 );

        /// <summary>
        /// Die erste unverschlüsselte Quelle der dritten Quellgruppe.
        /// </summary>
        public static IScheduleSource Source1Group3Free = new SourceMock( "S1.G3", false, SourceGroup3 );

        /// <summary>
        /// Der Name der simulierten Quelle.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Meldet, ob diese Quelle eine Entschlüsselung benötigt.
        /// </summary>
        public bool IsEncrypted { get; private set; }

        /// <summary>
        /// Meldet, ob es sich bei der Quelle um einen Radiosender handelt.
        /// </summary>
        public bool IsAudioOnly { get; private set; }

        /// <summary>
        /// Die simulierte Quellgruppe.
        /// </summary>
        public Guid SourceGroup { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Simulation.
        /// </summary>
        /// <param name="name">Der Name der simulierten Quelle.</param>
        /// <param name="isEncrypted">Gesetzt, wenn die Quelle entschlüsselt werden muss.</param>
        /// <param name="sourceGroup">Die simulierte Quellgruppe.</param>
        private SourceMock( string name, bool isEncrypted, Guid sourceGroup )
        {
            // Remember
            SourceGroup = sourceGroup;
            IsEncrypted = isEncrypted;
            Name = name;
        }

        /// <summary>
        /// Prüft, ob diese Quelle mit einer anderen parallel aufgezeichnet werden kann.
        /// </summary>
        /// <param name="source">Eine andere Quelle.</param>
        /// <returns>Gesetzt, wenn eine parallele Aufzeichnung theoretisch möglich ist.</returns>
        public bool BelongsToSameSourceGroupAs( IScheduleSource source )
        {
            // Check type first
            var typedSource = source as SourceMock;
            if (typedSource == null)
                return false;
            else
                return (SourceGroup == typedSource.SourceGroup);
        }

        /// <summary>
        /// Erzeugt einen Anzeigenamen zu Testzwecken.
        /// </summary>
        /// <returns>Der Name der simulierten Quelle.</returns>
        public override string ToString()
        {
            // Report
            return string.Format( "{0}@{1:N} {2}{3}", Name, SourceGroup, IsEncrypted ? "Pay" : "Free", IsAudioOnly ? " Radio" : string.Empty );
        }

        /// <summary>
        /// Prüft ob zwei Quellen identisch sind.
        /// </summary>
        /// <param name="source">Eine andere Quelle.</param>
        /// <returns>Gesetzt, wenn die Quellen identisch sind.</returns>
        public bool IsSameAs( IScheduleSource source )
        {
            // Pre-test
            if (!BelongsToSameSourceGroupAs( source ))
                return false;

            // Can now safe cast
            var typedSource = source as SourceMock;

            // Just test the identification
            return Equals( Name, typedSource.Name );
        }

        /// <summary>
        /// Erzeugt eine neue Simulation.
        /// </summary>
        /// <param name="name">Der Name der simulierten Quelle.</param>
        /// <param name="isEncrypted">Gesetzt, wenn die Quelle entschlüsselt werden muss.</param>
        /// <returns>Die angeforderte Suimulation.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Name angegeben.</exception>
        public static SourceMock Create( string name, bool isEncrypted = false )
        {
            // Use private group
            return Create( name, Guid.NewGuid(), isEncrypted );
        }

        /// <summary>
        /// Erzeugt eine neue Simulation.
        /// </summary>
        /// <param name="name">Der Name der simulierten Quelle.</param>
        /// <param name="isEncrypted">Gesetzt, wenn die Quelle entschlüsselt werden muss.</param>
        /// <param name="sourceGroup">Die simulierte Quellgruppe.</param>
        /// <returns>Die angeforderte Suimulation.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Name angegeben.</exception>
        public static SourceMock Create( string name, Guid sourceGroup, bool isEncrypted = false )
        {
            // Forward
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );
            else
                return new SourceMock( name, isEncrypted, sourceGroup );
        }
    }
}
