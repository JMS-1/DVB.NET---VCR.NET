using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Administration.ProfileManager
{
    /// <summary>
    /// Beschreibt eine Art von unterstützter Hardware.
    /// </summary>
    public class HardwareTypeItem
    {
        /// <summary>
        /// Der Name der .NET Klasse, die eine Implementierung anbietet.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Ein Anzeigename für diese Implementierung.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public HardwareTypeItem()
        {
        }

        /// <summary>
        /// Meldet einen Anzeigenamen.
        /// </summary>
        /// <returns>Der gewünschte Anzeigename oder die .NET Klasse, wenn
        /// kein Anzeigename bekannt ist.</returns>
        public override string ToString()
        {
            // Check mode
            if (string.IsNullOrEmpty( DisplayName ))
                return Type;
            else
                return DisplayName;
        }
    }
}
