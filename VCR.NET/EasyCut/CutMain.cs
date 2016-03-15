using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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
        private CheckBox ckMux;

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
            this.ckMux = new System.Windows.Forms.CheckBox();
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
            resources.ApplyResources( this.selRate, "selRate" );
            this.selRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selRate.Name = "selRate";
            // 
            // label3
            // 
            resources.ApplyResources( this.label3, "label3" );
            this.label3.Name = "label3";
            // 
            // udMinimum
            // 
            resources.ApplyResources( this.udMinimum, "udMinimum" );
            this.udMinimum.DecimalPlaces = 2;
            this.udMinimum.Increment = new decimal( new int[] {
            1,
            0,
            0,
            65536} );
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
            resources.ApplyResources( this.udCorrect, "udCorrect" );
            this.udCorrect.Increment = new decimal( new int[] {
            15,
            0,
            0,
            0} );
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
            this.saveCut.FilterIndex = 2;
            // 
            // selPage
            // 
            resources.ApplyResources( this.selPage, "selPage" );
            this.selPage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selPage.FormattingEnabled = true;
            this.selPage.Name = "selPage";
            this.selPage.Sorted = true;
            // 
            // ckMux
            // 
            resources.ApplyResources( this.ckMux, "ckMux" );
            this.ckMux.Name = "ckMux";
            this.ckMux.UseVisualStyleBackColor = true;
            // 
            // CutMain
            // 
            resources.ApplyResources( this, "$this" );
            this.Controls.Add( this.ckMux );
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
            Properties.Settings.Default.MuxDefault = ckMux.Checked;
            Properties.Settings.Default.Save();

            // Check selection of page
            if (selPage.SelectedItem != null)
                Properties.Settings.Default.SubtitlePage = (int) selPage.SelectedItem;

            // With error handling
            try
            {
                // Load file
                var outFile = new FileInfo( saveCut.FileName );

                // Get the type
                var suffix = outFile.Extension;

                // Check
                if ((suffix == null) || (suffix.Length < 2))
                    throw new ApplicationException( Properties.Resources.Error_Format );

                // Cut off
                suffix = suffix.Substring( 1 );

                // Load type name
                var typeName = ConfigurationManager.AppSettings[suffix.ToUpper()];

                // Check
                if ((typeName == null) || (typeName.Length < 1))
                    throw new ApplicationException( Properties.Resources.Error_Format );

                // Find the type
                var cutType = Type.GetType( typeName, true );

                // Create extractor
                using (var cutter = (ICutter) Activator.CreateInstance( cutType ))
                {
                    // Configure
                    cutter.Framerate = FrameRateInfo.FindFrameRate( selRate.SelectedIndex ).Rate;
                    cutter.MinDuration = (double) udMinimum.Value;

                    // Check for VCR.NET 3.1 or later
                    var cutter31 = cutter as ICutter2;
                    if (cutter31 != null)
                        cutter31.TimeCorrection = (double) udCorrect.Value;

                    // Position in output file
                    long pos = 0;

                    // Process all cuts
                    foreach (var cut in m_ProjectFile.CutElements)
                    {
                        // Get the core name
                        var coreName = Path.GetFileNameWithoutExtension( cut.VideoFile.FullName );

                        // Append page number
                        if (!ckDVB.Checked)
                            coreName += "[" + Properties.Settings.Default.SubtitlePage.ToString() + "]";

                        // Replace suffix
                        var ttxFile = new FileInfo( cut.VideoFile.DirectoryName + @"\" + coreName + "." + suffix );

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

                    // Mux it
                    if (ckMux.Checked)
                        DoMux( saveCut.FileName );
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
        /// Mischt die Untertitel in eine Datei ein.
        /// </summary>
        /// <param name="supPath">Der volle Pfad zur Untertiteldatei.</param>
        private void DoMux( string supPath )
        {
            // Check mode
            var palPath = supPath + ".pal";
            if (!File.Exists( palPath ))
                return;

            // Must have GfD Path
            var gfdPath = Properties.Settings.Default.GfDFolder;
            if (!string.IsNullOrEmpty( gfdPath ))
                if (!Directory.Exists( gfdPath ))
                    gfdPath = null;

            // Ask user
            if (string.IsNullOrEmpty( gfdPath ))
                using (var dlg = new FolderBrowserDialog())
                {
                    dlg.Description = Properties.Resources.Browse_GfD;
                    dlg.ShowNewFolderButton = false;

                    if (dlg.ShowDialog( this ) != DialogResult.OK)
                        return;

                    // Take it
                    Properties.Settings.Default.GfDFolder = gfdPath = dlg.SelectedPath;
                    Properties.Settings.Default.Save();
                }

            // Ask for source file
            string videoFile;

            using (var mpg = new OpenFileDialog())
            {
                // Configure
                mpg.Title = Properties.Resources.Browse_Mpg;
                mpg.Filter = "Videos|*.mpg";
                mpg.CheckFileExists = true;
                mpg.AddExtension = true;
                mpg.DefaultExt = "mpg";

                // Select the file
                if (mpg.ShowDialog( this ) != DialogResult.OK)
                    return;

                videoFile = mpg.FileName;
            }

            // Fixed output path
            var supDir = Path.Combine( Path.GetDirectoryName( supPath ), Path.GetFileNameWithoutExtension( supPath ) + ".d" );

            if (Directory.Exists( supDir ))
                Directory.Delete( supDir, true );

            // Generate pictures
            var procInfo =
                new ProcessStartInfo
                {
                    Arguments = $"\"{supPath.Replace( "\"", "\"\"" )}\" \"{palPath.Replace( "\"", "\"\"" )}\"",
                    FileName = Path.Combine( gfdPath, "sup2png.exe" ),
                    UseShellExecute = false,
                };

            using (var process = Process.Start( procInfo ))
                process.WaitForExit();

            // Mux - in-place, better user SSD!
            procInfo =
                new ProcessStartInfo
                {
                    Arguments = $"-s0 \"{Path.Combine( supDir, "spumux.xml" ).Replace( "\"", "\"\"" )}\"",
                    FileName = Path.Combine( gfdPath, "spumux.exe" ),
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                };

            using (var process = Process.Start( procInfo ))
            {
                var outbuffer = new byte[10000000];

                // Send input file
                Task.Run( () =>
                 {
                     var inbuffer = new byte[outbuffer.Length];

                     using (var input = new FileStream( videoFile, FileMode.Open, FileAccess.Read, FileShare.Read, inbuffer.Length ))
                         for (int n; (n = input.Read( inbuffer, 0, inbuffer.Length )) > 0;)
                             process.StandardInput.BaseStream.Write( inbuffer, 0, n );

                     process.StandardInput.BaseStream.Close();
                 } );

                // Generate target file
                using (var output = new FileStream( Path.Combine( Path.GetDirectoryName( videoFile ), Path.GetFileNameWithoutExtension( videoFile ) + "_SUP.mpg" ), FileMode.Create, FileAccess.Write, FileShare.None, outbuffer.Length ))
                    for (int n; (n = process.StandardOutput.BaseStream.Read( outbuffer, 0, outbuffer.Length )) > 0;)
                        output.Write( outbuffer, 0, n );

                // Synchronize
                process.WaitForExit();
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
            ckMux.Checked = Properties.Settings.Default.MuxDefault;
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
