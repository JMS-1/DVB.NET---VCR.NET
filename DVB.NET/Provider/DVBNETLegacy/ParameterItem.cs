using System;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;

using JMS.DVB.DeviceAccess.Editors;


namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Beschreibt einen Parameter zu einem Geräteprofil.
    /// </summary>
    internal class ParameterItem : ListViewItem
    {
        /// <summary>
        /// Die zugehörige XML Struktur für die Definition des Parameters.
        /// </summary>
        private XmlElement m_Node;

        /// <summary>
        /// Die Daten zum aktuellen Wert.
        /// </summary>
        private ParameterValue m_Value;

        /// <summary>
        /// Die normale Hintergrundfarbe der Wertzelle.
        /// </summary>
        private Color m_CellBack;

        /// <summary>
        /// Meldet, ob dieser Parameter veränderbar ist.
        /// </summary>
        public bool IsEditable { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung für einen Parameter.
        /// </summary>
        /// <param name="node">Die Definition des Parameters.</param>
        /// <param name="displayValue">Der ursprüngliche Wert des Parameters, wie er dem Anwender zur Auswahl präsentiert wird.</param>
        /// <param name="selectionValue">Der Wert, wie er wirklich gespeichert ist - evtl. abweichend von der 
        /// Darstellung, die der Anwender auswählt.</param>
        public ParameterItem( XmlElement node, string displayValue, string selectionValue )
            : base( node.Name )
        {
            // Remember
            m_Value = (null == displayValue) ? null : new ParameterValue( displayValue, selectionValue );
            IsEditable = node.HasAttribute( "editor" );
            m_Node = node;

            // Add current value
            SubItems.Add( (null != m_Value) ? m_Value.DisplayText : DefaultValue );

            // Fix color
            if (!IsEditable)
                BackColor = Color.LightGreen;

            // Remember color
            m_CellBack = BackColor;

            // Check it
            TestValue();
        }

        /// <summary>
        /// Zeigt an, dass die aktuelle Auswahl gültig ist.
        /// </summary>
        public bool IsValid
        {
            get
            {
                // Use helper
                return (BackColor == m_CellBack);
            }
        }

        /// <summary>
        /// Meldet, ob eine Eingabe dieses Parameters zwingend erforderlich ist.
        /// </summary>
        public bool IsRequired
        {
            get
            {
                // Validate
                if (null == m_Node)
                    return false;

                // Find it
                string flag = m_Node.GetAttribute( "required" );
                if (string.IsNullOrEmpty( flag ))
                    return false;

                // Result
                bool required;
                if (bool.TryParse( flag, out required ))
                    return required;
                else
                    return false;
            }
        }

        /// <summary>
        /// Prüft, ob ein zwingend erforderlicher Wert angegeben ist.
        /// </summary>
        private void TestValue()
        {
            // Reset color
            BackColor = m_CellBack;

            // Check for missing value
            if (string.IsNullOrEmpty( SubItems[1].Text ))
                if (IsRequired)
                    BackColor = Color.Yellow;
        }

        /// <summary>
        /// Setzt oder liest den aktuellen Wert.
        /// </summary>
        public ParameterValue Value
        {
            get
            {
                // Report
                return m_Value;
            }
            set
            {
                // Remember
                m_Value = value;

                // Update
                SubItems[1].Text = (null == m_Value) ? null : m_Value.DisplayText;

                // Check it
                TestValue();
            }
        }

        /// <summary>
        /// Ruft den Dialog zur Pflege des Parameters auf.
        /// </summary>
        /// <param name="owner">Der übergeordnete Dialog.</param>
        /// <returns>Gesetzt, wenn eine Veränderung vorgenommen wurde.</returns>
        public bool Edit( IWin32Window owner )
        {
            // Get the current value
            var value = Value;

            // Get the editor
            string editorType = m_Node.GetAttribute( "editor" );

            // Create it
            using (var editor = (IParameterEditor) Activator.CreateInstance( Type.GetType( editorType ) ))
                if (!editor.Edit( owner, m_Node.Name, ref value ))
                    return false;

            // Update
            Value = value;

            // We changed
            return true;
        }

        /// <summary>
        /// Meldet den Vorgabewert für diesen Parameter.
        /// </summary>
        public string DefaultValue
        {
            get
            {
                // Report
                return m_Node.InnerText;
            }
        }
    }
}
