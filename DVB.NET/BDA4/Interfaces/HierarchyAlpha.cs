using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Parameter f�r den DVB-T Empfang.
    /// </summary>
    public enum HierarchyAlpha
	{
        /// <summary>
        /// Unbekannt.
        /// </summary>
		NotDefined = 0,

        /// <summary>
        /// 1.
        /// </summary>
		One = 1,

        /// <summary>
        /// 2.
        /// </summary>
		Two = 2,

        /// <summary>
        /// 4.
        /// </summary>
		Four = 3,

        /// <summary>
        /// Der gr��te bisher definierte Wert.
        /// </summary>
		Maximum = 4,

        /// <summary>
        /// Bewusst nicht gesetzt.
        /// </summary>
		NotSet = -1
	}
}
