namespace JMS.DVB.Administration.ProfileManager
{
    partial class ProfileConversion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ProfileConversion ) );
            this.label1 = new System.Windows.Forms.Label();
            this.lstProfiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.lbConfirmOverwrite = new System.Windows.Forms.Label();
            this.ckConfirmOverwrite = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources( this.label1, "label1" );
            this.label1.Name = "label1";
            // 
            // lstProfiles
            // 
            this.lstProfiles.CheckBoxes = true;
            this.lstProfiles.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1} );
            this.lstProfiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            resources.ApplyResources( this.lstProfiles, "lstProfiles" );
            this.lstProfiles.Name = "lstProfiles";
            this.lstProfiles.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstProfiles.UseCompatibleStateImageBehavior = false;
            this.lstProfiles.View = System.Windows.Forms.View.Details;
            this.lstProfiles.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler( this.lstProfiles_ItemChecked );
            // 
            // columnHeader1
            // 
            resources.ApplyResources( this.columnHeader1, "columnHeader1" );
            // 
            // lbConfirmOverwrite
            // 
            resources.ApplyResources( this.lbConfirmOverwrite, "lbConfirmOverwrite" );
            this.lbConfirmOverwrite.Name = "lbConfirmOverwrite";
            // 
            // ckConfirmOverwrite
            // 
            resources.ApplyResources( this.ckConfirmOverwrite, "ckConfirmOverwrite" );
            this.ckConfirmOverwrite.Name = "ckConfirmOverwrite";
            this.ckConfirmOverwrite.UseVisualStyleBackColor = true;
            this.ckConfirmOverwrite.CheckedChanged += new System.EventHandler( this.ckConfirmOverwrite_CheckedChanged );
            // 
            // ProfileConversion
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.ckConfirmOverwrite );
            this.Controls.Add( this.lbConfirmOverwrite );
            this.Controls.Add( this.lstProfiles );
            this.Controls.Add( this.label1 );
            this.Name = "ProfileConversion";
            this.Load += new System.EventHandler( this.ProfileConversion_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView lstProfiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label lbConfirmOverwrite;
        private System.Windows.Forms.CheckBox ckConfirmOverwrite;
    }
}
