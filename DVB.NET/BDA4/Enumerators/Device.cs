using System;
using System.Linq;
using System.Text;
using System.Security;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Enumerators
{
    /// <summary>
    /// Helper class to start and stop devices. 
    /// </summary>
    public abstract class Device
    {
        /// <summary>
        /// Standardkonvertierung von Zeichen.
        /// </summary>
        private static Encoding AnsiEncoding = Encoding.GetEncoding( 1252 );

        #region Setup API

        /// <summary>
        /// Wrapper of the ANSI version of the SetupAPI function <i>SetupDiGetDeviceRegistryProperty</i>.
        /// </summary>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiGetDeviceRegistryPropertyA" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool SetupDiGetDeviceRegistryProperty( IntPtr DeviceInfoSet, ref DeviceInfoData DeviceInfoData, Int32 Property, out UInt32 PropertyRegDataType, byte[] PropertyBuffer, Int32 PropertyBufferSize, out UInt32 RequiredSize );

        /// <summary>
        /// Wrapper of the ANSI version of the SetupAPI function <i>SetupDiGetDeviceRegistryProperty</i>.
        /// </summary>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiGetDeviceRegistryPropertyA" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool SetupDiGetDeviceRegistryProperty( IntPtr DeviceInfoSet, ref DeviceInfoData DeviceInfoData, UInt32 Property, out UInt32 PropertyRegDataType, IntPtr PropertyBuffer, UInt32 PropertyBufferSize, out UInt32 RequiredSize );

        /// <summary>
        /// Wrapper of the SetupAPI function <i>SetupDiGetDeviceInterfaceDetail</i>.
        /// </summary>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiGetDeviceInterfaceDetailW" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool SetupDiGetDeviceInterfaceDetail( IntPtr deviceInfoSet, ref DeviceInterfaceData deviceInterfaceData, IntPtr deviceInterfaceDetailData, Int32 deviceInterfaceDetailDataSize, out UInt32 requiredSize, IntPtr deviceInfoData );

        /// <summary>
        /// Wrapper of the SetupAPI function <i>SetupDiSetClassInstallParams</i>.
        /// </summary>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiSetClassInstallParams" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool SetupDiSetClassInstallParams( IntPtr DeviceInfoSet, ref DeviceInfoData rData, ref PropChangeParams ClassInstallParams, UInt32 ClassInstallParamsSize );

        /// <summary>
        /// Wrapper of the SetupAPI function <i>SetupDiCallClassInstaller</i>.
        /// </summary>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiCallClassInstaller" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool SetupDiCallClassInstaller( UInt32 InstallFunction, IntPtr DeviceInfoSet, ref DeviceInfoData rData );

        /// <summary>
        /// Wrapper of the SetupAPI function <i>SetupDiEnumDeviceInfo</i>.
        /// </summary>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiEnumDeviceInfo" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool SetupDiEnumDeviceInfo( IntPtr DeviceInfoSet, UInt32 MemberIndex, ref DeviceInfoData rData );

        /// <summary>
        /// Wrapper of the SetupAPI function <i>SetupDiGetClassDevs</i>.
        /// </summary>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiGetClassDevs" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern IntPtr SetupDiGetClassDevs( ref Guid pGUID, IntPtr Enumerator, IntPtr hwndParent, UInt32 Flags );

        /// <summary>
        /// Wrapper of the SetupAPI function <i>SetupDiDestroyDeviceInfoList</i>.
        /// </summary>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiDestroyDeviceInfoList" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool SetupDiDestroyDeviceInfoList( IntPtr DeviceInfoSet );

        /// <summary>
        /// Wrapper of the SetupAPI function <i>SetupDiEnumDeviceInterfaces</i>.
        /// </summary>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiEnumDeviceInterfaces" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool SetupDiEnumDeviceInterfaces( IntPtr deviceInfoSet, IntPtr deviceInfoData, ref Guid interfaceClassGuid, UInt32 memberIndex, ref DeviceInterfaceData deviceInterfaceData );

        /// <summary>
        /// Ermittelt die Instanzennummer eines Gerätes.
        /// </summary>
        /// <param name="deviceInfoSet">Eine Geräteauflistung.</param>
        /// <param name="deviceInfoData">Die Daten zu einem Gerät.</param>
        /// <param name="buffer">Ein Speicherbereich für die Nummer.</param>
        /// <param name="bufferSize">Die Größe des Speicherbereichs in Bytes.</param>
        /// <param name="requiredSize">Die Anzahl der benötigten Bytes.</param>
        /// <returns>Gesetzt, wenn der Aufruf erfolgreich war.</returns>
        [DllImport( "setupapi.dll", EntryPoint = "SetupDiGetDeviceInstanceIdA" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool SetupDiGetDeviceInstanceId( IntPtr deviceInfoSet, ref DeviceInfoData deviceInfoData, byte[] buffer, int bufferSize, out UInt32 requiredSize );

        /// <summary>
        /// Äquivalent der <i>SetupAPI</i> Struktur <i>SP_DEVICE_INTERFACE_DATA</i>.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct DeviceInterfaceData
        {
            /// <summary>
            /// Die Größe dieser Struktur in Bytes.
            /// </summary>
            public Int32 cbSize;

            /// <summary>
            /// Die eindeutige Kennung der Schnittstelle.
            /// </summary>
            public Guid InterfaceClassGuid;

            /// <summary>
            /// Informationen zum Zustand.
            /// </summary>
            public UInt32 Flags;

            /// <summary>
            /// Reserviert.
            /// </summary>
            public IntPtr Reserved;
        }

        /// <summary>
        /// Maps the SetupAPI structure <i>SP_DEVINFO_DATA</i>.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct DeviceInfoData
        {
            /// <summary>
            /// Size of this structure in bytes.
            /// </summary>
            public Int32 cbSize;

            /// <summary>
            /// Hold the unique identifier of the device class. For VCR.NET this
            /// will always be <see cref="m_Class"/>.
            /// </summary>
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 )]
            public byte[] ClassGuid;

            /// <summary>
            /// Device instance identifier.
            /// </summary>
            public UInt32 DevInst;

            /// <summary>
            /// Not used.
            /// </summary>
            public IntPtr Reserved;
        }

        /// <summary>
        /// Maps the SetupAPI structure <i>SP_CLASSINSTALL_HEADER</i>.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        struct ClassInstallHeader
        {
            /// <summary>
            /// Size of this structure in bytes.
            /// </summary>
            public const uint SizeOf = 8;

            /// <summary>
            /// Will be filled with <see cref="SizeOf"/>.
            /// </summary>
            public UInt32 cbSize;

            /// <summary>
            /// Function to use. VCR.NET only uses <i>DIF_PROPERTYCHANGE</i>.
            /// </summary>
            public UInt32 InstallFunction;
        }

        /// <summary>
        /// Maps the SetupAPI structure <i>SP_PROPCHANGE_PARAMS</i> to change
        /// single configuration items of a device.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        struct PropChangeParams
        {
            /// <summary>
            /// Overall size of this structure in bytes.
            /// </summary>
            public const uint SizeOf = 12 + ClassInstallHeader.SizeOf;

            /// <summary>
            /// Describes the function to use.
            /// </summary>
            public ClassInstallHeader ClassInstallHeader;

            /// <summary>
            /// The new value of the property.
            /// </summary>
            public UInt32 StateChange;

            /// <summary>
            /// The scope of the change.
            /// </summary>
            public UInt32 Scope;

            /// <summary>
            /// See SetupAPI documentation for details.
            /// </summary>
            public UInt32 HwProfile;
        }

        #endregion

        /// <summary>
        /// The unique identifier of the device class. It will be constructed
        /// using <see cref="Guid.ToByteArray"/>.
        /// </summary>
        private Guid m_Class;

        /// <summary>
        /// The display name of the device used for this instance.
        /// </summary>
        private string m_Adaptor;

        /// <summary>
        /// The property used to find the device by name.
        /// </summary>
        private uint m_FilterIndex = 0;

        /// <summary>
        /// Create a new device management instance.
        /// </summary>
        /// <param name="sAdaptorName">Remembered in <see cref="m_Adaptor"/>.</param>
        /// <param name="classIdentifier">The Class Identifier of the related Windows
        /// Device Class.</param>
        /// <param name="propertyIndex">The name of the property used to filter for
        /// the device by name. <i>0</i> stands for the device name.</param>
        protected Device( string sAdaptorName, string classIdentifier, uint propertyIndex )
        {
            // Remember
            m_Class = new Guid( classIdentifier );
            m_FilterIndex = propertyIndex;
            m_Adaptor = sAdaptorName;
        }

        /// <summary>
        /// Create a new device management instance.
        /// </summary>
        /// <param name="sAdaptorName">Remembered in <see cref="m_Adaptor"/>.</param>
        /// <param name="classIdentifier">The Class Identifier of the related Windows
        /// Device Class.</param>
        protected Device( string sAdaptorName, string classIdentifier )
            : this( sAdaptorName, classIdentifier, 0 )
        {
        }

        /// <summary>
        /// Will use the SetupAPI to locate the <see cref="m_Adaptor"/> network device
        /// and apply the indicated change to its activation state.
        /// </summary>
        /// <exception cref="DeviceException">For all errors from the SetupAPI
        /// calls.</exception>
        public bool Enabled
        {
            set
            {
                // Use helper
                ChangeState( value ? 1 : 2 );
            }
        }

        /// <summary>
        /// Will use the SetupAPI to locate the <see cref="m_Adaptor"/> network device
        /// and apply the indicated change to its running state.
        /// </summary>
        /// <exception cref="DeviceException">For all errors from the SetupAPI
        /// calls.</exception>
        public bool Running
        {
            set
            {
                // Use helper
                ChangeState( value ? 4 : 5 );
            }
        }

        /// <summary>
        /// Find the related device and change the operating state.
        /// </summary>
        /// <param name="stateCode">The code for the state corresponding to
        /// the <i>DICS_</i> constants of Windows.</param>
        /// <returns></returns>
        private void ChangeState( int stateCode )
        {
            // Open device
            IntPtr hInfoList = SetupDiGetClassDevs( ref m_Class, IntPtr.Zero, IntPtr.Zero, 2 );

            // Validate
            if (-1 == (int) hInfoList)
                throw new DeviceException( "Could not create Device Set" );

            // With cleanup
            try
            {
                // Create the device information data block
                DeviceInfoData pData = new DeviceInfoData();

                // Initialize
                pData.cbSize = Marshal.SizeOf( pData );

                // Find them all
                for (uint ix = 0; SetupDiEnumDeviceInfo( hInfoList, ix++, ref pData ); )
                {
                    // Load the name
                    string name;

                    // Check mode
                    if (m_FilterIndex == uint.MaxValue)
                        name = GetInstanceId( hInfoList, ref pData );
                    else
                        name = ReadStringProperty( hInfoList, ref pData, (int) m_FilterIndex );

                    // Skip
                    if (string.IsNullOrEmpty( name ))
                        continue;

                    // Compare
                    if (!m_Adaptor.Equals( name ))
                        continue;

                    // Create update helper
                    PropChangeParams pParams = new PropChangeParams();

                    // Fill
                    pParams.ClassInstallHeader.cbSize = ClassInstallHeader.SizeOf;
                    pParams.ClassInstallHeader.InstallFunction = 0x12;
                    pParams.Scope = (uint) ((stateCode < 4) ? 1 : 2);
                    pParams.StateChange = (uint) stateCode;

                    // Prepare
                    if (!SetupDiSetClassInstallParams( hInfoList, ref pData, ref pParams, PropChangeParams.SizeOf ))
                        throw new DeviceException( "Could not update Class Install Parameters" );

                    // Change mode
                    if (!SetupDiCallClassInstaller( pParams.ClassInstallHeader.InstallFunction, hInfoList, ref pData ))
                        throw new DeviceException( "Unable to finish Change Request" );

                    // Did it 
                    return;
                }

                // Not found
                throw new DeviceException( "No Device " + m_Adaptor + " found" );
            }
            finally
            {
                // Clean it
                SetupDiDestroyDeviceInfoList( hInfoList );
            }
        }

        /// <summary>
        /// Ermittelt die Anzeigenamen aller Geräte einer bestimmten Art.
        /// </summary>
        /// <param name="deviceClass">Die Art des Gerätes.</param>
        /// <returns>Eine Liste aller Anzeigenamen.</returns>
        public static string[] GetDevices( string deviceClass )
        {
            // Forward
            return GetDeviceInformations( deviceClass ).Select( i => i.DisplayName ).ToArray();
        }

        /// <summary>
        /// Ermittelt die Anzeigenamen aller Geräte einer bestimmten Art.
        /// </summary>
        /// <param name="deviceClass">Die Art des Gerätes.</param>
        /// <returns>Eine Liste aller Anzeigenamen.</returns>
        public static List<DeviceInformation> GetDeviceInformations( string deviceClass )
        {
            // Result
            var names = new List<DeviceInformation>();

            // Open enumerator
            var @class = new Guid( deviceClass );
            IntPtr hInfoList = SetupDiGetClassDevs( ref @class, IntPtr.Zero, IntPtr.Zero, 2 );
            if ((int) hInfoList == -1)
                throw new DeviceException( "Could not create Device Set" );

            // With cleanup
            try
            {
                // Create the device information data block
                var pData = new DeviceInfoData { cbSize = Marshal.SizeOf( typeof( DeviceInfoData ) ) };

                // Find them all
                for (uint ix = 0; SetupDiEnumDeviceInfo( hInfoList, ix++, ref pData ); )
                {
                    // Friendly name
                    string friendly = ReadStringProperty( hInfoList, ref pData, 0 );
                    if (string.IsNullOrEmpty( friendly ))
                        continue;

                    // Hardware identifiers
                    string instance = GetInstanceId( hInfoList, ref pData );
                    if (string.IsNullOrEmpty( instance ))
                        continue;

                    // Remember
                    names.Add( new DeviceInformation( friendly, instance ) );
                }
            }
            finally
            {
                // Clean it
                SetupDiDestroyDeviceInfoList( hInfoList );
            }

            // Sort
            names.Sort( ( l, r ) =>
                {
                    // Friendly names
                    int delta = l.DisplayName.CompareTo( r.DisplayName );

                    // Check mode
                    if (0 == delta)
                        return l.DevicePath.CompareTo( r.DevicePath );
                    else
                        return delta;
                } );

            // Report
            return names;
        }

        /// <summary>
        /// Ermittelt alle Gerätepfade einer bestimmten Art.
        /// </summary>
        /// <param name="deviceClassIdentifier">Die Art des Gerätes.</param>
        /// <returns>Eine Liste aller Gerätepfade.</returns>
        public static string[] GetDevicePaths( Guid deviceClassIdentifier )
        {
            // Result
            List<string> names = new List<string>();

            // Open device
            IntPtr hInfoList = SetupDiGetClassDevs( ref deviceClassIdentifier, IntPtr.Zero, IntPtr.Zero, 0x12 );
            if ((int) hInfoList == -1)
                throw new DeviceException( "Could not create Device Set" );

            // With cleanup
            try
            {
                // Create the device information data block
                var interfaceData = new DeviceInterfaceData { cbSize = Marshal.SizeOf( typeof( DeviceInterfaceData ) ) };

                // Buffer to use
                int bufferSize = 1000;

                // Memory allocation
                IntPtr sysmem = Marshal.AllocHGlobal( bufferSize );

                // Initialize
                Marshal.WriteInt32( sysmem, 6 );

                // Calculate offset
                IntPtr pathPtr = new IntPtr( sysmem.ToInt64() + 4 );

                // Be safe
                try
                {
                    // Find them all
                    for (uint ix = 0; SetupDiEnumDeviceInterfaces( hInfoList, IntPtr.Zero, ref deviceClassIdentifier, ix++, ref interfaceData ); )
                    {
                        // Helper
                        UInt32 needed;

                        // Time to read the details
                        if (!SetupDiGetDeviceInterfaceDetail( hInfoList, ref interfaceData, sysmem, bufferSize, out needed, IntPtr.Zero ))
                            continue;

                        // Read the path
                        names.Add( Marshal.PtrToStringUni( pathPtr ) );
                    }
                }
                finally
                {
                    // Release
                    Marshal.FreeHGlobal( sysmem );
                }
            }
            finally
            {
                // Clean it
                SetupDiDestroyDeviceInfoList( hInfoList );
            }

            // Sort
            names.Sort();

            // Report
            return names.ToArray();
        }

        /// <summary>
        /// Führt eine Reaktivierung eines Gerätes aus.
        /// </summary>
        protected void WakeUp()
        {
            // Be safe
            try
            {
                // Disable and re-enable
                Enabled = false;
                Enabled = true;
            }
            catch (Exception e)
            {
                // Report
                System.Diagnostics.EventLog.WriteEntry( "DVB.NET", string.Format( "WakeUp failed for {1}: {0}", e.Message, m_Adaptor ) );
            }
        }

        /// <summary>
        /// Ermittelt eine Zeichenketteneigenschaft eines Gerätes.
        /// </summary>
        /// <param name="handle">Die Referenz auf die Liste aller Geräte (einer Art).</param>
        /// <param name="info">Das gewünschte Gerät.</param>
        /// <param name="propertyIndex">Die gewünschte Eigenschaft.</param>
        /// <returns>Der Wert der Eigenschaft oder <i>null</i>.</returns>
        private static string ReadStringProperty( IntPtr handle, ref DeviceInfoData info, int propertyIndex )
        {
            // Allocate buffer
            var buffer = new byte[1000];

            // Load the name
            UInt32 charCount, regType;
            if (!SetupDiGetDeviceRegistryProperty( handle, ref info, propertyIndex, out regType, buffer, buffer.Length - 1, out charCount ))
                return null;
            else if (charCount-- > 0)
                return AnsiEncoding.GetString( buffer, 0, (int) charCount );
            else
                return null;
        }

        /// <summary>
        /// Ermittelt die Instanzennummer eines Gerätes.
        /// </summary>
        /// <param name="handle">Eine Geräteauflistung.</param>
        /// <param name="info">Eine Information zu dem gewünschten Gerät.</param>
        /// <returns>Die Instanzennummer oder <i>null</i>.</returns>
        private static string GetInstanceId( IntPtr handle, ref DeviceInfoData info )
        {
            // Allocate buffer
            var buffer = new byte[1000];

            // Load the name
            UInt32 charCount;
            if (!SetupDiGetDeviceInstanceId( handle, ref info, buffer, buffer.Length - 1, out charCount ))
                return null;
            else if (charCount-- > 0)
                return AnsiEncoding.GetString( buffer, 0, (int) charCount );
            else
                return null;
        }
    }
}
