using System;


namespace JMS.DVB.Provider.FireDTV
{
    /// <summary>
    /// Legt fest, wie mit den Daten an das CI umgegagen werden soll.
    /// </summary>
    internal enum CAListTypes
    {
        /// <summary>
        /// Es handelt sich um ein mittleres Fragement.
        /// </summary>
        More,

        /// <summary>
        /// Das erste Fragment einer Reihe von Fragementen.
        /// </summary>
        First,

        /// <summary>
        /// Das letzte Fragment einer Reihe von Fragementen.
        /// </summary>
        Last,

        /// <summary>
        /// Das einzige Fragment einer Übertragung.
        /// </summary>
        One,

        /// <summary>
        /// Entschlüsselungsinformationen ergänzen.
        /// </summary>
        Add,

        /// <summary>
        /// Vorhandene Entschlüsselungsinformationen ersetzen.
        /// </summary>
        Update
    }
}
