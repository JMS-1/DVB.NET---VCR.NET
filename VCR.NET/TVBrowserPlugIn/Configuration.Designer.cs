namespace TVBrowserPlugIn
{
	partial class Configuration
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Configuration));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.txConnection = new System.Windows.Forms.TextBox();
			this.txPre = new System.Windows.Forms.TextBox();
			this.txPost = new System.Windows.Forms.TextBox();
			this.lstMappings = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.cmdSave = new System.Windows.Forms.Button();
			this.errors = new System.Windows.Forms.ErrorProvider(this.components);
			this.ckOpen = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.errors)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AccessibleDescription = null;
			this.label1.AccessibleName = null;
			resources.ApplyResources(this.label1, "label1");
			this.errors.SetError(this.label1, resources.GetString("label1.Error"));
			this.label1.Font = null;
			this.errors.SetIconAlignment(this.label1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label1.IconAlignment"))));
			this.errors.SetIconPadding(this.label1, ((int)(resources.GetObject("label1.IconPadding"))));
			this.label1.Name = "label1";
			// 
			// label2
			// 
			this.label2.AccessibleDescription = null;
			this.label2.AccessibleName = null;
			resources.ApplyResources(this.label2, "label2");
			this.errors.SetError(this.label2, resources.GetString("label2.Error"));
			this.label2.Font = null;
			this.errors.SetIconAlignment(this.label2, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label2.IconAlignment"))));
			this.errors.SetIconPadding(this.label2, ((int)(resources.GetObject("label2.IconPadding"))));
			this.label2.Name = "label2";
			// 
			// label3
			// 
			this.label3.AccessibleDescription = null;
			this.label3.AccessibleName = null;
			resources.ApplyResources(this.label3, "label3");
			this.errors.SetError(this.label3, resources.GetString("label3.Error"));
			this.label3.Font = null;
			this.errors.SetIconAlignment(this.label3, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label3.IconAlignment"))));
			this.errors.SetIconPadding(this.label3, ((int)(resources.GetObject("label3.IconPadding"))));
			this.label3.Name = "label3";
			// 
			// label4
			// 
			this.label4.AccessibleDescription = null;
			this.label4.AccessibleName = null;
			resources.ApplyResources(this.label4, "label4");
			this.errors.SetError(this.label4, resources.GetString("label4.Error"));
			this.label4.Font = null;
			this.errors.SetIconAlignment(this.label4, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label4.IconAlignment"))));
			this.errors.SetIconPadding(this.label4, ((int)(resources.GetObject("label4.IconPadding"))));
			this.label4.Name = "label4";
			// 
			// txConnection
			// 
			this.txConnection.AccessibleDescription = null;
			this.txConnection.AccessibleName = null;
			resources.ApplyResources(this.txConnection, "txConnection");
			this.txConnection.BackgroundImage = null;
			this.errors.SetError(this.txConnection, resources.GetString("txConnection.Error"));
			this.txConnection.Font = null;
			this.errors.SetIconAlignment(this.txConnection, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txConnection.IconAlignment"))));
			this.errors.SetIconPadding(this.txConnection, ((int)(resources.GetObject("txConnection.IconPadding"))));
			this.txConnection.Name = "txConnection";
			this.txConnection.TextChanged += new System.EventHandler(this.CheckInput);
			// 
			// txPre
			// 
			this.txPre.AccessibleDescription = null;
			this.txPre.AccessibleName = null;
			resources.ApplyResources(this.txPre, "txPre");
			this.txPre.BackgroundImage = null;
			this.errors.SetError(this.txPre, resources.GetString("txPre.Error"));
			this.txPre.Font = null;
			this.errors.SetIconAlignment(this.txPre, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txPre.IconAlignment"))));
			this.errors.SetIconPadding(this.txPre, ((int)(resources.GetObject("txPre.IconPadding"))));
			this.txPre.Name = "txPre";
			this.txPre.TextChanged += new System.EventHandler(this.CheckInput);
			// 
			// txPost
			// 
			this.txPost.AccessibleDescription = null;
			this.txPost.AccessibleName = null;
			resources.ApplyResources(this.txPost, "txPost");
			this.txPost.BackgroundImage = null;
			this.errors.SetError(this.txPost, resources.GetString("txPost.Error"));
			this.txPost.Font = null;
			this.errors.SetIconAlignment(this.txPost, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txPost.IconAlignment"))));
			this.errors.SetIconPadding(this.txPost, ((int)(resources.GetObject("txPost.IconPadding"))));
			this.txPost.Name = "txPost";
			this.txPost.TextChanged += new System.EventHandler(this.CheckInput);
			// 
			// lstMappings
			// 
			this.lstMappings.AccessibleDescription = null;
			this.lstMappings.AccessibleName = null;
			resources.ApplyResources(this.lstMappings, "lstMappings");
			this.lstMappings.BackgroundImage = null;
			this.lstMappings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.errors.SetError(this.lstMappings, resources.GetString("lstMappings.Error"));
			this.lstMappings.Font = null;
			this.lstMappings.FullRowSelect = true;
			this.lstMappings.GridLines = true;
			this.lstMappings.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.errors.SetIconAlignment(this.lstMappings, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("lstMappings.IconAlignment"))));
			this.errors.SetIconPadding(this.lstMappings, ((int)(resources.GetObject("lstMappings.IconPadding"))));
			this.lstMappings.MultiSelect = false;
			this.lstMappings.Name = "lstMappings";
			this.lstMappings.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.lstMappings.UseCompatibleStateImageBehavior = false;
			this.lstMappings.View = System.Windows.Forms.View.Details;
			this.lstMappings.DoubleClick += new System.EventHandler(this.lstMappings_DoubleClick);
			// 
			// columnHeader1
			// 
			resources.ApplyResources(this.columnHeader1, "columnHeader1");
			// 
			// columnHeader2
			// 
			resources.ApplyResources(this.columnHeader2, "columnHeader2");
			// 
			// cmdSave
			// 
			this.cmdSave.AccessibleDescription = null;
			this.cmdSave.AccessibleName = null;
			resources.ApplyResources(this.cmdSave, "cmdSave");
			this.cmdSave.BackgroundImage = null;
			this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.errors.SetError(this.cmdSave, resources.GetString("cmdSave.Error"));
			this.cmdSave.Font = null;
			this.errors.SetIconAlignment(this.cmdSave, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cmdSave.IconAlignment"))));
			this.errors.SetIconPadding(this.cmdSave, ((int)(resources.GetObject("cmdSave.IconPadding"))));
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.UseVisualStyleBackColor = true;
			this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
			// 
			// errors
			// 
			this.errors.ContainerControl = this;
			resources.ApplyResources(this.errors, "errors");
			// 
			// ckOpen
			// 
			this.ckOpen.AccessibleDescription = null;
			this.ckOpen.AccessibleName = null;
			resources.ApplyResources(this.ckOpen, "ckOpen");
			this.ckOpen.BackgroundImage = null;
			this.errors.SetError(this.ckOpen, resources.GetString("ckOpen.Error"));
			this.ckOpen.Font = null;
			this.errors.SetIconAlignment(this.ckOpen, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("ckOpen.IconAlignment"))));
			this.errors.SetIconPadding(this.ckOpen, ((int)(resources.GetObject("ckOpen.IconPadding"))));
			this.ckOpen.Name = "ckOpen";
			this.ckOpen.UseVisualStyleBackColor = true;
			this.ckOpen.CheckedChanged += new System.EventHandler(this.CheckInput);
			// 
			// Configuration
			// 
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.Controls.Add(this.ckOpen);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cmdSave);
			this.Controls.Add(this.lstMappings);
			this.Controls.Add(this.txPost);
			this.Controls.Add(this.txPre);
			this.Controls.Add(this.txConnection);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Font = null;
			this.Name = "Configuration";
			this.Load += new System.EventHandler(this.Configuration_Load);
			((System.ComponentModel.ISupportInitialize)(this.errors)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txConnection;
		private System.Windows.Forms.TextBox txPre;
		private System.Windows.Forms.TextBox txPost;
		private System.Windows.Forms.ListView lstMappings;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button cmdSave;
		private System.Windows.Forms.ErrorProvider errors;
		private System.Windows.Forms.CheckBox ckOpen;
	}
}