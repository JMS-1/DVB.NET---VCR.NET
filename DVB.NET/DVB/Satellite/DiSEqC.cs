using System;

namespace JMS.DVB.Satellite
{
	/// <summary>
	/// Base class for DiSEqC description.
	/// </summary>
	[Serializable] public abstract class DiSEqC : LNBSettings
	{
		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="pLNB">The related LNB settings.</param>
		protected DiSEqC(LNBSettings pLNB) : base(pLNB)
		{
		}

		/// <summary>
		/// Uses <see cref="LNBSettings.DefaultSettings"/> calling the default
		/// constructor with two parameters.
		/// </summary>
		protected DiSEqC() : this(LNBSettings.DefaultSettings)
		{
		}

		/// <summary>
		/// Report the byte sequence to send.
		/// </summary>
		/// <remarks>
		/// The caller is allowed to modify the resulting <see cref="Array"/>.
		/// </remarks>
		public abstract byte[] Data { get; }

        /// <summary>
        /// Create a DiSEqC message description.
        /// </summary>
        /// <remarks>
        /// This method replaced the direct call to <see cref="Data"/>.
        /// </remarks>
        /// <param name="channel">Related satellite channel.</param>
        /// <returns>A DiSEqC message to be sent.</returns>
        public abstract DiSEqCMessage CreateMessage(SatelliteChannel channel);
	}
}
