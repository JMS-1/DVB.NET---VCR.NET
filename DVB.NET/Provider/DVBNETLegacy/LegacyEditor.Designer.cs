namespace JMS.DVB.Provider.Legacy
{
    partial class LegacyEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( LegacyEditor ) );
            this.grpProperties = new System.Windows.Forms.GroupBox();
            this.lstParams = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.lstCards = new System.Windows.Forms.ListBox();
            this.selDevice = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.grpProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpProperties
            // 
            this.grpProperties.AccessibleDescription = null;
            this.grpProperties.AccessibleName = null;
            resources.ApplyResources( this.grpProperties, "grpProperties" );
            this.grpProperties.BackgroundImage = null;
            this.grpProperties.Controls.Add( this.lstParams );
            this.grpProperties.Controls.Add( this.lstCards );
            this.grpProperties.Controls.Add( this.selDevice );
            this.grpProperties.Controls.Add( this.label2 );
            this.grpProperties.Font = null;
            this.grpProperties.Name = "grpProperties";
            this.grpProperties.TabStop = false;
            // 
            // lstParams
            // 
            this.lstParams.AccessibleDescription = null;
            this.lstParams.AccessibleName = null;
            resources.ApplyResources( this.lstParams, "lstParams" );
            this.lstParams.BackgroundImage = null;
            this.lstParams.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2} );
            this.lstParams.Font = null;
            this.lstParams.FullRowSelect = true;
            this.lstParams.GridLines = true;
            this.lstParams.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstParams.MultiSelect = false;
            this.lstParams.Name = "lstParams";
            this.lstParams.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstParams.UseCompatibleStateImageBehavior = false;
            this.lstParams.View = System.Windows.Forms.View.Details;
            this.lstParams.DoubleClick += new System.EventHandler( this.lstParams_DoubleClick );
            // 
            // columnHeader1
            // 
            resources.ApplyResources( this.columnHeader1, "columnHeader1" );
            // 
            // columnHeader2
            // 
            resources.ApplyResources( this.columnHeader2, "columnHeader2" );
            // 
            // lstCards
            // 
            this.lstCards.AccessibleDescription = null;
            this.lstCards.AccessibleName = null;
            resources.ApplyResources( this.lstCards, "lstCards" );
            this.lstCards.BackgroundImage = null;
            this.lstCards.Font = null;
            this.lstCards.FormattingEnabled = true;
            this.lstCards.Name = "lstCards";
            this.lstCards.Sorted = true;
            // 
            // selDevice
            // 
            this.selDevice.AccessibleDescription = null;
            this.selDevice.AccessibleName = null;
            resources.ApplyResources( this.selDevice, "selDevice" );
            this.selDevice.BackgroundImage = null;
            this.selDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selDevice.Font = null;
            this.selDevice.FormattingEnabled = true;
            this.selDevice.Name = "selDevice";
            this.selDevice.Sorted = true;
            this.selDevice.SelectionChangeCommitted += new System.EventHandler( this.selDevice_SelectionChangeCommitted );
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources( this.label2, "label2" );
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // LegacyEditor
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add( this.grpProperties );
            this.Font = null;
            this.Name = "LegacyEditor";
            this.Load += new System.EventHandler( this.LegacyEditor_Load );
            this.grpProperties.ResumeLayout( false );
            this.grpProperties.PerformLayout();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.GroupBox grpProperties;
        private System.Windows.Forms.ListView lstParams;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ListBox lstCards;
        private System.Windows.Forms.ComboBox selDevice;
        private System.Windows.Forms.Label label2;
    }
}
