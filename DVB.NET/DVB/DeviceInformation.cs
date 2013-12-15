using System;
using System.Collections;
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
        public DeviceInformation( XmlElement provider )
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

        public XmlNodeList Parameters
        {
            get
            {
                // Report
                return FindElement( "Parameters" ).ChildNodes;
            }
        }

        public string UniqueIdentifier
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
        public string DriverType
        {
            get
            {
                // Report
                return FindElement( "Driver" ).InnerText; ;
            }
        }
        
        public string[] Names
        {
            get
            {
                // Helper
                ArrayList names = new ArrayList();

                // All my names
                foreach (XmlNode name in Root.SelectNodes( "CardNames/CardName" ))
                {
                    // Remember
                    names.Add( name.InnerText );
                }

                // Report
                return (string[]) names.ToArray( typeof( string ) );
            }
        }
    }
}
