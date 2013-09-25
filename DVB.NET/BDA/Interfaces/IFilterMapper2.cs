using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Verwaltet die Abbildung von Filtern.
    /// </summary>
    [
        ComImport,
        Guid( "b79bb0b0-33c1-11d1-abe1-00a0c905f375" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IFilterMapper2
    {
        /// <summary>
        /// Legt eine neue Kategorie von Filter an.
        /// </summary>
        void CreateCategory();

        /// <summary>
        /// Meldet einen Filter ab.
        /// </summary>
        void UnregisterFilter();

        /// <summary>
        /// Meldet einen Filter an.
        /// </summary>
        void RegisterFilter();

        /// <summary>
        /// Meldet alle passenden Filter.
        /// </summary>
        /// <param name="monikers">Eine Auflistung über die passenden Filter.</param>
        /// <param name="flags">Optionen zur durchzuführenden Suche.</param>
        /// <param name="exactMatch">Beschreibt die Art des Vergleichs.</param>
        /// <param name="merit">Beschreibt die Auswertung des Filtermerits.</param>
        /// <param name="inputNeeded">Teilt mit, ob Eingänge benötigt werden.</param>
        /// <param name="inputTypes">Anzahl der Datenformate für die Eingänge.</param>
        /// <param name="inputTypeArray">Erlaubte Datentypen für die Eingänge.</param>
        /// <param name="mediaTypeIn">Eingangsseitiges Datenformat.</param>
        /// <param name="categoryIn">Eingangsseitige Filterkategorie.</param>
        /// <param name="render">Beschreibt die Anforderung an die Darstellung.</param>
        /// <param name="outputNeeded">Legt fest, ob Ausgänge benötigt werden.</param>
        /// <param name="outputTypes">Anzahl der Datenformat für die Ausgänge.</param>
        /// <param name="outputTypeArray">Erlaubte Datentypen für die Ausgänge.</param>
        /// <param name="mediaTypeOut">Ausgangsseitiges Datenformat.</param>
        /// <param name="categoryOut">Ausgangsseitige Filterkategorie.</param>
        void EnumMatchingFilters( out IEnumMoniker monikers, Int32 flags, Int32 exactMatch, UInt32 merit, Int32 inputNeeded, Int32 inputTypes, IntPtr inputTypeArray, IntPtr mediaTypeIn, IntPtr categoryIn, Int32 render, Int32 outputNeeded, Int32 outputTypes, IntPtr outputTypeArray, IntPtr mediaTypeOut, IntPtr categoryOut );
    }
}




