namespace JMS.DVB.Viewer
{
    partial class ViewerControl
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
            // Detach remote control
            using (m_RCReceiver)
                m_RCReceiver = null;

            // Detach overlay
            using (m_Overlay)
                m_Overlay = null;

            // Check for accessor
            using (m_CurrentAdaptor)
                m_CurrentAdaptor = null;

            // Remove favorite manager
            using (m_FavoriteManager)
                m_FavoriteManager = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ViewerControl ) );
            this.directShow = new JMS.DVB.DirectShow.UI.BDAWindow();
            this.osdOff = new System.Windows.Forms.Timer( this.components );
            this.signalTest = new System.Windows.Forms.Timer( this.components );
            this.selScratch = new System.Windows.Forms.ComboBox();
            this.selOptions = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // directShow
            // 
            this.directShow.AC3Decoder = null;
            this.directShow.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources( this.directShow, "directShow" );
            this.directShow.EnforceVideoSynchronisation = false;
            this.directShow.FullScreen = false;
            this.directShow.H264Decoder = null;
            this.directShow.MP2Decoder = null;
            this.directShow.MPEG2Decoder = null;
            this.directShow.Name = "directShow";
            this.directShow.UseCyberlink = null;
            this.directShow.Volume = 0D;
            // 
            // osdOff
            // 
            this.osdOff.Enabled = true;
            this.osdOff.Interval = 1000;
            this.osdOff.Tick += new System.EventHandler( this.osdOff_Tick );
            // 
            // signalTest
            // 
            this.signalTest.Interval = 1000;
            this.signalTest.Tick += new System.EventHandler( this.signalTest_Tick );
            // 
            // selScratch
            // 
            this.selScratch.FormattingEnabled = true;
            resources.ApplyResources( this.selScratch, "selScratch" );
            this.selScratch.Name = "selScratch";
            // 
            // selOptions
            // 
            this.selOptions.FormattingEnabled = true;
            resources.ApplyResources( this.selOptions, "selOptions" );
            this.selOptions.Name = "selOptions";
            // 
            // ViewerControl
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add( this.selScratch );
            this.Controls.Add( this.selOptions );
            this.Controls.Add( this.directShow );
            this.Name = "ViewerControl";
            this.Load += new System.EventHandler( this.ViewerControl_Load );
            this.ResumeLayout( false );

        }

        #endregion

        private JMS.DVB.DirectShow.UI.BDAWindow directShow;
        private System.Windows.Forms.Timer osdOff;
        private System.Windows.Forms.Timer signalTest;
        private System.Windows.Forms.ComboBox selScratch;
        private System.Windows.Forms.ComboBox selOptions;
    }
}
