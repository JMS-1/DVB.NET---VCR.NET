using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft den Umgang mit Geräten.
    /// </summary>
    [TestClass]
    public class ResourceSpecs
    {
        /// <summary>
        /// Es kann kein Geräte <i>null</i> angemeldet werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentNullException ) )]
        public void A_Resource_Must_Not_Be_Null()
        {
            // Create the component under test
            var componentUnderTest = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase );

            // Register a null resource
            componentUnderTest.Add( default( IScheduleResource ) );
        }

        /// <summary>
        /// Ein Gerät kann nicht mehrfach angemeldet werden.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentException ) )]
        public void No_Resource_Can_Be_Used_Twice()
        {
            // Create the component under test
            var componentUnderTest = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase );
            var resource = ResourceMock.Create( "a" );

            // Register devices
            componentUnderTest.Add( resource );
            componentUnderTest.Add( resource );
        }

        /// <summary>
        /// Es können mehrere Geräte angemeldet werden.
        /// </summary>
        [TestMethod]
        public void Can_Add_Multiple_Resources()
        {
            // Create the component under test
            var componentUnderTest = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase );

            // Register devices
            for (int i = 0; i < 100; i++)
                componentUnderTest.Add( ResourceMock.Create( i.ToString( "00" ) ) );
        }

        /// <summary>
        /// Die Anzahl gleichzeitig entschlüsselbarer Quellen darf nicht negativ sein.
        /// </summary>
        [TestMethod, ExpectedException( typeof( ArgumentOutOfRangeException ) )]
        public void Individual_Decryption_Counter_Must_Not_Be_Negative()
        {
            // Create the component under test
            var componentUnderTest = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase );

            // Register device
            componentUnderTest.Add( ResourceMock.Create( "a" ).SetEncryptionLimit( -1 ) );
        }
    }
}
