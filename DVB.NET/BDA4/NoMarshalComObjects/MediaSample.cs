using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess.NoMarshalComObjects
{
    /// <summary>
    /// Vermittelt den Zugriff auf ein einzelnen <i>Direct Show</i> Speichrblock
    /// ohne Rücksicht auf COM Apartment Regeln.
    /// </summary>
    public class MediaSample : NoMarshalBase<MediaSample.Interface>
    {
        /// <summary>
        /// Die zugrunde liegende COM Schnittstelle.
        /// </summary>
        [
            ComImport,
            Guid( "56a8689a-0ad4-11ce-b03a-0020af0ba770" ),
            InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
        ]
        public interface Interface
        {
            /// <summary>
            /// Ermittelt die Adresse des Speicherbereichs.
            /// </summary>
            /// <param name="buffer">Adresse des ersten Speicherbytes..</param>
            void GetPointer( out IntPtr buffer );

            /// <summary>
            /// Meldet die Größe des Speicherbereichs.
            /// </summary>
            /// <returns>Die Größe des Speicherbereichs.</returns>
            [PreserveSig]
            Int32 GetSize();

            /// <summary>
            /// Ermittelt den abgedeckten Zeitbereich.
            /// </summary>
            /// <param name="timeStart">Die Anfangszeit.</param>
            /// <param name="timeEnd">Die Endzeit.</param>
            void GetTime( out long timeStart, out long timeEnd );

            /// <summary>
            /// Setzt den abgdeckten Zeitbereich.
            /// </summary>
            /// <param name="timeStart">Die Anfangszeit.</param>
            /// <param name="timeEnd">Die Endzeit.</param>
            void SetTime( ref long timeStart, ref long timeEnd );

            /// <summary>
            /// Meldet, ob es sich um einen Synchronisationspunkt im Datenstrom handelt.
            /// </summary>
            /// <returns>Ergebnis der Prüfung, negativ im Fehlerfall.</returns>
            [PreserveSig]
            Int32 IsSyncPoint();

            /// <summary>
            /// Ändert die Zuordnung eines Synchronisationspunktes.
            /// </summary>
            /// <param name="isSyncPoint">Gesetzt, wenn ein Synchronisationspunkt angelegt werden soll.</param>
            void SetSyncPoint( bool isSyncPoint );

            /// <summary>
            /// Meldet, ob eine Vorabinformation vorliegt.
            /// </summary>
            /// <returns>Ergebnis der Prüfung, negativ im Fehlerfall.</returns>
            [PreserveSig]
            Int32 IsPreroll();

            /// <summary>
            /// Ändert die Vorabinformation.
            /// </summary>
            /// <returns>Gesetzt, wenn es sich um eine Vorabinformation handelt.</returns>
            void SetPreroll( bool isPreroll );

            /// <summary>
            /// Meldet die aktuelle Datengröße.
            /// </summary>
            /// <returns>Die tatsächliche Größe des Speicherbereiches.</returns>
            [PreserveSig]
            Int32 GetActualDataLength();

            /// <summary>
            /// Ändert die Datengröße.
            /// </summary>
            /// <param name="length">Die Anzahl der relevanten Bytes im Zwischenspeicher.</param>
            void SetActualDataLength( Int32 length );

            /// <summary>
            /// Meldet die Art der Daten.
            /// </summary>
            /// <param name="mediaType">Das aktuelle Datenformat.</param>
            void GetMediaType( out IntPtr mediaType );

            /// <summary>
            /// Legt die Art der Daten fest.
            /// </summary>
            /// <returns>Ergebnis der Änderung, negative Werte deuten auf einen Fehler hin.</returns>
            [PreserveSig]
            Int32 SetMediaType( IntPtr mediaType );

            /// <summary>
            /// Meldet, ob Lücken im Datenstrom aufgetreten sind.
            /// </summary>
            /// <returns>Ergebnis der Prüfung, negativ im Fehlerfall.</returns>
            [PreserveSig]
            Int32 IsDiscontinuity();

            /// <summary>
            /// Setzt die Information zu Lücken im Datenstrom.
            /// </summary>
            /// <param name="isDiscontinuity">Gesetzt, wenn eine Lücke erkannt wurde.</param>
            void SetDiscontinuity( bool isDiscontinuity );

            /// <summary>
            /// Ermittelt die Laufzeit.
            /// </summary>
            /// <param name="timeStart">Startzeitpunkt.</param>
            /// <param name="timeEnd">Endzeitpunkt.</param>
            void GetMediaTime( out long timeStart, out long timeEnd );

            /// <summary>
            /// Legt die Laufzeit fest.
            /// </summary>
            /// <param name="timeStart">Startzeitpunkt.</param>
            /// <param name="timeEnd">Endzeitpunkt.</param>
            void SetMediaTime( ref long timeStart, ref long timeEnd );
        }

        /// <summary>
        /// Signatur der Methode zum Ermitteln der Basisadresse des Speicherbereichs.
        /// </summary>
        /// <param name="comObject">Das Objekt zum Speicherbereich.</param>
        /// <param name="buffer">Die gewünschte Adresse.</param>
        /// <returns>Der COM Fehler zum Aufruf der Methode.</returns>
        private delegate Int32 GetPointerSignature( IntPtr comObject, out IntPtr buffer );

        /// <summary>
        /// Signatur einer Methode zur Festlegung der Zeitstempel für einen Speicherbereich.
        /// </summary>
        /// <param name="comObject">Das Objekt zum Speicherbereich.</param>
        /// <param name="timeStart">Der Anfangszeitpunkt.</param>
        /// <param name="timeEnd">Der Endzeitpunkt.</param>
        /// <returns>Der COM Fehler zum Aufruf der Methode.</returns>
        private delegate Int32 SetTimeSignature( IntPtr comObject, ref long timeStart, ref long timeEnd );

        /// <summary>
        /// Signatur einer Methode zum Auslesen der Zeitstempel für einen Speicherbereich.
        /// </summary>
        /// <param name="comObject">Das Objekt zum Speicherbereich.</param>
        /// <param name="timeStart">Der Anfangszeitpunkt.</param>
        /// <param name="timeEnd">Der Endzeitpunkt.</param>
        /// <returns>Der COM Fehler zum Aufruf der Methode.</returns>
        private delegate Int32 GetTimeSignature( IntPtr comObject, out long timeStart, out long timeEnd );

        /// <summary>
        /// Signatur einer Methode zur Festlegung, ob die Daten in diesem Speicherbereich einen
        /// Synchronisationspunkt enthalten.
        /// </summary>
        /// <param name="comObject">Das Objekt zum Speicherbereich.</param>
        /// <param name="isSyncPoint">Gesetzt, wenn ein Synchronisationspunkt vorliegt.</param>
        /// <returns>Der COM Fehler zum Aufruf der Methode.</returns>
        private delegate Int32 SetSyncPointSignature( IntPtr comObject, bool isSyncPoint );

        /// <summary>
        /// Signatur einer Methode zur Ermittelung der tatsächlichen Größe des Speicherbereichs.
        /// </summary>
        /// <param name="comObject">Das Objekt zum Speicherbereich.</param>
        /// <returns>Die tatsächliche Größe der Nutzdaten.</returns>
        private delegate Int32 GetActualDataLengthSignature( IntPtr comObject );

        /// <summary>
        /// Signatur einer Methode zur Festlegung der Größe der Nutzdaten im Speicherbereich.
        /// </summary>
        /// <param name="comObject">Das Objekt zum Speicherbereich.</param>
        /// <param name="length">Die Größe der Nutzdaten im Speicherbereich.</param>
        /// <returns>Der COM Fehler zum Aufruf der Methode.</returns>
        private delegate Int32 SetActualDataLengthSignature( IntPtr comObject, Int32 length );

        /// <summary>
        /// Signatur einer Methode zur Festlegung des Datentyps zum Speicherbereich.
        /// </summary>
        /// <param name="comObject">Das Objekt zum Speicherbereich.</param>
        /// <param name="mediaType">Der neue Datentyp.</param>
        /// <returns>Der COM Fehler zum Aufruf der Methode.</returns>
        private delegate Int32 SetMediaTypeSignature( IntPtr comObject, IntPtr mediaType );

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="sample">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        public MediaSample( IntPtr sample )
            : base( sample )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="sample">Die COM Referenz auf das <i>Direct Show</i> Objekt. Der
        /// Aufrufer hat für die Freigabe zu sorgen.</param>
        public MediaSample( object sample )
            : base( sample )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="sample">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        /// <param name="autoRelease">Gesetzt, wenn die Referenz nach der Übergabe freigegeben werden soll.</param>
        public MediaSample( object sample, bool autoRelease )
            : base( sample, autoRelease )
        {
        }

        /// <summary>
        /// Erzeugt alle notwendigen Methodensignaturen.
        /// </summary>
        protected override void OnCreateDelegates()
        {
        }

        /// <summary>
        /// Liest oder meldet die Größe der Nutzdaten.
        /// </summary>
        /// <exception cref="COMException">Leitet den unterliegenden COM Fehler durch.</exception>
        public int ActualDataLength
        {
            get
            {
                // Load
                int len = CreateDelegate<GetActualDataLengthSignature>( 8 )( ComInterface );
                if (len < 0)
                    throw new COMException( "MediaSample.ActualDataLength", len );

                // Report
                return len;
            }
            set
            {
                // Process
                Int32 hResult = CreateDelegate<SetActualDataLengthSignature>( 9 )( ComInterface, value );
                if (hResult < 0)
                    throw new COMException( "MediaSample.ActualDataLength", hResult );
            }
        }

        /// <summary>
        /// Legt fest, ob der Speicherbereich einen Synchronisationspunkt enthält.
        /// </summary>
        /// <exception cref="COMException">Leitet den unterliegenden COM Fehler durch.</exception>
        public bool IsSyncPoint
        {
            set
            {
                // Process
                Int32 hResult = CreateDelegate<SetSyncPointSignature>( 5 )( ComInterface, value );
                if (hResult < 0)
                    throw new COMException( "MediaSample.IsSyncPoint", hResult );
            }
        }

        /// <summary>
        /// Legt den Zeitbereich für diesen Speicherbereich fest.
        /// </summary>
        /// <param name="start">Die Anfangszeit.</param>
        /// <param name="end">Die Endzeit.</param>
        /// <exception cref="COMException">Leitet den unterliegenden COM Fehler durch.</exception>
        public void SetTime( long start, long end )
        {
            // Process
            var hResult = CreateDelegate<SetTimeSignature>( 3 )( ComInterface, ref start, ref end );
            if (hResult < 0)
                throw new COMException( "MediaSample.SetTime", hResult );
        }

        /// <summary>
        /// Liest den Zeitbereich für diesen Speicherbereich aus.
        /// </summary>
        /// <param name="start">Die Anfangszeit.</param>
        /// <param name="end">Die Endzeit.</param>
        /// <exception cref="COMException">Leitet den unterliegenden COM Fehler durch.</exception>
        public void GetTime( out long start, out long end )
        {
            // Process
            var hResult = CreateDelegate<GetTimeSignature>( 2 )( ComInterface, out start, out end );
            if (hResult < 0)
                throw new COMException( "MediaSample.GetTime", hResult );
        }

        /// <summary>
        /// Legt den Datentyp für diesen Speicherberich fest.
        /// </summary>
        /// <exception cref="COMException">Leitet den unterliegenden COM Fehler durch.</exception>
        public MediaType MediaType
        {
            set
            {
                // Get the data
                IntPtr mediaType = (value == null) ? IntPtr.Zero : value.GetReference();

                // Process
                var hResult = CreateDelegate<SetMediaTypeSignature>( 11 )( ComInterface, mediaType );
                if (hResult < 0)
                    throw new COMException( "MediaSample.SetMediaType", hResult );
            }
        }

        /// <summary>
        /// Meldet die Basisadresse des Speicherbereichs.
        /// </summary>
        /// <exception cref="COMException">Leitet den unterliegenden COM Fehler durch.</exception>
        public IntPtr BaseAddress
        {
            get
            {
                // Result
                IntPtr addr;

                // Load
                var hResult = CreateDelegate<GetPointerSignature>( 0 )( ComInterface, out addr );
                if (hResult < 0)
                    throw new COMException( "MediaSample.BaseAddress", hResult );

                // Report
                return addr;
            }
        }
    }
}
