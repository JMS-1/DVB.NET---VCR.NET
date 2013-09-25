using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Verwaltet eine Liste von Ein- und Ausgängen.
    /// </summary>
    public class PinEnum : IEnumPins
    {
        /// <summary>
        /// Die elementaren Referenzen der verwalteten Instanzen.
        /// </summary>
        private List<TypedComIdentity<IPin>> m_Pins = new List<TypedComIdentity<IPin>>();

        /// <summary>
        /// Die aktuelle Position zum Auslesen.
        /// </summary>
        private uint m_Index = 0;

        /// <summary>
        /// Erzeugt eine neue Liste.
        /// </summary>
        /// <param name="pins">Die zu verwaltenden Instanzen.</param>
        private PinEnum( List<TypedComIdentity<IPin>> pins )
        {
            // Remember
            m_Pins = pins;
        }

        /// <summary>
        /// Erzeugt eine leere Liste.
        /// </summary>
        public PinEnum()
        {
        }

        /// <summary>
        /// Fügt eine Instanz zur Verwaltung hinzu.
        /// </summary>
        /// <param name="pin">Die zu verwaltende Instanz.</param>
        public void Add( TypedComIdentity<IPin> pin )
        {
            // Remember
            m_Pins.Add( pin );
        }

        #region IEnumPins Members

        /// <summary>
        /// Ermittelt die nächste Instanz.
        /// </summary>
        /// <param name="pins">Die Anzahl der Element im Ergebnisfeld.</param>
        /// <param name="pinArray">Das Ergebnisfeld.</param>
        /// <param name="fetched">Die Anzahl der übermittelten Instanzen.</param>
        /// <returns>Ein Fehlercode oder <i>1</i>, wenn keine weiteren Instanzen vorhanden sind.</returns>
        public Int32 Next( uint pins, IntPtr pinArray, IntPtr fetched )
        {
            // Not possible
            if ((m_Index >= m_Pins.Count) || (pins < 1))
            {
                // Report
                if (fetched != IntPtr.Zero)
                    Marshal.WriteInt32( fetched, 0 );

                // Done
                return 1;
            }

            // Report the next pin
            Marshal.WriteIntPtr( pinArray, m_Pins[(int) m_Index].AddRef() );

            // Report
            if (fetched != IntPtr.Zero)
                Marshal.WriteInt32( fetched, 1 );

            // Increment
            m_Index++;

            // Success
            return 0;
        }

        /// <summary>
        /// Überspringt einige Instanzen beim Auslesen.
        /// </summary>
        /// <param name="pins">Die Anzahl der zu überpringenden Instanzen.</param>
        public void Skip( uint pins )
        {
            // Advance
            m_Index = Math.Min( (uint) m_Pins.Count, m_Index + pins );
        }

        /// <summary>
        /// Meldet bei der nächsten Abfrage wieder die erste Instanz.
        /// </summary>
        public void Reset()
        {
            // Back
            m_Index = 0;
        }

        /// <summary>
        /// Erzeugt eine Kopie dieser Liste, deren Auflistung wieder
        /// am Anfang der verwalteten Instanzen beginnt.
        /// </summary>
        /// <returns>Die angeforderte Kopie.</returns>
        public IEnumPins Clone()
        {
            // Create
            return new PinEnum( m_Pins ) { m_Index = m_Index };
        }

        #endregion
    }
}
