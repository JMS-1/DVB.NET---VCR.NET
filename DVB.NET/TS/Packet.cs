using System;
using System.Collections;

namespace JMS.DVB.TS
{
    /// <summary>
    /// This class manages PES packages. One outermost instance for every PID
    /// collects single packets and in addition holds a queue of complete
    /// packages already collected but not yet send to the transport stream.
    /// </summary>
    internal class Packet
    {
        private enum StreamTypes
        {
            Other,
            Audio,
            Video
        }

        /// <summary>
        /// Each PTS has 33 bits and this is the value to add or subtract in 
        /// case of under- or overflows.
        /// </summary>
        public const long PTSOverrun = ((long) 1) << 33;

        /// <summary>
        /// Set when a packet with no PTS is found.
        /// </summary>
        public bool PTSMissing = false;

        /// <summary>
        /// Set to ignore PTS on this kind of package.
        /// </summary>
        public bool IgnorePTS = false;

        /// <summary>
        /// The relaqted transport stream.
        /// </summary>
        private Manager m_Manager;

        /// <summary>
        /// Current PTS excluding any correction.
        /// </summary>
        private long m_PTS = -1;

        /// <summary>
        /// Maximum number of entries in pending queue.
        /// </summary>
        private int m_MaxQueue = 0;

        /// <summary>
        /// Number of buffer cleanups due to overflow condition.
        /// </summary>
        private int m_Overflow = 0;

        /// <summary>
        /// Set if this instance holds video frames.
        /// <seealso cref="SetAudioVideo"/>
        /// </summary>
        private StreamTypes m_Video = StreamTypes.Other;

        /// <summary>
        /// Complete PES packages waiting to be sent to the transport stream.
        /// </summary>
        private ArrayList Pending = new ArrayList();

        /// <summary>
        /// When the last packet arrived.
        /// </summary>
        private DateTime m_LastPacket = DateTime.UtcNow;

        /// <summary>
        /// Partial PES package while collecting.
        /// </summary>
        private ArrayList Parts = new ArrayList();

        /// <summary>
        /// The stream identifier of this package.
        /// </summary>
        public readonly int PID;

        /// <summary>
        /// Create a new PES packet manager.
        /// </summary>
        /// <param name="manager">The corresponding transport stream.</param>
        /// <param name="pid">The stream identifier of this package.</param>
        public Packet( Manager manager, int pid )
        {
            // Remember
            m_Manager = manager;
            PID = pid;
        }

        /// <summary>
        /// Create a new PES packet manager.
        /// </summary>
        /// <param name="other">Template manager.</param>
        public Packet( Packet other )
            : this( other.m_Manager, other.PID )
        {
            // Clone the type
            m_Video = other.m_Video;

            // Copy flags
            IgnorePTS = other.IgnorePTS;
        }

        /// <summary>
        /// Add a partial PES package for collection.
        /// </summary>
        /// <param name="buffer"><i>null</i> will be used to indicate that
        /// the following package carries a PCR.</param>
        public void Add( byte[] buffer )
        {
            // Append
            Parts.Add( buffer );
        }

        /// <summary>
        /// Clear the current PES package data.
        /// </summary>
        public void Clear()
        {
            // Cleanup
            Parts.Clear();
        }

        /// <summary>
        /// Number of parts in the current PES package.
        /// </summary>
        public int Count
        {
            get
            {
                // Report
                return Parts.Count;
            }
        }

        /// <summary>
        /// Report a part of the current PES package.
        /// </summary>
        public byte[] this[int index]
        {
            get
            {
                // Report
                return (byte[]) Parts[index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsActive
        {
            get
            {
                // Check when the last packet arrived
                return (DateTime.UtcNow < m_LastPacket.AddSeconds( 1 ));
            }
        }

        /// <summary>
        /// Append a complete PES package to the processing queue.
        /// </summary>
        /// <param name="buffer">A complete PES package.</param>
        public void Enqueue( Packet buffer )
        {
            // Count
            m_LastPacket = DateTime.UtcNow;

            // Set flag
            if (!PTSMissing)
                if (IgnorePTS)
                    PTSMissing = true;
                else if (buffer.Count > 0)
                    PTSMissing = (buffer.PTS < 0);

            // Remember
            Pending.Add( buffer );

            // Check
            if (Pending.Count > m_MaxQueue) m_MaxQueue = Pending.Count;
        }

        /// <summary>
        /// Get or set the PTS of this package.
        /// </summary>
        public long PTS
        {
            get
            {
                // Report
                return m_PTS;
            }
            set
            {
                // Never set PTS to invalid
                if (value >= 0) m_PTS = value;
            }
        }

        /// <summary>
        /// Remove the next complete PES package from the waiting queue
        /// and <see cref="SendTo"/> the transport stream.
        /// </summary>
        /// <remarks>
        /// Only call this method if the queue is not empty.
        /// </remarks>
        /// <returns>Set if anything has been sent.</returns>
        public bool Dequeue()
        {
            // Forward
            return Dequeue( (Packet) Pending[0] );
        }

        /// <summary>
        /// Remove the next complete PES package from the waiting queue
        /// and <see cref="SendTo"/> the transport stream.
        /// </summary>
        /// <remarks>
        /// Only call this method if the queue is not empty.
        /// </remarks>
        /// <param name="packet">First packet already read from the queue.</param>
        /// <returns>Set if anything has been sent.</returns>
        private bool Dequeue( Packet packet )
        {
            // Dequeue
            Pending.RemoveAt( 0 );

            // Send all
            return packet.SendTo();
        }

        /// <summary>
        /// Send all complete PES packages from the waiting queue to
        /// the transport stream which have no valid PTS.
        /// <seealso cref="Dequeue(Packet)"/>
        /// </summary>
        /// <remarks>
        /// If a PTS skip has been detected the queue will be cleared, too.
        /// </remarks>
        /// <returns>Set if anything has been sent.</returns>
        public bool DequeueNoPTS()
        {
            // Return flag
            bool send = false;

            // Process all
            while (Pending.Count > 0)
            {
                // Load the first one
                Packet peek = (Packet) Pending[0];

                // No need to process
                if (!IgnorePTS)
                    if (peek.PTS >= 0)
                        break;

                // Dequeue
                if (Dequeue( peek ))
                    send = true;
            }

            // Report
            return send;
        }

        /// <summary>
        /// Shrink waiting queue if it becomes too long.
        /// <seealso cref="SendTo"/>
        /// </summary>
        /// <returns>Set if anything has been sent.</returns>
        public bool DequeueOverflow()
        {
            // Answer
            bool send = false;

            // Set the limit
            const int limit = 400;

            // Process overflow data
            if (Pending.Count > limit)
            {
                // Count
                ++m_Overflow;

                // Send out at least 20%
                while (Pending.Count > limit * 80 / 100)
                    if (Dequeue())
                        send = true;
            }

            // Report
            return send;
        }

        /// <summary>
        /// Send this PES packet to the transport stream.
        /// <seealso cref="Manager.SendBuffer"/>
        /// </summary>
        /// <returns>Set if anything has been sent.</returns>
        private bool SendTo()
        {
            // See if video length should be calculated
            if (Manager.SetVideoLength && (StreamTypes.Video == m_Video))
            {
                // Find the first buffer available
                for (int i = 0; i < Parts.Count; ++i)
                {
                    // Attach to the buffer
                    byte[] buffer = (byte[]) Parts[i];

                    // Skip
                    if (null == buffer) continue;

                    // Not usable
                    if (buffer.Length < 10) break;

                    // Read flags
                    byte flags = (byte) (0xf0 & buffer[3]);

                    // Check TS header
                    if (0x47 != buffer[0]) break;

                    // Check packet start
                    if (0x40 != (0x40 & buffer[1]))
                    {
                        // It's allowed to skip the PCR (adaption only) packet
                        if (0x20 == flags)
                        {
                            // Be sure that it's only the one TS package
                            if (Manager.FullSize == buffer.Length) continue;

                            // No chance
                            break;
                        }

                        // Some different package - strange but better to stop here
                        break;
                    }

                    // Offset
                    int offset = 4;

                    // Test mode
                    if (0x10 != flags)
                    {
                        // See if only adaption field data is used
                        if (0x30 != flags) break;

                        // Yes, correct offset
                        offset += buffer[4];

                        // Length field
                        ++offset;

                        // Check boundary
                        if ((offset + 5) >= buffer.Length) break;
                    }

                    // Check the PES header
                    if (0x00 != buffer[offset + 0]) break;
                    if (0x00 != buffer[offset + 1]) break;
                    if (0x01 != buffer[offset + 2]) break;
                    if (0xe0 != (0xf0 & buffer[offset + 3])) break;

                    // Wipe out length
                    buffer[offset + 4] = 0;
                    buffer[offset + 5] = 0;

                    // Done
                    break;
                }
            }

            // Flag
            bool send = false;

            // Send all
            foreach (byte[] buffer in Parts)
                if (null != buffer)
                    if (m_Manager.SendBuffer( buffer, this ))
                        send = true;

            // Discard all
            Parts.Clear();

            // Report
            return send;
        }

        /// <summary>
        /// Check if there is at least one complete PES package in the waiting queue.
        /// </summary>
        public bool HasQueue
        {
            get
            {
                // Report
                return (Pending.Count > 0);
            }
        }

        /// <summary>
        /// Report the PTS of the first complete PES package in the waiting
        /// queue.
        /// </summary>
        /// <remarks>
        /// Do not call this property if the waiting queue is empty.
        /// </remarks>
        public long FirstPTS
        {
            get
            {
                // Load
                long pts = ((Packet) Pending[0]).PTS;

                // Video will be shifted a bit
                if (StreamTypes.Video == m_Video)
                    pts += m_Manager.ActiveVideoDelay;

                // Report
                return pts;
            }
        }

        /// <summary>
        /// Get the maximum size of the waiting queue.
        /// </summary>
        public int MaxQueueLength
        {
            get
            {
                // Report
                return m_MaxQueue;
            }
        }

        /// <summary>
        /// Get the number of packets sent due to overflow condition.
        /// </summary>
        public int Overflows
        {
            get
            {
                // Report
                return m_Overflow;
            }
        }

        /// <summary>
        /// Call to mark this as a holder of video packets.
        /// </summary>
        /// <param name="isVideo">Set to indicate a video stream, unset for an audio stream. 
        /// Do not call this method for any other stream type.</param>
        public void SetAudioVideo( bool isVideo )
        {
            // Remember
            m_Video = isVideo ? StreamTypes.Video : StreamTypes.Audio;
        }

        /// <summary>
        /// Reports if this is an audio or video stream package manager.
        /// </summary>
        public bool IsAudioOrVideo
        {
            get
            {
                // Report
                return (StreamTypes.Other != m_Video);
            }
        }
    }
}
