namespace DVBNETAdmin
{
    partial class AdminMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdminMain));
            this.pnlWizard = new System.Windows.Forms.Panel();
            this.pnlNavigation = new System.Windows.Forms.Panel();
            this.cmdFinish = new System.Windows.Forms.Button();
            this.cmdNext = new System.Windows.Forms.Button();
            this.cmdPrev = new System.Windows.Forms.Button();
            this.pnlHeadLine = new System.Windows.Forms.Panel();
            this.lbCurrentPhase = new System.Windows.Forms.Label();
            this.pnlNavigation.SuspendLayout();
            this.pnlHeadLine.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlWizard
            // 
            resources.ApplyResources(this.pnlWizard, "pnlWizard");
            this.pnlWizard.Name = "pnlWizard";
            // 
            // pnlNavigation
            // 
            resources.ApplyResources(this.pnlNavigation, "pnlNavigation");
            this.pnlNavigation.Controls.Add(this.cmdFinish);
            this.pnlNavigation.Controls.Add(this.cmdNext);
            this.pnlNavigation.Controls.Add(this.cmdPrev);
            this.pnlNavigation.Name = "pnlNavigation";
            // 
            // cmdFinish
            // 
            resources.ApplyResources(this.cmdFinish, "cmdFinish");
            this.cmdFinish.Name = "cmdFinish";
            this.cmdFinish.UseVisualStyleBackColor = true;
            this.cmdFinish.Click += new System.EventHandler(this.cmdFinish_Click);
            // 
            // cmdNext
            // 
            resources.ApplyResources(this.cmdNext, "cmdNext");
            this.cmdNext.Name = "cmdNext";
            this.cmdNext.UseVisualStyleBackColor = true;
            this.cmdNext.Click += new System.EventHandler(this.cmdNext_Click);
            // 
            // cmdPrev
            // 
            resources.ApplyResources(this.cmdPrev, "cmdPrev");
            this.cmdPrev.Name = "cmdPrev";
            this.cmdPrev.UseVisualStyleBackColor = true;
            this.cmdPrev.Click += new System.EventHandler(this.cmdPrev_Click);
            // 
            // pnlHeadLine
            // 
            resources.ApplyResources(this.pnlHeadLine, "pnlHeadLine");
            this.pnlHeadLine.Controls.Add(this.lbCurrentPhase);
            this.pnlHeadLine.Name = "pnlHeadLine";
            // 
            // lbCurrentPhase
            // 
            resources.ApplyResources(this.lbCurrentPhase, "lbCurrentPhase");
            this.lbCurrentPhase.Name = "lbCurrentPhase";
            // 
            // AdminMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlHeadLine);
            this.Controls.Add(this.pnlNavigation);
            this.Controls.Add(this.pnlWizard);
            this.Name = "AdminMain";
            this.Load += new System.EventHandler(this.AdminMain_Load);
            this.pnlNavigation.ResumeLayout(false);
            this.pnlHeadLine.ResumeLayout(false);
            this.pnlHeadLine.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlWizard;
        private System.Windows.Forms.Panel pnlNavigation;
        private System.Windows.Forms.Panel pnlHeadLine;
        private System.Windows.Forms.Label lbCurrentPhase;
        private System.Windows.Forms.Button cmdFinish;
        private System.Windows.Forms.Button cmdNext;
        private System.Windows.Forms.Button cmdPrev;

    }
}

