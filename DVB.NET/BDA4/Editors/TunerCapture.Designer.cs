namespace JMS.DVB.DeviceAccess.Editors
{
	partial class TunerCapture
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( TunerCapture ) );
            this.cmdSave = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lstDevices = new System.Windows.Forms.ListBox();
            this.tips = new System.Windows.Forms.ToolTip( this.components );
            this.cmdReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdSave
            // 
            this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources( this.cmdSave, "cmdSave" );
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.UseVisualStyleBackColor = true;
            this.cmdSave.Click += new System.EventHandler( this.cmdSave_Click );
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources( this.cmdCancel, "cmdCancel" );
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // lstDevices
            // 
            resources.ApplyResources( this.lstDevices, "lstDevices" );
            this.lstDevices.FormattingEnabled = true;
            this.lstDevices.Name = "lstDevices";
            this.lstDevices.SelectedIndexChanged += new System.EventHandler( this.lstDevices_SelectedIndexChanged );
            this.lstDevices.DoubleClick += new System.EventHandler( this.lstDevices_DoubleClick );
            // 
            // cmdReset
            // 
            this.cmdReset.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources( this.cmdReset, "cmdReset" );
            this.cmdReset.Name = "cmdReset";
            this.cmdReset.UseVisualStyleBackColor = true;
            this.cmdReset.Click += new System.EventHandler( this.cmdReset_Click );
            // 
            // TunerCapture
            // 
            this.AcceptButton = this.cmdSave;
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.Controls.Add( this.lstDevices );
            this.Controls.Add( this.cmdCancel );
            this.Controls.Add( this.cmdReset );
            this.Controls.Add( this.cmdSave );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TunerCapture";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler( this.TunerCapture_Load );
            this.ResumeLayout( false );

		}

		#endregion

		private System.Windows.Forms.Button cmdSave;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.ListBox lstDevices;
		private System.Windows.Forms.ToolTip tips;
        private System.Windows.Forms.Button cmdReset;
	}
}