using System;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Eine Ansammlungen von Geräten.
    /// </summary>
    internal class ResourceCollection : IEnumerable<IScheduleResource>
    {
        /// <summary>
        /// Alle angemeldeten Geräte.
        /// </summary>
        private HashSet<IScheduleResource> m_Resources = new HashSet<IScheduleResource>( ReferenceComparer<IScheduleResource>.Default );

        /// <summary>
        /// Die Geräte geordnet nach der Priorität, wobei das Geräte mit der höchsten Priorität am Ende steht.
        /// </summary>
        private List<IScheduleResource> m_ByPriority = new List<IScheduleResource>();

        /// <summary>
        /// Alle zusätzlichen Entschlüsselungsinformationen.
        /// </summary>
        private List<DecryptionGroup> m_Decryption = new List<DecryptionGroup>();

        /// <summary>
        /// Erzeugt eine neue Ansammlung.
        /// </summary>
        public ResourceCollection()
        {
        }

        /// <summary>
        /// Erstellt eine Nachschlagekarte aller verwendeten Geräte und initialisiert den Nachschlagewert
        /// mit einer bestimmten Zeit.
        /// </summary>
        /// <param name="initialTime">Der gewünschte Zeitwert.</param>
        /// <returns>Die Nachschlagekarte.</returns>
        public Dictionary<IScheduleResource, DateTime> ToDictionary( DateTime initialTime )
        {
            // Report
            return m_ByPriority.ToDictionary( r => r, r => initialTime, m_Resources.Comparer );
        }

        /// <summary>
        /// Prüft, ob ein Gerät bereits verwaltet wird.
        /// </summary>
        /// <param name="resource">Das zu prüfende Gerät.</param>
        /// <returns>Gesetzt, wenn das Gerät verwaltet wird.</returns>
        public bool Contains( IScheduleResource resource )
        {
            // Forward
            return m_Resources.Contains( resource );
        }

        /// <summary>
        /// Meldet alle bisher angemeldeten Entschlüsselungsgruppen.
        /// </summary>
        public IEnumerable<DecryptionGroup> DecryptionGroups
        {
            get
            {
                // Report
                return m_Decryption;
            }
        }

        /// <summary>
        /// Meldet ein Gerät zur Verwendung an.
        /// </summary>
        /// <param name="resource">Das gewünschte Gerät.</param>
        public void Add( IScheduleResource resource )
        {
            // Must be set
            if (resource == null)
                throw new ArgumentNullException( "resource" );
            if (resource.Decryption.MaximumParallelSources < 0)
                throw new ArgumentOutOfRangeException( "resource", string.Format( Properties.SchedulerResources.Exception_NegativeEncryption, resource.Decryption.MaximumParallelSources ) );
            if (resource.SourceLimit < 0)
                throw new ArgumentOutOfRangeException( "resource", string.Format( Properties.SchedulerResources.Exception_NegativeSourceLimit, resource.SourceLimit ) );

            // Must not be duplicated
            if (m_Resources.Contains( resource ))
                throw new ArgumentException( resource.Name, "resource" );

            // Add it to the map
            m_Resources.Add( resource );

            // Add according to priority
            var index = m_ByPriority.FindIndex( r => r.AbsolutePriority > resource.AbsolutePriority );
            if (index < 0)
                m_ByPriority.Add( resource );
            else
                m_ByPriority.Insert( index, resource );

#if !SILVERLIGHT
            // Report
            if (RecordingScheduler.SchedulerTrace.TraceInfo)
                Trace.TraceInformation( Properties.SchedulerResources.Trace_AddResource, index, resource, resource.AbsolutePriority );
#endif
        }

        /// <summary>
        /// Meldet komplete Entschlüsselungsregeln an.
        /// </summary>
        /// <param name="group">Eine neue Regel.</param>
        /// <exception cref="ArgumentNullException">Die Regel ist ungültig.</exception>
        public void Add( DecryptionGroup group )
        {
            // Check
            group.Validate();

            // Remember
            m_Decryption.Add( group );
        }

        #region IEnumerable<IScheduleResource> Members

        /// <summary>
        /// Meldet eine Auflistung über alle Geräte geordnet nach der Verteilungswichtung.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        public IEnumerator<IScheduleResource> GetEnumerator()
        {
            // Forward
            return m_ByPriority.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Meldet eine Auflistung über alle Geräte geordnet nach der Verteilungswichtung.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // Forward
            return GetEnumerator();
        }

        #endregion
    }
}
