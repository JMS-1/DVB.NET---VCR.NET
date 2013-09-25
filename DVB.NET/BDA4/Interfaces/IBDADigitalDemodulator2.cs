using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Steuert die Konfiguration der Quellgruppe, die auszulesen ist.
    /// </summary>
    [
        ComImport,
        Guid( "525ed3ee-5cf3-4e1e-9a06-5368a84f9a6e" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBDADigitalDemodulator2
    {
        #region IBDADigitalDemodulator

        /// <summary>
        /// Legt die Modulationsart fest.
        /// </summary>
        /// <param name="type">Die gewünschte Art der Modulation.</param>
        void SetModulation( ref ModulationType type );

        /// <summary>
        /// Meldet die aktuelle Modulationsart.
        /// </summary>
        ModulationType Modulation { get; }

        /// <summary>
        /// Legt die innere Fehlerkorrekturmethode fest.
        /// </summary>
        /// <param name="type">Die gewünschte Korrekturmethethode.</param>
        void SetInnerFECMethod( ref FECMethod type );

        /// <summary>
        /// Meldet die innere Fehlerkorrekturmethode.
        /// </summary>
        FECMethod InnerFECMethod { get; }

        /// <summary>
        /// Legt die innere Korrekturrate fest.
        /// </summary>
        /// <param name="type">Die gewünschte Rate.</param>
        void SetInnerFECRate( ref BinaryConvolutionCodeRate type );

        /// <summary>
        /// Meldet die innere Korrekturrate.
        /// </summary>
        BinaryConvolutionCodeRate InnerFECRate { get; }

        /// <summary>
        /// Legt die äußere Fehlerkorrekturmethode fest.
        /// </summary>
        /// <param name="type">Die gewünschte Korrekturmethethode.</param>
        void SetOuterFECMethod( ref FECMethod type );

        /// <summary>
        /// Meldet die äußere Fehlerkorrekturmethode.
        /// </summary>
        FECMethod OuterFECMethod { get; }

        /// <summary>
        /// Legt die äußere Korrekturrate fest.
        /// </summary>
        /// <param name="type">Die gewünschte Rate.</param>
        void SetOuterFECRate( ref BinaryConvolutionCodeRate type );

        /// <summary>
        /// Meldet die äußere Korrekturrate.
        /// </summary>
        BinaryConvolutionCodeRate OuterFECRate { get; }

        /// <summary>
        /// Legt die Symbolrate fest.
        /// </summary>
        /// <param name="rate">Die gewünschte Rate.</param>
        void SetSymbolRate( ref UInt32 rate );

        /// <summary>
        /// Meldet die Symbolrate.
        /// </summary>
        UInt32 SymbolRate { get; }

        /// <summary>
        /// Legt die spektrale Inversion fest.
        /// </summary>
        /// <param name="inversion">Die gewünschte Inversionsmethode.</param>
        void SetSpectralInversion( ref SpectralInversion inversion );

        /// <summary>
        /// Meldet die aktuelle Inversionsmethode.
        /// </summary>
        SpectralInversion SpectralInversion { get; }

        #endregion

        /// <summary>
        /// Setzt den Überwachungsbereich.
        /// </summary>
        /// <param name="guardInterval">Der neue Bereich.</param>
        void SetGuardInterval( ref DVBS2GuardInterval guardInterval );

        /// <summary>
        /// Meldet den Überwachungsbereich.
        /// </summary>
        Int32 GuardInterval { get; }

        /// <summary>
        /// Setzt den Übertragungsmodus.
        /// </summary>
        /// <param name="transmissionMode">Der neue Übertragungsmodus.</param>
        void SetTransmissionMode( ref DVBS2TransmissionMode transmissionMode );

        /// <summary>
        /// Meldet den aktuellen Übertragungsmodus.
        /// </summary>
        Int32 TransmissionMode { get; }

        /// <summary>
        /// legt den Roll-Off der DVB-S2 Übertragung fest.
        /// </summary>
        /// <param name="rollOff">Der neue Roll-Off.</param>
        void SetRollOff( ref RollOff rollOff );

        /// <summary>
        /// Meldet den Roll-Off der DVB-S2 Übertragung.
        /// </summary>
        Int32 RollOff { get; }

        /// <summary>
        /// Legt die Nutzung des Pilottons fest.
        /// </summary>
        /// <param name="pilot">Die neue Nutzung.</param>
        void SetPilot( ref PilotMode pilot );

        /// <summary>
        /// Meldet die Nutzung des Pilottons.
        /// </summary>
        Int32 Pilot { get; }
    }
}
