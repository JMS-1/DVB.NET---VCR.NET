using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Possible audio types.
    /// </summary>
	public enum AudioTypes
	{
        /// <summary>
        /// Undefined.
        /// </summary>
        Undefined,

        /// <summary>
        /// No special effects.
        /// </summary>
        CleanEffects,

        /// <summary>
        /// Hearing impaired.
        /// </summary>
        HearingImpaired,

        /// <summary>
        /// Visual impaired.
        /// </summary>
        VisualImpairedCommentary
	}
}
