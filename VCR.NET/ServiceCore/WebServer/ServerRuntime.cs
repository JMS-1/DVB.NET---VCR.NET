using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Web;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.Persistence;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;


namespace JMS.DVBVCR.RecordingService.WebServer
{
    /// <summary>
    /// Eine Instanz dieser Klasse sorgt f�r die Bereitstellung der ASP.NET
    /// Laufzeitumgebung. Sie wird in einer eigenen <see cref="AppDomain"/>
    /// gestartet und nimmt HTTP Anfragen zur Bearbeitung entgegen.
    /// </summary>
    public class ServerRuntime : MarshalByRefObject
    {
        /// <summary>
        /// Erm�glicht das dynamische Laden der DVB.NET Bibliotheken.
        /// </summary>
        static ServerRuntime()
        {
            // Start dynamic loading
            RunTimeLoader.Startup();
        }

        /// <summary>
        /// Startet die Laufzeitumgebung.
        /// </summary>
        public static void WebStartup()
        {
            // Register REST helper module
            DynamicModuleUtility.RegisterModule( typeof( StarterModule ) );

            // Register profile manager
            System.Web.Profile.ProfileManager.Providers.Clear();
            System.Web.Profile.ProfileManager.Providers.Add( new UserProfileManager() );
        }

        /// <summary>
        /// 
        /// </summary>
        private static VCRServer m_Server = null;

        /// <summary>
        /// Erzeugt eine ASP.NET Laufzeitumgebung.
        /// </summary>
        public ServerRuntime()
        {
            // Check for active debugger
            Tools.EnableTracing = Debugger.IsAttached;
            Tools.DomainName = "Virtual Directory";

            // Install watch-dog
            AppDomain.CurrentDomain.DomainUnload += WatchDog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WatchDog( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Instanzen dieser Klasse sind nicht zeitgebunden.
        /// </summary>
        /// <returns>Die Antwort muss immer <i>null</i> sein.</returns>
        public override object InitializeLifetimeService()
        {
            // No lease at all
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest( ContextAccessor context )
        {
            // Execute
            HttpRuntime.ProcessRequest( new Request( context ) );
        }

        /// <summary>
        /// Verbindet die ASP.NET Laufzeitumgebung des aktuellen virtuellen Verzeichnisses
        /// mit der aktiven VCR.NET Instanz.
        /// </summary>
        /// <param name="server">Die aktive VCR.NET Instanz.</param>
        public void SetServer( VCRServer server )
        {
            // Activate configuration from main domain
            VCRConfiguration.Register( server.Configuration );

            // Add to permanent cache
            m_Server = server;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Test()
        {
        }

        /// <summary>
        /// Beendet die ASP.NET Laufzeitumgebung.
        /// </summary>
        /// <remarks>
        /// Der Aufruf kehrt erst wieder zur�ck, wenn alle ausstehenden Anfragen bearbeitet
        /// wurden. Neue Anfragen werden nicht angenommen.
        /// </remarks>
        public void Stop()
        {
            // Reset
            m_Server = null;

            // Shutdown ASP.NET properly
            HttpRuntime.Close();
        }

        /// <summary>
        /// Meldet die <see cref="AppDomain"/>, in der ASP.NET l�uft.
        /// </summary>
        public AppDomain AppDomain { get { return AppDomain.CurrentDomain; } }

        /// <summary>
        /// Referenz auf die <see cref="AppDomain"/> des Dienstes melden.
        /// </summary>
        public static VCRServer VCRServer
        {
            get
            {
                // Make sure that user is allowed to access the server
                TestWebAccess();

                // Report
                return m_Server;
            }
        }

        /// <summary>
        /// Pr�ft, ob ein Anwender Zugriff auf die Webanwendung hat.
        /// </summary>
        /// <returns>Gesetzt, wenn es sich sogar um einen Administrator handelt.</returns>
        public static bool TestWebAccess()
        {
            // Forward
            return TestWebAccess( true );
        }

        /// <summary>
        /// Pr�ft, ob ein Anwender Zugriff auf den VCR.NET Recording Service hat.
        /// </summary>
        /// <param name="endOnFail">Gesetzt, wenn ein HTTP Zugriffsfehler ausgel�st werden
        /// soll, wenn der Zugriff nicht gestattet ist.</param>
        /// <returns>Gesetzt, wenn der Zugriff gestattet ist.</returns>
        public static bool TestWebAccess( bool endOnFail )
        {
            // Administrators can do everything
            if (IsAdmin)
                return true;

            // Attach to the current context
            HttpContext request = HttpContext.Current;

            // Attach to user
            IPrincipal user = request.User;

            // See if user is provided
            if (null != user)
            {
                // Get the user role
                string userRole = VCRConfiguration.Current.UserRole;

                // No restriction
                if (string.IsNullOrEmpty( userRole ))
                    return true;

                // Full test
                if (user.IsInRole( userRole ))
                    return true;
            }

            // Terminate
            if (endOnFail)
            {
                // Reject
                request.Response.StatusCode = 401;
                request.Response.End();
            }

            // Done
            return false;
        }

        /// <summary>
        /// Pr�ft, ob der aktuelle Anwender administrativen Zugriff auf den VCR.NET Recording
        /// Service besitzt. Ist das nicht der Fall, wird ein HTTP Fehler ausgel�st.
        /// </summary>
        public static void TestAdminAccess()
        {
            // Execute
            if (IsAdmin)
                return;

            // Attach to the current context
            HttpContext request = HttpContext.Current;

            // Reject
            request.Response.StatusCode = 401;
            request.Response.End();
        }

        /// <summary>
        /// Meldet, ob der aktuelle Anwender ein VCR.NET Administrator ist,
        /// </summary>
        public static bool IsAdmin
        {
            get
            {
                // Attach to the current context
                var request = HttpContext.Current;

                // Attach to user
                var user = request.User;

                // Load role
                var adminRole = VCRConfiguration.Current.AdminRole;

                // See if user is provided
                if (user == null)
                    return false;

                // No restriction
                if (string.IsNullOrEmpty( adminRole ))
                    return true;

                // Full test
                return user.IsInRole( adminRole );
            }
        }

        /// <summary>
        /// Ermittelt eine Referenz f�r eine bestimmte Aufzeichung in einem Auftrag, so dass diese
        /// auch in einer URL verwendet werden kann.
        /// </summary>
        /// <param name="job">Ein Auftrag.</param>
        /// <param name="schedule">Eine Aufzeichnung.</param>
        /// <returns>Die eindeutige Referenz.</returns>
        public static string GetUniqueWebId( VCRJob job, VCRSchedule schedule )
        {
            // Forward
            if (job == null)
                return "*";
            else if (schedule == null)
                return string.Format( "*{0:N}", job.UniqueID.Value );
            else
                return GetUniqueWebId( job.UniqueID.Value, schedule.UniqueID.Value );
        }

        /// <summary>
        /// Ermittelt eine Referenz f�r eine bestimmte Aufzeichung in einem Auftrag, so dass diese
        /// auch in einer URL verwendet werden kann.
        /// </summary>
        /// <param name="job">Die eindeutige Kennung eines Auftrags.</param>
        /// <param name="schedule">Die eindeutige Kennung einer Aufzeichnung des Auftrags.</param>
        /// <returns>Die eindeutige Referenz.</returns>
        public static string GetUniqueWebId( string job, string schedule )
        {
            // Use defaults
            if (string.IsNullOrEmpty( job ))
                job = Guid.Empty.ToString( "N" );
            if (string.IsNullOrEmpty( schedule ))
                schedule = Guid.Empty.ToString( "N" );

            // Create
            return string.Format( "{0}{1}", job, schedule );
        }

        /// <summary>
        /// Ermittelt eine Referenz f�r eine bestimmte Aufzeichung in einem Auftrag, so dass diese
        /// auch in einer URL verwendet werden kann.
        /// </summary>
        /// <param name="job">Die eindeutige Kennung eines Auftrags.</param>
        /// <param name="schedule">Die eindeutige Kennung einer Aufzeichnung des Auftrags.</param>
        /// <returns>Die eindeutige Referenz.</returns>
        public static string GetUniqueWebId( Guid job, Guid schedule )
        {
            // Forward
            return GetUniqueWebId( job.ToString( "N" ), schedule.ToString( "N" ) );
        }

        /// <summary>
        /// Rekonstruiert einen Auftrag und eine Aufzeichnung aus einer Textdarstellung.
        /// </summary>
        /// <param name="id">Die Textdarstellung.</param>
        /// <param name="job">Der zugeh�rige Auftrag.</param>
        /// <param name="schedule">Die Aufzeichnung in dem Auftrag.</param>
        public static void ParseUniqueWebId( string id, out Guid job, out Guid schedule )
        {
            // Read all
            schedule = new Guid( id.Substring( 32, 32 ) );
            job = new Guid( id.Substring( 0, 32 ) );
        }

        /// <summary>
        /// Rekonstruiert einen Auftrag und eine Aufzeichnung aus einer Textdarstellung.
        /// </summary>
        /// <param name="id">Die Textdarstellung.</param>
        /// <param name="job">Der ermittelte Auftrag.</param>
        /// <returns>Die zugeh�rige Aufzeichnung im Auftrag.</returns>
        public static VCRSchedule ParseUniqueWebId( string id, out VCRJob job )
        {
            // Read all
            Guid jobID, scheduleID;
            ParseUniqueWebId( id, out jobID, out scheduleID );

            // Find the job
            job = VCRServer.FindJob( jobID );

            // Report schedule if job exists
            if (job == null)
                return null;
            else
                return job[scheduleID];
        }

        /// <summary>
        /// Aktualisiert die Regeln f�r die Aufzeichnungsplanung.
        /// </summary>
        /// <param name="newRules">Die ab nun zu verwendenden Regeln.</param>
        /// <returns>Meldet, ob ein Neustart erforderlich ist.</returns>
        public static bool? UpdateSchedulerRules( string newRules )
        {
            // Must be admin
            TestAdminAccess();

            // Check state
            if (VCRServer.IsActive)
                return null;

            // Process
            VCRServer.SchedulerRules = newRules;

            // Do not restart in debug mode
            if (VCRServer.InDebugMode)
                return null;

            // Create new process to restart the service
            Process.Start( Tools.ExecutablePath, "Restart" ).Dispose();

            // Finally back to the administration page
            return true;
        }

        /// <summary>
        /// F�hrt eine Aktualisierung von Konfigurationswerten durch.
        /// </summary>
        /// <param name="settings">Die zu aktualisierenden Konfigurationswerte.</param>
        /// <param name="forceRestart">Erzwingt einen Neustart des Dienstes.</param>
        /// <returns>Gesetzt, wenn ein Neustart erforderlich ist.</returns>
        public static bool? Update( IEnumerable<VCRConfiguration.SettingDescription> settings, bool forceRestart = false )
        {
            // Must be admin
            TestAdminAccess();

            // Check state
            if (VCRServer.IsActive)
                return null;

            // Process
            if (VCRConfiguration.Current.CommitUpdate( settings ) || forceRestart)
            {
                // Do not restart in debug mode
                if (VCRServer.InDebugMode)
                    return null;

                // Create new process to restart the service
                Process.Start( Tools.ExecutablePath, "Restart" ).Dispose();

                // Finally back to the administration page
                return true;
            }
            else
            {
                // Check for new tasks
                m_Server.BeginNewPlan();

                // Finally back to the administration page
                return false;
            }
        }
    }

    /// <summary>
    /// Einige Hilfsmethoden zur Vereinfachung der Webanwendung.
    /// </summary>
    public static class ServerRuntimeExtensions
    {
        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration eine Dolby Digital Tonspur nicht 
        /// grunds�tzlich ausschlie�t.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn die AC3 Tonspur nicht grunds�tzlich deaktiviert ist.</returns>
        public static bool GetUsesDolbyAudio( this StreamSelection streams )
        {
            // Check mode
            if (null == streams)
                return false;
            else if (streams.AC3Tracks.LanguageMode != LanguageModes.Selection)
                return true;
            else
                return (streams.AC3Tracks.Languages.Count > 0);
        }

        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration alle Tonspuren einschlie�t.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.</returns>
        public static bool GetUsesAllAudio( this StreamSelection streams )
        {
            // Check mode
            if (null == streams)
                return false;
            else
                return (streams.MP2Tracks.LanguageMode == LanguageModes.All);
        }

        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration DVB Untertitel nicht 
        /// grunds�tzlich ausschlie�t.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn die DVB Untertitel nicht grunds�tzlich deaktiviert sind.</returns>
        public static bool GetUsesSubtitles( this StreamSelection streams )
        {
            // Check mode
            if (null == streams)
                return false;
            else if (streams.SubTitles.LanguageMode != LanguageModes.Selection)
                return true;
            else
                return (streams.SubTitles.Languages.Count > 0);
        }

        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration auch den Videotext umfasst.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn der Videotext aufgezeichnet werden soll.</returns>
        public static bool GetUsesVideotext( this StreamSelection streams )
        {
            // Check mode
            if (null == streams)
                return false;
            else
                return streams.Videotext;
        }

        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration auch einen Extrakt der Programmzeitschrift umfasst.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn die Programmzeitschrift ber�cksichtigt werden soll.</returns>
        public static bool GetUsesProgramGuide( this StreamSelection streams )
        {
            // Check mode
            if (null == streams)
                return false;
            else
                return streams.ProgramGuide;
        }

        /// <summary>
        /// Legt fest, ob die Dolby Digital Tonspur aufgezeichnet werden soll.
        /// </summary>
        /// <param name="streams">Die Konfiguration der zu verwendenden Datenstr�me.</param>
        /// <param name="set">Gesetzt, wenn die Datenspur aktiviert werden soll.</param>
        public static void SetUsesDolbyAudio( this StreamSelection streams, bool set )
        {
            // Reset language list
            streams.AC3Tracks.Languages.Clear();

            // Check mode
            if (set)
                if (streams.MP2Tracks.LanguageMode == LanguageModes.Selection)
                    streams.AC3Tracks.LanguageMode = LanguageModes.Primary;
                else
                    streams.AC3Tracks.LanguageMode = streams.MP2Tracks.LanguageMode;
            else
                streams.AC3Tracks.LanguageMode = LanguageModes.Selection;
        }

        /// <summary>
        /// Legt fest, ob alle Tonspuren aufgezeichnet werden sollen.
        /// </summary>
        /// <param name="streams">Die Konfiguration der zu verwendenden Datenstr�me.</param>
        /// <param name="set">Gesetzt, wenn die Datenspuren aktiviert werden sollen.</param>
        public static void SetUsesAllAudio( this StreamSelection streams, bool set )
        {
            // Clear all
            streams.MP2Tracks.Languages.Clear();
            streams.AC3Tracks.Languages.Clear();

            // Check mode
            if (set)
            {
                // All
                streams.MP2Tracks.LanguageMode = LanguageModes.All;
            }
            else
            {
                // Remember
                streams.MP2Tracks.LanguageMode = LanguageModes.Primary;
            }

            // Forward
            if (streams.AC3Tracks.LanguageMode != LanguageModes.Selection)
                streams.AC3Tracks.LanguageMode = streams.MP2Tracks.LanguageMode;
        }

        /// <summary>
        /// Legt fest, ob DVB Untertitel aufgezeichnet werden sollen.
        /// </summary>
        /// <param name="streams">Die Konfiguration der zu verwendenden Datenstr�me.</param>
        /// <param name="set">Gesetzt, wenn die Datenspuren aktiviert werden sollen.</param>
        public static void SetUsesSubtitles( this StreamSelection streams, bool set )
        {
            // Reset language list
            streams.SubTitles.Languages.Clear();

            // Check mode
            if (set)
                streams.SubTitles.LanguageMode = LanguageModes.All;
            else
                streams.SubTitles.LanguageMode = LanguageModes.Selection;
        }

        /// <summary>
        /// Legt fest, ob der Videotext mit aufgezeichnet werden soll.
        /// </summary>
        /// <param name="streams">Die Konfiguration der zu verwendenden Datenstr�me.</param>
        /// <param name="set">Gesetzt, wenn die Datenspur aktiviert werden soll.</param>
        public static void SetUsesVideotext( this StreamSelection streams, bool set )
        {
            // Direct
            streams.Videotext = set;
        }
    }
}
