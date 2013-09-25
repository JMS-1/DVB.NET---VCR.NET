namespace JMS.DVB
{
    /// <summary>
    /// Der Roll-Off Faktor für Satellitenempfang mit DVB-S2 Modulierung.
    /// </summary>
    public enum S2RollOffs
    {
        /// <summary>
        /// 0.35.
        /// </summary>
        Alpha35 = 0,

        /// <summary>
        /// 0.25.
        /// </summary>
        Alpha25 = 1,

        /// <summary>
        /// 0.20.
        /// </summary>
        Alpha20 = 2,

        /// <summary>
        /// Reserviert.
        /// </summary>
        Reserved = 3,

        /// <summary>
        /// Unbekannt.
        /// </summary>
        NotDefined = -1
    }
}
