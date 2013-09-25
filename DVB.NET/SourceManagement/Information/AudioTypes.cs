namespace JMS.DVB
{
    /// <summary>
    /// Die Arten einer Tonspur.
    /// </summary>
    public enum AudioTypes
    {
        /// <summary>
        /// Die Art der Tonspur ist nicht bekannt.
        /// </summary>
        Unknown,

        /// <summary>
        /// Es handelt sich um eine MP2 Tonspur.
        /// </summary>
        MP2,

        /// <summary>
        /// Die Tonspur enthält Dolby Digital (AC3) Nutzdaten.
        /// </summary>
        AC3
    }
}
