namespace JMS.DVB.Administration.ProfileManager
{
    partial class RecordingSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecordingSettings));
            this.ckGroup = new System.Windows.Forms.CheckBox();
            this.udGroup = new System.Windows.Forms.NumericUpDown();
            this.ckDecrypt = new System.Windows.Forms.CheckBox();
            this.udDecrypt = new System.Windows.Forms.NumericUpDown();
            this.ckSource = new System.Windows.Forms.CheckBox();
            this.udSource = new System.Windows.Forms.NumericUpDown();
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.ckSourceLimit = new System.Windows.Forms.CheckBox();
            this.udSourceLimit = new System.Windows.Forms.NumericUpDown();
            this.ckDecLimit = new System.Windows.Forms.CheckBox();
            this.udDecLimit = new System.Windows.Forms.NumericUpDown();
            this.ckPrio = new System.Windows.Forms.CheckBox();
            this.udPrio = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.udGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDecrypt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udSourceLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDecLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udPrio)).BeginInit();
            this.SuspendLayout();
            // 
            // ckGroup
            // 
            resources.ApplyResources(this.ckGroup, "ckGroup");
            this.ckGroup.Checked = true;
            this.ckGroup.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.ckGroup.Name = "ckGroup";
            this.ckGroup.ThreeState = true;
            this.ckGroup.UseVisualStyleBackColor = true;
            this.ckGroup.CheckStateChanged += new System.EventHandler(this.CheckStateChanged);
            // 
            // udGroup
            // 
            resources.ApplyResources(this.udGroup, "udGroup");
            this.udGroup.Maximum = new decimal(new int[] {
            7200,
            0,
            0,
            0});
            this.udGroup.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udGroup.Name = "udGroup";
            this.udGroup.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // ckDecrypt
            // 
            resources.ApplyResources(this.ckDecrypt, "ckDecrypt");
            this.ckDecrypt.Checked = true;
            this.ckDecrypt.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.ckDecrypt.Name = "ckDecrypt";
            this.ckDecrypt.ThreeState = true;
            this.ckDecrypt.UseVisualStyleBackColor = true;
            this.ckDecrypt.CheckStateChanged += new System.EventHandler(this.CheckStateChanged);
            // 
            // udDecrypt
            // 
            resources.ApplyResources(this.udDecrypt, "udDecrypt");
            this.udDecrypt.Maximum = new decimal(new int[] {
            7200,
            0,
            0,
            0});
            this.udDecrypt.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udDecrypt.Name = "udDecrypt";
            this.udDecrypt.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // ckSource
            // 
            resources.ApplyResources(this.ckSource, "ckSource");
            this.ckSource.Checked = true;
            this.ckSource.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.ckSource.Name = "ckSource";
            this.ckSource.ThreeState = true;
            this.ckSource.UseVisualStyleBackColor = true;
            this.ckSource.CheckStateChanged += new System.EventHandler(this.CheckStateChanged);
            // 
            // udSource
            // 
            resources.ApplyResources(this.udSource, "udSource");
            this.udSource.Maximum = new decimal(new int[] {
            7200,
            0,
            0,
            0});
            this.udSource.Name = "udSource";
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
            // ckSourceLimit
            // 
            resources.ApplyResources(this.ckSourceLimit, "ckSourceLimit");
            this.ckSourceLimit.Checked = true;
            this.ckSourceLimit.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.ckSourceLimit.Name = "ckSourceLimit";
            this.ckSourceLimit.ThreeState = true;
            this.ckSourceLimit.UseVisualStyleBackColor = true;
            this.ckSourceLimit.CheckStateChanged += new System.EventHandler(this.CheckStateChanged);
            // 
            // udSourceLimit
            // 
            resources.ApplyResources(this.udSourceLimit, "udSourceLimit");
            this.udSourceLimit.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.udSourceLimit.Name = "udSourceLimit";
            // 
            // ckDecLimit
            // 
            resources.ApplyResources(this.ckDecLimit, "ckDecLimit");
            this.ckDecLimit.Checked = true;
            this.ckDecLimit.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.ckDecLimit.Name = "ckDecLimit";
            this.ckDecLimit.ThreeState = true;
            this.ckDecLimit.UseVisualStyleBackColor = true;
            this.ckDecLimit.CheckStateChanged += new System.EventHandler(this.CheckStateChanged);
            // 
            // udDecLimit
            // 
            resources.ApplyResources(this.udDecLimit, "udDecLimit");
            this.udDecLimit.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.udDecLimit.Name = "udDecLimit";
            // 
            // ckPrio
            // 
            resources.ApplyResources(this.ckPrio, "ckPrio");
            this.ckPrio.Checked = true;
            this.ckPrio.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.ckPrio.Name = "ckPrio";
            this.ckPrio.ThreeState = true;
            this.ckPrio.UseVisualStyleBackColor = true;
            this.ckPrio.CheckStateChanged += new System.EventHandler(this.CheckStateChanged);
            // 
            // udPrio
            // 
            resources.ApplyResources(this.udPrio, "udPrio");
            this.udPrio.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.udPrio.Name = "udPrio";
            // 
            // RecordingSettings
            // 
            this.AcceptButton = this.cmdSave;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdSave);
            this.Controls.Add(this.udPrio);
            this.Controls.Add(this.udDecLimit);
            this.Controls.Add(this.udSourceLimit);
            this.Controls.Add(this.udSource);
            this.Controls.Add(this.udDecrypt);
            this.Controls.Add(this.ckPrio);
            this.Controls.Add(this.udGroup);
            this.Controls.Add(this.ckDecLimit);
            this.Controls.Add(this.ckSourceLimit);
            this.Controls.Add(this.ckSource);
            this.Controls.Add(this.ckDecrypt);
            this.Controls.Add(this.ckGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "RecordingSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.udGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDecrypt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udSourceLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDecLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udPrio)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox ckGroup;
        private System.Windows.Forms.NumericUpDown udGroup;
        private System.Windows.Forms.CheckBox ckDecrypt;
        private System.Windows.Forms.NumericUpDown udDecrypt;
        private System.Windows.Forms.CheckBox ckSource;
        private System.Windows.Forms.NumericUpDown udSource;
        private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.CheckBox ckSourceLimit;
        private System.Windows.Forms.NumericUpDown udSourceLimit;
        private System.Windows.Forms.CheckBox ckDecLimit;
        private System.Windows.Forms.NumericUpDown udDecLimit;
        private System.Windows.Forms.CheckBox ckPrio;
        private System.Windows.Forms.NumericUpDown udPrio;
    }
}