using System;
using System.Linq;
using System.Collections.Generic;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Bietet eine Quelle zur Auswahl an.
    /// </summary>
    public class SourceItem
    {
        /// <summary>
        /// Die Quelle zu dieser Auswahl.
        /// </summary>
        public SourceSelection Source { get; private set; }

        /// <summary>
        /// Die Quelle vor dieser Auswahl.
        /// </summary>
        public SourceSelection PreviousSource { get; set; }

        /// <summary>
        /// Die Quelle nach dieser Auswahl.
        /// </summary>
        public SourceSelection NextSource { get; set; }

        /// <summary>
        /// Erzeugt eine neue Auswahl.
        /// </summary>
        /// <param name="source">Die zu verwaltende Quelle.</param>
        public SourceItem( SourceSelection source )
        {
            // Remember
            Source = source;
        }

        /// <summary>
        /// Erzeugt den Namen der Quelle, der zur Auswahl angeboten werden soll.
        /// </summary>
        /// <returns>Der Anzeigename der Quelle.</returns>
        public override string ToString()
        {
            // Check duplicate names backward
            if (null != PreviousSource)
                if (PreviousSource.DisplayName.Equals( Source.DisplayName, StringComparison.InvariantCultureIgnoreCase ))
                    return Source.QualifiedName;

            // Check duplicate names forward
            if (null != NextSource)
                if (NextSource.DisplayName.Equals( Source.DisplayName, StringComparison.InvariantCultureIgnoreCase ))
                    return Source.QualifiedName;

            // Can use short name
            return Source.DisplayName;
        }

        /// <summary>
        /// Meldet alle Quellen zu einem Geräteprofil vorbereitet zur Anzeige.
        /// </summary>
        /// <param name="profile">Das gewünschte Geräteprofil.</param>
        /// <returns>Die sortierte Liste aller Quellen.</returns>
        public static List<SourceItem> CreateSortedListFromProfile( Profile profile )
        {
            // Create list
            List<SourceItem> items =
                profile
                    .AllSources
                    .Where( s => { Station st = (Station) s.Source; return st.IsService || (st.SourceType != SourceTypes.Unknown); } )
                    .Select( s => new SourceItem( s ) )
                    .ToList();

            // Sort by unique name
            items.Sort( ( l, r ) => string.Compare( l.Source.QualifiedName, r.Source.QualifiedName, StringComparison.InvariantCultureIgnoreCase ) );

            // Link together
            for (int i = 0; i < items.Count; )
            {
                // Back
                if (i > 0)
                    items[i].PreviousSource = items[i - 1].Source;

                // Forward
                if (++i < items.Count)
                    items[i - 1].NextSource = items[i].Source;
            }

            // Report
            return items;
        }
    }
}
