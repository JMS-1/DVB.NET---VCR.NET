using System;
using System.Collections.Generic;
using System.Threading;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Ergänzt die Schnittstelle <see cref="IAsyncResult"/> um den Zugriff
    /// auf einen Rückgabewert.
    /// </summary>
    /// <typeparam name="T">Der Datentyp des Rückgabewertes.</typeparam>
    public interface IAsyncResult<T> : IAsyncResult
    {
        /// <summary>
        /// Wartet auf das Ende der Ausführung und meldet einen Rückgabewert. Diese
        /// Methode darf nur ein einziges Mal aufgerufen werden.
        /// </summary>
        T Result { get; }
    }

    /// <summary>
    /// Die Basisklasse für alle <i>Card Server</i> Implementierungen.
    /// </summary>
    public abstract class ServerImplementation : IDisposable
    {
        /// <summary>
        /// Steuert die Ausführung einer Operation.
        /// </summary>
        private class _AsyncControl : IAsyncResult
        {
            /// <summary>
            /// Die zu steuernde Einheit.
            /// </summary>
            private ServerImplementation m_Server;

            /// <summary>
            /// Wird gesetzt, sobald die Operation als abgeschlossen bekannt ist.
            /// </summary>
            private bool m_IsCompleted;

            /// <summary>
            /// Erzeugt eine neue Steuerung.
            /// </summary>
            /// <param name="server">Die zu steuernde Einheit.</param>
            /// <exception cref="ArgumentNullException">Es wurde keine Steuerinstanz angegeben.</exception>
            public _AsyncControl( ServerImplementation server )
            {
                // Validate
                if (server == null)
                    throw new ArgumentNullException( "server" );

                // Remember
                m_Server = server;
            }

            /// <summary>
            /// Erzeugt eine neue Steuerinstanz.
            /// </summary>
            /// <typeparam name="T">Die Art des Rückgabewertes.</typeparam>
            /// <param name="server">Die aktuelle Implementierung.</param>
            /// <returns>Die gewünschte Steuerinstanz.</returns>
            public static IAsyncResult Create<T>( ServerImplementation server )
            {
                // Check mode
                if (typeof( T ) == typeof( object ))
                    return new _AsyncControl( server );
                else
                    return new _AsyncControl<T>( server );
            }

            #region IAsyncResult Members

            /// <summary>
            /// Meldet den Kontext dieser asynchronen Ausführung.
            /// </summary>
            object IAsyncResult.AsyncState
            {
                get
                {
                    // Report server
                    return m_Server;
                }
            }

            /// <summary>
            /// Meldet ein Synchronisationsobjekt für die Ausführung dieses Aufrufs.
            /// </summary>
            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get
                {
                    // Report
                    return m_Server.m_Done;
                }
            }

            /// <summary>
            /// Meldet, ob die Aufgabe synchron abgeschlossen werden konnte.
            /// </summary>
            bool IAsyncResult.CompletedSynchronously
            {
                get
                {
                    // Never
                    return false;
                }
            }

            /// <summary>
            /// Meldet, ob die Bearbeitung abgeschlossen wurde.
            /// </summary>
            bool IAsyncResult.IsCompleted
            {
                get
                {
                    // Load once
                    if (!m_IsCompleted)
                        if (!m_Server.IsBusy)
                            m_IsCompleted = true;

                    // Report
                    return m_IsCompleted;
                }
            }

            #endregion
        }

        /// <summary>
        /// Erweitert <see cref="_AsyncControl"/> um die Möglichkeit, einen Rückgabewert 
        /// abzufragen.
        /// </summary>
        /// <typeparam name="T">Die Art des Rückgabewertes.</typeparam>
        private class _AsyncControl<T> : _AsyncControl, IAsyncResult<T>
        {
            /// <summary>
            /// Erzeugt eine neue Steuerinstanz.
            /// </summary>
            /// <param name="server">Die zu verwaltenden Implementierung.</param>
            public _AsyncControl( ServerImplementation server )
                : base( server )
            {
            }

            /// <summary>
            /// Wartet auf das Ende der Ausführung und meldet einen Rückgabewert.
            /// </summary>
            public T Result
            {
                get
                {
                    // Report
                    return ServerImplementation.EndRequest<T>( this );
                }
            }
        }

        /// <summary>
        /// Der Name des aktuell zugeordneten DVB.NET Geräteprofils.
        /// </summary>
        private string m_Profile;

        /// <summary>
        /// Die aktuelle, noch nicht abgeschlossene Aufgabe.
        /// </summary>
        private volatile Action<object> m_Pending;

        /// <summary>
        /// Enthält das Ergebnis der letzten Ausführung einer Aufgabe.
        /// </summary>
        private CardServerException m_LastError;

        /// <summary>
        /// Der Rückgabewert der letzten Aufgabe.
        /// </summary>
        private object m_Result;

        /// <summary>
        /// Signalisiert den Abschluss einer Aufgabe.
        /// </summary>
        private ManualResetEvent m_Done = new ManualResetEvent( true );

        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        protected ServerImplementation()
        {
        }

        /// <summary>
        /// Erzeugt eine Implementierung, die in diesem Prozess arbeitet.
        /// </summary>
        /// <returns>Die gewünschte Implementierung.</returns>
        public static ServerImplementation CreateInMemory()
        {
            // Forward
            return new InMemoryCardServer();
        }

        /// <summary>
        /// Erzeugt eine Implementierung, die in einem separaten Prozess arbeitet.
        /// </summary>
        /// <returns>Die gewünschte Implementierung.</returns>
        public static ServerImplementation CreateOutOfProcess()
        {
            // Forward
            return new OutOfProcessCardServer();
        }

        /// <summary>
        /// Wählt eine bestimmte Quellgruppe (Transponder) an.
        /// </summary>
        /// <param name="selection">Die Beschreibung einer Quelle, deren Gruppe aktiviert werden soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellgruppe angegeben.</exception>
        /// <exception cref="CardServerException">Es wird bereits eine Anfrage ausgeführt.</exception>
        protected abstract void OnSelect( SourceSelection selection );

        /// <summary>
        /// Wählt eine bestimmte Quellgruppe (Transponder) an.
        /// </summary>
        /// <param name="selectionKey">Die Beschreibung einer Quelle, deren Gruppe aktiviert werden soll.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellgruppe angegeben.</exception>
        /// <exception cref="CardServerException">Es wird bereits eine Anfrage ausgeführt.</exception>
        public IAsyncResult BeginSelect( string selectionKey )
        {
            // Validate
            if (selectionKey == null)
                throw new ArgumentNullException( "selectionKey" );

            // Reconstruct
            var selection = new SourceSelection { SelectionKey = selectionKey };

            // Check for profile match
            if (string.IsNullOrEmpty( m_Profile ) || !string.Equals( m_Profile, selection.ProfileName, StringComparison.InvariantCultureIgnoreCase ))
                CardServerException.Throw( new ProfileMismatchFault( m_Profile, selection.ProfileName ) );

            // Start action
            return Start<object>( () => { OnSelect( selection ); } );
        }

        /// <summary>
        /// Führt eine Erweiterungsoperation aus.
        /// </summary>
        /// <param name="actionType">Die Klasse, von der aus die Erweiterungsmethode abgerufen werden kann.</param>
        /// <param name="parameters">Optionale Parameter zur Ausführung.</param>
        protected abstract void OnCustomAction<TInput, TOutput>( string actionType, TInput parameters );

        /// <summary>
        /// Führt eine Erweiterungsoperation aus.
        /// </summary>
        /// <typeparam name="TInput">Der Datentyp des Eingangswertes.</typeparam>
        /// <typeparam name="TOutput">Der Datentyp des Ergebniswertes.</typeparam>
        /// <param name="actionType">Die Klasse, von der aus die Erweiterungsmethode abgerufen werden kann.</param>
        /// <param name="parameters">Optionale Parameter zur Ausführung.</param>
        /// <returns>Steuereinheit zur Ausführung der Operation.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Erweiterung angegeben.</exception>
        public IAsyncResult<TOutput> BeginCustomAction<TInput, TOutput>( string actionType, TInput parameters )
        {
            // Validate
            if (string.IsNullOrEmpty( actionType ))
                throw new ArgumentNullException( "actionType" );

            // Start action
            return (IAsyncResult<TOutput>) Start<TOutput>( () => { OnCustomAction<TInput, TOutput>( actionType, parameters ); } );
        }

        /// <summary>
        /// Lädt eine Bibliothek mit Erweiterungen.
        /// </summary>
        /// <param name="actionAssembly">Der Binärcode der Bibliothek.</param>
        /// <param name="symbols">Informationen zum Debuggen der Erweiterung.</param>
        /// <returns>Steuereinheit zur Synchronisation des Aufrufs.</returns>
        protected abstract void OnLoadExtensions( byte[] actionAssembly, byte[] symbols );

        /// <summary>
        /// Lädt eine Bibliothek mit Erweiterungen.
        /// </summary>
        /// <param name="actionAssembly">Der Binärcode der Bibliothek.</param>
        /// <param name="symbols">Informationen zum Debuggen der Erweiterung.</param>
        /// <returns>Steuereinheit zur Synchronisation des Aufrufs.</returns>
        public IAsyncResult BeginLoadExtensions( byte[] actionAssembly, byte[] symbols )
        {
            // Validate
            if (actionAssembly == null)
                throw new ArgumentNullException( "actionAssembly" );

            // Start action
            return Start<object>( () => { OnLoadExtensions( actionAssembly, symbols ); } );
        }

        /// <summary>
        /// Aktiviert den Empfang einer Quelle.
        /// </summary>
        /// <param name="sources">Informationen zu den zu aktivierenden Quellen.</param>
        protected abstract void OnAddSources( ReceiveInformation[] sources );

        /// <summary>
        /// Aktiviert den Empfang einer Quelle.
        /// </summary>
        /// <param name="sources">Informationen zu den zu aktivierenden Quellen.</param>
        /// <returns>Steuereinheit für diesen Aufruf. Der Ergebniswert enthält alle Quellen, die erfolgreich
        /// aktiviert wurden - eventuell mit reduzierten Detailaspekten.</returns>
        /// <exception cref="ArgumentNullException">Mindestens eine Quelle ist nicht gesetzt.</exception>
        public IAsyncResult<StreamInformation[]> BeginAddSources( ReceiveInformation[] sources )
        {
            // Validate
            if (sources == null)
                throw new ArgumentNullException( "sources" );

            // Create helper
            var clones = new List<ReceiveInformation>();

            // Process all
            for (int i = 0; i < sources.Length; ++i)
            {
                // Load the one
                var source = sources[i];
                if (null == source)
                    throw new ArgumentNullException( string.Format( "sources[{0}]", i ) );

                // Clone it
                source = source.Clone();

                // More tests
                if (source.SelectionKey == null)
                    throw new ArgumentNullException( string.Format( "sources[{0}].SelectionKey", i ) );
                if (source.Streams == null)
                    throw new ArgumentNullException( string.Format( "sources[{0}].Streams", i ) );

                // Check for profile match
                if (string.IsNullOrEmpty( m_Profile ) || !string.Equals( m_Profile, source.Selection.ProfileName, StringComparison.InvariantCultureIgnoreCase ))
                    CardServerException.Throw( new ProfileMismatchFault( m_Profile, source.Selection.ProfileName ) );

                // Merge
                clones.Add( source );
            }

            // Start action
            return (IAsyncResult<StreamInformation[]>) Start<StreamInformation[]>( () => { OnAddSources( clones.ToArray() ); } );
        }

        /// <summary>
        /// Stellt den Empfang für eine Quelle ein.
        /// </summary>
        /// <param name="source">Die betroffene Quelle.</param>
        /// <param name="uniqueIdentifier">Der eindeutige Name der Quelle.</param>
        protected abstract void OnRemoveSource( SourceIdentifier source, Guid uniqueIdentifier );

        /// <summary>
        /// Stellt den Empfang für eine Quelle ein.
        /// </summary>
        /// <param name="source">Die betroffene Quelle.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        public IAsyncResult BeginRemoveSource( SourceIdentifier source )
        {
            // Forward
            return BeginRemoveSource( source, Guid.Empty );
        }

        /// <summary>
        /// Stellt den Empfang für eine Quelle ein.
        /// </summary>
        /// <param name="source">Die betroffene Quelle.</param>
        /// <param name="uniqueIdentifier">Der eindeutige Name der Quelle.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        public IAsyncResult BeginRemoveSource( SourceIdentifier source, Guid uniqueIdentifier )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );

            // Start action
            return Start<object>( () => { OnRemoveSource( source, uniqueIdentifier ); } );
        }

        /// <summary>
        /// Aktiviert eine einzelne Quelle für den <i>Zapping Modus</i>.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <param name="target">Die Netzwerkadresse, an die alle Daten versendet werden sollen.</param>
        protected abstract void OnSetZappingSource( SourceSelection source, string target );

        /// <summary>
        /// Aktiviert eine einzelne Quelle für den <i>Zapping Modus</i>.
        /// </summary>
        /// <param name="selectionKey">Die gewünschte Quelle.</param>
        /// <param name="target">Die Netzwerkadresse, an die alle Daten versendet werden sollen.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public IAsyncResult<ServerInformation> BeginSetZappingSource( string selectionKey, string target )
        {
            // Validate
            if (string.IsNullOrEmpty( selectionKey ))
                throw new ArgumentNullException( "selectionKey" );
            if (string.IsNullOrEmpty( target ))
                throw new ArgumentNullException( "target" );

            // Create the selection
            var source = new SourceSelection { SelectionKey = selectionKey };

            // Check for profile match
            if (string.IsNullOrEmpty( m_Profile ) || !string.Equals( m_Profile, source.ProfileName, StringComparison.InvariantCultureIgnoreCase ))
                CardServerException.Throw( new ProfileMismatchFault( m_Profile, source.ProfileName ) );

            // Start action
            return (IAsyncResult<ServerInformation>) Start<ServerInformation>( () => { OnSetZappingSource( source, target ); } );
        }

        /// <summary>
        /// Beginnt mit der Sammlung der Daten für die elektronische Programmzeitschrift
        /// (EPG).
        /// </summary>
        /// <param name="sources">Die zu berücksichtigenden Quellen.</param>
        /// <param name="extensions">Spezielle Zusatzfunktionalitäten der Sammlung.</param>
        protected abstract void OnStartEPGCollection( SourceIdentifier[] sources, EPGExtensions extensions );

        /// <summary>
        /// Beginnt mit der Sammlung der Daten für die elektronische Programmzeitschrift
        /// (EPG).
        /// </summary>
        /// <param name="sources">Die zu berücksichtigenden Quellen.</param>
        /// <param name="extensions">Spezielle Zusatzfunktionalitäten der Sammlung.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        public IAsyncResult BeginStartEPGCollection( SourceIdentifier[] sources, EPGExtensions extensions )
        {
            // Remap
            if (sources != null)
            {
                // Create clone
                sources = (SourceIdentifier[]) sources.Clone();

                // Create real identifiers
                for (int i = sources.Length; i-- > 0; )
                    if (sources[i] != null)
                        sources[i] = new SourceIdentifier( sources[i] );
            }

            // Forward
            return Start<object>( () => { OnStartEPGCollection( sources, extensions ); } );
        }

        /// <summary>
        /// Beendet die Aktualisierung der Programmzeitschrift.
        /// </summary>
        protected abstract void OnEndEPGCollection();

        /// <summary>
        /// Beendet die Aktualisierung der Programmzeitschrift.
        /// </summary>
        /// <returns>Steuereinheit für diesen Aufruf. Als Ergebnis werden alle Einträge
        /// für die Programmzeitschrift gemeldet, die bisher ermittelt wurden.</returns>
        public IAsyncResult<ProgramGuideItem[]> BeginEndEPGCollection()
        {
            // Forward
            return (IAsyncResult<ProgramGuideItem[]>) Start<ProgramGuideItem[]>( OnEndEPGCollection );
        }

        /// <summary>
        /// Verändert den Netzwerkversand für eine aktive Quelle.
        /// </summary>
        /// <param name="source">Die Auswahl der Quelle.</param>
        /// <param name="uniqueIdentifier">Der eindeutige Name der Quelle.</param>
        /// <param name="target">Die Daten zum Netzwerkversand.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        protected abstract void OnSetStreamTarget( SourceIdentifier source, Guid uniqueIdentifier, string target );

        /// <summary>
        /// Verändert den Netzwerkversand für eine aktive Quelle.
        /// </summary>
        /// <param name="source">Die Auswahl der Quelle.</param>
        /// <param name="target">Die Daten zum Netzwerkversand.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public IAsyncResult BeginSetStreamTarget( SourceIdentifier source, string target )
        {
            // Forward
            return BeginSetStreamTarget( source, Guid.Empty, target );
        }

        /// <summary>
        /// Verändert den Netzwerkversand für eine aktive Quelle.
        /// </summary>
        /// <param name="source">Die Auswahl der Quelle.</param>
        /// <param name="uniqueIdentifier">Der eindeutige Name der Quelle.</param>
        /// <param name="target">Die Daten zum Netzwerkversand.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public IAsyncResult BeginSetStreamTarget( SourceIdentifier source, Guid uniqueIdentifier, string target )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );

            // Start action
            return Start<object>( () => { OnSetStreamTarget( new SourceIdentifier( source ), uniqueIdentifier, target ); } );
        }

        /// <summary>
        /// Stellt den Empfang für alle Quellen ein.
        /// </summary>
        protected abstract void OnRemoveAllSources();

        /// <summary>
        /// Stellt den Empfang für alle Quellen ein.
        /// </summary>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        public IAsyncResult BeginRemoveAllSources()
        {
            // Start action
            return Start<object>( OnRemoveAllSources );
        }

        /// <summary>
        /// Aktiviert die Nutzung eines DVB.NET Geräteprofils.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="reset">Gesetzt, wenn das zugehörige Windows Gerät neu initialisiert werden soll.</param>
        /// <param name="disablePCRFromH264">Wird gesetzt um zu verhindern, dass die Systemzeit (PCR) aus
        /// dem H.264 Bildsignal ermittelt wird, da dieser Mechanismus hochgradig unsicher ist.</param>
        /// <param name="disablePCRFromMPEG2">Wird gesetzt um zu verhindern, dass die Systemzeit (PCR) aus
        /// dem MPEG2 Bildsignal ermittelt wird, da dieser Mechanismus hochgradig unsicher ist.</param>
        /// <exception cref="CardServerException">Es existiert kein Geräteprofil mit dem
        /// angegebenen Namen.</exception>
        protected abstract void OnAttachProfile( string profileName, bool reset, bool disablePCRFromH264, bool disablePCRFromMPEG2 );

        /// <summary>
        /// Setzt das aktuell zugeordnete DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des zu verwendenden Geräteprofils.</param>
        /// <param name="reset">Gesetzt, wenn das zugehörige Windows Gerät neu initialisiert werden soll.</param>
        /// <param name="disablePCRFromH264">Wird gesetzt um zu verhindern, dass die Systemzeit (PCR) aus
        /// dem H.264 Bildsignal ermittelt wird, da dieser Mechanismus hochgradig unsicher ist.</param>
        /// <param name="disablePCRFromMPEG2">Wird gesetzt um zu verhindern, dass die Systemzeit (PCR) aus
        /// dem MPEG2 Bildsignal ermittelt wird, da dieser Mechanismus hochgradig unsicher ist.</param>
        /// <returns>Steuerinstanz zur asynchronen Ausführung.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil angegeben.</exception>
        /// <exception cref="CardServerException">Es wurde bereits ein Geräteprofil aktiviert.</exception>
        public IAsyncResult BeginSetProfile( string profileName, bool reset, bool disablePCRFromH264, bool disablePCRFromMPEG2 )
        {
            // Validate
            if (string.IsNullOrEmpty( profileName ))
                throw new ArgumentNullException( "value" );

            // We are in use
            if (!string.IsNullOrEmpty( m_Profile ))
                CardServerException.Throw( new ProfileAlreadyAttachedFault( m_Profile ) );

            // Set the closing action
            return Start<object>( () => { OnAttachProfile( profileName, reset, disablePCRFromH264, disablePCRFromMPEG2 ); }, r => { m_Profile = profileName; } );
        }

        /// <summary>
        /// Ermittelt den aktuellen Zustand des <i>Card Servers</i>.
        /// </summary>
        protected abstract void OnGetState();

        /// <summary>
        /// Ermittelt den aktuellen Zustand des <i>Card Servers</i>.
        /// </summary>
        /// <returns>Steuereinheit für diesen Aufruf. Als Ergebnis wird eine Beschreibung des
        /// Zustands gemeldet.</returns>
        public IAsyncResult<ServerInformation> BeginGetState()
        {
            // Start action
            return (IAsyncResult<ServerInformation>) Start<ServerInformation>( OnGetState );
        }

        /// <summary>
        /// Beginnt einen Sendersuchlauf auf dem aktuellen Geräteprofil.
        /// </summary>
        protected abstract void OnStartScan();

        /// <summary>
        /// Beginnt einen Sendersuchlauf auf dem aktuellen Geräteprofil.
        /// </summary>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        public IAsyncResult BeginStartScan()
        {
            // Start action
            return Start<object>( OnStartScan );
        }

        /// <summary>
        /// Beendet einen Sendersuchlauf auf dem aktuellen Geräteprofil.
        /// </summary>
        /// <param name="updateProfile">Gesetzt, wenn das Geräteprofil aktualisiert werden soll.</param>
        protected abstract void OnEndScan( bool? updateProfile );

        /// <summary>
        /// Beendet einen Sendersuchlauf auf dem aktuellen Geräteprofil.
        /// </summary>
        /// <param name="updateProfile">Gesetzt, wenn das Geräteprofil aktualisiert werden soll.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        public IAsyncResult BeginEndScan( bool? updateProfile )
        {
            // Start action
            return Start<object>( () => { OnEndScan( updateProfile ); } );
        }

        /// <summary>
        /// Beginnt mit der Ausführung einer Aufgabe.
        /// </summary>
        /// <typeparam name="T">Die Art dces Rückgabewertes.</typeparam>
        /// <param name="action">Die auszuführende Aktion.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        private IAsyncResult Start<T>( Action action )
        {
            // Forward
            return Start<T>( action, null );
        }

        /// <summary>
        /// Beginnt mit der Ausführung einer Aufgabe.
        /// </summary>
        /// <typeparam name="TResult">Die Art des Rückgabewertes.</typeparam>
        /// <param name="action">Die auszuführende Aktion.</param>
        /// <param name="finalizer"></param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        private IAsyncResult Start<TResult>( Action action, Action<object> finalizer )
        {
            // We are Processing
            if (IsBusy)
                CardServerException.Throw( new ServerBusyFault() );

            // Set the closing action
            m_Pending = finalizer;

            // Reset error
            m_LastError = null;
            m_Result = null;

            // Reset synchronizer
            m_Done.Reset();

            // Be safe
            try
            {
                // Initiate the operation
                action();

                // Report control element
                return _AsyncControl.Create<TResult>( this );
            }
            catch (Exception e)
            {
                // Synchronous error
                ActionDone( e, null );

                // Forward
                throw e;
            }
        }

        /// <summary>
        /// Meldet das Ergebnis der letzten Befehlsausführung. Diese Methode
        /// kann nur ein einziges Mal aufgerufen werden.
        /// </summary>
        /// <returns>Das Ergebnis der letzten Befehlsausführung.</returns>
        private T GetResult<T>()
        {
            // Still running
            if (IsBusy)
                CardServerException.Throw( new ServerBusyFault() );

            // Result
            var e = m_LastError;

            // Clear
            m_LastError = null;

            // Report error
            if (e != null)
                throw e;

            // Report result
            return (T) m_Result;
        }

        /// <summary>
        /// Meldet, dass die aktuelle Aufgabe abgeschlossen wurde.
        /// </summary>
        /// <param name="e">Die zugehörige Ausnahme oder <i>null</i>, wenn bei der Ausführung
        /// kein Fehler aufgetreten ist.</param>
        /// <param name="result">Das Ergebnis der ausgeführten Aktion</param>
        protected void ActionDone( Exception e, object result )
        {
            // Remember
            m_Result = result;

            // Remember
            if (e == null)
                m_LastError = null;
            else if (e is CardServerException)
                m_LastError = (CardServerException) e;
            else
                m_LastError = new CardServerException( new CardServerFault( e.Message ) );

            // Finish
            if (m_Pending != null)
                try
                {
                    // Process if there was no error
                    if (m_LastError == null)
                        m_Pending( m_Result );
                }
                finally
                {
                    // Forget
                    m_Pending = null;
                }

            // Signal
            m_Done.Set();
        }

        /// <summary>
        /// Meldet, ob dieser <i>Card Server</i> noch mit der Ausführung einer Anfrage 
        /// beschäftigt ist.
        /// </summary>
        private bool IsBusy
        {
            get
            {
                // Report
                return !m_Done.WaitOne( 0, false );
            }
        }

        /// <summary>
        /// Wartet das Ende eines asynchronen Aufrufs ab.
        /// </summary>
        /// <param name="request">Die Steuereinheit zum Aufruf.</param>
        public static void EndRequest( IAsyncResult request )
        {
            // Forward
            EndRequest<object>( request );
        }

        /// <summary>
        /// Wartet das Ende eines asynchronen Aufrufs ab.
        /// </summary>
        /// <typeparam name="TResult">Die Art des Ergebnisses.</typeparam>
        /// <param name="request">Die Steuereinheit zum Aufruf.</param>
        /// <returns>Das Ergebnis der Operation.</returns>
        public static TResult EndRequest<TResult>( IAsyncResult<TResult> request )
        {
            // Forward
            return EndRequest<TResult>( (IAsyncResult) request );
        }

        /// <summary>
        /// Ermittelt das Ergebnis eines asynchronen Aufrufs.
        /// </summary>
        /// <typeparam name="T">Der Datentyp des Ergebnisses.</typeparam>
        /// <param name="request">Die Steuereinheit zum Aufruf.</param>
        /// <returns>Das Ergebnis des Aufrufs oder <i>null</i>, wenn es sich nicht um
        /// eine Funktion handelte.</returns>
        public static T EndRequest<T>( IAsyncResult request )
        {
            // Validate
            if (request == null)
                throw new ArgumentNullException( "request" );

            // Change type
            var myRequest = request as _AsyncControl;
            if (myRequest == null)
                throw new ArgumentException( request.GetType().FullName, "request" );

            // Wait until it finishes
            request.AsyncWaitHandle.WaitOne();

            // Get the server
            var server = (ServerImplementation) request.AsyncState;

            // Report the result
            return server.GetResult<T>();
        }

        /// <summary>
        /// Hilfsmethode zum einfachen Einspielen von Erweiterungsbibliotheken.
        /// </summary>
        /// <param name="extensionType">Eine beliebige Erweiterung.</param>
        public virtual void LoadExtension( Type extensionType )
        {
        }

        #region IDisposable Members

        /// <summary>
        /// Meldet, ob diese Instanz bereits beendet wurde.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Meldet, ob diese Instanz gerade beendet wird.
        /// </summary>
        protected bool IsDisposing { get; private set; }

        /// <summary>
        /// Gibt alle mit dieser Instanz verbundenen Ressourcen frei.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        /// Gibt alle mit dieser Instanz verbundenen Ressourcen frei.
        /// </summary>
        public void Dispose()
        {
            // Did it
            if (IsDisposed)
                return;

            // Be safe
            try
            {
                // Set flag with reset
                IsDisposing = true;
                try
                {
                    // Call derived classes
                    OnDispose();
                }
                finally
                {
                    // Reset flag
                    IsDisposing = false;
                }
            }
            finally
            {
                // Mark it
                IsDisposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Hilfsklasse zur einfacheren Nutzung der <see cref="ServerImplementation"/> Abstraktion.
    /// </summary>
    public static class ServerImplementationExtensions
    {
        /// <summary>
        /// Wählt eine bestimmte Quellgruppe (Transponder) an.
        /// </summary>
        /// <param name="server">Die Implementierung einer Geräteansteuerung.</param>
        /// <param name="selection">Die Beschreibung einer Quelle, deren Gruppe aktiviert werden soll.</param>
        /// <returns>Steuereinheit für diesen Aufruf.</returns>
        /// <exception cref="NullReferenceException">Es wurde keine Implementierung angegeben.</exception>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellgruppe angegeben.</exception>
        /// <exception cref="CardServerException">Es wird bereits eine Anfrage ausgeführt.</exception>
        public static IAsyncResult BeginSelect( this ServerImplementation server, SourceSelection selection )
        {
            // Validate
            if (server == null)
                throw new NullReferenceException( "server" );
            if (selection == null)
                throw new ArgumentNullException( "selection" );

            // Forward
            return server.BeginSelect( selection.SelectionKey );
        }
    }
}
