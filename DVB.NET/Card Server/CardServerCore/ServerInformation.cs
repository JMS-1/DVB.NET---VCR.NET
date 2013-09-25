using System;
using System.Collections.Generic;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Ermittelt den aktuellen Zustand des <i>Card Servers</i>.
    /// </summary>
    [Serializable]
    public class ServerInformation
    {
        /// <summary>
        /// Die aktuell angewählte Quellgruppe (Transponder) inklusive dem Namen des Geräteprofils.
        /// </summary>
        public string Selection { get; set; }

        /// <summary>
        /// Die Informationen zu allen aktiven Quellen.
        /// </summary>
        public readonly List<StreamInformation> Streams = new List<StreamInformation>();

        /// <summary>
        /// Meldet oder legt fest, ob gerade die elektronische Programmzeitschrift (EPG)
        /// aktualisiert wird und wie weit die Aktualisierung fortgeschritten ist.
        /// </summary>
        public double? ProgramGuideProgress { get; set; }

        /// <summary>
        /// Meldet oder setzt die Anzahl der Einträge für die Programmzeitschrift, die bereits 
        /// ermittelt wurden.
        /// </summary>
        public int CurrentProgramGuideItems { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob gerade eine Aktualisierung der Quellen (Sendersuchlauf)
        /// durchgeführt wird und wie weit dieser abgeschlossen wurde.
        /// </summary>
        public double? UpdateProgress { get; set; }

        /// <summary>
        /// Meldet oder legt fest, wieviele Quellen (aller Art) bereits gefunden wurden.
        /// </summary>
        public int UpdateSourceCount { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob auf der aktuellen Quellegruppe (Transponder) noch Daten
        /// empfangen werden.
        /// </summary>
        public bool HasGroupInformation { get; set; }

        /// <summary>
        /// Die Liste aller auf dem aktuellen Portal angebotenen NVOD Dienste oder <i>null</i>,
        /// wenn die Auswertung der Dienste nicht aktiv ist.
        /// </summary>
        public ServiceInformation[] Services { get; set; }

        /// <summary>
        /// Erzeugt eine neue Informationsinstanz.
        /// </summary>
        public ServerInformation()
        {
        }
    }
}
