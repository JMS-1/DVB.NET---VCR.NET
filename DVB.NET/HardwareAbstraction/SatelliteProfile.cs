using System;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine DVB Hardware für den Satellitenempfang.
    /// </summary>
    [Serializable]
    public class SatelliteProfile : Profile<SatelliteLocation, SatelliteGroup, SatelliteScanLocation>
    {
        /// <summary>
        /// Wird gesetzt, um DVB-S2 Gruppen grundsätzlich auszuschliessen.
        /// </summary>
        public bool DisableS2Groups { get; set; }

        /// <summary>
        /// Prüft, ob das zugehörige Gerät eine bestimmte Quellgruppe überhaupt unterstützt.
        /// </summary>
        /// <param name="group">Die zu prüfende Quellgruppe.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe unterstützt wird.</returns>
        public override bool SupportsGroup( SatelliteGroup group )
        {
            // Ask base (will test for null)
            if (!base.SupportsGroup( group ))
                return false;

            // DVB-S2
            if (DisableS2Groups)
                if (group.UsesS2Modulation)
                    return false;

            // Allow
            return true;
        }
    }
}
