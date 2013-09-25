namespace JMS.DVB.DirectShow.UI
{
	partial class BDAWindow
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
			// Access module first
			if (null != m_Accessor)
			{
				// Shut down
				m_Accessor.Dispose();

				// Forget
				m_Accessor = null;
			}

			// Then the graph
			if (null != Graph)
			{
				// Shut down
				Graph.Dispose();

				// Forget
				Graph = null;
			}

			// Wizard created
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BDAWindow));
			this.SuspendLayout();
			// 
			// BDAWindow
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Blue;
			this.Name = "BDAWindow";
			this.ResumeLayout(false);

		}

		#endregion
	}
}
