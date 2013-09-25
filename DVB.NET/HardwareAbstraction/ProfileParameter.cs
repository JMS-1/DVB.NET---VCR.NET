using System;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVB
{
    /// <summary>
    /// Ein Parameter für eine Beschreibung einer DVB.NET Hardwareabstraktion.
    /// </summary>
    [Serializable]
    public class ProfileParameter
    {
        /// <summary>
        /// Der Name des Parameters.
        /// </summary>
        [XmlAttribute( "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Der Wert des Parameters.
        /// </summary>
        [XmlText]
        public string Value { get; set; }

        /// <summary>
        /// Erzeugt einen neuen Parameter.
        /// </summary>
        public ProfileParameter()
        {
        }

        /// <summary>
        /// Erzeugt einen neuen Parameter.
        /// </summary>
        /// <param name="name">Der Name des Parameters.</param>
        /// <param name="value">Der Wert des Parameters.</param>
        public ProfileParameter( string name, object value )
        {
            // Remember
            Value = (value == null) ? null : value.ToString();
            Name = name;
        }

        /// <summary>
        /// Meldet die interne Kennung für Erweiterungsparameter einer bestimmten Art.
        /// </summary>
        /// <param name="type">Die Art der Erweiterung.</param>
        /// <returns>Der zu verwendende Namenspräfix.</returns>
        internal static string GetPrefixForExtensionParameter( PipelineTypes type )
        {
            // Create
            return string.Format( "#ExtParam#{0}_", type );
        }
    }
}
