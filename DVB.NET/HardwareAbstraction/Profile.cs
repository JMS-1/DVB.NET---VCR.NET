using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVB
{
    /// <summary>
    /// Basisklasse zur Beschreibung einer DVB.NET Hardwareabstraktion.
    /// </summary>
    [Serializable]
    [XmlInclude( typeof( TerrestrialProfile ) )]
    [XmlInclude( typeof( SatelliteProfile ) )]
    [XmlInclude( typeof( CableProfile ) )]
    public abstract class Profile
    {
        /// <summary>
        /// Der XML Namensraum für Geräteprofile.
        /// </summary>
        public const string Namespace = "http://psimarron.net/DVBNET/Profiles";

        /// <summary>
        /// Der Name der .NET Klasse mit der Hardwareabstraktion.
        /// </summary>
        [XmlElement( "Provider" )]
        public string HardwareType { get; set; }

        /// <summary>
        /// Zusätzliche Erweiterung für Spezialaufgaben etwa während der Senderauswahl.
        /// </summary>
        public readonly List<PipelineItem> Pipeline = new List<PipelineItem>();

        /// <summary>
        /// Ein beliebiger eindeutiger Name, über den dass DVB Gerät ermittelt werden kann,
        /// dass mit diesem Profil verbunden ist.
        /// </summary>
        [XmlElement( "Device" )]
        public readonly List<DeviceAspect> DeviceAspects = new List<DeviceAspect>();

        /// <summary>
        /// Weitere Einstellungen für die Hardwareabstraktion.
        /// </summary>
        [XmlElement( "Parameter" )]
        public readonly List<ProfileParameter> Parameters = new List<ProfileParameter>();

        /// <summary>
        /// Der vollständige Pfad zur Datei, aus der diese Beschreibung entnommen wurde.
        /// </summary>
        [XmlIgnore]
        public FileInfo ProfilePath { get; set; }

        /// <summary>
        /// Wird gesetzt um anzuzeigen, dass dieses Profil nur im Speicher vorhanden ist
        /// und Änderungen nicht persistent gespeichert werden.
        /// </summary>
        [XmlIgnore]
        public string VolatileName { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob dieses Profil eigene Quellen verwendet oder die
        /// Quellen eines anderen Profils referenziert.
        /// </summary>
        public string UseSourcesFrom { get; set; }

        /// <summary>
        /// Meldet alle Ursprünge zu diesem Profil.
        /// </summary>
        [XmlIgnore]
        public abstract IList Locations { get; }

        /// <summary>
        /// Erlaubt den Zugriff auf die Liste der Ursprünge für den Sendersuchlauf.
        /// </summary>
        [XmlIgnore]
        public abstract IList ScanLocations { get; }

        /// <summary>
        /// Meldet die Sonderkonfiguration für den Sendersuchlauf.
        /// </summary>
        [XmlIgnore]
        public abstract ScanningFilter ScanConfiguration { get; }

        /// <summary>
        /// Meldet oder legt fest, ob die Programmzeitschrift für alle Quellen dieses
        /// Geräteprpofiles deaktiviert werden sollen.
        /// </summary>
        public bool DisableProgramGuide { get; set; }

        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        internal Profile()
        {
        }

        /// <summary>
        /// Speichert ein DVB.NET Geräteprofil.
        /// </summary>
        /// <exception cref="ArgumentNullException">Diesem Geräteprofil ist keine Datei zugeordnet.</exception>
        public void Save()
        {
            // Memory only profile
            if (!string.IsNullOrEmpty( VolatileName ))
                return;

            // Store using default path
            Save( ProfilePath );

            // Force reload
            ProfileManager.Refresh();
        }

        /// <summary>
        /// Speichert ein DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="file">Zielort für die Ablage.</param>
        /// <exception cref="ArgumentNullException">Diesem Geräteprofil ist keine Datei zugeordnet.</exception>
        public void Save( FileInfo file )
        {
            // Validate
            if (file == null)
                throw new ArgumentNullException( "file" );

            // Make sure that directory exists
            file.Directory.Create();

            // Create serializer and settings
            var serializer = new XmlSerializer( GetType(), Namespace );
            var settings = new XmlWriterSettings { Encoding = Encoding.Unicode, Indent = true };

            // Store
            using (var writer = XmlWriter.Create( file.FullName, settings ))
                serializer.Serialize( writer, this );
        }

        /// <summary>
        /// Ermittelt alle Erweiterungsinstanzen einer bestimmten Art.
        /// </summary>
        /// <param name="forType">Die gewünschte Art von Erweiterung.</param>
        /// <returns>Eine Auflistung aller Erweiterungen dieser Art.</returns>
        public IEnumerable<PipelineItem> GetPipelineItems( PipelineTypes forType )
        {
            // Report
            return Pipeline.Where( p => (p.SupportedOperations & forType) != 0 );
        }

        /// <summary>
        /// Entfernt die Datei zu einem Geräteprofil.
        /// </summary>
        public void Delete()
        {
            // Forward
            if (ProfilePath != null)
                if (ProfilePath.Exists)
                    ProfilePath.Delete();
        }

        /// <summary>
        /// Meldet den Namen dieser Beschreibung.
        /// </summary>
        public string Name
        {
            get
            {
                // Check mode
                if (string.IsNullOrEmpty( VolatileName ))
                    return (ProfilePath == null) ? null : Path.GetFileNameWithoutExtension( ProfilePath.FullName );
                else
                    return VolatileName;
            }
        }

        /// <summary>
        /// Erzeugt eine neue Instanz der Abstraktion zu diesem Geräteprofil.
        /// </summary>
        /// <returns>Die neu erzeugte Instanz, die mit <see cref="IDisposable.Dispose"/>
        /// freigegeben werden muss.</returns>
        public Hardware CreateHardware()
        {
            // Forward
            return Hardware.Create( this, false );
        }

        /// <summary>
        /// Stellt ein temporäres Profil auf ein permanentes um.
        /// </summary>
        public void MakePermanent()
        {
            // Already is
            if (string.IsNullOrEmpty( VolatileName ))
                return;

            // Get the storage path
            ProfilePath = new FileInfo( Path.Combine( ProfileManager.ProfilePath.FullName, VolatileName + "." + ProfileManager.ProfileExtension ) );

            // Store
            Save( ProfilePath );

            // Forget
            VolatileName = null;
        }

        /// <summary>
        /// Zeigt den Namen dieses Geräteprofils an.
        /// </summary>
        /// <returns>Der Name des Geräteprofils.</returns>
        public override string ToString()
        {
            // Report
            return Name;
        }

        /// <summary>
        /// Prüft, ob ein Geräteprofil zu einem anderen kompatibel ist. Kompatibel sind jeweils
        /// nur DVB-S und DVB-S etc.
        /// </summary>
        /// <param name="other">Das andere Geräteprofil.</param>
        /// <returns>Gesetzt, wenn beide Profile kompatibel sind.</returns>
        public abstract bool IsCompatibleTo( Profile other );

        /// <summary>
        /// Meldet den Namen des Geräteprofils, in dem die Senderliste abgelegt ist.
        /// </summary>
        [XmlIgnore]
        public Profile LeafProfile
        {
            get
            {
                // It's us
                if (string.IsNullOrEmpty( UseSourcesFrom ))
                    return this;

                // Create cycle lock
                var done = new HashSet<string>();

                // Process all
                for (Profile test = this; (test = ProfileManager.FindProfile( test.UseSourcesFrom )) != null; )
                {
                    // Wrong type
                    if (!IsCompatibleTo( test ))
                        break;

                    // This is a cycle
                    if (done.Contains( test.Name ))
                        break;

                    // This is the leaf
                    if (string.IsNullOrEmpty( test.UseSourcesFrom ))
                        return test;

                    // Lock out
                    done.Add( test.Name );
                }

                // Not found or cycle
                return null;
            }
        }

        /// <summary>
        /// Ermittelt die Zugriffsdaten für eine bestimmte Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die Informationen zur Quelle.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public SourceSelection[] FindSource( SourceIdentifier source )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );
            else
                return InternalFindSource( source ).ToArray();
        }

        /// <summary>
        /// Meldet die Zugriffsdaten zu allen Quellen dieses Geräteprofils.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<SourceSelection> AllSources { get { return InternalFindSource( null ); } }

        /// <summary>
        /// Ermittelt alle Quellen sortiert nach dem Anzeigenamen <see cref="SourceSelection.DisplayName"/>
        /// aus Quellname und Dienstanbieter, wobei
        /// Datendienste explizit ausgeblendet werden. Existieren zu einem Namen mehrere Quellen, so
        /// wird der Anzeigename durch den eindeutigen Namen <see cref="SourceSelection.QualifiedName"/>
        /// ersetzt. Unterschiede zwischen Groß- und Kleinschreibungen werden durch Nutzung des
        /// <see cref="StringComparer.InvariantCultureIgnoreCase"/> Vergleichsalgorithmus ignoriert.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<SourceSelection> AllSourcesByDisplayName
        {
            get
            {
                // Last processed
                SourceSelection last = null;
                string lastName = null;

                // Process all sorted
                foreach (var source in AllSources.OrderBy( s => s.DisplayName, StringComparer.InvariantCultureIgnoreCase ))
                {
                    // Attach to the station
                    var station = (Station) source.Source;
                    if (station.SourceType == SourceTypes.Unknown)
                        if (!station.IsService)
                            continue;

                    // Check names
                    if (lastName != null)
                        if (StringComparer.InvariantCultureIgnoreCase.Equals( source.DisplayName, lastName ))
                        {
                            // Send last entry
                            if (last != null)
                                yield return new SourceSelection { DisplayName = last.QualifiedName, ProfileName = last.ProfileName, Location = last.Location, Group = last.Group, Source = last.Source };

                            // Send this entry
                            yield return new SourceSelection { DisplayName = source.QualifiedName, ProfileName = source.ProfileName, Location = source.Location, Group = source.Group, Source = source.Source };

                            // Reset
                            last = null;

                            // Next
                            continue;
                        }

                    // Send
                    if (last != null)
                        yield return last;

                    // Next
                    lastName = source.DisplayName;
                    last = source;
                }

                // Finish
                if (last != null)
                    yield return last;
            }
        }

        /// <summary>
        /// Ermittelt eine oder mehrere Quellen nach dem Namen. Diese Methode ist relativ langsam und
        /// sollte nicht verwendet werden, wenn häufig nach Namen nachgeschlagen werden muss.
        /// </summary>
        /// <param name="name">Der gewünschte Name.</param>
        /// <returns>Alle Quellen, deren Namen dem Muster entsprechen.</returns>
        /// <exception cref="ArgumentNullException">Der Name darf nicht leer sein.</exception>
        public SourceSelection[] FindSource( string name )
        {
            // Forward
            return FindSource( name, SourceNameMatchingModes.All );
        }

        /// <summary>
        /// Ermittelt eine oder mehrere Quellen nach dem Namen. Diese Methode ist relativ langsam und
        /// sollte nicht verwendet werden, wenn häufig nach Namen nachgeschlagen werden muss.
        /// </summary>
        /// <param name="name">Der gewünschte Name.</param>
        /// <param name="matching">Der Vergleichsmodus.</param>
        /// <returns>Alle Quellen, deren Namen dem Muster entsprechen.</returns>
        /// <exception cref="ArgumentNullException">Der Name darf nicht leer sein.</exception>
        public SourceSelection[] FindSource( string name, SourceNameMatchingModes matching )
        {
            // Validate
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );
            else
                return InternalFindSource( name, matching ).ToArray();
        }

        /// <summary>
        /// Ermittelt eine oder mehrere Quellen nach dem Namen.
        /// </summary>
        /// <param name="name">Der gewünschte Name.</param>
        /// <param name="matching">Der Vergleichsmodus.</param>
        /// <returns>Alle Quellen, deren Namen dem Muster entsprechen.</returns>
        private IEnumerable<SourceSelection> InternalFindSource( string name, SourceNameMatchingModes matching )
        {
            // Process all
            foreach (var source in AllSources)
            {
                // Attach to station
                var station = (Station) source.Source;

                // Test all
                if (((matching & SourceNameMatchingModes.Name) != 0) && StringComparer.InvariantCultureIgnoreCase.Equals( name, station.Name ))
                    yield return source;
                else if (((matching & SourceNameMatchingModes.FullName) != 0) && StringComparer.InvariantCultureIgnoreCase.Equals( name, station.FullName ))
                    yield return source;
                else if (((matching & SourceNameMatchingModes.QualifiedName) != 0) && StringComparer.InvariantCultureIgnoreCase.Equals( name, station.QualifiedName ))
                    yield return source;
            }
        }

        /// <summary>
        /// Ermittelt die Zugriffsdaten für eine bestimmte Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die Informationen zur Quelle.</returns>
        private IEnumerable<SourceSelection> InternalFindSource( SourceIdentifier source )
        {
            // Get the name of the real profile
            var leaf = LeafProfile;

            // Not found
            if (ReferenceEquals( leaf, null ))
                yield break;

            // Check mode
            if (ReferenceEquals( leaf, this ))
            {
                // Scan us completly
                foreach (GroupLocation location in Locations)
                    foreach (SourceGroup group in location.Groups)
                        if (SupportsGroup( group ))
                            foreach (var identifier in group.Sources)
                                if ((source == null) || Equals( source, identifier ))
                                    yield return
                                        new SourceSelection
                                        {
                                            DisplayName = ((Station) identifier).FullName,
                                            Location = location,
                                            Source = identifier,
                                            ProfileName = Name,
                                            Group = group
                                        };
            }
            else
            {
                // Update profile name
                foreach (var selection in leaf.InternalFindSource( source ))
                    if (SupportsGroup( selection.Group ))
                    {
                        // Update profile association
                        selection.ProfileName = Name;

                        // Report
                        yield return selection;
                    }
            }
        }

        /// <summary>
        /// Erzeugt eine Liste von Quellgruppen (Transponder) mit zugeordneten Ursprüngen, die
        /// für den Sendersuchlauf verwendet werden können.
        /// </summary>
        /// <returns>Die zugehörige Liste von Ursprüngen.</returns>
        public abstract GroupLocation[] CreateScanLocations();

        /// <summary>
        /// Prüft, ob das zugehörige Gerät eine bestimmte Quellgruppe überhaupt unterstützt.
        /// </summary>
        /// <param name="group">Die zu prüfende Quellgruppe.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe unterstützt wird.</returns>
        public abstract bool SupportsGroup( SourceGroup group );

        /// <summary>
        /// Ermittelt besondere Einschränkungen des zugehörigen Gerätes.
        /// </summary>
        [XmlIgnore]
        public HardwareRestriction Restrictions
        {
            get
            {
                // Be safe
                try
                {
                    // Check type
                    if (string.IsNullOrEmpty( HardwareType ))
                        return null;

                    // Create type
                    var device = Type.GetType( HardwareType, false );
                    if (device == null)
                        return null;

                    // Check methode
                    var getRestriction = device.GetMethod( "GetRestriction", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy );
                    if (getRestriction == null)
                        return null;

                    // Forward                    
                    var restrictions = (HardwareRestriction) getRestriction.Invoke( null, new object[] { this } );

                    // Check extensions
                    if (restrictions != null)
                        if (!restrictions.ProvidesSignalInformation)
                            restrictions.ProvidesSignalInformation = GetPipelineItems( PipelineTypes.SignalInformation ).Any();

                    // Done
                    return restrictions;
                }
                catch
                {
                    // None
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// Beschreibt eine DVB Hardware.
    /// </summary>
    /// <typeparam name="TLocationType">Die Art des Ursprungs.</typeparam>
    /// <typeparam name="TGroupType">Die Art der Quellgruppe.</typeparam>
    /// <typeparam name="TScanType">Die Art der Informationen für die Aktualisierung der Quellen.</typeparam>
    [Serializable]
    public abstract class Profile<TLocationType, TGroupType, TScanType> : Profile
        where TLocationType : GroupLocation<TGroupType>, new()
        where TGroupType : SourceGroup, new()
        where TScanType : ScanLocation<TGroupType>
    {
        /// <summary>
        /// Initialisiert eine neue Beschreibung.
        /// </summary>
        protected Profile()
        {
            // Create helper
            TypedScanLocations = new List<ScanTemplate<TLocationType>>();
            ScanningFilter = new ScanningFilter<TGroupType>();
        }

        /// <summary>
        /// Alle Informationen zur Durchführung eines Sendersuchlaufs.
        /// </summary>
        [XmlElement( "ScanLocation" )]
        public readonly List<ScanTemplate<TLocationType>> TypedScanLocations;

        /// <summary>
        /// Setzt oder meldet die Regeln, die beim Sendersuchlauf als Ausnahmen berücksichtigt werden sollen.
        /// </summary>
        public ScanningFilter<TGroupType> ScanningFilter { get; set; }

        /// <summary>
        /// Enthält alle möglichen Ursprünge und die damit verbundenen Gruppen von Quellen.
        /// </summary>
        public readonly List<TLocationType> GroupLocations = new List<TLocationType>();

        /// <summary>
        /// Prüft, ob ein Geräteprofil zu einem anderen kompatibel ist. Kompatibel sind jeweils
        /// nur DVB-S und DVB-S etc.
        /// </summary>
        /// <param name="other">Das andere Geräteprofil.</param>
        /// <returns>Gesetzt, wenn beide Profile kompatibel sind.</returns>
        public override bool IsCompatibleTo( Profile other )
        {
            // Never
            if (ReferenceEquals( other, null ))
                return false;
            else
                return (other is Profile<TLocationType, TGroupType, TScanType>);
        }

        /// <summary>
        /// Erzeugt eine Liste von Quellgruppen (Transponder) mit zugeordneten Ursprüngen, die
        /// für den Sendersuchlauf verwendet werden können.
        /// </summary>
        /// <returns>Die zugehörige Liste von Ursprüngen.</returns>
        public override GroupLocation[] CreateScanLocations()
        {
            // Create result
            var locations = new List<TLocationType>();

            // Process all known templates
            foreach (var location in TypedScanLocations)
                if (location.GroupLocation != null)
                {
                    // Clone it
                    var clone = (TLocationType) location.GroupLocation.Clone();

                    // Process all entries
                    if (location.ScanLocations != null)
                        foreach (var scan in location.ScanLocations)
                        {
                            // Try to find
                            var config = JMS.DVB.ScanLocations.Default.Find<TScanType>( scan );
                            if (config == null)
                                continue;

                            // Process all
                            foreach (SourceGroup group in config.Groups)
                            {
                                // Clone it
                                var clonedGroup = SourceGroup.FromString<SourceGroup>( group.ToString() );

                                // Add it
                                if (clonedGroup != null)
                                    clone.Groups.Add( clonedGroup );
                            }
                        }

                    // Remember
                    if (clone.Groups.Count > 0)
                        locations.Add( clone );
                }

            // Report
            return locations.ToArray();
        }

        /// <summary>
        /// Meldet die Sonderkonfiguration für den Sendersuchlauf.
        /// </summary>
        [XmlIgnore]
        public override ScanningFilter ScanConfiguration { get { return ScanningFilter; } }

        /// <summary>
        /// Erlaubt den Zugriff auf die Liste der Ursprünge für den Sendersuchlauf.
        /// </summary>
        [XmlIgnore]
        public override IList ScanLocations { get { return TypedScanLocations; } }

        /// <summary>
        /// Meldet alle Ursprünge zu diesem Profil.
        /// </summary>
        [XmlIgnore]
        public override IList Locations { get { return GroupLocations; } }

        /// <summary>
        /// Prüft, ob das zugehörige Gerät eine bestimmte Quellgruppe überhaupt unterstützt.
        /// </summary>
        /// <param name="group">Die zu prüfende Quellgruppe.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe unterstützt wird.</returns>
        public virtual bool SupportsGroup( TGroupType group )
        {
            // Check parameter
            if (ReferenceEquals( group, null ))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Prüft, ob das zugehörige Gerät eine bestimmte Quellgruppe überhaupt unterstützt.
        /// </summary>
        /// <param name="group">Die zu prüfende Quellgruppe.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe unterstützt wird.</returns>
        public override sealed bool SupportsGroup( SourceGroup group )
        {
            // Forward
            return SupportsGroup( group as TGroupType );
        }
    }
}
