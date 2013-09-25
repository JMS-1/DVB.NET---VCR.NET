namespace JMS.DVB.DeviceAccess.Editors
{
	partial class DeviceIndex
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceIndex));
			this.cmdSave = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.selIndex = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.selIndex)).BeginInit();
			this.SuspendLayout();
			// 
			// cmdSave
			// 
			this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.cmdSave, "cmdSave");
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.UseVisualStyleBackColor = true;
			this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.cmdCancel, "cmdCancel");
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// selIndex
			// 
			resources.ApplyResources(this.selIndex, "selIndex");
			this.selIndex.Name = "selIndex";
			this.selIndex.ValueChanged += new System.EventHandler(this.selIndex_ValueChanged);
			// 
			// DeviceIndex
			// 
			this.AcceptButton = this.cmdSave;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.Controls.Add(this.selIndex);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdSave);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DeviceIndex";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.DeviceIndex_Load);
			((System.ComponentModel.ISupportInitialize)(this.selIndex)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cmdSave;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.NumericUpDown selIndex;
	}
}