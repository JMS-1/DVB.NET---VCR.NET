namespace JMS.DVB.Provider.Duoflex
{
    partial class AdditionalFilterSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdditionalFilterSelector));
            this.selFilter = new System.Windows.Forms.ComboBox();
            this.cmdAccept = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.ckDisplayOnly = new System.Windows.Forms.CheckBox();
            this.ckReset = new System.Windows.Forms.CheckBox();
            this.ckResetTune = new System.Windows.Forms.CheckBox();
            this.ckDisableOnChange = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.udChangeDelay = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.udChangeDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // selFilter
            // 
            resources.ApplyResources(this.selFilter, "selFilter");
            this.selFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selFilter.FormattingEnabled = true;
            this.selFilter.Name = "selFilter";
            this.selFilter.Sorted = true;
            // 
            // cmdAccept
            // 
            resources.ApplyResources(this.cmdAccept, "cmdAccept");
            this.cmdAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdAccept.Name = "cmdAccept";
            this.cmdAccept.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            resources.ApplyResources(this.cmdCancel, "cmdCancel");
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // ckDisplayOnly
            // 
            resources.ApplyResources(this.ckDisplayOnly, "ckDisplayOnly");
            this.ckDisplayOnly.Name = "ckDisplayOnly";
            this.ckDisplayOnly.UseVisualStyleBackColor = true;
            // 
            // ckReset
            // 
            resources.ApplyResources(this.ckReset, "ckReset");
            this.ckReset.Name = "ckReset";
            this.ckReset.UseVisualStyleBackColor = true;
            this.ckReset.CheckedChanged += new System.EventHandler(this.ckReset_CheckedChanged);
            // 
            // ckResetTune
            // 
            resources.ApplyResources(this.ckResetTune, "ckResetTune");
            this.ckResetTune.Checked = true;
            this.ckResetTune.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckResetTune.Name = "ckResetTune";
            this.ckResetTune.UseVisualStyleBackColor = true;
            // 
            // ckDisableOnChange
            // 
            resources.ApplyResources(this.ckDisableOnChange, "ckDisableOnChange");
            this.ckDisableOnChange.Name = "ckDisableOnChange";
            this.ckDisableOnChange.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // udChangeDelay
            // 
            resources.ApplyResources(this.udChangeDelay, "udChangeDelay");
            this.udChangeDelay.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.udChangeDelay.Name = "udChangeDelay";
            // 
            // AdditionalFilterSelector
            // 
            this.AcceptButton = this.cmdAccept;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.Controls.Add(this.udChangeDelay);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ckDisableOnChange);
            this.Controls.Add(this.ckResetTune);
            this.Controls.Add(this.ckReset);
            this.Controls.Add(this.ckDisplayOnly);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdAccept);
            this.Controls.Add(this.selFilter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdditionalFilterSelector";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.udChangeDelay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox selFilter;
        private System.Windows.Forms.Button cmdAccept;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.CheckBox ckDisplayOnly;
        private System.Windows.Forms.CheckBox ckReset;
        private System.Windows.Forms.CheckBox ckResetTune;
        private System.Windows.Forms.CheckBox ckDisableOnChange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown udChangeDelay;
    }
}