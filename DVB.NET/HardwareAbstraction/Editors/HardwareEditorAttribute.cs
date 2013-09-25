using System;


namespace JMS.DVB.Editors
{
    /// <summary>
    /// Legt für eine Hardwareimplemntierung fest, mit welcher Eingabemaske die
    /// Geräteparameter gepflegt werden sollen.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
    public class HardwareEditorAttribute : Attribute
    {
        /// <summary>
        /// Die .NET Klasse zur Pflege der Geräteparameter.
        /// </summary>
        public Type EditorType { get; set; }

        /// <summary>
        /// Erzeugt eine neue Markierung.
        /// </summary>
        /// <param name="editorType">Die .NET Klasse zur Pflege der Geräteparameter.</param>
        public HardwareEditorAttribute( Type editorType )
        {
            // Remember
            EditorType = editorType;
        }

        /// <summary>
        /// Erzeugt eine neue Markierung.
        /// </summary>
        public HardwareEditorAttribute()
        {
        }

        /// <summary>
        /// Ermittelt zu einer Implementierung einer Hardware einen Dialog zur
        /// Pflege der Geräteinformationen.
        /// </summary>
        /// <param name="hardwareType">Die .NET Klasse der Implementierung.</param>
        /// <returns>Der Dialog oder <i>null</i>, wenn kein Dialog ermittelt werden
        /// konnte.</returns>
        public static IHardwareEditor CreateEditor( string hardwareType )
        {
            // Forward
            return CreateEditor( Type.GetType( hardwareType ) );
        }

        /// <summary>
        /// Ermittelt zu einer Implementierung einer Hardware einen Dialog zur
        /// Pflege der Geräteinformationen.
        /// </summary>
        /// <param name="hardwareType">Die .NET Klasse der Implementierung.</param>
        /// <returns>Der Dialog oder <i>null</i>, wenn kein Dialog ermittelt werden
        /// konnte.</returns>
        public static IHardwareEditor CreateEditor( Type hardwareType )
        {
            // None
            if (null == hardwareType)
                return null;

            // Check attribute
            object[] attributes = hardwareType.GetCustomAttributes( typeof( HardwareEditorAttribute ), true );

            // None
            if ((null == attributes) || (1 != attributes.Length))
                return null;

            // Get the type
            Type editorType = ((HardwareEditorAttribute) attributes[0]).EditorType;

            // None
            if (null == editorType)
                return null;

            // Try create
            try
            {
                // Process
                return (IHardwareEditor) Activator.CreateInstance( editorType );
            }
            catch
            {
                // Failed
                return null;
            }
        }
    }
}
