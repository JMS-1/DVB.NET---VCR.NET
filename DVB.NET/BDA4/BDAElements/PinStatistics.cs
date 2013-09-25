using System;


namespace JMS.DVB.DeviceAccess.BDAElements
{
    /// <summary>
    /// Report the data flow through a DVB.NET PID filter.
    /// </summary>
    public class PinStatistics
    {
        /// <summary>
        /// Synchronize access to all members during a clone.
        /// </summary>
        private object m_Lock = new object();

        /// <summary>
        /// The size of the smallest sample we got (in bytes).
        /// </summary>
        public int SampleMinSize = int.MaxValue;

        /// <summary>
        /// The size of largest sample we got (in bytes).
        /// </summary>
        public int SampleMaxSize = int.MinValue;

        /// <summary>
        /// The total number of samples processed by the filter.
        /// </summary>
        public long SampleCount = 0;

        /// <summary>
        /// The total number of bytes passed through the filter.
        /// </summary>
        public long SampleTotal = 0;

        /// <summary>
        /// Create a new statistic instance.
        /// </summary>
        public PinStatistics()
        {
        }

        /// <summary>
        /// Create a new statistic instance and load all members from 
        /// another one.
        /// </summary>
        /// <param name="other">The origin of the counters. Access to
        /// all memebers will by synchronized.</param>
        public PinStatistics( PinStatistics other )
        {
            // Read out
            lock (other.m_Lock)
            {
                // All
                SampleCount = other.SampleCount;
                SampleTotal = other.SampleTotal;
                SampleMinSize = other.SampleMinSize;
                SampleMaxSize = other.SampleMaxSize;
            }
        }

        /// <summary>
        /// Report the average number of bytes in a sample.
        /// </summary>
        public int BytesPerSample
        {
            get
            {
                // None
                if (SampleCount < 1) return 0;

                // Calculate
                return (int) (SampleTotal / SampleCount);
            }
        }

        /// <summary>
        /// Add a new sample to the statistics.
        /// </summary>
        /// <remarks>
        /// Access to all members will be synchronized.
        /// </remarks>
        /// <param name="size">Size of the sample in bytes.</param>
        public void AddSample( int size )
        {
            // Synchronize
            lock (m_Lock)
            {
                // Count
                ++SampleCount;

                // Overall length
                SampleTotal += size;

                // Bounds
                if (size < SampleMinSize)
                    SampleMinSize = size;
                if (size > SampleMaxSize)
                    SampleMaxSize = size;
            }
        }
    }
}
