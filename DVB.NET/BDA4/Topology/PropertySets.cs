using System;


namespace JMS.DVB.DeviceAccess.Topology
{
    /// <summary>
    /// Enthält die eindeutigen Kennungen aller Eigenschaftsblöcke, die in einer
    /// Topologie verwendert werden können.
    /// </summary>
    public static class PropertySets
    {
        /// <summary>
        /// Feinsteuerung des Tuners.
        /// </summary>
        public static readonly Guid FrequencyFilter = new Guid( "71985f47-1ca1-11d3-9cc8-00c04f7971e0" );
    }
}
