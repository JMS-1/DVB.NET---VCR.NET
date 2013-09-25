using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Bietet eine Auflistung über Datenformate an.
    /// </summary>
    public class TypeEnum : IEnumMediaTypes
    {
        /// <summary>
        /// Die Datenformate der Auflistung.
        /// </summary>
        private MediaType[] m_Types;

        /// <summary>
        /// Das nächste zu meldende Format.
        /// </summary>
        private int m_Index = 0;

        /// <summary>
        /// Erzeugt eine neue Auflistung.
        /// </summary>
        /// <param name="types">Die Datenformate der Auflistung.</param>
        public TypeEnum( MediaType[] types )
        {
            // Remember
            m_Types = types;
        }

        #region IEnumMediaTypes Members

        /// <summary>
        /// Fragt die nächsten Formate der Auflistung ab.
        /// </summary>
        /// <param name="mediaTypes">Die maximale Anzahl der abzufragenden Elemente.</param>
        /// <param name="mediaTypeArray">Der Speicherbereich zur Meldung der Elemente.</param>
        /// <param name="fetched">Die tatsächlich ausgelesene Anzahl von Elementen.</param>
        /// <returns>Das Ergebnis der Abfrage, negative Werte zeigen Fehler an.</returns>
        public Int32 Next( uint mediaTypes, IntPtr mediaTypeArray, out uint fetched )
        {
            // Already done
            if ((m_Index >= m_Types.Length) || (mediaTypes < 1))
            {
                // Report
                fetched = 0;

                // Done
                return 1;
            }

            // Load next
            Marshal.WriteIntPtr( mediaTypeArray, m_Types[m_Index++].CreateCopy() );

            // Report
            fetched = 1;

            // Done
            return 0;
        }

        /// <summary>
        /// Übespringt Datenformate in der Auflistung.
        /// </summary>
        /// <param name="mediaTypes">Die Anzahl der zu überspringenden Element.</param>
        public void Skip( uint mediaTypes )
        {
            // Check
            if (mediaTypes > m_Types.Length)
            {
                // All done
                m_Index = m_Types.Length;
            }
            else
            {
                // Process
                m_Index += (int) mediaTypes;

                // Correct
                if (m_Index > m_Types.Length)
                    m_Index = m_Types.Length;
            }
        }

        /// <summary>
        /// Beginnt die Auflistung mit dem ersten Datenformat neu.
        /// </summary>
        public void Reset()
        {
            // From scratch
            m_Index = 0;
        }

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Auflistung.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public IEnumMediaTypes Clone()
        {
            // Easy
            return new TypeEnum( m_Types ) { m_Index = m_Index };
        }

        #endregion
    }
}
