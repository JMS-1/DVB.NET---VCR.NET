using System;
using System.Linq;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt Einschränkungen bei der Entschlüsselung, die für mehrere Geräte
    /// gemeinsam gilt.
    /// </summary>
    /// <remarks>
    /// Man beachte, dass es sich nicht um Klassen handelt. Eine zyklische Referenz über
    /// die <see cref="DecryptionGroups"/> ist daher nicht möglich.
    /// </remarks>
    public struct DecryptionGroup
    {
        /// <summary>
        /// Die maximale Anzahl von Quellen, die gleichzeitig entschlüssel werden können.
        /// </summary>
        public int MaximumParallelSources;

        /// <summary>
        /// Meldet alle untergeordneten Entschlüsselungsgruppen.
        /// </summary>
        public DecryptionGroup[] DecryptionGroups;

        /// <summary>
        /// Meldet alle Geräte in dieser Gruppe.
        /// </summary>
        public IScheduleResource[] ScheduleResources;

        /// <summary>
        /// Erstellt einen Anzeigetext zu Testzwecken.
        /// </summary>
        /// <returns>Der gewünschte Anzeigetext.</returns>
        public override string ToString()
        {
            // Merge
            return
                string.Format
                    (
                        "CI({0}*{1})<{2}",
                        (DecryptionGroups == null) ? null : string.Join( ",", DecryptionGroups.Select( g => g.ToString() ).ToArray() ),
                        (ScheduleResources == null) ? null : string.Join( ",", ScheduleResources.Select( r => (r == null) ? null : r.Name ).ToArray() ),
                        MaximumParallelSources + 1
                    );
        }

        /// <summary>
        /// Prüft eine Entschlüsselungsregel.
        /// </summary>
        /// <exception cref="ArgumentNullException">Die Regel ist ungültig.</exception>
        public void Validate()
        {
            // Self
            if (ScheduleResources != null)
                if (ScheduleResources.Any( r => r == null ))
                    throw new ArgumentNullException( "ScheduleResources" );

            // Forward
            if (DecryptionGroups != null)
                foreach (var subGroup in DecryptionGroups)
                    subGroup.Validate();
        }
    }
}
