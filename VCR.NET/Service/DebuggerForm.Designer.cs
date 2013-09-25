namespace JMS.DVBVCR.RecordingService
{
    partial class DebuggerForm
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
            if (disposing)
            {
                // Get rid of helper
                using (components)
                    components = null;

                // Restore sleep method
                Tools.SendSystemToSleep = s_initialSleepMethod;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebuggerForm));
            this.cmdDone = new System.Windows.Forms.Button();
            this.cmdSuspend = new System.Windows.Forms.Button();
            this.cmdResume = new System.Windows.Forms.Button();
            this.cmdAutomatic = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdHibernate = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdDone
            // 
            resources.ApplyResources(this.cmdDone, "cmdDone");
            this.cmdDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdDone.Name = "cmdDone";
            this.cmdDone.UseVisualStyleBackColor = true;
            this.cmdDone.Click += new System.EventHandler(this.cmdDone_Click);
            // 
            // cmdSuspend
            // 
            resources.ApplyResources(this.cmdSuspend, "cmdSuspend");
            this.cmdSuspend.Name = "cmdSuspend";
            this.cmdSuspend.UseVisualStyleBackColor = true;
            this.cmdSuspend.Click += new System.EventHandler(this.FireAction);
            // 
            // cmdResume
            // 
            resources.ApplyResources(this.cmdResume, "cmdResume");
            this.cmdResume.Name = "cmdResume";
            this.cmdResume.UseVisualStyleBackColor = true;
            this.cmdResume.Click += new System.EventHandler(this.FireAction);
            // 
            // cmdAutomatic
            // 
            resources.ApplyResources(this.cmdAutomatic, "cmdAutomatic");
            this.cmdAutomatic.Name = "cmdAutomatic";
            this.cmdAutomatic.UseVisualStyleBackColor = true;
            this.cmdAutomatic.Click += new System.EventHandler(this.FireAction);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmdSuspend);
            this.groupBox1.Controls.Add(this.cmdHibernate);
            this.groupBox1.Controls.Add(this.cmdResume);
            this.groupBox1.Controls.Add(this.cmdAutomatic);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // cmdHibernate
            // 
            resources.ApplyResources(this.cmdHibernate, "cmdHibernate");
            this.cmdHibernate.Name = "cmdHibernate";
            this.cmdHibernate.UseVisualStyleBackColor = true;
            this.cmdHibernate.Click += new System.EventHandler(this.FireAction);
            // 
            // DebuggerForm
            // 
            this.AcceptButton = this.cmdDone;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cmdDone);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DebuggerForm";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdDone;
        private System.Windows.Forms.Button cmdSuspend;
        private System.Windows.Forms.Button cmdResume;
        private System.Windows.Forms.Button cmdAutomatic;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button cmdHibernate;
    }
}