using System;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Beschreibt eine Steuermeldung f�r den Satellitenempfang.
    /// </summary>
    public class StandardDiSEqC : ICloneable
    {
        /// <summary>
        /// Die Standardsequenz von Bytes f�r die Meldung.
        /// </summary>
        public readonly byte[] Request;

        /// <summary>
        /// Zeigt einen Burst Modus an.
        /// </summary>
        public readonly byte Burst;

        /// <summary>
        /// Beschreibt die notwendigen Wiederholungen.
        /// </summary>
        public readonly int Repeat;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="data">Die Sequenz der Bytes f�r die Meldung.</param>
        /// <param name="burst">Anzeige des Burst Modus.</param>
        /// <param name="repeat">Die Anzahl der ben�tigten Wiederholungen der Sendung.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Meldung angegeben.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Die Anzahl der Wiederholungen muss positiv sein.</exception>
        private StandardDiSEqC( byte[] data, byte burst, int repeat )
        {
            // Validate
            if (data == null)
                throw new ArgumentNullException( "data" );
            if (repeat <= 0)
                throw new ArgumentOutOfRangeException( "repeat" );

            // Remember
            Repeat = repeat;
            Request = data;
            Burst = burst;
        }

        /// <summary>
        /// Ein Kurzschl�ssel f�r diese Meldung.
        /// </summary>
        /// <returns>Der angeforderte Schl�ssel.</returns>
        public override int GetHashCode()
        {
            // Merge all
            return Request.GetHashCode() ^ Burst.GetHashCode() ^ Repeat.GetHashCode();
        }

        /// <summary>
        /// Vergleicht zwei Meldungen auf semantische Gleichheit.
        /// </summary>
        /// <param name="obj">Ein beliebiges .NET Objekt.</param>
        /// <returns>Gesetzt, wenn es sich bei dem Paramater um eine semantisch �quivalente Meldung handelt.</returns>
        public override bool Equals( object obj )
        {
            // Convert
            var other = obj as StandardDiSEqC;
            if (other == null)
                return false;

            // Pre-Test
            if (Request.Length != other.Request.Length)
                return false;
            if (Burst != other.Burst)
                return false;
            if (Repeat != other.Repeat)
                return false;

            // Compare message itself
            for (int i = Request.Length; i-- > 0; )
                if (Request[i] != other.Request[i])
                    return false;

            // Same message
            return true;
        }

        /// <summary>
        /// Erzeugt eine exakte, eigenst�ndige Kopie dieser Meldung.
        /// </summary>
        /// <returns>Die gew�nschte Kopie.</returns>
        public StandardDiSEqC Clone()
        {
            // Process
            return new StandardDiSEqC( (byte[]) Request.Clone(), Burst, Repeat );
        }

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine exakte, eigenst�ndige Kopie dieser Meldung.
        /// </summary>
        /// <returns>Die gew�nschte Kopie.</returns>
        object ICloneable.Clone()
        {
            // Forward
            return Clone();
        }

        #endregion

        /// <summary>
        /// Ermittelt die DiSEqC Steuersequenz aus der Angabe einer Quellgruppe und ihres Ursprungs.
        /// </summary>
        /// <param name="group">Die zu pr�fende Quellgruppe.</param>
        /// <param name="location">Der zugeh�rige Ursprung.</param>
        /// <returns>Die gew�nschten Informationen.</returns>
        /// <exception cref="ArgumentNullException">Mindestens ein Parameter ist nicht gesetzt.</exception>
        public static StandardDiSEqC FromSourceGroup( SatelliteGroup group, SatelliteLocation location )
        {
            // Validate
            if (group == null)
                throw new ArgumentNullException( "group" );
            if (location == null)
                throw new ArgumentNullException( "location" );

            // Core flags
            int choice = 0;
            if (group.Frequency >= location.SwitchFrequency)
                choice |= 0x01;
            if (group.Polarization == Polarizations.Horizontal)
                choice |= 0x02;

            // Check mode
            switch (location.LNB)
            {
                case DiSEqCLocations.None: return new StandardDiSEqC( new byte[] { 0xe0, 0x00, 0x00 }, 0xff, 1 );
                case DiSEqCLocations.BurstOn: return new StandardDiSEqC( new byte[0], 0, 1 );
                case DiSEqCLocations.BurstOff: return new StandardDiSEqC( new byte[0], 1, 1 );
                case DiSEqCLocations.DiSEqC1: return new StandardDiSEqC( new byte[] { 0xe0, 0x10, 0x38, (byte) (0xf0 | choice) }, 0xff, 3 );
                case DiSEqCLocations.DiSEqC2: return new StandardDiSEqC( new byte[] { 0xe0, 0x10, 0x38, (byte) (0xf4 | choice) }, 0xff, 3 );
                case DiSEqCLocations.DiSEqC3: return new StandardDiSEqC( new byte[] { 0xe0, 0x10, 0x38, (byte) (0xf8 | choice) }, 0xff, 3 );
                case DiSEqCLocations.DiSEqC4: return new StandardDiSEqC( new byte[] { 0xe0, 0x10, 0x38, (byte) (0xfc | choice) }, 0xff, 3 );
                default: throw new NotImplementedException( location.LNB.ToString() );
            }
        }
    }
}
