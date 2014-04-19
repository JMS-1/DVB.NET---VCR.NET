using System;


namespace JMS.TV.Core
{
    /// <summary>
    /// Die Steuereinheit für die Fernsehanzeige.
    /// </summary>
    public class TvController
    {
        /// <summary>
        /// Erstellt eine neue Steuereinheit.
        /// </summary>
        private TvController()
        {
        }

        /// <summary>
        /// Erstellt eine neue Steuereinheit.
        /// </summary>
        /// <returns>Die gewünschte Steuereinheit.</returns>
        public static TvController Create()
        {
            return new TvController();
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Die Verwaltung aller Sender.</param>
        public static IFeedSet CreateFeedSet( IFeedProvider provider )
        {
            // Validate
            if (ReferenceEquals( provider, null ))
                throw new ArgumentException( "keine Senderverwaltung angegeben", "provider" );
            else
                return new FeedSet( provider );
        }
    }
}
