using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace EasyCut
{
    /// <summary>
    /// Das Hauptfenster der Schnittanwendung.
    /// </summary>
    public class CutMain : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button cmdRun;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox selRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown udMinimum;
        private Label label4;
        private NumericUpDown udCorrect;
        private CheckBox ckDVB;
        private OpenFileDialog openCuttermaran;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private SaveFileDialog saveCut;
        private ComboBox selPage;

        /// <summary>
        /// Die aktuelle Projektdatei.
        /// </summary>
        private CPFReader m_ProjectFile;

        /// <summary>
        /// Create the cutter main window.
        /// </summary>
        public CutMain()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( CutMain ) );
            this.cmdRun = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.selRate = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.udMinimum = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.udCorrect = new System.Windows.Forms.NumericUpDown();
            this.ckDVB = new System.Windows.Forms.CheckBox();
            this.openCuttermaran = new System.Windows.Forms.OpenFileDialog();
            this.saveCut = new System.Windows.Forms.SaveFileDialog();
            this.selPage = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize) (this.udMinimum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.udCorrect)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdRun
            // 
            resources.ApplyResources( this.cmdRun, "cmdRun" );
            this.cmdRun.Name = "cmdRun";
            this.cmdRun.Click += new System.EventHandler( this.cmdRun_Click );
            // 
            // label1
            // 
            resources.ApplyResources( this.label1, "label1" );
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources( this.label2, "label2" );
            this.label2.Name = "label2";
            // 
            // selRate
            // 
            this.selRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources( this.selRate, "selRate" );
            this.selRate.Name = "selRate";
            // 
            // label3
            // 
            resources.ApplyResources( this.label3, "label3" );
            this.label3.Name = "label3";
            // 
            // udMinimum
            // 
            this.udMinimum.DecimalPlaces = 2;
            this.udMinimum.Increment = new decimal( new int[] {
            1,
            0,
            0,
            65536} );
            resources.ApplyResources( this.udMinimum, "udMinimum" );
            this.udMinimum.Maximum = new decimal( new int[] {
            5,
            0,
            0,
            0} );
            this.udMinimum.Name = "udMinimum";
            // 
            // label4
            // 
            resources.ApplyResources( this.label4, "label4" );
            this.label4.Name = "label4";
            // 
            // udCorrect
            // 
            this.udCorrect.Increment = new decimal( new int[] {
            15,
            0,
            0,
            0} );
            resources.ApplyResources( this.udCorrect, "udCorrect" );
            this.udCorrect.Maximum = new decimal( new int[] {
            10000,
            0,
            0,
            0} );
            this.udCorrect.Minimum = new decimal( new int[] {
            10000,
            0,
            0,
            -2147483648} );
            this.udCorrect.Name = "udCorrect";
            // 
            // ckDVB
            // 
            resources.ApplyResources( this.ckDVB, "ckDVB" );
            this.ckDVB.Name = "ckDVB";
            this.ckDVB.UseVisualStyleBackColor = true;
            this.ckDVB.CheckedChanged += new System.EventHandler( this.ckDVB_CheckedChanged );
            // 
            // openCuttermaran
            // 
            resources.ApplyResources( this.openCuttermaran, "openCuttermaran" );
            // 
            // saveCut
            // 
            resources.ApplyResources( this.saveCut, "saveCut" );
            // 
            // selPage
            // 
            this.selPage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selPage.FormattingEnabled = true;
            resources.ApplyResources( this.selPage, "selPage" );
            this.selPage.Name = "selPage";
            this.selPage.Sorted = true;
            // 
            // CutMain
            // 
            resources.ApplyResources( this, "$this" );
            this.Controls.Add( this.selPage );
            this.Controls.Add( this.ckDVB );
            this.Controls.Add( this.udCorrect );
            this.Controls.Add( this.udMinimum );
            this.Controls.Add( this.label4 );
            this.Controls.Add( this.label3 );
            this.Controls.Add( this.selRate );
            this.Controls.Add( this.label2 );
            this.Controls.Add( this.label1 );
            this.Controls.Add( this.cmdRun );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "CutMain";
            this.Load += new System.EventHandler( this.CutMain_Load );
            ((System.ComponentModel.ISupportInitialize) (this.udMinimum)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.udCorrect)).EndInit();
            this.ResumeLayout( false );
            this.PerformLayout();

        }
        #endregion

        [STAThread]
        public static void Main( string[] args )
        {
            // Check settings
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!version.Equals( Properties.Settings.Default.Version ))
            {
                // Upgrade
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Version = version;
                Properties.Settings.Default.Save();
            }

            // Check for language
            if (args.Length > 0)
                try
                {
                    // Set language
                    System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture( args[0] );
                }
                catch
                {
                }

            // Prepare
            Application.EnableVisualStyles();

            // Bug work-around (see http://www.codeproject.com/buglist/EnableVisualStylesBug.asp)
            Application.DoEvents();

            // Execute
            Application.Run( new CutMain() );
        }

        /// <summary>
        /// Der Anwender wünscht den Schnitt auszuführen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdRun_Click( object sender, System.EventArgs e )
        {
            // Update settings
            Properties.Settings.Default.Framerate = FrameRateInfo.FindFrameRate( selRate.SelectedIndex ).Rate;
            Properties.Settings.Default.Threshold = udMinimum.Value;
            Properties.Settings.Default.Save();

            // Check selection of page
            if (selPage.SelectedItem != null)
                Properties.Settings.Default.SubtitlePage = (int) selPage.SelectedItem;

            // With error handling
            try
            {
                // Load file
                FileInfo outFile = new FileInfo( saveCut.FileName );

                // Get the type
                string suffix = outFile.Extension;

                // Check
                if ((null == suffix) || (suffix.Length < 2))
                    throw new ApplicationException( Properties.Resources.Error_Format );

                // Cut off
                suffix = suffix.Substring( 1 );

                // Load type name
                string typeName = ConfigurationManager.AppSettings[suffix.ToUpper()];

                // Check
                if ((null == typeName) || (typeName.Length < 1))
                    throw new ApplicationException( Properties.Resources.Error_Format );

                // Find the type
                Type cutType = Type.GetType( typeName, true );

                // Create extractor
                using (ICutter cutter = (ICutter) Activator.CreateInstance( cutType ))
                {
                    // Configure
                    cutter.Framerate = FrameRateInfo.FindFrameRate( selRate.SelectedIndex ).Rate;
                    cutter.MinDuration = (double) udMinimum.Value;

                    // Check for VCR.NET 3.1 or later
                    ICutter2 cutter31 = cutter as ICutter2;
                    if (null != cutter31) cutter31.TimeCorrection = (double) udCorrect.Value;

                    // Position in output file
                    long pos = 0;

                    // Process all cuts
                    foreach (CutElement cut in m_ProjectFile.CutElements)
                    {
                        // Get the core name
                        string coreName = Path.GetFileNameWithoutExtension( cut.VideoFile.FullName );

                        // Append page number
                        if (!ckDVB.Checked)
                            coreName += "[" + Properties.Settings.Default.SubtitlePage.ToString() + "]";

                        // Replace suffix
                        FileInfo ttxFile = new FileInfo( cut.VideoFile.DirectoryName + @"\" + coreName + "." + suffix );

                        // Must exist
                        if (!ttxFile.Exists)
                            continue;

                        // Validate
                        if (cut.Start > cut.End)
                            continue;

                        // Process
                        cutter.Cut( ttxFile.FullName, cut.Start, cut.End, pos );

                        // Adjust frame counter
                        pos += cut.End - cut.Start + 1;
                    }

                    // Finish
                    cutter.Save( saveCut.FileName );
                }

                // Done on success
                Close();
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Properties.Resources.Error_Failed );
            }
        }

        /// <summary>
        /// Füllt das Hauptfenster mit Voreinstellungen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void CutMain_Load( object sender, System.EventArgs e )
        {
            // Ask for project file
            if (DialogResult.OK != openCuttermaran.ShowDialog( this ))
            {
                // Finish
                Close();

                // Done
                return;
            }

            // Ask for result file
            if (DialogResult.OK != saveCut.ShowDialog( this ))
            {
                // Finish
                Close();

                // Done
                return;
            }

            // Create the project reader
            m_ProjectFile = new CPFReader( openCuttermaran.FileName );

            // All frame rates
            for (int i = 0; i < 9; i++)
            {
                // Load
                FrameRateInfo info = FrameRateInfo.FindFrameRate( i );

                // Report to list
                selRate.Items.Add( info );

                // Select
                if (info.Rate == Properties.Settings.Default.Framerate)
                    selRate.SelectedIndex = i;
            }

            // Load other defaults
            udMinimum.Value = Properties.Settings.Default.Threshold;

            // Prepare UI
            ckDVB.Enabled = false;

            // Load pages
            foreach (int? page in m_ProjectFile.GetAvailableSubtitles( Path.GetExtension( saveCut.FileName ).Substring( 1 ) ))
                if (page.HasValue)
                {
                    // Add to list
                    selPage.Items.Add( page.Value );

                    // Select it
                    if (page.Value == Properties.Settings.Default.SubtitlePage)
                        selPage.SelectedItem = page.Value;
                }
                else
                    ckDVB.Enabled = true;

            // Finish selection
            if (selPage.Items.Count > 0)
                if (selPage.SelectedItem == null)
                    selPage.SelectedIndex = 0;

            // Disable selection
            selPage.Enabled = (selPage.Items.Count > 1);

            // Disable command
            cmdRun.Enabled = (selPage.Items.Count > 0) || ckDVB.Enabled;

            // Special
            if (cmdRun.Enabled)
                if (selPage.Items.Count < 1)
                {
                    // Force DVB subtitles
                    ckDVB.Checked = true;
                    ckDVB.Enabled = false;
                }
        }

        private void ckDVB_CheckedChanged( object sender, EventArgs e )
        {
            // Forward
            selPage.Enabled = !ckDVB.Checked;
        }
    }
}
