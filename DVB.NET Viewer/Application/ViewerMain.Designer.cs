namespace DVBNETViewer
{
	partial class ViewerMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewerMain));
			this.theViewer = new JMS.DVB.Viewer.ViewerControl();
			this.tickStart = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// theViewer
			// 
			this.theViewer.BackColor = System.Drawing.SystemColors.Control;
			resources.ApplyResources(this.theViewer, "theViewer");
			this.theViewer.Name = "theViewer";
			// 
			// tickStart
			// 
			this.tickStart.Interval = 250;
			this.tickStart.Tick += new System.EventHandler(this.tickStart_Tick);
			// 
			// ViewerMain
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.theViewer);
			this.MaximizeBox = false;
			this.Name = "ViewerMain";
			this.Load += new System.EventHandler(this.ViewerMain_Load);
			this.SizeChanged += new System.EventHandler(this.ViewerMain_SizeChanged);
			this.LocationChanged += new System.EventHandler(this.ViewerMain_SizeChanged);
			this.ResumeLayout(false);

		}

		#endregion

		private JMS.DVB.Viewer.ViewerControl theViewer;
		private System.Windows.Forms.Timer tickStart;
	}
}

