namespace DVBNETViewer.Dialogs
{
	partial class ProgramSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgramSettings));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.selPrio = new System.Windows.Forms.ComboBox();
            this.selType = new System.Windows.Forms.ComboBox();
            this.selEnc = new System.Windows.Forms.ComboBox();
            this.selOSD = new System.Windows.Forms.NumericUpDown();
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.selPort = new System.Windows.Forms.NumericUpDown();
            this.txMCast = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txURL = new System.Windows.Forms.TextBox();
            this.ckCyberlink = new System.Windows.Forms.CheckBox();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.ckRemote = new System.Windows.Forms.CheckBox();
            this.ckHideCursor = new System.Windows.Forms.CheckBox();
            this.tabVideo = new System.Windows.Forms.TabPage();
            this.selHue = new System.Windows.Forms.TrackBar();
            this.selBrightness = new System.Windows.Forms.TrackBar();
            this.label11 = new System.Windows.Forms.Label();
            this.selSaturation = new System.Windows.Forms.TrackBar();
            this.label10 = new System.Windows.Forms.Label();
            this.selContrast = new System.Windows.Forms.TrackBar();
            this.label9 = new System.Windows.Forms.Label();
            this.ckOverwrite = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tabFilter = new System.Windows.Forms.TabPage();
            this.selH264 = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.selAC3 = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.selMP2 = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.selMPEG2 = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.selOSD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selPort)).BeginInit();
            this.tabs.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabVideo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selContrast)).BeginInit();
            this.tabFilter.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
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
            // selPrio
            // 
            resources.ApplyResources(this.selPrio, "selPrio");
            this.selPrio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selPrio.FormattingEnabled = true;
            this.selPrio.Items.AddRange(new object[] {
            resources.GetString("selPrio.Items"),
            resources.GetString("selPrio.Items1"),
            resources.GetString("selPrio.Items2"),
            resources.GetString("selPrio.Items3"),
            resources.GetString("selPrio.Items4"),
            resources.GetString("selPrio.Items5")});
            this.selPrio.Name = "selPrio";
            // 
            // selType
            // 
            resources.ApplyResources(this.selType, "selType");
            this.selType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selType.FormattingEnabled = true;
            this.selType.Items.AddRange(new object[] {
            resources.GetString("selType.Items"),
            resources.GetString("selType.Items1"),
            resources.GetString("selType.Items2")});
            this.selType.Name = "selType";
            // 
            // selEnc
            // 
            resources.ApplyResources(this.selEnc, "selEnc");
            this.selEnc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selEnc.FormattingEnabled = true;
            this.selEnc.Items.AddRange(new object[] {
            resources.GetString("selEnc.Items"),
            resources.GetString("selEnc.Items1"),
            resources.GetString("selEnc.Items2")});
            this.selEnc.Name = "selEnc";
            // 
            // selOSD
            // 
            resources.ApplyResources(this.selOSD, "selOSD");
            this.selOSD.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.selOSD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.selOSD.Name = "selOSD";
            this.selOSD.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cmdSave
            // 
            resources.ApplyResources(this.cmdSave, "cmdSave");
            this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.UseVisualStyleBackColor = true;
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // cmdCancel
            // 
            resources.ApplyResources(this.cmdCancel, "cmdCancel");
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // selPort
            // 
            resources.ApplyResources(this.selPort, "selPort");
            this.selPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.selPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.selPort.Name = "selPort";
            this.selPort.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // txMCast
            // 
            resources.ApplyResources(this.txMCast, "txMCast");
            this.txMCast.Name = "txMCast";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // txURL
            // 
            resources.ApplyResources(this.txURL, "txURL");
            this.txURL.Name = "txURL";
            // 
            // ckCyberlink
            // 
            resources.ApplyResources(this.ckCyberlink, "ckCyberlink");
            this.ckCyberlink.Name = "ckCyberlink";
            this.ckCyberlink.UseVisualStyleBackColor = true;
            // 
            // tabs
            // 
            resources.ApplyResources(this.tabs, "tabs");
            this.tabs.Controls.Add(this.tabGeneral);
            this.tabs.Controls.Add(this.tabVideo);
            this.tabs.Controls.Add(this.tabFilter);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            // 
            // tabGeneral
            // 
            resources.ApplyResources(this.tabGeneral, "tabGeneral");
            this.tabGeneral.BackColor = System.Drawing.SystemColors.Control;
            this.tabGeneral.Controls.Add(this.ckRemote);
            this.tabGeneral.Controls.Add(this.ckHideCursor);
            this.tabGeneral.Controls.Add(this.label1);
            this.tabGeneral.Controls.Add(this.selPort);
            this.tabGeneral.Controls.Add(this.ckCyberlink);
            this.tabGeneral.Controls.Add(this.label6);
            this.tabGeneral.Controls.Add(this.label7);
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.label5);
            this.tabGeneral.Controls.Add(this.selOSD);
            this.tabGeneral.Controls.Add(this.txURL);
            this.tabGeneral.Controls.Add(this.label4);
            this.tabGeneral.Controls.Add(this.selPrio);
            this.tabGeneral.Controls.Add(this.selEnc);
            this.tabGeneral.Controls.Add(this.label3);
            this.tabGeneral.Controls.Add(this.txMCast);
            this.tabGeneral.Controls.Add(this.selType);
            this.tabGeneral.Name = "tabGeneral";
            // 
            // ckRemote
            // 
            resources.ApplyResources(this.ckRemote, "ckRemote");
            this.ckRemote.Name = "ckRemote";
            this.ckRemote.UseVisualStyleBackColor = true;
            // 
            // ckHideCursor
            // 
            resources.ApplyResources(this.ckHideCursor, "ckHideCursor");
            this.ckHideCursor.Name = "ckHideCursor";
            this.ckHideCursor.UseVisualStyleBackColor = true;
            // 
            // tabVideo
            // 
            resources.ApplyResources(this.tabVideo, "tabVideo");
            this.tabVideo.BackColor = System.Drawing.SystemColors.Control;
            this.tabVideo.Controls.Add(this.selHue);
            this.tabVideo.Controls.Add(this.selBrightness);
            this.tabVideo.Controls.Add(this.label11);
            this.tabVideo.Controls.Add(this.selSaturation);
            this.tabVideo.Controls.Add(this.label10);
            this.tabVideo.Controls.Add(this.selContrast);
            this.tabVideo.Controls.Add(this.label9);
            this.tabVideo.Controls.Add(this.ckOverwrite);
            this.tabVideo.Controls.Add(this.label8);
            this.tabVideo.Name = "tabVideo";
            // 
            // selHue
            // 
            resources.ApplyResources(this.selHue, "selHue");
            this.selHue.Maximum = 100;
            this.selHue.Minimum = -100;
            this.selHue.Name = "selHue";
            this.selHue.TickFrequency = 10;
            this.selHue.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.selHue.ValueChanged += new System.EventHandler(this.selHue_ValueChanged);
            // 
            // selBrightness
            // 
            resources.ApplyResources(this.selBrightness, "selBrightness");
            this.selBrightness.Maximum = 100;
            this.selBrightness.Minimum = -100;
            this.selBrightness.Name = "selBrightness";
            this.selBrightness.TickFrequency = 10;
            this.selBrightness.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.selBrightness.ValueChanged += new System.EventHandler(this.selBrightness_ValueChanged);
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // selSaturation
            // 
            resources.ApplyResources(this.selSaturation, "selSaturation");
            this.selSaturation.Maximum = 100;
            this.selSaturation.Minimum = -100;
            this.selSaturation.Name = "selSaturation";
            this.selSaturation.TickFrequency = 10;
            this.selSaturation.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.selSaturation.ValueChanged += new System.EventHandler(this.selSaturation_ValueChanged);
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // selContrast
            // 
            resources.ApplyResources(this.selContrast, "selContrast");
            this.selContrast.Maximum = 100;
            this.selContrast.Minimum = -100;
            this.selContrast.Name = "selContrast";
            this.selContrast.TickFrequency = 10;
            this.selContrast.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.selContrast.ValueChanged += new System.EventHandler(this.selContrast_ValueChanged);
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // ckOverwrite
            // 
            resources.ApplyResources(this.ckOverwrite, "ckOverwrite");
            this.ckOverwrite.Name = "ckOverwrite";
            this.ckOverwrite.UseVisualStyleBackColor = true;
            this.ckOverwrite.CheckedChanged += new System.EventHandler(this.ckOverwrite_CheckedChanged);
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // tabFilter
            // 
            resources.ApplyResources(this.tabFilter, "tabFilter");
            this.tabFilter.BackColor = System.Drawing.SystemColors.Control;
            this.tabFilter.Controls.Add(this.selH264);
            this.tabFilter.Controls.Add(this.label15);
            this.tabFilter.Controls.Add(this.selAC3);
            this.tabFilter.Controls.Add(this.label14);
            this.tabFilter.Controls.Add(this.selMP2);
            this.tabFilter.Controls.Add(this.label13);
            this.tabFilter.Controls.Add(this.selMPEG2);
            this.tabFilter.Controls.Add(this.label12);
            this.tabFilter.Name = "tabFilter";
            // 
            // selH264
            // 
            resources.ApplyResources(this.selH264, "selH264");
            this.selH264.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selH264.FormattingEnabled = true;
            this.selH264.Name = "selH264";
            this.selH264.Sorted = true;
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // selAC3
            // 
            resources.ApplyResources(this.selAC3, "selAC3");
            this.selAC3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selAC3.FormattingEnabled = true;
            this.selAC3.Name = "selAC3";
            this.selAC3.Sorted = true;
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // selMP2
            // 
            resources.ApplyResources(this.selMP2, "selMP2");
            this.selMP2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selMP2.FormattingEnabled = true;
            this.selMP2.Name = "selMP2";
            this.selMP2.Sorted = true;
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // selMPEG2
            // 
            resources.ApplyResources(this.selMPEG2, "selMPEG2");
            this.selMPEG2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selMPEG2.FormattingEnabled = true;
            this.selMPEG2.Name = "selMPEG2";
            this.selMPEG2.Sorted = true;
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // ProgramSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgramSettings";
            this.ShowIcon = false;
            this.Load += new System.EventHandler(this.ProgramSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.selOSD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selPort)).EndInit();
            this.tabs.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabVideo.ResumeLayout(false);
            this.tabVideo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selContrast)).EndInit();
            this.tabFilter.ResumeLayout(false);
            this.tabFilter.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox selPrio;
		private System.Windows.Forms.ComboBox selType;
		private System.Windows.Forms.ComboBox selEnc;
		private System.Windows.Forms.NumericUpDown selOSD;
		private System.Windows.Forms.Button cmdSave;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown selPort;
		private System.Windows.Forms.TextBox txMCast;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox txURL;
		private System.Windows.Forms.CheckBox ckCyberlink;
		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabVideo;
		private System.Windows.Forms.CheckBox ckOverwrite;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TrackBar selHue;
		private System.Windows.Forms.TrackBar selBrightness;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TrackBar selSaturation;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TrackBar selContrast;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TabPage tabFilter;
		private System.Windows.Forms.ComboBox selH264;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.ComboBox selAC3;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.ComboBox selMP2;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.ComboBox selMPEG2;
		private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox ckHideCursor;
        private System.Windows.Forms.CheckBox ckRemote;
	}
}