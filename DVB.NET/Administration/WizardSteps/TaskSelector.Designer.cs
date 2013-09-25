namespace DVBNETAdmin.WizardSteps
{
    partial class TaskSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( TaskSelector ) );
            this.grpTasks = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // grpTasks
            // 
            this.grpTasks.AccessibleDescription = null;
            this.grpTasks.AccessibleName = null;
            resources.ApplyResources( this.grpTasks, "grpTasks" );
            this.grpTasks.BackgroundImage = null;
            this.grpTasks.Font = null;
            this.grpTasks.Name = "grpTasks";
            this.grpTasks.TabStop = false;
            // 
            // TaskSelector
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources( this, "$this" );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add( this.grpTasks );
            this.Font = null;
            this.Name = "TaskSelector";
            this.Load += new System.EventHandler( this.TaskSelector_Load );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.GroupBox grpTasks;
    }
}
