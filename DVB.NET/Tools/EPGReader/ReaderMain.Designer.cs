namespace EPGReader
{
	partial class ReaderMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ReaderMain ) );
            this.openInput = new System.Windows.Forms.OpenFileDialog();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.cmdStop = new System.Windows.Forms.Button();
            this.starter = new System.Windows.Forms.Timer( this.components );
            this.lstEntries = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.ckAll = new System.Windows.Forms.CheckBox();
            this.ckStandardSI = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // openInput
            // 
            this.openInput.DefaultExt = "bin";
            resources.ApplyResources( this.openInput, "openInput" );
            // 
            // progress
            // 
            resources.ApplyResources( this.progress, "progress" );
            this.progress.Maximum = 1000;
            this.progress.Name = "progress";
            // 
            // cmdStop
            // 
            resources.ApplyResources( this.cmdStop, "cmdStop" );
            this.cmdStop.Name = "cmdStop";
            this.cmdStop.UseVisualStyleBackColor = true;
            this.cmdStop.Click += new System.EventHandler( this.cmdStop_Click );
            // 
            // starter
            // 
            this.starter.Tick += new System.EventHandler( this.starter_Tick );
            // 
            // lstEntries
            // 
            resources.ApplyResources( this.lstEntries, "lstEntries" );
            this.lstEntries.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4} );
            this.lstEntries.FullRowSelect = true;
            this.lstEntries.Name = "lstEntries";
            this.lstEntries.UseCompatibleStateImageBehavior = false;
            this.lstEntries.View = System.Windows.Forms.View.Details;
            this.lstEntries.DoubleClick += new System.EventHandler( this.lstEntries_DoubleClick );
            this.lstEntries.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler( this.lstEntries_ColumnClick );
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
            // ckAll
            // 
            resources.ApplyResources( this.ckAll, "ckAll" );
            this.ckAll.Name = "ckAll";
            this.ckAll.UseVisualStyleBackColor = true;
            // 
            // ckStandardSI
            // 
            resources.ApplyResources( this.ckStandardSI, "ckStandardSI" );
            this.ckStandardSI.Name = "ckStandardSI";
            this.ckStandardSI.UseVisualStyleBackColor = true;
            // 
            // ReaderMain
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.ckStandardSI );
            this.Controls.Add( this.ckAll );
            this.Controls.Add( this.lstEntries );
            this.Controls.Add( this.cmdStop );
            this.Controls.Add( this.progress );
            this.Name = "ReaderMain";
            this.Load += new System.EventHandler( this.ReaderMain_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog openInput;
		private System.Windows.Forms.ProgressBar progress;
		private System.Windows.Forms.Button cmdStop;
		private System.Windows.Forms.Timer starter;
		private System.Windows.Forms.ListView lstEntries;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.CheckBox ckAll;
        private System.Windows.Forms.CheckBox ckStandardSI;
	}
}