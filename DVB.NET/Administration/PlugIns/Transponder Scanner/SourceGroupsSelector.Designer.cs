namespace JMS.DVB.Administration.SourceScanner
{
    partial class SourceGroupsSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( SourceGroupsSelector ) );
            this.lbFeedback = new System.Windows.Forms.Label();
            this.lstLocations = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // lbFeedback
            // 
            this.lbFeedback.AccessibleDescription = null;
            this.lbFeedback.AccessibleName = null;
            resources.ApplyResources( this.lbFeedback, "lbFeedback" );
            this.lbFeedback.Font = null;
            this.lbFeedback.Name = "lbFeedback";
            // 
            // lstLocations
            // 
            this.lstLocations.AccessibleDescription = null;
            this.lstLocations.AccessibleName = null;
            resources.ApplyResources( this.lstLocations, "lstLocations" );
            this.lstLocations.BackgroundImage = null;
            this.lstLocations.CheckBoxes = true;
            this.lstLocations.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1} );
            this.lstLocations.Font = null;
            this.lstLocations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstLocations.Name = "lstLocations";
            this.lstLocations.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstLocations.UseCompatibleStateImageBehavior = false;
            this.lstLocations.View = System.Windows.Forms.View.Details;
            this.lstLocations.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler( this.lstLocations_ItemChecked );
            // 
            // columnHeader1
            // 
            resources.ApplyResources( this.columnHeader1, "columnHeader1" );
            // 
            // SourceGroupsSelector
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add( this.lstLocations );
            this.Controls.Add( this.lbFeedback );
            this.Font = null;
            this.Name = "SourceGroupsSelector";
            this.Load += new System.EventHandler( this.SourceGroupsSelector_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbFeedback;
        private System.Windows.Forms.ListView lstLocations;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}
