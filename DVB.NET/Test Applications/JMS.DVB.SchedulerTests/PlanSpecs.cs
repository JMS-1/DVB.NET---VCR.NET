using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft das Erzeugen von Aufzeichnungsinformationen.
    /// </summary>
    [TestClass]
    public class PlanSpecs
    {
        /// <summary>
        /// Es ist nicht möglich, eine Aufzeichnung ohne Quelle anzulegen. 
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentNullException ) )]
        public void A_Plan_Item_Must_Have_A_Source()
        {
            // Process
            RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, null, DateTime.UtcNow, TimeSpan.FromMinutes( 10 ) );
        }

        /// <summary>
        /// Es kann keine Aufzeichnung vor dem Jahr 2011 geplant werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentOutOfRangeException ) )]
        public void A_Plan_Item_Must_Not_Be_Older_Than_2000()
        {
            // Process
            RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1" ), new DateTime( 1999, 2, 13, 0, 0, 0, DateTimeKind.Utc ), TimeSpan.FromMinutes( 10 ) );
        }

        /// <summary>
        /// Die Dauer einer Aufzeichnung darf nicht <i>0</i> sein.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentOutOfRangeException ) )]
        public void Plan_Item_Duration_Must_Not_Be_Zero()
        {
            // Process
            RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1" ), DateTime.UtcNow, TimeSpan.Zero );
        }

        /// <summary>
        /// Die Dauer einer Aufzeichnung darf nicht negativ sein.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentOutOfRangeException ) )]
        public void Plan_Item_Duration_Must_Not_Be_Negative()
        {
            // Process
            RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1" ), DateTime.UtcNow, new TimeSpan( -1 ) );
        }

        /// <summary>
        /// Die Daten einer Aufzeichnung können wieder abgefragt werden.
        /// </summary>
        [TestMethod]
        public void Plan_Item_Remembers_Settings()
        {
            // Data under test
            var duration = TimeSpan.FromMinutes( 45 );
            var source = SourceMock.Create( "s1" );
            var start = DateTime.UtcNow;

            // Process
            var componentUnderTest = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, source, start, duration );

            // Validate
            Assert.IsNotNull( componentUnderTest, "Create" );
            Assert.AreSame( source, componentUnderTest.Source, "Source" );

            // Load plan
            var times = componentUnderTest.GetTimes( DateTime.MinValue ).Select( s => s.Planned ).ToArray();

            // Validate
            Assert.AreEqual( 1, times.Length, "Times" );
            Assert.AreEqual( start, times[0].Start, "Start" );
            Assert.AreEqual( duration, times[0].Duration, "Duration" );
        }

        /// <summary>
        /// Es ist nicht möglich, eine <i>null</i> Aufzeichnung anzumelden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentNullException ) )]
        public void Can_Not_Add_A_Null_Plan_Item()
        {
            // Create component under test
            var componentUnderTest = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase );

            // Add
            componentUnderTest.Add( default( IRecordingDefinition ) );
        }

        /// <summary>
        /// Die Quelle einer Aufzeichnung muss mindestens von einem Gerät unterstützt
        /// werden.
        /// </summary>
        [TestMethod]
        public void Encrypted_Source_Of_A_Plan_Item_Is_Supported_By_Resource()
        {
            // Create component under test
            var componentUnderTest = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase );

            // Add
            componentUnderTest.Add( ResourceMock.Create( "r1", SourceMock.Create( "s1" ) ).SetEncryptionLimit( 1 ) );
            componentUnderTest.Add( RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1", true ), DateTime.UtcNow, TimeSpan.FromMinutes( 12 ) ) );
        }

        /// <summary>
        /// Die Zeitangaben von Ausnahmeregeln dürfen keine Uhrzeit enthalten.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void Exceptions_Must_Be_Defined_On_A_Full_Date()
        {
            // Create bad exception
            var exception = new PlanException { ExceptionDate = DateTime.Now.Date.AddMinutes( 12 ) };

            // Create component under test
            var componentUnderTest = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase );

            // Add
            componentUnderTest.Add( ResourceMock.Create( "r1", SourceMock.Create( "s1" ) ) );
            componentUnderTest.Add( RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1" ), DateTime.UtcNow, TimeSpan.FromMinutes( 12 ) ), exception );
        }

        /// <summary>
        /// Ausnahmeregeln dürfen pro Tag nur einmal definiert werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void There_Can_Be_Only_One_Exception_Per_Date()
        {
            // Create bad exception
            var exception1 = new PlanException { ExceptionDate = DateTime.Now.Date };
            var exception2 = new PlanException { ExceptionDate = exception1.ExceptionDate };

            // Create component under test
            var componentUnderTest = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase );

            // Add
            componentUnderTest.Add( ResourceMock.Create( "r1", SourceMock.Create( "s1" ) ) );
            componentUnderTest.Add( RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1" ), DateTime.UtcNow, TimeSpan.FromMinutes( 12 ) ), exception1, exception2 );
        }

        /// <summary>
        /// Das Ende einer sich wiederholenden Aufzeichnung muss als Datum angegeben werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void End_Of_Repeating_Recording_Must_Be_A_Date()
        {
            // Process
            RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "a" ), DateTime.UtcNow, TimeSpan.FromMinutes( 10 ), DateTime.Now.Date.AddMinutes( 1 ), DayOfWeek.Monday );
        }

        /// <summary>
        /// Das Ende einer sich wiederholenden Aufzeichnung muss nach dem Beginn liegen.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void End_Of_Repeating_Recording_Must_Be_In_The_Future()
        {
            // Process
            RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "a" ), DateTime.UtcNow, TimeSpan.FromMinutes( 10 ), DateTime.Now.Date.AddYears( -1 ), DayOfWeek.Monday );
        }

        /// <summary>
        /// Das Wiederholdungsmuster muss mindestens einen Wochentag enthalten.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void Repeat_Must_Have_At_Least_One_Day()
        {
            // Process
            RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "a" ), DateTime.UtcNow, TimeSpan.FromMinutes( 10 ), DateTime.Now.Date.AddYears( 1 ) );
        }

        /// <summary>
        /// Das Wiederholdungsmuster darf jeden Wochentag nur einmal enthalten.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void Repeat_Must_Not_Use_A_Day_Twice()
        {
            // Process
            RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "a" ), DateTime.UtcNow, TimeSpan.FromMinutes( 10 ), DateTime.Now.Date.AddYears( 1 ), DayOfWeek.Monday, DayOfWeek.Saturday, DayOfWeek.Monday );
        }

        /// <summary>
        /// Eine Aufzeichnung kann sich wiederholen.
        /// </summary>
        [TestMethod]
        public void A_Plan_Can_Have_A_Repeating_Pattern()
        {
            // Create the plan
            var localStart = new DateTime( 2011, 9, 8, 21, 35, 0, DateTimeKind.Local );
            var localEnd = new DateTime( 2012, 3, 31, 0, 0, 0, DateTimeKind.Local );
            var gmtStart = localStart.ToUniversalTime();
            var plan = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1" ), gmtStart, TimeSpan.FromMinutes( 10 ), localEnd.Date, DayOfWeek.Monday, DayOfWeek.Friday, DayOfWeek.Tuesday );

            // Cross check map
            var allowed = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Friday, DayOfWeek.Tuesday };

            // Scan all
            foreach (var time in plan.GetTimes( gmtStart.AddYears( -1 ) ).Select( s => s.Planned ))
            {
                // Get next good time
                while (!allowed.Contains( localStart.DayOfWeek ))
                    localStart = localStart.AddDays( 1 );

                // Test it
                Assert.AreEqual( localStart, time.LocalStart );
                Assert.AreEqual( TimeSpan.FromMinutes( 10 ), time.Duration );

                // Next day
                localStart = localStart.AddDays( 1 );
            }

            // Check end
            Assert.AreEqual( localEnd + localStart.TimeOfDay, localStart );
        }

        /// <summary>
        /// Für eine Aufzeichnung kann festgelegt werden, dass sie nur auf bestimmten Geräten ausgeführt werden kann.
        /// </summary>
        [TestMethod]
        public void A_Plan_Can_Have_A_List_Of_Preferred_Resources()
        {
            // Create environment
            var source = SourceMock.Create( "A" );
            var device1 = ResourceMock.Create( "D1", source );
            var device2 = ResourceMock.Create( "D2", source );
            var device3 = ResourceMock.Create( "D3", source );

            // Create the plan
            var plan = RecordingDefinition.Create( false, "test", Guid.NewGuid(), new[] { device2 }, source, DateTime.UtcNow, TimeSpan.FromMinutes( 10 ) );

            // Create the component under test
            var cut = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase ) { device1, device3, plan };

            // Get the schedule
            var schedule = cut.GetSchedules( DateTime.UtcNow.AddYears( -1 ) ).Single();

            // Validate
            Assert.IsNull( schedule.Resource, "Resource" );
        }
    }
}
