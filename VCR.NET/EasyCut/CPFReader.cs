using System;
using System.Xml;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace EasyCut
{
    /// <summary>
    /// Beschreibt ein einzelnes Schnittelement aus der <i>Cuttermaran</i> Projektdatai.
    /// </summary>
    public class CutElement
    {
        /// <summary>
        /// Die zugehörige Bilddatei.
        /// </summary>
        public FileInfo VideoFile { get; set; }

        /// <summary>
        /// Die Nummer der zugehörigen Bilddatei.
        /// </summary>
        public int VideoIndex { get; private set; }

        /// <summary>
        /// Das erste Bild in diesem Schnittelement.
        /// </summary>
        public long Start { get; private set; }

        /// <summary>
        /// Das letzte Bild in diesem Schnittelement.
        /// </summary>
        public long End { get; private set; }

        /// <summary>
        /// Erzeugt ein neues Schnittelement.
        /// </summary>
        /// <param name="element">Die Rohdaten aus der Projektdatei.</param>
        public CutElement( XmlElement element )
        {
            // Load all
            VideoIndex = int.Parse( element.GetAttribute( "refVideoFile" ) );
            Start = long.Parse( element.GetAttribute( "StartPosition" ) );
            End = long.Parse( element.GetAttribute( "EndPosition" ) );
        }

        /// <summary>
        /// Meldet alle Untertitelseiten, die für diesen Schnitt verfügbar sind.
        /// </summary>
        /// <param name="type">Die Art der Untertitel bezeichnet durch die Dateiendung.</param>
        /// <returns>Alle Untertitelnummer - <i>null</i> wird für DVB Untertitel gemeldet.</returns>
        public IEnumerable<int?> GetAvailableSubtitles( string type )
        {
            // Get the core name
            string coreName = Path.GetFileNameWithoutExtension( VideoFile.FullName );

            // Process all
            foreach (FileInfo candidate in VideoFile.Directory.GetFiles( "*." + type ))
                if (candidate.Name.StartsWith( coreName, StringComparison.InvariantCultureIgnoreCase ))
                {
                    // Get the inner part
                    string title = Path.GetFileNameWithoutExtension( candidate.Name ).Substring( coreName.Length );

                    // Subtitle
                    int page;

                    // DVB Subtitle
                    if (string.IsNullOrEmpty( title ))
                        yield return null;
                    else if (title.StartsWith( "[" ))
                        if (title.EndsWith( "]" ))
                            if (int.TryParse( title.Substring( 1, title.Length - 2 ), out page ))
                                if ((page >= 100) && (page <= 899))
                                    yield return page;
                }
        }
    }

    /// <summary>
    /// Eine Hilfsklasse zum Einlesen einer <i>Cuttermaran</i> Projektdatei.
    /// </summary>
    public class CPFReader
    {
        /// <summary>
        /// Der volle Pfad zur Projektdatei.
        /// </summary>
        public FileInfo ProjectPath { get; private set; }

        /// <summary>
        /// Die Projektdatei gelesen als XML Dokument.
        /// </summary>
        private XmlDocument m_Document = new XmlDocument();

        /// <summary>
        /// Verwaltung der XML Namensräume in der Projektdatei.
        /// </summary>
        private XmlNamespaceManager m_Namespaces;

        /// <summary>
        /// Erzeugt eine neue Hilfsklasse.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Projektdatei.</param>
        public CPFReader( string path )
        {
            // Remember
            ProjectPath = new FileInfo( path );

            // Load it
            m_Document.Load( ProjectPath.FullName );

            // Uses namespaces
            m_Namespaces = new XmlNamespaceManager( m_Document.NameTable );

            // Configure
            m_Namespaces.AddNamespace( "cm", "http://cuttermaran.kickme.to/StateData.xsd" );
        }

        /// <summary>
        /// Ermittelt eine Bilddatei aus der Projektdatei.
        /// </summary>
        /// <param name="videoIndex">Die laufende Nummer der Bilddatei.</param>
        /// <returns>Der Pfad zur Bilddatei.</returns>
        private FileInfo FindVideoFile( int videoIndex )
        {
            // Find the video
            XmlElement video = (XmlElement) m_Document.DocumentElement.SelectSingleNode( string.Format( "cm:usedVideoFiles[@FileID='{0}']", videoIndex ), m_Namespaces );

            // Not found
            if (null == video)
                return null;

            // Load the file
            string fileName = video.GetAttribute( "FileName" );

            // Must be set
            if ((null == fileName) || (fileName.Length < 1))
                return null;

            // Attach to video file
            FileInfo videoFile = new FileInfo( fileName );

            // Check it
            if (videoFile.Exists)
                return videoFile;
            else
                return null;
        }

        /// <summary>
        /// Meldet alle Untertitelseiten, die für diese Projektdatei überhaupt verfügbar sind.
        /// </summary>
        /// <param name="type">Die Art der Untertitel, bezeichnet durch die Dateiendung.</param>
        /// <returns>Eine Auflistung über alle Untertitel.</returns>
        public IEnumerable<int?> GetAvailableSubtitles( string type )
        {
            // See if we reported DVB subtitles
            bool hasDVB = false;

            // Helper
            Dictionary<int, bool> alreadyProcessed = new Dictionary<int, bool>();

            // Process all
            foreach (CutElement element in CutElements)
                foreach (int? page in element.GetAvailableSubtitles( type ))
                {
                    // Check for duplicates
                    if (page.HasValue)
                        if (alreadyProcessed.ContainsKey( page.Value ))
                            continue;
                        else
                            alreadyProcessed[page.Value] = true;
                    else if (hasDVB)
                        continue;
                    else
                        hasDVB = true;

                    // Forward
                    yield return page;
                }
        }

        /// <summary>
        /// Meldet alle Schnittelemente aus der Projektdatei.
        /// </summary>
        public IEnumerable<CutElement> CutElements
        {
            get
            {
                // Process all cuts
                foreach (XmlElement cut in m_Document.DocumentElement.SelectNodes( "cm:CutElements", m_Namespaces ))
                {
                    // Element to process
                    CutElement element;

                    // Load it
                    try
                    {
                        // Create
                        element = new CutElement( cut );
                    }
                    catch
                    {
                        // Skip
                        continue;
                    }

                    // Load the file
                    element.VideoFile = FindVideoFile( element.VideoIndex );

                    // Report
                    if (null != element.VideoFile)
                        yield return element;
                }
            }
        }
    }
}
