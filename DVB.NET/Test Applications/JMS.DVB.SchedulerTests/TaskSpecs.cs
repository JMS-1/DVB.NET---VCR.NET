using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft das Einmischen periodischer Aufgaben.
    /// </summary>
    [TestClass]
    public class TaskSpecs
    {
        /// <summary>
        /// Simuliert eine periodische Aufgabe.
        /// </summary>
        private class _TaskMock : PeriodicScheduler
        {
            /// <summary>
            /// Alle Geräte, die diese Aufgabe ausführen können.
            /// </summary>
            private IScheduleResource[] m_Resources;

            /// <summary>
            /// Erzeugt eine neue Simulation.
            /// </summary>
            /// <param name="resource">Das zu verwendende Gerät.</param>
            public _TaskMock( IScheduleResource resource )
                : base( "task", Guid.NewGuid() )
            {
                // Remember
                m_Resources = new[] { resource };
            }

            /// <summary>
            /// Meldet alle Geräte, die verwendet werden dürfen, um die Aufgabe zu erledigen.
            /// </summary>
            public override IScheduleResource[] Resources
            {
                get
                {
                    // Report
                    return m_Resources;
                }
            }
            /// <summary>
            /// Meldet die Zeitspanne zwischen zwei Läufen.
            /// </summary>
            public override TimeSpan DefaultInterval
            {
                get
                {
                    // Not set
                    return new TimeSpan( 1 );
                }
            }

            /// <summary>
            /// Meldet die maximale Dauer eines Laufs.
            /// </summary>
            public override TimeSpan Duration
            {
                get
                {
                    // Report
                    return TimeSpan.FromMinutes( 20 );
                }
            }

            /// <summary>
            /// Meldet, ob eine Bearbeitung erwünscht ist.
            /// </summary>
            public override bool IsEnabled
            {
                get
                {
                    // Make no sense if not
                    return true;
                }
            }

            /// <summary>
            /// Meldet, ob eine vorzeigtig Aktualisierung unterstützt wird.
            /// </summary>
            public override TimeSpan? JoinThreshold
            {
                get
                {
                    // Nope
                    return null;
                }
            }

            /// <summary>
            /// Meldet den Zeitpunkt der letzten Ausführung.
            /// </summary>
            public override DateTime? LastRun
            {
                get
                {
                    // Not needed
                    return null;
                }
            }

            /// <summary>
            /// Meldet die Stunden, an denen eine Aktualisierung erlaubt ist. Die Angabe bezieht
            /// sich auf Zeiten in der lokalen Zeitzone.
            /// </summary>
            public override uint[] PreferredHours
            {
                get
                {
                    // Report
                    return new uint[] { 10, 20 };
                }
            }
        }

        /// <summary>
        /// Das Gerät, an das die Aufgabe gebunden wird.
        /// </summary>
        private readonly IScheduleResource TaskDevice = ResourceMock.Device1;

        /// <summary>
        /// Ein anderes Gerät.
        /// </summary>
        private readonly IScheduleResource OtherDevice = ResourceMock.Device2;

        /// <summary>
        /// Die vordefinierte Aufgabe, die jeden Tag bevorzug um 10 Uhr Morgens und 8 Uhr Abends für
        /// 20 Minuten ausgeführt wird.
        /// </summary>
        private readonly IScheduleDefinition Task;

        /// <summary>
        /// Die für diesen Test verwendete Uhrzeit.
        /// </summary>
        private static readonly DateTime TimeBias = new DateTime( 2011, 9, 11, 15, 51, 13, DateTimeKind.Utc );

        /// <summary>
        /// Erzeugt eine neue Testumgebung.
        /// </summary>
        public TaskSpecs()
        {
            // Finish
            Task = new _TaskMock( TaskDevice );
        }

        /// <summary>
        /// Solange keine Aufzeichnungen aktiv sind meldet die Planung nur die Zeiten 
        /// der periodischen Aufgaben.
        /// </summary>
        [TestMethod]
        public void Scheduler_Reports_Task_Times_If_No_Recording_Is_Available()
        {
            // Create the component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { TaskDevice, OtherDevice };

            // Load some
            var schedules = cut.GetSchedules( TimeBias, Task ).Take( 100 ).ToArray();

            // Validate
            Assert.AreEqual( 100, schedules.Length, "Schedules" );
            Assert.AreEqual( TimeBias, schedules[0].Time.Start, "Start 0" );
            Assert.AreEqual( TimeBias.ToLocalTime().Date.AddDays( 49 ).AddHours( 20 ).ToUniversalTime(), schedules[99].Time.Start, "Start 99" );
        }

        /// <summary>
        /// Eine Aufzeichnung wird immer höher bewertet als eine Aufgabe.
        /// </summary>
        [TestMethod]
        public void Recording_Has_Priority_Over_Task()
        {
            // Create the recording
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddMinutes( 15 ), TimeSpan.FromMinutes( 80 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 3 ), TimeSpan.FromMinutes( 100 ) );

            // Create the component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { TaskDevice, plan1, plan2 };

            // Load some
            var schedules = cut.GetSchedules( TimeBias, Task ).Take( 5 ).ToArray();

            // Validate
            Assert.AreEqual( 5, schedules.Length, "Schedules" );
            Assert.AreSame( plan1, schedules[0].Definition, "Definition 1" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.AreSame( Task, schedules[1].Definition, "Definition 2" );
            Assert.IsTrue( schedules[1].StartsLate, "Late 2" );
            Assert.AreSame( Task, schedules[2].Definition, "Definition 3" );
            Assert.IsFalse( schedules[2].StartsLate, "Late 3" );
            Assert.AreEqual( TimeBias.ToLocalTime().Date.AddHours( 20 ).ToUniversalTime(), schedules[2].Time.Start, "Start 3" );
            Assert.AreSame( plan2, schedules[3].Definition, "Definition 4" );
            Assert.IsFalse( schedules[3].StartsLate, "Late 4" );
            Assert.AreSame( Task, schedules[4].Definition, "Definition 5" );
            Assert.IsFalse( schedules[4].StartsLate, "Late 5" );
            Assert.AreEqual( TimeBias.ToLocalTime().Date.AddDays( 1 ).AddHours( 10 ).ToUniversalTime(), schedules[4].Time.Start, "Start 5" );
        }
    }
}
