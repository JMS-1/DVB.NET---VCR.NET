using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace VCRControlCenter
{
    /// <summary>
    /// Summary description for HibernateDialog.
    /// </summary>
    public class HibernateDialog : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label lbInfo;
        private System.Windows.Forms.Button cmdCancl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private DateTime m_FinishTime = DateTime.UtcNow;
        private volatile bool m_Closed = false;
        private string m_Format = null;

        private ProcessingItem m_forHibernate;

        /// <summary>
        /// Create a new dialog instance.
        /// </summary>
        public HibernateDialog()
        {
            // Required for Windows Form Designer support
            InitializeComponent();

            // Remember
            m_Format = lbInfo.Text;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( HibernateDialog ) );
            this.lbInfo = new System.Windows.Forms.Label();
            this.cmdCancl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbInfo
            // 
            this.lbInfo.AccessibleDescription = null;
            this.lbInfo.AccessibleName = null;
            resources.ApplyResources( this.lbInfo, "lbInfo" );
            this.lbInfo.ForeColor = System.Drawing.Color.FromArgb( ((int) (((byte) (192)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))) );
            this.lbInfo.Name = "lbInfo";
            // 
            // cmdCancl
            // 
            this.cmdCancl.AccessibleDescription = null;
            this.cmdCancl.AccessibleName = null;
            resources.ApplyResources( this.cmdCancl, "cmdCancl" );
            this.cmdCancl.BackgroundImage = null;
            this.cmdCancl.Font = null;
            this.cmdCancl.Name = "cmdCancl";
            this.cmdCancl.Click += new System.EventHandler( this.cmdCancl_Click );
            // 
            // HibernateDialog
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources( this, "$this" );
            this.BackgroundImage = null;
            this.Controls.Add( this.cmdCancl );
            this.Controls.Add( this.lbInfo );
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = null;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HibernateDialog";
            this.TopMost = true;
            this.Load += new System.EventHandler( this.HibernateDialog_Load );
            this.ResumeLayout( false );

        }
        #endregion

        private void cmdCancl_Click( object sender, System.EventArgs e )
        {
            // Hide me
            Visible = false;
        }

        private void HibernateDialog_Load( object sender, System.EventArgs e )
        {
        }

        /// <summary>
        /// Start the dialog with the indicated maximum wait time.
        /// </summary>
        /// <param name="minutesToWait">Time to wait for an user action.</param>
        public void Start( int minutesToWait )
        {
            // Remember the end time
            m_FinishTime = DateTime.UtcNow.AddMinutes( minutesToWait );

            // Show
            bool bTest = MustHibernateNow;
        }

        /// <summary>
        /// Check if we should hibernate the system right now.
        /// </summary>
        public bool MustHibernateNow
        {
            get
            {
                // We are closed
                if (m_Closed)
                    return false;

                // Calculate the time to wait
                TimeSpan tWait = m_FinishTime - DateTime.UtcNow;

                // Get seconds
                int dSecs = (int) Math.Max( 0, tWait.TotalSeconds );

                // Get the number of minutes
                int nMins = (dSecs + 59) / 60;

                // Check it
                lbInfo.Text = string.Format( m_Format, nMins );

                // Go on
                return (dSecs <= 0) && Visible;
            }
        }

        /// <summary>
        /// Sets <see cref="IsClosed"/>.
        /// </summary>
        /// <param name="e">Ignored.</param>
        protected override void OnClosing( CancelEventArgs e )
        {
            // We are closed
            m_Closed = true;

            // Forward
            base.OnClosing( e );
        }

        /// <summary>
        /// Report if this window has been closed
        /// </summary>
        public bool IsClosed { get { return m_Closed; } }

        /// <summary>
        /// Wird aufgerufen, wenn das Fenster geschlossen wurde.
        /// </summary>
        /// <remarks>
        /// Wir befinden uns aber noch in der UI Schleife!
        /// </remarks>
        /// <param name="e">Wird ignoriert.</param>
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            // Initiate hibernation
            var controller = Interlocked.Exchange( ref m_forHibernate, null );
            if (controller != null)
                try
                {
                    // Do the web call on a separate thread to allow the UI operation to finish properly
                    Task.Run( () => VCRNETRestProxy.TryHibernate( controller.EndPoint ) );
                }
                catch
                {
                    // Ignore any error
                }
        }

        /// <summary>
        /// Forderrt zum Einschläfern des Dienstrechners auf.
        /// </summary>
        /// <param name="controller">Der zu verwendende Dienstrechner.</param>
        internal void TryHibernate( ProcessingItem controller )
        {
            // Fire here after close
            m_forHibernate = controller;

            // Close
            Close();
        }
    }
}
