using System;
using System.Windows.Forms;

namespace JMS.DVB.Favorites
{
	partial class ChannelSelector
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
				// Remove filter
				Application.RemoveMessageFilter(this);

				// Deregister
				Application.EnterThreadModal -= new EventHandler(Application_EnterThreadModal);
				Application.LeaveThreadModal -= new EventHandler(Application_LeaveThreadModal);

				// Cleanup
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChannelSelector));
			this.selChannel = new System.Windows.Forms.ComboBox();
			this.cmdConfig = new System.Windows.Forms.PictureBox();
			this.selAudio = new System.Windows.Forms.ComboBox();
			this.selService = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.cmdConfig)).BeginInit();
			this.SuspendLayout();
			// 
			// selChannel
			// 
			resources.ApplyResources(this.selChannel, "selChannel");
			this.selChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.selChannel.FormattingEnabled = true;
			this.selChannel.Name = "selChannel";
			this.selChannel.Sorted = true;
			this.selChannel.SelectionChangeCommitted += new System.EventHandler(this.selChannel_SelectionChangeCommitted);
			// 
			// cmdConfig
			// 
			resources.ApplyResources(this.cmdConfig, "cmdConfig");
			this.cmdConfig.Image = global::JMS.DVB.Properties.Resources.Chooser;
			this.cmdConfig.Name = "cmdConfig";
			this.cmdConfig.TabStop = false;
			this.cmdConfig.DoubleClick += new System.EventHandler(this.cmdConfig_DoubleClick);
			// 
			// selAudio
			// 
			resources.ApplyResources(this.selAudio, "selAudio");
			this.selAudio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.selAudio.FormattingEnabled = true;
			this.selAudio.Name = "selAudio";
			this.selAudio.Sorted = true;
			this.selAudio.SelectionChangeCommitted += new System.EventHandler(this.selAudio_SelectionChangeCommitted);
			// 
			// selService
			// 
			resources.ApplyResources(this.selService, "selService");
			this.selService.BackColor = System.Drawing.Color.Lime;
			this.selService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.selService.FormattingEnabled = true;
			this.selService.Name = "selService";
			this.selService.Sorted = true;
			this.selService.SelectionChangeCommitted += new System.EventHandler(this.selService_SelectionChangeCommitted);
			// 
			// ChannelSelector
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.selService);
			this.Controls.Add(this.selAudio);
			this.Controls.Add(this.cmdConfig);
			this.Controls.Add(this.selChannel);
			this.Name = "ChannelSelector";
			((System.ComponentModel.ISupportInitialize)(this.cmdConfig)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ComboBox selChannel;
		private PictureBox cmdConfig;
		private ComboBox selAudio;
		private ComboBox selService;


	}
}
