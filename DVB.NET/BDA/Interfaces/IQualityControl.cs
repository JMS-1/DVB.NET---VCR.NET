using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Die Art der Qualit�tsinformation.
    /// </summary>
    internal enum QualityMessageType
    {
        /// <summary>
        /// (don't know).
        /// </summary>
        Famine = 0,

        /// <summary>
        /// (don't know).
        /// </summary>
        Flood = Famine + 1,
    };

    /// <summary>
    /// Information zur Qualit�t.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    internal struct Quality
    {
        /// <summary>
        /// Die Art der Informationen.
        /// </summary>
        QualityMessageType Type;

        /// <summary>
        /// Daten zu den Proportionen.
        /// </summary>
        Int32 Proportion;

        /// <summary>
        /// Anzahl der Verz�gerungen.
        /// </summary>
        long Late;

        /// <summary>
        /// Aktueller Zeitstempel.
        /// </summary>
        long TimeStamp;
    };

    /// <summary>
    /// Schnittstelle zur Abfrage von Qualit�tsinformationen.
    /// </summary>
    [
        ComImport,
        Guid( "56a868a5-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IQualityControl
    {
        /// <summary>
        /// Meldet Qualit�tsinformationen zu einem Filter.
        /// </summary>
        /// <param name="self">Der betroffene Filter.</param>
        /// <param name="quality">Die zugeh�rigen Informationen.</param>
        void Notify( IBaseFilter self, Quality quality );

        /// <summary>
        /// Meldet einen Verbraucher von Qualit�tinformationen an.
        /// </summary>
        /// <param name="sink">Die COM Schnittstelle zum Verbraucher.</param>
        void SetSink( IQualityControl sink );
    }
}
