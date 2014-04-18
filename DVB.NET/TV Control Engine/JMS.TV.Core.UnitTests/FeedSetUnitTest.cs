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
            cut.TryChangePrimaryView( "BBC 12" );
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
            Assert.IsTrue( cut.TryChangePrimaryView( "WDR" ), "choose" );

            // Ask for validation
            provider.AssertDevice( 0, "WDR" );
            provider.AssertIdle( 1, 2, 3 );
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
            Assert.IsTrue( cut.TryChangePrimaryView( "WDR" ), "choose 1" );
            Assert.IsTrue( cut.TryChangePrimaryView( "ARD" ), "choose 2" );
            Assert.IsTrue( cut.TryChangePrimaryView( "VOX" ), "choose 3" );

            // Ask for validation
            provider.AssertDevice( 0, "VOX" );
            provider.AssertIdle( 1, 2, 3 );
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
            Assert.IsTrue( cut.TryChangePrimaryView( "WDR" ), "primary" );
            Assert.IsTrue( cut.TryChangeSecondaryView( "ARD", true ), "secondary" );

            // Ask for validation
            provider.AssertDevice( 0, "ARD", "WDR" );
            provider.AssertIdle( 1, 2, 3 );
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
            Assert.IsTrue( cut.TryChangePrimaryView( "WDR" ), "primary" );
            Assert.IsTrue( cut.TryChangeSecondaryView( "VOX", true ), "secondary" );

            // Ask for validation
            provider.AssertDevice( 0, "WDR" );
            provider.AssertDevice( 1, "VOX" );
            provider.AssertIdle( 2, 3 );
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
            Assert.IsTrue( cut.TryChangePrimaryView( "WDR" ), "primary" );
            Assert.IsFalse( cut.TryChangeSecondaryView( "VOX", true ), "secondary" );

            // Ask for validation
            provider.AssertDevice( 0, "WDR" );
        }
    }
}
