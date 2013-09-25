using System;


namespace JMS.DVB.Editors
{
    /// <summary>
    /// Diese Schnittstelle wird von Dialogen zur Pflege der Geräteparameter einer DVB.NET
    /// Hardwareabstraktion verwendet.
    /// </summary>
    public interface IHardwareEditor
    {
        /// <summary>
        /// Liest oder setzt das zu pflegende Geräteprofil.
        /// </summary>
        Profile Profile { get; set; }

        /// <summary>
        /// Meldet, ob die aktuellen Eingaben zulässig sind und ein Speichern
        /// möglich wäre.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Kopiert die aktuelle Eingabe in das aktuelle Geräteprofil.
        /// </summary>
        void UpdateProfile();
    }
}
