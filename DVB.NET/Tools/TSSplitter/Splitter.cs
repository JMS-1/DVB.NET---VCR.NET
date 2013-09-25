using System;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace TSSplitter
{
	/// <summary>
	/// Simple application to verify and demultiplex a TS stream to
	/// its individual parts. The stream is expected to be perfect.
	/// </summary>
	public class Splitter: IDisposable
	{
		/// <summary>
		/// Size of a single packet.
		/// </summary>
		private const int PacketSize = 188;

		/// <summary>
		/// Our buffer size.
		/// </summary>
		private const int PacketsInBuffer = 100000;

		/// <summary>
		/// The streams in this transport stream.
		/// </summary>
		private Hashtable Streams = new Hashtable();

		/// <summary>
		/// Allocate buffer once.
		/// </summary>
		private static byte[] Buffer = new byte[PacketSize * PacketsInBuffer];

		/// <summary>
		/// The file we are working on.
		/// </summary>
		private FileInfo File;

		/// <summary>
		/// The log file to create.
		/// </summary>
		private StreamWriter LogFile = null;

		/// <summary>
		/// Current PID for counter.
		/// </summary>
		private int LogPID = -1;

		/// <summary>
		/// Current counter.
		/// </summary>
		private int LogCount = 0;

		/// <summary>
		/// Set if counter reset on a sequence start.
		/// </summary>
		private bool LogStartsClosed = false;

		/// <summary>
		/// Create a new splitter instance.
		/// </summary>
		/// <param name="path">Full path to a TS file.</param>
		private Splitter(string path)
		{
			// Load it
			File = new FileInfo(path);

			// Check it
			if ( !File.Exists ) throw new ArgumentException("File not found: " + path, "path");

			// Create log file
			LogFile = new StreamWriter(Path.ChangeExtension(File.FullName, ".log"), false);

			// Create header
			LogFile.WriteLine("TS ID\tCount\tFirst\tEmpty");
		}

		/// <summary>
		/// Process all files given.
		/// </summary>
		/// <param name="args">Command line arguments with each argument
		/// being the full path to a TS file.</param>
		[STAThread] public static void Main(string[] args)
		{
			// All files
			foreach ( string path in args ) Process(path);

			// For now
			Console.WriteLine("Press <Enter> to Continue");
			Console.ReadLine();
		}

		/// <summary>
		/// Analyse and demultiplex a single TS file.
		/// </summary>
		/// <param name="path">Full path to the file.</param>
		private static void Process(string path)
		{
			// Be safe
			try
			{
				// Report
				Console.WriteLine("Processing {0}", path);

				// Run it
				using (Splitter inst = new Splitter(path)) inst.Process();
			}
			catch (Exception e)
			{
				// Report
				Console.WriteLine(e.ToString());
			}
		}

		/// <summary>
		/// Analyse and demultiplex a single TS file.
		/// </summary>
		private void Process()
		{
			// Open the file
			using (FileStream read = new FileStream(File.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, Buffer.Length))
			{
				// Full offset
				long blockpos = 0;

				// Load
				for ( int n ; (n = read.Read(Buffer, 0, Buffer.Length)) > 0 ; blockpos += n )
				{
					// Forward
					for ( int i = 0 ; (i + PacketSize) <= n ; i+= PacketSize )
						try
						{
							// Run
							Process(i);
						}
						catch
						{
							// Report
							Console.WriteLine("Block at {0}", blockpos);

							// Rethrow
							throw;
						}
				}
			}
		}

		/// <summary>
		/// Process a single TS packet.
		/// </summary>
		/// <param name="i">The offset in our buffer where the packet lives.</param>
		private void Process(int i)
		{
			// Corrupted
			//if (0 == Buffer[i]) return;

			// Test
			if ( 0x47 != Buffer[i++] ) throw new ArgumentException("not a TS package at " + i.ToString(), "i");

			// Decode all (slow)
			bool terr = (0x80 == (0x80&Buffer[i]));
			bool fstp = (0x40 == (0x40&Buffer[i]));
			bool prio = (0x20 == (0x20&Buffer[i]));
			int pidh = Buffer[i++]&0x1f;
			int pidl = Buffer[i++];
			int pid = pidl + 256 * pidh;
			int scram = (Buffer[i]>>6)&0x3;
			int adap = (Buffer[i]>>4)&0x3;
			int count = Buffer[i++]&0xf;

			// Skip padding
			if (0x1fff == pid) return;

			// Not supported by VCR.NET
			if ( terr ) throw new ArgumentException("unexpected transmission error at " + i.ToString(), "i");
			//if ( prio ) throw new ArgumentException("unexpected priority at " + i.ToString(), "i");
			//if ( 0 != scram ) throw new ArgumentException("unexpected scrambling at " + i.ToString(), "i");
			if ( 0 == adap ) throw new ArgumentException("expected adaption at " + i.ToString(), "i");

			// Split again
			bool adaption = (2 == (2&adap));
			bool payload = (1 == (1&adap));

			// Find the handler
			TSStream stream = (TSStream)Streams[pid];

			// Create
			if ( null == stream )
			{
				// New one
				stream = new TSStream(File, pid);

				// Remember
				Streams[pid] = stream;
			}

			// Forward
			bool hasData = stream.Process(Buffer, i, adaption, payload, fstp, count, LogFile);

			// Report
			if ( (0 != pid) && (256 != pid) )
				if ( (pid == LogPID) && hasData && !fstp )
				{
					// Count hits
					++LogCount;
				}
				else
				{
					// Report
					if ( LogCount > 0 ) LogFile.WriteLine("{0}\t{1}\t{2}", LogPID, LogCount, LogStartsClosed ? "+" : "-");

					// Reset
					LogStartsClosed = fstp;
					LogPID = pid;
					LogCount = 1;

					// Again if filler
					if ( !hasData )
					{
						// Debug only
						PCRDecode(Buffer, i);

						// Report
						LogFile.WriteLine("{0}\t1\t{1}\t*", LogPID, LogStartsClosed ? "+" : "-");

						// Reset
						LogCount = 0;
						LogPID = -1;
					}
				}
		}

		[Conditional("VERBOSE")] private void PCRDecode(byte[] buffer, int offset)
		{
			// Check
			if ( 0xb7 != buffer[offset++] ) return;
			if ( 0x10 != buffer[offset++] ) return;

			// Load PCR
			long pcr0 = buffer[offset++];
			long pcr1 = buffer[offset++];
			long pcr2 = buffer[offset++];
			long pcr3 = buffer[offset++];
			long pcr4 = buffer[offset++]>>7;
			long pcr = pcr4 + 2 *(pcr3 + 256 * (pcr2 + 256 *(pcr1 + 256 * pcr0)));

			// Report
			LogFile.WriteLine("PCR={0}", new TimeSpan(pcr * 1000 / 9));
		}


		#region IDisposable Members

		/// <summary>
		/// Dispose this instance which has to close all <see cref="TSStream"/> files.
		/// </summary>
		public void Dispose()
		{
			// Forward
			foreach ( TSStream stream in Streams.Values ) stream.Dispose();

			// Close 
			if ( null != LogFile )
			{
				// Finish
				if ( LogCount > 0 ) LogFile.WriteLine("{0}\t{1}\t{2}", LogPID, LogCount, LogStartsClosed ? "+" : "-");

				// Close
				LogFile.Close();

				// Forget
				LogFile = null;
			}

			// Clear
			Streams.Clear();
		}

		#endregion
	}
}
