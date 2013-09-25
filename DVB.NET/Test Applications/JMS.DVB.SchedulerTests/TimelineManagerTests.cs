using System;
using System.Linq;
using JMS.DVB.Algorithms.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft die Manipulationen einer Zeitlinienverwaltung.
    /// </summary>
    [TestClass]
    public class TimelineManagerTests
    {
        /// <summary>
        /// Unsere eigene Implementierung einer Zeitlinie.
        /// </summary>
        private class Timeline : TimelineManager<int>
        {
            /// <summary>
            /// Führt die Daten zweier Bereiche zusammen.
            /// </summary>
            /// <param name="existing">Die Daten des existierenden Bereiches.</param>
            /// <param name="added">Die Daten des neuen Bereichs.</param>
            /// <returns>Die Zusammenfassung der Daten.</returns>
            protected override int Merge( int existing, int added )
            {
                // Simply add
                return existing + added;
            }
        }

        /// <summary>
        /// Es ist möglich, einen einzelnen Bereich anzulegen.
        /// </summary>
        [TestMethod]
        public void CanAddASingleRangeByTimeSpan()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays( 5 );
            var range = Timeline.Range.Create( now, TimeSpan.FromMinutes( 120 ), 0 );

            // Create component under test
            var cut =
                new Timeline
                {
                    range,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.AreEqual( 1, ranges.Length, "#ranges" );
            Assert.AreEqual( now, ranges[0].Start, "start" );
            Assert.AreEqual( now.AddMinutes( 120 ), ranges[0].End, "end" );
            Assert.AreEqual( 0, ranges[0].Data, "data" );
        }

        /// <summary>
        /// Es ist möglich, einen einzelnen Bereich anzulegen.
        /// </summary>
        [TestMethod]
        public void CanAddASingleRangeByEndTime()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays( 5 );
            var range = Timeline.Range.Create( now, now.AddHours( 2 ), 0 );

            // Create component under test
            var cut =
                new Timeline
                {
                    range,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.AreEqual( 1, ranges.Length, "#ranges" );
            Assert.AreEqual( now, ranges[0].Start, "start" );
            Assert.AreEqual( now.AddMinutes( 120 ), ranges[0].End, "end" );
            Assert.AreEqual( 0, ranges[0].Data, "data" );
        }

        /// <summary>
        /// Es ist möglich, einen neuen Bereich ganz am Anfang der Liste anzulegen.
        /// </summary>
        [TestMethod]
        public void CanAddOneTimeBeforeTheFirst()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays( 5 );
            var range1 = Timeline.Range.Create( now, now.AddHours( 2 ), 1 );
            var range2 = Timeline.Range.Create( now.AddHours( -2 ), now, 2 );

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.AreEqual( 2, ranges.Length, "#ranges" );
            Assert.AreEqual( now.AddMinutes( -120 ), ranges[0].Start, "start 0" );
            Assert.AreEqual( now, ranges[0].End, "end 0" );
            Assert.AreEqual( 2, ranges[0].Data, "data 0" );
            Assert.AreEqual( now, ranges[1].Start, "start 1" );
            Assert.AreEqual( now.AddMinutes( 120 ), ranges[1].End, "end 1" );
            Assert.AreEqual( 1, ranges[1].Data, "data 1" );
        }

        /// <summary>
        /// Es ist möglich, einen neuen Bereich in die Mitte der Liste zu legen.
        /// </summary>
        [TestMethod]
        public void CanAddOneTimeInBetween()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays( 5 );
            var range1 = Timeline.Range.Create( now, now.AddHours( 2 ), 1 );
            var range2 = Timeline.Range.Create( now.AddHours( -4 ), now.AddHours( -2 ), 2 );
            var range3 = Timeline.Range.Create( now.AddHours( -2 ), now, 4 );

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                    range3,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.AreEqual( 3, ranges.Length, "#ranges" );
            Assert.AreEqual( now.AddMinutes( -240 ), ranges[0].Start, "start 0" );
            Assert.AreEqual( now.AddMinutes( -120 ), ranges[0].End, "end 0" );
            Assert.AreEqual( 2, ranges[0].Data, "data 0" );
            Assert.AreEqual( now.AddMinutes( -120 ), ranges[1].Start, "start 1" );
            Assert.AreEqual( now, ranges[1].End, "end 1" );
            Assert.AreEqual( 4, ranges[1].Data, "data 1" );
            Assert.AreEqual( now, ranges[2].Start, "start 2" );
            Assert.AreEqual( now.AddMinutes( 120 ), ranges[2].End, "end 2" );
            Assert.AreEqual( 1, ranges[2].Data, "data 2" );
        }

        /// <summary>
        /// Es ist möglich, einen Bereicht zu teilen.
        /// </summary>
        [TestMethod]
        public void CanSplitAndMerge()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays( 5 );
            var range1 = Timeline.Range.Create( now, now.AddHours( 2 ), 1 );
            var range2 = Timeline.Range.Create( now.AddHours( -1 ), now.AddHours( 1 ), 2 );

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.AreEqual( 3, ranges.Length, "#ranges" );
            Assert.AreEqual( now.AddMinutes( -60 ), ranges[0].Start, "start 0" );
            Assert.AreEqual( now, ranges[0].End, "end 0" );
            Assert.AreEqual( 2, ranges[0].Data, "data 0" );
            Assert.AreEqual( now, ranges[1].Start, "start 1" );
            Assert.AreEqual( now.AddMinutes( 60 ), ranges[1].End, "end 1" );
            Assert.AreEqual( 3, ranges[1].Data, "data 1" );
            Assert.AreEqual( now.AddMinutes( 60 ), ranges[2].Start, "start 2" );
            Assert.AreEqual( now.AddMinutes( 120 ), ranges[2].End, "end 2" );
            Assert.AreEqual( 1, ranges[2].Data, "data 2" );
        }

        /// <summary>
        /// Es ist möglich, eine Lücke zwischen zwei Bereichen zu füllen.
        /// </summary>
        [TestMethod]
        public void CanMergeGap()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays( 5 );
            var range1 = Timeline.Range.Create( now.AddHours( -1 ), now.AddHours( 1 ), 1 );
            var range2 = Timeline.Range.Create( now.AddHours( 2 ), now.AddHours( 4 ), 2 );
            var range3 = Timeline.Range.Create( now, now.AddHours( 3 ), 4 );

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                    range3,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.AreEqual( 5, ranges.Length, "#ranges" );
            Assert.AreEqual( now.AddMinutes( -60 ), ranges[0].Start, "start 0" );
            Assert.AreEqual( now, ranges[0].End, "end 0" );
            Assert.AreEqual( 1, ranges[0].Data, "data 0" );
            Assert.AreEqual( now, ranges[1].Start, "start 1" );
            Assert.AreEqual( now.AddMinutes( 60 ), ranges[1].End, "end 1" );
            Assert.AreEqual( 5, ranges[1].Data, "data 1" );
            Assert.AreEqual( now.AddMinutes( 60 ), ranges[2].Start, "start 2" );
            Assert.AreEqual( now.AddMinutes( 120 ), ranges[2].End, "end 2" );
            Assert.AreEqual( 4, ranges[2].Data, "data 2" );
            Assert.AreEqual( now.AddMinutes( 120 ), ranges[3].Start, "start 3" );
            Assert.AreEqual( now.AddMinutes( 180 ), ranges[3].End, "end 3" );
            Assert.AreEqual( 6, ranges[3].Data, "data 3" );
            Assert.AreEqual( now.AddMinutes( 180 ), ranges[4].Start, "start 4" );
            Assert.AreEqual( now.AddMinutes( 240 ), ranges[4].End, "end 4" );
            Assert.AreEqual( 2, ranges[4].Data, "data 4" );
        }

        /// <summary>
        /// Zwei Bereiche mit gleicher Startzeit können überlagert werden.
        /// </summary>
        [TestMethod]
        public void CanAlignAtStart()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays( 5 );
            var range1 = Timeline.Range.Create( now, now.AddHours( 1 ), 1 );
            var range2 = Timeline.Range.Create( now, now.AddHours( 2 ), 2 );

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.AreEqual( 2, ranges.Length, "#ranges" );
            Assert.AreEqual( now, ranges[0].Start, "start 0" );
            Assert.AreEqual( now.AddMinutes( 60 ), ranges[0].End, "end 0" );
            Assert.AreEqual( 3, ranges[0].Data, "data 0" );
            Assert.AreEqual( now.AddMinutes( 60 ), ranges[1].Start, "start 1" );
            Assert.AreEqual( now.AddMinutes( 120 ), ranges[1].End, "end 1" );
            Assert.AreEqual( 2, ranges[1].Data, "data 1" );
        }

        /// <summary>
        /// Zwei Bereiche mit gleicher Endzeit können überlagert werden.
        /// </summary>
        [TestMethod]
        public void CanAlignAtEnd()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays( 5 );
            var range1 = Timeline.Range.Create( now, now.AddHours( 1 ), 1 );
            var range2 = Timeline.Range.Create( now.AddHours( -1 ), now.AddHours( 1 ), 2 );

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.AreEqual( 2, ranges.Length, "#ranges" );
            Assert.AreEqual( now.AddMinutes( -60 ), ranges[0].Start, "start 0" );
            Assert.AreEqual( now, ranges[0].End, "end 0" );
            Assert.AreEqual( 2, ranges[0].Data, "data 0" );
            Assert.AreEqual( now, ranges[1].Start, "start 1" );
            Assert.AreEqual( now.AddMinutes( 60 ), ranges[1].End, "end 1" );
            Assert.AreEqual( 3, ranges[1].Data, "data 1" );
        }

        /// <summary>
        /// Füllt die Zeitschiene mit Zufallswerten.
        /// </summary>
        [TestMethod]
        public void RandomFillTimeline()
        {
            // Create random generator
            var rnd = new Random( Environment.TickCount );

            // Create scratch area
            var cnt = new int[rnd.Next( 2500, 7500 )];
            var cmp = new int[cnt.Length];

            // Create component under test
            var now = DateTime.UtcNow.Date;
            var cut = new Timeline();

            // Process
            for (var n = rnd.Next( 1000, 5000 ); n-- > 0; )
            {
                // Choose range
                var startIndex = rnd.Next( cnt.Length - 10 );
                var endIndex = startIndex + rnd.Next( 10 );

                // Register
                cut.Add( Timeline.Range.Create( now.AddMinutes( startIndex ), now.AddMinutes( endIndex + 1 ), 1 ) );

                // Count self
                while (startIndex <= endIndex)
                    cnt[startIndex++]++;
            }

            // Helper
            var lastEnd = now;

            // Resolve
            foreach (var range in cut)
            {
                // Validate
                Assert.IsTrue( range.Start >= lastEnd, "start {0}", range );
                Assert.IsTrue( range.End > range.Start, "duration {0}", range );

                // Get index
                var startIndex = (int) (range.Start - now).TotalMinutes;
                var endIndex = (int) (range.End - now).TotalMinutes;

                // Fill
                while (startIndex < endIndex)
                    cmp[startIndex++] += range.Data;

                // Update
                lastEnd = range.End;
            }

            // Validate
            CollectionAssert.AreEqual( cnt, cmp );

            // See if we checked the good stuff
            Assert.IsTrue( cnt.Min() == 0, "min" );
            Assert.IsTrue( cnt.Max() >= 4, "max" );
        }
    }
}
