using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt eine Quellgruppe für den Empfang über Antenne.
    /// </summary>
    [
		ComImport,
		Guid("8664da16-dda2-42ac-926a-c18f9127c302"),
		InterfaceType(ComInterfaceType.InterfaceIsIDispatch)
	]
    public interface IDVBTLocator // : ILocator
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
        /// Die Bandbreite.
        /// </summary>
        Int32 Bandwidth { get; set; }

        /// <summary>
        /// Die Methode der inneren Fehlerkorrektor (LP).
        /// </summary>
		FECMethod LPInnerFEC { get; set; }

        /// <summary>
        /// Die innere Fehlerkorrekturrate (LP).
        /// </summary>
		BinaryConvolutionCodeRate LPInnerFECRate { get; set; }

        /// <summary>
        /// Informationen zur Hierarchie.
        /// </summary>
		HierarchyAlpha HAlpha { get; set; }

        /// <summary>
        /// Der Wächterbereich.
        /// </summary>
		GuardInterval Guard { get; set; }

        /// <summary>
        /// Die Übertragungsart.
        /// </summary>
		TransmissionMode Mode { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob andere Frequenzen aktiv sind.
        /// </summary>
		bool OtherFrequencyInUse { get; set; }
	} 
}
