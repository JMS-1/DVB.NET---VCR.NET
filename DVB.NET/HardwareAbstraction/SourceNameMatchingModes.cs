using System;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt, wie ein Namensvergleich erfolgen soll.
    /// </summary>
    [Flags]
    public enum SourceNameMatchingModes
    {
        /// <summary>
        /// Es wird nur nach dem Namen der Quelle verglichen.
        /// </summary>
        Name = 0x01,

        /// <summary>
        /// Zum Vergleich wird der Name der Quelle und des Dienstanbieters 
        /// verwendet.
        /// </summary>
        FullName = 0x02,

        /// <summary>
        /// Für den Vergleich wird neben den Namen von Quelle und Dienstanbieter
        /// auch die Dienstnummer berücksichtigt.
        /// </summary>
        QualifiedName = 0x04,

        /// <summary>
        /// Es werden alle Varianten berücksichtigt.
        /// </summary>
        All = Name | FullName | QualifiedName
    }
}
