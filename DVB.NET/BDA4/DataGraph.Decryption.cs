using System;

using JMS.DVB.DeviceAccess.Pipeline;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess
{
    partial class DataGraph
    {
        /// <summary>
        /// Enthält alle Informationen zum Entschlüsseln einer Quelle.
        /// </summary>
        public partial class DecryptToken : PipelineToken<DecryptToken>, IDisposable
        {
            /// <summary>
            /// Die Liste der zu entschlüsselnden Quellen.
            /// </summary>
            public SourceIdentifier[] Sources { get; private set; }

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="pipeline">Die zugehörige Gesamtliste aller Aktionen gleicher Art.</param>
            /// <param name="sources">Die Liste der zu entschlüsselnden Quellen.</param>
            private DecryptToken( ActionPipeline<DecryptToken> pipeline, SourceIdentifier[] sources )
                : base( pipeline )
            {
                // Remember
                Sources = sources;
            }

            /// <summary>
            /// Erzeugt ein neue Zustandsinformation.
            /// </summary>
            /// <param name="pipeline">Die zugehörige Gesamtliste aller Aktionen gleicher Art.</param>
            /// <param name="sources">Die Liste der zu entschlüsselnden Quellen.</param>
            /// <returns>Die gewünschte Information.</returns>
            /// <exception cref="ArgumentNullException">Es wurde kein Graph übergeben.</exception>
            internal static DecryptToken Create( ActionPipeline<DecryptToken> pipeline, SourceIdentifier[] sources )
            {
                // Create new
                return new DecryptToken( pipeline, sources );
            }

            /// <summary>
            /// Meldet das Warten auf die Informationen zu einer Quelle an.
            /// </summary>
            /// <param name="source">Die betroffene Quelle.</param>
            /// <param name="consumer">Die bei Bereitstellung der Informationen zu startende Aktion.</param>
            public void WaitForPMT( SourceIdentifier source, Action<EPG.Tables.PMT> consumer )
            {
                // Blind forward
                Pipeline.Graph.ActivatePMTWatchDog( source, consumer );
            }

            /// <summary>
            /// Erstellt eine neue Überwachung der Nutzdatenströme.
            /// </summary>
            /// <param name="processor">Der Verarbeitungsalgorithmus.</param>
            /// <param name="services">Die Liste der Dienste.</param>
            public void WaitForPMTs( Func<EPG.Tables.PMT, bool> processor, params SourceIdentifier[] services )
            {
                // Forward
                Pipeline.Graph.ActivatePMTWatchDog( processor, services );
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
        /// Die für die Entschlüsselung einer Quelle verantwortliche Aktionslist.
        /// </summary>
        public ActionPipeline<DecryptToken> DecryptionPipeline { get; private set; }
    }
}
