namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt die verwendete Fehlerkorrekturmethode.
    /// </summary>
    public enum InnerForwardErrorCorrectionModes 
	{
        /// <summary>
        /// Nicht festgelegt.
        /// </summary>
		NotDefined = 0,

        /// <summary>
        /// FEC 1/2.
        /// </summary>
		Conv1_2 = 1,

        /// <summary>
        /// FEC 2/3.
        /// </summary>
		Conv2_3 = 2,

        /// <summary>
        /// FEC 3/4.
        /// </summary>
		Conv3_4 = 3,

        /// <summary>
        /// FEC 5/6.
        /// </summary>
		Conv5_6 = 4,

        /// <summary>
        /// FEC 7/8.
        /// </summary>
		Conv7_8 = 5,

        /// <summary>
        /// FEC 8/9.
        /// </summary>
		Conv8_9 = 6,

        /// <summary>
        /// FEC 3/5.
        /// </summary>
		Conv3_5 = 7,

        /// <summary>
        /// FEC 4/5.
        /// </summary>
        Conv4_5 = 8,

        /// <summary>
        /// FEC 9/10.
        /// </summary>
        Conv9_10 = 9,

        /// <summary>
        /// (Noch) Undefiniert.
        /// </summary>
        Reserved1010 = 10,

        /// <summary>
        /// (Noch) Undefiniert.
        /// </summary>
        Reserved1011 = 11,

        /// <summary>
        /// (Noch) Undefiniert.
        /// </summary>
        Reserved1100 = 12,

        /// <summary>
        /// (Noch) Undefiniert.
        /// </summary>
        Reserved1101 = 13,

        /// <summary>
        /// (Noch) Undefiniert.
        /// </summary>
        Reserved1110 = 14,

        /// <summary>
        /// Es wird keine Korrektur verwendet.
        /// </summary>
		NoConv = 15
	}}
