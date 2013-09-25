namespace JMS.DVB.DirectShow.UI
{
    partial class OverlayWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picOSD = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize) (this.picOSD)).BeginInit();
            this.SuspendLayout();
            // 
            // picOSD
            // 
            this.picOSD.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.picOSD.Enabled = false;
            this.picOSD.Location = new System.Drawing.Point( 12, 12 );
            this.picOSD.Name = "picOSD";
            this.picOSD.Size = new System.Drawing.Size( 260, 240 );
            this.picOSD.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picOSD.TabIndex = 0;
            this.picOSD.TabStop = false;
            // 
            // OverlayWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 284, 264 );
            this.Controls.Add( this.picOSD );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "OverlayWindow";
            this.ShowInTaskbar = false;
            this.Text = "OverlayWindow";
            ((System.ComponentModel.ISupportInitialize) (this.picOSD)).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.PictureBox picOSD;
    }
}