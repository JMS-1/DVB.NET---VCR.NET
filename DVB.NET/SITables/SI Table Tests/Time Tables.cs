using System;
using JMS.DVB.SI;
using NUnit.Framework;
using System.Collections.Generic;

namespace JMS.DVB.SITables.Tests
{
    /// <summary>
    /// Liest SI Tabellen mit Zeitinformationen.
    /// </summary>
    [TestFixture]
    public class Time_Tables
    {
        /// <summary>
        /// Erzeugt einen neuen Test.
        /// </summary>
        public Time_Tables()
        {
        }

        /// <summary>
        /// Liest die Uhrzeit von einer <i>Time and Date Table</i>.
        /// </summary>
        [Test]
        public void FromTDT()
        {
            // Result
            var times = new List<DateTime>();

            // Create parser
            var parser = TableParser.Create<TDT>( t => { times.Add( t.TimeStamp ); } );

            // Feed it
            parser.AddPayload( Properties.Resources.timeinfo1 );

            // Report
            foreach (var time in times)
                Console.WriteLine( time.ToLocalTime() );

            // Check it
            Assert.AreEqual( new DateTime( 2009, 12, 27, 17, 41, 35, DateTimeKind.Utc ), times[0] );
        }

        /// <summary>
        /// Liest die Uhrzeit von einer <i>Time Offset Table</i>.
        /// </summary>
        [Test]
        public void FromTOT()
        {
            // Result
            var times = new List<DateTime>();

            // Create parser
            var parser = TableParser.Create<TOT>( t => { times.Add( t.TimeStamp ); } );

            // Feed it
            parser.AddPayload( Properties.Resources.timeinfo0 );

            // Report
            foreach (var time in times)
                Console.WriteLine( time.ToLocalTime() );

            // Check it
            Assert.AreEqual( new DateTime( 2009, 12, 27, 17, 29, 11, DateTimeKind.Utc ), times[0] );
        }
    }
}
