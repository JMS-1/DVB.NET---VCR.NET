using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft den Umgang mit einer Verwaltungsinstanz für Geräte.
    /// </summary>
    [TestClass]
    public class ManagerSpecs
    {
        /// <summary>
        /// Beim Hinzufügen von Geräten müssen auch tatsächlich Geräte angegeben werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentNullException ) )]
        public void ResourceAddedToManagerMustNotBeNull()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
                cut.Add( default( IScheduleResource ) );
        }

        /// <summary>
        /// Die Konfiguration der Entschlüsselung muss immer gültig sein.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentNullException ) )]
        public void DecryptionGroupsMustBeValid()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
                cut.Add( new DecryptionGroup { DecryptionGroups = new[] { new DecryptionGroup { ScheduleResources = new IScheduleResource[1] } } } );
        }

        /// <summary>
        /// Es ist nicht erlaubt, ein Gerät mehrfach zu verwenden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void EachResourceCanOnlyBeUsedOnce()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Add it
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device1 );
            }
        }

        /// <summary>
        /// Es können mehrere Geräte verwaltet werden.
        /// </summary>
        [TestMethod]
        public void CanUseMultipleResources()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Add it
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );
            }
        }

        /// <summary>
        /// Eine Aufzeichnung muss eine positive Laufzeit haben.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentOutOfRangeException ) )]
        public void RecordingTimeMustBePositive()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );

                // Get some time
                var time = DateTime.UtcNow;

                // Try it
                cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, Guid.NewGuid(), "test", time, time );
            }
        }

        /// <summary>
        /// Die Zuordnung einer Aufzeichnung ist nur bei Angabe eines Geräte möglich.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentNullException ) )]
        public void StartRequiresResource()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
                cut.Start( null, SourceMock.Source1Group1Free, Guid.NewGuid(), "test", DateTime.UtcNow, DateTime.UtcNow.AddHours( 1 ) );
        }

        /// <summary>
        /// Eine Aufzeichnung kann nur auf einem vorher definierten Gerät gestartet werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void StartRequiresKnownResource()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
                cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, Guid.NewGuid(), "test", DateTime.UtcNow, DateTime.UtcNow.AddHours( 1 ) );
        }

        /// <summary>
        /// Wenn eine Quelle beim Starten angegeben wird, so muss diese von dem Gerät empfangen werden können.
        /// </summary>
        [TestMethod]
        public void StartSourceMustBeAccessibleByDevice()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );

                // Try it
                Assert.IsFalse( cut.Start( ResourceMock.Device1, SourceMock.Source5Group1Pay, Guid.NewGuid(), "test", DateTime.UtcNow, DateTime.UtcNow.AddHours( 1 ) ) );
            }
        }

        /// <summary>
        /// Eine einzelne Aufzeichnung wird korrekt vermerkt.
        /// </summary>
        [TestMethod]
        public void CanAddASingleRecordingAndReportAllocation()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start = DateTime.UtcNow;
                var end = start.AddMinutes( 23 );
                var id = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test1", start, end ) );

                // Read out
                var allocations = cut.CurrentAllocations;
                var resources = cut.Resources;

                // Validate
                CollectionAssert.AreEquivalent( new[] { ResourceMock.Device1, ResourceMock.Device2 }, resources, "Resources" );
                Assert.AreEqual( 1, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device1, allocations[0].Resources.Single(), "Resource" );
                Assert.AreEqual( "test1", allocations[0].Name, "Name" );
                Assert.AreSame( SourceMock.Source1Group1Free, allocations[0].Source, "Source" );
                Assert.AreEqual( id, allocations[0].UniqueIdentifier, "UniqueIdentifier" );
                Assert.AreEqual( start, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end, allocations[0].Time.End, "End" );
            }
        }

        /// <summary>
        /// Es ist möglich, zwei Aufzeichnungen zu starten, die auf verschiedenen Quellen einer Quellgruppe
        /// basieren.
        /// </summary>
        [TestMethod]
        public void CanAddTwoSourcesOnSameGroup()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes( 23 );
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes( 7 );
                var end2 = start2.AddMinutes( 120 );
                var id2 = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1 ) );
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source2Group1Free, id2, "test2", start2, end2 ) );

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.AreEqual( 2, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device1, allocations[0].Resources.Single(), "Resource 1" );
                Assert.AreEqual( "test1", allocations[0].Name, "Name 1" );
                Assert.AreSame( SourceMock.Source1Group1Free, allocations[0].Source, "Source 1" );
                Assert.AreEqual( id1, allocations[0].UniqueIdentifier, "UniqueIdentifier 1" );
                Assert.AreEqual( start1, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end1, allocations[0].Time.End, "End" );
                Assert.AreSame( ResourceMock.Device1, allocations[1].Resources.Single(), "Resource 2" );
                Assert.AreEqual( "test2", allocations[1].Name, "Name 2" );
                Assert.AreSame( SourceMock.Source2Group1Free, allocations[1].Source, "Source 2" );
                Assert.AreEqual( id2, allocations[1].UniqueIdentifier, "UniqueIdentifier 2" );
                Assert.AreEqual( start2, allocations[1].Time.Start, "Start" );
                Assert.AreEqual( end2, allocations[1].Time.End, "End" );
            }
        }

        /// <summary>
        /// Es ist nicht möglich, zwei Aufzeichnungen auf verschiedenen Quellgruppen zu starten.
        /// </summary>
        [TestMethod]
        public void CanNotStartTwoSourcesOnDifferentGroups()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes( 23 );
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes( 7 );
                var end2 = start2.AddMinutes( 120 );
                var id2 = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1 ) );
                Assert.IsFalse( cut.Start( ResourceMock.Device1, SourceMock.Source1Group2Free, id2, "test2", start2, end2 ) );

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.AreEqual( 1, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device1, allocations[0].Resources.Single(), "Resource 1" );
                Assert.AreEqual( "test1", allocations[0].Name, "Name 1" );
                Assert.AreSame( SourceMock.Source1Group1Free, allocations[0].Source, "Source 1" );
                Assert.AreEqual( id1, allocations[0].UniqueIdentifier, "UniqueIdentifier 1" );
                Assert.AreEqual( start1, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end1, allocations[0].Time.End, "End" );
            }
        }

        /// <summary>
        /// Wenn ein Gerät Aufzeichnungen durchführt ist es nicht möglich, eine Aufgabe zu starten.
        /// </summary>
        [TestMethod]
        public void CanNotStartTaskIfRegularRecordingIsActive()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes( 23 );
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes( 7 );
                var end2 = start2.AddMinutes( 120 );
                var id2 = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1 ) );
                Assert.IsFalse( cut.Start( ResourceMock.Device1, null, id2, "test2", start2, end2 ) );

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.AreEqual( 1, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device1, allocations[0].Resources.Single(), "Resource 1" );
                Assert.AreEqual( "test1", allocations[0].Name, "Name 1" );
                Assert.AreSame( SourceMock.Source1Group1Free, allocations[0].Source, "Source 1" );
                Assert.AreEqual( id1, allocations[0].UniqueIdentifier, "UniqueIdentifier 1" );
                Assert.AreEqual( start1, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end1, allocations[0].Time.End, "End" );
            }
        }

        /// <summary>
        /// Es ist nicht erlaubt, eine Aufzeichnung mehrfach zu starten.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void EachRecordingCanOnlyBeStartedOnce()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start = DateTime.UtcNow;
                var end = start.AddMinutes( 23 );
                var id = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test", start, end ) );

                // Fail it
                cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test", start, end );
            }
        }

        /// <summary>
        /// Führt ein Gerät eine Aufgabe aus, so können keine regulären Aufzeichnungen ergänzt werden.
        /// </summary>
        [TestMethod]
        public void CanNotStartRegularRecordingIfTaskIsActive()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes( 23 );
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes( 7 );
                var end2 = start2.AddMinutes( 120 );
                var id2 = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, null, id2, "test2", start2, end2 ) );
                Assert.IsFalse( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1 ) );

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.AreEqual( 1, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device1, allocations[0].Resources.Single(), "Resource 1" );
                Assert.AreEqual( "test2", allocations[0].Name, "Name 1" );
                Assert.IsNull( allocations[0].Source, "Source 1" );
                Assert.AreEqual( id2, allocations[0].UniqueIdentifier, "UniqueIdentifier 1" );
                Assert.AreEqual( start2, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end2, allocations[0].Time.End, "End" );
            }
        }

        /// <summary>
        /// Führt ein Gerät eine Aufgabe aus, so können keine regulären Aufzeichnungen ergänzt werden.
        /// </summary>
        [TestMethod]
        public void CanNotStartRegularRecordingIfTaskIsActiveEvenIfOverlapping()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes( 120 );
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes( 7 );
                var end2 = start2.AddMinutes( 23 );
                var id2 = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, null, id2, "test2", start2, end2 ) );
                Assert.IsFalse( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1 ) );

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.AreEqual( 1, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device1, allocations[0].Resources.Single(), "Resource 1" );
                Assert.AreEqual( "test2", allocations[0].Name, "Name 1" );
                Assert.IsNull( allocations[0].Source, "Source 1" );
                Assert.AreEqual( id2, allocations[0].UniqueIdentifier, "UniqueIdentifier 1" );
                Assert.AreEqual( start2, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end2, allocations[0].Time.End, "End" );
            }
        }

        /// <summary>
        /// Führt ein Gerät eine Aufgabe aus, so kann keine weitere Aufgabe gestartet werden.
        /// </summary>
        [TestMethod]
        public void CanNotStartTwoTasks()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes( 120 );
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes( 7 );
                var end2 = start2.AddMinutes( 23 );
                var id2 = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, null, id2, "test2", start2, end2 ) );
                Assert.IsFalse( cut.Start( ResourceMock.Device1, null, id1, "test1", start1, end1 ) );

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.AreEqual( 1, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device1, allocations[0].Resources.Single(), "Resource 1" );
                Assert.AreEqual( "test2", allocations[0].Name, "Name 1" );
                Assert.IsNull( allocations[0].Source, "Source 1" );
                Assert.AreEqual( id2, allocations[0].UniqueIdentifier, "UniqueIdentifier 1" );
                Assert.AreEqual( start2, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end2, allocations[0].Time.End, "End" );
            }
        }

        /// <summary>
        /// Eine Aufzeichnung kann beendet werden.
        /// </summary>
        [TestMethod]
        public void CanStopARecording()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes( 23 );
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes( 7 );
                var end2 = start2.AddMinutes( 120 );
                var id2 = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1 ) );
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source2Group1Free, id2, "test2", start2, end2 ) );

                // Remove one
                cut.Stop( id1 );

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.AreEqual( 1, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device1, allocations[0].Resources.Single(), "Resource" );
                Assert.AreEqual( "test2", allocations[0].Name, "Name" );
                Assert.AreSame( SourceMock.Source2Group1Free, allocations[0].Source, "Source" );
                Assert.AreEqual( id2, allocations[0].UniqueIdentifier, "UniqueIdentifier" );
                Assert.AreEqual( start2, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end2, allocations[0].Time.End, "End" );
            }
        }

        /// <summary>
        /// Es können nur bekannte Aufzeichnungen beendet werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void StopRequestWillBeValidated()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
                cut.Stop( Guid.NewGuid() );
        }

        /// <summary>
        /// Es können nur bekannte Aufzeichnungen verändert werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void ModifyRequestWillBeValidated()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
                cut.Modify( Guid.NewGuid(), DateTime.MaxValue );
        }

        /// <summary>
        /// Der Endzeitpunkt einer Aufzeichnung kann verändert werden.
        /// </summary>
        [TestMethod]
        public void CanModifyARecording()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start = DateTime.UtcNow;
                var end1 = start.AddMinutes( 23 );
                var end2 = start.AddMinutes( 90 );
                var id = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test1", start, end1 ) );
                Assert.IsTrue( cut.Modify( id, end2 ) );

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.AreEqual( 1, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device1, allocations[0].Resources.Single(), "Resource" );
                Assert.AreEqual( "test1", allocations[0].Name, "Name" );
                Assert.AreSame( SourceMock.Source1Group1Free, allocations[0].Source, "Source" );
                Assert.AreEqual( id, allocations[0].UniqueIdentifier, "UniqueIdentifier" );
                Assert.AreEqual( start, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end2, allocations[0].Time.End, "End" );
            }
        }

        /// <summary>
        /// Der Endzeitpunkt einer Aufzeichnung kann niemals vor dem Beginn liegen.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentOutOfRangeException ) )]
        public void CanNotSetEndBeforeStart()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device
                cut.Add( ResourceMock.Device1 );
                cut.Add( ResourceMock.Device2 );

                // Time
                var start = DateTime.UtcNow;
                var end = start.AddMinutes( 23 );
                var id = Guid.NewGuid();

                // Try it
                Assert.IsTrue( cut.Start( ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test1", start, end ) );

                // Will fail
                cut.Modify( id, start );
            }
        }

        /// <summary>
        /// Beim Ändern des Endzeitpunktes einer Aufzeichnung werden die Grenzwerte berücksichtigt.
        /// </summary>
        [TestMethod]
        public void ModifyWillVerifyResourceUsage()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Register device - this selection has a single decryption slot available
                cut.Add( ResourceMock.Device2 );

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes( 23 );
                var id1 = Guid.NewGuid();
                var start2 = end1;
                var end2 = start2.AddMinutes( 120 );
                var id2 = Guid.NewGuid();

                // Create non overlapping recordings allowing the device to add both
                Assert.IsTrue( cut.Start( ResourceMock.Device2, SourceMock.Source1Group1Pay, id1, "test1", start1, end1 ), "Start 1" );
                Assert.IsTrue( cut.Start( ResourceMock.Device2, SourceMock.Source4Group1Pay, id2, "test2", start2, end2 ), "Start 2" );

                // Now extend the first recording just a tiny bit
                Assert.IsFalse( cut.Modify( id1, start2.AddTicks( 1 ) ), "Modify 1" );

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.AreEqual( 2, allocations.Length, "Allocations" );
                Assert.AreSame( ResourceMock.Device2, allocations[0].Resources.Single(), "Resource 1" );
                Assert.AreEqual( "test1", allocations[0].Name, "Name 1" );
                Assert.AreSame( SourceMock.Source1Group1Pay, allocations[0].Source, "Source 1" );
                Assert.AreEqual( id1, allocations[0].UniqueIdentifier, "UniqueIdentifier 1" );
                Assert.AreEqual( start1, allocations[0].Time.Start, "Start" );
                Assert.AreEqual( end1, allocations[0].Time.End, "End" );
                Assert.AreSame( ResourceMock.Device2, allocations[1].Resources.Single(), "Resource 2" );
                Assert.AreEqual( "test2", allocations[1].Name, "Name 2" );
                Assert.AreSame( SourceMock.Source4Group1Pay, allocations[1].Source, "Source 2" );
                Assert.AreEqual( id2, allocations[1].UniqueIdentifier, "UniqueIdentifier 2" );
                Assert.AreEqual( start2, allocations[1].Time.Start, "Start" );
                Assert.AreEqual( end2, allocations[1].Time.End, "End" );
            }
        }

        /// <summary>
        /// Eine Aufzeichnung ohne Wiederholung wird einmal ausgeführt.
        /// </summary>
        [TestMethod]
        public void WillScheduleSingleNonRepeatedTask()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Add a single device
                cut.Add( ResourceMock.Device1 );

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create( this, "test", Guid.NewGuid(), new[] { ResourceMock.Device1 }, SourceMock.Source1Group1Free, now.AddMinutes( 10 ), TimeSpan.FromMinutes( 100 ) );

                // Create initializer
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> callback =
                    ( s, t ) =>
                    {
                        // Load
                        s.Add( play1 );

                        // Report
                        return s.GetSchedules( t );
                    };

                // Check action - should be waiting on the next recording to start
                var next = cut.GetNextActivity( now, callback );

                // Test
                Assert.IsInstanceOfType( next, typeof( WaitActivity ) );

                // Advance
                now = ((WaitActivity) next).RetestTime;

                // Check action - next recording is now ready to start
                next = cut.GetNextActivity( now, callback );

                // Test
                Assert.IsInstanceOfType( next, typeof( StartActivity ) );

                // Get all 
                var plan = ((StartActivity) next).Recording;

                // Validate
                Assert.AreEqual( now, plan.Time.Start, "Initial Start" );
                Assert.IsNull( cut.GetEndOfAllocation(), "Initial Allocation" );

                // Mark as started
                Assert.IsTrue( cut.Start( plan ), "Start" );
                Assert.AreEqual( plan.Time.End, cut.GetEndOfAllocation(), "End" );

                // Check action - should now wait until recording ends
                next = cut.GetNextActivity( now, callback );

                // Test
                Assert.IsInstanceOfType( next, typeof( WaitActivity ) );

                // Advance
                now = ((WaitActivity) next).RetestTime;

                // Test
                Assert.AreEqual( plan.Time.End, now );

                // Check action - recording is done and can be terminated
                next = cut.GetNextActivity( now, callback );

                // Test
                Assert.IsInstanceOfType( next, typeof( StopActivity ) );
                Assert.AreEqual( play1.UniqueIdentifier, ((StopActivity) next).UniqueIdentifier );

                // Do it
                cut.Stop( play1.UniqueIdentifier );

                // Retest - nothing more to do
                Assert.IsNull( cut.GetNextActivity( now, callback ) );
            }
        }

        /// <summary>
        /// Eine Aufzeichnung mit Wiederholung wird bei der Folgeplanung korrekt ausgeblendet.
        /// </summary>
        [TestMethod]
        public void WillScheduleSingleRepeatedTask()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Add a single device
                cut.Add( ResourceMock.Device1 );

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create( this, "test", Guid.NewGuid(), new[] { ResourceMock.Device1 }, SourceMock.Source1Group1Free, now.AddMinutes( 10 ), TimeSpan.FromMinutes( 100 ), new DateTime( 2100, 12, 31 ), DayOfWeek.Monday );

                // Get the schedule
                var scheduler = cut.CreateScheduler();

                // Load recordings
                scheduler.Add( play1 );

                // Get all 
                var plan = scheduler.GetSchedules( now ).Take( 100 ).ToArray();

                // Validate
                Assert.AreEqual( 100, plan.Length, "Initial Count" );
                Assert.AreEqual( DayOfWeek.Monday, plan[0].Time.LocalStart.DayOfWeek, "Initial Start" );
                Assert.IsNull( cut.GetEndOfAllocation(), "Initial Allocation" );

                // Mark as started
                Assert.IsTrue( cut.Start( plan[0] ), "Start" );

                // Get the schedule
                scheduler = cut.CreateScheduler();

                // Load recordings
                scheduler.Add( play1 );

                // Test
                Assert.IsFalse( scheduler.GetSchedules( now ).Any(), "Ignore Recording" );
                Assert.AreEqual( plan[0].Time.End, cut.GetEndOfAllocation(), "End" );
            }
        }

        /// <summary>
        /// Eine Aufzeichnung mit Wiederholung wird bei der Folgeplanung korrekt ausgeblendet.
        /// </summary>
        [TestMethod]
        public void WillNotShowCurrentRecordingInPlan()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Add a single device
                cut.Add( ResourceMock.Device1 );

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create( this, "test", Guid.NewGuid(), new[] { ResourceMock.Device1 }, SourceMock.Source1Group1Free, now.AddMinutes( 10 ), TimeSpan.FromMinutes( 100 ), new DateTime( 2100, 12, 31 ), DayOfWeek.Monday );

                // Loading helper
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> loader =
                    ( s, d ) =>
                    {
                        // Load recording
                        s.Add( play1 );

                        // Report
                        return s.GetSchedules( d );
                    };

                // Find empty list
                var initial = cut.GetSchedules( now, loader ).Take( 2 ).ToArray();

                // Start the first one
                Assert.IsTrue( cut.Start( initial[0] ) );

                // Recheck the list
                var follower = cut.GetSchedules( now, loader ).First();

                // Make sure that first is skipped
                Assert.AreEqual( initial[1].Time.Start, follower.Time.Start );
            }
        }

        /// <summary>
        /// Eine Aufzeichnung mit Wiederholung wird bei der Folgeplanung nicht eingeblendet, wenn die Laufzeit verlängert wurde.
        /// </summary>
        [TestMethod]
        public void WillNotShowCurrentRecordingInPlanIfExtended()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Add a single device
                cut.Add( ResourceMock.Device1 );

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create( this, "test", Guid.NewGuid(), new[] { ResourceMock.Device1 }, SourceMock.Source1Group1Free, now.AddMinutes( 10 ), TimeSpan.FromMinutes( 100 ), new DateTime( 2100, 12, 31 ), DayOfWeek.Monday );

                // Loading helper
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> loader =
                    ( s, d ) =>
                    {
                        // Load recording
                        s.Add( play1 );

                        // Report
                        return s.GetSchedules( d );
                    };

                // Find empty list
                var initial = cut.GetSchedules( now, loader ).Take( 2 ).ToArray();

                // Start the first one
                Assert.IsTrue( cut.Start( initial[0] ) );

                // Move it
                Assert.IsTrue( cut.Modify( initial[0].Definition.UniqueIdentifier, initial[0].Time.End.AddMinutes( 20 ) ) );

                // Recheck the list
                var follower = cut.GetSchedules( now, loader ).First();

                // Make sure that first is skipped
                Assert.AreEqual( initial[1].Time.Start, follower.Time.Start );
            }
        }

        /// <summary>
        /// Eine Aufzeichnung mit Wiederholung wird bei der Folgeplanung eingeblendet, wenn die Laufzeit verkürzt wurde.
        /// </summary>
        [TestMethod]
        public void WillNotShowCurrentRecordingInPlanIfCut()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Add a single device
                cut.Add( ResourceMock.Device1 );

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create( this, "test", Guid.NewGuid(), new[] { ResourceMock.Device1 }, SourceMock.Source1Group1Free, now.AddMinutes( 10 ), TimeSpan.FromMinutes( 100 ), new DateTime( 2100, 12, 31 ), DayOfWeek.Monday );

                // Loading helper
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> loader =
                    ( s, d ) =>
                    {
                        // Load recording
                        s.Add( play1 );

                        // Report
                        return s.GetSchedules( d );
                    };

                // Find empty list
                var initial = cut.GetSchedules( now, loader ).Take( 2 ).ToArray();

                // Start the first one
                Assert.IsTrue( cut.Start( initial[0] ) );

                // Move it
                Assert.IsTrue( cut.Modify( initial[0].Definition.UniqueIdentifier, initial[0].Time.End.AddMinutes( -20 ) ) );

                // Recheck the list
                var follower = cut.GetSchedules( now, loader ).First();

                // Make sure that first is skipped
                Assert.AreEqual( initial[1].Time.Start, follower.Time.Start );
            }
        }


        /// <summary>
        /// Eine laufende Aufgabe wird korrekt berücksichtigt.
        /// </summary>
        [TestMethod]
        public void CanAddRecordingOverlappingRunning()
        {
            // Create component under test
            using (var cut = ResourceManager.Create( StringComparer.InvariantCultureIgnoreCase ))
            {
                // Add a single device
                cut.Add( ResourceMock.Device1 );

                // Time to use
                var now = new DateTime( 2013, 12, 3, 17, 0, 0, DateTimeKind.Utc );

                // Start a task
                Assert.IsTrue( cut.Start( ResourceMock.Device1, null, Guid.NewGuid(), "EPG", now, now.AddMinutes( 20 ) ), "epg" );

                // Create the recording
                var plan1 = RecordingDefinition.Create( false, "testA", Guid.NewGuid(), null, SourceMock.Source1Group1Free, now.AddMinutes( 1 ), TimeSpan.FromMinutes( 120 ) );
                var plan2 = RecordingDefinition.Create( false, "testB", Guid.NewGuid(), null, SourceMock.Source1Group2Free, now.AddDays( 1 ).AddHours( 1 ), TimeSpan.FromMinutes( 30 ) );
                var plan3 = RecordingDefinition.Create( false, "testC", Guid.NewGuid(), null, SourceMock.Source1Group3Free, now.AddDays( 1 ).AddHours( 2 ), TimeSpan.FromMinutes( 30 ) );

                // Loading helper
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> loader =
                    ( s, d ) =>
                    {
                        // Load recording
                        s.Add( plan1 );
                        s.Add( plan2 );
                        s.Add( plan3 );

                        // Report
                        return s.GetSchedules( d );
                    };

                // Find empty list
                var schedules = cut.GetSchedules( now, loader ).ToArray();

                // Validate
                Assert.AreEqual( 3, schedules.Count( s => s.Resource != null ), "#" );
            }
        }
    }
}
