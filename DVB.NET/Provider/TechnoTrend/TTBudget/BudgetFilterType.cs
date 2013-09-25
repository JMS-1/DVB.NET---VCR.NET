using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.DVB.Provider.TTBudget
{
	internal enum BudgetFilterType
	{
		None,
		Streaming,
		Piping,
		PacketizedElementaryStream,
		ElementaryStream,
		Section,
		MPESection,
		PID,
		MultiPID,
		TransportStream,
		MultiMPE
	}
}
