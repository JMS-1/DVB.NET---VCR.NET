using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMS.TV.Core.UnitTests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JMS.TV.Core.UnitTests
{
    /// <summary>
    /// Testumgebung für die Gesamtanwendung.
    /// </summary>
    [TestClass]
    public class EngineTest
    {
        /// <summary>
        /// Erzeugt eine Testumgebung.
        /// </summary>
        [TestMethod]
        public void CanCreate()
        {
            using (var env = EngineMock.Create())
                Assert.IsNotNull( env );
        }
    }
}
