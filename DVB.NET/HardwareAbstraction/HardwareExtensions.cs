using System;
using System.Collections.Generic;
using System.Linq;


namespace JMS.DVB
{
    /// <summary>
    /// Einige Erweiterungsmethoden zum Verbinden der abstrakten Quellinformationen 
    /// mit einer konkreten Implementierung für eine Hardware.
    /// </summary>
    public static class HardwareExtensions
    {
        /// <summary>
        /// Aktiviert eine Quellinformation auf einer bestimmten Hardware.
        /// </summary>
        /// <param name="selection">Die gewünschte Quellinformation.</param>
        /// <param name="hardware">Die Hardware, die für den Empfang zu verwenden ist.</param>
        /// <exception cref="ArgumentNullException">Es wurden keine Informationen zur
        /// Quelle angegeben.</exception>
        public static void SelectGroup( this SourceSelection selection, Hardware hardware )
        {
            // Validate
            if (null == selection)
                throw new ArgumentNullException( "selection" );

            // Nothing to do
            if (null == hardware)
                return;

            // Forward
            hardware.SelectGroup( selection );
        }

        /// <summary>
        /// Aktiviert eine Quellinformation.
        /// </summary>
        /// <param name="selection">Die gewünschte Quellinformation.</param>
        /// <exception cref="ArgumentNullException">Es wurden keine Informationen zur
        /// Quelle angegeben.</exception>
        public static void SelectGroup( this SourceSelection selection )
        {
            // Validate
            if (null == selection)
                throw new ArgumentNullException( "selection" );

            // Forward
            using (HardwareManager.Open())
                selection.SelectGroup( selection.GetHardware() );
        }

        /// <summary>
        /// Ermittelt die Geräteabstraktion zu einer Quellinformation.
        /// </summary>
        /// <param name="selection">Die betroffene Quellinformation.</param>
        /// <returns>Die zugehörige Geräteabstraktion.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellinformation angegeben.</exception>
        public static Hardware GetHardware( this SourceSelection selection )
        {
            // Validate
            if (null == selection)
                throw new ArgumentNullException( "selection" );

            // Forward
            using (HardwareManager.Open())
                return HardwareManager.OpenHardware( selection.ProfileName );
        }

        /// <summary>
        /// Ermittelt das Geräteprofile mit der Senderliste für eine Quellinformation.
        /// </summary>
        /// <param name="selection">Die betroffene Quellinformation.</param>
        /// <returns>Das zugehörige Profil mit der Senderliste, das durchaus von <see cref="GetProfile"/>
        /// abweichen kann.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellinformation angegeben.</exception>
        public static Profile GetLeafProfile( this SourceSelection selection )
        {
            // Forward
            Profile profile = selection.GetProfile();

            // Check result
            if (null == profile)
                return null;
            else
                return profile.LeafProfile;
        }

        /// <summary>
        /// Ermittelt das Geräteprofile für eine Quellinformation.
        /// </summary>
        /// <param name="selection">Die betroffene Quellinformation.</param>
        /// <returns>Das zugehörige Profil.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellinformation angegeben.</exception>
        public static Profile GetProfile( this SourceSelection selection )
        {
            // Validate
            if (null == selection)
                throw new ArgumentNullException( "selection" );

            // Forward
            if (string.IsNullOrEmpty( selection.ProfileName ))
                return null;
            else
                return ProfileManager.FindProfile( selection.ProfileName );
        }

        /// <summary>
        /// Erzeugt einen Aufzeichnungskontext für eine Quelle.
        /// </summary>
        /// <param name="source">Die Informationen zur Quelle.</param>
        /// <param name="hardware">Das zu verwendende Gerät.</param>
        /// <param name="profile">Opetional ein Geräteprofil mit der zugehörigen Senderliste.</param>
        /// <param name="streams">Die gewünschten Aufzeichnungsparameter.</param>
        /// <returns>Eine Kontrollinstanz für die Aufzeichnung. Diese muss mittels <see cref="IDisposable.Dispose"/>
        /// freigegeben werden.</returns>
        public static SourceStreamsManager Open( this SourceIdentifier source, Hardware hardware, Profile profile, StreamSelection streams )
        {
            // Forward
            return new SourceStreamsManager( hardware, profile, source, streams );
        }

        /// <summary>
        /// Erzeugt einen Aufzeichnungskontext für eine Quelle.
        /// </summary>
        /// <param name="selection">Die Informationen zur Quelle.</param>
        /// <param name="streams">Die gewünschten Aufzeichnungsparameter.</param>
        /// <returns>Eine Kontrollinstanz für die Aufzeichnung. Diese muss mittels <see cref="IDisposable.Dispose"/>
        /// freigegeben werden.</returns>
        public static SourceStreamsManager Open( this SourceSelection selection, StreamSelection streams )
        {
            // Validate
            if (selection == null)
                throw new ArgumentNullException( "selection" );
            else
                return selection.Source.Open( selection.GetHardware(), selection.GetLeafProfile(), streams );
        }

        /// <summary>
        /// Ermittelt einen Geräteparameter.
        /// </summary>
        /// <param name="profile">Ein Profil, das ausgewertet werden soll.</param>
        /// <param name="name">Der Name des Parameters.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public static string GetDeviceAspect( this Profile profile, string name )
        {
            // Validate
            if (profile == null)
                throw new ArgumentNullException( "profile" );
            else
                return profile.DeviceAspects.GetDeviceAspect( name );
        }

        /// <summary>
        /// Ermittelt einen Geräteparameter.
        /// </summary>
        /// <param name="aspects">Eine Liste von Parametern.</param>
        /// <param name="name">Der Name des Parameters.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        /// <exception cref="ArgumentNullException">Der Name wurde nicht angegeben.</exception>
        public static string GetDeviceAspect( this IEnumerable<DeviceAspect> aspects, string name )
        {
            // Find
            if (aspects == null)
                return null;
            else
                return aspects.Where( a => string.Equals( name, a.Aspekt ) ).Select( a => a.Value ).FirstOrDefault();
        }

        /// <summary>
        /// Ermittelt einen Parameter.
        /// </summary>
        /// <param name="profile">Ein Profil, das ausgewertet werden soll.</param>
        /// <param name="name">Der Name des Parameters.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public static string GetParameter( this Profile profile, string name )
        {
            // Validate
            if (profile == null)
                throw new ArgumentNullException( "profile" );
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );

            // Find
            return profile.Parameters.GetParameter( name );
        }

        /// <summary>
        /// Ermittelt einen Parameter.
        /// </summary>
        /// <param name="parameters">Die Liste der zu verwendenden Parameter.</param>
        /// <param name="name">Der Name des Parameters.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public static string GetParameter( this IEnumerable<ProfileParameter> parameters, string name )
        {
            // Validate
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );

            // Find
            if (parameters == null)
                return null;
            else
                return parameters.Where( p => name.Equals( p.Name ) ).Select( p => p.Value ).FirstOrDefault();
        }

        /// <summary>
        /// Ermittelt einen Parameter.
        /// </summary>
        /// <param name="parameters">Die Liste der zu verwendenden Parameter.</param>
        /// <param name="type">Die Art der Erweiterung, die einen Sonderwert sucht.</param>
        /// <param name="name">Der Name des Parameters.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public static string GetParameter( this IEnumerable<ProfileParameter> parameters, PipelineTypes type, string name )
        {
            // Forward
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );
            else
                return parameters.GetParameter( string.Format( "{0}{1}", ProfileParameter.GetPrefixForExtensionParameter( type ), name ) );
        }

        /// <summary>
        /// Ermittelt zu einer Quelle die Sonderwünsche für einen Sendersuchlauf.
        /// </summary>
        /// <param name="profile">Das betroffene Geräteprofil.</param>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die vorzunehmenden Modifikationen.</returns>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public static SourceModifier GetFilter( this Profile profile, SourceIdentifier source )
        {
            // Validate
            if (null == profile)
                throw new ArgumentNullException( "profile" );
            if (null == source)
                throw new ArgumentNullException( "source" );

            // Process
            if (null == profile.ScanConfiguration)
                return new SourceModifier { Network = source.Network, TransportStream = source.TransportStream, Service = source.Service };
            else
                return profile.ScanConfiguration.GetFilter( source );
        }

        /// <summary>
        /// Ermittelt die Einschränkungen des Sendersuchlaufs bezüglich einer Quellgruppe (Transponder).
        /// </summary>
        /// <param name="profile">Das betroffene Geräteprofil.</param>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        /// <returns>Informationen zur Quellgruppe.</returns>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public static SourceGroupFilter GetFilter( this Profile profile, SourceGroup group )
        {
            // Validate
            if (null == profile)
                throw new ArgumentNullException( "profile" );
            if (null == group)
                throw new ArgumentNullException( "group" );

            // Forward to helper
            if (null == profile.ScanConfiguration)
                return new SourceGroupFilter();
            else
                return profile.ScanConfiguration.GetFilter( group );
        }

        /// <summary>
        /// Erzeugt eine Kopie der angegebenen Quellgruppe.
        /// </summary>
        /// <param name="group">Die ursprüngliche Quellgruppe, wobei <i>null</i> erlaubt ist.</param>
        /// <returns>Eine Kopie der Quellgruppe oder <i>null</i>.</returns>
        public static T CloneGroup<T>( this T group ) where T : SourceGroup
        {
            // Create it
            if (group == null)
                return null;
            else
                return SourceGroup.FromString<T>( group.ToString() );
        }

        /// <summary>
        /// Erzeugt eine Kopie der angegebenen Ursprungs.
        /// </summary>
        /// <param name="location">Der ursprüngliche Ursprung, wobei <i>null</i> erlaubt ist.</param>
        /// <returns>Eine Kopie des Ursprungs oder <i>null</i>.</returns>
        public static T CloneLocation<T>( this T location ) where T : GroupLocation
        {
            // Create it
            if (location == null)
                return null;
            else
                return GroupLocation.FromString<T>( location.ToString() );
        }
    }
}
