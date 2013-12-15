using System;
using System.Collections;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.Editors;
using JMS.TechnoTrend;


namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Diese Klasse vermittelt den Zugriff auf eine vorhandene DVB.NET Abstraktion vor
    /// Version 3.5.1.
    /// </summary>
    [HardwareEditor( typeof( LegacyEditor ) )]
    public abstract class LegacyHardware<P, L, G> : Hardware<P, L, G>
        where P : Profile
        where L : GroupLocation
        where G : SourceGroup
    {
        /// <summary>
        /// Eine Maske für die Tabellenfilterung der DVB Hardware.
        /// </summary>
        private static readonly byte[] m_TableMask = { 0x80 };

        /// <summary>
        /// Der Filter für die Standardtabellenkennungen (0x00 bis 0x7f).
        /// </summary>
        private static readonly byte[] m_StandardTables = { 0x00 };

        /// <summary>
        /// Der Filter für die Tabellenkennungen des Erweiterungsbereichs (0x80 bis 0xff).
        /// </summary>
        private static readonly byte[] m_CustomTables = { 0x80 };

        /// <summary>
        /// Das zugeordnete DVB.NET 3.5 (oder früher) Geräteprofil.
        /// </summary>
        protected DVB.IDeviceProvider LegacyDevice { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Vermittlungsinstanz.
        /// </summary>
        /// <param name="profile">Das zugeordnete Geräteprofil.</param>
        internal LegacyHardware( P profile )
            : base( profile )
        {
            // Create the device configuration
            var settings = new Hashtable();

            // Fill the configuration
            foreach (var parameter in profile.Parameters)
                if (!string.IsNullOrEmpty( parameter.Value ))
                    settings[parameter.Name] = parameter.Value;

            // Pre-set system type
            settings["Type"] = SystemType.ToString();

            // Find the primary aspect
            var aspect = profile.DeviceAspects.Find( a => string.IsNullOrEmpty( a.Aspekt ) );

            // Create the device
            LegacyDevice = (DVB.IDeviceProvider) Activator.CreateInstance( Type.GetType( aspect.Value, true ), settings );

            // Start it
            LegacyDevice.SetVideoAudio( 0, 0, 0 );
        }

        /// <summary>
        /// Ermittelt die Einschränkung eines Gerätes.
        /// </summary>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <returns>Die aktiven Einschränlungen.</returns>
        public static HardwareRestriction GetRestriction( Profile profile )
        {
            // No profile - no description
            if (profile == null)
                return null;

            // Get the driver
            var aspect = profile.DeviceAspects.Find( a => string.IsNullOrEmpty( a.Aspekt ) );
            if (aspect == null)
                return null;

            // None set
            if (string.IsNullOrEmpty( aspect.Value ))
                return null;

            // Be safe
            try
            {
                // Get the type
                var driver = Type.GetType( aspect.Value, false );
                if (driver == null)
                    return null;

                // Create 
                var restr = new HardwareRestriction();

                // Check mode
                if (typeof( TTPremium.TTDeviceProvider ).IsAssignableFrom( driver ))
                {
                    // Maximum number of streams is restricted
                    restr.ConsumerLimit = 8;

                    // But is able to provide signal information
                    restr.ProvidesSignalInformation = true;
                }
                else if (typeof( TTBudget.BudgetProvider ).IsAssignableFrom( driver ))
                {
                    // Maximum number of streams is restricted
                    restr.ConsumerLimit = 200;
                }

                // Report
                return restr;
            }
            catch
            {
                // In error
                return null;
            }
        }

        /// <summary>
        /// Meldet die Art des DVB Empfangs.
        /// </summary>
        protected abstract DVBSystemType SystemType { get; }

        /// <summary>
        /// Gibt alle mit dieser Instanz verbundenen Ressourcen frei.
        /// </summary>
        protected override void OnDispose()
        {
            // Check device handle
            using (LegacyDevice)
                LegacyDevice = null;
        }

        /// <summary>
        /// Konfiguriert den Empfang einer Quellgruppe.
        /// </summary>
        /// <param name="location">Optional der Ursprung der Gruppe.</param>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        protected override void OnSelect( L location, G group )
        {
            // Forward
            if (group != null)
                LegacyDevice.Tune( group, location );
        }

        /// <summary>
        /// Aktiviert den Datenempfang für einen bestimmten Datenstrom.
        /// </summary>
        /// <param name="stream">Die Informationen zum betroffenen Datenstrom.</param>
        /// <exception cref="ArgumentNullException">Es wurden keine Informationen angeben.</exception>
        protected override void OnStart( StreamInformation stream )
        {
            // Validate
            if (stream == null)
                throw new ArgumentNullException( "stream" );

            // Not of interest to us
            if (stream.Consumer == null)
                return;

            // Check type of stream
            if (stream.StreamType == StreamTypes.StandardTable)
            {
                // Table consumer - autostart included
                LegacyDevice.StartSectionFilter( stream.Identifier, p => stream.Consumer( p, 0, p.Length ), m_StandardTables, m_TableMask );
            }
            else if (stream.StreamType == StreamTypes.ExtendedTable)
            {
                // Table consumer - autostart included
                LegacyDevice.StartSectionFilter( stream.Identifier, p => stream.Consumer( p, 0, p.Length ), m_CustomTables, m_TableMask );
            }
            else
            {
                // Prepare exception mapping
                try
                {
                    // Check mode
                    switch (stream.StreamType)
                    {
                        case StreamTypes.Audio: LegacyDevice.RegisterPipingFilter( stream.Identifier, false, true, p => stream.Consumer( p, 0, p.Length ) ); break;
                        case StreamTypes.SubTitle: LegacyDevice.RegisterPipingFilter( stream.Identifier, false, true, p => stream.Consumer( p, 0, p.Length ) ); break;
                        case StreamTypes.UnknownPES: LegacyDevice.RegisterPipingFilter( stream.Identifier, true, false, p => stream.Consumer( p, 0, p.Length ) ); break;
                        case StreamTypes.Video: LegacyDevice.RegisterPipingFilter( stream.Identifier, true, false, p => stream.Consumer( p, 0, p.Length ) ); break;
                        case StreamTypes.VideoText: LegacyDevice.RegisterPipingFilter( stream.Identifier, false, true, p => stream.Consumer( p, 0, p.Length ) ); break;
                        default: throw new ArgumentException( stream.StreamType.ToString(), "stream.StreamType" );
                    }

                    // Must start filter explicitly
                    LegacyDevice.StartFilter( stream.Identifier );
                }
                catch (DVBException e)
                {
                    // Check mode
                    if (e.ErrorCode == DVBError.Resources)
                        throw new OutOfConsumersException( 1, 0 );
                    else
                        throw;
                }
            }
        }

        /// <summary>
        /// Deaktiviert den Datenempfang für einen bestimmten Datenstrom.
        /// </summary>
        /// <param name="stream">Die Informationen zum betroffenen Datenstrom.</param>
        /// <exception cref="ArgumentNullException">Es wurden keine Informationen angeben.</exception>
        protected override void OnStop( StreamInformation stream )
        {
            // Validate
            if (stream == null)
                throw new ArgumentNullException( "stream" );

            // Not of interest to us
            if (stream.Consumer != null)
                LegacyDevice.StopFilter( stream.Identifier );
        }

        /// <summary>
        /// Aktiviert die Entschlüsselung einer Liste von Quellen.
        /// </summary>
        /// <param name="sources">Die gewünschten Quellen.</param>
        public override void Decrypt( params SourceIdentifier[] sources )
        {
            // Check mode
            if ((sources == null) || (sources.Length < 1))
            {
                // Forward
                LegacyDevice.Decrypt( null );
            }
            else if (sources.Length > 1)
            {
                // Not allowed
                throw new ArgumentException( sources.Length.ToString(), "sources" );
            }
            else
            {
                // Attach to the source
                var source = sources[0];
                if (source == null)
                    throw new ArgumentNullException( "sources[0]" );

                // Process
                LegacyDevice.Decrypt( source.Service );
            }
        }

        /// <summary>
        /// Reinitialisiert das mit dieser Hardware verbundene Windows Gerät.
        /// </summary>
        public override void ResetWakeupDevice()
        {
            // Forward
            LegacyDevice.WakeUp();
        }

        /// <summary>
        /// Deaktiviert den Datenempfang unmittelbar bevor eine neue Quellgruppe aktiviert wird.
        /// </summary>
        /// <returns>Immer gesetzt, da DVB.NET 3.5 (oder früher) das Stoppen aller Filter unterstützt.</returns>
        protected override bool OnStopAll()
        {
            // Forward
            LegacyDevice.StopFilters();

            // Did it
            return true;
        }

        /// <summary>
        /// Ermittelt die aktuellen Daten zum empfangenen Signal.
        /// </summary>
        /// <param name="signal">Die vorbereitete Informationsstruktur.</param>
        protected override void OnGetSignal( SignalInformation signal )
        {
            // No device
            if (LegacyDevice == null)
                return;

            // Read it
            var status = LegacyDevice.SignalStatus;

            // None
            if (status == null)
                return;

            // Copy
            signal.Locked = status.Locked;
            signal.Strength = status.Strength;
            signal.Quality = status.Quality;
        }
    }
}
