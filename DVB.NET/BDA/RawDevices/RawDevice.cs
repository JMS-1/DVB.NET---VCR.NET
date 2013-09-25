using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Die Arten von Eingabegeräten.
    /// </summary>
    public enum RawDeviceType : uint
    {
        /// <summary>
        /// Eine Maus.
        /// </summary>
        Mouse,

        /// <summary>
        /// Eine Tastatur.
        /// </summary>
        Keyboard,

        /// <summary>
        /// Ein anderes Eingabegerät.
        /// </summary>
        Other
    }

    /// <summary>
    /// Beschreibt ein einzelnes Eingabegerät.
    /// </summary>
    public class RawDevice
    {
        /// <summary>
        /// Ermittelt Daten zu einem Eingabegerät.
        /// </summary>
        /// <param name="hDevice">Die Windows Referenz zum Gerät.</param>
        /// <param name="uiCommand">Die Art der benötigten Informationen.</param>
        /// <param name="pData">Verweis auf ein Datenfeld, in dem die Informationen abgelegt werden sollen.</param>
        /// <param name="pcbSize">Größe des Speicherbereichs.</param>
        /// <returns>Ergebnis des Aufrufs.</returns>
        [DllImport( "user32.dll" )]
        private static extern Int32 GetRawInputDeviceInfoW( IntPtr hDevice, Int32 uiCommand, IntPtr pData, ref Int32 pcbSize );

        /// <summary>
        /// Informationen über ein Eingabegerät der Art <i>Maus</i>.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct _InfoMouse
        {
            /// <summary>
            /// Die Größe diese Struktur.
            /// </summary>
            public static readonly int SizeOf = Marshal.SizeOf( typeof( _InfoMouse ) );

            /// <summary>
            /// Die Größe der Gesamtstruktur.
            /// </summary>
            public UInt32 cbSize;

            /// <summary>
            /// Die Art des Gerätes.
            /// </summary>
            public RawDeviceType dwType;

            /// <summary>
            /// Die Kennung des Gerätes.
            /// </summary>
            public UInt32 dwId;

            /// <summary>
            /// Die Anzahl der Knöpfe für das Gerät.
            /// </summary>
            public UInt32 dwNumberOfButtons;

            /// <summary>
            /// Die Anzahl der Datenpunkte pro Sekunde.
            /// </summary>
            public UInt32 dwSampleRate;

            /// <summary>
            /// Gesetzt, wenn ein horizontales Mausrad zur Verfügung steht.
            /// </summary>
            public bool fHasHorizontalWheel;
        };

        /// <summary>
        /// Informationen über ein Eingabegerät der Art <i>Tastatur</i>.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct _InfoKeyboard
        {
            /// <summary>
            /// Die Größe diese Struktur.
            /// </summary>
            public static readonly int SizeOf = Marshal.SizeOf( typeof( _InfoKeyboard ) );

            /// <summary>
            /// Die Größe der Gesamtstruktur.
            /// </summary>
            public UInt32 cbSize;

            /// <summary>
            /// Die Art des Gerätes.
            /// </summary>
            public RawDeviceType dwType;

            /// <summary>
            /// Die Art der Tastatur.
            /// </summary>
            public UInt32 dwKeyboardType;

            /// <summary>
            /// Die Variante der Tastatur.
            /// </summary>
            public UInt32 dwKeyboardSubType;

            /// <summary>
            /// Die Art der Tastenauswertung.
            /// </summary>
            public UInt32 dwKeyboardMode;

            /// <summary>
            /// Die Anzahl der Funktionstasten.
            /// </summary>
            public UInt32 dwNumberOfFunctionKeys;

            /// <summary>
            /// Die Anzahl der Kontrolltasten.
            /// </summary>
            public UInt32 dwNumberOfIndicators;

            /// <summary>
            /// Die gesamte Anzahl der Tasten.
            /// </summary>
            public UInt32 dwNumberOfKeysTotal;
        };

        /// <summary>
        /// Informationen über ein allgemeines Eingabegerät.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct _InfoHID
        {
            /// <summary>
            /// Die Größe diese Struktur.
            /// </summary>
            public static readonly int SizeOf = Marshal.SizeOf( typeof( _InfoHID ) );

            /// <summary>
            /// Die Größe der Gesamtstruktur.
            /// </summary>
            public UInt32 cbSize;

            /// <summary>
            /// Die Art des Gerätes.
            /// </summary>
            public RawDeviceType dwType;

            /// <summary>
            /// Die Kennung des Anbieters.
            /// </summary>
            public UInt32 dwVendorId;

            /// <summary>
            /// Die Kennung des Produktes.
            /// </summary>
            public UInt32 dwProductId;

            /// <summary>
            /// Die Versionsnummer.
            /// </summary>
            public UInt32 dwVersionNumber;

            /// <summary>
            /// Die <i>HID</i> Hauptkategorie.
            /// </summary>
            public UInt16 usUsagePage;

            /// <summary>
            /// Die <i>HID</i> Art.
            /// </summary>
            public UInt16 usUsage;
        }

        /// <summary>
        /// Die Daten zum Gerät.
        /// </summary>
        private RawDeviceCollection._RawDevice m_Device;

        /// <summary>
        /// Der eindeutige Name des Gerätes.
        /// </summary>
        private string m_Name;

        /// <summary>
        /// Die Rohinformationen zu diesem Gerät.
        /// </summary>
        private object m_Info;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="device">Die Daten zum Gerät.</param>
        internal RawDevice( RawDeviceCollection._RawDevice device )
        {
            // Remember
            m_Device = device;
        }

        /// <summary>
        /// Meldet die Art des Gerätes.
        /// </summary>
        public RawDeviceType DeviceType
        {
            get
            {
                // Report
                return m_Device.dwType;
            }
        }

        /// <summary>
        /// Meldet eine Beschreibung des Gerätes.
        /// </summary>
        public RawDeviceInformation Information
        {
            get
            {
                // Attach to raw information
                var rawInformation = RawInformation;

                // Check mouse
                var rawMouse = rawInformation as _InfoMouse?;
                if (rawMouse.HasValue)
                    return new RawMouseInformation { NumberOfButtons = rawMouse.Value.dwNumberOfButtons };

                // Check keyboard
                var rawKeyboard = rawInformation as _InfoKeyboard?;
                if (rawKeyboard.HasValue)
                    return new RawKeyboardInformation { NumberOfKeys = rawKeyboard.Value.dwNumberOfKeysTotal };

                // Other
                var rawOther = rawInformation as _InfoHID?;
                if (rawOther.HasValue)
                    return new RawOtherInformation { UsagePage = rawOther.Value.usUsagePage, Usage = rawOther.Value.usUsage };

                // Ups
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Meldet die Rohinformationen zu diesem Gerät.
        /// </summary>
        private object RawInformation
        {
            get
            {
                // Load once only
                if (m_Info == null)
                {
                    // Get the size to allocate
                    int bytes = Math.Max( Math.Max( _InfoMouse.SizeOf, _InfoKeyboard.SizeOf ), _InfoHID.SizeOf );

                    // Allocate
                    byte[] buffer = new byte[bytes];

                    // Lock in memory
                    var locked = GCHandle.Alloc( buffer, GCHandleType.Pinned );
                    try
                    {
                        // Load
                        if (GetRawInputDeviceInfoW( m_Device.hDevice, 0x2000000b, locked.AddrOfPinnedObject(), ref bytes ) != buffer.Length)
                            throw new OutOfMemoryException();

                        // Make it a structure
                        switch (m_Device.dwType)
                        {
                            case RawDeviceType.Mouse: m_Info = Marshal.PtrToStructure( locked.AddrOfPinnedObject(), typeof( _InfoMouse ) ); break;
                            case RawDeviceType.Keyboard: m_Info = Marshal.PtrToStructure( locked.AddrOfPinnedObject(), typeof( _InfoKeyboard ) ); break;
                            case RawDeviceType.Other: m_Info = Marshal.PtrToStructure( locked.AddrOfPinnedObject(), typeof( _InfoHID ) ); break;
                            default: throw new NotSupportedException( m_Device.dwType.ToString() );
                        }
                    }
                    finally
                    {
                        // Move back
                        locked.Free();
                    }
                }

                // Report
                return m_Info;
            }
        }

        /// <summary>
        /// Ermittelt den eindeutigen Namen des Gerätes.
        /// </summary>
        public string Name
        {
            get
            {
                // Once only
                if (m_Name == null)
                {
                    // Helper
                    Int32 bufferSize = 0;

                    // Request the size
                    if (GetRawInputDeviceInfoW( m_Device.hDevice, 0x20000007, IntPtr.Zero, ref bufferSize ) != 0)
                        throw new NotImplementedException();

                    // Allocate
                    char[] buffer = new char[bufferSize];

                    // Lock in memory
                    var locked = GCHandle.Alloc( buffer, GCHandleType.Pinned );
                    try
                    {
                        // Load it
                        if (GetRawInputDeviceInfoW( m_Device.hDevice, 0x20000007, locked.AddrOfPinnedObject(), ref bufferSize ) != buffer.Length)
                            throw new OutOfMemoryException();
                    }
                    finally
                    {
                        // Release
                        locked.Free();
                    }

                    // Make it a string
                    m_Name = new string( buffer, 0, buffer.Length - 1 );
                }

                // Report
                return m_Name;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigenamen zu Testzwecken.
        /// </summary>
        /// <returns>Der gewünschte Anzeigename.</returns>
        public override string ToString()
        {
            // Combine
            return string.Format( "{0} ({1})", m_Device.dwType, Name );
        }
    }
}
