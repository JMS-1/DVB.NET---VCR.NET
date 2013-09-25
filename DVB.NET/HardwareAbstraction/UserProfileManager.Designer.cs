namespace JMS.DVB
{
	partial class UserProfileManager
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( UserProfileManager ) );
            this.label1 = new System.Windows.Forms.Label();
            this.selProfile = new System.Windows.Forms.ComboBox();
            this.ckAllways = new System.Windows.Forms.CheckBox();
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.selLanguage = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources( this.label1, "label1" );
            this.label1.Name = "label1";
            // 
            // selProfile
            // 
            resources.ApplyResources( this.selProfile, "selProfile" );
            this.selProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selProfile.FormattingEnabled = true;
            this.selProfile.Name = "selProfile";
            this.selProfile.SelectionChangeCommitted += new System.EventHandler( this.UpdateSaveButton );
            // 
            // ckAllways
            // 
            resources.ApplyResources( this.ckAllways, "ckAllways" );
            this.ckAllways.Name = "ckAllways";
            this.ckAllways.UseVisualStyleBackColor = true;
            // 
            // cmdSave
            // 
            resources.ApplyResources( this.cmdSave, "cmdSave" );
            this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.UseVisualStyleBackColor = true;
            this.cmdSave.Click += new System.EventHandler( this.cmdSave_Click );
            // 
            // cmdCancel
            // 
            resources.ApplyResources( this.cmdCancel, "cmdCancel" );
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            resources.ApplyResources( this.label2, "label2" );
            this.label2.Name = "label2";
            // 
            // selLanguage
            // 
            resources.ApplyResources( this.selLanguage, "selLanguage" );
            this.selLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selLanguage.FormattingEnabled = true;
            this.selLanguage.Name = "selLanguage";
            this.selLanguage.SelectionChangeCommitted += new System.EventHandler( this.UpdateSaveButton );
            // 
            // UserProfileManager
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.cmdCancel );
            this.Controls.Add( this.cmdSave );
            this.Controls.Add( this.ckAllways );
            this.Controls.Add( this.selLanguage );
            this.Controls.Add( this.label2 );
            this.Controls.Add( this.selProfile );
            this.Controls.Add( this.label1 );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserProfileManager";
            this.Load += new System.EventHandler( this.UserProfileManager_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox selProfile;
		private System.Windows.Forms.CheckBox ckAllways;
		private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox selLanguage;
	}
}