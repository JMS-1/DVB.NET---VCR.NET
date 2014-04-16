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
    public abstract class FeedSet
    {
        /// <summary>
        /// Initialisiert eine Senderverwaltung.
        /// </summary>
        internal FeedSet()
        {
        }

        /// <summary>
        /// Meldet den primären Sender, zu dem zum Beispiel auch Ton und Videotext
        /// verfügbar sind.
        /// </summary>
        public Feed Primary { get; private set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Die Verwaltung aller Sender.</param>
        /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
        public static FeedSet Create<TSourceType>( IFeedProvider<TSourceType> provider )
        {
            // Validate
            if (ReferenceEquals( provider, null ))
                throw new ArgumentException( "keine Senderverwaltung angegeben", "provider" );

            // Forward
            return new FeedSet<TSourceType>( provider );
        }
    }

    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    public class FeedSet<TSourceType> : FeedSet
    {
        /// <summary>
        /// Verwaltete alle verfügbaren Sender.
        /// </summary>
        private readonly IFeedProvider<TSourceType> m_provider;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Die Verwaltung aller Sender.</param>
        internal FeedSet( IFeedProvider<TSourceType> provider )
        {
            m_provider = provider;
        }
    }
}
