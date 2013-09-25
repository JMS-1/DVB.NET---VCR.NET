using System;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess
{
    partial class DataGraph
    {
        partial class TuneToken
        {
            /// <summary>
            /// Die COM Klasse, die einen DVB-S Namensraum implementiert.
            /// </summary>
            private static readonly Guid DVBSTuningSpaceClassIdentifier = new Guid( "b64016f3-c9a2-4066-96f0-bd9563314726" );

            /// <summary>
            /// Die COM Klasse, die eine DVB-S Quellgruppe beschreibt.
            /// </summary>
            private static readonly Guid DVBSLocatorClassIdentifier = new Guid( "1df7d126-4050-47f0-a7cf-4c4ca9241333" );

            /// <summary>
            /// Bereitet die internen Strukturen für die Konfiguration einer Quellgruppe für den
            /// Satellitenempfang vor.
            /// </summary>
            /// <param name="initializer">Methode zur Übernahme der Parameter der Quellgruppe.</param>
            /// <returns>Ein neuer Namensraum.</returns>
            private IDVBTuningSpace2 PrepareSatellite( out Action<ILocator> initializer )
            {
                // Get native types
                var location = GroupLocation as SatelliteLocation;
                var group = SourceGroup as SatelliteGroup;

                // Create DiSEqC message
                if (location != null)
                    if (group != null)
                        DiSEqCMessage = StandardDiSEqC.FromSourceGroup( group, location );

                // Create tuning space
                var space = Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSTuningSpaceClassIdentifier ) );
                try
                {
                    // Change type
                    var tuningSpace = (IDVBSTuningSpace) space;

                    // Configure
                    tuningSpace.UniqueName = "DVBNET Satellite";
                    tuningSpace.FriendlyName = "DVB.NET Satellite Tuning Space";
                    tuningSpace.NetworkType = "{FA4B375A-45B4-4D45-8440-263957B11623}";
                    tuningSpace.SystemType = DVBSystemType.Satellite;

                    // Configure location
                    if (location != null)
                    {
                        // Configure
                        tuningSpace.LNBSwitch = checked( (int) location.SwitchFrequency );
                        tuningSpace.HighOscillator = checked( (int) location.Frequency2 );
                        tuningSpace.LowOscillator = checked( (int) location.Frequency1 );
                        tuningSpace.SpectralInversion = SpectralInversion.Automatic;
                        tuningSpace.FrequencyMapping = string.Empty;
                        tuningSpace.InputRange = string.Empty;
                        tuningSpace.NetworkID = -1;
                    }

                    // Create locator
                    var locator = Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSLocatorClassIdentifier ) );
                    try
                    {
                        // Configure group
                        if (group == null)
                            initializer = null;
                        else
                            initializer = clone =>
                                {
                                    // Change type
                                    var dvbs = (IDVBSLocator) clone;

                                    // Configure
                                    dvbs.Modulation = group.UsesS2Modulation ? ModulationType.VSB8 : ModulationType.QPSK;
                                    dvbs.SymbolRate = checked( (int) (group.SymbolRate / 1000) );
                                    dvbs.CarrierFrequency = checked( (int) group.Frequency );
                                    dvbs.InnerFECRate = BinaryConvolutionCodeRate.NotSet;
                                    dvbs.OuterFECRate = BinaryConvolutionCodeRate.NotSet;
                                    dvbs.SignalPolarisation = Polarisation.NotSet;
                                    dvbs.WestPosition = group.IsWestPosition;
                                    dvbs.OuterFEC = FECMethod.NotSet;
                                    dvbs.InnerFEC = FECMethod.NotSet;
                                    dvbs.OrbitalPosition = -1;
                                    dvbs.Elevation = -1;
                                    dvbs.Azimuth = 0;

                                    // Set polarisation
                                    switch (group.Polarization)
                                    {
                                        case Polarizations.Horizontal: dvbs.SignalPolarisation = Polarisation.Horizontal; break;
                                        case Polarizations.Vertical: dvbs.SignalPolarisation = Polarisation.Vertical; break;
                                    }

                                    // Set rate
                                    switch (group.InnerFEC)
                                    {
                                        case InnerForwardErrorCorrectionModes.Conv1_2: dvbs.InnerFEC = FECMethod.Viterbi; dvbs.InnerFECRate = BinaryConvolutionCodeRate.Rate1_2; break;
                                        case InnerForwardErrorCorrectionModes.Conv2_3: dvbs.InnerFEC = FECMethod.Viterbi; dvbs.InnerFECRate = BinaryConvolutionCodeRate.Rate2_3; break;
                                        case InnerForwardErrorCorrectionModes.Conv3_4: dvbs.InnerFEC = FECMethod.Viterbi; dvbs.InnerFECRate = BinaryConvolutionCodeRate.Rate3_4; break;
                                        case InnerForwardErrorCorrectionModes.Conv3_5: dvbs.InnerFEC = FECMethod.Viterbi; dvbs.InnerFECRate = BinaryConvolutionCodeRate.Rate3_5; break;
                                        case InnerForwardErrorCorrectionModes.Conv4_5: dvbs.InnerFEC = FECMethod.Viterbi; dvbs.InnerFECRate = BinaryConvolutionCodeRate.Rate4_5; break;
                                        case InnerForwardErrorCorrectionModes.Conv5_6: dvbs.InnerFEC = FECMethod.Viterbi; dvbs.InnerFECRate = BinaryConvolutionCodeRate.Rate5_6; break;
                                        case InnerForwardErrorCorrectionModes.Conv7_8: dvbs.InnerFEC = FECMethod.Viterbi; dvbs.InnerFECRate = BinaryConvolutionCodeRate.Rate7_8; break;
                                        case InnerForwardErrorCorrectionModes.Conv8_9: dvbs.InnerFEC = FECMethod.Viterbi; dvbs.InnerFECRate = BinaryConvolutionCodeRate.Rate8_9; break;
                                        case InnerForwardErrorCorrectionModes.Conv9_10: dvbs.InnerFEC = FECMethod.Viterbi; dvbs.InnerFECRate = BinaryConvolutionCodeRate.Rate9_10; break;
                                    }
                                };

                        // Change type - actually test it
                        tuningSpace.DefaultLocator = (ILocator) locator;
                    }
                    finally
                    {
                        // Cleanup
                        BDAEnvironment.Release( ref locator );
                    }

                    // Report
                    return (IDVBTuningSpace2) tuningSpace;
                }
                catch
                {
                    // Cleanup
                    BDAEnvironment.Release( ref space );

                    // Forward
                    throw;
                }
            }
        }
    }
}
