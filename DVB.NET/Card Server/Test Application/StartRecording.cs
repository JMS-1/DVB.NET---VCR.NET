using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using JMS.DVB.CardServer;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace CardServerTester
{
    /// <summary>
    /// Über diesen Dialog werden die Quellen für den Empfang ausgewählt.
    /// </summary>
    public partial class StartRecording : Form
    {
        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        public StartRecording()
        {
            // Load designer stuff
            InitializeComponent();
        }

        /// <summary>
        /// Initialisiert die Liste der möglichen Quellen.
        /// </summary>
        /// <param name="allItems">Die Liste aller Quellen.</param>
        /// <param name="primary">Die primär ausgewählte Quelle.</param>
        public void LoadItems( IEnumerable allItems, SourceItem primary )
        {
            // Forward
            selStream.LoadItems( allItems, primary );
        }

        /// <summary>
        /// Ermittelt die ausgewählten Quellen.
        /// </summary>
        /// <param name="directory">Optional ein Verzeichnis für die Aufzeichnungsdateien.</param>
        /// <returns>Alle ausgewählten Quellen.</returns>
        public ReceiveInformation[] GetSources( string directory )
        {
            // Create helper
            List<ReceiveInformation> sources = new List<ReceiveInformation>();

            // Process all
            foreach (Control control in Controls)
            {
                // Change type
                SelectStream selector = control as SelectStream;
                if (null == selector)
                    continue;

                // Get the selection
                SourceItem item = selector.Source;
                if (null == item)
                    continue;

                // Construct the information
                sources.Add(
                    new ReceiveInformation
                        {
                            RecordingPath = item.GetRecordingPath( directory ),
                            SelectionKey = item.Selection.SelectionKey,
                            Streams = selector.Selection
                        } );
            }

            // Report
            return sources.ToArray();
        }

        /// <summary>
        /// Es soll eine weitere Quelle hinzugefügt werden.
        /// </summary>
        /// <param name="sender">Die Informationen zur auslösenden Quelle.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selStream_MoreClicked( object sender, EventArgs e )
        {
            // Change type
            SelectStream selector = (SelectStream) sender;

            // Create a new one
            SelectStream newSelector = new SelectStream();

            // Configure
            newSelector.MoreClicked += selStream_MoreClicked; 
            newSelector.Visible = true;
            
            // Put it in place
            newSelector.Location = new Point( selector.Left, selector.Bottom + Margin.Vertical );
            newSelector.Size = selector.Size;

            // Remember
            Controls.Add( newSelector );

            // Set it up
            newSelector.SourceItems = selector.SourceItems;
        }
    }
}
