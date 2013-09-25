using System;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Stellt einen Vergleich nach der Objektidentität bereit.
    /// </summary>
    /// <typeparam name="T">Die Art der zu vergleichenden Objekte.</typeparam>
    internal static class ReferenceComparer<T> where T : class
    {
        /// <summary>
        /// Die einzige Instanz zu jeder Art.
        /// </summary>
        public static readonly IEqualityComparer<T> Default = new _Instance();

        /// <summary>
        /// Stellt die eigentliche Funktionalität zur Verfügung.
        /// </summary>
        private class _Instance : IEqualityComparer<T>
        {
            #region IEqualityComparer<T> Members

            /// <summary>
            /// Prüft, ob zwei Objekte identisch sind.
            /// </summary>
            /// <param name="x">Das erste Objekt.</param>
            /// <param name="y">Das zweite Objekt.</param>
            /// <returns>Gesetzt, wenn die beiden Objekte identisch sind.</returns>
            public bool Equals( T x, T y )
            {
                // Same objects?
                return ReferenceEquals( x, y );
            }

            /// <summary>
            /// Meldet ein Kürzel.
            /// </summary>
            /// <param name="obj">Ein Objekt.</param>
            /// <returns>Das gewünschte Kürzel.</returns>
            public int GetHashCode( T obj )
            {
                // Report
                if (obj == null)
                    return 0;
                else
                    return obj.GetHashCode();
            }

            #endregion
        }
    }
}
