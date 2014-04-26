using System;
using System.Linq;
using System.Threading;
using JMS.DVB;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JMS.TV.Core.UnitTests
{
    /// <summary>
    /// Verifizierte die Funktionalität der Senderverwaltung.
    /// </summary>
    [TestClass]
    public class FeedSetUnitTest
    {
        /// <summary>
        /// Um eine Senderverwaltung anzulegen muss eine entsprechende Kernverwaltung angegeben
        /// werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void FeedSetCanNotBeCreatedWithoutAProvider()
        {
            // Create will fail
            TvController.CreateFeedSet( null );
        }

        /// <summary>
        /// Eine neu erzeugte Senderverwaltung kennt keinen primären Sender.
        /// </summary>
        [TestMethod]
        public void FeedSetInitiallyDoesNotProvideAPrimaryFeed()
        {
            // Create component under test
            var cut = FeedProviderMock.CreateDefault().CreateFeedSet();

            // Check it
            Assert.IsFalse( cut.Feeds.Any(), "feeds found" );
            Assert.IsNull( cut.PrimaryView, "primary found" );
            Assert.IsFalse( cut.SecondaryViews.Any(), "secondaries found" );
        }

        /// <summary>
        /// Bei dem Zugriff auf eine ungültige Quelle wird eine Ausnahme ausgelöst.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void InvalidSourceForPrimaryViewWillThrowException()
        {
            // Create component under test
            var cut = FeedProviderMock.CreateDefault().CreateFeedSet();

            // Check it
            cut.TryPrimary( "BBC 12" );
        }

        /// <summary>
        /// Es ist möglich, den primären Sender zu wählen.
        /// </summary>
        [TestMethod]
        public void CanSetInitialPrimaryView()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = provider.CreateFeedSet();

            // Process
            Assert.IsTrue( cut.TryPrimary( "WDR" ), "choose" );

            // Ask for validation
            provider.AssertDevice( 0, "WDR" );
            provider.AssertIdle( 1, 2, 3 );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary not found" );
            Assert.IsFalse( cut.SecondaryViews.Any(), "secondaries found" );
        }

        /// <summary>
        /// Es ist möglich, den primären Sender zu ändern.
        /// </summary>
        [TestMethod]
        public void CanChangeInitialPrimaryView()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = provider.CreateFeedSet();

            // Process
            Assert.IsTrue( cut.TryPrimary( "WDR" ), "choose 1" );
            Assert.IsTrue( cut.TryPrimary( "ARD" ), "choose 2" );
            Assert.IsTrue( cut.TryPrimary( "VOX" ), "choose 3" );

            // Ask for validation
            provider.AssertDevice( 0, "VOX" );
            provider.AssertIdle( 1, 2, 3 );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary not found" );
            Assert.IsFalse( cut.SecondaryViews.Any(), "secondaries found" );
        }

        /// <summary>
        /// Ein PiP kann ein Gerät mitbenutzen.
        /// </summary>
        [TestMethod]
        public void CanActivateSecondaryOnSameDevice()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = provider.CreateFeedSet();

            // Process
            Assert.IsTrue( cut.TryPrimary( "WDR" ), "primary" );
            Assert.IsTrue( cut.TrySecondary( "ARD" ), "secondary" );

            // Ask for validation
            provider.AssertDevice( 0, "ARD", "WDR" );
            provider.AssertIdle( 1, 2, 3 );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary not found" );
            Assert.AreEqual( 1, cut.SecondaryViews.Count(), "#secondaries" );
        }

        /// <summary>
        /// Ein PiP kann ein neues Gerät verwenden.
        /// </summary>
        [TestMethod]
        public void CanActivateSecondaryOnSecondDevice()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = provider.CreateFeedSet();

            // Process
            Assert.IsTrue( cut.TryPrimary( "WDR" ), "primary" );
            Assert.IsTrue( cut.TrySecondary( "VOX" ), "secondary" );

            // Ask for validation
            provider.AssertDevice( 0, "WDR" );
            provider.AssertDevice( 1, "VOX" );
            provider.AssertIdle( 2, 3 );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary not found" );
            Assert.AreEqual( 1, cut.SecondaryViews.Count(), "#secondaries" );
        }

        /// <summary>
        /// Ein PiP kann eine primäre Anzeige nicht deaktivieren.
        /// </summary>
        [TestMethod]
        public void SecondaryWillNotDeactivatePrimary()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault( 1 );
            var cut = provider.CreateFeedSet();

            // Process
            Assert.IsTrue( cut.TryPrimary( "WDR" ), "primary" );
            Assert.IsFalse( cut.TrySecondary( "VOX" ), "secondary" );

            // Ask for validation
            provider.AssertDevice( 0, "WDR" );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary not found" );
            Assert.AreEqual( 0, cut.SecondaryViews.Count(), "#secondaries" );
        }

        /// <summary>
        /// Sekundäre Sender können abgeschaltet werden, wenn ein neuer primärer
        /// Sender angefordert wird.
        /// </summary>
        [TestMethod]
        public void WillSwitchOffSecondaryToUsePrimary()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault( 2 );
            var cut = provider.CreateFeedSet();

            // Process
            Assert.IsTrue( cut.TrySecondary( "RTL" ), "secondary 1" );
            Assert.IsTrue( cut.TrySecondary( "Pro7" ), "secondary 2" );

            // Ask for validation
            provider.AssertDevice( 0, "RTL" );
            provider.AssertDevice( 1, "Pro7" );

            // Process
            Assert.IsTrue( cut.TryPrimary( "Sat1" ), "primary 1" );

            // Ask for validation
            provider.AssertDevice( 0, "RTL" );
            provider.AssertDevice( 1, "Pro7", "Sat1" );

            // Process
            Assert.IsTrue( cut.TrySecondary( "VOX" ), "secondary 3" );
            Assert.IsTrue( cut.TryPrimary( "ARD" ), "primary 2" );

            // Ask for validation
            provider.AssertDevice( 0, "RTL", "VOX" );
            provider.AssertDevice( 1, "ARD" );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary not found" );
            Assert.AreEqual( 2, cut.SecondaryViews.Count(), "#secondaries" );
        }

        /// <summary>
        /// Wird ein sekundärer Sender zum primären, so wird der vorher sekundären deaktiviert.
        /// </summary>
        [TestMethod]
        public void PrimaryViewRequestWillDeactiveSecondary()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = provider.CreateFeedSet();

            // Process
            Assert.IsTrue( cut.TryPrimary( "RTL" ), "primary 1" );
            Assert.IsTrue( cut.TrySecondary( "VOX" ), "secondary" );
            Assert.IsTrue( cut.TryPrimary( "VOX" ), "primary 2" );

            // Validate
            Assert.AreSame( cut.FindFeed( "VOX" ), cut.PrimaryView, "primary" );
            Assert.IsFalse( cut.SecondaryViews.Any(), "#secondary" );
        }

        /// <summary>
        /// Es kann eine einzelne Aufzeichnung gestartet werden.
        /// </summary>
        [TestMethod]
        public void CanStartStandAloneRecording()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = provider.CreateFeedSet();

            // Start recording
            Assert.IsTrue( cut.TryRecord( "ARD", "0" ), "record" );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary" );
            Assert.AreSame( cut.PrimaryView, cut.Recordings.Single(), "#recordings" );

            // Stop it
            cut.StopRecordingFeed( "ARD", "0" );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary" );
            Assert.IsFalse( cut.Recordings.Any(), "#recordings" );
        }

        /// <summary>
        /// Es ist möglich, den gerade angezeigten Sender aufzuzeichnen.
        /// </summary>
        [TestMethod]
        public void CanRecordPrimaryView()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault( 1 );
            var cut = provider.CreateFeedSet();

            // Start recording
            Assert.IsTrue( cut.TryPrimary( "ARD" ), "primary" );
            Assert.IsTrue( cut.TryRecord( "ARD", "0" ), "record" );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary" );
            Assert.AreSame( cut.PrimaryView, cut.Recordings.Single(), "#recordings" );
        }

        /// <summary>
        /// Es kann eine parallele Aufzeichnung gestartet werden.
        /// </summary>
        [TestMethod]
        public void CanRecordParallelToPrimaryView()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault( 1 );
            var cut = provider.CreateFeedSet();

            // Start recording
            Assert.IsTrue( cut.TryPrimary( "ARD" ), "primary" );
            Assert.IsTrue( cut.TryRecord( "WDR", "0" ), "record" );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary" );
            Assert.AreNotSame( cut.PrimaryView, cut.Recordings.Single(), "#recordings" );
        }

        /// <summary>
        /// Eine Aufzeichnung kann den aktuellen Sender deaktivieren.
        /// </summary>
        [TestMethod]
        public void RecordMayTerminatePrimaryView()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault( 1 );
            var cut = provider.CreateFeedSet();

            // Start recording
            Assert.IsTrue( cut.TryPrimary( "ARD" ), "primary" );
            Assert.IsTrue( cut.TryRecord( "VOX", "0" ), "record" );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary" );
            Assert.AreSame( cut.FindFeed( "VOX" ), cut.PrimaryView, "new primary" );
            Assert.AreSame( cut.PrimaryView, cut.Recordings.Single(), "#recordings" );
        }

        /// <summary>
        /// Eine Aufzeichnung kann eine andere nicht stoppen.
        /// </summary>
        [TestMethod]
        public void NewRecordingWillNotStopAnotherRecording()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault( 1 );
            var cut = provider.CreateFeedSet();

            // Start recording
            Assert.IsTrue( cut.TryRecord( "VOX", "0" ), "record 1" );
            Assert.IsFalse( cut.TryRecord( "MDR", "1" ), "record 2" );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary" );
            Assert.AreSame( cut.FindFeed( "VOX" ), cut.PrimaryView, "primary" );
            Assert.AreSame( cut.PrimaryView, cut.Recordings.Single(), "#recordings" );

            provider.AssertDevice( 0, "VOX" );
        }

        /// <summary>
        /// Eine Aufzeichnung wird ein sekundäres Betrachten beenden.
        /// </summary>
        [TestMethod]
        public void NewRecordingWillStopSecondary()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault( 1 );
            var cut = provider.CreateFeedSet();

            // Start recording
            Assert.IsTrue( cut.TrySecondary( "MDR" ), "secondary" );
            Assert.IsTrue( cut.TryRecord( "VOX", "0" ), "record" );

            // Test
            Assert.IsFalse( cut.SecondaryViews.Any(), "#secondaries" );
        }

        /// <summary>
        /// Eine Senderauswahl kann durch eine laufende Aufzeichnung unterdrückt werden.
        /// </summary>
        [TestMethod]
        public void CanNotStartPrimaryViewWhileRecording()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault( 1 );
            var cut = provider.CreateFeedSet();

            // Start recording
            Assert.IsTrue( cut.TryRecord( "VOX", "0" ), "record" );
            Assert.IsFalse( cut.TryPrimary( "MDR" ), "primary" );
            Assert.IsFalse( cut.TrySecondary( "Pro7" ), "secondary" );

            // Validate
            Assert.AreSame( cut.FindFeed( "VOX" ), cut.PrimaryView, "primary" );
            Assert.AreSame( cut.PrimaryView, cut.Recordings.Single(), "recording" );
            Assert.IsFalse( cut.SecondaryViews.Any(), "#secondaries" );
        }

        /// <summary>
        /// Es ist möglich, die Senderverwaltung zu beenden.
        /// </summary>
        [TestMethod]
        public void CanTerminate()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = provider.CreateFeedSet();

            // Start all
            Assert.IsTrue( cut.TryPrimary( "MDR" ), "primary" );
            Assert.IsTrue( cut.TrySecondary( "Pro7" ), "secondary" );
            Assert.IsTrue( cut.TryRecord( "VOX", "0" ), "record 1" );
            Assert.IsTrue( cut.TryRecord( "RTL", "1" ), "record 2" );

            // Validate
            provider.AssertDevice( 0, "MDR" );
            provider.AssertDevice( 1, "Pro7" );
            provider.AssertDevice( 2, "VOX", "RTL" );
            provider.AssertIdle( 3 );

            // Request termination
            cut.Shutdown();
        }

        /// <summary>
        /// Es ist möglich, Daten zum aktuellen Sender zu ermitteln.
        /// </summary>
        [TestMethod]
        public void CanRetrieveSourceInformationForPrimaryView()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var info = default( SourceInformation );
            var cut = provider.CreateFeedSet();
            var sync = new object();

            // Register
            cut.PrimaryViewVisibilityChanged += async ( feed, enabled ) =>
                {
                    if (!enabled)
                        return;

                    info = await feed.SourceInformationReader;

                    lock (sync)
                        Monitor.Pulse( sync );
                };

            // Start
            Assert.IsTrue( cut.TryPrimary( "MDR" ), "primary" );

            // Wait for result
            lock (sync)
                while (info == null)
                    Monitor.Wait( sync );

            // Test
            Assert.IsNotNull( info, "info" );
            Assert.AreEqual( provider.Translate( "MDR" ).Source, info.Source, "source" );

            // Do proper shutdown
            cut.Shutdown();
        }
    }
}
