namespace CardServerTester
{
    partial class TesterMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( TesterMain ) );
            this.grpConfig = new System.Windows.Forms.GroupBox();
            this.ckRestart = new System.Windows.Forms.CheckBox();
            this.ckInProcess = new System.Windows.Forms.CheckBox();
            this.selProfiles = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdStart = new System.Windows.Forms.Button();
            this.cmdStop = new System.Windows.Forms.Button();
            this.ticker = new System.Windows.Forms.Timer( this.components );
            this.label2 = new System.Windows.Forms.Label();
            this.selStation = new System.Windows.Forms.ComboBox();
            this.cmdTune = new System.Windows.Forms.Button();
            this.cmdReceive = new System.Windows.Forms.Button();
            this.cmdUnReceive = new System.Windows.Forms.Button();
            this.cmdUnAll = new System.Windows.Forms.Button();
            this.cmdState = new System.Windows.Forms.Button();
            this.selFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.label3 = new System.Windows.Forms.Label();
            this.txDir = new System.Windows.Forms.TextBox();
            this.cmdSelDir = new System.Windows.Forms.Button();
            this.cmdStream = new System.Windows.Forms.Button();
            this.txStream = new System.Windows.Forms.TextBox();
            this.lstStreams = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
            this.cmdStartEPG = new System.Windows.Forms.Button();
            this.cmdEPGEnd = new System.Windows.Forms.Button();
            this.pnlState = new System.Windows.Forms.Panel();
            this.selServices = new System.Windows.Forms.ComboBox();
            this.cmdStateRefresh = new System.Windows.Forms.Button();
            this.psiProgress = new System.Windows.Forms.ProgressBar();
            this.epgProgress = new System.Windows.Forms.ProgressBar();
            this.txPSISources = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txEPGItems = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txGroup = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txLocation = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txProfile = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.toolTips = new System.Windows.Forms.ToolTip( this.components );
            this.cmdStartPSI = new System.Windows.Forms.Button();
            this.cmdStopPSI = new System.Windows.Forms.Button();
            this.cmdSetZapping = new System.Windows.Forms.Button();
            this.dlgEPG = new System.Windows.Forms.SaveFileDialog();
            this.grpConfig.SuspendLayout();
            this.pnlState.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpConfig
            // 
            resources.ApplyResources( this.grpConfig, "grpConfig" );
            this.grpConfig.Controls.Add( this.ckRestart );
            this.grpConfig.Controls.Add( this.ckInProcess );
            this.grpConfig.Controls.Add( this.selProfiles );
            this.grpConfig.Controls.Add( this.label1 );
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.TabStop = false;
            // 
            // ckRestart
            // 
            resources.ApplyResources( this.ckRestart, "ckRestart" );
            this.ckRestart.Name = "ckRestart";
            this.ckRestart.UseVisualStyleBackColor = true;
            // 
            // ckInProcess
            // 
            resources.ApplyResources( this.ckInProcess, "ckInProcess" );
            this.ckInProcess.Name = "ckInProcess";
            this.ckInProcess.UseVisualStyleBackColor = true;
            this.ckInProcess.CheckedChanged += new System.EventHandler( this.ckInProcess_CheckedChanged );
            // 
            // selProfiles
            // 
            resources.ApplyResources( this.selProfiles, "selProfiles" );
            this.selProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selProfiles.FormattingEnabled = true;
            this.selProfiles.Name = "selProfiles";
            this.selProfiles.Sorted = true;
            this.selProfiles.SelectionChangeCommitted += new System.EventHandler( this.selProfiles_SelectionChangeCommitted );
            // 
            // label1
            // 
            resources.ApplyResources( this.label1, "label1" );
            this.label1.Name = "label1";
            // 
            // cmdStart
            // 
            resources.ApplyResources( this.cmdStart, "cmdStart" );
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.UseVisualStyleBackColor = true;
            this.cmdStart.Click += new System.EventHandler( this.cmdStart_Click );
            // 
            // cmdStop
            // 
            resources.ApplyResources( this.cmdStop, "cmdStop" );
            this.cmdStop.Name = "cmdStop";
            this.cmdStop.UseVisualStyleBackColor = true;
            this.cmdStop.Click += new System.EventHandler( this.cmdStop_Click );
            this.cmdStop.EnabledChanged += new System.EventHandler( this.cmdStop_EnabledChanged );
            // 
            // ticker
            // 
            this.ticker.Enabled = true;
            this.ticker.Tick += new System.EventHandler( this.ticker_Tick );
            // 
            // label2
            // 
            resources.ApplyResources( this.label2, "label2" );
            this.label2.Name = "label2";
            // 
            // selStation
            // 
            resources.ApplyResources( this.selStation, "selStation" );
            this.selStation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selStation.FormattingEnabled = true;
            this.selStation.Name = "selStation";
            this.selStation.Sorted = true;
            this.selStation.SelectionChangeCommitted += new System.EventHandler( this.selStation_SelectionChangeCommitted );
            // 
            // cmdTune
            // 
            resources.ApplyResources( this.cmdTune, "cmdTune" );
            this.cmdTune.Name = "cmdTune";
            this.cmdTune.UseVisualStyleBackColor = true;
            this.cmdTune.Click += new System.EventHandler( this.cmdTune_Click );
            // 
            // cmdReceive
            // 
            resources.ApplyResources( this.cmdReceive, "cmdReceive" );
            this.cmdReceive.Name = "cmdReceive";
            this.cmdReceive.UseVisualStyleBackColor = true;
            this.cmdReceive.Click += new System.EventHandler( this.cmdReceive_Click );
            // 
            // cmdUnReceive
            // 
            resources.ApplyResources( this.cmdUnReceive, "cmdUnReceive" );
            this.cmdUnReceive.Name = "cmdUnReceive";
            this.cmdUnReceive.UseVisualStyleBackColor = true;
            this.cmdUnReceive.Click += new System.EventHandler( this.cmdUnReceive_Click );
            // 
            // cmdUnAll
            // 
            resources.ApplyResources( this.cmdUnAll, "cmdUnAll" );
            this.cmdUnAll.Name = "cmdUnAll";
            this.cmdUnAll.UseVisualStyleBackColor = true;
            this.cmdUnAll.Click += new System.EventHandler( this.cmdUnAll_Click );
            // 
            // cmdState
            // 
            resources.ApplyResources( this.cmdState, "cmdState" );
            this.cmdState.Name = "cmdState";
            this.cmdState.UseVisualStyleBackColor = true;
            this.cmdState.Click += new System.EventHandler( this.cmdState_Click );
            // 
            // selFolder
            // 
            resources.ApplyResources( this.selFolder, "selFolder" );
            // 
            // label3
            // 
            resources.ApplyResources( this.label3, "label3" );
            this.label3.Name = "label3";
            // 
            // txDir
            // 
            resources.ApplyResources( this.txDir, "txDir" );
            this.txDir.Name = "txDir";
            this.txDir.ReadOnly = true;
            // 
            // cmdSelDir
            // 
            resources.ApplyResources( this.cmdSelDir, "cmdSelDir" );
            this.cmdSelDir.Name = "cmdSelDir";
            this.cmdSelDir.UseVisualStyleBackColor = true;
            this.cmdSelDir.Click += new System.EventHandler( this.cmdSelDir_Click );
            // 
            // cmdStream
            // 
            resources.ApplyResources( this.cmdStream, "cmdStream" );
            this.cmdStream.Name = "cmdStream";
            this.cmdStream.UseVisualStyleBackColor = true;
            this.cmdStream.Click += new System.EventHandler( this.cmdStream_Click );
            // 
            // txStream
            // 
            resources.ApplyResources( this.txStream, "txStream" );
            this.txStream.Name = "txStream";
            // 
            // lstStreams
            // 
            resources.ApplyResources( this.lstStreams, "lstStreams" );
            this.lstStreams.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12} );
            this.lstStreams.Name = "lstStreams";
            this.lstStreams.UseCompatibleStateImageBehavior = false;
            this.lstStreams.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources( this.columnHeader1, "columnHeader1" );
            // 
            // columnHeader2
            // 
            resources.ApplyResources( this.columnHeader2, "columnHeader2" );
            // 
            // columnHeader3
            // 
            resources.ApplyResources( this.columnHeader3, "columnHeader3" );
            // 
            // columnHeader4
            // 
            resources.ApplyResources( this.columnHeader4, "columnHeader4" );
            // 
            // columnHeader5
            // 
            resources.ApplyResources( this.columnHeader5, "columnHeader5" );
            // 
            // columnHeader6
            // 
            resources.ApplyResources( this.columnHeader6, "columnHeader6" );
            // 
            // columnHeader7
            // 
            resources.ApplyResources( this.columnHeader7, "columnHeader7" );
            // 
            // columnHeader8
            // 
            resources.ApplyResources( this.columnHeader8, "columnHeader8" );
            // 
            // columnHeader9
            // 
            resources.ApplyResources( this.columnHeader9, "columnHeader9" );
            // 
            // columnHeader10
            // 
            resources.ApplyResources( this.columnHeader10, "columnHeader10" );
            // 
            // columnHeader11
            // 
            resources.ApplyResources( this.columnHeader11, "columnHeader11" );
            // 
            // columnHeader12
            // 
            resources.ApplyResources( this.columnHeader12, "columnHeader12" );
            // 
            // cmdStartEPG
            // 
            resources.ApplyResources( this.cmdStartEPG, "cmdStartEPG" );
            this.cmdStartEPG.Name = "cmdStartEPG";
            this.cmdStartEPG.UseVisualStyleBackColor = true;
            this.cmdStartEPG.Click += new System.EventHandler( this.cmdStartEPG_Click );
            // 
            // cmdEPGEnd
            // 
            resources.ApplyResources( this.cmdEPGEnd, "cmdEPGEnd" );
            this.cmdEPGEnd.Name = "cmdEPGEnd";
            this.cmdEPGEnd.UseVisualStyleBackColor = true;
            this.cmdEPGEnd.Click += new System.EventHandler( this.cmdEPGEnd_Click );
            // 
            // pnlState
            // 
            this.pnlState.BackColor = System.Drawing.Color.White;
            this.pnlState.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlState.Controls.Add( this.selServices );
            this.pnlState.Controls.Add( this.cmdStateRefresh );
            this.pnlState.Controls.Add( this.psiProgress );
            this.pnlState.Controls.Add( this.epgProgress );
            this.pnlState.Controls.Add( this.txPSISources );
            this.pnlState.Controls.Add( this.label9 );
            this.pnlState.Controls.Add( this.label8 );
            this.pnlState.Controls.Add( this.txEPGItems );
            this.pnlState.Controls.Add( this.label7 );
            this.pnlState.Controls.Add( this.txGroup );
            this.pnlState.Controls.Add( this.label6 );
            this.pnlState.Controls.Add( this.txLocation );
            this.pnlState.Controls.Add( this.label5 );
            this.pnlState.Controls.Add( this.txProfile );
            this.pnlState.Controls.Add( this.label4 );
            resources.ApplyResources( this.pnlState, "pnlState" );
            this.pnlState.Name = "pnlState";
            this.toolTips.SetToolTip( this.pnlState, resources.GetString( "pnlState.ToolTip" ) );
            this.pnlState.Click += new System.EventHandler( this.pnlState_Click );
            // 
            // selServices
            // 
            resources.ApplyResources( this.selServices, "selServices" );
            this.selServices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selServices.FormattingEnabled = true;
            this.selServices.Name = "selServices";
            // 
            // cmdStateRefresh
            // 
            resources.ApplyResources( this.cmdStateRefresh, "cmdStateRefresh" );
            this.cmdStateRefresh.Name = "cmdStateRefresh";
            this.cmdStateRefresh.UseVisualStyleBackColor = true;
            this.cmdStateRefresh.Click += new System.EventHandler( this.cmdState_Click );
            // 
            // psiProgress
            // 
            resources.ApplyResources( this.psiProgress, "psiProgress" );
            this.psiProgress.Maximum = 1000;
            this.psiProgress.Name = "psiProgress";
            // 
            // epgProgress
            // 
            resources.ApplyResources( this.epgProgress, "epgProgress" );
            this.epgProgress.Maximum = 1000;
            this.epgProgress.Name = "epgProgress";
            // 
            // txPSISources
            // 
            resources.ApplyResources( this.txPSISources, "txPSISources" );
            this.txPSISources.Name = "txPSISources";
            this.txPSISources.ReadOnly = true;
            // 
            // label9
            // 
            resources.ApplyResources( this.label9, "label9" );
            this.label9.Name = "label9";
            // 
            // label8
            // 
            resources.ApplyResources( this.label8, "label8" );
            this.label8.Name = "label8";
            // 
            // txEPGItems
            // 
            resources.ApplyResources( this.txEPGItems, "txEPGItems" );
            this.txEPGItems.Name = "txEPGItems";
            this.txEPGItems.ReadOnly = true;
            // 
            // label7
            // 
            resources.ApplyResources( this.label7, "label7" );
            this.label7.Name = "label7";
            // 
            // txGroup
            // 
            resources.ApplyResources( this.txGroup, "txGroup" );
            this.txGroup.Name = "txGroup";
            this.txGroup.ReadOnly = true;
            // 
            // label6
            // 
            resources.ApplyResources( this.label6, "label6" );
            this.label6.Name = "label6";
            // 
            // txLocation
            // 
            resources.ApplyResources( this.txLocation, "txLocation" );
            this.txLocation.Name = "txLocation";
            this.txLocation.ReadOnly = true;
            // 
            // label5
            // 
            resources.ApplyResources( this.label5, "label5" );
            this.label5.Name = "label5";
            // 
            // txProfile
            // 
            resources.ApplyResources( this.txProfile, "txProfile" );
            this.txProfile.Name = "txProfile";
            this.txProfile.ReadOnly = true;
            // 
            // label4
            // 
            resources.ApplyResources( this.label4, "label4" );
            this.label4.Name = "label4";
            // 
            // cmdStartPSI
            // 
            resources.ApplyResources( this.cmdStartPSI, "cmdStartPSI" );
            this.cmdStartPSI.Name = "cmdStartPSI";
            this.cmdStartPSI.UseVisualStyleBackColor = true;
            this.cmdStartPSI.Click += new System.EventHandler( this.cmdStartPSI_Click );
            // 
            // cmdStopPSI
            // 
            resources.ApplyResources( this.cmdStopPSI, "cmdStopPSI" );
            this.cmdStopPSI.Name = "cmdStopPSI";
            this.cmdStopPSI.UseVisualStyleBackColor = true;
            this.cmdStopPSI.Click += new System.EventHandler( this.cmdPSIStop_Click );
            // 
            // cmdSetZapping
            // 
            resources.ApplyResources( this.cmdSetZapping, "cmdSetZapping" );
            this.cmdSetZapping.Name = "cmdSetZapping";
            this.cmdSetZapping.UseVisualStyleBackColor = true;
            this.cmdSetZapping.Click += new System.EventHandler( this.cmdSetZapping_Click );
            // 
            // dlgEPG
            // 
            this.dlgEPG.DefaultExt = "xml";
            resources.ApplyResources( this.dlgEPG, "dlgEPG" );
            // 
            // TesterMain
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.pnlState );
            this.Controls.Add( this.cmdStopPSI );
            this.Controls.Add( this.cmdStartPSI );
            this.Controls.Add( this.cmdEPGEnd );
            this.Controls.Add( this.cmdStartEPG );
            this.Controls.Add( this.lstStreams );
            this.Controls.Add( this.txStream );
            this.Controls.Add( this.cmdStream );
            this.Controls.Add( this.cmdSelDir );
            this.Controls.Add( this.txDir );
            this.Controls.Add( this.label3 );
            this.Controls.Add( this.cmdState );
            this.Controls.Add( this.cmdUnAll );
            this.Controls.Add( this.cmdUnReceive );
            this.Controls.Add( this.cmdReceive );
            this.Controls.Add( this.cmdTune );
            this.Controls.Add( this.cmdSetZapping );
            this.Controls.Add( this.selStation );
            this.Controls.Add( this.label2 );
            this.Controls.Add( this.cmdStop );
            this.Controls.Add( this.cmdStart );
            this.Controls.Add( this.grpConfig );
            this.Name = "TesterMain";
            this.grpConfig.ResumeLayout( false );
            this.grpConfig.PerformLayout();
            this.pnlState.ResumeLayout( false );
            this.pnlState.PerformLayout();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.CheckBox ckInProcess;
        private System.Windows.Forms.ComboBox selProfiles;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdStart;
        private System.Windows.Forms.Button cmdStop;
        private System.Windows.Forms.Timer ticker;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox selStation;
        private System.Windows.Forms.Button cmdTune;
        private System.Windows.Forms.Button cmdReceive;
        private System.Windows.Forms.Button cmdUnReceive;
        private System.Windows.Forms.Button cmdUnAll;
        private System.Windows.Forms.Button cmdState;
        private System.Windows.Forms.FolderBrowserDialog selFolder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txDir;
        private System.Windows.Forms.Button cmdSelDir;
        private System.Windows.Forms.Button cmdStream;
        private System.Windows.Forms.TextBox txStream;
        private System.Windows.Forms.ListView lstStreams;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.Button cmdStartEPG;
        private System.Windows.Forms.Button cmdEPGEnd;
        private System.Windows.Forms.Panel pnlState;
        private System.Windows.Forms.TextBox txGroup;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txLocation;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txProfile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.TextBox txEPGItems;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.SaveFileDialog dlgEPG;
        private System.Windows.Forms.ProgressBar epgProgress;
        private System.Windows.Forms.ProgressBar psiProgress;
        private System.Windows.Forms.TextBox txPSISources;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button cmdStartPSI;
        private System.Windows.Forms.Button cmdStopPSI;
        private System.Windows.Forms.Button cmdStateRefresh;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.CheckBox ckRestart;
        private System.Windows.Forms.Button cmdSetZapping;
        private System.Windows.Forms.ComboBox selServices;
        private System.Windows.Forms.Label label9;
    }
}

