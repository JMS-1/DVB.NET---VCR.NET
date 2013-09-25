using System;
using JMS.DVB;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Represents a single service in a service group.
    /// </summary>
    public class ServiceItem : IComparable
    {
        /// <summary>
        /// The identifier of the service.
        /// </summary>
        public SourceIdentifier Identifier { get; private set; }

        /// <summary>
        /// The name of the service.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The index of the service.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="id">The identifier of the service.</param>
        /// <param name="name">The name of the service.</param>
        public ServiceItem( SourceIdentifier id, string name )
        {
            // Split
            int split = name.IndexOf( ',' );

            // Remember
            Index = int.Parse( name.Substring( 0, split++ ) );
            Name = name.Substring( split );
            Identifier = id;
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="id">The identifier of the service.</param>
        /// <param name="name">The name of the service.</param>
        public ServiceItem( Station id, string name )
        {
            // Split
            int split = name.IndexOf( ',' );

            // Remember
            Index = int.Parse( name.Substring( 0, split++ ) );
            Name = name.Substring( split );
            Identifier = id;
        }

        /// <summary>
        /// Will report the <see cref="Name"/> for the portal and additional
        /// append the service identification from the <see cref="Identifier"/>
        /// for each service channel.
        /// </summary>
        /// <returns>Some display name for the service.</returns>
        public override string ToString()
        {
            // Special
            return string.Format( "{0} [{1}] ({2})", Name, Identifier.Service, Index );
        }

        #region IComparable Members

        /// <summary>
        /// Compare two instances by <see cref="Name"/> but make sure that the portal
        /// itself is always the first to show.
        /// </summary>
        /// <param name="obj">Some other service representative.</param>
        /// <returns><i>0</i> if the services names are identical, <i>-1</i> or <i>+1</i>
        /// if they are differen.</returns>
        public int CompareTo( object obj )
        {
            // Change type
            ServiceItem other = obj as ServiceItem;

            // Right ones first
            if (null == other) return -1;

            // Process
            int cmp = Index.CompareTo( other.Index );

            // Same
            if (0 == cmp)
                return 0;

            // Make sure that portal is last
            if (Index == 0)
                return +1;
            else if (other.Index == 0)
                return -1;
            else
                return cmp;
        }

        #endregion
    }
}
