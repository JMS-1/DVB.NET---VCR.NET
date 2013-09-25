using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Verwaltet einen Speicherbereich für eine Enumeration von COM Schnittstellen.
    /// </summary>
    public class COMArray : IDisposable
    {
        /// <summary>
        /// Referenz auf das Feld der Schnittstellen.
        /// </summary>
        private IntPtr[] m_Array;

        /// <summary>
        /// Fixierung des Feldes im Speicher.
        /// </summary>
        private GCHandle m_Handle;

        /// <summary>
        /// Gesetzt, wenn eine Freigabe bei <see cref="Dispose"/> erforderlich ist.
        /// </summary>
        private bool m_MustRelease;

        /// <summary>
        /// Erzeugt ein neues Feld.
        /// </summary>
        /// <param name="size">Die Anzahl der Elemente in dem Feld.</param>
        public COMArray( int size )
            : this( size, true )
        {
        }

        /// <summary>
        /// Erzeugt ein neues Feld.
        /// </summary>
        /// <param name="size">Die Anzahl der Elemente in dem Feld.</param>
        /// <param name="mustRelease">Gesetzt, wenn der Speicher des Feldes von dieser Instanz verwaltet wird.</param>
        public COMArray( int size, bool mustRelease )
        {
            // Remember
            m_MustRelease = mustRelease;

            // Allocate
            m_Array = new IntPtr[size];

            // Lock
            m_Handle = GCHandle.Alloc( m_Array, GCHandleType.Pinned );
        }

        /// <summary>
        /// Meldet die Speicheradresse des Feldes.
        /// </summary>
        public IntPtr Address
        {
            get
            {
                // Report
                return m_Handle.AddrOfPinnedObject();
            }
        }

        /// <summary>
        /// Ermittelt über die COM Schnittstelle eines Feldelementes eine .NET Schnittstelle.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des Elementes.</param>
        /// <returns>Das zugehörige .NET Objekt.</returns>
        public object GetObject( int index )
        {
            // Load - will add reference count
            return Marshal.GetObjectForIUnknown( m_Array[index] );
        }

        /// <summary>
        /// Ermittelt über die COM Schnittstelle eines Feldelementes eine .NET Schnittstelle.
        /// </summary>
        /// <typeparam name="T">Die gewünschte Art der .NET Schnittstelle.</typeparam>
        /// <param name="index">Die 0-basierte laufende Nummer des Elementes.</param>
        /// <returns>Das zugehörige .NET Objekt.</returns>
        public T GetObject<T>( int index )
        {
            // Load 
            var instance = GetObject( index );
            try
            {
                // Change type
                return (T) instance;
            }
            catch
            {
                // Cleanup
                BDAEnvironment.Release( ref instance );

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Meldet eine COM Schnittstelle im Feld.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer eines Feldelementes.</param>
        public IntPtr this[int index]
        {
            get
            {
                // Report
                return m_Array[index];
            }
        }

        /// <summary>
        /// Meldet die Anzahl der Feldelemente.
        /// </summary>
        public int Length
        {
            get
            {
                // Report
                return m_Array.Length;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieses Feldes endgültig.
        /// </summary>
        public void Dispose()
        {
            // Cleanup array
            if (m_MustRelease)
                if (m_Array != null)
                    foreach (var comRef in m_Array)
                        if (comRef != IntPtr.Zero)
                            Marshal.Release( comRef );

            // Forget
            m_Array = null;

            // Release memory
            if (m_Handle.IsAllocated)
                m_Handle.Free();
        }

        #endregion
    }
}
