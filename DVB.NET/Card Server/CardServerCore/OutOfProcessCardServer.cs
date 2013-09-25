using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Implementierung eines <i>Card Servers</i>, der alle Befehle in einem separaten
    /// Prozess ausführt.
    /// </summary>
    internal class OutOfProcessCardServer : ServerImplementation
    {
        /// <summary>
        /// Der Datenstrom, an den die Anfragen geschickt werden.
        /// </summary>
        private AnonymousPipeServerStream m_RequestStream;

        /// <summary>
        /// Der Datenstrom, über den die Antworten eintreffen.
        /// </summary>
        private AnonymousPipeServerStream m_ResponseStream;

        /// <summary>
        /// Alle bisher angemeldeten Erweiterungsbibliotheken.
        /// </summary>
        private HashSet<string> m_Extensions = new HashSet<string>( StringComparer.InvariantCultureIgnoreCase );

        /// <summary>
        /// Current worker thread.
        /// </summary>
        private Thread m_Worker;

        /// <summary>
        /// Erzeugt eine neue Implementierung.
        /// </summary>
        internal OutOfProcessCardServer()
        {
            // Streams to use
            AnonymousPipeServerStream writer = null, reader = null;
            try
            {
                // Create streams
                writer = new AnonymousPipeServerStream( PipeDirection.Out, HandleInheritability.Inheritable );
                reader = new AnonymousPipeServerStream( PipeDirection.In, HandleInheritability.Inheritable );

                // Create start information
                ProcessStartInfo info = new ProcessStartInfo();

                // Configure
                info.Arguments = string.Format( "\"{0}\" \"{1}\"", writer.GetClientHandleAsString().Replace( "\"", "\"\"" ), reader.GetClientHandleAsString().Replace( "\"", "\"\"" ) );
                info.FileName = Path.Combine( RunTimeLoader.ServerDirectory.FullName, "JMS.DVB.CardServer.exe" );
                info.UseShellExecute = false;
                info.LoadUserProfile = true;

                // With cleanup
                try
                {
                    // Create the process
                    Process.Start( info );
                }
                finally
                {
                    // Destroy temporary data
                    writer.DisposeLocalCopyOfClientHandle();
                    reader.DisposeLocalCopyOfClientHandle();
                }

                // Remember
                m_RequestStream = writer;
                m_ResponseStream = reader;

                // Avoid cleanup
                writer = null;
                reader = null;
            }
            finally
            {
                // Cleanup all
                if (null != writer)
                    writer.Dispose();
                if (null != reader)
                    reader.Dispose();
            }
        }

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <param name="request">Die gewünschte Anfrage.</param>
        private void Execute( Request request )
        {
            // Create the thread
            m_Worker = new Thread( () =>
                {
                    // Be fully safe
                    try
                    {
                        // Serialize the request to the card server
                        request.SendRequest( m_RequestStream );

                        // Wait for the answer
                        Response response = request.ReceiveResponse( m_ResponseStream );

                        // Report result
                        if (null != response.Fault)
                            ActionDone( new CardServerException( response.Fault ), null );
                        else
                            ActionDone( null, response.Data );
                    }
                    catch (Exception e)
                    {
                        // Forward
                        ActionDone( e, null );
                    }
                } );

            // Run it
            m_Worker.Start();
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
        /// <exception cref="NoProfileFault">Es existiert kein Geräteprofil mit dem
        /// angegebenen Namen.</exception>
        protected override void OnAttachProfile( string profileName, bool reset, bool disablePCRFromH264, bool disablePCRFromMPEG2 )
        {
            // Process
            Execute(
                new SetProfileRequest
                    {
                        DisablePCRFromMPEG2Reconstruction = disablePCRFromMPEG2,
                        DisablePCRFromH264Reconstruction = disablePCRFromH264,
                        ProfileName = profileName,
                        ResetWakeUpDevice = reset,
                    } );
        }

        /// <summary>
        /// Wählt eine bestimmte Quellgruppe (Transponder) an.
        /// </summary>
        /// <param name="selection">Die Beschreibung einer Quelle, deren Gruppe aktiviert werden soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellgruppe angegeben.</exception>
        /// <exception cref="CardServerException">Es wird bereits eine Anfrage ausgeführt.</exception>
        protected override void OnSelect( SourceSelection selection )
        {
            // Process
            Execute( new SelectRequest { SelectionKey = selection.SelectionKey } );
        }

        /// <summary>
        /// Aktiviert eine einzelne Quelle für den <i>Zapping Modus</i>.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <param name="target">Die Netzwerkadresse, an die alle Daten versendet werden sollen.</param>
        protected override void OnSetZappingSource( SourceSelection source, string target )
        {
            // Process
            Execute( new SetZappingSourceRequest { SelectionKey = source.SelectionKey, Target = target } );
        }

        /// <summary>
        /// Aktiviert den Empfang einer Quelle.
        /// </summary>
        /// <param name="sources">Informationen zu den zu aktivierenden Quellen.</param>
        protected override void OnAddSources( ReceiveInformation[] sources )
        {
            // Process
            Execute( new AddSourcesRequest { Sources = sources } );
        }

        /// <summary>
        /// Lädt eine Bibliothek mit Erweiterungen.
        /// </summary>
        /// <param name="actionAssembly">Der Binärcode der Bibliothek.</param>
        /// <param name="symbols">Informationen zum Debuggen der Erweiterung.</param>
        /// <returns>Steuereinheit zur Synchronisation des Aufrufs.</returns>
        protected override void OnLoadExtensions( byte[] actionAssembly, byte[] symbols )
        {
            // Process
            Execute( new LoadExtensionsRequest { AssemblyData = actionAssembly, DebugData = symbols } );
        }

        /// <summary>
        /// Führt eine Erweiterungsoperation aus.
        /// </summary>
        /// <param name="actionType">Die Klasse, von der aus die Erweiterungsmethode abgerufen werden kann.</param>
        /// <param name="parameters">Optionale Parameter zur Ausführung.</param>
        protected override void OnCustomAction<IN, OUT>( string actionType, IN parameters )
        {
            // Process
            Execute( new CustomActionRequest<IN, OUT> { ActionType = actionType, Parameters = parameters } );
        }

        /// <summary>
        /// Stellt den Empfang für eine Quelle ein.
        /// </summary>
        /// <param name="source">Die betroffene Quelle.</param>
        /// <param name="uniqueIdentifier">Der eindeutige Name der Quelle.</param>
        protected override void OnRemoveSource( SourceIdentifier source, Guid uniqueIdentifier )
        {
            // Forward
            Execute( new RemoveSourceRequest { Source = new SourceIdentifier( source ), UniqueIdentifier = uniqueIdentifier } );
        }

        /// <summary>
        /// Stellt den Empfang für alle Quellen ein.
        /// </summary>
        protected override void OnRemoveAllSources()
        {
            // Forward
            Execute( new RemoveAllSourcesRequest() );
        }

        /// <summary>
        /// Ermittelt den aktuellen Zustand des <i>Card Servers</i>.
        /// </summary>
        protected override void OnGetState()
        {
            // Forward
            Execute( new GetStateRequest() );
        }

        /// <summary>
        /// Verändert den Netzwerkversand für eine aktive Quelle.
        /// </summary>
        /// <param name="source">Die Auswahl der Quelle.</param>
        /// <param name="uniqueIdentifier">Der eindeutige Name der Quelle.</param>
        /// <param name="target">Die Daten zum Netzwerkversand.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        protected override void OnSetStreamTarget( SourceIdentifier source, Guid uniqueIdentifier, string target )
        {
            // Process
            Execute( new SetStreamTargetRequest { Source = source, UniqueIdentifier = uniqueIdentifier, Target = target } );
        }

        /// <summary>
        /// Beginnt mit der Sammlung der Daten für die elektronische Programmzeitschrift
        /// (EPG).
        /// </summary>
        /// <param name="sources">Die zu berücksichtigenden Quellen.</param>
        /// <param name="extensions">Spezielle Zusatzfunktionalitäten der Sammlung.</param>
        protected override void OnStartEPGCollection( SourceIdentifier[] sources, EPGExtensions extensions )
        {
            // Forward
            Execute( new StartEPGRequest { Sources = sources, Extensions = extensions } );
        }

        /// <summary>
        /// Beendet die Aktualisierung der Programmzeitschrift.
        /// </summary>
        protected override void OnEndEPGCollection()
        {
            // Forward
            Execute( new EndEPGRequest() );
        }

        /// <summary>
        /// Beginnt einen Sendersuchlauf auf dem aktuellen Geräteprofil.
        /// </summary>
        protected override void OnStartScan()
        {
            // Forward
            Execute( new StartScanRequest() );
        }

        /// <summary>
        /// Beendet einen Sendersuchlauf auf dem aktuellen Geräteprofil.
        /// </summary>
        /// <param name="updateProfile">Gesetzt, wenn das Geräteprofil aktualisiert werden soll.</param>
        protected override void OnEndScan( bool? updateProfile )
        {
            // Forward
            Execute( new EndScanRequest { UpdateProfile = updateProfile } );
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        protected override void OnDispose()
        {
            // Forward to base
            base.OnDispose();

            // Check streams
            if (null != m_ResponseStream)
                try
                {
                    // Forward
                    m_ResponseStream.Dispose();
                }
                catch
                {
                    // Ignore any error
                }
                finally
                {
                    // Forget
                    m_ResponseStream = null;
                }
            if (null != m_RequestStream)
                try
                {
                    // Forward
                    m_RequestStream.Dispose();
                }
                catch
                {
                    // Ignore any error
                }
                finally
                {
                    // Forget
                    m_RequestStream = null;
                }

            // Wait for thread to finish
            if (null != m_Worker)
            {
                // Synchronize
                m_Worker.Join();

                // Forget
                m_Worker = null;
            }
        }

        /// <summary>
        /// Hilfsmethode zum einfachen Einspielen von Erweiterungsbibliotheken.
        /// </summary>
        /// <param name="extensionType">Eine beliebige Erweiterung.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Bibliothek angegeben.</exception>
        /// <exception cref="ArgumentException">Die Angegebenen Bibliothek existiert nicht.</exception>
        public override void LoadExtension( Type extensionType )
        {
            // Validate
            if (extensionType == null)
                throw new ArgumentNullException( "type" );

            // Already did it
            lock (m_Extensions)
                if (!m_Extensions.Add( extensionType.Assembly.FullName ))
                    return;

            // Attach to the file
            FileInfo file = new FileInfo( extensionType.Assembly.CodeBase.Substring( 8 ).Replace( '/', '\\' ) );
            if (!file.Exists)
                throw new ArgumentException( extensionType.AssemblyQualifiedName, "type" );

            // Load assembly data
            var assemblyData = File.ReadAllBytes( file.FullName );

            // Attach to default file name
            FileInfo symbolFile = new FileInfo( Path.ChangeExtension( file.FullName, "pdb" ) );

            // Optional symbol data
            byte[] symbols = null;
            if (symbolFile.Exists)
                symbols = File.ReadAllBytes( symbolFile.FullName );
            else
                symbols = null;

            // Register types - client side
            Request.AddTypes( extensionType.Assembly );

            // Process
            EndRequest( BeginLoadExtensions( assemblyData, symbols ) );
        }
    }
}
