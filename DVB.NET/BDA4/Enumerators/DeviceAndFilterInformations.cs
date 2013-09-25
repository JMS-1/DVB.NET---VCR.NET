using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess.Enumerators
{
    /// <summary>
    /// Verwaltet alle Geräte und <i>Direct Show</i> Filter.
    /// </summary>
    public class DeviceAndFilterInformations
    {
        /// <summary>
        /// Die einzige zur Verfügung stehende Instanz.
        /// </summary>
        public static readonly DeviceAndFilterInformations Cache = new DeviceAndFilterInformations();

        /// <summary>
        /// Die Informationen zu allen Geräten.
        /// </summary>
        private Dictionary<string, DeviceInformation> m_MediaDevices;

        /// <summary>
        /// Die Informationen zu allen Filtern.
        /// </summary>
        private List<FilterInformation> m_Filters = new List<FilterInformation>();

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        private DeviceAndFilterInformations()
        {
            // Fill in media devices
            m_MediaDevices = MediaDevice.DeviceInformations.ToDictionary( d => d.DevicePath, StringComparer.InvariantCultureIgnoreCase );

            // All tuners and captures are statically cached
            m_Filters.AddRange( FindByCategoryInternal( BDAEnvironment.CategoryTunerFilter ) );
            m_Filters.AddRange( FindByCategoryInternal( BDAEnvironment.CategoryReceiverFilter ) );
        }

        /// <summary>
        /// Ermittelt alle Filter einer bestimmten Art.
        /// </summary>
        /// <param name="category">Die gewünschte Art.</param>
        /// <returns>Liste der Beschreibungen.</returns>
        private IEnumerable<FilterInformation> FindByCategoryInternal( Guid category )
        {
            // Just process
            return
                FindFiltersByCategory( category )
                    .GroupBy( f => f.DisplayName )
                    .SelectMany( g =>
                        {
                            // Get all and see if we have to update names
                            var same = g.ToArray();
                            if (same.Length < 2)
                                return same;

                            // Reorder
                            Array.Sort( same, FilterInformation.Comparer );

                            // Create format
                            var fmt = new string( '0', same.Length.ToString().Length );

                            // Convert names                                
                            for (var i = same.Length; i-- > 0; )
                                same[i].FilterDisplayName = string.Format( "{0} ({1})", g.Key, (i + 1).ToString( fmt ) );

                            // Report
                            return same;
                        } );
        }

        /// <summary>
        /// Ermittelt alle Filter einer bestimmten Art.
        /// </summary>
        /// <param name="category">Die gewünschte Art.</param>
        /// <returns>Liste der Beschreibungen.</returns>
        public IEnumerable<FilterInformation> FindByCategory( Guid category )
        {
            // Check mode
            if (category == BDAEnvironment.CategoryTunerFilter)
                return TunerFilters;
            else if (category == BDAEnvironment.CategoryReceiverFilter)
                return CaptureFilters;
            else
                return FindByCategoryInternal( category );
        }

        /// <summary>
        /// Ermittelt alle Filter einer bestimmten Art.
        /// </summary>
        /// <param name="category">Die gewünschte Art.</param>
        /// <returns>Liste der Namen.</returns>
        private List<FilterInformation> FindFiltersByCategory( Guid category )
        {
            // Result
            var names = new List<FilterInformation>();

            // Create system device enumerator
            var devEnum = (ICreateDevEnum) Activator.CreateInstance( Type.GetTypeFromCLSID( BDAEnvironment.SystemDeviceEnumeratorClassIdentifier ) );
            try
            {
                // Get the enumerator
                IEnumMoniker monikers;
                if (devEnum.CreateClassEnumerator( ref category, out monikers, 0 ) == 0)
                    LoadFromEnumeration( category, "DevicePath", names, monikers );
            }
            finally
            {
                // Back to COM
                BDAEnvironment.Release( ref devEnum );
            }

            // Report
            return names;
        }

        /// <summary>
        /// Füllt eine Liste mit Filterinformationen aus einer Auflistung.
        /// </summary>
        /// <param name="category">Die Art des Filters.</param>
        /// <param name="uniqueProp">Die Eigenschaft, die als eindeutige Kennung verwendet werden soll.</param>
        /// <param name="list">Die Informationen.</param>
        /// <param name="monikers">Die Auflistung.</param>
        public void LoadFromEnumeration( Guid category, string uniqueProp, List<FilterInformation> list, IEnumMoniker monikers )
        {
            // Nothing to do
            if (monikers == null)
                return;

            // With cleanup
            try
            {
                // Process all
                for (; ; )
                    using (var array = new COMArray( 1 ))
                    {
                        // Load
                        uint count;
                        if (monikers.Next( 1, array.Address, out count ) != 0)
                            break;

                        // Load object
                        if (count != 1)
                            continue;

                        // Load the one
                        var moniker = array.GetObject<IMoniker>( 0 );
                        try
                        {
                            // Remember
                            list.Add( new FilterInformation( category, moniker.ReadProperty( "FriendlyName" ), moniker.ReadProperty( uniqueProp ), m_MediaDevices ) );
                        }
                        finally
                        {
                            // Cleanup
                            BDAEnvironment.Release( ref moniker );
                        }
                    }
            }
            finally
            {
                // Cleanup
                BDAEnvironment.Release( ref monikers );
            }
        }

        /// <summary>
        /// Meldet alle bekannten Filter.
        /// </summary>
        public IEnumerable<FilterInformation> AllFilters
        {
            get
            {
                // Report
                return GetFilters( null );
            }
        }

        /// <summary>
        /// Meldet alle bekannten Nutzdatenkomponenten.
        /// </summary>
        public IEnumerable<FilterInformation> CaptureFilters
        {
            get
            {
                // Report
                return GetFilters( FilterInformation.FilterType.Capture );
            }
        }

        /// <summary>
        /// Meldet alle bekannten Empfangskomponenten.
        /// </summary>
        public IEnumerable<FilterInformation> TunerFilters
        {
            get
            {
                // Report
                return GetFilters( FilterInformation.FilterType.Tuner );
            }
        }

        /// <summary>
        /// Meldet alle Filter einer bestimmten Art.
        /// </summary>
        /// <param name="ofType">Optional die gewünschte Art.</param>
        /// <returns>Die Liste der gewünschten Filter.</returns>
        public IEnumerable<FilterInformation> GetFilters( FilterInformation.FilterType? ofType )
        {
            // Just process
            return m_Filters.Where( f => !ofType.HasValue || (f.Type == ofType.Value) );
        }

        /// <summary>
        /// Meldet alle Geräte.
        /// </summary>
        public IEnumerable<DeviceInformation> MediaDevices
        {
            get
            {
                // Report
                return m_MediaDevices.Values;
            }
        }
    }

    /// <summary>
    /// Hilfsklasse zum Arbeiten mit Listen von Filtern.
    /// </summary>
    public static class DeviceAndFilterInformationExtensions
    {
        /// <summary>
        /// Ermittelt die Informationen zu einem bestimmten Filter.
        /// </summary>
        /// <param name="filters">Eine Liste von Filtern.</param>
        /// <param name="name">Der Anzeigename des Filters.</param>
        /// <param name="moniker">Der eindeutige Name des Filters.</param>
        /// <returns>Der gewünschte Filter.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Liste angegeben.</exception>
        public static FilterInformation FindFilter( this IEnumerable<FilterInformation> filters, string name, string moniker )
        {
            // Validate
            if (filters == null)
                throw new ArgumentNullException( "filters" );

            // Find all
            var devices = filters.ToList();

            // Try moniker first
            FilterInformation device = null;
            if (!string.IsNullOrEmpty( moniker ))
                device = devices.Find( i => moniker.Equals( i.Moniker ) );

            // Done
            if (device != null)
                return device;

            // Must try name
            if (string.IsNullOrEmpty( name ))
                return null;

            // Remove all non matches
            devices.RemoveAll( i => !name.Equals( i.DisplayName ) );

            // Not unique - fail if moniker has been provided
            if (devices.Count < 1)
                return null;
            else if (string.IsNullOrEmpty( moniker ))
                return devices[0];
            else
                return null;
        }
    }
}
