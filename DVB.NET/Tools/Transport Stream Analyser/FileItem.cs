using System;
using System.IO;
using System.Xml;
using JMS.DVB.TS;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Transport_Stream_Analyser
{
    /// <summary>
    /// Beschreibt eine einzelne, bereits analysierte Datei.
    /// </summary>
    public class FileItem : ListViewItem
    {
        /// <summary>
        /// Informationen zu allen Datenströmen in dieser Datei.
        /// </summary>
        public readonly List<StreamItem> Streams = new List<StreamItem>();

        /// <summary>
        /// Wird während der Deserialisierung verwendet.
        /// </summary>
        public FileItem()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="path">Der volle Pfad zur zugehörigen Datei.</param>
        public FileItem( string path )
            : base( path )
        {
            // Allocate sub items
            for (int i = 12; i-- > 0; )
                SubItems.Add( string.Empty );
        }

        /// <summary>
        /// Überträgt dieses Listenelement in eine XML Form.
        /// </summary>
        /// <param name="parent">Das übergeordnete Listenelement.</param>
        /// <returns>Das neu erzeugte Element.</returns>
        public XmlElement SaveToXml( XmlNode parent )
        {
            // Self
            var self = AnalyserMain.SaveToXml( this, parent );

            // Children
            foreach (var stream in Streams)
                stream.SaveToXml( self );

            // Report
            return self;
        }

        /// <summary>
        /// Rekonstruiert diesen Eintrag aus der XML Form.
        /// </summary>
        /// <param name="node">Die XML Form.</param>
        /// <returns>Der rekonstruierte Eintrag.</returns>
        public static FileItem LoadFromXml( XmlElement node )
        {
            // Process
            var self = AnalyserMain.LoadFromXml<FileItem>( node );

            // All children
            foreach (XmlElement child in node.SelectNodes( typeof( StreamItem ).Name ))
                self.Streams.Add( StreamItem.LoadFromXml( child ) );

            // Report
            return self;
        }

        /// <summary>
        /// Analysiert die aktuelle Datei.
        /// </summary>
        public void Analyse()
        {
            // Reset
            Streams.Clear();

            // Be safe
            try
            {
                // First run to get the stream analysis
                using (var parser = new TSParser { FillStatistics = true })
                {
                    // Fill it
                    ReadFile( parser );

                    // Load statistics
                    foreach (var info in parser.PacketStatistics.OrderBy( p => p.Key ))
                        Streams.Add( new StreamItem( info.Key, info.Value ) );

                    // Add statistics
                    SubItems[2].Text = parser.BytesReceived.ToString( "N0" );
                    SubItems[3].Text = parser.BytesSkipped.ToString( "N0" );
                    SubItems[4].Text = parser.Callbacks.ToString();
                    SubItems[5].Text = parser.Resynchronized.ToString();
                    SubItems[6].Text = parser.Scrambled.ToString( "N0" );
                    SubItems[7].Text = parser.TransmissionErrors.ToString();
                    SubItems[8].Text = parser.ValidPATCount.ToString();
                    SubItems[9].Text = parser.PacketsReceived.ToString( "N0" );

                    // Check mode
                    if (parser.ValidPATCount > 0)
                        SubItems[10].Text = Math.Round( parser.BytesReceived * 1.0 / parser.ValidPATCount ).ToString( "N0" );
                }

                // First run to do detail analysis
                using (var parser = new TSParser())
                {
                    // Connect
                    foreach (var stream in Streams)
                        stream.RegisterPhase1( parser );

                    // Fill it
                    ReadFile( parser );
                }

                // Second run to do get SI Tables
                using (var parser = new TSParser())
                {
                    // Connect
                    foreach (var stream in Streams)
                        stream.RegisterPhase2( parser );

                    // Fill it
                    ReadFile( parser );
                }

                // See if PAT is available
                StreamItem pat = Streams.FirstOrDefault( s => s.PID == 0 );
                if (pat != null)
                    SubItems[11].Text = string.Join( ", ", pat.TransportIdentifiers.Select( t => t.ToString() ).ToArray() );

            }
            catch (Exception e)
            {
                // Remember
                SubItems[12].Text = e.Message;
            }

            // Finish
            foreach (var stream in Streams)
                stream.RefreshUI();
        }

        /// <summary>
        /// Überträgt den Dateiinhalt zur Analyse.
        /// </summary>
        /// <param name="target">Die Analyseinstanz, die befüllt werden soll.</param>
        private void ReadFile( TSParser target )
        {
            // Create buffer to read file
            var buffer = new byte[100000];

            // Open the file
            using (var file = new FileStream( Text, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length ))
            {
                // Load the file size
                SubItems[1].Text = Math.Round( file.Length / 1024.0 / 1024.0 ).ToString( "N0" );

                // Load
                for (int n; (n = file.Read( buffer, 0, buffer.Length )) > 0; )
                    target.AddPayload( buffer, 0, n );
            }
        }
    }
}
