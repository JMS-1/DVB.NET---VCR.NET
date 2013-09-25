using System;
using System.Linq;
using System.Windows.Forms;


namespace JMS.DVB.Editors
{
    /// <summary>
    /// Erlaubt die Auswahl einer Aktionserweiterung.
    /// </summary>
    internal class ProviderSelector
    {
        /// <summary>
        /// Die zugehörige Kennzeichnung.
        /// </summary>
        public PipelineAttribute Attribute { get; private set; }

        /// <summary>
        /// Der Datentyp mit der Implementierung.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Erzeugt ein neues Auswahlelement.
        /// </summary>
        /// <param name="type">Die Implementierung der Aktion.</param>
        /// <param name="attribute">Die kennzeichnende Eigenschaft.</param>
        public ProviderSelector( Type type, PipelineAttribute attribute )
        {
            // Remember
            Attribute = attribute;
            Type = type;
        }

        /// <summary>
        /// Meldet den Anzeigenamen.
        /// </summary>
        /// <returns>Der gewünschte Anzeigename.</returns>
        public override string ToString()
        {
            // Report
            return Attribute.DisplayName;
        }

        /// <summary>
        /// Meldet den vollen Namen des Datentyps.
        /// </summary>
        public string TypeName
        {
            get
            {
                // Report
                return string.Format( "{0}, {1}", Type.FullName, Type.Assembly.GetName().Name );
            }
        }

        /// <summary>
        /// Aktiviert die Auswahl einer Erweiterung.
        /// </summary>
        /// <param name="selector">Die zugehörige Auswahlliste.</param>
        /// <param name="typeName">Der Name der zu verwendenden Klasse.</param>
        /// <param name="editor">Das verwendete Formular.</param>
        public static void Select( ComboBox selector, string typeName, StandardHardwareEditor editor )
        {
            // Choose
            foreach (var item in selector.Items.OfType<ProviderSelector>())
                if (Equals( item.TypeName, typeName ))
                {
                    // Just select
                    selector.SelectedItem = item;

                    // Report
                    editor.PipelineSelectionChanged( selector, true );

                    // Done
                    return;
                }
        }
    }
}
