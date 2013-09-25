using System;
using System.Xml;
using JMS.DVB.SI;
using JMS.DVB.TS;
using System.Windows.Forms;
using JMS.DVB.TS.TSBuilders;
using System.Collections.Generic;

namespace Transport_Stream_Analyser
{
    /// <summary>
    /// Beschreibt einen einzelnen Datenstrom.
    /// </summary>
    public class StreamItem : ListViewItem
    {
        /// <summary>
        /// Übernimmt voranalysierte Daten in Rohform.
        /// </summary>
        private class _Builder : TSBuilder
        {
            /// <summary>
            /// Die zugehörige Beschreibung.
            /// </summary>
            private StreamItem m_Item;

            /// <summary>
            /// Erzeugt einen neuen Vermittler.
            /// </summary>
            /// <param name="analyser">Die eigentliche Analyseeinheit.</param>
            /// <param name="item">Die zugehörige Beschreibung.</param>
            public _Builder( TSParser analyser, StreamItem item )
                : base( analyser, null )
            {
                // Remember
                m_Item = item;
            }

            /// <summary>
            /// Bearbeitet ein TS Paket.
            /// </summary>
            /// <param name="packet">Das gesamte Paket.</param>
            /// <param name="offset">Das erste nutzbare Byte im Paket.</param>
            /// <param name="length">Die Anzahl der nutzbaren Bytes.</param>
            /// <param name="noincrement">Gesetzt, wenn der Paketzähler nicht verändert werden darf.</param>
            /// <param name="first">Gesetzt, wenn es sich um das erste Paket handelt.</param>
            /// <param name="counter">Der aktuelle Paketzähler.</param>
            public override void AddPacket( byte[] packet, int offset, int length, bool noincrement, bool first, byte counter )
            {
                // Forward
                m_Item.AddPacket( packet, offset, length, noincrement, first, counter );
            }

            /// <summary>
            /// Beginnt eine neue Analyse.
            /// </summary>
            public override void Reset()
            {
                // Forward
                m_Item.Reset();
            }
        }

        /// <summary>
        /// Die zugehörige Datenstromkennung.
        /// </summary>
        public ushort PID { get; private set; }

        /// <summary>
        /// Die Anzahl der Neustarts.
        /// </summary>
        private int m_Reset;

        /// <summary>
        /// Die gesamte Anzahl der Bytes.
        /// </summary>
        private long m_Bytes;

        /// <summary>
        /// Die Anzahl der vollständigen Pakete.
        /// </summary>
        private int m_Starts;

        /// <summary>
        /// Die Anzahl elementarer Pakete.
        /// </summary>
        private int m_Packets;

        /// <summary>
        /// Die Anzahl von SI Tabellen.
        /// </summary>
        private int m_Tables;

        /// <summary>
        /// Die gemeldeten Transportstromkennungen.
        /// </summary>
        public readonly HashSet<ushort> TransportIdentifiers = new HashSet<ushort>();

        /// <summary>
        /// Wird während der Deserialisierung verwendet.
        /// </summary>
        public StreamItem()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="pid">Die zugehörige Datenstromkennung.</param>
        /// <param name="packets">Die Anzahl der Pakete in diesem Datenstrom.</param>
        public StreamItem( ushort pid, long packets )
            : base( string.Format( "{0} (0x{0:x})", pid ) )
        {
            // Remember
            PID = pid;

            // Add helper columns
            for (int i = 7; i-- > 0; )
                SubItems.Add( string.Empty );

            // Remember
            SubItems[1].Text = packets.ToString( "N0" );
        }

        /// <summary>
        /// Überträgt dieses Listenelement in eine XML Form.
        /// </summary>
        /// <param name="parent">Das übergeordnete Listenelement.</param>
        /// <returns>Das neu erzeugte Element.</returns>
        public XmlElement SaveToXml( XmlNode parent )
        {
            // Self
            var self = AnalyserMain.SaveToXml( this, parent );

            // Settings
            self.SetAttribute( "pid", PID.ToString() );

            // Report
            return self;
        }

        /// <summary>
        /// Rekonstruiert diesen Eintrag aus der XML Form.
        /// </summary>
        /// <param name="node">Die XML Form.</param>
        /// <returns>Der rekonstruierte Eintrag.</returns>
        public static StreamItem LoadFromXml( XmlElement node )
        {
            // Process
            var self = AnalyserMain.LoadFromXml<StreamItem>( node );

            // Load id
            self.PID = ushort.Parse( node.GetAttribute( "pid" ) );

            // Report
            return self;
        }

        /// <summary>
        /// Meldet sich als Verbraucher an eine Analyse an.
        /// </summary>
        /// <param name="analyser">Die Analyseeinheit.</param>
        public void RegisterPhase1( TSParser analyser )
        {
            // Add as a custom builder
            analyser.RegisterCustomFilter( PID, new _Builder( analyser, this ) );
        }

        /// <summary>
        /// Zählt SI Tabellen.
        /// </summary>
        /// <param name="table">Eine Tabelle.</param>
        private void CountTable( Table table )
        {
            // Count
            m_Tables++;

            // Check mode
            var pat = table as PAT;
            if (pat != null)
                TransportIdentifiers.Add( pat.TransportStream );
        }

        /// <summary>
        /// Meldet sich als Verbraucher an eine Analyse an.
        /// </summary>
        /// <param name="analyser">Die Analyseeinheit.</param>
        public void RegisterPhase2( TSParser analyser )
        {
            // See if there is a well known table 
            foreach (var tableType in typeof( WellKnownTable ).Assembly.GetExportedTypes())
                if (typeof( WellKnownTable ).IsAssignableFrom( tableType ))
                    if (!tableType.IsAbstract)
                        if (WellKnownTable.GetWellKnownStream( tableType ) == PID)
                        {
                            // Remember
                            SubItems[6].Text = tableType.Name;

                            // Create consumer
                            var parser = TableParser.Create( CountTable, tableType );

                            // Connect
                            analyser.SetFilter( PID, true, parser.AddPayload );

                            // Done
                            break;
                        }
        }

        /// <summary>
        /// Aktualisiert die Anzeige.
        /// </summary>
        public void RefreshUI()
        {
            // Fill
            SubItems[2].Text = m_Bytes.ToString( "N0" );
            SubItems[3].Text = m_Starts.ToString( "N0" );
            SubItems[4].Text = m_Packets.ToString( "N0" );
            SubItems[5].Text = m_Reset.ToString();

            // Conditional
            if (!string.IsNullOrEmpty( SubItems[6].Text ))
                SubItems[7].Text = m_Tables.ToString();
        }

        /// <summary>
        /// Bearbeitet ein TS Paket.
        /// </summary>
        /// <param name="packet">Das gesamte Paket.</param>
        /// <param name="offset">Das erste nutzbare Byte im Paket.</param>
        /// <param name="length">Die Anzahl der nutzbaren Bytes.</param>
        /// <param name="noincrement">Gesetzt, wenn der Paketzähler nicht verändert werden darf.</param>
        /// <param name="first">Gesetzt, wenn es sich um das erste Paket handelt.</param>
        /// <param name="counter">Der aktuelle Paketzähler.</param>
        private void AddPacket( byte[] packet, int offset, int length, bool noincrement, bool first, byte counter )
        {
            // Count
            m_Packets++;

            // Increment
            m_Bytes += length;

            // Conditional count
            if (first)
                m_Starts++;
        }

        /// <summary>
        /// Beginnt eine neue Analyse.
        /// </summary>
        private void Reset()
        {
            // Count
            m_Reset++;
        }
    }
}
