using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using JMS.TechnoTrend;


namespace JMS.DVB.Provider.TTBudget
{
    internal abstract class Filter : IDisposable
    {
        public readonly ushort FilterPID;

        protected ClassHolder m_Class = null;

        private bool m_Running = false;

        protected Filter( ushort pid )
        {
            // Remember
            FilterPID = pid;
        }

        public bool IsActive
        {
            get
            {
                // Report
                lock (this) return m_Running;
            }
        }

        protected abstract void OnStart( byte[] filterData, byte[] filterMask );

        protected abstract void OnStart();

        protected abstract void OnStop();

        public abstract long Length { get; }

        public void Stop()
        {
            // Synchronize
            lock (this) if (!m_Running) return;

            // Forward call
            OnStop();

            // Remember state
            lock (this) m_Running = false;
        }

        public void Start( byte[] filterData, byte[] filterMask )
        {
            // Validate - filtering is required
            if (null == filterData) throw new ArgumentNullException( "filterData" );
            if (null == filterMask) throw new ArgumentNullException( "filterMask" );
            if (filterData.Length != filterMask.Length) throw new ArgumentException( "Filter data and mask are not of same size" );
            if (filterData.Length < 1) throw new ArgumentNullException( "filterData" );
            if (filterData.Length > 255) throw new ArgumentException( "Filter data and mask are too large - a maximum of 255 bytes is allowed" );

            // Stop it
            Stop();

            // Synchronize
            lock (this)
            {
                // Start in memory filter
                OnStart( filterData, filterMask );

                // Mark as active
                m_Running = true;
            }
        }

        public void Start()
        {
            // Stop it
            Stop();

            // Synchronize
            lock (this)
            {
                // Set up filter
                OnStart();

                // Mark as active
                m_Running = true;
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            // Make sure that filter is deactivated
            Stop();

            // Load the class
            ClassHolder instance;

            // Protected
            lock (this)
            {
                // Load
                instance = m_Class;

                // Forget
                m_Class = null;
            }

            // Wipe out
            if (null != instance) instance.Dispose();
        }

        #endregion
    }
}
