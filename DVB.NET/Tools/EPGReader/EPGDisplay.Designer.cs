namespace EPGReader
{
	partial class EPGDisplay
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EPGDisplay));
			this.lbName = new System.Windows.Forms.Label();
			this.lbTime = new System.Windows.Forms.Label();
			this.lbDescription = new System.Windows.Forms.Label();
			this.cmdCopy = new System.Windows.Forms.Button();
			this.cmdClose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lbName
			// 
			this.lbName.AccessibleDescription = null;
			this.lbName.AccessibleName = null;
			resources.ApplyResources(this.lbName, "lbName");
			this.lbName.Name = "lbName";
			// 
			// lbTime
			// 
			this.lbTime.AccessibleDescription = null;
			this.lbTime.AccessibleName = null;
			resources.ApplyResources(this.lbTime, "lbTime");
			this.lbTime.Name = "lbTime";
			// 
			// lbDescription
			// 
			this.lbDescription.AccessibleDescription = null;
			this.lbDescription.AccessibleName = null;
			resources.ApplyResources(this.lbDescription, "lbDescription");
			this.lbDescription.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lbDescription.Name = "lbDescription";
			// 
			// cmdCopy
			// 
			this.cmdCopy.AccessibleDescription = null;
			this.cmdCopy.AccessibleName = null;
			resources.ApplyResources(this.cmdCopy, "cmdCopy");
			this.cmdCopy.BackgroundImage = null;
			this.cmdCopy.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCopy.Font = null;
			this.cmdCopy.Name = "cmdCopy";
			this.cmdCopy.UseVisualStyleBackColor = true;
			this.cmdCopy.Click += new System.EventHandler(this.cmdCopy_Click);
			// 
			// cmdClose
			// 
			this.cmdClose.AccessibleDescription = null;
			this.cmdClose.AccessibleName = null;
			resources.ApplyResources(this.cmdClose, "cmdClose");
			this.cmdClose.BackgroundImage = null;
			this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdClose.Font = null;
			this.cmdClose.Name = "cmdClose";
			this.cmdClose.UseVisualStyleBackColor = true;
			this.cmdClose.Click += new System.EventHandler(this.cmdCopy_Click);
			// 
			// EPGDisplay
			// 
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.Controls.Add(this.cmdClose);
			this.Controls.Add(this.cmdCopy);
			this.Controls.Add(this.lbDescription);
			this.Controls.Add(this.lbTime);
			this.Controls.Add(this.lbName);
			this.Font = null;
			this.Icon = null;
			this.MinimizeBox = false;
			this.Name = "EPGDisplay";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.EPGDisplay_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lbName;
		private System.Windows.Forms.Label lbTime;
		private System.Windows.Forms.Label lbDescription;
		private System.Windows.Forms.Button cmdCopy;
		private System.Windows.Forms.Button cmdClose;
	}
}