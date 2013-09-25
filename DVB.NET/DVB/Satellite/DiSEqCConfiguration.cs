using System;
using System.IO;
using System.Collections;
using System.Xml.Serialization;

namespace JMS.DVB.Satellite
{
	/// <summary>
	/// Hold the configuration for four LNBs.
	/// </summary>
	[Serializable] public class DiSEqCConfiguration
	{
		/// <summary>
		/// All variations of a single configuration.
		/// </summary>
		private static Type[] m_KnownTypes = { typeof(DiSEqCNone), typeof(DiSEqCSimple), typeof(DiSEqCMulti) };

		/// <summary>
		/// First LNB.
		/// </summary>
		public DiSEqC LNB0 = new DiSEqCNone();

		/// <summary>
		/// Second LNB.
		/// </summary>
		public DiSEqC LNB1 = new DiSEqCNone();

		/// <summary>
		/// Third LNB.
		/// </summary>
		public DiSEqC LNB2 = new DiSEqCNone();

		/// <summary>
		/// Forth LNB. 
		/// </summary>
		public DiSEqC LNB3 = new DiSEqCNone();

		/// <summary>
		/// The file path used in the last successfull <see cref="Load(string)"/>.
		/// </summary>
		private string m_LoadedFrom = null;

		/// <summary>
		/// Create an uninitialized instance.
		/// </summary>
		public DiSEqCConfiguration()
		{
		}

		/// <summary>
		/// Get the number of configurable receivers - currently four.
		/// </summary>
		public int Count
		{
			get
			{
				// Fixed
				return 4;
			}
		}

		/// <summary>
		/// Access a receiver by its index.
		/// </summary>
		public DiSEqC this[int lnbIndex]
		{
			get
			{
				// Easy
				switch (lnbIndex)
				{
					case 0	: return LNB0;
					case 1	: return LNB1;
					case 2	: return LNB2;
					case 3	: return LNB3;
				}

				// Not there
				throw new ArgumentOutOfRangeException("lnbIndex", lnbIndex, "There are only four settings available");
			}
			set
			{
				// Verify
				if ( null == value ) throw new ArgumentNullException("value");

				// Easy
				switch (lnbIndex)
				{
					case 0	: LNB0 = value; return;
					case 1	: LNB1 = value; return;
					case 2	: LNB2 = value; return;
					case 3	: LNB3 = value; return;
				}

				// Not there
				throw new ArgumentOutOfRangeException("lnbIndex", lnbIndex, "There are only four settings available");
			}
		}

		/// <summary>
		/// Save to the file we <see cref="Load(string)"/> from.
		/// </summary>
		public void Save()
		{
			// Use last loaded path
			Save(m_LoadedFrom);
		}

		/// <summary>
		/// Save configuration to a disk file.
		/// </summary>
		/// <param name="filePath">Full path to the file.</param>
		public void Save(string filePath)
		{
			// Create the file
			using (FileStream pOut = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				// Create a serializer
				XmlSerializer pSer = new XmlSerializer(GetType(), m_KnownTypes);

				// Store self
				pSer.Serialize(pOut, this);
			}
		}

		/// <summary>
		/// Load configuration from a disk file.
		/// </summary>
		/// <remarks>If successfull the file name is remembered for <see cref="Save()"/>.</remarks>
		/// <param name="filePath">Full path to a disk file.</param>
		public void Load(string filePath)
		{
			// Create the file
			using (FileStream pOut = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				// Create a serializer
				XmlSerializer pSer = new XmlSerializer(GetType(), m_KnownTypes);

				// Store self
				DiSEqCConfiguration pNew = (DiSEqCConfiguration)pSer.Deserialize(pOut);

				// Copy over
				LNB0 = pNew.LNB0;
				LNB1 = pNew.LNB1;
				LNB2 = pNew.LNB2;
				LNB3 = pNew.LNB3;

				// Remember
				m_LoadedFrom = filePath;
			}
		}

		/// <summary>
		/// Allow easy iteration on the four configurable settings.
		/// </summary>
		/// <returns>Enumerator on an <see cref="Array"/> of the four configurable
		/// receiver settings.</returns>
		public IEnumerator GetEnumerator()
		{
			// Create helper array
			DiSEqC[] ret = new DiSEqC[] { LNB0, LNB1, LNB2, LNB3 };

			// Report
			return ret.GetEnumerator();
		}
	}
}
