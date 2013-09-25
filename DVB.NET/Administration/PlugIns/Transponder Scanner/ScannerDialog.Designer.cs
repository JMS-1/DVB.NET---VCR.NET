namespace JMS.DVB.Administration.SourceScanner
{
    partial class ScannerDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ScannerDialog ) );
            this.lbProfile = new System.Windows.Forms.Label();
            this.ckMerge = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.prgProgress = new System.Windows.Forms.ProgressBar();
            this.lbFound = new System.Windows.Forms.Label();
            this.tickEnd = new System.Windows.Forms.Timer( this.components );
            this.saveProtocol = new System.Windows.Forms.SaveFileDialog();
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
            // ckMerge
            // 
            this.ckMerge.AccessibleDescription = null;
            this.ckMerge.AccessibleName = null;
            resources.ApplyResources( this.ckMerge, "ckMerge" );
            this.ckMerge.BackgroundImage = null;
            this.ckMerge.Checked = true;
            this.ckMerge.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckMerge.Font = null;
            this.ckMerge.Name = "ckMerge";
            this.ckMerge.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources( this.label1, "label1" );
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // prgProgress
            // 
            this.prgProgress.AccessibleDescription = null;
            this.prgProgress.AccessibleName = null;
            resources.ApplyResources( this.prgProgress, "prgProgress" );
            this.prgProgress.BackgroundImage = null;
            this.prgProgress.Font = null;
            this.prgProgress.Maximum = 1000;
            this.prgProgress.Name = "prgProgress";
            // 
            // lbFound
            // 
            this.lbFound.AccessibleDescription = null;
            this.lbFound.AccessibleName = null;
            resources.ApplyResources( this.lbFound, "lbFound" );
            this.lbFound.Font = null;
            this.lbFound.Name = "lbFound";
            // 
            // tickEnd
            // 
            this.tickEnd.Tick += new System.EventHandler( this.tickEnd_Tick );
            // 
            // saveProtocol
            // 
            this.saveProtocol.DefaultExt = "csv";
            resources.ApplyResources( this.saveProtocol, "saveProtocol" );
            // 
            // ScannerDialog
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add( this.lbFound );
            this.Controls.Add( this.prgProgress );
            this.Controls.Add( this.label1 );
            this.Controls.Add( this.ckMerge );
            this.Controls.Add( this.lbProfile );
            this.Font = null;
            this.Name = "ScannerDialog";
            this.Load += new System.EventHandler( this.ScannerDialog_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbProfile;
        private System.Windows.Forms.CheckBox ckMerge;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar prgProgress;
        private System.Windows.Forms.Label lbFound;
        private System.Windows.Forms.Timer tickEnd;
        private System.Windows.Forms.SaveFileDialog saveProtocol;
    }
}
