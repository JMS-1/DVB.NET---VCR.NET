using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die einzelnen Zustände, in denen sich ein Filter befinden kann.
    /// </summary>
    public enum FilterStates
    {
        /// <summary>
        /// Der Filter wurde noch gar nicht gestartet.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// Der Filter wurde gestartet, der Datenfluss ist aber zur Zeit angehalten.
        /// </summary>
        Paused = 1,

        /// <summary>
        /// Der Filter ist aktiv.
        /// </summary>
        Running = 2,
    }
}
