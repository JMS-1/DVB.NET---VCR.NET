using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;


namespace JMS.DVBVCR.RecordingService.Win32Tools
{
    /// <summary>
    /// Hilfsmethoden zum Erkennen angemeldeter Benutzer.
    /// </summary>
    public static class LogonManager
    {
        /// <summary>
        /// Das Kürzel für die Registrierung der Machinendaten.
        /// </summary>
        private static readonly UInt32 LocalMachineKey = 0x80000002;

        /// <summary>
        /// Der Name des Registryschlüssels, der hier interessiert.
        /// </summary>
        private const string KeyName = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";

        /// <summary>
        /// Der Name des Wertes, der ausgelesen werden soll.
        /// </summary>
        private const string ValueName = "Shell";

        /// <summary>
        /// Öffnet einen Registrierungsschlüssel.
        /// </summary>
        /// <param name="hKey">Der Bezugsschlüssel.</param>
        /// <param name="lpSubKey">Der relative Name des gewünschten Schlüssels.</param>
        /// <param name="ulOptions">Optionen zum Öffnen.</param>
        /// <param name="samDesired">Die gewünschten Zugriffsrechte.</param>
        /// <param name="phkResult">Der neu geöffnete Schlüssel.</param>
        /// <returns>0, wenn kein Fehler aufgetreten ist.</returns>
        [DllImport( "Advapi32.dll" )]
        private static extern Int32 RegOpenKeyEx( UInt32 hKey, string lpSubKey, UInt32 ulOptions, UInt32 samDesired, out IntPtr phkResult );

        /// <summary>
        /// Schließt einen Registrierungsschlüssel.
        /// </summary>
        /// <param name="hKey">Der gewünschte Schlüssel.</param>
        /// <returns>0, wenn kein Fehler aufgetreten ist.</returns>
        [DllImport( "Advapi32.dll" )]
        private static extern Int32 RegCloseKey( IntPtr hKey );

        /// <summary>
        /// Liest einen Wert.
        /// </summary>
        /// <param name="hKey">Der Schlüssel, unter dem der Wert angelegt ist.</param>
        /// <param name="lpValueName">Der Name des Wertes.</param>
        /// <param name="lpReserved">Reserviert für zukünftige Nutzung.</param>
        /// <param name="lpType">Meldet die Art des Wertes.</param>
        /// <param name="result">Meldet den Wert.</param>
        /// <param name="lpcbData">Meldet die Größe des Wertes.</param>
        /// <returns>0, wenn kein Fehler aufgetreten ist.</returns>
        [DllImport( "Advapi32.dll" )]
        private static extern Int32 RegQueryValueEx( IntPtr hKey, string lpValueName, IntPtr lpReserved, out Int32 lpType, StringBuilder result, out Int32 lpcbData );

        /// <summary>
        /// Meldet den Registrierungseintrag mit dem Namen der Shell.
        /// </summary>
        private static string ShellName
        {
            get
            {
                // Try to open - 64 Bit first on 64 Bit OS
                if (RegOpenKeyEx(LocalMachineKey, KeyName, 0, 0x20119, out IntPtr key) == 0)
                    try
                    {
                        if (RegQueryValueEx(key, ValueName, IntPtr.Zero, out int type, null, out int size) == 0)
                            if ((type == 1) || (type == 2))
                                if (size > 1)
                                {
                                    // Allocate enough space
                                    var buffer = new StringBuilder(size + 1);

                                    // Read
                                    if (RegQueryValueEx(key, ValueName, IntPtr.Zero, out type, buffer, out size) == 0)
                                    {
                                        // Finish buffer
                                        buffer.Length = size - 1;

                                        // Report
                                        return buffer.ToString();
                                    }
                                }
                    }
                    finally
                    {
                        // Free resources
                        RegCloseKey(key);
                    }

                // Find the name of the shell
                using (var reg = Registry.LocalMachine.OpenSubKey( KeyName ))
                    if (reg == null)
                        return null;
                    else
                        return reg.GetValue( ValueName ) as string;
            }
        }

        /// <summary>
        /// Meldet, ob ein Anwender angemeldet ist. Die Prüfung erfolgt über die in der (32 Bit) Registry
        /// vermerke <i>Shell</i> Anwendung, i.a. <i>explorer.exe</i>. Wird eine solche Anwendung als
        /// aktiver Prozess erkannt, so wird von einem angemeldeten Anwender ausgegangen.
        /// </summary>
        public static bool HasInteractiveUser
        {
            get
            {
                // Find the name of the shell
                var shell = ShellName;
                if (string.IsNullOrEmpty( shell ))
                    return true;

                // Get the name of the file - registry will hold some absolute or relative entry
                shell = Path.GetFileNameWithoutExtension( shell );
                if (string.IsNullOrEmpty( shell ))
                    return true;

                // See if we found some
                var found = false;

                // All processes - should call a proper dispose on all requested items
                foreach (var process in Process.GetProcesses())
                    using (process)
                        if (!found)
                            try
                            {
                                // Test it - ignore any error
                                found = shell.Equals( process.ProcessName, StringComparison.InvariantCultureIgnoreCase );
                            }
                            catch
                            {
                            }

                // Report
                return found;
            }
        }
    }
}
