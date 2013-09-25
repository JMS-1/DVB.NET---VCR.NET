using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Hilfsklasse zur Verwaltung aktiver Geräteabstraktionen.
    /// </summary>
    public static class HardwareManager
    {
        /// <summary>
        /// Erzeugt eine neue Steuerinstanz für Zugriffe auf die Geräteverwaltung.
        /// </summary>
        private class _Counter : IDisposable
        {
            /// <summary>
            /// Meldet einen Client an.
            /// </summary>
            public _Counter()
            {
                // Forward
                lock (m_ActiveHardware)
                    ++m_ReferenceCounter;
            }

            #region IDisposable Members

            /// <summary>
            /// Meldet einen Client ab.
            /// </summary>
            void IDisposable.Dispose()
            {
                // Forward
                lock (m_ActiveHardware)
                    if (m_ReferenceCounter < 1)
                        throw new InvalidOperationException();
                    else if (0 == --m_ReferenceCounter)
                        Shutdown();
            }

            #endregion
        }

        /// <summary>
        /// Zählt die aktiven Registrierungen.
        /// </summary>
        private static Int32 m_ReferenceCounter = 0;

        /// <summary>
        /// Verwaltet die aktiven Geräte.
        /// </summary>
        private static Dictionary<string, Hardware> m_ActiveHardware = new Dictionary<string, Hardware>();

        /// <summary>
        /// Meldet einen Client für die Verwendung der Geräteverwaltung an.
        /// </summary>
        /// <returns>Eine Steuerinstanz die mittels <see cref="IDisposable.Dispose"/>
        /// freigegeben wird, wenn die Geräteverwaltung nicht weiter benötigt wird.</returns>
        public static IDisposable Open()
        {
            // Report
            return new _Counter();
        }

        /// <summary>
        /// Aktiviert eine bestimmte Geräteabstraktion.
        /// </summary>
        /// <param name="profileName">Der Name des zugehörigen Geräteprofils.</param>
        /// <returns>Die gewünschte Hardware oder <i>null</i>, wenn die Geräteverwaltung
        /// nicht aktiv ist.</returns>
        /// <exception cref="ArgumentException">Es wurde kein Geräteprofil angegeben.</exception>
        public static Hardware OpenHardware( string profileName )
        {
            // Validate
            if (string.IsNullOrEmpty( profileName ))
                throw new ArgumentException( profileName, "profileName" );

            // Load the profile
            Profile profile = ProfileManager.FindProfile( profileName );
            if (null == profile)
                throw new ArgumentException( profileName, "profileName" );

            // Forward
            return OpenHardware( profile );
        }

        /// <summary>
        /// Aktiviert eine bestimmte Geräteabstraktion.
        /// </summary>
        /// <param name="profile">Das zugehörigen Geräteprofils.</param>
        /// <returns>Die gewünschte Hardware oder <i>null</i>, wenn die Geräteverwaltung
        /// nicht aktiv ist.</returns>
        /// <exception cref="ArgumentException">Es wurde kein Geräteprofil angegeben.</exception>
        public static Hardware OpenHardware( Profile profile )
        {
            // Validate
            if (null == profile)
                throw new ArgumentNullException( "profile" );
            if (string.IsNullOrEmpty( profile.Name ))
                throw new ArgumentNullException( "profile.Name" );

            // Be safe
            lock (m_ActiveHardware)
            {
                // Not locked
                if (m_ReferenceCounter < 1)
                    return null;

                // Already active
                Hardware hardware;
                if (m_ActiveHardware.TryGetValue( profile.Name, out hardware ))
                    return hardware;

                // Ask profile
                hardware = profile.CreateHardware();

                // Remember
                m_ActiveHardware[profile.Name] = hardware;

                // Report
                return hardware;
            }
        }

        /// <summary>
        /// Beendet die Nutzung aller aktiven Geräteabstraktionen.
        /// </summary>
        private static void Shutdown()
        {
            // Be safe
            lock (m_ActiveHardware)
                try
                {
                    // Forward to all
                    foreach (Hardware hardware in m_ActiveHardware.Values)
                        try
                        {
                            // Forward
                            hardware.Dispose();
                        }
                        catch
                        {
                            // Ignore any error
                        }
                }
                finally
                {
                    // Forget all
                    m_ActiveHardware.Clear();
                }
        }
    }
}
