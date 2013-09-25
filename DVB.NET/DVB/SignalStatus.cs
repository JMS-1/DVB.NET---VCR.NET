using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB
{
	public class SignalStatus
	{
		public readonly bool Locked;
		public readonly double Strength;
		public readonly double Quality;

		public SignalStatus(bool locked, double strength, double quality)
		{
			// Remember
			Locked = locked;
			Strength = strength;
			Quality = quality;
		}
	}
}
