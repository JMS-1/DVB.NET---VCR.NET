using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Basisklasse für Fehlermeldungen.
    /// </summary>
    [Serializable]
    public class CardServerFault
    {
        /// <summary>
        /// Die eigentliche Fehlermeldung.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public CardServerFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="text">Der eigentliche Fehler, der aber möglicherweise nicht serialisierbar ist.</param>
        public CardServerFault( string text )
        {
            // Remember
            Message = text;
        }
    }

    /// <summary>
    /// Basisklasse für alle Ausnahmen.
    /// </summary>
    [Serializable]
    public class CardServerException : Exception
    {
        /// <summary>
        /// Der ursprüngliche Fehler, falls dieser einem bekannten Fehler entspricht.
        /// </summary>
        public CardServerFault Fault { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="fault">Der innerer Fehler.</param>
        public CardServerException( CardServerFault fault )
            : base( fault.Message )
        {
            // Remember
            Fault = fault;
        }

        /// <summary>
        /// Wird zur Deserialisierung benötigt.
        /// </summary>
        /// <param name="info">Daten der Ausnahme.</param>
        /// <param name="context">Aktuelle Deserialisierungsumgebung.</param>
        public CardServerException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

        /// <summary>
        /// Wandelt einen Fehler in eine Ausnahme um.
        /// </summary>
        /// <param name="fault">Der beobachtete Fehler.</param>
        /// <returns>Die zugehörige Ausnahme.</returns>
        public static void Throw( CardServerFault fault )
        {
            // Forward
            if (null == fault)
                throw new ArgumentNullException( "fault" );
            else
                throw new CardServerException( fault );
        }
    }
}
