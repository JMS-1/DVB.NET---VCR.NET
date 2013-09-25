using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt die Parameter f�r eine Quellgruppe beim Kabelempfang.
    /// </summary>
    [
        ComImport,
        Guid( "6e42f36e-1dd2-43c4-9f78-69d25ae39034" ),
        InterfaceType( ComInterfaceType.InterfaceIsIDispatch )
    ]
    public interface IDVBCLocator // : ILocator
    {
        #region ILocator

        /// <summary>
        /// Die Tr�gerfrequenz.
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
        /// Die Art der �u�eren Fehlerkorrektur.
        /// </summary>
        FECMethod OuterFEC { get; set; }

        /// <summary>
        /// Die �u�ere Fehlerkorrekturrate.
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
    }
}
