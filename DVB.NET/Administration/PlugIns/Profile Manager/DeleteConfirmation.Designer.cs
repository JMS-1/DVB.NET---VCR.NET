namespace JMS.DVB.Administration.ProfileManager
{
    partial class DeleteConfirmation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( DeleteConfirmation ) );
            this.lbProfile = new System.Windows.Forms.Label();
            this.lbInUse = new System.Windows.Forms.Label();
            this.ckBackup = new System.Windows.Forms.CheckBox();
            this.lbBackup = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbProfile
            // 
            this.lbProfile.AccessibleDescription = null;
            this.lbProfile.AccessibleName = null;
            resources.ApplyResources( this.lbProfile, "lbProfile" );
            this.lbProfile.Font = null;
            this.lbProfile.Name = "lbProfile";
            // 
            // lbInUse
            // 
            this.lbInUse.AccessibleDescription = null;
            this.lbInUse.AccessibleName = null;
            resources.ApplyResources( this.lbInUse, "lbInUse" );
            this.lbInUse.ForeColor = System.Drawing.Color.Red;
            this.lbInUse.Name = "lbInUse";
            // 
            // ckBackup
            // 
            this.ckBackup.AccessibleDescription = null;
            this.ckBackup.AccessibleName = null;
            resources.ApplyResources( this.ckBackup, "ckBackup" );
            this.ckBackup.BackgroundImage = null;
            this.ckBackup.Checked = true;
            this.ckBackup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckBackup.Font = null;
            this.ckBackup.Name = "ckBackup";
            this.ckBackup.UseVisualStyleBackColor = true;
            // 
            // lbBackup
            // 
            this.lbBackup.AccessibleDescription = null;
            this.lbBackup.AccessibleName = null;
            resources.ApplyResources( this.lbBackup, "lbBackup" );
            this.lbBackup.Font = null;
            this.lbBackup.Name = "lbBackup";
            // 
            // DeleteConfirmation
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add( this.ckBackup );
            this.Controls.Add( this.lbBackup );
            this.Controls.Add( this.lbInUse );
            this.Controls.Add( this.lbProfile );
            this.Font = null;
            this.Name = "DeleteConfirmation";
            this.Load += new System.EventHandler( this.DeleteConfirmation_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbProfile;
        private System.Windows.Forms.Label lbInUse;
        private System.Windows.Forms.CheckBox ckBackup;
        private System.Windows.Forms.Label lbBackup;
    }
}
