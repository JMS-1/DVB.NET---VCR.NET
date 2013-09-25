using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.TechnoTrend
{
	public class DVBSignalStatus
	{
		public readonly bool Locked;

		public readonly double Level;

		public readonly double Quality;

		public readonly double ErrorRate;

		public readonly double Strength;

		public readonly double SignalNoise;

		public DVBSignalStatus(bool locked, double level, double quality, double BER, double strength, double rawQuality)
		{
			// Remember
			SignalNoise = rawQuality;
			Strength = strength;
			Quality = quality;
			ErrorRate = BER;
			Locked = locked;
			Level = level;
		}
	}
}
