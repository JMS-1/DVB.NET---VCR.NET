using System;
using System.Linq;
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
            FeedSet.Create( default( FeedProviderMock ) );
        }

        /// <summary>
        /// Eine neu erzeugte Senderverwaltung kennt keinen primären Sender.
        /// </summary>
        [TestMethod]
        public void FeedSetInitiallyDoesNotProvideAPrimaryFeed()
        {
            // Create component under test
            var cut = FeedSet.Create( FeedProviderMock.CreateDefault() );

            // Check it
            Assert.IsFalse( cut.Any(), "feeds found" );
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
            var cut = FeedSet.Create( FeedProviderMock.CreateDefault() );

            // Check it
            cut.TryStartPrimaryFeed( "BBC 12" );
        }

        /// <summary>
        /// Es ist möglich, den primären Sender zu wählen.
        /// </summary>
        [TestMethod]
        public void CanSetInitialPrimaryView()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryStartPrimaryFeed( "WDR" ), "choose" );

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
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryStartPrimaryFeed( "WDR" ), "choose 1" );
            Assert.IsTrue( cut.TryStartPrimaryFeed( "ARD" ), "choose 2" );
            Assert.IsTrue( cut.TryStartPrimaryFeed( "VOX" ), "choose 3" );

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
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryStartPrimaryFeed( "WDR" ), "primary" );
            Assert.IsTrue( cut.TryStartSecondaryFeed( "ARD" ), "secondary" );

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
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryStartPrimaryFeed( "WDR" ), "primary" );
            Assert.IsTrue( cut.TryStartSecondaryFeed( "VOX" ), "secondary" );

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
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryStartPrimaryFeed( "WDR" ), "primary" );
            Assert.IsFalse( cut.TryStartSecondaryFeed( "VOX" ), "secondary" );

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
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryStartSecondaryFeed( "RTL" ), "secondary 1" );
            Assert.IsTrue( cut.TryStartSecondaryFeed( "Pro7" ), "secondary 2" );

            // Ask for validation
            provider.AssertDevice( 0, "RTL" );
            provider.AssertDevice( 1, "Pro7" );

            // Process
            Assert.IsTrue( cut.TryStartPrimaryFeed( "Sat1" ), "primary 1" );

            // Ask for validation
            provider.AssertDevice( 0, "RTL" );
            provider.AssertDevice( 1, "Pro7", "Sat1" );

            // Process
            Assert.IsTrue( cut.TryStartSecondaryFeed( "VOX" ), "secondary 3" );
            Assert.IsTrue( cut.TryStartPrimaryFeed( "ARD" ), "primary 2" );

            // Ask for validation
            provider.AssertDevice( 0, "RTL", "VOX" );
            provider.AssertDevice( 1, "ARD" );

            // Test
            Assert.IsNotNull( cut.PrimaryView, "primary not found" );
            Assert.AreEqual( 2, cut.SecondaryViews.Count(), "#secondaries" );
        }

        /// <summary>
        /// Wird ein sekundärer Sender zum primären, so wird der vorher primäre zum sekundären.
        /// </summary>
        [TestMethod]
        public void PrimaryViewRequestMaySwapWithActiveSecondary()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryStartPrimaryFeed( "RTL" ), "primary 1" );
            Assert.IsTrue( cut.TryStartSecondaryFeed( "VOX" ), "secondary" );
            Assert.IsTrue( cut.TryStartPrimaryFeed( "VOX" ), "primary 2" );

            // Validate
            Assert.AreSame( cut.FindFeed( "VOX" ), cut.PrimaryView, "primary" );
            Assert.AreSame( cut.FindFeed( "RTL" ), cut.SecondaryViews.Single(), "secondary" );
        }

        /// <summary>
        /// Ein Gerät, das nicht mehr in Benutzung ist, wird freigegeben.
        /// </summary>
        [TestMethod]
        public void WillReleaseDeviceWhenNoLongerUsed()
        {
            // Create component under test
            var provider = FeedProviderMock.CreateDefault();
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryStartSecondaryFeed( "VOX" ), "secondary on" );
            Assert.IsTrue( cut.TryStopSecondaryFeed( "VOX" ), "secondary off" );

            // Test
            provider.AssertIdle( 0, 1, 2, 3 );
        }
    }
}
