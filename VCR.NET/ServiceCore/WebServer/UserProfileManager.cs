using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Profile;
using System.Xml;


namespace JMS.DVBVCR.RecordingService.WebServer
{
    /// <summary>
    /// Verwaltet VCR.NET Benutzerprofile.
    /// </summary>
    public class UserProfileManager : ProfileProvider
    {
        /// <summary>
        /// Die Signature einer Methode zum Umwandeln von Zeichenketten in
        /// skalare Werte.
        /// </summary>
        private static Type[] ParseSignature = { typeof( string ) };

        /// <summary>
        /// Globale Sperre für Dateizugriffe.
        /// </summary>
        private static object m_FileLock = new object();

        /// <summary>
        /// Der Name der zugehörigen Anwendung.
        /// </summary>
        public override string ApplicationName { get; set; }

        /// <summary>
        /// Erzeugt eine neue Verwaltungsinstanz.
        /// </summary>
        public UserProfileManager()
        {
            // Configure 
            ApplicationName = "VCR.NET";
        }

        /// <summary>
        /// Wird zum Löschen von Benutzerprofilen aufgerufen und macht in dieser Implementierung
        /// gar nichts.
        /// </summary>
        /// <param name="usernames">Die Namen der Anwender, deren Profile entfernt werden sollen.</param>
        /// <returns>Die Anzahl der entfernten Profile.</returns>
        public override int DeleteProfiles( string[] usernames )
        {
            return 0;
        }

        /// <summary>
        /// Wird zum Löschen von Benutzerprofilen aufgerufen und macht in dieser Implementierung
        /// gar nichts.
        /// </summary>
        /// <param name="profiles">Eine Sammlung der zu löschenden Profile..</param>
        /// <returns>Die Anzahl der entfernten Profile.</returns>
        public override int DeleteProfiles( ProfileInfoCollection profiles )
        {
            return 0;
        }

        /// <summary>
        /// Jöscht nicht mehr verwendete Benutzerprofile.
        /// </summary>
        /// <param name="authenticationOption">Die Art der Benutzerauthentisierung.</param>
        /// <param name="userInactiveSinceDate">Das Bezugsdatum für die Prüfung auf Inaktivität.</param>
        /// <returns>Die Anzahl der entfernten Profile.</returns>
        public override int DeleteInactiveProfiles( ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate )
        {
            return 0;
        }

        /// <summary>
        /// Ermittelt die Anzahl nicht mehr verwendeter Benutzerprofile.
        /// </summary>
        /// <param name="authenticationOption">Die Art der Benutzerauthentisierung.</param>
        /// <param name="userInactiveSinceDate">Das Bezugsdatum für die Prüfung auf Inaktivität.</param>
        /// <returns>Die Anzahl der nicht mehr verwendeten Benutzerprofile.</returns>
        public override int GetNumberOfInactiveProfiles( ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate )
        {
            return 0;
        }

        /// <summary>
        /// Erzeugt eine leere Auflistung für Benutzerprofile.
        /// </summary>
        /// <param name="totalRecords">Meldet die gesamte Anzahl von Einträgen.</param>
        /// <returns>Eine neue, leere Auflistung.</returns>
        private ProfileInfoCollection CreateEmptyCollection( out int totalRecords )
        {
            // Create collection
            var result = new ProfileInfoCollection();

            // None
            totalRecords = 0;

            // Finish
            return result;
        }

        /// <summary>
        /// Meldet alle bekannten Benutzerprofile.
        /// </summary>
        /// <param name="authenticationOption">Die Art der Benutzerauthentisierung.</param>
        /// <param name="pageIndex">Die aktuelle Seite in der Liste zum Durchblättern.</param>
        /// <param name="pageSize">Die Anzahl der Profile pro Seite.</param>
        /// <param name="totalRecords">Meldet die gesamte Anzahl von Profilen.</param>
        /// <returns>Eine auflistung mit den gewünschten Profilen.</returns>
        public override ProfileInfoCollection GetAllProfiles( ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords )
        {
            // Forward
            return CreateEmptyCollection( out totalRecords );
        }

        /// <summary>
        /// Meldet alle Benutzerprofile, die nicht mehr verwendet werden.
        /// </summary>
        /// <param name="authenticationOption">Die Art der Benutzerauthentisierung.</param>
        /// <param name="userInactiveSinceDate">Das Datum zur Feststellung der Inaktivität.</param>
        /// <param name="pageIndex">Die aktuelle Seite in der Liste zum Durchblättern.</param>
        /// <param name="pageSize">Die Anzahl der Profile pro Seite.</param>
        /// <param name="totalRecords">Meldet die gesamte Anzahl von Profilen.</param>
        /// <returns>Eine auflistung mit den gewünschten Profilen.</returns>
        public override ProfileInfoCollection GetAllInactiveProfiles( ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords )
        {
            // Forward
            return CreateEmptyCollection( out totalRecords );
        }

        /// <summary>
        /// Meldet alle Benutzerprofile zu einem Benutzernamen.
        /// </summary>
        /// <param name="authenticationOption">Die Art der Benutzerauthentisierung.</param>
        /// <param name="usernameToMatch">Der bei der Suche zu verwendende Benutzername.</param>
        /// <param name="pageIndex">Die aktuelle Seite in der Liste zum Durchblättern.</param>
        /// <param name="pageSize">Die Anzahl der Profile pro Seite.</param>
        /// <param name="totalRecords">Meldet die gesamte Anzahl von Profilen.</param>
        /// <returns>Eine auflistung mit den gewünschten Profilen.</returns>
        public override ProfileInfoCollection FindProfilesByUserName( ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords )
        {
            // Forward
            return CreateEmptyCollection( out totalRecords );
        }

        /// <summary>
        /// Meldet alle nicht mehr verwendeten Benutzerprofile zu einem Benutzernamen.
        /// </summary>
        /// <param name="authenticationOption">Die Art der Benutzerauthentisierung.</param>
        /// <param name="usernameToMatch">Der bei der Suche zu verwendende Benutzername.</param>
        /// <param name="userInactiveSinceDate">Das Datum zur Feststellung der Inaktivität.</param>
        /// <param name="pageIndex">Die aktuelle Seite in der Liste zum Durchblättern.</param>
        /// <param name="pageSize">Die Anzahl der Profile pro Seite.</param>
        /// <param name="totalRecords">Meldet die gesamte Anzahl von Profilen.</param>
        /// <returns>Eine auflistung mit den gewünschten Profilen.</returns>
        public override ProfileInfoCollection FindInactiveProfilesByUserName( ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords )
        {
            // Forward
            return CreateEmptyCollection( out totalRecords );
        }

        /// <summary>
        /// Ermittelt alle Einträge aus einem Benutzerprofil.
        /// </summary>
        /// <param name="context">Informationen zur aktuellen Arbeitsumgebung.</param>
        /// <param name="collection">Alle benötigten Eigenschaften aus dem Profil.</param>
        /// <returns>Eine Auflistung der gewünschten Eigenschaften und den zugehörigen Werten.</returns>
        public override SettingsPropertyValueCollection GetPropertyValues( SettingsContext context, SettingsPropertyCollection collection )
        {
            // Profile data
            XmlDocument xml = new XmlDocument();

            // Must be autenticated
            if ((bool) context["IsAuthenticated"])
                try
                {
                    // Try load
                    lock (m_FileLock)
                        xml.Load( GetUserProfile( context ) );
                }
                catch
                {
                    // Save reset
                    xml = new XmlDocument();
                }

            // Create empty
            var result = new SettingsPropertyValueCollection();

            // Blind copy all
            foreach (SettingsProperty property in collection)
            {
                // Create
                var value = new SettingsPropertyValue( property );

                // Get default
                string defaultValue = null;

                // Attach to the related node
                var node = xml.SelectSingleNode( "VCRNETUserSettings/" + property.Name );
                if (node != null)
                    if (node.Name.Equals( "RecentChannels" ))
                    {
                        // Attach
                        var strings = (StringCollection) value.PropertyValue;

                        // Reset
                        strings.Clear();

                        // Fill
                        foreach (XmlNode channel in node.SelectNodes( "Channel" ))
                            strings.Add( channel.InnerText );
                    }
                    else
                    {
                        // Load
                        defaultValue = node.InnerText;
                    }

                // Get default
                if (string.IsNullOrEmpty( defaultValue ))
                    defaultValue = property.DefaultValue as string;

                // Update
                if (!string.IsNullOrEmpty( defaultValue ))
                {
                    // Find method
                    var parse = property.PropertyType.GetMethod( "Parse", ParseSignature );

                    // Set current value
                    if (null == parse)
                        value.PropertyValue = defaultValue;
                    else
                        value.PropertyValue = parse.Invoke( null, new object[] { defaultValue } );
                }

                // Store
                result.Add( value );
            }

            // Report
            return result;
        }

        /// <summary>
        /// Ändert Einstellungen im Benutzerprofil.
        /// </summary>
        /// <param name="context">Die aktuelle Arbeitsumgebung.</param>
        /// <param name="collection">Die Einstellungen und die zugehörigen Werte, wie sie zu aktualisieren sind.</param>
        public override void SetPropertyValues( SettingsContext context, SettingsPropertyValueCollection collection )
        {
            // Must be autenticated
            if (!(bool) context["IsAuthenticated"])
                return;

            // Create the file contents as XML
            var xml = new XmlDocument();
            var xmlRoot = xml.AppendChild( xml.CreateElement( "VCRNETUserSettings" ) );

            // Fill
            foreach (SettingsPropertyValue property in collection)
            {
                // Store contents
                var xmlNode = xmlRoot.AppendChild( xml.CreateElement( property.Name ) );
                if (xmlNode.Name.Equals( "RecentChannels" ))
                    foreach (string channel in (IEnumerable) property.PropertyValue)
                        xmlNode.AppendChild( xml.CreateElement( "Channel" ) ).InnerText = channel;
                else
                    xmlNode.InnerText = property.PropertyValue.ToString();
            }

            // Synchronized
            lock (m_FileLock)
                xml.Save( GetUserProfile( context ) );
        }

        /// <summary>
        /// Ermittelt das Benutzerprofil einer Arbeitsumgebung.
        /// </summary>
        /// <param name="context">Die betroffene Arbeitsumgebung.</param>
        /// <returns>Der volle Pfad zum VCR.NET Benutzerprofil.</returns>
        private string GetUserProfile( SettingsContext context )
        {
            // Get the user name
            var user = (string) context["UserName"];

            // Beautify it
            foreach (var test in Path.GetInvalidFileNameChars())
                user = user.Replace( test, '_' );

            // Attach to directory
            var dir = new DirectoryInfo( Path.Combine( HttpContext.Current.Request.PhysicalApplicationPath, "App_Data" ) );

            // Create
            if (!dir.Exists)
                dir.Create();

            // Report
            return Path.Combine( dir.FullName, user + ".vup" );
        }

        /// <summary>
        /// Meldet eine Beschreibung.
        /// </summary>
        public override string Description { get { return "User Profile Manager for VCR.NET"; } }

        /// <summary>
        /// Meldet unseren Namen.
        /// </summary>
        public override string Name { get { return "VCRNETProvider"; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize( string name, NameValueCollection config )
        {
            base.Initialize( name, config );
        }
    }
}
