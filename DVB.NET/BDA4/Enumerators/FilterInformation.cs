using System;
using System.Collections.Generic;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess.Enumerators
{
    /// <summary>
    /// Beschreibt einen einzelnen <i>DirectShow</i> Filter.
    /// </summary>
    public class FilterInformation : IDeviceOrFilterInformation
    {
        /// <summary>
        /// Vergleichskomponente für Filterinformationen.
        /// </summary>
        public static readonly IComparer<FilterInformation> Comparer = new _Comparer();

        /// <summary>
        /// Vergleicht zwei Filterbeschreibungen.
        /// </summary>
        private class _Comparer : IComparer<FilterInformation>
        {
            #region IComparer<FilterInformation> Members

            /// <summary>
            /// Vergleicht zwei Filterinformationen.
            /// </summary>
            /// <param name="x">Die erste Information.</param>
            /// <param name="y">Die zweite Information.</param>
            /// <returns>Der Unterschied zwischen den Informationen.</returns>
            int IComparer<FilterInformation>.Compare( FilterInformation x, FilterInformation y )
            {
                // Short cut
                if (ReferenceEquals( x, y ))
                    return 0;

                // Special cases
                if (x == null)
                    if (y == null)
                        return 0;
                    else
                        return -1;
                else if (y == null)
                    return +1;

                // By type
                var delta = x.Type.CompareTo( y.Type );
                if (delta != 0)
                    return delta;

                // By name
                delta = string.Compare( x.DisplayName, y.DisplayName );
                if (delta != 0)
                    return delta;

                // By path
                if (x.Hardware == null)
                    if (y.Hardware == null)
                        return 0;
                    else
                        return -1;
                else if (y.Hardware == null)
                    return +1;
                else
                    return string.Compare( x.Hardware.DevicePath, y.Hardware.DevicePath );
            }

            #endregion
        }

        /// <summary>
        /// Bechreibt die Art des Filters.
        /// </summary>
        public enum FilterType
        {
            /// <summary>
            /// Die Art des Filters ist nicht bekannt.
            /// </summary>
            Other,

            /// <summary>
            /// Es handelt sich um eine Nutzdateneinheit.
            /// </summary>
            Capture,

            /// <summary>
            /// Es handelt sich um eine Empfangseinheit.
            /// </summary>
            Tuner,
        }

        /// <summary>
        /// Meldet die Art des Filters.
        /// </summary>
        public FilterType Type
        {
            get
            {
                // Check it
                if (Category == BDAEnvironment.CategoryTunerFilter)
                    return FilterType.Tuner;
                else if (Category == BDAEnvironment.CategoryReceiverFilter)
                    return FilterType.Capture;
                else
                    return FilterType.Other;
            }
        }

        /// <summary>
        /// Die Art dieses Filters.
        /// </summary>
        public Guid Category { get; private set; }

        /// <summary>
        /// Der eindetuige (COM) Names des Filters.
        /// </summary>
        public string Moniker { get; private set; }

        /// <summary>
        /// Der Anzeigename des Filters.
        /// </summary>
        public string FilterDisplayName { get; internal set; }

        /// <summary>
        /// Meldet das zugehörige Gerät.
        /// </summary>
        public DeviceInformation Hardware { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="type">Die Art diese Filters.</param>
        /// <param name="displayName">Der Anzeigename des Filters.</param>
        /// <param name="moniker">Der eindeutige Name des Filters.</param>
        /// <param name="deviceMap">Die bekannten Geräte.</param>
        /// <exception cref="ArgumentNullException">Es wurden nicht alle Informationen angegeben.</exception>
        public FilterInformation( Guid type, string displayName, string moniker, Dictionary<string, DeviceInformation> deviceMap )
        {
            // Validate
            if (string.IsNullOrEmpty( displayName ))
                throw new ArgumentNullException( "displayName" );
            if (string.IsNullOrEmpty( moniker ))
                throw new ArgumentNullException( "moniker" );
            if (deviceMap == null)
                throw new ArgumentNullException( "deviceMap" );

            // Remember
            FilterDisplayName = displayName;
            Moniker = moniker;
            Category = type;

            // See if we could translate
            if (Moniker == null)
                return;
            if (!Moniker.StartsWith( @"\\?\" ))
                return;

            // Split off device path
            var outer = Moniker.Substring( 4 ).Split( '\\' );
            if (outer.Length != 2)
                return;

            // Get the inner name
            var inner = outer[0].Replace( '#', '\\' );
            var last = inner.LastIndexOf( '\\' );
            if (last < 0)
                return;

            // See if we know it
            DeviceInformation device;
            if (deviceMap.TryGetValue( inner.Substring( 0, last ), out device ))
                Hardware = device;
        }

        /// <summary>
        /// Meldet den Anzeigenamen, wie ein Anwender ihn zur Auswahl verwenden sollte.
        /// </summary>
        public string DisplayName
        {
            get
            {
                // Check mode
                if (string.IsNullOrEmpty( FilterDisplayName ))
                    if (Hardware == null)
                        return Moniker;
                    else
                        return Hardware.DisplayName;
                else
                    return FilterDisplayName;
            }
        }

        /// <summary>
        /// Erzeugt einen Anzeigetext zu Testwecken.
        /// </summary>
        /// <returns>Der gewünschte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "{0} ({1} {2})", FilterDisplayName, Type, Moniker );
        }

        /// <summary>
        /// Erzeugt den zugehörigen Filter.
        /// </summary>
        /// <returns>Der gewünschte Filter.</returns>
        /// <exception cref="NotSupportedException">Der angeforderte Filter existiert.</exception>
        public TypedComIdentity<IBaseFilter> CreateFilter()
        {
            // Create system device enumerator
            var devEnum = (ICreateDevEnum) Activator.CreateInstance( System.Type.GetTypeFromCLSID( BDAEnvironment.SystemDeviceEnumeratorClassIdentifier ) );
            try
            {
                // Helper
                var category = Category;

                // Get the enumerator
                IEnumMoniker monikers;
                if (devEnum.CreateClassEnumerator( ref category, out monikers, 0 ) >= 0)
                    if (monikers != null)
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
                                    if (count != 1)
                                        break;

                                    // Load object
                                    var moniker = array.GetObject<IMoniker>( 0 );
                                    try
                                    {
                                        // Check name
                                        if (string.Equals( Moniker, moniker.ReadProperty( "DevicePath" ) ))
                                        {
                                            // Create the instance
                                            var iid = new Guid( "56a86895-0ad4-11ce-b03a-0020af0ba770" );
                                            var filter = moniker.BindToObject( null, null, ref iid );

                                            // Report safely
                                            try
                                            {
                                                // Construct
                                                return ComIdentity.Create( (IBaseFilter) filter );
                                            }
                                            catch
                                            {
                                                // Cleanup
                                                BDAEnvironment.Release( ref filter );

                                                // Forward
                                                throw;
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        // Release
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
            finally
            {
                // Back to COM
                BDAEnvironment.Release( ref devEnum );
            }

            // Not found
            throw new NotSupportedException( string.Format( Properties.Resources.Exception_BadFilter, this ) );
        }

        #region IDeviceOrFilterInformation Members

        /// <summary>
        /// Der Name, der dem Anwender zur Auswahl angeboten wird.
        /// </summary>
        string IDeviceOrFilterInformation.DisplayName
        {
            get
            {
                // Report
                return DisplayName;
            }
        }

        /// <summary>
        /// Der eindeutige Name zur Auswahl.
        /// </summary>
        string IDeviceOrFilterInformation.UniqueName
        {
            get
            {
                // Report
                return Moniker;
            }
        }

        #endregion
    }
}
