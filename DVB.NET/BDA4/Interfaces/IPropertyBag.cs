using System;
using System.Runtime.InteropServices;

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// COM Schnittstelle zur dynamischen Verwaltung von Werten.
    /// </summary>
    [
        ComImport,
        Guid( "55272a00-42cb-11ce-8135-00aa004bb851" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IPropertyBag
    {
        /// <summary>
        /// Liest einen Wert aus.
        /// </summary>
        /// <param name="propertyName">Der Name des Wertes.</param>
        /// <param name="var">Der gewünschte Wert.</param>
        /// <param name="errorLog">Verweis auf eine Hilfsklasse, an die Fehler gemeldet werden.</param>
        void Read( [MarshalAs( UnmanagedType.LPWStr )] string propertyName, ref object var, object errorLog );

        /// <summary>
        /// Verändert einen Wert.
        /// </summary>
        /// <param name="propertyName">Der Name des Wertes.</param>
        /// <param name="var">Der neue Wert.</param>
        void Write( [MarshalAs( UnmanagedType.LPWStr )] string propertyName, ref object var );
    };

    /// <summary>
    /// Erweiterte COM Schnittstelle zur dynamischen Verwaltung von Werten.
    /// </summary>
    [
        ComImport,
        Guid( "22f55882-280b-11d0-a8a9-00a0c90c2004" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IPropertyBag2 // : IPropertyBag
    {
        /// <summary>
        /// Liest einen Wert aus.
        /// </summary>
        void Read();

        /// <summary>
        /// Setzt einen Wert.
        /// </summary>
        void Write();

        /// <summary>
        /// Meldet die Anzahl der Werte.
        /// </summary>
        UInt32 Count { get; }

        /// <summary>
        /// Ermittelt Informationen zu einem Wert.
        /// </summary>
        void GetPropertyInfo();

        /// <summary>
        /// Initialisiert die Werte.
        /// </summary>
        void LoadObject();
    };

    /// <summary>
    /// Erweiterungsmethoden zur einfacheren Nutzung der Schnittstellen.
    /// </summary>
    public static class IPropertyBagExtensions
    {
        /// <summary>
        /// Ermittelt die Anzahl der Werte in einer Werteverwaltung.
        /// </summary>
        /// <typeparam name="T">Die konkrete Art der Implementierung.</typeparam>
        /// <param name="properties">Die zugehörige Werteverwaltung.</param>
        /// <returns>Die Anzahl der Werte oder <i>null</i>, wenn diese nicht
        /// ermittelt werden kann.</returns>
        public static uint? GetCount<T>( this T properties ) where T : IPropertyBag
        {
            // Forward
            var bag2 = properties as IPropertyBag2;
            if (bag2 == null)
                return null;
            else
                return bag2.Count;
        }

        /// <summary>
        /// Ermittelt einen Wert.
        /// </summary>
        /// <typeparam name="T">Der erwartete Datentyp des Wertes.</typeparam>
        /// <param name="properties">Die Werteverwaltung.</param>
        /// <param name="name">Der Name des Wertes.</param>
        /// <returns>Der gewünschte Wert.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Werteverwaltung angegeben.</exception>
        public static T Get<T>( this IPropertyBag properties, string name )
        {
            // Validate
            if (properties == null)
                throw new ArgumentNullException( "properties" );

            // Helper
            object value = null;

            // Report
            properties.Read( name, ref value, null );
            if (value == null)
                return default( T );
            else
                return (T) value;
        }
    }
}
