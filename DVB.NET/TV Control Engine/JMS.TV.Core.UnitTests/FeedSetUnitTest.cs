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
            var cut = FeedSet.Create( FeedProviderMock.Default );

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
            var cut = FeedSet.Create( FeedProviderMock.Default );

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
            var provider = FeedProviderMock.Default;
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryChangePrimaryView( "WDR" ), "choose" );

            // Ask for validation
            provider.AssertDevice( 0, "WDR" );
        }

        /// <summary>
        /// Es ist möglich, den primären Sender zu ändern.
        /// </summary>
        [TestMethod]
        public void CanChangeInitialPrimaryView()
        {
            // Create component under test
            var provider = FeedProviderMock.Default;
            var cut = FeedSet.Create( provider );

            // Process
            Assert.IsTrue( cut.TryChangePrimaryView( "WDR" ), "choose 1" );
            Assert.IsTrue( cut.TryChangePrimaryView( "ARD" ), "choose 2" );
            Assert.IsTrue( cut.TryChangePrimaryView( "VOX" ), "choose 3" );

            // Ask for validation
            provider.AssertDevice( 0, "VOX" );
        }
    }
}
