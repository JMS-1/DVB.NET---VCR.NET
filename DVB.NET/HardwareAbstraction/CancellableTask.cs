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
            // Allow self reference to new instance
            CancellableTask<TResultType> task = null;

            // Create instance
            task =
                new CancellableTask<TResultType>( () =>
                {
                    using (var controller = task.m_cancel)
                        try
                        {
                            return worker( controller.Token );
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                        finally
                        {
                            task.m_cancel = null;
                        }
                } );

            // Start it
            try
            {
                // Request start
                task.Start();

                // Report
                return task;
            }
            catch (Exception)
            {
                // With cleanup
                using (task.m_cancel)
                    throw;
            }
        }
    }
}
