using System;
using JMS.DVB;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace JMS.TechnoTrend
{
    internal class FilterQueue : IDisposable
    {
        private AutoResetEvent m_Request = new AutoResetEvent( false );
        private List<byte[]> m_Queue = new List<byte[]>();
        private object m_Lock = new object();
        private Thread m_Worker = null;
        private long m_QueueLimit = 0;
        private long m_QueueSize = 0;

        public Action<byte[]> Handler;

        public FilterQueue( Action<byte[]> callback )
        {
            // Remember
            Handler = callback;

            // Create thread
            m_Worker = new Thread( new ThreadStart( Worker ) );

            // Start
            m_Worker.Start();
        }

        private void Worker()
        {
            // Forever
            for (; ; )
            {
                // Await request
                m_Request.WaitOne();

                // Empty queue
                for (; ; )
                {
                    // To be processed
                    List<byte[]> process;

                    // Synchronize
                    lock (m_Lock)
                    {
                        // Should finish
                        if (null == m_Queue) return;

                        // Wait for next event
                        if (m_Queue.Count < 1) break;

                        // Load
                        process = m_Queue;

                        // Reset
                        m_Queue = new List<byte[]>();
                        m_QueueSize = 0;
                    }

                    // Load handler
                    var handler = Handler;
                    if (null != handler)
                        foreach (byte[] data in process)
                            try
                            {
                                // Process
                                handler( data );
                            }
                            catch
                            {
                                // Ignore all
                            }
                }
            }
        }

        public void Clear()
        {
            // Synchronize
            lock (m_Lock)
                if (null != m_Queue)
                {
                    // Reset
                    m_Queue = new List<byte[]>();
                    m_QueueSize = 0;
                }
        }

        public void Enqueue( byte[] data )
        {
            // Validate
            if (null == data) throw new ArgumentNullException( "data" );

            // Process synchronized
            lock (m_Lock)
                if (null != m_Queue)
                {
                    // Enter
                    m_Queue.Add( data );

                    // Count
                    m_QueueSize += data.Length;

                    // Wake up
                    if (m_QueueSize >= m_QueueLimit) m_Request.Set();
                }
        }

        #region IDisposable Members

        public void Dispose()
        {
            // Check mode
            if (null != m_Worker)
            {
                // Report finish
                lock (m_Lock)
                {
                    // Destroy queue
                    m_Queue = null;

                    // Wakeup call
                    m_Request.Set();
                }

                // Wait for thread to finish
                m_Worker.Join();

                // Back to CLR
                m_Worker = null;
            }
        }

        #endregion
    }
}
