namespace DVBNETAdmin.WizardSteps
{
    partial class NewProfileSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( NewProfileSelector ) );
            this.label1 = new System.Windows.Forms.Label();
            this.selProfile = new System.Windows.Forms.ComboBox();
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
            // selProfile
            // 
            this.selProfile.AccessibleDescription = null;
            this.selProfile.AccessibleName = null;
            resources.ApplyResources( this.selProfile, "selProfile" );
            this.selProfile.BackgroundImage = null;
            this.selProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selProfile.Font = null;
            this.selProfile.FormattingEnabled = true;
            this.selProfile.Name = "selProfile";
            this.selProfile.SelectionChangeCommitted += new System.EventHandler( this.selProfile_SelectionChangeCommitted );
            // 
            // NewProfileSelector
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add( this.selProfile );
            this.Controls.Add( this.label1 );
            this.Font = null;
            this.Name = "NewProfileSelector";
            this.Load += new System.EventHandler( this.NewProfileSelector_Load );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox selProfile;
    }
}
