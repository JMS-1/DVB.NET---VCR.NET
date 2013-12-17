using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using JMS.DVB.DeviceAccess.Enumerators;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.TechnoTrend;


namespace JMS.DVB.Provider.TTBudget
{
    /// <summary>
    /// Anbindung an die <i>TechoTrend Budget Line</i> Geräte.
    /// </summary>
    public class BudgetProvider : ILegacyDevice
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "?InitDvbApiDll@@YAXXZ", ExactSpelling = true )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void InitDvbApiDll();

        private static bool m_Initialized = false;

        private Dictionary<ushort, Filter> m_Filters = new Dictionary<ushort, Filter>();

        private Common m_DeviceManager = null;

        private BoardControl m_Board = null;

        private Frontend m_Frontend = null;

        private CommonInterface m_CI = null;

        private int m_DeviceIndex = 0;

        private string m_WakeupDevice = null;

        private string m_WakeupDeviceInstance = null;

        /// <summary>
        /// Erstellt eine neue Anbindung.
        /// </summary>
        /// <param name="args">Die KOnfiguration des zu verwendenden Gerätes.</param>
        public BudgetProvider( Hashtable args )
        {
            // Remember
            m_WakeupDeviceInstance = (string) args["WakeupDeviceMoniker"];
            m_WakeupDevice = (string) args["WakeupDevice"];
            m_DeviceIndex = ArgumentToDevice( args );
        }

        /// <summary>
        /// Meldet Informationen zum aktuellen Empfangssignal.
        /// </summary>
        public SignalStatus SignalStatus
        {
            get
            {
                // Must access hardware
                Open();

                // Load
                var status = m_Frontend.SignalStatus;

                // Convert
                return new SignalStatus( status.Locked, status.Strength, status.Quality );
            }
        }

        private static int ArgumentToDevice( Hashtable args )
        {
            // Load
            var device = (string) args["Device"];

            // Process
            return (null == device) ? 0 : int.Parse( device );
        }

        private void Open()
        {
            // Already did it
            if (null != m_DeviceManager) return;

            // Once only
            if (!m_Initialized)
            {
                // Lock out
                m_Initialized = true;

                // Call initializer
                InitDvbApiDll();
            }

            // Create device manager
            m_DeviceManager = new Common();

            // Open the hardware channel
            m_DeviceManager.Open( m_DeviceIndex );

            // Attach to board
            m_Board = new BoardControl();

            // Do a full reset
            m_Board.Initialize();

            // Attach to frontend
            m_Frontend = new Frontend();

            // Start it up
            m_Frontend.Initialize();

            // Enable DMA access
            m_Board.EnableDMA();

            // Create CI accessor
            m_CI = new CommonInterface();
        }

        /// <summary>
        /// Erstellt einen Anzeigenamen zu Testzwecken.
        /// </summary>
        /// <returns>Der gewünschte Anzeigename.</returns>
        public override string ToString()
        {
            // Must access hardware
            Open();

            // Report
            return string.Format( "TechnoTrend Budget #{0}/{1} {2}", m_DeviceIndex, m_DeviceManager.Count, ReceiverType );
        }

        #region IDeviceProvider Members

        /// <summary>
        /// Aktiviert die Entschlüsselung.
        /// </summary>
        /// <param name="station">Die Kennung der zu entschlüsselnden Quelle.</param>
        public void Decrypt( ushort? station )
        {
            // Connect to hardware
            Open();

            // Forward
            if (station.HasValue)
            {
                // Switch on
                m_CI.Decrypt( station.Value );
            }
            else
            {
                // Switch off
                m_CI.Decrypt( 0 );
            }
        }

        /// <summary>
        /// Die Art des DVB Gerätes.
        /// </summary>
        private FrontendType ReceiverType
        {
            get
            {
                // Connect to hardware
                Open();

                // Report
                return m_Frontend.FrontendType;
            }
        }

        /// <summary>
        /// Meldet einen Datenstrom zum Empfang von Nutzdaten an.
        /// </summary>
        /// <param name="pid">Die Datenstromkennung.</param>
        /// <param name="video">Gesetzt, wenn es sich um ein Bildsignal handelt.</param>
        /// <param name="smallBuffer">Gesetzt, wenn kleine Zwischenspeicher verwendet werden sollen.</param>
        /// <param name="callback">Methode zur Aufnahme der Nutzdaten.</param>
        public void RegisterPipingFilter( ushort pid, bool video, bool smallBuffer, Action<byte[]> callback )
        {
            // Attach to hardware
            Open();

            // See if filter already exists
            if (m_Filters.ContainsKey( pid ))
                return;

            // Create new
            m_Filters[pid] = new FilterToCode( pid, callback );
        }

        /// <summary>
        /// Legt den Empfang der Nutzdatenströme fest.
        /// </summary>
        /// <param name="video">Die Datenstromkennung des Bildsignals.</param>
        /// <param name="audio">Die Datenstromkennung des Tonsignals.</param>
        public void SetVideoAudio( ushort video, ushort audio )
        {
            // Validate
            if (video != 0)
                throw new ArgumentException( video.ToString(), "video" );
            if (audio != 0)
                throw new ArgumentException( audio.ToString(), "audio" );

            // Startup API
            Open();
        }

        /// <summary>
        /// Beginnt mit dem Empfang eines Datenstrom.
        /// </summary>
        /// <param name="streamIdentifier">Die gewünschte Datenstromkennung.</param>
        public void StartFilter( ushort streamIdentifier )
        {
            // Forward
            m_Filters[streamIdentifier].Start();
        }

        /// <summary>
        /// Beginnt mit dem Empfang eines Steuerdatenstroms.
        /// </summary>
        /// <param name="pid">Die Datenstromkennung.</param>
        /// <param name="callback">Der Verarbeitungsalgorithmus.</param>
        /// <param name="match">Die Vergleichsdaten.</param>
        /// <param name="mask">Die Auswahl der Vergleichsbits.</param>
        public void StartSectionFilter( ushort pid, Action<byte[]> callback, byte[] match, byte[] mask )
        {
            // Attach to hardware
            Open();

            // Stop if running
            StopFilter( pid );

            // Create new
            var filter = new FilterToCode( pid, callback );

            // Remember
            m_Filters[pid] = filter;

            // Start at once
            filter.Start( match, mask );
        }

        /// <summary>
        /// Beendet den Empfang eines Datenstrom.
        /// </summary>
        /// <param name="streamIdentifier">Due Kennung des Datenstroms.</param>
        public void StopFilter( ushort streamIdentifier )
        {
            // Load it
            Filter filter;
            if (!m_Filters.TryGetValue( streamIdentifier, out filter ))
                return;

            // Remove
            using (filter)
                m_Filters.Remove( streamIdentifier );
        }

        /// <summary>
        /// Beendet den Emfpang.
        /// </summary>
        public void StopFilters()
        {
            // Load
            var filters = m_Filters.Values.ToArray();

            // Reset
            m_Filters.Clear();

            // Process
            foreach (var filter in filters)
                try
                {
                    // Process
                    filter.Dispose();
                }
                catch
                {
                    // Ignore any error
                }
        }

        /// <summary>
        /// Wählt eine Quellgruppe an.
        /// </summary>
        /// <param name="group">Die Quellgruppe.</param>
        /// <param name="location">Der Ursprung zur Quellgruppe.</param>
        public void Tune( SourceGroup group, GroupLocation location )
        {
            // Attach to hardware
            Open();

            // Invalidate all filters
            StopFilters();

            // Send request
            m_Frontend.Tune( group, location );
        }

        /// <summary>
        /// Reaktiviert das zugehörige Windows Gerät.
        /// </summary>
        public void WakeUp()
        {
            // Process
            if (!string.IsNullOrEmpty( m_WakeupDeviceInstance ))
                MediaDevice.WakeUpInstance( m_WakeupDeviceInstance );
            else if (!string.IsNullOrEmpty( m_WakeupDevice ))
                MediaDevice.WakeUp( m_WakeupDevice );
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung des Gerätes endgültig.
        /// </summary>
        public void Dispose()
        {
            // Stop all filters
            StopFilters();

            // Forward to all
            using (m_CI)
                m_CI = null;
            using (m_Frontend)
                m_Frontend = null;
            using (m_Board)
                m_Board = null;
            using (m_DeviceManager)
                m_DeviceManager = null;
        }

        #endregion
    }
}
