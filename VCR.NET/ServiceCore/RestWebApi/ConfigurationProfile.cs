using System;
using System.Runtime.Serialization;
using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt ein Geräteprofil, so wie es für die Konfiguration verwendet werden soll.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ConfigurationProfile
    {
        /// <summary>
        /// Der Name des Geräteprofils.
        /// </summary>
        [DataMember( Name = "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Gesetzt, wenn das Geräteprofil für Aufzeichnungen verwendet wird.
        /// </summary>
        [DataMember( Name = "active" )]
        public bool UsedForRecording { get; set; }

        /// <summary>
        /// Die maximale Anzahl von Quellen, die gleichzeitig aufgezeichnet werden dürfen.
        /// </summary>
        [DataMember( Name = "sourceLimit" )]
        public uint SourceLimit { get; set; }

        /// <summary>
        /// Die maximale Anzahl von gleichzeitig entschlüsselbaren Quellen.
        /// </summary>
        [DataMember( Name = "ciLimit" )]
        public uint DecryptionLimit { get; set; }

        /// <summary>
        /// Die Planungspriorität.
        /// </summary>
        [DataMember( Name = "priority" )]
        public uint Priority { get; set; }

        /// <summary>
        /// Liest eine Einstellung aus.
        /// </summary>
        /// <param name="profile">Ein Geräteprofil.</param>
        /// <param name="settingName">Der Name der Einstellung.</param>
        /// <param name="settingDefault">Die Voreinstellung der Einstellung.</param>
        /// <returns>Der aktuelle Wert, gegebenenfalls die Voreinstellung.</returns>
        private static uint ReadSetting( Profile profile, string settingName, uint settingDefault )
        {
            // Check value
            var settings = profile.GetParameter( settingName );
            if (string.IsNullOrEmpty( settings ))
                return settingDefault;

            // Check value
            if (uint.TryParse(settings, out uint value))
                return value;
            else
                return settingDefault;
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="profile">Das zugehörige Geräteprpfil.</param>
        /// <param name="active">Gesetzt, wenn das Geräteprofil verwendet wird.</param>
        /// <returns>Die angeforderte Beschreibung.</returns>
        public static ConfigurationProfile Create( Profile profile, bool active )
        {
            // Report
            return
                new ConfigurationProfile
                {
                    SourceLimit = ReadSetting( profile, ProfileScheduleResource.ParallelSourceLimitName, ProfileScheduleResource.DefaultParallelSourceLimit ),
                    DecryptionLimit = ReadSetting( profile, ProfileScheduleResource.DecryptionLimitName, ProfileScheduleResource.DefaultDecryptionLimit ),
                    Priority = ReadSetting( profile, ProfileScheduleResource.SchedulePriorityName, ProfileScheduleResource.DefaultSchedulePriority ),
                    UsedForRecording = active,
                    Name = profile.Name,
                };
        }

        /// <summary>
        /// Aktualisiert eine einzelne Eigenschaft.
        /// </summary>
        /// <param name="profile">Ein Geräteprofil.</param>
        /// <param name="propertyName">Der Name der Eigenschaft.</param>
        /// <param name="currentValue">Der aktuelle Wert der Eigenschaft.</param>
        /// <param name="defaultValue">Der bevorzugte Wert der Eigenschaft.</param>
        /// <returns>Gesetzt, wenn die Eigenschaft verändert wurde.</returns>
        private static bool Update( Profile profile, string propertyName, uint currentValue, uint defaultValue )
        {
            // Read the current setting
            var newValue = ReadSetting( profile, propertyName, defaultValue );
            if (newValue == currentValue)
                return false;

            // Reset
            profile.Parameters.RemoveAll( parameter => propertyName.Equals( parameter.Name ) );

            // Add
            if (currentValue != defaultValue)
                profile.Parameters.Add( new ProfileParameter( propertyName, currentValue ) );

            // Did it
            return true;
        }

        /// <summary>
        /// Führt Änderungen am Geräteprofil aus.
        /// </summary>
        /// <param name="profile">Das Geräteprofil.</param>
        /// <returns>Gesetzt, wenn etwas verändert wurde.</returns>
        public bool WriteBack( Profile profile )
        {
            // Process all
            var newDecryption = Update( profile, ProfileScheduleResource.DecryptionLimitName, DecryptionLimit, ProfileScheduleResource.DefaultDecryptionLimit );
            var newSource = Update( profile, ProfileScheduleResource.ParallelSourceLimitName, SourceLimit, ProfileScheduleResource.DefaultParallelSourceLimit );
            var newPriority = Update( profile, ProfileScheduleResource.SchedulePriorityName, Priority, ProfileScheduleResource.DefaultSchedulePriority );
            var changed = newPriority || newDecryption || newSource;

            // Must persist
            if (changed)
                profile.Save();

            // Done
            return changed;
        }
    }
}
