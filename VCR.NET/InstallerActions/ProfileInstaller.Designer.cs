namespace JMS.DVBVCR.InstallerActions
{
	partial class ProfileInstaller
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileInstaller));
			this.lbIntro = new System.Windows.Forms.Label();
			this.lbProfile = new System.Windows.Forms.Label();
			this.selProfile = new System.Windows.Forms.ComboBox();
			this.cmdNewProfile = new System.Windows.Forms.Button();
			this.cmdTemplate = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.selTemplate = new System.Windows.Forms.ComboBox();
			this.lbName = new System.Windows.Forms.Label();
			this.txProfileName = new System.Windows.Forms.TextBox();
			this.cmdUse = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lbIntro
			// 
			resources.ApplyResources(this.lbIntro, "lbIntro");
			this.lbIntro.Name = "lbIntro";
			// 
			// lbProfile
			// 
			resources.ApplyResources(this.lbProfile, "lbProfile");
			this.lbProfile.Name = "lbProfile";
			// 
			// selProfile
			// 
			this.selProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.selProfile.FormattingEnabled = true;
			resources.ApplyResources(this.selProfile, "selProfile");
			this.selProfile.Name = "selProfile";
			this.selProfile.Sorted = true;
			this.selProfile.SelectionChangeCommitted += new System.EventHandler(this.selProfile_SelectionChangeCommitted);
			// 
			// cmdNewProfile
			// 
			resources.ApplyResources(this.cmdNewProfile, "cmdNewProfile");
			this.cmdNewProfile.Name = "cmdNewProfile";
			this.cmdNewProfile.UseVisualStyleBackColor = true;
			this.cmdNewProfile.Click += new System.EventHandler(this.cmdNewProfile_Click);
			// 
			// cmdTemplate
			// 
			resources.ApplyResources(this.cmdTemplate, "cmdTemplate");
			this.cmdTemplate.Name = "cmdTemplate";
			this.cmdTemplate.UseVisualStyleBackColor = true;
			this.cmdTemplate.Click += new System.EventHandler(this.cmdTemplate_Click);
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// selTemplate
			// 
			this.selTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.selTemplate.FormattingEnabled = true;
			resources.ApplyResources(this.selTemplate, "selTemplate");
			this.selTemplate.Name = "selTemplate";
			this.selTemplate.Sorted = true;
			this.selTemplate.SelectionChangeCommitted += new System.EventHandler(this.selTemplate_SelectionChangeCommitted);
			// 
			// lbName
			// 
			resources.ApplyResources(this.lbName, "lbName");
			this.lbName.Name = "lbName";
			// 
			// txProfileName
			// 
			resources.ApplyResources(this.txProfileName, "txProfileName");
			this.txProfileName.Name = "txProfileName";
			this.txProfileName.TextChanged += new System.EventHandler(this.txProfileName_TextChanged);
			// 
			// cmdUse
			// 
			this.cmdUse.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.cmdUse, "cmdUse");
			this.cmdUse.Name = "cmdUse";
			this.cmdUse.UseVisualStyleBackColor = true;
			this.cmdUse.Click += new System.EventHandler(this.cmdUse_Click);
			// 
			// ProfileInstaller
			// 
			this.AcceptButton = this.cmdUse;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.cmdUse);
			this.Controls.Add(this.txProfileName);
			this.Controls.Add(this.lbName);
			this.Controls.Add(this.selTemplate);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cmdTemplate);
			this.Controls.Add(this.cmdNewProfile);
			this.Controls.Add(this.selProfile);
			this.Controls.Add(this.lbProfile);
			this.Controls.Add(this.lbIntro);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ProfileInstaller";
			this.ShowIcon = false;
			this.TopMost = true;
			this.Load += new System.EventHandler(this.ProfileInstaller_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lbIntro;
		private System.Windows.Forms.Label lbProfile;
		private System.Windows.Forms.ComboBox selProfile;
		private System.Windows.Forms.Button cmdNewProfile;
		private System.Windows.Forms.Button cmdTemplate;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox selTemplate;
		private System.Windows.Forms.Label lbName;
		private System.Windows.Forms.TextBox txProfileName;
		private System.Windows.Forms.Button cmdUse;
	}
}