using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// Dient zum verz�gerten Decodieren der Descriptoren in einer 
	/// SI Struktur.
	/// </summary>
	public class DescriptorLoader
	{
		/// <summary>
		/// Die zugeh�rige SI Struktur.
		/// </summary>
		private IDescriptorContainer m_Container;

		/// <summary>
		/// Erstes Byte der Daten f�r die Descriptoren.
		/// </summary>
		private int m_Offset;

		/// <summary>
		/// Anzahl der Bytes f�r Descriptoren.
		/// </summary>
		private int m_Length;

		/// <summary>
		/// Die decodierten Descriptoren.
		/// </summary>
		private Descriptor[] m_Descriptors = null;

		/// <summary>
		/// Erzeugt eine neue Ladeinstanz.
		/// </summary>
		/// <param name="table">Die zugeh�rige SI Struktur.</param>
		/// <param name="offset">Erstes Byte f�r Descriptoren.</param>
		/// <param name="length">Anzahl der Bytes f�r Descriptoren.</param>
		public DescriptorLoader(IDescriptorContainer table, int offset, int length)
		{
			// Remember all
			m_Container = table;
			m_Offset = offset;
			m_Length = length;
		}

		/// <summary>
		/// Meldet die zugeh�rigen Descriptoren.
		/// </summary>
		public Descriptor[] Descriptors
		{
			get
			{
				// Load once
				if (null == m_Descriptors) m_Descriptors = Descriptor.Load(m_Container, m_Offset, m_Length);

				// Report
				return m_Descriptors;
			}
		}
	}
}
