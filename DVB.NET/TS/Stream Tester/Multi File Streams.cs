using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using JMS.DVB.BDA.AccessModules;

namespace JMS.DVB.TS.Tests
{
    /// <summary>
    /// Prüft den Zugriff auf Dateigruppen.
    /// </summary>
    [TestFixture]
    public class Multi_File_Streams : TestBase
    {
        /// <summary>
        /// Erzeugt eine neue Testumgebung.
        /// </summary>
        public Multi_File_Streams()
        {
        }

        /// <summary>
        /// Erzeugt ein Muster mit zufälligem Inhalt.
        /// </summary>
        /// <returns>Das gewünschte Muster.</returns>
        private static byte[] CreateRandomSet()
        {
            // Generate somewhat between 10 and 20 thousand
            byte[] testSet = new byte[Generator.Next( 10000, 20000 )];

            // Fill it randomly
            for (int i = testSet.Length; i-- > 0; )
                testSet[i] = (byte) Generator.Next( 256 );

            // Report
            return testSet;
        }

        /// <summary>
        /// Erzeugt einen Satz zufälliger Daten und verteilt diesen auf unterschiedliche Dateien.
        /// </summary>
        [Test]
        public void SplitFiles()
        {
            // Get the test set
            var testData = CreateRandomSet();

            // Get the number of files to use
            var fileSet = Enumerable.Range( 0, Generator.Next( 4, 7 ) ).Select( i => GetUniqueFile() ).ToArray();

            // Split input data
            int blockSize = testData.Length / fileSet.Length;

            // Process
            for (int i = fileSet.Length; i > 0; )
            {
                // Get the size
                int end;
                if (i-- == fileSet.Length)
                    end = testData.Length;
                else
                    end = (i + 1) * blockSize;

                // Get the start
                int start = i * blockSize;

                // Copy
                using (var stream = File.Create( fileSet[i].FullName ))
                    stream.Write( testData, start, end - start );
            }

            // Load all but the last
            using (var stream = new MultiFileStream( fileSet.Take( fileSet.Length - 1 ) ))
            {
                // Get length
                long length = fileSet.Take( fileSet.Length - 1 ).Sum( f => f.Length );

                // Check size and name
                Assert.AreEqual( Path.GetFileNameWithoutExtension( fileSet[0].Name ), stream.Name, "name" );
                Assert.AreEqual( length, stream.Length, "partial length" );

                // Validate sub set
                Validate( stream, null, testData, 0, (int) length );

                // Get length
                length += fileSet[fileSet.Length - 1].Length;

                // Add the last file
                stream.Add( fileSet[fileSet.Length - 1] );

                // Check size 
                Assert.AreEqual( length, stream.Length, "full length" );

                // Validate full set
                Validate( stream, 0, testData, 0, (int) length );

                // Random read
                for (int c = 1000; c-- > 0; )
                {
                    // Find length to process
                    int count = Generator.Next( testData.Length / 10 );

                    // Find some position to start at
                    int offset = Generator.Next( 0, testData.Length - count );

                    // Read it
                    Validate( stream, offset, testData, offset, count );
                }

                // Reset
                stream.Position = 0;

                // Find length to process
                int part = Generator.Next( testData.Length / 20 );

                // Full partial read
                for (int offset = 0; offset < testData.Length; offset += part)
                {
                    // Process part
                    Validate( stream, null, testData, offset, Math.Min( part, testData.Length - offset ) );
                }

                // Create extended buffer
                byte[] extended = new byte[2 * testData.Length];

                // Reset
                stream.Position = 0;

                // Read extended buffer
                Assert.AreEqual( testData.Length, stream.Read( extended, 0, extended.Length ), "Clip (1)" );
                Assert.AreEqual( 0, stream.Read( extended, 0, extended.Length ), "Clip (2)" );
            }
        }

        /// <summary>
        /// Überprüft den Inhalt der zusammengesetzten Datei.
        /// </summary>
        /// <param name="stream">Die zusammengesetzte Datei.</param>
        /// <param name="position">Optional eine neue Position in der Datei.</param>
        /// <param name="data">Die erwarteten Daten.</param>
        /// <param name="offset">Die erste Position in den erwarteten Daten.</param>
        /// <param name="count">Die Anzahl der zu überprüfenden Bytes.</param>
        private static void Validate( IVirtualStream stream, long? position, byte[] data, int offset, int count )
        {
            // Report 
            Console.WriteLine( "Validate({0}, {1}, {2})", position, offset, count );

            // Move position
            if (position.HasValue)
                stream.Position = position.Value;

            // Load position
            long initialPosition = stream.Position;

            // Validate
            if (position.HasValue)
                Assert.AreEqual( position.Value, initialPosition, "Initital Position" );

            // Random shift
            byte[] shift = Enumerable.Range( 0, Generator.Next( 100 ) ).Select( i => (byte) Generator.Next( 256 ) ).ToArray();

            // Allocate read buffer
            byte[] read = new byte[shift.Length + count];

            // Fill
            Array.Copy( shift, read, shift.Length );

            // Process the read
            Assert.AreEqual( count, stream.Read( read, shift.Length, count ) );

            // Validate
            Assert.AreEqual( initialPosition + count, stream.Position, "Final Position" );

            // Validate random pattern
            for (int i = shift.Length; i-- > 0; )
                Assert.AreEqual( shift[i], read[i], "Guard at {0}", i );

            // Real data
            for (int i = count; i-- > 0; )
                Assert.AreEqual( data[i + offset], read[i + shift.Length], "Data at {0}", i );
        }
    }
}
