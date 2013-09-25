using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Mit dieser Struktur beschreibt DirectShow das Format von
    /// Datenpaketen im Datenstrom.
    /// </summary>
    /// <remarks>
    /// Mit der Struktur verbunden sind weitere Speicherstrukturen, 
    /// die mit <see cref="Marshal.AllocCoTaskMem"/> reserviert 
    /// werden. Bei Benutzung der Struktur ist auf das korrekte
    /// Verwenden der <see cref="FreeMemory"/> Methode zu achten.
    /// </remarks>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct RawMediaType
    {
        /// <summary>
        /// Prim�rer Datentyp.
        /// </summary>
        public Guid MajorType;

        /// <summary>
        /// Sekund�rer Datentyp.
        /// </summary>
        public Guid SubType;

        /// <summary>
        /// Zeigt an, ob alle Datenpakete die gleiche Gr��e haben.
        /// </summary>
        public Int32 FixedSizeSamples;

        /// <summary>
        /// Zeigt an, ob eine zeitliche Komprimierung vorliegt.
        /// </summary>
        public Int32 TemporalCompression;

        /// <summary>
        /// Maximale Gr��e eines Datenpaketes.
        /// </summary>
        public UInt32 SampleSize;

        /// <summary>
        /// Optionale Formatbezeichnung.
        /// </summary>
        public Guid FormatType;

        /// <summary>
        /// Ein Objekt, dass mit dieser Struktur verwaltet wird.
        /// </summary>
        private IntPtr Holder;

        /// <summary>
        /// Gr��e der zus�tzlichen Formatinformation.
        /// </summary>
        private Int32 FormatSize;

        /// <summary>
        /// Zus�tzliche Formatinformation.
        /// </summary>
        /// <remarks>
        /// Der Speicher muss mit <see cref="Marshal.FreeCoTaskMem"/>
        /// freigegeben werden.
        /// </remarks>
        private IntPtr FormatPtr;

        /// <summary>
        /// �bertr�gt eine beliebige Struktur in die zus�tzlichen Formatinformationen.
        /// </summary>
        /// <param name="structure">Formatinformationen als Struktur oder
        /// <i>null</i>.</param>
        public void SetFormat( object structure )
        {
            // Wipe out
            Format = null;

            // Done
            if (structure == null)
                return;

            // Get the size
            FormatSize = Marshal.SizeOf( structure );

            // Check mode
            if (FormatSize > 0)
            {
                // Allocate
                FormatPtr = Marshal.AllocCoTaskMem( FormatSize );

                // Fill
                Marshal.StructureToPtr( structure, FormatPtr, false );
            }
        }

        /// <summary>
        /// Ermittelt oder ver�ndert die Formatinformationen.
        /// </summary>
        public byte[] Format
        {
            get
            {
                // None
                if (FormatSize < 1)
                    return null;

                // Helper
                var current = new byte[FormatSize];

                // Fill helper
                Marshal.Copy( FormatPtr, current, 0, current.Length );

                // Report
                return current;
            }
            set
            {
                // Free
                if (FormatSize > 0)
                    if (FormatPtr != IntPtr.Zero)
                        Marshal.FreeCoTaskMem( FormatPtr );

                // Reset
                FormatPtr = IntPtr.Zero;
                FormatSize = 0;

                // Done
                if (value == null)
                    return;
                if (value.Length < 1)
                    return;

                // Remember
                FormatSize = value.Length;

                // Check mode
                if (FormatSize > 0)
                {
                    // Allocate
                    FormatPtr = Marshal.AllocCoTaskMem( value.Length );

                    // Use
                    Marshal.Copy( value, 0, FormatPtr, value.Length );
                }
            }
        }

        /// <summary>
        /// Erstellt eine Kopie dieser Formatbeschreibung.
        /// </summary>
        /// <remarks>
        /// Die Kopie mu� sp�ter mit <see cref="FreeMemory"/>
        /// freigegeben werden.
        /// </remarks>
        /// <param name="target">Zu f�llende Kopie. Der Aufrufer hat daf�r
        /// Sorge zu tragen, dass alle sekund�ren Speicherstrukturen
        /// bereits freigegeben sind.</param>
        public void CopyTo( ref RawMediaType target )
        {
            // Copy all
            target.TemporalCompression = TemporalCompression;
            target.FixedSizeSamples = FixedSizeSamples;
            target.SampleSize = SampleSize;
            target.FormatType = FormatType;
            target.MajorType = MajorType;
            target.SubType = SubType;
            target.Format = Format;
            target.Holder = Holder;

            // Correct the reference
            if (target.Holder != IntPtr.Zero)
                Marshal.AddRef( target.Holder );
        }

        /// <summary>
        /// Gibt alle sekund�ren Ressourcen zu dieser Formatbeschreibung
        /// frei.
        /// <seealso cref="CopyTo"/>
        /// </summary>
        public void FreeMemory()
        {
            // Check for holder
            BDAEnvironment.Release( ref Holder );

            // Reset
            Format = null;
        }
    }
}
