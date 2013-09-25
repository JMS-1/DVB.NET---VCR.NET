using System;


namespace JMS.DVB.DeviceAccess.Pipeline
{
    /// <summary>
    /// Beschreibt die Informationen während der Bearbeitung einer Aktion in einer Ausführungsliste.
    /// </summary>
    public abstract class PipelineToken
    {
        /// <summary>
        /// Die Ausführungsliste, in der die aktuelle Ausführung aktiv ist.
        /// </summary>
        public ActionPipeline Pipeline { get; private set; }

        /// <summary>
        /// Initialisiert eine Beschreibung.
        /// </summary>
        /// <param name="pipeline">Die zugehörige Gesamtliste aller Aktionen gleicher Art.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Gesamtliste angegeben.</exception>
        protected PipelineToken( ActionPipeline pipeline )
        {
            // Validate
            if (pipeline == null)
                throw new ArgumentNullException( "pipeline" );

            // Remember
            Pipeline = pipeline;
        }

        /// <summary>
        /// Führt die mit diesem Zustand verbundenen Aktionen aus.
        /// </summary>
        public void Execute()
        {
            // Forward
            Pipeline.Execute( this );
        }
    }

    /// <summary>
    /// Beschreibt die Informationen während der Bearbeitung einer Aktion in einer Ausführungsliste.
    /// </summary>
    /// <typeparam name="T">Die konkrete Implementierung dieses Zustands.</typeparam>
    public abstract class PipelineToken<T> : PipelineToken where T : PipelineToken
    {
        /// <summary>
        /// Die Ausführungsliste, in der die aktuelle Ausführung aktiv ist.
        /// </summary>
        public new ActionPipeline<T> Pipeline
        {
            get
            {
                // Forward
                return (ActionPipeline<T>) base.Pipeline;
            }
        }

        /// <summary>
        /// Initialisiert eine Beschreibung.
        /// </summary>
        /// <param name="pipeline">Die zugehörige Gesamtliste aller Aktionen gleicher Art.</param>
        protected PipelineToken( ActionPipeline<T> pipeline )
            : base( pipeline )
        {
        }
    }
}
