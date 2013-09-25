using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Die unterschiedlichen Arten von Datenströmen, die innerhalb einer <see cref="SourceGroup"/>
    /// angeboten werden. Je nach konkreter Hardware kann es notwendig sein, hier die korrekte
    /// Art zu verwenden.
    /// </summary>
    public enum StreamTypes
    {
        /// <summary>
        /// Ein nicht bekannter Datenstrom, der allerdings zumindest als <i>Packetized
        /// Elementary Stream (PES)</i> empfangen wird.
        /// </summary>
        UnknownPES = 0,

        /// <summary>
        /// Der Datenstrom besteht aus einzelnen SI Tabellen (Tabellenkennungen 0x00 bis 0x7f).
        /// </summary>
        StandardTable = 1,

        /// <summary>
        /// Der Datenstrom enthält ein Bildsignal - über das verwendete Kompressionsverfahren
        /// wie MPEG-2 oder H.264 wird in dieser Version keine Aussage gemacht.
        /// </summary>
        Video = 2,

        /// <summary>
        /// Für Datenströme mit Tonsignalen - in der aktuellen Version wird nicht zwischen
        /// verschiedenen Formaten wie MP2 oder AC3 unterschieden.
        /// </summary>
        Audio = 3,

        /// <summary>
        /// Hier wird im Datenstrom der Videotext übertragen.
        /// </summary>
        VideoText = 4,

        /// <summary>
        /// Es handelt sich um einen Datenstrom für DVB Untertitel.
        /// </summary>
        SubTitle = 5,

        /// <summary>
        /// Der Datenstrom besteht aus einzelnen SI Tabellen (Tabellenkennungen 0x80 bis 0xff).
        /// </summary>
        ExtendedTable = 6
    }
}
