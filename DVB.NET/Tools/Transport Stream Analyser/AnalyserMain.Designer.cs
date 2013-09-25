namespace Transport_Stream_Analyser
{
    partial class AnalyserMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( AnalyserMain ) );
            this.cmdFile = new System.Windows.Forms.Button();
            this.cmdDir = new System.Windows.Forms.Button();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader17 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader21 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader20 = new System.Windows.Forms.ColumnHeader();
            this.selFile = new System.Windows.Forms.OpenFileDialog();
            this.lstStreans = new System.Windows.Forms.ListView();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader14 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader15 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader16 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader18 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader19 = new System.Windows.Forms.ColumnHeader();
            this.selFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.pnlOuter = new System.Windows.Forms.Panel();
            this.split = new System.Windows.Forms.Splitter();
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdLoad = new System.Windows.Forms.Button();
            this.xmlCreate = new System.Windows.Forms.SaveFileDialog();
            this.xmlRestore = new System.Windows.Forms.OpenFileDialog();
            this.pnlOuter.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdFile
            // 
            resources.ApplyResources( this.cmdFile, "cmdFile" );
            this.cmdFile.Name = "cmdFile";
            this.cmdFile.UseVisualStyleBackColor = true;
            this.cmdFile.Click += new System.EventHandler( this.cmdFile_Click );
            // 
            // cmdDir
            // 
            resources.ApplyResources( this.cmdDir, "cmdDir" );
            this.cmdDir.Name = "cmdDir";
            this.cmdDir.UseVisualStyleBackColor = true;
            this.cmdDir.Click += new System.EventHandler( this.cmdDir_Click );
            // 
            // lstFiles
            // 
            this.lstFiles.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader4,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader17,
            this.columnHeader21,
            this.columnHeader20} );
            resources.ApplyResources( this.lstFiles, "lstFiles" );
            this.lstFiles.FullRowSelect = true;
            this.lstFiles.GridLines = true;
            this.lstFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstFiles.HideSelection = false;
            this.lstFiles.MinimumSize = new System.Drawing.Size( 615, 150 );
            this.lstFiles.MultiSelect = false;
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            this.lstFiles.View = System.Windows.Forms.View.Details;
            this.lstFiles.SelectedIndexChanged += new System.EventHandler( this.lstFiles_SelectedIndexChanged );
            // 
            // columnHeader1
            // 
            resources.ApplyResources( this.columnHeader1, "columnHeader1" );
            // 
            // columnHeader4
            // 
            resources.ApplyResources( this.columnHeader4, "columnHeader4" );
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
            // columnHeader13
            // 
            resources.ApplyResources( this.columnHeader13, "columnHeader13" );
            // 
            // columnHeader17
            // 
            resources.ApplyResources( this.columnHeader17, "columnHeader17" );
            // 
            // columnHeader21
            // 
            resources.ApplyResources( this.columnHeader21, "columnHeader21" );
            // 
            // columnHeader20
            // 
            resources.ApplyResources( this.columnHeader20, "columnHeader20" );
            // 
            // selFile
            // 
            this.selFile.DefaultExt = "ts";
            resources.ApplyResources( this.selFile, "selFile" );
            // 
            // lstStreans
            // 
            this.lstStreans.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader5,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader16,
            this.columnHeader18,
            this.columnHeader19} );
            resources.ApplyResources( this.lstStreans, "lstStreans" );
            this.lstStreans.FullRowSelect = true;
            this.lstStreans.GridLines = true;
            this.lstStreans.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstStreans.HideSelection = false;
            this.lstStreans.MinimumSize = new System.Drawing.Size( 615, 252 );
            this.lstStreans.Name = "lstStreans";
            this.lstStreans.UseCompatibleStateImageBehavior = false;
            this.lstStreans.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            resources.ApplyResources( this.columnHeader2, "columnHeader2" );
            // 
            // columnHeader3
            // 
            resources.ApplyResources( this.columnHeader3, "columnHeader3" );
            // 
            // columnHeader5
            // 
            resources.ApplyResources( this.columnHeader5, "columnHeader5" );
            // 
            // columnHeader14
            // 
            resources.ApplyResources( this.columnHeader14, "columnHeader14" );
            // 
            // columnHeader15
            // 
            resources.ApplyResources( this.columnHeader15, "columnHeader15" );
            // 
            // columnHeader16
            // 
            resources.ApplyResources( this.columnHeader16, "columnHeader16" );
            // 
            // columnHeader18
            // 
            resources.ApplyResources( this.columnHeader18, "columnHeader18" );
            // 
            // columnHeader19
            // 
            resources.ApplyResources( this.columnHeader19, "columnHeader19" );
            // 
            // selFolder
            // 
            resources.ApplyResources( this.selFolder, "selFolder" );
            this.selFolder.ShowNewFolderButton = false;
            // 
            // pnlOuter
            // 
            resources.ApplyResources( this.pnlOuter, "pnlOuter" );
            this.pnlOuter.Controls.Add( this.split );
            this.pnlOuter.Controls.Add( this.lstStreans );
            this.pnlOuter.Controls.Add( this.lstFiles );
            this.pnlOuter.Name = "pnlOuter";
            // 
            // split
            // 
            resources.ApplyResources( this.split, "split" );
            this.split.Name = "split";
            this.split.TabStop = false;
            // 
            // cmdSave
            // 
            resources.ApplyResources( this.cmdSave, "cmdSave" );
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.UseVisualStyleBackColor = true;
            this.cmdSave.Click += new System.EventHandler( this.cmdSave_Click );
            // 
            // cmdLoad
            // 
            resources.ApplyResources( this.cmdLoad, "cmdLoad" );
            this.cmdLoad.Name = "cmdLoad";
            this.cmdLoad.UseVisualStyleBackColor = true;
            this.cmdLoad.Click += new System.EventHandler( this.cmdLoad_Click );
            // 
            // xmlCreate
            // 
            this.xmlCreate.DefaultExt = "xml";
            resources.ApplyResources( this.xmlCreate, "xmlCreate" );
            // 
            // xmlRestore
            // 
            this.xmlRestore.DefaultExt = "xml";
            resources.ApplyResources( this.xmlRestore, "xmlRestore" );
            // 
            // AnalyserMain
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.cmdLoad );
            this.Controls.Add( this.cmdSave );
            this.Controls.Add( this.pnlOuter );
            this.Controls.Add( this.cmdDir );
            this.Controls.Add( this.cmdFile );
            this.Name = "AnalyserMain";
            this.Load += new System.EventHandler( this.AnalyserMain_Load );
            this.pnlOuter.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Button cmdFile;
        private System.Windows.Forms.Button cmdDir;
        private System.Windows.Forms.ListView lstFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.OpenFileDialog selFile;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ListView lstStreans;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.ColumnHeader columnHeader18;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.FolderBrowserDialog selFolder;
        private System.Windows.Forms.Panel pnlOuter;
        private System.Windows.Forms.Splitter split;
        private System.Windows.Forms.ColumnHeader columnHeader21;
        private System.Windows.Forms.ColumnHeader columnHeader20;
        private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.Button cmdLoad;
        private System.Windows.Forms.SaveFileDialog xmlCreate;
        private System.Windows.Forms.OpenFileDialog xmlRestore;
    }
}

