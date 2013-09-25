using System;

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Wächterbereich für den DVB-T Empfang.
    /// </summary>
    public enum GuardInterval
	{
        /// <summary>
        /// Unbekannt.
        /// </summary>
		NotDefined = 0,

        /// <summary>
        /// 1/32.
        /// </summary>
		ThirtyTwo = 1,

        /// <summary>
        /// 1/16.
        /// </summary>
		Sixteen = 2,

        /// <summary>
        /// 1/8.
        /// </summary>
		Eight = 3,

        /// <summary>
        /// 1/4.
        /// </summary>
		Four = 4,

        /// <summary>
        /// Maximalwert bisher.
        /// </summary>
		Maximum = 5,

        /// <summary>
        /// Bewußt nicht gesetzt.
        /// </summary>
		NotSet = -1
	}
}
