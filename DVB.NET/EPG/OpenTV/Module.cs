using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMS.DVB.EPG.OpenTV
{
    /// <summary>
    /// Hilfsklasse zum Zusammenstellen der Daten eines einzelnen Moduls.
    /// </summary>
    public class Module
    {
        /// <summary>
        /// Signatur einer Methode, die aufgerufen wird, wenn ein Modul komplett zusammengestellt wurde.
        /// </summary>
        /// <param name="lastTable">Die letzte SI Tabelle des Moduls.</param>
        /// <param name="module">Das fertiggestellte Modul.</param>
        public delegate void CompleteHandler(Tables.OpenTV lastTable, Module module);

        /// <summary>
        /// Zähler, der anzeigt, wie oft ein fehlerhafter Datenoffset erkannt wurde.
        /// </summary>
        public long WrongOffset = 0;

        /// <summary>
        /// Zähler, der anzeigt, wie oft das Modul zu gross war.
        /// </summary>
        public long MemoryError = 0;

        /// <summary>
        /// Zähler, der anzeigt, wie oft ein Datenüberlauf erkannt wurde.
        /// </summary>
        public long OverRunError = 0;

        /// <summary>
        /// Zähler, der anzeigt, wie oft die Modullänge nicht mit den Daten der SI Tabellen
        /// konsistent war.
        /// </summary>
        public long WrongLength = 0;

        /// <summary>
        /// Zähler, der anzeigt, wie oft eine Dekomprimierung fehlgeschlagen ist.
        /// </summary>
        public long CompressionError = 0;

        /// <summary>
        /// Nächster erwarteter Offset.
        /// </summary>
        private uint m_NextOffset = 0;

        /// <summary>
        /// Daten zum Modul.
        /// </summary>
        private byte[] m_Collector = null;

        /// <summary>
        /// Methode, die bei Komplettierung eines Moduls aufgerufen wird.
        /// </summary>
        public event CompleteHandler OnModuleComplete;

        /// <summary>
        /// Erzeugt eine neues Modul.
        /// </summary>
        public Module()
        {
        }

        /// <summary>
        /// Meldet die Daten zum Modul - diese Methode darf nur
        /// innerhalb von <see cref="OnModuleComplete"/> aufgerufen
        /// werden.
        /// </summary>
        public byte[] ModuleData
        {
            get
            {
                // Report
                return m_Collector;
            }
        }

        /// <summary>
        /// Dekomprimiert die Moduldaten, sofern erforderlich.
        /// </summary>
        /// <returns>Gesetzt, wenn keine Komprimierung vorlag oder diese erfolgreich aufgelöst wurde.</returns>
        private bool Decompress()
        {
            // Be safe
            try
            {
                // Create helper
                Decompressor worker = new Decompressor();

                // Set it up
                bool? start = worker.StartDecompression(m_Collector).GetValueOrDefault();

                // We are in error
                if (!start.HasValue)
                {
                    // Count
                    ++CompressionError;

                    // Done
                    return false;
                }

                // There is no compression
                if (!start.Value) return true;

                // Loop over
                while (worker.DecompressPacket()) ;

                // Check for error
                m_Collector = worker.FinishDecompression();

                // Did it
                if (null != m_Collector) return true;
            }
            catch
            {
                // Report any error
            }

            // Report
            ++CompressionError;

            // Stop right here
            return false;
        }

        /// <summary>
        /// Ergänz Teildaten zu diesem Modul.
        /// </summary>
        /// <param name="table">Die SI Tabelle mit den Teildaten.</param>
        public void AddPartialModule(Tables.OpenTV table)
        {
            // Check for expected offset
            if (table.SectionOffset != m_NextOffset)
            {
                // See if we are synchronizing
                if (0 != m_NextOffset)
                {
                    // Lost a part.
                    ++WrongOffset;

                    // Restart
                    m_NextOffset = 0;
                    m_Collector = null;
                }

                // Continue synchronizing.
                if (table.SectionOffset != 0) return;
            }

            // Check for the head of the module
            if (0 == m_NextOffset)
                try
                {
                    // Allocate memory
                    m_Collector = new byte[table.ModuleLength];
                }
                catch
                {
                    // Not enough free space
                    ++MemoryError;

                    // Done
                    return;
                }

            // Check consistency
            if (m_Collector.Length != table.ModuleLength)
            {
                // SI table data mismatch
                ++WrongLength;

                // Reset
                m_NextOffset = 0;
                m_Collector = null;

                // Next
                return;
            }

            // See if data fits
            if ((m_NextOffset + table.DataLength) > m_Collector.Length)
            {
                // Too much data
                ++OverRunError;

                // Reset
                m_NextOffset = 0;
                m_Collector = null;

                // Next
                return;
            }

            // Store data into raw buffer
            table.CopyTo(m_Collector, (int)m_NextOffset);

            // Advance offset
            m_NextOffset += table.DataLength;

            // See if module is complete
            if (m_NextOffset == table.ModuleLength)
            {
                // Try decompression
                if (Decompress())
                {
                    // Get the callback
                    CompleteHandler callback = OnModuleComplete;

                    // Report finished packet to client
                    if (null != callback) callback(table, this);
                }

                // Reset
                m_NextOffset = 0;
                m_Collector = null;
            }
        }
    }
}
