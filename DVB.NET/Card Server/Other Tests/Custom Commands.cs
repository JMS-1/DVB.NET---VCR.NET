using System;
using System.Linq;
using NUnit.Framework;
using System.Configuration;
using Card_Server_Extender;

namespace JMS.DVB.CardServer.Tests
{
    /// <summary>
    /// Tests zum Aufruf von erweiterten Befehlen an den <i>Card Server</i>.
    /// </summary>
    [TestFixture]
    public class Custom_Commands : TestBase
    {
        /// <summary>
        /// Erzeugt eine neue Testumgebung.
        /// </summary>
        public Custom_Commands()
        {
        }

        /// <summary>
        /// Wählt eine Quellgruppe an und ermittelt die Informatiationen zu den Quellen.
        /// </summary>
        [Test]
        public void GetInformations()
        {
            // Get the test source
            var testSource = Profile.FindSource( ConfigurationManager.AppSettings["Station"] )[0];

            // Activate
            ServerImplementation.EndRequest( CardServer.BeginSelect( testSource.SelectionKey ) );

            // Create request helper
            var requestor = new RequestInformation( CardServer );

            // Remember
            var sources = ((GenericInformationResponse) requestor.BeginGetGroupInformation().Result).Strings.Select( s => SourceIdentifier.Parse( s ) ).ToArray();

            // Request data
            foreach (var group in ((NetworkInformationResponse) requestor.BeginGetNetworkInformation().Result).Groups)
                Console.WriteLine( group );

            // Resolve names
            var names = ((GenericInformationResponse) requestor.BeginGetSourceInformation( sources ).Result).Strings;

            // Report
            for (int i = 0, imax = Math.Max( sources.Length, names.Length ); i < imax; i++)
                Console.WriteLine( "{0} => {1}", sources[i], names[i] );
        }
    }
}
