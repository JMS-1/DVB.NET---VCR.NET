using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Wird zur Ansteuerung der Antenne verwendet.
    /// </summary>
    [
        ComImport,
        Guid( "f84e2ab0-3c6b-45e3-a0fc-8669d4b81f11" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBDADiseqCommand
    {
        /// <summary>
        /// Legt fest, ob Befehle gesendet werden können.
        /// </summary>
        bool Enable { set; }

        /// <summary>
        /// Definiert die zu verwendende Antenne.
        /// </summary>
        UInt32 LNBSource { set; }

        /// <summary>
        /// Legt fest, ob das <i>Tone Burst</i> Verfahren zur Umschaltung zwischen zwei Antennen verwendet werden soll.
        /// </summary>
        bool UseToneBurst { set; }

        /// <summary>
        /// Spezifiziert die Anzahl der Wiederholungen eines Befehls.
        /// </summary>
        UInt32 Repeats { set; }

        /// <summary>
        /// Sendet einen Befehl.
        /// </summary>
        /// <param name="requestIdentifier">Die eindeutige Identifikation des Befehls.</param>
        /// <param name="commandLength">Die Anzahl der Bytes im Befehl.</param>
        /// <param name="command">Der gewünschte Befehl.</param>
        void Send( UInt32 requestIdentifier, UInt32 commandLength, byte[] command );

        /// <summary>
        /// Ermittelt die Antwort zu einem Befehl.
        /// </summary>
        /// <param name="requestIdentifier">Die eindeutige Identifikation des Befehls.</param>
        /// <param name="commandLength">Die Anzahl der Bytes im Zwischenspeicher für die Antwort.</param>
        /// <param name="response">Ein Zwischenspeicher für die Antwort.</param>
        void Receive( UInt32 requestIdentifier, ref UInt32 commandLength, byte[] response );
    }
}
