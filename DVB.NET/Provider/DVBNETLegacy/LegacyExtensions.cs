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

        #endregion

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
    }
}
