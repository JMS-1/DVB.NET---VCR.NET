using System;
using System.Collections;

namespace JMS.DVB.TS
{
    /// <summary>
    /// Diese Schnittstelle wird von Objekten implementiert, die an dem voranalysierten PES Paketstrom
    /// interessiert sind, den ein <see cref="StreamBase"/> Objekt erzeugt.
    /// </summary>
    public interface IStreamConsumer
    {
        /// <summary>
        /// Übermittelt Nutzdaten.
        /// </summary>
        /// <param name="counter">Paketzähler für den zugeordneten Datenstrom im <i>Transport Stream</i>.</param>
        /// <param name="pid">Nummer des Datenstroms.</param>
        /// <param name="buffer">Puffer mit Daten.</param>
        /// <param name="start">Das erste im Puffer zu verwendende Byte.</param>
        /// <param name="packs">Die Anzahl der <i>Transport Stream</i> Pakete, die zu übertragen sind.</param>
        /// <param name="isFirst">Gesetzt, wenn die Daten mit einem PES Paketkopf beginnen.</param>
        /// <param name="sizeOfLast">Anzahl der Daten im letzten <i>Transport Stream</i> Paket.</param>
        /// <param name="pts">Zeitstempel, sofern bekannt.</param>
        void Send( ref int counter, int pid, byte[] buffer, int start, int packs, bool isFirst, int sizeOfLast, long pts );

        /// <summary>
        /// Übermittelt die Systemzeit.
        /// </summary>
        /// <param name="counter">Paketzähler für den zugeordneten Datenstrom im <i>Transport Stream</i>.</param>
        /// <param name="pid">Nummer des Datenstroms.</param>
        /// <param name="pts">Zeitstempel, aus dem die Systemzeit abgeleitet wird.</param>
        void SendPCR( int counter, int pid, long pts );

        /// <summary>
        /// Gesetzt, wenn die Systemzeit bekannt ist.
        /// </summary>
        bool PCRAvailable { get; }
    }

    /// <summary>
    /// Erweiterte Schnittstelle für Verbraucher von voranalysierten PES Paketströmen.
    /// </summary>
    public interface IStreamConsumer2 : IStreamConsumer
    {
        /// <summary>
        /// Gesetzt, wenn keine Analyse des Paketstroms erwünscht ist.
        /// </summary>
        bool IgnoreInput { get; }
    }

    /// <summary>
    /// Manager base class for all PES streams inside a transport stream.
    /// </summary>
    public abstract class StreamBase : IDisposable
    {
        /// <summary>
        /// Current byte position in the stream.
        /// </summary>
        protected long Position = 0;

        /// <summary>
        /// Buffer used during on-the-fly parsing of a PES stream.
        /// </summary>
        private byte[] Delayed = new byte[Manager.PacketSize + 6];

        /// <summary>
        /// Current parsing state.
        /// </summary>
        private ParseStates State = ParseStates.Synchronize;

        /// <summary>
        /// Expected position of the next PES header.
        /// </summary>
        private long NextPosition = -1;

        /// <summary>
        /// Last PES start code byte found.
        /// </summary>
        private byte LastStartCode = 0;

        /// <summary>
        /// A guess for the start code to use.
        /// </summary>
        private byte UnconfirmedStartCode = 0;

        /// <summary>
        /// Used for startup synchronisation to find the correct start code.
        /// </summary>
        private int ValidationCount = 0;

        /// <summary>
        /// See if this is the very first packet.
        /// </summary>
        private bool VeryFirstPacket = true;

        /// <summary>
        /// The current start code for this PES stream - once set it may not be changed 
        /// later on.
        /// </summary>
        private byte StartCode = 0x00;

        /// <summary>
        /// Number of bytes currently in the look-ahead buffer <see cref="Delayed"/>.
        /// </summary>
        private int DelayedBytes = 0;

        /// <summary>
        /// Set when the next data sent to the transport stream starts with a PES header.
        /// </summary>
        private bool IsFirst = true;

        /// <summary>
        /// Last length upper byte found while parsing.
        /// </summary>
        private int LastLenHigh = 0;

        /// <summary>
        /// Last length lower byte found while parsing.
        /// </summary>
        private int LastLenLow = 0;

        /// <summary>
        /// Related transport stream.
        /// </summary>
        protected IStreamConsumer Consumer;

        /// <summary>
        /// Check for extended functionality.
        /// </summary>
        private IStreamConsumer2 m_Consumer2;

        /// <summary>
        /// Transport stream packet counter.
        /// </summary>
        private int Counter = 0;

        /// <summary>
        /// Set if this stream provides the PCR from the PTS of the PES headers.
        /// </summary>
        private bool IsPCR;

        /// <summary>
        /// Unset if we are awaiting the first key frame.
        /// </summary>
        private bool EnableDataFlow;

        /// <summary>
        /// The transport stream identifier for this stream.
        /// </summary>
        public readonly short PID;

        /// <summary>
        /// Counts how often we found non-aligned PES packets.
        /// </summary>
        public long LostData = 0;

        /// <summary>
        /// Initialize this instance.
        /// </summary>
        /// <remarks>
        /// This stream will not provide the PCR.
        /// </remarks>
        /// <param name="consumer">Related transport stream.</param>
        /// <param name="pid">Corresponding transport stream identifier.</param>
        protected StreamBase( IStreamConsumer consumer, short pid )
            : this( consumer, pid, false )
        {
        }

        /// <summary>
        /// Initialize this instance.
        /// </summary>
        /// <param name="consumer">Related transport stream.</param>
        /// <param name="pid">Corresponding transport stream identifier.</param>
        /// <param name="isPCR">Set if this stream provides the PCR from the PTS information
        /// in the PES headers.</param>
        protected StreamBase( IStreamConsumer consumer, short pid, bool isPCR )
        {
            // Remember
            m_Consumer2 = consumer as IStreamConsumer2;
            Consumer = consumer;
            IsPCR = isPCR;
            PID = pid;

            // Configure
            EnableDataFlow = !AwaitKeyFrame || !IsPCR;
        }

        /// <summary>
        /// Report, if input should be discarded.
        /// </summary>
        protected bool IgnoreInput
        {
            get
            {
                // Report
                return (null != m_Consumer2) && m_Consumer2.IgnoreInput;
            }
        }

        /// <summary>
        /// Process the indicated number of bytes and sent data to the transport
        /// stream as needed.
        /// </summary>
        /// <param name="buffer">Some buffer.</param>
        /// <param name="start">First byte to process.</param>
        /// <param name="length">Total number of bytes to process.</param>
        public virtual void AddPayload( byte[] buffer, int start, int length )
        {
            // Not active
            if (IgnoreInput)
                return;

            // All
            for (int i = start, l = length; l-- > 0; )
            {
                // To test
                byte test = buffer[i++];

                // Count total number of bytes
                ++Position;

                // Check mode
                if (ParseStates.Synchronize == State)
                {
                    // Wait for zero
                    if (test == 0)
                        State = ParseStates.Found0;

                    // Next
                    continue;
                }

                // Check mode
                if (ParseStates.Found00 == State)
                {
                    // Wait for one
                    if (test == 1)
                    {
                        // Next
                        State = ParseStates.Found001;

                        // Next
                        continue;
                    }

                    // Fall back
                    State = ParseStates.Found0;
                }

                // Check mode
                if (ParseStates.Found0 == State)
                {
                    // Set
                    State = (test == 0) ? ParseStates.Found00 : ParseStates.Synchronize;

                    // Next
                    continue;
                }

                // Check mode
                if (ParseStates.Found001 == State)
                {
                    // Check for start code
                    if (StartCode != 0)
                    {
                        // Set
                        State = (test == StartCode) ? ParseStates.LenHigh : ((0 == test) ? ParseStates.Found0 : ParseStates.Synchronize);
                    }
                    else if (IsValidStartCode( test ))
                    {
                        // Remember
                        LastStartCode = test;

                        // Set
                        State = ParseStates.LenHigh;
                    }
                    else
                    {
                        // Restart
                        State = (test == 0) ? ParseStates.Found0 : ParseStates.Synchronize;
                    }

                    // Next
                    continue;
                }

                // Length high byte
                if (ParseStates.LenHigh == State)
                {
                    // Remember
                    LastLenHigh = test;

                    // Advance
                    State = ParseStates.LenLow;

                    // Go on
                    continue;
                }

                // Length low byte
                if (ParseStates.LenLow == State)
                {
                    // Remember
                    LastLenLow = test;

                    // Advance
                    State = ParseStates.Found;
                }

                // Not found
                if (ParseStates.Found != State)
                    continue;

                // Construct length
                int pesLength = LastLenLow + 256 * LastLenHigh;

                // Validate
                if (!IsValidLength( pesLength ))
                {
                    // Reset state
                    if (LastLenLow == 0)
                        if (LastLenHigh == 0)
                            State = ParseStates.Found00;
                        else
                            State = ParseStates.Found0;
                    else
                        State = ParseStates.Synchronize;

                    // Go on
                    continue;
                }

                // Length to process
                int found = i - start;

                // Flush buffers
                Flush( buffer, start, found, 6 );

                // Time to adjust delayed buffer
                if (StartCode == 0)
                {
                    // Copy to the very beginning
                    Array.Copy( Delayed, DelayedBytes - 6, Delayed, 0, 6 );

                    // Reset
                    DelayedBytes = 6;
                }

                // Next write will be a start
                IsFirst = true;

                // Readjust buffer
                start = i;
                length -= found;

                // Remember
                StartCode = LastStartCode;

                // Reset
                State = ParseStates.Synchronize;
            }

            // Set delay mode
            int delay = 0;

            // Check state
            switch (State)
            {
                case ParseStates.Found0: delay = 1; break;
                case ParseStates.Found00: delay = 2; break;
                case ParseStates.Found001: delay = 3; break;
                case ParseStates.LenHigh: delay = 4; break;
                case ParseStates.LenLow: delay = 5; break;
            }

            // All but these
            Flush( buffer, start, length, delay );
        }

        /// <summary>
        /// Send some data to the transport stream.
        /// </summary>
        /// <remarks>
        /// <b>Problem:</b> in very rare cases pseudo-PES headers in the data at the
        /// very beginning of the PES stream may be misinterpreted in a ways that leads
        /// to invalid data being sent to the transport stream.
        /// </remarks>
        /// <param name="buffer">Buffer to use.</param>
        /// <param name="start">First byte to send.</param>
        /// <param name="length">Number of bytes to send.</param>
        /// <param name="delay">Number of bytes not to send but to keep in the
        /// look-ahead buffer.</param>
        private void Flush( byte[] buffer, int start, int length, int delay )
        {
            // Calculate the overall length
            int len = DelayedBytes + length - delay;

            // Get the number of full packets to send
            int packs = len / Manager.PacketSize;

            // If there is at least one packed cleanup delay first
            while ((packs > 0) && (DelayedBytes > 0))
            {
                // Bytes eaten up
                int steal = Manager.PacketSize - DelayedBytes;

                // Check mode
                if (steal <= 0)
                {
                    // Send and correct
                    DelayedBytes -= Send( Delayed, 0, 1, Manager.PacketSize );

                    // Copy
                    if (DelayedBytes > 0) Array.Copy( Delayed, Manager.PacketSize, Delayed, 0, DelayedBytes );
                }
                else
                {
                    // Fill up the packet
                    Array.Copy( buffer, start, Delayed, DelayedBytes, steal );

                    // Adjust
                    start += steal;
                    length -= steal;

                    // Reset
                    DelayedBytes = 0;

                    // Send and correct
                    Send( Delayed, 0, 1, Manager.PacketSize );
                }

                // Full packet less
                len -= Manager.PacketSize;

                // Correct overall counter
                --packs;
            }

            // Send
            if (packs > 0)
            {
                // Send
                int trans = Send( buffer, start, packs, Manager.PacketSize );

                // Correct
                start += trans;
                length -= trans;

                // Reset length
                len -= trans;
            }

            // Check for a new boundary
            if ((len > 0) && (6 == delay))
            {
                // Fill up the packet
                Array.Copy( buffer, start, Delayed, DelayedBytes, length );

                // Remember
                DelayedBytes += length;

                // Send
                DelayedBytes -= Send( Delayed, 0, 1, len );

                // Correct buffer
                if (DelayedBytes > 0)
                    Array.Copy( Delayed, len, Delayed, 0, DelayedBytes );

                // Finished
                return;
            }

            // All done
            if (length < 1) return;

            // Remember
            Array.Copy( buffer, start, Delayed, DelayedBytes, length );

            // Adjust
            DelayedBytes += length;
        }

        /// <summary>
        /// Process the indicated bytes.
        /// <seealso cref="AddPayload(byte[], int, int)"/>
        /// </summary>
        /// <param name="buffer">All of this buffer should be processed.</param>
        public void AddPayload( byte[] buffer )
        {
            // Full buffer
            AddPayload( buffer, 0, buffer.Length );
        }

        /// <summary>
        /// Check if a PES start code byte is valid for this type of stream.
        /// </summary>
        /// <param name="code">PES start code byte to test.</param>
        /// <returns>Set if the PES start code is valid for this type of stream.</returns>
        protected abstract bool IsValidStartCode( byte code );

        /// <summary>
        /// Check if the PES packet length is consistent.
        /// </summary>
        /// <param name="length">Current length.</param>
        /// <returns>Set if the current PES header looks valid.</returns>
        protected virtual bool IsValidLength( int length )
        {
            // The next synchronisation point has not been reached
            if (Position < NextPosition)
                return false;

            // See if the start code has already been fixed once and forever
            if (0 != StartCode)
            {
                // Check for error
                if (Position > NextPosition)
                    ++LostData;

                // Next position where we expect a start code - actually we may have lost a header but we can't do anything to correct this
                NextPosition = Position + 6 + length;

                // Send data
                return true;
            }

            // See if this is a regular hit on the expected synchronisation point during startup
            if ((Position != NextPosition) || (LastStartCode != UnconfirmedStartCode))
            {
                // Start synchronizing on the current start code
                UnconfirmedStartCode = LastStartCode;

                // Must occur at least three times to be accepted
                ValidationCount = 3;
            }

            // Will be acceptable if found at this position
            NextPosition = Position + 6 + length;

            // Done
            return (--ValidationCount < 1);
        }

        /// <summary>
        /// Send the indicated number of transport stream packets to the transport stream.
        /// <seealso cref="JMS.DVB.TS.Manager.Send"/>
        /// </summary>
        /// <remarks>
        /// This function may automatically generate a PCR entry if necessary and if
        /// the current stream is the PCR source. To do so the PCR will simply be copied 
        /// from the PTS.
        /// </remarks>
        /// <param name="buffer">Data source.</param>
        /// <param name="start">First byte to send.</param>
        /// <param name="packs">Number of <see cref="JMS.DVB.TS.Manager.PacketSize"/> transport packets to send.</param>
        /// <param name="last">Number of bytes in the last packet - which eventually
        /// will be padded.</param>
        /// <returns>Number of bytes sent.</returns>
        private int Send( byte[] buffer, int start, int packs, int last )
        {
            // Get the total size
            int total = (packs - 1) * Manager.PacketSize + last;

            // Send
            if (0 != StartCode)
            {
                // PTS to report
                long pts = -1;

                // See if this is a PES Header
                if (IsFirst)
                {
                    // See if there is enough stuff available to read a PTS from the PES header
                    if ((packs > 1) || ((1 == packs) && (last >= 14)))
                    {
                        // Size of header extension
                        byte ext = buffer[start + 8];

                        // See if PTS and DTS is provided
                        if ((ext >= 5) && (0x80 == (0x80 & buffer[start + 7])))
                        {
                            // Load parts
                            long b4 = (buffer[start + 9] >> 1) & 0x7;
                            long b3 = buffer[start + 10];
                            long b2 = buffer[start + 11] >> 1;
                            long b1 = buffer[start + 12];
                            long b0 = buffer[start + 13] >> 1;

                            // Create
                            pts = b0 + 128 * (b1 + 256 * (b2 + 128 * (b3 + 256 * b4)));
                        }
                    }

                    // See if we should generate a PCR clock reference
                    if (IsPCR && (pts >= 0))
                    {
                        // Is this a key frame
                        bool? isKeyFrame = null;

                        // See if we have to check if this is a key frame (may be expensive)
                        if (!VeryFirstPacket || !EnableDataFlow)
                        {
                            // Calculate
                            isKeyFrame = IsKeyFrame( buffer, start, total );

                            // See if we can send data
                            if (VeryFirstPacket) EnableDataFlow = isKeyFrame.Value;
                        }

                        // See if we must send a PCR
                        if (EnableDataFlow)
                            if (VeryFirstPacket || isKeyFrame.Value)
                            {
                                // Correct counter
                                if (VeryFirstPacket) ++Counter;

                                // Send PCR
                                Consumer.SendPCR( Counter, PID, pts );
                            }
                    }
                }

                // Data
                if (EnableDataFlow)
                {
                    // Forward data
                    Consumer.Send( ref Counter, PID, buffer, start, packs, IsFirst, last, pts );

                    // Did some
                    VeryFirstPacket = false;
                }
            }

            // Reset
            IsFirst = false;

            // Report
            return total;
        }

        /// <summary>
        /// Report if this stream uses a small piping buffer.
        /// </summary>
        protected virtual bool IsVideo
        {
            get
            {
                // All but video
                return false;
            }
        }

        /// <summary>
        /// Reports if streaming has to be synchronized with a key frame.
        /// </summary>
        protected virtual bool AwaitKeyFrame
        {
            get
            {
                // Start streaming with the first byte
                return false;
            }
        }

        /// <summary>
        /// See, if the indicated position in the buffer is a keyframe. Prior to
        /// calling this method the caller checks that it really starts with
        /// a PES header.
        /// </summary>
        /// <param name="buffer">Some buffer.</param>
        /// <param name="start">First byte of the packet.</param>
        /// <param name="length">Number of bytes in the packet.</param>
        /// <returns>Set if this packet starts a key frame.</returns>
        protected virtual bool IsKeyFrame( byte[] buffer, int start, int length )
        {
            // Normally we consider each PES a key frame
            return true;
        }

        #region IDisposable Members

        /// <summary>
        /// Cleanup.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }
}
