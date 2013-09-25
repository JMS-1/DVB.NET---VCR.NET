namespace JMS.DVB
{
    /// <summary>
    /// Die möglichen Polarisationen beim Empfang eines Satellitensignals.
    /// </summary>
    public enum Polarizations
    {
        /// <summary>
        /// Lineare horizontale Polarisation.
        /// </summary>
		Horizontal = 0,

        /// <summary>
        /// Lineare vertikale Polarisaion.
        /// </summary>
		Vertical = 1,

        /// <summary>
        /// Links zirkulare Polarisation.
        /// </summary>
		Left = 2,

        /// <summary>
        /// Rechts zirkulare Polarisation.
        /// </summary>
		Right = 3,

        /// <summary>
        /// Es besteht keine Polarisation oder diese ist unbekannt.
        /// </summary>
        NotDefined = -1
    }
}
