using System;

using JMS.DVB.DeviceAccess.Pipeline;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess
{
    partial class DataGraph
    {
        /// <summary>
        /// Beschreibt den Zustand bei der Ermittelung der Signalinformationen.
        /// </summary>
        public partial class SignalToken : PipelineToken<SignalToken>, IDisposable
        {
            /// <summary>
            /// Die bisher ermittelten Informationen zur Signalstärke.
            /// </summary>
            public BDASignalStatus Status { get; private set; }

            /// <summary>
            /// Das Ergebnis für die DVB.NET Geräteabstraktion.
            /// </summary>
            public SignalInformation SignalInformation { get; set; }

            /// <summary>
            /// Die vom Empfänger gemeldete Signalstärke.
            /// </summary>
            public int? SignalStrength { get; private set; }

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="pipeline">Die zugehörige Gesamtliste aller Aktionen gleicher Art.</param>
            /// <param name="status">Der initiale Status.</param>
            /// <param name="tunerStrength">Die vom Empfängerfilter gemeldete Signalstärke.</param>
            private SignalToken( ActionPipeline<SignalToken> pipeline, BDASignalStatus status, int? tunerStrength )
                : base( pipeline )
            {
                // Remember
                SignalStrength = tunerStrength;
                Status = status;

                // Prepare result
                if (Status == null)
                    SignalInformation = new SignalInformation();
                else
                    SignalInformation = new SignalInformation { Locked = status.Locked, Quality = status.Quality, Strength = status.Strength };
            }

            /// <summary>
            /// Erzeugt ein neue Zustandsinformation.
            /// </summary>
            /// <param name="pipeline">Die zugehörige Gesamtliste aller Aktionen gleicher Art.</param>
            /// <returns>Die gewünschte Information.</returns>
            /// <exception cref="ArgumentNullException">Es wurde kein Graph übergeben.</exception>
            internal static SignalToken Create( ActionPipeline<SignalToken> pipeline )
            {
                // Status to use
                BDASignalStatus status = null;
                int? strength = null;

                // See if something is provided
                var tuner = pipeline.Graph.TunerFilter;
                if (tuner != null)
                {
                    // Ask network provider for signal information
                    using (var filter = pipeline.Graph.NetworkProvider.MarshalToManaged())
                        try
                        {
                            // Attach to the primary interface and read the strength if provided - may be more current than the signal statistics
                            var tunerInterface = filter.Object as ITuner;
                            if (tunerInterface != null)
                                strength = tunerInterface.SignalStrength;
                        }
                        catch
                        {
                            // Just ignore any error
                        }

                    // Attach to interface
                    var statistics = tuner.GetSignalStatistics();
                    if (statistics != null)
                        try
                        {
                            // Report
                            status = new BDASignalStatus( statistics.SignalLocked != 0, statistics.SignalStrength, statistics.SignalQuality / 100.0 );
                        }
                        catch
                        {
                            // Just ignore any error
                        }
                        finally
                        {
                            // Back to COM
                            BDAEnvironment.Release( ref statistics );
                        }
                }

                // Create new
                return new SignalToken( pipeline, status, strength );
            }

            #region IDisposable Members

            /// <summary>
            /// Beendet die Nutzung dieser Instanz endgültig.
            /// </summary>
            public void Dispose()
            {
            }

            #endregion
        }

        /// <summary>
        /// Die für die Umsetzung der Signalinformationen verantwortliche Aktionslist.
        /// </summary>
        public ActionPipeline<SignalToken> SignalPipeline { get; private set; }
    }
}
