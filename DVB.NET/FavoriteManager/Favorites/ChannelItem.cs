using System;


namespace JMS.DVB.Favorites
{
    /// <summary>
    /// Repräsentiert eine einzelne Quelle.
    /// </summary>
    public class ChannelItem : IComparable
    {
        /// <summary>
        /// Die Nummer der Quelle.
        /// </summary>
        public string Index = null;

        /// <summary>
        /// Der Name der Quelle.
        /// </summary>
        public readonly string ChannelName;

        /// <summary>
        /// Weitere Detailinformationen zur Quelle.
        /// </summary>
        public readonly object Context;

        /// <summary>
        /// Erstellt eine neue Quelle.
        /// </summary>
        /// <param name="channelName">Der Name der Quelle.</param>
        /// <param name="context">Zugehörige Detailinformationen.</param>
        public ChannelItem( string channelName, object context )
        {
            // Remember all
            ChannelName = channelName;
            Context = context;
        }

        /// <summary>
        /// Erstellt einen Anzeigenamen zur Quelle.
        /// </summary>
        /// <returns>Der gewünschte Anzeigename.</returns>
        public override string ToString()
        {
            // Report inner name
            if (null == Index) return ChannelName;

            // Merge
            return string.Format( ChannelSelector.IndexFormat, Index, ChannelName );
        }

        #region IComparable Members

        /// <summary>
        /// Fragt eine Ordnung ab.
        /// </summary>
        /// <param name="obj">Irgendein .NET Objekt.</param>
        /// <returns>Die Differenz der Ordnungszahlen zweier Quellen.</returns>
        public int CompareTo( object obj )
        {
            // Change type
            ChannelItem other = obj as ChannelItem;

            // Not comparable
            if (null == other) return -1;

            // By name
            return ToString().CompareTo( other.ToString() );
        }

        #endregion
    }
}
