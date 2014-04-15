using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JMS.TV.Core.UnitTests
{
    /// <summary>
    /// Prüft die elementaren Funktionen der Kernsteuereinheit.
    /// </summary>
    [TestClass]
    public class ControllerUnitTests
    {
        /// <summary>
        /// [tbd]
        /// </summary>
        [TestMethod]
        public void CanCreate()
        {
            // Create component under test
            var cut = TvController.Create();

            // Validate
            Assert.IsNotNull( cut, "controller" );
        }
    }
}
