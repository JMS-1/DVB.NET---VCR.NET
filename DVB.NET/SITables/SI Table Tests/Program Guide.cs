using System;
using JMS.DVB.SI;
using System.Linq;
using NUnit.Framework;
using System.Threading;
using System.Configuration;
using JMS.DVB.SI.ProgramGuide;
using System.Collections.Generic;
using JMS.DVB.CardServer;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Text;

namespace JMS.DVB.SITables.Tests
{
    /// <summary>
    /// Führt einige Tests zur Analyse der elektronischen Programmzeitschrift aus.
    /// </summary>
    [TestFixture]
    public class Program_Guide : TestBase
    {
        /// <summary>
        /// Alle gesammelten Ereignisse.
        /// </summary>
        private List<Event> m_Events = new List<Event>();

        /// <summary>
        /// Erzeugt einen neuen Test.
        /// </summary>
        public Program_Guide()
        {
        }

        /// <summary>
        /// Prüft auf unbekannte Beschreibungselemente.
        /// </summary>
        [Test]
        public void CheckDescriptors()
        {
            // Process all sources
            for (int i = 0; ; i++)
            {
                // Load the source name
                string sourceName = ConfigurationManager.AppSettings[string.Format( "EPG.Source{0}", i )];
                if (string.IsNullOrEmpty( sourceName ))
                    break;

                // Report it
                Console.WriteLine( sourceName );

                // Attach to the DVB source
                var source = Profile.FindSource( sourceName )[0];

                // Forward
                RunDescriptorTest( source );
            }

            // Add BBC EPG
            RunDescriptorTest( Profile.FindSource( EIT.FreeSatEPGTriggerSource )[0], EIT.FreeSatEPGPID );
        }

        /// <summary>
        /// Führt eine vollständige Analyse der Programmzeitschrift aus.
        /// </summary>
        [Test]
        public void ParseEvents()
        {
            // Process all sources
            for (int i = 0; ; i++)
            {
                // Load the source name
                string sourceName = ConfigurationManager.AppSettings[string.Format( "EPG.Source{0}", i )];
                if (string.IsNullOrEmpty( sourceName ))
                    break;

                // Report it
                Console.WriteLine( sourceName );

                // Attach to the DVB source
                var source = Profile.FindSource( sourceName )[0];

                // Forward
                RunDescriptorTest( source, EventCollector );
            }

            // Add BBC EPG
            RunDescriptorTest( Profile.FindSource( EIT.FreeSatEPGTriggerSource )[0], EIT.FreeSatEPGPID, EventCollector );

            // Now report all
            foreach (var @event in m_Events)
            {
                // Resolve source
                var source = Profile.FindSource( @event.Source ).FirstOrDefault();

                // Source to display
                SourceIdentifier display;
                if (source == null)
                    display = @event.Source;
                else
                    display = source.Source;

                // Report
                Console.WriteLine( "{0} #{6:00000} {1}..{3:HH:mm:ss} ({2}) {5} {4}", display, @event.StartTime.ToLocalTime(), @event.Duration, @event.EndTime.ToLocalTime(), @event.Name, @event.Language, @event.Identifier );

                // Optional data
                if (!string.IsNullOrEmpty( @event.Description ))
                    Console.WriteLine( "\tDescription: {0}", @event.Description.Replace( '\r', ' ' ).Replace( '\n', ' ' ) );

                // Ratings et al
                Console.WriteLine( "\tRatings: {0}", string.Join( ", ", @event.Ratings ) );
                Console.WriteLine( "\tContent: {0}", string.Join( ", ", @event.Content.Select( c => c.ToString() ).ToArray() ) );
            }
        }

        /// <summary>
        /// Prüft die Serialisierung für die Verwendung im <i>Card Server</i>.
        /// </summary>
        [Test]
        public void SerializeGuideItems()
        {
            // Create serializer
            var serializer = new XmlSerializer( typeof( ProgramGuideItem ) );

            // Process all sources
            for (int i = 0; ; i++)
            {
                // Load the source name
                string sourceName = ConfigurationManager.AppSettings[string.Format( "EPG.Source{0}", i )];
                if (string.IsNullOrEmpty( sourceName ))
                    break;

                // Report it
                Console.WriteLine( sourceName );

                // Attach to the DVB source
                var source = Profile.FindSource( sourceName )[0];

                // Forward
                RunDescriptorTest( source, EventCollector );
            }

            // Add BBC EPG
            RunDescriptorTest( Profile.FindSource( EIT.FreeSatEPGTriggerSource )[0], EIT.FreeSatEPGPID, EventCollector );

            // Create formatting helper
            var settings = new XmlWriterSettings { Indent = true, Encoding = Encoding.Unicode };

            // Helper
            using (var stream = new MemoryStream())
                foreach (var item in m_Events.Select( e => new ProgramGuideItem( e ) ))
                {
                    // Reset stream
                    stream.SetLength( 0 );

                    // Fill stream
                    using (var writer = XmlWriter.Create( stream, settings ))
                        serializer.Serialize( writer, item );

                    // Get as string
                    string xml = Encoding.Unicode.GetString( stream.ToArray() );

                    // Reset stream
                    stream.Seek( 0, SeekOrigin.Begin );

                    // Reconstruct
                    var recall = (ProgramGuideItem) serializer.Deserialize( stream );

                    // Validate
                    Assert.AreEqual( item.Identifier, recall.Identifier, "Identifier" );
                    CollectionAssert.AreEqual( item.Content, recall.Content, "Content" );
                    Assert.AreEqual( item.Duration, recall.Duration, "Duration" );
                    Assert.AreEqual( item.End, recall.End, "End" );
                    Assert.AreEqual( item.Language, recall.Language, "Language" );
                    Assert.AreEqual( item.Name, recall.Name, "Name" );
                    CollectionAssert.AreEqual( item.Ratings, recall.Ratings, "Ratings" );
                    Assert.AreEqual( item.Source, recall.Source, "Source" );
                    Assert.AreEqual( item.Start, recall.Start, "Start" );
                }
        }

        /// <summary>
        /// Führt einen Test für eine Quellgruppe aus.
        /// </summary>
        /// <param name="source">Eine beliebige Quelle auf der Quellgruppe.</param>
        private void RunDescriptorTest( SourceSelection source )
        {
            // Forward
            RunDescriptorTest( source, EIT.DefaultStreamIdentifier );
        }

        /// <summary>
        /// Führt einen Test für eine Quellgruppe aus.
        /// </summary>
        /// <param name="source">Eine beliebige Quelle auf der Quellgruppe.</param>
        /// <param name="callback">Die Methode zur Auswertung der Programmzeitschrift.</param>
        private void RunDescriptorTest( SourceSelection source, Action<EIT> callback )
        {
            // Forward
            RunDescriptorTest( source, EIT.DefaultStreamIdentifier, callback );
        }

        /// <summary>
        /// Führt einen Test für eine Quellgruppe aus.
        /// </summary>
        /// <param name="source">Eine beliebige Quelle auf der Quellgruppe.</param>
        /// <param name="streamIdentifier">Die zu verwendende Datenstromkennung.</param>
        private void RunDescriptorTest( SourceSelection source, ushort streamIdentifier )
        {
            // Forward
            RunDescriptorTest( source, streamIdentifier, TagValidator );
        }

        /// <summary>
        /// Führt einen Test für eine Quellgruppe aus.
        /// </summary>
        /// <param name="source">Eine beliebige Quelle auf der Quellgruppe.</param>
        /// <param name="streamIdentifier">Die zu verwendende Datenstromkennung.</param>
        /// <param name="callback">Die Methode zur Auswertung der Programmzeitschrift.</param>
        private void RunDescriptorTest( SourceSelection source, ushort streamIdentifier, Action<EIT> callback )
        {
            // Report it
            Console.WriteLine( source.Source );

            // Tune it
            source.SelectGroup();

            // Attach to the device
            var device = Hardware;

            // Attach EPG consumer
            Guid consumer = device.AddConsumer<EIT>( streamIdentifier, callback );
            try
            {
                // Start it
                device.SetConsumerState( consumer, true );

                // Process a while
                Thread.Sleep( 500 );
            }
            finally
            {
                // Detach EPG consumer
                device.SetConsumerState( consumer, null );
            }
        }

        /// <summary>
        /// Sammelt die Einträge der Programmzeitschrift.
        /// </summary>
        private void EventCollector( EIT table )
        {
            // Easy
            m_Events.AddRange( table.Events.Select( e => { e.Load(); return e; } ) );
        }

        /// <summary>
        /// Sammelt die Einträge der Programmzeitschrift.
        /// </summary>
        private void TagValidator( EIT table )
        {
            // Process all descriptors
            foreach (var entry in table.Table.Entries)
                foreach (var descriptor in entry.Descriptors)
                {
                    // All we process
                    if (descriptor.Tag == EPG.DescriptorTags.ShortEvent)
                        continue;
                    if (descriptor.Tag == EPG.DescriptorTags.ExtendedEvent)
                        continue;
                    if (descriptor.Tag == EPG.DescriptorTags.ParentalRating)
                        continue;
                    if (descriptor.Tag == EPG.DescriptorTags.Linkage)
                        continue;
                    if (descriptor.Tag == EPG.DescriptorTags.Content)
                        continue;

                    // All we will process in the next release
                    if (descriptor.Tag == EPG.DescriptorTags.Component)
                        continue;

                    // All we are currently not willing to process
                    if (descriptor.Tag == EPG.DescriptorTags.PrivateDataSpecifier)
                        continue;
                    if (descriptor.Tag == EPG.DescriptorTags.PDC)
                        continue;
                    if (descriptor.Tag == EPG.DescriptorTags.ContentIdentifier)
                        continue;

                    // For now skip reserved area
                    if (descriptor.Tag > (EPG.DescriptorTags) 0x7f)
                        continue;

                    // Report
                    Console.WriteLine( "\t{0}: {1}", descriptor.Tag, descriptor.GetType() );
                }
        }

        /// <summary>
        /// Wird vor jedem Test aufgerufen.
        /// </summary>
        [SetUp]
        public void StartupTest()
        {
            // Reset earlier collections
            m_Events.Clear();
        }
    }
}
