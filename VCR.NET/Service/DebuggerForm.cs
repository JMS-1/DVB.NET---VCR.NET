using System;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Threading;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Dient zum Debuggen des VCR.NET Recording Service ausserhalb der Windows Dienstumgebung.
    /// </summary>
    public partial class DebuggerForm : Form
    {
        /// <summary>
        /// Der zugehörige Dienst.
        /// </summary>
        private readonly Service m_service;

        /// <summary>
        /// Die Methode, die für den Übergang in den Schlafzustand verwendet werden soll.
        /// </summary>
        private readonly Func<bool, bool> s_initialSleepMethod;

        /// <summary>
        /// Erzeugt ein neues Steuerfenster.
        /// </summary>
        /// <param name="service">Der zugehörige Dienst.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Dienst angegeben.</exception>
        public DebuggerForm( Service service )
        {
            // Validate
            if (service == null)
                throw new ArgumentNullException( "service" );

            // Remember
            m_service = service;

            // Load designer stuff
            InitializeComponent();

            // Configure
            cmdAutomatic.Tag = (Action) (() => m_service.SendPowerCommand( PowerBroadcastStatus.ResumeAutomatic ));
            cmdResume.Tag = (Action) (() => m_service.SendPowerCommand( PowerBroadcastStatus.ResumeSuspend ));
            cmdHibernate.Tag = (Action) (() => m_service.VCRServer.TryHibernateIgnoringInteractiveUsers());
            cmdSuspend.Tag = (Action) (() => m_service.SendPowerCommand( PowerBroadcastStatus.Suspend ));
            
            // Remember old sleep method
            s_initialSleepMethod = Tools.SendSystemToSleep;

            // Overwrite
            Tools.SendSystemToSleep = SendToSleep;
        }

        /// <summary>
        /// Wird ausgelöst, wenn ein Übergang in den Schlafzustand erfolgen soll.
        /// </summary>
        /// <param name="useSuspend">Gesetzt, wenn S3 statt S4 verwendet werden soll.</param>
        /// <returns>Gesetzt, wenn der Übergang erlaubt ist.</returns>
        private bool SendToSleep( bool useSuspend )
        {
            // Prepare for delay send
            ThreadPool.QueueUserWorkItem( svc => ((Service) svc).SendPowerCommand( PowerBroadcastStatus.Suspend ), m_service );

            // Did it
            return true;
        }

        /// <summary>
        /// Beendet die Simulation.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdDone_Click( object sender, EventArgs e )
        {
            // Done
            Close();
        }

        /// <summary>
        /// Löst einen Befehl aus.
        /// </summary>
        /// <param name="sender">Eine Schaltfläche.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void FireAction( object sender, EventArgs e )
        {
            // Load action
            var command = (Button) sender;
            var action = (Action) command.Tag;

            // Fire
            action();
        }
    }
}
