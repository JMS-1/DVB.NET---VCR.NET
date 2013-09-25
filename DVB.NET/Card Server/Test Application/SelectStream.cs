using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using JMS.DVB;

namespace CardServerTester
{
    /// <summary>
    /// Dieses Element wird eingesetzt, um eine Aufzeichnung von einer oder mehreren
    /// Quellen zu aktivieren.
    /// </summary>
    [DefaultEvent( "MoreClicked" )]
    public partial class SelectStream : UserControl
    {
        /// <summary>
        /// Wird ausgelöst, wenn weitere Quellen angelegt werden sollen.
        /// </summary>
        public event EventHandler MoreClicked;

        /// <summary>
        /// Erzeugt ein neues Element.
        /// </summary>
        public SelectStream()
        {
            // Load designer stuff
            InitializeComponent();
        }

        /// <summary>
        /// Initialisiert dieses Element.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void SelectStream_Load( object sender, EventArgs e )
        {
            // Get our language
            string language = Thread.CurrentThread.CurrentUICulture.ThreeLetterISOLanguageName.FromISOLanguage();

            // Update items
            selSubtitles.Items[2] = string.Format( (string) selSubtitles.Items[2], language );
            selMP2.Items[2] = string.Format( (string) selMP2.Items[2], language );
            selAC3.Items[2] = string.Format( (string) selAC3.Items[2], language );

            // Select all we have
            selSubtitles.SelectedIndex = 0;
            selMP2.SelectedIndex = 3;
            selAC3.SelectedIndex = 3;
            ckEPG.Checked = true;
            ckTTX.Checked = true;
        }

        /// <summary>
        /// Meldet die Quellen, die nach Abzug der aktuellen Auswahl noch verfügbar sind.
        /// </summary>
        [Browsable( false )]
        public SourceItem[] SourceItems
        {
            get
            {
                // Result list
                List<SourceItem> items = new List<SourceItem>();

                // Fill
                foreach (SourceItem item in selStation.Items)
                    if (item != selStation.SelectedItem)
                        items.Add( item );

                // Report
                return items.ToArray();
            }
            set
            {
                // Reset
                selStation.Items.Clear();

                // Load
                if (null != value)
                    selStation.Items.AddRange( value );

                // Select first
                if (selStation.Items.Count > 0)
                    selStation.SelectedIndex = 0;

                // Set button
                cmdMore.Enabled = (selStation.Items.Count > 1);
            }
        }
        /// <summary>
        /// Initialisiert die Liste der möglichen Quellen.
        /// </summary>
        /// <param name="allItems">Die Liste aller Quellen.</param>
        /// <param name="primary">Die primär ausgewählte Quelle.</param>
        public void LoadItems( IEnumerable allItems, SourceItem primary )
        {
            // Process all
            foreach (SourceItem item in allItems)
                if (Equals( item.Selection.Location, primary.Selection.Location ))
                    if (Equals( item.Selection.Group, primary.Selection.Group ))
                        selStation.Items.Add( item );

            // Choose the primary one
            selStation.SelectedItem = primary;

            // Not changable if loaded this way
            selStation.Enabled = false;

            // Set button
            cmdMore.Enabled = (selStation.Items.Count > 1);
        }

        /// <summary>
        /// Meldet die aktuelle ausgewählte Quelle.
        /// </summary>
        public SourceItem Source
        {
            get
            {
                // Load
                return (SourceItem) selStation.SelectedItem;
            }
        }

        /// <summary>
        /// Meldet die aktuelle Auswahl der Teildatenströme.
        /// </summary>
        public StreamSelection Selection
        {
            get
            {
                // Get our language
                string language = Thread.CurrentThread.CurrentUICulture.ThreeLetterISOLanguageName.FromISOLanguage();

                // Create new
                StreamSelection selection = new StreamSelection();

                // Fill languages
                if (1 == selMP2.SelectedIndex)
                    selection.MP2Tracks.LanguageMode = LanguageModes.Primary;
                else if (2 == selMP2.SelectedIndex)
                    selection.MP2Tracks.Languages.Add( language );
                else if (3 == selMP2.SelectedIndex)
                    selection.MP2Tracks.LanguageMode = LanguageModes.All;
                if (1 == selAC3.SelectedIndex)
                    selection.AC3Tracks.LanguageMode = LanguageModes.Primary;
                else if (2 == selAC3.SelectedIndex)
                    selection.AC3Tracks.Languages.Add( language );
                else if (3 == selAC3.SelectedIndex)
                    selection.AC3Tracks.LanguageMode = LanguageModes.All;
                if (1 == selSubtitles.SelectedIndex)
                    selection.SubTitles.LanguageMode = LanguageModes.Primary;
                else if (2 == selSubtitles.SelectedIndex)
                    selection.SubTitles.Languages.Add( language );
                else if (3 == selSubtitles.SelectedIndex)
                    selection.SubTitles.LanguageMode = LanguageModes.All;

                // Fill flags
                selection.ProgramGuide = ckEPG.Checked;
                selection.Videotext = ckTTX.Checked;

                // Report
                return selection;
            }
        }

        /// <summary>
        /// Es sollen weitere Quellen angegeben werden.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdMore_Click( object sender, EventArgs e )
        {
            // Disable 
            selStation.Enabled = false;
            cmdMore.Enabled = false;

            // Forward
            if (null != MoreClicked)
                MoreClicked( this, EventArgs.Empty );
        }
    }
}
