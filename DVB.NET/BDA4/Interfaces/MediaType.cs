using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Diese Klasse hilft bei der Verwaltung der DirectShow
    /// Struktur <see cref="RawMediaType"/>.
    /// </summary>
    public class MediaType : IDisposable
    {
        /// <summary>
        /// Die verwaltete Strukturinstanz.
        /// </summary>
        private RawMediaType m_MediaType = new RawMediaType();

        /// <summary>
        /// Speicherkopie der Strukturinstanz.
        /// </summary>
        private IntPtr m_RawMediaType = IntPtr.Zero;

        /// <summary>
        /// Ist gesetzt, wenn die verwaltete Stukturinstanz initialisiert ist.
        /// </summary>
        private bool m_HasMediaType = true;

        /// <summary>
        /// Erzeugt eine neue Strukturinstanz.
        /// </summary>
        /// <param name="majorType">Primäres Format.</param>
        /// <param name="subType">Sekundäres Format.</param>
        /// <param name="formatType">Optionale Formatbeschreibung.</param>
        /// <param name="fixedSize">Gesetzt, wenn alle Pakete die gleiche Größe haben.</param>
        /// <param name="sampleSize">Maximale Paketgröße.</param>
        public MediaType( Guid majorType, Guid subType, Guid formatType, bool fixedSize, uint sampleSize )
        {
            // Fill
            m_MediaType.FixedSizeSamples = fixedSize ? 1 : 0;
            m_MediaType.FormatType = formatType;
            m_MediaType.SampleSize = sampleSize;
            m_MediaType.MajorType = majorType;
            m_MediaType.SubType = subType;
        }

        /// <summary>
        /// Erzeugt eine neue, nicht initialisierte Strukturinstanz.
        /// </summary>
        public MediaType()
        {
        }

        /// <summary>
        /// Kopiert eine DirectShow Strukturinstanz.
        /// </summary>
        /// <param name="mediaType">Die DirectShow Instanz.</param>
        public MediaType( RawMediaType mediaType )
            : this( mediaType, true )
        {
        }

        /// <summary>
        /// Kopiert oder verwendet eine DirectShow Strukturinstanz.
        /// </summary>
        /// <param name="mediaType">Die DirectShow Instanz.</param>
        /// <param name="copy">Ist dieser Parameter gesetzt, so wird
        /// eine eigenständige Kopie erstellt. Ansonsten wird die
        /// DirectShow Instanz samt der damit verbundenen Speicherstrukturen
        /// übernommen. Insbesondere darf es keinen Aufruf von
        /// <see cref="Interfaces.MediaType.FreeMemory"/> auf dem ersten Parameter
        /// mehr geben.</param>
        public MediaType( RawMediaType mediaType, bool copy )
        {
            // Check mode
            if (copy)
            {
                // Make sure that secondary memory is duplicated
                mediaType.CopyTo( ref m_MediaType );
            }
            else
            {
                // Hold this
                m_MediaType = mediaType;
            }
        }

        /// <summary>
        /// Kopiert eine DirectShow Strukturinstanz, die über ihre
        /// Speicheradresse übergeben wird.
        /// </summary>
        /// <param name="rawMediaType">Die DirectShow Instanz.</param>
        public MediaType( IntPtr rawMediaType )
            : this( rawMediaType, true )
        {
        }

        /// <summary>
        /// Kopiert oder verwendet eine DirectShow Strukturinstanz, deren
        /// Speicheradresse übergeben wird.
        /// </summary>
        /// <param name="rawMediaType">Die Speicheradresse der DirectShow Instanz.</param>
        /// <param name="copy">Ist dieser Wert gesetzt, so wird die
        /// DirectShow Instanz samt den zugehörigen Speicherstrukturen
        /// dupliziert. Ansonsten wird die Instanz verwendet und
        /// der ursprünglich dafür alloziierte Speicher freigegeben.</param>
        public MediaType( IntPtr rawMediaType, bool copy )
        {
            // Check mode
            m_HasMediaType = (rawMediaType != IntPtr.Zero);

            // Nothing to do
            if (!m_HasMediaType)
                return;

            // Unpack
            var mediaType = (RawMediaType) Marshal.PtrToStructure( rawMediaType, typeof( RawMediaType ) );

            // Check mode
            if (copy)
            {
                // Copy over
                mediaType.CopyTo( ref m_MediaType );
            }
            else
            {
                // Use as is
                m_RawMediaType = rawMediaType;
                m_MediaType = mediaType;
            }
        }

        /// <summary>
        /// Erzeugt die Speicherstruktur für eine DirectShow Instanz.
        /// </summary>
        /// <remarks>
        /// Die optional assoziierten Speicherstrukturen werden dabei nicht
        /// dupliziert.
        /// </remarks>
        /// <param name="mediaType">Eine DirectShow Speicherstruktur.</param>
        /// <returns>Die Adresse eines Speicherbereiches mit der Instanz.</returns>
        private static IntPtr GetReference( RawMediaType mediaType )
        {
            // Allocate
            IntPtr rawMediaType = Marshal.AllocCoTaskMem( Marshal.SizeOf( typeof( RawMediaType ) ) );

            // Copy in
            Marshal.StructureToPtr( mediaType, rawMediaType, false );

            // Report
            return rawMediaType;
        }

        /// <summary>
        /// Erstellt eine vollständige Kopie der DirectShow Instanz. Dies
        /// schließt alls Substrukturen mit ein.
        /// <seealso cref="GetReference(RawMediaType)"/>
        /// </summary>
        /// <returns>Die Speicheradress der kopierten Instanz.</returns>
        public IntPtr CreateCopy()
        {
            // None
            if (!m_HasMediaType)
                return IntPtr.Zero;

            // Copy
            var mediaType = new RawMediaType();

            // Full copy
            m_MediaType.CopyTo( ref mediaType );

            // Report
            return GetReference( mediaType );
        }

        /// <summary>
        /// Ermittelt eine Speicheradresse zu dieser DirectShow
        /// Instanz.
        /// <seealso cref="GetReference(RawMediaType)"/>
        /// </summary>
        /// <returns>Speicheradresse mit der Instanz.</returns>
        public IntPtr GetReference()
        {
            // None
            if (!m_HasMediaType)
                return IntPtr.Zero;

            // Load cache
            if (m_RawMediaType == IntPtr.Zero)
                m_RawMediaType = GetReference( m_MediaType );

            // Report
            return m_RawMediaType;
        }

        /// <summary>
        /// Kopiert die DriectShow Instanz mit sämtlichen Substrukturen in eine
        /// andere Instanz.
        /// </summary>
        /// <param name="target">Zu überschreibenede Instanz.</param>
        public void CopyTo( ref RawMediaType target )
        {
            // Forward
            m_MediaType.CopyTo( ref target );
        }

        /// <summary>
        /// Liest oder verändert das primäre Format.
        /// </summary>
        public Guid MajorType
        {
            get
            {
                // Report
                return m_MediaType.MajorType;
            }
            set
            {
                // Change
                m_MediaType.MajorType = value;

                // Get rid of helper
                FreeMemory();
            }
        }

        /// <summary>
        /// Liest oder verändert die optionale Formatbeschreibung.
        /// </summary>
        public Guid FormatType
        {
            get
            {
                // Report
                return m_MediaType.FormatType;
            }
            set
            {
                // Change
                m_MediaType.FormatType = value;

                // Get rid of helper
                FreeMemory();
            }
        }

        /// <summary>
        /// Liest oder verändert das sekundäre Format.
        /// </summary>
        public Guid SubType
        {
            get
            {
                // Report
                return m_MediaType.SubType;
            }
            set
            {
                // Change
                m_MediaType.SubType = value;

                // Get rid of helper
                FreeMemory();
            }
        }

        /// <summary>
        /// Assoziiert eine Detailbeschreibung zu einem Format mit
        /// dieser DirectShow Instanz.
        /// </summary>
        /// <param name="structure">Eine Formatbeschreibung.</param>
        public void SetFormat( object structure )
        {
            // Forward
            m_MediaType.SetFormat( structure );

            // Get rid of helper
            FreeMemory();
        }

        /// <summary>
        /// Gestattet Zugriff auf die unterliegende DirectShow Struktur. Dabei wird
        /// keine vollständige Kopie angelegt, ein Freigeben von Unterstrukturen ist nicht
        /// notwendig. Auf die Kopie sollte nur lesend zugegriffen werden.
        /// </summary>
        public RawMediaType DirectShowObject
        {
            get
            {
                // Report
                return m_MediaType;
            }
        }

        /// <summary>
        /// Gibt nach Veränderungen an der DirectShow Instanz eine möglicherweise
        /// bereits erzeugte Speicherkopie der Instanz frei.
        /// <seealso cref="GetReference()"/>
        /// </summary>
        private void FreeMemory()
        {
            // Release helper memory
            if (m_RawMediaType != IntPtr.Zero)
                try
                {
                    // Release
                    Marshal.FreeCoTaskMem( m_RawMediaType );
                }
                finally
                {
                    // Forget
                    m_RawMediaType = IntPtr.Zero;
                }
        }

        #region IDisposable Members

        /// <summary>
        /// Gibt alle mit dieser DirectShow Instanz verbundenen Ressourcen
        /// wieder frei.
        /// </summary>
        public void Dispose()
        {
            // Release helper memory
            FreeMemory();

            // Release primary memory
            m_MediaType.FreeMemory();
        }

        #endregion
    }
}
