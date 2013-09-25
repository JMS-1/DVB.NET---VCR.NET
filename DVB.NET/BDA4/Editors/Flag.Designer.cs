namespace JMS.DVB.DeviceAccess.Editors
{
	partial class Flag
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Flag));
			this.ckSetting = new System.Windows.Forms.CheckBox();
			this.cmdSave = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ckSetting
			// 
			this.ckSetting.AccessibleDescription = null;
			this.ckSetting.AccessibleName = null;
			resources.ApplyResources(this.ckSetting, "ckSetting");
			this.ckSetting.BackgroundImage = null;
			this.ckSetting.Font = null;
			this.ckSetting.Name = "ckSetting";
			this.ckSetting.UseVisualStyleBackColor = true;
			this.ckSetting.CheckedChanged += new System.EventHandler(this.ckSetting_CheckedChanged);
			// 
			// cmdSave
			// 
			this.cmdSave.AccessibleDescription = null;
			this.cmdSave.AccessibleName = null;
			resources.ApplyResources(this.cmdSave, "cmdSave");
			this.cmdSave.BackgroundImage = null;
			this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdSave.Font = null;
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.UseVisualStyleBackColor = true;
			this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.AccessibleDescription = null;
			this.cmdCancel.AccessibleName = null;
			resources.ApplyResources(this.cmdCancel, "cmdCancel");
			this.cmdCancel.BackgroundImage = null;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Font = null;
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// Flag
			// 
			this.AcceptButton = this.cmdSave;
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.CancelButton = this.cmdCancel;
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdSave);
			this.Controls.Add(this.ckSetting);
			this.Font = null;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = null;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Flag";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.Flag_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox ckSetting;
		private System.Windows.Forms.Button cmdSave;
		private System.Windows.Forms.Button cmdCancel;
	}
}