using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Beinhaltet Standardeinstellungen für einen Empfangsgraphen.
    /// </summary>
    public class GraphConfiguration
    {
        /// <summary>
        /// Der zugehörige DirectShow Graph.
        /// </summary>
        public DataGraph Graph { get; private set; }

        /// <summary>
        /// Die minimale Anzahl von PAT Tabellen die erkannt werden müssen, bevor 
        /// der aktuelle <i>Transport Stream</i> als gültig erkannt wird. Die
        /// Voreinstellung ist <i>5</i>.
        /// </summary>
        public int MinimumPATCount { get; set; }

        /// <summary>
        /// Die maximale Wartezeit bis <see cref="MinimumPATCount"/> erreicht
        /// wird. Die Zählung erfolgt in Einheiten von einer zehntel Sekunde.
        /// Die Voreinstellung ist <i>50</i> entsprechend 5 Sekunden.
        /// </summary>
        public int MinimumPATCountWaitTime { get; set; }

        /// <summary>
        /// Erzeugt neue Einstellungen.
        /// </summary>
        /// <param name="graph">Der zugehörige Graph.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Graph angegeben.</exception>
        internal GraphConfiguration( DataGraph graph )
        {
            // Validate
            if (graph == null)
                throw new ArgumentNullException( "graph" );

            // Remember
            Graph = graph;

            // Finish
            MinimumPATCountWaitTime = 50;
            MinimumPATCount = 5;
        }
    }
}
