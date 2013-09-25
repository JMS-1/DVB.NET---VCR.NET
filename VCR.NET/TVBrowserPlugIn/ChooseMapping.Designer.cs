namespace TVBrowserPlugIn
{
	partial class ChooseMapping
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseMapping));
			this.selStation = new System.Windows.Forms.ComboBox();
			this.cmdChoose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// selStation
			// 
			this.selStation.AccessibleDescription = null;
			this.selStation.AccessibleName = null;
			resources.ApplyResources(this.selStation, "selStation");
			this.selStation.BackgroundImage = null;
			this.selStation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.selStation.Font = null;
			this.selStation.FormattingEnabled = true;
			this.selStation.Name = "selStation";
			this.selStation.Sorted = true;
			this.selStation.SelectionChangeCommitted += new System.EventHandler(this.selStation_SelectionChangeCommitted);
			// 
			// cmdChoose
			// 
			this.cmdChoose.AccessibleDescription = null;
			this.cmdChoose.AccessibleName = null;
			resources.ApplyResources(this.cmdChoose, "cmdChoose");
			this.cmdChoose.BackgroundImage = null;
			this.cmdChoose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdChoose.Font = null;
			this.cmdChoose.Name = "cmdChoose";
			this.cmdChoose.UseVisualStyleBackColor = true;
			this.cmdChoose.Click += new System.EventHandler(this.cmdChoose_Click);
			// 
			// ChooseMapping
			// 
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.Controls.Add(this.cmdChoose);
			this.Controls.Add(this.selStation);
			this.Font = null;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ChooseMapping";
			this.Load += new System.EventHandler(this.ChooseMapping_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox selStation;
		private System.Windows.Forms.Button cmdChoose;
	}
}