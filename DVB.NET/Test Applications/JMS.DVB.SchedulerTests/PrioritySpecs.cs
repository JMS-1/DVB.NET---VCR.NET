using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft die Priorisierung von Geräten.
    /// </summary>
    [TestClass]
    public class PrioritySpecs
    {
        /// <summary>
        /// Gibt es zwei Geräte, die eine Quelle unterstützen, so wird die mit der höheren Priorität
        /// gewählt, wenn nur eine Aufzeichnugen ansteht.
        /// </summary>
        [TestMethod]
        public void Will_Choose_Highest_Priority_Source_For_Single_Plan_Item()
        {
            // Create plan
            var plan = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1" ), DateTime.UtcNow.AddHours( 1 ), TimeSpan.FromMinutes( 20 ) );
            var best = ResourceMock.Create( "r3", SourceMock.Create( "s1" ) ).SetPriority( 6 );
            var times = plan.GetTimes( DateTime.UtcNow ).Select( s => s.Planned ).ToArray();

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    {
                        { ResourceMock.Create( "r1", SourceMock.Create( "s1" ) ).SetPriority( 1 ) },
                        { best },
                        { ResourceMock.Create( "r2", SourceMock.Create( "s1" ) ).SetPriority( -1 ) },
                        { plan },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules( DateTime.UtcNow ).ToArray();

            // Validate
            Assert.AreEqual( 1, schedules.Length, "Schedules" );
            Assert.AreSame( best, schedules[0].Resource, "Resource" );
            Assert.AreSame( plan, schedules[0].Definition, "Plan" );
            Assert.AreEqual( times.Single(), schedules[0].Time, "Time" );
            Assert.IsFalse( schedules[0].StartsLate, "Late" );
        }

        /// <summary>
        /// Auch bei expliziter Bindung an ein Gerät werden direkt nachfolgende Aufzeichnungen auf dem Gerät mit der
        /// höchsten Priorität ausgeführt.
        /// </summary>
        [TestMethod]
        public void Will_Use_Resource_With_Highest_Priority_When_Explicit_Binding_Is_Used()
        {
            // Create plan
            var source1 = SourceMock.Create( "s1" );
            var source2 = SourceMock.Create( "s2" );
            var res1 = ResourceMock.Create( "r1", source1, source2 ).SetPriority( 5 );
            var res2 = ResourceMock.Create( "r2", source1, source2 ).SetPriority( 6 );
            var res3 = ResourceMock.Create( "r3", source1, source2 ).SetPriority( 0 );
            var refTime = DateTime.UtcNow;
            var plan1 = RecordingDefinition.Create( false, "A1", Guid.NewGuid(), new[] { res1 }, source1, refTime.AddMinutes( 100 ), TimeSpan.FromMinutes( 20 ) );
            var plan2 = RecordingDefinition.Create( false, "A2", Guid.NewGuid(), new[] { res1 }, source1, refTime.AddMinutes( 110 ), TimeSpan.FromMinutes( 20 ) );
            var plan3 = RecordingDefinition.Create( false, "A3", Guid.NewGuid(), null, source2, refTime.AddMinutes( 130 ), TimeSpan.FromMinutes( 20 ) );

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    {
                        { res1 },
                        { res2 },
                        { res3 },
                        { plan1 },
                        { plan2 },
                        { plan3 },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules( refTime ).ToArray();

            // Validate
            Assert.AreEqual( 3, schedules.Length, "#schedule" );
            Assert.AreSame( res1, schedules[0].Resource, "resource 0" );
            Assert.AreSame( res1, schedules[1].Resource, "resource 1" );
            Assert.AreSame( res2, schedules[2].Resource, "resource 2" );
        }

        /// <summary>
        /// Gibt es zwei Geräte, die eine Quelle unterstützen, so wird die mit der höheren Priorität
        /// gewählt, wenn zwei überlappende Aufzeichnugen anstehen.
        /// </summary>
        [TestMethod]
        public void Will_Choose_Highest_Priority_Source_For_Two_Overlapping_Plan_Items()
        {
            // Create plan
            var group = Guid.NewGuid();
            var plan1 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1", group ), DateTime.UtcNow.AddHours( 1 ), TimeSpan.FromMinutes( 20 ) );
            var times1 = plan1.GetTimes( DateTime.UtcNow ).Select( s => s.Planned ).Single();
            var plan2 = RecordingDefinition.Create( false, "test", Guid.NewGuid(), null, SourceMock.Create( "s1", group ), times1.End - TimeSpan.FromMinutes( 10 ), TimeSpan.FromMinutes( 20 ) );
            var times2 = plan2.GetTimes( DateTime.UtcNow ).Select( s => s.Planned ).Single();
            var best = ResourceMock.Create( "r3", SourceMock.Create( "s1", group ) ).SetPriority( 6 );

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase )
                    {   
                        { ResourceMock.Create( "r1", SourceMock.Create( "s1", group ) ).SetPriority( 1 ) },
                        { best },   
                        { ResourceMock.Create( "r2", SourceMock.Create( "s1", group ) ).SetPriority( -1 ) },
                        { plan2 },
                        { plan1 },
                };

            // Load
            var schedules = componentUnderTest.GetSchedules( DateTime.UtcNow ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "Schedules" );
            Assert.AreSame( best, schedules[0].Resource, "Resource 0" );
            Assert.AreSame( best, schedules[1].Resource, "Resource 1" );
            Assert.AreSame( plan1, schedules[0].Definition, "Plan 0" );
            Assert.AreSame( plan2, schedules[1].Definition, "Plan 1" );
            Assert.AreEqual( times1, schedules[0].Time, "Time 0" );
            Assert.AreEqual( times2, schedules[1].Time, "Time 1" );
            Assert.IsFalse( schedules[0].StartsLate, "Late 0" );
            Assert.IsFalse( schedules[1].StartsLate, "Late 1" );
        }

        /// <summary>
        /// Es wird versucht den Zeitraum, bei dem eine Quelle auf mehreren Resourcen gleichzeitig
        /// aufgeszeichnet wird, zu minimieren.
        /// </summary>
        [TestMethod]
        public void Will_Avoid_Source_On_Multiple_Resources()
        {
            // Create plan
            var source1 = SourceMock.Create( "s1" );
            var source2 = SourceMock.Create( "s2" );
            var res1 = ResourceMock.Create( "r1", source1, source2 ).SetPriority( 1 );
            var res2 = ResourceMock.Create( "r2", source1, source2 ).SetPriority( 2 );
            var refTime = DateTime.UtcNow.Date.AddDays( 10 );
            var plan1 = RecordingDefinition.Create( false, "A1", Guid.NewGuid(), null, source1, refTime.AddHours( 18 ), TimeSpan.FromHours( 2 ) );
            var plan2 = RecordingDefinition.Create( false, "A2", Guid.NewGuid(), null, source2, refTime.AddHours( 19 ), TimeSpan.FromHours( 4 ) );
            var plan3 = RecordingDefinition.Create( false, "A3", Guid.NewGuid(), null, source2, refTime.AddHours( 21 ), TimeSpan.FromHours( 2 ) );

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase, Properties.Files.AvoidSourcesOnMultipledResources )
                    {
                        { res1 },
                        { res2 },
                        { plan1 },
                        { plan2 },
                        { plan3 },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules( refTime ).ToArray();

            // Validate
            Assert.AreEqual( 3, schedules.Length, "#schedules" );
            Assert.AreSame( plan1, schedules[0].Definition, "plan 1" );
            Assert.AreSame( plan2, schedules[1].Definition, "plan 2" );
            Assert.AreSame( plan3, schedules[2].Definition, "plan 3" );
            Assert.AreSame( res2, schedules[0].Resource, "resource 1" );
            Assert.AreSame( res1, schedules[1].Resource, "resource 2" );
            Assert.AreSame( res1, schedules[2].Resource, "resource 3" );
        }

        /// <summary>
        /// Es ist möglich, die Priorität durch ein Startverbot zu überschreiben.
        /// </summary>
        [TestMethod]
        public void Will_Enforce_Start_Order()
        {
            // Create plan
            var source1 = SourceMock.Create( "s1" );
            var source2 = SourceMock.Create( "s2" );
            var res1 = ResourceMock.Create( "r1", source1, source2 ).SetPriority( 1 );
            var res2 = ResourceMock.Create( "r2", source1, source2 ).SetPriority( 2 );
            var refTime = DateTime.UtcNow.Date.AddDays( 10 );
            var plan1 = RecordingDefinition.Create( false, "A1", Guid.NewGuid(), null, source1, refTime.AddHours( 18 ), TimeSpan.FromHours( 2 ) );
            var plan2 = RecordingDefinition.Create( false, "A2", Guid.NewGuid(), null, source2, refTime.AddHours( 19 ), TimeSpan.FromHours( 2 ) );

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase, Properties.Files.EnforceResourceStartOrder )
                    {
                        { res1 },
                        { res2 },
                        { plan1 },
                        { plan2 },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules( refTime ).ToArray();

            // Validate
            Assert.AreEqual( 2, schedules.Length, "#schedules" );
            Assert.AreSame( plan1, schedules[0].Definition, "plan 1" );
            Assert.AreSame( plan2, schedules[1].Definition, "plan 2" );
            Assert.AreSame( res1, schedules[0].Resource, "resource 1" );
            Assert.AreSame( res2, schedules[1].Resource, "resource 2" );
        }

        /// <summary>
        /// Es wird versucht die Anzahl unterschiedlicher Quellen auf einer Ressource zu minimieren.
        /// </summary>
        [TestMethod]
        public void Will_Minimize_Sources_Per_Resource()
        {
            // Create plan
            var source1 = SourceMock.Create( "s1" );
            var source2 = SourceMock.Create( "s2" );
            var source3 = SourceMock.Create( "s3" );
            var res1 = ResourceMock.Create( "r1", source1, source2, source3 ).SetPriority( 1 );
            var res2 = ResourceMock.Create( "r2", source1, source2, source3 ).SetPriority( 1 );
            var res3 = ResourceMock.Create( "r3", source1, source2, source3 ).SetPriority( 0 );
            var refTime = DateTime.UtcNow.Date.AddDays( 10 );
            var plan1 = RecordingDefinition.Create( false, "A1", Guid.NewGuid(), null, source1, refTime.AddHours( 19 ), TimeSpan.FromHours( 2 ) );
            var plan2 = RecordingDefinition.Create( false, "A2", Guid.NewGuid(), null, source2, refTime.AddHours( 20 ), TimeSpan.FromHours( 3 ) );
            var plan3 = RecordingDefinition.Create( false, "A3", Guid.NewGuid(), null, source3, refTime.AddHours( 20 ), TimeSpan.FromHours( 2 ) );
            var plan4 = RecordingDefinition.Create( false, "A4", Guid.NewGuid(), null, source2, refTime.AddHours( 22 ), TimeSpan.FromHours( 2 ) );

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase, Properties.Files.MinimizeSourcesPerResource )
                    {
                        { res1 },
                        { res2 },
                        { res3 },
                        { plan1 },
                        { plan2 },
                        { plan3 },
                        { plan4 },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules( refTime ).ToArray();

            // Validate
            Assert.AreEqual( 4, schedules.Length, "#schedule" );
            var exec1 = schedules.Single( s => s.Definition.UniqueIdentifier == plan1.UniqueIdentifier );
            var exec2 = schedules.Single( s => s.Definition.UniqueIdentifier == plan2.UniqueIdentifier );
            var exec3 = schedules.Single( s => s.Definition.UniqueIdentifier == plan3.UniqueIdentifier );
            var exec4 = schedules.Single( s => s.Definition.UniqueIdentifier == plan4.UniqueIdentifier );
            Assert.AreSame( res1, exec1.Resource, "A1" );
            Assert.AreSame( res2, exec2.Resource, "A2" );
            Assert.AreSame( res3, exec3.Resource, "A3" );
            Assert.AreSame( res2, exec4.Resource, "A4" );
        }
    }
}
