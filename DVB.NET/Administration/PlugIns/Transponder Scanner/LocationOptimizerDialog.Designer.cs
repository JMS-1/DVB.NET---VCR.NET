namespace JMS.DVB.Administration.SourceScanner
{
    partial class LocationOptimizerDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( LocationOptimizerDialog ) );
            this.label1 = new System.Windows.Forms.Label();
            this.ticker = new System.Windows.Forms.Timer( this.components );
            this.lbFound = new System.Windows.Forms.Label();
            this.prgProgress = new System.Windows.Forms.ProgressBar();
            this.dlgSave = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources( this.label1, "label1" );
            this.label1.Name = "label1";
            // 
            // ticker
            // 
            this.ticker.Tick += new System.EventHandler( this.ticker_Tick );
            // 
            // lbFound
            // 
            resources.ApplyResources( this.lbFound, "lbFound" );
            this.lbFound.Name = "lbFound";
            // 
            // prgProgress
            // 
            resources.ApplyResources( this.prgProgress, "prgProgress" );
            this.prgProgress.Maximum = 1000;
            this.prgProgress.Name = "prgProgress";
            // 
            // dlgSave
            // 
            this.dlgSave.DefaultExt = "dss";
            resources.ApplyResources( this.dlgSave, "dlgSave" );
            // 
            // LocationOptimizerDialog
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.prgProgress );
            this.Controls.Add( this.lbFound );
            this.Controls.Add( this.label1 );
            this.Name = "LocationOptimizerDialog";
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer ticker;
        private System.Windows.Forms.Label lbFound;
        private System.Windows.Forms.ProgressBar prgProgress;
        private System.Windows.Forms.SaveFileDialog dlgSave;
    }
}
