using System;
using System.Collections.Generic;
using System.Linq;
using JMS.DVB;


namespace JMS.DVBVCR.RecordingService.ProgramGuide
{
    /// <summary>
    /// Diese Klasse beschreibt eine Auswahl auf die Programmzeitschrift.
    /// </summary>
    public class GuideEntryFilter
    {
        /// <summary>
        /// Vergleichsalorithmus zum Anordnen von Einträgen.
        /// </summary>
        private static readonly IComparer<ProgramGuideEntry> Comparer = new EntryComparer();

        /// <summary>
        /// Implementiert einen Vergleichalgorithmus.
        /// </summary>
        private class EntryComparer : IComparer<ProgramGuideEntry>
        {
            /// <summary>
            /// Vergleicht zwei Einträge.
            /// </summary>
            /// <param name="left">Der erste Eintrag.</param>
            /// <param name="right">Der zweite Eintrag.</param>
            /// <returns>Der Unterschied zwischen den Einträgen.</returns>
            public int Compare( ProgramGuideEntry left, ProgramGuideEntry right )
            {
                // Test for nothing
                if (left == null)
                    if (right == null)
                        return 0;
                    else
                        return -1;
                else if (right == null)
                    return +1;

                // Start time
                var delta = left.StartTime.CompareTo( right.StartTime );
                if (delta != 0)
                    return delta;

                // Name of station - since there is no provider this may be misleading
                delta = StringComparer.InvariantCultureIgnoreCase.Compare( left.StationName, right.StationName );
                if (delta != 0)
                    return delta;

                // Try source
                var leftSource = left.Source;
                var rightSource = right.Source;

                // Test for nothing
                if (leftSource == null)
                    if (rightSource == null)
                        return 0;
                    else
                        return -1;
                else if (rightSource == null)
                    return +1;

                // Dive into
                delta = leftSource.Network.CompareTo( rightSource.Network );
                if (delta != 0)
                    return delta;
                delta = leftSource.TransportStream.CompareTo( rightSource.TransportStream );
                if (delta != 0)
                    return delta;
                delta = leftSource.Service.CompareTo( rightSource.Service );
                if (delta != 0)
                    return delta;

                // Duration
                return left.Duration.CompareTo( right.Duration );
            }
        }

        /// <summary>
        /// Der Name des zu verwendenden Geräteprofils.
        /// </summary>
        public string ProfileName { get; set; }

        /// <summary>
        /// Optional die Quelle.
        /// </summary>
        public SourceIdentifier Source { get; set; }

        /// <summary>
        /// Optional der Startzeitpunkt.
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        /// Das Suchmuster für den Titel, das erste Zeichen bestimmt den Suchmodus.
        /// </summary>
        public string TitlePattern { get; set; }

        /// <summary>
        /// Das Suchmuster für den Inhalt, das erste Zeichen bestimmt den Suchmodus.
        /// </summary>
        public string ContentPattern { get; set; }

        /// <summary>
        /// Die gewünschte Seitengröße.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Die aktuell gewünschte Seite.
        /// </summary>
        public int PageIndex;

        /// <summary>
        /// Wendet die Fiterbedingung an.
        /// </summary>
        /// <param name="entries">Eine Liste von Einträgen.</param>
        /// <returns>Die gefilterte Liste.</returns>
        public IEnumerable<ProgramGuideEntry> Filter( IEnumerable<ProgramGuideEntry> entries )
        {
            // Only use sources available to the target profile
            entries = entries.Where( entry => VCRProfiles.FindSource( ProfileName, entry.Source ) != null );

            // Name of the station - best filter first
            if (Source != null)
                entries = entries.Where( entry => Source.Equals( entry.Source ) );

            // Start time
            if (Start.HasValue)
            {
                // Later
                entries = entries.Where( entry => entry.StartTime >= Start.Value );
            }
            else
            {
                // Current
                var now = DateTime.UtcNow;

                // Still active
                entries = entries.Where( entry => entry.EndTime > now );
            }

            // Matcher on content
            Func<ProgramGuideEntry, bool> matchTitle = null;
            Func<ProgramGuideEntry, bool> matchContent = null;

            // Title
            if (!string.IsNullOrEmpty( TitlePattern ))
            {
                var title = TitlePattern.Substring( 1 );
                switch (TitlePattern[0])
                {
                    case '=': matchTitle = entry => (entry.Name ?? string.Empty).Equals( title, StringComparison.InvariantCultureIgnoreCase ); break;
                    case '*': matchTitle = entry => (entry.Name ?? string.Empty).IndexOf( title, StringComparison.InvariantCultureIgnoreCase ) >= 0; break;
                }
            }

            // Both descriptions
            if (!string.IsNullOrEmpty( ContentPattern ))
            {
                var content = ContentPattern.Substring( 1 );
                switch (ContentPattern[0])
                {
                    case '=': matchContent = entry => (entry.Description ?? string.Empty).Equals( content, StringComparison.InvariantCultureIgnoreCase ) || (entry.ShortDescription ?? string.Empty).Equals( content, StringComparison.InvariantCultureIgnoreCase ); break;
                    case '*': matchContent = entry => ((entry.Description ?? string.Empty).IndexOf( content, StringComparison.InvariantCultureIgnoreCase ) >= 0) || ((entry.ShortDescription ?? string.Empty).IndexOf( content, StringComparison.InvariantCultureIgnoreCase ) >= 0); break;
                }
            }

            // Apply content filter
            if (matchTitle != null)
                if (matchContent != null)
                    entries = entries.Where( entry => matchTitle( entry ) || matchContent( entry ) );
                else
                    entries = entries.Where( matchTitle );
            else if (matchContent != null)
                entries = entries.Where( matchContent );

            // Caller will get it all
            if (PageSize < 1)
                return entries;

            // Copy local
            var matches = entries.ToList();

            // Sort in list to improve overall performance
            matches.Sort( Comparer );

            // Adjust extract - report one more if possible to indicate that there is more available
            return matches.Skip( PageIndex * PageSize ).Take( PageSize + 1 );
        }
    }
}
