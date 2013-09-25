using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt die Feinsteuerung des Demodulators.
    /// </summary>
    [
        ComImport,
        Guid( "ef30f379-985b-4d10-b640-a79d5e04e1e0" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBDADigitalDemodulator
    {
        /// <summary>
        /// Legt die Modulationsart fest.
        /// </summary>
        /// <param name="type">Die gew�nschte Art der Modulation.</param>
        void SetModulation( ref ModulationType type );

        /// <summary>
        /// Meldet die aktuelle Modulationsart.
        /// </summary>
        ModulationType Modulation { get; }

        /// <summary>
        /// Legt die innere Fehlerkorrekturmethode fest.
        /// </summary>
        /// <param name="type">Die gew�nschte Korrekturmethethode.</param>
        void SetInnerFECMethod( ref FECMethod type );

        /// <summary>
        /// Meldet die innere Fehlerkorrekturmethode.
        /// </summary>
        FECMethod InnerFECMethod { get; }

        /// <summary>
        /// Legt die innere Korrekturrate fest.
        /// </summary>
        /// <param name="type">Die gew�nschte Rate.</param>
        void SetInnerFECRate( ref BinaryConvolutionCodeRate type );

        /// <summary>
        /// Meldet die innere Korrekturrate.
        /// </summary>
        BinaryConvolutionCodeRate InnerFECRate { get; }

        /// <summary>
        /// Legt die �u�ere Fehlerkorrekturmethode fest.
        /// </summary>
        /// <param name="type">Die gew�nschte Korrekturmethethode.</param>
        void SetOuterFECMethod( ref FECMethod type );

        /// <summary>
        /// Meldet die �u�ere Fehlerkorrekturmethode.
        /// </summary>
        FECMethod OuterFECMethod { get; }

        /// <summary>
        /// Legt die �u�ere Korrekturrate fest.
        /// </summary>
        /// <param name="type">Die gew�nschte Rate.</param>
        void SetOuterFECRate( ref BinaryConvolutionCodeRate type );

        /// <summary>
        /// Meldet die �u�ere Korrekturrate.
        /// </summary>
        BinaryConvolutionCodeRate OuterFECRate { get; }

        /// <summary>
        /// Legt die Symbolrate fest.
        /// </summary>
        /// <param name="rate">Die gew�nschte Rate.</param>
        void SetSymbolRate( ref UInt32 rate );

        /// <summary>
        /// Meldet die Symbolrate.
        /// </summary>
        UInt32 SymbolRate { get; }

        /// <summary>
        /// Legt die spektrale Inversion fest.
        /// </summary>
        /// <param name="inversion">Die gew�nschte Inversionsmethode.</param>
        void SetSpectralInversion( ref SpectralInversion inversion );

        /// <summary>
        /// Meldet die aktuelle Inversionsmethode.
        /// </summary>
        SpectralInversion SpectralInversion { get; }
    }
}
