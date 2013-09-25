using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Beschreibt eine erweiterte Identifikation einer Quelle.
    /// </summary>
    public class SourceIdenfierWithKey
    {
        /// <summary>
        /// Die zu verwaltende Quelle.
        /// </summary>
        public SourceIdentifier Source { get; private set; }

        /// <summary>
        /// Die zugehörige eindeutige Kennung.
        /// </summary>
        public Guid UniqueIdentifier { get; private set; }

        /// <summary>
        /// Erzeugt eine Identifikation.
        /// </summary>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung dieser Quelle.</param>
        /// <param name="source">Die zugehörige Quelle.</param>
        public SourceIdenfierWithKey( Guid uniqueIdentifier, SourceIdentifier source )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );

            // Remember            
            UniqueIdentifier = uniqueIdentifier;
            Source = source;
        }

        /// <summary>
        /// Meldet einen Kurzschlüssel für diese Quelle.
        /// </summary>
        /// <returns>Der gewünschte Kurzschlüssel.</returns>
        public override int GetHashCode()
        {
            // Merge
            return (311 * Source.GetHashCode()) ^ UniqueIdentifier.GetHashCode();
        }

        /// <summary>
        /// Vergleicht diese Quelle mit einem beliebigen Objekt.
        /// </summary>
        /// <param name="obj">Ein anderes Objekt.</param>
        /// <returns>Gesetzt, wenn das andere Objekt die gleiche Quelle mit dme gleichen Schlüssel bezeichnet.</returns>
        public override bool Equals( object obj )
        {
            // Check type
            var other = obj as SourceIdenfierWithKey;
            if (other == null)
                return false;

            // Forward
            return Source.Equals( other.Source ) && (UniqueIdentifier == other.UniqueIdentifier);
        }
    }
}
