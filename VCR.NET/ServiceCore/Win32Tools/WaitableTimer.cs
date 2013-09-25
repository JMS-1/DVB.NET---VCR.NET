using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Microsoft.Win32.SafeHandles;


namespace JMS.DVBVCR.RecordingService.Win32Tools
{
    /// <summary>
    /// Implements a timer which the process can be waiting on. The 
    /// timer supports waking up the system from a hibernated state.
    /// </summary>
    public unsafe sealed class WaitableTimer : WaitHandle
    {
        /// <summary>
        /// Wrap the system function <i>SetWaitableTimer</i>.
        /// </summary>
        [DllImport( "Kernel32.dll", EntryPoint = "SetWaitableTimer", SetLastError = true )]
        [SuppressUnmanagedCodeSecurity]
        static extern private bool SetWaitableTimer( SafeWaitHandle hTimer, Int64* pDue, Int32 lPeriod, IntPtr rNotify, IntPtr pArgs, bool bResume );

        /// <summary>
        /// Wrap the system function <i>CreateWaitableTimer</i>.
        /// </summary>
        [DllImport( "Kernel32.dll", EntryPoint = "CreateWaitableTimer" )]
        [SuppressUnmanagedCodeSecurity]
        static extern private SafeWaitHandle CreateWaitableTimer( IntPtr pSec, bool bManual, string szName );

        /// <summary>
        /// Wrap the system function <i>CancelWaitableTimer</i>.
        /// </summary>
        [DllImport( "Kernel32.dll", EntryPoint = "CancelWaitableTimer" )]
        [SuppressUnmanagedCodeSecurity]
        static extern private bool CancelWaitableTimer( SafeWaitHandle hTimer );

        /// <summary>
        /// Clients can register for the expiration of this timer.
        /// </summary>
        public event Action OnTimerExpired;

        /// <summary>
        /// This <see cref="Thread"/> will be create by <see cref="SecondsToWait"/> and
        /// runs <see cref="WaitThread"/>.
        /// </summary>
        private volatile Thread m_Waiting = null;

        /// <summary>
        /// 
        /// </summary>
        private ManualResetEvent m_Terminator = new ManualResetEvent( false );

        /// <summary>
        /// <see cref="DateTime.ToFileTime"/> of the time when the timer should
        /// expire.
        /// </summary>
        private long m_Interval = 0;

        /// <summary>
        /// When the next event is expected to fire.
        /// </summary>
        private volatile object m_NextEvent = null;

        /// <summary>
        /// Create the timer. The caller should call <see cref="Close"/> as soon as
        /// the timer is no longer needed.
        /// </summary>
        /// <remarks>
        /// <see cref="WaitHandle.Handle"/> will be used to store the system API
        /// handle of the newly created timer.
        /// </remarks>
        /// <exception cref="TimerException">When the timer could not be created.</exception>
        public WaitableTimer()
        {
            // Create it
            SafeWaitHandle = CreateWaitableTimer( IntPtr.Zero, false, null );

            // Test
            if (SafeWaitHandle.IsInvalid)
                throw new TimerException( Properties.Resources.CreateWaitableTimer );
        }

        /// <summary>
        /// Make sure that <see cref="Close"/> is called.
        /// </summary>
        ~WaitableTimer()
        {
            // Forward
            Close();
        }

        /// <summary>
        /// Stop <see cref="m_Waiting"/> if necessary. To do so <see cref="Thread.Abort()"/>
        /// is used.
        /// <seealso cref="SecondsToWait"/>
        /// <seealso cref="Close"/>
        /// </summary>
        private void AbortWaiter()
        {
            // Kill thread
            if (null == m_Waiting)
                return;

            // Terminate it
            m_Terminator.Set();

            // Must reset
            try
            {
                // Synchronize
                m_Waiting.Join();

                // Detach
                m_Waiting = null;
            }
            finally
            {
                // Recover event
                m_Terminator.Reset();
            }
        }

        /// <summary>
        /// The time (UTC) the next event is expected to fire. The property reports
        /// <see cref="DateTime.MaxValue"/> when the timer is not active.
        /// </summary>
        public DateTime? NextEventTime
        {
            get
            {
                // Report
                return (DateTime?) m_NextEvent;
            }
        }

        /// <summary>
        /// Activate the timer to stop after the indicated number of seconds.
        /// </summary>
        /// <remarks>
        /// This method will always call <see cref="AbortWaiter"/>. If the number
        /// of seconds is positive a new <see cref="m_Waiting"/> <see cref="Thread"/>
        /// will be created running <see cref="WaitThread"/>. Before calling
        /// <see cref="Thread.Start()"/> the <see cref="m_Interval"/> is initialized
        /// with the correct value. If the number of seconds is zero or negative
        /// the timer is canceled.
        /// </remarks>
        public double SecondsToWait
        {
            set
            {
                // Done with thread
                AbortWaiter();

                // Check mode
                if (value > 0)
                {
                    // Get the time for the event
                    DateTime nextEvent = DateTime.UtcNow.AddSeconds( value );

                    // Remember
                    m_NextEvent = nextEvent;

                    // Calculate
                    m_Interval = nextEvent.ToFileTimeUtc();

                    // Create thread
                    m_Waiting = new Thread( WaitThread );

                    // Configure
                    m_Waiting.Name = "Waitable Timer";

                    // Synchronize the start 
                    var sync = new object();

                    // Report
                    Tools.ExtendedLogging( "Starting to synchronize on new Timer Thread" );

                    // Wait for propert start
                    lock (sync)
                    {
                        // Run it
                        m_Waiting.Start( sync );

                        // Until thread is up
                        Monitor.Wait( sync );
                    }

                    // Report
                    Tools.ExtendedLogging( "Timer Thread is now up and running" );
                }
                else
                {
                    // Reset
                    m_NextEvent = null;
                    m_Interval = 0;

                    // No timer
                    CancelWaitableTimer( SafeWaitHandle );
                }
            }
        }

        /// <summary>
        /// Initializes the timer with <see cref="m_Interval"/> and waits for it
        /// to expire. If the timer expires <see cref="OnTimerExpired"/> is fired.
        /// </summary>
        /// <remarks>
        /// The <see cref="Thread"/> may be terminated with a call to <see cref="AbortWaiter"/>
        /// before the time expires.
        /// </remarks>
        /// <param name="startSync">Will be fired as soon as the timer is registered.</param>
        private void WaitThread( object startSync )
        {
            // Be safe
            try
            {
                // Interval to use
                long lInterval = m_Interval;

                // Start timer
                var setResult = SetWaitableTimer( SafeWaitHandle, &lInterval, 0, IntPtr.Zero, IntPtr.Zero, true );
                var error = Marshal.GetLastWin32Error();

                // Report
                Tools.ExtendedLogging( "Timer Set Result = {0} (Error Code is {1})", setResult, error );

                // Failed
                if (!setResult)
                    throw new TimerException( Properties.Resources.StartWaitableTimer );
            }
            finally
            {
                // Wakeup call
                lock (startSync)
                    Monitor.Pulse( startSync );
            }

            // Wait for the timer to expire
            var doneReason = WaitAny( new WaitHandle[] { this, m_Terminator } );

            // Report
            Tools.ExtendedLogging( "Timer finished with Reason Code {0} (1=abort, 0=expired)", doneReason );

            // Process
            if (doneReason != 0)
                return;

            // Forward
            var sink = OnTimerExpired;
            if (sink != null)
                sink();
        }

        /// <summary>
        /// Calles <see cref="AbortWaiter"/> and forwards to the base <see cref="WaitHandle.Close"/>
        /// method.
        /// </summary>
        public override void Close()
        {
            // Kill thread
            AbortWaiter();

            // Forward
            base.Close();
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instantz.
        /// </summary>
        /// <param name="explicitDisposing">Wird ignoriert.</param>
        protected override void Dispose( bool explicitDisposing )
        {
            // Self
            try
            {
                // Forward
                AbortWaiter();
            }
            catch (Exception e)
            {
                // Report
                VCRServer.Log( e );
            }

            // Forward
            base.Dispose( explicitDisposing );
        }
    }
}
