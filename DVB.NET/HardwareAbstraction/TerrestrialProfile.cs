using System;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine DVB Hardware für den Empfang über Antenne.
    /// </summary>
    [Serializable]
    public class TerrestrialProfile : Profile<TerrestrialLocation, TerrestrialGroup, TerrestrialScanLocation>
    {
    }
}

