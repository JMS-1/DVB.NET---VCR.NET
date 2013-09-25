using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt die Empfangsparameter einer Quellgruppe.
    /// </summary>
    [
		ComImport, 
		Guid("286d7f89-760c-4f89-80c4-66841d2507aa"), 
		InterfaceType(ComInterfaceType.InterfaceIsIDispatch)
	]
    public interface ILocator
	{
        /// <summary>
        /// Die Tr‰gerfrequenz.
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
        /// Die Art der ‰uﬂeren Fehlerkorrektur.
        /// </summary>
		FECMethod OuterFEC { get; set; }

        /// <summary>
        /// Die ‰uﬂere Fehlerkorrekturrate.
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
		[return: MarshalAs(UnmanagedType.Interface)] ILocator Clone();
	}
}
