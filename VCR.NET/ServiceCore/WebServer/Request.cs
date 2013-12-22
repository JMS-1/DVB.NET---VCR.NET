using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;


namespace JMS.DVBVCR.RecordingService.WebServer
{
    /// <summary>
    /// Instanzen dieser Klasse bechreiben eine HTTP Zugriff über die
    /// ASP.NET Laufzeitumgebung.
    /// </summary>
    public class Request : HttpWorkerRequest
    {
        /// <summary>
        /// Informationen zum zugehörigen HTTP Zugriff.
        /// </summary>
        private ContextAccessor m_Context;

        private bool m_ClientRunning = true;

        /// <summary>
        /// Erzeugt eine neue Instanz für die Bearbeitung eines HHTP Zugriffs.
        /// </summary>
        /// <param name="context">Der HTTP Zugriff.</param>
        public Request( ContextAccessor context )
        {
            // Remember
            m_Context = context;
        }

        /// <summary>
        /// Der HTTP Zugriff ist beendet.
        /// </summary>
        public override void EndOfRequest()
        {
            // Forward
            m_Context.End();
        }

        /// <summary>
        /// Alle zwischengespeicherten Daten zum Aufrufer senden.
        /// </summary>
        /// <param name="finalFlush">Dieser Parameter wird ignoriert.</param>
        public override void FlushResponse( bool finalFlush )
        {
            // Check for error
            try
            {
                // Forward (ignoring flag)
                m_Context.OutputStream.Flush();
            }
            catch
            {
                // Mark as disconnected
                m_ClientRunning = false;
            }
        }

        /// <summary>
        /// Meldet, ob noch eine Netzwerkverbindung zum Client besteht.
        /// </summary>
        /// <returns>Gesetzt, wenn noch eine Verbindung aktiv ist.</returns>
        public override bool IsClientConnected()
        {
            // Forward
            return m_ClientRunning;
        }

        /// <summary>
        /// Ermittelt die HTTP Aufrufmethode.
        /// </summary>
        /// <returns>Die HTTP Methode, die vom Client zum Aufruf verwendet wird.</returns>
        public override string GetHttpVerbName()
        {
            // Forward to context
            return m_Context.HttpMethod;
        }

        /// <summary>
        /// Ermittelt die HTTP Version.
        /// </summary>
        /// <returns></returns>
        public override string GetHttpVersion()
        {
            // Load the version
            Version version = m_Context.ProtocolVersion;

            // Ask Context
            return string.Format( "HTTP/{0}.{1}", version.Major, version.Minor );
        }

        /// <summary>
        /// Liefert die IP Adresse des Servers.
        /// </summary>
        /// <returns>Die IP Adresse des Servers.</returns>
        public override string GetLocalAddress()
        {
            // Forward
            return m_Context.LocalEndPoint.Address.ToString();
        }

        /// <summary>
        /// Liefert den TCP/IP Port unter dem der Server HHTP Aufrufe annimmt.
        /// </summary>
        /// <returns>Der TCP/IP Port des Servers.</returns>
        public override int GetLocalPort()
        {
            // Forward
            return m_Context.LocalEndPoint.Port;
        }

        /// <summary>
        /// Ermittelt die Suchzeichenkette zum Aufruf.
        /// </summary>
        /// <returns>Das Ergebnis enthält das Fragezeichen am Anfang nicht.</returns>
        public override string GetQueryString()
        {
            // Get the full URL
            string fullUrl = m_Context.RawUrl;

            // Check for query stringt
            int index = fullUrl.IndexOf( '?' );

            // None
            if (index++ < 0) return string.Empty;

            // Split off
            return fullUrl.Substring( index );
        }

        /// <summary>
        /// Liefert die ursprüngliche URL des Zugriffs.
        /// </summary>
        /// <returns>Die URL des HTTP Zugrifss.</returns>
        public override string GetRawUrl()
        {
            // Forward
            return m_Context.RawUrl;
        }

        /// <summary>
        /// Ermittelt die IP Adresse des Aufrufers.
        /// </summary>
        /// <returns>Die IP Adresse des Aufrufers.</returns>
        public override string GetRemoteAddress()
        {
            // Forward to context
            return m_Context.RemoteEndPoint.Address.ToString();
        }

        /// <summary>
        /// Liefert den TCP/IP Port, an den die HHTP Antwort gesendet wird.
        /// </summary>
        /// <returns>Der TCP/IP Port für die Rückgabedaten.</returns>
        public override int GetRemotePort()
        {
            // Forward
            return m_Context.RemoteEndPoint.Port;
        }

        /// <summary>
        /// Ermittelt die lokale URI zum Aufruf.
        /// </summary>
        /// <returns>URI relativ zum Server.</returns>
        public override string GetUriPath()
        {
            // Forward to context
            return m_Context.Url.LocalPath;
        }

        /// <summary>
        /// Uberträgt einen vordefinierten HTTP Header Wert.
        /// </summary>
        /// <param name="index">Index des vorderfinierten Wertes.</param>
        /// <param name="value">In den Header zu übernehmende Daten.</param>
        public override void SendKnownResponseHeader( int index, string value )
        {
            // Forward to context
            m_Context.SetHeader( HttpWorkerRequest.GetKnownResponseHeaderName( index ), value );
        }

        /// <summary>
        /// Überträgt den Inhalt einer Datei zum Aufrufer.
        /// </summary>
        /// <remarks>
        /// Diese Methode ist nicht implementiert.
        /// </remarks>
        /// <param name="handle">Referenz auf die Datei.</param>
        /// <param name="offset">Erstes zu sendendes Byte in der Datei.</param>
        /// <param name="length">Anzahl der zu sendenden Bytes.</param>
        public override void SendResponseFromFile( IntPtr handle, long offset, long length )
        {
            // Not yet
            throw new NotSupportedException();
        }

        /// <summary>
        /// Überträgt den Inhalt einer Datei zum Aufrufer.
        /// </summary>
        /// <remarks>
        /// Diese Methode ist nicht implementiert.
        /// </remarks>
        /// <param name="filename">Absoluter Pfad zu Datei.</param>
        /// <param name="offset">Erstes zu sendendes Byte in der Datei.</param>
        /// <param name="length">Anzahl der zu sendenden Bytes.</param>
        public override void SendResponseFromFile( string filename, long offset, long length )
        {
            // Allocate memory
            var toBeSend = new byte[length];

            // Load from file
            using (var stream = new FileStream( filename, FileMode.Open, FileAccess.Read, FileShare.Read ))
            {
                // Position
                stream.Seek( offset, SeekOrigin.Begin );

                // Read and forward
                SendResponseFromMemory( toBeSend, stream.Read( toBeSend, 0, (int) length ) );
            }
        }

        /// <summary>
        /// Überträgt Daten zum Aufrufer.
        /// </summary>
        /// <param name="data">Zu übertragende Daten.</param>
        /// <param name="length">Anzahl der zu übertragenden Bytes.</param>
        public override void SendResponseFromMemory( byte[] data, int length )
        {
            // Be safe
            try
            {
                // Blind forward
                m_Context.OutputStream.Write( data, 0, length );
            }
            catch
            {
                // Mark as disconnected
                m_ClientRunning = false;
            }
        }

        /// <summary>
        /// Setzt den Rückgabecode für diesen HTTP Aufruf.
        /// </summary>
        /// <param name="statusCode">HTTP Statuscode.</param>
        /// <param name="statusDescription">Ergänzende Bexschreibung zum Status.</param>
        public override void SendStatus( int statusCode, string statusDescription )
        {
            // Forward to context
            m_Context.SetStatus( statusCode, statusDescription );
        }

        /// <summary>
        /// Überträgt einen HTTP Header Wert.
        /// </summary>
        /// <param name="name">Name des Header Wertes.</param>
        /// <param name="value">In den Header zu übernehmende Daten.</param>
        public override void SendUnknownResponseHeader( string name, string value )
        {
            // Blind forward as is
            m_Context.SetHeader( name, value );
        }

        /// <summary>
        /// Liefert den Namen des virtuellen Verzeichnisses.
        /// </summary>
        /// <returns>Das virtuelle Verzeichnis.</returns>
        public override string GetAppPath()
        {
            // Report
            return HttpRuntime.AppDomainAppVirtualPath;
        }

        /// <summary>
        /// Liefert den physikalischen Pfad zum virtuellen Verzeichnis.
        /// </summary>
        /// <returns>Voller Pfad zum physikalischen Verzeichnis.</returns>
        public override string GetAppPathTranslated()
        {
            // Report
            return HttpRuntime.AppDomainAppPath;
        }

        /// <summary>
        /// Ermittelt den virtuellen Pfad zum HTTP Zugriff.
        /// </summary>
        /// <returns>Der virtuelle Pfad des aktuellen Zugriffs.</returns>
        public override string GetFilePath()
        {
            // Load
            string path = base.GetFilePath();

            // Split
            string[] parts = path.Split( '/' );

            // Not allowed
            if (parts.Length < 3) return path;

            // Check for Web Service call
            if (!parts[parts.Length - 2].ToLower().EndsWith( ".asmx" )) return path;

            // Cut off the method name
            return path.Substring( 0, path.LastIndexOf( '/' ) );
        }

        /// <summary>
        /// Liefert den physikalischen Pfad zum HTTP Zugriff.
        /// </summary>
        /// <returns>Der volle Pfad für den aktuellen HHTP Zugriff.</returns>
        public override string GetFilePathTranslated()
        {
            // The the path
            string path = GetFilePath();

            // Cut length
            int cut = HttpRuntime.AppDomainAppVirtualPath.Length + 1;

            // Test
            if (path.Length > cut)
            {
                // Suffix only
                path = path.Substring( cut );

                // Make it a path
                path = path.Replace( '/', '\\' );
            }
            else
            {
                // None
                path = string.Empty;
            }

            // Construct
            return Path.Combine( HttpRuntime.AppDomainAppPath, path );
        }

        /// <summary>
        /// Liest einen vordefinierten HHTP Header Wert.
        /// </summary>
        /// <param name="index">Index des Header Wertes.</param>
        /// <returns>Der vom Aufrufer übertragene Wert.</returns>
        public override string GetKnownRequestHeader( int index )
        {
            // Process
            switch (index)
            {
                case HeaderUserAgent:
                    // Special override
                    return m_Context.UserAgent;
                default:
                    // Standard
                    return m_Context.GetHeader( GetKnownRequestHeaderName( index ) );
            }
        }

        /// <summary>
        /// Ermittelt den relativen virtuellen Pfad zum aktuellen Aufruf.
        /// </summary>
        /// <returns>Der relative virtuelle Pfad zum Aufruf.</returns>
        public override string GetPathInfo()
        {
            // Retrieve full path
            string basePath = GetFilePath();

            // Retrieve local path
            string local = m_Context.Url.LocalPath;

            // Absolute
            if (basePath.Length >= local.Length) return string.Empty;

            // Get suffix
            return local.Substring( basePath.Length );
        }

        /// <summary>
        /// Ermittelt eine besondere Variable für den HTTP Aufruf oder zur
        /// aktuellen Umgebung.
        /// </summary>
        /// <param name="name">Name der Variable.</param>
        /// <returns>Wert der Variable.</returns>
        public override string GetServerVariable( string name )
        {
            // Check some
            switch (name)
            {
                case "HTTPS":
                    // SSL
                    return m_Context.IsSecureConnection ? "on" : "off";
                case "HTTP_USER_AGENT":
                    // Special
                    return m_Context.GetHeader( "UserAgent" );
                case "AUTH_TYPE":
                    // Forward
                    return m_Context.AuthType;
                case "LOGON_USER":
                    // Forward
                    return m_Context.AuthUser;
                default:
                    // Normally null
                    return base.GetServerVariable( name );
            }
        }

        /// <summary>
        /// Ermittelt alle nicht vordefinierten HTTP Header Werte, die der Aufrufer
        /// im HTTP Zugriff übertragen hat.
        /// </summary>
        /// <returns>Liste von HTTP Header Werten.</returns>
        public override string[][] GetUnknownRequestHeaders()
        {
            // Result
            List<string[]> pairs = new List<string[]>();

            // Create as list
            NameValueCollection headers = m_Context.RequestHeaders;

            // All of it
            for (int i = 0; i < headers.Count; ++i)
            {
                // Load the name
                string name = headers.GetKey( i );

                // Do not use
                if (GetKnownRequestHeaderIndex( name ) >= 0) continue;

                // Remember
                pairs.Add( new string[] { name, headers.Get( i ) } );
            }

            // Done
            return pairs.ToArray();
        }

        /// <summary>
        /// Ermittelt den aktuellen Anwender.
        /// </summary>
        /// <returns>Windows interne Referenz (Token) zum aktuellen Anwender.</returns>
        public override IntPtr GetUserToken()
        {
            // Forward
            return m_Context.UserToken;
        }

        /// <summary>
        /// List die übertragenen Nutzdaten.
        /// </summary>
        /// <param name="buffer">Speicherbereich für den Emfpang der Daten.</param>
        /// <param name="size">Anzahl der zu lesenden Bytes.</param>
        /// <returns>Anzahl der gelesenen Bytes.</returns>
        public override int ReadEntityBody( byte[] buffer, int size )
        {
            // Forward
            return m_Context.InputStream.Read( buffer, 0, size );
        }

        /// <summary>
        /// Prüfe, ob der aktuelle HTTP Zugriff SSL verwendet.
        /// </summary>
        /// <returns>Gesetzt, wenn die Verbindung zum Aufrufer über SSL erfolgt.</returns>
        public override bool IsSecure()
        {
            // Forward
            return m_Context.IsSecureConnection;
        }

        /// <summary>
        /// Ermittelt den Namen dieses Rechners.
        /// </summary>
        /// <returns>Der Name des Rechners, wie der Aufrufer ihn kennt.</returns>
        public override string GetServerName()
        {
            // Load
            string name = GetKnownRequestHeader( HeaderHost );

            // Use it
            if (!string.IsNullOrEmpty( name ))
            {
                // Check for port
                string[] splitName = name.Split( ':' );

                // Can use it
                if ((1 == splitName.Length) || (2 == splitName.Length)) return splitName[0];
            }

            // Default
            return Dns.GetHostName();
        }
    }
}
