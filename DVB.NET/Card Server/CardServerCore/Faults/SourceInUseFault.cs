using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Wird ausgelöst, wenn eine Quelle mehrfach aktiviert werden soll.
    /// </summary>
    [Serializable]
    public class SourceInUseFault : CardServerFault
    {
        /// <summary>
        /// Die betroffene Quelle.
        /// </summary>
        public SourceIdentifier Source { get; set; }

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public SourceInUseFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="source">Die doppelt verwendete Quelle.</param>
        public SourceInUseFault( SourceIdentifier source )
            : base( string.Format( Properties.Resources.Exception_DuplicateSource, SourceIdentifier.ToString( source ) ) )
        {
            // Remember
            Source = source;
        }
    }
}
