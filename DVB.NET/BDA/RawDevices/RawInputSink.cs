using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Klasse zum Empfang von Eingangsdaten eines <i>Raw Devices</i> von Windows.
    /// </summary>
    public class RawInputSink : IDisposable
    {
        /// <summary>
        /// Nimmt alle Nachrichten entgegen.
        /// </summary>
        private class _Sink : Form
        {
            /// <summary>
            /// Die zugehörige Steuereinheit.
            /// </summary>
            public RawInputSink RawDevice { get; set; }

            /// <summary>
            /// Erzeugt ein neues Fenster.
            /// </summary>
            public _Sink()
            {
                // Configure
                Text = "[RC Receiver]";
            }

            /// <summary>
            /// Bearbeitet eine Windows Meldung über eine Benutzereingabe.
            /// </summary>
            /// <param name="m">Die zu bearbeitende Meldung</param>
            protected override void WndProc( ref Message m )
            {
                // Pre process
                if (RawDevice != null)
                    if (RawDevice.ProcessMessage( ref m ))
                        return;

                // Forward
                base.WndProc( ref m );
            }
        }

        /// <summary>
        /// Beschreibt die Struktur zur Anmeldung des Empfangs von Rohdaten eines
        /// Eingabegerätes.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct _RawInputDevice
        {
            /// <summary>
            /// Die Größe dieser Struktur in der serialisierten Form.
            /// </summary>
            public static readonly int SizeOf = Marshal.SizeOf( typeof( _RawInputDevice ) );

            /// <summary>
            /// Die Hauptkategorie des Gerätes.
            /// </summary>
            public UInt16 usUsagePage;

            /// <summary>
            /// Die Unterkategorie des Gerätes.
            /// </summary>
            public UInt16 usUsage;

            /// <summary>
            /// Zusätzliche Feineinstellungen.
            /// </summary>
            public Int32 dwFlags;

            /// <summary>
            /// Das Fenster, an das die Meldungen geschickt werden sollen.
            /// </summary>
            public IntPtr hwndTarget;
        }

        /// <summary>
        /// Kopfinformationen für Rohdaten.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct _RawInputHeader
        {
            /// <summary>
            /// Die Größe dieser Struktur in der serialisierten Form.
            /// </summary>
            public static readonly int SizeOf = Marshal.SizeOf( typeof( _RawInputHeader ) );

            /// <summary>
            /// Art der Daten.
            /// </summary>
            public RawDeviceType dwType;

            /// <summary>
            /// Größe der Daten.
            /// </summary>
            public UInt32 dwSize;

            /// <summary>
            /// Zugehöriges Eingabegerät.
            /// </summary>
            public IntPtr hDevice;

            /// <summary>
            /// Optionaler Parameter
            /// </summary>
            public IntPtr wParam;
        };

        /// <summary>
        /// Meldet den Empfang von Rohdaten an.
        /// </summary>
        /// <param name="pRawInputDevices">Die Beschreibungen des Empfangs.</param>
        /// <param name="uiNumDevices">Die Anzahl der Beschreibungen.</param>
        /// <param name="cbSize">Die Größe einer Beschreibung.</param>
        /// <returns>Gesetzt, wenn die Anmeldung erfolgreich war.</returns>
        [DllImport( "user32.dll" )]
        private static extern bool RegisterRawInputDevices( _RawInputDevice[] pRawInputDevices, Int32 uiNumDevices, Int32 cbSize );

        /// <summary>
        /// Liest Roheingabedateien ein.
        /// </summary>
        /// <param name="hRawInput">Der Verweis auf die Eingabedaten aus der <see cref="Message"/>.</param>
        /// <param name="uiCommand">Die Art der abzurufenden Daten.</param>
        /// <param name="pData">Die bereitgestellten Daten.</param>
        /// <param name="pcbSize">Die Größe des Speicherbereiches.</param>
        /// <param name="cbSizeHeader">Die Größe des Kopffeldes.</param>
        /// <returns>Ergebnis des Aufrufs.</returns>
        [DllImport( "user32.dll" )]
        private static extern UInt32 GetRawInputData( IntPtr hRawInput, UInt32 uiCommand, IntPtr pData, ref UInt32 pcbSize, Int32 cbSizeHeader );

        /// <summary>
        /// Wird aufgerufen, wenn ein Steuercode empfangen wurde.
        /// </summary>
        public event Action<ushort> OnRCReceived;

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste erkannt wurde.
        /// </summary>
        public event Action<Keys> OnKeyReceived;

        /// <summary>
        /// Fenster zu Koppelung an die Meldungen der Fernbedienung.
        /// </summary>
        private _Sink m_Form;

        /// <summary>
        /// Erzeugt eine neue Empfangseinheit.
        /// </summary>
        private RawInputSink()
        {
        }

        /// <summary>
        /// Meldet einen Verbraucher für Sequenzen an.
        /// </summary>
        /// <param name="receiver">Der gewünschte Verbraucher.</param>
        public void SetReceiver( Action<MappingItem> receiver )
        {
            // Attach to all events
            OnRCReceived = code => receiver( code );
            OnKeyReceived = key => receiver( key );
        }

        /// <summary>
        /// Bearbeitet eine Windows Nachricht.
        /// </summary>
        /// <param name="msg">Die zu bearbeitende Nachricht.</param>
        /// <returns>Gesetzt, wenn die Nachricht endgültig bearbeitet wurde.</returns>
        public bool ProcessMessage( ref Message msg )
        {
            // Check code
            if (msg.Msg != 255)
                return false;

            // We process it
            msg.Result = IntPtr.Zero;

            // Size needed
            UInt32 size = 0;

            // Request size
            if (GetRawInputData( msg.LParam, 0x10000003, IntPtr.Zero, ref size, _RawInputHeader.SizeOf ) != 0)
                return true;

            // Allocate
            byte[] rawData = new byte[size];

            // Lock in memory
            var locked = GCHandle.Alloc( rawData, GCHandleType.Pinned );
            try
            {
                // Attach to the address
                IntPtr baseAddress = locked.AddrOfPinnedObject();

                // Load it
                if (GetRawInputData( msg.LParam, 0x10000003, baseAddress, ref size, _RawInputHeader.SizeOf ) != rawData.Length)
                    return true;

                // Take a look in the header only
                var header = (_RawInputHeader) Marshal.PtrToStructure( baseAddress, typeof( _RawInputHeader ) );
                if (header.dwType == RawDeviceType.Keyboard)
                {
                    // Load
                    var keyCallback = OnKeyReceived;
                    if (keyCallback == null)
                        return true;

                    // Read the key
                    var key = Marshal.ReadInt16( baseAddress, _RawInputHeader.SizeOf + 6 );

                    // Read the message
                    var code = Marshal.ReadInt32( baseAddress, _RawInputHeader.SizeOf + 8 );

                    // Report
                    if ((code == 0x101) || (code == 0x105))
                        keyCallback( (Keys) key );

                    // Done
                    return true;
                }
                else if (header.dwType == RawDeviceType.Mouse)
                {
                    // Just eaten
                    return true;
                }
                else if (header.dwType != RawDeviceType.Other)
                {
                    // Just eaten
                    return true;
                }

                // Load
                var rcCallback = OnRCReceived;
                if (rcCallback == null)
                    return true;

                // Read the size per entry
                var sizePerEntry = Marshal.ReadInt32( baseAddress, _RawInputHeader.SizeOf + 0 );
                if (sizePerEntry < 3)
                    return true;

                // Read the number of entries
                var entryCount = Marshal.ReadInt32( baseAddress, _RawInputHeader.SizeOf + sizeof( Int32 ) );

                // Process all codes
                for (int offset = _RawInputHeader.SizeOf + 2 * sizeof( Int32 ) + 1; entryCount-- > 0; offset += sizePerEntry)
                {
                    // Full cross check
                    if ((offset + 1) > rawData.Length)
                        break;

                    // Get the RC code
                    var rc = Marshal.ReadInt16( baseAddress, offset );

                    // Report it
                    if (rc != 0)
                        rcCallback( (ushort) rc );
                }

                // Done
                return true;
            }
            finally
            {
                // Release
                locked.Free();
            }
        }

        /// <summary>
        /// Erzeugt eine neue Empfangseinheit.
        /// </summary>
        /// <param name="usagePage">Die Hauptkategorie des Gerätes.</param>
        /// <param name="usage">Die Unterkategorie des Gerätes.</param>
        /// <returns>Die angeforderte Empfangseinheit.</returns>
        public static RawInputSink Create( ushort usagePage, ushort usage )
        {
            // Forward
            return Create( usagePage, usage, IntPtr.Zero );
        }

        /// <summary>
        /// Erzeugt eine neue Empfangseinheit.
        /// </summary>
        /// <returns>Die angeforderte Empfangseinheit.</returns>
        public static RawInputSink Create()
        {
            // Will create a private form
            var form = new _Sink();
            try
            {
                // Create an connect
                form.RawDevice = RawInputSink.Create( form.Handle );

                // Register for cleanup
                form.RawDevice.m_Form = form;

                // Report
                return form.RawDevice;
            }
            catch
            {
                // Cleanup
                form.Dispose();

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Erzeugt eine neue Empfangseinheit.
        /// </summary>
        /// <param name="window">Optional das Fenster, das die Benachrichtigungen erhalten soll.</param>
        /// <returns>Die angeforderte Empfangseinheit.</returns>
        public static RawInputSink Create( IntPtr window )
        {
            // Create registration structure
            var register =
                new _RawInputDevice[]
                    {
                        // RC
                        new _RawInputDevice { usUsagePage = 12, usUsage = 1, hwndTarget = window, dwFlags = 0 },
                        // RC
                        new _RawInputDevice { usUsagePage = 0xffbc, usUsage = 0x88, hwndTarget = window, dwFlags = 0 },
                        // Keyboard
                        new _RawInputDevice { usUsagePage = 1, usUsage = 6, hwndTarget = window, dwFlags = 0x00000230 },
                    };

            // Override flags
            if (window != IntPtr.Zero)
                for (int i = register.Length; i-- > 0; )
                    register[i].dwFlags |= 0x00000100;

            // Process
            if (!RegisterRawInputDevices( register, register.Length, _RawInputDevice.SizeOf ))
                throw new InvalidOperationException();

            // Report
            return new RawInputSink();
        }

        /// <summary>
        /// Erzeugt eine neue Empfangseinheit.
        /// </summary>
        /// <param name="usagePage">Die Hauptkategorie des Gerätes.</param>
        /// <param name="usage">Die Unterkategorie des Gerätes.</param>
        /// <param name="window">Optional das Fenster, das die Benachrichtigungen erhalten soll.</param>
        /// <returns>Die angeforderte Empfangseinheit.</returns>
        public static RawInputSink Create( ushort usagePage, ushort usage, IntPtr window )
        {
            // Create registration structure
            var register =
                new _RawInputDevice[]
                    {
                        new _RawInputDevice { usUsagePage = usagePage, usUsage = usage, hwndTarget = window, dwFlags = 0 },
                    };

            // Override flags
            if (window != IntPtr.Zero)
                register[0].dwFlags |= 0x00000100;

            // Process
            if (!RegisterRawInputDevices( register, register.Length, _RawInputDevice.SizeOf ))
                throw new InvalidOperationException();

            // Report
            return new RawInputSink();
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public void Dispose()
        {
            // Get rid of optional form
            using (m_Form)
                m_Form = null;
        }

        #endregion
    }
}
