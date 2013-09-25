using System;
using System.Linq;
using JMS.DVB.Algorithms.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Arbeitet mit einem einzelnen Gerät.
    /// </summary>
    [TestClass]
    public class SingleDeviceSpecs
    {
        /// <summary>
        /// Das in diesem Test verwendete Gerät ohne Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource FreeTVDevice = ResourceMock.Device1;

        /// <summary>
        /// Das in diesem Test verwendete Gerät mit Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource PayTVDevice = ResourceMock.Device2;

        /// <summary>
        /// Der Bezugspunkt für alle Zeitmessungen.
        /// </summary>
        public static readonly DateTime TimeBias = new DateTime( 2011, 9, 7, 22, 28, 13, DateTimeKind.Utc );

        /// <summary>
        /// Ein verschlüsselter Sender kann nur auf einem Gerät mit Entschlüsselung ausgeführt werden.
        /// </summary>
        [TestMethod]
        public void Decrypted_Source_Requires_Decryption_Resource()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test1", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "test2", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 90 ) );

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { FreeTVDevice, plan1, plan2 };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedule" );
            Assert.IsNull( schedules[0].Resource, "Resource 1" );
            Assert.AreSame( FreeTVDevice, schedules[1].Resource, "Resource 2" );
        }

        /// <summary>
        /// Ein verschlüsselter Sender kann nur auf einem Gerät mit Entschlüsselung ausgeführt werden.
        /// </summary>
        [TestMethod]
        public void Decrypted_Source_Will_Run_On_Decryption_Device()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 90 ) );
            var plan3 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours( 3 ), TimeSpan.FromMinutes( 90 ) );

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { PayTVDevice, plan1, plan2, plan3 };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 3, schedules.Length, "Schedule" );
            Assert.AreSame( PayTVDevice, schedules[0].Resource, "Resource 1" );
            Assert.AreSame( PayTVDevice, schedules[1].Resource, "Resource 2" );
            Assert.AreSame( PayTVDevice, schedules[2].Resource, "Resource 3" );
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf die selbe Quellgruppe beziehen, können simulan ausgeführt 
        /// werden.
        /// </summary>
        [TestMethod]
        public void Two_Recordings_Of_The_Same_Group_Can_Be_Recorded_Simultanously()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 60 ) );

            // Create component under test
            var cut =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    {
                        { FreeTVDevice },
                        { plan1 },
                        { plan2 },
                    };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.AreSame( FreeTVDevice, schedules[0].Resource, "Resource 1" );
            Assert.AreSame( FreeTVDevice, schedules[1].Resource, "Resource 2" );
            Assert.AreSame( plan1, schedules[0].Definition, "Plan 1" );
            Assert.AreSame( plan2, schedules[1].Definition, "Plan 2" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
        }

        /// <summary>
        /// Drei Aufzeichnungen, die sich auf die selbe Quellgruppe beziehen, können simulan ausgeführt 
        /// werden.
        /// </summary>
        [TestMethod]
        public void Unrestricted_Resource_Can_Serve_Three_Records_At_A_Time()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 70 ) );
            var plan3 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source3Group1Free, TimeBias.AddHours( 3 ), TimeSpan.FromMinutes( 90 ) );

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { FreeTVDevice, plan1, plan2, plan3 };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 3, schedules.Length, "Schedules" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
            Assert.IsFalse( schedules[2].StartsLate, "Late 3" );
        }

        /// <summary>
        /// Drei Aufzeichnungen, die sich auf die selbe Quellgruppe beziehen, können simulan ausgeführt 
        /// werden.
        /// </summary>
        [TestMethod]
        public void Restricted_Resource_Can_Not_Serve_Three_Records_At_A_Time()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 70 ) );
            var plan3 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source3Group1Free, TimeBias.AddHours( 2.25 ), TimeSpan.FromMinutes( 90 ) );

            // Load current
            var device = (ResourceMock) FreeTVDevice;
            var limit = device.SourceLimit;
            try
            {
                // Create component under test
                var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { device.SetSourceLimit( 2 ), plan1, plan2, plan3 };

                // Resolve
                var schedules = cut.GetSchedules( TimeBias ).ToArray();

                // Validate
                Assert.AreEqual( 3, schedules.Length, "Schedules" );
                Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
                Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
                Assert.IsTrue( schedules[2].StartsLate, "Late 3" );
            }
            finally
            {
                // Reset
                device.SetSourceLimit( limit );
            }
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf die selbe Quellgruppe beziehen, können hintereinander ausgeführt 
        /// werden.
        /// </summary>
        [TestMethod]
        public void Two_Recordings_Of_The_Same_Group_Can_Be_Recorded_After_Each_Other()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours( 3 ), TimeSpan.FromMinutes( 60 ) );

            // Create component under test
            var cut =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    {
                        { FreeTVDevice },
                        { plan2 },
                        { plan1 },
                    };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.AreSame( FreeTVDevice, schedules[0].Resource, "Resource 1" );
            Assert.AreSame( FreeTVDevice, schedules[1].Resource, "Resource 2" );
            Assert.AreSame( plan1, schedules[0].Definition, "Plan 1" );
            Assert.AreSame( plan2, schedules[1].Definition, "Plan 2" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf unterschiedliche Quellgruppen beziehen, können hintereinander ausgeführt 
        /// werden.
        /// </summary>
        [TestMethod]
        public void Two_Recordings_Of_Different_Groups_Can_Be_Recorded_After_Each_Other()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours( 3 ), TimeSpan.FromMinutes( 60 ) );

            // Create component under test
            var cut =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    {
                        { FreeTVDevice },
                        { plan2 },
                        { plan1 },
                    };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.AreSame( plan1, schedules[0].Definition, "Plan 1" );
            Assert.AreSame( plan2, schedules[1].Definition, "Plan 2" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
        }

        /// <summary>
        /// Aufzeichnungen auf einer Quelle werden kombiniert und zählen nur einmal.
        /// </summary>
        [TestMethod]
        public void Will_Detect_Recordings_On_Same_Source_As_One()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "testA", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddMinutes( 60 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "testB", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddMinutes( 90 ), TimeSpan.FromMinutes( 40 ) );
            var plan3 = RecordingDefinition.Create( false, "testC", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddMinutes( 100 ), TimeSpan.FromMinutes( 40 ) );

            // Load current
            var device = (ResourceMock) FreeTVDevice;
            var limit = device.SourceLimit;
            try
            {
                // Create component under test
                var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { device.SetSourceLimit( 2 ), plan1, plan2, plan3 };

                // Resolve
                var schedules = cut.GetSchedules( TimeBias ).ToArray();

                // Validate
                Assert.AreEqual( 3, schedules.Length, "Schedules" );
                Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
                Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
                Assert.IsFalse( schedules[2].StartsLate, "Late 3" );
            }
            finally
            {
                // Reset
                device.SetSourceLimit( limit );
            }
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf unterschiedliche Quellgruppen beziehen, können nicht simultan ausgeführt 
        /// werden.
        /// </summary>
        [TestMethod]
        public void A_Recording_May_Start_Late_If_Overlapping_Occurs_On_Different_Groups()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 60 ) );

            // Create component under test
            var cut =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    {
                        { FreeTVDevice },
                        { plan2 },
                        { plan1 },
                    };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.AreSame( FreeTVDevice, schedules[0].Resource, "Resource 1" );
            Assert.AreSame( FreeTVDevice, schedules[1].Resource, "Resource 2" );
            Assert.AreSame( plan1, schedules[0].Definition, "Plan 1" );
            Assert.AreSame( plan2, schedules[1].Definition, "Plan 2" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsTrue( schedules[1].StartsLate, "Late 2" );
            Assert.AreEqual( schedules[1].Time.Start, TimeBias.AddHours( 2.5 ), "Start" );
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf unterschiedliche Quellgruppen beziehen, können nicht simultan ausgeführt 
        /// werden.
        /// </summary>
        [TestMethod]
        public void A_Recording_May_Be_Discarded_If_Overlapping_Occurs_On_Different_Groups()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "test1", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );
            var plan2 = RecordingDefinition.Create( false, "test2", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours( 2 ), TimeSpan.FromMinutes( 10 ) );

            // Create component under test
            var cut =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    {
                        { FreeTVDevice },
                        { plan2 },
                        { plan1 },
                    };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.IsNull( schedules[0].Resource, "Resource 1" );
            Assert.AreSame( FreeTVDevice, schedules[1].Resource, "Resource 2" );
            Assert.AreSame( plan2, schedules[0].Definition, "Plan 1" );
            Assert.AreSame( plan1, schedules[1].Definition, "Plan 2" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsFalse( schedules[1].StartsLate, "Late 2" );
        }

        /// <summary>
        /// Eine Quelle, die dem Gerät nicht bekannt ist, kann nicht aufgezeichnet werden.
        /// </summary>
        [TestMethod]
        public void An_Unknown_Source_Can_Not_Be_Recorded()
        {
            // Create recordings
            var plan = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "unknown" ), TimeBias.AddHours( 1 ), TimeSpan.FromMinutes( 90 ) );

            // Create component under test
            var cut =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    {
                        { FreeTVDevice },
                        { plan },
                    };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 1, schedules.Length, "Schedules" );
            Assert.AreSame( null, schedules[0].Resource, "Resource" );
            Assert.AreSame( plan, schedules[0].Definition, "Plan" );
        }

        /// <summary>
        /// Die Planung der Aufzeichnungen wird nach einer festen Anzahl von Einträgen
        /// als Sollbruchstelle unterbrochen.
        /// </summary>
        [TestMethod]
        public void The_Number_Of_Recordings_Per_Plan_Is_Limited()
        {
            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { FreeTVDevice };

            // Add all plans
            for (int i = 0; i <= RecordingScheduler.MaximumRecordingsInPlan; i++)
            {
                // Create recording
                var plan = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours( i ), TimeSpan.FromMinutes( 90 ) );

                // Add it
                cut.Add( plan );
            }

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( RecordingScheduler.MaximumRecordingsInPlan + 1, (uint) schedules.Length, "Schedules" );

            // Check all
            for (int i = 0; i < RecordingScheduler.MaximumRecordingsInPlan; i++)
            {
                // Load
                var schedule = schedules[i];

                // Validate
                Assert.AreSame( FreeTVDevice, schedule.Resource, "Resource {0}", i );
                Assert.AreEqual( TimeBias.AddHours( i ), schedule.Time.Start, "Start {0}", i );
                Assert.AreEqual( TimeSpan.FromMinutes( 90 ), schedule.Time.Duration, "Duration {0}", i );
                Assert.IsFalse( schedule.StartsLate, "Late {0}", i );
            }

            // Load the last
            var last = schedules[RecordingScheduler.MaximumRecordingsInPlan];

            // Validate - internal planning is not touched
            Assert.AreSame( FreeTVDevice, last.Resource, "Resource" );
            Assert.AreEqual( TimeBias.AddHours( RecordingScheduler.MaximumRecordingsInPlan ), last.Time.Start, "Start" );
            Assert.AreEqual( TimeSpan.FromMinutes( 90 ), last.Time.Duration, "Duration" );
            Assert.IsFalse( last.StartsLate, "Late" );
        }

        /// <summary>
        /// Sich wiederholende und einzelne Aufzeichnungen können überlappen.
        /// </summary>
        [TestMethod]
        public void A_Repeating_Recording_May_Be_Overlapped_By_A_Single_Recording()
        {
            // Create recordings
            var repeatStart = TimeBias.AddHours( 1 );
            var repeatStartLocal = repeatStart.ToLocalTime();
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, repeatStart, TimeSpan.FromMinutes( 90 ), DateTime.MaxValue.Date, repeatStartLocal.DayOfWeek );
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, repeatStart.AddDays( 7 ).AddMinutes( -10 ), TimeSpan.FromMinutes( 60 ) );

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { FreeTVDevice, plan2, plan1, };

            // Load first repeats
            var schedules = cut.GetSchedules( TimeBias ).Take( 3 ).ToArray();

            // Validate
            Assert.AreEqual( 3, schedules.Length, "Schedules" );
            Assert.AreSame( plan1, schedules[0].Definition, "Plan 1" );
            Assert.AreSame( FreeTVDevice, schedules[0].Resource, "Resource 1" );
            Assert.AreSame( plan2, schedules[1].Definition, "Plan 2" );
            Assert.AreSame( FreeTVDevice, schedules[1].Resource, "Resource 2" );
            Assert.AreEqual( repeatStart.AddDays( 7 ).AddMinutes( -10 ), schedules[1].Time.Start, "Start 2" );
            Assert.AreEqual( TimeSpan.FromMinutes( 60 ), schedules[1].Time.Duration, "Duration 2" );
            Assert.AreSame( plan1, schedules[2].Definition, "Plan 3" );
            Assert.AreSame( FreeTVDevice, schedules[2].Resource, "Resource 3" );
            Assert.IsTrue( schedules[2].StartsLate, "Late" );
            Assert.AreEqual( repeatStart.AddDays( 7 ).AddMinutes( 50 ), schedules[2].Time.Start, "Start 3" );
            Assert.AreEqual( TimeSpan.FromMinutes( 40 ), schedules[2].Time.Duration, "Duration 3" );
        }

        /// <summary>
        /// Auf eine sich wiederholende Aufzeichnung können Ausnahmen angewendet werden.
        /// </summary>
        [TestMethod]
        public void A_Repeating_Recording_Can_Have_Exceptions()
        {
            // Create recordings
            var repeatStart = TimeBias.AddHours( 1 );
            var repeatStartLocal = repeatStart.ToLocalTime();
            var plan =
                RecordingDefinition.Create
                    (
                        false,
                        "test",
                        Guid.NewGuid(),
                        null,
                        SourceMock.Source1Group1Free,
                        repeatStart,
                        TimeSpan.FromMinutes( 90 ),
                        DateTime.MaxValue.Date,
                        Enum.GetValues( typeof( DayOfWeek ) ).Cast<DayOfWeek>().ToArray()
                    );
            var exception1 =
                new PlanException
                    {
                        ExceptionDate = repeatStartLocal.AddDays( 12 ).Date,
                        DurationDelta = TimeSpan.FromMinutes( -10 ),
                        StartDelta = TimeSpan.FromMinutes( 10 ),
                    };
            var exception2 =
                new PlanException
                {
                    ExceptionDate = repeatStartLocal.AddDays( 22 ).Date,
                    DurationDelta = TimeSpan.FromMinutes( 10 ),
                };
            var exception3 =
                new PlanException
                {
                    ExceptionDate = repeatStartLocal.AddDays( 24 ).Date,
                    DurationDelta = TimeSpan.FromMinutes( -100 ),
                };

            // Create component under test
            var cut =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    { 
                        { FreeTVDevice }, 
                        { plan, exception1, exception2, exception3 }, 
                    };

            // Load 
            var schedules = cut.GetSchedules( TimeBias ).Take( 30 ).ToArray();

            // Check
            for (int i = 30; i-- > 0; )
                if (i == 12)
                {
                    // Validate
                    Assert.AreEqual( schedules[i].Time.Start, repeatStart.AddDays( i ).AddMinutes( 10 ), "Start {0}", i );
                    Assert.AreEqual( schedules[i].Time.Duration, TimeSpan.FromMinutes( 80 ), "Duration {0}", i );
                }
                else if (i == 22)
                {
                    // Validate
                    Assert.AreEqual( schedules[i].Time.Start, repeatStart.AddDays( i ), "Start {0}", i );
                    Assert.AreEqual( schedules[i].Time.Duration, TimeSpan.FromMinutes( 100 ), "Duration {0}", i );
                }
                else
                {
                    // Correct
                    var day = (i < 24) ? i : i + 1;

                    // Validate
                    Assert.AreEqual( schedules[i].Time.Start, repeatStart.AddDays( day ), "Start {0}", i );
                    Assert.AreEqual( schedules[i].Time.Duration, TimeSpan.FromMinutes( 90 ), "Duration {0}", i );
                }
        }

        /// <summary>
        /// Die zeitliche Ordnung der Aufzeichnungen wird bei der Planung berücksichtigt.
        /// </summary>
        [TestMethod]
        public void Will_Keep_Time_Order_When_Planning()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create( false, "testA", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddMinutes( 60 ), TimeSpan.FromMinutes( 60 ) );
            var plan2 = RecordingDefinition.Create( false, "testB", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddMinutes( 90 ), TimeSpan.FromMinutes( 60 ) );
            var plan3 = RecordingDefinition.Create( false, "testC", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddMinutes( 100 ), TimeSpan.FromMinutes( 100 ) );

            // Create component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { FreeTVDevice, plan1, plan2, plan3 };

            // Resolve
            var schedules = cut.GetSchedules( TimeBias ).ToArray();

            // Validate
            Assert.AreEqual( 3, schedules.Length, "Schedules" );
            Assert.AreEqual( "testA", schedules[0].Definition.Name, "Name 1" );
            Assert.AreEqual( "testB", schedules[1].Definition.Name, "Name 2" );
            Assert.AreEqual( "testC", schedules[2].Definition.Name, "Name 3" );
            Assert.AreSame( FreeTVDevice, schedules[0].Resource, "Resource 1" );
            Assert.AreSame( FreeTVDevice, schedules[1].Resource, "Resource 2" );
            Assert.AreSame( FreeTVDevice, schedules[2].Resource, "Resource 3" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 1" );
            Assert.IsTrue( schedules[1].StartsLate, "Late 2" );
            Assert.IsTrue( schedules[2].StartsLate, "Late 3" );
        }
    }
}
