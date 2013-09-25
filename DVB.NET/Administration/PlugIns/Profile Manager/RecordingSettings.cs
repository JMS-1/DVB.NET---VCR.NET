using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVB.Administration.ProfileManager
{
    /// <summary>
    /// Erlaubt die Einstellung zusätzliche Parameter für Aufzeichnungen.
    /// </summary>
    public partial class RecordingSettings : Form
    {
        /// <summary>
        /// Die Anzahl von Sekunden zur Überprüfung, ob überhaupt noch Daten auf dem gesamtem Datenstrom empfangen werden.
        /// </summary>
        public const string StreamWatchDogIntervalName = "CardServer.StreamWatchDogSeconds";

        /// <summary>
        /// Die Anzahl von Sekunden zur Überprüfung, ob nach einer Entschlüsselung überhaupt Nutzdaten empfangen werden.
        /// </summary>
        public const string DecryptWatchDogIntervalName = "CardServer.DecryptionWatchDogSeconds";

        /// <summary>
        /// Die Anzahl von Sekunden zur Überprüfung, ob sich die Senderdaten verändert haben.
        /// </summary>
        public const string PSIWatchDogIntervalName = "CardServer.SourceWatchDogSeconds";

        /// <summary>
        /// Alle Parameter, die mit diesem Dialog verwaltet werden.
        /// </summary>
        private static readonly string[] s_Settings =
            {
                // Einstellungen für den Gerätedienst
                StreamWatchDogIntervalName,
                DecryptWatchDogIntervalName,
                PSIWatchDogIntervalName,
                // Einstellungen für den Zeitplaner
                ProfileScheduleResource.ParallelSourceLimitName,
                ProfileScheduleResource.DecryptionLimitName,
                ProfileScheduleResource.SchedulePriorityName,
            };

        /// <summary>
        /// Die von diesem Formular verwalteten Werte.
        /// </summary>
        private Dictionary<string, string> m_Settings;

        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        /// <param name="settings">Die zu verwaltenden Werte.</param>
        public RecordingSettings( Dictionary<string, string> settings )
        {
            // Remember
            m_Settings = settings;

            // Load designer stuff
            InitializeComponent();

            // Connect controls to settings
            udSourceLimit.Tag = ProfileScheduleResource.ParallelSourceLimitName;
            udDecLimit.Tag = ProfileScheduleResource.DecryptionLimitName;
            udPrio.Tag = ProfileScheduleResource.SchedulePriorityName;
            udDecrypt.Tag = DecryptWatchDogIntervalName;
            udGroup.Tag = StreamWatchDogIntervalName;
            udSource.Tag = PSIWatchDogIntervalName;
            ckSourceLimit.Tag = udSourceLimit;
            ckDecLimit.Tag = udDecLimit;
            ckDecrypt.Tag = udDecrypt;
            ckSource.Tag = udSource;
            ckGroup.Tag = udGroup;
            ckPrio.Tag = udPrio;

            // Startup
            ReadSetting( ckSourceLimit, ProfileScheduleResource.DefaultParallelSourceLimit );
            ReadSetting( ckDecLimit, ProfileScheduleResource.DefaultDecryptionLimit );
            ReadSetting( ckPrio, ProfileScheduleResource.DefaultSchedulePriority );
            ReadSetting( ckDecrypt, 10 );
            ReadSetting( ckSource, 0 );
            ReadSetting( ckGroup, 15 );
        }

        /// <summary>
        /// Liest Einstellungen aus dem Geräteprofil.
        /// </summary>
        /// <param name="check">Die Auswahl der Einstellung.</param>
        /// <param name="defaultValue">Die Voreinstellung.</param>
        private void ReadSetting( CheckBox check, uint defaultValue )
        {
            // Load the corresponding data
            var valueDisplay = (NumericUpDown) check.Tag;
            var parameterName = (string) valueDisplay.Tag;
            var currentValue = m_Settings[parameterName];

            // Try read
            uint value;
            if (!uint.TryParse( currentValue, out value ))
            {
                // Should mean use the default
                check.CheckState = CheckState.Indeterminate;
                valueDisplay.Enabled = false;

                // Be safe
                m_Settings[parameterName] = null;

                // Value to show
                value = defaultValue;
            }

            // See if this is disabled
            else if ((value == 0) && (value != defaultValue))
            {
                // Visualize
                check.CheckState = CheckState.Unchecked;
                valueDisplay.Enabled = false;

                // Value to show
                value = defaultValue;
            }

            // Move in range
            else
            {
                // Visualize
                check.CheckState = CheckState.Checked;
            }

            // Load value
            valueDisplay.Value = Math.Max( valueDisplay.Minimum, Math.Min( valueDisplay.Maximum, value ) );
        }

        /// <summary>
        /// Liest Einstellungen aus dem Geräteprofil.
        /// </summary>
        /// <param name="check">Die Auswahl der Einstellung.</param>
        /// <param name="defaultValue">Die Voreinstellung.</param>
        private void ReadSetting( CheckBox check, bool defaultValue )
        {
            // Load the corresponding data
            var parameterName = (string) check.Tag;
            var currentValue = m_Settings[parameterName];

            // Try read
            bool value;
            if (!bool.TryParse( currentValue, out value ))
                value = defaultValue;

            // Activate UI
            check.Checked = value;
        }

        /// <summary>
        /// Ermittelt die Sondereinstellungen für Aufzeichnungen.
        /// </summary>
        /// <param name="profile">Ein Geräteprofil.</param>
        /// <returns>Alle Einstellungen, die auf diesem Dialog verwaltet werden.</returns>
        internal static Dictionary<string, string> ExtractSettings( Profile profile )
        {
            // Process
            return s_Settings.ToDictionary( n => n, n => profile.GetParameter( n ) );
        }

        /// <summary>
        /// Eine Einstellung wurde verändert.
        /// </summary>
        /// <param name="sender">Das auslösende Element.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void CheckStateChanged( object sender, EventArgs e )
        {
            // Load all
            var check = (CheckBox) sender;
            var valueDisplay = (NumericUpDown) check.Tag;

            // Update
            valueDisplay.Enabled = (check.CheckState == CheckState.Checked);
        }

        /// <summary>
        /// Der Anwender möchte die aktuellen Daten speichern.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ingoriert.</param>
        private void cmdSave_Click( object sender, EventArgs e )
        {
            // Remap - we do not allow to disable the source check
            if (ckSource.CheckState == CheckState.Unchecked)
                ckSource.CheckState = CheckState.Indeterminate;

            // Update all
            WriteNumberSetting( ckSourceLimit );
            WriteNumberSetting( ckDecLimit );
            WriteNumberSetting( ckDecrypt );
            WriteNumberSetting( ckSource );
            WriteNumberSetting( ckGroup );
            WriteNumberSetting( ckPrio );
        }

        /// <summary>
        /// Überträgt Einstellungen in das Geräteprofil.
        /// </summary>
        /// <param name="check">Ein Oberflächenelement.</param>
        private void WriteBooleanSetting( CheckBox check )
        {
            // Attach to the data
            var parameterName = (string) check.Tag;

            // Transfer
            m_Settings[parameterName] = check.Checked.ToString();
        }

        /// <summary>
        /// Überträgt Einstellungen in das Geräteprofil.
        /// </summary>
        /// <param name="check">Ein Oberflächenelement.</param>
        private void WriteNumberSetting( CheckBox check )
        {
            // Attach to the data
            var valueDisplay = (NumericUpDown) check.Tag;
            var parameterName = (string) valueDisplay.Tag;

            // Check mode
            switch (check.CheckState)
            {
                case CheckState.Checked: m_Settings[parameterName] = ((uint) valueDisplay.Value).ToString(); break;
                case CheckState.Indeterminate: m_Settings[parameterName] = null; break;
                case CheckState.Unchecked: m_Settings[parameterName] = "0"; break;
            }
        }
    }
}
