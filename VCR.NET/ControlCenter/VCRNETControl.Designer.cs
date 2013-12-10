namespace VCRControlCenter
{
	partial class VCRNETControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = new System.ComponentModel.Container();

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				// Hibernation control
				if (null != m_Hibernation) m_Hibernation.Dispose();

				// Forget
				m_Hibernation = null;

				// All icons
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VCRNETControl));
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuOpenJobList = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEPG = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNewJob = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCurrent = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLiveConnect = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAdmin = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHibernateServer = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuWakeupServer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuHibernate = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHibernateReset = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHibernateSep = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuClose = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdAdd = new System.Windows.Forms.Button();
            this.cmdUpdate = new System.Windows.Forms.Button();
            this.lstServers = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txServer = new System.Windows.Forms.TextBox();
            this.selPort = new System.Windows.Forms.NumericUpDown();
            this.selInterval = new System.Windows.Forms.NumericUpDown();
            this.ckExtensions = new System.Windows.Forms.CheckBox();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.frameLocal = new System.Windows.Forms.GroupBox();
            this.cmdUpdateAll = new System.Windows.Forms.Button();
            this.selHibernate = new System.Windows.Forms.NumericUpDown();
            this.ckAutoStart = new System.Windows.Forms.CheckBox();
            this.ckHibernate = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.selLanguage = new System.Windows.Forms.ComboBox();
            this.tickProcess = new System.Windows.Forms.Timer(this.components);
            this.grpStreaming = new System.Windows.Forms.GroupBox();
            this.cmdStreaming = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.txArgs = new System.Windows.Forms.TextBox();
            this.txMultiCast = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cmdAppChooser = new System.Windows.Forms.Button();
            this.txViewer = new System.Windows.Forms.TextBox();
            this.selStreamPort = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.errorMessages = new System.Windows.Forms.ErrorProvider(this.components);
            this.label10 = new System.Windows.Forms.Label();
            this.selDelay = new System.Windows.Forms.NumericUpDown();
            this.txSubNet = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.fileChooser = new System.Windows.Forms.OpenFileDialog();
            this.trayMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selInterval)).BeginInit();
            this.frameLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selHibernate)).BeginInit();
            this.grpStreaming.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selStreamPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorMessages)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDefault,
            this.toolStripMenuItem2,
            this.mnuOpenJobList,
            this.mnuEPG,
            this.mnuNewJob,
            this.mnuCurrent,
            this.mnuLiveConnect,
            this.toolStripMenuItem1,
            this.mnuAdmin,
            this.mnuHibernateServer,
            this.mnuWakeupServer,
            this.toolStripMenuItem3,
            this.mnuHibernate,
            this.mnuHibernateSep,
            this.mnuSettings,
            this.mnuClose});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.ShowImageMargin = false;
            resources.ApplyResources(this.trayMenu, "trayMenu");
            this.trayMenu.Opening += new System.ComponentModel.CancelEventHandler(this.trayMenu_Opening);
            // 
            // mnuDefault
            // 
            this.mnuDefault.Name = "mnuDefault";
            resources.ApplyResources(this.mnuDefault, "mnuDefault");
            this.mnuDefault.Click += new System.EventHandler(this.mnuDefault_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // mnuOpenJobList
            // 
            this.mnuOpenJobList.Name = "mnuOpenJobList";
            resources.ApplyResources(this.mnuOpenJobList, "mnuOpenJobList");
            this.mnuOpenJobList.Click += new System.EventHandler(this.mnuOpenJobList_Click);
            // 
            // mnuEPG
            // 
            this.mnuEPG.Name = "mnuEPG";
            resources.ApplyResources(this.mnuEPG, "mnuEPG");
            this.mnuEPG.Click += new System.EventHandler(this.mnuEPG_Click);
            // 
            // mnuNewJob
            // 
            this.mnuNewJob.Name = "mnuNewJob";
            resources.ApplyResources(this.mnuNewJob, "mnuNewJob");
            this.mnuNewJob.Click += new System.EventHandler(this.mnuNewJob_Click);
            // 
            // mnuCurrent
            // 
            this.mnuCurrent.Name = "mnuCurrent";
            resources.ApplyResources(this.mnuCurrent, "mnuCurrent");
            this.mnuCurrent.Click += new System.EventHandler(this.mnuCurrent_Click);
            // 
            // mnuLiveConnect
            // 
            this.mnuLiveConnect.Name = "mnuLiveConnect";
            resources.ApplyResources(this.mnuLiveConnect, "mnuLiveConnect");
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // mnuAdmin
            // 
            this.mnuAdmin.Name = "mnuAdmin";
            resources.ApplyResources(this.mnuAdmin, "mnuAdmin");
            this.mnuAdmin.Click += new System.EventHandler(this.mnuAdmin_Click);
            // 
            // mnuHibernateServer
            // 
            this.mnuHibernateServer.Name = "mnuHibernateServer";
            resources.ApplyResources(this.mnuHibernateServer, "mnuHibernateServer");
            this.mnuHibernateServer.Click += new System.EventHandler(this.mnuHibernateServer_Click);
            // 
            // mnuWakeupServer
            // 
            this.mnuWakeupServer.Name = "mnuWakeupServer";
            resources.ApplyResources(this.mnuWakeupServer, "mnuWakeupServer");
            this.mnuWakeupServer.Click += new System.EventHandler(this.mnuWakeupServer_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            // 
            // mnuHibernate
            // 
            this.mnuHibernate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHibernateReset});
            this.mnuHibernate.Name = "mnuHibernate";
            resources.ApplyResources(this.mnuHibernate, "mnuHibernate");
            // 
            // mnuHibernateReset
            // 
            this.mnuHibernateReset.Name = "mnuHibernateReset";
            resources.ApplyResources(this.mnuHibernateReset, "mnuHibernateReset");
            this.mnuHibernateReset.Click += new System.EventHandler(this.mnuHibernateReset_Click);
            // 
            // mnuHibernateSep
            // 
            this.mnuHibernateSep.Name = "mnuHibernateSep";
            resources.ApplyResources(this.mnuHibernateSep, "mnuHibernateSep");
            // 
            // mnuSettings
            // 
            this.mnuSettings.Name = "mnuSettings";
            resources.ApplyResources(this.mnuSettings, "mnuSettings");
            this.mnuSettings.Click += new System.EventHandler(this.mnuSettings_Click);
            // 
            // mnuClose
            // 
            this.mnuClose.Name = "mnuClose";
            resources.ApplyResources(this.mnuClose, "mnuClose");
            this.mnuClose.Click += new System.EventHandler(this.mnuClose_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmdAdd
            // 
            resources.ApplyResources(this.cmdAdd, "cmdAdd");
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.UseVisualStyleBackColor = true;
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdUpdate
            // 
            resources.ApplyResources(this.cmdUpdate, "cmdUpdate");
            this.cmdUpdate.Name = "cmdUpdate";
            this.cmdUpdate.UseVisualStyleBackColor = true;
            this.cmdUpdate.Click += new System.EventHandler(this.cmdUpdate_Click);
            // 
            // lstServers
            // 
            resources.ApplyResources(this.lstServers, "lstServers");
            this.lstServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.lstServers.FullRowSelect = true;
            this.lstServers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstServers.MultiSelect = false;
            this.lstServers.Name = "lstServers";
            this.lstServers.UseCompatibleStateImageBehavior = false;
            this.lstServers.View = System.Windows.Forms.View.Details;
            this.lstServers.SelectedIndexChanged += new System.EventHandler(this.lstServers_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // columnHeader5
            // 
            resources.ApplyResources(this.columnHeader5, "columnHeader5");
            // 
            // txServer
            // 
            resources.ApplyResources(this.txServer, "txServer");
            this.txServer.Name = "txServer";
            this.txServer.TextChanged += new System.EventHandler(this.UpdateGUI);
            // 
            // selPort
            // 
            resources.ApplyResources(this.selPort, "selPort");
            this.selPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.selPort.Name = "selPort";
            this.selPort.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            this.selPort.ValueChanged += new System.EventHandler(this.UpdateGUI);
            // 
            // selInterval
            // 
            resources.ApplyResources(this.selInterval, "selInterval");
            this.selInterval.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.selInterval.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.selInterval.Name = "selInterval";
            this.selInterval.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.selInterval.ValueChanged += new System.EventHandler(this.UpdateGUI);
            // 
            // ckExtensions
            // 
            resources.ApplyResources(this.ckExtensions, "ckExtensions");
            this.ckExtensions.Checked = true;
            this.ckExtensions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckExtensions.Name = "ckExtensions";
            this.ckExtensions.UseVisualStyleBackColor = true;
            this.ckExtensions.CheckedChanged += new System.EventHandler(this.UpdateGUI);
            // 
            // cmdDelete
            // 
            resources.ApplyResources(this.cmdDelete, "cmdDelete");
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // frameLocal
            // 
            this.frameLocal.Controls.Add(this.cmdUpdateAll);
            this.frameLocal.Controls.Add(this.selHibernate);
            this.frameLocal.Controls.Add(this.ckAutoStart);
            this.frameLocal.Controls.Add(this.ckHibernate);
            resources.ApplyResources(this.frameLocal, "frameLocal");
            this.frameLocal.Name = "frameLocal";
            this.frameLocal.TabStop = false;
            // 
            // cmdUpdateAll
            // 
            resources.ApplyResources(this.cmdUpdateAll, "cmdUpdateAll");
            this.cmdUpdateAll.Name = "cmdUpdateAll";
            this.cmdUpdateAll.UseVisualStyleBackColor = true;
            this.cmdUpdateAll.Click += new System.EventHandler(this.cmdUpdateAll_Click);
            // 
            // selHibernate
            // 
            resources.ApplyResources(this.selHibernate, "selHibernate");
            this.selHibernate.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.selHibernate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.selHibernate.Name = "selHibernate";
            this.selHibernate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.selHibernate.ValueChanged += new System.EventHandler(this.CheckLocal);
            // 
            // ckAutoStart
            // 
            resources.ApplyResources(this.ckAutoStart, "ckAutoStart");
            this.ckAutoStart.Name = "ckAutoStart";
            this.ckAutoStart.UseVisualStyleBackColor = true;
            this.ckAutoStart.CheckedChanged += new System.EventHandler(this.CheckLocal);
            // 
            // ckHibernate
            // 
            resources.ApplyResources(this.ckHibernate, "ckHibernate");
            this.ckHibernate.Name = "ckHibernate";
            this.ckHibernate.UseVisualStyleBackColor = true;
            this.ckHibernate.CheckedChanged += new System.EventHandler(this.CheckLocal);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // selLanguage
            // 
            this.selLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selLanguage.FormattingEnabled = true;
            resources.ApplyResources(this.selLanguage, "selLanguage");
            this.selLanguage.Name = "selLanguage";
            this.selLanguage.Sorted = true;
            this.selLanguage.SelectionChangeCommitted += new System.EventHandler(this.selLanguage_SelectionChangeCommitted);
            // 
            // tickProcess
            // 
            this.tickProcess.Interval = 1000;
            this.tickProcess.Tick += new System.EventHandler(this.tickProcess_Tick);
            // 
            // grpStreaming
            // 
            this.grpStreaming.Controls.Add(this.cmdStreaming);
            this.grpStreaming.Controls.Add(this.label9);
            this.grpStreaming.Controls.Add(this.txArgs);
            this.grpStreaming.Controls.Add(this.txMultiCast);
            this.grpStreaming.Controls.Add(this.label8);
            this.grpStreaming.Controls.Add(this.label7);
            this.grpStreaming.Controls.Add(this.cmdAppChooser);
            this.grpStreaming.Controls.Add(this.txViewer);
            this.grpStreaming.Controls.Add(this.selStreamPort);
            this.grpStreaming.Controls.Add(this.label6);
            resources.ApplyResources(this.grpStreaming, "grpStreaming");
            this.grpStreaming.Name = "grpStreaming";
            this.grpStreaming.TabStop = false;
            // 
            // cmdStreaming
            // 
            resources.ApplyResources(this.cmdStreaming, "cmdStreaming");
            this.cmdStreaming.Name = "cmdStreaming";
            this.cmdStreaming.UseVisualStyleBackColor = true;
            this.cmdStreaming.Click += new System.EventHandler(this.cmdStreaming_Click);
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // txArgs
            // 
            this.errorMessages.SetIconAlignment(this.txArgs, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txArgs.IconAlignment"))));
            resources.ApplyResources(this.txArgs, "txArgs");
            this.txArgs.Name = "txArgs";
            this.txArgs.Validating += new System.ComponentModel.CancelEventHandler(this.txArgs_Validating);
            // 
            // txMultiCast
            // 
            this.errorMessages.SetIconAlignment(this.txMultiCast, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txMultiCast.IconAlignment"))));
            resources.ApplyResources(this.txMultiCast, "txMultiCast");
            this.txMultiCast.Name = "txMultiCast";
            this.txMultiCast.Validating += new System.ComponentModel.CancelEventHandler(this.txMultiCast_Validating);
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // cmdAppChooser
            // 
            resources.ApplyResources(this.cmdAppChooser, "cmdAppChooser");
            this.cmdAppChooser.Name = "cmdAppChooser";
            this.cmdAppChooser.UseVisualStyleBackColor = true;
            this.cmdAppChooser.Click += new System.EventHandler(this.cmdAppChooser_Click);
            // 
            // txViewer
            // 
            this.errorMessages.SetIconAlignment(this.txViewer, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txViewer.IconAlignment"))));
            resources.ApplyResources(this.txViewer, "txViewer");
            this.txViewer.Name = "txViewer";
            this.txViewer.ReadOnly = true;
            // 
            // selStreamPort
            // 
            resources.ApplyResources(this.selStreamPort, "selStreamPort");
            this.selStreamPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.selStreamPort.Name = "selStreamPort";
            this.selStreamPort.Value = new decimal(new int[] {
            2910,
            0,
            0,
            0});
            this.selStreamPort.ValueChanged += new System.EventHandler(this.UpdateGUI);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // errorMessages
            // 
            this.errorMessages.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.AlwaysBlink;
            this.errorMessages.ContainerControl = this;
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // selDelay
            // 
            resources.ApplyResources(this.selDelay, "selDelay");
            this.selDelay.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.selDelay.Name = "selDelay";
            this.selDelay.ValueChanged += new System.EventHandler(this.selDelay_ValueChanged);
            // 
            // txSubNet
            // 
            resources.ApplyResources(this.txSubNet, "txSubNet");
            this.txSubNet.Name = "txSubNet";
            this.txSubNet.TextChanged += new System.EventHandler(this.UpdateGUI);
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // fileChooser
            // 
            this.fileChooser.DefaultExt = "exe";
            this.fileChooser.FileName = "vlc.exe";
            resources.ApplyResources(this.fileChooser, "fileChooser");
            // 
            // VCRNETControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.selDelay);
            this.Controls.Add(this.grpStreaming);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdAdd);
            this.Controls.Add(this.cmdUpdate);
            this.Controls.Add(this.frameLocal);
            this.Controls.Add(this.selLanguage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.txSubNet);
            this.Controls.Add(this.txServer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lstServers);
            this.Controls.Add(this.selInterval);
            this.Controls.Add(this.ckExtensions);
            this.Controls.Add(this.selPort);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "VCRNETControl";
            this.trayMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.selPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selInterval)).EndInit();
            this.frameLocal.ResumeLayout(false);
            this.frameLocal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selHibernate)).EndInit();
            this.grpStreaming.ResumeLayout(false);
            this.grpStreaming.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selStreamPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorMessages)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selDelay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ContextMenuStrip trayMenu;
		private System.Windows.Forms.ToolStripMenuItem mnuDefault;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem mnuOpenJobList;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem mnuAdmin;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem mnuSettings;
		private System.Windows.Forms.ToolStripMenuItem mnuClose;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button cmdAdd;
		private System.Windows.Forms.Button cmdUpdate;
		private System.Windows.Forms.ListView lstServers;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.TextBox txServer;
		private System.Windows.Forms.NumericUpDown selPort;
		private System.Windows.Forms.NumericUpDown selInterval;
		private System.Windows.Forms.CheckBox ckExtensions;
		private System.Windows.Forms.Button cmdDelete;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ToolStripMenuItem mnuCurrent;
		private System.Windows.Forms.GroupBox frameLocal;
        private System.Windows.Forms.CheckBox ckAutoStart;
		private System.Windows.Forms.CheckBox ckHibernate;
		private System.Windows.Forms.Button cmdUpdateAll;
		private System.Windows.Forms.NumericUpDown selHibernate;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox selLanguage;
		private System.Windows.Forms.Timer tickProcess;
		private System.Windows.Forms.ToolStripMenuItem mnuEPG;
		private System.Windows.Forms.ToolStripMenuItem mnuNewJob;
		private System.Windows.Forms.GroupBox grpStreaming;
		private System.Windows.Forms.Button cmdStreaming;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox txMultiCast;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button cmdAppChooser;
		private System.Windows.Forms.TextBox txViewer;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ErrorProvider errorMessages;
		private System.Windows.Forms.TextBox txArgs;
		private System.Windows.Forms.NumericUpDown selStreamPort;
		private System.Windows.Forms.OpenFileDialog fileChooser;
		private System.Windows.Forms.NumericUpDown selDelay;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.ToolStripMenuItem mnuHibernate;
		private System.Windows.Forms.ToolStripMenuItem mnuHibernateReset;
		private System.Windows.Forms.ToolStripSeparator mnuHibernateSep;
        private System.Windows.Forms.ToolStripMenuItem mnuLiveConnect;
        private System.Windows.Forms.ToolStripMenuItem mnuHibernateServer;
        private System.Windows.Forms.ToolStripMenuItem mnuWakeupServer;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txSubNet;
	}
}

