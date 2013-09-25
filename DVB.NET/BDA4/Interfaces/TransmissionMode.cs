using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die Übertragungsarten beim DVB-T Empfang.
    /// </summary>
    public enum TransmissionMode
	{
        /// <summary>
        /// Unbekannt.
        /// </summary>
		NotDefined = 0,

        /// <summary>
        /// 2.
        /// </summary>
		Two = 1,

        /// <summary>
        /// 8.
        /// </summary>
		Eight = 2,

        /// <summary>
        /// Der bisher maximal erlaubte Wert.
        /// </summary>
		Maximum = 3,

        /// <summary>
        /// Bewusst nicht gesetzt.
        /// </summary>
		NotSet = -1
	}
}
