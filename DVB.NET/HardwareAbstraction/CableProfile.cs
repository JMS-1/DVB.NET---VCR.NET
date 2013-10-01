using System;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine DVB Hardware für den Empfang über Kabel.
    /// </summary>
    [Serializable]
    public class CableProfile : Profile<CableLocation, CableGroup, CableScanLocation>
    {
    }
}
