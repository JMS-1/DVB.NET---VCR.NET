using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.NoMarshalComObjects;


namespace JMS.DVB.Provider.Duoflex
{
    /// <summary>
    /// Beschreibt die Steuerschnittstelle für die CI/CAM Aktivierung.
    /// </summary>
    internal class KsControl : NoMarshalBase<KsControl.Interface>
    {
        /// <summary>
        /// Gruppenkennung für die Ansteuerung der CI/CAM Einheit.
        /// </summary>
        private static readonly Guid CamControlIdentifier = new Guid( "0aa8a511-a240-11de-b130-000000004d56" );

        /// <summary>
        /// Gruppenkennung für die CI Einheit der Hardware.
        /// </summary>
        private static readonly Guid CommonInterfaceIdentifier = new Guid( "0aa8a501-a240-11de-b130-000000004d56" );

        /// <summary>
        /// Die eigentliche COM Schnittstelle.
        /// </summary>
        [
            ComImport,
            Guid( "28f54685-06fd-11d2-b27a-00a0c9223196" ),
            InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
        ]
        public interface Interface
        {
            /// <summary>
            /// Greift auf eine Eigenschaft zu.
            /// </summary>
            /// <param name="Property">Die Identifikation der Eigenschaft.</param>
            /// <param name="PropertyLength">Speichergröße der Identifikation.</param>
            /// <param name="PropertyValues">Die für den Aufruf benötigten Daten.</param>
            /// <param name="DataLength">Die Größe der Daten.</param>
            /// <param name="BytesReturned">Die tatsächlichen benutzen Datenbytes.</param>
            /// <returns>Negativ, falls die Operation nicht abgeschlossen werden konnte.</returns>
            [PreserveSig]
            Int32 KsProperty( ref KsEntity Property, int PropertyLength, UInt32[] PropertyValues, int DataLength, out UInt32 BytesReturned );

            /// <summary>
            /// Greift auf eine Methode zu.
            /// </summary>
            /// <param name="Method">Die Identifikation der Methode.</param>
            /// <param name="MethodLength">Speichergröße der Identifikation.</param>
            /// <param name="MethodData">Die für den Aufruf benötigten Daten.</param>
            /// <param name="DataLength">Die Größe der Daten.</param>
            /// <param name="BytesReturned">Die tatsächlichen benutzen Datenbytes.</param>
            /// <returns>Negativ, falls die Operation nicht abgeschlossen werden konnte.</returns>
            [PreserveSig]
            Int32 KsMethod( ref KsEntity Method, int MethodLength, IntPtr MethodData, int DataLength, out UInt32 BytesReturned );

            /// <summary>
            /// Greift auf ein Ereignis zu.
            /// </summary>
            /// <param name="Event">Die Identifikation des Ereignisses.</param>
            /// <param name="EventLength">Speichergröße der Identifikation.</param>
            /// <param name="EventData">Die für den Aufruf benötigten Daten.</param>
            /// <param name="DataLength">Die Größe der Daten.</param>
            /// <param name="BytesReturned">Die tatsächlichen benutzen Datenbytes.</param>
            /// <returns>Negativ, falls die Operation nicht abgeschlossen werden konnte.</returns>
            [PreserveSig]
            Int32 KsEvent( ref KsEntity Event, int EventLength, IntPtr EventData, int DataLength, out UInt32 BytesReturned );
        }

        /// <summary>
        /// Signature einer Methode zum Zugriff auf eine Eigenschaft.
        /// </summary>
        /// <param name="comObject">Das tatsächlich zu verwendende <i>Direct Show</i> Objekt.</param>
        /// <param name="Property">Die Identifikation der Eigenschaft.</param>
        /// <param name="PropertyLength">Speichergröße der Identifikation.</param>
        /// <param name="PropertyValues">Die für den Aufruf benötigten Daten.</param>
        /// <param name="DataLength">Die Größe der Daten.</param>
        /// <param name="BytesReturned">Die tatsächlichen benutzen Datenbytes.</param>
        /// <returns>Negativ, falls die Operation nicht abgeschlossen werden konnte.</returns>
        private delegate Int32 PropertySignature( IntPtr comObject, ref KsEntity Property, int PropertyLength, UInt32[] PropertyValues, int DataLength, out UInt32 BytesReturned );

        /// <summary>
        /// Signatur einer Methode zum Aufruf einer Methode.
        /// </summary>
        /// <param name="comObject">Das tatsächlich zu verwendende <i>Direct Show</i> Objekt.</param>
        /// <param name="Method">Die Identifikation der Methode.</param>
        /// <param name="MethodLength">Speichergröße der Identifikation.</param>
        /// <param name="MethodData">Die für den Aufruf benötigten Daten.</param>
        /// <param name="DataLength">Die Größe der Daten.</param>
        /// <param name="BytesReturned">Die tatsächlichen benutzen Datenbytes.</param>
        /// <returns>Negativ, falls die Operation nicht abgeschlossen werden konnte.</returns>       
        private delegate Int32 MethodSignature( IntPtr comObject, ref KsEntity Method, int MethodLength, IntPtr MethodData, int DataLength, out UInt32 BytesReturned );

        /// <summary>
        /// Die Methode zum Arbeiten mit Eigenschaften.
        /// </summary>
        private PropertySignature m_Property;

        /// <summary>
        /// Die Methode zum Arbeiten mit Methoden.
        /// </summary>
        private MethodSignature m_Method;

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="set">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        public KsControl( IntPtr set )
            : base( set )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="set">Die COM Referenz auf das <i>Direct Show</i> Objekt. Der
        /// Aufrufer hat für die Freigabe zu sorgen.</param>
        public KsControl( object set )
            : base( set )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="set">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        /// <param name="autoRelease">Gesetzt, wenn die Referenz nach der Übergabe freigegeben werden soll.</param>
        public KsControl( object set, bool autoRelease )
            : base( set, autoRelease )
        {
        }

        /// <summary>
        /// Erzeugt alle notwendigen Methodensignaturen.
        /// </summary>
        protected override void OnCreateDelegates()
        {
            // Create signature
            m_Property = CreateDelegate<PropertySignature>( 0 );
            m_Method = CreateDelegate<MethodSignature>( 1 );
        }

        /// <summary>
        /// Aktiviert die Entschlüsselung.
        /// </summary>
        /// <param name="services">Die Liste aller zu entschlüsselnden Dienste.</param>
        public void SetServices( params ushort[] services )
        {
            // None
            if (services == null)
                return;
            if (services.Length < 1)
                return;

            // Report
            if (BDASettings.BDATraceSwitch.Enabled)
                foreach (var service in services)
                    Trace.WriteLine( string.Format( Properties.Resources.Trace_Decrypt, service ), BDASettings.BDATraceSwitch.DisplayName );

            // Convert
            var identifiers = services.Select( s => (UInt32) s ).ToArray();

            // Create property to set it
            var entity = new KsEntity { Set = CommonInterfaceIdentifier, Id = (UInt32) CommonInterface.Decrpyt, Flags = 2 };

            // Process                
            UInt32 processed;
            var retcode = m_Property( ComInterface, ref entity, KsEntity.SizeOf, identifiers, identifiers.Length * sizeof( UInt32 ), out processed );
            if (retcode < 0)
                throw new COMException( "IKsControl.KsProperty", retcode );
        }

        /// <summary>
        /// Setzt die Hardware zurück.
        /// </summary>
        public void Reset()
        {
            // Create property to set it
            var entity = new KsEntity { Set = CamControlIdentifier, Id = (UInt32) CAMControl.Reset, Flags = 1 };

            // Process                
            UInt32 processed;
            var retcode = m_Method( ComInterface, ref entity, KsEntity.SizeOf, IntPtr.Zero, 0, out processed );
            if (retcode < 0)
                throw new COMException( "IKsControl.KsMethod", retcode );
        }
    }
}
