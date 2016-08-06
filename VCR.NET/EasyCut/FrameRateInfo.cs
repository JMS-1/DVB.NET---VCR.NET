namespace EasyCut
{
    /// <summary>
    /// Maps the MPEG2 frame rate codes to more useful values.
    /// </summary>
    public sealed class FrameRateInfo
    {
        /// <summary>
        /// Supported frame rate codes.
        /// </summary>
        private static readonly FrameRateInfo[] m_RateMap = new FrameRateInfo[16];

        /// <summary>
        /// Load the frame rate code table.
        /// </summary>
        static FrameRateInfo()
        {
            // Load all
            m_RateMap[0x0] = new FrameRateInfo( 1, 1 );
            m_RateMap[0x1] = new FrameRateInfo( 24000, 1001 );
            m_RateMap[0x2] = new FrameRateInfo( 24, 1 );
            m_RateMap[0x3] = new FrameRateInfo( 25, 1 );
            m_RateMap[0x4] = new FrameRateInfo( 30000, 1001 );
            m_RateMap[0x5] = new FrameRateInfo( 30, 1 );
            m_RateMap[0x6] = new FrameRateInfo( 50, 1 );
            m_RateMap[0x7] = new FrameRateInfo( 60000, 1001 );
            m_RateMap[0x8] = new FrameRateInfo( 60, 1 );
            m_RateMap[0x9] = new FrameRateInfo( 15, 1001 );
            m_RateMap[0xa] = new FrameRateInfo( 5, 1001 );
            m_RateMap[0xb] = new FrameRateInfo( 10, 1001 );
            m_RateMap[0xc] = new FrameRateInfo( 12, 1001 );
            m_RateMap[0xd] = new FrameRateInfo( 15, 1001 );
            m_RateMap[0xe] = new FrameRateInfo( 1, 1 );
            m_RateMap[0xf] = new FrameRateInfo( 1, 1 );
        }

        /// <summary>
        /// The nominator.
        /// </summary>
        public readonly int Nominator;

        /// <summary>
        /// The denominator.
        /// </summary>
        public readonly int Denominator;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="nominator">The nominator.</param>
        /// <param name="denominator">The denominator.</param>
        private FrameRateInfo( int nominator, int denominator )
        {
            // Remember
            Nominator = nominator;
            Denominator = denominator;
        }

        /// <summary>
        /// Report the frame rate related to the frame rate code.
        /// </summary>
        public double Rate => Nominator * 1.0 / Denominator;

        /// <summary>
        /// Find a frame rate for a given MPEG2 frame rate code.
        /// </summary>
        /// <param name="index">A frame rate code from 0 to 15.</param>
        /// <returns>The related information instance.</returns>
        public static FrameRateInfo FindFrameRate( int index ) => m_RateMap[index];

        /// <summary>
        /// Show as string.
        /// </summary>
        /// <returns>Formatted version of <see cref="Rate"/>.</returns>
        public override string ToString()
        {
            // Check mode
            if (Denominator == 1)
                return Rate.ToString( "00.00" );

            // Special
            return $"{Rate:00.00} ({Nominator}/{Denominator})";
        }
    }
}
