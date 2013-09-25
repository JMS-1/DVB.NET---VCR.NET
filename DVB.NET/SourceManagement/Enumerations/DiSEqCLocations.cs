namespace JMS.DVB
{
    /// <summary>
    /// Die von DVB.NET unterstützen LNB Auswahlmöglichkeiten.
    /// </summary>
    public enum DiSEqCLocations
    {
        /// <summary>
        /// Es wird keine Umschaltung benötigt.
        /// </summary>
        None,

        /// <summary>
        /// Es wird eine <i>Burst</i> Umschaltung verwendet und es handelt
        /// sich hier um die inaktive Variante.
        /// </summary>
        BurstOff,

        /// <summary>
        /// Es wird eine <i>Burst</i> Umschaltung verwendet und es handelt
        /// sich hier um die aktive Variante.
        /// </summary>
        BurstOn,

        /// <summary>
        /// Es wird eine DiSEqC 1.0 4fach Umschaltung verwendet und es handelt
        /// sich um den ersten LNB (Option A bei Position A).
        /// </summary>
        DiSEqC1,

        /// <summary>
        /// Es wird eine DiSEqC 1.0 4fach Umschaltung verwendet und es handelt
        /// sich um den zweiten LNB (Option A bei Position B).
        /// </summary>
        DiSEqC2,

        /// <summary>
        /// Es wird eine DiSEqC 1.0 4fach Umschaltung verwendet und es handelt
        /// sich um den dritten LNB (Option B bei Position A).
        /// </summary>
        DiSEqC3,

        /// <summary>
        /// Es wird eine DiSEqC 1.0 4fach Umschaltung verwendet und es handelt
        /// sich um den vierten LNB (Option B bei Position B).
        /// </summary>
        DiSEqC4
    }
}
