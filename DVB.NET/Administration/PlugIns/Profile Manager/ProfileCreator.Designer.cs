namespace JMS.DVB.Administration.ProfileManager
{
    partial class ProfileCreator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ProfileCreator ) );
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.optTerrestrial = new System.Windows.Forms.RadioButton();
            this.optCable = new System.Windows.Forms.RadioButton();
            this.optSatellite = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources( this.label1, "label1" );
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources( this.label2, "label2" );
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // txName
            // 
            this.txName.AccessibleDescription = null;
            this.txName.AccessibleName = null;
            resources.ApplyResources( this.txName, "txName" );
            this.txName.BackgroundImage = null;
            this.txName.Font = null;
            this.txName.Name = "txName";
            this.txName.TextChanged += new System.EventHandler( this.txName_TextChanged );
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources( this.groupBox1, "groupBox1" );
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add( this.optTerrestrial );
            this.groupBox1.Controls.Add( this.optCable );
            this.groupBox1.Controls.Add( this.optSatellite );
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // optTerrestrial
            // 
            this.optTerrestrial.AccessibleDescription = null;
            this.optTerrestrial.AccessibleName = null;
            resources.ApplyResources( this.optTerrestrial, "optTerrestrial" );
            this.optTerrestrial.BackgroundImage = null;
            this.optTerrestrial.Font = null;
            this.optTerrestrial.Name = "optTerrestrial";
            this.optTerrestrial.TabStop = true;
            this.optTerrestrial.UseVisualStyleBackColor = true;
            this.optTerrestrial.CheckedChanged += new System.EventHandler( this.optSatellite_CheckedChanged );
            // 
            // optCable
            // 
            this.optCable.AccessibleDescription = null;
            this.optCable.AccessibleName = null;
            resources.ApplyResources( this.optCable, "optCable" );
            this.optCable.BackgroundImage = null;
            this.optCable.Font = null;
            this.optCable.Name = "optCable";
            this.optCable.TabStop = true;
            this.optCable.Tag = "CableProfile";
            this.optCable.UseVisualStyleBackColor = true;
            this.optCable.CheckedChanged += new System.EventHandler( this.optSatellite_CheckedChanged );
            // 
            // optSatellite
            // 
            this.optSatellite.AccessibleDescription = null;
            this.optSatellite.AccessibleName = null;
            resources.ApplyResources( this.optSatellite, "optSatellite" );
            this.optSatellite.BackgroundImage = null;
            this.optSatellite.Font = null;
            this.optSatellite.Name = "optSatellite";
            this.optSatellite.TabStop = true;
            this.optSatellite.Tag = "SatelliteProfile";
            this.optSatellite.UseVisualStyleBackColor = true;
            this.optSatellite.CheckedChanged += new System.EventHandler( this.optSatellite_CheckedChanged );
            // 
            // ProfileCreator
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add( this.groupBox1 );
            this.Controls.Add( this.txName );
            this.Controls.Add( this.label2 );
            this.Controls.Add( this.label1 );
            this.Font = null;
            this.Name = "ProfileCreator";
            this.groupBox1.ResumeLayout( false );
            this.groupBox1.PerformLayout();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton optTerrestrial;
        private System.Windows.Forms.RadioButton optCable;
        private System.Windows.Forms.RadioButton optSatellite;
    }
}
