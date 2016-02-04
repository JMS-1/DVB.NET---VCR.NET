using System;
using System.Runtime.Serialization;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt ein einzelnen Geräteprofil.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ProfileInfo
    {
        /// <summary>
        /// Der Name des Geräteprofils.
        /// </summary>
        [DataMember( Name = "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="profile">Das zu beschreibende Geräteprofil.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static ProfileInfo Create( ProfileState profile )
        {
            // Validate
            if (profile == null)
                throw new ArgumentNullException( nameof( profile ) );

            // Create
            return new ProfileInfo { Name = profile.ProfileName };
        }
    }
}
