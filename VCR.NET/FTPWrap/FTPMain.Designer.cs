using System;
using System.Net.Sockets;

namespace FTPWrap
{
	partial class FTPMain
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
				// Destroy FTP server socket
				EndFTPServer();

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FTPMain));
			this.notifier = new System.Windows.Forms.NotifyIcon(this.components);
			this.waitExit = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// notifier
			// 
			resources.ApplyResources(this.notifier, "notifier");
			this.notifier.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnTerminate);
			// 
			// waitExit
			// 
			this.waitExit.Interval = 500;
			this.waitExit.Tick += new System.EventHandler(this.waitExit_Tick);
			// 
			// Form1
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FTPMain";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon notifier;
		private System.Windows.Forms.Timer waitExit;
	}
}

