using System;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Enumerators;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.Editors;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt die Standardanbindung für DVB.NET 4.0ff Geräte. Die Standardansteuerung setzt 
    /// auf die Microsoft <i>Broadcast Driver Architecture (BDA)</i> Technologie.
    /// </summary>
    /// <typeparam name="TProfileType">Die genaue Art des Geräteprofils.</typeparam>
    /// <typeparam name="TLocationType">Die Art des Ursprungs.</typeparam>
    /// <typeparam name="TSourceGroupType">Die Art der Quellgruppe.</typeparam>
    [HardwareEditor( typeof( StandardHardwareEditor ) )]
    public abstract class StandardHardware<TProfileType, TLocationType, TSourceGroupType> : Hardware<TProfileType, TLocationType, TSourceGroupType>
        where TProfileType : Profile
        where TLocationType : GroupLocation
        where TSourceGroupType : SourceGroup
    {
        /// <summary>
        /// Steuert den Zugriff auf das Gerät über eine DirectShow Graphen.
        /// </summary>
        private DataGraph m_receiver;

        /// <summary>
        /// Gesetzt, wenn die Entschlüsselung deaktiviert werden soll.
        /// </summary>
        private bool m_disableEncryption;

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        public StandardHardware( TProfileType profile )
            : base( profile )
        {
        }

        /// <summary>
        /// Meldet die Art des DVB Empfangs.
        /// </summary>
        protected abstract DVBSystemType DVBType { get; }

        /// <summary>
        /// Ermittelt die Einschränkung eines Gerätes.
        /// </summary>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <returns>Die aktiven Einschränlungen.</returns>
        public static HardwareRestriction GetRestriction( Profile profile )
        {
            // Report default
            if (profile == null)
                return null;
            else
                return new HardwareRestriction();
        }

        /// <summary>
        /// Ermittelt die Beschreibung eines Gerätefilters.
        /// </summary>
        /// <param name="displayAspectName">Der Name des Parameters mit dem Anzeigenamen.</param>
        /// <param name="monikerAspectName">Der Name des Parameters mit dem eindeutigen Namen.</param>
        /// <param name="tuner">Gesetzt, wenn nach dem Empfängerfilter gesucht werden soll.</param>
        /// <returns>Die gewünschten Informationen, soweit verfügbar.</returns>
        private FilterInformation FindFilter( string displayAspectName, string monikerAspectName, bool tuner )
        {
            // Load data
            var display = GetDeviceAspect( displayAspectName );
            var moniker = GetDeviceAspect( monikerAspectName );

            // Check mode
            if (string.IsNullOrEmpty( display ))
                if (string.IsNullOrEmpty( moniker ))
                    return null;

            // Ask enumerator
            return (tuner ? DeviceAndFilterInformations.Cache.TunerFilters : DeviceAndFilterInformations.Cache.CaptureFilters).FindFilter( display, moniker );
        }

        /// <summary>
        /// Beendet den Empfang eines Datenstroms.
        /// </summary>
        /// <param name="stream">Der betroffene Datenstrom.</param>
        protected override void OnStop( Hardware.StreamInformation stream )
        {
            // Validate
            if (stream == null)
                throw new ArgumentNullException( "stream" );

            // Not started
            if (m_receiver == null)
                return;

            // Not of interest to us
            if (stream.Consumer == null)
                return;

            // Just forward
            m_receiver.TransportStreamAnalyser.DataManager.StopFilter( stream.Identifier );
        }

        /// <summary>
        /// Beginnt den Empfang eines Datenstroms.
        /// </summary>
        /// <param name="stream">Der gewünschte Datenstrom.</param>
        protected override void OnStart( Hardware.StreamInformation stream )
        {
            // Validate
            if (stream == null)
                throw new ArgumentNullException( "stream" );

            // Not started
            if (m_receiver == null)
                return;

            // Not of interest to us
            if (stream.Consumer == null)
                return;

            // Attach to the data manager
            var manager = m_receiver.TransportStreamAnalyser.DataManager;

            // Check mode
            var type = stream.StreamType;
            bool siTableMode;
            switch (type)
            {
                case StreamTypes.StandardTable: siTableMode = true; break;
                case StreamTypes.ExtendedTable: siTableMode = true; break;
                case StreamTypes.UnknownPES: siTableMode = false; break;
                case StreamTypes.VideoText: siTableMode = false; break;
                case StreamTypes.SubTitle: siTableMode = false; break;
                case StreamTypes.Audio: siTableMode = false; break;
                case StreamTypes.Video: siTableMode = false; break;
                default: throw new NotSupportedException( type.ToString() );
            }

            // Register callback
            manager.AddFilter( stream.Identifier, siTableMode, p => stream.Consumer( p, 0, p.Length ) );

            // Start filter at once
            manager.StartFilter( stream.Identifier );
        }

        /// <summary>
        /// Meldet Informationen zum aktuellen Signal.
        /// </summary>
        /// <param name="signal">Die zu befüllenden Informationen.</param>
        protected override void OnGetSignal( SignalInformation signal )
        {
            // Only if started
            if (m_receiver != null)
            {
                // Try to fill
                var status = m_receiver.SignalStatus;
                if (status != null)
                {
                    // Copy over
                    signal.Strength = status.Strength;
                    signal.Quality = status.Quality;
                    signal.Locked = status.Locked;
                }
            }

            // Forward to base
            base.OnGetSignal( signal );
        }

        /// <summary>
        /// Meldet, ob der Pipelinemechanismus von DVB.NET 3.9 verwendet werden soll.
        /// </summary>
        protected override bool UsesLegacyPipeline
        {
            get
            {
                // No, this is our pipeline stuff
                return false;
            }
        }

        /// <summary>
        /// Wählt eine neue Quellgruppe aus.
        /// </summary>
        /// <param name="location">Der Ursprung der Quellgruppe.</param>
        /// <param name="group">Die eigentliche Quellgruppe.</param>
        protected override void OnSelect( TLocationType location, TSourceGroupType group )
        {
            // Create once
            if (m_receiver == null)
            {
                // Create receiver
                var receiver = new DataGraph();
                try
                {
                    // Load device data
                    receiver.CaptureInformation = FindFilter( Aspect_CaptureName, Aspect_CaptureMoniker, false );
                    receiver.TunerInformation = FindFilter( Aspect_TunerName, Aspect_TunerMoniker, true );
                    receiver.DisableCIResetOnTuneFailure = m_disableEncryption;
                    receiver.DVBType = DVBType;

                    // Load optional data           
                    int value;
                    if (int.TryParse( GetParameter( BDAEnvironment.MiniumPATCountName ), out value ))
                        if (value >= 0)
                            receiver.Configuration.MinimumPATCount = value;
                    if (int.TryParse( GetParameter( BDAEnvironment.MinimumPATCountWaitName ), out value ))
                        if (value >= 0)
                            receiver.Configuration.MinimumPATCountWaitTime = value;

                    // Configure pipeline
                    foreach (var item in Pipeline)
                        item.CreateExtension<IPipelineExtension>().Install( receiver, Profile, item.SupportedOperations );

                    // Time to create the graph
                    receiver.Create( location, group );

                    // Activate it
                    receiver.Start();
                }
                catch
                {
                    // Cleanup
                    receiver.Dispose();

                    // Forward
                    throw;
                }

                // Remember
                m_receiver = receiver;
            }

            // Blind forward
            m_receiver.Tune( location, group );
        }

        /// <summary>
        /// Deaktiviert den Datenempfang unmittelbar bevor eine neue Quellgruppe aktiviert wird.
        /// </summary>
        /// <returns>Meldet, dass die Operation erfolgreich ausgeführt wurde.</returns>
        protected override bool OnStopAll()
        {
            // Not started
            if (m_receiver == null)
                return false;

            // Forward
            m_receiver.TransportStreamAnalyser.DataManager.RemoveAllFilters();

            // Did it
            return true;
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        protected override void OnDispose()
        {
            // Forget graph
            using (m_receiver)
                m_receiver = null;
        }

        /// <summary>
        /// Prüft, ob eine bestimmte Quellgruppe verwendet werden kann.
        /// </summary>
        /// <param name="group">Die zu prüfende Quellgruppe.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe verwendet werden kann.</returns>
        protected override bool OnCanHandle( TSourceGroupType group )
        {
            // Let's try
            return true;
        }

        /// <summary>
        /// Fordert zur Entschlüsselung auf.
        /// </summary>
        /// <param name="sources">Eine Liste von zu entschlüsselnden Quellen.</param>
        public override void Decrypt( params SourceIdentifier[] sources )
        {
            // Forward
            if (m_receiver != null)
                m_receiver.Decrypt( sources );
        }

        /// <summary>
        /// Bereitet einen Sendersuchlauf vor.
        /// </summary>
        public override void PrepareSourceScan()
        {
            // Check mode
            bool enableDecryptionDuringScan;
            if (bool.TryParse( GetParameter( Parameter_EnableCIDuringScan ), out enableDecryptionDuringScan ))
                if (enableDecryptionDuringScan)
                    return;

            // Disable encryption to speed up source scan
            m_disableEncryption = true;
        }

        /// <summary>
        /// Reaktiviert den Treiber über den Neustart eines Gerätes.
        /// </summary>
        public override void ResetWakeupDevice()
        {
            // Check mode
            bool enabled;
            if (!bool.TryParse( GetParameter( Parameter_EnableWakeup ), out enabled ))
                return;
            if (!enabled)
                return;

            // Try explicitly configured moniker
            var deviceName = GetParameter( Parameter_WakeupMoniker );
            if (!string.IsNullOrEmpty( deviceName ))
            {
                // Do it
                MediaDevice.WakeUpInstance( deviceName );
                return;
            }

            // Try explicitly configured display name
            deviceName = GetParameter( Parameter_WakeupDevice );
            if (!string.IsNullOrEmpty( deviceName ))
            {
                // Do it
                MediaDevice.WakeUp( deviceName );
                return;
            }

            // Attach to tuner
            var tuner = FindFilter( Aspect_TunerName, Aspect_TunerMoniker, true );
            if (tuner == null)
                return;

            // Attach to the device
            var device = tuner.Hardware;
            if (device == null)
                return;

            // Do it
            MediaDevice.WakeUpInstance( device.DevicePath );
        }
    }

    /// <summary>
    /// Beschreibt die Standardanbindung für DVB.NET 4.0ff DVB-S Geräte. Die Standardansteuerung setzt 
    /// auf die Microsoft <i>Broadcast Driver Architecture (BDA)</i> Technologie.
    /// </summary>
    public class StandardSatelliteHardware : StandardHardware<SatelliteProfile, SatelliteLocation, SatelliteGroup>
    {
        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        public StandardSatelliteHardware( SatelliteProfile profile )
            : base( profile )
        {
        }

        /// <summary>
        /// Meldet die Art des DVB Empfangs.
        /// </summary>
        protected override DVBSystemType DVBType { get { return DVBSystemType.Satellite; } }
    }

    /// <summary>
    /// Beschreibt die Standardanbindung für DVB.NET 4.0ff DVB-C Geräte. Die Standardansteuerung setzt 
    /// auf die Microsoft <i>Broadcast Driver Architecture (BDA)</i> Technologie.
    /// </summary>
    public class StandardCableHardware : StandardHardware<CableProfile, CableLocation, CableGroup>
    {
        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        public StandardCableHardware( CableProfile profile )
            : base( profile )
        {
        }

        /// <summary>
        /// Meldet die Art des DVB Empfangs.
        /// </summary>
        protected override DVBSystemType DVBType { get { return DVBSystemType.Cable; } }
    }

    /// <summary>
    /// Beschreibt die Standardanbindung für DVB.NET 4.0ff DVB-T Geräte. Die Standardansteuerung setzt 
    /// auf die Microsoft <i>Broadcast Driver Architecture (BDA)</i> Technologie.
    /// </summary>
    public class StandardTerrestrialHardware : StandardHardware<TerrestrialProfile, TerrestrialLocation, TerrestrialGroup>
    {
        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        public StandardTerrestrialHardware( TerrestrialProfile profile )
            : base( profile )
        {
        }

        /// <summary>
        /// Meldet die Art des DVB Empfangs.
        /// </summary>
        protected override DVBSystemType DVBType { get { return DVBSystemType.Terrestrial; } }
    }
}
