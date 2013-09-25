using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine DVB Hardware für den Empfang über Kabel.
    /// </summary>
    [Serializable]
    public class CableProfile : Profile<CableLocation, CableGroup, CableScanLocation>
    {
        /// <summary>
        /// Erzeugt eine neue DVB-C Beschreibung.
        /// </summary>
        public CableProfile()
        {
        }
    }
}
