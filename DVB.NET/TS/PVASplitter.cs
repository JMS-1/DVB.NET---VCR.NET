using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace JMS.DVB.TS
{
	/// <summary>
	/// Diese Klasse nimmt PVA Pakete entgegen und zerteilt den gesamten PVA
	/// Datenstrom in Bild- und Tonströme. Das Ergebnis wird in einen
	/// <i>Transport Stream</i> eingemischt.
	/// <seealso cref="Manager"/>
	/// </summary>
	public class PVASplitter : IDisposable
	{
		/// <summary>
		/// Die zu erzeugende Gesamtdatei.
		/// </summary>
		public readonly Manager TransportStream;

		/// <summary>
		/// Konstante, in der nur Bit 32 gesetzt ist.
		/// </summary>
		private const long PTSOffset = ((long)1) << 32;

		/// <summary>
		/// Interne Zustände während der Analyse der PVA Daten.
		/// </summary>
		private enum SyncStates
		{
			/// <summary>
			/// Erstes Zeichen (A) des Kontrollwortes wird erwartet.
			/// </summary>
			SyncA,

			/// <summary>
			/// Zweites Zeichen (V) des Kontrollwortes wird erwartet.
			/// </summary>
			SyncV,

			/// <summary>
			/// PVA Kopf wird eingelesen.
			/// </summary>
			Header,

			/// <summary>
			/// Optionaler Video PTS wird eingelesen.
			/// </summary>
			PTS,

			/// <summary>
			/// Nutzdaten werden übertragen.
			/// </summary>
			PayLoad
		};

		/// <summary>
		/// Der aktuelle Analysestand.
		/// </summary>
		private SyncStates m_State = SyncStates.SyncA;

		/// <summary>
		/// Dient zur Zwischenspeicherung eines Audio PES Kopfes.
		/// </summary>
		private byte[] m_AudioPES = new byte[9 + 256];

		/// <summary>
		/// Dient zur Rekonstruktion des Video PES Kopfes.
		/// </summary>
		private byte[] m_VideoPES = { 0x00, 0x00, 0x01, 0xef, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

		/// <summary>
		/// Dient zur Zwischenspeicherung des PVA Kopfes.
		/// </summary>
		private byte[] m_Header = new byte[6];

		/// <summary>
		/// Dient zur Zwischenspeicherung des PVA Video PTS.
		/// </summary>
		private byte[] m_PTS = new byte[4];

		/// <summary>
		/// Wird gesetzt, sobald der PES Kopf für den Videostrom rekonstruiert wurde.
		/// </summary>
		private bool m_PESCreated = false;

		/// <summary>
		/// Zählt, wie oft eine Anpassung des Audio PTS nicht möglich war.
		/// </summary>
		private long m_BadAudioPES = 0;

		/// <summary>
		/// Der PTS der Gesamtdatei.
		/// </summary>
		private long m_GuidePTS = -2;

		/// <summary>
		/// Wird gesetzt, sobald Daten in den Gesamtstrom eingespielt werden sollen.
		/// </summary>
		private bool m_Running = false;

		/// <summary>
		/// Gibt an, wieviele Videobytes eines Paketes noch vor den PES
		/// Kopf zu ziehen sind.
		/// </summary>
		private int m_VideoAlign = 0;

		/// <summary>
		/// Zählt, wie oft im PVA Kontrollwort nach einem A kein V kam.
		/// </summary>
		private long m_Restarted = 0;

		/// <summary>
		/// Zähler zum Füllen diverser Hilfsstrukturen.
		/// </summary>
		private int m_BytesLeft = 0;

		/// <summary>
		/// Der Videostrom in der Gesamtdatei.
		/// </summary>
		private VideoStream m_Video;

		/// <summary>
		/// Der Audiostrom in der Gesamtdatei.
		/// </summary>
		private AudioStream m_Audio;

		/// <summary>
		/// Position in den Nutzdaten des aktuellen Paketes.
		/// </summary>
		private int m_PacketPos = 0;

		/// <summary>
		/// Zählt, wieviele Bytes überprungen werden mußten, um das
		/// erste Zeichen (A) des Kontrollwortes zu finden.
		/// </summary>
		private long m_Skipped = 0;

		/// <summary>
		/// Die noch zu bearbeitenden Pakete.
		/// </summary>
		private Queue<byte[]> m_Waiting = new Queue<byte[]>();

		/// <summary>
		/// Benachrichtigt den <see cref="Thread"/> zur Bearbeitung der Pakete.
		/// </summary>
		private AutoResetEvent m_ReadQueue = new AutoResetEvent(false);

		/// <summary>
		/// Der eigentliche Verteiler.
		/// </summary>
		private Thread m_Worker = null;

		/// <summary>
		/// Erzeugt eine neue Instanz und bindet sie an einen <i>Transport
		/// Stream</i>.
		/// </summary>
		/// <param name="tsFile">Der <i>Transport Stream</i>, zu dem noch keine
		/// <param name="audioName">Der ISO Name der Sprache.</param>
		/// Datenströme hinzugefügt wurden.</param>
		public PVASplitter(Manager tsFile, string audioName)
		{
			// Remember
			TransportStream = tsFile;

			// Open streams
			m_Video = TransportStream.AddVideo(255);
			m_Audio = TransportStream.AddAudio(audioName);

			// Register self for PTS guidance
			TransportStream.PVASplitter = this;

			// Create reader
			m_Worker = new Thread(new ThreadStart(Worker));

			// Configure
			m_Worker.SetApartmentState(ApartmentState.MTA);

			// Run it
			m_Worker.Start();
		}

		/// <summary>
		/// Hier wird vom Gesamtstrom die Zeitbasis eines beliebigen Teilstroms
		/// übertragen.
		/// <seealso cref="CorrectByGuide"/>
		/// </summary>
		public long GuidePTS
		{
			set
			{
				// Remember
				m_GuidePTS = value;
			}
		}

		/// <summary>
		/// Bearbeitet einen Auszug aus dem PVA Datenstrom.
		/// </summary>
		/// <param name="buffer">Bytes aus dem PVA Datenstrom. Dieser wird
		/// intern verwendet und darf nach dem Aufruf nicht verändert werden.</param>
		public void AddPayload(byte[] buffer)
		{
			// Not active
			if (!m_Running) return;

			// Still awaiting guide
			if (-2 == m_GuidePTS) return;

			// Enqueue
			lock (m_Waiting) m_Waiting.Enqueue(buffer);

			// Wake up
			m_ReadQueue.Set();
		}

		/// <summary>
		/// Aktivieren.
		/// </summary>
		public void Start()
		{
			// Enable
			m_Running = true;
		}

		/// <summary>
		/// Deaktivieren.
		/// </summary>
		public void Stop()
		{
			// Enable
			m_Running = false;

			// Wakeup thread for termination
			m_ReadQueue.Set();

			// Wait for termination
			if (null != m_Worker)
			{
				// Wait for termination
				m_Worker.Join();

				// Reset
				m_Worker = null;

				// Clear memory
				m_Waiting.Clear();
			}
		}

		/// <summary>
		/// Die Trennung des PVA Datenstroms erfolgt auf einem eigenen Thread.
		/// </summary>
		private void Worker()
		{
			// As long as necessary
			for (; ; )
			{
				// Wait
				m_ReadQueue.WaitOne();

				// Process all
				for (; ; )
				{
					// Check for done
					if (!m_Running) return;

					// Read next
					byte[] buffer;

					// Safe read
					lock (m_Waiting)
					{
						// Got all
						if (m_Waiting.Count < 1) break;

						// Load
						buffer = m_Waiting.Dequeue();
					}

					// Process the buffer we currently read
					Worker(buffer, 0, buffer.Length);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		private void Worker(byte[] buffer, int start, int length)
		{
			while (length > 0)
			{
				// Find the first byte of the magic word
				if (SyncStates.SyncA == m_State)
				{
					// Find the sync byte
					for (; ; )
					{
						// Is there any more data
						if (length-- < 1) return;

						// Check for hit
						if (0x41 == buffer[start++]) break;

						// Count errors
						++m_Skipped;
					}

					// Next
					m_State = SyncStates.SyncV;
				}

				// Find the second byte of the magic word
				if (SyncStates.SyncV == m_State)
				{
					// Check if
					if (0x56 != buffer[start])
					{
						// Count
						++m_Restarted;

						// Start resync
						m_State = SyncStates.SyncA;

						// Test again
						continue;
					}

					// Adjust
					--length;
					++start;

					// Next
					m_State = SyncStates.Header;

					// Reset header counter
					m_BytesLeft = m_Header.Length;

					// Done
					if (length < 1) return;
				}

				// Find the header
				if (SyncStates.Header == m_State)
				{
					// How much to copy
					int len = Math.Min(m_BytesLeft, length);

					// Copy over
					Array.Copy(buffer, start, m_Header, m_Header.Length - m_BytesLeft, len);

					// Adjust
					m_BytesLeft -= len;
					length -= len;
					start += len;

					// Incomplete
					if (m_BytesLeft > 0) return;

					// See if PTS follows
					if (PTSPresent)
					{
						// Next
						m_State = SyncStates.PTS;

						// Reset header counter
						m_BytesLeft = m_PTS.Length;
					}
					else
					{
						// Report
						StartPackage();
					}

					// Done
					if (length < 1) return;
				}

				// Find the header
				if (SyncStates.PTS == m_State)
				{
					// How much to copy
					int len = Math.Min(m_BytesLeft, length);

					// Copy over
					Array.Copy(buffer, start, m_PTS, m_PTS.Length - m_BytesLeft, len);

					// Adjust
					m_BytesLeft -= len;
					length -= len;
					start += len;

					// Incomplete
					if (m_BytesLeft > 0) return;

					// Report
					StartPackage();

					// Done
					if (length < 1) return;
				}

				// How much to eat up
				int payload = Math.Min(m_BytesLeft, length);

				// Process
				SendPayload(buffer, start, payload);

				// Adjust
				m_BytesLeft -= payload;
				length -= payload;
				start += payload;

				// Start from scratch
				if (m_BytesLeft < 1) m_State = SyncStates.SyncA;
			}
		}

		/// <summary>
		/// Beginnt mit der Bearbeitung der Nutzdaten in einem PVA Paket.
		/// </summary>
		private void StartPackage()
		{
			// Next
			m_State = SyncStates.PayLoad;

			// Get length parts
			int high = m_Header[4];
			int low = m_Header[5];

			// Reset length counter
			m_BytesLeft = low + 256 * high;

			// Special
			if (PTSPresent) m_BytesLeft = Math.Max(m_BytesLeft - m_PTS.Length, 0);

			// Reset transfer state
			m_VideoAlign = m_Header[3] & 0x3;
			m_PESCreated = false;
			m_PacketPos = 0;
		}

		/// <summary>
		/// Meldet den 32-Bit PTS eine Videopaketes.
		/// </summary>
		private UInt32 PTS32
		{
			get
			{
				// Read parts
				UInt32 b3 = m_PTS[0];
				UInt32 b2 = m_PTS[1];
				UInt32 b1 = m_PTS[2];
				UInt32 b0 = m_PTS[3];

				// Construct
				return b0 + 256 * (b1 + 256 * (b2 + 256 * b3));
			}
		}

		/// <summary>
		/// Meldet, ob ein Videopaket mit folgendem 32-Bit PTS vorliegt.
		/// </summary>
		private bool PTSPresent
		{
			get
			{
				// Report
				return ((1 == m_Header[0]) && PTSFlagPresent);
			}
		}

		/// <summary>
		/// Meldet, ob zu dem aktuellen Paket ein PTS vorhanden ist.
		/// </summary>
		private bool PTSFlagPresent
		{
			get
			{
				// Report
				return (0x10 == (0x10 & m_Header[3]));
			}
		}

		/// <summary>
		/// Vearbeitet Nutzdaten.
		/// </summary>
		/// <param name="buffer">Paketdaten.</param>
		/// <param name="start">Erstes Byte der Nutzdaten.</param>
		/// <param name="length">Anzahl der Nutzdaten.</param>
		private void SendPayload(byte[] buffer, int start, int length)
		{
			// Skip
			if (length < 1) return;

			// Check mode
			switch (m_Header[0])
			{
				case 1: SendVideo(buffer, start, length); break;
				case 2: SendAudio(buffer, start, length); break;
			}

			// Adjust
			m_PacketPos += length;
		}

		/// <summary>
		/// Verarbeitet Nutzdaten des Videostroms.
		/// </summary>
		/// <remarks>
		/// Je nach Situation muß ein PES Kopf aus den PVA Daten rekonstruiert werden.
		/// </remarks>
		/// <param name="buffer">Paketdaten.</param>
		/// <param name="start">Erstes Byte der Nutzdaten.</param>
		/// <param name="length">Anzahl der Nutzdaten.</param>
		private void SendVideo(byte[] buffer, int start, int length)
		{
			// See how many bytes have to be skipped for alignment
			int align = Math.Min(m_VideoAlign, length);

			// See if alignment is necessary
			if (align > 0)
			{
				// Send
				m_Video.AddPayload(buffer, start, align);

				// Adjust
				m_VideoAlign -= align;
				length -= align;
				start += align;

				// Not yet done
				if (m_VideoAlign > 0) return;
			}

			// See if header has to be sent
			if (!m_PESCreated)
			{
				// Test once only
				m_PESCreated = true;

				// Check mode
				if (PTSPresent)
				{
					// Get the PTS
					long pts = PTS32;

					// Correct
					pts = CorrectByGuide(pts);

					// Fill the PES header
					m_VideoPES[7] = 0x80;
					m_VideoPES[8] = 0x05;
					m_VideoPES[9] = (byte)(0x21 | (2 * ((pts >> 30) & 0x7)));
					m_VideoPES[10] = (byte)((pts >> 22) & 0xff);
					m_VideoPES[11] = (byte)(0x01 | (2 * ((pts >> 15) & 0x7f)));
					m_VideoPES[12] = (byte)((pts >> 7) & 0xff);
					m_VideoPES[13] = (byte)(0x01 | (2 * (pts & 0x7f)));

					// Send the header
					m_Video.AddPayload(m_VideoPES, 0, 14);
				}
				else
				{
					// Fill the PES header
					m_VideoPES[7] = 0x00;
					m_VideoPES[8] = 0x00;

					// Send the header
					m_Video.AddPayload(m_VideoPES, 0, 9);
				}
			}

			// Send payload
			m_Video.AddPayload(buffer, start, length);
		}

		/// <summary>
		/// Verarbeitet Nutzdaten des Audiostroms.
		/// </summary>
		/// <param name="buffer">Paketdaten.</param>
		/// <param name="start">Erstes Byte der Nutzdaten.</param>
		/// <param name="length">Anzahl der Nutzdaten.</param>
		private void SendAudio(byte[] buffer, int start, int length)
		{
			// Update PTS if necessary
			ProcessAudioPTS(buffer, start, length);

			// Send all to stream
			m_Audio.AddPayload(buffer, start, length);
		}

		/// <summary>
		/// Korrigiert bei Bedarf den PVA 32-Bit PTS im Audiostrom gemäß
		/// der Zeitbasis der Gesamtdatei.
		/// </summary>
		/// <param name="buffer">Paketdaten.</param>
		/// <param name="start">Erstes Byte der Nutzdaten.</param>
		/// <param name="length">Anzahl der Nutzdaten.</param>
		private void ProcessAudioPTS(byte[] buffer, int start, int length)
		{
			// No PES header
			if (!PTSFlagPresent) return;

			// My private packet position
			int packetPos = m_PacketPos;

			// Time to collect PES header
			if (packetPos < 9)
			{
				// How many can we eat up
				int rest = Math.Min(9 - packetPos, length);

				// Copy over
				Array.Copy(buffer, start, m_AudioPES, packetPos, rest);

				// Adjust
				packetPos += rest;
				length -= rest;
				start += rest;

				// Incomplete
				if (packetPos < 9) return;
			}

			// Extension length
			int extensions = m_AudioPES[8];

			// See if there is a PTS in it
			if ((extensions < 5) || (0x80 != (0x80 & m_AudioPES[7]))) return;

			// Full length
			int fullPES = 9 + extensions;

			// Time to collect PTS
			if (packetPos >= fullPES) return;

			// See if we can update the PES bit - actually we could skip the rest but then an error indicator could not be set
			bool canUpdate = (9 == packetPos);

			// How many can we eat up
			int len = Math.Min(fullPES - packetPos, length);

			// Copy over
			Array.Copy(buffer, start, m_AudioPES, packetPos, len);

			// Adjust - let length and start as is
			packetPos += len;

			// See if it is complete
			if (packetPos < fullPES) return;

			// Read the 32-Bit PVA PTS
			long b4 = (m_AudioPES[9] >> 1) & 0x7;
			long b3 = m_AudioPES[10];
			long b2 = m_AudioPES[11] >> 1;
			long b1 = m_AudioPES[12];
			long b0 = m_AudioPES[13] >> 1;

			// Reconstruct from slices
			long audioPTS = b0 + 128 * (b1 + 256 * (b2 + 128 * (b3 + 256 * b4)));

			// Correct
			long pts = CorrectByGuide(audioPTS);

			// No update necessary
			if (pts == audioPTS) return;

			// See if byte is still available
			if (canUpdate)
			{
				// Update
				buffer[start] ^= 0x08;
			}
			else
			{
				// Count errors
				++m_BadAudioPES;
			}
		}

		/// <summary>
		/// Meldet, ob die Auftrennung des PVA Stroms in Video und Audio erfolgreich
		/// war.
		/// </summary>
		public bool Corrupted
		{
			get
			{
				// Report
				return ((0 != m_BadAudioPES) || (0 != m_Restarted) || (0 != m_Skipped));
			}
		}

		/// <summary>
		/// Passt die 32-Bit PTS Werte eines PVA Stroms an echte 33-Bit PTS Werte innerhalb
		/// einer Gesamtdatei an.
		/// </summary>
		/// <remarks>
		/// Der Gesamtstrom trägt über <see cref="GuidePTS"/> den vollen PTS eines
		/// Datenstroms ein - <i>-1</i>, wenn keine weiteren Datenströme vorhanden
		/// sind. Die Methode übernimmt das 33te Bit in die PTS Werte des PVA Stroms
		/// und prüft dann die Abweichung dieses korrigierten Wertes gegen den
		/// <see cref="GuidePTS"/>. Ist diese größer als ein Schwellwert (es wurde
		/// willkürlich 2 Minuten gewählt), so wird das 33te Bit invertiert. So sollte
		/// für alle praktische Fälle erreicht werden, dass sich die rekonstruierte PTS
		/// homogen in den Gesamtstrom einfügt.
		/// </remarks>
		/// <param name="pts">32-Bit PVA PTS.</param>
		/// <returns>33-Bit PTS passend zum Gesamtstrom.</returns>
		private long CorrectByGuide(long pts)
		{
			// See if guide is active
			if (m_GuidePTS < 0) return pts;

			// Take Bit 33 from the guide line
			pts |= m_GuidePTS & PTSOffset;

			// Check delta
			long delta = Math.Abs(pts - m_GuidePTS);

			// May be the wrong decision - change the 33th bit
			if (delta > 10000000) pts ^= PTSOffset;

			// Report
			return pts;
		}

		#region IDisposable Members

		/// <summary>
		/// Freigabe aller zugeordneten Ressourcen.
		/// </summary>
		public void Dispose()
		{
			// Stop processing at once
			Stop();
		}

		#endregion
	}
}
