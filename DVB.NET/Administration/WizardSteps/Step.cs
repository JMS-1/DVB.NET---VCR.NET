using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using JMS.DVB.Administration;
using System.Collections.Generic;

namespace DVBNETAdmin.WizardSteps
{
    /// <summary>
    /// Auf Basis dieser Komponente sind alle Arbeitsschritte der Administration
    /// implementiert.
    /// </summary>
    public partial class Step : UserControl
    {
        /// <summary>
        /// Initialisiert einen neuen Arbeitsschritt.
        /// </summary>
        public Step()
        {
            // Copy designer stuff
            InitializeComponent();
        }

        /// <summary>
        /// Meldet die Administration selbst.
        /// </summary>
        public AdminMain MainForm
        {
            get
            {
                // Report
                return (AdminMain) this.ParentForm;
            }
        }

        /// <summary>
        /// Meldet die Überschrift für diesen Arbeitsschritt.
        /// </summary>
        public virtual string Headline
        {
            get
            {
                // Report
                return GetType().FullName;
            }
        }

        /// <summary>
        /// Meldet, ob der nächste Arbeitsschritt ausgelöst werden darf.
        /// </summary>
        public virtual bool EnableNext
        {
            get
            {
                // None
                return false;
            }
        }

        /// <summary>
        /// Meldet, ob der gesamte Arbeitsvorgang abgeschlossen werden kann.
        /// </summary>
        public virtual bool EnableFinish
        {
            get
            {
                // None
                return false;
            }
        }

        /// <summary>
        /// Führt den nächsten Arbeitsschritt aus.
        /// </summary>
        public virtual void NextStep()
        {
        }

        /// <summary>
        /// Aktualisiert die Anzeige der Schaltflächen.
        /// </summary>
        protected void CheckButtons()
        {
            // Forward
            MainForm.CheckButtons();
        }

        /// <summary>
        /// Meldet den aktuellen Zustand einer bestimmten Art.
        /// </summary>
        /// <typeparam name="T">Die Art des Zustands.</typeparam>
        /// <returns>Der aktuelle Zustand oder ein neu initialisierter Wert.</returns>
        public T GetState<T>()
        {
            // Forward
            return GetState<T>( GetType() );
        }

        /// <summary>
        /// Meldet den Zustand zu einer bestimmten Art von Arbeitsschritt.
        /// </summary>
        /// <typeparam name="T">Die Art des Zustands.</typeparam>
        /// <param name="stepType">Die Art des Arbeitsschritts.</param>
        /// <returns>Der aktuelle Zustand oder ein neu initialisierter Wert.</returns>
        public T GetState<T>( Type stepType )
        {
            // Ask form
            object untypedState;
            if (MainForm.States.TryGetValue( stepType, out untypedState ))
                return (T) untypedState;
            else
                return default( T );
        }

        /// <summary>
        /// Meldet den Zustand zu einer bestimmten Art von Arbeitsschritt.
        /// </summary>
        /// <typeparam name="T">Die Art des Zustands.</typeparam>
        /// <typeparam name="S">Die Art des Arbeitsschritts.</typeparam>
        /// <returns>Der aktuelle Zustand oder ein neu initialisierter Wert.</returns>
        public T GetState<T, S>() where S : Step
        {
            // Forward
            return GetState<T>( typeof( S ) );
        }

        /// <summary>
        /// Vermerkt einen Zustand für diesen Arbeitsschritt.
        /// </summary>
        /// <typeparam name="T">Die Art des Zustands.</typeparam>
        /// <param name="state">Der neue Zustandswert.</param>
        public void SetState<T>( T state )
        {
            // Forward
            SetState( GetType(), state );
        }

        /// <summary>
        /// Vermerkt einen Zustand für einen Arbeitsschritt.
        /// </summary>
        /// <typeparam name="T">Die Art des Zustands.</typeparam>
        /// <param name="stepType">Die Art des Arbeitsschritts.</param>
        /// <param name="state">Der neue Zustandswert.</param>
        public void SetState<T>( Type stepType, T state )
        {
            // Send to form
            MainForm.States[stepType] = state;
        }

        /// <summary>
        /// Vermerkt einen Zustand für einen Arbeitsschritt.
        /// </summary>
        /// <typeparam name="T">Die Art desZustands.</typeparam>
        /// <typeparam name="S">Die Art des Arbeitsschritts.</typeparam>
        /// <param name="state">Der neue Zustandswert.</param>
        public void SetState<T, S>( T state ) where S : Step
        {
            // Forward
            SetState( typeof( S ), state );
        }

        /// <summary>
        /// Beginnt mit der Ausführung einer Aufgabe.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe synchron abgeschlossen wurde.</returns>
        public virtual bool StartOperation()
        {
            // Not in base class
            throw new NotSupportedException();
        }

        /// <summary>
        /// Meldet, ob <see cref="StartOperation"/> aufgerufen werden darf.
        /// </summary>
        /// <returns>Gesetzt, wenn ein Aufruf erlaubt ist.</returns>
        public virtual bool CanStartOperation()
        {
            // Yes, we can
            return true;
        }

        /// <summary>
        /// Meldet, ob eine einmal gestartete Operation auch wieder abgebrochen werden kann.
        /// </summary>
        public virtual bool CanCancel
        {
            get
            {
                // Report
                return false;
            }
        }
    }

    /// <summary>
    /// Einige Erweiterungemethoden für das Arbeiten mit Arbeitsschritten.
    /// </summary>
    public static class StepExtensions
    {
    }
}
