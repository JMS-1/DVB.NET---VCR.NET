using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Erlaubt die Auswahl von Quellgruppen und meldet weitere Informationen zum Empfang.
    /// </summary>
    [
        ComImport,
        Guid( "28C52640-018A-11D3-9D8E-00C04F72D980" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface ITuner
    {
        /// <summary>
        /// Meldet den aktiven Namensraum.
        /// </summary>
        ITuningSpace TuningSpace { get; set; }

        /// <summary>
        /// Liefert eine Auflistung �ber alle Namensr�ume.
        /// </summary>
        /// <returns>Die angeforderte Auflistung.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumTuningSpaces EnumTuningSpaces();

        /// <summary>
        /// Der aktuelle �nderungswunsch f�r Quellgruppen.
        /// </summary>
        ITuneRequest TuneRequest { get; set; }

        /// <summary>
        /// Pr�ft einen �nderungswunsch.
        /// </summary>
        /// <param name="TuneRequest">Informationen f�r eine neue Quellgruppe.</param>
        void Validate( ITuneRequest TuneRequest );

        /// <summary>
        /// Meldet die bevorzugten Arten von Komponenten.
        /// </summary>
        IComponentTypes PreferredComponentTypes { get; set; }

        /// <summary>
        /// Meldet die St�rke des Signals.
        /// </summary>
        int SignalStrength { get; }

        /// <summary>
        /// Aktiviert Ereignisse zur Signal�berwachung.
        /// </summary>
        /// <param name="Interval">Pause zwischen dem Ausl�sen von Ereignissen.</param>
        void TriggerSignalEvents( int Interval );
    }

}
