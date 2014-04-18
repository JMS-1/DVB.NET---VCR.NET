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
        /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
        /// <typeparam name="TRecordingType">Die Art der Identifikation der Aufzeichnungen.</typeparam>
        public static IFeedSet<TRecordingType> CreateFeedSet<TSourceType, TRecordingType>( IFeedProvider<TSourceType> provider ) where TSourceType : class
        {
            // Validate
            if (ReferenceEquals( provider, null ))
                throw new ArgumentException( "keine Senderverwaltung angegeben", "provider" );
            else
                return new FeedSet<TSourceType, TRecordingType>( provider );
        }


    }
}
