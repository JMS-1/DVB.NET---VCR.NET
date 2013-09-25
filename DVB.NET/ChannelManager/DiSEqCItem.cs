using System;
using System.Text;
using JMS.DVB.Satellite;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.ChannelManagement
{
	/// <summary>
	/// Single DVB-S receiver.
	/// </summary>
	[Serializable]
	public class DiSEqCItem
	{
		/// <summary>
		/// The LNB settings.
		/// </summary>
		[XmlElement(typeof(DiSEqCNone))]
		[XmlElement(typeof(DiSEqCSimple))]
		[XmlElement(typeof(DiSEqCMulti))]
		public DiSEqC LNB;

		/// <summary>
		/// Satellite names attached to this LNB.
		/// </summary>
		public string[] TargetNames;

		/// <summary>
		/// Create a new instance.
		/// </summary>
		public DiSEqCItem()
		{
		}

		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="settings">The LNB configuration.</param>
		/// <param name="satellites">The satellites received using this LNB.</param>
		public DiSEqCItem(DiSEqC settings, params string[] satellites)
		{
			// Remember
			TargetNames = satellites;
			LNB = settings;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// Create base
			int hash = LNB.GetHashCode() ^ TargetNames.Length.GetHashCode();

			// Finish
			foreach (string target in TargetNames) 
				if (null != target)
					hash ^= target.GetHashCode();

			// Report
			return hash;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			// Change type
			DiSEqCItem other = obj as DiSEqCItem;

			// Not possible
			if (null == other) return false;

			// Core
			if (!Equals(LNB, other.LNB) || (TargetNames.Length != other.TargetNames.Length)) return false;

			// All
			for (int i = TargetNames.Length; i-- > 0; )
				if (!Equals(TargetNames[i], other.TargetNames[i]))
					return false;

			// Yeah
			return true;
		}
	}
}
