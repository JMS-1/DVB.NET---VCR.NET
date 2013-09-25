using System;
using System.Xml.Serialization;

namespace JMS.DVB
{
    /// <summary>
    /// Unique identification of any DVB station.
    /// </summary>
    [Serializable]
    public class Identifier
    {
        /// <summary>
        /// The original network identifier.
        /// </summary>
        public ushort NetworkIdentifier;

        /// <summary>
        /// The transport stream identifier.
        /// </summary>
        public ushort TransportStreamIdentifier;

        /// <summary>
        /// The service identifier
        /// </summary>
        public ushort ServiceIdentifier;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="networkID">The original network identifier.</param>
        /// <param name="transportID">The transport strea, identifier.</param>
        /// <param name="serviceID">The service identifier.</param>
        public Identifier( ushort networkID, ushort transportID, ushort serviceID )
        {
            // Remember
            TransportStreamIdentifier = transportID;
            NetworkIdentifier = networkID;
            ServiceIdentifier = serviceID;
        }

        /// <summary>
        /// Create an empty instance used for serialization.
        /// </summary>
        public Identifier()
        {
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="other">Identifier to clone.</param>
        public Identifier( Identifier other )
            : this( other.NetworkIdentifier, other.TransportStreamIdentifier, other.ServiceIdentifier )
        {
        }

        /// <summary>
        /// Use the three public fields to create a hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // Construct
            return NetworkIdentifier.GetHashCode() ^ TransportStreamIdentifier.GetHashCode() ^ ServiceIdentifier.GetHashCode();
        }

        /// <summary>
        /// Compare this key with another one.
        /// </summary>
        /// <param name="obj">Some other instance.</param>
        /// <returns>Set if the parameter is a <see cref="Identifier"/> with identical
        /// fields.</returns>
        public override bool Equals( object obj )
        {
            // Check type
            Identifier pOther = obj as Identifier;

            // Never
            if (null == pOther) return false;

            // Full test
            return (NetworkIdentifier == pOther.NetworkIdentifier) && (TransportStreamIdentifier == pOther.TransportStreamIdentifier) && (ServiceIdentifier == pOther.ServiceIdentifier);
        }

        /// <summary>
        /// Visualize as string for debug purposes.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Print formatted
            return string.Format( "({0}, {1}, {2})", NetworkIdentifier, TransportStreamIdentifier, ServiceIdentifier );
        }

        public int CompareTo( Identifier other )
        {
            // Network first
            int result = NetworkIdentifier.CompareTo( other.NetworkIdentifier );
            if (0 != result) return result;

            // Then transport
            result = TransportStreamIdentifier.CompareTo( other.TransportStreamIdentifier );
            if (0 != result) return result;

            // Finally service
            return ServiceIdentifier.CompareTo( other.ServiceIdentifier );
        }
    }
}
