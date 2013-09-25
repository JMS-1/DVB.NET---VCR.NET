using System;
using JMS.DVB.TS;
using System.Diagnostics;
using System.Windows.Forms;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Verwaltet einen einzelnen Datenstrom einer Quelle.
    /// </summary>
    public class StreamItem : ListViewItem, IStreamConsumer2
    {
        /// <summary>
        /// Das zu verwendende DVB.NET Gerät.
        /// </summary>
        private Hardware m_Hardware;

        /// <summary>
        /// Die eindeutige Kennung der Anmeldung des Datenstroms.
        /// </summary>
        private Guid m_ConsumerId;

        /// <summary>
        /// Die gesamte Anzahl von empfangenen Bytes.
        /// </summary>
        private volatile object m_Total;

        /// <summary>
        /// Die zuletzt abgefragte Datenmenge.
        /// </summary>
        private long m_LastTotal;

        /// <summary>
        /// Ein Zähler für die bisher verstrichene Zeit.
        /// </summary>
        private Stopwatch m_Timer = Stopwatch.StartNew();

        /// <summary>
        /// Eine Hilfsklasse zur Analyse des eingehenden Datenstroms.
        /// </summary>
        private StreamBase m_StreamDecoder;

        /// <summary>
        /// Die Anzahl der bisher empfangenen Pakete.
        /// </summary>
        private volatile uint m_Packets;

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="hardware">Das zu verwendende DVB.NET Gerät.</param>
        /// <param name="pid">Die gewünschte Datenstromkennung.</param>
        /// <param name="type">Die Art des Datenstroms.</param>
        /// <param name="name">Der Name dieses Eintrags.</param>
        public StreamItem( Hardware hardware, ushort pid, StreamTypes type, string name )
            : this( hardware, pid, type, name, null )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="hardware">Das zu verwendende DVB.NET Gerät.</param>
        /// <param name="pid">Die gewünschte Datenstromkennung.</param>
        /// <param name="type">Die Art des Datenstroms.</param>
        /// <param name="name">Der Name dieses Eintrags.</param>
        /// <param name="videoType">Gesetzt, wenn es sich um ein HDTV Bildsignal handelt. Ist der
        /// Datenstrom kein Bildsignal, so wird <i>null</i> verwendet.</param>
        public StreamItem( Hardware hardware, ushort pid, StreamTypes type, string name, bool? videoType )
            : base( string.Format( "{0} ({1})", name, pid ) )
        {
            // Create rate indicator
            SubItems.Add( "?" );
            SubItems.Add( string.Empty );

            // Reset
            m_Total = (long) 0;

            // Remember
            m_Hardware = hardware;

            // Register
            m_ConsumerId = m_Hardware.AddConsumer( pid, type, OnData );

            // Create decoder
            if (videoType.HasValue)
                if (videoType.Value)
                    m_StreamDecoder = new HDTVStream( this, 512, true );
                else
                    m_StreamDecoder = new VideoStream( this, 512, true );

            // Start it
            m_Hardware.SetConsumerState( m_ConsumerId, true );
        }

        /// <summary>
        /// Aktualisiert die Anzeige der Datenrate.
        /// </summary>
        public void Update()
        {
            // Just do it
            SubItems[1].Text = ((long) (BitRate / 1024)).ToString();

            // Check packet counter
            if (m_Packets > 0)
                SubItems[2].Text = m_Packets.ToString();
        }

        /// <summary>
        /// Beendet die Nutzung des Datenstroms.
        /// </summary>
        public void Close()
        {
            // Forward
            m_Hardware.SetConsumerState( m_ConsumerId, null );
        }

        /// <summary>
        /// Meldet die aktuelle Datenrate.
        /// </summary>
        public double BitRate
        {
            get
            {
                // Get the time
                double elapsed = m_Timer.Elapsed.TotalSeconds;

                // Calculate
                double rate = (0 == elapsed) ? 0 : 8 * (((long) m_Total) - m_LastTotal) / elapsed;

                // Reset timer
                m_Timer.Reset();
                m_Timer.Start();

                // Reset counter
                m_LastTotal = (long) m_Total;

                // Report
                return rate;
            }
        }

        /// <summary>
        /// Nimmt Daten aus einem Teildatenstrom entgegen.
        /// </summary>
        /// <param name="buffer">Ein Speicherblock, der die angeforderten Daten enthält.</param>
        /// <param name="start">Das erste genutzte Byte.</param>
        /// <param name="length">Die Anzahl der bereitgestellten Daten.</param>
        private void OnData( byte[] buffer, int start, int length )
        {
            // Remember
            m_Total = ((long) m_Total) + length;

            // Forward
            if (null != m_StreamDecoder)
                m_StreamDecoder.AddPayload( buffer, start, length );
        }

        #region IStreamConsumer2 Members

        /// <summary>
        /// Meldet, ob die eingehenden Daten ignoriert werden sollen.
        /// </summary>
        bool IStreamConsumer2.IgnoreInput
        {
            get
            {
                // Never
                return false;
            }
        }

        #endregion

        #region IStreamConsumer Members

        /// <summary>
        /// Meldet, ob eine Systemuhr für diesen Datenstrom bereit steht.
        /// </summary>
        bool IStreamConsumer.PCRAvailable
        {
            get
            {
                // Always
                return true;
            }
        }

        /// <summary>
        /// Meldet ein analyisiertes Teilpaket.
        /// </summary>
        /// <param name="counter">Der aktuelle Paketzähler.</param>
        /// <param name="pid">Die Datenstromkennung des zu erzeugenen Teilstroms.</param>
        /// <param name="buffer">Ein Speicherbereich mit zu sendenden Daten.</param>
        /// <param name="start">Das erste Byte im Speicher, das zu den Daten gehört.</param>
        /// <param name="packs">Die Anzahl der Pakete.</param>
        /// <param name="isFirst">Gesetzt, wenn ein Paketkopf erkannt wurde.</param>
        /// <param name="sizeOfLast">Die Anzahl der Bytes im letzten Paket.</param>
        /// <param name="pts">Der aktuelle Zeitstempel.</param>
        void IStreamConsumer.Send( ref int counter, int pid, byte[] buffer, int start, int packs, bool isFirst, int sizeOfLast, long pts )
        {
            // Just count
            m_Packets += 1;
        }

        /// <summary>
        /// Dieser Aufruf ist die Aufforderung, die Systemuhr für diesen Datenstrom zu setzen.
        /// </summary>
        /// <param name="counter">Der aktuelle Paketzähler.</param>
        /// <param name="pid">Die verwendete Datenstromkennung.</param>
        /// <param name="pts">Der aktuelle Zeitstempel.</param>
        void IStreamConsumer.SendPCR( int counter, int pid, long pts )
        {
        }

        #endregion
    }
}
