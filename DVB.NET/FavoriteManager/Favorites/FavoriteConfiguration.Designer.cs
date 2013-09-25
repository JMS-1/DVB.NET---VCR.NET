namespace JMS.DVB.Favorites
{
	partial class FavoriteConfiguration
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FavoriteConfiguration));
			this.label1 = new System.Windows.Forms.Label();
			this.selLanguage = new System.Windows.Forms.ComboBox();
			this.ckUse = new System.Windows.Forms.CheckBox();
			this.pnlLists = new System.Windows.Forms.Panel();
			this.lstAll = new System.Windows.Forms.ListBox();
			this.lstFavorites = new System.Windows.Forms.ListBox();
			this.cmdSave = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.ckAC3 = new System.Windows.Forms.CheckBox();
			this.pnlLists.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// selLanguage
			// 
			this.selLanguage.FormattingEnabled = true;
			resources.ApplyResources(this.selLanguage, "selLanguage");
			this.selLanguage.Name = "selLanguage";
			this.selLanguage.Sorted = true;
			this.selLanguage.SelectionChangeCommitted += new System.EventHandler(this.selLanguage_SelectionChangeCommitted);
			this.selLanguage.TextChanged += new System.EventHandler(this.selLanguage_SelectionChangeCommitted);
			// 
			// ckUse
			// 
			resources.ApplyResources(this.ckUse, "ckUse");
			this.ckUse.Name = "ckUse";
			this.ckUse.UseVisualStyleBackColor = true;
			this.ckUse.CheckedChanged += new System.EventHandler(this.ckUse_CheckedChanged);
			// 
			// pnlLists
			// 
			resources.ApplyResources(this.pnlLists, "pnlLists");
			this.pnlLists.Controls.Add(this.lstAll);
			this.pnlLists.Controls.Add(this.lstFavorites);
			this.pnlLists.Name = "pnlLists";
			this.pnlLists.SizeChanged += new System.EventHandler(this.pnlLists_SizeChanged);
			// 
			// lstAll
			// 
			resources.ApplyResources(this.lstAll, "lstAll");
			this.lstAll.FormattingEnabled = true;
			this.lstAll.Name = "lstAll";
			this.lstAll.Sorted = true;
			this.lstAll.DoubleClick += new System.EventHandler(this.lstAll_DoubleClick);
			this.lstAll.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.lstAll_KeyPress);
			// 
			// lstFavorites
			// 
			resources.ApplyResources(this.lstFavorites, "lstFavorites");
			this.lstFavorites.FormattingEnabled = true;
			this.lstFavorites.Name = "lstFavorites";
			this.lstFavorites.DoubleClick += new System.EventHandler(this.lstFavorites_DoubleClick);
			this.lstFavorites.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lstFavorites_KeyUp);
			this.lstFavorites.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstFavorites_KeyDown);
			// 
			// cmdSave
			// 
			resources.ApplyResources(this.cmdSave, "cmdSave");
			this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.UseVisualStyleBackColor = true;
			this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
			// 
			// cmdCancel
			// 
			resources.ApplyResources(this.cmdCancel, "cmdCancel");
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// ckAC3
			// 
			resources.ApplyResources(this.ckAC3, "ckAC3");
			this.ckAC3.Name = "ckAC3";
			this.ckAC3.UseVisualStyleBackColor = true;
			this.ckAC3.CheckedChanged += new System.EventHandler(this.ckUse_CheckedChanged);
			// 
			// FavoriteConfiguration
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdSave);
			this.Controls.Add(this.pnlLists);
			this.Controls.Add(this.ckAC3);
			this.Controls.Add(this.ckUse);
			this.Controls.Add(this.selLanguage);
			this.Controls.Add(this.label1);
			this.Name = "FavoriteConfiguration";
			this.Load += new System.EventHandler(this.FavoriteConfiguration_Load);
			this.pnlLists.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox selLanguage;
		private System.Windows.Forms.CheckBox ckUse;
		private System.Windows.Forms.Panel pnlLists;
		private System.Windows.Forms.Button cmdSave;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.ListBox lstFavorites;
		private System.Windows.Forms.ListBox lstAll;
		private System.Windows.Forms.CheckBox ckAC3;
	}
}
