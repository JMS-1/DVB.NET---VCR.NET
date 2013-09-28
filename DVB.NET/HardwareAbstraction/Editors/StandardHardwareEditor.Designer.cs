namespace JMS.DVB.Editors
{
    partial class StandardHardwareEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StandardHardwareEditor));
            this.label1 = new System.Windows.Forms.Label();
            this.selTuner = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.selCapture = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.selPATCount = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.selPATDelay = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.selDiSEqC = new System.Windows.Forms.ComboBox();
            this.selCICAM = new System.Windows.Forms.ComboBox();
            this.selSignal = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.selWakeup = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ckNoMoniker = new System.Windows.Forms.CheckBox();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.selDVBS2 = new System.Windows.Forms.ComboBox();
            this.ckWakeup = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ckCIDuringScan = new System.Windows.Forms.CheckBox();
            this.cmdEditSig = new System.Windows.Forms.Button();
            this.cmdEditCAM = new System.Windows.Forms.Button();
            this.cmdEditS2 = new System.Windows.Forms.Button();
            this.cmdEditDiSEqC = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.selPATCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selPATDelay)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.toolTips.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
            // 
            // selTuner
            // 
            resources.ApplyResources(this.selTuner, "selTuner");
            this.selTuner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selTuner.FormattingEnabled = true;
            this.selTuner.Name = "selTuner";
            this.selTuner.Sorted = true;
            this.toolTips.SetToolTip(this.selTuner, resources.GetString("selTuner.ToolTip"));
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            this.toolTips.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
            // 
            // selCapture
            // 
            resources.ApplyResources(this.selCapture, "selCapture");
            this.selCapture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selCapture.FormattingEnabled = true;
            this.selCapture.Name = "selCapture";
            this.selCapture.Sorted = true;
            this.toolTips.SetToolTip(this.selCapture, resources.GetString("selCapture.ToolTip"));
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.toolTips.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
            // 
            // selPATCount
            // 
            resources.ApplyResources(this.selPATCount, "selPATCount");
            this.selPATCount.Name = "selPATCount";
            this.toolTips.SetToolTip(this.selPATCount, resources.GetString("selPATCount.ToolTip"));
            this.selPATCount.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            this.toolTips.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
            // 
            // selPATDelay
            // 
            resources.ApplyResources(this.selPATDelay, "selPATDelay");
            this.selPATDelay.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.selPATDelay.Name = "selPATDelay";
            this.toolTips.SetToolTip(this.selPATDelay, resources.GetString("selPATDelay.ToolTip"));
            this.selPATDelay.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            this.toolTips.SetToolTip(this.label5, resources.GetString("label5.ToolTip"));
            // 
            // selDiSEqC
            // 
            resources.ApplyResources(this.selDiSEqC, "selDiSEqC");
            this.selDiSEqC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selDiSEqC.FormattingEnabled = true;
            this.selDiSEqC.Name = "selDiSEqC";
            this.selDiSEqC.Sorted = true;
            this.toolTips.SetToolTip(this.selDiSEqC, resources.GetString("selDiSEqC.ToolTip"));
            this.selDiSEqC.SelectionChangeCommitted += new System.EventHandler(this.PipelineSelectionChanged);
            // 
            // selCICAM
            // 
            resources.ApplyResources(this.selCICAM, "selCICAM");
            this.selCICAM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selCICAM.FormattingEnabled = true;
            this.selCICAM.Name = "selCICAM";
            this.selCICAM.Sorted = true;
            this.toolTips.SetToolTip(this.selCICAM, resources.GetString("selCICAM.ToolTip"));
            this.selCICAM.SelectionChangeCommitted += new System.EventHandler(this.PipelineSelectionChanged);
            // 
            // selSignal
            // 
            resources.ApplyResources(this.selSignal, "selSignal");
            this.selSignal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selSignal.FormattingEnabled = true;
            this.selSignal.Name = "selSignal";
            this.selSignal.Sorted = true;
            this.toolTips.SetToolTip(this.selSignal, resources.GetString("selSignal.ToolTip"));
            this.selSignal.SelectionChangeCommitted += new System.EventHandler(this.PipelineSelectionChanged);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            this.toolTips.SetToolTip(this.label6, resources.GetString("label6.ToolTip"));
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            this.toolTips.SetToolTip(this.label7, resources.GetString("label7.ToolTip"));
            // 
            // selWakeup
            // 
            resources.ApplyResources(this.selWakeup, "selWakeup");
            this.selWakeup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selWakeup.FormattingEnabled = true;
            this.selWakeup.Name = "selWakeup";
            this.selWakeup.Sorted = true;
            this.toolTips.SetToolTip(this.selWakeup, resources.GetString("selWakeup.ToolTip"));
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            this.toolTips.SetToolTip(this.label8, resources.GetString("label8.ToolTip"));
            // 
            // ckNoMoniker
            // 
            resources.ApplyResources(this.ckNoMoniker, "ckNoMoniker");
            this.ckNoMoniker.Name = "ckNoMoniker";
            this.toolTips.SetToolTip(this.ckNoMoniker, resources.GetString("ckNoMoniker.ToolTip"));
            this.ckNoMoniker.UseVisualStyleBackColor = true;
            // 
            // selDVBS2
            // 
            resources.ApplyResources(this.selDVBS2, "selDVBS2");
            this.selDVBS2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selDVBS2.FormattingEnabled = true;
            this.selDVBS2.Name = "selDVBS2";
            this.selDVBS2.Sorted = true;
            this.toolTips.SetToolTip(this.selDVBS2, resources.GetString("selDVBS2.ToolTip"));
            this.selDVBS2.SelectionChangeCommitted += new System.EventHandler(this.PipelineSelectionChanged);
            // 
            // ckWakeup
            // 
            resources.ApplyResources(this.ckWakeup, "ckWakeup");
            this.ckWakeup.Name = "ckWakeup";
            this.toolTips.SetToolTip(this.ckWakeup, resources.GetString("ckWakeup.ToolTip"));
            this.ckWakeup.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.ckCIDuringScan);
            this.groupBox1.Controls.Add(this.cmdEditSig);
            this.groupBox1.Controls.Add(this.cmdEditCAM);
            this.groupBox1.Controls.Add(this.cmdEditS2);
            this.groupBox1.Controls.Add(this.cmdEditDiSEqC);
            this.groupBox1.Controls.Add(this.ckWakeup);
            this.groupBox1.Controls.Add(this.selDVBS2);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.ckNoMoniker);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.selWakeup);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.selSignal);
            this.groupBox1.Controls.Add(this.selCICAM);
            this.groupBox1.Controls.Add(this.selDiSEqC);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.selPATDelay);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.selPATCount);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.selCapture);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.selTuner);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            this.toolTips.SetToolTip(this.groupBox1, resources.GetString("groupBox1.ToolTip"));
            // 
            // ckCIDuringScan
            // 
            resources.ApplyResources(this.ckCIDuringScan, "ckCIDuringScan");
            this.ckCIDuringScan.Name = "ckCIDuringScan";
            this.toolTips.SetToolTip(this.ckCIDuringScan, resources.GetString("ckCIDuringScan.ToolTip"));
            this.ckCIDuringScan.UseVisualStyleBackColor = true;
            // 
            // cmdEditSig
            // 
            resources.ApplyResources(this.cmdEditSig, "cmdEditSig");
            this.cmdEditSig.Name = "cmdEditSig";
            this.toolTips.SetToolTip(this.cmdEditSig, resources.GetString("cmdEditSig.ToolTip"));
            this.cmdEditSig.UseVisualStyleBackColor = true;
            this.cmdEditSig.Click += new System.EventHandler(this.PipelineParameterEdit);
            // 
            // cmdEditCAM
            // 
            resources.ApplyResources(this.cmdEditCAM, "cmdEditCAM");
            this.cmdEditCAM.Name = "cmdEditCAM";
            this.toolTips.SetToolTip(this.cmdEditCAM, resources.GetString("cmdEditCAM.ToolTip"));
            this.cmdEditCAM.UseVisualStyleBackColor = true;
            this.cmdEditCAM.Click += new System.EventHandler(this.PipelineParameterEdit);
            // 
            // cmdEditS2
            // 
            resources.ApplyResources(this.cmdEditS2, "cmdEditS2");
            this.cmdEditS2.Name = "cmdEditS2";
            this.toolTips.SetToolTip(this.cmdEditS2, resources.GetString("cmdEditS2.ToolTip"));
            this.cmdEditS2.UseVisualStyleBackColor = true;
            this.cmdEditS2.Click += new System.EventHandler(this.PipelineParameterEdit);
            // 
            // cmdEditDiSEqC
            // 
            resources.ApplyResources(this.cmdEditDiSEqC, "cmdEditDiSEqC");
            this.cmdEditDiSEqC.Name = "cmdEditDiSEqC";
            this.toolTips.SetToolTip(this.cmdEditDiSEqC, resources.GetString("cmdEditDiSEqC.ToolTip"));
            this.cmdEditDiSEqC.UseVisualStyleBackColor = true;
            this.cmdEditDiSEqC.Click += new System.EventHandler(this.PipelineParameterEdit);
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            this.toolTips.SetToolTip(this.label9, resources.GetString("label9.ToolTip"));
            // 
            // StandardHardwareEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "StandardHardwareEditor";
            this.toolTips.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Load += new System.EventHandler(this.StandardHardwareEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.selPATCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selPATDelay)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox selTuner;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox selCapture;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown selPATCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown selPATDelay;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox selDiSEqC;
        private System.Windows.Forms.ComboBox selCICAM;
        private System.Windows.Forms.ComboBox selSignal;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox selWakeup;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox ckNoMoniker;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox selDVBS2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox ckWakeup;
        private System.Windows.Forms.Button cmdEditSig;
        private System.Windows.Forms.Button cmdEditCAM;
        private System.Windows.Forms.Button cmdEditS2;
        private System.Windows.Forms.Button cmdEditDiSEqC;
        private System.Windows.Forms.CheckBox ckCIDuringScan;
    }
}
