using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt die Parameter einer DVB.NET Hardwareabstraktion.
    /// </summary>
    public class DeviceInformation
    {
        /// <summary>
        /// Das Wurzelelement der Konfiguration.
        /// </summary>
        private XmlElement Root { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Das Wurzelelement der Konfiguration.</param>
        internal DeviceInformation( XmlElement provider )
        {
            // Load
            Root = provider;

            // Verifiy
            if (!Root.Name.Equals( "DVBNETProvider" ))
                throw new ArgumentException( "bad provider definition", "file" );
        }

        private XmlElement FindElement( string name )
        {
            // Report
            return (XmlElement) Root.SelectSingleNode( name );
        }

        public XmlNodeList Parameters { get { return FindElement( "Parameters" ).ChildNodes; } }

        internal string UniqueIdentifier
        {
            get
            {
                // Report
                return (string) Root.GetAttribute( "id" );
            }
        }

        public override string ToString()
        {
            // Report
            return UniqueIdentifier;
        }

        /// <summary>
        /// Meldet den Namen der .NET Klasse zum Zugriff auf die DVB Hardware.
        /// </summary>
        public string DriverType { get { return FindElement( "Driver" ).InnerText; ;            } }

        public string[] Names
        {
            get
            {
                // Helper
                var names = new ArrayList();

                // All my names
                foreach (XmlNode name in Root.SelectNodes( "CardNames/CardName" ))
                    names.Add( name.InnerText );

                // Report
                return (string[]) names.ToArray( typeof( string ) );
            }
        }

        public static DeviceInformation[] Load()
        {
            // Remember
            var settings = new Hashtable();

            // Get the root
            string root = RunTimeLoader.RootDirectory.FullName;

            // Attach to the provider configuration file
            var path = new FileInfo( Path.Combine( root, "DVBNETProviders.xml" ) );
            var file = new XmlDocument();

            // Process
            if (path.Exists)
            {
                // Load the DOM from file
                file.Load( path.FullName );
            }
            else
            {
                // Get the scope
                var me = typeof( DeviceInformation );

                // Load the DOM from resource
                using (var providers = me.Assembly.GetManifestResourceStream( me.Namespace + ".DVBNETProviders.xml" ))
                    file.Load( providers );
            }

            // Verify
            if (!file.DocumentElement.Name.Equals( "DVBNETProviders" ))
                throw new ArgumentException( "bad provider definition", "file" );
            if (!Equals( file.DocumentElement.GetAttribute( "SchemaVersion" ), "3.9" ))
                throw new ArgumentException( "invalid schema version", "file" );

            // All providers
            return
                file
                    .DocumentElement
                    .SelectNodes( "DVBNETProvider" )
                    .Cast<XmlElement>()
                    .Select( node => new DeviceInformation( node ) )
                    .ToDictionary( info => info.UniqueIdentifier )
                    .Values
                    .ToArray();
        }
    }
}
