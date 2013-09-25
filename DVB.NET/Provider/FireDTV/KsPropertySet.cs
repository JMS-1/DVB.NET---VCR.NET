using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.NoMarshalComObjects;


namespace JMS.DVB.Provider.FireDTV
{
    /// <summary>
    /// Beschreibt die Schnittstelle zum Setzen von Detailparametern.
    /// </summary>
    internal class KsPropertySetFireDTV : NoMarshalBase<KsPropertySetFireDTV.Interface>
    {
        /// <summary>
        /// Die eigentliche COM Schnittstelle.
        /// </summary>
        [
            ComImport,
            Guid( "31efac30-515c-11d0-a9aa-00aa0061be93" ),
            InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
        ]
        public interface Interface
        {
            /// <summary>
            /// Setzt einen Parameter.
            /// </summary>
            /// <param name="propertySetId">Die eindeutige Kennung der Parametergruppe.</param>
            /// <param name="propertyId">Die laufende Nummer des Parameters.</param>
            /// <param name="instanceData">Die Daten der Instanz.</param>
            /// <param name="instanceDataLength">Die Größe der Daten der Instanz.</param>
            /// <param name="value">Der neue Wert.</param>
            /// <param name="valueLength">Die Größe des neuen Wertes.</param>
            void Set( ref Guid propertySetId, BDAProperties propertyId, ref CACommand instanceData, Int32 instanceDataLength, ref CACommand value, Int32 valueLength );

            /// <summary>
            /// Platzhalter zum Auslesen eines Wertes.
            /// </summary>
            /// <returns>Das Ergebnis des Auslesens.</returns>
            [return: MarshalAs( UnmanagedType.U4 )]
            UInt32 Get();

            /// <summary>
            /// Prüft, ob ein bestimmter Parameter angeboten wird.
            /// </summary>
            /// <param name="propertySetId">Die betroffene Parametergruppe.</param>
            /// <param name="propertyId">Der gewünschte Parameter.</param>
            /// <returns>Die Art der Unterstützung des Parameters.</returns>
            [return: MarshalAs( UnmanagedType.I4 )]
            PropertySetSupportedTypes QuerySupported( ref Guid propertySetId, BDAProperties propertyId );
        }

        /// <summary>
        /// Signature einer Methode zum Setzen eines Parameters.
        /// </summary>
        /// <param name="comObject">Die Rohschnittstelle zum Aufruf.</param>
        /// <param name="propertySetId">Die eindeutige Kennung der Parametergruppe.</param>
        /// <param name="propertyId">Die laufende Nummer des Parameters.</param>
        /// <param name="instanceData">Die Daten der Instanz.</param>
        /// <param name="instanceDataLength">Die Größe der Daten der Instanz.</param>
        /// <param name="value">Der neue Wert.</param>
        /// <param name="valueLength">Die Größe des neuen Wertes.</param>
        private delegate Int32 SetSignature( IntPtr comObject, ref Guid propertySetId, BDAProperties propertyId, ref CACommand instanceData, Int32 instanceDataLength, ref CACommand value, Int32 valueLength );

        /// <summary>
        /// Die Methode zum Setzen eines Parameters.
        /// </summary>
        private SetSignature m_Set;

        /// <summary>
        /// Signatur einer Methode zur Prüfung, ob ein bestimmter Parameter angeboten wird.
        /// </summary>
        /// <param name="comObject">Die Rohschnittstelle zum Aufruf.</param>
        /// <param name="propertySetId">Die betroffene Parametergruppe.</param>
        /// <param name="propertyId">Der gewünschte Parameter.</param>
        /// <param name="mode">Die Art der Unterstützung des Parameters.</param>
        /// <returns>Der COM Fehlercode</returns>
        private delegate Int32 QuerySupportedSignature( IntPtr comObject, ref Guid propertySetId, BDAProperties propertyId, out PropertySetSupportedTypes mode );

        /// <summary>
        /// Die Methode zur Prüfung, ob ein bestimmter Parameter angeboten wird.
        /// </summary>
        private QuerySupportedSignature m_QuerySupported;

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="set">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        public KsPropertySetFireDTV( IntPtr set )
            : base( set )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="set">Die COM Referenz auf das <i>Direct Show</i> Objekt. Der
        /// Aufrufer hat für die Freigabe zu sorgen.</param>
        public KsPropertySetFireDTV( object set )
            : base( set )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="set">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        /// <param name="autoRelease">Gesetzt, wenn die Referenz nach der Übergabe freigegeben werden soll.</param>
        public KsPropertySetFireDTV( object set, bool autoRelease )
            : base( set, autoRelease )
        {
        }

        /// <summary>
        /// Erzeugt alle notwendigen Methodensignaturen.
        /// </summary>
        protected override void OnCreateDelegates()
        {
            // Create signature
            m_QuerySupported = CreateDelegate<QuerySupportedSignature>( 2 );
            m_Set = CreateDelegate<SetSignature>( 0 );
        }

        /// <summary>
        /// Prüft, ob ein bestimmter Parameter angeboten wird.
        /// </summary>
        /// <param name="propertySetId">Die betroffene Parametergruppe.</param>
        /// <param name="propertyId">Der gewünschte Parameter.</param>
        /// <returns>Die Art der Unterstützung des Parameters.</returns>
        public PropertySetSupportedTypes QuerySupported( ref Guid propertySetId, BDAProperties propertyId )
        {
            // Result
            PropertySetSupportedTypes mode;

            // Process
            Int32 hResult = m_QuerySupported( ComInterface, ref propertySetId, propertyId, out mode );
            if (hResult < 0)
                throw new COMException( "KsPropertySet.PropertySetSupportedTypes", hResult );

            // Report
            return mode;
        }

        /// <summary>
        /// Setzt einen Parameter.
        /// </summary>
        /// <param name="propertySetId">Die eindeutige Kennung der Parametergruppe.</param>
        /// <param name="propertyId">Die laufende Nummer des Parameters.</param>
        /// <param name="instanceData">Die Daten der Instanz.</param>
        /// <param name="instanceDataLength">Die Größe der Daten der Instanz.</param>
        /// <param name="value">Der neue Wert.</param>
        /// <param name="valueLength">Die Größe des neuen Wertes.</param>
        public void Set( ref Guid propertySetId, BDAProperties propertyId, ref CACommand instanceData, Int32 instanceDataLength, ref CACommand value, Int32 valueLength )
        {
            // Process
            Int32 hResult = m_Set( ComInterface, ref propertySetId, propertyId, ref instanceData, instanceDataLength, ref value, valueLength );
            if (hResult < 0)
                throw new COMException( "KsPropertySet.Set", hResult );
        }

        /// <summary>
        /// Führt einen CI Befehl aus.
        /// </summary>
        /// <param name="command">Der gewünschte Befehl.</param>
        public void Send( CACommand command )
        {
            // Load property set identifier
            var propSetId = CIControl.PropertySetId;

            // Create a clone
            var clone = command.Clone();

            // Get the raw size
            int commandSize = Marshal.SizeOf( command ), cloneSize = Marshal.SizeOf( clone );

            // Send the command
            Set( ref propSetId, BDAProperties.SendCA, ref command, commandSize, ref clone, cloneSize );
        }
    }
}
