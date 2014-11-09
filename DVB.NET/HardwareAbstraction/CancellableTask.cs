using System;
using System.Threading;
using System.Threading.Tasks;


namespace JMS.DVB
{
    /// <summary>
    /// Eine Hintergrundaufgabe, die auch abgebrochen werden kann.
    /// </summary>
    /// <typeparam name="TResultType">Das Ergebnis der Hintergrundaufgabe.</typeparam>
    public sealed class CancellableTask<TResultType> : Task<TResultType> where TResultType : class
    {
        /// <summary>
        /// Steuert das vorzeitige Beenden.
        /// </summary>
        private volatile CancellationTokenSource m_cancel = new CancellationTokenSource();

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="taskMethod">Die Methode, die zur Ausführung der Aufgabe aufgerufen werden soll.</param>
        private CancellableTask( Func<TResultType> taskMethod )
            : base( taskMethod )
        {
        }

        /// <summary>
        /// Bricht die Aufgabe sofort ab.
        /// </summary>
        public void Cancel()
        {
            var cancel = m_cancel;
            if (cancel != null)
                try
                {
                    cancel.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }
        }

        /// <summary>
        /// Bricht die Aufgabe nach einer bestimmten Zeit automatisch ab.
        /// </summary>
        /// <param name="timeout">Die gewünschte Zeit in Millisekunden.</param>
        public Task<TResultType> CancelAfter( int timeout )
        {
            // Process
            var cancel = m_cancel;
            if (cancel != null)
                try
                {
                    cancel.CancelAfter( timeout );
                }
                catch (ObjectDisposedException)
                {
                }

            // Make us regular
            return this;
        }

        /// <summary>
        /// Erzeugt eine neue Hintergrundaufgabe.
        /// </summary>
        /// <param name="worker">Der Algorithmus zur Ausführung.</param>
        /// <returns>Die neue Aufgabe.</returns>
        public static CancellableTask<TResultType> Run( Func<CancellationToken, TResultType> worker )
        {
            // Validate
            if (worker == null)
                throw new ArgumentException( "no task worker provided", "worker" );

            // Allow self reference to new instance
            CancellableTask<TResultType> task = null;

            // Create instance
            task =
                new CancellableTask<TResultType>( () =>
                {
                    // Provide cleanup of cancel source
                    using (var controller = task.m_cancel)
                        try
                        {
                            // Try to process without generating exception - if possible
                            return worker( controller.Token );
                        }
                        catch (Exception)
                        {
                            // In case of any error just report nothing - not applyable to value types
                            return null;
                        }
                        finally
                        {
                            // Never use cancel source again
                            task.m_cancel = null;
                        }
                } );

            // Make sure we clean up in case we are not able to start the task
            try
            {
                // Request start
                task.Start();
            }
            catch (Exception)
            {
                // Forward with cleanup
                using (task.m_cancel)
                    throw;
            }

            // Report
            return task;
        }
    }

    /// <summary>
    /// Hilfsmethoden zur einfacheren Nutzung von abbrechbaren Hintergrundaufgaben.
    /// </summary>
    public static class CancellableTaskExtensions
    {
        /// <summary>
        /// Wartet eine begrenzte Zeit auf das Ende einer Hintergrundaufgabe.
        /// </summary>
        /// <param name="task">Die Aufgabe.</param>
        /// <param name="cancel">Steuerung des externen Abbruchs.</param>
        /// <param name="timeout">Optional die maximale Wartezeit in Millisekunden.</param>
        /// <returns>Gesetzt, wenn die Aufgabe abgeschlossen wurde.</returns>
        /// <typeparam name="TResultType">Die Art des Ergebnisses.</typeparam>
        public static bool CancellableWait<TResultType>( this Task<TResultType> task, CancellationToken cancel, int timeout = Timeout.Infinite )
        {
            // Validate
            if (task == null)
                throw new ArgumentException( "no task to wait on", "task" );

            // Pre-check to avoid exception if possible
            if (cancel.IsCancellationRequested)
                return task.IsCompleted;

            // Full mode
            try
            {
                // May throw an exception if token is signaled or simply report the completition state
                return task.Wait( timeout, cancel );
            }
            catch (Exception)
            {
                // Hide exception
                return false;
            }
        }
    }
}
