using System;

namespace JMS.DVB.TS
{
    /// <summary>
    /// Repräsentiert einen Rohdatenstrom mit echten DVB Untertiteln.
    /// </summary>
	public class SubtitleStream: StreamBase
	{
        /// <summary>
        /// Erzeugt eine neue Repräsentation.
        /// </summary>
        /// <param name="consumer">Der zugehörige Gesamtdatenstrom, im Allgemeinen ein
        /// <i>Transport Stream</i> <see cref="Manager"/>.</param>
        /// <param name="pid">Die Datenstromkennung im Gesamtstrom.</param>
        /// <param name="isPCR">Gesetzt, wenn dieser Datenstrom die Zeitbasis für den Gesamtstrom
        /// bereitstellt (sehr unüblich für Untertitel).</param>
        public SubtitleStream(IStreamConsumer consumer, short pid, bool isPCR)
			: base(consumer, pid, isPCR)
		{
		}

        /// <summary>
        /// Prüft, ob ein Zeichen ein legaler MPEG-2 Startcode für DVB Untertitelströme
        /// ist.
        /// </summary>
        /// <param name="code">Der zu prüfende Code.</param>
        /// <returns>Gesetzt, wenn es sich um einen legalen Startcode handelt.</returns>
		protected override bool IsValidStartCode(byte code)
		{
			// Must be private data
			return (0xbd == code);
		}

        /// <summary>
        /// Nimmt Daten entgegen aber verzögert die Übernahme in den Gesamtdatenstrom bis
        /// ein PCR vorliegt.
        /// </summary>
        /// <param name="buffer">Speicher mit den Nutzdaten.</param>
        /// <param name="start">Das erste zu verwendende Byte in den Nutzdaten.</param>
        /// <param name="length">Die Anzahl der Bytes in den Nutzdaten.</param>
        public override void AddPayload(byte[] buffer, int start, int length)
        {
            // Forward to base if PCR is sent to the stream
            if (Consumer.PCRAvailable) base.AddPayload(buffer, start, length);
        }
    }
}
