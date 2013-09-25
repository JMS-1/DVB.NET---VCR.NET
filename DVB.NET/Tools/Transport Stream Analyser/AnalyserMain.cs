using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Transport_Stream_Analyser
{
    /// <summary>
    /// Das Hauptfenster der Anwendung.
    /// </summary>
    public partial class AnalyserMain : Form
    {
        /// <summary>
        /// Erstellt ein neues Hauptfenster.
        /// </summary>
        public AnalyserMain()
        {
            // Load designer stuff
            InitializeComponent();

            // Load settings
            selFolder.SelectedPath = Properties.Settings.Default.LastDirectory;
            xmlRestore.FileName = Properties.Settings.Default.LastSave;
            xmlCreate.FileName = Properties.Settings.Default.LastSave;
            selFile.FileName = Properties.Settings.Default.LastFile;
        }

        /// <summary>
        /// Wird bei der ersten Anzeige des Fensters aufgerufen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void AnalyserMain_Load( object sender, EventArgs e )
        {
            // Beautify lists
            SetColumnWidths( lstFiles );
            SetColumnWidths( lstStreans );
        }

        /// <summary>
        /// Optimiert die Spaltenbreite einer Tabelle.
        /// </summary>
        /// <param name="list">Die zu betrachtende Tabelle.</param>
        private void SetColumnWidths( ListView list )
        {
            // Add dummy column
            list.Columns.Add( "-" );

            // Process
            foreach (ColumnHeader column in list.Columns)
            {
                // First try
                column.AutoResize( ColumnHeaderAutoResizeStyle.ColumnContent );

                // Remember
                int content = column.Width;

                // Second try
                column.AutoResize( ColumnHeaderAutoResizeStyle.HeaderSize );

                // Merge
                column.Width = Math.Max( column.Width, content );
            }

            // Remove dummy column
            list.Columns.RemoveAt( list.Columns.Count - 1 );
        }

        /// <summary>
        /// Analyisiert eine einzelne Datei.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignnoriert.</param>
        private void cmdFile_Click( object sender, EventArgs e )
        {
            // Ask user
            if (selFile.ShowDialog( this ) != DialogResult.OK)
                return;

            // Remember
            Properties.Settings.Default.LastFile = selFile.FileName;
            Properties.Settings.Default.Save();

            // Process
            Analyse( selFile.FileName );

            // Choose last
            lstFiles.Items[lstFiles.Items.Count - 1].Selected = true;
        }

        /// <summary>
        /// Bearbeitet eine einzelne Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        private void Analyse( string path )
        {
            // Create the item
            var file = new FileItem( path );

            // Add it
            lstFiles.Items.Add( file );

            // Show it
            file.EnsureVisible();

            // Run analysis
            file.Analyse();

            // Beautify
            SetColumnWidths( lstFiles );
        }

        /// <summary>
        /// Der Anwender möchte sich den Inhalt einer Datei anzeigen lassen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void lstFiles_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Fast mode
            lstStreans.BeginUpdate();

            // Reset
            lstStreans.Items.Clear();

            // Load it
            var file = lstFiles.SelectedItems.Cast<FileItem>().FirstOrDefault();
            if (file != null)
                lstStreans.Items.AddRange( file.Streams.ToArray() );

            // Beautify
            SetColumnWidths( lstStreans );

            // Finish
            lstStreans.EndUpdate();
        }

        /// <summary>
        /// Der Anwender möchte ein ganzes Verzeichnis bearbeiten.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdDir_Click( object sender, EventArgs e )
        {
            // Ask user
            if (selFolder.ShowDialog( this ) != DialogResult.OK)
                return;

            // Remember
            Properties.Settings.Default.LastDirectory = selFolder.SelectedPath;
            Properties.Settings.Default.Save();

            // Process
            foreach (var file in Directory.GetFiles( selFolder.SelectedPath, "*.ts" ))
                Analyse( file );
        }

        /// <summary>
        /// Erstellt eine XML Beschreibung für einen Eintrag.
        /// </summary>
        /// <param name="item">Eine Zeile in der Liste.</param>
        /// <param name="parent">Der Vaterknoten für die neue Beschreibung.</param>
        /// <returns>Die neu erzeugte Beschreibung.</returns>
        public static XmlElement SaveToXml( ListViewItem item, XmlNode parent )
        {
            // Create entry
            var node = parent.AppendChild( parent.OwnerDocument.CreateElement( item.GetType().Name ) );

            // Process all names
            foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
            {
                // Create node
                var child = node.AppendChild( parent.OwnerDocument.CreateElement( "Item" ) );

                // Configure
                child.InnerText = subitem.Text;
            }

            // Report
            return (XmlElement) node;
        }

        /// <summary>
        /// Rekonstruiert einen Eintrag aus einer XML Beschreibung.
        /// </summary>
        /// <typeparam name="T">Die Art des Eintrags.</typeparam>
        /// <param name="node">Die zu verwendende Beschreibung.</param>
        /// <returns>Das neue Element.</returns>
        public static T LoadFromXml<T>( XmlElement node ) where T : ListViewItem
        {
            // Get the element name
            var fullName = string.Format( "{0}.{1}", typeof( FileItem ).Namespace, node.Name );

            // Create new instance
            T item = (T) Activator.CreateInstance( Type.GetType( fullName, true ) );

            // Load settings
            var children = node.SelectNodes( "Item" );

            // Create
            for (int i = children.Count - 1; i-- > 0; )
                item.SubItems.Add( string.Empty );

            // Fill
            for (int i = children.Count; i-- > 0; )
                item.SubItems[i].Text = children[i].InnerText;

            // Report
            return item;
        }

        /// <summary>
        /// Der Anwender möchste die aktuelle Analyse in einer Datei speichern.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdSave_Click( object sender, EventArgs e )
        {
            // Ask user
            if (xmlCreate.ShowDialog( this ) != DialogResult.OK)
                return;

            // Remember
            Properties.Settings.Default.LastSave = xmlCreate.FileName;
            Properties.Settings.Default.Save();

            // Forward
            xmlRestore.FileName = xmlCreate.FileName;

            // Process
            try
            {
                // Create target
                var xml = new XmlDocument();

                // Create root
                var root = xml.AppendChild( xml.CreateElement( "Analysis" ) );

                // Store all
                foreach (FileItem item in lstFiles.Items)
                    item.SaveToXml( root );

                // Store
                xml.Save( xmlCreate.FileName );
            }
            catch (Exception ex)
            {
                // Laze report
                MessageBox.Show( this, ex.Message, Text );
            }
        }

        /// <summary>
        /// Der Anwender möchte die Darstellung rekostruieren.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdLoad_Click( object sender, EventArgs e )
        {
            // Ask user
            if (xmlRestore.ShowDialog( this ) != DialogResult.OK)
                return;

            // Remember
            Properties.Settings.Default.LastSave = xmlRestore.FileName;
            Properties.Settings.Default.Save();

            // Over
            xmlCreate.FileName = xmlRestore.FileName;

            // Be safe
            try
            {
                // Helper
                var xml = new XmlDocument();

                // Load
                xml.Load( xmlRestore.FileName );

                // Helper
                var items = new List<FileItem>();

                // Fill
                foreach (XmlElement node in xml.DocumentElement.SelectNodes( typeof( FileItem ).Name ))
                    items.Add( FileItem.LoadFromXml( node ) );

                // Reset
                lstFiles.Items.Clear();
                lstStreans.Items.Clear();

                // Do it
                lstFiles.Items.AddRange( items.ToArray() );

                // Update all lists
                SetColumnWidths( lstStreans );
                SetColumnWidths( lstFiles );
            }
            catch (Exception ex)
            {
                // Laze report
                MessageBox.Show( this, ex.Message, Text );
            }
        }
    }
}
