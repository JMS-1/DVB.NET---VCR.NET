using System;


namespace JMS.DVB
{
    /// <summary>
    /// Kennzeichnet eine Klasse, die Aufgaben für Aufgabenlisten anbieten kann.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
    public class PipelineAttribute : Attribute
    {
        /// <summary>
        /// Die von der Klasse angebotenen Arten von Aufgaben.
        /// </summary>
        public PipelineTypes Types { get; private set; }

        /// <summary>
        /// Ein Anzeigetext zur Auswahl durch den Anwender (multinational).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gesetzt, wenn es einen separaten Dialog zur Pflege der Detailparameter gibt.
        /// </summary>
        public bool HasAdditionalParameters { get; set; }

        /// <summary>
        /// Erzeugt eine neue Kennzeichnung.
        /// </summary>
        /// <param name="types">Die von dieser Klasse unterstützten Aufgabenarten.</param>
        /// <param name="displayName">Eine Beschreibung für diese Klasse.</param>
        public PipelineAttribute( PipelineTypes types, string displayName )
        {
            // Remember
            DisplayName = displayName;
            Types = types;
        }
    }
}
