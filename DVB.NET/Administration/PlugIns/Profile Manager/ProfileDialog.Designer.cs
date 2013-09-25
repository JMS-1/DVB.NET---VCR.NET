namespace JMS.DVB.Administration.ProfileManager
{
    partial class ProfileDialog
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
            // Cleanup
            if (null != m_PlugIn)
                m_PlugIn.CurrentEditor = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txName = new System.Windows.Forms.TextBox();
            this.selType = new System.Windows.Forms.ComboBox();
            this.pnlEditor = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.selShare = new System.Windows.Forms.ComboBox();
            this.ckDVBS2 = new System.Windows.Forms.CheckBox();
            this.ckDisableEPG = new System.Windows.Forms.CheckBox();
            this.cmdRecordings = new System.Windows.Forms.Button();
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
            // txName
            // 
            resources.ApplyResources(this.txName, "txName");
            this.txName.Name = "txName";
            this.txName.TextChanged += new System.EventHandler(this.txName_TextChanged);
            // 
            // selType
            // 
            resources.ApplyResources(this.selType, "selType");
            this.selType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selType.FormattingEnabled = true;
            this.selType.Name = "selType";
            this.selType.SelectedIndexChanged += new System.EventHandler(this.selType_SelectedIndexChanged);
            this.selType.SelectionChangeCommitted += new System.EventHandler(this.selType_SelectionChangeCommitted);
            // 
            // pnlEditor
            // 
            resources.ApplyResources(this.pnlEditor, "pnlEditor");
            this.pnlEditor.Name = "pnlEditor";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // selShare
            // 
            resources.ApplyResources(this.selShare, "selShare");
            this.selShare.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selShare.FormattingEnabled = true;
            this.selShare.Name = "selShare";
            this.selShare.SelectionChangeCommitted += new System.EventHandler(this.txName_TextChanged);
            // 
            // ckDVBS2
            // 
            resources.ApplyResources(this.ckDVBS2, "ckDVBS2");
            this.ckDVBS2.Name = "ckDVBS2";
            this.ckDVBS2.UseVisualStyleBackColor = true;
            // 
            // ckDisableEPG
            // 
            resources.ApplyResources(this.ckDisableEPG, "ckDisableEPG");
            this.ckDisableEPG.Name = "ckDisableEPG";
            this.ckDisableEPG.UseVisualStyleBackColor = true;
            // 
            // cmdRecordings
            // 
            resources.ApplyResources(this.cmdRecordings, "cmdRecordings");
            this.cmdRecordings.Name = "cmdRecordings";
            this.cmdRecordings.UseVisualStyleBackColor = true;
            this.cmdRecordings.Click += new System.EventHandler(this.cmdRecordings_Click);
            // 
            // ProfileDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmdRecordings);
            this.Controls.Add(this.ckDisableEPG);
            this.Controls.Add(this.ckDVBS2);
            this.Controls.Add(this.selShare);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pnlEditor);
            this.Controls.Add(this.selType);
            this.Controls.Add(this.txName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ProfileDialog";
            this.Load += new System.EventHandler(this.ProfileDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txName;
        private System.Windows.Forms.ComboBox selType;
        private System.Windows.Forms.Panel pnlEditor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox selShare;
        private System.Windows.Forms.CheckBox ckDVBS2;
        private System.Windows.Forms.CheckBox ckDisableEPG;
        private System.Windows.Forms.Button cmdRecordings;
    }
}
