extern alias oldVersion;

using System;

using legacy = oldVersion::JMS.DVB;


namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Einige Erweiterungsmethoden für die einfachere Unterstützung des Zugriffs
    /// auf DVB.NET 3.5 (oder früher) Abstraktionen.
    /// </summary>
    public static class LegacyExtensions
    {
        #region DVB-S(2)

        /// <summary>
        /// Wandelt einen DVB.NET 3.9ff Ursprung in eine DiSEqC Ansteuerung um.
        /// </summary>
        /// <param name="location">Die 3.9ff Notation.</param>
        /// <returns>Die zugehörige 3.5 (oder früher) Notation.</returns>
        /// <exception cref="ArgumentException">Es wurde eine ungültige DiSEqC Notation verwendet.</exception>
        public static legacy.Satellite.DiSEqC ToLegacy( this SatelliteLocation location )
        {
            // None
            if (null == location)
                return null;

            // Create it
            legacy.Satellite.DiSEqC settings;

            // Check types
            switch (location.LNB)
            {
                case DiSEqCLocations.None: settings = new legacy.Satellite.DiSEqCNone(); break;
                case DiSEqCLocations.BurstOn: settings = new legacy.Satellite.DiSEqCSimple { Position = true }; break;
                case DiSEqCLocations.BurstOff: settings = new legacy.Satellite.DiSEqCSimple { Position = false }; break;
                case DiSEqCLocations.DiSEqC1: settings = new legacy.Satellite.DiSEqCMulti { AlternatePosition = false, AlternateOption = false }; break;
                case DiSEqCLocations.DiSEqC2: settings = new legacy.Satellite.DiSEqCMulti { AlternatePosition = true, AlternateOption = false }; break;
                case DiSEqCLocations.DiSEqC3: settings = new legacy.Satellite.DiSEqCMulti { AlternatePosition = false, AlternateOption = true }; break;
                case DiSEqCLocations.DiSEqC4: settings = new legacy.Satellite.DiSEqCMulti { AlternatePosition = true, AlternateOption = true }; break;
                default: throw new ArgumentException( location.LNB.ToString(), "location.LNB" );
            }

            // Copy parameters
            settings.Switch = location.SwitchFrequency;
            settings.UsePower = location.UsePower;
            settings.LOF1 = location.Frequency1;
            settings.LOF2 = location.Frequency2;

            // Report
            return settings;
        }

        /// <summary>
        /// Erzeugt einen DVB-S Ursprung aus einer DiSEqC Konfiguration.
        /// </summary>
        /// <param name="diseqc">Die existierende Konfiguration.</param>
        /// <returns>Der gewüpnschte Ursprung.</returns>
        public static SatelliteLocation FromLegacy( this legacy.Satellite.DiSEqC diseqc )
        {
            // Create location
            SatelliteLocation location = new SatelliteLocation { LNB = DiSEqCLocations.None };

            // Load core settings
            if (null == diseqc)
            {
                // Default common data
                location.Frequency1 = legacy.Satellite.DiSEqC.DefaultSettings.LOF1;
                location.Frequency2 = legacy.Satellite.DiSEqC.DefaultSettings.LOF2;
                location.SwitchFrequency = legacy.Satellite.DiSEqC.DefaultSettings.Switch;
                location.UsePower = legacy.Satellite.DiSEqC.DefaultSettings.UsePower;
            }
            else
            {
                // Copy common data
                location.Frequency1 = diseqc.LOF1;
                location.Frequency2 = diseqc.LOF2;
                location.SwitchFrequency = diseqc.Switch;
                location.UsePower = diseqc.UsePower;

                // Check for DiSEqC 1.0
                legacy.Satellite.DiSEqCMulti multi = diseqc as legacy.Satellite.DiSEqCMulti;
                if (null != multi)
                {
                    // Check mode
                    if (multi.AlternatePosition)
                        if (multi.AlternateOption)
                            location.LNB = DiSEqCLocations.DiSEqC4;
                        else
                            location.LNB = DiSEqCLocations.DiSEqC2;
                    else if (multi.AlternateOption)
                        location.LNB = DiSEqCLocations.DiSEqC3;
                    else
                        location.LNB = DiSEqCLocations.DiSEqC1;
                }
                else
                {
                    // Check for burst
                    legacy.Satellite.DiSEqCSimple tone = diseqc as legacy.Satellite.DiSEqCSimple;
                    if (null != tone)
                    {
                        // Check mode
                        if (tone.Position)
                            location.LNB = DiSEqCLocations.BurstOn;
                        else
                            location.LNB = DiSEqCLocations.BurstOff;
                    }
                }
            }

            // Report
            return location;
        }

        /// <summary>
        /// Ermittelt einen <see cref="legacy.Transponder"/> zu einer Quellgruppe.
        /// </summary>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        /// <returns>Der <see cref="legacy.Transponder"/> für die Nutzung in DVB.NET 3.5 (oder früher).</returns>
        public static legacy.Transponder ToLegacy( this SatelliteGroup group )
        {
            // None
            if (group == null)
                return null;

            // Create the channel
            var channel =
                new legacy.Satellite.SatelliteChannel
                {
                    SpectrumInversion = legacy.SpectrumInversion.Auto,
                    S2Modulation = group.UsesS2Modulation,
                    SymbolRate = group.SymbolRate,
                    Frequency = group.Frequency,
                    LNBIndex = 0,
                };

            // Power modes
            switch (group.Polarization)
            {
                case Polarizations.NotDefined: channel.Power = legacy.Satellite.PowerMode.Off; break;
                case Polarizations.Horizontal: channel.Power = legacy.Satellite.PowerMode.Horizontal; break;
                case Polarizations.Vertical: channel.Power = legacy.Satellite.PowerMode.Vertical; break;
                default: throw new ArgumentException( group.Polarization.ToString(), "group.Polarization" );
            }

            // Error control
            switch (group.InnerFEC)
            {
                case InnerForwardErrorCorrectionModes.NotDefined: channel.Viterbi = legacy.Satellite.Viterbi.Auto; break;
                case InnerForwardErrorCorrectionModes.Conv1_2: channel.Viterbi = legacy.Satellite.Viterbi.Rate1_2; break;
                case InnerForwardErrorCorrectionModes.Conv2_3: channel.Viterbi = legacy.Satellite.Viterbi.Rate2_3; break;
                case InnerForwardErrorCorrectionModes.Conv3_4: channel.Viterbi = legacy.Satellite.Viterbi.Rate3_4; break;
                case InnerForwardErrorCorrectionModes.Conv4_5: channel.Viterbi = legacy.Satellite.Viterbi.Rate4_5; break;
                case InnerForwardErrorCorrectionModes.Conv5_6: channel.Viterbi = legacy.Satellite.Viterbi.Rate5_6; break;
                case InnerForwardErrorCorrectionModes.Conv7_8: channel.Viterbi = legacy.Satellite.Viterbi.Rate7_8; break;
                case InnerForwardErrorCorrectionModes.Conv8_9: channel.Viterbi = legacy.Satellite.Viterbi.Rate8_9; break;
                case InnerForwardErrorCorrectionModes.Conv9_10: channel.Viterbi = legacy.Satellite.Viterbi.Rate9_10; break;
                case InnerForwardErrorCorrectionModes.Conv3_5: channel.Viterbi = legacy.Satellite.Viterbi.Rate3_5; break;
                default: throw new ArgumentException( group.InnerFEC.ToString(), "group.InnerFEC" );
            }

            // Add additional information needed for tuning
            channel.TuningExtensions =
                new legacy.Transponder.TuningExtension[]
                    {
                        new legacy.Transponder.TuningExtension { Name = "Modulation", Value = (int)group.Modulation },
                        new legacy.Transponder.TuningExtension { Name = "S2RollOff", Value = (int)group.RollOff }
                    };

            // Report
            return channel;
        }

        /// <summary>
        /// Wandelt eine DVB-S Quellgruppe (Transponder) in die neue Repräsentation um.
        /// </summary>
        /// <param name="channel">Die alte Repräsentation.</param>
        /// <returns>Die zugehörige neue Repräsentation.</returns>
        public static SatelliteGroup FromLegacy( this legacy.Satellite.SatelliteChannel channel )
        {
            // Create the group
            SatelliteGroup group = new SatelliteGroup();

            // Configure
            group.UsesS2Modulation = channel.S2Modulation;
            group.SymbolRate = channel.SymbolRate;
            group.Frequency = channel.Frequency;

            // Power modes
            switch (channel.Power)
            {
                case legacy.Satellite.PowerMode.Off: group.Polarization = Polarizations.NotDefined; break;
                case legacy.Satellite.PowerMode.Horizontal: group.Polarization = Polarizations.Horizontal; break;
                case legacy.Satellite.PowerMode.Vertical: group.Polarization = Polarizations.Vertical; break;
                default: group.Polarization = Polarizations.NotDefined; break;
            }

            // Error control
            switch (channel.Viterbi)
            {
                case legacy.Satellite.Viterbi.Auto: group.InnerFEC = InnerForwardErrorCorrectionModes.NotDefined; break;
                case legacy.Satellite.Viterbi.Rate1_2: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv1_2; break;
                case legacy.Satellite.Viterbi.Rate2_3: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv2_3; break;
                case legacy.Satellite.Viterbi.Rate3_4: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv3_4; break;
                case legacy.Satellite.Viterbi.Rate4_5: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv4_5; break;
                case legacy.Satellite.Viterbi.Rate5_6: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv5_6; break;
                case legacy.Satellite.Viterbi.Rate7_8: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv7_8; break;
                case legacy.Satellite.Viterbi.Rate8_9: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv8_9; break;
                case legacy.Satellite.Viterbi.Rate9_10: group.InnerFEC = InnerForwardErrorCorrectionModes.Conv9_10; break;
                default: group.InnerFEC = InnerForwardErrorCorrectionModes.NotDefined; break;
            }

            // Report
            return group;
        }

        #endregion

        #region DVB-C

        /// <summary>
        /// Ermittelt einen <see cref="legacy.Transponder"/> zu einer Quellgruppe.
        /// </summary>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        /// <returns>Der <see cref="legacy.Transponder"/> für die Nutzung in DVB.NET 3.5 (oder früher).</returns>
        public static legacy.Transponder ToLegacy( this CableGroup group )
        {
            // Create the transponder
            legacy.Cable.CableChannel channel = new legacy.Cable.CableChannel();

            // Fill statics
            channel.Frequency = group.Frequency;
            channel.SymbolRate = group.SymbolRate;
            channel.Bandwidth = group.Bandwidth.ToLegacy();

            // Spectrum inversion
            switch (group.SpectrumInversion)
            {
                case SpectrumInversions.On: channel.SpectrumInversion = legacy.SpectrumInversion.On; break;
                case SpectrumInversions.Off: channel.SpectrumInversion = legacy.SpectrumInversion.Off; break;
                case SpectrumInversions.Auto: channel.SpectrumInversion = legacy.SpectrumInversion.Auto; break;
                default: channel.SpectrumInversion = legacy.SpectrumInversion.Auto; break;
            }

            // Modulation
            switch (group.Modulation)
            {
                case CableModulations.QAM16: channel.QAM = legacy.Cable.Qam.Qam16; break;
                case CableModulations.QAM32: channel.QAM = legacy.Cable.Qam.Qam32; break;
                case CableModulations.QAM64: channel.QAM = legacy.Cable.Qam.Qam64; break;
                case CableModulations.QAM128: channel.QAM = legacy.Cable.Qam.Qam128; break;
                case CableModulations.QAM256: channel.QAM = legacy.Cable.Qam.Qam256; break;
                default: channel.QAM = legacy.Cable.Qam.Qam64; break;
            }

            // Report
            return channel;
        }

        /// <summary>
        /// Wandelt eine alte Repräsentation einer DVB-C Quellgruppe (Transponder) in die neue
        /// Notation um.
        /// </summary>
        /// <param name="channel">Die alte Repräsentation.</param>
        /// <returns>Die aktuelle Repräsentation.</returns>
        public static CableGroup FromLegacy( this legacy.Cable.CableChannel channel )
        {
            // Create the group
            CableGroup group = new CableGroup();

            // Fill statics
            group.Frequency = channel.Frequency;
            group.SymbolRate = channel.SymbolRate;
            group.Bandwidth = channel.Bandwidth.FromLegacy();

            // Spectrum inversion
            switch (channel.SpectrumInversion)
            {
                case legacy.SpectrumInversion.Auto: group.SpectrumInversion = SpectrumInversions.Auto; break;
                case legacy.SpectrumInversion.On: group.SpectrumInversion = SpectrumInversions.On; break;
                case legacy.SpectrumInversion.Off: group.SpectrumInversion = SpectrumInversions.Off; break;
                default: group.SpectrumInversion = SpectrumInversions.Auto; break;
            }

            // Modulation
            switch (channel.QAM)
            {
                case legacy.Cable.Qam.Qam16: group.Modulation = CableModulations.QAM16; break;
                case legacy.Cable.Qam.Qam32: group.Modulation = CableModulations.QAM32; break;
                case legacy.Cable.Qam.Qam64: group.Modulation = CableModulations.QAM64; break;
                case legacy.Cable.Qam.Qam128: group.Modulation = CableModulations.QAM128; break;
                case legacy.Cable.Qam.Qam256: group.Modulation = CableModulations.QAM256; break;
                default: group.Modulation = CableModulations.QAM64; break;
            }

            // Report
            return group;
        }

        #endregion

        #region DVB-T

        /// <summary>
        /// Ermittelt einen <see cref="legacy.Transponder"/> zu einer Quellgruppe.
        /// </summary>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        /// <returns>Der <see cref="legacy.Transponder"/> für die Nutzung in DVB.NET 3.5 (oder früher).</returns>
        public static legacy.Transponder ToLegacy( this TerrestrialGroup group )
        {
            // Create the channel
            return
                new legacy.Terrestrial.TerrestrialChannel
                    (
                        group.Frequency,
                        legacy.SpectrumInversion.Off,
                        false,
                        group.Bandwidth.ToLegacy()
                    );
        }

        /// <summary>
        /// Wandelt eine alte Repräsentation einer DVB-T Quellgruppe (Transponder) in die neue
        /// Notation um.
        /// </summary>
        /// <param name="channel">Die alte Repräsentation.</param>
        /// <returns>Die aktuelle Repräsentation.</returns>
        public static TerrestrialGroup FromLegacy( this legacy.Terrestrial.TerrestrialChannel channel )
        {
            // Create the group
            return
                new TerrestrialGroup
                {
                    Frequency = channel.Frequency,
                    Bandwidth = channel.Bandwidth.FromLegacy()
                };
        }

        #endregion

        /// <summary>
        /// Wandelt eine Quellrepräsentation in die neue Darstellung um.
        /// </summary>
        /// <param name="station">Die alte Darstellung als Sender.</param>
        /// <returns>Die neue Darstellung als erweiterte Quelle.</returns>
        public static Station FromLegacy( this legacy.Station station )
        {
            // None
            if (null == station)
                return null;

            // Create new
            Station newStation =
                new Station
                    {
                        SourceType = (0 == station.VideoPID) ? SourceTypes.Radio : SourceTypes.TV,
                        TransportStream = station.TransportStreamIdentifier,
                        Network = station.NetworkIdentifier,
                        Service = station.ServiceIdentifier,
                        Provider = station.TransponderName,
                        IsEncrypted = station.Encrypted,
                        Name = station.Name
                    };

            // Check for service
            if (newStation.IsService = (0xffff == newStation.TransportStream))
            {
                // Correct
                newStation.TransportStream = 0;
                newStation.Network = 0;
            }

            // Done
            return newStation;
        }

        /// <summary>
        /// Wandelt die Bandbreitenaufzählung von der neuen in die alte Darstellung um.
        /// </summary>
        /// <param name="bandwidth">Die aktuell verwendete Darstellung der Bandbreite.</param>
        /// <returns>Die frühere Repräsentation.</returns>
        private static legacy.BandwidthType ToLegacy( this Bandwidths bandwidth )
        {
            // Check supported modes
            switch (bandwidth)
            {
                case Bandwidths.Six: return legacy.BandwidthType.Six;
                case Bandwidths.Seven: return legacy.BandwidthType.Seven;
                case Bandwidths.Eight: return legacy.BandwidthType.Eight;
                case Bandwidths.NotDefined: return legacy.BandwidthType.None;
            }

            // Fall back
            return legacy.BandwidthType.Auto;
        }

        /// <summary>
        /// Wandelt die Bandbreitenaufzählung von der alten in die neue Darstellung um.
        /// </summary>
        /// <param name="bandwidth">Die frühere Repräsentation.</param>
        /// <returns>Die aktuell verwendete Darstellung der Bandbreite.</returns>
        private static Bandwidths FromLegacy( this legacy.BandwidthType bandwidth )
        {
            // Check supported modes
            switch (bandwidth)
            {
                case legacy.BandwidthType.Auto: return Bandwidths.NotDefined;
                case legacy.BandwidthType.Six: return Bandwidths.Six;
                case legacy.BandwidthType.Seven: return Bandwidths.Seven;
                case legacy.BandwidthType.Eight: return Bandwidths.Eight;
                case legacy.BandwidthType.None: return Bandwidths.NotDefined;
            }

            // Fallback
            return Bandwidths.NotDefined;
        }
    }
}
