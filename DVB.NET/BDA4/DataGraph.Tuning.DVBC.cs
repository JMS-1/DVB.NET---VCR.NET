using System;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess
{
    partial class DataGraph
    {
        partial class TuneToken
        {
            /// <summary>
            /// Die COM Klasse, die einen DVB-C Namensraum implementiert.
            /// </summary>
            private static readonly Guid DVBCTuningSpaceClassIdentifier = new Guid( "c6b14b32-76aa-4a86-a7ac-5c79aaf58da7" );

            /// <summary>
            /// Die COM Klasse, die eine DVB-C Quellgruppe beschreibt.
            /// </summary>
            private static readonly Guid DVBCLocatorClassIdentifier = new Guid( "c531d9fd-9685-4028-8b68-6e1232079f1e" );

            /// <summary>
            /// Bereitet die internen Strukturen für die Konfiguration einer Quellgruppe für den
            /// Kabelempfang vor.
            /// </summary>
            /// <param name="initializer">Methode zur Übernahme der Parameter der Quellgruppe.</param>
            /// <returns>Der zu verwendende Namensraum.</returns>
            private IDVBTuningSpace2 PrepareCable( out Action<ILocator> initializer )
            {
                // Create tuning space
                var space = Activator.CreateInstance( Type.GetTypeFromCLSID( DVBCTuningSpaceClassIdentifier ) );
                try
                {
                    // Remember
                    var tuningSpace = (IDVBTuningSpace2) space;

                    // Configure
                    tuningSpace.UniqueName = "DVBNET Cable";
                    tuningSpace.FriendlyName = "DVB.NET Cable Tuning Space";
                    tuningSpace.NetworkType = "{DC0C0FE7-0485-4266-B93F-68FBF80ED834}";
                    tuningSpace.SystemType = DVBSystemType.Cable;

                    // Create locator
                    var locator = Activator.CreateInstance( Type.GetTypeFromCLSID( DVBCLocatorClassIdentifier ) );
                    try
                    {
                        // Check group
                        var group = SourceGroup as CableGroup;
                        if (group == null)
                            initializer = null;
                        else
                            initializer = clone =>
                                {
                                    // Change type
                                    var dvbc = (IDVBCLocator) clone;

                                    // Configure
                                    dvbc.SymbolRate = checked( (int) (group.SymbolRate / 1000) );
                                    dvbc.CarrierFrequency = checked( (int) group.Frequency );
                                    dvbc.InnerFECRate = BinaryConvolutionCodeRate.NotSet;
                                    dvbc.OuterFECRate = BinaryConvolutionCodeRate.NotSet;
                                    dvbc.Modulation = ModulationType.NotSet;
                                    dvbc.InnerFEC = FECMethod.NotSet;
                                    dvbc.OuterFEC = FECMethod.NotSet;

                                    // Check modulation
                                    switch (group.Modulation)
                                    {
                                        case CableModulations.QAM16: dvbc.Modulation = ModulationType.QAM16; break;
                                        case CableModulations.QAM32: dvbc.Modulation = ModulationType.QAM32; break;
                                        case CableModulations.QAM64: dvbc.Modulation = ModulationType.QAM64; break;
                                        case CableModulations.QAM128: dvbc.Modulation = ModulationType.QAM128; break;
                                        case CableModulations.QAM256: dvbc.Modulation = ModulationType.QAM256; break;
                                    }

                                    // See if tuner is available
                                    var tuner = Pipeline.Graph.TunerFilter;
                                    if (tuner != null)
                                    {
                                        // Attach to the demodulator interface
                                        var demodulator = tuner.GetDigitalDemodulator();
                                        if (demodulator != null)
                                            try
                                            {
                                                // Strange API needs helpers - sets values using pointers
                                                var inversion = SpectralInversion.NotSet;

                                                // Check spectral inversion
                                                switch (group.SpectrumInversion)
                                                {
                                                    case SpectrumInversions.Auto: inversion = SpectralInversion.Automatic; break;
                                                    case SpectrumInversions.Off: inversion = SpectralInversion.Normal; break;
                                                    case SpectrumInversions.On: inversion = SpectralInversion.Inverted; break;
                                                }

                                                // Set inversion
                                                demodulator.SetSpectralInversion( ref inversion );
                                            }
                                            finally
                                            {
                                                // Cleanup
                                                BDAEnvironment.Release( ref demodulator );
                                            }
                                    }
                                };

                        // Remember - acutally test the type
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
