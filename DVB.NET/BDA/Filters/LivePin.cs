using System;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DirectShow.Interfaces;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.BDAElements;


namespace JMS.DVB.DirectShow.Filters
{
    /// <summary>
    /// Ein Pin f�r eine Direct Show <i>Live Source</i>.
    /// </summary>
    internal class LivePin : OutputPin, IAMPushSource
    {
        /// <summary>
        /// Erzeugt einen neuen Live Pin.
        /// </summary>
        /// <param name="filter">Der zugeh�rige Filter.</param>
        /// <param name="name">Der Name des Pins.</param>
        /// <param name="mediaType">Der Datentyp, der durch den Pin flie�t.</param>
        public LivePin( TypedComIdentity<IBaseFilter> filter, string name, MediaType mediaType )
            : base( filter, name, mediaType )
        {
        }

        #region IAMPushSource Members

        long IAMPushSource.GetLatency()
        {
            // Don't know
            return 0;
        }

        uint IAMPushSource.GetPushSourceFlags()
        {
            // Don't know
            return 0;
        }

        void IAMPushSource.SetPushSourceFlags( uint flags )
        {
            // Not implemented
            throw new NotImplementedException();
        }

        void IAMPushSource.SetStreamOffset( long offset )
        {
            // Not implemented
            throw new NotImplementedException();
        }

        long IAMPushSource.GetStreamOffset()
        {
            // Not implemented
            throw new NotImplementedException();
        }

        long IAMPushSource.GetMaxStreamOffset()
        {
            // Not implemented
            throw new NotImplementedException();
        }

        void IAMPushSource.SetMaxStreamOffset( long offset )
        {
            // Not implemented
            throw new NotImplementedException();
        }

        #endregion
    }
}
