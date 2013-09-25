namespace JMS.DVB.Administration.SourceScanner
{
    partial class GroupExclusion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( GroupExclusion ) );
            this.lbProfile = new System.Windows.Forms.Label();
            this.lbGroups = new System.Windows.Forms.Label();
            this.lstGroups = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // lbProfile
            // 
            resources.ApplyResources( this.lbProfile, "lbProfile" );
            this.lbProfile.Name = "lbProfile";
            // 
            // lbGroups
            // 
            resources.ApplyResources( this.lbGroups, "lbGroups" );
            this.lbGroups.Name = "lbGroups";
            // 
            // lstGroups
            // 
            resources.ApplyResources( this.lstGroups, "lstGroups" );
            this.lstGroups.CheckBoxes = true;
            this.lstGroups.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1} );
            this.lstGroups.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstGroups.MultiSelect = false;
            this.lstGroups.Name = "lstGroups";
            this.lstGroups.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstGroups.UseCompatibleStateImageBehavior = false;
            this.lstGroups.View = System.Windows.Forms.View.Details;
            this.lstGroups.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler( this.lstGroups_ItemChecked );
            // 
            // columnHeader1
            // 
            resources.ApplyResources( this.columnHeader1, "columnHeader1" );
            // 
            // GroupExclusion
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.lstGroups );
            this.Controls.Add( this.lbGroups );
            this.Controls.Add( this.lbProfile );
            this.Name = "GroupExclusion";
            this.Load += new System.EventHandler( this.GroupExclusion_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbProfile;
        private System.Windows.Forms.Label lbGroups;
        private System.Windows.Forms.ListView lstGroups;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}
