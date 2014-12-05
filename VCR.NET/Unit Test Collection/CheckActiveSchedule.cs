using System;
using JMS.DVBVCR.RecordingService.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JMS.DVBVCR.UnitTests
{
    /// <summary>
    /// Führt verschiedene Tests auf aktive und nicht aktive Aufzeichnungen durch.
    /// </summary>
    [TestClass]
    public class CheckActiveSchedule
    {
        /// <summary>
        /// Eine Aufzeichnung in der Zukunft gilt als aktiv.
        /// </summary>
        [TestMethod]
        public void FutureStartIsActive()
        {
            var cut =
                new VCRSchedule
                {
                    FirstStart = DateTime.UtcNow.AddDays( 1 ),
                    UniqueID = Guid.NewGuid(),
                    Duration = 10,
                };

            Assert.IsTrue( cut.IsActive );
        }

        /// <summary>
        /// Auch bei einer Wiederholung einzelner Tage ist eine zukünftige Aufzeichnung
        /// aktiv, wenn der Zeitraum nur lang genug ist.
        /// </summary>
        [TestMethod]
        public void FutureStartWithRepeatIsActive()
        {
            foreach (DayOfWeek day in Enum.GetValues( typeof( DayOfWeek ) ))
            {
                var cut =
                    new VCRSchedule
                    {
                        FirstStart = DateTime.UtcNow.AddDays( 1 ),
                        Days = VCRSchedule.GetDay( day ),
                        UniqueID = Guid.NewGuid(),
                        Duration = 10,
                    };

                cut.LastDay = cut.FirstStart.ToLocalTime().Date.AddDays( 14 );

                Assert.IsTrue( cut.IsActive, "{0}", day );
            }
        }

        /// <summary>
        /// Ist der Zeitraum der Wiederholung zu kurz, so kann eine Aufzeichnung evtl. ganz entfallen.
        /// </summary>
        [TestMethod]
        public void FutureStartWithRepeatIsInactive()
        {
            for (var delta = 10; delta-- > 0; )
                foreach (DayOfWeek day in Enum.GetValues( typeof( DayOfWeek ) ))
                {
                    var cut =
                        new VCRSchedule
                        {
                            FirstStart = DateTime.UtcNow.AddDays( 1 + 40 * delta ),
                            Days = VCRSchedule.GetDay( day ),
                            UniqueID = Guid.NewGuid(),
                            Duration = 10,
                        };

                    cut.LastDay = cut.FirstStart.ToLocalTime().Date.AddDays( 5 );

                    Assert.AreEqual( day != cut.FirstStart.ToLocalTime().AddDays( -1 ).DayOfWeek, cut.IsActive, "{0} {1}", delta, day );
                }
        }
    }
}
