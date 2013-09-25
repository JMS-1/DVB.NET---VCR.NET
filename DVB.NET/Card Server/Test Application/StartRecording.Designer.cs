namespace CardServerTester
{
    partial class StartRecording
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( StartRecording ) );
            this.cmdStart = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.selStream = new CardServerTester.SelectStream();
            this.SuspendLayout();
            // 
            // cmdStart
            // 
            this.cmdStart.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources( this.cmdStart, "cmdStart" );
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources( this.cmdCancel, "cmdCancel" );
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // selStream
            // 
            resources.ApplyResources( this.selStream, "selStream" );
            this.selStream.Name = "selStream";
            this.selStream.SourceItems = new CardServerTester.SourceItem[0];
            this.selStream.MoreClicked += new System.EventHandler( this.selStream_MoreClicked );
            // 
            // StartRecording
            // 
            this.AcceptButton = this.cmdStart;
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.Controls.Add( this.selStream );
            this.Controls.Add( this.cmdCancel );
            this.Controls.Add( this.cmdStart );
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StartRecording";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Button cmdStart;
        private System.Windows.Forms.Button cmdCancel;
        private SelectStream selStream;
    }
}