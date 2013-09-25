using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Threading;
using System.Collections.Generic;

namespace JMS.DVB.TS.Tests
{
    /// <summary>
    /// Überprüft das Erzeugen von <i>Transport Streams</i>.
    /// </summary>
    [TestFixture]
    public class Stream_Generation : TestBase
    {
        /// <summary>
        /// Muster zur Nummerierung von Dateien.
        /// </summary>
        private const string FileNamePattern = "{0}-{1:000}.ts";

        /// <summary>
        /// Erzeugt einen neuen Test.
        /// </summary>
        public Stream_Generation()
        {
        }

        /// <summary>
        /// Überprüft, ob die Meldungen der Systemzeit korrekt empfangen werden.
        /// </summary>
        [Test]
        public void ReportPCR()
        {
            // Prepare receiption
            var station = GetDefaultStation();

            // Prepare selection
            var selection = new StreamSelection { Videotext = false, ProgramGuide = false };

            // Add all
            selection.AC3Tracks.LanguageMode = LanguageModes.Selection;
            selection.SubTitles.LanguageMode = LanguageModes.Selection;
            selection.MP2Tracks.LanguageMode = LanguageModes.Primary;

            // Get a file name
            var tempFile = GetUniqueFile();

            // Collected PCR data
            var pcrData = new Dictionary<long, TimeSpan>();

            // Minium
            TimeSpan? minPCR = null;

            // Open it
            using (var streamManager = new SourceStreamsManager( Device, Profile, station.Source, selection ))
            {
                // Attach PCR handler
                streamManager.OnWritingPCR += ( path, position, packet ) =>
                    {
                        // Test
                        Assert.AreEqual( tempFile.FullName, path );

                        // Get the PCR
                        TimeSpan pcr = Manager.GetPCR( packet );

                        // Remember minimum
                        if (!minPCR.HasValue)
                            minPCR = pcr;

                        // Remember
                        pcrData.Add( position, pcr );
                    };

                // Send to file
                streamManager.CreateStream( tempFile.FullName, station );

                // Process
                Thread.Sleep( 10000 );

                // Done
                streamManager.CloseStream();
            }

            // Must have some
            Assert.IsTrue( minPCR.HasValue );

            // Report
            foreach (var pair in pcrData.OrderBy( p => p.Key ))
                Console.WriteLine( "{0:000000000}\t{1}\t{2}", pair.Key, pair.Value, pair.Value - minPCR.Value );
        }

        /// <summary>
        /// Prüft die Unterteilung in mehrere Dateien.
        /// </summary>
        [Test]
        public void SplitFiles()
        {
            // Prepare receiption
            var station = GetDefaultStation();

            // Prepare selection
            var selection = new StreamSelection { Videotext = true, ProgramGuide = false };

            // Add all
            selection.AC3Tracks.LanguageMode = LanguageModes.All;
            selection.MP2Tracks.LanguageMode = LanguageModes.All;
            selection.SubTitles.LanguageMode = LanguageModes.All;

            // Get a file name
            var tempFile = GetUniqueFile();

            // Get the file name pattern
            var filePattern = Path.Combine( tempFile.DirectoryName, Path.GetFileNameWithoutExtension( tempFile.Name ) );

            // Open it
            using (var streamManager = new SourceStreamsManager( Device, Profile, station.Source, selection ))
            {
                // Send to file
                streamManager.CreateStream( string.Format( FileNamePattern, filePattern, 0 ), station );

                // Process
                for (int i = 0; i++ < 3; Thread.Sleep( 10000 ))
                {
                    // Delay - first and last will have only half the size
                    Thread.Sleep( 10000 );

                    // New file
                    Assert.IsTrue( streamManager.SplitFile( string.Format( FileNamePattern, filePattern, i ) ) );
                }

                // Done
                streamManager.CloseStream();

                // Report
                foreach (var file in streamManager.AllFiles)
                    Console.WriteLine( file );
            }
        }
    }
}
