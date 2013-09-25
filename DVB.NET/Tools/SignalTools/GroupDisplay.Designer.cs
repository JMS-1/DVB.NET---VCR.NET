namespace JMS.DVB.Administration.Tools
{
    partial class GroupDisplay
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( GroupDisplay ) );
            this.picPaint = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize) (this.picPaint)).BeginInit();
            this.SuspendLayout();
            // 
            // picPaint
            // 
            resources.ApplyResources( this.picPaint, "picPaint" );
            this.picPaint.Name = "picPaint";
            this.picPaint.TabStop = false;
            // 
            // GroupDisplay
            // 
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.picPaint );
            this.Name = "GroupDisplay";
            this.Load += new System.EventHandler( this.GroupDisplay_Load );
            ((System.ComponentModel.ISupportInitialize) (this.picPaint)).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.PictureBox picPaint;
    }
}
