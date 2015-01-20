using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft den Umgang mit der Entschlüsselung von Quellen.
    /// </summary>
    [TestClass]
    public class DecryptionSpecs
    {
        /// <summary>
        /// Das in diesem Test verwendete erste Gerät mit Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource PayTVDevice1 = ResourceMock.Device2;

        /// <summary>
        /// Das in diesem Test verwendete zweite Gerät mit Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource PayTVDevice2 = ResourceMock.Device3;

        /// <summary>
        /// Das in diesem Test verwendete erste Gerät ohne Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource FreeTVDevice = ResourceMock.Device1;

        /// <summary>
        /// Der Bezugspunkt für alle Zeitmessungen.
        /// </summary>
        public static readonly DateTime TimeBias = new DateTime( 2011, 9, 9, 22, 44, 59, DateTimeKind.Utc );

        /// <summary>
        /// Eine verschlüsselte Aufzeichnung wird überprungen, wenn sie nicht entschlüsselt werden kann.
        /// </summary>
        [TestMethod]
        public void Will_Skip_Recording_If_Unable_To_Decrypt()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test1", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 100 ) );
            var plan2 = RecordingDefinition.Create( false, "test2", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 100 ) );

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { FreeTVDevice, plan1, plan2 };

            // Process
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.IsNull( schedules[0].Resource, "Resource 1" );
            Assert.AreSame( FreeTVDevice, schedules[1].Resource, "Resource 2" );
            Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
        }

        /// <summary>
        /// Verschlüsselte Aufzeichnungen werden auf zwei Geräte verteilt, selbst wenn
        /// sie von der selben Quellgruppe stammen.
        /// </summary>
        [TestMethod]
        public void Two_Decrypted_Recordings_On_Same_Group_Require_Two_Decryption_Devices()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 100 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 10 ) );

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { PayTVDevice1, PayTVDevice2, plan1, plan2 };

            // Process
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.IsNotNull( schedules[0].Resource, "Resource 1" );
            Assert.IsNotNull( schedules[1].Resource, "Resource 2" );
            Assert.AreNotSame( schedules[0].Resource, schedules[1].Resource, "Resources" );
        }

        /// <summary>
        /// Ein höher priorisiertes Geräte wird nicht verwendet, wenn es keine Entschlüsselung unterstützt.
        /// </summary>
        [TestMethod]
        public void High_Priority_Device_Will_Be_Ignored_If_Not_Able_To_Decrypt()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 100 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 10 ) );

            // Attach to the free device
            var device = (ResourceMock) FreeTVDevice;
            var prio = device.AbsolutePriority;

            // With reset
            try
            {
                // Create component under test
                var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { device.SetPriority( 100 ), PayTVDevice1, PayTVDevice2, plan1, plan2 };

                // Process
                var schedules = cut.GetSchedules( TimeBias ).ToArray();

                // Validate
                Assert.AreEqual( 2, schedules.Length, "Schedules" );
                Assert.IsNotNull( schedules[0].Resource, "Resource 1" );
                Assert.AreNotSame( device, schedules[0].Resource, "Resource 1 Free" );
                Assert.IsNotNull( schedules[1].Resource, "Resource 2" );
                Assert.AreNotSame( device, schedules[1].Resource, "Resource 2 Free" );
                Assert.AreNotSame( schedules[0].Resource, schedules[1].Resource, "Resources" );
            }
            finally
            {
                // Back to normal
                device.SetPriority( prio );
            }
        }

        /// <summary>
        /// Eine Verschlüsselungsgruppe kann verhindert, dass zwei Entschlüsselungen auf verschiedenen Geräten
        /// gleichzeitig abgearbeitet werden.
        /// </summary>
        [TestMethod]
        public void Decryption_Group_May_Forbid_Using_Two_Resources_At_The_Same_Time()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 100 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 100 ) );

            // Create group
            var group =
                new DecryptionGroup
                    {
                        ScheduleResources = new[] { PayTVDevice1, PayTVDevice2 },
                        MaximumParallelSources = 1,
                    };

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { PayTVDevice1, PayTVDevice2, plan1, plan2, group };

            // Process
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsTrue( schedules[1].StartsLate, "Late 2" );
        }

        /// <summary>
        /// Jede entschlüsselte Quelle wird zu jedem Zeitpunkt nur einmal berücksichtigt.
        /// </summary>
        [TestMethod]
        public void Same_Source_Does_Not_Require_Additional_Decyrption_Slot()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 100 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 100 ) );

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { PayTVDevice1, plan1, plan2 };

            // Process
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
        }

        /// <summary>
        /// Eine Verschlüsselungsgruppe kann erlauben, dass zwei Entschlüsselungen auf verschiedenen Geräten
        /// gleichzeitig abgearbeitet werden.
        /// </summary>
        [TestMethod]
        public void Decryption_Group_May_Allow_Using_Two_Resources_At_The_Same_Time()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 100 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 100 ) );

            // Create group
            var group =
                new DecryptionGroup
                {
                    ScheduleResources = new[] { PayTVDevice1, PayTVDevice2 },
                    MaximumParallelSources = 2,
                };

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { PayTVDevice1, PayTVDevice2, plan1, plan2, group };

            // Process
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
        }

        /// <summary>
        /// Bei unglücklicher Wahl der Aufzeichnungen müssen drei Geräte verwendet werden.
        /// </summary>
        [TestMethod]
        public void May_Require_Three_Resources_On_Bad_Mixture_Of_Three_Recordings()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 100 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 100 ) );
            var plan3 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours( 2 ).AddMinutes( 10 ), TimeSpan.FromMinutes( 100 ) );

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { FreeTVDevice, PayTVDevice1, PayTVDevice2, plan1, plan2, plan3 };

            // Process
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 3, schedules.Length, "Schedules" );
            Assert.AreNotSame( FreeTVDevice, schedules[0].Resource, "Resource 1" );
            Assert.AreNotSame( FreeTVDevice, schedules[1].Resource, "Resource 2" );
            Assert.AreSame( FreeTVDevice, schedules[2].Resource, "Resource 3" );
        }

        /// <summary>
        /// Sind Geräte durch Entschlüsselungen unterschiedlich blockiert, so muss eine folgende freie
        /// Aufzeichnung auf dem Gerät mit dem kleinsten Zeitverlust aufgezeichnet werdenm egal, welche 
        /// Priorität den Geräten zugeordnet ist.
        /// </summary>
        [TestMethod]
        public void Free_Recording_Must_Use_Best_Fit_Resource()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source5Group1Pay, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 100 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 100 ) );
            var plan3 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours( 2 ).AddMinutes( 10 ), TimeSpan.FromMinutes( 100 ) );

            // Attach to the device
            var device = (ResourceMock) PayTVDevice1;
            var prio = device.AbsolutePriority;

            // Must reset
            try
            {
                // Create component under test but make the device to choose the one with the least priority
                var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { device.SetPriority( -100 ), PayTVDevice2, plan1, plan2, plan3 };

                // Process
                var schedules = cut.GetSchedules( TimeBias ).ToArray();

                // Validate
                Assert.AreEqual( 3, schedules.Length, "Schedules" );
                Assert.AreSame( device, schedules[2].Resource, "Resource" );
            }
            finally
            {
                // Reset
                device.SetPriority( prio );
            }
        }

        /// <summary>
        /// Eine Aufzeichnungsplanung kommt durcheinander, wenn eine Aufzeichnung gestartet wird.
        /// </summary>
        [TestMethod]
        public void Bad_Planning_After_Recording_Start()
        {
            // Create component under test
            using (var rm = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                var now = DateTime.Now.Date.AddDays( 10 );

                var source1 = SourceMock.Create( "s1" );
                var source2 = SourceMock.Create( "s2" );
                var source3 = SourceMock.Create( "s3" );

                var dev1 = ResourceMock.Create( "dev1", source1, source2, source3 );
                var dev2 = ResourceMock.Create( "dev2", source1, source2, source3 );

                var id1 = Guid.NewGuid();
                var start1 = now.AddHours( 11 ).AddMinutes( 40 );
                var dur1 = TimeSpan.FromMinutes( 15 );

                var plan1 = RecordingDefinition.Create( false, "test1", id1, null, source1, start1, dur1 );
                var plan2 = RecordingDefinition.Create( false, "test2", Guid.NewGuid(), null, source2, now.AddHours( 11 ).AddMinutes( 45 ), TimeSpan.FromMinutes( 15 ) );
                var plan3 = RecordingDefinition.Create( false, "test3", Guid.NewGuid(), null, source3, now.AddHours( 11 ).AddMinutes( 50 ), TimeSpan.FromMinutes( 15 ) );

                rm.Add( dev1 );
                rm.Add( dev2 );

                Assert.IsTrue( rm.Start( dev2, source1, id1, "test1", start1, start1 + dur1 ) );

                var cut = rm.CreateScheduler( false );
                cut.Add( plan1 );
                cut.Add( plan2 );
                cut.Add( plan3 );

                var schedules = cut.GetSchedules( start1.AddMinutes( 5 ).AddTicks( 1 ) ).Where(s => s.Definition.UniqueIdentifier != id1).ToArray();

                Assert.AreEqual( 2, schedules.Length, "#schedules" );
                Assert.AreEqual( "test2", schedules[0].Definition.Name, "1" );
                Assert.AreEqual( "test3", schedules[1].Definition.Name, "2" );
            }
        }
    }
}
