using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Steuert den Mischvorgang der Bilddarstellung.
    /// </summary>
    [
        ComImport,
        Guid( "1a777eaa-47c8-4930-b2c9-8fee1c1b0f3b" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IVMRMixerControl
    {
        /// <summary>
        /// Setzt die Transparenz.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <param name="alpha">Gewünschte Transparenz.</param>
        void SetAlpha( UInt32 streamID, float alpha );

        /// <summary>
        /// Meldet die Transparenz eines Datenstroms.
        /// </summary>
        /// <param name="streamID">Laufende Nummer des Datenstroms.</param>
        /// <returns>Aktuelle Transparenz.</returns>
        float GetAlpha( UInt32 streamID );

        /// <summary>
        /// Setzt die Darstellungsebene.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <param name="order">Die gewünschte Ebene.</param>
        void SetZOrder( UInt32 streamID, UInt32 order );

        /// <summary>
        /// Meldet die Darstellungsebene eines Datenstroms.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <returns>Die aktuell verwendete Ebene.</returns>
        UInt32 GetZOrder( UInt32 streamID );

        /// <summary>
        /// Setzt den Darstellungsbereich.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <param name="rect">Der gewünschte Darstellungsbereich.</param>
        void SetOutputRect( UInt32 streamID, VMRNormalizedRectangle rect );
        
        /// <summary>
        /// Meldet den Darstellungsbereich eines Datenstroms.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <returns>Der aktuell zugeordnete Darstellungsbereich.</returns>
        VMRNormalizedRectangle GetOutputRect( UInt32 streamID );

        /// <summary>
        /// Legt die Hintergrundfarbe fest.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <param name="color">Die zu verwendene Hintergrundfarbe.</param>
        void SetBackgroundClr( UInt32 streamID, UInt32 color );

        /// <summary>
        /// Meldet die Hintergrundfarbe eines Datenstroms.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <returns>Die aktuell verwendete Hintergrundfarbe.</returns>
        UInt32 GetBackgroundClr( UInt32 streamID );

        /// <summary>
        /// Legt Voreinstellungen für einen Datenstrom fest.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <param name="prefs">Die neuen Voreinstellungen.</param>
        void SetMixingPrefs( UInt32 streamID, UInt32 prefs );
        
        /// <summary>
        /// Meldet die Voreinstellungen eines Datenstroms.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <returns>Die aktuell verwendeten Voreinstellungen.</returns>
        UInt32 GetMixingPrefs( UInt32 streamID );

        /// <summary>
        /// Legt Parameter eines Datenstroms fest.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <param name="control">Die neuen Parameter.</param>
        void SetProcAmpControl( UInt32 streamID, ref VMRProcAmpControl control );
        
        /// <summary>
        /// Meldet die Parameter eines Datenstroms.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <param name="control">Die aktuell verwendeten Parameter.</param>
        void GetProcAmpControl( UInt32 streamID, ref VMRProcAmpControl control );

        /// <summary>
        /// Meldet die Grenzwerte von Parameters.
        /// </summary>
        /// <param name="streamID">Laufende Nummer eines Datenstroms.</param>
        /// <param name="range">Die aktuell gültigen Grenzwerte.</param>
        void GetProcAmpControlRange( UInt32 streamID, ref VMRProcAmpControlRange range );
    }
}