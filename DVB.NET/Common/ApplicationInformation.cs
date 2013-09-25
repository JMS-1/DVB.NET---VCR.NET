using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;


namespace JMS.DVB
{
    /// <summary>
    /// Stellt diverse Informationen zur aktuellen Anwendung zur Verfügung.
    /// </summary>
    public static class CurrentApplication
    {
        /// <summary>
        /// Ermittelt den Pfad zu einem Programmmodul.
        /// </summary>
        /// <param name="module">Das gewünschte Modul oder <i>null</i> für die aktuelle Anwendung selbst.</param>
        /// <param name="filename">Der volle Pfad zu dem Modul.</param>
        /// <param name="bufferSize">Die Größe des Speichers zur Aufnahme des Pfades.</param>
        /// <returns></returns>
        [DllImport( "Kernel32.dll", CharSet = CharSet.Unicode )]
        [SuppressUnmanagedCodeSecurity]
        private static extern Int32 GetModuleFileName( IntPtr module, StringBuilder filename, Int32 bufferSize );

        /// <summary>
        /// Der volle Pfad zur aktuellen Anwendung.
        /// </summary>
        public static readonly FileInfo Executable;

        /// <summary>
        /// Initialisiert die statischen einmal berechneten Informationen zur aktuellen Anwendung.
        /// </summary>
        static CurrentApplication()
        {
            // The file name buffer
            var buf = new StringBuilder( 10000 );

            // Load the name
            buf.Length = GetModuleFileName( IntPtr.Zero, buf, buf.Capacity - 1 );

            // Use it
            Executable = new FileInfo( buf.ToString() );
        }

        /// <summary>
        /// Meldet ein Unterverzeichnis zum Arbeitsverzeichnis der aktuellen Anwendung.
        /// </summary>
        /// <param name="relativeName">Der gewünschte Verzeichnisname.</param>
        /// <returns>Das angeforderte Verzeichnis.</returns>
        public static DirectoryInfo GetDirectory( string relativeName )
        {
            // Merge
            return new DirectoryInfo( Path.Combine( Executable.DirectoryName, relativeName ) );
        }

        /// <summary>
        /// Lädt alle Erweiterungen einer bestimmten Art aus den Assemblies in einem
        /// Unterverzeichnis. Alle Assemblies werden dazu in den Speicher geladen.
        /// </summary>
        /// <typeparam name="T">Die Art der Erweiterung, üblicherweise eine <i>Factory</i>
        /// für tatsächliche Instanzen.</typeparam>
        /// <param name="dirName">Das gewünschte Verzeichnis.</param>
        /// <returns>Alle ermittelten Erweiterungen.</returns>
        public static T[] CreatePlugInFactories<T>( string dirName )
        {
            // Result
            var list = new List<T>();

            // Attach to directory
            var plugInDir = GetDirectory( dirName );
            if (plugInDir.Exists)
                foreach (var plugInFile in plugInDir.GetFiles( "*.dll" ))
                {
                    // Fast check
                    AssemblyName name;
                    try
                    {
                        // Load name
                        name = AssemblyName.GetAssemblyName( plugInFile.FullName );
                        if (name == null)
                            continue;
                    }
                    catch
                    {
                        // Skip
                        continue;
                    }

                    // Safe load
                    Assembly plugInAssembly;
                    try
                    {
                        // Load it
                        plugInAssembly = Assembly.Load( name );
                        if (plugInAssembly == null)
                            continue;
                    }
                    catch
                    {
                        // Skip
                        continue;
                    }

                    // Process all types
                    try
                    {
                        // All types
                        foreach (var plugIn in plugInAssembly.GetExportedTypes())
                            try
                            {
                                // See if we should use it
                                if (plugIn.IsClass)
                                    if (!plugIn.IsAbstract)
                                        if (typeof( T ).IsAssignableFrom( plugIn ))
                                            list.Add( (T) Activator.CreateInstance( plugIn ) );
                            }
                            catch
                            {
                                // Skip
                                continue;
                            }
                    }
                    catch
                    {
                        // Skip
                        continue;
                    }
                }

            // Report
            return list.ToArray();
        }
    }
}
