using System;
using JMS.DVB;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace CardServerTester
{
    /// <summary>
    /// Bietet dem Anwender einen Sender zur Auswahl an.
    /// </summary>
    public class SourceItem
    {
        /// <summary>
        /// Die eindeutige Auswahl der Quelle.
        /// </summary>
        public SourceSelection Selection { get; private set; }

        /// <summary>
        /// Der eindeutige (!) Name des Senders.
        /// </summary>
        private string Name;

        /// <summary>
        /// Erzeugt eine neue Auswahl.
        /// </summary>
        private SourceItem()
        {
        }

        /// <summary>
        /// Meldet den eindeutigen (!) Namen des Senders.
        /// </summary>
        /// <returns>Ein eindeutiger Name.</returns>
        public override string ToString()
        {
            // Report
            return Name;
        }

        /// <summary>
        /// Ermittelt alle Sender zu einerm Geräteprofil.
        /// </summary>
        /// <param name="profile">Das gewünschte Geräteprofil.</param>
        /// <returns>Alle Sender, versehen mit eindeutigen Namen.</returns>
        public static SourceItem[] GetSourceItems( Profile profile )
        {
            // The list
            List<SourceItem> items = new List<SourceItem>();

            // All items
            foreach (SourceSelection source in profile.AllSources)
            {
                // Attach to station in it
                Station station = source.Source as Station;

                // Ups
                if (null == station)
                    continue;

                // TV and radio only
                if (station.SourceType != SourceTypes.TV)
                    if (station.SourceType != SourceTypes.Radio)
                        if (!station.IsService)
                            continue;

                // Create the item
                items.Add( new SourceItem { Selection = source, Name = station.FullName } );
            }

            // First duplicate pass - use service identifier
            RemoveDuplicatesAddService( items );

            // Sort
            items.Sort( ( l, r ) => l.Name.CompareTo( r.Name ) );

            // Report
            return items.ToArray();
        }

        /// <summary>
        /// Erkennt Sender mit identischem Namen und ergänzt die Servicekennung im Namen.
        /// </summary>
        /// <param name="items">Alle Sender.</param>
        private static void RemoveDuplicatesAddService( List<SourceItem> items )
        {
            // Helper
            Dictionary<string, bool> duplicates = new Dictionary<string, bool>();

            // Check all
            foreach (SourceItem item in items)
                if (duplicates.ContainsKey( item.Name ))
                    duplicates[item.Name] = true;
                else
                    duplicates[item.Name] = false;

            // Update names
            foreach (SourceItem item in items)
                if (duplicates[item.Name])
                    item.Name += string.Format( " {{Service {0}}}", item.Selection.Source.Service );
        }

        /// <summary>
        /// Ermittelt den Namen einer Aufzeichnungsdatei für diese Quelle.
        /// </summary>
        /// <param name="directory">Das Verzeichnis, in dem die Aufzeichnungsdatei abgelegt
        /// werden soll.</param>
        /// <returns>Die Name der Aufzeichnungsdatei oder <i>null</i>, wenn kein Aufzeichnungsverzeichnis
        /// angegeben wurde.</returns>
        public string GetRecordingPath( string directory )
        {
            // See if path is available
            if (string.IsNullOrEmpty( directory ))
                return null;

            // Get our name
            string path = ToString();

            // Correct
            foreach (char ch in Path.GetInvalidFileNameChars())
                path = path.Replace( ch, '_' );

            // Finish
            return Path.Combine( directory, string.Format( "{0:yyyy-MM-dd-HH-mm-ss} {1}.ts", DateTime.Now, path ) );
        }
    }
}
