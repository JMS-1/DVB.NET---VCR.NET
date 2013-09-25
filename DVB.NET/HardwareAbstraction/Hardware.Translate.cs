using System;
using System.Reflection;
using System.Collections.Generic;


namespace JMS.DVB
{
    partial class Hardware
    {
        /// <summary>
        /// Der Name der Bibliothek, in der die Abstraktionsimplementierungen vor DVB.NET 4.0 eingebettet
        /// sind.
        /// </summary>
        private const string LegacyAssemblyName = "JMS.DVB.Provider.Legacy";

        /// <summary>
        /// DiSEqC Steuering für die neuere Nova-S Linie.
        /// </summary>
        private const string HauppaugeNovaSDiSEqC = "JMS.DVB.Provider.NovaS2.NovaDiSEqC, JMS.DVB.Provider.NovaS2";

        /// <summary>
        /// DVB-S2 Anwahl für die Nova-HD-S2.
        /// </summary>
        private const string HauppaugeNovaS2Modulation = "JMS.DVB.Provider.NovaS2.NovaHDS2Modulation, JMS.DVB.Provider.NovaS2";

        /// <summary>
        /// Signalinformationen für die Nova-HD-S2.
        /// </summary>
        private const string HauppaugeNovaS2Signal = "JMS.DVB.Provider.NovaS2.NovaHDS2Signal, JMS.DVB.Provider.NovaS2";

        /// <summary>
        /// Die BDA de facto Standardimplementierung für DiSEqC Ansteuerungen.
        /// </summary>
        private const string DiSEqCByFrequencyFilter = "JMS.DVB.StandardActions.ByFrequencyFilter, JMS.DVB.HardwareAbstraction";

        /// <summary>
        /// Beschreibt die Abbildung der Abstraktionsklassen vor DVB.NET 4.0 auf die aktuelle Implementierung.
        /// </summary>
        private static readonly Dictionary<string, Type> s_LegacyMapping = new Dictionary<string, Type>
            {
                { "JMS.DVB.Provider.Legacy.DVBSLegacy", typeof(StandardSatelliteHardware) },
                { "JMS.DVB.Provider.Legacy.DVBCLegacy", typeof(StandardCableHardware) },
                { "JMS.DVB.Provider.Legacy.DVBTLegacy", typeof(StandardTerrestrialHardware) },
            };

        /// <summary>
        /// Methoden zur Umsetzung alter Konfigurationen in die aktuellen.
        /// </summary>
        private static readonly Dictionary<string, Func<Profile, Type, Hardware>> s_Translators = new Dictionary<string, Func<Profile, Type, Hardware>>
            {
                { "JMS.DVB.Provider.BDA.Provider, JMS.DVB.Provider.BDA", StandardTranslation },
                { "JMS.DVB.Provider.BDA.ProviderWithStandardDiSEqC, JMS.DVB.Provider.BDA", StandardSatelliteTranslationWithDiSEqC },
                { "JMS.DVB.Provider.BDA.NovaSE2Provider, JMS.DVB.Provider.BDA", TranslateNova },
            };

        /// <summary>
        /// Alle bekannten DVB.NET 3.9 Sonderimplementierungen für eine DiSEqC Ansteuerung.
        /// </summary>
        private static readonly Dictionary<string, string> s_KnownDiSEqCProviders = new Dictionary<string, string>
            {
                { "JMS.DVB.Provider.BDA.DiSEqCProvider.ByFrequencyFilter, JMS.DVB.Provider.BDA", DiSEqCByFrequencyFilter },
                { "JMS.DVB.Provider.BDA.DiSEqCProvider.NovaSPlus, JMS.DVB.Provider.BDA", HauppaugeNovaSDiSEqC },
                { "JMS.DVB.Provider.NovaS2.NovaHDS2, JMS.DVB.Provider.NovaS2", HauppaugeNovaSDiSEqC },
                { "JMS.DVB.Provider.TTBudget.NativeDiSEqC, JMS.DVB.Provider.TTBudget", null },
                { "JMS.DVB.Provider.KNC.DiSEqCByCapture, JMS.DVB.Provider.KNC", null },
            };

        /// <summary>
        /// Alle bekannten DVB.NET 3.9 Sonderimplementierungen für die Entschüsselung über CI/CAM.
        /// </summary>
        private static readonly Dictionary<string, string> s_KnownPayTVProviders = new Dictionary<string, string>
            {
                { "JMS.DVB.Provider.FireDTV.CIControl, JMS.DVB.Provider.FireDTV", null },
                { "JMS.DVB.Provider.TTBudget.NativeCI, JMS.DVB.Provider.TTBudget", null },
            };

        /// <summary>
        /// Die Parameter des Geräteprofils zum Zeitpunkt der Erzeugung der Abstraktion.
        /// </summary>
        internal List<ProfileParameter> EffectiveProfileParameters { get; private set; }

        /// <summary>
        /// Die Geräteaspekte zum Zeitpunkt der Erzeugung der Abstraktion.
        /// </summary>
        internal List<DeviceAspect> EffectiveDeviceAspects { get; private set; }

        /// <summary>
        /// Die Informationen zur Aktionsliste zum Zeitpunkt der Erzeugung der Abstraktion.
        /// </summary>
        internal List<PipelineItem> EffectivePipeline { get; private set; }

        /// <summary>
        /// Ermittelt alle Einträge der Aktionsliste.
        /// </summary>
        protected IEnumerable<PipelineItem> Pipeline
        {
            get
            {
                // Report
                return EffectivePipeline;
            }
        }

        /// <summary>
        /// Wandelt die Parameter von der alten Darstellung vor DVB.NET 4.0 in die aktuelle Darstellung um.
        /// </summary>
        /// <returns>Gesetzt, wenn die Umwandlung erfolgreich war.</returns>
        private bool Translate()
        {
            // Reset copy from profile - will reprocess
            EffectivePipeline.Clear();
            EffectiveDeviceAspects.Clear();

            // Translate parameters
            for (int i = EffectiveProfileParameters.Count; i-- > 0; )
            {
                // Attach to parameter
                var setting = EffectiveProfileParameters[i];
                if (setting == null)
                    continue;
                if (string.IsNullOrEmpty( setting.Name ))
                    continue;

                // Dispatch
                switch (setting.Name)
                {
                    case "CaptureMoniker": EffectiveDeviceAspects.Add( new DeviceAspect { Aspekt = Aspect_CaptureMoniker, Value = setting.Value } ); break;
                    case "TunerMoniker": EffectiveDeviceAspects.Add( new DeviceAspect { Aspekt = Aspect_TunerMoniker, Value = setting.Value } ); break;
                    case "Capture": EffectiveDeviceAspects.Add( new DeviceAspect { Aspekt = Aspect_CaptureName, Value = setting.Value } ); break;
                    case "Tuner": EffectiveDeviceAspects.Add( new DeviceAspect { Aspekt = Aspect_TunerName, Value = setting.Value } ); break;
                    case "Type": break;
                    case "DiSEqCProvider":
                        {
                            // Check legacy providers
                            if (!string.IsNullOrEmpty( setting.Value ))
                            {
                                // Load
                                string mapping;
                                if (!s_KnownDiSEqCProviders.TryGetValue( setting.Value, out mapping ))
                                    return false;

                                // Remember DiSEqC
                                EffectivePipeline.Add( new PipelineItem { SupportedOperations = PipelineTypes.DiSEqC, ComponentType = mapping ?? setting.Value } );

                                // Hauppauge special - DiSEqC implies DVB-S2 control which is separated starting with DVB.NET 4.0
                                if (setting.Value.Equals( "JMS.DVB.Provider.NovaS2.NovaHDS2, JMS.DVB.Provider.NovaS2" ))
                                    EffectivePipeline.Add( new PipelineItem { SupportedOperations = PipelineTypes.DVBS2, ComponentType = HauppaugeNovaS2Modulation } );
                            }

                            // Remove from parameter
                            break;
                        }
                    case "PayTVProvider":
                        {
                            // Check legacy providers
                            if (!string.IsNullOrEmpty( setting.Value ))
                            {
                                // Load
                                string mapping;
                                if (!s_KnownPayTVProviders.TryGetValue( setting.Value, out mapping ))
                                    return false;

                                // Remember CI/CAM
                                EffectivePipeline.Add( new PipelineItem { SupportedOperations = PipelineTypes.CICAM, ComponentType = mapping ?? setting.Value } );
                            }

                            // Remove from parameter
                            break;
                        }
                    default: continue;
                }

                // Eaten up
                EffectiveProfileParameters.RemoveAt( i );
            }

            // Refill from original settings
            foreach (var item in Profile.Pipeline)
                if (item.SupportedOperations != PipelineTypes.SignalInformation)
                    return false;
                else if (string.IsNullOrEmpty( item.ComponentType ))
                    return false;
                else
                    switch (item.ComponentType)
                    {
                        case "JMS.DVB.Provider.NovaS2.SignalExtension, JMS.DVB.Provider.NovaS2": EffectivePipeline.Add( new PipelineItem { SupportedOperations = PipelineTypes.SignalInformation, ComponentType = HauppaugeNovaS2Signal } ); break;
                        case "JMS.DVB.Provider.TTBudget.SignalExtension, JMS.DVB.Provider.TTBudget": EffectivePipeline.Add( new PipelineItem { SupportedOperations = PipelineTypes.SignalInformation, ComponentType = item.ComponentType } ); break;
                        default: return false;
                    }

            // Did it
            return true;
        }

        /// <summary>
        /// Führt eine Standardumwandlung für den Satellitenempfang mit DiSEqC aus.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="hardwareType">Die konkrete Art der DVB.NET 4.0ff Implementierung.</param>
        /// <returns>Die gewünschte Abstraktion.</returns>
        private static Hardware StandardSatelliteTranslationWithDiSEqC( Profile profile, Type hardwareType )
        {
            // Forward
            var hardware = StandardTranslation( profile, hardwareType );
            if (hardware == null)
                return null;

            // Add item to pipeline
            hardware.EffectivePipeline.Add( new PipelineItem { SupportedOperations = PipelineTypes.DiSEqC, ComponentType = DiSEqCByFrequencyFilter } );

            // Report
            return hardware;
        }

        /// <summary>
        /// Wandelt die Abstraktion für die Hauppauge Nova-S Plus und Nova-SE2 um.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="hardwareType">Die konkrete Art der DVB.NET 4.0ff Implementierung.</param>
        /// <returns>Die gewünschte Abstraktion.</returns>
        private static Hardware TranslateNova( Profile profile, Type hardwareType )
        {
            // Forward
            var hardware = StandardTranslation( profile, hardwareType );
            if (hardware == null)
                return null;

            // Default names
            hardware.EffectiveDeviceAspects.Add( new DeviceAspect { Aspekt = Aspect_TunerName, Value = "Hauppauge WinTV 88x DVB-S Tuner/Demod" } );
            hardware.EffectiveDeviceAspects.Add( new DeviceAspect { Aspekt = Aspect_CaptureName, Value = "Hauppauge WinTV 88x TS Capture" } );

            // Add to pipeline
            hardware.EffectivePipeline.Add( new PipelineItem { SupportedOperations = PipelineTypes.DiSEqC, ComponentType = HauppaugeNovaSDiSEqC } );

            // Report
            return hardware;
        }

        /// <summary>
        /// Führt eine Standardumwandlung aus.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="hardwareType">Die konkrete Art der DVB.NET 4.0ff Implementierung.</param>
        /// <returns>Die gewünschte Abstraktion.</returns>
        private static Hardware StandardTranslation( Profile profile, Type hardwareType )
        {
            // Result
            Hardware result = null;

            // Create the hardware
            var hardware = (Hardware) Activator.CreateInstance( hardwareType, profile );
            try
            {
                // Report
                if (hardware.Translate())
                {
                    // Reload restrictions - can only use full featured algorithm when fully ported
                    hardware.Restrictions = new HardwareRestriction { ProvidesSignalInformation = true };

                    // Remember
                    result = hardware;
                }
            }
            catch
            {
            }

            // Cleanup
            if (result == null)
                hardware.Dispose();

            // Failed
            return result;
        }

        /// <summary>
        /// Erzeugt eine neue Instanz der Abstraktion zu einem Geräteprofil.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="migrateOnly">Gesetzt, wenn nur eine Migration der Einstellungen vorgenommen werden soll.</param>
        /// <returns>Die neu erzeugte Instanz, die mit <see cref="IDisposable.Dispose"/>
        /// freigegeben werden muss.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil angegeben.</exception>
        /// <exception cref="NotSupportedException">Das Geräteprofil verwendet die nicht mehr unterstützten BDA Abstraktion
        /// vor DVB.NET 4.0.</exception>
        internal static Hardware Create( Profile profile, bool migrateOnly )
        {
            // Validate
            if (profile == null)
                throw new ArgumentNullException( "profile" );

            // For exception translation
            try
            {
                // Load the primary type
                var primaryType = Type.GetType( profile.HardwareType, true );

                // See we we can translate
                Type translationType;
                if (LegacyAssemblyName.Equals( primaryType.Assembly.GetName().Name ))
                    if (s_LegacyMapping.TryGetValue( primaryType.FullName, out translationType ))
                    {
                        // Load the provider mapping
                        var provider = profile.GetDeviceAspect( null );
                        if (!string.IsNullOrEmpty( provider ))
                        {
                            // Load the translatior
                            Func<Profile, Type, Hardware> translator;
                            if (s_Translators.TryGetValue( provider, out translator ))
                            {
                                // Try translate
                                var hardware = translator( profile, translationType );
                                if (hardware == null)
                                    if (migrateOnly)
                                        return null;
                                    else
                                        throw new NotSupportedException( string.Format( Properties.Resources.Exception_NoLongerSupported, profile ) );
                                else
                                    return hardware;
                            }
                        }
                    }

                // Create and report
                if (migrateOnly)
                    return null;
                else
                    return (Hardware) Activator.CreateInstance( primaryType, profile );
            }
            catch (TargetInvocationException e)
            {
                // Forward
                if (migrateOnly)
                    return null;
                else
                    throw (null == e.InnerException) ? e : e.InnerException;
            }
        }


        /// <summary>
        /// Ermittelt einen Parameter des Geräteprofils.
        /// </summary>
        /// <param name="name">Der Name des Parameters.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Name angegeben.</exception>
        protected string GetParameter( string name )
        {
            // Validate
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );
            else
                return EffectiveProfileParameters.GetParameter( name );
        }

        /// <summary>
        /// Ermittelt einen Geräteparameter.
        /// </summary>
        /// <param name="name">Der Name des Parameters.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Name angegeben.</exception>
        protected string GetDeviceAspect( string name )
        {
            // Validate
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );
            else
                return EffectiveDeviceAspects.GetDeviceAspect( name );
        }
    }
}
