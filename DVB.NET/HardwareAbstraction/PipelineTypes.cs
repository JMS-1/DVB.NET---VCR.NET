using System;


namespace JMS.DVB
{
    /// <summary>
    /// Die Arten von Erweiterungselementen für die Bearbeitungskette.
    /// </summary>
    [Flags]
    public enum PipelineTypes
    {
        /// <summary>
        /// Stellt Informationen zur Signalstärke und -qualität zur Verfügung.
        /// </summary>
        SignalInformation = 0x00000001,

        /// <summary>
        /// Kann zwischen verschiedenen Antennen umschalten.
        /// </summary>
        DiSEqC = 0x00000002,

        /// <summary>
        /// Erlaubt die Ansteuerung von DVB-S2 Quellgruppen (Transpondern).
        /// </summary>
        DVBS2 = 0x00000004,

        /// <summary>
        /// Ermöglicht die Entschlüsselung unter Verwendung einer CI/CAM Hardware.
        /// </summary>
        CICAM = 0x00000008,
    }
}
