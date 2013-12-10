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
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txServer = new System.Windows.Forms.TextBox();
            this.selPort = new System.Windows.Forms.NumericUpDown();
            this.selInterval = new System.Windows.Forms.NumericUpDown();
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
            resources.ApplyResources(this.trayMenu, "trayMenu");
            this.errorMessages.SetError(this.trayMenu, resources.GetString("trayMenu.Error"));
            this.errorMessages.SetIconAlignment(this.trayMenu, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("trayMenu.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.trayMenu, ((int)(resources.GetObject("trayMenu.IconPadding"))));
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
            this.trayMenu.Opening += new System.ComponentModel.CancelEventHandler(this.trayMenu_Opening);
            // 
            // mnuDefault
            // 
            resources.ApplyResources(this.mnuDefault, "mnuDefault");
            this.mnuDefault.Name = "mnuDefault";
            this.mnuDefault.Click += new System.EventHandler(this.mnuDefault_Click);
            // 
            // toolStripMenuItem2
            // 
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            // 
            // mnuOpenJobList
            // 
            resources.ApplyResources(this.mnuOpenJobList, "mnuOpenJobList");
            this.mnuOpenJobList.Name = "mnuOpenJobList";
            this.mnuOpenJobList.Click += new System.EventHandler(this.mnuOpenJobList_Click);
            // 
            // mnuEPG
            // 
            resources.ApplyResources(this.mnuEPG, "mnuEPG");
            this.mnuEPG.Name = "mnuEPG";
            this.mnuEPG.Click += new System.EventHandler(this.mnuEPG_Click);
            // 
            // mnuNewJob
            // 
            resources.ApplyResources(this.mnuNewJob, "mnuNewJob");
            this.mnuNewJob.Name = "mnuNewJob";
            this.mnuNewJob.Click += new System.EventHandler(this.mnuNewJob_Click);
            // 
            // mnuCurrent
            // 
            resources.ApplyResources(this.mnuCurrent, "mnuCurrent");
            this.mnuCurrent.Name = "mnuCurrent";
            this.mnuCurrent.Click += new System.EventHandler(this.mnuCurrent_Click);
            // 
            // mnuLiveConnect
            // 
            resources.ApplyResources(this.mnuLiveConnect, "mnuLiveConnect");
            this.mnuLiveConnect.Name = "mnuLiveConnect";
            // 
            // toolStripMenuItem1
            // 
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            // 
            // mnuAdmin
            // 
            resources.ApplyResources(this.mnuAdmin, "mnuAdmin");
            this.mnuAdmin.Name = "mnuAdmin";
            this.mnuAdmin.Click += new System.EventHandler(this.mnuAdmin_Click);
            // 
            // mnuHibernateServer
            // 
            resources.ApplyResources(this.mnuHibernateServer, "mnuHibernateServer");
            this.mnuHibernateServer.Name = "mnuHibernateServer";
            this.mnuHibernateServer.Click += new System.EventHandler(this.mnuHibernateServer_Click);
            // 
            // mnuWakeupServer
            // 
            resources.ApplyResources(this.mnuWakeupServer, "mnuWakeupServer");
            this.mnuWakeupServer.Name = "mnuWakeupServer";
            this.mnuWakeupServer.Click += new System.EventHandler(this.mnuWakeupServer_Click);
            // 
            // toolStripMenuItem3
            // 
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            // 
            // mnuHibernate
            // 
            resources.ApplyResources(this.mnuHibernate, "mnuHibernate");
            this.mnuHibernate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHibernateReset});
            this.mnuHibernate.Name = "mnuHibernate";
            // 
            // mnuHibernateReset
            // 
            resources.ApplyResources(this.mnuHibernateReset, "mnuHibernateReset");
            this.mnuHibernateReset.Name = "mnuHibernateReset";
            this.mnuHibernateReset.Click += new System.EventHandler(this.mnuHibernateReset_Click);
            // 
            // mnuHibernateSep
            // 
            resources.ApplyResources(this.mnuHibernateSep, "mnuHibernateSep");
            this.mnuHibernateSep.Name = "mnuHibernateSep";
            // 
            // mnuSettings
            // 
            resources.ApplyResources(this.mnuSettings, "mnuSettings");
            this.mnuSettings.Name = "mnuSettings";
            this.mnuSettings.Click += new System.EventHandler(this.mnuSettings_Click);
            // 
            // mnuClose
            // 
            resources.ApplyResources(this.mnuClose, "mnuClose");
            this.mnuClose.Name = "mnuClose";
            this.mnuClose.Click += new System.EventHandler(this.mnuClose_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.errorMessages.SetError(this.label1, resources.GetString("label1.Error"));
            this.errorMessages.SetIconAlignment(this.label1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label1.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label1, ((int)(resources.GetObject("label1.IconPadding"))));
            this.label1.Name = "label1";
            // 
            // cmdAdd
            // 
            resources.ApplyResources(this.cmdAdd, "cmdAdd");
            this.errorMessages.SetError(this.cmdAdd, resources.GetString("cmdAdd.Error"));
            this.errorMessages.SetIconAlignment(this.cmdAdd, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cmdAdd.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.cmdAdd, ((int)(resources.GetObject("cmdAdd.IconPadding"))));
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.UseVisualStyleBackColor = true;
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdUpdate
            // 
            resources.ApplyResources(this.cmdUpdate, "cmdUpdate");
            this.errorMessages.SetError(this.cmdUpdate, resources.GetString("cmdUpdate.Error"));
            this.errorMessages.SetIconAlignment(this.cmdUpdate, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cmdUpdate.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.cmdUpdate, ((int)(resources.GetObject("cmdUpdate.IconPadding"))));
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
            this.columnHeader5});
            this.errorMessages.SetError(this.lstServers, resources.GetString("lstServers.Error"));
            this.lstServers.FullRowSelect = true;
            this.lstServers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.errorMessages.SetIconAlignment(this.lstServers, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("lstServers.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.lstServers, ((int)(resources.GetObject("lstServers.IconPadding"))));
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
            // columnHeader5
            // 
            resources.ApplyResources(this.columnHeader5, "columnHeader5");
            // 
            // txServer
            // 
            resources.ApplyResources(this.txServer, "txServer");
            this.errorMessages.SetError(this.txServer, resources.GetString("txServer.Error"));
            this.errorMessages.SetIconAlignment(this.txServer, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txServer.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.txServer, ((int)(resources.GetObject("txServer.IconPadding"))));
            this.txServer.Name = "txServer";
            this.txServer.TextChanged += new System.EventHandler(this.UpdateGUI);
            // 
            // selPort
            // 
            resources.ApplyResources(this.selPort, "selPort");
            this.errorMessages.SetError(this.selPort, resources.GetString("selPort.Error"));
            this.errorMessages.SetIconAlignment(this.selPort, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("selPort.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.selPort, ((int)(resources.GetObject("selPort.IconPadding"))));
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
            this.errorMessages.SetError(this.selInterval, resources.GetString("selInterval.Error"));
            this.errorMessages.SetIconAlignment(this.selInterval, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("selInterval.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.selInterval, ((int)(resources.GetObject("selInterval.IconPadding"))));
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
            // cmdDelete
            // 
            resources.ApplyResources(this.cmdDelete, "cmdDelete");
            this.errorMessages.SetError(this.cmdDelete, resources.GetString("cmdDelete.Error"));
            this.errorMessages.SetIconAlignment(this.cmdDelete, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cmdDelete.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.cmdDelete, ((int)(resources.GetObject("cmdDelete.IconPadding"))));
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.errorMessages.SetError(this.label2, resources.GetString("label2.Error"));
            this.errorMessages.SetIconAlignment(this.label2, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label2.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label2, ((int)(resources.GetObject("label2.IconPadding"))));
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.errorMessages.SetError(this.label3, resources.GetString("label3.Error"));
            this.errorMessages.SetIconAlignment(this.label3, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label3.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label3, ((int)(resources.GetObject("label3.IconPadding"))));
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.errorMessages.SetError(this.label4, resources.GetString("label4.Error"));
            this.errorMessages.SetIconAlignment(this.label4, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label4.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label4, ((int)(resources.GetObject("label4.IconPadding"))));
            this.label4.Name = "label4";
            // 
            // frameLocal
            // 
            resources.ApplyResources(this.frameLocal, "frameLocal");
            this.frameLocal.Controls.Add(this.cmdUpdateAll);
            this.frameLocal.Controls.Add(this.selHibernate);
            this.frameLocal.Controls.Add(this.ckAutoStart);
            this.frameLocal.Controls.Add(this.ckHibernate);
            this.errorMessages.SetError(this.frameLocal, resources.GetString("frameLocal.Error"));
            this.errorMessages.SetIconAlignment(this.frameLocal, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("frameLocal.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.frameLocal, ((int)(resources.GetObject("frameLocal.IconPadding"))));
            this.frameLocal.Name = "frameLocal";
            this.frameLocal.TabStop = false;
            // 
            // cmdUpdateAll
            // 
            resources.ApplyResources(this.cmdUpdateAll, "cmdUpdateAll");
            this.errorMessages.SetError(this.cmdUpdateAll, resources.GetString("cmdUpdateAll.Error"));
            this.errorMessages.SetIconAlignment(this.cmdUpdateAll, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cmdUpdateAll.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.cmdUpdateAll, ((int)(resources.GetObject("cmdUpdateAll.IconPadding"))));
            this.cmdUpdateAll.Name = "cmdUpdateAll";
            this.cmdUpdateAll.UseVisualStyleBackColor = true;
            this.cmdUpdateAll.Click += new System.EventHandler(this.cmdUpdateAll_Click);
            // 
            // selHibernate
            // 
            resources.ApplyResources(this.selHibernate, "selHibernate");
            this.errorMessages.SetError(this.selHibernate, resources.GetString("selHibernate.Error"));
            this.errorMessages.SetIconAlignment(this.selHibernate, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("selHibernate.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.selHibernate, ((int)(resources.GetObject("selHibernate.IconPadding"))));
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
            this.errorMessages.SetError(this.ckAutoStart, resources.GetString("ckAutoStart.Error"));
            this.errorMessages.SetIconAlignment(this.ckAutoStart, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("ckAutoStart.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.ckAutoStart, ((int)(resources.GetObject("ckAutoStart.IconPadding"))));
            this.ckAutoStart.Name = "ckAutoStart";
            this.ckAutoStart.UseVisualStyleBackColor = true;
            this.ckAutoStart.CheckedChanged += new System.EventHandler(this.CheckLocal);
            // 
            // ckHibernate
            // 
            resources.ApplyResources(this.ckHibernate, "ckHibernate");
            this.errorMessages.SetError(this.ckHibernate, resources.GetString("ckHibernate.Error"));
            this.errorMessages.SetIconAlignment(this.ckHibernate, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("ckHibernate.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.ckHibernate, ((int)(resources.GetObject("ckHibernate.IconPadding"))));
            this.ckHibernate.Name = "ckHibernate";
            this.ckHibernate.UseVisualStyleBackColor = true;
            this.ckHibernate.CheckedChanged += new System.EventHandler(this.CheckLocal);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.errorMessages.SetError(this.label5, resources.GetString("label5.Error"));
            this.errorMessages.SetIconAlignment(this.label5, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label5.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label5, ((int)(resources.GetObject("label5.IconPadding"))));
            this.label5.Name = "label5";
            // 
            // selLanguage
            // 
            resources.ApplyResources(this.selLanguage, "selLanguage");
            this.selLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.errorMessages.SetError(this.selLanguage, resources.GetString("selLanguage.Error"));
            this.selLanguage.FormattingEnabled = true;
            this.errorMessages.SetIconAlignment(this.selLanguage, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("selLanguage.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.selLanguage, ((int)(resources.GetObject("selLanguage.IconPadding"))));
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
            resources.ApplyResources(this.grpStreaming, "grpStreaming");
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
            this.errorMessages.SetError(this.grpStreaming, resources.GetString("grpStreaming.Error"));
            this.errorMessages.SetIconAlignment(this.grpStreaming, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("grpStreaming.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.grpStreaming, ((int)(resources.GetObject("grpStreaming.IconPadding"))));
            this.grpStreaming.Name = "grpStreaming";
            this.grpStreaming.TabStop = false;
            // 
            // cmdStreaming
            // 
            resources.ApplyResources(this.cmdStreaming, "cmdStreaming");
            this.errorMessages.SetError(this.cmdStreaming, resources.GetString("cmdStreaming.Error"));
            this.errorMessages.SetIconAlignment(this.cmdStreaming, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cmdStreaming.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.cmdStreaming, ((int)(resources.GetObject("cmdStreaming.IconPadding"))));
            this.cmdStreaming.Name = "cmdStreaming";
            this.cmdStreaming.UseVisualStyleBackColor = true;
            this.cmdStreaming.Click += new System.EventHandler(this.cmdStreaming_Click);
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.errorMessages.SetError(this.label9, resources.GetString("label9.Error"));
            this.errorMessages.SetIconAlignment(this.label9, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label9.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label9, ((int)(resources.GetObject("label9.IconPadding"))));
            this.label9.Name = "label9";
            // 
            // txArgs
            // 
            resources.ApplyResources(this.txArgs, "txArgs");
            this.errorMessages.SetError(this.txArgs, resources.GetString("txArgs.Error"));
            this.errorMessages.SetIconAlignment(this.txArgs, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txArgs.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.txArgs, ((int)(resources.GetObject("txArgs.IconPadding"))));
            this.txArgs.Name = "txArgs";
            this.txArgs.Validating += new System.ComponentModel.CancelEventHandler(this.txArgs_Validating);
            // 
            // txMultiCast
            // 
            resources.ApplyResources(this.txMultiCast, "txMultiCast");
            this.errorMessages.SetError(this.txMultiCast, resources.GetString("txMultiCast.Error"));
            this.errorMessages.SetIconAlignment(this.txMultiCast, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txMultiCast.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.txMultiCast, ((int)(resources.GetObject("txMultiCast.IconPadding"))));
            this.txMultiCast.Name = "txMultiCast";
            this.txMultiCast.Validating += new System.ComponentModel.CancelEventHandler(this.txMultiCast_Validating);
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.errorMessages.SetError(this.label8, resources.GetString("label8.Error"));
            this.errorMessages.SetIconAlignment(this.label8, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label8.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label8, ((int)(resources.GetObject("label8.IconPadding"))));
            this.label8.Name = "label8";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.errorMessages.SetError(this.label7, resources.GetString("label7.Error"));
            this.errorMessages.SetIconAlignment(this.label7, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label7.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label7, ((int)(resources.GetObject("label7.IconPadding"))));
            this.label7.Name = "label7";
            // 
            // cmdAppChooser
            // 
            resources.ApplyResources(this.cmdAppChooser, "cmdAppChooser");
            this.errorMessages.SetError(this.cmdAppChooser, resources.GetString("cmdAppChooser.Error"));
            this.errorMessages.SetIconAlignment(this.cmdAppChooser, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cmdAppChooser.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.cmdAppChooser, ((int)(resources.GetObject("cmdAppChooser.IconPadding"))));
            this.cmdAppChooser.Name = "cmdAppChooser";
            this.cmdAppChooser.UseVisualStyleBackColor = true;
            this.cmdAppChooser.Click += new System.EventHandler(this.cmdAppChooser_Click);
            // 
            // txViewer
            // 
            resources.ApplyResources(this.txViewer, "txViewer");
            this.errorMessages.SetError(this.txViewer, resources.GetString("txViewer.Error"));
            this.errorMessages.SetIconAlignment(this.txViewer, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txViewer.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.txViewer, ((int)(resources.GetObject("txViewer.IconPadding"))));
            this.txViewer.Name = "txViewer";
            this.txViewer.ReadOnly = true;
            // 
            // selStreamPort
            // 
            resources.ApplyResources(this.selStreamPort, "selStreamPort");
            this.errorMessages.SetError(this.selStreamPort, resources.GetString("selStreamPort.Error"));
            this.errorMessages.SetIconAlignment(this.selStreamPort, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("selStreamPort.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.selStreamPort, ((int)(resources.GetObject("selStreamPort.IconPadding"))));
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
            this.errorMessages.SetError(this.label6, resources.GetString("label6.Error"));
            this.errorMessages.SetIconAlignment(this.label6, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label6.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label6, ((int)(resources.GetObject("label6.IconPadding"))));
            this.label6.Name = "label6";
            // 
            // errorMessages
            // 
            this.errorMessages.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.AlwaysBlink;
            this.errorMessages.ContainerControl = this;
            resources.ApplyResources(this.errorMessages, "errorMessages");
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.errorMessages.SetError(this.label10, resources.GetString("label10.Error"));
            this.errorMessages.SetIconAlignment(this.label10, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label10.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label10, ((int)(resources.GetObject("label10.IconPadding"))));
            this.label10.Name = "label10";
            // 
            // selDelay
            // 
            resources.ApplyResources(this.selDelay, "selDelay");
            this.errorMessages.SetError(this.selDelay, resources.GetString("selDelay.Error"));
            this.errorMessages.SetIconAlignment(this.selDelay, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("selDelay.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.selDelay, ((int)(resources.GetObject("selDelay.IconPadding"))));
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
            this.errorMessages.SetError(this.txSubNet, resources.GetString("txSubNet.Error"));
            this.errorMessages.SetIconAlignment(this.txSubNet, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txSubNet.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.txSubNet, ((int)(resources.GetObject("txSubNet.IconPadding"))));
            this.txSubNet.Name = "txSubNet";
            this.txSubNet.TextChanged += new System.EventHandler(this.UpdateGUI);
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.errorMessages.SetError(this.label11, resources.GetString("label11.Error"));
            this.errorMessages.SetIconAlignment(this.label11, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label11.IconAlignment"))));
            this.errorMessages.SetIconPadding(this.label11, ((int)(resources.GetObject("label11.IconPadding"))));
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
		private System.Windows.Forms.TextBox txServer;
		private System.Windows.Forms.NumericUpDown selPort;
        private System.Windows.Forms.NumericUpDown selInterval;
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

