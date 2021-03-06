using System;
using System.Collections.Generic;
using System.Threading;


namespace JMS.DVB.Provider.TTBudget
{
    internal class FilterQueue : IDisposable
    {
        private AutoResetEvent m_Request = new AutoResetEvent( false );
        private List<byte[]> m_Queue = new List<byte[]>();
        private object m_Lock = new object();
        private Thread m_Worker = null;
        private long m_QueueLimit = 0;
        private long m_QueueSize = 0;

        private Action<byte[]> m_Handler;

        public FilterQueue( Action<byte[]> callback )
        {
            // Remember
            m_Handler = callback;

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

                    // Process
                    if (null != m_Handler)
                        foreach (byte[] data in process)
                            try
                            {
                                // Process
                                m_Handler( data );
                            }
                            catch
                            {
                                // Ignore all
                            }
                }
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
