using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Basisklasse zur Implementierung von Erweiterungen.
    /// </summary>
    /// <typeparam name="TInput">Datentyp der Eingangsdaten.</typeparam>
    /// <typeparam name="TOutput">Datentype der Ergebnisdaten.</typeparam>
    public abstract class CustomAction<TInput, TOutput>
    {
        /// <summary>
        /// Die zugehörige <i>Card Server</i> Instanz.
        /// </summary>
        private ServerImplementation m_CardServer;

        /// <summary>
        /// Das aktuell zu verwendende Gerät.
        /// </summary>
        protected Hardware Device { get; private set; }

        /// <summary>
        /// Meldet die zugehörige <i>Card Server</i> Instanz.
        /// </summary>
        protected InMemoryCardServer CardServer
        {
            get
            {
                // Report
                return (InMemoryCardServer) m_CardServer;
            }
        }

        /// <summary>
        /// Meldet die zugehörige <i>Card Server</i> Instanz.
        /// </summary>
        protected ServerImplementation CardServerProxy
        {
            get
            {
                // Report
                return m_CardServer;
            }
        }

        /// <summary>
        /// Initialisiert eine neue Instanz.
        /// </summary>
        /// <param name="cardServer">Der zugehörige <i>Card Server</i>.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Instanz angegeben.</exception>
        protected CustomAction( ServerImplementation cardServer )
        {
            // Validate
            if (cardServer == null)
                throw new ArgumentNullException( "cardServer" );

            // Remember
            m_CardServer = cardServer;

            // Register for operation
            m_CardServer.LoadExtension( GetType() );
        }

        /// <summary>
        /// Führt die zugehörige Operation aus.
        /// </summary>
        /// <param name="parameters">Optionale Parameter für den Aufruf.</param>
        /// <returns>Das Ergebnis der Operation.</returns>
        protected abstract TOutput OnExecute( TInput parameters );

        /// <summary>
        /// Führt die zugehörige Operation aus.
        /// </summary>
        /// <param name="device">Das zu verwendende Gerät.</param>
        /// <param name="parameters">Optionale Parameter für den Aufruf.</param>
        /// <returns>Das Ergebnis der Operation.</returns>
        internal object Execute( Hardware device, object parameters )
        {
            // Configure
            Device = device;
            try
            {
                // Process
                return OnExecute( (TInput) parameters );
            }
            finally
            {
                // Reset
                Device = null;
            }
        }

        /// <summary>
        /// Führt die zugehörige Operation aus.
        /// </summary>
        /// <param name="parameters">Optionale Parameter für den Aufruf.</param>
        /// <returns>Das Ergebnis der Operation.</returns>
        public IAsyncResult<TOutput> BeginExecute( TInput parameters )
        {
            // Process
            return m_CardServer.BeginCustomAction<TInput, TOutput>( GetType().AssemblyQualifiedName, parameters );
        }
    }
}
