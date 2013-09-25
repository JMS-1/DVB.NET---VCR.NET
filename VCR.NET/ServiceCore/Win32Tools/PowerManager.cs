using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;


namespace JMS.DVBVCR.RecordingService.Win32Tools
{
    /// <summary>
    /// Manages access to the Power Managerment API included in Windows 2000. Currently
    /// VCR.NET makes sure that in some situations the system can not hibernate.
    /// </summary>
    public static class PowerManager
    {
        /// <summary>
        /// Possible execution states.
        /// </summary>
        [Flags]
        private enum ExecutionState : uint
        {
            /// <summary>
            /// Some error.
            /// </summary>
            Error = 0,

            /// <summary>
            /// System is required, do not hibernate.
            /// </summary>
            SystemRequired = 1,

            /// <summary>
            /// Display is required, do not hibernate.
            /// </summary>
            DisplayRequired = 2,

            /// <summary>
            /// User is active, do not hibernate.
            /// </summary>
            UserPresent = 4,

            /// <summary>
            /// Die Simulation des Schlafzustands ist gestattet.
            /// </summary>
            AwayModeRequired = 0x40,

            /// <summary>
            /// Use together with the above options to report a
            /// state until explicitly changed.
            /// </summary>
            Continuous = 0x80000000
        }

        /// <summary>
        /// Eine Änderungsoperation.
        /// </summary>
        private class Operation
        {
            /// <summary>
            /// Gesetzt, wenn der Schlafzustand verboten werden soll.
            /// </summary>
            private readonly bool m_forbid;

            /// <summary>
            /// Meldet, ob der Schlafzustand verboten werden soll.
            /// </summary>
            public bool IsForbid { get { return m_forbid; } }

            /// <summary>
            /// Erzeugt eine neue Operation.
            /// </summary>
            /// <param name="forbid">Gesetzt, wenn der Schlafzustand verboten werden soll.</param>
            private Operation( bool forbid )
            {
                // Remember
                m_forbid = forbid;
            }

            /// <summary>
            /// Erzeugt eine neues Verbot.
            /// </summary>
            public static Operation Forbid
            {
                get
                {
                    // Create
                    return new Operation( true );
                }
            }

            /// <summary>
            /// Erzeugt eine neue Erlaubnis.
            /// </summary>
            public static Operation Allow
            {
                get
                {
                    // Create
                    return new Operation( false );
                }
            }
        }

        /// <summary>
        /// Wrap the Power Managerment function <i>SetThreadExecutionState</i>.
        /// </summary>
        [DllImport( "kernel32.dll", EntryPoint = "SetThreadExecutionState" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern ExecutionState SetThreadExecutionState( ExecutionState esFlags );

        /// <summary>
        /// Handler to be called when hibernation is enabled or disabled.
        /// </summary>
        public delegate void ChangedHandler( bool bForbid );

        /// <summary>
        /// Clients can register with this event to become notified
        /// when <see cref="OnResume"/> is called.
        /// </summary>
        public static event Action OnPowerUp;

        /// <summary>
        /// Reports changes in the hibernation stage.
        /// </summary>
        public static event ChangedHandler OnChanged;

        /// <summary>
        /// Current hibernation lock depth. If zero hibernation is allowed.
        /// </summary>
        private static volatile int m_HibCount = 0;

        /// <summary>
        /// Gesetzt, wenn sich der Dienst im Schlafzustand befindet.
        /// </summary>
        private static volatile bool m_isSuspended;

        /// <summary>
        /// Steuert die kurzzeitige Deaktivierung des Schlafzustands.
        /// </summary>
        private class Controller : IDisposable
        {
            /// <summary>
            /// Stellt sicher, dass die Anzahl der Aufrufe korrekt ist.
            /// </summary>
            private int m_isDisposed;

            /// <summary>
            /// Verbietet den Schlafzustand.
            /// </summary>
            public Controller()
            {
                // Forbid
                PowerManager.RequestChange( Operation.Forbid );
            }

            /// <summary>
            /// Erlaubt den Schlafzustand wieder.
            /// </summary>
            public void Dispose()
            {
                // Allow
                if (Interlocked.CompareExchange( ref m_isDisposed, 1, 0 ) == 0)
                    PowerManager.RequestChange( Operation.Allow );
            }
        }

        /// <summary>
        /// Meldet oder legt fest, ob sich der Dienst im Schlafzustand befindet.
        /// </summary>
        public static bool IsSuspended
        {
            get
            {
                // Report
                return m_isSuspended;
            }
            set
            {
                // Change 
                m_isSuspended = value;
            }
        }

        /// <summary>
        /// Erzeugt eine neue Warteschlange für Änderungen.
        /// </summary>
        private static readonly Queue<Operation> m_operations = new Queue<Operation>();

        /// <summary>
        /// Steuert die Übergänge zwischen den Zuständen.
        /// </summary>
        private static void ControlThread()
        {
            // Process
            for (; ; )
            {
                // Collected change
                var operations = new List<Operation>();

                // Wait for next operation
                lock (m_operations)
                {
                    // Load
                    while (m_operations.Count < 1)
                        Monitor.Wait( m_operations );

                    // Detach - true is for forbid and false is for allow
                    while (m_operations.Count > 0)
                        operations.Add( m_operations.Dequeue() );
                }

                // See if there is something to do
                var delta = operations.Sum( operation => operation.IsForbid ? +1 : -1 );
                if (delta == 0)
                {
                    // Report
                    Tools.ExtendedLogging( "PowerManagement Delta is 0" );
                }
                else
                {
                    // To report transitions
                    var sink = OnChanged;

                    // Must forbid now
                    var oldCount = m_HibCount;
                    if (oldCount == 0)
                        if (SetThreadExecutionState( ExecutionState.SystemRequired | ExecutionState.Continuous ) == ExecutionState.Error)
                            VCRServer.Log( LoggingLevel.Errors, Properties.Resources.HibernationNotBlocked );
                        else if (sink != null)
                            sink( true );

                    // Adjust
                    m_HibCount += delta;

                    // Must allow now
                    var newCount = m_HibCount;
                    if (newCount == 0)
                        if (SetThreadExecutionState( ExecutionState.Continuous ) == ExecutionState.Error)
                            VCRServer.Log( LoggingLevel.Errors, Properties.Resources.HibernationNotUnblocked );
                        else if (sink != null)
                            sink( false );

                    // Report
                    Tools.ExtendedLogging( "PowerManagement: {0} => {1}", oldCount, newCount );
                }

                // Wakeup all requestors to make call appear synchronously
                operations.ForEach( operation =>
                    {
                        // Wakeup
                        lock (operation)
                            Monitor.PulseAll( operation );
                    } );
            }
        }

        /// <summary>
        /// Startet den Steuerthread.
        /// </summary>
        static PowerManager()
        {
            // Process
            new Thread( ControlThread ) { Name = "PowerManager", IsBackground = true, Priority = ThreadPriority.Highest }.Start();
        }

        /// <summary>
        /// Fordert eine Änderung an.
        /// </summary>
        /// <param name="operation">Die Art der Änderung.</param>
        private static void RequestChange( Operation operation )
        {
            // Send request
            lock (operation)
            {
                // Send request
                lock (m_operations)
                {
                    // Send forbid
                    m_operations.Enqueue( operation );

                    // Wake up
                    Monitor.PulseAll( m_operations );
                }

                // Wait for response
                Monitor.Wait( operation );
            }
        }

        /// <summary>
        /// Verbietet kurzfristig den Übergang in den Schlafzustand.
        /// </summary>
        /// <returns>Eine Steuereinheit, deren Freigabe den Schlafzustand wieder erlaubt.</returns>
        public static IDisposable StartForbidHibernation()
        {
            // Report
            return new Controller();
        }

        /// <summary>
        /// Synchronized check whether <see cref="m_HibCount"/> is zero or
        /// not.
        /// </summary>
        static public bool HibernationAllowed
        {
            get
            {
                // Report
                return (m_HibCount < 1);
            }
        }

        /// <summary>
        /// Löst auf einem separaten <see cref="Thread"/> <see cref="OnPowerUp"/> aus.
        /// </summary>
        public static void OnResume()
        {
            // Start wakeup thread
            new Thread( forbid =>
                {
                    // With reset
                    using ((IDisposable) forbid)
                        try
                        {
                            // Trigger events
                            var sink = OnPowerUp;
                            if (sink != null)
                                sink();
                        }
                        catch (Exception e)
                        {
                            // Report
                            VCRServer.Log( e );
                        }
                } ).Start( StartForbidHibernation() );
        }
    }
}
