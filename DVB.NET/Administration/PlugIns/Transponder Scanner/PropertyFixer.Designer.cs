namespace JMS.DVB.Administration.SourceScanner
{
    partial class PropertyFixer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( PropertyFixer ) );
            this.lbProfile = new System.Windows.Forms.Label();
            this.lstSources = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.lbGroups = new System.Windows.Forms.Label();
            this.grpData = new System.Windows.Forms.GroupBox();
            this.cmdDelAudio = new System.Windows.Forms.Button();
            this.cmdAddAudio = new System.Windows.Forms.Button();
            this.selAudioPID = new System.Windows.Forms.NumericUpDown();
            this.selAudioType = new System.Windows.Forms.ComboBox();
            this.txAudioLanguage = new System.Windows.Forms.TextBox();
            this.ckAudio = new System.Windows.Forms.CheckBox();
            this.selAudio = new System.Windows.Forms.ComboBox();
            this.ckEPG = new System.Windows.Forms.CheckBox();
            this.selTextPID = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.selVideoPID = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.selVideo = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ckService = new System.Windows.Forms.CheckBox();
            this.ckEncrypted = new System.Windows.Forms.CheckBox();
            this.selType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txProvider = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.selAudioPID)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.selTextPID)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.selVideoPID)).BeginInit();
            this.SuspendLayout();
            // 
            // lbProfile
            // 
            resources.ApplyResources( this.lbProfile, "lbProfile" );
            this.lbProfile.Name = "lbProfile";
            // 
            // lstSources
            // 
            resources.ApplyResources( this.lstSources, "lstSources" );
            this.lstSources.CheckBoxes = true;
            this.lstSources.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1} );
            this.lstSources.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstSources.MultiSelect = false;
            this.lstSources.Name = "lstSources";
            this.lstSources.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstSources.UseCompatibleStateImageBehavior = false;
            this.lstSources.View = System.Windows.Forms.View.Details;
            this.lstSources.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler( this.lstSources_ItemChecked );
            this.lstSources.SelectedIndexChanged += new System.EventHandler( this.lstSources_SelectedIndexChanged );
            // 
            // columnHeader1
            // 
            resources.ApplyResources( this.columnHeader1, "columnHeader1" );
            // 
            // lbGroups
            // 
            resources.ApplyResources( this.lbGroups, "lbGroups" );
            this.lbGroups.Name = "lbGroups";
            // 
            // grpData
            // 
            resources.ApplyResources( this.grpData, "grpData" );
            this.grpData.Controls.Add( this.cmdDelAudio );
            this.grpData.Controls.Add( this.cmdAddAudio );
            this.grpData.Controls.Add( this.selAudioPID );
            this.grpData.Controls.Add( this.selAudioType );
            this.grpData.Controls.Add( this.txAudioLanguage );
            this.grpData.Controls.Add( this.ckAudio );
            this.grpData.Controls.Add( this.selAudio );
            this.grpData.Controls.Add( this.ckEPG );
            this.grpData.Controls.Add( this.selTextPID );
            this.grpData.Controls.Add( this.label6 );
            this.grpData.Controls.Add( this.selVideoPID );
            this.grpData.Controls.Add( this.label5 );
            this.grpData.Controls.Add( this.selVideo );
            this.grpData.Controls.Add( this.label4 );
            this.grpData.Controls.Add( this.ckService );
            this.grpData.Controls.Add( this.ckEncrypted );
            this.grpData.Controls.Add( this.selType );
            this.grpData.Controls.Add( this.label3 );
            this.grpData.Controls.Add( this.txProvider );
            this.grpData.Controls.Add( this.label2 );
            this.grpData.Controls.Add( this.txName );
            this.grpData.Controls.Add( this.label1 );
            this.grpData.Name = "grpData";
            this.grpData.TabStop = false;
            // 
            // cmdDelAudio
            // 
            resources.ApplyResources( this.cmdDelAudio, "cmdDelAudio" );
            this.cmdDelAudio.Name = "cmdDelAudio";
            this.cmdDelAudio.UseVisualStyleBackColor = true;
            this.cmdDelAudio.Click += new System.EventHandler( this.cmdDelAudio_Click );
            // 
            // cmdAddAudio
            // 
            resources.ApplyResources( this.cmdAddAudio, "cmdAddAudio" );
            this.cmdAddAudio.Name = "cmdAddAudio";
            this.cmdAddAudio.UseVisualStyleBackColor = true;
            this.cmdAddAudio.Click += new System.EventHandler( this.cmdAddAudio_Click );
            // 
            // selAudioPID
            // 
            resources.ApplyResources( this.selAudioPID, "selAudioPID" );
            this.selAudioPID.Maximum = new decimal( new int[] {
            8191,
            0,
            0,
            0} );
            this.selAudioPID.Name = "selAudioPID";
            this.selAudioPID.ValueChanged += new System.EventHandler( this.selAudioPID_ValueChanged );
            // 
            // selAudioType
            // 
            this.selAudioType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selAudioType.FormattingEnabled = true;
            this.selAudioType.Items.AddRange( new object[] {
            resources.GetString("selAudioType.Items"),
            resources.GetString("selAudioType.Items1")} );
            resources.ApplyResources( this.selAudioType, "selAudioType" );
            this.selAudioType.Name = "selAudioType";
            this.selAudioType.SelectedIndexChanged += new System.EventHandler( this.selAudioType_SelectedIndexChanged );
            // 
            // txAudioLanguage
            // 
            resources.ApplyResources( this.txAudioLanguage, "txAudioLanguage" );
            this.txAudioLanguage.Name = "txAudioLanguage";
            this.txAudioLanguage.TextChanged += new System.EventHandler( this.txAudioLanguage_TextChanged );
            // 
            // ckAudio
            // 
            resources.ApplyResources( this.ckAudio, "ckAudio" );
            this.ckAudio.Name = "ckAudio";
            this.ckAudio.ThreeState = true;
            this.ckAudio.UseVisualStyleBackColor = true;
            this.ckAudio.CheckStateChanged += new System.EventHandler( this.ckAudio_CheckStateChanged );
            // 
            // selAudio
            // 
            resources.ApplyResources( this.selAudio, "selAudio" );
            this.selAudio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selAudio.FormattingEnabled = true;
            this.selAudio.Name = "selAudio";
            this.selAudio.Sorted = true;
            this.selAudio.SelectedIndexChanged += new System.EventHandler( this.selAudio_SelectedIndexChanged );
            // 
            // ckEPG
            // 
            resources.ApplyResources( this.ckEPG, "ckEPG" );
            this.ckEPG.Name = "ckEPG";
            this.ckEPG.UseVisualStyleBackColor = true;
            this.ckEPG.CheckedChanged += new System.EventHandler( this.ckEPG_CheckedChanged );
            // 
            // selTextPID
            // 
            resources.ApplyResources( this.selTextPID, "selTextPID" );
            this.selTextPID.Maximum = new decimal( new int[] {
            8191,
            0,
            0,
            0} );
            this.selTextPID.Name = "selTextPID";
            this.selTextPID.ValueChanged += new System.EventHandler( this.selTextPID_ValueChanged );
            // 
            // label6
            // 
            resources.ApplyResources( this.label6, "label6" );
            this.label6.Name = "label6";
            // 
            // selVideoPID
            // 
            resources.ApplyResources( this.selVideoPID, "selVideoPID" );
            this.selVideoPID.Maximum = new decimal( new int[] {
            8191,
            0,
            0,
            0} );
            this.selVideoPID.Name = "selVideoPID";
            this.selVideoPID.ValueChanged += new System.EventHandler( this.selVideoPID_ValueChanged );
            // 
            // label5
            // 
            resources.ApplyResources( this.label5, "label5" );
            this.label5.Name = "label5";
            // 
            // selVideo
            // 
            this.selVideo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selVideo.FormattingEnabled = true;
            this.selVideo.Items.AddRange( new object[] {
            resources.GetString("selVideo.Items"),
            resources.GetString("selVideo.Items1"),
            resources.GetString("selVideo.Items2"),
            resources.GetString("selVideo.Items3"),
            resources.GetString("selVideo.Items4")} );
            resources.ApplyResources( this.selVideo, "selVideo" );
            this.selVideo.Name = "selVideo";
            this.selVideo.SelectedIndexChanged += new System.EventHandler( this.selVideo_SelectedIndexChanged );
            // 
            // label4
            // 
            resources.ApplyResources( this.label4, "label4" );
            this.label4.Name = "label4";
            // 
            // ckService
            // 
            resources.ApplyResources( this.ckService, "ckService" );
            this.ckService.Name = "ckService";
            this.ckService.ThreeState = true;
            this.ckService.UseVisualStyleBackColor = true;
            this.ckService.CheckStateChanged += new System.EventHandler( this.ckService_CheckStateChanged );
            // 
            // ckEncrypted
            // 
            resources.ApplyResources( this.ckEncrypted, "ckEncrypted" );
            this.ckEncrypted.Name = "ckEncrypted";
            this.ckEncrypted.ThreeState = true;
            this.ckEncrypted.UseVisualStyleBackColor = true;
            this.ckEncrypted.CheckStateChanged += new System.EventHandler( this.ckEncrypted_CheckStateChanged );
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
            this.selType.SelectedIndexChanged += new System.EventHandler( this.selType_SelectedIndexChanged );
            // 
            // label3
            // 
            resources.ApplyResources( this.label3, "label3" );
            this.label3.Name = "label3";
            // 
            // txProvider
            // 
            resources.ApplyResources( this.txProvider, "txProvider" );
            this.txProvider.Name = "txProvider";
            this.txProvider.TextChanged += new System.EventHandler( this.txProvider_TextChanged );
            // 
            // label2
            // 
            resources.ApplyResources( this.label2, "label2" );
            this.label2.Name = "label2";
            // 
            // txName
            // 
            resources.ApplyResources( this.txName, "txName" );
            this.txName.Name = "txName";
            this.txName.TextChanged += new System.EventHandler( this.txName_TextChanged );
            // 
            // label1
            // 
            resources.ApplyResources( this.label1, "label1" );
            this.label1.Name = "label1";
            // 
            // PropertyFixer
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.grpData );
            this.Controls.Add( this.lstSources );
            this.Controls.Add( this.lbGroups );
            this.Controls.Add( this.lbProfile );
            this.Name = "PropertyFixer";
            this.Load += new System.EventHandler( this.PropertyFixer_Load );
            this.grpData.ResumeLayout( false );
            this.grpData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.selAudioPID)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.selTextPID)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.selVideoPID)).EndInit();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbProfile;
        private System.Windows.Forms.ListView lstSources;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label lbGroups;
        private System.Windows.Forms.GroupBox grpData;
        private System.Windows.Forms.NumericUpDown selTextPID;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown selVideoPID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox selVideo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox ckService;
        private System.Windows.Forms.CheckBox ckEncrypted;
        private System.Windows.Forms.ComboBox selType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txProvider;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox ckEPG;
        private System.Windows.Forms.NumericUpDown selAudioPID;
        private System.Windows.Forms.ComboBox selAudioType;
        private System.Windows.Forms.TextBox txAudioLanguage;
        private System.Windows.Forms.CheckBox ckAudio;
        private System.Windows.Forms.ComboBox selAudio;
        private System.Windows.Forms.Button cmdDelAudio;
        private System.Windows.Forms.Button cmdAddAudio;
    }
}
