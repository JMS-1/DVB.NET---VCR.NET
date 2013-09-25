namespace JMS.DVB
{
    /// <summary>
    /// Die möglichen Modulationsarten für den Kabelempfang.
    /// </summary>
    public enum CableModulations
    {
        /// <summary>
        /// Undefiniert.
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// 16-QAM.
        /// </summary>
        QAM16 = 1,

        /// <summary>
        /// 32-QAM.
        /// </summary>
        QAM32 = 2,

        /// <summary>
        /// 64-QAM.
        /// </summary>
        QAM64 = 3,

        /// <summary>
        /// 128-QAM.
        /// </summary>
        QAM128 = 4,

        /// <summary>
        /// 256-QAM.
        /// </summary>
        QAM256 = 5
    }
}
