using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace JMS.DVB.DirectShow.UI
{
    partial class TransitionConfiguration
    {
        /// <summary>
        /// Steuert die Umwandlung einer .NET Instanz in das zugehörige XML Format.
        /// </summary>
        private static readonly XmlSerializer s_Serializer = new XmlSerializer( typeof( TransitionConfiguration ), "http://psimarron.net/DVBNET/Transitions" );

        /// <summary>
        /// Beschreibt, wie wie Umwandlung aus dem XML Format zu erfolgen hat.
        /// </summary>
        private static readonly XmlReaderSettings s_Reader = new XmlReaderSettings { CheckCharacters = false };

        /// <summary>
        /// Beschreibt, wie die Umwandlung in das XML Format zu erfolgen hat.
        /// </summary>
        private static readonly XmlWriterSettings s_Writer = new XmlWriterSettings { CheckCharacters = false, Indent = true, Encoding = Encoding.Unicode };
        /// <summary>
        /// Erstellt die XML Repräsentation dieser Konfiguration.
        /// </summary>
        /// <param name="stream">Die Ablagestruktur für die XML Repräsentation.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Ziel angegeben.</exception>
        public void Save( Stream stream )
        {
            // Validate
            if (stream == null)
                throw new ArgumentNullException( "stream" );

            // Process
            using (var writer = XmlWriter.Create( stream, s_Writer ))
                s_Serializer.Serialize( writer, this );
        }

        /// <summary>
        /// Rekonstruiert eine Instanz aus einer XML Repräsentation.
        /// </summary>
        /// <param name="stream">Die zu verwendende Ablagestruktur.</param>
        /// <returns>Die zugehörige Instanz.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Ablagestruktur angegeben.</exception>
        public static TransitionConfiguration Load( Stream stream )
        {
            // Validate
            if (stream == null)
                throw new ArgumentNullException( "stream" );

            // Process
            using (var reader = XmlReader.Create( stream, s_Reader ))
            {
                // Load
                var config = (TransitionConfiguration) s_Serializer.Deserialize( reader );

                // Connect states to use
                foreach (var state in config.States)
                    state.Configuration = config;

                // Create maps
                config.m_States = config.States.ToDictionary( s => s.UniqueIdentifier );

                // Report
                return config;
            }
        }
    }
}
