using System;


namespace JMS.DVB.DeviceAccess.Editors
{
    /// <summary>
    /// Beschreibt einen einzelnen Parameter.
    /// </summary>
    public class ParameterValue
    {
        /// <summary>
        /// Der Anzeigetext des Wertes.
        /// </summary>
        public string DisplayText { get; private set; }

        /// <summary>
        /// Der ausgewählte Wert in Textdarstellung.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Erzeugt einen neuen Parameter.
        /// </summary>
        /// <param name="value">Der Wert des Parameters.</param>
        /// <exception cref="ArgumentNullException">Ein Parameter muss einen Anzeigetext haben.</exception>
        public ParameterValue( string value )
            : this( value, value )
        {
        }

        /// <summary>
        /// Erzeugt einen neuen Parameter.
        /// </summary>
        /// <param name="display">Der Anzeigetext für den Wert.</param>
        /// <param name="value">Der Wert des Parameters.</param>
        /// <exception cref="ArgumentNullException">Ein Parameter muss einen Anzeigetext haben.</exception>
        public ParameterValue( string display, string value )
        {
            // Validate
            if (string.IsNullOrEmpty( display ))
                throw new ArgumentNullException( "display" );

            // Remember
            DisplayText = display;
            Value = value;
        }
    }
}
