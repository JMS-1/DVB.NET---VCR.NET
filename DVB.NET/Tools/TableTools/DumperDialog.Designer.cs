namespace JMS.DVB.Administration.Tools
{
    partial class DumperDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( DumperDialog ) );
            this.lbProfile = new System.Windows.Forms.Label();
            this.selType = new System.Windows.Forms.ComboBox();
            this.selSource = new System.Windows.Forms.ComboBox();
            this.selPIDType = new System.Windows.Forms.ComboBox();
            this.optStandard = new System.Windows.Forms.RadioButton();
            this.optExtended = new System.Windows.Forms.RadioButton();
            this.selPid = new System.Windows.Forms.NumericUpDown();
            this.checkEnd = new System.Windows.Forms.Timer( this.components );
            this.lbStatus = new System.Windows.Forms.Label();
            this.dlgFile = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize) (this.selPid)).BeginInit();
            this.SuspendLayout();
            // 
            // lbProfile
            // 
            resources.ApplyResources( this.lbProfile, "lbProfile" );
            this.lbProfile.Name = "lbProfile";
            // 
            // selType
            // 
            this.selType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selType.FormattingEnabled = true;
            this.selType.Items.AddRange( new object[] {
            resources.GetString("selType.Items"),
            resources.GetString("selType.Items1"),
            resources.GetString("selType.Items2"),
            resources.GetString("selType.Items3")} );
            resources.ApplyResources( this.selType, "selType" );
            this.selType.Name = "selType";
            this.selType.SelectionChangeCommitted += new System.EventHandler( this.selType_SelectionChangeCommitted );
            // 
            // selSource
            // 
            resources.ApplyResources( this.selSource, "selSource" );
            this.selSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selSource.FormattingEnabled = true;
            this.selSource.Name = "selSource";
            this.selSource.SelectionChangeCommitted += new System.EventHandler( this.selSource_SelectionChangeCommitted );
            // 
            // selPIDType
            // 
            this.selPIDType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selPIDType.FormattingEnabled = true;
            this.selPIDType.Items.AddRange( new object[] {
            resources.GetString("selPIDType.Items"),
            resources.GetString("selPIDType.Items1"),
            resources.GetString("selPIDType.Items2"),
            resources.GetString("selPIDType.Items3"),
            resources.GetString("selPIDType.Items4")} );
            resources.ApplyResources( this.selPIDType, "selPIDType" );
            this.selPIDType.Name = "selPIDType";
            this.selPIDType.SelectionChangeCommitted += new System.EventHandler( this.selPIDType_SelectionChangeCommitted );
            // 
            // optStandard
            // 
            resources.ApplyResources( this.optStandard, "optStandard" );
            this.optStandard.Checked = true;
            this.optStandard.Name = "optStandard";
            this.optStandard.TabStop = true;
            this.optStandard.UseVisualStyleBackColor = true;
            // 
            // optExtended
            // 
            resources.ApplyResources( this.optExtended, "optExtended" );
            this.optExtended.Name = "optExtended";
            this.optExtended.UseVisualStyleBackColor = true;
            // 
            // selPid
            // 
            resources.ApplyResources( this.selPid, "selPid" );
            this.selPid.Maximum = new decimal( new int[] {
            8191,
            0,
            0,
            0} );
            this.selPid.Name = "selPid";
            this.selPid.ValueChanged += new System.EventHandler( this.selPid_ValueChanged );
            // 
            // checkEnd
            // 
            this.checkEnd.Tick += new System.EventHandler( this.checkEnd_Tick );
            // 
            // lbStatus
            // 
            resources.ApplyResources( this.lbStatus, "lbStatus" );
            this.lbStatus.Name = "lbStatus";
            // 
            // dlgFile
            // 
            this.dlgFile.DefaultExt = "bin";
            resources.ApplyResources( this.dlgFile, "dlgFile" );
            // 
            // DumperDialog
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.lbStatus );
            this.Controls.Add( this.selPid );
            this.Controls.Add( this.optExtended );
            this.Controls.Add( this.optStandard );
            this.Controls.Add( this.selPIDType );
            this.Controls.Add( this.selSource );
            this.Controls.Add( this.selType );
            this.Controls.Add( this.lbProfile );
            this.Name = "DumperDialog";
            this.Load += new System.EventHandler( this.DumperDialog_Load );
            ((System.ComponentModel.ISupportInitialize) (this.selPid)).EndInit();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbProfile;
        private System.Windows.Forms.ComboBox selType;
        private System.Windows.Forms.ComboBox selSource;
        private System.Windows.Forms.ComboBox selPIDType;
        private System.Windows.Forms.RadioButton optStandard;
        private System.Windows.Forms.RadioButton optExtended;
        private System.Windows.Forms.NumericUpDown selPid;
        private System.Windows.Forms.Timer checkEnd;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.SaveFileDialog dlgFile;
    }
}
