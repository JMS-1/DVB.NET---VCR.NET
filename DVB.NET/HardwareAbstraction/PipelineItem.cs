using System;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt einen Eintrag in der Bearbeitungskette des Profils.
    /// </summary>
    [Serializable]
    public class PipelineItem
    {
        /// <summary>
        /// Die Arten von Operationen, die dieses Element unterstützt.
        /// </summary>
        [XmlAttribute( "operations" )]
        public PipelineTypes SupportedOperations { get; set; }

        /// <summary>
        /// Die zugehörige .NET Klasse.
        /// </summary>
        [XmlText]
        public string ComponentType { get; set; }

        /// <summary>
        /// Erstellt einen neuen Bearbeitungseintrag.
        /// </summary>
        public PipelineItem()
        {
        }

        /// <summary>
        /// Erzeugt die zugehörige Erweiterung.
        /// <typeparam name="T">Die gewünschte Art der Erweiterung.</typeparam>
        /// </summary>
        /// <returns>Die Erweitgerung gemäß <see cref="ComponentType"/> oder <i>null</i>,
        /// wenn diese nicht unterstützt wird.</returns>
        public T CreateExtension<T>() where T : class, IHardwareExtension
        {
            // Unsupported
            if (typeof( T ) != typeof( IPipelineExtension ))
                throw new ArgumentException( typeof( T ).FullName, "T" );

            // Be safe
            try
            {
                // Create it
                return (T) Activator.CreateInstance( Type.GetType( ComponentType, true ) );
            }
            catch (Exception e)
            {
                // Report
                throw new ArgumentException( string.Format( Properties.Resources.Exception_NoExtension, ComponentType, e.Message ), "ComponentType", e );
            }
        }
    }
}
