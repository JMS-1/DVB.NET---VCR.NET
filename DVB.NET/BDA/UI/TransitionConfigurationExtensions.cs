using System;
using System.IO;


namespace JMS.DVB.DirectShow.UI
{
    /// <summary>
    /// Methoden zur einfacheren Nutzung der <see cref="TransitionConfiguration"/> Klasse.
    /// </summary>
    public static class TransitionConfigurationExtensions
    {
        /// <summary>
        /// Speichert die Konfiguration in einer Datei.
        /// </summary>
        /// <param name="configuration">Die gewünschte Konfiguration.</param>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        /// <param name="mode">Die Art, wie die Datei zu öffnen ist.</param>
        public static void Save( this TransitionConfiguration configuration, string path, FileMode mode )
        {
            // Validate
            if (configuration == null)
                throw new ArgumentNullException( "configuration" );
            if (string.IsNullOrEmpty( path ))
                throw new ArgumentNullException( "path" );

            // Just open file and do it
            using (var stream = new FileStream( path, mode, FileAccess.Write, FileShare.None ))
                configuration.Save( stream );
        }

        /// <summary>
        /// Speichert die Konfiguration in einer Datei.
        /// </summary>
        /// <param name="configuration">Die gewünschte Konfiguration.</param>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        public static void Save( this TransitionConfiguration configuration, string path )
        {
            // Forward
            configuration.Save( path, FileMode.Create );
        }

        /// <summary>
        /// Speichert die Konfiguration in einer Datei.
        /// </summary>
        /// <param name="configuration">Die gewünschte Konfiguration.</param>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        /// <param name="mode">Die Art, wie die Datei zu öffnen ist.</param>
        public static void Save( this TransitionConfiguration configuration, FileInfo path, FileMode mode )
        {
            // Validate
            if (configuration == null)
                throw new ArgumentNullException( "configuration" );
            if (path == null)
                throw new ArgumentNullException( "path" );

            // Forward
            configuration.Save( path.FullName, mode );
        }

        /// <summary>
        /// Speichert die Konfiguration in einer Datei.
        /// </summary>
        /// <param name="configuration">Die gewünschte Konfiguration.</param>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        public static void Save( this TransitionConfiguration configuration, FileInfo path )
        {
            // Forward
            configuration.Save( path, FileMode.Create );
        }

        /// <summary>
        /// Lädt eine Konfiguration aus einer Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        /// <returns>Die rekonstruierte Instanz.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Datei angegeben.</exception>
        public static TransitionConfiguration Load( string path )
        {
            // Validate
            if (string.IsNullOrEmpty( path ))
                throw new ArgumentNullException( "path" );

            // Open and process
            using (var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read ))
                return TransitionConfiguration.Load( stream );
        }

        /// <summary>
        /// Lädt eine Konfiguration aus einer Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        /// <returns>Die rekonstruierte Instanz.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Datei angegeben.</exception>
        public static TransitionConfiguration Load( FileInfo path )
        {
            // Validate
            if (path == null)
                throw new ArgumentNullException( "path" );

            // Forward
            return Load( path.FullName );
        }

        /// <summary>
        /// Lädt eine Konfiguration aus Ressourcen.
        /// </summary>
        /// <param name="data">Die binär gespeicherten Ressourcen, erwartet wird
        /// letzlich ein Dateiinhalt im UNICODE Format.</param>
        /// <returns>Die gewünschte Instanz.</returns>
        /// <exception cref="ArgumentNullException">Es wurden keinerlei Daten übergeben.</exception>
        public static TransitionConfiguration Load( byte[] data )
        {
            // Validate
            if (data == null)
                throw new ArgumentNullException( "data" );

            // Open and process
            using (var stream = new MemoryStream( data, false ))
                return TransitionConfiguration.Load( stream );
        }
    }
}
