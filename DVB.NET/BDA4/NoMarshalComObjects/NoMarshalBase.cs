using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.NoMarshalComObjects
{
    /// <summary>
    /// Verwaltet den Zugriff auf ein COM Objekt.
    /// </summary>
    public abstract class NoMarshalBase<I> : IDisposable
    {
        /// <summary>
        /// Hält alle Signaturen einmalig vor.
        /// </summary>
        private volatile Dictionary<int, object> m_DelegateCache = new Dictionary<int, object>();

        /// <summary>
        /// Das verwaltete Objekt.
        /// </summary>
        private IntPtr m_Object;

        /// <summary>
        /// Initialisiert eine Verwaltungsinstanz.
        /// </summary>
        /// <param name="comObject">Das verwaltete Objekt. Diese Verwaltungsinstanz
        /// übernimmt mit der Erzeugung auch die Kontrolle über den aktuellen Stand
        /// des COM Referenzzählers.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Objekt angegeben.</exception>
        protected NoMarshalBase( IntPtr comObject )
        {
            // Validate
            if (IntPtr.Zero == comObject)
                throw new ArgumentNullException( "comObject" );

            // Remember
            m_Object = comObject;

            // Time to create delegates
            OnCreateDelegates();
        }

        /// <summary>
        /// Initialisiert eine Verwaltungsinstanz.
        /// </summary>
        /// <param name="comObject">Das zugehörige COM Objekt. Die Freigabe erfolgt durch
        /// den Aufrufer.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Objekt angegeben.</exception>
        protected NoMarshalBase( object comObject )
            : this( comObject, false )
        {
        }

        /// <summary>
        /// Initialisiert eine Verwaltungsinstanz.
        /// </summary>
        /// <param name="comObject">Das zugehörige COM Objekt.</param>
        /// <param name="autoRelease">Gesetzt, wenn dieses nach der Übernahme freigegeben werden soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Objekt angegeben.</exception>
        protected NoMarshalBase( object comObject, bool autoRelease )
        {
            // Validate
            if (null == comObject)
                throw new ArgumentNullException( "comObject" );

            // Be safe
            try
            {
                // Get the interface
                m_Object = Marshal.GetComInterfaceForObject( comObject, typeof( I ) );

                // Time to create delegates
                OnCreateDelegates();
            }
            finally
            {
                // Back to COM
                if (autoRelease)
                    if (Marshal.IsComObject( comObject ))
                        Marshal.ReleaseComObject( comObject );
            }
        }

        /// <summary>
        /// Wird zur Initialisierung der Instanz aufgerufen.
        /// </summary>
        protected abstract void OnCreateDelegates();

        /// <summary>
        /// Gibt das aktuelle Objekt frei.
        /// </summary>
        protected virtual void Release()
        {
            // Nothing more to do
            if (m_Object != IntPtr.Zero)
                try
                {
                    // Back to COM
                    Marshal.Release( m_Object );
                }
                finally
                {
                    // Forget it
                    m_Object = IntPtr.Zero;
                }
        }

        /// <summary>
        /// Meldet das aktuelle COM Objekt. Die Freigabe muss durch den Aufrufer erfolgen.
        /// </summary>
        public I ComObject
        {
            get
            {
                // Create
                return (I) Marshal.GetObjectForIUnknown( m_Object );
            }
        }

        /// <summary>
        /// Meldet das aktuelle COM Objekt.
        /// </summary>
        public IntPtr ComInterface
        {
            get
            {
                // Report
                return m_Object;
            }
        }

        /// <summary>
        /// Erzeugt eine Aufrufsignatur für eine COM Methode.
        /// </summary>
        /// <typeparam name="T">Die gewünschte Signatur.</typeparam>
        /// <param name="relativeMethodIndex">Die relative Nummer der COM Methode bezogen auf die
        /// Basisschnittstelle <i>IUnknown</i>.</param>
        /// <returns>Die gewünschte Aufrufsignatur.</returns>
        protected T CreateDelegate<T>( int relativeMethodIndex )
        {
            // See if delegate already exists
            object signature;
            if (m_DelegateCache.TryGetValue( relativeMethodIndex, out signature ))
                return (T) signature;

            // Attach to the vTable
            IntPtr vTable = Marshal.ReadIntPtr( m_Object );

            // Attach to the function pointer
            IntPtr method = Marshal.ReadIntPtr( vTable, 4 * (relativeMethodIndex + 3) );

            // Create the delegate
            signature = (object) Marshal.GetDelegateForFunctionPointer( method, typeof( T ) );

            // Process as long as necessary
            for (; ; )
            {
                // Load current cache
                Dictionary<int, object> current = m_DelegateCache;

                // See if delegate already exists
                if (m_DelegateCache.ContainsKey( relativeMethodIndex ))
                    return (T) signature;

                // Create a clone
                Dictionary<int, object> cache = new Dictionary<int, object>( current );

                // Add to cache
                cache[relativeMethodIndex] = signature;

                // Update map and report
#pragma warning disable 0420
                if (ReferenceEquals( current, Interlocked.CompareExchange( ref m_DelegateCache, cache, current ) ))
                    return (T) signature;
#pragma warning restore 0420
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung des COM Objektes kontrolliert.
        /// </summary>
        public void Dispose()
        {
            // Free
            Release();
        }

        #endregion
    }
}
