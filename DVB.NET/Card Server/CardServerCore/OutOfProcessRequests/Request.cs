using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Basisklasse zur Implementierung von Anfragen an einen <i>Card Server</i>.
    /// </summary>
    [Serializable]
    [XmlInclude( typeof( AddSourcesRequest ) )]
    [XmlInclude( typeof( EndEPGRequest ) )]
    [XmlInclude( typeof( EndScanRequest ) )]
    [XmlInclude( typeof( GetStateRequest ) )]
    [XmlInclude( typeof( RemoveAllSourcesRequest ) )]
    [XmlInclude( typeof( RemoveSourceRequest ) )]
    [XmlInclude( typeof( SelectRequest ) )]
    [XmlInclude( typeof( SetProfileRequest ) )]
    [XmlInclude( typeof( SetStreamTargetRequest ) )]
    [XmlInclude( typeof( StartEPGRequest ) )]
    [XmlInclude( typeof( StartScanRequest ) )]
    [XmlInclude( typeof( SetZappingSourceRequest ) )]
    [XmlInclude( typeof( LoadExtensionsRequest ) )]
    public abstract class Request
    {
        /// <summary>
        /// Die Einheiten, in denen Pakete verschickt werden.
        /// </summary>
        private const int BlockSize = 10000;

        /// <summary>
        /// Die Instanz zur Rekonstruktion von Anfragen.
        /// </summary>
        private static XmlSerializer m_Serializer;

        /// <summary>
        /// Alle zusätzlichen Datentypen, die bei der Serialisierung berücksichtigt werden sollen.
        /// </summary>
        private static HashSet<Type> m_ExtraTypes = new HashSet<Type>();

        /// <summary>
        /// Initialisiert allgemeine Felder.
        /// </summary>
        static Request()
        {
            // Load serializer
            CreateSerializer();
        }

        /// <summary>
        /// Erzeugt die Serialisierungskomponente.
        /// </summary>
        private static void CreateSerializer()
        {
            // Load serializer
            lock (m_ExtraTypes)
                m_Serializer = new XmlSerializer( typeof( Request ), m_ExtraTypes.ToArray() );
        }

        /// <summary>
        /// Stellt sicher, dass der Serialisierer alle benötigten Datentypen kennt.
        /// </summary>
        /// <param name="assembly">Eine Erweiterungsbibliothek.</param>
        internal static void AddTypes( Assembly assembly )
        {
            // Nothing to do
            if (assembly == null)
                return;

            // Process all
            lock (m_ExtraTypes)
            {
                // Check for update
                bool mustUpdate = false;

                // Check all
                foreach (var type in assembly.GetExportedTypes())
                {
                    // Resolve to base
                    var genericBase = FindCustomActionBase( type );
                    if (genericBase != null)
                        if (m_ExtraTypes.Add( typeof( CustomActionRequest<,> ).MakeGenericType( genericBase.GetGenericArguments() ) ))
                            mustUpdate = true;
                }

                // Reload serializer
                if (mustUpdate)
                    CreateSerializer();
            }
        }

        /// <summary>
        /// Ermittelt die passende Basisklasse zu einer beliebigen Klasse.
        /// </summary>
        /// <param name="anyType">Eine beliebige Klasse.</param>
        /// <returns>Ein Basistyp dieser generischen Klasse oder <i>null</i>.</returns>
        internal static Type FindCustomActionBase( Type anyType )
        {
            // Process
            if (anyType != null)
                if (anyType.IsClass)
                    if (!anyType.IsAbstract)
                        for (; anyType != null; anyType = anyType.BaseType)
                            if (anyType.IsGenericType)
                                if (anyType.GetGenericTypeDefinition() == typeof( CustomAction<,> ))
                                    return anyType;

            // Not found
            return null;
        }

        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        internal Request()
        {
        }

        /// <summary>
        /// Meldet die Art der Antwort.
        /// </summary>
        protected abstract Type ResponseType { get; }

        /// <summary>
        /// Überträgt ein Objekt in eine Kommunikationseinheit.
        /// </summary>
        /// <param name="pipe">Die Kommunikationseinheit.</param>
        /// <param name="serializer">Die Serialisierungsinstanz.</param>
        /// <param name="response">Das betroffene Objekt.</param>
        internal static void SendToPipe( PipeStream pipe, XmlSerializer serializer, Response response )
        {
            // Forward
            SendToPipe( pipe, serializer, (object) response );
        }

        /// <summary>
        /// Überträgt ein Objekt in eine Kommunikationseinheit.
        /// </summary>
        /// <param name="pipe">Die Kommunikationseinheit.</param>
        /// <param name="serializer">Die Serialisierungsinstanz.</param>
        /// <param name="instance">Das betroffene Objekt.</param>
        internal static void SendToPipe( PipeStream pipe, XmlSerializer serializer, object instance )
        {
            // Create helper
            using (var temp = new MemoryStream())
            {
                // Create writer configuration
                var settings = new XmlWriterSettings { CheckCharacters = false };

                // Create
                using (var writer = XmlWriter.Create( temp, settings ))
                    serializer.Serialize( writer, instance );

                // Read data and create buffer for length
                var len = new byte[sizeof( long )];
                var data = temp.ToArray();

                // Lock briefly
                var lenLock = GCHandle.Alloc( len, GCHandleType.Pinned );
                try
                {
                    // Fill
                    Marshal.WriteInt64( lenLock.AddrOfPinnedObject(), data.LongLength );
                }
                finally
                {
                    // Release
                    lenLock.Free();
                }

                // Send all
                pipe.Write( len, 0, len.Length );

                // Write in blocks
                for (int n = 0; n < data.Length; )
                {
                    // Block size
                    int block = Math.Min( BlockSize, data.Length - n );

                    // Send chunck
                    pipe.Write( data, n, block );
                    pipe.Flush();

                    // Advance
                    n += block;
                }

                // Flush
                pipe.Flush();
                pipe.WaitForPipeDrain();
            }
        }

        /// <summary>
        /// Rekonstruiert ein Objekt aus dem angegebenen Kommunikationskanal.
        /// </summary>
        /// <param name="pipe">Der gewünschte Kanal.</param>
        /// <param name="serializer">Die Instanz zur Rekonstruktion des Objektes.</param>
        /// <returns>Das rekonstruierte Objekt.</returns>
        private static object ReadFromPipe( PipeStream pipe, XmlSerializer serializer )
        {
            // Allocate length field
            byte[] len = new byte[sizeof( long )], data;

            // Load it
            if (len.Length != pipe.Read( len, 0, len.Length ))
                return null;

            // Lock briefly
            var lenLock = GCHandle.Alloc( len, GCHandleType.Pinned );
            try
            {
                // Fill
                data = new byte[Marshal.ReadInt64( lenLock.AddrOfPinnedObject() )];
            }
            finally
            {
                // Release
                lenLock.Free();
            }

            // Read in blocks
            for (int n = 0; n < data.Length; )
            {
                // Block size
                int block = Math.Min( BlockSize, data.Length - n );

                // Load chunk
                if (pipe.Read( data, n, block ) != block)
                    return null;

                // Advance
                n += block;
            }

            // Create settings
            var settings = new XmlReaderSettings { CheckCharacters = false };

            // Reconstruct
            using (var temp = new MemoryStream( data, false ))
            using (var reader = XmlReader.Create( temp, settings ))
                return serializer.Deserialize( reader );
        }

        /// <summary>
        /// Überträgt diese Anfrage in der XML Repräsentation in einen Datenstrom.
        /// </summary>
        /// <param name="stream">Der gewünschte Datenstrom.</param>
        public void SendRequest( AnonymousPipeServerStream stream )
        {
            // Forward
            SendToPipe( stream, m_Serializer, this );
        }

        /// <summary>
        /// Nimmt eine Anfrage entgegen.
        /// </summary>
        /// <param name="stream">Der Datenstrom, über den die Anfrage übertragen wird.</param>
        /// <returns>Die gewünschte Anfrage.</returns>
        public static Request ReceiveRequest( AnonymousPipeClientStream stream )
        {
            // Process
            return (Request) ReadFromPipe( stream, m_Serializer );
        }

        /// <summary>
        /// Nimmt eine Antwort zu einer Anfrage entgegen.
        /// </summary>
        /// <param name="stream">Der Datenstrom, auf dem die Antwort bereitgestellt wird.</param>
        /// <returns>Die gewünschte Antwort.</returns>
        public Response ReceiveResponse( AnonymousPipeServerStream stream )
        {
            // Process
            return (Response) ReadFromPipe( stream, new XmlSerializer( ResponseType ) );
        }

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <param name="server">Die aktuelle Implementierung des <i>Card Servers</i>.</param>
        /// <returns>Die Antwort zur Anfrage.</returns>
        public abstract Response Execute( ServerImplementation server );
    }

    /// <summary>
    /// Basisklasse zur Implementierung von Anfragen an einen <i>Card Server</i>.
    /// </summary>
    /// <typeparam name="T">Die Art der Antwort.</typeparam>
    public abstract class Request<T> : Request where T : Response, new()
    {
        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        protected Request()
        {
        }

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <param name="server">Die aktuelle Implementierung des <i>Card Servers</i>.</param>
        /// <returns>Die Antwort zur Anfrage.</returns>
        public override Response Execute( ServerImplementation server )
        {
            // Create response
            T response = new T();

            // Be full safe - will always report a legal instance
            try
            {
                // Just forward
                OnExecute( response, server );
            }
            catch (CardServerException e)
            {
                // Translate
                response.Fault = e.Fault;
            }
            catch (Exception e)
            {
                // Translate
                response.Fault = new CardServerFault( e.Message );
            }

            // Report
            return response;
        }

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <param name="response">Die zu befüllende Antwort für den Aufrufer.</param>
        /// <param name="server">Die aktuelle Implementierung des <i>Card Servers</i>.</param>
        /// <returns>Die Antwort zur Anfrage.</returns>
        protected abstract void OnExecute( T response, ServerImplementation server );

        /// <summary>
        /// Meldet die Art der Antwort.
        /// </summary>
        protected override Type ResponseType
        {
            get
            {
                // Report
                return typeof( T );
            }
        }
    }
}
