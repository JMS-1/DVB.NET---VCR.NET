using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Einige Hilfsklassen zum Umgang mit in XML Form persistent gespeicherten .NET Datenstrukturen.
    /// </summary>
    public static class SerializationTools
    {
        /// <summary>
        /// Lädt eine Datenstruktur aus einer Datei.
        /// </summary>
        /// <typeparam name="T">Die Art der Datenstruktur.</typeparam>
        /// <param name="info">Die gewünschte Datei.</param>
        /// <returns>Die rekonstruierte Datenstruktur.</returns>
        public static T Load<T>( FileInfo info )
        {
            // Be safe
            try
            {
                // Open stream and read
                using (var file = new FileStream( info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read ))
                    return Load<T>( file );
            }
            catch
            {
                // None
                return default( T );
            }
        }

        /// <summary>
        /// Lädt eine Datenstruktur aus einem Datenstrom.
        /// </summary>
        /// <typeparam name="T">Die Art der Datenstruktur.</typeparam>
        /// <param name="stream">Der vorpositionierte Datenstrom.</param>
        /// <returns>Die rekonstruierte Datenstruktur.</returns>
        public static T Load<T>( Stream stream )
        {
            // Be safe
            try
            {
                // Deserialisierer erzeugen
                var deserializer = new XmlSerializer( typeof( T ) );

                // Create reader settings
                var settings = new XmlReaderSettings { CheckCharacters = false };

                // Create
                using (var reader = XmlReader.Create( stream, settings ))
                    return (T) deserializer.Deserialize( reader );
            }
            catch
            {
                // None
                return default( T );
            }
        }

        /// <summary>
        /// Speichert ein Objekt im XML Format in eine Datei.
        /// </summary>
        /// <param name="instance">Das zu speichernde Objekt.</param>
        /// <param name="path">Der Pfad zur Datei.</param>
        public static void Save( object instance, FileInfo path )
        {
            // Forward
            Save( instance, path.FullName );
        }

        /// <summary>
        /// Speichert ein Objekt im XML Format in eine Datei.
        /// </summary>
        /// <param name="instance">Das zu speichernde Objekt.</param>
        /// <param name="path">Der Pfad zur Datei.</param>
        public static void Save( object instance, string path )
        {
            // Forward
            Save( instance, path, Encoding.Unicode );
        }

        /// <summary>
        /// Speichert ein Objekt im XML Format in eine Datei. Fehler werden protokolliert und
        /// nicht als Ausnahmen gemeldet.
        /// </summary>
        /// <param name="instance">Das zu speichernde Objekt.</param>
        /// <param name="path">Der Pfad zur Datei.</param>
        /// <returns>Gesetzt, wenn kein Fehler aufgetreten ist.</returns>
        public static bool SafeSave( object instance, string path )
        {
            // Be safe
            try
            {
                // Write it out
                SerializationTools.Save( instance, path );

                // Did it
                return true;
            }
            catch (Exception e)
            {
                // Report
                VCRServer.Log( e );

                // Failed
                return false;
            }
        }

        /// <summary>
        /// Speichert ein Objekt im XML Format in eine Datei.
        /// </summary>
        /// <param name="instance">Das zu speichernde Objekt.</param>
        /// <param name="path">Der Pfad zur Datei.</param>
        /// <param name="encoding">Die zu verwendende Zeichensatzcodierung.</param>
        public static void Save( object instance, FileInfo path, Encoding encoding )
        {
            // Forward
            Save( instance, path.FullName, encoding );
        }

        /// <summary>
        /// Speichert ein Objekt im XML Format in eine Datei.
        /// </summary>
        /// <param name="instance">Das zu speichernde Objekt.</param>
        /// <param name="path">Der Pfad zur Datei.</param>
        /// <param name="encoding">Die zu verwendende Zeichensatzcodierung.</param>
        public static void Save( object instance, string path, Encoding encoding )
        {
            // Be safe
            try
            {
                // Create stream and forward
                using (var file = new FileStream( path, FileMode.Create, FileAccess.Write, FileShare.None ))
                    Save( instance, file, encoding );
            }
            catch
            {
                // Try to delete
                try
                {
                    // Process
                    File.Delete( path );
                }
                catch
                {
                    // Ignore any error
                }

                // Forward original error
                throw;
            }
        }

        /// <summary>
        /// Speichert ein Objekt im XML Format in einen Datenstrom.
        /// </summary>
        /// <param name="instance">Das zu speichernde Objekt.</param>
        /// <param name="stream">Der Datenstrom, an dessen aktuelle Position das Objekt gespeichert werden soll.</param>
        public static void Save( object instance, Stream stream )
        {
            // Forward
            Save( instance, stream, Encoding.Unicode );
        }

        /// <summary>
        /// Speichert ein Objekt im XML Format in einen Datenstrom.
        /// </summary>
        /// <param name="instance">Das zu speichernde Objekt.</param>
        /// <param name="stream">Der Datenstrom, an dessen aktuelle Position das Objekt gespeichert werden soll.</param>
        /// <param name="encoding">Die zu verwendende Zeichensatzcodierung.</param>
        public static void Save( object instance, Stream stream, Encoding encoding )
        {
            // Create serializer
            var serializer = new XmlSerializer( instance.GetType() );

            // Create settings
            var settings = new XmlWriterSettings { Encoding = encoding, Indent = true, CheckCharacters = false };

            // Create writer
            using (var writer = XmlWriter.Create( stream, settings ))
                serializer.Serialize( writer, instance );
        }
    }
}
