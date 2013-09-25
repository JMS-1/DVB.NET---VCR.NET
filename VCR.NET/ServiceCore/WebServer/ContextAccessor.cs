using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;


namespace JMS.DVBVCR.RecordingService.WebServer
{
    /// <summary>
    /// Diese Klasse erlaubt den Zugriff der ASP.NET <see cref="AppDomain"/> auf
    /// die Daten des HTTP Zugriffs.
    /// </summary>
    /// <remarks>
    /// Zurzeit wird eine sehr einfache Implementierung verwendet, die für jeden
    /// Zugriff einen Wechsel zwoschen der <see cref="AppDomain"/> des VCR.NET Recording
    /// Service und der von ASP.NET auslöst. Sollten sich dadurch Geschwindigkeitseinbussen
    /// ergeben, so wäre eine Umstellung auf eine serialisierbares Klasse nicht wirklich 
    /// aufwändig. Lediglich die Frage der Rückübertragung von HTTP Kopfdaten und
    /// des HTTP Statuscodes muß überdacht werden.
    /// </remarks>
    public class ContextAccessor : MarshalByRefObject
    {
        /// <summary>
        /// Erzeugt die Anmeldeinformation für einen Anwender.
        /// </summary>
        /// <param name="lpszUsername">Der Name des Anwenders.</param>
        /// <param name="lpszDomain">Die Domäne des Anwenders</param>
        /// <param name="lpszPassword">Das Kennwort des Anwenders.</param>
        /// <param name="dwLogonType">Die gewünschte Art der Anmeldung.</param>
        /// <param name="dwLogonProvider">Die gewünschte Authenifizierung.</param>
        /// <param name="phToken">Das Ergebnis der Anmeldung.</param>
        /// <returns>Gesetzt, wenn der Anmeldevorgang erfolgreich war.</returns>
        [DllImport( "advapi32.dll", CharSet = CharSet.Unicode )]
        private static extern bool LogonUser( string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken );

        /// <summary>
        /// Alle Informationen zum aktuellen Aufruf.
        /// </summary>
        public readonly HttpListenerContext Context;

        /// <summary>
        /// Die Informationen zum Anwender, sofern diese explizit erstellt werden mussten.
        /// </summary>
        private WindowsIdentity m_userHandle;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public ContextAccessor( HttpListenerContext context )
        {
            // Remember
            Context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        public Stream OutputStream
        {
            get
            {
                // Report
                return Context.Response.OutputStream;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Stream InputStream
        {
            get
            {
                // Report
                return Context.Request.InputStream;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string HttpMethod
        {
            get
            {
                // Report
                return Context.Request.HttpMethod;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string RawUrl
        {
            get
            {
                // Report
                return Context.Request.RawUrl;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string UserAgent
        {
            get
            {
                // Report
                return Context.Request.UserAgent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Version ProtocolVersion
        {
            get
            {
                // Report
                return Context.Request.ProtocolVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get
            {
                // Report
                return Context.Request.LocalEndPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                // Report
                return Context.Request.RemoteEndPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Uri Url
        {
            get
            {
                // Report
                return Context.Request.Url;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetHeader( string name, string value )
        {
            // Check mode
            if (Equals( name, "Content-Length" ))
            {
                // Special
                ContentLength = long.Parse( value );
            }
            else
            {
                // Forward
                Context.Response.Headers[name] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetHeader( string name )
        {
            // Forward
            return Context.Request.Headers[name];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        public void SetStatus( int code, string description )
        {
            // Forward
            Context.Response.StatusDescription = description;
            Context.Response.StatusCode = code;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSecureConnection
        {
            get
            {
                // Forward
                return Context.Request.IsSecureConnection;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string AuthType
        {
            get
            {
                // Not autorized
                if (null == Context.User) return null;

                // None
                if (!Context.User.Identity.IsAuthenticated) return null;

                // Forward
                return Context.User.Identity.AuthenticationType.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string AuthUser
        {
            get
            {
                // Not autorized
                if (null == Context.User) return null;

                // None
                if (!Context.User.Identity.IsAuthenticated) return null;

                // Forward
                return Context.User.Identity.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public NameValueCollection RequestHeaders
        {
            get
            {
                // Report
                return Context.Request.Headers;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IntPtr UserToken
        {
            get
            {
                // Attach to user
                var user = Context.User;
                if (user == null)
                    return IntPtr.Zero;
                var identity = user.Identity;
                if (identity == null)
                    return IntPtr.Zero;

                // Check windows
                var windowsIdentity = identity as WindowsIdentity;
                if (windowsIdentity != null)
                    return windowsIdentity.Token;

                // Check basic
                var basicIdentity = identity as HttpListenerBasicIdentity;
                if (basicIdentity == null)
                    return IntPtr.Zero;

                // Can provide some cached data
                if (m_userHandle != null)
                    return m_userHandle.Token;

                // Attach to the user name
                var userName = basicIdentity.Name;
                if (string.IsNullOrEmpty( userName ))
                    return IntPtr.Zero;

                // Split name
                var parts = userName.Split( '\\' );
                if (parts.Length < 1)
                    return IntPtr.Zero;
                if (parts.Length > 2)
                    return IntPtr.Zero;

                // Get parts
                var name = parts[parts.Length - 1];
                var domain = (parts.Length == 2) ? parts[0] : ".";

                // Try to logon
                IntPtr handle;
                if (!LogonUser( name, domain, basicIdentity.Password, 3, 0, out handle ))
                {
                    // Set result
                    var response = Context.Response;
                    if (response != null)
                    {
                        // Set code and finish
                        response.StatusCode = 401;
                        response.Close();
                    }

                    // Done
                    return IntPtr.Zero;
                }

                // Create wrapper
                m_userHandle = new WindowsIdentity( handle );

                // Report
                return m_userHandle.Token;
            }
        }

        /// <summary>
        /// Wird mit Abschluss des Aufrufs aufgerufen.
        /// </summary>
        public void End()
        {
            // Get rid of tokens
            using (m_userHandle)
                m_userHandle = null;
        }

        /// <summary>
        /// Setzt die Anzahl der an den Client zu sendenden Bytes.
        /// </summary>
        public long ContentLength
        {
            set
            {
                // Store
                Context.Response.ContentLength64 = value;
            }
        }

        /// <summary>
        /// Legt das Inhaltsformat der Daten fest, die an den Client gesendet werden.
        /// </summary>
        public string ContentType
        {
            set
            {
                // Store
                Context.Response.ContentType = value;
            }
        }

        /// <summary>
        /// Legt fest, ob die Datenübermittlung an den Client in Häppchen erfolgt.
        /// </summary>
        public bool Chunked
        {
            set
            {
                // Store
                Context.Response.SendChunked = value;
            }
        }

        /// <summary>
        /// Legt fest, ob die Verbindung zum Client aufrecht erhalten werden soll.
        /// </summary>
        public bool KeepAlive
        {
            set
            {
                // Store
                Context.Response.KeepAlive = value;
            }
        }
    }
}
