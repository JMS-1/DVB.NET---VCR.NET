using System;


namespace JMS.DVB.Provider.Duoflex
{
    /// <summary>
    /// Alle Befehle, die an die CI/CAM Einheit geschickt werden dürfen.
    /// </summary>
    internal enum CAMControl
    {
        /// <summary>
        /// Zurücksetzen.
        /// </summary>
        Reset = 0,

        /// <summary>
        /// Menü öffnen.
        /// </summary>
        EnterMenu = 1,

        /// <summary>
        /// Menü schliessen.
        /// </summary>
        CloseMenu = 2,

        /// <summary>
        /// Menü abrufen.
        /// </summary>
        GetMenu = 3,

        /// <summary>
        /// Antwort anfordern.
        /// </summary>
        MenuReply = 4,

        /// <summary>
        /// Antwort.
        /// </summary>
        Answer = 5,
    }
}
