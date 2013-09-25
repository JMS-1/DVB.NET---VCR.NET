using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Teletext types.
    /// </summary>
	public enum TeletextTypes
	{
        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved,

        /// <summary>
        /// Initial teletext page.
        /// </summary>
        InitialPage,

        /// <summary>
        /// Teletext subtitle page.
        /// </summary>
        SubTitlePage,

        /// <summary>
        /// Additional information page.
        /// </summary>
        AdditionalInformation,

        /// <summary>
        /// Programm schedule page.
        /// </summary>
        ProgrammeSchedule,

        /// <summary>
        /// Teletext subtitle page for hearing impaired people.
        /// </summary>
        HearingImpaired
	}
}
