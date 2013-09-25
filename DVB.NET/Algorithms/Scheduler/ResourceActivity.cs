using System;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt die nächste auszuführende Aktion.
    /// </summary>
    public abstract class ResourceActivity
    {
        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        protected ResourceActivity()
        {
        }
    }

    /// <summary>
    /// Bis zur Durchführung der nächsten Aktion muss eine Pause eingelegt werden.
    /// </summary>
    public sealed class WaitActivity : ResourceActivity
    {
        /// <summary>
        /// Meldet den Zeitpunkt für die nächste Prüfung.
        /// </summary>
        public DateTime RetestTime { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="when">Der Zeitpunkt, an dem die nächste Prüfung auszuführen ist.</param>
        internal WaitActivity( DateTime when )
        {
            // Rememebr
            RetestTime = when;
        }

        /// <summary>
        /// Erstellt eine Anzeigetext zu Testzwecken.
        /// </summary>
        /// <returns>Der gewünschte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Wait until {0}", RetestTime.ToLocalTime() );
        }
    }

    /// <summary>
    /// Eine Aufzeichnung oder Aufgabe kann beendet werden.
    /// </summary>
    public sealed class StopActivity : ResourceActivity
    {
        /// <summary>
        /// Die eindeutige Kennung der Aufzeichnung oder Aufgabe.
        /// </summary>
        public Guid UniqueIdentifier { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="definitionIdentifier">Die eindeutige Kennung der betroffenen Aufzeichnung oder Aufgabe.</param>
        internal StopActivity( Guid definitionIdentifier )
        {
            // Remember
            UniqueIdentifier = definitionIdentifier;
        }
    }

    /// <summary>
    /// Eine Aufzeichnung oder Aufgabe kann gestartet werden.
    /// </summary>
    public sealed class StartActivity : ResourceActivity
    {
        /// <summary>
        /// Die Beschreibung der Aufzeichnung oder Aufgabe.
        /// </summary>
        public IScheduleInformation Recording { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="definition">Die Daten zur Aufzeichnung oder Aufgabe.</param>
        internal StartActivity( IScheduleInformation definition )
        {
            // Remember
            Recording = definition;
        }
    }
}
