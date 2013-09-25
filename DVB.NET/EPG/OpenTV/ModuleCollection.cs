using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMS.DVB.EPG.OpenTV
{
    /// <summary>
    /// Hilfsklasse zur simultanen Rekonstuktion von Modulen.
    /// </summary>
    public class ModuleCollection
    {
        /// <summary>
        /// Alle bisher empfangenen Module.
        /// </summary>
        private Dictionary<ushort, Module> m_Modules = new Dictionary<ushort, Module>();

        /// <summary>
        /// Liste zur Teilweisen Freischaltung des Empfangs.
        /// </summary>
        private Dictionary<ushort, bool> m_Enabled = new Dictionary<ushort, bool>();

        /// <summary>
        /// Methode, die bei Komplettierung eines Moduls aufgerufen wird.
        /// </summary>
        public event Module.CompleteHandler OnModuleComplete;

        /// <summary>
        /// Erzeugt eine neue Rekonstruktionsinstanz.
        /// </summary>
        public ModuleCollection()
        {
        }

        /// <summary>
        /// Schaltet bestimmte Module für die Rekonstruktion frei.
        /// </summary>
        /// <param name="moduleIdentifier">Die Kennung des gewünschten Moduls.</param>
        public void AddModule(ushort moduleIdentifier)
        {
            // Remember synchronized.
            lock (m_Enabled)
                m_Enabled[moduleIdentifier] = true;
        }

        /// <summary>
        /// Übermittelt eine SI Tabelle zur Bearbeitung.
        /// </summary>
        /// <param name="table">Die neu empfangene SI Tabelle mit Teildaten zu einem Modul.</param>
        public void AddPartialModule(Tables.OpenTV table)
        {
            // See if module should be processed
            lock (m_Enabled)
                if (m_Enabled.Count > 0)
                    if (!m_Enabled.ContainsKey(table.ModuleIdentifier))
                        return;

            // The new module
            Module module;

            // Check for module manager inside collection
            lock (m_Modules)
                if (!m_Modules.TryGetValue(table.ModuleIdentifier, out module))
                {
                    // Create new
                    module = new Module();

                    // Connect
                    module.OnModuleComplete += ForwardComplete;

                    // Remember
                    m_Modules[table.ModuleIdentifier] = module;
                }

            // Forward
            module.AddPartialModule(table);
        }

        /// <summary>
        /// Wird aufgerufen, wenn ein Modul komplettiert wurde.
        /// </summary>
        /// <param name="lastTable">Die letzte SI Tabelle des Moduls.</param>
        /// <param name="module">Das fertiggestellte Modul.</param>
        private void ForwardComplete(Tables.OpenTV lastTable, Module module)
        {
            // Check for interested client
            Module.CompleteHandler callback = OnModuleComplete;

            // Report to client
            if (null != callback) callback(lastTable, module);
        }
    }
}
