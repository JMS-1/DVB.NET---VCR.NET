using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Administration
{
    /// <summary>
    /// Diese Schnittstelle wird von der Anwendung angeboten, in die Erweiterungen
    /// geladen werden.
    /// </summary>
    public interface IPlugInUISite
    {
        /// <summary>
        /// Fordert zur Aktualisierung der Anzeige auf - betroffen sind im Allgemeinen Schaltflächen.
        /// </summary>
        void UpdateGUI();

        /// <summary>
        /// Meldet, ob der Anwender die Aufgabe vorzeitig beenden will.
        /// </summary>
        bool HasBeenCancelled { get; }

        /// <summary>
        /// Wird aufgerufen, sobald eine Aufgabe abgeschlossen wurde.
        /// </summary>
        void OperationDone();

        /// <summary>
        /// Wählt ein bestimmtes Geräteprofil für die nächsten Aufgaben.
        /// </summary>
        /// <param name="profileName">Der Name des gewünschten Profils.</param>
        void SelectProfile( string profileName );

        /// <summary>
        /// Wählt eine andere Erweiterung zur Ausführung aus.
        /// </summary>
        /// <param name="plugInType">Die Art der gwünschten Erweiterung.</param>
        void SelectNextPlugIn( Type plugInType );
    }
}
