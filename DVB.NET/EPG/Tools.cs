using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Configuration;
using System.Globalization;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Helper methods for decoding raw <see cref="Section"/> data.
    /// </summary>
    public static class Tools
    {
        private static object m_ScanLogLock = new object();

        /// <summary>
        /// Create some identifier lookup map from a handler <see cref="Type"/> list.
        /// <see cref="Table"/>
        /// <see cref="Descriptor"/>
        /// </summary>
        /// <remarks>
        /// The handler class <see cref="Array"/> in the second parameter will have exactly 256 
        /// elements - one for each possible identifier. Each element will be set to the fallback
        /// <see cref="Type"/> provided in the third paramter. Each handler <see cref="Type"/> from
        /// the first parameter will then be asked for support of some identifier - for this a static
        /// method <i>IsHandlerFor</i> of type <see cref="bool"/> with a single <see cref="byte"/>
        /// parameter is used. The last handler <see cref="Type"/> reporting responsibilty will
        /// then be put in the provider map and used later on.
        /// </remarks>
        /// <param name="handlers">All handler classes to inspect.</param>
        /// <param name="providers">The identifier lookup map to fill.</param>
        /// <param name="defaultProvider">The default <see cref="Type"/> for the lookup map.</param>
        static public void InitializeDynamicCreate( Type[] handlers, Type[] providers, Type defaultProvider )
        {
            // Methods
            MethodInfo[] aTest = new MethodInfo[handlers.Length];

            // Attach to methode
            for (int ih = handlers.Length; ih-- > 0; )
            {
                // Find it
                aTest[ih] = handlers[ih].GetMethod( "IsHandlerFor", BindingFlags.Public | BindingFlags.Static );
            }

            // Process all
            for (int ii = providers.Length; ii-- > 0; )
            {
                // Set default
                providers[ii] = defaultProvider;

                // Parameter to test for
                object[] iiArray = new object[] { (byte) ii };

                // Find it
                for (int ih = handlers.Length; ih-- > 0; )
                    if ((bool) aTest[ih].Invoke( null, iiArray ))
                    {
                        // Remember
                        providers[ii] = handlers[ih];

                        // Next code
                        break;
                    }
            }
        }

        /// <summary>
        /// Merge to bytes to create a full 16-bit word.
        /// </summary>
        /// <param name="bLow">Lower 8 bits.</param>
        /// <param name="bHigh">Upper 8 bits.</param>
        /// <returns>Some word.</returns>
        static public ushort MergeBytesToWord( byte bLow, byte bHigh )
        {
            // Merge
            return (ushort) ((((ushort) bHigh) << 8) + (ushort) bLow);
        }

        /// <summary>
        /// Merge to bytes to create a full 32-bit word.
        /// </summary>
        /// <param name="b0">Lowest 8 bits.</param>
        /// <param name="b1">Middle lower 8 bits.</param>
        /// <param name="b2">Middle upper 8 bits.</param>
        /// <param name="b3">Highest 8 bits.</param>
        /// <returns>Some word.</returns>
        static public uint MergeBytesToDoubleWord( byte b0, byte b1, byte b2, byte b3 )
        {
            // Merge
            return ((uint) b0) + 256 * (((uint) b1) + 256 * (((uint) b2) + 256 * ((uint) b3)));
        }

        /// <summary>
        /// Decode BCD coded number.
        /// </summary>
        /// <remarks>
        /// There is no check on a valid BCD number.
        /// </remarks>
        /// <param name="uBCD">Must be some BCD number.</param>
        /// <returns>Decoded decimal number.</returns>
        static public int FromBCD( byte uBCD )
        {
            // Both parts
            return 10 * ((uBCD >> 4) & 0xf) + (uBCD & 0xf);
        }

        /// <summary>
        /// Create a GMT/UTC time representation from the <see cref="Section"/>
        /// raw data.
        /// </summary>
        /// <remarks>
        /// Please refer to the original document for the encoding algorithm used, e.g
        /// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
        /// </remarks>
        /// <param name="section">The raw data holder.</param>
        /// <param name="offset">The offset to the first of five bytes in
        /// the raw data.</param>
        /// <returns>The corresponding GMT/UTC date and time.</returns>
        static public DateTime DecodeTime( Section section, int offset )
        {
            // Read all parts
            byte t0 = section[offset + 0];
            byte t1 = section[offset + 1];
            int t2 = FromBCD( section[offset + 2] );
            int t3 = FromBCD( section[offset + 3] );
            int t4 = FromBCD( section[offset + 4] );

            // Calculate
            return new DateTime( 1970, 1, 1, t2, t3, t4, DateTimeKind.Utc ).AddDays( MergeBytesToWord( t1, t0 ) - 40587 );
        }

        /// <summary>
        /// Create a GMT/UTC date representation from the <see cref="Section"/>
        /// raw data.
        /// </summary>
        /// <remarks>
        /// Please refer to the original document for the encoding algorithm used, e.g
        /// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
        /// </remarks>
        /// <param name="section">The raw data holder.</param>
        /// <param name="offset">The offset to the first of two bytes in
        /// the raw data.</param>
        /// <returns>The corresponding GMT/UTC date and.</returns>
        static public DateTime DecodeDate( Section section, int offset )
        {
            // Read all parts
            byte t0 = section[offset + 0];
            byte t1 = section[offset + 1];

            // Calculate
            return new DateTime( 1970, 1, 1, 0, 0, 0 ).AddDays( MergeBytesToWord( t1, t0 ) - 40587 );
        }

        /// <summary>
        /// Create a duration representation from the <see cref="Section"/>
        /// raw data.
        /// </summary>
        /// <remarks>
        /// Please refer to the original document for the encoding algorithm used, e.g
        /// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
        /// </remarks>
        /// <param name="section">The raw data holder.</param>
        /// <param name="offset">The offset to the first of three bytes in
        /// the raw data.</param>
        /// <returns>The corresponding duration.</returns>
        static public TimeSpan DecodeDuration( Section section, int offset )
        {
            // Read all parts
            int d0 = FromBCD( section[offset + 0] );
            int d1 = FromBCD( section[offset + 1] );
            int d2 = FromBCD( section[offset + 2] );

            // Calculate
            return new TimeSpan( d0, d1, d2 );
        }

        /// <summary>
        /// Schreibt eine Zeile in die Protokolldatei des Sendersuchlaufs. Diese wird über
        /// den Konfigurationseintrag <i>ScanLog</i> ermittelt.
        /// </summary>
        /// <param name="text">Der gewünschte Text, die aktuelle Uhrzeit sowie die .NET Nummer
        /// <see cref="Thread.ManagedThreadId"/> des laufenden <see cref="Thread"/>s wird automatisch ergänzt.</param>
        public static void WriteToScanLog( string text )
        {
            // Load the file name
            string path = ConfigurationManager.AppSettings["ScanLog"];
            if (string.IsNullOrEmpty( path )) return;

            // Be safe
            try
            {
                // Attach to the file
                lock (m_ScanLogLock)
                    using (StreamWriter writer = new StreamWriter( path, true, Encoding.GetEncoding( 1252 ) ))
                        writer.WriteLine( "{0}\t{2}\t{1}", DateTime.Now, text, Thread.CurrentThread.ManagedThreadId );
            }
            catch
            {
                // Ignore any error
            }
        }

        /// <summary>
        /// Überträgt eine Nachricht in das Protokoll.
        /// </summary>
        /// <param name="format">Das Format zum Aufbau der Nachricht.</param>
        /// <param name="args">Die Parameter zum Aufbau der Nachricht.</param>
        public static void WriteToScanLog( string format, params object[] args )
        {
            // Forward
            WriteToScanLog( string.Format( format, args ) );
        }
    }
}
