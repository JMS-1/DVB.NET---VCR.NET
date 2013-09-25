using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.NoMarshalComObjects;


namespace JMS.DVB.DirectShow.NoMarshalComObjects
{
    /// <summary>
    /// Vermittelt den Zugriff auf einen <i>Direct Show</i> Referenzzähler
    /// ohne Rücksicht auf COM Apartment Regeln.
    /// </summary>
    internal class ReferenceClock : NoMarshalBase<ReferenceClock.Interface>
    {
        /// <summary>
        /// Die zugehörige COM Schnittstelle.
        /// </summary>
        [
            ComImport,
            InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
            Guid( "56a86897-0ad4-11ce-b03a-0020af0ba770" )
        ]
        public interface Interface
        {
            /// <summary>
            /// Ermittelt den aktuellen Zählerstand.
            /// </summary>
            /// <returns>Der aktuelle Zählerstand.</returns>
            [return: MarshalAs( UnmanagedType.I8 )]
            long GetTime();

            /// <summary>
            /// Meldet eine Überwachung des Zählerstands an.
            /// </summary>
            /// <param name="baseTime"></param>
            /// <param name="streamTime"></param>
            /// <param name="eventHandle"></param>
            /// <returns>Registrierungsinformation für die Überwachung.</returns>
            [return: MarshalAs( UnmanagedType.U4 )]
            uint AdviseTime( long baseTime, long streamTime, uint eventHandle );

            /// <summary>
            /// Meldet eine periodische Überwachung des Zählerstands an.
            /// </summary>
            /// <param name="startTime"></param>
            /// <param name="periodTime"></param>
            /// <param name="semaphoreHandle"></param>
            /// <returns>Registrierungsinformation für die Überwachung.</returns>
            [return: MarshalAs( UnmanagedType.U4 )]
            uint AdvisePeriodic( long startTime, long periodTime, uint semaphoreHandle );

            /// <summary>
            /// Meldet eine Überwachung des Zählerstands ab.
            /// </summary>
            /// <param name="adviseCookie">Die Registrierungsinformationen von der Anmeldung.</param>
            void Unadvise( uint adviseCookie );
        }

        /// <summary>
        /// Signatur der COM Methode zum Auslesen des aktuellen Zeitstempels.
        /// </summary>
        /// <param name="time">Der aktuelle Zeitstempel.</param>
        /// <param name="comObject">Der COM Fehlercode.</param>
        private delegate Int32 GetTimeSignature( IntPtr comObject, out long time );

        /// <summary>
        /// Methode zum Abruf des aktuellen Zeitstempels.
        /// </summary>
        private GetTimeSignature m_GetTime;

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="clock">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        public ReferenceClock( IntPtr clock )
            : base( clock )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="clock">Die COM Referenz auf das <i>Direct Show</i> Objekt. Der
        /// Aufrufer hat für die Freigabe zu sorgen.</param>
        public ReferenceClock( object clock )
            : base( clock )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz. Die COM Lebenszeitkontrolle wird
        /// von dieser Instanz übernommen.
        /// </summary>
        /// <param name="clock">Die COM Referenz auf das <i>Direct Show</i> Objekt.</param>
        /// <param name="autoRelease">Gesetzt, wenn die Referenz nach der Übergabe freigegeben werden soll.</param>
        public ReferenceClock( object clock, bool autoRelease )
            : base( clock, autoRelease )
        {
        }

        /// <summary>
        /// Erzeugt alle notwendigen Methodensignaturen.
        /// </summary>
        protected override void OnCreateDelegates()
        {
            // Create signature
            m_GetTime = CreateDelegate<GetTimeSignature>( 0 );
        }

        /// <summary>
        /// Meldet den aktuellen Zeitstempel.
        /// </summary>
        /// <exception cref="COMException">Meldet einen COM Aufruffehler.</exception>
        public long Time
        {
            get
            {
                // Request the current time
                long time;
                var hResult = m_GetTime( ComInterface, out time );
                if (hResult < 0) 
                    throw new COMException( "ReferenceClock.Time", hResult );

                // Report
                return time;
            }
        }
    }
}
