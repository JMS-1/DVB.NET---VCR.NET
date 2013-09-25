using System;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Beschreibt ein Eingabegerät.
    /// </summary>
    public abstract class RawDeviceInformation
    {
        /// <summary>
        /// Initialisiert eine Beschreibung.
        /// </summary>
        protected RawDeviceInformation()
        {
        }
    }

    /// <summary>
    /// Beschreibt die Informationen zu einer Tastatur.
    /// </summary>
    public class RawKeyboardInformation : RawDeviceInformation
    {
        /// <summary>
        /// Die Anzahl der Tasten.
        /// </summary>
        public uint NumberOfKeys { get; internal set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        internal RawKeyboardInformation()
        {
        }

        /// <summary>
        /// Meldet eine Kurzbezeichnung zu Testzwecken.
        /// </summary>
        /// <returns>Die gewünschte Kurzbezeichnung.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Keyboard with {0} Keys(s)", NumberOfKeys );
        }
    }

    /// <summary>
    /// Beschreibt die Informationen zu einem Eingabegerät.
    /// </summary>
    public class RawOtherInformation : RawDeviceInformation
    {
        /// <summary>
        /// Die Hauptkategorie.
        /// </summary>
        public ushort UsagePage { get; internal set; }

        /// <summary>
        /// Die Unterkategorie.
        /// </summary>
        public ushort Usage { get; internal set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        internal RawOtherInformation()
        {
        }

        /// <summary>
        /// Meldet eine Kurzbezeichnung zu Testzwecken.
        /// </summary>
        /// <returns>Die gewünschte Kurzbezeichnung.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Input Device ({0}, {1})", UsagePage, Usage );
        }
    }

    /// <summary>
    /// Beschreibt die Informationen zu einer Maus.
    /// </summary>
    public class RawMouseInformation : RawDeviceInformation
    {
        /// <summary>
        /// Die Anzahl der Tasten.
        /// </summary>
        public uint NumberOfButtons { get; internal set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        internal RawMouseInformation()
        {
        }

        /// <summary>
        /// Meldet eine Kurzbezeichnung zu Testzwecken.
        /// </summary>
        /// <returns>Die gewünschte Kurzbezeichnung.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Mouse with {0} Button(s)", NumberOfButtons );
        }
    }
}
