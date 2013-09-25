namespace JMS.DVB.Administration.SourceScanner
{
    partial class ConfigurationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ConfigurationDialog ) );
            this.lbProfile = new System.Windows.Forms.Label();
            this.pnlDiSEqC = new System.Windows.Forms.Panel();
            this.pnlLNB = new System.Windows.Forms.Panel();
            this.cmdDish = new System.Windows.Forms.Button();
            this.selDish = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.selMode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlSpecial = new System.Windows.Forms.Panel();
            this.ckPower = new System.Windows.Forms.CheckBox();
            this.selSwitch = new System.Windows.Forms.NumericUpDown();
            this.selLOF2 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.selLOF1 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.selGroups = new JMS.DVB.Administration.SourceScanner.SourceGroupsSelector();
            this.pnlDiSEqC.SuspendLayout();
            this.pnlLNB.SuspendLayout();
            this.pnlSpecial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.selSwitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.selLOF2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.selLOF1)).BeginInit();
            this.SuspendLayout();
            // 
            // lbProfile
            // 
            resources.ApplyResources( this.lbProfile, "lbProfile" );
            this.lbProfile.Name = "lbProfile";
            // 
            // pnlDiSEqC
            // 
            resources.ApplyResources( this.pnlDiSEqC, "pnlDiSEqC" );
            this.pnlDiSEqC.Controls.Add( this.pnlLNB );
            this.pnlDiSEqC.Controls.Add( this.selMode );
            this.pnlDiSEqC.Controls.Add( this.label1 );
            this.pnlDiSEqC.Name = "pnlDiSEqC";
            // 
            // pnlLNB
            // 
            this.pnlLNB.Controls.Add( this.cmdDish );
            this.pnlLNB.Controls.Add( this.selDish );
            this.pnlLNB.Controls.Add( this.label2 );
            resources.ApplyResources( this.pnlLNB, "pnlLNB" );
            this.pnlLNB.Name = "pnlLNB";
            // 
            // cmdDish
            // 
            resources.ApplyResources( this.cmdDish, "cmdDish" );
            this.cmdDish.Name = "cmdDish";
            this.cmdDish.UseVisualStyleBackColor = true;
            this.cmdDish.Click += new System.EventHandler( this.cmdDish_Click );
            // 
            // selDish
            // 
            this.selDish.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selDish.FormattingEnabled = true;
            resources.ApplyResources( this.selDish, "selDish" );
            this.selDish.Name = "selDish";
            this.selDish.SelectionChangeCommitted += new System.EventHandler( this.selDish_SelectionChangeCommitted );
            // 
            // label2
            // 
            resources.ApplyResources( this.label2, "label2" );
            this.label2.Name = "label2";
            // 
            // selMode
            // 
            this.selMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selMode.FormattingEnabled = true;
            this.selMode.Items.AddRange( new object[] {
            resources.GetString("selMode.Items"),
            resources.GetString("selMode.Items1"),
            resources.GetString("selMode.Items2")} );
            resources.ApplyResources( this.selMode, "selMode" );
            this.selMode.Name = "selMode";
            this.selMode.SelectionChangeCommitted += new System.EventHandler( this.selMode_SelectionChangeCommitted );
            // 
            // label1
            // 
            resources.ApplyResources( this.label1, "label1" );
            this.label1.Name = "label1";
            // 
            // pnlSpecial
            // 
            this.pnlSpecial.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlSpecial.Controls.Add( this.ckPower );
            this.pnlSpecial.Controls.Add( this.selSwitch );
            this.pnlSpecial.Controls.Add( this.selLOF2 );
            this.pnlSpecial.Controls.Add( this.label5 );
            this.pnlSpecial.Controls.Add( this.selLOF1 );
            this.pnlSpecial.Controls.Add( this.label4 );
            this.pnlSpecial.Controls.Add( this.label3 );
            resources.ApplyResources( this.pnlSpecial, "pnlSpecial" );
            this.pnlSpecial.Name = "pnlSpecial";
            // 
            // ckPower
            // 
            resources.ApplyResources( this.ckPower, "ckPower" );
            this.ckPower.Name = "ckPower";
            this.ckPower.UseVisualStyleBackColor = true;
            this.ckPower.CheckedChanged += new System.EventHandler( this.ckPower_CheckedChanged );
            // 
            // selSwitch
            // 
            resources.ApplyResources( this.selSwitch, "selSwitch" );
            this.selSwitch.Maximum = new decimal( new int[] {
            100000000,
            0,
            0,
            0} );
            this.selSwitch.Name = "selSwitch";
            this.selSwitch.ValueChanged += new System.EventHandler( this.selSwitch_ValueChanged );
            // 
            // selLOF2
            // 
            resources.ApplyResources( this.selLOF2, "selLOF2" );
            this.selLOF2.Maximum = new decimal( new int[] {
            100000000,
            0,
            0,
            0} );
            this.selLOF2.Name = "selLOF2";
            this.selLOF2.ValueChanged += new System.EventHandler( this.selLOF2_ValueChanged );
            // 
            // label5
            // 
            resources.ApplyResources( this.label5, "label5" );
            this.label5.Name = "label5";
            // 
            // selLOF1
            // 
            resources.ApplyResources( this.selLOF1, "selLOF1" );
            this.selLOF1.Maximum = new decimal( new int[] {
            100000000,
            0,
            0,
            0} );
            this.selLOF1.Name = "selLOF1";
            this.selLOF1.ValueChanged += new System.EventHandler( this.selLOF1_ValueChanged );
            // 
            // label4
            // 
            resources.ApplyResources( this.label4, "label4" );
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources( this.label3, "label3" );
            this.label3.Name = "label3";
            // 
            // selGroups
            // 
            resources.ApplyResources( this.selGroups, "selGroups" );
            this.selGroups.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.selGroups.Name = "selGroups";
            // 
            // ConfigurationDialog
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.pnlSpecial );
            this.Controls.Add( this.pnlDiSEqC );
            this.Controls.Add( this.selGroups );
            this.Controls.Add( this.lbProfile );
            this.Name = "ConfigurationDialog";
            this.Load += new System.EventHandler( this.ConfigurationDialog_Load );
            this.pnlDiSEqC.ResumeLayout( false );
            this.pnlDiSEqC.PerformLayout();
            this.pnlLNB.ResumeLayout( false );
            this.pnlLNB.PerformLayout();
            this.pnlSpecial.ResumeLayout( false );
            this.pnlSpecial.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.selSwitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.selLOF2)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.selLOF1)).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Label lbProfile;
        private SourceGroupsSelector selGroups;
        private System.Windows.Forms.Panel pnlDiSEqC;
        private System.Windows.Forms.Button cmdDish;
        private System.Windows.Forms.ComboBox selDish;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox selMode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlLNB;
        private System.Windows.Forms.Panel pnlSpecial;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown selLOF1;
        private System.Windows.Forms.NumericUpDown selLOF2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox ckPower;
        private System.Windows.Forms.NumericUpDown selSwitch;
        private System.Windows.Forms.Label label5;
    }
}
