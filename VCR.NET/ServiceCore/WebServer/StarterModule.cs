using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Http;


namespace JMS.DVBVCR.RecordingService.WebServer
{
    /// <summary>
    /// Diese Erweiterung sorgt für die einmalige Initialisierung der ASP.NET Laufzeitumgebung.
    /// </summary>
    public class StarterModule : IHttpModule
    {
        /// <summary>
        /// Ein eindeutiger Schlüssel.
        /// </summary>
        private static readonly string _Key = Guid.NewGuid().ToString();

        /// <summary>
        /// Alle Dateiendungen, für die wir ETags erzeugen.
        /// </summary>
        private static readonly HashSet<string> _ETagExtensions =
            new HashSet<string>( StringComparer.InvariantCultureIgnoreCase ) 
            { 
                ".html",
                ".css",
                ".js",
                ".png",
                ".gif",
                ".jpg",
                ".ico",
                ".map",
            };

        #region IHttpModule Members

        /// <summary>
        /// Wird beim Entfernen des Moduls aufgerufen.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Wird bei jedem einzelnen Aufruf aktiviert.
        /// </summary>
        /// <param name="context">Die aktuelle ASP.NET Anwendung.</param>
        public void Init( HttpApplication context )
        {
            // Register
            context.BeginRequest += OnBeginRequest;
            context.EndRequest += OnEndRequest;

            // Already did it
            var application = context.Application;
            if (application[_Key] != null)
                return;

            // Protect and try again
            application.Lock();
            try
            {
                // Already did it
                if (application[_Key] != null)
                    return;

                // Not again
                application[_Key] = true;

                // Only serve the main application
                if (StringComparer.InvariantCultureIgnoreCase.Equals( HttpRuntime.AppDomainAppVirtualPath, "/VCR.NET" ))
                    SetupWebApplication();
            }
            finally
            {
                // Can run in multi-threaded mode
                application.UnLock();
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn ein Zurgiff beginnt.
        /// </summary>
        /// <param name="sender">Die zugehörige Anwendung.</param>
        /// <param name="e">Wird ignroiert.</param>
        private static void OnBeginRequest( object sender, EventArgs e )
        {
            // Skip test in debug mode
            if (Debugger.IsAttached)
                return;

            // Check out context
            var context = HttpContext.Current;
            var worker = ((IServiceProvider) context).GetService( typeof( HttpWorkerRequest ) ) as Request;
            if (worker == null)
                return;

            // Check for header data
            var etagTest = worker.GetKnownRequestHeader( HttpWorkerRequest.HeaderIfNoneMatch );
            if (string.IsNullOrEmpty( etagTest ))
                return;

            // Check for static file
            var etag = GetETag();
            if (string.IsNullOrEmpty( etag ))
                return;

            // Check for match
            if (etagTest != etag)
                return;

            // Attach to the response
            var response = context.Response;

            // End it
            response.StatusCode = 304;
            response.StatusDescription = "Not Modified";
            response.End();
        }

        /// <summary>
        /// Wird aufgerufen, wenn ein Zurgiff beendet wurde.
        /// </summary>
        /// <param name="sender">Die zugehörige Anwendung.</param>
        /// <param name="e">Wird ignroiert.</param>
        private static void OnEndRequest( object sender, EventArgs e )
        {
            // Skip test in debug mode
            if (Debugger.IsAttached)
                return;

            // Check response state
            var context = HttpContext.Current;
            var response = context.Response;
            if (response.StatusCode != 200)
                return;

            // Check our context
            var worker = ((IServiceProvider) context).GetService( typeof( HttpWorkerRequest ) ) as Request;
            if (worker == null)
                return;

            // Load the ETag
            var etag = GetETag();
            if (!string.IsNullOrEmpty( etag ))
                worker.SendKnownResponseHeader( HttpWorkerRequest.HeaderEtag, etag );
        }

        /// <summary>
        /// Ermittelt die Dateiversion zum aktuellen Aufruf.
        /// </summary>
        /// <returns>Die gewünschte Dateikennung.</returns>
        private static string GetETag()
        {
            // Check out context
            var context = HttpContext.Current;
            var request = context.Request;
            var extension = request.CurrentExecutionFilePathExtension;
            if (extension == null)
                return null;
            if (!_ETagExtensions.Contains( extension ))
                return null;

            // Attach to the file
            var file = new FileInfo( request.PhysicalPath );
            if (!file.Exists)
                return null;

            // Load the date
            var lastWrite = file.LastWriteTimeUtc;
            var lastModified = new DateTime( lastWrite.Year, lastWrite.Month, lastWrite.Day, lastWrite.Hour, lastWrite.Minute, lastWrite.Second, 0, DateTimeKind.Utc );
            var utcNow = DateTime.UtcNow;
            if (lastModified > utcNow)
                lastModified = new DateTime( utcNow.Ticks - (utcNow.Ticks % 10000000L), DateTimeKind.Utc );

            // Convert
            var modifiedAsFileTime = lastModified.ToFileTime();
            var nowAsFileTime = utcNow.ToFileTime();
            var etag = string.Format( "\"{0:X8}\"", modifiedAsFileTime );

            // Check mode
            if ((nowAsFileTime - modifiedAsFileTime) <= 30000000L)
                etag = "W/" + etag;

            // Store
            return etag;
        }

        #endregion

        /// <summary>
        /// Wird einmalig aufgerufen und konfiguriert die ASP.NET Anwendung.
        /// </summary>
        private static void SetupWebApplication()
        {
            // Register main route
            GlobalConfiguration.Configuration.Routes.MapHttpRoute
                (
                    name: "Default",
                    routeTemplate: "{controller}/{detail}",
                    defaults: new { detail = RouteParameter.Optional }
                );
        }
    }
}
