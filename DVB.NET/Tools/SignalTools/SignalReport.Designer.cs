using System;

namespace JMS.DVB.Administration.Tools
{
    partial class SignalReport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( SignalReport ) );
            this.updater = new System.Windows.Forms.Timer( this.components );
            this.lbProfile = new System.Windows.Forms.Label();
            this.selGroup = new System.Windows.Forms.ComboBox();
            this.picView = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize) (this.picView)).BeginInit();
            this.SuspendLayout();
            // 
            // updater
            // 
            this.updater.Tick += new System.EventHandler( this.updater_Tick );
            // 
            // lbProfile
            // 
            resources.ApplyResources( this.lbProfile, "lbProfile" );
            this.lbProfile.Name = "lbProfile";
            // 
            // selGroup
            // 
            resources.ApplyResources( this.selGroup, "selGroup" );
            this.selGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selGroup.FormattingEnabled = true;
            this.selGroup.Name = "selGroup";
            this.selGroup.Sorted = true;
            this.selGroup.SelectionChangeCommitted += new System.EventHandler( this.selGroup_SelectionChangeCommitted );
            // 
            // picView
            // 
            resources.ApplyResources( this.picView, "picView" );
            this.picView.Name = "picView";
            this.picView.TabStop = false;
            // 
            // SignalReport
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.picView );
            this.Controls.Add( this.selGroup );
            this.Controls.Add( this.lbProfile );
            this.Name = "SignalReport";
            this.Load += new System.EventHandler( this.SignalReport_Load );
            ((System.ComponentModel.ISupportInitialize) (this.picView)).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Timer updater;
        private System.Windows.Forms.Label lbProfile;
        private System.Windows.Forms.ComboBox selGroup;
        private System.Windows.Forms.PictureBox picView;
    }
}
