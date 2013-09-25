using System;
using System.Security;
using System.Diagnostics;
using System.Runtime.InteropServices;

using comTypes = System.Runtime.InteropServices.ComTypes;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Verwaltet eine einzelne globale Anmeldung eines COM Objektes.
    /// </summary>
    public class ROTRegister : IDisposable
    {
        /// <summary>
        /// Ermittelt die Tabelle aller globalen Anmeldungen.
        /// </summary>
        /// <param name="reserved">Wird ignoriert.</param>
        /// <param name="rot">Die gewünschte Tabelle.</param>
        /// <returns>Negativ, wenn ein Abruf nicht möglich war.</returns>
        [DllImport( "ole32.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern Int32 GetRunningObjectTable( UInt32 reserved, out comTypes.IRunningObjectTable rot );

        /// <summary>
        /// Erzeugt eine Namensreferenz auf ein Objekt.
        /// </summary>
        /// <param name="delim">Trennzeichen für den hierarchischen Aufbau von Namen.</param>
        /// <param name="item">Name des Objektes.</param>
        /// <param name="moniker">Die gewünschte Namensreferenz.</param>
        /// <returns>Negativ, wenn die Erstellung fehlgeschlagen ist.</returns>
        [DllImport( "ole32.dll", CharSet = CharSet.Unicode )]
        [SuppressUnmanagedCodeSecurity]
        private static extern Int32 CreateItemMoniker( string delim, string item, out comTypes.IMoniker moniker );

        /// <summary>
        /// Die Tabelle, in der die Anmeldung erfolgte.
        /// </summary>
        private comTypes.IRunningObjectTable m_ROT = null;

        /// <summary>
        /// Das bei der Anmeldung erhaltene Kürzel.
        /// </summary>
        private int? m_Register = null;

        /// <summary>
        /// Erzeugt eine neue Anmeldung.
        /// </summary>
        /// <param name="comObject">Ein beliebiges Objekt.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Objekt angegeben.</exception>
        public ROTRegister( object comObject )
        {
            // Validate
            if (comObject == null)
                throw new ArgumentNullException( "comObject" );

            // Attach to ROT
            if (GetRunningObjectTable( 0, out m_ROT ) < 0)
                return;

            // Attach to COM interface
            IntPtr unk = Marshal.GetIUnknownForObject( comObject );
            try
            {
                // Create the name
                string moniker = string.Format( "FilterGraph {0:x8} pid {1:x8}", unk.ToInt32(), Process.GetCurrentProcess().Id );

                // Create the item moniker for the format
                comTypes.IMoniker mon;
                if (CreateItemMoniker( "!", moniker, out mon ) >= 0)
                    try
                    {
                        // Register
                        m_Register = m_ROT.Register( 1, comObject, mon );
                    }
                    finally
                    {
                        // Back to COM
                        BDAEnvironment.Release( ref mon );
                    }
            }
            finally
            {
                // Back to COM
                BDAEnvironment.Release( ref unk );
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Anmeldung endfültig.
        /// </summary>
        public void Dispose()
        {
            // Check registration
            if (m_Register.HasValue)
            {
                // Unregister
                m_ROT.Revoke( m_Register.Value );

                // Forget
                m_Register = null;
            }

            // Forget
            BDAEnvironment.Release( ref m_ROT );
        }

        #endregion
    }
}
