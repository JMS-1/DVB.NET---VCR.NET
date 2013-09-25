using System;


namespace JMS.DVB.Provider.FireDTV
{
    /// <summary>
    /// Kommunikationscodes für das CI.
    /// </summary>
    internal enum CACommandTags
    {
        /// <summary>
        /// Hardware-Reset ausführen.
        /// </summary>
        Reset,

        /// <summary>
        /// Informationen vom CI.
        /// </summary>
        ApplicationInfo,

        /// <summary>
        /// Entschlüsseung aktivieren.
        /// </summary>
        Decrypt,

        /// <summary>
        /// Antwort auf eine Entschlüsselungsanfrage.
        /// </summary>
        DecryptReply,

        /// <summary>
        /// Uhrzeit übertragen (beide Richtungen).
        /// </summary>
        DateAndTime,

        /// <summary>
        /// MMI (Men-Machine-Interface) Kommunikation (beide Richtungen).
        /// </summary>
        MMI,

        /// <summary>
        /// Debug Informationen vom CI.
        /// </summary>
        DebugError,

        /// <summary>
        /// Menuanforderung senden.
        /// </summary>
        EnterMenu,
    }
}
