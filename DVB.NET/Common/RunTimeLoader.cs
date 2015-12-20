using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;


namespace JMS.DVB
{
    /// <summary>
    /// Übernimmt das dynamische Laden der DVB.NET Laufzeitbibliotheken.
    /// </summary>
    public class RunTimeLoader
    {
        /// <summary>
        /// Ergänzt ein Verzeichnis zum konventionellen Suchpfad für Bibliotheken.
        /// </summary>
        /// <param name="path">Der gewünschte Pfad.</param>
        /// <returns>Gesetzt, wenn die Operation erfolgreich war.</returns>
        [DllImport( "kernel32.dll" )]
        private static extern bool SetDllDirectory( string path );

        /// <summary>
        /// Stellt die einzige Instanz zur Verfügung.
        /// </summary>
        private static readonly RunTimeLoader m_Instance = new RunTimeLoader();

        /// <summary>
        /// Alle bereits dynamisch geladenen Bibliotheken.
        /// </summary>
        private Dictionary<string, Assembly> m_Loaded = new Dictionary<string, Assembly>();

        /// <summary>
        /// Gesetzt, wenn keine DVB.NET Installation vorhanden ist.
        /// </summary>
        public static bool InStandAloneMode { get; private set; }

        /// <summary>
        /// Der volle Pfad zur aktuellen Anwendung.
        /// </summary>
        private static volatile FileInfo m_ApplicationPath;

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        private RunTimeLoader()
        {
            // Register
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;

            // Extend driver path
            SetDllDirectory( GetDirectory( "Driver" ).FullName );
        }

        /// <summary>
        /// Prüft, ob die Laufzeitumgebung korrekt aufgesetzt ist.
        /// </summary>
        public static void Startup()
        {
            // Test
            if (m_Instance == null)
                throw new InvalidOperationException( "Startup" );
        }

        /// <summary>
        /// Meldet das DVB.NET Installationsverzeichnis.
        /// </summary>
        public static DirectoryInfo RootDirectory
        {
            get
            {
                // Forward
                return GetInstallationDirectory( "Root" );
            }
        }

        /// <summary>
        /// Meldet das Installationsverzeichnis des <i>Card Servers</i>.
        /// </summary>
        public static DirectoryInfo ServerDirectory
        {
            get
            {
                // Forward
                return GetInstallationDirectory( "Server" );
            }
        }

        /// <summary>
        /// Meldet den Pfad zur Anwendung.
        /// </summary>
        public static FileInfo ExecutablePath
        {
            get
            {
                // See if we already did it
                if (m_ApplicationPath == null)
                    m_ApplicationPath = CurrentApplication.Executable;

                // Report
                return m_ApplicationPath;
            }
        }

        /// <summary>
        /// Meldet das Arbeitsverzeichnis für den Fall, dass DVB.NET ohne Installation betrieben wird.
        /// </summary>
        private static DirectoryInfo StandAloneDirectory
        {
            get
            {
                // Remember - may be overwritten multiple times - so what?
                InStandAloneMode = true;

                // Report
                return ExecutablePath.Directory;
            }
        }

        /// <summary>
        /// Meldet ein Installationsverzeichnis.
        /// </summary>
        /// <param name="scope">Der Name der zugehörigen Installation.</param>
        /// <returns>Das gewünschte Verzeichnis, das über die Windows Registrierung ermittelt wird.</returns>
        /// <exception cref="InvalidOperationException">Es existiert keine entsprechende Installation.</exception>
        private static DirectoryInfo GetInstallationDirectory( string scope )
        {
            // Full name
            var entryName = string.Format( "{0} Directory", scope );

            // Get our version
            var name = Assembly.GetExecutingAssembly().GetName().Version;

            // Get the related registry path
            var path = @"SOFTWARE\JMS\DVB.NET\" + name.ToString();

            // Attach to the registry
            using (var reg32 = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Registry32 ))
            using (var key = reg32.OpenSubKey( path, false ))
            {
                // Not found
                if (key == null)
                    return StandAloneDirectory;

                // Read the entry
                var root = key.GetValue( entryName ) as string;
                if (string.IsNullOrEmpty( root ))
                    return StandAloneDirectory;

                // Report
                return new DirectoryInfo( root );
            }
        }

        /// <summary>
        /// Ermittelt ein Unterverzeichnis des Installationsverzeichnisses.
        /// </summary>
        /// <param name="relative">Der Name des Unterverzeichnisses.</param>
        /// <returns>Der gewünschte volle Pfad.</returns>
        public static DirectoryInfo GetDirectory( string relative )
        {
            // Merge
            return new DirectoryInfo( Path.Combine( RootDirectory.FullName, relative ) );
        }

        /// <summary>
        /// Meldet das Verzeichnis, an dem die Laufzeitbibiotheken erwartet werden.
        /// </summary>
        public static DirectoryInfo RunTimePath
        {
            get
            {
                // Merge
                return GetDirectory( "RunTime" );
            }
        }

        /// <summary>
        /// Meldet das Verzeichnis, an dem die spezifischen Geräteimplementierungen erwartet werden.
        /// </summary>
        public static DirectoryInfo AdapterPath
        {
            get
            {
                // Merge
                return GetDirectory( "Adapter" );
            }
        }

        /// <summary>
        /// Stellt eine Bibliothek dynamisch zur Verfügung.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="args">Informationen zur nicht gefundenen Bibliothek.</param>
        /// <returns>Die gewünschte Bibliothek.</returns>
        private Assembly Resolve( object sender, ResolveEventArgs args )
        {
            // Synchronize
            lock (m_Loaded)
            {
                // See if we already did it
                Assembly loaded;
                if (m_Loaded.TryGetValue( args.Name, out loaded ))
                    return loaded;

                // Split the name
                var name = new AssemblyName( args.Name );

                // Create the path
                var path = new FileInfo( Path.Combine( RunTimePath.FullName, name.Name + ".dll" ) );
                if (!path.Exists)
                    path = new FileInfo( Path.Combine( AdapterPath.FullName, name.Name + ".dll" ) );
                if (!path.Exists)
                    path = new FileInfo( Path.Combine( Path.Combine( ServerDirectory.FullName, "RunTime" ), name.Name + ".dll" ) );
                if (!path.Exists)
                    loaded = null;
                else
                    try
                    {
                        // Load the assembly
                        loaded = Assembly.LoadFrom( path.FullName );
                    }
                    catch
                    {
                        // Ignore any error
                        loaded = null;
                    }

                // Remember
                m_Loaded[args.Name] = loaded;

                // Report
                return loaded;
            }
        }
    }
}
