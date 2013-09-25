using System;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
	/// <summary>
	/// Informationen zu einem einzelnen Tonkanal.
	/// </summary>
	[Serializable]
	public class AudioInfo
	{
		/// <summary>
		/// Name des Tonkanals.
		/// </summary>
		[XmlAttribute]
		public string Name;

		/// <summary>
		/// Datenstromkennung des Tonkanals.
		/// </summary>
		[XmlAttribute]
		public ushort PID;

		/// <summary>
		/// Gesetzt, wenn es sich um einen AC3 Tonkanal handel.
		/// </summary>
		[XmlIgnore]
		public bool AC3;

		/// <summary>
		/// Erzeugt eine neue Information.
		/// </summary>
		public AudioInfo()
		{
		}

		/// <summary>
		/// Erzeugt eine neue Information
		/// </summary>
		/// <param name="pid">Datenstromkennung.</param>
		/// <param name="name">Anzeigename.</param>
		/// <param name="isAC3">Getzt für AC3 Tonspuren.</param>
		public AudioInfo(ushort pid, string name, bool isAC3)
		{
			// Remember
			Name = name;
			AC3 = isAC3;
			PID = pid;
		}

		/// <summary>
		/// Erzeugt eine neue Information
		/// </summary>
		/// <param name="pid">Datenstromkennung.</param>
		/// <param name="name">Anzeigename.</param>
		public AudioInfo(ushort pid, string name)
			: this(pid, name, false)
		{
		}

		public static AudioInfo Create(ushort pid, string name, bool isAC3)
		{
			// Create the instance with the full name
			return new AudioInfo(pid, string.Format("{0} {2}[{1}]", name, pid, isAC3 ? "(AC3) " : string.Empty), isAC3);
		}

		public override string ToString()
		{
			// Unique name
			return Name;
		}

		public string ISOLanguage
		{
			get
			{
				// Report
				return Station.FindISOLanguage(Name);
			}
		}
	}
}
