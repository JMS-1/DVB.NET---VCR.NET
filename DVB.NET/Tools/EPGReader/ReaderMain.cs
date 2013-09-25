using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using JMS.DVB;
using JMS.DVB.TS;
using JMS.DVB.EPG;
using JMS.DVB.EPG.Tables;
using JMS.DVB.EPG.Descriptors;


namespace EPGReader
{
    public partial class ReaderMain : Form
    {
        /// <summary>
        /// Installiert die Laufzeitumgebung.
        /// </summary>
        static ReaderMain()
        {
            // Activate dynamic loading
            RunTimeLoader.Startup();
        }

        public int SortIndex = +2;

        private Dictionary<string, bool> m_Entries = new Dictionary<string, bool>();
        private List<EPGEntry> m_ListItems = new List<EPGEntry>();
        private Parser EPGParser = new Parser( null );
        private bool m_Loading = false;
        private FileInfo File = null;
        private string m_StopText;
        private string m_LoadText;

        public ReaderMain( string[] args )
        {
            // Load file
            if (args.Length > 0) File = new FileInfo( args[0] );

            // Setup form
            InitializeComponent();

            // Load
            m_StopText = cmdStop.Text;
            m_LoadText = Properties.Resources.Reload;

            // Connect
            EPGParser.SectionFound += new Parser.SectionFoundHandler( SectionFound );
        }

        [STAThread]
        public static void Main( string[] args )
        {
            // Set user language according to preferrences
            UserProfile.ApplyLanguage();

            // Prepare GUI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new ReaderMain( args ) );
        }

        private void ReaderMain_Load( object sender, EventArgs e )
        {
            // Ask use
            if (null == File)
                if (DialogResult.OK == openInput.ShowDialog( this ))
                    File = new FileInfo( openInput.FileName );

            // Set starter
            starter.Enabled = true;
        }

        private void starter_Tick( object sender, EventArgs e )
        {
            // Disable
            starter.Enabled = false;

            // Finsih
            if (null == File)
            {
                // Stop
                Close();

                // Done
                return;
            }

            // Set mode
            m_Loading = true;

            // May stop
            cmdStop.Enabled = true;

            // Reset GUI
            m_ListItems.Clear();
            m_Entries.Clear();

            // Be safe
            try
            {
                // Choose decoding mode
                Section.ISO6937Encoding = ckStandardSI.Checked;

                // The mode
                bool TSMode = (0 == string.Compare( File.Extension, ".ts", true ));

                // Blocksize
                byte[] Buffer = new byte[TSMode ? 10000000 : 100000];

                // Open the file and create parser
                using (FileStream read = new FileStream( File.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, Buffer.Length ))
                using (TSParser parser = new TSParser())
                {
                    // Skip junk
                    if (TSMode) parser.SetFilter( 0x12, true, EPGParser.OnData );

                    // Content
                    for (int n; (n = read.Read( Buffer, 0, Buffer.Length )) > 0; )
                    {
                        // Report progress
                        progress.Value = (int) (read.Position * progress.Maximum / read.Length);

                        // Show up
                        Application.DoEvents();

                        // Done
                        if (!cmdStop.Enabled) break;

                        // Check mode
                        if (TSMode)
                        {
                            // Feed into parser
                            parser.AddPayload( Buffer, 0, n );
                        }
                        else
                        {
                            // SI Table
                            EPGParser.OnData( Buffer, 0, n );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message );
            }
            finally
            {
                // Done
                m_Loading = false;
            }

            // Prepare load
            cmdStop.Text = m_LoadText;
            cmdStop.Enabled = true;

            // Load all we found
            lstEntries.Items.Clear();
            lstEntries.Items.AddRange( m_ListItems.ToArray() );

            // Prepare sorter
            lstEntries.ListViewItemSorter = new EPGEntry.Comparer();
        }

        private void cmdStop_Click( object sender, EventArgs e )
        {
            // Check mode
            if (!m_Loading)
            {
                // Ask
                if (DialogResult.OK != openInput.ShowDialog( this )) return;

                // Reload
                File = new FileInfo( openInput.FileName );

                // Set starter
                starter.Enabled = true;

                // Set text
                cmdStop.Text = m_StopText;
            }

            // Once
            cmdStop.Enabled = false;
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            // Force stop
            cmdStop.Enabled = false;

            // Forward
            base.OnClosing( e );
        }

        private void SectionFound( Section section )
        {
            // Check
            if ((null == section) || !section.IsValid) return;

            // Test all
            if (ProcessEITEvents( section.Table as EIT )) return;
            if (ProcessCITEvents( section.Table as CITPremiere )) return;
        }

        private bool ProcessCITEvents( CITPremiere table )
        {
            // Check 
            if (null == table) return false;
            if (!table.IsValid) return true;

            // Process all descriptors
            foreach (Descriptor descriptor in table.Descriptors)
            {
                // Load type
                ContentTransmissionPremiere ctp = descriptor as ContentTransmissionPremiere;
                if ((null != ctp) && ctp.IsValid) AddEntry( ctp );
            }

            // Did it
            return true;
        }

        private bool ProcessEITEvents( EIT epgTable )
        {
            // Check 
            if (null == epgTable) return false;
            if (!epgTable.IsValid) return true;

            // Process all events
            foreach (EventEntry entry in epgTable.Entries)
                if (ckAll.Checked || (EventStatus.Running == entry.Status))
                    AddEntry( epgTable.ServiceIdentifier, entry );

            // Did it
            return true;
        }

        private void AddEntry( ushort service, EventEntry entry )
        {
            // Create a key
            string simpleKey = string.Format( "{0}-{1}", service, entry.EventIdentifier );

            // Already collected
            if (m_Entries.ContainsKey( simpleKey )) return;

            // Lock out
            m_Entries[simpleKey] = true;

            // Load all
            string name, description;
            LoadEventData( out name, out description, entry.Descriptors );

            // Remember
            m_ListItems.Add( new EPGEntry( service, name, description, entry.StartTime.ToLocalTime(), entry.Duration ) );
        }

        private void AddEntry( ContentTransmissionPremiere entry )
        {
            // Attach to the table
            CITPremiere table = (CITPremiere) entry.Table;

            // Information
            string name = null, description = null;

            // Load once once
            bool loaded = false;

            // Process all schedules
            foreach (DateTime schedule in entry.StartTimes)
            {
                // Create a key
                string simpleKey = string.Format( "{0}-{1}", entry.ServiceIdentifier, schedule.Ticks );

                // Already collected
                if (m_Entries.ContainsKey( simpleKey )) return;

                // Lock out
                m_Entries[simpleKey] = true;

                // Load once
                if (!loaded)
                {
                    // Lock out
                    loaded = true;

                    // Process
                    LoadEventData( out name, out description, table.Descriptors );
                }

                // Remember
                m_ListItems.Add( new EPGEntry( entry.ServiceIdentifier, name, description, schedule.ToLocalTime(), table.Duration ) );
            }
        }

        private void LoadEventData( out string name, out string description, Descriptor[] descriptors )
        {
            // Descriptors we can have
            ShortEvent shortEvent = null;

            // Extended events
            List<ExtendedEvent> exEvents = new List<ExtendedEvent>();

            // Check all descriptors
            foreach (Descriptor descr in descriptors)
                if (descr.IsValid)
                {
                    // Check type
                    if (null == shortEvent)
                    {
                        // Read
                        shortEvent = descr as ShortEvent;

                        // Done for now
                        if (null != shortEvent) continue;
                    }

                    // Test
                    ExtendedEvent exEvent = descr as ExtendedEvent;

                    // Register
                    if (null != exEvent) exEvents.Add( exEvent );
                }

            // Reset all
            description = null;
            name = null;

            // Take the best we got
            if (exEvents.Count > 0)
            {
                // Text builder
                StringBuilder text = new StringBuilder();

                // Process all
                foreach (ExtendedEvent exEvent in exEvents)
                {
                    // Normal
                    if (null == name) name = exEvent.Name;

                    // Merge
                    if (exEvent.Text != null) text.Append( exEvent.Text );
                }

                // Use
                description = text.ToString();
            }

            // Try short event
            if (null != shortEvent)
            {
                // Read
                if (null == name) name = shortEvent.Name;
                if (null == description) description = shortEvent.Text;

                // Check for additional information
                if (string.IsNullOrEmpty( name ))
                    name = shortEvent.Text;
                else if (!string.IsNullOrEmpty( shortEvent.Text ))
                    name += string.Format( " ({0})", shortEvent.Text );
            }
        }

        private void lstEntries_ColumnClick( object sender, ColumnClickEventArgs e )
        {
            // Get the sort index
            int sortIndex = SortIndex;
            bool ascending = (sortIndex > 0);

            // Correct
            if (!ascending) sortIndex = -sortIndex;

            // New index
            int newSortIndex = e.Column + 1;

            // Check mode
            if (newSortIndex == sortIndex)
            {
                // Turn direction
                SortIndex = -SortIndex;
            }
            else
            {
                // Use as is
                SortIndex = newSortIndex;
            }

            // Resort
            lstEntries.Sort();
        }

        private void lstEntries_DoubleClick( object sender, EventArgs e )
        {
            // Check
            if (1 != lstEntries.SelectedItems.Count) return;

            // Load
            EPGEntry entry = lstEntries.SelectedItems[0] as EPGEntry;

            // None
            if (null == entry) return;

            // Show
            using (EPGDisplay dialog = new EPGDisplay( entry ))
                if (DialogResult.OK == dialog.ShowDialog( this ))
                    Close();
        }

    }
}