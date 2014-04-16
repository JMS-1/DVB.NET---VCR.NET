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
        /// Die zugehörige Quelle.
        /// </summary>
        public TSourceType Source { get; private set; }

        /// <summary>
        /// Beschreibt, wie viele Verbraucher es für diesen Sender gibt.
        /// </summary>
        public int UsageCounter { get; set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="source">Die zugehörige Quelle.</param>
        public Feed( TSourceType source )
        {
            Source = source;
        }
    }
}
