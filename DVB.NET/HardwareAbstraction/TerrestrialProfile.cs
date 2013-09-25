using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine DVB Hardware für den Empfang über Antenne.
    /// </summary>
    [Serializable]
    public class TerrestrialProfile : Profile<TerrestrialLocation, TerrestrialGroup, TerrestrialScanLocation>
    {
        /// <summary>
        /// Erzeugt eine neue DVB-T Beschreibung.
        /// </summary>
        public TerrestrialProfile()
        {
        }
    }
}

