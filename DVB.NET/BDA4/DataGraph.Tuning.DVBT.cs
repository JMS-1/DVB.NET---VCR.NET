using System;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess
{
    partial class DataGraph
    {
        partial class TuneToken
        {
            /// <summary>
            /// Die COM Klasse, die einen DVB-T Namensraum implementiert.
            /// </summary>
            private static readonly Guid DVBTTuningSpaceClassIdentifier = new Guid( "c6b14b32-76aa-4a86-a7ac-5c79aaf58da7" );

            /// <summary>
            /// Die COM Klasse, die eine DVB-T Quellgruppe beschreibt.
            /// </summary>
            private static readonly Guid DVBTLocatorClassIdentifier = new Guid( "9cd64701-bdf3-4d14-8e03-f12983d86664" );

            /// <summary>
            /// Bereitet die internen Strukturen für die Konfiguration einer Quellgruppe für den
            /// Antennenempfang vor.
            /// </summary>
            /// <param name="initializer">Methode zur Übernahme der Parameter der Quellgruppe.</param>
            /// <returns>Der zu verwendende Namensraum.</returns>
            private IDVBTuningSpace2 PrepareTerrestrial( out Action<ILocator> initializer )
            {
                // Create tuning space
                var space = Activator.CreateInstance( Type.GetTypeFromCLSID( DVBTTuningSpaceClassIdentifier ) );
                try
                {
                    // Remember
                    var tuningSpace = (IDVBTuningSpace2) space;

                    // Configure
                    tuningSpace.UniqueName = "DVBNET Terrestrial";
                    tuningSpace.FriendlyName = "DVB.NET Terrestrial Tuning Space";
                    tuningSpace.NetworkType = "{216C62DF-6D7F-4E9A-8571-05F14EDB766A}";
                    tuningSpace.SystemType = DVBSystemType.Terrestrial;

                    // Create locator
                    var locator = Activator.CreateInstance( Type.GetTypeFromCLSID( DVBTLocatorClassIdentifier ) );
                    try
                    {
                        // See if we are tuning
                        var group = SourceGroup as TerrestrialGroup;
                        if (group == null)
                            initializer = null;
                        else
                            initializer = clone =>
                                {
                                    // Change type
                                    var dvbt = (IDVBTLocator) clone;

                                    // Configure
                                    dvbt.LPInnerFECRate = BinaryConvolutionCodeRate.NotSet;
                                    dvbt.InnerFECRate = BinaryConvolutionCodeRate.NotSet;
                                    dvbt.OuterFECRate = BinaryConvolutionCodeRate.NotSet;
                                    dvbt.CarrierFrequency = (int) group.Frequency;
                                    dvbt.Modulation = ModulationType.NotSet;
                                    dvbt.HAlpha = HierarchyAlpha.NotSet;
                                    dvbt.Mode = TransmissionMode.NotSet;
                                    dvbt.LPInnerFEC = FECMethod.NotSet;
                                    dvbt.Guard = GuardInterval.NotSet;
                                    dvbt.InnerFEC = FECMethod.NotSet;
                                    dvbt.OtherFrequencyInUse = false;
                                    dvbt.OuterFEC = FECMethod.NotSet;
                                    dvbt.SymbolRate = -1;
                                    dvbt.Bandwidth = -1;

                                    // Check bandwidth
                                    switch (group.Bandwidth)
                                    {
                                        case Bandwidths.Six: dvbt.Bandwidth = 6; break;
                                        case Bandwidths.Seven: dvbt.Bandwidth = 7; break;
                                        case Bandwidths.Eight: dvbt.Bandwidth = 8; break;
                                    }
                                };

                        // Remember - and test the locator type
                        tuningSpace.DefaultLocator = (ILocator) locator;
                    }
                    finally
                    {
                        // Cleanup
                        BDAEnvironment.Release( ref locator );
                    }

                    // Report
                    return tuningSpace;
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
