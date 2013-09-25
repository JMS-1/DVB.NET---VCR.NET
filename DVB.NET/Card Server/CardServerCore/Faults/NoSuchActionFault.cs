using System;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass eine bestimmte Erweiterung nicht zur Verfügung steht.
    /// </summary>
    [Serializable]
    public class NoSuchActionFault : CardServerFault
    {
        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public NoSuchActionFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="actionType">Der Name der angeforderten Erweiterungsklasse.</param>
        public NoSuchActionFault( string actionType )
            : base( string.Format( Properties.Resources.Exception_NoSuchAction, actionType ) )
        {
        }
    }
}
