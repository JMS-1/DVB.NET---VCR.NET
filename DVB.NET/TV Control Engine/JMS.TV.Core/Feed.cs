using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt einen einzelnen Sender - in der ersten Version wird es nur Fernsehsender
    /// geben.
    /// </summary>
    public abstract class Feed
    {
        /// <summary>
        /// Erstellt die Beschreibung eines Senders.
        /// </summary>
        internal Feed()
        {
        }
    }

    /// <summary>
    /// Beschreibt einen einzelnen Sender - in der ersten Version wird es nur Fernsehsender
    /// geben.
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    internal class Feed<TSourceType> : Feed
    {
        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        public Feed()
        {
        }
    }
}
