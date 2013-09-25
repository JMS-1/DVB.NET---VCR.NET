using System;
using System.IO;
using System.Diagnostics;

namespace TSSplitter
{
	/// <summary>
	/// Represents a single stream inside a TS file.
	/// </summary>
	public class TSStream: IDisposable
	{
		/// <summary>
		/// The TS file we are part of.
		/// </summary>
		private FileInfo m_File;

		/// <summary>
		/// Our private stream.
		/// </summary>
		private FileStream m_Stream = null;

		/// <summary>
		/// The transport identifier of this stream.
		/// </summary>
		private int m_PID;

		/// <summary>
		/// Expected counter field.
		/// </summary>
		private int m_Counter = -1;

		/// <summary>
		/// Create a new stream instance.
		/// </summary>
		/// <param name="master">Master TS file.</param>
		/// <param name="pid">Out stream identifier.</param>
		internal TSStream(FileInfo master, int pid)
		{
			// Remember
			m_File = master;
			m_PID = pid;

			// Report
			Console.WriteLine("Found Stream 0x{0:x}", pid);
		}

		/// <summary>
		/// Process a single TS package.
		/// </summary>
		/// <param name="buffer">Data holder.</param>
		/// <param name="i">Index of the first byte.</param>
		/// <param name="hasAdaption">Set if adaption field is present.</param>
		/// <param name="hasPayload">Set if payload is present.</param>
		/// <param name="counter">Actual counter.</param>
		/// <param name="pes">Set if this is a packet including the PES header.</param>
		/// <param name="log">Log file to use.</param>
		/// <returns>Set if any data was in the package.</returns>
		public bool Process(byte[] buffer, int i, bool hasAdaption, bool hasPayload, bool pes, int counter, StreamWriter log)
		{
			// Get the number of payload bytes
			int payload = 184;

			// Skip if no payload is present
			if ( hasAdaption )
			{
				// Correct
				payload -= buffer[i];

				// Test
				if ( --payload < 0 ) throw new ArgumentException("stream 0x" + m_PID.ToString("x") + ": adaption field overrun at " + i.ToString());
			}

			// Check type
			bool noinc = (hasAdaption && !hasPayload && (0 == payload));

			// Correct very first call
			bool first = (m_Counter < 0);

			// Very first call
			if ( first )
			{
				// Use as is
				m_Counter = counter;
			}
			else if ( noinc )
			{
				// No need to increment
				counter = (counter + 1)&0xf;
			}

			// Check counter
			if ( counter != m_Counter ) throw new ArgumentException("stream 0x" + m_PID.ToString("x") + ": counter mismatch at " + i.ToString() + ", expected " + m_Counter.ToString() + ", got " + counter.ToString(), "i");

			// Count only if payload is present
			if ( first || !noinc ) m_Counter = (m_Counter + 1)&0xf;

			// Done
			if ( payload < 1 ) return false;

			// Create the file
			if ( null == m_Stream )
			{
				// Get the name
				string path = m_File.DirectoryName + @"\" + m_File.Name + "_" + m_PID.ToString() + ".pes";

				// Open
				m_Stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 1000000);
			}

			// Process PES header
			if ( pes ) PESDecode(buffer, i + 184 - payload, payload, log);

			// Send data
			m_Stream.Write(buffer, i + 184 - payload, payload);

			// Games (Debug only)
			//m_Stream.Write(buffer, i - 4, 188);

			// Yeah
			return true;
		}

		[Conditional("VERBOSE")] private void PESDecode(byte[] buffer, int offset, int length, StreamWriter log)
		{
			// Too small
			if ( length < 9 ) return;

			// Check for PES header code
			if ( 0x00 != buffer[offset++] ) return;
			if ( 0x00 != buffer[offset++] ) return;
			if ( 0x01 != buffer[offset++] ) return;

			// Load
			byte code = buffer[offset++];

			// Check
			bool audio = (((code >= 0xc0) && (code < 0xe0)) || (0xbd == code));

			// Check
			if ( !audio && ((code < 0xc0) || (code >= 0xf0)) ) return;

			// Skip
			offset += 2;
			length -= 6;

			// Check for PTS/DTS
			bool pts = (0 != (buffer[offset + 1]&0x80));
			bool dts = (0 != (buffer[offset + 1]&0x40));

			// Check
			if ( !pts && !dts ) return;

			// Adjust
			offset += 3;

			// Read all
			long PTS = 0, DTS = 0;
			
			// PTS has to advance offset
			if ( pts )
			{
				// Read
				PTS = ReadTimeStamp(buffer, offset);

				// Advance
				offset += 5;
			}

			// DST can read as is
			if ( dts ) DTS = ReadTimeStamp(buffer, offset);

			// Report
			log.WriteLine("{0:x} PTS={1} DTS={2}", code, new TimeSpan(PTS * 1000 / 9), new TimeSpan(DTS * 1000 / 9));
		}

		private static long ReadTimeStamp(byte[] data, int offset)
		{
			// Load parts
			long b4 = (data[offset + 0] >> 1)&0x7;
			long b3 = data[offset + 1];
			long b2 = data[offset + 2] >> 1;
			long b1 = data[offset + 3];
			long b0 = data[offset + 4] >> 1;

			// Create
			return b0 + 128 * (b1 + 256 * (b2 + 128 * (b3 + 256 * b4)));
		}

		#region IDisposable Members

		public void Dispose()
		{
			// Close stream
			if ( null != m_Stream )
			{
				// Report
				Console.WriteLine("stream 0x{0:x} has {1} bytes", m_PID, m_Stream.Length);

				// Close
				m_Stream.Close();

				// Discard
				m_Stream = null;
			}
		}

		#endregion
	}
}
