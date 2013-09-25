using System;
using System.Collections;

namespace JMS.DVB.TS.Tables
{
	/// <summary>
	/// Instances of this class represent SI service description tables.
	/// </summary>
	public class SDT: SITableBase
	{
		/// <summary>
		/// Related network identifier.
		/// </summary>
		private short m_NetworkNumber;

		/// <summary>
		/// Program reference inside the transport stream.
		/// </summary>
		private short m_ProgramNumber;

		/// <summary>
		/// Create a new table instance.
		/// </summary>
		/// <param name="network">Related network identifier.</param>
		/// <param name="program">Program reference inside the transport stream.</param>
		public SDT(short network, short program) : base(0x0011)
		{
			// Remember
			m_NetworkNumber = network;
			m_ProgramNumber = program;
		}

		/// <summary>
		/// The identifier for this type of table is <i>0x42</i>.
		/// </summary>
		protected override byte TableIdentifier
		{
			get
			{
				// Identifier
				return 0x42;
			}
		}

		/// <summary>
		/// Private data is always reported as <i>1</i>.
		/// </summary>
		protected override short PrivateData
		{
			get
			{
				// Report
				return 1;
			}
		}

		/// <summary>
		/// Create the inner data of a service description table.
		/// </summary>
		/// <returns></returns>
		protected override byte[] CreateTable()
		{
			// Allocate
			byte[] table = new byte[29];

			// Fill
			table[ 0] = (byte)(m_NetworkNumber / 256);
			table[ 1] = (byte)(m_NetworkNumber & 0xff);
			table[ 2] = 0xff;
			table[ 3] = (byte)(m_ProgramNumber / 256);
			table[ 4] = (byte)(m_ProgramNumber & 0xff);
			table[ 5] = 0xfc;
			table[ 6] = 0x80;
			table[ 7] = 0x15;
			table[ 8] = 0x48;
			table[ 9] = 0x13;
			table[10] = 0x01;
			table[11] = 0x08;
			table[12] = 0x05;
			table[13] = (byte)('D');
			table[14] = (byte)('V');
			table[15] = (byte)('B');
			table[16] = (byte)('.');
			table[17] = (byte)('N');
			table[18] = (byte)('E');
			table[19] = (byte)('T');
			table[20] = 0x08;
			table[21] = 0x05;
			table[22] = (byte)('D');
			table[23] = (byte)('V');
			table[24] = (byte)('B');
			table[25] = (byte)('.');
			table[26] = (byte)('N');
			table[27] = (byte)('E');
			table[28] = (byte)('T');

			// Report
			return table;
		}
	}
}
