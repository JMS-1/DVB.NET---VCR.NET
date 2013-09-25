using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Administration
{
    /// <summary>
    /// Eine grobe Einordnung von Erweiterungen für das Administrationswerkzeug.
    /// </summary>
    public enum PlugInCategories
    {
        /// <summary>
        /// Alles rund um die Pflege von Geräteprofilen.
        /// </summary>
        ProfileManagement,

        /// <summary>
        /// Aufgaben zum Sendersuchlauf.
        /// </summary>
        SourceScanning,

        /// <summary>
        /// Allgemeine Aufgaben rund um DVB.NET.
        /// </summary>
        Tools,
    }
}
