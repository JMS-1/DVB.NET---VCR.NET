using System;
using System.Collections.Generic;
using System.Linq;
using JMS.DVB.EPG.Tables;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Hilfsklasse zum Überwachen der Datenströme von Quellen
    /// </summary>
    internal class PMTSequencer
    {
        /// <summary>
        /// Die zugehörige Empfangsinstanz.
        /// </summary>
        private readonly DataGraph m_graph;

        /// <summary>
        /// Alle zu überwachenden Quellen.
        /// </summary>
        private readonly SourceIdentifier[] m_services;

        /// <summary>
        /// Die nächste abzufragende Quelle.
        /// </summary>
        private int m_index;

        /// <summary>
        /// Die zuletzt empfangenen Daten.
        /// </summary>
        private readonly Dictionary<ushort, PMT> m_lastTable;

        /// <summary>
        /// Die Verarbeitungsmethode.
        /// </summary>
        private readonly Func<PMT, bool> m_processor;

        /// <summary>
        /// Startet eine Überwachung.
        /// </summary>
        /// <param name="graph">Die zugehörige Empfangsinstanz.</param>
        /// <param name="services">Die Liste der Dienste.</param>
        /// <param name="processor">Der Verarbeitungsalgorithmus.</param>
        public static void Start( DataGraph graph, SourceIdentifier[] services, Func<PMT, bool> processor )
        {
            // Validate
            if (graph == null)
                throw new ArgumentNullException( "graph" );
            if (processor == null)
                throw new ArgumentNullException( "processor" );

            // Nothing to do
            if (services == null)
                return;

            // Create scope and process
            var sequencer = new PMTSequencer( graph, services, processor );

            // Nothing to do
            if (sequencer.m_services.Length < 1)
                return;

            // Install watch dog
            sequencer.ProcessNext( null );
        }

        /// <summary>
        /// Startet eine Überwachung.
        /// </summary>
        /// <param name="graph">Die zugehörige Empfangsinstanz.</param>
        /// <param name="services">Die Liste der Dienste.</param>
        /// <param name="processor">Der Verarbeitungsalgorithmus.</param>
        private PMTSequencer( DataGraph graph, SourceIdentifier[] services, Func<PMT, bool> processor )
        {
            // Remember
            m_services = services.Where( s => s != null ).ToArray();
            m_lastTable = m_services.ToDictionary( s => s.Service, s => default( PMT ) );
            m_processor = processor;
            m_graph = graph;
        }

        /// <summary>
        /// Wertet die nächste Tabelle aus.
        /// </summary>
        /// <param name="table">Eine aktuelle Tabelle.</param>
        private void ProcessNext( PMT table )
        {
            // Got new data
            if (table != null)
                if (table.IsValid)
                {
                    // See of there is a previous one
                    PMT previous;
                    if (m_lastTable.TryGetValue( table.ProgramNumber, out previous ))
                    {
                        // Report on change or first request
                        if ((previous == null) || (previous.Version != table.Version))
                            if (!m_processor( table ))
                                return;

                        // Update
                        m_lastTable[table.ProgramNumber] = table;
                    }
                }

            // Next source to request
            var source = m_services[m_index++];

            // Clip
            if (m_index == m_services.Length)
                m_index = 0;

            // Fire up
            m_graph.ActivatePMTWatchDog( source, ProcessNext );
        }
    }
}
