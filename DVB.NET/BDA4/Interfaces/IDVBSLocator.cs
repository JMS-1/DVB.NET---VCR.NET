using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt die Parameter beim Empfang über Satellit.
    /// </summary>
    [
		ComImport, 
		Guid("3d7c353c-0d04-45f1-a742-f97cc1188dc8"),
		InterfaceType(ComInterfaceType.InterfaceIsIDispatch)
	]
    public interface IDVBSLocator // : ILocator
    {
        #region ILocator

        /// <summary>
        /// Die Trägerfrequenz.
        /// </summary>
        int CarrierFrequency { get; set; }

        /// <summary>
        /// Die Art der inneren Fehlerkorrektur.
        /// </summary>
        FECMethod InnerFEC { get; set; }

        /// <summary>
        /// Die innere Fehlerkorrekturrate.
        /// </summary>
        BinaryConvolutionCodeRate InnerFECRate { get; set; }

        /// <summary>
        /// Die Art der äußeren Fehlerkorrektur.
        /// </summary>
        FECMethod OuterFEC { get; set; }

        /// <summary>
        /// Die äußere Fehlerkorrekturrate.
        /// </summary>
        BinaryConvolutionCodeRate OuterFECRate { get; set; }

        /// <summary>
        /// Die verwendete Modulation.
        /// </summary>
        ModulationType Modulation { get; set; }

        /// <summary>
        /// Die Symbolrate.
        /// </summary>
        int SymbolRate { get; set; }

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Beschreibung.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        ILocator Clone();

        #endregion

        /// <summary>
        /// Die Polarisation des Signals.
        /// </summary>
        Polarisation SignalPolarisation { get; set; }

        /// <summary>
        /// Gesetzt, wenn der Satellit in einer westlichen Position ist.
        /// </summary>
		bool WestPosition { get; set; }

        /// <summary>
        /// Die orbitale Position des Satelliten.
        /// </summary>
		int OrbitalPosition { get; set; }

        /// <summary>
        /// Der Azimuth des Satelliten.
        /// </summary>
		int Azimuth { get; set; }

        /// <summary>
        /// Die Höhe des Satelliten.
        /// </summary>
		int Elevation { get; set; }
	}
 
}
