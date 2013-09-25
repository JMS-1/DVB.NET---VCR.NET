using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Die Arbeitsumgebung für das Anlernen der Fernbedienung.
    /// </summary>
    public class LearnContext : INotifyPropertyChanged
    {
        /// <summary>
        /// Die Umwandlung für Eingabecodes in Zeichenketten.
        /// </summary>
        public static readonly IValueConverter InputKeyConverter = new _Converter();

        /// <summary>
        /// Wandelt Aktionscodes für die Anzeige um.
        /// </summary>
        private class _Converter : IValueConverter
        {
            /// <summary>
            /// Erzeugt einen neuen Wandler.
            /// </summary>
            public _Converter()
            {
            }

            #region IValueConverter Members

            /// <summary>
            /// Wandelt einen Rohwert.
            /// </summary>
            /// <param name="value">Der Rohwert.</param>
            /// <param name="targetType">Der gewünschte Zieldatentyp.</param>
            /// <param name="parameter">Optionaler Parameter.</param>
            /// <param name="culture">Die zu verwenende Sprachumgebung.</param>
            /// <returns>Der gewandelte Wert.</returns>
            public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            {
                // Change type
                var key = value as InputKey?;
                if (!key.HasValue)
                    return value;

                // Check target
                if (targetType != typeof( string ))
                    return value;

                // Load the resource
                var display = Properties.Resources.ResourceManager.GetString( string.Format( "InputKey_{0}", key.Value ) );
                if (string.IsNullOrEmpty( display ))
                    return value;
                else
                    return display;
            }

            /// <summary>
            /// Wandelt einen Anzeigewert.
            /// </summary>
            /// <param name="value">Der angezeigte Wert.</param>
            /// <param name="targetType">Der Zieldatentyp.</param>
            /// <param name="parameter">Optionaler Parameter für die Wandlung.</param>
            /// <param name="culture">Die zu verwendende Sprachumgebung.</param>
            /// <returns>Der Rohdatenwert.</returns>
            public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
            {
                // Not supported
                return value;
            }

            #endregion
        }

        /// <summary>
        /// Implementiert einen Befehl.
        /// </summary>
        private class _Command : ICommand
        {
            /// <summary>
            /// Prüft, ob der Befehl ausgeführt werden kann.
            /// </summary>
            private Func<bool> m_Test;

            /// <summary>
            /// Führt den Befehl tatsächlich aus.
            /// </summary>
            private Action m_Command;

            /// <summary>
            /// Erzeugt einen neuen Befehl.
            /// </summary>
            /// <param name="canExecute">Methode zur Prüfung, ob der Befehl ausgeführt werden kann.</param>
            /// <param name="execute">Methode zur Ausführung des Befehls.</param>
            public _Command( Func<bool> canExecute, Action execute )
            {
                // Remember
                m_Test = canExecute;
                m_Command = execute;
            }

            /// <summary>
            /// Meldet, wenn sich die Ausführungsbedingung verändert hat.
            /// </summary>
            public void FireChanged()
            {
                // Load notifier
                var notify = CanExecuteChanged;
                if (notify == null)
                    return;

                // Process
                notify( this, EventArgs.Empty );
            }

            #region ICommand Members

            /// <summary>
            /// Meldet, wenn sich die Ausführungssituation verändert hat.
            /// </summary>
            public event EventHandler CanExecuteChanged;

            /// <summary>
            /// Prüft, ob der Befehl ausgeführt werden kann.
            /// </summary>
            /// <param name="parameter">Ein optionaler Parameter.</param>
            /// <returns>Gesetzt, wenn eine Ausführung möglich ist.</returns>
            public bool CanExecute( object parameter )
            {
                // Forward
                return m_Test();
            }

            /// <summary>
            /// Führt den Befehl aus.
            /// </summary>
            /// <param name="parameter">Ein optionaler Parameter.</param>
            public void Execute( object parameter )
            {
                // Forward
                m_Command();
            }

            #endregion
        }

        /// <summary>
        /// Meldet die zugehörige Konfiguration.
        /// </summary>
        public RCSettings Configuration { get; private set; }

        /// <summary>
        /// Die verwaltete Konfigurationdatei.
        /// </summary>
        public FileInfo Path { get; private set; }

        /// <summary>
        /// Die aktuelle Auswahl.
        /// </summary>
        private InputKey m_CurrentKey;

        /// <summary>
        /// Die aktuelle Liste der gesammelten Codes.
        /// </summary>
        private List<MappingItem> m_Sequence = new List<MappingItem>();

        /// <summary>
        /// Die aktuelle Länge der Eingabesequenz.
        /// </summary>
        public int SequenceLength
        {
            get
            {
                // Report
                return m_Sequence.Count;
            }
        }

        /// <summary>
        /// Verändert die aktuelle Auswahl.
        /// </summary>
        public InputKey CurrentKey
        {
            get
            {
                // Report
                return m_CurrentKey;
            }
            set
            {
                // Remember
                m_CurrentKey = value;

                // Refresh buttons
                RefreshButtons( ClearKeySequences );

                // Reset sequence
                RegisterSequence( null );
            }
        }

        /// <summary>
        /// Befehl zum Ergänzen einer neuen Sequenz.
        /// </summary>
        public ICommand AddNewSequence { get; private set; }

        /// <summary>
        /// Befehl zum Entfernen der Sequenzen einer Aktion.
        /// </summary>
        public ICommand ClearKeySequences { get; private set; }

        /// <summary>
        /// Befehl zum Aktualisieren der Konfiguration-
        /// </summary>
        public ICommand SaveConfiguration { get; private set; }

        /// <summary>
        /// Befehl zum Leeren der Gesamtkonfiguration.
        /// </summary>
        public ICommand ClearConfiguration { get; private set; }

        /// <summary>
        /// Befehl zum Beenden der Anwendung ohne zu Speichern.
        /// </summary>
        public ICommand Exit { get; private set; }

        /// <summary>
        /// Gesetzt, sobald die Konfiguration verändert wurde.
        /// </summary>
        private bool m_Changed = false;

        /// <summary>
        /// Kann zu Designzwecken verwendet werden.
        /// </summary>
        public LearnContext()
        {
            // Define buttons
            ClearConfiguration = new _Command( () => true, () => { } );
            ClearKeySequences = new _Command( () => true, () => { } );
            SaveConfiguration = new _Command( () => true, () => { } );
            AddNewSequence = new _Command( () => true, () => { } );
            Exit = new _Command( () => true, () => { } );

            // Load defaults
            Path = new FileInfo( @"c:\temp\test.xml" );
            Configuration = new RCSettings();

            // Finish
            CurrentKey = AllKeys[0];
        }

        /// <summary>
        /// Erzeugt eine neue Arbeitsumgebung.
        /// </summary>
        /// <param name="path">Die zu verwendende Konfigurationsdatei.</param>
        /// <param name="exitAction">Wird beim Beenden aufgerufen.</param>
        private LearnContext( FileInfo path, Action exitAction )
        {
            // Reset the whole configuration
            ClearConfiguration =
                new _Command( () => Configuration.Mappings.Length > 0, () =>
                    {
                        // Process
                        Configuration.Mappings = null;

                        // We changed
                        m_Changed = true;

                        // Refresh nearly all
                        RefreshButtons( ClearConfiguration, ClearKeySequences, SaveConfiguration );
                    } );


            // Reset the configuration of a single key
            ClearKeySequences =
                new _Command( () => Configuration.Mappings.Any( m => m.Meaning == m_CurrentKey ), () =>
                    {
                        // Process
                        Configuration.Mappings = Configuration.Mappings.Where( m => m.Meaning != m_CurrentKey ).ToArray();

                        // We changed
                        m_Changed = true;

                        // Refresh
                        RefreshButtons( ClearConfiguration, ClearKeySequences, SaveConfiguration );
                    } );

            // Save the configuration and leave
            SaveConfiguration =
                new _Command( () => m_Changed, () =>
                    {
                        // Safe process
                        try
                        {
                            // Send to disk
                            Configuration.Save( Path.FullName );

                            // Done
                            Exit.Execute( null );
                        }
                        catch (Exception e)
                        {
                            // Show error
                            MessageBox.Show( e.Message, Properties.Resources.RC_Learn_SaveError );
                        }
                    } );

            // Register a new sequence
            AddNewSequence =
                new _Command( () => m_Sequence.Count > 0, () =>
                    {
                        // Test for add
                        if (m_Sequence.Count > 0)
                        {
                            // Create mapping
                            var mapping = new InputMapping { Meaning = m_CurrentKey };

                            // Fill sequence
                            mapping.Items.AddRange( m_Sequence );

                            // Add if not already there
                            if (Configuration.Add( mapping ))
                            {
                                // Now we changed
                                m_Changed = true;
                            }
                        }

                        // Reset
                        RegisterSequence( null );

                        // Refresh
                        RefreshButtons( ClearKeySequences, ClearConfiguration, SaveConfiguration );
                    } );

            // Other buttons
            Exit = new _Command( () => true, exitAction );

            // Remember
            Path = path;

            // Load configuration
            if (path.Exists)
                Configuration = RCSettings.Load( path.FullName );
            else
                Configuration = new RCSettings();

            // Finish
            CurrentKey = AllKeys[0];
        }

        /// <summary>
        /// Meldet alle möglichen Eingabesequenzen.
        /// </summary>
        public static InputKey[] AllKeys
        {
            get
            {
                // Report
                return Enum.GetValues( typeof( InputKey ) ).Cast<InputKey>().OrderBy( k => k.ToString() ).ToArray();
            }
        }

        /// <summary>
        /// Erzeugt eine neue Arbeitsumgebung.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Konfigurationsdatei - diese wird
        /// bei Bedarf angelegt, wenn sie noch nicht exitiert.</param>
        /// <param name="exitAction">Wird zum Beenden aufgerufen. Ist dieser Parameter nicht angegeben,
        /// so wird die Anwendung beendet.</param>
        /// <returns>Die angeforderte neue Umgebung.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Datei angegeben.</exception>
        public static LearnContext Create( string path, Action exitAction )
        {
            // Validate
            if (string.IsNullOrEmpty( path ))
                throw new ArgumentNullException( "path" );

            // Create a file information - will validate
            var file = new FileInfo( path );

            // Defaults
            if (exitAction == null)
                exitAction = () => Application.Current.Shutdown();

            // Process
            return new LearnContext( file, exitAction );
        }

        /// <summary>
        /// Meldet, dass sich die Bedingungen für die Ausführung eines Befehls nun verändert haben.
        /// </summary>
        /// <param name="commands">Der zu verändernde Befehl.</param>
        private static void RefreshButtons( params ICommand[] commands )
        {
            // Forward
            foreach (_Command command in commands)
                command.FireChanged();
        }

        /// <summary>
        /// Meldet eine neue Sequenz von Eingaben an.
        /// </summary>
        /// <param name="item">Eine neue Eingabe.</param>
        public void RegisterSequence( MappingItem item )
        {
            // Update
            if (item == null)
                m_Sequence.Clear();
            else
                m_Sequence.Add( item );

            // Notify commands
            RefreshButtons( AddNewSequence );

            // Check notify
            var notify = PropertyChanged;
            if (notify == null)
                return;

            // Report
            notify( this, new PropertyChangedEventArgs( "SequenceLength" ) );
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Wird bei Veränderungen ausgelöst.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
