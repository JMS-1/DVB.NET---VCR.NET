using System;
using System.Reflection;

namespace JMS.DVB.EPG
{
	/// <summary>
	/// Helper class to verify CRC32 checksums as used in the <see cref="Section"/>
	/// raw data.
	/// </summary>
	/// <remarks>
	/// For more details refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public sealed class CRC32
	{
		/// <summary>
		/// Indexed with some byte the result is a byte with reversed bit 
		/// ordering.
		/// </summary>
		static private byte[] m_Reflection = new byte[256];

		/// <summary>
		/// CRC32 table will be created in static constructor.
		/// </summary>
		static private uint[] m_CRC32 = new uint[256];

		/// <summary>
		/// Fill all class members.
		/// </summary>
		static CRC32()
		{
			// Reflection first
			for ( int ix = 256 ; ix-- > 0 ; )
			{
				// The inverted one
				byte iRef = 0;

				// Take the eaqsy way
				if ( 0 != (ix&0x01) ) iRef |= 0x80;
				if ( 0 != (ix&0x02) ) iRef |= 0x40;
				if ( 0 != (ix&0x04) ) iRef |= 0x20;
				if ( 0 != (ix&0x08) ) iRef |= 0x10;
				if ( 0 != (ix&0x10) ) iRef |= 0x08;
				if ( 0 != (ix&0x20) ) iRef |= 0x04;
				if ( 0 != (ix&0x40) ) iRef |= 0x02;
				if ( 0 != (ix&0x80) ) iRef |= 0x01;

				// Store
				m_Reflection[ix] = iRef;
			}

			// CRC32 table
			uint ulPolynomial = 0x04c11db7, ulTest = 0x80000000;

			// All entries
			for ( uint i = 0 ; i <= 255 ; ++i )
			{
				// Start code
				uint uCRC = i << 24;

				// Create CRC32 code for index - reflection is not used in EPG
				for ( int j = 0 ; j < 8 ; ++j )
				{
					// Adjustment
					uint uAdj = (0 != (uCRC&ulTest)) ? ulPolynomial : 0;

					// Shift
					uCRC <<= 1;
					
					// Update
					uCRC ^= uAdj;
				}

				// Store
				m_CRC32[i] = uCRC;
			}
		}

		/// <summary>
		/// The constructor is marked as private so this class becomes really static.
		/// </summary>
		private CRC32()
		{
		}

		/// <summary>
		/// Swap bits in an 8-bit byte.
		/// </summary>
		/// <param name="data">Input data.</param>
		/// <returns>Bits reversed in byte.</returns>
		static public byte ReflectByte(byte data)
		{
			// Direct access
			return m_Reflection[data];
		}

		/// <summary>
		/// Swap bits in a 32-bit word.
		/// </summary>
		/// <param name="word">A full 32-bit word.</param>
		/// <returns>Bits reversed in word.</returns>
		static public UInt32 ReflectWord(UInt32 word)
		{
			// All parts
			UInt32 uWord0 = m_Reflection[(word >>  0) & 0xff];
			UInt32 uWord1 = m_Reflection[(word >>  8) & 0xff];
			UInt32 uWord2 = m_Reflection[(word >> 16) & 0xff];
			UInt32 uWord3 = m_Reflection[(word >> 24) & 0xff];

			// Create back
			return uWord3 | (uWord2 << 8) | (uWord1 << 16) | (uWord0 << 24);
		}

		/// <summary>
		/// Verify the CRC32 checksum over the indicated bytes.
		/// </summary>
		/// <param name="aData">Full raw input data.</param>
		/// <param name="offset">First byte of the range to verify.</param>
		/// <param name="length">Number of bytes to verify - the last four bytes are
		/// the CRC32 counterpart.</param>
		/// <returns>Set if the CRC code matches.</returns>
		static public bool CheckCRC(byte[] aData, long offset, long length)
		{
			// CRC initializer
			uint uCRC = 0xffffffff;

			// Process
			while ( length-- > 0 )
			{
				// Process
				uCRC = (uCRC << 8) ^ m_CRC32[((uCRC >> 24) ^ aData[offset++]) & 0xff];
			}

			// Should be zero
			return (0 == uCRC);
		}

		/// <summary>
		/// Calculate the CRC32 checksum over the indicated bytes.
		/// </summary>
		/// <param name="aData">Full raw input data.</param>
		/// <param name="offset">First byte of the range to verify.</param>
		/// <param name="length">Number of bytes to verify - the last four bytes are
		/// the CRC32 counterpart.</param>
		/// <returns>The CRC code for the bytes.</returns>
		static public uint GetCRC(byte[] aData, long offset, long length)
		{
			// CRC initializer
			uint uCRC = 0xffffffff;

			// Process
			while ( length-- > 0 )
			{
				// Process
				uCRC = (uCRC << 8) ^ m_CRC32[((uCRC >> 24) ^ aData[offset++]) & 0xff];
			}

			// Report
			return uCRC;
		}
	}
}
