using System;

namespace JMS.DVB.Administration.Tools
{
    partial class StreamDisplay
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
            // Terminate streams
            CloseStreams();

            // Detach from hardware
            using (IDisposable hardware = m_HardwareManager)
                m_HardwareManager = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( StreamDisplay ) );
            this.lbProfile = new System.Windows.Forms.Label();
            this.selSource = new System.Windows.Forms.ComboBox();
            this.lstStreams = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.updater = new System.Windows.Forms.Timer( this.components );
            this.lbSignal = new System.Windows.Forms.Label();
            this.txSignal = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lbProfile
            // 
            resources.ApplyResources( this.lbProfile, "lbProfile" );
            this.lbProfile.Name = "lbProfile";
            // 
            // selSource
            // 
            resources.ApplyResources( this.selSource, "selSource" );
            this.selSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selSource.FormattingEnabled = true;
            this.selSource.Name = "selSource";
            this.selSource.SelectionChangeCommitted += new System.EventHandler( this.selSource_SelectionChangeCommitted );
            // 
            // lstStreams
            // 
            resources.ApplyResources( this.lstStreams, "lstStreams" );
            this.lstStreams.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3} );
            this.lstStreams.Name = "lstStreams";
            this.lstStreams.UseCompatibleStateImageBehavior = false;
            this.lstStreams.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources( this.columnHeader1, "columnHeader1" );
            // 
            // columnHeader2
            // 
            resources.ApplyResources( this.columnHeader2, "columnHeader2" );
            // 
            // columnHeader3
            // 
            resources.ApplyResources( this.columnHeader3, "columnHeader3" );
            // 
            // updater
            // 
            this.updater.Enabled = true;
            this.updater.Interval = 1000;
            this.updater.Tick += new System.EventHandler( this.updater_Tick );
            // 
            // lbSignal
            // 
            resources.ApplyResources( this.lbSignal, "lbSignal" );
            this.lbSignal.Name = "lbSignal";
            // 
            // txSignal
            // 
            resources.ApplyResources( this.txSignal, "txSignal" );
            this.txSignal.Name = "txSignal";
            this.txSignal.ReadOnly = true;
            // 
            // StreamDisplay
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.txSignal );
            this.Controls.Add( this.lbSignal );
            this.Controls.Add( this.lstStreams );
            this.Controls.Add( this.selSource );
            this.Controls.Add( this.lbProfile );
            this.Name = "StreamDisplay";
            this.Load += new System.EventHandler( this.StreamDisplay_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbProfile;
        private System.Windows.Forms.ComboBox selSource;
        private System.Windows.Forms.ListView lstStreams;
        private System.Windows.Forms.Timer updater;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label lbSignal;
        private System.Windows.Forms.TextBox txSignal;
    }
}
