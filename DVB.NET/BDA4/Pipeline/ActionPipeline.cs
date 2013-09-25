using System;
using System.Diagnostics;
using System.Collections.Generic;


namespace JMS.DVB.DeviceAccess.Pipeline
{
    /// <summary>
    /// Beschreibt eine Liste zur Ausführung von beliebigen Operationen.
    /// </summary>
    public abstract class ActionPipeline
    {
        /// <summary>
        /// Der zugehörige DirectShow Graph.
        /// </summary>
        public DataGraph Graph { get; private set; }

        /// <summary>
        /// Meldet einen internen Namen für diese Liste.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Liste.
        /// </summary>
        /// <param name="graph">Der zugehörige DirectShow Graph.</param>
        /// <param name="name">Der Name der Liste.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Graph oder kein Name angegeben.</exception>
        internal ActionPipeline( DataGraph graph, string name )
        {
            // Validate
            if (graph == null)
                throw new ArgumentNullException( "graph" );
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );

            // Remember
            Graph = graph;
            Name = name;
        }

        /// <summary>
        /// Führt alle Aktionen im Kontext eines Zustands aus.
        /// </summary>
        /// <param name="token">Der auszuführende Zustand.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Zustand übergeben.</exception>
        internal void Execute( PipelineToken token )
        {
            // Validate
            if (token == null)
                throw new ArgumentNullException( "token" );

            // Forward
            OnExecute( token );
        }

        /// <summary>
        /// Führt alle Aktionen im Kontext eines Zustands aus.
        /// </summary>
        /// <param name="token">Der auszuführende Zustand.</param>
        protected abstract void OnExecute( PipelineToken token );

        /// <summary>
        /// Meldet die Terminierung an alle Aktionen dieser Liste.
        /// </summary>
        protected abstract void OnTerminate();

        /// <summary>
        /// Gesetzt, wenn diese Liste keine Aktionen enthält.
        /// </summary>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Meldet die Terminierung an alle Aktionen dieser Liste.
        /// </summary>
        internal void Terminate()
        {
            // Forward
            OnTerminate();
        }
    }

    /// <summary>
    /// Beschreibt eine Liste zur Ausführung von beliebigen Operationen.
    /// </summary>
    /// <typeparam name="T">Die Art der verwendeten Zustandsinformationen bei der Ausführung einer Operation.</typeparam>
    public class ActionPipeline<T> : ActionPipeline where T : PipelineToken
    {
        /// <summary>
        /// Die einzelnen Aktionen dieser Liste.
        /// </summary>
        private List<Func<T, PipelineResult>> m_Actions = new List<Func<T, PipelineResult>>();

        /// <summary>
        /// Die Position der Standardaktion für diese Ausführungsliste.
        /// </summary>
        private int m_DefaultAction = 0;

        /// <summary>
        /// Erzeugt eine neue Liste.
        /// </summary>
        /// <param name="graph">Der zugehörige DirectShow Graph.</param>
        /// <param name="name">Der Name der Liste.</param>
        /// <param name="defaultAction">Der immer auszuführende Arbeitsschritt.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Graph angegeben.</exception>
        internal ActionPipeline( DataGraph graph, string name, Func<T, PipelineResult> defaultAction )
            : base( graph, name )
        {
            // Create default action entry
            if (defaultAction == null)
                m_DefaultAction = -1;
            else
                m_Actions.Add( defaultAction );
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        protected override void OnTerminate()
        {
            // Send termination to all
            foreach (var action in m_Actions)
                action( null );
        }

        /// <summary>
        /// Ergänzt einen weiteren Schritt in dieser Liste.
        /// </summary>
        /// <param name="action">Die auszuführende Aktion.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Aktion angegeben.</exception>
        public void AddPostProcessing( Func<T, PipelineResult> action )
        {
            // Validate
            if (action == null)
                throw new ArgumentNullException( "action" );

            // Do it
            m_Actions.Add( action );
        }

        /// <summary>
        /// Ergänzt einen weiteren Schritt in dieser Liste.
        /// </summary>
        /// <param name="action">Die auszuführende Aktion.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Aktion angegeben.</exception>
        /// <exception cref="NotSupportedException">Diese Liste besitzt keine Standardaktion.</exception>
        public void AddPreProcessing( Func<T, PipelineResult> action )
        {
            // Validate
            if (action == null)
                throw new ArgumentNullException( "action" );
            if (m_DefaultAction < 0)
                throw new NotSupportedException( Properties.Resources.Exception_NoDefaultAction );

            // Do it
            m_Actions.Insert( m_DefaultAction++, action );
        }

        /// <summary>
        /// Gesetzt, wenn diese Liste keine Aktionen enthält.
        /// </summary>
        public override bool IsEmpty
        {
            get
            {
                // Report
                return (m_Actions.Count < 1);
            }
        }

        /// <summary>
        /// Führt alle Aktionen im Kontext eines Zustands aus.
        /// </summary>
        /// <param name="token">Der auszuführende Zustand.</param>
        protected override void OnExecute( PipelineToken token )
        {
            // Blind forward
            Process( (T) token );
        }

        /// <summary>
        /// Führt die Aktionen in der Liste aus.
        /// </summary>
        /// <param name="token">Die zugehörigen Zustandsinformationen.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Zustand angegeben.</exception>
        /// <exception cref="InvalidOperationException">Die Zustandsinformation befindet sich bereits in Bearbeitung.</exception>
        /// <exception cref="NotSupportedException">Ein Arbeitsschritt meldet ein unerwartetes Ergebnis.</exception>
        private void Process( T token )
        {
            // Validate
            if (token == null)
                throw new ArgumentNullException( "token" );
            if (!ReferenceEquals( token.Pipeline, this ))
                throw new InvalidOperationException( Properties.Resources.Exception_WrongPipeline );

            // Run all extensions
            for (int i = 0; i < m_Actions.Count; i++)
                try
                {
                    // Process and check next step
                    var result = m_Actions[i]( token );
                    switch (result)
                    {
                        case PipelineResult.Continue: break;
                        case PipelineResult.Terminate: return;
                        default: throw new NotSupportedException( result.ToString() );
                    }
                }
                catch (Exception e)
                {
                    // Be safe
                    try
                    {
                        // Get the type
                        string type;
                        if (i < m_DefaultAction)
                            type = string.Format( Properties.Resources.Step_Prepare, i + 1 );
                        else if (i > m_DefaultAction)
                            type = string.Format( Properties.Resources.Step_Finish, i - m_DefaultAction );
                        else
                            type = Properties.Resources.Step_Main;

                        // Report
                        EventLog.WriteEntry( "DVB.NET", string.Format( Properties.Resources.Exception_PipelineAction, Name, type, e ), EventLogEntryType.Error );
                    }
                    catch
                    {
                        // For now ignore any error
                    }

                    // Forward
                    throw e;
                }
        }
    }
}
