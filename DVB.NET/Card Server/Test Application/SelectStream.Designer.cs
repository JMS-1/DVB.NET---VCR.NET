namespace CardServerTester
{
    partial class SelectStream
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( SelectStream ) );
            this.selStation = new System.Windows.Forms.ComboBox();
            this.selMP2 = new System.Windows.Forms.ComboBox();
            this.selAC3 = new System.Windows.Forms.ComboBox();
            this.ckTTX = new System.Windows.Forms.CheckBox();
            this.ckEPG = new System.Windows.Forms.CheckBox();
            this.cmdMore = new System.Windows.Forms.Button();
            this.selSubtitles = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // selStation
            // 
            this.selStation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selStation.FormattingEnabled = true;
            resources.ApplyResources( this.selStation, "selStation" );
            this.selStation.Name = "selStation";
            this.selStation.Sorted = true;
            // 
            // selMP2
            // 
            this.selMP2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selMP2.FormattingEnabled = true;
            this.selMP2.Items.AddRange( new object[] {
            resources.GetString("selMP2.Items"),
            resources.GetString("selMP2.Items1"),
            resources.GetString("selMP2.Items2"),
            resources.GetString("selMP2.Items3")} );
            resources.ApplyResources( this.selMP2, "selMP2" );
            this.selMP2.Name = "selMP2";
            // 
            // selAC3
            // 
            this.selAC3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selAC3.FormattingEnabled = true;
            this.selAC3.Items.AddRange( new object[] {
            resources.GetString("selAC3.Items"),
            resources.GetString("selAC3.Items1"),
            resources.GetString("selAC3.Items2"),
            resources.GetString("selAC3.Items3")} );
            resources.ApplyResources( this.selAC3, "selAC3" );
            this.selAC3.Name = "selAC3";
            // 
            // ckTTX
            // 
            resources.ApplyResources( this.ckTTX, "ckTTX" );
            this.ckTTX.Name = "ckTTX";
            this.ckTTX.UseVisualStyleBackColor = true;
            // 
            // ckEPG
            // 
            resources.ApplyResources( this.ckEPG, "ckEPG" );
            this.ckEPG.Name = "ckEPG";
            this.ckEPG.UseVisualStyleBackColor = true;
            // 
            // cmdMore
            // 
            resources.ApplyResources( this.cmdMore, "cmdMore" );
            this.cmdMore.Name = "cmdMore";
            this.cmdMore.UseVisualStyleBackColor = true;
            this.cmdMore.Click += new System.EventHandler( this.cmdMore_Click );
            // 
            // selSubtitles
            // 
            this.selSubtitles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selSubtitles.FormattingEnabled = true;
            this.selSubtitles.Items.AddRange( new object[] {
            resources.GetString("selSubtitles.Items"),
            resources.GetString("selSubtitles.Items1"),
            resources.GetString("selSubtitles.Items2"),
            resources.GetString("selSubtitles.Items3")} );
            resources.ApplyResources( this.selSubtitles, "selSubtitles" );
            this.selSubtitles.Name = "selSubtitles";
            // 
            // SelectStream
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.cmdMore );
            this.Controls.Add( this.ckEPG );
            this.Controls.Add( this.ckTTX );
            this.Controls.Add( this.selSubtitles );
            this.Controls.Add( this.selAC3 );
            this.Controls.Add( this.selMP2 );
            this.Controls.Add( this.selStation );
            this.Name = "SelectStream";
            this.Load += new System.EventHandler( this.SelectStream_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox selStation;
        private System.Windows.Forms.ComboBox selMP2;
        private System.Windows.Forms.ComboBox selAC3;
        private System.Windows.Forms.CheckBox ckTTX;
        private System.Windows.Forms.CheckBox ckEPG;
        private System.Windows.Forms.Button cmdMore;
        private System.Windows.Forms.ComboBox selSubtitles;
    }
}
