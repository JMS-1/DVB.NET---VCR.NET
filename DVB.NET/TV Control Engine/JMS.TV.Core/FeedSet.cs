using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    public class FeedSet
    {
        /// <summary>
        /// Verwaltete alle verfügbaren Sender.
        /// </summary>
        private readonly IFeedProvider m_provider;

        /// <summary>
        /// Meldet den primären Sender, zu dem zum Beispiel auch Ton und Videotext
        /// verfügbar sind.
        /// </summary>
        public Feed Primary { get; private set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Die Verwaltung aller Sender.</param>
        private FeedSet( IFeedProvider provider )
        {
            m_provider = provider;
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Die Verwaltung aller Sender.</param>
        public static FeedSet Create( IFeedProvider provider )
        {
            // Validate
            if (ReferenceEquals( provider, null ))
                throw new ArgumentException( "keine Senderverwaltung angegeben", "provider" );

            // Forward
            return new FeedSet( provider );
        }
    }
}
