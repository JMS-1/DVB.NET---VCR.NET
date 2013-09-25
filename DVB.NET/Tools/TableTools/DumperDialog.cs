extern alias oldVersion;

using System;
using System.IO;
using JMS.DVB.SI;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using legacy = oldVersion.JMS.DVB.EPG;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Dieser Dialog fragt den Anwender, welchen Datenstrom mit SI Tabellen von 
    /// welcher Quelle gespeichert werden soll.
    /// </summary>
    public partial class DumperDialog : UserControl, IPlugInControl
    {
        /// <summary>
        /// Repräsentiert irgendeine Tabellenart.
        /// </summary>
        private class GenericTable : LegacyTable<legacy.Table>
        {
            /// <summary>
            /// Eine Liste aller möglichen Tabellenkennungen.
            /// </summary>
            private static readonly byte[] m_All = Enumerable.Range( 0, 255 ).Select( i => (byte) i ).ToArray();

            /// <summary>
            /// Erzeugt eine neue Tabelleninstanz.
            /// </summary>
            /// <param name="table">Eine beliebige Tabelle.</param>
            public GenericTable( legacy.Table table )
                : base( table )
            {
            }

            /// <summary>
            /// Wird für interne Zwecke verwendet.
            /// </summary>
            public GenericTable()
                : base( null )
            {
            }

            /// <summary>
            /// Meldet die Tabellenkennung, für die diese Art von Tabelle zuständig ist.
            /// </summary>
            public override byte[] TableIdentifiers
            {
                get
                {
                    // Report
                    return m_All;
                }
            }
        }

        /// <summary>
        /// Meldet die zugehörige administrative Erweiterung.
        /// </summary>
        public TableDumper PlugIn { get; private set; }

        /// <summary>
        /// Meldet die zugehörige administrative Umgebung.
        /// </summary>
        public IPlugInUISite AdminSite { get; private set; }

        /// <summary>
        /// Die Auswahl für die Programmzeitschrift der englischen FreeSat Sender.
        /// </summary>
        private SourceItem m_FreeSat;

        /// <summary>
        /// Die Auswahl für die Programmzeitschrift der PREMIERE DIRECT Filmdienste.
        /// </summary>
        private SourceItem m_DirectEPG;

        /// <summary>
        /// Die Auswahl für die Programmzeitschrift von PREMIERE SPORT.
        /// </summary>
        private SourceItem m_SportEPG;

        /// <summary>
        /// Auswahlpunkt für die Programmzeitschrift der englischen FreeSat Sender.
        /// </summary>
        private int m_FreeSatIndex = 1;

        /// <summary>
        /// Auswahlpunkt für die Programmzeitschrift der PREMIERE DIRECT Filmdienste.
        /// </summary>
        private int m_DirectIndex = 2;

        /// <summary>
        /// Auswahlpunkt für die Programmzeitschrift von PREMIERE SPORT.
        /// </summary>
        private int m_SportIndex = 3;

        /// <summary>
        /// Der <see cref="Thread"/>, auf dem die Arbeit erledigt wird.
        /// </summary>
        private volatile Thread m_Worker;

        /// <summary>
        /// Die zu verwendende Datenstromkennung.
        /// </summary>
        private ushort m_Stream;

        /// <summary>
        /// Gesetzt, wenn erweiterte Tabellen verwendet werden sollen.
        /// </summary>
        private bool m_Extended;

        /// <summary>
        /// Verarbeitet SI Tabellen.
        /// </summary>
        private TableParser m_Parser;

        /// <summary>
        /// Die bisher empfangenen Datenbytes.
        /// </summary>
        private volatile object m_TotalBytes = (long) 0;

        /// <summary>
        /// Die bisher empfangenen Tabellen.
        /// </summary>
        private volatile uint m_TotalTables = 0;

        /// <summary>
        /// Die Datei, in der alle Daten abgelegt werden.
        /// </summary>
        private BinaryWriter m_TargetFile;

        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        /// <param name="plugIn">Die zugehörige administrative Erweiterung.</param>
        /// <param name="site">Die aktuelle administrative Arbeitsumgebung.</param>
        public DumperDialog( TableDumper plugIn, IPlugInUISite site )
        {
            // Remember
            PlugIn = plugIn;
            AdminSite = site;

            // Use designer settings
            InitializeComponent();

            // Finish
            lbProfile.Text = string.Format( lbProfile.Text, plugIn.Profile.Name );
        }

        /// <summary>
        /// Wird bei der ersten Anzeige des Dialogs aufgerufen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void DumperDialog_Load( object sender, EventArgs e )
        {
            // Load all sources
            List<SourceItem> sources = SourceItem.CreateSortedListFromProfile( PlugIn.Profile );

            // Load 
            selSource.Items.AddRange( sources.ToArray() );

            // Find the special sources
            SourceSelection freeSat = PlugIn.Profile.FindSource( EIT.FreeSatEPGTriggerSource ).FirstOrDefault();
            SourceSelection premDirect = PlugIn.Profile.FindSource( DirectCIT.TriggerSource ).FirstOrDefault();
            SourceSelection premSport = PlugIn.Profile.FindSource( SportCIT.TriggerSource ).FirstOrDefault();

            // Map to sources
            m_DirectEPG = sources.Find( s => s.Source.CompareTo( premDirect ) );
            m_SportEPG = sources.Find( s => s.Source.CompareTo( premSport ) );
            m_FreeSat = sources.Find( s => s.Source.CompareTo( freeSat ) );

            // Delete as needed
            if (null == m_FreeSat)
            {
                // Get rid
                selType.Items.RemoveAt( m_FreeSatIndex );

                // Correct
                m_FreeSatIndex = -1;
                m_DirectIndex -= 1;
                m_SportIndex -= 1;
            }
            if (null == m_DirectEPG)
            {
                // Get rid
                selType.Items.RemoveAt( m_DirectIndex );

                // Correct
                m_DirectIndex -= 1;
                m_SportIndex -= 1;
            }
            if (null == m_SportEPG)
            {
                // Get rid
                selType.Items.RemoveAt( m_SportIndex );

                // Correct
                m_SportIndex -= 1;
            }

            // Start with default
            selPid.Value = WellKnownTable.GetWellKnownStream<EIT>();

            // Choose
            selSource.SelectedItem = sources.Find( s => s.Source.CompareTo( PlugIn.LastSource ) );
            selType.SelectedIndex = 0;

            // Specials
            if (null == selSource.SelectedItem)
                PlugIn.LastSource = null;
            else if (selSource.SelectedItem == m_FreeSat)
                selType.SelectedIndex = m_FreeSatIndex;
            else if (selSource.SelectedItem == m_DirectEPG)
                selType.SelectedIndex = m_DirectIndex;
            else if (selSource.SelectedItem == m_SportEPG)
                selType.SelectedIndex = m_SportIndex;

            // Finish
            selType_SelectionChangeCommitted( selType, EventArgs.Empty );
        }

        /// <summary>
        /// Der Anwender hat die Art der Quellauswahl verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selType_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Check selection
            if (selType.SelectedIndex > 0)
                if (selType.SelectedIndex == m_FreeSatIndex)
                    SelectSpecial( m_FreeSat, EIT.FreeSatEPGPID, false );
                else if (selType.SelectedIndex == m_DirectIndex)
                    SelectSpecial( m_DirectEPG, WellKnownTable.GetWellKnownStream<DirectCIT>(), true );
                else if (selType.SelectedIndex == m_SportIndex)
                    SelectSpecial( m_SportEPG, WellKnownTable.GetWellKnownStream<SportCIT>(), true );

            // Always reset
            selType.SelectedIndex = 0;
        }

        /// <summary>
        /// Bereitet die Auswahl für eine bekannte Konfiguration vor.
        /// </summary>
        /// <param name="item">Die zugehörige Quelle.</param>
        /// <param name="pid">Die Datenstromkennung für den Datenstrom.</param>
        /// <param name="extended">Gesetzt, wenn erweiterte Tabellenkennungen verwendet werden.</param>
        private void SelectSpecial( SourceItem item, ushort pid, bool extended )
        {
            // Remember
            PlugIn.LastSource = item.Source;

            // Select all
            selSource.SelectedItem = item;
            selPIDType.SelectedIndex = 0;
            selPid.Value = pid;

            // Set option field
            if (extended)
                optExtended.Checked = true;
            else
                optStandard.Checked = true;

            // Refresh
            AdminSite.UpdateGUI();
        }

        /// <summary>
        /// Wählt einen besonderen Datenstrom aus.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selPIDType_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Check selectiojn
            switch (selPIDType.SelectedIndex)
            {
                case 1: selPid.Value = WellKnownTable.GetWellKnownStream<PAT>(); optStandard.Checked = true; break;
                case 2: selPid.Value = WellKnownTable.GetWellKnownStream<NIT>(); optStandard.Checked = true; break;
                case 3: selPid.Value = WellKnownTable.GetWellKnownStream<SDT>(); optStandard.Checked = true; break;
                case 4: selPid.Value = WellKnownTable.GetWellKnownStream<EIT>(); optStandard.Checked = true; break;
            }
        }

        /// <summary>
        /// Die Auswahl der Datenstromkennung wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selPid_ValueChanged( object sender, EventArgs e )
        {
            // Check it
            if (selPid.Value == WellKnownTable.GetWellKnownStream<PAT>())
                selPIDType.SelectedIndex = 1;
            else if (selPid.Value == WellKnownTable.GetWellKnownStream<NIT>())
                selPIDType.SelectedIndex = 2;
            else if (selPid.Value == WellKnownTable.GetWellKnownStream<SDT>())
                selPIDType.SelectedIndex = 3;
            else if (selPid.Value == WellKnownTable.GetWellKnownStream<EIT>())
                selPIDType.SelectedIndex = 4;
            else
                selPIDType.SelectedIndex = 0;
        }

        /// <summary>
        /// Der Anwender hat die Auswahl der Datenquelle verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selSource_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Attach to selection
            SourceItem item = (SourceItem) selSource.SelectedItem;

            // None
            if (null == item)
                return;

            // Reset
            selPid.Value = WellKnownTable.GetWellKnownStream<EIT>();
            optStandard.Checked = true;

            // Remember
            PlugIn.LastSource = item.Source;

            // Refresh
            AdminSite.UpdateGUI();
        }

        /// <summary>
        /// Nimmt Rohdaten entgegen.
        /// </summary>
        /// <param name="data">Ein Speicherbereich mit Daten.</param>
        /// <param name="start">Der Index des ersten Nutzbytes.</param>
        /// <param name="length">Die Anzahl der Nutzbytes.</param>
        private void OnData( byte[] data, int start, int length )
        {
            // Count
            m_TotalBytes = ((long) m_TotalBytes) + length;

            // Send to file
            m_TargetFile.Write( data, start, length );

            // Parse
            m_Parser.AddPayload( data, start, length );
        }

        /// <summary>
        /// Wird aufgerufen, sobald eine Tabelle erkannt wurde.
        /// </summary>
        /// <param name="table">Die erkannte Tabelle.</param>
        private void OnTable( GenericTable table )
        {
            // Just count
            m_TotalTables += 1;
        }

        /// <summary>
        /// Führt den eigentliche Zugriff aus.
        /// </summary>
        private void Worker()
        {
            // Be fully safe
            try
            {
                // With hardware
                using (HardwareManager.Open())
                {
                    // Attach to group
                    PlugIn.LastSource.SelectGroup();

                    // Attach to device
                    Hardware device = PlugIn.LastSource.GetHardware();

                    // Create parser
                    m_Parser = TableParser.Create<GenericTable>( OnTable );

                    // Register raw stream
                    Guid consumerId = device.AddConsumer( m_Stream, m_Extended ? StreamTypes.ExtendedTable : StreamTypes.StandardTable, OnData );

                    // Start it
                    device.SetConsumerState( consumerId, true );

                    // Process
                    while (null != m_Worker)
                        Thread.Sleep( 100 );

                    // Stop all
                    device.SetConsumerState( consumerId, null );
                }
            }
            catch
            {
            }

            // Close the file
            using (BinaryWriter writer = m_TargetFile)
                m_TargetFile = null;

            // Did it
            Invoke( new Action( AdminSite.OperationDone ) );
        }

        /// <summary>
        /// Prüft, ob der Anwender die Bearbeitung abgebrochen hat.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void checkEnd_Tick( object sender, EventArgs e )
        {
            // Report
            lbStatus.Text = string.Format( Properties.Resources.StatusFormat, m_TotalTables, m_TotalBytes );

            // Stop processing
            if (AdminSite.HasBeenCancelled)
                m_Worker = null;
        }

        #region IPlugInControl Members

        /// <summary>
        /// Meldet, ob diese Aufgabe durch den Anwender beendet werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // Yeah, this is the core of it
                return true;
            }
        }

        /// <summary>
        /// Prüft, ob eine Ausführung möglich ist.
        /// </summary>
        /// <returns>Gesetzt, wenn <see cref="IPlugInControl.Start"/> aufgerufen werden darf.</returns>
        bool IPlugInControl.TestStart()
        {
            // Yes, we can
            return true;
        }

        /// <summary>
        /// Start die Speicherung der SI Tabellen.
        /// </summary>
        /// <returns>Gesetzt, wenn die Ausführung synchron durchgeführt wurde.</returns>
        bool IPlugInControl.Start()
        {
            // Ask user
            if (DialogResult.OK != dlgFile.ShowDialog( this ))
                return true;

            // Open the file
            m_TargetFile = new BinaryWriter( new FileStream( dlgFile.FileName, FileMode.Create, FileAccess.Write, FileShare.Read ) );

            // Copy over
            m_Extended = optExtended.Checked;
            m_Stream = (ushort) selPid.Value;

            // Create worker
            m_Worker = new Thread( Worker );

            // Configure it
            m_Worker.SetApartmentState( ApartmentState.STA );

            // Start it
            m_Worker.Start();

            // Start watchdog
            checkEnd.Enabled = true;

            // Disable GUI
            Enabled = false;

            // Report
            return false;
        }

        #endregion
    }
}
